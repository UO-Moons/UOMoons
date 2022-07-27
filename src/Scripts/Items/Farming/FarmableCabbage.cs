namespace Server.Items;

public class FarmableCabbage : FarmableCrop
{
	private static int GetCropId() => 3254;
	public override int GetPickedId() => 3254;

	public override Item GetCropObject()
	{
		Cabbage cabbage = new()
		{
			ItemId = Utility.Random(3195, 2)
		};

		return cabbage;
	}

	[Constructable]
	public FarmableCabbage() : base(GetCropId())
	{
	}

	public FarmableCabbage(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.WriteEncodedInt(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}
