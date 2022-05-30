using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Spellweaving;

namespace Server.Mobiles
{
    public partial class CoreAI : BaseAI
	{
        public void SpellweavingPower()
        {
            CheckFocus();

            if (CheckForGiftOfRenewal()) // Always use renewal
            {
                return;
            }

            if (m_Mobile.Combatant == null)
            {
                return;
            }

            // TODO improve selecting the spell.

            int choices = 5; // default choices: allure, summon, melee, melee, thunderstorm

            if (m_Mobile.Mana > 50)
            {
                if (m_Mobile.Skills[SkillName.Spellweaving].Value > 90.0) // add word of death+, uses 50 mana
                {
                    choices += 4;
                }
                else if (m_Mobile.Skills[SkillName.Spellweaving].Value > 66.0) // add wildfire+, uses 50 mana
                {
                    choices += 3;
                }
                else if (m_Mobile.Skills[SkillName.Spellweaving].Value > 62.0) // add essence of wind+, uses 42 mana
                {
                    choices += 2;
                }
                else if (m_Mobile.Skills[SkillName.Spellweaving].Value > 44.0) // add empower, uses 50 mana
                {
                    ++choices;
                }
            }

            switch( Utility.Random(choices) )
            {
                case 0: // Allure
                    {
                        if (m_Mobile.Combatant is BaseCreature)
                        {
                            if (m_Mobile.Debug)
                            {
                                m_Mobile.Say(1436, "Casting Dryad Allure");
                            }

                            new DryadAllureSpell(m_Mobile, null).Cast();
                            break;
                        }
                        else
                            goto case 1;
                    }
                case 1: // Summon
                    {
                        if (m_Mobile.Followers < m_Mobile.FollowersMax)
                        {
                            Spell spell = GetSpellweavingSummon();

                            if (spell != null)
                            {
                                if (m_Mobile.Debug)
                                {
                                    m_Mobile.Say(1436, "Summoning help");
                                }

                                spell.Cast();
                            }

                            ForceTarget();
                            break;
                        }
                        else
                        {
                            goto case 2;
                        }
                    }
                case 2:
                case 3: // Do nothing, aka melee
                    {
                        break;
                    }
                case 4:
                    {
                        new ThunderstormSpell(m_Mobile, null).Cast();
                        break;
                    }
                case 5: // Essence of Wind, cold aura and speed debuff.
                    {
                        if (!EssenceOfWindSpell.IsDebuffed((Mobile)m_Mobile.Combatant))
                        {
                            new EssenceOfWindSpell(m_Mobile, null).Cast();
                            break;
                        }
                        else
                        {
                            goto case 2;
                        }
                    }
                case 6:
                    {
                        new WildfireSpell(m_Mobile, null).Cast();
                        ForceTarget();
                        break;
                    }
                case 7:
                    {
                        new WordOfDeathSpell(m_Mobile, null).Cast();
                        break;
                    }
            }

            return;
        }

        // Due to many spells having no target flag we have to force it.
        public void ForceTarget()
        {
            if (m_Mobile.Target != null && m_Mobile.Combatant != null)
            {
                m_Mobile.Target.Invoke(m_Mobile, m_Mobile.Combatant);
            }
        }

        public void CheckFocus()
        {
            ArcaneFocus focus = ArcanistSpell.FindArcaneFocus(m_Mobile);

            if (focus != null)
            {
                return;
            }

            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1436, "I have no Arcane Focus");
            }

            int power = 1;

            foreach (Mobile m in m_Mobile.GetMobilesInRange(10))
            {
                if (m == null)
                {
                    continue;
                }
                else if (m == m_Mobile)
                {
                    continue;
                }
                else if (!(m is BaseCreature))
                {
                    continue;
                }

                BaseCreature bc = (BaseCreature)m;

                if (bc.Skills[SkillName.Spellweaving].Value > 50.0)
                {
                    if (m_Mobile.Controlled == bc.Controlled && m_Mobile.Summoned == bc.Summoned)
                    {
                        power++;
                    }
                }
            }

            if (power > 6)
            {
                power = 6;
            }
            else if (power < 2) // No spellweavers found, setting to min required.
            {
                power = 2;
            }

            ArcaneFocus f = new ArcaneFocus(TimeSpan.FromHours(1), power);

            Container pack = m_Mobile.Backpack;

            if (pack == null)
            {
                m_Mobile.EquipItem(new Backpack());
                pack = m_Mobile.Backpack;
            }

            pack.DropItem(f);

            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1436, "I created an Arcane Focus, it's level is: " + power.ToString());
            }
        }

        public Spell GetSpellweavingSummon()
        {
            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 38.0)
            {
                if (m_Mobile.Serial.Value % 2 == 0)
                {
                    return new SummonFeySpell(m_Mobile, null);
                }
                else
                {
                    return new SummonFiendSpell(m_Mobile, null);
                }
            }
            else
            {
                return new NatureFurySpell(m_Mobile, null);
            }
        }

        public bool CheckForGiftOfRenewal()
        {
            if (GiftOfRenewalSpell.m_Table.ContainsKey(m_Mobile) || !m_Mobile.CanBeginAction(typeof(GiftOfRenewalSpell)))
            {
                return false;
            }
            else if (m_Mobile.Skills[SkillName.Spellweaving].Value > 20.0 && m_Mobile.Mana > 24)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(1436, "Casting Gift Of Renewal");
                }

                new GiftOfRenewalSpell(m_Mobile, null).Cast();
                return true;
            }

            return false;
        }
    }
}
