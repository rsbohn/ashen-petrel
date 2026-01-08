using System;

namespace Ashen
{
    internal sealed class Hp3000Memory
    {
        private readonly ushort[] _words;

        public Hp3000Memory(int wordCount)
        {
            if (wordCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(wordCount));
            }

            _words = new ushort[wordCount];
        }

        public int Size => _words.Length;

        public ushort Read(int address)
        {
            return _words[MaskAddress(address)];
        }

        public void Write(int address, ushort value)
        {
            _words[MaskAddress(address)] = value;
        }

        public void ReadBlock(int address, ushort[] destination, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (count < 0 || count > destination.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (var i = 0; i < count; i++)
            {
                destination[i] = Read(address + i);
            }
        }

        public void WriteBlock(int address, ushort[] source, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (count < 0 || count > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            for (var i = 0; i < count; i++)
            {
                Write(address + i, source[i]);
            }
        }

        private int MaskAddress(int address)
        {
            return address & 0x7fff;
        }
    }
}
