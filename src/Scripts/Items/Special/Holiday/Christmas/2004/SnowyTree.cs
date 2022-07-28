namespace Server.Items;

public class SnowyTree : BaseItem
{
	[Constructable]
	public SnowyTree() : base(0x2377)
	{
		Weight = 1.0;
		LootType = LootType.Blessed;
	}

	public SnowyTree(Serial serial) : base(serial)
	{
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);
		LabelTo(from, 1070880); // Winter 2004
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		list.Add(1070880); // Winter 2004
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
