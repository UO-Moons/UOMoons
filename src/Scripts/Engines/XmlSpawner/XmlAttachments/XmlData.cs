using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlData : XmlAttachment
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public string Data { get; set; } = null;

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlData(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlData(string name)
		{
			Name = name;
			Data = string.Empty;
		}

		[Attachable]
		public XmlData(string name, string data)
		{
			Name = name;
			Data = data;
		}

		[Attachable]
		public XmlData(string name, string data, double expiresin)
		{
			Name = name;
			Data = data;
			Expiration = TimeSpan.FromMinutes(expiresin);

		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Data);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			Data = reader.ReadString();
		}

		public override string OnIdentify(Mobile from)
		{
			if (from == null || from.AccessLevel == AccessLevel.Player)
				return null;

			return Expiration > TimeSpan.Zero ? $"{Name}: Data {Data} expires in {Expiration.TotalMinutes} mins" : $"{Name}: Data {Data}";
		}
	}
}
