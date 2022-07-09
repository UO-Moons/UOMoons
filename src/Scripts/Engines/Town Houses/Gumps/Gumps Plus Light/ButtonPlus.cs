using Server.Gumps;

namespace Server.Engines.TownHouses;

public class ButtonPlus : GumpButton
{
	private readonly object _cCallback;
	private readonly object _cParam;

	public string Name { get; }

	public ButtonPlus(int x, int y, int normalId, int pressedId, int buttonId, string name, GumpCallback back)
		: base(x, y, normalId, pressedId, buttonId, GumpButtonType.Reply, 0)
	{
		Name = name;
		_cCallback = back;
		_cParam = "";
	}

	public ButtonPlus(int x, int y, int normalID, int pressedId, int buttonId, string name, GumpStateCallback back,
		object param) : base(x, y, normalID, pressedId, buttonId, GumpButtonType.Reply, 0)
	{
		Name = name;
		_cCallback = back;
		_cParam = param;
	}

	public void Invoke()
	{
		switch (_cCallback)
		{
			case GumpCallback callback:
				callback();
				break;
			case GumpStateCallback callback1:
				callback1(_cParam);
				break;
		}
	}
}
