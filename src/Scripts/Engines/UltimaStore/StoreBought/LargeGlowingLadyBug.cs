using System;

namespace Server.Items;

public class LargeGlowingLadyBug : BaseLight
{
	public override int LabelNumber => 1071400;  // Large Glowing Lady Bug

	public override int LitItemId => SouthFacing ? 0x2CFE : 0x2D00;

	public override int UnlitItemId => SouthFacing ? 0x2CFD : 0x2CFF;

	public bool SouthFacing => ItemId == 0x2CFD || ItemId == 0x2CFE;

	[Constructable]
	public LargeGlowingLadyBug()
		: base(0x2CFD)
	{
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle225;
		Weight = 3.0;
	}

	public LargeGlowingLadyBug(Serial serial)
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
			0x2CFD => 0x2CFF,
			0x2CFE => 0x2D00,
			0x2CFF => 0x2CFF,
			0x2D00 => 0x2CFE,
			_ => ItemId
		};
	}
}
