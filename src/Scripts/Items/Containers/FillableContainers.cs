using Server.Mobiles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public abstract class FillableContainer : LockableContainer
{
	public virtual int MinRespawnMinutes => 60;
	public virtual int MaxRespawnMinutes => 90;

	public virtual bool IsLockable => true;
	public virtual bool IsTrapable => IsLockable;

	public virtual int SpawnThreshold => 2;

	protected FillableContent m_Content;

	private Timer m_RespawnTimer;

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime NextRespawnTime { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public FillableContentType ContentType
	{
		get => FillableContent.Lookup(m_Content);
		set => Content = FillableContent.Lookup(value);
	}

	public FillableContent Content
	{
		get => m_Content;
		set
		{
			if (m_Content == value)
				return;

			m_Content = value;

			for (int i = Items.Count - 1; i >= 0; --i)
			{
				if (i < Items.Count)
					Items[i].Delete();
			}

			Respawn();
		}
	}

	public FillableContainer(int itemId)
		: base(itemId)
	{
		Movable = false;
	}

	public override void OnMapChange()
	{
		base.OnMapChange();
		AcquireContent();
	}

	public override void OnLocationChange(Point3D oldLocation)
	{
		base.OnLocationChange(oldLocation);
		AcquireContent();
	}

	public virtual void AcquireContent()
	{
		if (m_Content != null)
			return;

		m_Content = FillableContent.Acquire(GetWorldLocation(), Map);

		if (m_Content != null)
			Respawn();
	}

	public override void OnItemRemoved(Item item)
	{
		CheckRespawn();
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		if (m_RespawnTimer == null)
			return;

		m_RespawnTimer.Stop();
		m_RespawnTimer = null;
	}

	public int GetItemsCount()
	{
		return Items.Sum(item => item.Amount);
	}

	private void CheckRespawn()
	{
		bool canSpawn = m_Content != null && !Deleted && GetItemsCount() <= SpawnThreshold && !Movable && Parent == null && !IsLockedDown && !IsSecure;

		if (canSpawn)
		{
			if (m_RespawnTimer != null)
				return;

			int mins = Utility.RandomMinMax(MinRespawnMinutes, MaxRespawnMinutes);
			TimeSpan delay = TimeSpan.FromMinutes(mins);

			NextRespawnTime = DateTime.UtcNow + delay;
			m_RespawnTimer = Timer.DelayCall(delay, Respawn);
		}
		else if (m_RespawnTimer != null)
		{
			m_RespawnTimer.Stop();
			m_RespawnTimer = null;
		}
	}

	public void Respawn()
	{
		if (m_RespawnTimer != null)
		{
			m_RespawnTimer.Stop();
			m_RespawnTimer = null;
		}

		if (m_Content == null || Deleted)
			return;

		GenerateContent();

		if (IsLockable)
		{
			Locked = true;

			int difficulty = (m_Content.Level - 1) * 30;

			LockLevel = difficulty - 10;
			MaxLockLevel = difficulty + 30;
			RequiredSkill = difficulty;
		}

		if (IsTrapable && (m_Content.Level > 1 || 4 > Utility.Random(5)))
		{
			TrapType = m_Content.Level > Utility.Random(5) ? TrapType.PoisonTrap : TrapType.ExplosionTrap;

			TrapPower = m_Content.Level * Utility.RandomMinMax(10, 30);
			TrapLevel = m_Content.Level;
		}
		else
		{
			TrapType = TrapType.None;
			TrapPower = 0;
			TrapLevel = 0;
		}

		CheckRespawn();
	}

	protected virtual int GetSpawnCount()
	{
		int itemsCount = GetItemsCount();

		if (itemsCount > SpawnThreshold)
			return 0;

		int maxSpawnCount = (1 + SpawnThreshold - itemsCount) * 2;

		return Utility.RandomMinMax(0, maxSpawnCount);
	}

	public virtual void GenerateContent()
	{
		if (m_Content == null || Deleted)
			return;

		int toSpawn = GetSpawnCount();

		for (int i = 0; i < toSpawn; ++i)
		{
			Item item = m_Content.Construct();

			if (item != null)
			{
				List<Item> list = Items;

				for (int j = 0; j < list.Count; ++j)
				{
					Item subItem = list[j];

					if (subItem is not Container && subItem.StackWith(null, item, false))
						break;
				}

				if (!item.Deleted)
					DropItem(item);
			}
		}
	}

	public FillableContainer(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version

		writer.Write((int)ContentType);

		if (m_RespawnTimer != null)
		{
			writer.Write(true);
			writer.WriteDeltaTime(NextRespawnTime);
		}
		else
		{
			writer.Write(false);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				m_Content = FillableContent.Lookup((FillableContentType)reader.ReadInt());

				if (reader.ReadBool())
				{
					NextRespawnTime = reader.ReadDeltaTime();

					TimeSpan delay = NextRespawnTime - DateTime.UtcNow;
					m_RespawnTimer = Timer.DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, Respawn);
				}
				else
				{
					CheckRespawn();
				}

				break;
			}
		}
	}
}

