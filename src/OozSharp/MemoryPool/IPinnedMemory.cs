using System;

namespace OozSharp.MemoryPool
{
    public unsafe interface IPinnedMemoryOwner<T> : IDisposable
    {
        public PinnedMemory<T> PinnedMemory { get; }
    }
}
