using Server.Network;
using System;

namespace Server.Targeting;

public abstract class Target
{
	public static bool TargetIdValidation { get; set; } = true;

	public DateTime TimeoutTime { get; private set; }

	protected Target(int range, bool allowGround, TargetFlags flags)
	{
		TargetId = ++NextTargetId;
		Range = range;
		AllowGround = allowGround;
		Flags = flags;

		CheckLos = true;
	}

	public static void Cancel(Mobile m)
	{
		NetState ns = m.NetState;

		ns?.Send(CancelTarget.Instance);

		Target targ = m.Target;

		targ?.OnTargetCancel(m, TargetCancelType.Canceled);
	}

	private Timer _timeoutTimer;

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
		CancelTimeout();
		from.ClearTarget();

		Cancel(from);

		OnTargetCancel(from, TargetCancelType.Timeout);
		OnTargetFinish(from);
	}

	private class TimeoutTimer : Timer
	{
		private static readonly TimeSpan ThirtySeconds = TimeSpan.FromSeconds(30.0);
		private static readonly TimeSpan TenSeconds = TimeSpan.FromSeconds(10.0);
		private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1.0);

		public Mobile Mobile { get; }
		public Target Target { get; }

		public TimeoutTimer(Target target, Mobile m, TimeSpan delay) : base(delay)
		{
			Target = target;
			Mobile = m;

			if (delay >= ThirtySeconds)
				Priority = TimerPriority.FiveSeconds;
			else if (delay >= TenSeconds)
				Priority = TimerPriority.OneSecond;
			else if (delay >= OneSecond)
				Priority = TimerPriority.TwoFiftyMs;
			else
				Priority = TimerPriority.TwentyFiveMs;
		}

		protected override void OnTick()
		{
			if (Mobile.Target == Target)
				Target.Timeout(Mobile);
		}
	}

	public bool CheckLos { get; set; }

	public bool DisallowMultis { get; set; }

	public bool AllowNonlocal { get; set; }

	public int TargetId { get; }

	public virtual Packet GetPacketFor(NetState ns)
	{
		return new TargetReq(this);
	}

	public void Cancel(Mobile from, TargetCancelType type)
	{
		CancelTimeout();
		from.ClearTarget();

		OnTargetCancel(from, type);
		OnTargetFinish(from);
	}

	public void Invoke(Mobile from, object targeted)
	{
		CancelTimeout();
		from.ClearTarget();

		if (from.Deleted)
		{
			OnTargetCancel(from, TargetCancelType.Canceled);
			OnTargetFinish(from);
			return;
		}

		Point3D loc;
		Map map;

		switch (targeted)
		{
			case LandTarget target:
				loc = target.Location;
				map = from.Map;
				break;
			case StaticTarget staticTarget:
				loc = staticTarget.Location;
				map = from.Map;
				break;
			case Mobile {Deleted: true}:
				OnTargetDeleted(from, targeted);
				OnTargetFinish(from);
				return;
			case Mobile {CanTarget: false}:
				OnTargetUntargetable(from, targeted);
				OnTargetFinish(from);
				return;
			case Mobile mobileTarget:
				loc = mobileTarget.Location;
				map = mobileTarget.Map;
				break;
			case Item {Deleted: true}:
				OnTargetDeleted(from, targeted);
				OnTargetFinish(from);
				return;
			case Item {CanTarget: false}:
				OnTargetUntargetable(from, targeted);
				OnTargetFinish(from);
				return;
			case Item item:
			{
				object root = item.RootParent;

				if (!AllowNonlocal && root is Mobile && root != from && from.AccessLevel == AccessLevel.Player)
				{
					OnNonlocalTarget(from, targeted);
					OnTargetFinish(from);
					return;
				}

				loc = item.GetWorldLocation();
				map = item.Map;
				break;
			}
			default:
				OnTargetCancel(from, TargetCancelType.Canceled);
				OnTargetFinish(from);
				return;
		}

		if (map == null || map != from.Map || (Range != -1 && !from.InRange(loc, Range)))
		{
			OnTargetOutOfRange(from, targeted);
		}
		else
		{
			if (!from.CanSee(targeted))
				OnCantSeeTarget(from, targeted);
			else if (CheckLos && !from.InLOS(targeted))
				OnTargetOutOfLOS(from, targeted);
			else switch (targeted)
			{
				case Item {InSecureTrade: true}:
					OnTargetInSecureTrade(from, targeted);
					break;
				case Item item1 when !item1.IsAccessibleTo(from):
					OnTargetNotAccessible(from, targeted);
					break;
				case Item item2 when !item2.CheckTarget(from, this, targeted):
				case Mobile mobile when !mobile.CheckTarget(from, this, targeted):
					OnTargetUntargetable(from, targeted);
					break;
				default:
				{
					if (from.Region.OnTarget(from, this, targeted))
						OnTarget(from, targeted);
					break;
				}
			}
		}

		OnTargetFinish(from);
	}

	protected virtual void OnTarget(Mobile from, object targeted)
	{
	}

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
	{
	}

	protected virtual void OnTargetUntargetable(Mobile from, object targeted)
	{
		from.SendLocalizedMessage(500447); // That is not accessible.
	}

	protected virtual void OnTargetCancel(Mobile from, TargetCancelType cancelType)
	{
	}

	protected virtual void OnTargetFinish(Mobile from)
	{
	}

	public int Range { get; set; }

	public bool AllowGround { get; set; }

	public TargetFlags Flags { get; set; }

	public static int NextTargetId { get; set; }
}
