using Server.Network;

namespace Server.Menus;

public class ItemListMenu : IMenu
{
	private readonly int _serial;
	private static int _nextSerial;

	int IMenu.Serial => _serial;

	int IMenu.EntryLength => Entries.Length;

	public string Question { get; }

	public ItemListEntry[] Entries { get; set; }

	public ItemListMenu(string question, ItemListEntry[] entries)
	{
		Question = question;
		Entries = entries;

		do
		{
			_serial = _nextSerial++;
			_serial &= 0x7FFFFFFF;
		} while (_serial == 0);

		_serial = (int)((uint)_serial | 0x80000000);
	}

	public virtual void OnCancel(NetState state)
	{
	}

	public virtual void OnResponse(NetState state, int index)
	{
	}

	public void SendTo(NetState state)
	{
		state.AddMenu(this);
		state.Send(new DisplayItemListMenu(this));
	}
}
