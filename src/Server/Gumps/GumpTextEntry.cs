using Server.Network;

namespace Server.Gumps
{
	public class GumpTextEntry : GumpEntry
	{
		private int m_X, m_Y;
		private int m_Width, m_Height;
		private int m_Hue;
		private int m_EntryID;
		private string m_InitialText;

		public int X
		{
			get => m_X;
			set => Delta(ref m_X, value);
		}

		public int Y
		{
			get => m_Y;
			set => Delta(ref m_Y, value);
		}

		public int Width
		{
			get => m_Width;
			set => Delta(ref m_Width, value);
		}

		public int Height
		{
			get => m_Height;
			set => Delta(ref m_Height, value);
		}

		public int Hue
		{
			get => m_Hue;
			set => Delta(ref m_Hue, value);
		}

		public int EntryID
		{
			get => m_EntryID;
			set => Delta(ref m_EntryID, value);
		}

		public string InitialText
		{
			get => m_InitialText;
			set => Delta(ref m_InitialText, value);
		}

		public GumpTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
		{
			m_X = x;
			m_Y = y;
			m_Width = width;
			m_Height = height;
			m_Hue = hue;
			m_EntryID = entryID;
			m_InitialText = initialText;
		}

		public override string Compile()
		{
			return string.Format("{{ textentry {0} {1} {2} {3} {4} {5} {6} }}", m_X, m_Y, m_Width, m_Height, m_Hue, m_EntryID, Parent.Intern(m_InitialText));
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("textentry");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_X);
			disp.AppendLayout(m_Y);
			disp.AppendLayout(m_Width);
			disp.AppendLayout(m_Height);
			disp.AppendLayout(m_Hue);
			disp.AppendLayout(m_EntryID);
			disp.AppendLayout(Parent.Intern(m_InitialText));

			disp.TextEntries++;
		}
	}
}
