using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Lohn : MondainQuester
	{
		[Constructable]
		public Lohn()
			: base("Lohn", "the Metal Weaver")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public Lohn(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(CutsBothWaysQuest),
					typeof(DragonProtectionQuest),
					typeof(NothingFancyQuest),
					typeof(TheBulwarkQuest)
				};
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x8385;
			HairItemID = 0x2FC2;
			HairHue = 0x26B;
		}

		public override void InitOutfit()
		{
			AddItem(new Sandals(0x901));
			AddItem(new LongPants(0x359));
			AddItem(new ElvenShirt(0x359));
			AddItem(new SmithHammer());
			AddItem(new FullApron(0x1BB));
			AddItem(new GemmedCirclet());
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