[Flipable(0xA97, 0xA99, 0xA98, 0xA9A, 0xA9B, 0xA9C)]
public class LibraryBookcase : FillableContainer
{
	public override bool IsLockable => false;
	public override int SpawnThreshold => 5;

	protected override int GetSpawnCount()
	{
		return 5 - GetItemsCount();
	}

	public override void AcquireContent()
	{
		if (m_Content != null)
			return;

		m_Content = FillableContent.Library;

		if (m_Content != null)
			Respawn();
	}

	[Constructable]
	public LibraryBookcase()
		: base(0xA97)
	{
		Weight = 1.0;
	}

	public LibraryBookcase(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}

[Flipable(0xE3D, 0xE3C)]
public class FillableLargeCrate : FillableContainer
{
	[Constructable]
	public FillableLargeCrate()
		: base(0xE3D)
	{
		Weight = 1.0;
	}

	public FillableLargeCrate(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}

[Flipable(0x9A9, 0xE7E)]
public class FillableSmallCrate : FillableContainer
{
	[Constructable]
	public FillableSmallCrate()
		: base(0x9A9)
	{
		Weight = 1.0;
	}

	public FillableSmallCrate(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}

[Flipable(0x9AA, 0xE7D)]
public class FillableWoodenBox : FillableContainer
{
	[Constructable]
	public FillableWoodenBox()
		: base(0x9AA)
	{
		Weight = 4.0;
	}

