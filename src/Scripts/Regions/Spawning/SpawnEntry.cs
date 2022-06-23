using System;
using System.Collections;
using System.Collections.Generic;
using Server.Commands;
using Server.ContextMenus;
using Server.Mobiles;

namespace Server.Regions;

public class SpawnEntry : ISpawner
{
	public static readonly TimeSpan DefaultMinSpawnTime = TimeSpan.FromMinutes(2.0);
	public static readonly TimeSpan DefaultMaxSpawnTime = TimeSpan.FromMinutes(5.0);
	public static readonly Direction InvalidDirection = Direction.Running;
	private static readonly Hashtable MTable = new();
	private static List<IEntity> _mRemoveList;
	private readonly TimeSpan _mMinSpawnTime;
	private readonly TimeSpan _mMaxSpawnTime;
	private DateTime _mNextSpawn;
	private Timer _mSpawnTimer;
	public SpawnEntry(int id, BaseRegion region, Point3D home, int range, Direction direction, SpawnDefinition definition, int max, TimeSpan minSpawnTime, TimeSpan maxSpawnTime)
	{
		Id = id;
		Region = region;
		HomeLocation = home;
		HomeRange = range;
		Direction = direction;
		Definition = definition;
		SpawnedObjects = new List<ISpawnable>();
		Max = max;
		_mMinSpawnTime = minSpawnTime;
		_mMaxSpawnTime = maxSpawnTime;
		Running = false;

		if (MTable.Contains(id))
			Console.WriteLine("Warning: double SpawnEntry ID '{0}'", id);
		else
			MTable[id] = this;
	}

	public static Hashtable Table => MTable;
	// When a creature's AI is deactivated (PlayerRangeSensitive optimization) does it return home?
	public bool ReturnOnDeactivate => true;
	// Are creatures unlinked on taming (true) or should they also go out of the region (false)?
	public bool UnlinkOnTaming => false;
	// Are unlinked and untamed creatures removed after 20 hours?
	public bool RemoveIfUntamed => false;
	public int Id { get; }

	public BaseRegion Region { get; }

	public Point3D HomeLocation { get; }

	public int HomeRange { get; }

	public Direction Direction { get; }

	public SpawnDefinition Definition { get; }

	public List<ISpawnable> SpawnedObjects { get; }

	public int Max { get; private set; }

	public TimeSpan MinSpawnTime => _mMinSpawnTime;
	public TimeSpan MaxSpawnTime => _mMaxSpawnTime;
	public bool Running { get; private set; }

	public bool Complete => SpawnedObjects.Count >= Max;
	public bool Spawning => Running && !Complete;

	public virtual void GetSpawnProperties(ISpawnable spawn, ObjectPropertyList list)
	{ }

	public virtual void GetSpawnContextEntries(ISpawnable spawn, Mobile m, List<ContextMenuEntry> list)
	{ }

	public static void Remove(GenericReader reader, int version)
	{
		int count = reader.ReadInt();

		for (int i = 0; i < count; i++)
		{
			int serial = reader.ReadInt();
			IEntity entity = World.FindEntity(serial);

			if (entity != null)
			{
				if (_mRemoveList == null)
					_mRemoveList = new List<IEntity>();

				_mRemoveList.Add(entity);
			}
		}

		reader.ReadBool(); // m_Running

		if (reader.ReadBool())
			reader.ReadDeltaTime(); // m_NextSpawn
	}

	public static void Initialize()
	{
		if (_mRemoveList != null)
		{
			foreach (IEntity ent in _mRemoveList)
			{
				ent.Delete();
			}

			_mRemoveList = null;
		}

		SpawnPersistence.EnsureExistence();

		CommandSystem.Register("RespawnAllRegions", AccessLevel.Administrator, RespawnAllRegions_OnCommand);
		CommandSystem.Register("RespawnRegion", AccessLevel.GameMaster, RespawnRegion_OnCommand);
		CommandSystem.Register("DelAllRegionSpawns", AccessLevel.Administrator, DelAllRegionSpawns_OnCommand);
		CommandSystem.Register("DelRegionSpawns", AccessLevel.GameMaster, DelRegionSpawns_OnCommand);
		CommandSystem.Register("StartAllRegionSpawns", AccessLevel.Administrator, StartAllRegionSpawns_OnCommand);
		CommandSystem.Register("StartRegionSpawns", AccessLevel.GameMaster, StartRegionSpawns_OnCommand);
		CommandSystem.Register("StopAllRegionSpawns", AccessLevel.Administrator, StopAllRegionSpawns_OnCommand);
		CommandSystem.Register("StopRegionSpawns", AccessLevel.GameMaster, StopRegionSpawns_OnCommand);
	}

