using Server.Engines.XmlSpawner2;

namespace Server.Items
{
	[Flipable(0x1E5E, 0x1E5F)]
	public class QuestLeadersBoard : BaseItem
	{

		public QuestLeadersBoard(Serial serial) : base(serial)
		{
		}

		[Constructable]
		public QuestLeadersBoard() : base(0x1e5e)
		{
			Movable = false;
			Name = "Quest Leaders Board";
		}

		public override void OnDoubleClick(Mobile from)
		{
			from.SendGump(new XmlQuestLeaders.TopQuestPlayersGump(XmlAttach.FindAttachment(from, typeof(XmlQuestPoints)) as XmlQuestPoints));
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
