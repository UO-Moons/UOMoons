using System.Collections.Generic;
using System.IO;

namespace Server
{
	public sealed class QueuedMemoryWriter : BinaryFileWriter
	{
		private struct IndexInfo
		{
			public int Size;
			public int TypeCode;
			public int Serial;
		}

		private readonly MemoryStream _memStream;
		private readonly List<IndexInfo> _orderedIndexInfo = new();

		protected override int BufferSize => 512;

		public QueuedMemoryWriter()
			: base(new MemoryStream(1024 * 1024), true)
		{
			_memStream = UnderlyingStream as MemoryStream;
		}

		public void QueueForIndex(ISerializable serializable, int size)
		{
			IndexInfo info;

			info.Size = size;

			info.TypeCode = serializable.TypeReference; //For guilds, this will automagically be zero.
			info.Serial = serializable.SerialIdentity;

			_orderedIndexInfo.Add(info);
		}

		public void CommitTo(SequentialFileWriter dataFile, SequentialFileWriter indexFile)
		{
			Flush();

			int memLength = (int)_memStream.Position;

			if (memLength > 0)
			{
				byte[] memBuffer = _memStream.GetBuffer();

				long actualPosition = dataFile.Position;

				dataFile.Write(memBuffer, 0, memLength);    //The buffer contains the data from many items.

				//Console.WriteLine("Writing {0} bytes starting at {1}, with {2} things", memLength, actualPosition, _orderedIndexInfo.Count);

				byte[] indexBuffer = new byte[20];

				//int indexWritten = _orderedIndexInfo.Count * indexBuffer.Length;
				//int totalWritten = memLength + indexWritten

				for (int i = 0; i < _orderedIndexInfo.Count; i++)
				{
					IndexInfo info = _orderedIndexInfo[i];
					_ = info.TypeCode;
					_ = info.Serial;
					_ = info.Size;


					indexBuffer[0] = (byte)(info.TypeCode);
					indexBuffer[1] = (byte)(info.TypeCode >> 8);
					indexBuffer[2] = (byte)(info.TypeCode >> 16);
					indexBuffer[3] = (byte)(info.TypeCode >> 24);

					indexBuffer[4] = (byte)(info.Serial);
					indexBuffer[5] = (byte)(info.Serial >> 8);
					indexBuffer[6] = (byte)(info.Serial >> 16);
					indexBuffer[7] = (byte)(info.Serial >> 24);

					indexBuffer[8] = (byte)(actualPosition);
					indexBuffer[9] = (byte)(actualPosition >> 8);
					indexBuffer[10] = (byte)(actualPosition >> 16);
					indexBuffer[11] = (byte)(actualPosition >> 24);
					indexBuffer[12] = (byte)(actualPosition >> 32);
					indexBuffer[13] = (byte)(actualPosition >> 40);
					indexBuffer[14] = (byte)(actualPosition >> 48);
					indexBuffer[15] = (byte)(actualPosition >> 56);

					indexBuffer[16] = (byte)(info.Size);
					indexBuffer[17] = (byte)(info.Size >> 8);
					indexBuffer[18] = (byte)(info.Size >> 16);
					indexBuffer[19] = (byte)(info.Size >> 24);

					indexFile.Write(indexBuffer, 0, indexBuffer.Length);

					actualPosition += info.Size;
				}
			}

			Close();   //We're done with this writer.
		}
	}
}
