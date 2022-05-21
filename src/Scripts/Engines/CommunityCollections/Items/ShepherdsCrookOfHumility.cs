namespace Server.Items
{
	public class ShepherdsCrookOfHumility : BaseStaff
	{
		public override int LabelNumber => 1075856;  // Shepherd's Crook of Humility (Replica)

		[Constructable]
		public ShepherdsCrookOfHumility()
			: base(0xE81)
		{
			Weight = 4.0;
		}

		public override WeaponAbility PrimaryAbility => WeaponAbility.CrushingBlow;
		public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;

		public override int StrReq => Core.AOS ? 20 : 10;
		public override int MinDamageBase => Core.AOS ? 13 : 3;
		public override int MaxDamageBase => Core.AOS ? 16 : 12;
		public override float SpeedBase => Core.ML ? 2.75f : Core.AOS ? 40 : 30;

		public override int InitMinHits => 31;
		public override int InitMaxHits => 50;

		//public override bool CanBeWornByGargoyles => true;

		public ShepherdsCrookOfHumility(Serial serial)
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

		/*public override void OnDoubleClick(Mobile from)
        {needs fixed
            from.SendLocalizedMessage(502464); // Target the animal you wish to herd.
            from.Target = new HerdingTarget(this);
        }*/
	}
}
