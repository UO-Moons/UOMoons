using Server.Engines.Quests;

namespace Server.Items
{
	public class ChefsSatchel : BaseContainer, IDyable
	{
		[Constructable]
		public ChefsSatchel()
			: base(0xE75)
		{
			Hue = BaseReward.SatchelHue();
			Layer = Layer.Backpack;
			Weight = 3.0;
			AddItem(new SackFlour());
			AddItem(new Skillet());
		}

		public ChefsSatchel(Serial serial)
			: base(serial)
		{
		}
		public bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			Hue = sender.DyedHue;

			return true;
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