	public FillableWoodenBox(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

[Flipable(0x9A8, 0xE80)]
public class FillableMetalBox : FillableContainer
{
	[Constructable]
	public FillableMetalBox()
		: base(0x9A8)
	{
	}

	public FillableMetalBox(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}

public class FillableBarrel : FillableContainer
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D WorldLocation { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Map WorldMap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime NextReturn { get; set; }

	[Constructable]
	public FillableBarrel()
		: base(0xE77)
	{
	}

	public FillableBarrel(Serial serial)
		: base(serial)
	{
	}

	public override bool IsLockable => false;

	public void Pour(Mobile from, BaseBeverage beverage)
	{
		if (beverage.Content == BeverageType.Water)
		{
			if (Items.Count > 0)
			{
				from.SendLocalizedMessage(500848); // Couldn't pour it there.  It was already full.
				beverage.PrivateOverheadMessage(Network.MessageType.Regular, 0, 500841, from.NetState); // that has somethign in it.
			}
			else
			{
				WaterBarrel barrel = new WaterBarrel
				{
					Movable = false
				};
				barrel.MoveToWorld(Location, Map);

				WorldLocation = Location;
				WorldMap = Map;
				NextReturn = DateTime.UtcNow + TimeSpan.FromHours(1);

				beverage.Pour_OnTarget(from, barrel);

				Internalize();
			}
		}
	}

	public void TryReturn()
	{
		if (WorldMap != null)
		{
			IPooledEnumerable eable = WorldMap.GetItemsInRange(WorldLocation, 0);

			foreach (Item item in eable)
			{
				if (item is WaterBarrel && item.Z == WorldLocation.Z)
				{
					eable.Free();
					return;
				}
			}

			eable.Free();
			NextReturn = DateTime.MinValue;
			MoveToWorld(WorldLocation, WorldMap);
			Respawn();
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(2); // version

		writer.Write(WorldLocation);
		writer.Write(WorldMap);

		if (NextReturn != DateTime.MinValue && NextReturn < DateTime.UtcNow)
		{
			Timer.DelayCall(TimeSpan.FromSeconds(20), TryReturn);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 2:
				WorldLocation = reader.ReadPoint3D();
				WorldMap = reader.ReadMap();
				break;
		}

		if (Map == Map.Internal)
		{
			if (WorldMap != null)
			{
				NextReturn = DateTime.UtcNow;
				Timer.DelayCall(TimeSpan.FromSeconds(20), TryReturn);
			}
			else
			{
				Delete();
			}
		}
	}
}

[Flipable(0x9AB, 0xE7C)]
public class FillableMetalChest : FillableContainer
{
	[Constructable]
	public FillableMetalChest()
		: base(0x9AB)
	{
	}

	public FillableMetalChest(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

[Flipable(0xE41, 0xE40)]
public class FillableMetalGoldenChest : FillableContainer
{
	[Constructable]
	public FillableMetalGoldenChest()
		: base(0xE41)
	{
	}

	public FillableMetalGoldenChest(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

[Flipable(0xE43, 0xE42)]
public class FillableWoodenChest : FillableContainer
{
	[Constructable]
	public FillableWoodenChest()
		: base(0xE43)
	{
	}

	public FillableWoodenChest(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}

public class FillableEntry
{
	protected readonly Type[] m_Types;
	protected readonly int m_Weight;

	public Type[] Types => m_Types;
	public int Weight => m_Weight;

	public FillableEntry(Type type)
		: this(1, new[] { type })
	{
	}

	public FillableEntry(int weight, Type type)
		: this(weight, new[] { type })
	{
	}

	public FillableEntry(Type[] types)
		: this(1, types)
	{
	}

	public FillableEntry(int weight, Type[] types)
	{
		m_Weight = weight;
		m_Types = types;
	}

	public FillableEntry(int weight, Type[] types, int offset, int count)
	{
		m_Weight = weight;
		m_Types = new Type[count];

		for (int i = 0; i < m_Types.Length; ++i)
			m_Types[i] = types[offset + i];
	}

	public virtual Item Construct()
	{
		Item item = Loot.Construct(m_Types);

		switch (item)
		{
			case Key key:
				key.ItemId = Utility.RandomList((int)KeyType.Copper, (int)KeyType.Gold, (int)KeyType.Iron, (int)KeyType.Rusty);
				break;
			case Arrow:
			case CrossBowBolt:
				item.Amount = Utility.RandomMinMax(2, 6);
				break;
			case Bandage:
			case Lockpick:
				item.Amount = Utility.RandomMinMax(1, 3);
				break;
		}

		return item;
	}
}

public class FillableBvrge : FillableEntry
{
	private BeverageType Content { get; }

	public FillableBvrge(Type type, BeverageType content)
		: this(1, type, content)
	{
	}

	public FillableBvrge(int weight, Type type, BeverageType content)
		: base(weight, type)
	{
		Content = content;
	}

	public override Item Construct()
	{
		Item item;

		int index = Utility.Random(m_Types.Length);

		if (m_Types[index] == typeof(BeverageBottle))
		{
			item = new BeverageBottle(Content);
		}
		else if (m_Types[index] == typeof(Jug))
		{
			item = new Jug(Content);
		}
		else
		{
			item = base.Construct();

			if (item is not BaseBeverage bev)
				return item;
			bev.Content = Content;
			bev.Quantity = bev.MaxQuantity;
		}

		return item;
	}
}

public enum FillableContentType
{
	None = -1,
	Weaponsmith, Provisioner, Mage,
	Alchemist, Armorer, ArtisanGuild,
	Baker, Bard, Blacksmith,
	Bowyer, Butcher, Carpenter,
	Clothier, Cobbler, Docks,
	Farm, FighterGuild, Guard,
	Healer, Herbalist, Inn,
	Jeweler, Library, Merchant,
	Mill, Mine, Observatory,
	Painter, Ranger, Stables,
	Tanner, Tavern, ThiefGuild,
	Tinker, Veterinarian
}

public sealed class FillableContent
{
	private readonly FillableEntry[] m_Entries;
	private readonly int m_Weight;

	public int Level { get; }
	private Type[] Vendors { get; }

	public FillableContentType TypeId => Lookup(this);

	private FillableContent(int level, Type[] vendors, FillableEntry[] entries)
	{
		Level = level;
		Vendors = vendors;
		m_Entries = entries;

		for (int i = 0; i < entries.Length; ++i)
			m_Weight += entries[i].Weight;
	}

	public Item Construct()
	{
		int index = Utility.Random(m_Weight);

		for (int i = 0; i < m_Entries.Length; ++i)
		{
			FillableEntry entry = m_Entries[i];

			if (index < entry.Weight)
				return entry.Construct();

			index -= entry.Weight;
		}

		return null;
	}

	private static readonly FillableContent Alchemist = new(
		1,
		new[]
		{
			typeof( Alchemist )
		},
		new FillableEntry[]
		{
			new( typeof( NightSightPotion ) ),
			new( typeof( LesserCurePotion ) ),
			new( typeof( AgilityPotion ) ),
			new( typeof( StrengthPotion ) ),
			new( typeof( LesserPoisonPotion ) ),
			new( typeof( RefreshPotion ) ),
			new( typeof( LesserHealPotion ) ),
			new( typeof( LesserExplosionPotion ) ),
			new( typeof( MortarPestle ) )
		});

	private static readonly FillableContent Armorer = new(
		2,
		new[]
		{
			typeof( Armorer )
		},
		new FillableEntry[]
		{
			new( 2, typeof( ChainCoif ) ),
			new( 1, typeof( PlateGorget ) ),
			new( 1, typeof( BronzeShield ) ),
			new( 1, typeof( Buckler ) ),
			new( 2, typeof( MetalKiteShield ) ),
			new( 2, typeof( HeaterShield ) ),
			new( 1, typeof( WoodenShield ) ),
			new( 1, typeof( MetalShield ) )
		});

	private static readonly FillableContent ArtisanGuild = new(
		1,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 1, typeof( PaintsAndBrush ) ),
			new( 1, typeof( SledgeHammer ) ),
			new( 2, typeof( SmithHammer ) ),
			new( 2, typeof( Tongs ) ),
			new( 4, typeof( Lockpick ) ),
			new( 4, typeof( TinkerTools ) ),
			new( 1, typeof( MalletAndChisel ) ),
			new( 1, typeof( StatueEast2 ) ),
			new( 1, typeof( StatueSouth ) ),
			new( 1, typeof( StatueSouthEast ) ),
			new( 1, typeof( StatueWest ) ),
			new( 1, typeof( StatueNorth ) ),
			new( 1, typeof( StatueEast ) ),
			new( 1, typeof( BustEast ) ),
			new( 1, typeof( BustSouth ) ),
			new( 1, typeof( BearMask ) ),
			new( 1, typeof( DeerMask ) ),
			new( 4, typeof( OrcHelm ) ),
			new( 1, typeof( TribalMask ) ),
			new( 1, typeof( HornedTribalMask ) )
		});

	private static readonly FillableContent Baker = new(
		1,
		new[]
		{
			typeof( Baker ),
		},
		new FillableEntry[]
		{
			new( 1, typeof( RollingPin ) ),
			new( 2, typeof( SackFlour ) ),
			new( 2, typeof( BreadLoaf ) ),
			new( 1, typeof( FrenchBread ) )
		});

	private static readonly FillableContent Bard = new(
		1,
		new[]
		{
			typeof( Bard ),
			typeof( BardGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( LapHarp ) ),
			new( 2, typeof( Lute ) ),
			new( 1, typeof( Drums ) ),
			new( 1, typeof( Tambourine ) ),
			new( 1, typeof( TambourineTassel ) )
		});

	private static readonly FillableContent Blacksmith = new(
		2,
		new[]
		{
			typeof( Blacksmith ),
			typeof( BlacksmithGuildmaster )
		},
		new FillableEntry[]
		{
			new( 8, typeof( SmithHammer ) ),
			new( 8, typeof( Tongs ) ),
			new( 8, typeof( SledgeHammer ) ),
			//new FillableEntry( 8, typeof( IronOre ) ),
			new( 8, typeof( IronIngot ) ),
			new( 1, typeof( IronWire ) ),
			new( 1, typeof( SilverWire ) ),
			new( 1, typeof( GoldWire ) ),
			new( 1, typeof( CopperWire ) ),
			new( 1, typeof( HorseShoes ) ),
			new( 1, typeof( ForgedMetal ) )
		});

	private static readonly FillableContent Bowyer = new(
		2,
		new[]
		{
			typeof( Bowyer )
		},
		new FillableEntry[]
		{
			new( 2, typeof( Bow ) ),
			new( 2, typeof( Crossbow ) ),
			new( 1, typeof( Arrow ) )
		});

	private static readonly FillableContent Butcher = new(
		1,
		new[]
		{
			typeof( Butcher ),
		},
		new FillableEntry[]
		{
			new( 2, typeof( Cleaver ) ),
			new( 2, typeof( SlabOfBacon ) ),
			new( 2, typeof( Bacon ) ),
			new( 1, typeof( RawFishSteak ) ),
			new( 1, typeof( FishSteak ) ),
			new( 2, typeof( CookedBird ) ),
			new( 2, typeof( RawBird ) ),
			new( 2, typeof( Ham ) ),
			new( 1, typeof( RawLambLeg ) ),
			new( 1, typeof( LambLeg ) ),
			new( 1, typeof( Ribs ) ),
			new( 1, typeof( RawRibs ) ),
			new( 2, typeof( Sausage ) ),
			new( 1, typeof( RawChickenLeg ) ),
			new( 1, typeof( ChickenLeg ) )
		});

	private static readonly FillableContent Carpenter = new(
		1,
		new[]
		{
			typeof( Carpenter ),
			typeof( Architect ),
			typeof( RealEstateBroker )
		},
		new FillableEntry[]
		{
			new( 1, typeof( ChiselsNorth ) ),
			new( 1, typeof( ChiselsWest ) ),
			new( 2, typeof( DovetailSaw ) ),
			new( 2, typeof( Hammer ) ),
			new( 2, typeof( MouldingPlane ) ),
			new( 2, typeof( Nails ) ),
			new( 2, typeof( JointingPlane ) ),
			new( 2, typeof( SmoothingPlane ) ),
			new( 2, typeof( Saw ) ),
			new( 2, typeof( DrawKnife ) ),
			new( 1, typeof( Log ) ),
			new( 1, typeof( Froe ) ),
			new( 1, typeof( Inshave ) ),
			new( 1, typeof( Scorp ) )
		});

	private static readonly FillableContent Clothier = new(
		1,
		new[]
		{
			typeof( Tailor ),
			typeof( Weaver ),
			typeof( TailorGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Cotton ) ),
			new( 1, typeof( Wool ) ),
			new( 1, typeof( DarkYarn ) ),
			new( 1, typeof( LightYarn ) ),
			new( 1, typeof( LightYarnUnraveled ) ),
			new( 1, typeof( SpoolOfThread ) ),
			// Four different types
			//new FillableEntry( 1, typeof( FoldedCloth ) ),
			//new FillableEntry( 1, typeof( FoldedCloth ) ),
			//new FillableEntry( 1, typeof( FoldedCloth ) ),
			//new FillableEntry( 1, typeof( FoldedCloth ) ),
			new( 1, typeof( Dyes ) ),
			new( 2, typeof( Leather ) )
		});

	private static readonly FillableContent Cobbler = new(
		1,
		new[]
		{
			typeof( Cobbler )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Boots ) ),
			new( 2, typeof( Shoes ) ),
			new( 2, typeof( Sandals ) ),
			new( 1, typeof( ThighBoots ) )
		});

	private static readonly FillableContent Docks = new(
		1,
		new[]
		{
			typeof( Fisherman ),
			typeof( FisherGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( FishingPole ) ),
			// Two different types
			//new FillableEntry( 1, typeof( SmallFish ) ),
			//new FillableEntry( 1, typeof( SmallFish ) ),
			new( 4, typeof( Fish ) )
		});

	private static readonly FillableContent Farm = new(
		1,
		new[]
		{
			typeof( Farmer ),
			typeof( Rancher )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Shirt ) ),
			new( 1, typeof( ShortPants ) ),
			new( 1, typeof( Skirt ) ),
			new( 1, typeof( PlainDress ) ),
			new( 1, typeof( Cap ) ),
			new( 2, typeof( Sandals ) ),
			new( 2, typeof( GnarledStaff ) ),
			new( 2, typeof( Pitchfork ) ),
			new( 1, typeof( Bag ) ),
			new( 1, typeof( Kindling ) ),
			new( 1, typeof( Lettuce ) ),
			new( 1, typeof( Onion ) ),
			new( 1, typeof( Turnip ) ),
			new( 1, typeof( Ham ) ),
			new( 1, typeof( Bacon ) ),
			new( 1, typeof( RawLambLeg ) ),
			new( 1, typeof( SheafOfHay ) ),
			new FillableBvrge( 1, typeof( Pitcher ), BeverageType.Milk )
		});

	private static readonly FillableContent FighterGuild = new(
		3,
		new[]
		{
			typeof( WarriorGuildmaster )
		},
		new FillableEntry[]
		{
			new( 12, Loot.ArmorTypes ),
			new(  8, Loot.WeaponTypes ),
			new(  3, Loot.ShieldTypes ),
			new(  1, typeof( Arrow ) )
		});

	private static readonly FillableContent Guard = new(
		3,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 12, Loot.ArmorTypes ),
			new(  8, Loot.WeaponTypes ),
			new(  3, Loot.ShieldTypes ),
			new(  1, typeof( Arrow ) )
		});

