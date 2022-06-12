using System;
using System.Collections;
using Server;
using Server.Misc;
using Server.Gumps;
using Server.Mobiles;
using Server.Regions;
using Server.Network;

namespace Server.Gumps
{
	public class HavenRegion2 : Gump
	{
		public HavenRegion2( int i )
			: base( 0, 0 )
		{
			ArrayList       playerMobiles       = RegionPlayers();

			int mobs = 0;
		
			foreach (Mobile playerMobile in playerMobiles)
            		{
                		Region          playerregion = playerMobile.Region;
	
				if( playerregion.Name == "Haven" )
					++mobs;
				
			}
			this.Closable=true;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;
			this.AddPage(0);
			this.AddBackground(436, 231, 74, 230, 9200);
			this.AddImage(386, 111, 10440);
			this.AddImage(480, 171, 4502);
			this.AddImage(326, 172, 4506);
			this.AddImage(354, 181, 93);
			this.AddImage(354, 159, 93);
			this.AddImage(442, 394, 5564);
			this.AddImage(443, 166, 5564);
			this.AddImage(452, 169, 5021);
			this.AddLabel(357, 187, 47, @"Players in Region: "+ mobs);
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
}