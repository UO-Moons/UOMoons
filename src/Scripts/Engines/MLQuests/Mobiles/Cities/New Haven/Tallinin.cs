using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Tallinin : MondainQuester
	{
		[Constructable]
		public Tallinin()
			: base("Tallinin", "the Cloth Weaver")
		{
		}

		public Tallinin(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(ImpressivePlaidQuest),
					typeof(NiceShirtQuest),
					typeof(LeatherAndLaceQuest),
					typeof(FeyHeadgearQuest),
					typeof(ScaleArmorQuest),
					typeof(NewCloakQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			CantWalk = true;
			Race = Race.Elf;

			Hue = 0x876C;
			HairItemID = 0x2FC0;
			HairHue = 0x26B;
		}

		public override void InitOutfit()
		{
			AddItem(new ElvenBoots(0x901));
			AddItem(new Tunic(0x62));
			AddItem(new Cloak(0x71E));
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
