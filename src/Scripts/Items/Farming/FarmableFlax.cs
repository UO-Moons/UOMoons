namespace Server.Items;

public class FarmableFlax : FarmableCrop
{
	private static int GetCropId()
	{
		return Utility.Random(6809, 3);
	}

	public override Item GetCropObject()
	{
		Flax flax = new Flax
		{
			ItemId = Utility.Random(6812, 2)
		};

		return flax;
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableFlax() : base(GetCropId())
	{
	}

	public FarmableFlax(Serial serial) : base(serial)
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
