using Server.Items;
using Server.Targeting;

namespace Server.Spells.Third;

public class TelekinesisSpell : MagerySpell
{
	private static readonly SpellInfo m_Info = new(
		"Telekinesis", "Ort Por Ylem",
		203,
		9031,
		Reagent.Bloodmoss,
		Reagent.MandrakeRoot
	);

	public override SpellCircle Circle => SpellCircle.Third;

	public TelekinesisSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
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
			if (SpellTarget is ITelekinesisable target)
				Target(target);
			else
				FinishSequence();
		}
	}

	private void Target(ITelekinesisable obj)
	{
		if (CheckSequence())
		{
			SpellHelper.Turn(Caster, obj);

			obj.OnTelekinesis(Caster);
		}

		FinishSequence();
	}

	private void Target(Container item)
	{
		if (CheckSequence())
		{
			SpellHelper.Turn(Caster, item);

			object root = item.RootParent;

			if (!item.IsAccessibleTo(Caster))
			{
				item.OnDoubleClickNotAccessible(Caster);
			}
			else if (!item.CheckItemUse(Caster, item))
			{
			}
			else if (root is Mobile && root != Caster)
			{
				item.OnSnoop(Caster);
			}
			else if (item is Corpse corpse && !corpse.CheckLoot(Caster, null))
			{
			}
			else if (Caster.Region.OnDoubleClick(Caster, item))
			{
				Effects.SendLocationParticles(EffectItem.Create(item.Location, item.Map, EffectItem.DefaultDuration), 0x376A, 9, 32, 5022);
				Effects.PlaySound(item.Location, item.Map, 0x1F5);

				item.OnItemUsed(Caster, item);
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly TelekinesisSpell m_Owner;

		public InternalTarget(TelekinesisSpell owner) : base(owner.SpellRange, false, TargetFlags.None)
		{
			m_Owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is ITelekinesisable telekinesisable)
				m_Owner.Target(telekinesisable);
			else if (o is Container container)
				m_Owner.Target(container);
			else
				from.SendLocalizedMessage(501857); // This spell won't work on that!
		}

		protected override void OnTargetFinish(Mobile from)
		{
			m_Owner.FinishSequence();
		}
	}
}
