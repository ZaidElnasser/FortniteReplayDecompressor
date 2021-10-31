using System;
using System.Buffers;

namespace OozSharp.MemoryPool
{
    public unsafe class PinnedMemory<T> : IDisposable
    {
        private static readonly PinnedMemory<T> _empty = new PinnedMemory<T>();

        public Memory<T> Memory { get; private set; }
        public MemoryHandle Handle { get; private set; }
        public void* Pointer { get; private set; }
        public int Length { get; private set; }

        private PinnedMemory()
        {
            Memory = Memory<T>.Empty;
        }

        public PinnedMemory(T[] bytes)
        {
            Memory = new Memory<T>(bytes);
            Handle = Memory.Pin();
            Pointer = Handle.Pointer;
            Length = bytes.Length;
        }

        public void Dispose()
        {
            Handle.Dispose();
            Memory = null;
            Handle = new MemoryHandle();
            Pointer = Handle.Pointer;
        }

        public static PinnedMemory<T> Empty()
        {
            return _empty;
        }
    }
}