	private static readonly FillableContent Healer = new(
		1,
		new[]
		{
			typeof( Healer ),
			typeof( HealerGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Bandage ) ),
			new( 1, typeof( MortarPestle ) ),
			new( 1, typeof( LesserHealPotion ) )
		});

	private static readonly FillableContent Herbalist = new(
		1,
		new[]
		{
			typeof( Herbalist )
		},
		new FillableEntry[]
		{
			new( 10, typeof( Garlic ) ),
			new( 10, typeof( Ginseng ) ),
			new( 10, typeof( MandrakeRoot ) ),
			new(  1, typeof( DeadWood ) ),
			new(  1, typeof( WhiteDriedFlowers ) ),
			new(  1, typeof( GreenDriedFlowers ) ),
			new(  1, typeof( DriedOnions ) ),
			new(  1, typeof( DriedHerbs ) )
		});

	private static readonly FillableContent Inn = new(
		1,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 1, typeof( Candle ) ),
			new( 1, typeof( Torch ) ),
			new( 1, typeof( Lantern ) )
		});

	private static readonly FillableContent Jeweler = new(
		2,
		new[]
		{
			typeof( Jeweler )
		},
		new FillableEntry[]
		{
			new( 1, typeof( GoldRing ) ),
			new( 1, typeof( GoldBracelet ) ),
			new( 1, typeof( GoldEarrings ) ),
			new( 1, typeof( GoldNecklace ) ),
			new( 1, typeof( GoldBeadNecklace ) ),
			new( 1, typeof( Necklace ) ),
			new( 1, typeof( Beads ) ),
			new( 9, Loot.GemTypes )
		});

