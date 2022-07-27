using System;

namespace Server.Items;

public class HeatingStand : BaseLight
{
	public override int LitItemId => 0x184A;
	public override int UnlitItemId => 0x1849;

	[Constructable]
	public HeatingStand() : base(0x1849)
	{
		Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Empty;
		Weight = 1.0;
	}

	public override void Ignite()
	{
		base.Ignite();

		if (ItemId == LitItemId)
			Light = LightType.Circle150;
		else if (ItemId == UnlitItemId)
			Light = LightType.Empty;
	}

	public override void Douse()
	{
		base.Douse();

		if (ItemId == LitItemId)
			Light = LightType.Circle150;
		else if (ItemId == UnlitItemId)
			Light = LightType.Empty;
	}

	public HeatingStand(Serial serial) : base(serial)
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
