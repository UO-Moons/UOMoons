using Server.Items;
using Server.Spells;
using Server.Spells.Chivalry;

namespace Server.Mobiles
{
    public sealed partial class CoreAi
    {
	    private void ChivalryPower()
        {
            if (Utility.Random(100) > 30)
            {
                Spell spell = GetPaladinSpell();
                spell?.Cast();
            }
            else
            {
                UseWeaponStrike();
            }
        }

	    private Spell GetPaladinSpell()
        {
            if (CheckForRemoveCurse() && Utility.RandomDouble() > 0.25)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(1154, "Casting Remove Curse");
                }

                return new RemoveCurseSpell(m_Mobile, null);
            }

            int whichone = Utility.RandomMinMax(1, 4);

            switch (whichone)
            {
	            case 4 when m_Mobile.Skills[SkillName.Chivalry].Value >= 55.0 && m_Mobile.Mana >= 10:
	            {
		            if (m_Mobile.Debug)
		            {
			            m_Mobile.Say(1154, "Casting Holy Light");
		            }

		            return new HolyLightSpell(m_Mobile, null);
	            }
	            case >= 3 when CheckForDispelEvil():
	            {
		            if (m_Mobile.Debug)
		            {
			            m_Mobile.Say(1154, "Casting Dispel Evil");
		            }

		            return new DispelEvilSpell(m_Mobile, null);
	            }
	            case >= 2 when !(DivineFurySpell.UnderEffect(m_Mobile)) && m_Mobile.Skills[SkillName.Chivalry].Value >= 35.0:
	            {
		            if (m_Mobile.Debug)
		            {
			            m_Mobile.Say(1154, "Casting Divine Fury");
		            }

		            return new DivineFurySpell(m_Mobile, null);
	            }
	            default:
	            {
		            if (!CheckForConsecrateWeapon())
			            return null;

		            if (m_Mobile.Debug)
		            {
			            m_Mobile.Say(1154, "Casting Consecrate Weapon");
		            }

		            return new ConsecrateWeaponSpell(m_Mobile, null);

	            }
            }
        }

	    private bool CheckForConsecrateWeapon()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1154, "Checking to bless my weapon");
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value < 15.0 || m_Mobile.Mana <= 9)
            {
                return false;
            }

            return m_Mobile.Weapon is BaseWeapon { ConsecratedContext: { } };
        }

	    private bool CheckForDispelEvil()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1154, "Checking to dispel evil");
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value < 35.0 || m_Mobile.Mana <= 9)
            {
                return false;
            }

            bool cast = false;

            foreach (Mobile m in m_Mobile.GetMobilesInRange(4))
            {
                if (m != null)
                {
	                switch (m)
	                {
		                case BaseCreature { Summoned: true, IsAnimatedDead: false }:
		                case BaseCreature { Controlled: false, Karma: < 0 }:
			                cast = true;
			                break;
		                default:
		                {
			                if (TransformationSpellHelper.CheckCast(m, null))
			                {
				                cast = true;
			                }

			                break;
		                }
	                }
                }
            }

            return cast;
        }

	    private bool CheckForRemoveCurse()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1154, "Checking for remove curse");
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value < 5.0 || m_Mobile.Mana <= 19)
            {
                return false;
            }

            var mod = (m_Mobile.GetStatMod("[Magic] Str Offset") ?? m_Mobile.GetStatMod("[Magic] Dex Offset")) ??
                      m_Mobile.GetStatMod("[Magic] Int Offset");

            if (mod is { Offset: < 0 })
            {
                return true;
            }

            Mobile foe = (Mobile)m_Mobile.Combatant;

            if (foe == null)
            {
                return false;
            }

            //There is no way to know if they are under blood oath or strangle without editing the spells so we just check for necro skills instead.
            return foe.Skills[SkillName.Necromancy].Value > 20.0 && Utility.RandomDouble() > 0.6;
        }
    }
}
