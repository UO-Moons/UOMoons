namespace Server.Items;

public class RedBook : BaseBook
{
	[Constructable]
	public RedBook() : base(0xFF1)
	{
	}

	[Constructable]
	public RedBook(int pageCount, bool writable) : base(0xFF1, pageCount, writable)
	{
	}

	[Constructable]
	public RedBook(string title, string author, int pageCount, bool writable) : base(0xFF1, title, author, pageCount, writable)
	{
	}

	public RedBook(bool writable) : base(0xFF1, writable)
	{
	}

	public RedBook(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
