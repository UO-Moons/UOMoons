using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections;

namespace Server.Spells.Fourth;

public class FireFieldSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Fire Field", "In Flam Grav",
		215,
		9041,
		false,
		Reagent.BlackPearl,
		Reagent.SpidersSilk,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Fourth;
	public override bool CanTargetGround => true;

	public FireFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

			Effects.PlaySound(p, Caster.Map, 0x20C);

			int itemId = eastToWest ? 0x398C : 0x3996;

			var duration = Core.AOS ? TimeSpan.FromSeconds((15 + Caster.Skills.Magery.Fixed / 5.0) / 4) : TimeSpan.FromSeconds(4.0 + Caster.Skills[SkillName.Magery].Value * 0.5);

			Point3D pnt = new(p);

			for (var i = 1; i <= 2; ++i)
			{
				Timer.DelayCall(TimeSpan.FromMilliseconds(i * 300), index =>
				{
					Point3D point = new(eastToWest ? pnt.X + index : pnt.X, eastToWest ? pnt.Y : pnt.Y + index, pnt.Z);
					SpellHelper.AdjustField(ref point, Caster.Map, 16, false);
					if (SpellHelper.CheckField(point, Caster.Map))
					{
						new FireFieldItem(itemId, point, Caster, Caster.Map, duration);
					}

					point = new Point3D(eastToWest ? pnt.X + -index : pnt.X, eastToWest ? pnt.Y : pnt.Y + -index, pnt.Z);
					SpellHelper.AdjustField(ref point, Caster.Map, 16, false);

					if (SpellHelper.CheckField(point, Caster.Map))
					{
						new FireFieldItem(itemId, point, Caster, Caster.Map, duration);
					}
				}, i);
			}
		}

		FinishSequence();
	}

	[DispellableAttributes]
	public class FireFieldItem : BaseItem
	{
		private Timer _timer;
		private DateTime _end;
		private int _damage;

		public Mobile Caster { get; private set; }

		public override bool BlocksFit => true;

		public FireFieldItem(int itemId, Point3D loc, Mobile caster, Map map, TimeSpan duration)
			: this(itemId, loc, caster, map, duration, 2)
		{
		}

		private FireFieldItem(int itemId, Point3D loc, Mobile caster, Map map, TimeSpan duration, int damage)
			: base(itemId)
		{
			bool canFit = SpellHelper.AdjustField(ref loc, map, 12, false);


			Movable = false;
			Light = LightType.Circle300;

			MoveToWorld(loc, map);
			Effects.SendLocationParticles(EffectItem.Create(loc, map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5029);

			Caster = caster;

			_damage = damage;

			_end = DateTime.UtcNow + duration;

			_timer = new InternalTimer(this, caster.InLOS(this), canFit);
			_timer.Start();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			_timer?.Stop();
		}

		public FireFieldItem(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(_damage);
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
					_damage = reader.ReadInt();
					Caster = reader.ReadMobile();

					_end = reader.ReadDeltaTime();

					_timer = new InternalTimer(this, true, true);
					_timer.Start();

					break;
				}
			}

			if (version < 2)
				_damage = 2;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Visible && Caster != null && (!Core.AOS || m != Caster) && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
			{
				if (SpellHelper.CanRevealCaster(m))
					Caster.RevealingAction();

				Caster.DoHarmful(m);

				int damage = _damage;

				if (!Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
				{
					damage = 1;

					m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
				}

				AOS.Damage(m, Caster, damage, 0, 100, 0, 0, 0);
				m.PlaySound(0x208);

				if (m is BaseMobile bm)
					bm.OnHarmfulSpell(Caster);
			}

			return true;
		}

		private class InternalTimer : Timer
		{
			private static readonly Queue m_Queue = new();
			private readonly FireFieldItem _item;
			private readonly bool _inLos;
			private readonly bool _canFit;

			public InternalTimer(FireFieldItem item, bool inLos, bool canFit)
				: base(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1.0))
			{
				_item = item;
				_inLos = inLos;
				_canFit = canFit;

				Priority = TimerPriority.FiftyMs;
			}

			protected override void OnTick()
			{
				if (_item.Deleted)
				{
					return;
				}

				if (DateTime.UtcNow > _item._end)
				{
					_item.Delete();
					Stop();
				}
				else
				{
					Map map = _item.Map;
					Mobile caster = _item.Caster;

					if (map != null && caster != null)
					{
						IPooledEnumerable eable = _item.GetMobilesInRange(0);

						foreach (Mobile m in eable)
						{
							if (m.Z + 16 > _item.Z && _item.Z + 12 > m.Z && (!Core.AOS || m != caster) && SpellHelper.ValidIndirectTarget(caster, m) && caster.CanBeHarmful(m, false))
							{
								m_Queue.Enqueue(m);
							}
						}

						eable.Free();

						while (m_Queue.Count > 0)
						{
							Mobile m = (Mobile)m_Queue.Dequeue();

							if (SpellHelper.CanRevealCaster(m))
							{
								caster.RevealingAction();
							}

							caster.DoHarmful(m);

							int damage = _item._damage;

							if (m != null && !Core.AOS && m.CheckSkill(SkillName.MagicResist, 0.0, 30.0))
							{
								damage = 1;

								m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
							}

							AOS.Damage(m, caster, damage, 0, 100, 0, 0, 0);
							if (m == null) continue;
							m.PlaySound(0x208);

							if (m is BaseCreature creature)
							{
								creature.OnHarmfulSpell(caster);
							}
						}
					}
				}
			}
		}
	}

	public class InternalTarget : Target
	{
		private readonly FireFieldSpell _owner;

		public InternalTarget(FireFieldSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
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
