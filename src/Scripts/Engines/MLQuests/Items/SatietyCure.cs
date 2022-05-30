namespace Server.Items
{
	public class SatietyCure : Item
	{
		public override int LabelNumber => 1080542;  // Pepta's Satiety Cure

		[CommandProperty(AccessLevel.GameMaster)]
		public int Uses { get; set; }

		[Constructable]
		public SatietyCure() : base(0xEFC)
		{
			Weight = 1.0;
			Hue = 235;
			LootType = LootType.Blessed;
			Uses = 10;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				SendLocalizedMessageTo(from, 1042038); // You must have the object in your backpack to use it.
				return;
			}

			if (Uses > 0)
			{
				from.PlaySound(0x2D6);
				from.SendLocalizedMessage(501206); // An awful taste fills your mouth.

				if (from.Hunger > 0)
				{
					from.Hunger = 0;
					from.SendMessage("You feel as if you could eat more.");
				}

				Uses--;
			}
			else
			{
				Delete();
				from.SendLocalizedMessage(501201); // There wasn't enough left to have any effect.
			}
		}

		public SatietyCure(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
			writer.WriteEncodedInt(Uses);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			Uses = reader.ReadEncodedInt();
		}
	}
}
