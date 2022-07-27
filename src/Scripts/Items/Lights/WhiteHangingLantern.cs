using System;

namespace Server.Items;

[Flipable]
public class WhiteHangingLantern : BaseLight
{
	public override int LitItemId => ItemId == 0x24C6 ? 0x24C5 : 0x24C7;

	public override int UnlitItemId => ItemId == 0x24C5 ? 0x24C6 : 0x24C8;

	[Constructable]
	public WhiteHangingLantern() : base(0x24C6)
	{
		Movable = true;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle300;
		Weight = 3.0;
	}

	public WhiteHangingLantern(Serial serial) : base(serial)
	{
	}

	public void Flip()
	{
		Light = LightType.Circle300;

		ItemId = ItemId switch
		{
			0x24C6 => 0x24C8,
			0x24C5 => 0x24C7,
			0x24C8 => 0x24C6,
			0x24C7 => 0x24C5,
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
