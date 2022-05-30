using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Rollarn : MondainQuester
	{
		[Constructable]
		public Rollarn()
			: base("Lorekeeper Rollarn", "the Keeper of Tradition")
		{
		}

		public Rollarn(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(DaemonicPrismQuest),
					typeof(HowManyHeadsQuest),
					typeof(GlassyFoeQuest),
					typeof(HailstormQuest),
					typeof(WarriorsOfTheGemkeeperQuest),
					typeof(BrotherlyLoveQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Elf;

			Hue = 0x84DE;
			HairItemID = 0x2FC1;
			HairHue = 0x320;
		}

		public override void InitOutfit()
		{
			AddItem(new Shoes(0x1BB));
			AddItem(new Circlet());
			AddItem(new Cloak(0x296));
			AddItem(new LeafChest());
			AddItem(new LeafArms());

			Item item;

			item = new LeafLegs
			{
				Hue = 0x74E
			};
			AddItem(item);
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
