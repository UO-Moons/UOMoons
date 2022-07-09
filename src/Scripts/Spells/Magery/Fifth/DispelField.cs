using Server.Items;
using Server.Misc;
using Server.Targeting;
using System;

namespace Server.Spells.Fifth;

public class DispelFieldSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Dispel Field", "An Grav",
		206,
		9002,
		Reagent.BlackPearl,
		Reagent.SpidersSilk,
		Reagent.SulfurousAsh,
		Reagent.Garlic
	);

	public override SpellCircle Circle => SpellCircle.Fifth;

	public DispelFieldSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
			if (SpellTarget is Item target)
				Target(target);
			else
				FinishSequence();
		}
	}

	public void Target(Item item)
	{
		Type t = item.GetType();

		if (!Caster.CanSee(item))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (!t.IsDefined(typeof(DispellableAttributes), false))
		{
			Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
		}
		else if (item is Moongate {Dispellable: false})
		{
			Caster.SendLocalizedMessage(1005047); // That magic is too chaotic
		}
		else if (CheckSequence())
		{
			SpellHelper.Turn(Caster, item);

			Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 20, 5042);
			Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x201);

			item.Delete();
		}

		FinishSequence();
	}

	public class InternalTarget : Target
	{
		private readonly DispelFieldSpell _owner;

		public InternalTarget(DispelFieldSpell owner) : base(owner.SpellRange, false, TargetFlags.None)
		{
			_owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Item item)
			{
				_owner.Target(item);
			}
			else
			{
				_owner.Caster.SendLocalizedMessage(1005049); // That cannot be dispelled.
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			_owner.FinishSequence();
		}
	}
}
