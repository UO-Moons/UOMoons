using System;

namespace Server.Mobiles
{
	public class Szandor : MondainQuester
	{
		[Constructable]
		public Szandor()
			: base("Skeleton of Szandor", "the Late Architect")
		{
		}

		public Szandor(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => null;
		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Body = 0x32;
			Hue = 0x83F2;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
