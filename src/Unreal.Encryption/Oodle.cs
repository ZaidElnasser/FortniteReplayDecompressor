using OozSharp;
using System;

namespace Unreal.Encryption
{
    /// <summary>  
    /// see https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Plugins/Runtime/PacketHandlers/CompressionComponents/Oodle/Source/OodleHandlerComponent/Private/OodleUtils.cpp 
    /// </summary>
    public unsafe static class Oodle
    {
        static readonly Kraken kraken = new();

        /// <summary>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Plugins/Runtime/PacketHandlers/CompressionComponents/Oodle/Source/OodleHandlerComponent/Private/OodleUtils.cpp#L14
        /// </summary>
        public static void DecompressReplayData(byte* buffer, int bufferLength, byte* uncompressedBuffer, int uncompressedSize)
        {
            kraken.Decompress(buffer, bufferLength, uncompressedBuffer, uncompressedSize);
        }
    }
}
