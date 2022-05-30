namespace Server.Items
{
	public class CrossBowBolt : BaseItem, ICommodity
	{
		TextDefinition ICommodity.Description => LabelNumber;
		bool ICommodity.IsDeedable => true;

		public override double DefaultWeight => 0.1;

		[Constructable]
		public CrossBowBolt() : this(1)
		{
		}

		[Constructable]
		public CrossBowBolt(int amount) : base(0x1BFB)
		{
			Stackable = true;
			Amount = amount;
		}

		public CrossBowBolt(Serial serial) : base(serial)
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