	public Point3D RandomSpawnLocation(int spawnHeight, bool land, bool water)
	{
		return Region.RandomSpawnLocation(spawnHeight, land, water, HomeLocation, HomeRange);
	}

	public void Start()
	{
		if (Running)
			return;

		Running = true;
		CheckTimer();
	}

	public void Stop()
	{
		if (!Running)
			return;

		Running = false;
		CheckTimer();
	}

	public void DeleteSpawnedObjects()
	{
		InternalDeleteSpawnedObjects();

		Running = false;
		CheckTimer();
	}

	public void Respawn()
	{
		InternalDeleteSpawnedObjects();

		for (int i = 0; !Complete && i < Max; i++)
			Spawn();

		Running = true;
		CheckTimer();
	}

	public void Delete()
	{
		Max = 0;
		InternalDeleteSpawnedObjects();

		if (_mSpawnTimer != null)
		{
			_mSpawnTimer.Stop();
			_mSpawnTimer = null;
		}

		if (MTable[Id] == this)
			MTable.Remove(Id);
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(SpawnedObjects.Count);

		for (int i = 0; i < SpawnedObjects.Count; i++)
		{
			ISpawnable spawn = SpawnedObjects[i];

			int serial = spawn.Serial;

			writer.Write(serial);
		}

		writer.Write(Running);

		if (_mSpawnTimer != null)
		{
			writer.Write(true);
			writer.WriteDeltaTime(_mNextSpawn);
		}
		else
		{
			writer.Write(false);
		}
	}

	public void Deserialize(GenericReader reader, int version)
	{
		int count = reader.ReadInt();

		for (int i = 0; i < count; i++)
		{
			int serial = reader.ReadInt();

			if (World.FindEntity(serial) is ISpawnable spawnableEntity)
				Add(spawnableEntity);
		}

		Running = reader.ReadBool();

		if (reader.ReadBool())
		{
			_mNextSpawn = reader.ReadDeltaTime();

			if (Spawning)
			{
				_mSpawnTimer?.Stop();

				TimeSpan delay = _mNextSpawn - DateTime.UtcNow;
				_mSpawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, TimerCallback);
			}
		}

