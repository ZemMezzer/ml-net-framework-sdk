namespace RAG
{
    public struct ScoreHeap<T>
    {
        private readonly (T value, float score)[] _data;
        private int _size;

        public int Count => _size;

        public ScoreHeap(int capacity)
        {
            _data = new (T, float)[capacity];
            _size = 0;
        }

        public float MinScore => _data[0].score;

        public void Push((T value, float score) item)
        {
            var i = _size++;
            _data[i] = item;

            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_data[p].score <= item.score)
                    break;

                _data[i] = _data[p];
                i = p;
            }

            _data[i] = item;
        }

        public void ReplaceMin((T value, float score) item)
        {
            var i = 0;
            var half = _size >> 1;

            while (i < half)
            {
                var left = (i << 1) + 1;
                var right = left + 1;

                var best = right < _size && _data[right].score < _data[left].score
                    ? right
                    : left;

                if (_data[best].score >= item.score)
                    break;

                _data[i] = _data[best];
                i = best;
            }

            _data[i] = item;
        }

        public (T value, float score) PopMin()
        {
            var root = _data[0];
            var last = _data[--_size];

            if (_size <= 0) 
                return root;
            
            var i = 0;
            var half = _size >> 1;

            while (i < half)
            {
                var left = (i << 1) + 1;
                var right = left + 1;

                var best = right < _size && _data[right].score < _data[left].score
                    ? right
                    : left;

                if (_data[best].score >= last.score)
                    break;

                _data[i] = _data[best];
                i = best;
            }

            _data[i] = last;

            return root;
        }
    }
}