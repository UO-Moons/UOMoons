using Server.Network;
using System;
using System.Linq;

namespace Server.Items;

public class RaiseSwitch : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public RaisableItem RaisableItem { get; set; }

	[Constructable]
	public RaiseSwitch() : this(0x1093)
	{
	}

	protected RaiseSwitch(int itemId) : base(itemId)
	{
		Movable = false;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (!m.InRange(this, 2))
		{
			m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
			return;
		}

		if (RaisableItem is { Deleted: true })
			RaisableItem = null;

		Flip();

		if (RaisableItem != null)
		{
			if (RaisableItem.IsRaisable)
			{
				RaisableItem.Raise();
				m.LocalOverheadMessage(MessageType.Regular, 0x5A, true, "You hear a grinding noise echoing in the distance.");
			}
			else
			{
				m.LocalOverheadMessage(MessageType.Regular, 0x5A, true, "You flip the switch again, but nothing happens.");
			}
		}
	}

	protected virtual void Flip()
	{
		if (ItemId != 0x1093)
		{
			ItemId = 0x1093;

			StopResetTimer();
		}
		else
		{
			ItemId = 0x1095;

			if (RaisableItem != null && RaisableItem.CloseDelay >= TimeSpan.Zero)
				StartResetTimer(RaisableItem.CloseDelay);
			else
				StartResetTimer(TimeSpan.FromMinutes(2.0));
		}

		Effects.PlaySound(Location, Map, 0x3E8);
	}

	private ResetTimer m_ResetTimer;

	private void StartResetTimer(TimeSpan delay)
	{
		StopResetTimer();

		m_ResetTimer = new ResetTimer(this, delay);
		m_ResetTimer.Start();
	}

	private void StopResetTimer()
	{
		if (m_ResetTimer != null)
		{
			m_ResetTimer.Stop();
			m_ResetTimer = null;
		}
	}

	protected virtual void Reset()
	{
		if (ItemId != 0x1093)
			Flip();
	}

	private class ResetTimer : Timer
	{
		private readonly RaiseSwitch m_RaiseSwitch;

		public ResetTimer(RaiseSwitch raiseSwitch, TimeSpan delay) : base(delay)
		{
			m_RaiseSwitch = raiseSwitch;

			Priority = ComputePriority(delay);
		}

		protected override void OnTick()
		{
			if (m_RaiseSwitch.Deleted)
				return;

			m_RaiseSwitch.m_ResetTimer = null;

			m_RaiseSwitch.Reset();
		}
	}

	public RaiseSwitch(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version

		writer.Write(RaisableItem);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadEncodedInt();

		RaisableItem = (RaisableItem)reader.ReadItem();

		Reset();
	}
}

public class DisappearingRaiseSwitch : RaiseSwitch
{
	private int CurrentRange => Visible ? 3 : 2;

	[Constructable]
	public DisappearingRaiseSwitch() : base(0x108F)
	{
	}

	protected override void Flip()
	{
	}

	protected override void Reset()
	{
	}

	public override bool HandlesOnMovement => true;

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (Utility.InRange(m.Location, Location, CurrentRange) || Utility.InRange(oldLocation, Location, CurrentRange))
			Refresh();
	}

	public override void OnMapChange()
	{
		if (!Deleted)
			Refresh();
	}

	public override void OnLocationChange(Point3D oldLoc)
	{
		if (!Deleted)
			Refresh();
	}

	private void Refresh()
	{
		bool found = GetMobilesInRange(CurrentRange).Any(mob => !mob.Hidden || mob.AccessLevel <= AccessLevel.Player);

		Visible = found;
	}

	public DisappearingRaiseSwitch(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		if (RaisableItem != null && RaisableItem.Deleted)
			RaisableItem = null;

		base.Serialize(writer);

		writer.WriteEncodedInt(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();

		Timer.DelayCall(TimeSpan.Zero, Refresh);
	}
}