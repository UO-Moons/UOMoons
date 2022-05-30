using Server.Items;
using System;

namespace Server.Mobiles
{
	public class JackLoanShark : MondainQuester
	{
		public override Type[] Quests => null;   //JackTheVillainQuest

		[Constructable]
		public JackLoanShark()
			: base("Jack", "the Loan Shark")
		{
		}

		public JackLoanShark(Serial serial)
			: base(serial)
		{
		}

		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Female = false;
			Race = Race.Human;

			Hue = 0x83EC;
			HairItemID = 0x2045;
			HairHue = 0x464;
			FacialHairItemID = 0x204B;
			FacialHairHue = 0x464;
		}

		public override void InitOutfit()
		{
			AddItem(new Dagger());
			AddItem(new ThighBoots(0x901));
			AddItem(new LongPants(0x521));
			AddItem(new FancyShirt(0x5A7));
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
