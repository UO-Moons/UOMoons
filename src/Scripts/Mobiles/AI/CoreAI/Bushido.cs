using Server.Items;
using Server.Spells;
using Server.Spells.Bushido;

namespace Server.Mobiles
{
    public sealed partial class CoreAi : BaseAI
    {
        public void BushidoPower()
        {
            if (0.5 > Utility.RandomDouble() && !(Confidence.IsConfident(m_Mobile) || CounterAttack.IsCountering(m_Mobile) || Evasion.IsEvading(m_Mobile)))
            {
                UseBushidoStance();
            }
            else
            {
                UseBushidoMove();
            }
        }

        public void UseBushidoStance()
        {
            Spell spell = null;

            if (m_Mobile.Debug)
            {
                m_Mobile.Say(2117, "Using a samurai stance");
            }

            if (!(m_Mobile.Weapon is BaseWeapon))
            {
                return;
            }

            int whichone = Utility.RandomMinMax(1, 3);

            if (whichone == 3 && m_Mobile.Skills[SkillName.Bushido].Value >= 60.0)
            {
                spell = new Evasion(m_Mobile, null);
            }
            else if (whichone >= 2 && m_Mobile.Skills[SkillName.Bushido].Value >= 40.0)
            {
                spell = new CounterAttack(m_Mobile, null);
            }
            else if (whichone >= 1 && m_Mobile.Skills[SkillName.Bushido].Value >= 25.0)
            {
                spell = new Confidence(m_Mobile, null);
            }

            if (spell != null)
            {
                spell.Cast();
            }
        }

        public void UseBushidoMove()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(2117, "Using a samurai or special move strike");
            }

            Mobile comb = (Mobile)m_Mobile.Combatant;

            if (comb == null)
            {
                return;
            }


            if (!(m_Mobile.Weapon is BaseWeapon weapon))
            {
                return;
            }

            int whichone = Utility.RandomMinMax(1, 4);

            if (whichone == 4 && m_Mobile.Skills[SkillName.Bushido].Value >= 70.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new MomentumStrike());
            }
            else if (whichone >= 3 && m_Mobile.Skills[SkillName.Bushido].Value >= 50.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new LightningStrike());
            }
            else if (whichone >= 2 && m_Mobile.Skills[SkillName.Bushido].Value >= 25.0 && comb.Hits <= m_Mobile.DamageMin)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new HonorableExecution());
            }
            else if (whichone >= 2 && m_Mobile.Skills[SkillName.Tactics].Value >= 90.0 && weapon != null)
            {
                WeaponAbility.SetCurrentAbility(m_Mobile, weapon.PrimaryAbility);
            }
            else if (m_Mobile.Skills[SkillName.Tactics].Value >= 60.0 && weapon != null)
            {
                WeaponAbility.SetCurrentAbility(m_Mobile, weapon.SecondaryAbility);
            }
        }
    }
}
