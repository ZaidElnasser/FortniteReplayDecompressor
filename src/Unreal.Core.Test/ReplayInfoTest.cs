﻿using System.IO;
using Unreal.Core.Exceptions;
using Unreal.Core.Test.Mocks;
using Xunit;

namespace Unreal.Core.Test;

public class ReplayInfoTest
{
    [Theory]
    [InlineData(new byte[] {
        0x7F, 0xE2, 0xA2, 0x1C, 0x05, 0x00, 0x00, 0x00, 0x5D, 0x59, 0x13, 0x00,
        0x02, 0x00, 0x00, 0x00, 0x8D, 0xFF, 0x4C, 0x00, 0xFA, 0xFF, 0xFF, 0xFF,
        0x52, 0x00, 0x65, 0x00, 0x70, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x79, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x80, 0x49, 0xCF, 0xE2, 0xFC,
        0x98, 0xD6, 0x08, 0x01, 0x00, 0x00, 0x00
    })]
    [InlineData(new byte[] {
        0x7F, 0xE2, 0xA2, 0x1C, 0x05, 0x00, 0x00, 0x00, 0x10, 0x48, 0x13, 0x00,
        0x02, 0x00, 0x00, 0x00, 0xDA, 0x0F, 0x43, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x40, 0x04, 0x8F, 0x54, 0xC5, 0x24, 0xD6, 0x08,
        0x01, 0x00, 0x00, 0x00
    })]
    [InlineData(new byte[] {
        0x7F, 0xE2, 0xA2, 0x1C, 0x06, 0x00, 0x00, 0x00, 0x62, 0xD6, 0x01, 0x00,
        0x02, 0x00, 0x00, 0x00, 0x5A, 0x56, 0xB0, 0x00, 0xF8, 0xFF, 0xFF, 0xFF,
        0x55, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x61, 0x00, 0x76, 0x00, 0x65, 0x00,
        0x64, 0x00, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x2C, 0x9F, 0x35,
        0x1F, 0xB6, 0xD7, 0x08, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
        0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    })]
    public void ReadReplayInfoTest(byte[] rawData)
    {
        using var stream = new MemoryStream(rawData);
        using var archive = new Unreal.Core.BinaryReader(stream);
        var reader = new MockReplayReader();
        reader.ReadReplayInfo(archive);
        Assert.True(archive.AtEnd());
        Assert.False(archive.IsError);
    }

    [Fact]
    public void ReadReplayInfoThrowsOnWrongMagicTest()
    {
        byte[] rawData = {
            0x7F, 0xE2, 0xA2, 0x10, 0x06, 0x00, 0x00, 0x00, 0x62, 0xD6, 0x01, 0x00
        };
        using var stream = new MemoryStream(rawData);
        using var archive = new Unreal.Core.BinaryReader(stream);
        var reader = new MockReplayReader();

        var exception = Assert.Throws<InvalidReplayException>(() => reader.ReadReplayInfo(archive));
        Assert.Equal("Invalid replay file", exception.Message);
    }

    [Fact]
    public void ReadReplayInfoThrowsOnLiveAndNotCompletedTest()
    {
        byte[] rawData = {
            0x7F, 0xE2, 0xA2, 0x1C, 0x06, 0x00, 0x00, 0x00, 0x62, 0xD6, 0x01, 0x00,
            0x02, 0x00, 0x00, 0x00, 0x5A, 0x56, 0xB0, 0x00, 0xFA, 0xFF, 0xFF, 0xFF,
            0x52, 0x00, 0x65, 0x00, 0x70, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x79, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x80, 0x2C, 0x9F, 0x35, 0x1F, 0xB6, 0xD7, 0x08,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        using var stream = new MemoryStream(rawData);
        using var archive = new Unreal.Core.BinaryReader(stream);
        var reader = new MockReplayReader();

        var exception = Assert.Throws<InvalidReplayException>(() => reader.ReadReplayInfo(archive));
        Assert.Equal("Replay is marked encrypted but not yet marked as completed!", exception.Message);
    }

    [Fact]
    public void ReadReplayInfoThrowsOnCompletedButNoKeyTest()
    {
        byte[] rawData = {
            0x7F, 0xE2, 0xA2, 0x1C, 0x06, 0x00, 0x00, 0x00, 0x62, 0xD6, 0x01, 0x00,
            0x02, 0x00, 0x00, 0x00, 0x5A, 0x56, 0xB0, 0x00, 0xFA, 0xFF, 0xFF, 0xFF,
            0x52, 0x00, 0x65, 0x00, 0x70, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x79, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x80, 0x2C, 0x9F, 0x35, 0x1F, 0xB6, 0xD7, 0x08,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        using var stream = new MemoryStream(rawData);
        using var archive = new Unreal.Core.BinaryReader(stream);
        var reader = new MockReplayReader();

        var exception = Assert.Throws<InvalidReplayException>(() => reader.ReadReplayInfo(archive));
        Assert.Equal("Completed replay is marked encrypted but has no key!", exception.Message);
    }
}
