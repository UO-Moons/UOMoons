using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Lissbet : BaseEscort
	{
		public override Type[] Quests => new Type[] { typeof(ResponsibilityQuest) };

		[Constructable]
		public Lissbet()
			: base()
		{
			Name = "Lissbet";
			Title = "The Flower Girl";
		}

		public Lissbet(Serial serial)
			: base(serial)
		{
		}

		public override void Advertise()
		{
			Say(1074222); // Could I trouble you for some assistance?
		}

		public override void InitBody()
		{
			Female = true;
			Race = Race.Human;

			Hue = 0x8411;
			HairItemID = 0x203D;
			HairHue = 0x1BB;
		}

		public override void InitOutfit()
		{
			AddItem(new Backpack());
			AddItem(new Sandals());
			AddItem(new FancyShirt(0x6BF));
			AddItem(new Kilt(0x6AA));
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
