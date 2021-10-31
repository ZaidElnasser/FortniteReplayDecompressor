using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Unreal.Core.Models;
using Unreal.Core.Models.Enums;

namespace Unreal.Core
{
    /// <summary>
    /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/Serialization/BitArchive.h
    /// see https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Private/Serialization/BitArchive.cpp
    /// </summary>
    public unsafe partial class BitReader : FBitArchive
    {
        private bool _disposed;
        private int[] _tempLastBit = GetPool();
        private int[] _tempPosition = GetPool();
        private static ConcurrentQueue<int[]> _positionQueues = new ConcurrentQueue<int[]>();

        private int _position;
        public override int Position { get => _position; protected set => _position = value; }

        public int LastBit { get; private set; }
        public override int MarkPosition { get; protected set; }
        public override unsafe byte* BasePointer => (byte*)_owner.PinnedMemory.Pointer;

        public BitReader()
        {

        }

        public BitReader(byte* ptr, int byteCount, int bitCount)
        {
            CreateBitArray(ptr, byteCount, bitCount);
        }

        public BitReader(bool* boolPtr, int bitCount)
        {
            Bits = boolPtr;
            LastBit = bitCount;
        }

        private static int[] GetPool()
        {
            if (_positionQueues.TryDequeue(out var result))
            {
                return result;
            }

            return new int[8];
        }

        public override bool AtEnd()
        {
            return Position >= LastBit;
        }

        public override bool CanRead(int count)
        {
            return Position + count <= LastBit;
        }

        public override bool PeekBit()
        {
            return Bits[_position];
        }

        public override bool ReadBit()
        {
            if (_position >= LastBit)
            {
                IsError = true;
                return false;
            }

            return Bits[_position++];
        }

        public override T[] ReadArray<T>(Func<T> func1)
        {
            throw new NotImplementedException();
        }

        public override int ReadBitsToInt(int bitCount)
        {
            if (!CanRead(bitCount))
            {
                IsError = true;
                return 0;
            }

            var result = 0;
            for (var i = 0; i < bitCount; i++)
            {
                result |= (byte)(GetAsByte(_position + i) << i);
            }

            _position += bitCount;
            return result;
        }

        public override ReadOnlyMemory<bool> ReadBits(int bitCount)
        {
            if (!CanRead(bitCount) || bitCount < 0)
            {
                IsError = true;
                return ReadOnlyMemory<bool>.Empty;
            }

            var result = Items.Slice(_position, bitCount);
            _position += bitCount;
            return result;
        }


        public override ReadOnlyMemory<bool> ReadBits(uint bitCount)
        {
            return ReadBits((int)bitCount);
        }

        public override bool ReadBoolean()
        {
            return ReadBit();
        }

        public override byte PeekByte()
        {
            var result = ReadByte();
            Position -= 8;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe byte ReadByteNoCheck()
        {
            var result = new byte();

            var pos = _position;

            result |= (GetAsByte(pos + 0));
            result |= (byte)(GetAsByte(pos + 1) << 1);
            result |= (byte)(GetAsByte(pos + 2) << 2);
            result |= (byte)(GetAsByte(pos + 3) << 3);
            result |= (byte)(GetAsByte(pos + 4) << 4);
            result |= (byte)(GetAsByte(pos + 5) << 5);
            result |= (byte)(GetAsByte(pos + 6) << 6);
            result |= (byte)(GetAsByte(pos + 7) << 7);

            _position += 8;

            return result;
        }

        public override byte ReadByte()
        {
            if (!CanRead(8))
            {
                IsError = true;

                return 0;
            }

            return ReadByteNoCheck();
        }

        public override T ReadByteAsEnum<T>()
        {
            return (T)Enum.ToObject(typeof(T), ReadByte());
        }

        public void ReadBytes(Span<byte> data)
        {
            if (!CanRead(data.Length * 8))
            {
                IsError = true;
                return;
            }

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = ReadByteNoCheck();
            }
        }

        public override ReadOnlySpan<byte> ReadBytes(int byteCount)
        {
            if (byteCount < 0)
            {
                IsError = true;
                return ReadOnlySpan<byte>.Empty;
            }

            if (!CanRead(byteCount))
            {
                IsError = true;
                return ReadOnlySpan<byte>.Empty;
            }

            Span<byte> result = new byte[byteCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ReadByteNoCheck();
            }

            return result;
        }

