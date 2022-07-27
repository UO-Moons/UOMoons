namespace Server.Items;

public class FarmableCarrot : FarmableCrop
{
	private static int GetCropId()
	{
		return 3190;
	}

	public override Item GetCropObject()
	{
		Carrot carrot = new Carrot
		{
			ItemId = Utility.Random(3191, 2)
		};

		return carrot;
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableCarrot() : base(GetCropId())
	{
	}

	public FarmableCarrot(Serial serial) : base(serial)
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
