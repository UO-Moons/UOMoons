namespace Server.Gumps;

public class FOrcCaveRegion : Gump
{
	public FOrcCaveRegion()
		: base( 0, 0 )
	{
		Closable=true;
		Disposable=false;
		Dragable=false;
		Resizable=false;
		AddPage(0);
		AddImage(215, 35, 9007);
		AddBackground(337, 117, 213, 38, 9270);
		AddImage(531, 110, 4502);
		AddImage(337, 144, 2091);
		AddImage(337, 116, 2091);
		AddImage(290, 105, 5608);
		AddImage(536, 95, 94);
		AddImage(543, 121, 94);
		AddImage(502, 82, 4500);
		AddImage(502, 137, 4504);
		AddImage(504, 106, 2529);
		AddLabel(352, 126, 33, @"Orc Cave *FEL*");

	}
		

}