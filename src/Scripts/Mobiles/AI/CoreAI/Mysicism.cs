using Server.Spells;
using Server.Spells.Mysticism;

namespace Server.Mobiles
{
    public partial class CoreAI : BaseAI
	{
        public void MysticPower()
        {
            Spell spell = GetMysticSpell();

            if (spell != null)
            {
                spell.Cast();
            }

            return;
        }

        public Spell GetMysticSpell()
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
                        else
                            goto case 7;
                    }
                case 2:
                    {
                        if (m_Mobile.Followers < 2)
                        {
                            int whichone = Utility.Random(3);

                            if (m_Mobile.Skills[SkillName.Mysticism].Value > 80.0 && whichone > 0)
                            {
                                m_Mobile.DebugSay("Casting Rising Colossus");
                                spell = new RisingColossusSpell(m_Mobile, null);
                            }
                            else if (m_Mobile.Skills[SkillName.Mysticism].Value > 30.0)
                            {
                                m_Mobile.DebugSay("Casting Animated Weapon");
                                spell = new AnimatedWeaponSpell(m_Mobile, null);
                            }
                        }

                        if (spell != null)
                        {
                            break;
                        }
                        else
                        {
                            goto case 7;
                        }
                    }
                case 3:
                    {
                        if (CanShapeShift && m_Mobile.Skills[SkillName.Mysticism].Value > 30.0)
                        {
                            m_Mobile.DebugSay("Casting Stone Form");
                            spell = new StoneFormSpell(m_Mobile, null);
                            break;
                        }
                        else
                        {
                            goto case 7;
                        }
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
                        else
                        {
                            goto case 7;
                        }
                    }
                case 6:
                case 7:
                    {
                        switch( Utility.Random((int)(m_Mobile.Skills[SkillName.Mysticism].Value / 20)) )
                        {
                            default: spell = new NetherBoltSpell(m_Mobile, null); break;
                            case 1: spell = new EagleStrikeSpell(m_Mobile, null); break;
                            case 2: spell = new BombardSpell(m_Mobile, null); break;
                            case 3: spell = new HailStormSpell(m_Mobile, null); break;
                            case 4: spell = new NetherCycloneSpell(m_Mobile, null); break;
                        }

                        break;
                    }
            }

            return spell;
        }

        public bool CheckForSleep(Mobile m)
        {
            PlayerMobile pm = m as PlayerMobile;

            if (pm == null && m is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)m;

                pm = bc.ControlMaster as PlayerMobile;

                if (pm == null)
                {
                    pm = bc.SummonMaster as PlayerMobile;
                }
            }

            if (pm != null && !SleepSpell.m_Table.ContainsKey(pm))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
