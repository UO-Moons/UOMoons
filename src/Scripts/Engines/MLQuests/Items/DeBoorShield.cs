namespace Server.Items
{
	public class DeBoorShield : BaseShield, IDyable
	{
		public override int LabelNumber => 1075308;  // Ancestral Shield
		public override bool HiddenQuestItemHue => true;

		[Constructable]
		public DeBoorShield()
			: base(0x1B74)
		{
			LootType = LootType.Blessed;
			Weight = 7.0;
		}
		public override int BasePhysicalResistance => 0;
		public override int BaseFireResistance => 0;
		public override int BaseColdResistance => 0;
		public override int BasePoisonResistance => 0;
		public override int BaseEnergyResistance => 1;
		public override int InitMinHits => 45;
		public override int InitMaxHits => 60;
		public override int StrReq => 45;
		public override int ArmorBase => 16;

		public bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}
			Hue = sender.DyedHue;
			return true;
		}
		public DeBoorShield(Serial serial)
			: base(serial)
		{
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);//version
		}
	}
}
