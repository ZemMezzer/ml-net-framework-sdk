using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;

namespace RAG
{
    public class VectorDatabase<T> : IDisposable
    {
        [Serializable]
        private class DatabaseBlock
        {
            public T Block;
            public float[] Embed;

            public DatabaseBlock(T block, float[] embed)
            {
                Block = block;
                Embed = embed;
            }
        }
        
        public struct Block
        {
            public readonly float Score;
            public readonly T Value;

            public Block(float score, T value)
            {
                Score = score;
                Value = value;
            }
        }
        
        private readonly InferenceSession _session;
        private readonly BertTokenizer _tokenizer;
        private readonly int _maxLen;
        
        private readonly List<DatabaseBlock> _blocks = new List<DatabaseBlock>();
        private readonly List<Block> _blocksBuffer = new List<Block>();

        public VectorDatabase(byte[] embeddedModel, byte[] vocab, int maxLength = 256)
        {
            using var stream = new MemoryStream(vocab);

            _session = new InferenceSession(embeddedModel);
            _tokenizer = new BertTokenizer(stream);

            _maxLen = maxLength;
        }

        public void Load(byte[] bytes)
        {
            try
            {
                var json = Encoding.UTF8.GetString(bytes);
                var blocks = JsonConvert.DeserializeObject<DatabaseBlock[]>(json);
                
                if (blocks == null) 
                    return;
                _blocks.Clear();
                _blocks.AddRange(blocks);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public byte[] Save()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_blocks));
        }
        
        public float[] Embed(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            const long PAD = 0;
            const long CLS = 101;
            const long SEP = 102;

            var ids = _tokenizer.EncodeToIds(text).Select(i => (long)i).ToList();

            if (ids.Count == 0 || ids[0] != CLS) ids.Insert(0, CLS);
            if (ids.Count == 0 || ids[^1] != SEP) ids.Add(SEP);

            if (ids.Count > _maxLen)
                ids = ids.Take(_maxLen).ToList();
            
            var attention = Enumerable.Repeat(1L, ids.Count).ToList();

            while (ids.Count < _maxLen)
            {
                ids.Add(PAD);
                attention.Add(0);
            }

            var needsTokenTypes = _session.InputMetadata.Keys.Any(k =>
                k.Contains("token_type", StringComparison.OrdinalIgnoreCase) ||
                k.Contains("token_type_ids", StringComparison.OrdinalIgnoreCase) ||
                (k.Contains("token", StringComparison.OrdinalIgnoreCase) &&
                 k.Contains("type", StringComparison.OrdinalIgnoreCase)));

            var inputIdsTensor = new DenseTensor<long>(new[] { 1, _maxLen });
            var attentionTensor = new DenseTensor<long>(new[] { 1, _maxLen });
            var tokenTypesTensor = needsTokenTypes ? new DenseTensor<long>(new[] { 1, _maxLen }) : null;
            
            for (int i = 0; i < _maxLen; i++)
            {
                inputIdsTensor[0, i] = ids[i];
                attentionTensor[0, i] = attention[i];
                if (tokenTypesTensor != null) tokenTypesTensor[0, i] = 0;
            }

            var inputs = new List<NamedOnnxValue>();

            foreach (var name in _session.InputMetadata.Keys)
            {
                if (name.Contains("input", StringComparison.OrdinalIgnoreCase) &&
                    name.Contains("id", StringComparison.OrdinalIgnoreCase))
                {
                    inputs.Add(NamedOnnxValue.CreateFromTensor(name, inputIdsTensor));
                }
                else if (name.Contains("attention", StringComparison.OrdinalIgnoreCase))
                {
                    inputs.Add(NamedOnnxValue.CreateFromTensor(name, attentionTensor));
                }
                else if (tokenTypesTensor != null &&
                         (name.Contains("token_type", StringComparison.OrdinalIgnoreCase) ||
                          (name.Contains("token", StringComparison.OrdinalIgnoreCase) &&
                           name.Contains("type", StringComparison.OrdinalIgnoreCase))))
                {
                    inputs.Add(NamedOnnxValue.CreateFromTensor(name, tokenTypesTensor));
                }
            }
            if (!inputs.Any(v => v.Name.Contains("id", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"ONNX input_ids not mapped. Inputs: {string.Join(", ", _session.InputMetadata.Keys)}");

            if (!inputs.Any(v => v.Name.Contains("attention", StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException(
                    $"ONNX attention_mask not mapped. Inputs: {string.Join(", ", _session.InputMetadata.Keys)}");
            
            using var results = _session.Run(inputs);
            DenseTensor<float>? embeddingTensor = null;

            foreach (var r in results)
            {
                var dt = r.Value as DenseTensor<float>;
                if (dt == null)
                    continue;

                if (dt.Rank != 2)
                    continue;

                if (dt.Dimensions == null || dt.Dimensions.Length != 2)
                    continue;

                if (dt.Dimensions[0] != 1)
                    continue;
                
                embeddingTensor = dt;
                break;
            }

            embeddingTensor ??= results.Select(r => r.Value).OfType<DenseTensor<float>>().FirstOrDefault();

            if (embeddingTensor == null)
                throw new InvalidOperationException("ONNX outputs have no float tensor.");

            return embeddingTensor.ToArray();
        }

        public float[] EmbedL2(string query)
        {
            var q = Embed(query);
            L2Normalize(q);
            
            return q;
        }
        
        public void Insert(string text, T value)
        {
            var e = Embed(text);
            L2Normalize(e);
            _blocks.Add(new DatabaseBlock(value, e));
        }
        
        public List<Block> Search(string query, Func<T, bool> predicate = null, float minScore = 0.35f, int topK = 5) => Search(EmbedL2(query), predicate, minScore, topK);

        public List<Block> Search(float[] query, Func<T, bool> predicate = null, float minScore = 0.35f, int topK = 5)
        {
            var heap = new ScoreHeap<T>(topK);
            var blocks = _blocks;

            for (var i = 0; i < blocks.Count; i++)
            {
                var entry = blocks[i];
                var value = entry.Block;

                if (predicate != null && !predicate(value))
                    continue;

                var score = Dot(query, entry.Embed);

                if (score < minScore)
                    continue;

                if (heap.Count < topK)
                {
                    heap.Push((value, score));
                }
                else if (score > heap.MinScore)
                {
                    heap.ReplaceMin((value, score));
                }
            }

            _blocksBuffer.Clear();

            while (heap.Count > 0)
            {
                var block = heap.PopMin();
                _blocksBuffer.Insert(0, new Block(block.score, block.value));
            }

            return _blocksBuffer;
        }

        public bool Contains(string text, Func<T,bool> predicate, float duplicateThreshold = 0.93f)
        {
            var query = EmbedL2(text);
            var hits = Search(query, predicate, 0);

            if (hits.Count == 0)
                return false;

            var best = hits[0];
            return best.Score >= duplicateThreshold;
        }
 
        private float Dot(float[] a, float[] b)
        {
            double dot = 0;
            for (int i = 0; i < a.Length; i++) dot += a[i] * b[i];
            return (float)dot;
        }

        private void L2Normalize(float[] v)
        {
            double sum = 0;
            for (int i = 0; i < v.Length; i++)
            {
                sum += v[i] * v[i];
            }
            
            var inv = 1.0 / (Math.Sqrt(sum) + 1e-12);
            
            for (int i = 0; i < v.Length; i++)
            {
                v[i] = (float)(v[i] * inv);
            }
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}