	public static readonly FillableContent Library = new(
		1,
		new[]
		{
			typeof( Scribe )
		},
		new FillableEntry[]
		{
			new( 8, Loot.LibraryBookTypes ),
			new( 1, typeof( RedBook ) ),
			new( 1, typeof( BlueBook ) )
		});

	private static readonly FillableContent Mage = new(
		2,
		new[]
		{
			typeof( Mage ),
			typeof( HolyMage ),
			typeof( MageGuildmaster )
		},
		new FillableEntry[]
		{
			new( 16, typeof( BlankScroll ) ),
			new( 14, typeof( Spellbook ) ),
			new( 12, Loot.RegularScrollTypes,  0, 8 ),
			new( 11, Loot.RegularScrollTypes,  8, 8 ),
			new( 10, Loot.RegularScrollTypes, 16, 8 ),
			new(  9, Loot.RegularScrollTypes, 24, 8 ),
			new(  8, Loot.RegularScrollTypes, 32, 8 ),
			new(  7, Loot.RegularScrollTypes, 40, 8 ),
			new(  6, Loot.RegularScrollTypes, 48, 8 ),
			new(  5, Loot.RegularScrollTypes, 56, 8 )
		});

	private static readonly FillableContent Merchant = new(
		1,
		new[]
		{
			typeof( MerchantGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( CheeseWheel ) ),
			new( 1, typeof( CheeseWedge ) ),
			new( 1, typeof( CheeseSlice ) ),
			new( 1, typeof( Eggs ) ),
			new( 4, typeof( Fish ) ),
			new( 2, typeof( RawFishSteak ) ),
			new( 2, typeof( FishSteak ) ),
			new( 1, typeof( Apple ) ),
			new( 2, typeof( Banana ) ),
			new( 2, typeof( Bananas ) ),
			new( 2, typeof( OpenCoconut ) ),
			new( 1, typeof( SplitCoconut ) ),
			new( 1, typeof( Coconut ) ),
			new( 1, typeof( Dates ) ),
			new( 1, typeof( Grapes ) ),
			new( 1, typeof( Lemon ) ),
			new( 1, typeof( Lemons ) ),
			new( 1, typeof( Lime ) ),
			new( 1, typeof( Limes ) ),
			new( 1, typeof( Peach ) ),
			new( 1, typeof( Pear ) ),
			new( 2, typeof( SlabOfBacon ) ),
			new( 2, typeof( Bacon ) ),
			new( 2, typeof( CookedBird ) ),
			new( 2, typeof( RawBird ) ),
			new( 2, typeof( Ham ) ),
			new( 1, typeof( RawLambLeg ) ),
			new( 1, typeof( LambLeg ) ),
			new( 1, typeof( Ribs ) ),
			new( 1, typeof( RawRibs ) ),
			new( 2, typeof( Sausage ) ),
			new( 1, typeof( RawChickenLeg ) ),
			new( 1, typeof( ChickenLeg ) ),
			new( 1, typeof( Watermelon ) ),
			new( 1, typeof( SmallWatermelon ) ),
			new( 3, typeof( Turnip ) ),
			new( 2, typeof( YellowGourd ) ),
			new( 2, typeof( GreenGourd ) ),
			new( 2, typeof( Pumpkin ) ),
			new( 1, typeof( SmallPumpkin ) ),
			new( 2, typeof( Onion ) ),
			new( 2, typeof( Lettuce ) ),
			new( 2, typeof( Squash ) ),
			new( 2, typeof( HoneydewMelon ) ),
			new( 1, typeof( Carrot ) ),
			new( 2, typeof( Cantaloupe ) ),
			new( 2, typeof( Cabbage ) ),
			new( 4, typeof( EarOfCorn ) )
		});

