using System;

namespace Server.Items;

public class FreshGreenLadyBug : BaseLight
{
	public override int LabelNumber => 1071401;  // Fresh Green Ladybug

	public override int LitItemId => SouthFacing ? 0x2D04 : 0x2D02;

	public override int UnlitItemId => SouthFacing ? 0x2D03 : 0x2D01;

	public bool SouthFacing => ItemId is 0x2D03 or 0x2D04;

	[Constructable]
	public FreshGreenLadyBug()
		: base(0x2D04)
	{
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle225;
		Weight = 3.0;
	}

	public FreshGreenLadyBug(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}

	public void Flip()
	{
		ItemId = ItemId switch
		{
			0x2D01 => 0x2D03,
			0x2D02 => 0x2D04,
			0x2D03 => 0x2D01,
			0x2D04 => 0x2D02,
			_ => ItemId
		};
	}
}
