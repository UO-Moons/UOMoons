namespace Server.Items;

public class ZooMemberSkirt : PlainDress
{
	public override int LabelNumber => 1073221; // Britannia Royal Zoo Member

	[Constructable]
	public ZooMemberSkirt()
		: this(0)
	{
	}

	[Constructable]
	public ZooMemberSkirt(int hue)
		: base(hue)
	{
	}

	public ZooMemberSkirt(Serial serial)
		: base(serial)
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

public class ZooMemberBodySash : BodySash
{
	public override int LabelNumber => 1073221; // Britannia Royal Zoo Member

	[Constructable]
	public ZooMemberBodySash()
		: this(0)
	{
	}

	[Constructable]
	public ZooMemberBodySash(int hue)
		: base(hue)
	{
	}

	public ZooMemberBodySash(Serial serial)
		: base(serial)
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

public class ZooMemberRobe : Robe
{
	public override int LabelNumber => 1073221; // Britannia Royal Zoo Member

	[Constructable]
	public ZooMemberRobe()
		: this(0)
	{
	}

	[Constructable]
	public ZooMemberRobe(int hue)
		: base(hue)
	{
	}

	public ZooMemberRobe(Serial serial)
		: base(serial)
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

public class ZooMemberCloak : Cloak
{
	public override int LabelNumber => 1073221; // Britannia Royal Zoo Member

	[Constructable]
	public ZooMemberCloak()
		: this(0)
	{
	}

	[Constructable]
	public ZooMemberCloak(int hue)
		: base(hue)
	{
	}

	public ZooMemberCloak(Serial serial)
		: base(serial)
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

public class LibraryFriendBodySash : BodySash
{
	public override int LabelNumber => 1073346; // Friends of the Library Sash

	[Constructable]
	public LibraryFriendBodySash()
		: this(0)
	{
	}

	[Constructable]
	public LibraryFriendBodySash(int hue)
		: base(hue)
	{
	}

	public LibraryFriendBodySash(Serial serial)
		: base(serial)
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

public class LibraryFriendDoublet : Doublet
{
	public override int LabelNumber => 1073351; // Friends of the Library Doublet

	[Constructable]
	public LibraryFriendDoublet()
		: this(0)
	{
	}

	[Constructable]
	public LibraryFriendDoublet(int hue)
		: base(hue)
	{
	}

	public LibraryFriendDoublet(Serial serial)
		: base(serial)
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

public class LibraryFriendSurcoat : Surcoat
{
	public override int LabelNumber => 1073348; // Friends of the Library Surcoat

	[Constructable]
	public LibraryFriendSurcoat()
		: this(0)
	{
	}

	[Constructable]
	public LibraryFriendSurcoat(int hue)
		: base(hue)
	{
	}

	public LibraryFriendSurcoat(Serial serial)
		: base(serial)
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

public class LibraryFriendCloak : Cloak
{
	public override int LabelNumber => 1073350; // Friends of the Library Cloak

	[Constructable]
	public LibraryFriendCloak()
		: this(0)
	{
	}

	[Constructable]
	public LibraryFriendCloak(int hue)
		: base(hue)
	{
	}

	public LibraryFriendCloak(Serial serial)
		: base(serial)
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

public class Adranath : BodySash
{
	public override int LabelNumber => 1073253;// Adranath - Museum of Vesper Replica

	[Constructable]
	public Adranath()
		: this(0)
	{
	}

	[Constructable]
	public Adranath(int hue)
		: base(hue)
	{
	}

	public Adranath(Serial serial)
		: base(serial)
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

public class OdricsRobe : Robe
{
	public override int LabelNumber => 1073250;// Odric's Robe - Museum of Vesper Replica

	[Constructable]
	public OdricsRobe()
		: this(0)
	{
	}

	[Constructable]
	public OdricsRobe(int hue)
		: base(hue)
	{
	}

	public OdricsRobe(Serial serial)
		: base(serial)
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

public class BaronLenshiresCloak : Cloak
{
	public override int LabelNumber => 1073252;// Baron Lenshire's Cloak - Museum of Vesper Replica

	[Constructable]
	public BaronLenshiresCloak()
		: this(0)
	{
	}

	[Constructable]
	public BaronLenshiresCloak(int hue)
		: base(hue)
	{
	}

	public BaronLenshiresCloak(Serial serial)
		: base(serial)
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
