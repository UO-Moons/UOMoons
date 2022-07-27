namespace Server.Items;

public class FarmableCotton : FarmableCrop
{
	private static int GetCropId()
	{
		return Utility.Random(3153, 4);
	}

	public override Item GetCropObject()
	{
		return new Cotton();
	}

	public override int GetPickedId()
	{
		return 3254;
	}

	[Constructable]
	public FarmableCotton() : base(GetCropId())
	{
	}

	public FarmableCotton(Serial serial) : base(serial)
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
