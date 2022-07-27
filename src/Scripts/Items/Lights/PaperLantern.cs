using System;

namespace Server.Items;

[Flipable]
public class PaperLantern : BaseLight
{
	public override int LitItemId => 0x24BD;
	public override int UnlitItemId => 0x24BE;

	[Constructable]
	public PaperLantern() : base(0x24BE)
	{
		Movable = true;
		Duration = TimeSpan.Zero; // Never burnt out
		Burning = false;
		Light = LightType.Circle150;
		Weight = 3.0;
	}

	public PaperLantern(Serial serial) : base(serial)
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