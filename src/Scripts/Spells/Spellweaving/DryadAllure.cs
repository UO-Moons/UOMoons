using System;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Spells.Spellweaving;

public class DryadAllureSpell : ArcanistSpell
{
	private static readonly SpellInfo m_Info = new(
		"Dryad Allure", "Rathril",
		-1);
	public DryadAllureSpell(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{
	}

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(3);
	public override double RequiredSkill => 52.0;
	public override int RequiredMana => 40;

	private static bool IsValidTarget(BaseCreature bc)
	{
		if (bc == null || bc.IsParagon || (bc.Controlled && !bc.Allured) || bc.Summoned || bc.AllureImmunity)
			return false;

		SlayerEntry slayer = SlayerGroup.GetEntryByName(SlayerName.Repond);

		return slayer != null && slayer.Slays(bc);
	}

	public override void OnCast()
	{
		Caster.Target = new InternalTarget(this);
	}

	private void Target(BaseCreature bc)
	{
		if (!Caster.CanSee(bc.Location) || !Caster.InLOS(bc))
		{
			Caster.SendLocalizedMessage(500237); // Target can not be seen.
		}
		else if (!IsValidTarget(bc))
		{
			Caster.SendLocalizedMessage(1074379); // You cannot charm that!
		}
		else if (Caster.Followers + 3 > Caster.FollowersMax)
		{
			Caster.SendLocalizedMessage(1049607); // You have too many followers to control that creature.
		}
		else if (bc.Allured)
		{
			Caster.SendLocalizedMessage(1074380); // This humanoid is already controlled by someone else.				
		}
		else if (CheckSequence())
		{
			int level = GetFocusLevel(Caster);
			double skill = Caster.Skills[CastSkill].Value;

			double chance = skill / 150.0 + level / 50.0;

			if (chance > Utility.RandomDouble())
			{
				bc.ControlSlots = 3;
				bc.Combatant = null;

				if (Caster.Combatant == bc)
				{
					Caster.Combatant = null;
					Caster.Warmode = false;
				}

				if (bc.SetControlMaster(Caster))
				{
					bc.PlaySound(0x5C4);
					bc.Allured = true;

					Container pack = bc.Backpack;

					if (pack != null)
					{
						for (int i = pack.Items.Count - 1; i >= 0; --i)
						{
							if (i >= pack.Items.Count)
								continue;

							pack.Items[i].Delete();
						}
					}

					Caster.SendLocalizedMessage(1074377); // You allure the humanoid to follow and protect you.
				}
			}
			else
			{
				bc.PlaySound(0x5C5);
				bc.ControlTarget = Caster;
				bc.ControlOrder = OrderType.Attack;
				bc.Combatant = Caster;

				Caster.SendLocalizedMessage(1074378); // The humanoid becomes enraged by your charming attempt and attacks you.
			}
		}

		FinishSequence();
	}

	private class InternalTarget : Target
	{
		private readonly DryadAllureSpell m_Owner;
		public InternalTarget(DryadAllureSpell owner)
			: base(12, false, TargetFlags.None)
		{
			m_Owner = owner;
		}

		protected override void OnTarget(Mobile m, object o)
		{
			if (o is BaseCreature creature)
			{
				m_Owner.Target(creature);
			}
			else
			{
				m.SendLocalizedMessage(1074379); // You cannot charm that!
			}
		}

		protected override void OnTargetFinish(Mobile m)
		{
			m_Owner.FinishSequence();
		}
	}
}
