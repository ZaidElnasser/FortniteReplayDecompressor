using OozSharp.MemoryPool;
using System;
using System.Buffers;

namespace OozSharp
{
    public unsafe class KrakenDecoder : IDisposable
    {
        private bool _disposed;

        internal int SourceUsed { get; set; }
        internal int DestinationUsed { get; set; }
        internal KrakenHeader Header { get; set; }
        internal int ScratchSize { get; set; } = 0x6C000;
        internal byte* Scratch { get; set; }

        private IPinnedMemoryOwner<byte> _scratchOwner;

        public KrakenDecoder()
        {
            _scratchOwner = PinnedMemoryPool<byte>.Shared.Rent(ScratchSize);
            Scratch = (byte*)_scratchOwner.PinnedMemory.Pointer;

        }
        public void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _scratchOwner?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
