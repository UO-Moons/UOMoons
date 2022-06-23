using System.Collections;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public class FMaginciaRegion2 : Gump
{
	public FMaginciaRegion2( int i )
		: base( 0, 0 )
	{
		ArrayList       playerMobiles       = RegionPlayers();

		int mobs = 0;
		
		foreach (Mobile playerMobile in playerMobiles)
		{
			Region          playerregion = playerMobile.Region;
	
			if( playerregion.Name == "Magincia" )
				++mobs;
				
		}
		Closable=true;
		Disposable=false;
		Dragable=true;
		Resizable=false;
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
		AddLabel(357, 187, 33, @"Players in Region: "+ mobs);
	}

	public static ArrayList RegionPlayers()
	{
		ArrayList mobiles = new ArrayList();
            
		foreach ( NetState state in NetState.Instances ) 
		{
			if ( state.Mobile is PlayerMobile )
			{
				Mobile          mobile          = state.Mobile;
				AccessLevel     accessLevel     = mobile.AccessLevel;

				if (accessLevel != AccessLevel.Player)                  continue; // staff doesn't qualify
				if (!mobile.Alive)                                      continue; // for dead players

				mobiles.Add ( state.Mobile );
			}
		}
		return mobiles;
	}		
}