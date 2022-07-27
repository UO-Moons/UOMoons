using System;

namespace Server.Items;

public class CandleLarge : BaseLight
{
	public override int LitItemId => 0xB1A;
	public override int UnlitItemId => 0xA26;

	[Constructable]
	public CandleLarge() : base(0xA26)
	{
		if (Burnout)
			Duration = TimeSpan.FromMinutes(25);
		else
			Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Circle150;
		Weight = 2.0;
	}

	public CandleLarge(Serial serial) : base(serial)
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