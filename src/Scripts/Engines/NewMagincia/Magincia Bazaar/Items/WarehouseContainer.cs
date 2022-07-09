using Server.Items;

namespace Server.Engines.NewMagincia;

public class WarehouseContainer : MediumCrate
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; private set; }

	public WarehouseContainer(Mobile owner)
	{
		Owner = owner;
	}

	public WarehouseContainer(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(Owner);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		Owner = reader.ReadMobile();
	}
}

public class Warehouse : LargeCrate
{
	public Warehouse(Mobile owner)
	{
		Movable = false;
		Visible = false;
	}

	public Warehouse(Serial serial) : base(serial)
	{
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
