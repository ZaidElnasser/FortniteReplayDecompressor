using System.IO;
using Xunit;

namespace OozSharp.Test
{
    public unsafe class DecompressTest
    {
        [Theory]
        [InlineData(@"CompressedChunk/mermaid-fortnite.dump", 214323, 405273)]
        public void MermaidTest(string data, int compressedSize, int decompressedSize)
        {
            using var stream = File.Open(data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var rawData = ms.ToArray();

            var kraken = new Kraken();

            fixed (byte* source = rawData)
            fixed (byte* result = new byte[decompressedSize])
            {
                var local = source;
                var localResult = result;

                var ex = Record.Exception(() => kraken.Decompress(local, compressedSize, localResult, decompressedSize));
                Assert.Null(ex);
            }
        }
    }
}
