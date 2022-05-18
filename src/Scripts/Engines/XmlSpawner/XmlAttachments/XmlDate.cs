using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlDate : XmlAttachment
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime Date { get; set; }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlDate(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlDate(string name)
		{
			Name = name;
			Date = DateTime.UtcNow;
		}

		[Attachable]
		public XmlDate(string name, double expiresin)
		{
			Name = name;
			Date = DateTime.UtcNow;
			Expiration = TimeSpan.FromMinutes(expiresin);

		}

		[Attachable]
		public XmlDate(string name, DateTime value, double expiresin)
		{
			Name = name;
			Date = value;
			Expiration = TimeSpan.FromMinutes(expiresin);

		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Date);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			Date = reader.ReadDateTime();
		}

		public override string OnIdentify(Mobile from)
		{
			if (from == null || from.AccessLevel == AccessLevel.Player)
				return null;

			return Expiration > TimeSpan.Zero ? $"{Name}: Date {Date} expires in {Expiration.TotalMinutes} mins" : $"{Name}: Date {Date}";
		}
	}
}
