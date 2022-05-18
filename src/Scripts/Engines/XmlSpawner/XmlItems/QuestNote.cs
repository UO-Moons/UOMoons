using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public class QuestNote : XmlQuestToken
	{
		[Constructable]
		public QuestNote() : base(0x14EE)
		{
			Name = "A quest note";
			TitleString = "A quest note";
		}

		public QuestNote(Serial serial) : base(serial)
		{
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

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
			from.CloseGump(typeof(XmlQuestStatusGump));
			from.SendGump(new XmlQuestStatusGump(this, TitleString));
		}
	}


	public class OriginalQuestNote : XmlQuestToken
	{
		private int m_size = 1;

		[Constructable]
		public OriginalQuestNote() : base(0x14EE)
		{
			Name = "A quest note";
			TitleString = "A quest note";
		}

		public OriginalQuestNote(Serial serial) : base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Size
		{
			get => m_size;
			set
			{
				m_size = value;
				if (m_size < 1) m_size = 1;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TextColor { get; set; } = 0x3e8;

		[CommandProperty(AccessLevel.GameMaster)]
		public int TitleColor { get; set; } = 0xef0000;

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(TextColor);
			writer.Write(TitleColor);
			writer.Write(m_size);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					{
						TextColor = reader.ReadInt();
						TitleColor = reader.ReadInt();
						m_size = reader.ReadInt();
					}
					break;
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);
			from.CloseGump(typeof(QuestNoteGump));
			from.SendGump(new QuestNoteGump(this));
		}
	}

	public class QuestNoteGump : Gump
	{
		private readonly OriginalQuestNote m_Note;

		public static string HtmlFormat(string text, int color)
		{
			return string.Format("<BASEFONT COLOR=#{0}>{1}</BASEFONT>", color, text);
		}

		public QuestNoteGump(OriginalQuestNote note) : base(0, 0)
		{
			m_Note = note;

			AddPage(0);
			AddAlphaRegion(40, 41, 225, /*371*/70 * note.Size);
			// scroll top
			AddImageTiled(3, 5, 300, 37, 0x820);
			// scroll middle, upper portion
			AddImageTiled(19, 41, 263, 70, 0x821);
			for (int i = 1; i < note.Size; i++)
			{
				// scroll middle , lower portion
				AddImageTiled(19, 41 + 70 * i, 263, 70, 0x822);
			}
			// scroll bottom
			AddImageTiled(20, 111 + 70 * (note.Size - 1), 273, 34, 0x823);

			// title string
			AddHtml(55, 10, 200, 37, HtmlFormat(note.TitleString, note.TitleColor), false, false);
			// text string
			AddHtml(40, 41, 225, 70 * note.Size, HtmlFormat(note.NoteString, note.TextColor), false, false);

			// add the quest status gump button
			AddButton(40, 50 + note.Size * 70, 0x037, 0x037, 1, GumpButtonType.Reply, 0);

		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;
			if (info.ButtonID == 1)
			{
				XmlQuestStatusGump g = new(m_Note, m_Note.TitleString);
				from.SendGump(g);
			}
		}
	}


}
