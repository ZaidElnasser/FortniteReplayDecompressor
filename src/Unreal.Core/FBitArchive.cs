using System;
using Unreal.Core.Models;

namespace Unreal.Core
{
    /// <summary>
    /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Serialization/BitArchive.h
    /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Private/Serialization/BitArchive.cpp
    /// </summary>
    public abstract class FBitArchive : FArchive
    {
        /// <summary>
        /// Returns the bit at <see cref="Position"/> and does not advance the <see cref="Position"/> by one bit.
        /// </summary>
        public abstract bool PeekBit();

        /// <summary>
        /// Returns the byte at <see cref="Position"/>
        /// </summary>
        public abstract byte PeekByte();

        /// <summary>
        /// Returns the bit at <see cref="Position"/> and advances the <see cref="Position"/> by one bit.
        /// </summary>
        public abstract bool ReadBit();

        /// <summary>
        /// Retuns bool[] and advances the <see cref="Position"/> by <paramref name="bits"/> bits.
        /// </summary>
        public abstract ReadOnlyMemory<bool> ReadBits(int bitCount);

        /// <summary>
        /// Retuns bool[] and advances the <see cref="Position"/> by <paramref name="bits"/> bits.
        /// </summary>
        public abstract ReadOnlyMemory<bool> ReadBits(uint bitCount);

        /// <summary>
        /// Retuns int and advances the <see cref="Position"/> by <paramref name="bits"/> bits.
        /// </summary>
        public abstract int ReadBitsToInt(int bitCount);

        /// <summary>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Serialization/BitReader.h#L69
        /// </summary>
        public abstract uint ReadSerializedInt(int maxValue);

        /// <summary>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Engine/Classes/Engine/NetSerialization.h#L1210
        /// </summary>
        public abstract FVector ReadPackedVector(int scaleFactor, int maxBits);

        /// <summary>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Private/Math/UnrealMath.cpp#L79
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Math/Rotator.h#L654
        /// </summary>
        public abstract FRotator ReadRotation();

        /// <summary>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Private/Math/UnrealMath.cpp#L79
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Math/Rotator.h#L654
        /// </summary>
        public abstract FRotator ReadRotationShort();

        /// <summary>
        /// Skip next <paramref name="bitCount"/> bits.
        /// </summary>
        public abstract void SkipBits(int bitCount);

        /// <summary>
        /// Skip next <paramref name="bitCount"/> bits.
        /// </summary>
        public abstract void SkipBits(uint bitCount);

        /// <summary>
        /// For pushing and popping FBitReaderMark positions.
        /// </summary>
        public abstract int MarkPosition { get; protected set; }

        /// <summary>
        /// Save Position to <see cref="MarkPosition"/> so we can reset back to this point.
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Serialization/BitReader.h#L228
        /// </summary>
        public abstract void Mark();

        /// <summary>
        /// Set Position back to <see cref="MarkPosition"/>
        /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Serialization/BitReader.h#L228
        /// </summary>
        public abstract void Pop();

        /// <summary>
        /// Get number of bits left, including any bits after <see cref="LastBit"/>.
        /// </summary>
        public abstract int GetBitsLeft();

        /// <summary>
        /// Append bool array to this archive.
        /// </summary>
        public abstract void AppendDataFromChecked(ReadOnlyMemory<bool> data);

        /// <summary>
        /// Pretend this archive ends earlier to reduce alloctions. Make sure to call <see cref="RestoreTemp(int)"/> afterwards.
        /// </summary>
        public abstract void SetTempEnd(int totalBits, int index = 0);
        
        /// <summary>
        /// Restore the original end of the archive
        /// </summary>
        public abstract void RestoreTemp(int index = 0);
    }
}
