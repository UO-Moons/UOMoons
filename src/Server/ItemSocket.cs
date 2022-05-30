using System;

namespace Server
{
	public class ItemSocket
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public Item Owner { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Expires { get; set; }

		public virtual TimeSpan TickDuration => TimeSpan.FromMinutes(1);

		public Timer Timer { get; set; }

		public ItemSocket()
			: this(TimeSpan.Zero)
		{
		}

		public ItemSocket(TimeSpan duration)
		{
			if (duration != TimeSpan.Zero)
			{
				Expires = DateTime.UtcNow + duration;

				BeginTimer();
			}
		}

		protected void BeginTimer()
		{
			EndTimer();

			Timer = Timer.DelayCall(TickDuration, TickDuration, OnTick);
			Timer.Start();
		}

		protected void EndTimer()
		{
			if (Timer != null)
			{
				Timer.Stop();
				Timer = null;
			}
		}

		protected virtual void OnTick()
		{
			if (Expires < DateTime.UtcNow || Owner.Deleted)
			{
				Remove();
			}
		}

		public virtual void Remove()
		{
			EndTimer();

			Owner.RemoveItemSocket(this);
		}

		public virtual void OnRemoved()
		{
		}

		public virtual void GetProperties(ObjectPropertyList list)
		{
		}

		public virtual void OnOwnerDuped(Item newItem)
		{
			ItemSocket newSocket = null;

			try
			{
				newSocket = Activator.CreateInstance(GetType()) as ItemSocket;
			}
			catch
			{
				Console.WriteLine(
					"Warning: 0x{0:X}: Item socket must have a zero paramater constructor to be separated from a stack. '{1}'.",
					Owner.Serial.Value,
					GetType().Name);
			}

			if (newSocket != null)
			{
				newSocket.Expires = Expires;

				if (newSocket.Expires != DateTime.MinValue)
				{
					newSocket.BeginTimer();
				}

				newSocket.OnAfterDuped(this);
				newItem.AttachSocket(newSocket);
			}
		}

		public virtual void OnAfterDuped(ItemSocket oldSocket)
		{
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(0);
			writer.Write(Expires);
		}

		public virtual void Deserialize(Item owner, GenericReader reader)
		{
			reader.ReadInt(); // version

			Expires = reader.ReadDateTime();

			if (Expires != DateTime.MinValue)
			{
				if (Expires < DateTime.UtcNow)
				{
					return;
				}
				else
				{
					BeginTimer();
				}
			}

			owner.AttachSocket(this);
		}

		public static void Save(ItemSocket socket, GenericWriter writer)
		{
			writer.Write(socket.GetType().Name);
			socket.Serialize(writer);
		}

		public static void Load(Item item, GenericReader reader)
		{
			var typeName = Assembler.FindTypeByName(reader.ReadString());
			var socket = Activator.CreateInstance(typeName) as ItemSocket;

			socket.Deserialize(item, reader);
		}
	}
}
