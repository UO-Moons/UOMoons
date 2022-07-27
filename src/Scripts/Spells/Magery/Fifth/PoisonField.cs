using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections;

namespace Server.Spells.Fifth;

public class PoisonFieldSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Poison Field", "In Nox Grav",
		230,
		9052,
		false,
		Reagent.BlackPearl,
		Reagent.Nightshade,
		Reagent.SpidersSilk
	);

	public override SpellCircle Circle => SpellCircle.Fifth;
	public override bool CanTargetGround => true;

	public PoisonFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
			int itemId = eastToWest ? 0x3915 : 0x3922;

			Point3D pnt = new(p);
			TimeSpan duration = TimeSpan.FromSeconds(3 + Caster.Skills.Magery.Fixed / 25);
			if (SpellHelper.CheckField(pnt, Caster.Map))
			{
				new InternalItem(itemId, pnt, Caster, Caster.Map, duration);
			}

			for (int i = 1; i <= 2; ++i)
			{
				Timer.DelayCall(TimeSpan.FromMilliseconds(i * 300), index =>
				{
					Point3D point = new(eastToWest ? pnt.X + index : pnt.X, eastToWest ? pnt.Y : pnt.Y + index, pnt.Z);
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
			bool canFit = SpellHelper.AdjustField(ref loc, map, 12, false);

			Movable = false;
			Light = LightType.Circle300;

			MoveToWorld(loc, map);
			Effects.SendLocationParticles(EffectItem.Create(loc, map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5029);

			Caster = caster;

			_end = DateTime.UtcNow + duration;

			_timer = new InternalTimer(this, caster.InLOS(this), canFit);
			_timer.Start();
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
					_timer = new InternalTimer(this, true, true);
					_timer.Start();
					break;
				}
			}
		}

		private void ApplyPoisonTo(Mobile m)
		{
			if (Caster == null)
			{
				return;
			}

			Poison p;

			if (Core.AOS)
			{
				int total = (Caster.Skills.Magery.Fixed + Caster.Skills.Poisoning.Fixed) / 2;

				p = total switch
				{
					>= 1000 => Poison.Deadly,
					> 850 => Poison.Greater,
					> 650 => Poison.Regular,
					_ => Poison.Lesser
				};
			}
			else
			{
				p = Poison.Regular;
			}

			if (m.ApplyPoison(Caster, p) == ApplyPoisonResult.Poisoned)
			{
				if (SpellHelper.CanRevealCaster(m))
				{
					Caster.RevealingAction();
				}
			}

			if (m is BaseCreature creature)
			{
				creature.OnHarmfulSpell(Caster);
			}
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (Visible && Caster != null && (!Core.AOS || m != Caster) && SpellHelper.ValidIndirectTarget(Caster, m) && Caster.CanBeHarmful(m, false))
			{
				Caster.DoHarmful(m);

				ApplyPoisonTo(m);
				m.PlaySound(0x474);
			}

			return true;
		}

		private class InternalTimer : Timer
		{
			private static readonly Queue m_Queue = new();
			private readonly InternalItem _item;
			private readonly bool _inLos;
			private readonly bool _canFit;
			public InternalTimer(InternalItem item, bool inLos, bool canFit)
				: base(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1.5))
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

					if (map == null || caster == null)
						return;

					bool eastToWest = _item.ItemId == 0x3915;
					IPooledEnumerable eable = map.GetMobilesInBounds(new Rectangle2D(_item.X - (eastToWest ? 0 : 1), _item.Y - (eastToWest ? 1 : 0), eastToWest ? 1 : 2, eastToWest ? 2 : 1));

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

						caster.DoHarmful(m);

						_item.ApplyPoisonTo(m);
						m?.PlaySound(0x474);
					}
				}
			}
		}
	}

	public class InternalTarget : Target
	{
		private readonly PoisonFieldSpell _owner;
		public InternalTarget(PoisonFieldSpell owner)
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
