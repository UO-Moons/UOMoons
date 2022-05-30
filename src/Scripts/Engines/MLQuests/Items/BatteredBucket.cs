using System;

namespace Server.Engines.Quests
{
	public class BatteredBucket : BaseQuestItem
	{
		[Constructable]
		public BatteredBucket()
			: base(0x2004)
		{
		}

		public BatteredBucket(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(LostAndFoundQuest)
				};

		public override int LabelNumber => 1073129;// A battered bucket
		public override int Lifespan => 600;
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
