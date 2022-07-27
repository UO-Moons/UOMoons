namespace Server.Regions;

public class SpawnPersistence : BaseItem
{
	private static SpawnPersistence Instance { get; set; }

	public static void EnsureExistence()
	{
		Instance ??= new SpawnPersistence();
	}

	public override string DefaultName => "Region spawn persistence - Internal";

	private SpawnPersistence() : base(1)
	{
		Movable = false;
	}

	public SpawnPersistence(Serial serial) : base(serial)
	{
		Instance = this;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version

		writer.Write(SpawnEntry.Table.Values.Count);
		foreach (SpawnEntry entry in SpawnEntry.Table.Values)
		{
			writer.Write(entry.Id);

			entry.Serialize(writer);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		int count = reader.ReadInt();
		for (int i = 0; i < count; i++)
		{
			int id = reader.ReadInt();

			SpawnEntry entry = (SpawnEntry)SpawnEntry.Table[id];

			if (entry != null)
				entry.Deserialize(reader, version);
			else
				SpawnEntry.Remove(reader, version);
		}
	}
}
