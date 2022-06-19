namespace Server.ContextMenus;

public class PaperdollEntry : ContextMenuEntry
{
	private readonly Mobile _mMobile;

	public PaperdollEntry(Mobile m) : base(6123, 18)
	{
		_mMobile = m;
	}

	public override void OnClick()
	{
		if (_mMobile.CanPaperdollBeOpenedBy(Owner.From))
			_mMobile.DisplayPaperdollTo(Owner.From);
	}
}
