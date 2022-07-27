namespace Server.Items;

public class BrownBook : BaseBook
{
	[Constructable]
	public BrownBook() : base(0xFEF)
	{
	}

	[Constructable]
	public BrownBook(int pageCount, bool writable) : base(0xFEF, pageCount, writable)
	{
	}

	[Constructable]
	public BrownBook(string title, string author, int pageCount, bool writable) : base(0xFEF, title, author, pageCount, writable)
	{
	}

	public BrownBook(bool writable) : base(0xFEF, writable)
	{
	}

	public BrownBook(Serial serial) : base(serial)
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
