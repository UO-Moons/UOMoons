using Server.Engines.VeteranRewards;

namespace Server.Items;

[Flipable]
public abstract class BaseCloak : BaseClothing
{
	public BaseCloak(int itemID) : this(itemID, 0)
	{
	}

	public BaseCloak(int itemID, int hue) : base(itemID, Layer.Cloak, hue)
	{
		Weight = 5.0;
	}

	public BaseCloak(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
