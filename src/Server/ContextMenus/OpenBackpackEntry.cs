namespace Server.ContextMenus;

public class OpenBackpackEntry : ContextMenuEntry
{
	private readonly Mobile _mMobile;

	public OpenBackpackEntry(Mobile m) : base(6145)
	{
		_mMobile = m;
	}

	public override void OnClick()
	{
		_mMobile.Use(_mMobile.Backpack);
	}
}
