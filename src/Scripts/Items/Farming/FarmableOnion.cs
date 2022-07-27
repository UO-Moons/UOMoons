namespace Server.Items;

public class FarmableOnion : FarmableCrop
{
	private static int GetCropId()
	{
		return 3183;
	}

	public override Item GetCropObject()
	{
		Onion onion = new Onion
		{
			ItemId = Utility.Random(3181, 2)
		};

		return onion;
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableOnion() : base(GetCropId())
	{
	}

	public FarmableOnion(Serial serial) : base(serial)
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