        public override ReadOnlySpan<byte> ReadBytes(uint byteCount)
        {
            return ReadBytes((int)byteCount);
        }

        public override string ReadBytesToString(int count)
        {
            return Convert.ToHexString(ReadBytes(count)).Replace("-", "");
        }

        public override string ReadFString()
        {
            var length = ReadInt32();

            if (length == 0)
            {
                return "";
            }

            var isUnicode = length < 0;
            if (isUnicode)
            {
                length = -2 * length;
            }

            var encoding = isUnicode ? Encoding.Unicode : Encoding.Default;
            Span<byte> bytes = stackalloc byte[length];
            ReadBytes(bytes);

            return encoding.GetString(bytes).Trim(new[] { ' ', '\0' });
        }

        public override string ReadFName()
        {
            var isHardcoded = ReadBit();
            if (isHardcoded)
            {
                uint nameIndex;
                if (EngineNetworkVersion < EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES)
                {
                    nameIndex = ReadUInt32();
                }
                else
                {
                    nameIndex = ReadIntPacked();
                }

                return ((UnrealNames)nameIndex).ToString();
            }

            var inString = ReadFString();
            var inNumber = ReadInt32();

            return inString;
        }

        public override FTransform ReadFTransfrom()
        {
            throw new NotImplementedException();
        }

        public override string ReadGUID()
        {
            return ReadBytesToString(16);
        }

        public override string ReadGUID(int size)
        {
            return ReadBytesToString(size);
        }

        public override uint ReadSerializedInt(int maxValue)
        {
            int value = 0;
            int count = 0;

            for (uint mask = 1; (value + mask) < maxValue; mask *= 2)
            {
                if (_position >= LastBit)
                {
                    IsError = true;
                    return 0;
                }

                value |= GetAsByte(_position++) << count++;
            }

            return (uint)value;
        }

        public override short ReadInt16()
        {
            Span<byte> value = stackalloc byte[2];
            ReadBytes(value);
            return BinaryPrimitives.ReadInt16LittleEndian(value);
        }

        public override int ReadInt32()
        {
            Span<byte> value = stackalloc byte[4];
            ReadBytes(value);
            return BinaryPrimitives.ReadInt32LittleEndian(value);
        }

        public override bool ReadInt32AsBoolean()
        {
            return ReadInt32() == 1;
        }

        public override long ReadInt64()
        {
            Span<byte> value = stackalloc byte[8];
            ReadBytes(value);
            return BinaryPrimitives.ReadInt64LittleEndian(value);
        }

        public override uint ReadIntPacked()
        {
            int value = 0;
            byte count = 0;
            var remaining = true;

            while (remaining)
            {
                if (_position + 8 > LastBit)
                {
                    IsError = true;
                    return 0;
                }

                remaining = Bits[_position];

                value |= GetAsByte(_position + 1) << count;
                value |= GetAsByte(_position + 2) << (count + 1);
                value |= GetAsByte(_position + 3) << (count + 2);
                value |= GetAsByte(_position + 4) << (count + 3);
                value |= GetAsByte(_position + 5) << (count + 4);
                value |= GetAsByte(_position + 6) << (count + 5);
                value |= GetAsByte(_position + 7) << (count + 6);

                _position += 8;
                count += 7;
            }

            return (uint)value;
        }

        public override FQuat ReadFQuat()
        {
            throw new NotImplementedException();
        }

