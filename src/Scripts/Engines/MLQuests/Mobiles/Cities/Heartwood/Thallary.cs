using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Thallary : MondainQuester
	{
		[Constructable]
		public Thallary()
			: base("Thallary", "the Cloth Weaver")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public Thallary(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(ScaleArmorQuest),
					typeof(ThePuffyShirtQuest),
					typeof(FromTheGaultierCollectionQuest),
					typeof(HuteCoutureQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x8389;
			HairItemID = 0x2FCF;
			HairHue = 0x33;
		}

		public override void InitOutfit()
		{
			AddItem(new Sandals(0x901));
			AddItem(new LongPants(0x721));
			AddItem(new Cloak(0x3B3));
			AddItem(new FancyShirt(0x62));
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