		CheckTimer();
	}

	private static BaseRegion GetCommandData(CommandEventArgs args)
	{
		Mobile from = args.Mobile;

		Region reg;
		if (args.Length == 0)
		{
			reg = from.Region;
		}
		else
		{
			string name = args.GetString(0);
			//reg = (Region) from.Map.Regions[name];

			if (!from.Map.Regions.TryGetValue(name, out reg))
			{
				from.SendMessage("Could not find region '{0}'.", name);
				return null;
			}
		}

		BaseRegion br = reg as BaseRegion;

		if (br == null || br.Spawns == null)
		{
			from.SendMessage("There are no spawners in region '{0}'.", reg);
			return null;
		}

		return br;
	}

	[Usage("RespawnAllRegions")]
	[Description("Respawns all regions and sets the spawners as running.")]
	private static void RespawnAllRegions_OnCommand(CommandEventArgs args)
	{
		foreach (SpawnEntry entry in MTable.Values)
		{
			entry.Respawn();
		}

		args.Mobile.SendMessage("All regions have respawned.");
	}

	[Usage("RespawnRegion [<region name>]")]
	[Description("Respawns the region in which you are (or that you provided) and sets the spawners as running.")]
	private static void RespawnRegion_OnCommand(CommandEventArgs args)
	{
		BaseRegion region = GetCommandData(args);

		if (region == null)
			return;

		for (int i = 0; i < region.Spawns.Length; i++)
			region.Spawns[i].Respawn();

		args.Mobile.SendMessage("Region '{0}' has respawned.", region);
	}

	[Usage("DelAllRegionSpawns")]
	[Description("Deletes all spawned objects of every regions and sets the spawners as not running.")]
	private static void DelAllRegionSpawns_OnCommand(CommandEventArgs args)
	{
		foreach (SpawnEntry entry in MTable.Values)
		{
			entry.DeleteSpawnedObjects();
		}

		args.Mobile.SendMessage("All region spawned objects have been deleted.");
	}

	[Usage("DelRegionSpawns [<region name>]")]
	[Description("Deletes all spawned objects of the region in which you are (or that you provided) and sets the spawners as not running.")]
	private static void DelRegionSpawns_OnCommand(CommandEventArgs args)
	{
		BaseRegion region = GetCommandData(args);

		if (region == null)
			return;

		for (int i = 0; i < region.Spawns.Length; i++)
			region.Spawns[i].DeleteSpawnedObjects();

		args.Mobile.SendMessage("Spawned objects of region '{0}' have been deleted.", region);
	}

	[Usage("StartAllRegionSpawns")]
	[Description("Sets the region spawners of all regions as running.")]
	private static void StartAllRegionSpawns_OnCommand(CommandEventArgs args)
	{
		foreach (SpawnEntry entry in MTable.Values)
		{
			entry.Start();
		}

		args.Mobile.SendMessage("All region spawners have started.");
	}

	[Usage("StartRegionSpawns [<region name>]")]
	[Description("Sets the region spawners of the region in which you are (or that you provided) as running.")]
	private static void StartRegionSpawns_OnCommand(CommandEventArgs args)
	{
		BaseRegion region = GetCommandData(args);

		if (region == null)
			return;

		for (int i = 0; i < region.Spawns.Length; i++)
			region.Spawns[i].Start();

		args.Mobile.SendMessage("Spawners of region '{0}' have started.", region);
	}

	[Usage("StopAllRegionSpawns")]
	[Description("Sets the region spawners of all regions as not running.")]
	private static void StopAllRegionSpawns_OnCommand(CommandEventArgs args)
	{
		foreach (SpawnEntry entry in MTable.Values)
		{
			entry.Stop();
		}

		args.Mobile.SendMessage("All region spawners have stopped.");
	}

	[Usage("StopRegionSpawns [<region name>]")]
	[Description("Sets the region spawners of the region in which you are (or that you provided) as not running.")]
	private static void StopRegionSpawns_OnCommand(CommandEventArgs args)
	{
		BaseRegion region = GetCommandData(args);

		if (region == null)
			return;

		for (int i = 0; i < region.Spawns.Length; i++)
			region.Spawns[i].Stop();

		args.Mobile.SendMessage("Spawners of region '{0}' have stopped.", region);
	}

	private void Spawn()
	{
		ISpawnable spawn = Definition.Spawn(this);

		if (spawn != null)
			Add(spawn);
	}

	private void Add(ISpawnable spawn)
	{
		SpawnedObjects.Add(spawn);

		spawn.Spawner = this;

		if (spawn is BaseCreature)
			((BaseCreature)spawn).RemoveIfUntamed = RemoveIfUntamed;
	}

	void ISpawner.Remove(ISpawnable spawn)
	{
		SpawnedObjects.Remove(spawn);

		CheckTimer();
	}

	private TimeSpan RandomTime()
	{
		int min = (int)_mMinSpawnTime.TotalSeconds;
		int max = (int)_mMaxSpawnTime.TotalSeconds;

		int rand = Utility.RandomMinMax(min, max);
		return TimeSpan.FromSeconds(rand);
	}

	private void CheckTimer()
	{
		if (Spawning)
		{
			if (_mSpawnTimer == null)
			{
				TimeSpan time = RandomTime();
				_mSpawnTimer = Timer.DelayCall(time, TimerCallback);
				_mNextSpawn = DateTime.UtcNow + time;
			}
		}
		else if (_mSpawnTimer != null)
		{
			_mSpawnTimer.Stop();
			_mSpawnTimer = null;
		}
	}

	private void TimerCallback()
	{
		int amount = Math.Max((Max - SpawnedObjects.Count) / 3, 1);

		for (int i = 0; i < amount; i++)
			Spawn();

		_mSpawnTimer = null;
		CheckTimer();
	}

	private void InternalDeleteSpawnedObjects()
	{
		foreach (ISpawnable spawnable in SpawnedObjects)
		{
			spawnable.Spawner = null;

			bool uncontrolled = spawnable is not BaseCreature {Controlled: true};

			if (uncontrolled)
				spawnable.Delete();
		}

		SpawnedObjects.Clear();
	}
}
