using System.IO;

namespace Server
{
	public sealed class BinaryMemoryWriter : BinaryFileWriter
	{
		private readonly MemoryStream _stream;

		protected override int BufferSize => 512;

		public BinaryMemoryWriter()
		 : base(new MemoryStream(512), true)
		{
			_stream = UnderlyingStream as MemoryStream;
		}

		private static byte[] _indexBuffer;

		public int CommitTo(SequentialFileWriter dataFile, SequentialFileWriter indexFile, int typeCode, int serial)
		{
			Flush();

			byte[] buffer = _stream.GetBuffer();
			int length = (int)_stream.Length;

			long position = dataFile.Position;

			dataFile.Write(buffer, 0, length);

			_indexBuffer ??= new byte[20];

			_indexBuffer[0] = (byte)(typeCode);
			_indexBuffer[1] = (byte)(typeCode >> 8);
			_indexBuffer[2] = (byte)(typeCode >> 16);
			_indexBuffer[3] = (byte)(typeCode >> 24);

			_indexBuffer[4] = (byte)(serial);
			_indexBuffer[5] = (byte)(serial >> 8);
			_indexBuffer[6] = (byte)(serial >> 16);
			_indexBuffer[7] = (byte)(serial >> 24);

			_indexBuffer[8] = (byte)(position);
			_indexBuffer[9] = (byte)(position >> 8);
			_indexBuffer[10] = (byte)(position >> 16);
			_indexBuffer[11] = (byte)(position >> 24);
			_indexBuffer[12] = (byte)(position >> 32);
			_indexBuffer[13] = (byte)(position >> 40);
			_indexBuffer[14] = (byte)(position >> 48);
			_indexBuffer[15] = (byte)(position >> 56);

			_indexBuffer[16] = (byte)(length);
			_indexBuffer[17] = (byte)(length >> 8);
			_indexBuffer[18] = (byte)(length >> 16);
			_indexBuffer[19] = (byte)(length >> 24);

			indexFile.Write(_indexBuffer, 0, _indexBuffer.Length);

			_stream.SetLength(0);

			return length;
		}
	}
}
