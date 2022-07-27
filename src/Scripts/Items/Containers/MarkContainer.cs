using System;

namespace Server.Items;

public class MarkContainer : LockableContainer
{
	private static bool FindMarkContainer(Point3D p, Map map)
	{
		IPooledEnumerable eable = map.GetItemsInRange(p, 0);

		foreach (Item item in eable)
		{
			if (item.Z == p.Z && item is MarkContainer)
			{
				eable.Free();
				return true;
			}
		}

		eable.Free();
		return false;
	}

	public static void CreateMalasPassage(int x, int y, int z, int xTarget, int yTarget, int zTarget, bool bone, bool locked)
	{
		Point3D location = new(x, y, z);

		if (FindMarkContainer(location, Map.Malas))
			return;

		MarkContainer cont = new(bone, locked)
		{
			TargetMap = Map.Malas,
			Target = new Point3D(xTarget, yTarget, zTarget),
			Description = "strange location"
		};

		cont.MoveToWorld(location, Map.Malas);
	}

	private bool m_AutoLock;
	private InternalTimer m_RelockTimer;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AutoLock
	{
		get => m_AutoLock;
		set
		{
			m_AutoLock = value;

			if (!m_AutoLock)
				StopTimer();
			else if (!Locked && m_RelockTimer == null)
				m_RelockTimer = new InternalTimer(this);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Map TargetMap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Target { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Bone
	{
		get => ItemId == 0xECA;
		set
		{
			ItemId = value ? 0xECA : 0xE79;
			Hue = value ? 1102 : 0;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string Description { get; set; }

	public override bool IsDecoContainer => false;

	[Constructable]
	public MarkContainer() : this(false)
	{
	}

	[Constructable]
	public MarkContainer(bool bone, bool locked = false) : base(bone ? 0xECA : 0xE79)
	{
		Movable = false;

		if (bone)
			Hue = 1102;

		m_AutoLock = locked;
		Locked = locked;

		if (locked)
			LockLevel = -255;
	}

	public MarkContainer(Serial serial) : base(serial)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public override bool Locked
	{
		get => base.Locked;
		set
		{
			base.Locked = value;

			if (m_AutoLock)
			{
				StopTimer();

				if (!Locked)
					m_RelockTimer = new InternalTimer(this);
			}
		}
	}

	private void StopTimer()
	{
		m_RelockTimer?.Stop();

		m_RelockTimer = null;
	}

	private class InternalTimer : Timer
	{
		private MarkContainer Container { get; }
		public DateTime RelockTime { get; }

		public InternalTimer(MarkContainer container) : this(container, TimeSpan.FromMinutes(5.0))
		{
		}

		public InternalTimer(MarkContainer container, TimeSpan delay) : base(delay)
		{
			Container = container;
			RelockTime = DateTime.UtcNow + delay;

			Start();
		}

		protected override void OnTick()
		{
			Container.Locked = true;
			Container.LockLevel = -255;
		}
	}

	private void Mark(RecallRune rune)
	{
		if (TargetMap == null)
			return;

		rune.Marked = true;
		rune.TargetMap = TargetMap;
		rune.Target = Target;
		rune.Description = Description;
		rune.House = null;
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (dropped is RecallRune rune && base.OnDragDrop(from, dropped))
		{
			Mark(rune);

			return true;
		}

		return false;
	}

	public override bool OnDragDropInto(Mobile from, Item dropped, Point3D p)
	{
		if (dropped is RecallRune rune && base.OnDragDropInto(from, dropped, p))
		{
			Mark(rune);

			return true;
		}

		return false;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(m_AutoLock);

		if (!Locked && m_AutoLock)
			writer.WriteDeltaTime(m_RelockTimer.RelockTime);

		writer.Write(TargetMap);
		writer.Write(Target);
		writer.Write(Description);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		m_AutoLock = reader.ReadBool();

		if (!Locked && m_AutoLock)
			m_RelockTimer = new InternalTimer(this, reader.ReadDeltaTime() - DateTime.UtcNow);

		TargetMap = reader.ReadMap();
		Target = reader.ReadPoint3D();
		Description = reader.ReadString();
	}
}
