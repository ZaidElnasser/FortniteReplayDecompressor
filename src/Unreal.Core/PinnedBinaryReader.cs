using System;
using System.IO;
using System.Text;
using Unreal.Core.Models;
using Unreal.Core.Models.Enums;

namespace Unreal.Core
{
    public class PinnedBinaryReader : FArchive
    {
        private byte[] Bytes { get; set; }
        private bool _isDisposed;
        private readonly int _length;
        private int _position;
        public override int Position { get => _position; protected set => Seek(value); }

        public PinnedBinaryReader(ReadOnlyMemory<byte> input)
        {
            // allocate requested memory length from the pinned memory heap
            Bytes = GC.AllocateUninitializedArray<byte>(input.Length, pinned: true);
            input.CopyTo(Bytes);
        }

        public PinnedBinaryReader(Stream stream)
        {
            // allocate requested memory length from the pinned memory heap
            Bytes = GC.AllocateUninitializedArray<byte>((int) stream.Length, pinned: true);
            stream.Read(Bytes, 0, (int) stream.Length);
        }

        public override bool AtEnd()
        {
            return _position >= _length;
        }

        public override bool CanRead(int count)
        {
            return _position + count < _length;
        }

        public override T[] ReadArray<T>(Func<T> func1)
        {
            var count = ReadUInt32();
            var arr = new T[count];
            for (var i = 0; i < count; i++)
            {
                arr[i] = (func1.Invoke());
            }
            return arr;
        }

        public override bool ReadBoolean()
        {
            var result = Bytes[_position] == 1;
            _position++;
            return result;
        }

        public override byte ReadByte()
        {
            var result = Bytes[_position];
            _position++;
            return result;
        }

        public override T ReadByteAsEnum<T>()
        {
            return (T)Enum.ToObject(typeof(T), ReadByte());
        }

        public override ReadOnlySpan<byte> ReadBytes(int byteCount)
        {
            var result = new byte[byteCount];
            for (var i = 0; i < byteCount; i++)
            {
                result[i] = Bytes[_position + i];
            }
            _position += byteCount;
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
            return encoding.GetString(ReadBytes(length))
                .Trim(new[] { ' ', '\0' });
        }

        public override string ReadFName()
        {
            var isHardcoded = ReadBoolean();
            if (isHardcoded)
            {
                var nameIndex = EngineNetworkVersion < EngineNetworkVersionHistory.HISTORY_CHANNEL_NAMES ? ReadUInt32() : ReadIntPacked();
                // https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/UObject/UnrealNames.h#L31
                // hard coded names in "UnrealNames.inl"
                // https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/UObject/UnrealNames.inl
                // https://github.com/EpicGames/UnrealEngine/blob/375ba9730e72bf85b383c07a5e4a7ba98774bcb9/Engine/Source/Runtime/Core/Public/UObject/NameTypes.h#L599
                // https://github.com/EpicGames/UnrealEngine/blob/375ba9730e72bf85b383c07a5e4a7ba98774bcb9/Engine/Source/Runtime/Core/Private/UObject/UnrealNames.cpp#L283
                return ((UnrealNames)nameIndex).ToString();
            }

            // https://github.com/EpicGames/UnrealEngine/blob/70bc980c6361d9a7d23f6d23ffe322a2d6ef16fb/Engine/Source/Runtime/Core/Public/UObject/UnrealNames.h#L17
            // MAX_NETWORKED_HARDCODED_NAME = 410

            // https://github.com/EpicGames/UnrealEngine/blob/375ba9730e72bf85b383c07a5e4a7ba98774bcb9/Engine/Source/Runtime/Core/Public/UObject/NameTypes.h#L34
            // NAME_SIZE = 1024

            // InName.GetComparisonIndex() <= MAX_NETWORKED_HARDCODED_NAME;
            // InName.GetPlainNameString();
            // InName.GetNumber();

            var inString = ReadFString();
            ReadInt32(); // inNumber

            return inString;
        }

        public override FTransform ReadFTransfrom()
        {
            return new FTransform
            {
                Rotation = ReadFQuat(),
                Translation = ReadFVector(),
                Scale3D = ReadFVector(),
            };
        }

