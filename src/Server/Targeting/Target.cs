using Server.Network;
using System;

namespace Server.Targeting;

public abstract class Target
{
	private static int _nextTargetId;

	private Timer _timeoutTimer;

	private bool _finished;

	public int TargetId { get; }

	public bool CheckLos { get; set; }
	public bool DisallowMultis { get; set; }
	public bool AllowNonlocal { get; set; }
	public bool AllowGround { get; set; }

	public int Range { get; set; }

	public TargetFlags Flags { get; set; }

	public DateTime TimeoutTime { get; private set; }

	protected Target(int range, bool allowGround, TargetFlags flags)
	{
		TargetId = ++_nextTargetId;
		Range = range;
		AllowGround = allowGround;
		Flags = flags;

		CheckLos = true;
	}

	public void BeginTimeout(Mobile from, TimeSpan delay)
	{
		TimeoutTime = DateTime.UtcNow + delay;

		_timeoutTimer?.Stop();

		_timeoutTimer = new TimeoutTimer(this, from, delay);
		_timeoutTimer.Start();
	}

	public void CancelTimeout()
	{
		_timeoutTimer?.Stop();

		_timeoutTimer = null;
	}

	public void Timeout(Mobile from)
	{
		Cancel(from, TargetCancelType.Timeout);
	}

	private class TimeoutTimer : Timer
	{
		private readonly Target _target;
		private readonly Mobile _mobile;

		private static readonly TimeSpan ThirtySeconds = TimeSpan.FromSeconds(30.0);
		private static readonly TimeSpan TenSeconds = TimeSpan.FromSeconds(10.0);
		private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1.0);

		public TimeoutTimer(Target target, Mobile m, TimeSpan delay)
			: base(delay)
		{
			_target = target;
			_mobile = m;

			if (delay >= ThirtySeconds)
			{
				Priority = TimerPriority.FiveSeconds;
			}
			else if (delay >= TenSeconds)
			{
				Priority = TimerPriority.OneSecond;
			}
			else if (delay >= OneSecond)
			{
				Priority = TimerPriority.TwoFiftyMs;
			}
			else
			{
				Priority = TimerPriority.TwentyFiveMs;
			}
		}

		protected override void OnTick()
		{
			if (_mobile.Target == _target)
			{
				_target.Timeout(_mobile);
			}
		}
	}

	public virtual Packet GetPacketFor(NetState ns)
	{
		return new TargetReq(this);
	}

	public void Cancel(Mobile from)
	{
		Cancel(from, TargetCancelType.Canceled);
	}

	public void Cancel(Mobile from, TargetCancelType type)
	{
		CancelTimeout();

		from.ClearTarget();

		if (type == TargetCancelType.Canceled || type == TargetCancelType.Timeout)
		{
			from.Send(CancelTarget.Instance);
		}

		OnTargetCancel(from, type);

		Finalize(from);
	}

	public void Invoke(Mobile from, object targeted)
	{
		try
		{
			if (from == null || from.Deleted)
			{
				Cancel(from, TargetCancelType.Invalid);
				return;
			}

			var enhancedClient = from.NetState != null && from.NetState.IsEnhancedClient;

			Point3D loc;
			Map map;

			if (targeted is LandTarget l)
			{
				loc = l.Location;
				map = from.Map;

				if (enhancedClient && loc.X == 0 && loc.Y == 0 && !from.InRange(loc, 10))
				{
					Cancel(from, TargetCancelType.Canceled);
					return;
				}
			}
			else if (targeted is StaticTarget s)
			{
				loc = s.Location;
				map = from.Map;
			}
			else if (targeted is Mobile m)
			{
				if (m.Deleted)
				{
					OnTargetDeleted(from, targeted);
					return;
				}

				if (!m.CanTarget)
				{
					OnTargetUntargetable(from, targeted);
					return;
				}

				loc = m.Location;
				map = m.Map;
			}
			else if (targeted is Item i)
			{
				if (i.Deleted)
				{
					OnTargetDeleted(from, targeted);
					return;
				}

				if (!i.CanTarget)
				{
					OnTargetUntargetable(from, targeted);
					return;
				}

				var root = i.RootParent;

				if (!AllowNonlocal && root is Mobile && root != from && from.AccessLevel == AccessLevel.Player)
				{
					OnNonlocalTarget(from, targeted);
					return;
				}

				loc = i.GetWorldLocation();
				map = i.Map;
			}
			else
			{
				Cancel(from, TargetCancelType.Canceled);
				return;
			}

			if (map == null || map != from.Map || (Range != -1 && !from.InRange(loc, Range)))
			{
				OnTargetOutOfRange(from, targeted);
			}
			else if (!from.CanSee(targeted))
			{
				OnCantSeeTarget(from, targeted);
			}
			else if (CheckLos && !from.InLOS(targeted))
			{
				OnTargetOutOfLOS(from, targeted);
			}
			else if (targeted is Item i1 && i1.InSecureTrade)
			{
				OnTargetInSecureTrade(from, targeted);
			}
			else if (targeted is Item i2 && !i2.IsAccessibleTo(from))
			{
				OnTargetNotAccessible(from, targeted);
			}
			else if (targeted is Item i3 && !i3.CheckTarget(from, this, targeted))
			{
				OnTargetUntargetable(from, targeted);
			}
			else if (targeted is Mobile m1 && !m1.CheckTarget(from, this, targeted))
			{
				OnTargetUntargetable(from, targeted);
			}
			else if (from.Region.OnTarget(from, this, targeted))
			{
				OnTarget(from, targeted);
			}
		}
		finally
		{
			Finalize(from);
		}
	}

	protected virtual void OnTarget(Mobile from, object targeted)
	{ }

	protected virtual void OnTargetNotAccessible(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500447); // That is not accessible.
	}

	protected virtual void OnTargetInSecureTrade(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500447); // That is not accessible.
	}

	protected virtual void OnNonlocalTarget(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500447); // That is not accessible.
	}

	protected virtual void OnCantSeeTarget(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500237); // Target can not be seen.
	}

	protected virtual void OnTargetOutOfLOS(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500237); // Target can not be seen.
	}

	protected virtual void OnTargetOutOfRange(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500446); // That is too far away.
	}

	protected virtual void OnTargetDeleted(Mobile from, object targeted)
	{ }

	protected virtual void OnTargetUntargetable(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500447); // That is not accessible.
	}

	protected virtual void OnTargetCancel(Mobile from, TargetCancelType cancelType)
	{ }

	protected virtual void OnTargetFinish(Mobile from)
	{ }

	private void Finalize(Mobile from)
	{
		if (!_finished)
		{
			_finished = true;

			OnTargetFinish(from);

			if (from.Target == this)
			{
				from.Target = null;
			}
		}
	}
}
