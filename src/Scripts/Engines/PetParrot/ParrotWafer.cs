namespace Server.Items
{
	public sealed class ParrotWafer : Item
	{
		[Constructable]
		public ParrotWafer()
			: base(0x2FD6)
		{
			Hue = 0x38;
			Stackable = true;
		}

		public ParrotWafer(Serial serial)
			: base(serial)
		{
		}

		public override int LabelNumber => 1072904;// Parrot Wafers
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
