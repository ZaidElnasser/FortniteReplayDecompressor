using Unreal.Core.Models;
using Xunit;

namespace Unreal.Core.Test;

public class FGameplayTagContainerTest
{
    [Theory]
    [InlineData(new byte[] {
        0x30, 0x67, 0x56, 0x73, 0xCA, 0xFD, 0xB4, 0x79, 0xCC, 0xA5, 0x32, 0x6B,
        0x58, 0xE7, 0x8C, 0xE9, 0x8C, 0x5B, 0x34, 0xDF, 0x8C, 0x0F, 0xB6, 0x5F,
        0x80, 0x4D, 0x7E, 0xE3, 0x7E, 0x83, 0x7E, 0xBB, 0x7E, 0x89, 0x52, 0x5F,
        0xF0, 0x6D, 0xD0, 0xC9, 0xBE, 0x55, 0xCE, 0x35, 0xCE, 0x85, 0x7E, 0xC5,
        0x7C }, 392)]
    [InlineData(new byte[] {
        0x38, 0x67, 0x56, 0x73, 0xCA, 0xFD, 0xB4, 0x79, 0xCC, 0xA5, 0x32, 0x69,
        0x58, 0xE7, 0x8C, 0xE9, 0x8C, 0x5B, 0x34, 0xDF, 0x8C, 0x0F, 0xB6, 0x5F,
        0x80, 0x57, 0x34, 0xD7, 0x14, 0xCF, 0x14, 0x4D, 0x7E, 0xE3, 0x7E, 0x83,
        0x7E, 0xBB, 0x7E, 0xAD, 0xEE, 0x41, 0xBE, 0x71, 0x7E, 0x95, 0x7E, 0xF7,
        0x12, 0xDB, 0x7E, 0x3F, 0xBE, 0xCB, 0x7C, 0xCF, 0x7C }, 456)]
    [InlineData(new byte[] {
        0x4C, 0x02, 0x2E, 0x30, 0x02, 0xCB, 0x24, 0x00, 0x04, 0x2E, 0x30, 0x02,
        0xB1, 0x24, 0x00, 0x06, 0x2E, 0x30, 0x02, 0xB3, 0x24, 0x00, 0x08, 0x2E,
        0x30, 0x02, 0xAF, 0x24, 0x00, 0x0A, 0x2E, 0x30, 0x02, 0x67, 0x24, 0x00,
        0x0C, 0x2E, 0x30, 0x02, 0xC1, 0x24, 0x00, 0x0E, 0x2E }, 360)]
    [InlineData(new byte[] { 0x04, 0x7F, 0x32, 0x8B, 0x32 }, 40)]
    public void GameplayTagContainerTest(byte[] rawData, int bitCount)
    {
        var reader = new NetBitReader(rawData, bitCount);
        var tagcontainer = new FGameplayTagContainer();
        tagcontainer.Serialize(reader);

        Assert.True(reader.AtEnd());
        Assert.False(reader.IsError);
    }
}
