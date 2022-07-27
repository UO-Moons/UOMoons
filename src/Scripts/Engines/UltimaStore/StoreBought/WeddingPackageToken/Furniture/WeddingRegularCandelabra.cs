using System;

namespace Server.Items;

public class WeddingRegularCandelabra : BaseLight, IDyable
{
	public override int LabelNumber => 1011213; // candelabra

	public override int LitItemId => 0x9EF1;
	public override int UnlitItemId => 0x9EF4;

	[Constructable]
	public WeddingRegularCandelabra()
		: base(0x9EF4)
	{
		Duration = TimeSpan.Zero; // Never burnt out
		Light = LightType.Circle225;
		LootType = LootType.Blessed;
	}

	public virtual bool Dye(Mobile from, DyeTub sender)
	{
		if (Deleted)
			return false;

		Hue = sender.DyedHue;
		return true;
	}

	public WeddingRegularCandelabra(Serial serial)
		: base(serial)
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
