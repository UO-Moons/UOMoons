using Server.Engines.Quests;
using System;

namespace Server.Items
{
	public class DenthesJournal : BaseQuestItem
	{
		[Constructable]
		public DenthesJournal()
			: base(0xFF2)
		{
		}

		public DenthesJournal(Serial serial)
			: base(serial)
		{
		}

		public override Type[] Quests => new Type[]
				{
					typeof(LastWordsQuest)
				};
		public override int LabelNumber => 1073240;// Lord Denthe's Journal
		public override int Lifespan => 3600;// ?
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
