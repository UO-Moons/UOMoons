using Server.Gumps;
using Server.Network;
using System;
using System.Collections;
using System.Linq;

namespace Server.Engines.TownHouses;

public delegate void GumpStateCallback(object obj);
public delegate void GumpCallback();

public abstract class GumpPlusLight : Gump
{
	public static void RefreshGump(Mobile m, Type type)
	{
		if (m.NetState == null)
		{
			return;
		}

		foreach (Gump g in m.NetState.Gumps.Where(g => g is GumpPlusLight && g.GetType() == type))
		{
			m.CloseGump(type);
			((GumpPlusLight)g).NewGump();
			return;
		}
	}

	private readonly Hashtable _cButtons;
	private readonly Hashtable _cFields;

	protected Mobile Owner { get; }

	protected GumpPlusLight(Mobile m, int x, int y) : base(x, y)
	{
		Owner = m;

		_cButtons = new Hashtable();
		_cFields = new Hashtable();

		Timer.DelayCall(TimeSpan.FromSeconds(0), NewGump);
	}

	private void Clear()
	{
		Entries.Clear();
		_cButtons.Clear();
		_cFields.Clear();
	}

	protected virtual void NewGump()
	{
		NewGump(true);
	}

	private void NewGump(bool clear)
	{
		if (clear)
		{
			Clear();
		}

		BuildGump();

		Owner.SendGump(this);
	}

	public void SameGump()
	{
		Owner.SendGump(this);
	}

	protected abstract void BuildGump();

	private int UniqueButton()
	{
		int random;
		do
		{
			random = Utility.Random(20000);

		} while (_cButtons[random] != null);

		return random;
	}

	private int UniqueTextId()
	{
		int random;
		do
		{
			random = Utility.Random(20000);

		} while (_cButtons[random] != null);

		return random;
	}

	protected void AddBackgroundZero(int x, int y, int width, int height, int back)
	{
		AddBackgroundZero(x, y, width, height, back, true);
	}

	private void AddBackgroundZero(int x, int y, int width, int height, int back, bool over)
	{
		BackgroundPlus plus = new(x, y, width, height, back, over);

		Entries.Insert(0, plus);
	}

	protected new void AddBackground(int x, int y, int width, int height, int back)
	{
		AddBackground(x, y, width, height, back, true);
	}

	private void AddBackground(int x, int y, int width, int height, int back, bool over)
	{
		BackgroundPlus plus = new(x, y, width, height, back, over);

		Add(plus);
	}

	public void AddButton(int x, int y, int id, GumpCallback callback)
	{
		AddButton(x, y, id, id, "None", callback);
	}

	public void AddButton(int x, int y, int id, GumpStateCallback callback, object arg)
	{
		AddButton(x, y, id, id, "None", callback, arg);
	}

	protected void AddButton(int x, int y, int id, string name, GumpCallback callback)
	{
		AddButton(x, y, id, id, name, callback);
	}

	protected void AddButton(int x, int y, int id, string name, GumpStateCallback callback, object arg)
	{
		AddButton(x, y, id, id, name, callback, arg);
	}

	public void AddButton(int x, int y, int up, int down, GumpCallback callback)
	{
		AddButton(x, y, up, down, "None", callback);
	}

	protected void AddButton(int x, int y, int up, int down, string name, GumpCallback callback)
	{
		int id = UniqueButton();

		ButtonPlus button = new(x, y, up, down, id, name, callback);

		Add(button);

		_cButtons[id] = button;
	}

	public void AddButton(int x, int y, int up, int down, GumpStateCallback callback, object arg)
	{
		AddButton(x, y, up, down, "None", callback, arg);
	}

	protected void AddButton(int x, int y, int up, int down, string name, GumpStateCallback callback, object arg)
	{
		int id = UniqueButton();

		ButtonPlus button = new(x, y, up, down, id, name, callback, arg);

		Add(button);

		_cButtons[id] = button;
	}

	protected void AddHtml(int x, int y, int width, string text)
	{
		AddHtml(x, y, width, 21, Html.White + text, false, false, true);
	}

	public void AddHtml(int x, int y, int width, string text, bool over)
	{
		AddHtml(x, y, width, 21, Html.White + text, false, false, over);
	}

	protected new void AddHtml(int x, int y, int width, int height, string text, bool back, bool scroll)
	{
		AddHtml(x, y, width, height, Html.White + text, back, scroll, true);
	}

	private void AddHtml(int x, int y, int width, int height, string text, bool back, bool scroll, bool over)
	{
		HtmlPlus html = new(x, y, width, height, Html.White + text, back, scroll, over);

		Add(html);
	}

	protected void AddTextField(int x, int y, int width, int height, int color, int back, string name, string text)
	{
		int id = UniqueTextId();

		AddImageTiled(x, y, width, height, back);
		AddTextEntry(x, y, width, height, color, id, text);

		_cFields[id] = name;
		_cFields[name] = text;
	}

	protected string GetTextField(string name)
	{
		return _cFields[name] == null ? "" : _cFields[name].ToString();
	}

	protected int GetTextFieldInt(string name)
	{
		return Utility.ToInt32(GetTextField(name));
	}

	protected virtual void OnClose()
	{
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		string name = "";

		try
		{
			if (info.ButtonID == -5)
			{
				NewGump();
				return;
			}

			foreach (TextRelay t in info.TextEntries)
			{
				_cFields[_cFields[t.EntryID]?.ToString() ?? string.Empty] = t.Text;
			}

			if (info.ButtonID == 0)
			{
				OnClose();
			}

			if (_cButtons[info.ButtonID] == null || _cButtons[info.ButtonID] is not ButtonPlus)
			{
				return;
			}

			name = ((ButtonPlus)_cButtons[info.ButtonID]).Name;

			((ButtonPlus)_cButtons[info.ButtonID]).Invoke();

		}
		catch (Exception e)
		{
			Errors.Report("An error occured during a gump response.  More information can be found on the console.");
			if (name != "")
			{
				Console.WriteLine("{0} gump name triggered an error.", name);
			}

			Console.WriteLine(e.Message);
			Console.WriteLine(e.Source);
			Console.WriteLine(e.StackTrace);
		}
	}
}
