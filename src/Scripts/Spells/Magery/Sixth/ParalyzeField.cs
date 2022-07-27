using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Sixth;

public class ParalyzeFieldSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Paralyze Field", "In Ex Grav",
		230,
		9012,
		false,
		Reagent.BlackPearl,
		Reagent.Ginseng,
		Reagent.SpidersSilk
	);

	public override SpellCircle Circle => SpellCircle.Sixth;
	public override bool CanTargetGround => true;

	public ParalyzeFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
		else if (SpellHelper.CheckTown(p, Caster) && SpellHelper.CheckWater(new Point3D(p), Caster.Map) && CheckSequence())
		{
			SpellHelper.Turn(Caster, p);

			SpellHelper.GetSurfaceTop(ref p);

			int dx = Caster.Location.X - p.X;
			int dy = Caster.Location.Y - p.Y;
			int rx = (dx - dy) * 44;
			int ry = (dx + dy) * 44;

			bool eastToWest = rx switch
			{
				>= 0 when ry >= 0 => false,
				>= 0 => true,
				_ => ry >= 0
			};

			Effects.PlaySound(p, Caster.Map, 0x20B);
			int itemId = eastToWest ? 0x3967 : 0x3979;

			Point3D pnt = new Point3D(p);
			TimeSpan duration = TimeSpan.FromSeconds(3.0 + (Caster.Skills[SkillName.Magery].Value / 3.0));

			if (SpellHelper.CheckField(pnt, Caster.Map))
			{
				new InternalItem(itemId, pnt, Caster, Caster.Map, duration);
			}

			for (int i = 1; i <= 2; ++i)
			{
				Timer.DelayCall(TimeSpan.FromMilliseconds(i * 300), index =>
				{
					Point3D point = new Point3D(eastToWest ? pnt.X + index : pnt.X, eastToWest ? pnt.Y : pnt.Y + index, pnt.Z);
					SpellHelper.AdjustField(ref point, Caster.Map, 16, false);

					if (SpellHelper.CheckField(point, Caster.Map))
					{
						new InternalItem(itemId, point, Caster, Caster.Map, duration);
					}

					point = new Point3D(eastToWest ? pnt.X + -index : pnt.X, eastToWest ? pnt.Y : pnt.Y + -index, pnt.Z);
					SpellHelper.AdjustField(ref point, Caster.Map, 16, false);

					if (SpellHelper.CheckField(point, Caster.Map))
					{
						new InternalItem(itemId, point, Caster, Caster.Map, duration);
					}
				}, i);
			}
		}

		FinishSequence();
	}

	[DispellableAttributes]
	public class InternalItem : Item
	{
		private Timer _timer;
		private DateTime _end;

		public Mobile Caster { get; private set; }

		public InternalItem(int itemId, Point3D loc, Mobile caster, Map map, TimeSpan duration)
			: base(itemId)
		{
			Movable = false;
			Light = LightType.Circle300;

			MoveToWorld(loc, map);
			Effects.SendLocationParticles(EffectItem.Create(loc, map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5048);

			if (Deleted)
			{
				return;
			}

			Caster = caster;

			_timer = new InternalTimer(this, duration);
			_timer.Start();

			_end = DateTime.UtcNow + duration;
		}

		public InternalItem(Serial serial)
			: base(serial)
		{
		}

		public override bool BlocksFit => true;
		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			_timer?.Stop();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(Caster);
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
					Caster = reader.ReadMobile();
					_end = reader.ReadDeltaTime();

					_timer = new InternalTimer(this, _end - DateTime.UtcNow);
					_timer.Start();

					break;
				}
			}
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Visible && Caster != null && (!Core.AOS || m != Caster) && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
			{
				if (SpellHelper.CanRevealCaster(m))
				{
					Caster.RevealingAction();
				}

				Caster.DoHarmful(m);

				double duration;

				if (Core.AOS)
				{
					duration = 2.0 + ((int)(Caster.Skills[SkillName.EvalInt].Value / 10) - (int)(m.Skills[SkillName.MagicResist].Value / 10));

					if (!m.Player)
					{
						duration *= 3.0;
					}

					if (duration < 0.0)
					{
						duration = 0.0;
					}
				}
				else
				{
					duration = 7.0 + (Caster.Skills[SkillName.Magery].Value * 0.2);
				}

				m.Paralyze(TimeSpan.FromSeconds(duration));

				m.PlaySound(0x204);
				m.FixedEffect(0x376A, 10, 16);

				if (m is BaseCreature creature)
				{
					creature.OnHarmfulSpell(Caster);
				}
			}

			return true;
		}

		private class InternalTimer : Timer
		{
			private readonly Item _item;
			public InternalTimer(Item item, TimeSpan duration)
				: base(duration)
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

	public class InternalTarget : Target
	{
		private readonly ParalyzeFieldSpell _owner;
		public InternalTarget(ParalyzeFieldSpell owner)
			: base(Core.TOL ? 15 : Core.ML ? 10 : 12, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D point3D)
			{
				_owner.Target(point3D);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
