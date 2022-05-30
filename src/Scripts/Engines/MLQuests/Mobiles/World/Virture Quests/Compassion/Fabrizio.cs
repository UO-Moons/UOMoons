using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Fabrizio : MondainQuester
	{
		[Constructable]
		public Fabrizio()
			: base("Fabrizio", "the Master Weaponsmith")
		{
		}

		public Fabrizio(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(GentleBladeQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Human;

			Hue = 0x840E;
			HairItemID = 0x203D;
			HairHue = 0x1;
			FacialHairItemID = 0x203F;
			FacialHairHue = 0x1;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Shoes(0x753));
			AddItem(new LongPants(0x59C));
			AddItem(new HalfApron(0x8FD));
			AddItem(new Tunic(0x58F));
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
