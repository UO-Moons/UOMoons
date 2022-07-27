namespace Server.Items;

public class PortcullisNs : BaseDoor
{
	public override bool UseChainedFunctionality => true;

	[Constructable]
	public PortcullisNs() : base(0x6F5, 0x6F5, 0xF0, 0xEF, new Point3D(0, 0, 20))
	{
	}

	public PortcullisNs(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class PortcullisEw : BaseDoor
{
	public override bool UseChainedFunctionality => true;

	[Constructable]
	public PortcullisEw() : base(0x6F6, 0x6F6, 0xF0, 0xEF, new Point3D(0, 0, 20))
	{
	}

	public PortcullisEw(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
