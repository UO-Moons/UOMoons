namespace Server.Items;

public enum ThickGrayStoneWallTypes
{
	WestArch,
	NorthArch,
	SouthArchTop,
	EastArchTop,
	EastArch,
	SouthArch,
	Wall1,
	Wall2,
	Wall3,
	SouthWindow,
	Wall4,
	EastWindow,
	WestArch2,
	NorthArch2,
	SouthArchTop2,
	EastArchTop2,
	EastArch2,
	SouthArch2,
	SwArchEdge2,
	SouthWindow2,
	NeArchEdge2,
	EastWindow2
}

public class ThickGrayStoneWall : BaseWall
{
	[Constructable]
	public ThickGrayStoneWall(ThickGrayStoneWallTypes type) : base(0x007A + (int)type)
	{
	}

	public ThickGrayStoneWall(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
