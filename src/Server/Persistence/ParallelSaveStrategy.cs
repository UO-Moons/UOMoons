using Server.Guilds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Server
{
	public sealed class ParallelSaveStrategy : SaveStrategy
	{
		public override string Name => "Parallel";

		private readonly int _processorCount;

		public ParallelSaveStrategy(int processorCount)
		{
			_processorCount = processorCount;

			_decayQueue = new Queue<Item>();
		}

		private int GetThreadCount()
		{
			return _processorCount - 1;
		}

		private SequentialFileWriter _itemData, _itemIndex;
		private SequentialFileWriter _mobileData, _mobileIndex;
		private SequentialFileWriter _guildData, _guildIndex;

		private readonly Queue<Item> _decayQueue;

		private Consumer[] _consumers;
		private int _cycle;

		private bool _finished;

		public override void Save(bool permitBackgroundWrite)
		{
			OpenFiles();

			_consumers = new Consumer[GetThreadCount()];

			for (var i = 0; i < _consumers.Length; ++i)
			{
				_consumers[i] = new Consumer(this, 256);
			}

			IEnumerable<ISerializable> collection = new Producer();

			foreach (ISerializable value in collection)
			{
				while (!Enqueue(value))
				{
					if (!Commit())
					{
						Thread.Sleep(0);
					}
				}
			}

			_finished = true;

			SaveTypeDatabases();

			WaitHandle.WaitAll(
				Array.ConvertAll<Consumer, WaitHandle>(
					_consumers,
					input => input.CompletionEvent
				)
			);

			Commit();

			CloseFiles();
		}

		public override void ProcessDecay()
		{
			while (_decayQueue.Count > 0)
			{
				Item item = _decayQueue.Dequeue();

				if (item.OnDecay())
				{
					item.Delete();
				}
			}
		}

		private static void SaveTypeDatabases()
		{
			SaveTypeDatabase(World.ItemTypesPath, World.m_ItemTypes);
			SaveTypeDatabase(World.MobileTypesPath, World.m_MobileTypes);
		}

		private static void SaveTypeDatabase(string path, List<Type> types)
		{
			BinaryFileWriter bfw = new(path, false);

			bfw.Write(types.Count);

			foreach (Type type in types)
			{
				bfw.Write(type.FullName);
			}

			bfw.Flush();

			bfw.Close();
		}

		private void OpenFiles()
		{
			_itemData = new SequentialFileWriter(World.ItemDataPath);
			_itemIndex = new SequentialFileWriter(World.ItemIndexPath);

			_mobileData = new SequentialFileWriter(World.MobileDataPath);
			_mobileIndex = new SequentialFileWriter(World.MobileIndexPath);

			_guildData = new SequentialFileWriter(World.GuildDataPath);
			_guildIndex = new SequentialFileWriter(World.GuildIndexPath);

			WriteCount(_itemIndex, World.Items.Count);
			WriteCount(_mobileIndex, World.Mobiles.Count);
			WriteCount(_guildIndex, World.Guilds.Count);
		}

		private static void WriteCount(SequentialFileWriter indexFile, int count)
		{
			byte[] buffer = new byte[4];

			buffer[0] = (byte)(count);
			buffer[1] = (byte)(count >> 8);
			buffer[2] = (byte)(count >> 16);
			buffer[3] = (byte)(count >> 24);

			indexFile.Write(buffer, 0, buffer.Length);
		}

		private void CloseFiles()
		{
			_itemData.Close();
			_itemIndex.Close();

			_mobileData.Close();
			_mobileIndex.Close();

			_guildData.Close();
			_guildIndex.Close();

			World.NotifyDiskWriteComplete();
		}

		private void OnSerialized(ConsumableEntry entry)
		{
			ISerializable value = entry.Value;
			BinaryMemoryWriter writer = entry.Writer;

			switch (value)
			{
				case Item item:
					Save(item, writer);
					break;
				case Mobile mob:
					Save(mob, writer);
					break;
				case BaseGuild guild:
					Save(guild, writer);
					break;
			}
		}

		private void Save(Item item, BinaryMemoryWriter writer)
		{
			_ = writer.CommitTo(_itemData, _itemIndex, item.MTypeRef, item.Serial);

			if (item.Decays && item.Parent == null && item.Map != Map.Internal && DateTime.UtcNow > (item.LastMoved + item.DecayTime))
			{
				_decayQueue.Enqueue(item);
			}
		}

		private void Save(Mobile mob, BinaryMemoryWriter writer)
		{
			_ = writer.CommitTo(_mobileData, _mobileIndex, mob.MTypeRef, mob.Serial);
		}

		private void Save(BaseGuild guild, BinaryMemoryWriter writer)
		{
			_ = writer.CommitTo(_guildData, _guildIndex, 0, guild.Serial);
		}

		private bool Enqueue(ISerializable value)
		{
			for (var i = 0; i < _consumers.Length; ++i)
			{
				Consumer consumer = _consumers[_cycle++ % _consumers.Length];

				if (consumer.Tail - consumer.Head >= consumer.Buffer.Length) continue;
				consumer.Buffer[consumer.Tail % consumer.Buffer.Length].Value = value;
				consumer.Tail++;

				return true;
			}

			return false;
		}

		private bool Commit()
		{
			bool committed = false;

			for (var i = 0; i < _consumers.Length; ++i)
			{
				Consumer consumer = _consumers[i];

				while (consumer.Head < consumer.Done)
				{
					OnSerialized(consumer.Buffer[consumer.Head % consumer.Buffer.Length]);
					consumer.Head++;

					committed = true;
				}
			}

			return committed;
		}

		private sealed class Producer : IEnumerable<ISerializable>
		{
			private readonly IEnumerable<Item> _items;
			private readonly IEnumerable<Mobile> _mobiles;
			private readonly IEnumerable<BaseGuild> _guilds;

			public Producer()
			{
				_items = World.Items.Values;
				_mobiles = World.Mobiles.Values;
				_guilds = World.Guilds.Values;
			}

			public IEnumerator<ISerializable> GetEnumerator()
			{
				foreach (Item item in _items)
				{
					yield return item;
				}

				foreach (Mobile mob in _mobiles)
				{
					yield return mob;
				}

				foreach (BaseGuild guild in _guilds)
				{
					yield return guild;
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				throw new NotImplementedException();
			}
		}

		private struct ConsumableEntry
		{
			public ISerializable Value;
			public BinaryMemoryWriter Writer;
		}

		private sealed class Consumer
		{
			private readonly ParallelSaveStrategy _owner;

			public readonly ManualResetEvent CompletionEvent;

			public readonly ConsumableEntry[] Buffer;
			public int Head, Done, Tail;

			public Consumer(ParallelSaveStrategy owner, int bufferSize)
			{
				_owner = owner;

				Buffer = new ConsumableEntry[bufferSize];

				for (var i = 0; i < Buffer.Length; ++i)
				{
					Buffer[i].Writer = new BinaryMemoryWriter();
				}

				CompletionEvent = new ManualResetEvent(false);

				var thread = new Thread(Processor)
				{
					Name = "Parallel Serialization Thread"
				};
				thread.Start();
			}

			private void Processor()
			{
				try
				{
					while (!_owner._finished)
					{
						Process();
						Thread.Sleep(0);
					}

					Process();

					CompletionEvent.Set();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			private void Process()
			{
				while (Done < Tail)
				{
					var entry = Buffer[Done % Buffer.Length];

					entry.Value.Serialize(entry.Writer);

					++Done;
				}
			}
		}
	}
}
