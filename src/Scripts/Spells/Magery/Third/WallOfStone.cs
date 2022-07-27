using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Third;

public class WallOfStoneSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Wall of Stone", "In Sanct Ylem",
		227,
		9011,
		false,
		Reagent.Bloodmoss,
		Reagent.Garlic
	);

	public override SpellCircle Circle => SpellCircle.Third;

	public override bool CanTargetGround => true;

	public WallOfStoneSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override void OnCast()
	{
		if (Precast)
		{
			Caster.Target = new InternalTarget(this);
		}
		else
		{
			if (SpellTarget is IPoint3D target)
				Target(target);
			else
				FinishSequence();
		}
	}

	private void Target(IPoint3D p)
	{
		if (!Caster.CanSee(p))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (SpellHelper.CheckTown(p, Caster) && CheckSequence())
		{
			SpellHelper.Turn(Caster, p);

			SpellHelper.GetSurfaceTop(ref p);

			int dx = Caster.Location.X - p.X;
			int dy = Caster.Location.Y - p.Y;
			int rx = (dx - dy) * 44;
			int ry = (dx + dy) * 44;

			bool eastToWest;

			switch (rx)
			{
				case >= 0 when ry >= 0:
					eastToWest = false;
					break;
				case >= 0:
					eastToWest = true;
					break;
				default:
				{
					eastToWest = ry >= 0;

					break;
				}
			}

			Effects.PlaySound(p, Caster.Map, 0x1F6);

			for (int i = -1; i <= 1; ++i)
			{
				Point3D loc = new(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
				bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 22, true);

				//Effects.SendLocationParticles( EffectItem.Create( loc, Caster.Map, EffectItem.DefaultDuration ), 0x376A, 9, 10, 5025 );

				if (!canFit)
					continue;

				Item item = new InternalItem(loc, Caster.Map, Caster);

				Effects.SendLocationParticles(item, 0x376A, 9, 10, 5025);

				//new InternalItem( loc, Caster.Map, Caster );
			}
		}

		FinishSequence();
	}

	[DispellableAttributes]
	private class InternalItem : BaseItem
	{
		private Timer _timer;
		private DateTime _end;
		private readonly Mobile _caster;

		public override bool BlocksFit => true;

		public InternalItem(Point3D loc, Map map, Mobile caster) : base(0x82)
		{
			Visible = false;
			Movable = false;

			MoveToWorld(loc, map);

			_caster = caster;

			if (caster.InLOS(this))
				Visible = true;
			else
				Delete();

			if (Deleted)
				return;

			_timer = new InternalTimer(this, TimeSpan.FromSeconds(10.0));
			_timer.Start();

			_end = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
		}

		public InternalItem(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.WriteDeltaTime(_end);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
				{
					_end = reader.ReadDeltaTime();

					_timer = new InternalTimer(this, _end - DateTime.UtcNow);
					_timer.Start();

					break;
				}
			}
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m is not PlayerMobile) return base.OnMoveOver(m);
			var noto = Notoriety.Compute(_caster, m);
			return noto is not (Notoriety.Enemy or Notoriety.Ally) && base.OnMoveOver(m);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			_timer?.Stop();
		}

		private class InternalTimer : Timer
		{
			private readonly InternalItem _item;

			public InternalTimer(InternalItem item, TimeSpan duration) : base(duration)
			{
				Priority = TimerPriority.OneSecond;
				_item = item;
			}

			protected override void OnTick()
			{
				_item.Delete();
			}
		}
	}

	private class InternalTarget : Target
	{
		private readonly WallOfStoneSpell _owner;

		public InternalTarget(WallOfStoneSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D d)
				_owner.Target(d);
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