        public override FVector ReadFVector()
        {
            return new FVector(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public override FVector ReadPackedVector(int scaleFactor, int maxBits)
        {
            var bits = ReadSerializedInt(maxBits);

            if (IsError)
            {
                return new FVector(0, 0, 0);
            }

            var bias = 1 << ((int)bits + 1);
            var max = 1 << ((int)bits + 2);

            var dx = ReadSerializedInt(max);
            var dy = ReadSerializedInt(max);
            var dz = ReadSerializedInt(max);

            if (IsError)
            {
                return new FVector(0, 0, 0);
            }

            var x = (dx - bias) / scaleFactor;
            var y = (dy - bias) / scaleFactor;
            var z = (dz - bias) / scaleFactor;

            return new FVector(x, y, z);
        }

        public override FRotator ReadRotation()
        {
            float pitch = 0;
            float yaw = 0;
            float roll = 0;

            if (ReadBit()) // Pitch
            {
                pitch = ReadByte() * 360 / 256;
            }

            if (ReadBit())
            {
                yaw = ReadByte() * 360 / 256;
            }

            if (ReadBit())
            {
                roll = ReadByte() * 360 / 256;
            }

            if (IsError)
            {
                return new FRotator(0, 0, 0);
            }

            return new FRotator(pitch, yaw, roll);
        }

        public override FRotator ReadRotationShort()
        {
            float pitch = 0;
            float yaw = 0;
            float roll = 0;

            if (ReadBit())
            {
                pitch = ReadUInt16() * 360 / 65536;
            }

            if (ReadBit())
            {
                yaw = ReadUInt16() * 360 / 65536;
            }

            if (ReadBit())
            {
                roll = ReadUInt16() * 360 / 65536;
            }

            if (IsError)
            {
                return new FRotator(0, 0, 0);
            }

            return new FRotator(pitch, yaw, roll);
        }

        public override sbyte ReadSByte()
        {
            throw new NotImplementedException();
        }

        public override float ReadSingle()
        {
            Span<byte> value = stackalloc byte[4];
            ReadBytes(value);
            return BinaryPrimitives.ReadSingleLittleEndian(value);
        }

        public override (T, U)[] ReadTupleArray<T, U>(Func<T> func1, Func<U> func2)
        {
            throw new NotImplementedException();
        }

        public override ushort ReadUInt16()
        {
            Span<byte> value = stackalloc byte[2];
            ReadBytes(value);
            return BinaryPrimitives.ReadUInt16LittleEndian(value);
        }

        public override uint ReadUInt32()
        {
            Span<byte> value = stackalloc byte[4];
            ReadBytes(value);
            return BinaryPrimitives.ReadUInt32LittleEndian(value);
        }

        public override bool ReadUInt32AsBoolean()
        {
            throw new NotImplementedException();
        }

        public override T ReadUInt32AsEnum<T>()
        {
            throw new NotImplementedException();
        }

        public override ulong ReadUInt64()
        {
            Span<byte> value = stackalloc byte[8];
            ReadBytes(value);
            return BinaryPrimitives.ReadUInt64LittleEndian(value);
        }

        public override void Seek(int offset, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            if (offset < 0 || offset > LastBit || (seekOrigin == SeekOrigin.Current && offset + _position > LastBit))
            {
                throw new ArgumentOutOfRangeException("Specified offset doesnt fit within the BitArray buffer");
            }

            _ = (seekOrigin switch
            {
                SeekOrigin.Begin => _position = offset,
                SeekOrigin.End => _position = LastBit - offset,
                SeekOrigin.Current => _position += offset,
                _ => _position = offset,
            });
        }

        public override void SkipBytes(uint byteCount)
        {
            SkipBytes((int)byteCount);
        }

        public override void SkipBytes(int byteCount)
        {
            Seek(byteCount * 8, SeekOrigin.Current);
        }

        public override void SkipBits(int numbits)
        {
            Seek(numbits, SeekOrigin.Current);
        }

        public override void SkipBits(uint numbits)
        {
            SkipBits((int)numbits);
        }

        public override void Mark()
        {
            MarkPosition = Position;
        }

        public override void Pop()
        {
            Position = MarkPosition;
        }

        public override int GetBitsLeft()
        {
            return LastBit - Position;
        }

        public override void AppendDataFromChecked(ReadOnlyMemory<bool> data)
        {
            AppendBits(data);
        }

        public override void SetTempEnd(int totalBits, int index = 0)
        {
            uint setPosition = (uint)(_position + totalBits);

            if (setPosition > LastBit)
            {
                IsError = true;

                return;
            }

            _tempLastBit[index] = LastBit;
            _tempPosition[index] = (int)setPosition;
            LastBit = _position + totalBits;
        }

        public override void RestoreTemp(int index = 0)
        {
            LastBit = _tempLastBit[index];
            _position = _tempPosition[index];

            /*
            _tempLastBit = 0;
            _tempPosition = 0;
            */

            IsError = false;
        }


        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                DisposeBits();

                _positionQueues.Enqueue(_tempLastBit);
                _positionQueues.Enqueue(_tempPosition);
                _disposed = true;
            }
        }
    }
}
