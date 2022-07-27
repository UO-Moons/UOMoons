using Server.Commands;
using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Doom;

public enum GauntletSpawnerState
{
	InSequence,
	InProgress,
	Completed
}

public sealed class GauntletSpawner : BaseItem
{
	private const int PlayersPerSpawn = 5;

	private const int InSequenceItemHue = 0x000;
	private const int InProgressItemHue = 0x676;
	private const int CompletedItemHue = 0x455;

	private GauntletSpawnerState _mState;
	private Rectangle2D _mRegionBounds;

	[CommandProperty(AccessLevel.GameMaster)]
	private string TypeName { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private BaseDoor Door { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private BaseAddon Addon { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public GauntletSpawner Sequence { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool HasCompleted
	{
		get
		{
			return Creatures.Count != 0 && Creatures.All(mob => mob.Deleted);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle2D RegionBounds
	{
		get => _mRegionBounds;
		private set => _mRegionBounds = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public GauntletSpawnerState State
	{
		get => _mState;
		set
		{
			if (_mState == value)
				return;

			_mState = value;

			int hue = 0;
			bool lockDoors = (_mState == GauntletSpawnerState.InProgress);

			hue = _mState switch
			{
				GauntletSpawnerState.InSequence => InSequenceItemHue,
				GauntletSpawnerState.InProgress => InProgressItemHue,
				GauntletSpawnerState.Completed => CompletedItemHue,
				_ => hue
			};

			if (Door != null)
			{
				Door.Hue = hue;
				Door.Locked = lockDoors;

				if (lockDoors)
				{
					Door.KeyValue = Key.RandomValue();
					Door.Open = false;
				}

				if (Door.Link != null)
				{
					Door.Link.Hue = hue;
					Door.Link.Locked = lockDoors;

					if (lockDoors)
					{
						Door.Link.KeyValue = Key.RandomValue();
						Door.Open = false;
					}
				}
			}

			if (Addon != null)
				Addon.Hue = hue;

			if (_mState == GauntletSpawnerState.InProgress)
			{
				CreateRegion();
				FullSpawn();

				_mTimer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), Slice);
			}
			else
			{
				ClearCreatures();
				ClearTraps();
				DestroyRegion();

				_mTimer?.Stop();

				_mTimer = null;
			}
		}
	}

	private Timer _mTimer;

	private List<Mobile> Creatures { get; set; }

	private List<BaseTrap> Traps { get; set; }

	private Region Region { get; set; }

	private void CreateRegion()
	{
		if (Region != null)
			return;

		Map map = Map;

		if (map == null || map == Map.Internal)
			return;

		Region = new GauntletRegion(this, map);
	}

	private void DestroyRegion()
	{
		Region?.Unregister();

		Region = null;
	}

	private int ComputeTrapCount()
	{
		int area = _mRegionBounds.Width * _mRegionBounds.Height;

		return area / 100;
	}

	private void ClearTraps()
	{
		for (var i = 0; i < Traps.Count; ++i)
			Traps[i].Delete();

		Traps.Clear();
	}

	private void SpawnTrap()
	{
		Map map = Map;

		if (map == null)
			return;
		int random = Utility.Random(100);

		BaseTrap trap = random switch
		{
			< 22 => new SawTrap(Utility.RandomBool() ? SawTrapType.WestFloor : SawTrapType.NorthFloor),
			< 44 => new SpikeTrap(Utility.RandomBool() ? SpikeTrapType.WestFloor : SpikeTrapType.NorthFloor),
			< 66 => new GasTrap(Utility.RandomBool() ? GasTrapType.NorthWall : GasTrapType.WestWall),
			< 88 => new FireColumnTrap(),
			_ => new MushroomTrap()
		};

		if (trap is FireColumnTrap or MushroomTrap)
			trap.Hue = 0x451;

		// try 10 times to find a valid location
		for (var i = 0; i < 10; ++i)
		{
			int x = Utility.Random(_mRegionBounds.X, _mRegionBounds.Width);
			int y = Utility.Random(_mRegionBounds.Y, _mRegionBounds.Height);
			int z = Z;

			if (!map.CanFit(x, y, z, 16, false, false))
				z = map.GetAverageZ(x, y);

			if (!map.CanFit(x, y, z, 16, false, false))
				continue;

			trap.MoveToWorld(new Point3D(x, y, z), map);
			Traps.Add(trap);

			return;
		}

		trap.Delete();
	}

	private int ComputeSpawnCount()
	{
		int playerCount = 0;

		Map map = Map;

		if (map != null)
		{
			Point3D loc = GetWorldLocation();

			Region reg = Region.Find(loc, map).GetRegion("Doom Gauntlet");

			if (reg != null)
				playerCount = reg.GetPlayerCount();
		}

		if (playerCount == 0 && Region != null)
			playerCount = Region.GetPlayerCount();

		int count = (playerCount + PlayersPerSpawn - 1) / PlayersPerSpawn;

		if (count < 1)
			count = 1;

		return count;
	}

	private void ClearCreatures()
	{
		for (var i = 0; i < Creatures.Count; ++i)
			Creatures[i].Delete();

		Creatures.Clear();
	}

	private void FullSpawn()
	{
		ClearCreatures();

		int count = ComputeSpawnCount();

		for (int i = 0; i < count; ++i)
			Spawn();

		ClearTraps();

		count = ComputeTrapCount();

		for (int i = 0; i < count; ++i)
			SpawnTrap();
	}

	private void Spawn()
	{
		try
		{
			if (TypeName == null)
				return;

			Type type = Assembler.FindTypeByName(TypeName, true);

			if (type == null)
				return;

			object obj = Activator.CreateInstance(type);

			switch (obj)
			{
				case null:
					return;
				case Item item:
					item.Delete();
					break;
				case Mobile mob:
					mob.MoveToWorld(GetWorldLocation(), Map);

					Creatures.Add(mob);
					break;
			}
		}
		catch
		{
			// ignored
		}
	}

	private void RecurseReset()
	{
		if (_mState == GauntletSpawnerState.InSequence) return;
		State = GauntletSpawnerState.InSequence;

		if (Sequence is { Deleted: false })
			Sequence.RecurseReset();
	}

	private void Slice()
	{
		if (_mState != GauntletSpawnerState.InProgress)
			return;

		int count = ComputeSpawnCount();

		for (int i = Creatures.Count; i < count; ++i)
			Spawn();

		if (!HasCompleted) return;
		State = GauntletSpawnerState.Completed;

		if (Sequence == null || Sequence.Deleted) return;
		if (Sequence.State == GauntletSpawnerState.Completed)
			RecurseReset();

		Sequence.State = GauntletSpawnerState.InProgress;
	}

	public override string DefaultName => "doom spawner";

	[Constructable]
	public GauntletSpawner() : this(null)
	{
	}

	[Constructable]
	private GauntletSpawner(string typeName) : base(0x36FE)
	{
		Visible = false;
		Movable = false;

		TypeName = typeName;
		Creatures = new List<Mobile>();
		Traps = new List<BaseTrap>();
	}

	public GauntletSpawner(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(_mRegionBounds);

		writer.WriteItemList(Traps, false);

		writer.Write(Creatures, false);

		writer.Write(TypeName);
		writer.Write(Door);
		writer.Write(Addon);
		writer.Write(Sequence);

		writer.Write((int)_mState);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_mRegionBounds = reader.ReadRect2D();
				Traps = reader.ReadStrongItemList<BaseTrap>();

				Creatures = reader.ReadStrongMobileList();

				TypeName = reader.ReadString();
				Door = reader.ReadItem<BaseDoor>();
				Addon = reader.ReadItem<BaseAddon>();
				Sequence = reader.ReadItem<GauntletSpawner>();

				State = (GauntletSpawnerState)reader.ReadInt();

				break;
			}
		}
	}

	public static void CreateTeleporter(int xFrom, int yFrom, int xTo, int yTo)
	{
		Static telePad = new(0x1822);
		Teleporter teleItem = new(new Point3D(xTo, yTo, -1), Map.Malas, false);

		telePad.Hue = 0x482;
		telePad.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

		teleItem.MoveToWorld(new Point3D(xFrom, yFrom, -1), Map.Malas);

		teleItem.SourceEffect = true;
		teleItem.DestEffect = true;
		teleItem.SoundID = 0x1FE;
	}

	public static BaseDoor CreateDoorSet(int xDoor, int yDoor, bool doorEastToWest, int hue)
	{
		BaseDoor hiDoor = new MetalDoor(doorEastToWest ? DoorFacing.NorthCcw : DoorFacing.WestCw);
		BaseDoor loDoor = new MetalDoor(doorEastToWest ? DoorFacing.SouthCw : DoorFacing.EastCcw);

		hiDoor.MoveToWorld(new Point3D(xDoor, yDoor, -1), Map.Malas);
		loDoor.MoveToWorld(new Point3D(xDoor + (doorEastToWest ? 0 : 1), yDoor + (doorEastToWest ? 1 : 0), -1), Map.Malas);

		hiDoor.Link = loDoor;
		loDoor.Link = hiDoor;

		hiDoor.Hue = hue;
		loDoor.Hue = hue;

		return hiDoor;
	}

	public static GauntletSpawner CreateSpawner(string typeName, int xSpawner, int ySpawner, int xDoor, int yDoor, int xPentagram, int yPentagram, bool doorEastToWest, int xStart, int yStart, int xWidth, int yHeight)
	{
		GauntletSpawner spawner = new(typeName);

		spawner.MoveToWorld(new Point3D(xSpawner, ySpawner, -1), Map.Malas);

		if (xDoor > 0 && yDoor > 0)
			spawner.Door = CreateDoorSet(xDoor, yDoor, doorEastToWest, 0);

		spawner.RegionBounds = new Rectangle2D(xStart, yStart, xWidth, yHeight);

		if (xPentagram > 0 && yPentagram > 0)
		{
			PentagramAddon pentagram = new();

			pentagram.MoveToWorld(new Point3D(xPentagram, yPentagram, -1), Map.Malas);

			spawner.Addon = pentagram;
		}

		return spawner;
	}

	public static void CreatePricedHealer(int price, int x, int y)
	{
		PricedHealer healer = new(price);

		healer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

		healer.Home = healer.Location;
		healer.RangeHome = 5;
	}

	public static void CreateMorphItem(int x, int y, int inactiveItemId, int activeItemId, int range, int hue)
	{
		MorphItem item = new(inactiveItemId, activeItemId, range)
		{
			Hue = hue
		};
		item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
	}

	public static void CreateVarietyDealer(int x, int y)
	{
		VarietyDealer dealer = new()
		{
			/* Begin outfit */
			Name = "Nix",
			Title = "the Variety Dealer",

			Body = 400,
			Female = false,
			Hue = 0x8835
		};

		List<Item> items = new(dealer.Items);

		for (var i = 0; i < items.Count; ++i)
		{
			Item item = items[i];

			if (item.Layer != Layer.ShopBuy && item.Layer != Layer.ShopResale && item.Layer != Layer.ShopSell)
				item.Delete();
		}

		dealer.HairItemId = 0x2049; // Pig Tails
		dealer.HairHue = 0x482;

		dealer.FacialHairItemId = 0x203E;
		dealer.FacialHairHue = 0x482;

		dealer.AddItem(new FloppyHat(1));
		dealer.AddItem(new Robe(1));

		dealer.AddItem(new LanternOfSouls());

		dealer.AddItem(new Sandals(0x482));
		/* End outfit */

		dealer.MoveToWorld(new Point3D(x, y, -1), Map.Malas);

		dealer.Home = dealer.Location;
		dealer.RangeHome = 2;
	}
}


public class GauntletRegion : BaseRegion
{
	private readonly GauntletSpawner m_Spawner;

	public GauntletRegion(GauntletSpawner spawner, Map map)
		: base(null, map, Find(spawner.Location, spawner.Map), spawner.RegionBounds)
	{
		m_Spawner = spawner;

		GoLocation = spawner.Location;

		Register();
	}
	 
	public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
	{
		if (global is < 12 or > 12)
			global = 12;
	}

	public override void OnEnter(Mobile m)
	{
	}

	public override void OnExit(Mobile m)
	{
	}
}
