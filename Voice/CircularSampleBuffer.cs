using System;

namespace InsanityMod.Voice
{
    internal sealed class CircularSampleBuffer
    {
        private readonly float[] _data;
        private readonly object _lock = new();
        private int _writeIdx;
        private int _filled;

        public CircularSampleBuffer(int capacity)
        {
            _data = new float[capacity];
        }

        public int Capacity => _data.Length;

        public int FilledSamples
        {
            get { lock (_lock) return _filled; }
        }

        public void Write(float[] samples, int offset, int count)
        {
            if (count <= 0) return;

            lock (_lock)
            {
                int cap = _data.Length;
                for (int i = 0; i < count; i++)
                {
                    _data[_writeIdx] = samples[offset + i];
                    _writeIdx = (_writeIdx + 1) % cap;
                }
                _filled = Math.Min(_filled + count, cap);
            }
        }

        public float[]? ExtractSnippet(int sampleCount, System.Random rng)
        {
            lock (_lock)
            {
                if (_filled < sampleCount) return null;

                int cap = _data.Length;
                int available = _filled;
                int oldestIdx = (_writeIdx - available + cap) % cap;

                int maxStartOffset = available - sampleCount;
                int startOffset = maxStartOffset > 0 ? rng.Next(0, maxStartOffset + 1) : 0;
                int readIdx = (oldestIdx + startOffset) % cap;

                var result = new float[sampleCount];
                for (int i = 0; i < sampleCount; i++)
                {
                    result[i] = _data[readIdx];
                    readIdx = (readIdx + 1) % cap;
                }
                return result;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_data, 0, _data.Length);
                _writeIdx = 0;
                _filled = 0;
            }
        }
    }
}