	private static readonly FillableContent Mill = new(
		1,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 1, typeof( SackFlour ) )
		});

	private static readonly FillableContent Mine = new(
		1,
		new[]
		{
			typeof( Miner )
		},
		new FillableEntry[]
		{
			new( 2, typeof( Pickaxe ) ),
			new( 2, typeof( Shovel ) ),
			new( 2, typeof( IronIngot ) ),
			//new FillableEntry( 2, typeof( IronOre ) ),
			new( 1, typeof( ForgedMetal ) )
		});

	private static readonly FillableContent Observatory = new(
		1,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 2, typeof( Sextant ) ),
			new( 2, typeof( Clock ) ),
			new( 1, typeof( Spyglass ) )
		});

	private static readonly FillableContent Painter = new(
		1,
		Array.Empty<Type>(),
		new FillableEntry[]
		{
			new( 1, typeof( PaintsAndBrush ) ),
			new( 2, typeof( PenAndInk ) )
		});

	private static readonly FillableContent Provisioner = new(
		1,
		new[]
		{
			typeof( Provisioner )
		},
		new FillableEntry[]
		{
			new( 1, typeof( CheeseWheel ) ),
			new( 1, typeof( CheeseWedge ) ),
			new( 1, typeof( CheeseSlice ) ),
			new( 1, typeof( Eggs ) ),
			new( 4, typeof( Fish ) ),
			new( 1, typeof( DirtyFrypan ) ),
			new( 1, typeof( DirtyPan ) ),
			new( 1, typeof( DirtyKettle ) ),
			new( 1, typeof( DirtySmallRoundPot ) ),
			new( 1, typeof( DirtyRoundPot ) ),
			new( 1, typeof( DirtySmallPot ) ),
			new( 1, typeof( DirtyPot ) ),
			new( 1, typeof( Apple ) ),
			new( 2, typeof( Banana ) ),
			new( 2, typeof( Bananas ) ),
			new( 2, typeof( OpenCoconut ) ),
			new( 1, typeof( SplitCoconut ) ),
			new( 1, typeof( Coconut ) ),
			new( 1, typeof( Dates ) ),
			new( 1, typeof( Grapes ) ),
			new( 1, typeof( Lemon ) ),
			new( 1, typeof( Lemons ) ),
			new( 1, typeof( Lime ) ),
			new( 1, typeof( Limes ) ),
			new( 1, typeof( Peach ) ),
			new( 1, typeof( Pear ) ),
			new( 2, typeof( SlabOfBacon ) ),
			new( 2, typeof( Bacon ) ),
			new( 1, typeof( RawFishSteak ) ),
			new( 1, typeof( FishSteak ) ),
			new( 2, typeof( CookedBird ) ),
			new( 2, typeof( RawBird ) ),
			new( 2, typeof( Ham ) ),
			new( 1, typeof( RawLambLeg ) ),
			new( 1, typeof( LambLeg ) ),
			new( 1, typeof( Ribs ) ),
			new( 1, typeof( RawRibs ) ),
			new( 2, typeof( Sausage ) ),
			new( 1, typeof( RawChickenLeg ) ),
			new( 1, typeof( ChickenLeg ) ),
			new( 1, typeof( Watermelon ) ),
			new( 1, typeof( SmallWatermelon ) ),
			new( 3, typeof( Turnip ) ),
			new( 2, typeof( YellowGourd ) ),
			new( 2, typeof( GreenGourd ) ),
			new( 2, typeof( Pumpkin ) ),
			new( 1, typeof( SmallPumpkin ) ),
			new( 2, typeof( Onion ) ),
			new( 2, typeof( Lettuce ) ),
			new( 2, typeof( Squash ) ),
			new( 2, typeof( HoneydewMelon ) ),
			new( 1, typeof( Carrot ) ),
			new( 2, typeof( Cantaloupe ) ),
			new( 2, typeof( Cabbage ) ),
			new( 4, typeof( EarOfCorn ) )
		});

	private static readonly FillableContent Ranger = new(
		2,
		new[]
		{
			typeof( Ranger ),
			typeof( RangerGuildmaster )
		},
		new FillableEntry[]
		{
			new( 2, typeof( StuddedChest ) ),
			new( 2, typeof( StuddedLegs ) ),
			new( 2, typeof( StuddedArms ) ),
			new( 2, typeof( StuddedGloves ) ),
			new( 1, typeof( StuddedGorget ) ),

			new( 2, typeof( LeatherChest ) ),
			new( 2, typeof( LeatherLegs ) ),
			new( 2, typeof( LeatherArms ) ),
			new( 2, typeof( LeatherGloves ) ),
			new( 1, typeof( LeatherGorget ) ),

			new( 2, typeof( FeatheredHat ) ),
			new( 1, typeof( CloseHelm ) ),
			new( 1, typeof( TallStrawHat ) ),
			new( 1, typeof( Bandana ) ),
			new( 1, typeof( Cloak ) ),
			new( 2, typeof( Boots ) ),
			new( 2, typeof( ThighBoots ) ),

			new( 2, typeof( GnarledStaff ) ),
			new( 1, typeof( Whip ) ),

			new( 2, typeof( Bow ) ),
			new( 2, typeof( Crossbow ) ),
			new( 2, typeof( HeavyCrossbow ) ),
			new( 4, typeof( Arrow ) )
		});

	private static readonly FillableContent Stables = new(
		1,
		new[]
		{
			typeof( AnimalTrainer ),
			typeof( GypsyAnimalTrainer )
		},
		new FillableEntry[]
		{
			//new FillableEntry( 1, typeof( Wheat ) ),
			new( 1, typeof( Carrot ) )
		});

	private static readonly FillableContent Tanner = new(
		2,
		new[]
		{
			typeof( Tanner ),
			typeof( LeatherWorker ),
			typeof( Furtrader )
		},
		new FillableEntry[]
		{
			new( 1, typeof( FeatheredHat ) ),
			new( 1, typeof( LeatherArms ) ),
			new( 2, typeof( LeatherLegs ) ),
			new( 2, typeof( LeatherChest ) ),
			new( 2, typeof( LeatherGloves ) ),
			new( 1, typeof( LeatherGorget ) ),
			new( 2, typeof( Leather ) )
		});

	private static readonly FillableContent Tavern = new(
		1,
		new[]
		{
			typeof( TavernKeeper ),
			typeof( Barkeeper ),
			typeof( Waiter ),
			typeof( Cook )
		},
		new FillableEntry[]
		{
			new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Ale ),
			new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Wine ),
			new FillableBvrge( 1, typeof( BeverageBottle ), BeverageType.Liquor ),
			new FillableBvrge( 1, typeof( Jug ), BeverageType.Cider )
		});

	private static readonly FillableContent ThiefGuild = new(
		1,
		new[]
		{
			typeof( Thief ),
			typeof( ThiefGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Lockpick ) ),
			new( 1, typeof( BearMask ) ),
			new( 1, typeof( DeerMask ) ),
			new( 1, typeof( TribalMask ) ),
			new( 1, typeof( HornedTribalMask ) ),
			new( 4, typeof( OrcHelm ) )
		});

	private static readonly FillableContent Tinker = new(
		1,
		new[]
		{
			typeof( Tinker ),
			typeof( TinkerGuildmaster )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Lockpick ) ),
			//new FillableEntry( 1, typeof( KeyRing ) ),
			new( 2, typeof( Clock ) ),
			new( 2, typeof( ClockParts ) ),
			new( 2, typeof( AxleGears ) ),
			new( 2, typeof( Gears ) ),
			new( 2, typeof( Hinge ) ),
			//new FillableEntry( 1, typeof( ArrowShafts ) ),
			new( 2, typeof( Sextant ) ),
			new( 2, typeof( SextantParts ) ),
			new( 2, typeof( Axle ) ),
			new( 2, typeof( Springs ) ),
			new( 5, typeof( TinkerTools ) ),
			new( 4, typeof( Key ) ),
			new( 1, typeof( DecoArrowShafts )),
			new( 1, typeof( Lockpicks )),
			new( 1, typeof( ToolKit ))
		});

	private static readonly FillableContent Veterinarian = new(
		1,
		new[]
		{
			typeof( Veterinarian )
		},
		new FillableEntry[]
		{
			new( 1, typeof( Bandage ) ),
			new( 1, typeof( MortarPestle ) ),
			new( 1, typeof( LesserHealPotion ) ),
			//new FillableEntry( 1, typeof( Wheat ) ),
			new( 1, typeof( Carrot ) )
		});

	private static readonly FillableContent Weaponsmith = new(
		2,
		new[]
		{
			typeof( Weaponsmith )
		},
		new FillableEntry[]
		{
			new( 8, Loot.WeaponTypes ),
			new( 1, typeof( Arrow ) )
		});

	public static FillableContent Lookup(FillableContentType type)
	{
		int v = (int)type;

		if (v >= 0 && v < m_ContentTypes.Length)
			return m_ContentTypes[v];

		return null;
	}

	public static FillableContentType Lookup(FillableContent content)
	{
		if (content == null)
			return FillableContentType.None;

		return (FillableContentType)Array.IndexOf(m_ContentTypes, content);
	}

	private static Hashtable _acquireTable;

	private static readonly FillableContent[] m_ContentTypes = {
		Weaponsmith,    Provisioner,    Mage,
		Alchemist,      Armorer,        ArtisanGuild,
		Baker,          Bard,           Blacksmith,
		Bowyer,         Butcher,        Carpenter,
		Clothier,       Cobbler,        Docks,
		Farm,           FighterGuild,   Guard,
		Healer,         Herbalist,      Inn,
		Jeweler,        Library,        Merchant,
		Mill,           Mine,           Observatory,
		Painter,        Ranger,         Stables,
		Tanner,         Tavern,         ThiefGuild,
		Tinker,         Veterinarian
	};

	public static FillableContent Acquire(Point3D loc, Map map)
	{
		if (map == null || map == Map.Internal)
			return null;

		if (_acquireTable == null)
		{
			_acquireTable = new Hashtable();

			for (int i = 0; i < m_ContentTypes.Length; ++i)
			{
				FillableContent fill = m_ContentTypes[i];

				for (int j = 0; j < fill.Vendors.Length; ++j)
					_acquireTable[fill.Vendors[j]] = fill;
			}
		}

		Mobile nearest = null;
		FillableContent content = null;

		foreach (Mobile mob in map.GetMobilesInRange(loc, 20))
		{
			if (nearest != null && mob.GetDistanceToSqrt(loc) > nearest.GetDistanceToSqrt(loc) && !(nearest is Cobbler && mob is Provisioner))
				continue;

			if (_acquireTable[mob.GetType()] is FillableContent check)
			{
				nearest = mob;
				content = check;
			}
		}

		return content;
	}
}
