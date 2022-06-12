using System;
using Server;
using Server.Gumps;

namespace Server.Gumps
{
	public class JhelomRegion : Gump
	{
		public JhelomRegion()
			: base( 0, 0 )
		{
			this.Closable=true;
			this.Disposable=false;
			this.Dragable=false;
			this.Resizable=false;
			this.AddPage(0);
			this.AddImage(215, 35, 9007);
			this.AddBackground(337, 117, 213, 38, 9270);
			this.AddImage(531, 110, 4502);
			this.AddImage(337, 144, 2091);
			this.AddImage(337, 116, 2091);
			this.AddImage(290, 105, 5608);
			this.AddImage(536, 95, 94);
			this.AddImage(543, 121, 94);
			this.AddImage(502, 82, 4500);
			this.AddImage(502, 137, 4504);
			this.AddImage(504, 106, 2529);
			this.AddLabel(352, 126, 47, @"Jhelom");

		}
		

	}
}