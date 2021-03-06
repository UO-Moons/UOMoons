namespace Server.Items;

public class LibraryFriendLantern : Lantern
{
	public override bool IsArtifact => true;
	[Constructable]
	public LibraryFriendLantern()
	{
	}

	public LibraryFriendLantern(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073339;// Friends of the Library Reading Lantern
	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}

public class LibraryFriendReadingChair : BigElvenChair
{
	public override bool IsArtifact => true;
	[Constructable]
	public LibraryFriendReadingChair()
	{
	}

	public LibraryFriendReadingChair(Serial serial)
		: base(serial)
	{
	}

	public override int LabelNumber => 1073340;// Friends of the Library Reading Chair
	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}
