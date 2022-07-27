namespace Server.Items;

public class FarmableWheat : FarmableCrop
{
	private static int GetCropId()
	{
		return Utility.Random(3157, 4);
	}

	public override Item GetCropObject()
	{
		return new WheatSheaf();
	}

	public override int GetPickedId()
	{
		return Utility.Random(3502, 2);
	}

	[Constructable]
	public FarmableWheat() : base(GetCropId())
	{
	}

	public FarmableWheat(Serial serial) : base(serial)
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
