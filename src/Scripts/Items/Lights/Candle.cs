using System;

namespace Server.Items;

public class Candle : BaseEquipableLight
{
	public override int LitItemId => 0xA0F;
	public override int UnlitItemId => 0xA28;

	[Constructable]
	public Candle() : base(0xA28)
	{
		Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Circle150;
		Weight = 1.0;
	}

	public Candle(Serial serial) : base(serial)
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
}
