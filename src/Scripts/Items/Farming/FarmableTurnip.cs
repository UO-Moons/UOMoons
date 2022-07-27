namespace Server.Items;

public class FarmableTurnip : FarmableCrop
{
	private static int GetCropId()
	{
		return Utility.Random(3169, 3);
	}

	public override Item GetCropObject()
	{
		Turnip turnip = new Turnip
		{
			ItemId = Utility.Random(3385, 2)
		};

		return turnip;
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableTurnip() : base(GetCropId())
	{
	}

	public FarmableTurnip(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}
