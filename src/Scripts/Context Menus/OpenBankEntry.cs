namespace Server.ContextMenus;

public class OpenBankEntry : ContextMenuEntry
{
	private readonly Mobile _mBanker;

	public OpenBankEntry(Mobile from, Mobile banker) : base(6105, 12)
	{
		_mBanker = banker;
	}

	public override void OnClick()
	{
		if (!Owner.From.CheckAlive())
			return;

		if (Owner.From.Criminal)
		{
			_mBanker.Say(500378); // Thou art a criminal and cannot access thy bank box.
		}
		else
		{
			Owner.From.BankBox.Open();
		}
	}
}
