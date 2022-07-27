using System;

namespace Server.Items;

[Flipable]
public class RedHangingLantern : BaseLight
{
	public override int LitItemId => ItemId == 0x24C2 ? 0x24C1 : 0x24C3;

	public override int UnlitItemId => ItemId == 0x24C1 ? 0x24C2 : 0x24C4;

	[Constructable]
	public RedHangingLantern() : base(0x24C2)
	{
		Movable = true;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle300;
		Weight = 3.0;
	}

	public RedHangingLantern(Serial serial) : base(serial)
	{
	}

	public void Flip()
	{
		Light = LightType.Circle300;

		ItemId = ItemId switch
		{
			0x24C2 => 0x24C4,
			0x24C1 => 0x24C3,
			0x24C4 => 0x24C2,
			0x24C3 => 0x24C1,
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
