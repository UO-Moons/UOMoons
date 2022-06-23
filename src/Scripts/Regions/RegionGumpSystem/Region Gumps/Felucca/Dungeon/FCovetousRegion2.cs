using System.Collections.Generic;
using System.Linq;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public class FCovetousRegion2 : Gump
{
	public FCovetousRegion2(int i)
		: base(0, 0)
	{
		List<Mobile> playerMobiles = RegionPlayers();

		int mobs = playerMobiles.Select(playerMobile => playerMobile.Region).Count(playerregion => playerregion.Name == "Covetous");

		Closable = true;
		Disposable = false;
		Dragable = true;
		Resizable = false;
		AddPage(0);
		AddBackground(436, 231, 74, 230, 9200);
		AddImage(386, 111, 10440);
		AddImage(480, 171, 4502);
		AddImage(326, 172, 4506);
		AddImage(354, 181, 93);
		AddImage(354, 159, 93);
		AddImage(442, 394, 5564);
		AddImage(443, 166, 5564);
		AddImage(452, 169, 5021);
		AddLabel(357, 187, 33, @"Players in Region: " + mobs);
	}

	public static List<Mobile> RegionPlayers()
	{
		return (from state in NetState.Instances where state.Mobile is PlayerMobile let mobile = state.Mobile let accessLevel = mobile.AccessLevel where accessLevel == AccessLevel.Player where mobile.Alive select state.Mobile).ToList();
	}
}
