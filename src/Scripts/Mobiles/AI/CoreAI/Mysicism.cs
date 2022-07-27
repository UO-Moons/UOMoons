using Server.Spells;
using Server.Spells.Mysticism;

namespace Server.Mobiles
{
    public sealed partial class CoreAi
    {
	    private void MysticPower()
        {
            Spell spell = GetMysticSpell();

            if (spell != null)
            {
                spell.Cast();
            }
        }

	    private Spell GetMysticSpell()
        {
            Spell spell = null;

            switch( Utility.Random(8) )
            {
                case 0:
                case 1:
                {
	                if (CheckForSleep((Mobile)m_Mobile.Combatant))
	                {
		                m_Mobile.DebugSay("Casting Sleep");
		                spell = new SleepSpell(m_Mobile, null);
		                break;
	                }

	                goto case 7;
                }
                case 2:
                    {
                        if (m_Mobile.Followers < 2)
                        {
	                        int whichone = Utility.Random(3);

	                        switch (m_Mobile.Skills[SkillName.Mysticism].Value)
	                        {
		                        case > 80.0 when whichone > 0:
			                        m_Mobile.DebugSay("Casting Rising Colossus");
			                        spell = new RisingColossusSpell(m_Mobile, null);
			                        break;
		                        case > 30.0:
			                        m_Mobile.DebugSay("Casting Animated Weapon");
			                        spell = new AnimatedWeaponSpell(m_Mobile, null);
			                        break;
	                        }
                        }

                        if (spell != null)
                        {
                            break;
                        }

                        goto case 7;
                    }
                case 3:
                {
	                if (CanShapeShift && m_Mobile.Skills[SkillName.Mysticism].Value > 30.0)
	                {
		                m_Mobile.DebugSay("Casting Stone Form");
		                spell = new StoneFormSpell(m_Mobile, null);
		                break;
	                }

	                goto case 7;
                }
                case 4:
                case 5:
                {
	                if (m_Mobile.Skills[SkillName.Mysticism].Value > 70.0)
	                {
		                m_Mobile.DebugSay("Casting Spell Plague");
		                spell = new SpellPlagueSpell(m_Mobile, null);
		                break;
	                }

	                goto case 7;
                }
                case 6:
                case 7:
                {
	                spell = Utility.Random((int)(m_Mobile.Skills[SkillName.Mysticism].Value / 20)) switch
	                {
		                1 => new EagleStrikeSpell(m_Mobile, null),
		                2 => new BombardSpell(m_Mobile, null),
		                3 => new HailStormSpell(m_Mobile, null),
		                4 => new NetherCycloneSpell(m_Mobile, null),
		                _ => new NetherBoltSpell(m_Mobile, null)
	                };

	                break;
                }
            }

            return spell;
        }

	    private static bool CheckForSleep(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (pm == null && m is BaseCreature bc)
            {
	            pm = bc.ControlMaster as PlayerMobile ?? bc.SummonMaster as PlayerMobile;
            }

            return pm != null && !SleepSpell.Table.ContainsKey(pm);
        }
    }
}
