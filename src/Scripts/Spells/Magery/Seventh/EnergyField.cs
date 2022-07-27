using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Spells.Seventh;

public class EnergyFieldSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Energy Field", "In Sanct Grav",
		221,
		9022,
		false,
		Reagent.BlackPearl,
		Reagent.MandrakeRoot,
		Reagent.SpidersSilk,
		Reagent.SulfurousAsh
	);

	public override SpellCircle Circle => SpellCircle.Seventh;
	public override bool CanTargetGround => true;

	public EnergyFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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

			Effects.PlaySound(p, Caster.Map, 0x20B);

			var duration = Core.AOS ? TimeSpan.FromSeconds((15 + Caster.Skills.Magery.Fixed / 5.0) / 7) : TimeSpan.FromSeconds(Caster.Skills[SkillName.Magery].Value * 0.28 + 2.0);

			int itemId = eastToWest ? 0x3946 : 0x3956;

			for (int i = -2; i <= 2; ++i)
			{
				Point3D loc = new(eastToWest ? p.X + i : p.X, eastToWest ? p.Y : p.Y + i, p.Z);
				bool canFit = SpellHelper.AdjustField(ref loc, Caster.Map, 12, false);

				if (!canFit)
					continue;

				Item item = new InternalItem(loc, Caster.Map, duration, itemId, Caster);
				item.ProcessDelta();

				Effects.SendLocationParticles(EffectItem.Create(loc, Caster.Map, EffectItem.DefaultDuration), 0x376A, 9, 10, 5051);
			}
		}

		FinishSequence();
	}

	[DispellableAttributes]
	private class InternalItem : BaseItem
	{
		private readonly Timer _timer;
		private readonly Mobile _caster;

		public override bool BlocksFit => true;

		public InternalItem(Point3D loc, Map map, TimeSpan duration, int itemId, Mobile caster) : base(itemId)
		{
			Visible = false;
			Movable = false;
			Light = LightType.Circle300;

			MoveToWorld(loc, map);

			_caster = caster;

			if (caster.InLOS(this))
				Visible = true;
			else
				Delete();

			if (Deleted)
				return;

			_timer = new InternalTimer(this, duration);
			_timer.Start();
		}

		public InternalItem(Serial serial) : base(serial)
		{
			_timer = new InternalTimer(this, TimeSpan.FromSeconds(5.0));
			_timer.Start();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();
			Delete();
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m is not PlayerMobile)
				return base.OnMoveOver(m);

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
		private readonly EnergyFieldSpell _owner;

		public InternalTarget(EnergyFieldSpell owner) : base(owner.SpellRange, true, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D point3D)
				_owner.Target(point3D);
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
