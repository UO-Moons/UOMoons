using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public class GumpOptions : Gump
{
	public GumpOptions(Mobile from) : base(0, 0)
	{
		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;
		AddPage(0);
		AddBackground(0, 29, 192, 168, 9200);
		AddImage(10, 41, 52);
		AddLabel(19, 123, 3, @"Region Gump (Auto/Man)");
		AddImage(82, 157, 113);
		AddButton(15, 163, 2111, 2112, 1, GumpButtonType.Reply, 0);
		AddImage(75, 56, 2529);
		AddButton(123, 162, 2114, 248, 2, GumpButtonType.Reply, 0);
		AddLabel(59, 44, 36, @"Gump Options");
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		Mobile from = sender.Mobile;

		PlayerMobile playerMobile = from as PlayerMobile;

		//From.CloseGump(typeof(PernOptions));

		switch (info.ButtonID)
		{
			case 1:
				playerMobile.RegionGump = true;
				return;
			case 2:
				playerMobile.RegionGump = false;
				return;
		}
	}
}
