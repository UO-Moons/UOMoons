using System;

namespace Server.Items;

public class CandleShort : BaseLight
{
	public override int LitItemId => 0x142C;
	public override int UnlitItemId => 0x142F;

	[Constructable]
	public CandleShort() : base(0x142F)
	{
		if (Burnout)
			Duration = TimeSpan.FromMinutes(25);
		else
			Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Circle150;
		Weight = 1.0;
	}

	public CandleShort(Serial serial) : base(serial)
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