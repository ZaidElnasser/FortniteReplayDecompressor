namespace Unreal.Core.MemoryPool
{
    internal sealed partial class PinnedArrayMemoryPool<T> : PinnedMemoryPool<T>
    {
        private const int MaximumBufferSize = int.MaxValue;

        public sealed override int MaxBufferSize => MaximumBufferSize;

        public sealed override IPinnedMemoryOwner<T> Rent(int minimumBufferSize = -1)
        {
            return new ArrayMemoryPoolBuffer(minimumBufferSize);
        }

        protected sealed override void Dispose(bool disposing) { }
    }

    internal sealed partial class PinnedArrayMemoryPool<T> : PinnedMemoryPool<T>
    {
        private sealed class ArrayMemoryPoolBuffer : IPinnedMemoryOwner<T>
        {
            private PinnedMemory<T> _array;
            private static readonly PinnedArrayPool<T> _pool = new PinnedArrayPool<T>();

            public PinnedMemory<T> PinnedMemory => _array;

            public ArrayMemoryPoolBuffer(int size)
            {
                _array = _pool.Rent(size);
            }

            public void Dispose()
            {
                var array = _array;

                if (array != null)
                {
                    _array = null;
                    _pool.Return(array);
                }
            }
        }
    }
}
