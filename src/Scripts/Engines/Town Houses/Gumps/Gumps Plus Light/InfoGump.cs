namespace Server.Engines.TownHouses;

public class InfoGump : GumpPlusLight
{
	private readonly int _cWidth;
	private readonly int _cHeight;
	private readonly string _cText;
	private readonly bool _cScroll;

	public InfoGump(Mobile m, int width, int height, string text, bool scroll) : base(m, 100, 100)
	{
		_cWidth = width;
		_cHeight = height;
		_cText = text;
		_cScroll = scroll;

		NewGump();
	}

	protected override void BuildGump()
	{
		AddBackground(0, 0, _cWidth, _cHeight, 0x13BE);

		AddHtml(20, 20, _cWidth - 40, _cHeight - 40, Html.White + _cText, false, _cScroll);
	}
}
