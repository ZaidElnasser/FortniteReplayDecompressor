using System;
using Unreal.Core.MemoryPool;

namespace Unreal.Core
{
    public unsafe partial class BitReader
    {
        protected bool* Bits;

        protected ReadOnlyMemory<bool> Items { get; set; }
        private IPinnedMemoryOwner<bool>? _owner;

        public void SetBits(byte* ptr, int byteCount, int bitCount)
        {
            CreateBitArray(ptr, byteCount, bitCount);
            _position = 0;
        }

        public void DisposeBits()
        {
            _owner?.Dispose();
            _owner = null;
            Items = null;
            Bits = null;
        }

        private void CreateBitArray(byte* ptr, int byteCount, int totalBits)
        {
            _owner = PinnedMemoryPool<bool>.Shared.Rent(totalBits);
            Items = _owner.PinnedMemory.Memory;
            LastBit = totalBits;
            Bits = (bool*)_owner.PinnedMemory.Pointer;

            for (var i = 0; i < byteCount; i++)
            {
                var offset = i * 8;
                var deref = *(ptr + i);

                *(Bits + offset) = (deref & 0x01) == 0x01;
                *(Bits + offset + 1) = (deref & 0x02) == 0x02;
                *(Bits + offset + 2) = (deref & 0x04) == 0x04;
                *(Bits + offset + 3) = (deref & 0x08) == 0x08;
                *(Bits + offset + 4) = (deref & 0x10) == 0x10;
                *(Bits + offset + 5) = (deref & 0x20) == 0x20;
                *(Bits + offset + 6) = (deref & 0x40) == 0x40;
                *(Bits + offset + 7) = (deref & 0x80) == 0x80;
            }
        }

        private void AppendBits(ReadOnlyMemory<bool> after)
        {
            var newOwner = PinnedMemoryPool<bool>.Shared.Rent(after.Length + LastBit);
            var newMemory = newOwner.PinnedMemory.Memory;
            var oldLength = LastBit;

            //Copy old array
            Items.CopyTo(newMemory);

            DisposeBits(); //Get rid of old

            Items = newMemory;

            _owner = newOwner;
            Bits = (bool*)_owner.PinnedMemory.Pointer;

            var afterPin = after.Pin();

            Buffer.MemoryCopy(afterPin.Pointer, Bits + oldLength, after.Length, after.Length);

            afterPin.Dispose();

            LastBit = after.Length + LastBit;
        }

        protected byte GetAsByte(int index)
        {
            return (*(byte*)(Bits + index));
        }
    }
}
