namespace Server.Items
{
	public class FreshGinger : Item
	{
		public override int LabelNumber => 1031235;

		[Constructable]
		public FreshGinger()
			: base(11235)
		{
		}

		public FreshGinger(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
