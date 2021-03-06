namespace Server.Items
{
	public class RedLeatherBook : BaseBook
	{
		[Constructable]
		public RedLeatherBook()
			: base(0xFF2)
		{
			Hue = 0x485;
		}

		public RedLeatherBook(Serial serial)
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
