namespace Server.Items
{
	//[Alterable(typeof(DefTailoring), typeof(GargishSash))]
	[Flipable(0x1541, 0x1542)]
	public class ForTheLifeOfBritanniaSash : BaseMiddleTorso
	{
		public override int LabelNumber => 1075792;  // For the Life of Britannia Sash
		[Constructable]
		public ForTheLifeOfBritanniaSash()
			: this(0)
		{
		}

		[Constructable]
		public ForTheLifeOfBritanniaSash(int hue)
			: base(0x1541, hue)
		{
			Weight = 1.0;
		}

		public ForTheLifeOfBritanniaSash(Serial serial)
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
