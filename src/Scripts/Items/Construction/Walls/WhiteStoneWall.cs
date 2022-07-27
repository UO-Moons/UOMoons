namespace Server.Items;

public enum WhiteStoneWallTypes
{
	EastWall,
	SouthWall,
	SeCorner,
	NwCornerPost,
	EastArrowLoop,
	SouthArrowLoop,
	EastWindow,
	SouthWindow,
	SouthWallMedium,
	EastWallMedium,
	SeCornerMedium,
	NwCornerPostMedium,
	SouthWallShort,
	EastWallShort,
	SeCornerShort,
	NwCornerPostShort,
	NeCornerPostShort,
	SwCornerPostShort,
	SouthWallVShort,
	EastWallVShort,
	SeCornerVShort,
	NwCornerPostVShort,
	SeCornerArch,
	SouthArch,
	WestArch,
	EastArch,
	NorthArch,
	EastBattlement,
	SeCornerBattlement,
	SouthBattlement,
	NeCornerBattlement,
	SwCornerBattlement,
	Column,
	SouthWallVvShort,
	EastWallVvShort
}

public class WhiteStoneWall : BaseWall
{
	[Constructable]
	public WhiteStoneWall(WhiteStoneWallTypes type) : base(0x0057 + (int)type)
	{
	}

	public WhiteStoneWall(Serial serial) : base(serial)
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
