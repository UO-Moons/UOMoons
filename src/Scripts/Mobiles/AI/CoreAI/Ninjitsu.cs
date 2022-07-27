using System;
using Server.Spells;
using Server.Spells.Ninjitsu;

namespace Server.Mobiles
{
    public sealed partial class CoreAi
    {
	    private DateTime m_NextShurikenThrow;
	    private int m_SmokeBombs;
	    private bool m_HasSetSmokeBombs;

	    private void NinjitsuPower()
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

                Timer.DelayCall(TimeSpan.FromSeconds(1), ShurikenDamage);

                m_NextShurikenThrow = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(5, 15));
            }
        }

	    private void GetHiddenNinjaMove()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(995, "Using a hidden ninja strike");
            }

            int whichone = Utility.RandomMinMax(1, 3);

            switch (whichone)
            {
	            case 3 when m_Mobile.Skills[SkillName.Ninjitsu].Value >= 80.0:
		            SpecialMove.SetCurrentMove(m_Mobile, new KiAttack());
		            break;
	            case >= 2 when m_Mobile.Skills[SkillName.Ninjitsu].Value >= 30.0:
		            SpecialMove.SetCurrentMove(m_Mobile, new SurpriseAttack());
		            break;
	            default:
	            {
		            if (m_Mobile.Skills[SkillName.Ninjitsu].Value >= 20.0)
		            {
			            SpecialMove.SetCurrentMove(m_Mobile, new Backstab());
		            }

		            break;
	            }
            }
        }

	    private void GetNinjaMove()
        {
            if (m_Mobile.Debug)
                m_Mobile.Say(995, "Using a ninja strike");

            int whichone = Utility.RandomMinMax(1, 3);

            switch (whichone)
            {
	            case 3 when m_Mobile.Skills[SkillName.Ninjitsu].Value >= 85.0:
		            SpecialMove.SetCurrentMove(m_Mobile, new DeathStrike());
		            break;
	            case >= 2 when m_Mobile.Skills[SkillName.Ninjitsu].Value >= 60.0:
		            SpecialMove.SetCurrentMove(m_Mobile, new FocusAttack());
		            break;
	            default:
		            UseWeaponStrike();
		            break;
            }
        }

	    private void ShurikenDamage()
        {
            Mobile target = (Mobile)m_Mobile.Combatant;

            if (target == null)
	            return;

            m_Mobile.DoHarmful(target);
            AOS.Damage(target, m_Mobile, Utility.RandomMinMax(3, 5), 100, 0, 0, 0, 0);

            switch (m_Mobile.Skills[SkillName.Ninjitsu].Value)
            {
	            case >= 120.0:
		            target.ApplyPoison(m_Mobile, Poison.Lethal);
		            break;
	            case >= 101.0:
		            target.ApplyPoison(m_Mobile, Poison.Deadly);
		            break;
	            case >= 100.0:
		            target.ApplyPoison(m_Mobile, Poison.Greater);
		            break;
	            case >= 70.0:
		            target.ApplyPoison(m_Mobile, Poison.Regular);
		            break;
	            case >= 50.0:
		            target.ApplyPoison(m_Mobile, Poison.Lesser);
		            break;
            }
        }
    }
}
