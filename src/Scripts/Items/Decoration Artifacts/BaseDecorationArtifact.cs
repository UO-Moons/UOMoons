namespace Server.Items;

public abstract class BaseDecorationArtifact : BaseItem
{
	public abstract int ArtifactRarity { get; }
	public override bool ForceShowProperties => true;

	public BaseDecorationArtifact(int itemId) : base(itemId)
	{
		Weight = 10.0;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
	}

	public BaseDecorationArtifact(Serial serial) : base(serial)
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

public abstract class BaseDecorationContainerArtifact : BaseContainer
{
	public abstract int ArtifactRarity { get; }
	public override bool ForceShowProperties => true;

	public BaseDecorationContainerArtifact(int itemId) : base(itemId)
	{
		Weight = 10.0;
	}

	public override void AddNameProperties(ObjectPropertyList list)
	{
		base.AddNameProperties(list);
		list.Add(1061078, ArtifactRarity.ToString()); // artifact rarity ~1_val~
	}

	public BaseDecorationContainerArtifact(Serial serial) : base(serial)
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
