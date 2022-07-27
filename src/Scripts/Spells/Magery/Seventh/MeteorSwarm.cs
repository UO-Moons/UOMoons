using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Spells.Seventh;

public class MeteorSwarmSpell : MagerySpell
{
	private Item Item { get; }
	private static readonly SpellInfo m_Info = new(
		"Meteor Swarm", "Flam Kal Des Ylem",
		233,
		9042,
		false,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot,
		Reagent.SulfurousAsh,
		Reagent.SpidersSilk
	);

	public override SpellCircle Circle => SpellCircle.Seventh;

	public MeteorSwarmSpell(Mobile caster, Item scroll, Item item) : base(caster, scroll, m_Info)
	{
		Item = item;
	}

	public override int GetMana()
	{
		return Item != null ? 0 : base.GetMana();
	}

	public override void OnCast()
	{
		if (Precast)
		{
			Caster.Target = new InternalTarget(this, Item);
		}
		else
		{
			if (SpellTarget is IPoint3D target)
				Target(target, Item);
			else
				FinishSequence();
		}
	}

	public override bool DelayedDamage => true;

	private void Target(IPoint3D p, Item item)
	{
		if (!Caster.CanSee(p))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (SpellHelper.CheckTown(p, Caster) && (item != null || CheckSequence()))
		{
			/*if (item != null)
			{
				if (item is MaskOfKhalAnkur)
				{
					((MaskOfKhalAnkur)item).Charges--;
				}

				if (item is PendantOfKhalAnkur)
				{
					((PendantOfKhalAnkur)item).Charges--;
				}
			}*/

			SpellHelper.Turn(Caster, p);

			if (p is Item item1)
			{
				p = item1.GetWorldLocation();
			}

			var targets = AcquireIndirectTargets(p, 2).ToList();
			var count = Math.Max(1, targets.Count);

			Effects.PlaySound(p, Caster.Map, 0x160);

			foreach (var id in targets)
			{
				Mobile m = id as Mobile;

				double damage = Core.AOS ? GetNewAosDamage(51, 1, 5, id is PlayerMobile, id) : Utility.Random(27, 22);

				switch (Core.AOS)
				{
					case true when count > 2:
						damage = damage * 2 / count;
						break;
					case false:
						damage /= count;
						break;
				}

				if (!Core.AOS && m != null && CheckResisted(m))
				{
					damage *= 0.5;

					m.SendLocalizedMessage(501783); // You feel yourself resisting magical energy.
				}

				IDamageable source = Caster;
				IDamageable target = id;

				if (SpellHelper.CheckReflect((int)Circle, ref source, ref target, SpellDamageType))
				{
					Timer.DelayCall(TimeSpan.FromSeconds(.5), () =>
					{
						source.MovingParticles(target, item != null ? 0xA1ED : 0x36D4, 7, 0, false, true, 9501, 1, 0, 0x100);
					});
				}

				if (m != null)
				{
					damage *= GetDamageScalar(m);
				}

				Caster.DoHarmful(id);
				SpellHelper.Damage(this, target, damage, 0, 100, 0, 0, 0);

				Caster.MovingParticles(id, item != null ? 0xA1ED : 0x36D4, 7, 0, false, true, 9501, 1, 0, 0x100);
			}

			ColUtility.Free(targets);
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly MeteorSwarmSpell _owner;
		private readonly Item _item;

		public InternalTarget(MeteorSwarmSpell owner, Item item)
			: base(Core.ML ? 10 : 12, true, TargetFlags.None)
		{
			_owner = owner;
			_item = item;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D p)
			{
				_owner.Target(p, _item);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
