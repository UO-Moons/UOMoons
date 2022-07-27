using System;

namespace Server.Items;

public class Torch : BaseEquipableLight
{
	public override int LitItemId => 0xA12;
	public override int UnlitItemId => 0xF6B;

	public override int LitSound => 0x54;
	public override int UnlitSound => 0x4BB;

	[Constructable]
	public Torch() : base(0xF6B)
	{
		Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Circle300;
		Weight = 1.0;
	}

	public override void OnAdded(IEntity parent)
	{
		base.OnAdded(parent);

		if (parent is Mobile mobile && Burning)
			Mobiles.MeerMage.StopEffect(mobile, true);
	}

	public override void Ignite()
	{
		base.Ignite();

		if (Parent is Mobile mobile && Burning)
			Mobiles.MeerMage.StopEffect(mobile, true);
	}

	public Torch(Serial serial) : base(serial)
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
