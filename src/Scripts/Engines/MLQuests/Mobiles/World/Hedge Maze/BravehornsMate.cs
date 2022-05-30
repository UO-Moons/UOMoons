namespace Server.Mobiles
{
	public class BravehornsMate : Hind
	{
		[Constructable]
		public BravehornsMate()
			: base()
		{
			Name = "Bravehorn's Mate";
			Tamable = false;
		}

		public BravehornsMate(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
