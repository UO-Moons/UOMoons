using System;
using Server.Spells;
using Server.Spells.Eighth;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Second;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Third;

namespace Server.Mobiles
{
    public partial class CoreAI : BaseAI
	{
        public static int[] m_MageryMana = new int[] { 4, 6, 9, 11, 14, 20, 40, 50 };
        DateTime m_LastExplosion;
        public bool CanUseMagerySummon
        {
            get
            {
                return false;
            }//m_ForceUseAll; }
        }
        public void MageryPower()
        {
            Spell spell = GetMagerySpell();
            if (spell != null)
            {
                m_NextCastTime = DateTime.UtcNow + spell.GetCastDelay() + spell.GetCastRecovery();
                spell.Cast();
            }

            return;
        }

        public Spell GetMagerySpell()
        {

            // always check for bless, per OSI
            Spell spell = CheckBless();

            if (spell != null)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(1156, "Blessing my self");
                }

                return spell;
            }

            // always check for curse, per OSI
            spell = CheckCurse();

            if (spell != null)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(1156, "Cursing my opponent");
                }

                return spell;
            }

            // 25% chance to cast poison if needed
            if (m_Mobile.Combatant != null && !m_Mobile.Poisoned && Utility.RandomDouble() > 0.75)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.Say(1156, "Casting Poison");
                }

                spell = new PoisonSpell(m_Mobile, null);
            }

            // scaling chance to drain mana based on how much of a caster the opponent is
            if (CheckManaDrain() > 0.75)
            {
                if (m_Mobile.Skills[SkillName.Magery].Value > 80.0)
                {
                    spell = new ManaVampireSpell(m_Mobile, null);
                }
                else if (m_Mobile.Skills[SkillName.Magery].Value > 40.0)
                {
                    spell = new ManaDrainSpell(m_Mobile, null);
                }

                if (spell != null)
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.Say(1156, "Draining mana");
                    }

                    return spell;
                }
            }

            // 10% chance to summon help
            if (CanUseMagerySummon && Utility.RandomDouble() > 0.90)
            {
                spell = CheckMagerySummon();

                if (spell != null)
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.Say(1156, "Summoning help");
                    }
                    return spell;
                }
            }

            // Let's just blast the hell out of them.
            return GetRandomMageryDamageSpell();
        }

        public Spell CheckBless()
        {
            StatMod mod = m_Mobile.GetStatMod("[Magic] Str Offset");

            if (mod != null && mod.Offset > 0)
            {
                return null;
            }

            if (m_Mobile.Skills[SkillName.Magery].Value >= 40.0)
            {
                return new BlessSpell(m_Mobile, null);
            }

            mod = m_Mobile.GetStatMod("[Magic] Int Offset");

            if (mod == null || mod.Offset < 0)
            {
                return new CunningSpell(m_Mobile, null);
            }

            mod = m_Mobile.GetStatMod("[Magic] Dex Offset");

            if (mod == null || mod.Offset < 0)
            {
                return new AgilitySpell(m_Mobile, null);
            }

            return new StrengthSpell(m_Mobile, null);
        }

        public Spell CheckCurse()
        {
            Mobile foe = (Mobile)m_Mobile.Combatant;

            if (foe == null)
            {
                return null;
            }

            StatMod mod = foe.GetStatMod("[Magic] Int Offset");

            if (mod != null && mod.Offset < 0)
            {
                return null;
            }

            if (m_Mobile.Skills[SkillName.Magery].Value >= 40.0)
            {
                return new CurseSpell(m_Mobile, null);
            }

            int whichone = 1;
            Spell spell = null;

            if ((_ = foe.GetStatMod("[Magic] Str Offset")) != null)
            {
                whichone++;
            }

            if ((_ = m_Mobile.GetStatMod("[Magic] Dex Offset")) != null)
            {
                whichone++;
            }

            switch ( whichone )
            {
                case 1: spell = new FeeblemindSpell(m_Mobile, null); break;
                case 2: spell = new WeakenSpell(m_Mobile, null); break;
                case 3: spell = new ClumsySpell(m_Mobile, null); break;
            }

            return spell;
        }

        public double CheckManaDrain()
        {
            Mobile foe = (Mobile)m_Mobile.Combatant;

            if (foe == null || foe.Mana < 10)
            {
                return 0.0;
            }

            double drain = 0.0;

            if (foe.Skills[SkillName.Bushido].Value > 35.0)
            {
                drain += 0.1;
            }

            if (foe.Skills[SkillName.Chivalry].Value > 35.0)
            {
                drain += 0.1;
            }

            if (foe.Skills[SkillName.Magery].Value > 35.0)
            {
                drain += 0.2;
            }

            if (foe.Skills[SkillName.Necromancy].Value > 35.0)
            {
                drain += 0.2;
            }

            if (foe.Skills[SkillName.Ninjitsu].Value > 35.0)
            {
                drain += 0.1;
            }

            if (drain == 0.0)
            {
                return drain;
            }
            else
            {
                return Utility.RandomDouble() + drain;
            }
        }

        public Spell CheckMagerySummon()
        {
            int slots = m_Mobile.FollowersMax - m_Mobile.Followers;

            if (slots < 2)
            {
                return null;
            }

            Spell spell = null;

            if (m_Mobile.Skills[SkillName.Magery].Value > 95.0 && m_Mobile.Mana > 50)
            {
                int whichone = Utility.Random(10);

                if (slots > 4 && whichone == 0)
                {
                    spell = new SummonDaemonSpell(m_Mobile, null);
                }
                else if (slots > 3 && whichone < 2)
                {
                    spell = new FireElementalSpell(m_Mobile, null);
                }
                else if (slots > 2 && whichone < 3)
                {
                    spell = new WaterElementalSpell(m_Mobile, null);
                }
                else if (whichone < 4)
                {
                    spell = new AirElementalSpell(m_Mobile, null);
                }
                else if (whichone < 5)
                {
                    spell = new EarthElementalSpell(m_Mobile, null);
                }
                else
                {
                    spell = new EnergyVortexSpell(m_Mobile, null);
                }
            }
            else if (m_Mobile.Skills[SkillName.Magery].Value > 55.0 && m_Mobile.Mana > 14) // 5th level
            {
                if (m_Mobile.InitialInnocent) // peace loving hippy using nature friendly animals
                {
                    spell = new SummonCreatureSpell(m_Mobile, null);
                }
                else
                {
                    spell = new BladeSpiritsSpell(m_Mobile, null);
                }
            }

            return spell;
        }

        public Spell GetRandomMageryDamageSpell()
        {
            if (m_Mobile.Debug)
            {
                m_Mobile.Say(1156, "Random damage spell");
            }

            int whichone = (int)(m_Mobile.Skills[SkillName.Magery].Value / 14.2) - 1;

            whichone += Utility.RandomMinMax(-2, -0);

            if (whichone > 6)
            {
                whichone = 6;
            }
            else if (whichone < 1)
            {
                whichone = 1;
            }

            // instead of checking each spell level to wipe all all the mana out only 
            // lower it once. Failure means mana might build up for a better spell
            if (m_MageryMana[whichone] > m_Mobile.Mana)
                whichone--;

            switch( whichone )
            {
                case 6:
                    return new FlameStrikeSpell(m_Mobile, null);
                case 5:
                    {
                        if (DateTime.UtcNow > m_LastExplosion)
                        {
                            m_LastExplosion = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                            return new ExplosionSpell(m_Mobile, null);
                        }
                        else
                        {
                            return new EnergyBoltSpell(m_Mobile, null);
                        }
                    }
                case 4: return new MindBlastSpell(m_Mobile, null);
                case 3: return new LightningSpell(m_Mobile, null);
                case 2: return new FireballSpell(m_Mobile, null);
                case 1: return new HarmSpell(m_Mobile, null);
                default: return new MagicArrowSpell(m_Mobile, null);
            }
        }
    }
}
