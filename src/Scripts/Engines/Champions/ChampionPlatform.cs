using Server.Items;

namespace Server.Engines.Champions;

public class ChampionPlatform : BaseAddon
{
	private ChampionSpawn _mSpawn;
	public ChampionPlatform(ChampionSpawn spawn)
	{
		_mSpawn = spawn;

		for (var x = -2; x <= 2; ++x)
		for (var y = -2; y <= 2; ++y)
			AddComponent(0x3EE, x, y, -5);

		for (var x = -1; x <= 1; ++x)
		for (var y = -1; y <= 1; ++y)
			AddComponent(0x3EE, x, y, 0);

		for (var i = -1; i <= 1; ++i)
		{
			AddComponent(0x3EF, i, 2, 0);
			AddComponent(0x3F0, 2, i, 0);

			AddComponent(0x3F1, i, -2, 0);
			AddComponent(0x3F2, -2, i, 0);
		}

		AddComponent(0x03F7, -2, -2, 0);
		AddComponent(0x03F8, 2, 2, 0);
		AddComponent(0x03F9, -2, 2, 0);
		AddComponent(0x03FA, 2, -2, 0);
	}

	public ChampionPlatform(Serial serial)
		: base(serial)
	{
	}

	public void AddComponent(int id, int x, int y, int z)
	{
		AddonComponent ac = new(id)
		{
			Hue = 0x452
		};

		AddComponent(ac, x, y, z);
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();
		_mSpawn?.Delete();
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(_mSpawn);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mSpawn = reader.ReadItem() as ChampionSpawn;

				if (_mSpawn == null)
					Delete();

				break;
			}
		}
	}
	/*
	private void FixComponents()
	{
		foreach (var comp in Components)
		{
			comp.Hue = 0x452;

			if (comp.ItemID == 0x750)
				comp.ItemID = 0x3EE;

			if (comp.ItemID == 0x751)
				comp.ItemID = 0x3EF;

			if (comp.ItemID == 0x752)
				comp.ItemID = 0x3F0;

			if (comp.ItemID == 0x753)
				comp.ItemID = 0x3F1;

			if (comp.ItemID == 0x754)
				comp.ItemID = 0x3F2;

			if (comp.ItemID == 0x759)
				comp.ItemID = 0x3F7;

			if (comp.ItemID == 0x75A)
				comp.ItemID = 0x3F8;

			if (comp.ItemID == 0x75B)
				comp.ItemID = 0x3F9;

			if (comp.ItemID == 0x75C)
				comp.ItemID = 0x3FA;
		}
	}
	*/
}
