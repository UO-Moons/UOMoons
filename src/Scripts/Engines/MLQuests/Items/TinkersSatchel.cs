using Server.Engines.Quests;

namespace Server.Items
{
	public class TinkersSatchel : BaseContainer, IDyable
	{
		[Constructable]
		public TinkersSatchel()
			: base(0xE75)
		{
			Hue = BaseReward.SatchelHue();
			Layer = Layer.Backpack;
			Weight = 3.0;
			AddItem(new TinkerTools());

			switch (Utility.Random(5))
			{
				case 0: AddItem(new Springs(3)); break;
				case 1: AddItem(new Axle(3)); break;
				case 2: AddItem(new Hinge(3)); break;
				case 3: AddItem(new Key()); break;
				case 4: AddItem(new Scissors()); break;
			}
		}

		public TinkersSatchel(Serial serial)
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
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
