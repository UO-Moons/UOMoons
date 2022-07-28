namespace Server.Items;

public class HarvestWine : BeverageBottle
{
	public override int LabelNumber => 1153873;//Harvest Wine
	public override double DefaultWeight => 1;

	[Constructable]
	public HarvestWine()
		: base(BeverageType.Wine)
	{
		Hue = 0xe0;
	}

	public HarvestWine(Serial serial)
		: base(serial)
	{
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);
		LabelTo(from, 1114092); // Harvest Wine 2009
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		list.Add(1114092); // Harvest Wine 2009
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
