using System;

namespace Unreal.Core.MemoryPool
{
    public unsafe interface IPinnedMemoryOwner<T> : IDisposable
    {
        public PinnedMemory<T> PinnedMemory { get; }
    }
}
