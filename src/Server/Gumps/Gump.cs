using Server.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Gumps
{
	public class Gump
	{
		private readonly List<string> _mStrings;

		internal int MTextEntries, MSwitches;

		private static int _mNextSerial = 1;

		private int _mSerial;
		private int _mX, _mY;

		private bool _mDraggable = true;
		private bool _mClosable = true;
		private bool _mResizable = true;
		private bool _mDisposable = true;

		public virtual int GetTypeId()
		{
			return GetType().FullName.GetHashCode();
		}

		//public static int GetTypeID(Type type)
		//{
		//	return type.FullName.GetHashCode();
		//}

		public Gump(int x, int y)
		{
			do
			{
				_mSerial = _mNextSerial++;
			} while (_mSerial == 0); // standard client apparently doesn't send a gump response packet if serial == 0

			_mX = x;
			_mY = y;

			TypeId = GetTypeId();
			//TypeID = GetTypeID(GetType());

			Entries = new List<GumpEntry>();
			_mStrings = new List<string>();
		}

		public static void Invalidate()
		{
			//if ( m_Strings.Count > 0 )
			//	m_Strings.Clear();
		}

		public int TypeId { get; set; }

		public List<GumpEntry> Entries { get; }

		public int Serial
		{
			get => _mSerial;
			set
			{
				if (_mSerial != value)
				{
					_mSerial = value;
					Invalidate();
				}
			}
		}

		public int X
		{
			get => _mX;
			set
			{
				if (_mX == value) return;
				_mX = value;
				Invalidate();
			}
		}

		public int Y
		{
			get => _mY;
			set
			{
				if (_mY == value) return;
				_mY = value;
				Invalidate();
			}
		}

		public bool Disposable
		{
			get => _mDisposable;
			set
			{
				if (_mDisposable == value) return;
				_mDisposable = value;
				Invalidate();
			}
		}

		public bool Resizable
		{
			get => _mResizable;
			set
			{
				if (_mResizable == value) return;
				_mResizable = value;
				Invalidate();
			}
		}

		public bool Dragable
		{
			get => _mDraggable;
			set
			{
				if (_mDraggable == value) return;
				_mDraggable = value;
				Invalidate();
			}
		}

		public bool Closable
		{
			get => _mClosable;
			set
			{
				if (_mClosable == value) return;
				_mClosable = value;
				Invalidate();
			}
		}

		public static string Right(string text)
		{
			return $"<DIV ALIGN=RIGHT>{text}</DIV>";
		}

		public static string Center(string text)
		{
			return $"<CENTER>{text}</CENTER>";
		}

		public static string Color(string text, int color)
		{
			return $"<BASEFONT COLOR=#{color:X6}>{text}</BASEFONT>";
		}

		public static string FormatTimeSpan(TimeSpan ts)
		{
			return $"{ts.Days:D2}:{ts.Hours % 24:D2}:{ts.Minutes % 60:D2}:{ts.Seconds % 60:D2}";
		}

		public void AddPage(int page)
		{
			Add(new GumpPage(page));
		}

		public void AddAlphaRegion(int x, int y, int width, int height)
		{
			Add(new GumpAlphaRegion(x, y, width, height));
		}

		public void AddBackground(int x, int y, int width, int height, int gumpId)
		{
			Add(new GumpBackground(x, y, width, height, gumpId));
		}

		public void AddButton(int x, int y, int normalId, int pressedId, int buttonId, GumpButtonType type, int param)
		{
			Add(new GumpButton(x, y, normalId, pressedId, buttonId, type, param));
		}

		public void AddCheck(int x, int y, int inactiveId, int activeId, bool initialState, int switchId)
		{
			Add(new GumpCheck(x, y, inactiveId, activeId, initialState, switchId));
		}

		public void AddGroup(int group)
		{
			Add(new GumpGroup(group));
		}

		public void AddTooltip(int number)
		{
			Add(new GumpTooltip(number));
		}

		public void AddTooltip(int number, string args)
		{
			Add(new GumpTooltip(number, args));
		}

		public void AddTooltip(string text)
		{
			Add(new GumpTooltip(1042971, text));
		}

		public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
		{
			Add(new GumpHtml(x, y, width, height, text, background, scrollbar));
		}

		public void AddHtmlIntern(int x, int y, int width, int height, int textid, bool background, bool scrollbar)
		{
			Add(new GumpHtml(x, y, width, height, textid, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, color, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, string args, int color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, args, color, background, scrollbar));
		}

		public void AddSpriteImage(int x, int y, int gumpId, int width, int height, int sx, int sy)
		{
			Add(new GumpSpriteImage(x, y, gumpId, width, height, sx, sy));
		}

		public void AddImage(int x, int y, int gumpId)
		{
			Add(new GumpImage(x, y, gumpId));
		}

		public void AddImage(int x, int y, int gumpId, int hue)
		{
			Add(new GumpImage(x, y, gumpId, hue));
		}

		public void AddImageTiled(int x, int y, int width, int height, int gumpId)
		{
			Add(new GumpImageTiled(x, y, width, height, gumpId));
		}

		public void AddImageTiledButton(int x, int y, int normalId, int pressedId, int buttonId, GumpButtonType type, int param, int itemId, int hue, int width, int height)
		{
			Add(new GumpImageTileButton(x, y, normalId, pressedId, buttonId, type, param, itemId, hue, width, height));
		}
		public void AddImageTiledButton(int x, int y, int normalId, int pressedId, int buttonId, GumpButtonType type, int param, int itemId, int hue, int width, int height, int localizedTooltip)
		{
			Add(new GumpImageTileButton(x, y, normalId, pressedId, buttonId, type, param, itemId, hue, width, height, localizedTooltip));
		}

		public void AddItem(int x, int y, int itemId)
		{
			Add(new GumpItem(x, y, itemId));
		}

		public void AddItem(int x, int y, int itemId, int hue)
		{
			Add(new GumpItem(x, y, itemId, hue));
		}

		public void AddLabelIntern(int x, int y, int hue, int textid)
		{
			Add(new GumpLabel(x, y, hue, textid));
		}

		public void AddLabel(int x, int y, int hue, string text)
		{
			Add(new GumpLabel(x, y, hue, text));
		}

		public void AddLabelCropped(int x, int y, int width, int height, int hue, string text)
		{
			Add(new GumpLabelCropped(x, y, width, height, hue, text));
		}

		public void AddLabelCroppedIntern(int x, int y, int width, int height, int hue, int textid)
		{
			Add(new GumpLabelCropped(x, y, width, height, hue, textid));
		}

		public void AddRadio(int x, int y, int inactiveId, int activeId, bool initialState, int switchId)
		{
			Add(new GumpRadio(x, y, inactiveId, activeId, initialState, switchId));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryId, string initialText)
		{
			Add(new GumpTextEntry(x, y, width, height, hue, entryId, initialText));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryId, string initialText, int size)
		{
			Add(new GumpTextEntryLimited(x, y, width, height, hue, entryId, initialText, size));
		}

		public void AddTextEntryIntern(int x, int y, int width, int height, int hue, int entryId, int initialTextId)
		{
			Add(new GumpTextEntry(x, y, width, height, hue, entryId, initialTextId));
		}

		public void AddItemProperty(Item item)
		{
			Add(new GumpItemProperty(item.Serial.Value));
		}

		public void AddItemProperty(int serial)
		{
			Add(new GumpItemProperty(serial));
		}

		public void AddEcHandleInput()
		{
			Add(new EcHandleInput());
		}

		public void Add(GumpEntry g)
		{
			if (g.Parent != this)
			{
				g.Parent = this;
			}
			else if (!Entries.Contains(g))
			{
				Invalidate();
				Entries.Add(g);
			}
		}

		public void Remove(GumpEntry g)
		{
			if (g == null || !Entries.Contains(g))
				return;

			Invalidate();
			Entries.Remove(g);
			g.Parent = null;
		}

		public int Intern(string value)
		{
			return Intern(value, false);
		}

		public int Intern(string value, bool enforceUnique)
		{
			if (enforceUnique)
			{
				int indexOf = _mStrings.IndexOf(value);

				if (indexOf >= 0)
					return indexOf;
			}

			_mStrings.Add(value);
			return _mStrings.Count - 1;
		}

		public void SendTo(NetState state)
		{
			state.AddGump(this);
			state.Send(Compile(state));
		}

		public static byte[] StringToBuffer(string str)
		{
			return Encoding.ASCII.GetBytes(str);
		}

		private static readonly byte[] MBeginLayout = StringToBuffer("{ ");
		private static readonly byte[] MEndLayout = StringToBuffer(" }");

		private static readonly byte[] MNoMove = StringToBuffer("{ nomove }");
		private static readonly byte[] MNoClose = StringToBuffer("{ noclose }");
		private static readonly byte[] MNoDispose = StringToBuffer("{ nodispose }");
		private static readonly byte[] MNoResize = StringToBuffer("{ noresize }");

		protected virtual Packet GetPacketFor(NetState ns)
		{
			return Compile(ns);
		}

		private Packet Compile(NetState ns)
		{
			IGumpWriter disp;

			if (ns is {Unpack: true})
				disp = new DisplayGumpPacked(this);
			else
				disp = new DisplayGumpFast(this);

			if (!_mDraggable)
				disp.AppendLayout(MNoMove);

			if (!_mClosable)
				disp.AppendLayout(MNoClose);

			if (!_mDisposable)
				disp.AppendLayout(MNoDispose);

			if (!_mResizable)
				disp.AppendLayout(MNoResize);

			int count = Entries.Count;
			GumpEntry e;

			for (int i = 0; i < count; ++i)
			{
				e = Entries[i];

				disp.AppendLayout(MBeginLayout);
				e.AppendTo(disp);
				disp.AppendLayout(MEndLayout);
			}

			disp.WriteStrings(_mStrings);

			disp.Flush();

			MTextEntries = disp.TextEntries;
			MSwitches = disp.Switches;

			return (Packet) disp;
		}

		public virtual void OnResponse(NetState sender, RelayInfo info)
		{
		}

		public virtual void OnServerClose(NetState owner)
		{
		}
	}
}
