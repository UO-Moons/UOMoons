namespace Server.Items
{
	[Flipable(0x1c00, 0x1c01)]
	public class LeatherShorts : BaseArmor
	{
		public override int BasePhysicalResistance => 2;
		public override int BaseFireResistance => 4;
		public override int BaseColdResistance => 3;
		public override int BasePoisonResistance => 3;
		public override int BaseEnergyResistance => 3;
		public override int InitHits => Utility.RandomMinMax(30, 40);
		public override int StrReq => Core.AOS ? 20 : 10;
		public override int ArmorBase => 13;
		public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
		public override CraftResource DefaultResource => CraftResource.RegularLeather;
		public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;
		public override bool AllowMaleWearer => false;

		[Constructable]
		public LeatherShorts() : base(0x1C00)
		{
			Weight = 3.0;
		}

		public LeatherShorts(Serial serial) : base(serial)
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
			int version = reader.ReadInt();
		}
	}
}
