using Server.Engines.Quests;

namespace Server.Items
{
	public class MinersQuestSatchel : BaseContainer, IDyable
	{
		[Constructable]
		public MinersQuestSatchel()
			: base(0xE75)
		{
			Hue = BaseReward.SatchelHue();
			Layer = Layer.Backpack;
			Weight = 3.0;
			AddItem(new Pickaxe());
			AddItem(new Pickaxe());
		}

		public MinersQuestSatchel(Serial serial)
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
