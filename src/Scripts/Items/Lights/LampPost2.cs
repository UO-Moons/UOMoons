using System;

namespace Server.Items;

public class LampPost2 : BaseLight
{
	public override int LitItemId => 0xB22;
	public override int UnlitItemId => 0xB23;

	[Constructable]
	public LampPost2() : base(0xB23)
	{
		Movable = false;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle300;
		Weight = 40.0;
	}

	public LampPost2(Serial serial) : base(serial)
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