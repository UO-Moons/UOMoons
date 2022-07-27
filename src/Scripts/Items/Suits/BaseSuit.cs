namespace Server.Items;

public class BaseSuit : BaseItem
{
	[CommandProperty(AccessLevel.Administrator)]
	private AccessLevel AccessLevel { get; set; }

	public BaseSuit(AccessLevel level, int hue, int itemId) : base(itemId)
	{
		Hue = hue;
		Weight = 1.0;
		Movable = false;
		LootType = LootType.Newbied;
		Layer = Layer.OuterTorso;

		AccessLevel = level;
	}

	public BaseSuit(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)AccessLevel);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		AccessLevel = version switch
		{
			0 => (AccessLevel)reader.ReadInt(),
			_ => AccessLevel
		};
	}

	private bool Validate()
	{
		object root = RootParent;

		if (root is not Mobile mobile || mobile.AccessLevel >= AccessLevel)
			return true;

		Delete();
		return false;

	}

	public override void OnSingleClick(Mobile from)
	{
		if (Validate())
			base.OnSingleClick(from);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (Validate())
			base.OnDoubleClick(from);
	}

	public override bool VerifyMove(Mobile from)
	{
		return from.AccessLevel >= AccessLevel;
	}

	public override bool OnEquip(Mobile from)
	{
		if (from.AccessLevel < AccessLevel)
			from.SendMessage("You may not wear this.");

		return from.AccessLevel >= AccessLevel;
	}
}
