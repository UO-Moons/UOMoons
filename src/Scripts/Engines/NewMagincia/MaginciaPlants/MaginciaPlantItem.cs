using Server.Network;
using System;
using System.Linq;

namespace Server.Engines.Plants;

public class MaginciaPlantItem : PlantItem
{
	public override bool MaginciaPlant => true;
	public override int BowlOfDirtID => 2323;
	public override int GreenBowlID => PlantStatus <= PlantStatus.Stage3 ? 0xC7E : 0xC62;

	public override int ContainerLocalization => 1150436;  // mound of dirt
	public override int OnPlantLocalization => 1150442;  // You plant the seed in the mound of dirt.
	public override int CantUseLocalization => 501648;  // You cannot use this unless you are the owner.

	public override int LabelNumber
	{
		get
		{
			var label = base.LabelNumber;

			if (label == 1029913)
				label = 1022321;    // patch of dirt

			return label;
		}
	}

	private DateTime _planted;
	private DateTime _contract;
	private Timer _timer;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime Planted { get => _planted;
		set { _planted = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime ContractTime { get => _contract;
		set { _contract = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime ContractEndTime => ContractTime + TimeSpan.FromDays(14);

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsContract => ContractEndTime > DateTime.UtcNow;

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime SetToDecorative { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public override bool ValidGrowthLocation => RootParent == null && !Movable;

	[Constructable]
	public MaginciaPlantItem()
		: this(false)
	{
	}

	[Constructable]
	public MaginciaPlantItem(bool fertile)
		: base(2323, fertile)
	{
		Movable = false;

		Planted = DateTime.UtcNow;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (PlantStatus >= PlantStatus.DecorativePlant)
			return;

		Point3D loc = GetWorldLocation();

		if (!from.InLOS(loc) || !from.InRange(loc, 2))
		{
			from.LocalOverheadMessage(MessageType.Regular, 0x3E9, 1019045); // I can't reach that.
			return;
		}

		if (!IsUsableBy(from))
		{
			LabelTo(from, CantUseLocalization);

			return;
		}

		from.SendGump(new MainPlantGump(this));
	}

	public override bool IsUsableBy(Mobile from)
	{
		return RootParent == null && !Movable && Owner == from && IsAccessibleTo(from);
	}

	public override void Die()
	{
		base.Die();

		Timer.DelayCall(TimeSpan.FromMinutes(Utility.RandomMinMax(2, 5)), Delete);
	}

	public override void Delete()
	{
		if (Owner != null && PlantStatus < PlantStatus.DecorativePlant)
			MaginciaPlantSystem.OnPlantDelete(Owner, Map);

		base.Delete();
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (Owner != null)
		{
			list.Add(1150474, $"#1011345\t{Owner.Name}"); // Planted in ~1_val~ by: ~2_val~
			list.Add(1150478, _planted.ToShortDateString());

			if (IsContract)
			{
				DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(ContractEndTime, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
				list.Add(1155763, easternTime.ToString("MM-dd-yyyy HH:mm 'ET'")); // Gardening Contract Expires: ~1_TIME~
			}

			if (PlantStatus == PlantStatus.DecorativePlant)
				list.Add(1150490, SetToDecorative.ToShortDateString()); // Date harvested: ~1_val~
		}
	}

	public void StartTimer()
	{
		_timer = Timer.DelayCall(TimeSpan.FromMinutes(2), Delete);
	}

	public override bool PlantSeed(Mobile from, Seed seed)
	{
		if (!CheckLocation(from, seed) || !base.PlantSeed(from, seed))
			return false;

		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}

		return true;
	}

	private bool CheckLocation(Mobile from, Seed seed)
	{
		if (!BlocksMovement(seed))
			return true;

		IPooledEnumerable eable = Map.GetItemsInRange(Location, 1);

		if (eable.Cast<Item>().Where(item => item != this && item is MaginciaPlantItem).Any(item => ((MaginciaPlantItem)item).BlocksMovement()))
		{
			eable.Free();
			from.SendLocalizedMessage(1150434); // Plants that block movement cannot be planted next to other plants that block movement.
			return false;
		}

		eable.Free();
		return true;
	}

	public bool BlocksMovement()
	{
		if (PlantStatus is PlantStatus.BowlOfDirt or PlantStatus.DeadTwigs)
			return false;

		PlantTypeInfo info = PlantTypeInfo.GetInfo(PlantType);
		ItemData data = TileData.ItemTable[info.ItemID & TileData.MaxItemValue];

		TileFlag flags = data.Flags;

		return (flags & TileFlag.Impassable) > 0;
	}

	public static bool BlocksMovement(Seed seed)
	{
		PlantTypeInfo info = PlantTypeInfo.GetInfo(seed.PlantType);
		ItemData data = TileData.ItemTable[info.ItemID & TileData.MaxItemValue];

		TileFlag flags = data.Flags;

		return (flags & TileFlag.Impassable) > 0;
	}

	public MaginciaPlantItem(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		writer.Write(ContractTime);
		writer.Write(Owner);
		writer.Write(_planted);
		writer.Write(SetToDecorative);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				ContractTime = reader.ReadDateTime();
				Owner = reader.ReadMobile();
				_planted = reader.ReadDateTime();
				SetToDecorative = reader.ReadDateTime();
				break;
			}
		}

		if (PlantStatus == PlantStatus.BowlOfDirt)
			Delete();
	}
}
