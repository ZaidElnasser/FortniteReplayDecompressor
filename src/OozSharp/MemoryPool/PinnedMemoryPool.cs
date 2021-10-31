using System;

namespace OozSharp.MemoryPool
{
    public abstract class PinnedMemoryPool<T> : IDisposable
    {
        // Store the shared ArrayMemoryPool in a field of its derived sealed type so the Jit can "see" the exact type
        // when the Shared property is inlined which will allow it to devirtualize calls made on it.
        private static readonly PinnedArrayMemoryPool<T> s_shared = new PinnedArrayMemoryPool<T>();

        /// <summary>
        /// Returns a singleton instance of a MemoryPool based on arrays.
        /// </summary>
        public static PinnedMemoryPool<T> Shared => s_shared;
        public abstract IPinnedMemoryOwner<T> Rent(int minBufferSize = -1);

        /// <summary>
        /// Returns the maximum buffer size supported by this pool.
        /// </summary>
        public abstract int MaxBufferSize { get; }

        /// <summary>
        /// Constructs a new instance of a memory pool.
        /// </summary>
        protected PinnedMemoryPool() { }

        /// <summary>
        /// Frees all resources used by the memory pool.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Frees all resources used by the memory pool.
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Dispose(bool disposing);
    }
}
