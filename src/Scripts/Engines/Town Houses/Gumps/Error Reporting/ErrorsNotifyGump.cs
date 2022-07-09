namespace Server.Engines.TownHouses;

public class ErrorsNotifyGump : GumpPlusLight
{
	public ErrorsNotifyGump(Mobile m) : base(m, 250, 100)
	{
		m.CloseGump(typeof(ErrorsNotifyGump));
	}

	protected override void BuildGump()
	{
		AddButton(0, 0, 0x1590, 0x1590, "Errors", Errors);
		AddItem(20, 20, 0x22C4);
	}

	private void Errors()
	{
		_ = new ErrorsGump(Owner);
	}
}
