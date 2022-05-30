using Server.Engines.Quests;
using System;

namespace Server.Mobiles
{
	public class FrightenedDryad : MondainQuester
	{
		[Constructable]
		public FrightenedDryad()
			: base("The Frightened Dryad")
		{
		}

		public FrightenedDryad(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(BoundToTheLandQuest)
				};

		public override void InitBody()
		{
			InitStats(100, 100, 25);
			Female = true;
			Body = 266;
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
