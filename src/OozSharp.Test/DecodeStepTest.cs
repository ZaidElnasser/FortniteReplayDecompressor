using OozSharp.Exceptions;
using Xunit;

namespace OozSharp.Test
{
    public unsafe class DecodeStepTest
    {
        [Theory]
        [InlineData(new byte[] { 0x8C, 0x01 }, "Decoder type LZH not supported")]
        [InlineData(new byte[] { 0x8C, 0x09 }, "Decoder type Kraken not supported")]
        public void DecodeStepThrows(byte[] rawData, string message)
        {
            var kraken = new Kraken();

            fixed (byte* source = rawData)
            {
                var local = source;
                var exception = Assert.Throws<DecoderException>(() => kraken.Decompress(local, 393294, local, 393294));
                Assert.Equal(message, exception.Message);
            }

        }
    }
}
