using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Braen : MondainQuester
	{
		[Constructable]
		public Braen()
			: base("Braen", "the Thaumaturgist")
		{
			SetSkill(SkillName.Meditation, 60.0, 83.0);
			SetSkill(SkillName.Focus, 60.0, 83.0);
		}

		public Braen(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[] { typeof(UnholyConstructQuest) };
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Elf;

			Hue = 0x876C;
			HairItemID = 0x2FBF;
			HairHue = 0x2C2;
		}

		public override void InitOutfit()
		{
			AddItem(new Sandals(0x722));
			AddItem(RandomWand.CreateWand());
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
