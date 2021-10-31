using OozSharp.Exceptions;
using System.IO;
using Xunit;

namespace Unreal.Encryption.Test
{
    public unsafe class OodleDecompressTest
    {
        [Theory]
        [InlineData(@"CompressedChunk/compressed-chunk-0.dump", 214323, 405273)]
        [InlineData(@"CompressedChunk/compressed-chunk-1.dump", 153441, 393295)]
        public void DecompressTest(string data, int compressedSize, int decompressedSize)
        {
            using var stream = File.Open(data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var compressedBuffer = ms.ToArray();

            fixed (byte* source = compressedBuffer)
            fixed (byte* result = new byte[decompressedSize])
            {
                var local = source;
                var localResult = result;

                var ex = Record.Exception(() => Oodle.DecompressReplayData(local, compressedSize, localResult, decompressedSize));
                Assert.Null(ex);
            }
        }

        [Theory]
        [InlineData(@"CompressedChunk/compressed-chunk-0.dump", 214323, 305273)]
        [InlineData(@"CompressedChunk/compressed-chunk-1.dump", 153441, 393294)]
        public void DecompressThrows(string data, int compressedSize, int decompressedSize)
        {
            using var stream = File.Open(data, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            var compressedBuffer = ms.ToArray();

            

            fixed (byte* source = compressedBuffer)
            fixed (byte* result = new byte[decompressedSize])
            {
                var local = source;
                var localResult = result;

                Assert.Throws<DecoderException>(() => Oodle.DecompressReplayData(local, compressedSize, localResult, decompressedSize));
            }
        }
    }
}
