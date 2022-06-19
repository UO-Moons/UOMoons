using Server.Network;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
	public delegate void FileCommitCallback(FileQueue.Chunk chunk);

	public sealed class FileQueue : IDisposable
	{
		public sealed class Chunk
		{
			private readonly FileQueue _owner;
			private readonly int _slot;
			private readonly int _offset;

			public byte[] Buffer { get; }

			public static int Offset => 0;

			public int Size { get; }

			public Chunk(FileQueue owner, int slot, byte[] buffer, int offset, int size)
			{
				_owner = owner;
				_slot = slot;

				Buffer = buffer;
				_offset = offset;
				Size = size;
			}

			public void Commit()
			{
				_owner.Commit(this, _slot);
			}
		}

		private struct Page
		{
			public byte[] Buffer;
			public int Length;
		}

		private static readonly int BufferSize;
		private static readonly BufferPool BufferPool;

		static FileQueue()
		{
			BufferSize = FileOperations.BufferSize;
			BufferPool = new BufferPool("File Buffers", 64, BufferSize);
		}

		private readonly object _syncRoot;

		private readonly Chunk[] _active;
		private int _activeCount;

		private readonly Queue<Page> _pending;
		private Page _buffered;

		private readonly FileCommitCallback _callback;

		private ManualResetEvent _idle;

		public long Position { get; private set; }

		public FileQueue(int concurrentWrites, FileCommitCallback callback)
		{
			if (concurrentWrites < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(concurrentWrites));
			}

			if (BufferSize < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(concurrentWrites));
			}

			_syncRoot = new object();

			_active = new Chunk[concurrentWrites];
			_pending = new Queue<Page>();

			this._callback = callback ?? throw new ArgumentNullException(nameof(callback));

			_idle = new ManualResetEvent(true);
		}

		private void Append(Page page)
		{
			lock (_syncRoot)
			{
				if (_activeCount == 0)
				{
					_idle.Reset();
				}

				++_activeCount;

				for (int slot = 0; slot < _active.Length; ++slot)
				{
					if (_active[slot] == null)
					{
						_active[slot] = new Chunk(this, slot, page.Buffer, 0, page.Length);

						_callback(_active[slot]);

						return;
					}
				}

				_pending.Enqueue(page);
			}
		}

		public void Dispose()
		{
			if (_idle == null) return;
			_idle.Close();
			_idle = null;
		}

		public void Flush()
		{
			if (_buffered.Buffer != null)
			{
				Append(_buffered);

				_buffered.Buffer = null;
				_buffered.Length = 0;
			}

			/*lock ( syncRoot ) {
				if ( pending.Count > 0 ) {
					idle.Reset();
				}

				for ( int slot = 0; slot < active.Length && pending.Count > 0; ++slot ) {
					if ( active[slot] == null ) {
						Page page = pending.Dequeue();

						active[slot] = new Chunk( this, slot, page.buffer, 0, page.length );

						++activeCount;

						callback( active[slot] );
					}
				}
			}*/

			_idle.WaitOne();
		}

		private void Commit(Chunk chunk, int slot)
		{
			if (slot < 0 || slot >= _active.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(slot));
			}

			lock (_syncRoot)
			{
				if (_active[slot] != chunk)
				{
					throw new ArgumentException();
				}

				BufferPool.ReleaseBuffer(chunk.Buffer);

				if (_pending.Count > 0)
				{
					Page page = _pending.Dequeue();

					_active[slot] = new Chunk(this, slot, page.Buffer, 0, page.Length);

					_callback(_active[slot]);
				}
				else
				{
					_active[slot] = null;
				}

				--_activeCount;

				if (_activeCount == 0)
				{
					_idle.Set();
				}
			}
		}

		public void Enqueue(byte[] buffer, int offset, int size)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}

			if (size < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(size));
			}

			if (buffer.Length - offset < size)
			{
				throw new ArgumentException();
			}

			Position += size;

			while (size > 0)
			{
				_buffered.Buffer ??= BufferPool.AcquireBuffer();

				byte[] page = _buffered.Buffer; // buffer page
				int pageSpace = page.Length - _buffered.Length; // available bytes in page
				int byteCount = (size > pageSpace ? pageSpace : size); // how many bytes we can copy over

				Buffer.BlockCopy(buffer, offset, page, _buffered.Length, byteCount);

				_buffered.Length += byteCount;
				offset += byteCount;
				size -= byteCount;

				if (_buffered.Length != page.Length) continue; // page full
				Append(_buffered);

				_buffered.Buffer = null;
				_buffered.Length = 0;
			}
		}
	}
}
