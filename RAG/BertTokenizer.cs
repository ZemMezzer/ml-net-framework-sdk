using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RAG
{
    public class BertTokenizer
    {
        private readonly Dictionary<string, int> _vocab;
        private readonly Dictionary<string, int[]> _cache = new Dictionary<string, int[]>();

        private readonly int _unkId;

        public BertTokenizer(Stream stream)
        {
            var vocab = new Dictionary<string, int>();

            using var sr = new StreamReader(stream, Encoding.UTF8);
            var id = 0;

            while (sr.ReadLine() is { } line)
            {
                vocab[line.Trim()] = id++;
            }

            _vocab = vocab;

            if (!vocab.TryGetValue("[UNK]", out _unkId))
                throw new InvalidDataException("vocab does not contain [UNK]");

            _unkId = vocab["[UNK]"];
        }

        public int[] EncodeToIds(string text)
        {
            text = text.ToLowerInvariant();
            var words = BasicTokenize(text);

            var result = new List<int>(words.Count * 2);
            foreach (var word in words)
            {
                if (_cache.TryGetValue(word, out var cached))
                {
                    result.AddRange(cached);
                    continue;
                }

                var tmp = new List<int>(4);
                WordPieceTokenize(word, tmp);
                var arr = tmp.ToArray();
                _cache[word] = arr;
                result.AddRange(arr);
            }

            return result.ToArray();
        }

        private static List<string> BasicTokenize(string text)
        {
            var tokens = new List<string>();
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (sb.Length <= 0)
                        continue;

                    tokens.Add(sb.ToString());
                    sb.Clear();
                }
            }

            if (sb.Length > 0)
                tokens.Add(sb.ToString());

            return tokens;
        }

        private void WordPieceTokenize(string word, List<int> output)
        {
            var start = 0;
            var isBad = false;

            while (start < word.Length)
            {
                var end = word.Length;
                var curId = -1;

                while (start < end)
                {
                    string sub = word.Substring(start, end - start);
                    if (start > 0) sub = "##" + sub;

                    if (_vocab.TryGetValue(sub, out curId))
                        break;

                    end--;
                }
                
                if (curId == -1)
                {
                    isBad = true;
                    break;
                }

                if (end <= start)
                {
                    isBad = true;
                    break;
                }

                output.Add(curId);
                start = end;
            }

            if (isBad)
                output.Add(_unkId);
        }
    }
}