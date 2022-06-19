using Server.Items;

namespace Server.Engines.Champions;

public sealed class ChampionAltar : PentagramAddon
{
	private ChampionSpawn _mSpawn;
	public ChampionAltar(ChampionSpawn spawn)
	{
		_mSpawn = spawn;
		Hue = 0x455;
	}

	public ChampionAltar(Serial serial)
		: base(serial)
	{
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
				else if (!_mSpawn.Active)
					Hue = 0x455;
				else
					Hue = 0;

				break;
			}
		}
	}
}