        public override FQuat ReadFQuat()
        {
            return new FQuat
            {
                X = ReadSingle(),
                Y = ReadSingle(),
                Z = ReadSingle(),
                W = ReadSingle()
            };
        }

        public override FVector ReadFVector()
        {
            return new FVector(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public override string ReadGUID()
        {
            return ReadBytesToString(16);
        }

        public override string ReadGUID(int size)
        {
            return ReadBytesToString(size);
        }

        public override short ReadInt16()
        {
            var result = (short)(Bytes[_position] | Bytes[_position + 1] << 8);
            _position += 2;
            return result;
        }

        public override int ReadInt32()
        {
            var result = (Bytes[_position] | Bytes[_position + 1] << 8 | Bytes[_position + 2] << 16 | Bytes[_position + 3] << 24);
            _position += 4;
            return result;
        }

        public override bool ReadInt32AsBoolean()
        {
            return ReadUInt32() >= 1;
        }

        public override long ReadInt64()
        {
            var lo = (uint)(Bytes[_position] | Bytes[_position + 1] << 8 | Bytes[_position + 2] << 16 | Bytes[_position + 3] << 24);
            var hi = (uint)(Bytes[_position + 4] | Bytes[_position + 5] << 8 | Bytes[_position + 6] << 16 | Bytes[_position + 7] << 24);
            var result = (long)((ulong)hi) << 32 | lo;
            _position += 8;
            return result;
        }

        public override uint ReadIntPacked()
        {
            uint value = 0;
            byte count = 0;
            var remaining = true;

            while (remaining)
            {
                var nextByte = ReadByte();
                remaining = (nextByte & 1) == 1;            // Check 1 bit to see if theres more after this
                nextByte >>= 1;                             // Shift to get actual 7 bit value
                value += (uint)nextByte << (7 * count++);   // Add to total value
            }
            return value;
        }

        public override sbyte ReadSByte()
        {
            var result = (sbyte)(Bytes[_position]);
            _position++;
            return result;
        }

        public override float ReadSingle()
        {
            var result = (float)(Bytes[_position] | Bytes[_position + 1] << 8 | Bytes[_position + 2] << 16 | Bytes[_position + 3] << 24);
            _position += 4;
            return result;
        }

        public override (T, U)[] ReadTupleArray<T, U>(Func<T> func1, Func<U> func2)
        {
            var count = ReadUInt32();
            var arr = new (T, U)[count];
            for (var i = 0; i < count; i++)
            {
                arr[i] = (func1.Invoke(), func2.Invoke());
            }
            return arr;
        }

        public override ushort ReadUInt16()
        {
            var result = (ushort)(Bytes[_position] | Bytes[_position + 1] << 8);
            _position += 2;
            return result;
        }

        public override uint ReadUInt32()
        {
            var result = (uint)(Bytes[_position] | Bytes[_position + 1] << 8 | Bytes[_position + 2] << 16 | Bytes[_position + 3] << 24);
            _position += 4;
            return result;
        }

        public override bool ReadUInt32AsBoolean()
        {
            return ReadUInt32() >= 1u;
        }

        public override T ReadUInt32AsEnum<T>()
        {
            return (T)Enum.ToObject(typeof(T), ReadUInt32());
        }

        public override ulong ReadUInt64()
        {
            var lo = (uint)(Bytes[_position] | Bytes[_position + 1] << 8 | Bytes[_position + 2] << 16 | Bytes[_position + 3] << 24);
            var hi = (uint)(Bytes[_position + 4] | Bytes[_position + 5] << 8 | Bytes[_position + 6] << 16 | Bytes[_position + 7] << 24);
            var result = (ulong)hi << 32 | lo;
            _position += 8;
            return result;
        }

        public override void Seek(int offset, SeekOrigin seekOrigin = SeekOrigin.Begin)
        {
            if (offset < 0 || offset > _length || (seekOrigin == SeekOrigin.Current && offset + _position > _length))
            {
                IsError = true;
                return;
            }

            _ = (seekOrigin switch
            {
                SeekOrigin.Begin => _position = offset,
                SeekOrigin.End => _position = _length - offset,
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
            _position += byteCount;
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Bytes = null;
                _isDisposed = true;
            }
        }
    }
}
