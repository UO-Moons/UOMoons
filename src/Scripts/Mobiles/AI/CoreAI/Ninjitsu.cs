using System;
using Server.Spells;
using Server.Spells.Ninjitsu;

namespace Server.Mobiles
{
    public partial class CoreAI : BaseAI
	{
        public DateTime m_NextShurikenThrow;
        public int m_SmokeBombs;
        public bool m_HasSetSmokeBombs;
        public void NinjitsuPower()
        {
            if (!m_HasSetSmokeBombs)
            {
                m_HasSetSmokeBombs = true;
                m_SmokeBombs = Utility.RandomMinMax(3, 5);
            }

            Spell spell = null;

            if (m_Mobile.Hidden)
            {
                GetHiddenNinjaMove();
            }
            else if (0.2 > Utility.RandomDouble())
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(995, "Casting Mirror Image");
                }

                spell = new MirrorImage(m_Mobile, null);
            }
            else
            {
                GetNinjaMove();
            }

            if (spell != null)
            {
                spell.Cast();
            }

            if (DateTime.UtcNow > m_NextShurikenThrow && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 12))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(995, "Throwing a shuriken");
                }

                m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);

                if (m_Mobile.Body.IsHuman)
                {
                    m_Mobile.Animate(m_Mobile.Mounted ? 26 : 9, 7, 1, true, false, 0);
                }

                m_Mobile.PlaySound(0x23A);
                m_Mobile.MovingEffect(m_Mobile.Combatant, 0x27AC, 1, 0, false, false);

                Timer.DelayCall(TimeSpan.FromSeconds(1), new TimerCallback(ShurikenDamage));

                m_NextShurikenThrow = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(5, 15));
            }
        }

        public void GetHiddenNinjaMove()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(995, "Using a hidden ninja strike");
            }

            int whichone = Utility.RandomMinMax(1, 3);

            if (whichone == 3 && m_Mobile.Skills[SkillName.Ninjitsu].Value >= 80.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new KiAttack());
            }
            else if (whichone >= 2 && m_Mobile.Skills[SkillName.Ninjitsu].Value >= 30.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new SurpriseAttack());
            }
            else if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 20.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new Backstab());
            }
        }

        public void GetNinjaMove()
        {
            if (m_Mobile.Debug)
                m_Mobile.Say(995, "Using a ninja strike");

            int whichone = Utility.RandomMinMax(1, 3);

            if (whichone == 3 && m_Mobile.Skills[SkillName.Ninjitsu].Value >= 85.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new DeathStrike());
            }
            else if (whichone >= 2 && m_Mobile.Skills[SkillName.Ninjitsu].Value >= 60.0)
            {
                SpecialMove.SetCurrentMove(m_Mobile, new FocusAttack());
            }
            else
                UseWeaponStrike();
        }

        public virtual void ShurikenDamage()
        {
            Mobile target = (Mobile)m_Mobile.Combatant;

            if (target != null)
            {
                m_Mobile.DoHarmful(target);
                AOS.Damage(target, m_Mobile, Utility.RandomMinMax(3, 5), 100, 0, 0, 0, 0);

                if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 120.0)
                {
                    target.ApplyPoison(m_Mobile, Poison.Lethal);
                }
                else if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 101.0)
                {
                    target.ApplyPoison(m_Mobile, Poison.Deadly);
                }
                else if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 100.0)
                {
                    target.ApplyPoison(m_Mobile, Poison.Greater);
                }
                else if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 70.0)
                {
                    target.ApplyPoison(m_Mobile, Poison.Regular);
                }
                else if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 50.0)
                {
                    target.ApplyPoison(m_Mobile, Poison.Lesser);
                }
            }
        }
    }
}
