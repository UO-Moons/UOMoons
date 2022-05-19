using Server.Commands;
using System.Collections;

namespace Server.Engines.XmlSpawner2
{
	public class QuestLeadersStone : BaseItem
	{

		[Constructable]
		public QuestLeadersStone() : base(0xED4)
		{
			Movable = false;
			Visible = false;
			Name = "Quest LeaderboardSave Stone";

			// is there already another?
			ArrayList dlist = new();
			foreach (Item i in World.Items.Values)
			{
				if (i is QuestLeadersStone && i != this)
				{
					dlist.Add(i);
				}
			}
			foreach (Item d in dlist)
			{
				d.Delete();
			}
		}

		public QuestLeadersStone(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (m != null && m.AccessLevel >= AccessLevel.Administrator)
			{
				CommandEventArgs e = new CommandEventArgs(m, "", "", System.Array.Empty<string>());
				XmlQuestLeaders.QuestLeaderboardSave_OnCommand(e);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			XmlQuestLeaders.QuestLBSSerialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			XmlQuestLeaders.QuestLBSDeserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
