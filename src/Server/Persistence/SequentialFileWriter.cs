using System;
using System.IO;

namespace Server
{
	public sealed class SequentialFileWriter : Stream
	{
		private FileStream _fileStream;
		private FileQueue _fileQueue;

		private AsyncCallback _writeCallback;

		public SequentialFileWriter(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			_fileStream = FileOperations.OpenSequentialStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

			_fileQueue = new FileQueue(
				Math.Max(1, FileOperations.Concurrency),
				FileCallback
			);
		}

		public override long Position
		{
			get => _fileQueue.Position;
			set => throw new InvalidOperationException();
		}

		private void FileCallback(FileQueue.Chunk chunk)
		{
			if (FileOperations.AreSynchronous)
			{
				_fileStream.Write(chunk.Buffer, FileQueue.Chunk.Offset, chunk.Size);

				chunk.Commit();
			}
			else
			{
				_writeCallback ??= OnWrite;

				_fileStream.BeginWrite(chunk.Buffer, FileQueue.Chunk.Offset, chunk.Size, _writeCallback, chunk);
			}
		}

		private void OnWrite(IAsyncResult asyncResult)
		{
			FileQueue.Chunk chunk = asyncResult.AsyncState as FileQueue.Chunk;

			_fileStream.EndWrite(asyncResult);

			chunk?.Commit();
		}

		public override void Write(byte[] buffer, int offset, int size)
		{
			_fileQueue.Enqueue(buffer, offset, size);
		}

		public override void Flush()
		{
			_fileQueue.Flush();
			_fileStream.Flush();
		}

		protected override void Dispose(bool disposing)
		{
			if (_fileStream != null)
			{
				Flush();

				_fileQueue.Dispose();
				_fileQueue = null;

				_fileStream.Close();
				_fileStream = null;
			}

			base.Dispose(disposing);
		}

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => Position;

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new InvalidOperationException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException();
		}

		public override void SetLength(long value)
		{
			_fileStream.SetLength(value);
		}
	}
}
