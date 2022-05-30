using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Pepta : MondainQuester
	{
		[Constructable]
		public Pepta()
			: base("Pepta", "the Royal Tastetester")
		{
		}

		public Pepta(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(IThinkIOverateQuest)
				};
		public override void InitBody()
		{
			Female = true;
			Race = Race.Human;

			Hue = 0x83F4;
			HairItemID = 0x2049;
			HairHue = 0x46A;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Shoes(0x346));
			AddItem(new PlainDress(0x27B));
			AddItem(new HalfApron());
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
