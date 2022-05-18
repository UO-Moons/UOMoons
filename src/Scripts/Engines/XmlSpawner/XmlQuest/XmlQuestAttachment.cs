using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlQuestAttachment : XmlAttachment
	{
		public DateTime Date { get; set; }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlQuestAttachment(ASerial serial)
			: base(serial)
		{
		}

		[Attachable]
		public XmlQuestAttachment(string name)
		{
			Name = name;
			Date = DateTime.UtcNow;
		}

		[Attachable]
		public XmlQuestAttachment(string name, double expiresin)
		{
			Name = name;
			Date = DateTime.UtcNow;
			Expiration = TimeSpan.FromMinutes(expiresin);

		}

		[Attachable]
		public XmlQuestAttachment(string name, DateTime value, double expiresin)
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
			if (from.AccessLevel == AccessLevel.Player)
				return null;

			if (Expiration > TimeSpan.Zero)
			{
				return string.Format("Quest '{2}' Completed {0} expires in {1} mins", Date, Expiration.TotalMinutes, Name);
			}
			else
			{
				return string.Format("Quest '{1}' Completed {0}", Date, Name);
			}
		}
	}
}
