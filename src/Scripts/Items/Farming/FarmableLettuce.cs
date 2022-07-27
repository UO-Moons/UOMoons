namespace Server.Items;

public class FarmableLettuce : FarmableCrop
{
	private static int GetCropId()
	{
		return 3254;
	}

	public override Item GetCropObject()
	{
		Lettuce lettuce = new Lettuce
		{
			ItemId = Utility.Random(3184, 2)
		};

		return lettuce;
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableLettuce() : base(GetCropId())
	{
	}

	public FarmableLettuce(Serial serial) : base(serial)
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
