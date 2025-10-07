using System.Security.Cryptography;

namespace MusicGenMVC.Services
{
    public sealed class SeededRandomFactory
    {
        // Mix user seed + page + index into a reproducible 64-bit seed
        public ulong Mix(ulong baseSeed, int page, int index = 0)
        {
            ulong golden = 0x9E3779B97F4A7C15UL; // Knuth constant
            return baseSeed ^ (golden * (ulong)(page + 1)) ^ (ulong)index * 0xBF58476D1CE4E5B9UL;
        }


        public Random Create(ulong seed) => new Random(unchecked((int)(seed ^ (seed >> 32))));
        public Random Create(ulong baseSeed, int page, int index = 0) => Create(Mix(baseSeed, page, index));


        public static ulong RandomSeed()
        {
            Span<byte> b = stackalloc byte[8];
            RandomNumberGenerator.Fill(b);
            return BitConverter.ToUInt64(b);
        }
    }
}
