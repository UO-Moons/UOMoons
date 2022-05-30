using Server.Engines.Quests;
using System;

namespace Server.Mobiles
{
	public class Bravehorn : BaseEscort
	{
		public override Type[] Quests => new Type[] { typeof(DefendingTheHerdQuest) };

		[Constructable]
		public Bravehorn()
			: base()
		{
			Name = "Bravehorn";
		}

		public Bravehorn(Serial serial)
			: base(serial)
		{
		}

		public override void InitBody()
		{
			InitStats(100, 100, 25);

			Blessed = false;
			Female = false;
			Body = 0xEA;
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
