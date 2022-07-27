using System;

namespace Server.Items;

[Flipable]
public class WallTorch : BaseLight
{
	public override int LitItemId => ItemId == 0xA05 ? 0xA07 : 0xA0C;

	public override int UnlitItemId => ItemId == 0xA07 ? 0xA05 : 0xA0A;

	[Constructable]
	public WallTorch() : base(0xA05)
	{
		Movable = false;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.WestBig;
		Weight = 3.0;
	}

	public WallTorch(Serial serial) : base(serial)
	{
	}

	public void Flip()
	{
		Light = Light switch
		{
			LightType.WestBig => LightType.NorthBig,
			LightType.NorthBig => LightType.WestBig,
			_ => Light
		};

		ItemId = ItemId switch
		{
			0xA05 => 0xA0A,
			0xA07 => 0xA0C,
			0xA0A => 0xA05,
			0xA0C => 0xA07,
			_ => ItemId
		};
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
}
