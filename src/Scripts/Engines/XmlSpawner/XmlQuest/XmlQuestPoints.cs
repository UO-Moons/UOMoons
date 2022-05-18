using Server.Commands;
using Server.Gumps;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Engines.XmlSpawner2
{
	public class XmlQuestPoints : XmlAttachment
	{
		public string guildFilter;
		public string nameFilter;

		public List<QuestEntry> QuestList { get; set; } = new();

		[CommandProperty(AccessLevel.GameMaster)]
		public int Rank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int DeltaRank { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime WhenRanked { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Points { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Credits { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int QuestsCompleted { get; set; }

		public class QuestEntry
		{
			public Mobile Quester;
			public string Name;
			public DateTime WhenCompleted;
			public DateTime WhenStarted;
			public int Difficulty;
			public bool PartyEnabled;
			public int TimesCompleted = 1;

			public QuestEntry()
			{
			}

			public QuestEntry(Mobile m, IXmlQuest quest)
			{
				Quester = m;
				if (quest != null)
				{
					WhenStarted = quest.TimeCreated;
					WhenCompleted = DateTime.UtcNow;
					Difficulty = quest.Difficulty;
					Name = quest.Name;
				}
			}

			public virtual void Serialize(GenericWriter writer)
			{

				writer.Write(0); // version

				writer.Write(Quester);
				writer.Write(Name);
				writer.Write(WhenCompleted);
				writer.Write(WhenStarted);
				writer.Write(Difficulty);
				writer.Write(TimesCompleted);
				writer.Write(PartyEnabled);


			}

			public virtual void Deserialize(GenericReader reader)
			{

				int version = reader.ReadInt();

				switch (version)
				{
					case 0:
						Quester = reader.ReadMobile();
						Name = reader.ReadString();
						WhenCompleted = reader.ReadDateTime();
						WhenStarted = reader.ReadDateTime();
						Difficulty = reader.ReadInt();
						TimesCompleted = reader.ReadInt();
						PartyEnabled = reader.ReadBool();
						break;
				}

			}

			public static void AddQuestEntry(Mobile m, IXmlQuest quest)
			{
				if (m == null || quest == null)
					return;

				// get the XmlQuestPoints attachment from the mobile
				XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

				if (p == null)
					return;

				// look through the list of quests and see if it is one that has already been done
				if (p.QuestList == null)
					p.QuestList = new List<QuestEntry>();

				bool found = false;
				foreach (QuestEntry e in p.QuestList)
				{
					if (e.Name == quest.Name)
					{
						// found a match, so just change the number and dates
						e.TimesCompleted++;
						e.WhenStarted = quest.TimeCreated;
						e.WhenCompleted = DateTime.UtcNow;
						// and update the difficulty and party status
						e.Difficulty = quest.Difficulty;
						e.PartyEnabled = quest.PartyEnabled;
						found = true;
						break;
					}
				}

				if (!found)
				{
					// add a new entry
					p.QuestList.Add(new QuestEntry(m, quest));

				}
			}
		}


		public XmlQuestPoints(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlQuestPoints()
		{
		}

		public static new void Initialize()
		{
			CommandSystem.Register("QuestPoints", AccessLevel.Player, new CommandEventHandler(CheckQuestPoints_OnCommand));

			CommandSystem.Register("QuestLog", AccessLevel.Player, new CommandEventHandler(QuestLog_OnCommand));

		}

		[Usage("QuestPoints")]
		[Description("Displays the players quest points and ranking")]
		public static void CheckQuestPoints_OnCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null) return;

			string msg = null;

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(e.Mobile, typeof(XmlQuestPoints));
			if (p != null)
			{
				msg = p.OnIdentify(e.Mobile);
			}

			if (msg != null)
				e.Mobile.SendMessage(msg);
		}



		[Usage("QuestLog")]
		[Description("Displays players quest history")]
		public static void QuestLog_OnCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null) return;

			e.Mobile.CloseGump(typeof(XMLQuestLogGump));
			e.Mobile.SendGump(new XMLQuestLogGump(e.Mobile));
		}


		public static void GiveQuestPoints(Mobile from, IXmlQuest quest)
		{
			if (from == null || quest == null) return;

			// find the XmlQuestPoints attachment

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(from, typeof(XmlQuestPoints));

			// if doesnt have one yet, then add it
			if (p == null)
			{
				p = new XmlQuestPoints();
				XmlAttach.AttachTo(from, p);
			}

			// if you wanted to scale the points given based on party size, karma, fame, etc.
			// this would be the place to do it
			int points = quest.Difficulty;

			// update the questpoints attachment information
			p.Points += points;
			p.Credits += points;
			p.QuestsCompleted++;

			if (from != null)
			{
				from.SendMessage("You have received {0} quest points!", points);
			}

			// add the completed quest to the quest list
			QuestEntry.AddQuestEntry(from, quest);

			// update the overall ranking list
			XmlQuestLeaders.UpdateQuestRanking(from, p);
		}

		public static int GetCredits(Mobile m)
		{
			int val = 0;

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));
			if (p != null)
			{
				val = p.Credits;
			}

			return val;
		}

		public static int GetPoints(Mobile m)
		{
			int val = 0;

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));
			if (p != null)
			{
				val = p.Points;
			}

			return val;
		}

		public static bool HasCredits(Mobile m, int credits, int minpoints)
		{
			if (m == null || m.Deleted) return false;

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

			if (p != null)
			{
				if (p.Credits >= credits && p.Points >= minpoints)
				{
					return true;
				}
			}

			return false;
		}

		public static bool TakeCredits(Mobile m, int credits)
		{
			if (m == null || m.Deleted) return false;

			XmlQuestPoints p = (XmlQuestPoints)XmlAttach.FindAttachment(m, typeof(XmlQuestPoints));

			if (p != null)
			{
				if (p.Credits >= credits)
				{
					p.Credits -= credits;
					return true;
				}
			}

			return false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			// version 0
			writer.Write(Points);
			writer.Write(Credits);
			writer.Write(QuestsCompleted);
			writer.Write(Rank);
			writer.Write(DeltaRank);
			writer.Write(WhenRanked);

			// save the quest history
			if (QuestList != null)
			{
				writer.Write(QuestList.Count);

				foreach (QuestEntry e in QuestList)
				{
					e.Serialize(writer);
				}
			}
			else
			{
				writer.Write(0);
			}

			// need this in order to rebuild the rankings on deser
			if (AttachedTo is Mobile)
				writer.Write(AttachedTo as Mobile);
			else
				writer.Write((Mobile)null);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:

					Points = reader.ReadInt();
					Credits = reader.ReadInt();
					QuestsCompleted = reader.ReadInt();
					Rank = reader.ReadInt();
					DeltaRank = reader.ReadInt();
					WhenRanked = reader.ReadDateTime();

					int nquests = reader.ReadInt();

					if (nquests > 0)
					{
						QuestList = new List<QuestEntry>(nquests);
						for (int i = 0; i < nquests; i++)
						{
							QuestEntry e = new();
							e.Deserialize(reader);

							QuestList.Add(e);
						}
					}

					// get the owner of this in order to rebuild the rankings
					Mobile quester = reader.ReadMobile();

					// rebuild the ranking list
					// if they have never made a kill, then dont rank
					if (quester != null && QuestsCompleted > 0)
					{
						XmlQuestLeaders.UpdateQuestRanking(quester, this);
					}
					break;
			}
		}

		public override string OnIdentify(Mobile from)
		{
			return string.Format("Quest Points Status:\nTotal Quest Points = {0}\nTotal Quests Completed = {1}\nQuest Credits Available = {2}", Points, QuestsCompleted, Credits);
		}
	}
}
