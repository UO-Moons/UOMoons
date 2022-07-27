using Server.Commands;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public abstract class BaseDoor : BaseItem, ILockable, ITelekinesisable
{
	private bool m_Open;
	private Point3D m_Offset;
	private BaseDoor m_Link;
	private Timer m_Timer;

	private static readonly Point3D[] m_Offsets = {
		new(-1, 1, 0 ),
		new( 1, 1, 0 ),
		new(-1, 0, 0 ),
		new( 1,-1, 0 ),
		new( 1, 1, 0 ),
		new( 1,-1, 0 ),
		new( 0, 0, 0 ),
		new( 0,-1, 0 ),

		new( 0, 0, 0 ),
		new( 0, 0, 0 ),
		new( 0, 0, 0 ),
		new( 0, 0, 0 )
	};

	public static void Initialize()
	{
		EventSink.OnOpenDoorMacroUsed += EventSink_OpenDoorMacroUsed;

		CommandSystem.Register("Link", AccessLevel.GameMaster, Link_OnCommand);
		CommandSystem.Register("ChainLink", AccessLevel.GameMaster, ChainLink_OnCommand);
	}

	[Usage("Link")]
	[Description("Links two targeted doors together.")]
	private static void Link_OnCommand(CommandEventArgs e)
	{
		e.Mobile.BeginTarget(-1, false, TargetFlags.None, Link_OnFirstTarget);
		e.Mobile.SendMessage("Target the first door to link.");
	}

	private static void Link_OnFirstTarget(Mobile from, object targeted)
	{
		if (targeted is not BaseDoor door)
		{
			from.BeginTarget(-1, false, TargetFlags.None, Link_OnFirstTarget);
			from.SendMessage("That is not a door. Try again.");
		}
		else
		{
			from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(Link_OnSecondTarget), door);
			from.SendMessage("Target the second door to link.");
		}
	}

	private static void Link_OnSecondTarget(Mobile from, object targeted, object state)
	{
		BaseDoor first = (BaseDoor)state;

		if (targeted is not BaseDoor second)
		{
			from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(Link_OnSecondTarget), first);
			from.SendMessage("That is not a door. Try again.");
		}
		else
		{
			first.Link = second;
			second.Link = first;
			from.SendMessage("The doors have been linked.");
		}
	}

	[Usage("ChainLink")]
	[Description("Chain-links two or more targeted doors together.")]
	private static void ChainLink_OnCommand(CommandEventArgs e)
	{
		e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLink_OnTarget), new List<BaseDoor>());
		e.Mobile.SendMessage("Target the first of a sequence of doors to link.");
	}

	private static void ChainLink_OnTarget(Mobile from, object targeted, object state)
	{
		if (targeted is not BaseDoor door)
		{
			from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLink_OnTarget), state);
			from.SendMessage("That is not a door. Try again.");
		}
		else
		{
			List<BaseDoor> list = (List<BaseDoor>)state;

			if (list.Count > 0 && list[0] == door)
			{
				if (list.Count >= 2)
				{
					for (int i = 0; i < list.Count; ++i)
						list[i].Link = list[(i + 1) % list.Count];

					from.SendMessage("The chain of doors have been linked.");
				}
				else
				{
					from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLink_OnTarget), state);
					from.SendMessage("You have not yet targeted two unique doors. Target the second door to link.");
				}
			}
			else if (list.Contains(door))
			{
				from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLink_OnTarget), state);
				from.SendMessage("You have already targeted that door. Target another door, or retarget the first door to complete the chain.");
			}
			else
			{
				list.Add(door);

				from.BeginTarget(-1, false, TargetFlags.None, new TargetStateCallback(ChainLink_OnTarget), state);

				from.SendMessage(list.Count == 1
					? "Target the second door to link."
					: "Target another door to link. To complete the chain, retarget the first door.");
			}
		}
	}

	private static void EventSink_OpenDoorMacroUsed(Mobile m)
	{
		if (m.Map == null)
			return;

		int x = m.X, y = m.Y;

		switch (m.Direction & Direction.Mask)
		{
			case Direction.North: --y; break;
			case Direction.Right: ++x; --y; break;
			case Direction.East: ++x; break;
			case Direction.Down: ++x; ++y; break;
			case Direction.South: ++y; break;
			case Direction.Left: --x; ++y; break;
			case Direction.West: --x; break;
			case Direction.Up: --x; --y; break;
		}

		Sector sector = m.Map.GetSector(x, y);

		foreach (var item in sector.Items.Where(item => item.Location.X == x && item.Location.Y == y && item.Z + item.ItemData.Height > m.Z && m.Z + 16 > item.Z && item is BaseDoor && m.CanSee(item) && m.InLOS(item)))
		{
			if (m.CheckAlive())
			{
				m.SendLocalizedMessage(500024); // Opening door...
				item.OnDoubleClick(m);
			}

			break;
		}
	}

	public static Point3D GetOffset(DoorFacing facing)
	{
		return m_Offsets[(int)facing];
	}

	private class InternalTimer : Timer
	{
		private readonly BaseDoor m_Door;

		public InternalTimer(BaseDoor door) : base(TimeSpan.FromSeconds(20.0), TimeSpan.FromSeconds(10.0))
		{
			Priority = TimerPriority.OneSecond;
			m_Door = door;
		}

		protected override void OnTick()
		{
			if (m_Door.Open && m_Door.IsFreeToClose())
				m_Door.Open = false;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Locked { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public uint KeyValue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Open
	{
		get => m_Open;
		set
		{
			if (m_Open == value)
				return;

			m_Open = value;

			ItemId = m_Open ? OpenedId : ClosedId;

			Location = m_Open ? new Point3D(X + m_Offset.X, Y + m_Offset.Y, Z + m_Offset.Z) : new Point3D(X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z);

			Effects.PlaySound(this, Map, m_Open ? OpenedSound : ClosedSound);

			if (m_Open)
				m_Timer.Start();
			else
				m_Timer.Stop();
		}
	}

	private bool CanClose()
	{
		if (!m_Open)
			return true;

		Map map = Map;

		if (map == null)
			return false;

		Point3D p = new(X - m_Offset.X, Y - m_Offset.Y, Z - m_Offset.Z);

		return CheckFit(map, p, 16);
	}

	private static bool CheckFit(Map map, Point3D p, int height)
	{
		if (map == Map.Internal)
			return false;

		int x = p.X;
		int y = p.Y;
		int z = p.Z;

		Sector sector = map.GetSector(x, y);
		List<Item> items = sector.Items;
		List<Mobile> mobs = sector.Mobiles;

		if ((from item in items where item is not BaseMulti && item.ItemId <= TileData.MaxItemValue && item.AtWorldPoint(x, y) && item is not BaseDoor let id = item.ItemData let surface = id.Surface let impassable = id.Impassable where (surface || impassable) && item.Z + id.CalcHeight > z && z + height > item.Z select item).Any())
		{
			return false;
		}

		return (from m in mobs where m.Location.X == x && m.Location.Y == y where !m.Hidden || m.AccessLevel <= AccessLevel.Player where m.Alive select m).All(m => m.Z + 16 <= z || z + height <= m.Z);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int OpenedId { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ClosedId { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int OpenedSound { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ClosedSound { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Offset
	{
		get => m_Offset;
		init => m_Offset = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public BaseDoor Link
	{
		get
		{
			if (m_Link is { Deleted: true })
				m_Link = null;

			return m_Link;
		}
		set => m_Link = value;
	}

	public virtual bool UseChainedFunctionality => false;

	private List<BaseDoor> GetChain()
	{
		List<BaseDoor> list = new();
		BaseDoor c = this;

		do
		{
			list.Add(c);
			c = c.Link;
		} while (c != null && !list.Contains(c));

		return list;
	}

	private bool IsFreeToClose()
	{
		if (!UseChainedFunctionality)
			return CanClose();

		List<BaseDoor> list = GetChain();

		bool freeToClose = true;

		for (int i = 0; freeToClose && i < list.Count; ++i)
			freeToClose = list[i].CanClose();

		return freeToClose;
	}

	public void OnTelekinesis(Mobile from)
	{
		Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
		Effects.PlaySound(Location, Map, 0x1F5);

		Use(from);
	}

	public virtual bool IsInside(Mobile from)
	{
		return false;
	}

	public virtual bool UseLocks()
	{
		return true;
	}

	public virtual void Use(Mobile from)
	{
		if (Locked && !m_Open && UseLocks())
		{
			if (from.AccessLevel >= AccessLevel.GameMaster)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502502); // That is locked, but you open it with your godly powers.
				//from.Send( new MessageLocalized( Serial, ItemID, MessageType.Regular, 0x3B2, 3, 502502, "", "" ) ); // That is locked, but you open it with your godly powers.
			}
			else if (Key.ContainsKey(from.Backpack, KeyValue))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501282); // You quickly unlock, open, and relock the door
			}
			else if (IsInside(from))
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 501280); // That is locked, but is usable from the inside.
			}
			else
			{
				if (Hue == 0x44E && Map == Map.Malas) // doom door into healer room in doom
					SendLocalizedMessageTo(from, 1060014); // Only the dead may pass.
				else
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502503); // That is locked.

				return;
			}
		}

		if (m_Open && !IsFreeToClose())
			return;

		if (m_Open)
			OnClosed();
		else
			OnOpened(from);

		if (UseChainedFunctionality)
		{
			bool open = !m_Open;

			List<BaseDoor> list = GetChain();

			for (int i = 0; i < list.Count; ++i)
				list[i].Open = open;
		}
		else
		{
			Open = !m_Open;

			BaseDoor link = Link;

			if (m_Open && link is { Open: false })
				link.Open = true;
		}
	}

	public virtual void OnOpened(Mobile from)
	{
	}

	public virtual void OnClosed()
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.AccessLevel == AccessLevel.Player && !from.InRange(GetWorldLocation(), 2))
			from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
		else
			Use(from);
	}

	public BaseDoor(int closedId, int openedId, int openedSound, int closedSound, Point3D offset) : base(closedId)
	{
		OpenedId = openedId;
		ClosedId = closedId;
		OpenedSound = openedSound;
		ClosedSound = closedSound;
		m_Offset = offset;

		m_Timer = new InternalTimer(this);

		Movable = false;
	}

	public BaseDoor(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(KeyValue);

		writer.Write(m_Open);
		writer.Write(Locked);
		writer.Write(OpenedId);
		writer.Write(ClosedId);
		writer.Write(OpenedSound);
		writer.Write(ClosedSound);
		writer.Write(m_Offset);
		writer.Write(m_Link);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				KeyValue = reader.ReadUInt();
				m_Open = reader.ReadBool();
				Locked = reader.ReadBool();
				OpenedId = reader.ReadInt();
				ClosedId = reader.ReadInt();
				OpenedSound = reader.ReadInt();
				ClosedSound = reader.ReadInt();
				m_Offset = reader.ReadPoint3D();
				m_Link = reader.ReadItem() as BaseDoor;

				m_Timer = new InternalTimer(this);

				if (m_Open)
					m_Timer.Start();

				break;
			}
		}
	}
}
