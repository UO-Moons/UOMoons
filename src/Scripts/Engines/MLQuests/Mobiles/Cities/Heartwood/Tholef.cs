using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Tholef : MondainQuester
	{
		[Constructable]
		public Tholef()
			: base("Tholef", "the Grape Tender")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public Tholef(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(TheSongOfTheWindQuest),
					typeof(BeerGogglesQuest),
					typeof(MessageInBottleQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x876C;
			HairItemID = 0x2FC2;
			HairHue = 0x15A;
		}

		public override void InitOutfit()
		{
			AddItem(new Sandals(0x901));
			AddItem(new ShortPants(0x28C));
			AddItem(new Shirt(0x28C));
			AddItem(new FullApron(0x72B));

			Item item;

			item = new LeafArms();
			item.Hue = 0x28C;
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
