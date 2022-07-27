using System;

namespace Server.Items;

[Flipable]
public class WallSconce : BaseLight
{
	public override int LitItemId => ItemId == 0x9FB ? 0x9FD : 0xA02;

	public override int UnlitItemId => ItemId == 0x9FD ? 0x9FB : 0xA00;

	[Constructable]
	public WallSconce() : base(0x9FB)
	{
		Movable = false;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.WestBig;
		Weight = 3.0;
	}

	public WallSconce(Serial serial) : base(serial)
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
			0x9FB => 0xA00,
			0x9FD => 0xA02,
			0xA00 => 0x9FB,
			0xA02 => 0x9FD,
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
