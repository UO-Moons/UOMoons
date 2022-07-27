using Server.Items;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Eighth;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Second;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Spellweaving;
using Server.Spells.Third;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;



namespace Server.Mobiles
{

    public class SuperAI : BaseAI
	{
        private DateTime m_NextActTime; //global delay for waiting to perform another spell/action (good to save mana)


        public SuperAI(BaseCreature m) : base(m)
        {
        }

        private bool targetpending = false; //this used to ensure target is only processed if it was invoked by the AI
        private DateTime targetpending_date;
        private DateTime NextRunCheck;


        private void SetTargPending()
        {
            targetpending = true;
            targetpending_date = DateTime.UtcNow;
        }

        private bool Check4Target()
        {
            if (!targetpending)
            {
                return false;
            }

            if ((targetpending_date + TimeSpan.FromSeconds(3)) < DateTime.UtcNow)
            {
                targetpending = false;
            }

            return true;
        }

        private bool HasSpecialMove()
        {
            SpecialMove move = SpecialMove.GetCurrentMove(m_Mobile);
            return !(move == null);
        }

        public override bool Think()
        {
            if (m_Mobile.Deleted)
            {
                return false;
            }

            Target targ = m_Mobile.Target;

            //if(targ!=null)Console.Write("*"); else Console.Write("."); //debug

            if (targ != null && targetpending)
            {
                targetpending = false;
                ProcessTarget(targ);
            }

            if (Check4Target())
            {
                return true;
            }

            return base.Think();
        }


        public bool IsPet()
        {
            return m_Mobile.SummonMaster != null || m_Mobile.ControlMaster != null;
        }


        //call this after any hiding
        public void AfterHide()
        {
            m_Mobile.Combatant = null;
            m_Mobile.Warmode = false;

            SetWait();
            last_invis = DateTime.UtcNow;
            Action = ActionType.Wander;
        }



        public virtual bool SmartAI => true;


        //private const double TeleportChance = 0.15; // 5% chance to teleport at gm magery


        public virtual double ScaleByMagery(double v)
        {
            return m_Mobile.Skills[SkillName.Magery].Value * v * 0.01;
        }

        private bool IsCasting()
        {
            if (m_Mobile.Spell != null && m_Mobile.Spell is Spell)
            {
                Spell curspell = (Spell)m_Mobile.Spell;

                if (curspell.IsCasting /* || curspell.IsSequencing*/)
                {
                    return true;
                }
            }

            return false;
        }

        //wether or not we should wait before performing another action
        private bool ShouldWait()
        {
            if (!(Check4Target() || m_NextActTime > DateTime.UtcNow || (m_Mobile.Hidden && m_Mobile.Skills[SkillName.Stealth].Value < 50 && 0.1 > Utility.RandomDouble())))
            {
                return false;
            }

            return true;
        }

        //set wait time for next action (global)
        private void SetWait()
        {
            SetWait(Utility.Random(10));
        }

        private void SetWait(int seconds)
        {
            if (Action == ActionType.Flee)
            {
                return;
            }

            m_Mobile.DebugSay("wait {0}", seconds);
            m_NextActTime = DateTime.UtcNow + TimeSpan.FromSeconds(seconds);
        }

        private void CheckRun(Mobile c)
        {
            if (c == null)
            {
                return;
            }

            if (NextRunCheck > DateTime.UtcNow)
            {
                return;
            }

            if (m_Mobile.Hidden && m_Mobile.Skills[SkillName.Stealth].Value > 0.1 && 0.9 > Utility.RandomDouble())
            {
                return;
            }

            //don't get close if we have no melee capabilities - assuming we have a combatant in range
            if (m_Mobile.InRange(c, 4) && !CanMelee() && m_Mobile.InLOS(c))
            {
                if (!RunFrom(c))
                {
                    WalkRandom(0, 0, 1);
                    NextRunCheck = DateTime.UtcNow + TimeSpan.FromSeconds(5 + Utility.Random(4));
                }
            }
            else if (!m_Mobile.InRange(c, 7) && !CanMelee())
            {
                RunTo(c);
            }
            else if (CanMelee() && !m_Mobile.InRange(c, 1))
            {
                RunTo(c);
            }
        }


        //used for curse wep and consecrate mostly
        public bool CanWeaponFight()
        {
            //if(CanMelee())return true;

            if (m_Mobile.Skills[SkillName.Archery].Value > 0.1 && m_Mobile.HasLongRangeWep())
            {
                return true;
            }

            if ((m_Mobile.Skills[SkillName.Swords].Value + m_Mobile.Skills[SkillName.Fencing].Value + m_Mobile.Skills[SkillName.Macing].Value) >= 10 && m_Mobile.HasWep())
            {
                return true;
            }

            return false;
        }

        public bool CanMelee()
        {
            bool haswep = m_Mobile.HasWep();

            if (m_Mobile.Skills[SkillName.Wrestling].Value >= 10 && !haswep)
            {
                return true;
            }

            if (m_Mobile.HasLongRangeWep())
            {
                return false;
            }

            if ((m_Mobile.Skills[SkillName.Swords].Value + m_Mobile.Skills[SkillName.Fencing].Value + m_Mobile.Skills[SkillName.Macing].Value) >= 10 && haswep)
            {
                return true;
            }

            return false;
        }

        public override bool CanDistanceFight()
        {
            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 || (m_Mobile.Skills[SkillName.Archery].Value > 0.1 && m_Mobile.HasLongRangeWep()))
            {
                return true;
            }

            return false;
        }



        // ****************************************************************************************

        //delay timers  
        DateTime last_healactioncontrolled;
        DateTime last_healaction;
        DateTime last_giftofrenewal;
        DateTime last_spiritspeak;
        DateTime last_confidence;
        DateTime last_chugpot;

        public bool DoHealingAction(bool frombasecreature)
        {
            if (!frombasecreature)
            {
                if (ShouldWait())
                {
                    return false;
                }

                if (m_Mobile.IsAnimatedDead)
                {
                    return false;
                }
            }

            if ((last_healactioncontrolled + TimeSpan.FromSeconds(15)) > DateTime.UtcNow)
            {
                return false;
            }

            if (!m_Mobile.IsSupreme && !m_Mobile.IsParagon && (last_healaction + m_Mobile.SuperAiHealingDelay) > DateTime.UtcNow)
            {
                return false;
            }

            if (m_Mobile.Controlled)
            {
                last_healactioncontrolled = DateTime.UtcNow;
            }

            last_healaction = DateTime.UtcNow;

            ArrayList actions = new ArrayList();


            //each item type is assigned a number which we'll check for after - this number has no real meaning exept for in this function		

            if (!MortalStrike.IsWounded(m_Mobile))
            {
                //cure or heal			


                if (m_Mobile.Skills[SkillName.Healing].Value > 0.1 && BandageContext.GetContext(m_Mobile) == null)
                {
                    actions.Add(1); //healing skill (this based on dex and adds 1 second delay)
                }

                //heal
                if (m_Mobile.Poison == null)
                {
                    if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 4 && !IsCasting())
                    {
                        actions.Add(2);  //mini heal spell
                    }

                    if (m_Mobile.Skills[SkillName.Magery].Value > 60 && m_Mobile.Mana >= 11 && !IsCasting())
                    {
                        actions.Add(3);  //greater heal spell
                    }

                    if (m_Mobile.Skills[SkillName.Chivalry].Value > 0.1 && m_Mobile.Mana >= 10 && !IsCasting())
                    {
                        actions.Add(4);  //close wounds
                    }

                    if (m_Mobile.Skills[SkillName.Alchemy].Value > 0.1 && (last_chugpot + TimeSpan.FromSeconds(11)) < DateTime.UtcNow)
                    {
                        actions.Add(9); //chug heal pot
                    }
                }
                else  //cure
                {
                    if (m_Mobile.Skills[SkillName.Chivalry].Value > 10 && m_Mobile.Mana >= 10 && !IsCasting())
                    {
                        actions.Add(5);  //clense by fire
                    }

                    if (m_Mobile.Skills[SkillName.Magery].Value > 20 && m_Mobile.Mana >= 6 && !IsCasting())
                    {
                        actions.Add(6);  //cure spell		
                    }

                    if (m_Mobile.Skills[SkillName.Alchemy].Value > 0.1 && (last_chugpot + TimeSpan.FromSeconds(11)) < DateTime.UtcNow)
                    {
                        actions.Add(11); //chug heal pot			
                    }
                }
            }

            if (m_Mobile.Skills[SkillName.SpiritSpeak].Value > 0.1 && m_Mobile.Mana >= 10 && (last_spiritspeak + TimeSpan.FromSeconds(10)) < DateTime.UtcNow && !IsCasting())
            {
                actions.Add(7); //spirit speak skill
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 0.1 && m_Mobile.Mana >= 24 && (last_giftofrenewal + TimeSpan.FromMinutes(6)) < DateTime.UtcNow && !IsCasting())
            {
                actions.Add(8); //gift of renewal spell
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 25 && m_Mobile.Mana >= 10 && last_confidence + TimeSpan.FromSeconds(10) < DateTime.UtcNow && !IsCasting())
            {
                actions.Add(10); //confidence
            }

            if (actions.Count <= 0)
            {
                return false;
            }


            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    Bandage band = new Bandage();
                    band.Delete(); //delete it right away so it does not stay stuck in internal map, should the target be canceled

                    band.OnDoubleClick(m_Mobile);

                    m_Mobile.DebugSay("using bandage");
                    return true;
                //-----------------------
                case 2:
                    SetTargPending();
                    new HealSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("heal spell");
                    return true;
                //-----------------------
                case 3:
                    SetTargPending();
                    new GreaterHealSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("greater heal spell");
                    return true;
                //-----------------------
                case 4:
                    SetTargPending();
                    new CloseWoundsSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("close wounds");
                    return true;
                //-----------------------
                case 5:
                    SetTargPending();
                    new CleanseByFireSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("clense by fire");
                    return true;
                //-----------------------
                case 6:
                    SetTargPending();
                    new CureSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("cure spell");
                    return true;
                //-----------------------
                case 7:
                    m_Mobile.UseSkill(SkillName.SpiritSpeak);
                    last_spiritspeak = DateTime.UtcNow;
                    m_Mobile.DebugSay("spirit speak skill");
                    return true;
                //-----------------------
                case 8:
                    SetTargPending();
                    new GiftOfRenewalSpell(m_Mobile, null).Cast();
                    last_giftofrenewal = DateTime.UtcNow;
                    m_Mobile.DebugSay("Gift of renewal spell");
                    return true;
                //-----------------------		
                case 9:
                    GreaterHealPotion healpot = new GreaterHealPotion();
                    last_chugpot = DateTime.UtcNow;

                    healpot.Drink(m_Mobile);

                    m_Mobile.DebugSay("chug heal pot");
                    return true;
                //-----------------------		
                case 10:
                    new Confidence(m_Mobile, null).Cast();
                    last_confidence = DateTime.UtcNow;
                    m_Mobile.DebugSay("confidence");
                    return true;
                //-----------------------			
                case 11:
                    GreaterCurePotion curepot = new GreaterCurePotion();
                    last_chugpot = DateTime.UtcNow;

                    curepot.Drink(m_Mobile);

                    m_Mobile.DebugSay("chug cure pot");
                    return true;
                    //-----------------------				
            }

            return false;
        }

        //****************************************************************************************

        //delay timers


        public bool DoReveilAction()
        {

            //only want to do this if we're a bad guy or controlled
            if (m_Mobile is BaseCreature bc)
            {
                if (!bc.Controlled && !bc.Summoned && m_Mobile.FightMode == FightMode.Aggressor || m_Mobile.FightMode == FightMode.Evil)
                {
                    return false;
                }
            }


            if (ShouldWait())
            {
                return false;
            }

            if (m_Mobile.FightMode == FightMode.Aggressor || m_Mobile.FightMode == FightMode.Evil)
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Controlled)
            {
                return false; //we don't want pets to use this, since they can reveil their owners/friends
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 40 && m_Mobile.Mana >= 30 && !IsCasting() && !m_Mobile.Hidden)
            {
                actions.Add(1); //reveil spell
            }

            if (m_Mobile.Skills[SkillName.DetectHidden].Value > 0.1)
            {
                actions.Add(2); //detect hidden skill	
            }

            if (actions.Count <= 0)
            {
                return false;
            }
            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    new RevealSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("reveil");
                    return true;
                //-----------------------	
                case 2:
                    SetTargPending();
                    m_Mobile.UseSkill(SkillName.DetectHidden);

                    m_Mobile.DebugSay("detect hidden");
                    return true;
                    //-----------------------				
            }

            return false;
        }



        //****************************************************************************************

        //delay timers
        DateTime last_attunement;
        private DateTime last_invis;

        //stuff to defend ourselves
        public bool DoDefensiveAction(bool fighting)
        {
            if (ShouldWait())
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 0.1 && m_Mobile.Mana >= 24 && (last_attunement + TimeSpan.FromMinutes(5)) < DateTime.UtcNow && !IsCasting())
            {
                actions.Add(1); //attunement
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 5 && !IsCasting() && fighting)
            {
                actions.Add(2); //clumbsy (to disturb)
            }

            if (!IsPet() && m_Mobile.Skills[SkillName.Magery].Value > 70 && m_Mobile.Mana >= 30 && !m_Mobile.Hidden && 0.2 > Utility.RandomDouble() && !IsCasting())
            {
                actions.Add(3); //invisibility
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 50 && m_Mobile.Mana >= 15)
            {
                actions.Add(4); //counter attack
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 70 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(5); //evasion
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 30 && m_Mobile.Mana >= 25 && !IsCasting() && fighting)
            {
                actions.Add(6); //blood oath
            }

            if (!IsPet() && m_Mobile.Skills[SkillName.Ninjitsu].Value > 70 && m_Mobile.Mana >= 30 && 0.7 > Utility.RandomDouble())
            {
                actions.Add(7); //smoke bomb	
            }

            if (!IsPet() && m_Mobile.Skills[SkillName.Hiding].Value > 50 && !fighting)
            {
                actions.Add(8); //hiding
            }
            else
            {
                actions.Add(9); //teleport action
            }

            if (actions.Count <= 0)
            {
                return false;
            }



            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    new AttuneWeaponSpell(m_Mobile, null).Cast();
                    last_attunement = DateTime.UtcNow;

                    m_Mobile.DebugSay("attunement");
                    return true;
                //-----------------------		
                case 2:
                    SetTargPending();
                    new ClumsySpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("clumbsy");
                    return true;
                //-----------------------					
                case 3:
                    SetTargPending();
                    new InvisibilitySpell(m_Mobile, null).Cast();
                    AfterHide();
                    m_Mobile.DebugSay("invis");
                    return true;
                //-----------------------			
                case 4:
                    new CounterAttack(m_Mobile, null).Cast();
                    SetWait(15);
                    m_Mobile.DebugSay("counter attack");
                    return true;
                //-----------------------	
                case 5:
                    new Evasion(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("evasion");
                    return true;
                //-----------------------			
                case 6:
                    SetTargPending();
                    new BloodOathSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("blood oath");
                    return true;
                //-----------------------			
                case 7:
                    SmokeBomb bomb = new SmokeBomb();

                    bomb.OnDoubleClick(m_Mobile);

                    AfterHide();
                    m_Mobile.DebugSay("smoke bomb ");
                    return true;
                //-----------------------				
                case 8:
                    m_Mobile.UseSkill(SkillName.Hiding);

                    AfterHide();
                    m_Mobile.DebugSay("hiding skill");
                    return true;
                //-----------------------	
                case 9:
                    return DoTeleportAction();
                    //-----------------------	

            }


            return false;
        }



        //****************************************************************************************
        //delay timers
        DateTime last_consecrate;
        DateTime last_divinefury;

        //any damaging, or otherwise harmful action, including spells that increase the next harmful action
        public bool DoOffensiveAction()
        {
            if (ShouldWait())
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 5)
            {
                actions.Add(1); //magic arrow
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 10 && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 3))
            {
                actions.Add(2); //harm
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 15 && m_Mobile.Mana >= 15)
            {
                actions.Add(3); //fireball
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 15 && m_Mobile.Mana >= 15)
            {
                actions.Add(4); //poison
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 30 && m_Mobile.Mana >= 24)
            {
                actions.Add(5); //lightning
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 45 && m_Mobile.Mana >= 24)
            {
                actions.Add(6); //mind blast
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 60 && m_Mobile.Mana >= 30)
            {
                actions.Add(7); //ebolt
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 60 && m_Mobile.Mana >= 30)
            {
                actions.Add(8); //explosion
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 74 && m_Mobile.Mana >= 45)
            {
                actions.Add(9); //flame strike
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 65 && m_Mobile.Mana >= 10 && !HasSpecialMove())
            {
                actions.Add(10); //lightning strike
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value > 20 && (last_consecrate + TimeSpan.FromSeconds(30 + Utility.Random(30))) < DateTime.UtcNow && m_Mobile.Mana >= 15 && m_Mobile.HasWep() && m_Mobile.Combatant != null && (m_Mobile.InRange(m_Mobile.Combatant, 1) || m_Mobile.HasLongRangeWep()))
            {
                actions.Add(11); //consecrate
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 0.1 && m_Mobile.Mana >= 10 && (CanMelee() && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 1)))
            {
                actions.Add(12); //curse wep
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 30 && m_Mobile.Mana >= 10)
            {
                actions.Add(13); //pain spike
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 70 && m_Mobile.Mana >= 40)
            {
                actions.Add(14); //strangle
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 60 && m_Mobile.Mana >= 25)
            {
                actions.Add(15); //poison strike 
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 75 && m_Mobile.Mana >= 30 && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 3))
            {
                actions.Add(16); //wither
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 90 && m_Mobile.Mana >= 50)
            {
                actions.Add(17); //vengeful spirit (custom)
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value > 65 && m_Mobile.Mana >= 15 && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 2))
            {
                actions.Add(18); //holy light		
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 0.1 && m_Mobile.Mana >= 10)
            {
                actions.Add(19); //evil omen
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 70 && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 5) && m_Mobile.Mana >= 90)
            {
                actions.Add(20); //Essense of wind 
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 15 && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 2) && m_Mobile.Mana >= 80)
            {
                actions.Add(21); //Thunder storm 
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 50 && m_Mobile.Mana >= 40 && m_Mobile.Hidden && !HasSpecialMove())
            {
                actions.Add(22); //backstab
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 95 && m_Mobile.Mana >= 45 && !HasSpecialMove())
            {
                actions.Add(23);    //death strike	
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 90 && m_Mobile.Mana >= 25 && !m_Mobile.Hidden && !HasSpecialMove() && m_Mobile.HasMeleeWep())
            {
                actions.Add(24);  //KI attack
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 40 && m_Mobile.Mana >= 35 && m_Mobile.Hidden && !HasSpecialMove())
            {
                actions.Add(25);     //suprise attack
            }

            if ((m_Mobile.Skills[SkillName.Chivalry].Value > 10 && m_Mobile.Mana >= 30 && (m_Mobile.Stam < (m_Mobile.StamMax - 100) || CanWeaponFight()) && (last_divinefury + TimeSpan.FromSeconds(30)) < DateTime.UtcNow))
            {
                actions.Add(26); //divine fury		
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 99 && m_Mobile.Mana >= 70 && m_Mobile.Combatant != null && m_Mobile.Combatant.Hits < (m_Mobile.Combatant.HitsMax / 10))
            {
                actions.Add(27); //word of death
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 90 && m_Mobile.Mana >= 60 && m_Mobile.Combatant != null)
            {
                actions.Add(28); //wild fire
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 90 && m_Mobile.Mana >= 20 && !HasSpecialMove())
            {
                actions.Add(29); //momentum strike
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 80 && m_Mobile.Mana >= 60 && 0.25 > Utility.RandomDouble())
            {
                actions.Add(30); //fire field
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 99 && m_Mobile.Mana >= 90 && 0.25 > Utility.RandomDouble())
            {
                actions.Add(31); //paralyze field
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 99 && m_Mobile.Skills[SkillName.Poisoning].Value > 50 && m_Mobile.Mana >= 90 && 0.5 > Utility.RandomDouble())
            {
                actions.Add(32); //poison field
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 99 && m_Mobile.Mana >= 90)
            {
                actions.Add(33); //Earth Quake
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 99 && m_Mobile.Mana >= 80)
            {
                actions.Add(34); //meteor swarm
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 99 && m_Mobile.Mana >= 80)
            {
                actions.Add(35); //chain lightning
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 60 && m_Mobile.Mana >= 50)
            {
                actions.Add(36); //paralyze		
            }

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 10 && m_Mobile.Mana >= 50 && m_Mobile.Followers + 1 < m_Mobile.FollowersMax && 0.1 > Utility.RandomDouble())
            {
                actions.Add(37); //Natures furry	
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 100 && m_Mobile.Mana >= 180 && m_Mobile.Followers + 2 < m_Mobile.FollowersMax && 0.06 > Utility.RandomDouble())
            {
                actions.Add(38); //Energy Vortex	
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 100 && m_Mobile.Mana >= 80 && m_Mobile.Followers + 4 < m_Mobile.FollowersMax && 0.12 > Utility.RandomDouble())
            {
                actions.Add(39); //Blade Spirits	
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 45 && m_Mobile.Mana >= 30 && m_Mobile.Followers + 1 < m_Mobile.FollowersMax && 0.25 > Utility.RandomDouble())
            {
                actions.Add(40); //Mirror images	
            }

            if (actions.Count <= 0)
            {
                return false;
            }

            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    new MagicArrowSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("magic arrow");
                    return true;
                //-----------------------					
                case 2:
                    SetTargPending();
                    new HarmSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("harm");
                    return true;
                //-----------------------					
                case 3:
                    SetTargPending();
                    new FireballSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("fire ball");
                    return true;
                //-----------------------		
                case 4:
                    SetTargPending();
                    new PoisonSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("poison");
                    return true;
                //-----------------------		
                case 5:
                    SetTargPending();
                    new LightningSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("lightning");
                    return true;
                //-----------------------		
                case 6:
                    SetTargPending();
                    new MindBlastSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("mind blast");
                    return true;
                //-----------------------		
                case 7:
                    SetTargPending();
                    new EnergyBoltSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("e bolt");
                    return true;
                //-----------------------		
                case 8:
                    SetTargPending();
                    new ExplosionSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("explosion");
                    return true;
                //-----------------------		
                case 9:
                    SetTargPending();
                    new FlameStrikeSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("flame strike");
                    return true;
                //-----------------------		
                case 10:
                    SpecialMove.SetCurrentMove(m_Mobile, new LightningStrike());

                    m_Mobile.DebugSay("lightning strike");
                    return true;
                //-----------------------		
                case 11:
                    new ConsecrateWeaponSpell(m_Mobile, null).Cast();
                    last_consecrate = DateTime.UtcNow;
                    m_Mobile.DebugSay("consecrate wep");
                    return true;
                //-----------------------		
                case 12:
                    new CurseWeaponSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("Curse wep");
                    return true;
                //-----------------------		
                case 13:
                    SetTargPending();
                    new PainSpikeSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("pain spike");
                    return true;
                //-----------------------		
                case 14:
                    SetTargPending();
                    new StrangleSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("strangle");
                    return true;
                //-----------------------		
                case 15:
                    SetTargPending();
                    new PoisonStrikeSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("poison strike");
                    return true;
                //-----------------------		
                case 16:
                    new WitherSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("wither");
                    return true;
                //-----------------------		
                case 17:
                    SetTargPending();
                    new VengefulSpiritSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("vengeful spirit");
                    return true;
                //-----------------------		
                case 18:
                    new HolyLightSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("holy light");
                    return true;
                //-----------------------			
                case 19:
                    SetTargPending();
                    new EvilOmenSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("evil omen");
                    return true;
                //-----------------------			
                case 20:
                    new EssenceOfWindSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("essense of wind");
                    return true;
                //-----------------------		
                case 21:
                    new ThunderstormSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("thunder storm");
                    return true;
                //-----------------------					
                case 22:
                    SpecialMove.SetCurrentMove(m_Mobile, new Backstab());

                    m_Mobile.DebugSay("backstab");
                    return true;
                //-----------------------		
                case 23:
                    SpecialMove.SetCurrentMove(m_Mobile, new DeathStrike());

                    m_Mobile.DebugSay("death strike");
                    return true;
                //-----------------------				
                case 24:
                    SpecialMove.SetCurrentMove(m_Mobile, new KiAttack());

                    m_Mobile.DebugSay("KI attack");
                    return true;
                //-----------------------				
                case 25:
                    SpecialMove.SetCurrentMove(m_Mobile, new SurpriseAttack());

                    m_Mobile.DebugSay("supprise attack");
                    return true;
                //-----------------------				
                case 26:
                    new DivineFurySpell(m_Mobile, null).Cast();
                    last_divinefury = DateTime.UtcNow;
                    m_Mobile.DebugSay("divine fury");
                    return true;
                //-----------------------				
                case 27:
                    SetTargPending();
                    new WordOfDeathSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("word of death");
                    return true;
                //-----------------------				
                case 28:
                    SetTargPending();
                    new WildfireSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("wild fire");
                    return true;
                //-----------------------				
                case 29:

                    SpecialMove.SetCurrentMove(m_Mobile, new MomentumStrike());
                    m_Mobile.DebugSay("momentum strike");
                    return true;
                //-----------------------				
                case 30:
                    SetTargPending();
                    new FireFieldSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Fire field ");
                    return true;
                //-----------------------				
                case 31:
                    SetTargPending();
                    new ParalyzeFieldSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("paralyze field");
                    return true;
                //-----------------------				
                case 32:
                    SetTargPending();
                    new PoisonFieldSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("poison field");
                    return true;
                //-----------------------				
                case 33:
                    SetTargPending();
                    new EarthquakeSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Earth Quake");
                    return true;
                //-----------------------				
                case 34:
                    SetTargPending();
                    new MeteorSwarmSpell(m_Mobile, null, null).Cast();
                    m_Mobile.DebugSay("meteor swarm");
                    return true;
                //-----------------------				
                case 35:
                    SetTargPending();
                    new ChainLightningSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("chain lightning");
                    return true;
                //-----------------------				
                case 36:
                    SetTargPending();
                    new ParalyzeSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("paralyze");
                    return true;
                //-----------------------				
                case 37:
                    SetTargPending();
                    new NatureFurySpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Nature Fury");
                    return true;
                //-----------------------				
                case 38:
                    SetTargPending();
                    new EnergyVortexSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Energy Vortex");
                    return true;
                //-----------------------				
                case 39:
                    SetTargPending();
                    new BladeSpiritsSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Blade Spirits");
                    return true;
                //-----------------------				
                case 40:
                    new MirrorImage(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("Mirror Image");
                    return true;
            }
            return false;
        }


        //****************************************************************************************
        public Mobile BardTarget; //used for provoc target and such
        public DateTime lastbuffdebuff; //last buff/debuff
        int buffdebuffchoices = 0;


        //both buffs and debuff related actions - rare enough occurences, usually if we have lot of extra mana
        public bool DoBuffAction(bool fighting)
        {
            if (ShouldWait())
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 10 && fighting && !IsCasting())
            {
                actions.Add(1); //weaken
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 0.1 && m_Mobile.Mana >= 10 && fighting && !IsCasting())
            {
                actions.Add(2); //feeblemind
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 20 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(3); //Agility
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 20 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(4); //Cunning
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 20 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(5); //Strenght
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 30 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(5); //Bless
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 30 && m_Mobile.Mana >= 20 && fighting && !IsCasting())
            {
                actions.Add(6); //Curse		
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 30 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(7); //Mana drain	
            }
            //if(m_Mobile.Skills[SkillName.Magery].Value>30 && m_Mobile.Mana>=20 && fighting && !IsCasting() && m_Mobile.Body.IsHuman)actions.Add(8);//IncognitoSpell
            //if(m_Mobile.Skills[SkillName.Chivalry].Value>10 && RemoveCurseSpell.IsCursed(m_Mobile) && m_Mobile.Mana>=30 && !IsCasting())actions.Add(9);//remove curse
            if (m_Mobile.Skills[SkillName.Necromancy].Value > 25 && m_Mobile.Mana >= 30 && fighting && !IsCasting())
            {
                actions.Add(11);//corpse skin
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 80 && m_Mobile.Mana >= 40 && fighting && !IsCasting())
            {
                actions.Add(12);//mana vampire
            }

            if (m_Mobile.Backpack != null && m_Mobile.Backpack.GetAmount(typeof(BaseInstrument)) > 0 && fighting)
            {
                if (m_Mobile.Skills[SkillName.Discordance].Value > 0.1 && !IsCasting())
                {
                    actions.Add(13); //disco
                }

                if (m_Mobile.Skills[SkillName.Peacemaking].Value > 0.1 && !IsCasting())
                {
                    actions.Add(14); //peace
                }

                if (m_Mobile.Skills[SkillName.Provocation].Value > 0.1 && !IsCasting())
                {
                    actions.Add(15); //provoc
                }
            }


            if (actions.Count <= 0)
            {
                return false;
            }

            lastbuffdebuff = DateTime.UtcNow;

            buffdebuffchoices = actions.Count;

            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    new WeakenSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("weaken");
                    return true;
                //-----------------------				
                case 2:
                    SetTargPending();
                    new FeeblemindSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("feeblemind");
                    return true;
                //-----------------------					
                case 3:
                    SetTargPending();
                    new AgilitySpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("agility");
                    return true;
                //-----------------------					
                case 4:
                    SetTargPending();
                    new CunningSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("cunning");
                    return true;
                //-----------------------		
                case 5:
                    SetTargPending();
                    new StrengthSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("strength");
                    return true;
                //-----------------------				
                case 6:
                    SetTargPending();
                    new CurseSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("curse");
                    return true;
                //-----------------------						
                case 7:
                    SetTargPending();
                    new ManaDrainSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("mana drain");
                    return true;
                //-----------------------				
                case 8:
                    new IncognitoSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("Incognito");
                    return true;
                //-----------------------				
                case 9:
                    SetTargPending();
                    new RemoveCurseSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("remove curse");
                    return true;
                //-----------------------				
                case 11:
                    SetTargPending();
                    new CorpseSkinSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("corpse skin");
                    return true;
                //-----------------------				
                case 12:
                    SetTargPending();
                    new ManaVampireSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("mana vampire");
                    return true;
                //-----------------------					
                case 13:
                    SetTargPending();
                    m_Mobile.UseSkill(SkillName.Discordance);

                    m_Mobile.DebugSay("Discordance");
                    return true;
                //-----------------------					
                case 14:
                    SetTargPending();
                    m_Mobile.UseSkill(SkillName.Peacemaking);

                    m_Mobile.DebugSay("peacemaking");
                    return true;
                //-----------------------				
                case 15:
                    SetTargPending();

                    //we do the check here as to avoid wasting server resources only to not even use the skill

                    ArrayList targets = new ArrayList();
                    if (m_Mobile.Combatant == null)
                    {
                        return false;
                    }
                    /*
foreach (Mobile m in m_Mobile.Combatant.GetMobilesInRange(BaseInstrument.GetBardRange(m_Mobile, SkillName.Provocation)))
{
   if (m != m_Mobile.Combatant && m != m_Mobile && m_Mobile.Combatant.CanBeHarmful(m, false) && m is BaseCreature)
   {
       BaseCreature bc = m as BaseCreature;

       if (bc.BardImmune || bc.Unprovokable || bc.SummonMaster != null || bc.ControlMaster != null) continue;

       targets.Add(bc);
   }
}*/

                    if (targets.Count <= 0)
                    {
                        return false;
                    }

                    BardTarget = (Mobile)targets[Utility.Random(targets.Count)];

                    m_Mobile.UseSkill(SkillName.Provocation);

                    m_Mobile.DebugSay("provocation");
                    return true;
                    //-----------------------				
            }

            return false;
        }

        //****************************************************************************************		

        //anything that can dispel
        public bool DoDispelAction()
        {
            if (ShouldWait())
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Skills[SkillName.Magery].Value > 65 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(1); //dispel
            }

            if (m_Mobile.Skills[SkillName.Chivalry].Value > 45 && m_Mobile.Mana >= 20 && !IsCasting())
            {
                actions.Add(2); //dispel evil
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 119 && m_Mobile.Mana >= 50 && !IsCasting())
            {
                actions.Add(3); //mass dispel
            }

            if (actions.Count <= 0)
            {
                return false;
            }

            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    new DispelSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("dispel");
                    return true;
                //-----------------------		
                case 2:
                    new DispelEvilSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("dispel evil");
                    return true;
                //-----------------------					
                case 3:
                    SetTargPending();
                    new MassDispelSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("Mass dispel");
                    return true;
                    //-----------------------						
            }
            return false;
        }



        //****************************************************************************************		

        //Spells to finish an oponent - used when hp is low
        public bool DoFinishAction()
        {
            if (ShouldWait())
            {
                return false;
            }

            ArrayList actions = new ArrayList();

            if (m_Mobile.Skills[SkillName.Spellweaving].Value > 100 && m_Mobile.Mana >= 50 && !IsCasting())
            {
                actions.Add(0); //word of death	
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 60 && m_Mobile.Mana >= 17 && !IsCasting())
            {
                actions.Add(1); //poison strike
            }

            if (m_Mobile.Skills[SkillName.Necromancy].Value > 40 && m_Mobile.Mana >= 5 && !IsCasting())
            {
                actions.Add(2); //pain spike
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 70 && m_Mobile.Mana >= 5 && !IsCasting())
            {
                actions.Add(3); //lightning strike
            }

            if (m_Mobile.Skills[SkillName.Magery].Value > 10 && m_Mobile.Mana >= 9 && !IsCasting())
            {
                actions.Add(4); //fireball		
            }

            if (m_Mobile.Skills[SkillName.Bushido].Value > 70 && m_Mobile.Mana >= 5 && !IsCasting())
            {
                actions.Add(5); //honorable execution
            }

            if (actions.Count <= 0)
            {
                return false;
            }

            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 0:
                    SetTargPending();
                    new WordOfDeathSpell(m_Mobile, null).Cast();
                    m_Mobile.DebugSay("word of death");
                    return true;
                //-----------------------		
                case 1:
                    SetTargPending();
                    new PoisonStrikeSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("poison strike");
                    return true;
                //-----------------------		
                case 2:
                    SetTargPending();
                    new PainSpikeSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("pain spike");
                    return true;
                //-----------------------		
                case 3:
                    SpecialMove.SetCurrentMove(m_Mobile, new LightningStrike());

                    m_Mobile.DebugSay("lightning strike");
                    return true;
                //-----------------------					
                case 4:
                    SetTargPending();
                    new FireballSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("fire ball");
                    return true;
                //-----------------------			
                case 5:
                    SpecialMove.SetCurrentMove(m_Mobile, new HonorableExecution());

                    m_Mobile.DebugSay("honorable execution");
                    return true;
                    //-----------------------				



            }
            return false;
        }



        //****************************************************************************************




        public bool DoTeleportAction()
        {
            if (ShouldWait() || m_Mobile.CantWalk || m_Mobile.Frozen || m_Mobile.DisallowAllMoves)
            {
                return false;
            }

            if (!CanMelee() && m_Mobile.Combatant != null && m_Mobile.InRange(m_Mobile.Combatant, 7))
            {
                return false;
            }

            ArrayList actions = new ArrayList();


            if (m_Mobile.Skills[SkillName.Magery].Value > 50)
            {
                actions.Add(1); //teleport spell
            }

            if (m_Mobile.Skills[SkillName.Ninjitsu].Value > 75)
            {
                if (m_Mobile.Hidden)
                {
                    actions.Add(2); //shadow jump
                }
                else
                {
                    actions.Add(3); //smoke bomb
                }
            }


            if (actions.Count <= 0)
            {
                return false;
            }
            //do one of the actions
            switch ((int)actions[Utility.Random(actions.Count)])
            {
                case 1:
                    SetTargPending();
                    new TeleportSpell(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("teleport");
                    return true;
                //-----------------------	
                case 2:
                    SetTargPending();
                    new Shadowjump(m_Mobile, null).Cast();

                    m_Mobile.DebugSay("Shadow Jump");
                    return true;
                //-----------------------				
                case 3:
                    SmokeBomb bomb = new SmokeBomb();

                    bomb.OnDoubleClick(m_Mobile);
                    bomb.Delete(); //avoid keeping stuck in internal map

                    m_Mobile.DebugSay("smoke bomb ");
                    return true;
                    //-----------------------			
            }

            return false;
        }





        //------------------------------------------------------------------------------------------------------------------------------------




        public override bool DoActionWander()
        {
            if (m_Mobile.Meditating || (m_Mobile.Skills[SkillName.Stealth].Value < 50 && m_Mobile.Hidden && 0.9 > Utility.RandomDouble()))
            {
                return true; //if we're meditating, don't move		
            }

            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                m_Mobile.DebugSay("I am going to attack {0}", m_Mobile.FocusMob.Name);

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
                m_NextActTime = DateTime.UtcNow;
            }
            else
            {
                if (m_Mobile.Hits < (m_Mobile.HitsMax - 10) && 0.25 > Utility.RandomDouble())
                {
                    DoHealingAction(false);
                }
                else if (m_Mobile.Mana > (m_Mobile.ManaMax - 30) && 0.03 > Utility.RandomDouble())
                {
                    DoReveilAction();
                }
                else if (m_Mobile.Mana < m_Mobile.ManaMax && m_Mobile.Skills[SkillName.Meditation].Value > 10 && !m_Mobile.Meditating)
                {
                    m_Mobile.UseSkill(SkillName.Meditation);
                    m_Mobile.DebugSay("I am meditating");
                }
                else if (m_Mobile.Skills[SkillName.Hiding].Value >= 100 && m_Mobile.Skills[SkillName.Stealth].Value >= 100 && !m_Mobile.Hidden && 0.02 > Utility.RandomDouble()) //use smoke bomb when idle
                {
                    SmokeBomb bomb = new SmokeBomb();

                    bomb.OnDoubleClick(m_Mobile);

                    AfterHide();
                }
                else
                {
                    m_Mobile.DebugSay("I am wandering");
                }

                m_Mobile.Warmode = false;

                base.DoActionWander();
            }

            return true;
        }

        public void RunTo(Mobile m)
        {
            if (!m_Mobile.InRange(m, m_Mobile.RangeFight))
            {
                if (!MoveTo(m, true, 1))
                {
                    OnFailedMove();
                }
            }
            else if (m_Mobile.InRange(m, m_Mobile.RangeFight - 1))
            {
                RunFrom(m);
            }

        }

        public bool RunFrom(Mobile m)
        {
            if (m == null)
            {
                return false;
            }

            return Run((m_Mobile.GetDirectionTo(m) - 4) & Direction.Mask);
        }

        public void OnFailedMove()
        {
            if (CanDistanceFight() && m_Mobile.Combatant != null && m_Mobile.InLOS(m_Mobile.Combatant))
            {
                return;
            }

            if (0.2 > Utility.RandomDouble())
            {
                if (m_Mobile.Target != null)
                {
                    m_Mobile.Target.Cancel(m_Mobile, TargetCancelType.Canceled);
                }

                DoTeleportAction();

                m_Mobile.DebugSay("I am stuck, I'm going to try teleporting away");
            }
            else if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("My move is blocked, so I am going to attack {0}", m_Mobile.FocusMob.Name);
                }

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }
            else
            {
                m_Mobile.DebugSay("I am stuck");
            }

        }




        private DateTime NextTargChangeTry;
        private bool noaccmobs = false;

        public override bool DoActionCombat()
        {
            var c = m_Mobile.Combatant;
            m_Mobile.Warmode = true;

            //lets try to switch targets just for fun if we're not controlled
            if (NextTargChangeTry < DateTime.UtcNow && !m_Mobile.CombatLocked() && m_Mobile.TargetSwitchChance > 0 && m_Mobile.TargetSwitchChance > Utility.RandomDouble() && m_Mobile.ControlMaster == null && m_Mobile.SummonMaster == null && !m_Mobile.Controlled && !m_Mobile.Summoned && !m_Mobile.Allured && !Check4Target() && !HasSpecialMove())
            {
                if (AcquireFocusMob(m_Mobile.RangePerception, FightMode.Random, false, false, true, false))
                {
                    m_Mobile.DebugSay("Locking to target {0} out of {1} valid targets", m_Mobile.FocusMob, LastAcquireAmt);

                    if (m_Mobile.Combatant != null && m_Mobile.Combatant is BaseCreature)
                    {
                        ((BaseCreature)m_Mobile.Combatant).Combatant = null;
                    }

                    m_Mobile.LockCombat(m_Mobile.FocusMob, TimeSpan.FromSeconds(10 + Utility.Random(21)));
                    m_Mobile.FocusMob = null;

                    int div = LastAcquireAmt;
                    if (div < 1)
                    {
                        div = 1;
                    }
                    else if (div > 10)
                    {
                        div = 10;
                    }

                    NextTargChangeTry = DateTime.UtcNow + TimeSpan.FromSeconds(10 + Utility.Random(30 / div));
                    noaccmobs = false;
                }
                else
                {
                    NextTargChangeTry = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                    noaccmobs = true;
                }

            }



            if (c == null || c.Deleted || !c.Alive || ((Mobile)c).Hidden || ((Mobile)c).IsDeadBondedPet || !m_Mobile.CanSee(c) || !m_Mobile.CanBeHarmful(c, false) || c.Map != m_Mobile.Map)
            {
                // Our combatant is deleted, dead, hidden, or we cannot hurt them
                // Try to find another combatant

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.DebugSay("Something happened to my combatant, so I am going to fight {0}", m_Mobile.FocusMob.Name);
                    }

                    m_Mobile.Combatant = c = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                }
                else
                {
                    m_Mobile.DebugSay("Something happened to my combatant, and nothing is around. I am on guard.");
                    Action = ActionType.Guard;
                    return true;
                }
            }

            if (!m_Mobile.InRange(c, m_Mobile.RangePerception))
            {
                // They are somewhat far away, can we find something else?

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    m_Mobile.Combatant = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                }
                else if (!m_Mobile.InRange(c, m_Mobile.RangePerception * 3))
                {
                    m_Mobile.Combatant = null;
                }

                c = m_Mobile.Combatant;

                if (c == null)
                {
                    m_Mobile.DebugSay("My combatant has fled, so I am on guard");
                    Action = ActionType.Guard;

                    return true;
                }
            }





            if (!m_Mobile.InLOS(c))
            {

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    m_Mobile.Combatant = c = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                    m_Mobile.DebugSay("Combatant no longer inLOS - found another mob");
                    return true;
                }

            }
            //TODO: come up with an algorthm to walk around walls more efficiently then it does now



            if (!m_Mobile.Controlled && !m_Mobile.Summoned && !m_Mobile.IsParagon && !m_Mobile.NoFlee && !m_Mobile.BardProvoked)
            {
                if (m_Mobile.Hits < m_Mobile.HitsMax * 20 / 100)
                {
                    // We are low on health, should we flee?

                    bool flee = false;

                    if (m_Mobile.Hits < c.Hits)
                    {
                        // We are more hurt than them

                        int diff = c.Hits - m_Mobile.Hits;

                        flee = true;
                    }
                    else
                    {
                        flee = Utility.Random(0, 30) > 10; // 10% chance to flee
                    }

                    if (flee)
                    {
                        if (m_Mobile.Debug)
                        {
                            m_Mobile.DebugSay("I am going to flee from {0}", c.Name);
                        }

                        RunFrom((Mobile)c);

                        Action = ActionType.Flee;
                        return true;
                    }
                }
            }

            if (m_Mobile.InRange(c, 1) && !CanMelee() && 0.3 > Utility.RandomDouble())//check if we are too close and should not be - PHEAR TEH NOOB DEXXARS!!11!
            {
                CheckRun((Mobile)c);
                SetWait(Utility.Random(3, 5));
                return true;
            }


            if (m_Mobile.ControlMaster != null && !m_Mobile.CanSee(m_Mobile.ControlMaster) && 0.2 > Utility.RandomDouble())
            {
                if (m_Mobile.Combatant != null && 0.35 > Utility.RandomDouble())
                {
                    RunFrom((Mobile)m_Mobile.Combatant);
                }
                else if (0.1 > Utility.RandomDouble())
                {
                    if (m_Mobile.Combatant != null)
                    {
                        //m_Mobile.Combatant.Combatant = null;
                        m_Mobile.Combatant = null;
                    }
                    DoTeleportAction();
                }
                else
                {
                    DoDefensiveAction(false);
                }

                m_Mobile.ControlOrder = OrderType.None;
                m_Mobile.DebugSay("My master is nowhere, I'll keep defending myself and slack off a bit");
                return true;
            }




            if (!ShouldWait() && m_Mobile.InRange(c, m_Mobile.RangePerception) && m_Mobile.InLOS(c))
            {
                // We are ready to do something

                Mobile toDispel = FindDispelTarget(true);

                bool didaction = false;

                //typecasting FTW
                double percentmana = m_Mobile.ManaPerc / (double)100;
                double percenthits = m_Mobile.HitsPerc / (double)100;


                if (m_Mobile.Mana >= 500)
                {
                    percentmana = 99; //default % to high value if we got 100 or more mana
                }

                if (m_Mobile.Hits >= 1000)
                {
                    percenthits = 99; //default % to high value if we got 1000 or more hits
                }


                //check if they're far, but not too far.  maybe we can try to teleport to them
                if (!m_Mobile.InRange(c, 6) && m_Mobile.InLOS(c) && 0.02 > Utility.RandomDouble())
                {
                    didaction = DoTeleportAction();
                }

                if (toDispel != null && 0.6 > Utility.RandomDouble()) // Something dispellable is attacking
                {
                    didaction = DoDispelAction();
                }

                //If we can't find anyone else to possibly attack and that we'er attacking a basecreature, look if there may be a hidden tamer
                if (noaccmobs && 0.4 > Utility.RandomDouble() && m_Mobile.Combatant != null && m_Mobile.Combatant is BaseCreature)
                {
                    didaction = DoReveilAction();
                }

                //FINISH HIM!  		*street fighter pin ball machine voice*  You cannot win if you walk away! Insert another quarter!  
                if (m_Mobile.Combatant != null && m_Mobile.Combatant.Hits < 10 + Utility.Random(30) && 0.75 > Utility.RandomDouble())
                {
                    didaction = DoFinishAction(); //KO!
                }

                if (!didaction && percenthits < (40 + Utility.Random(40)) && 0.55 > Utility.RandomDouble()) // should cure/heal or do defend action
                {
                    if (0.4 > Utility.RandomDouble())
                    {
                        DoBuffAction(m_Mobile.Combatant != null);
                    }

                    if (!didaction && 0.43 > Utility.RandomDouble())
                    {
                        didaction = DoHealingAction(false);
                    }

                    if (!didaction)
                    {
                        didaction = DoDefensiveAction(m_Mobile.Combatant != null);
                    }
                }


                if ((!didaction && percentmana > 20 && ((lastbuffdebuff + TimeSpan.FromSeconds(25)) < DateTime.UtcNow && (0.065 * buffdebuffchoices) > Utility.RandomDouble()) || 0.15 > Utility.RandomDouble())) //do a buff/debuf spell
                {
                    didaction = DoBuffAction(m_Mobile.Combatant != null);
                }


                if (!didaction && 0.85 > Utility.RandomDouble()) //deal offensive action
                {
                    didaction = DoOffensiveAction();
                }
                else if (percentmana > 30 && 0.38 > Utility.RandomDouble())
                {
                    didaction = DoReveilAction();
                }

                int chancetodelay = 100 + m_Mobile.SuperAiIntensity;
                if (m_Mobile.IsSupreme || m_Mobile.IsParagon)
                {
                    chancetodelay += 500;
                }

                if (Utility.Random(chancetodelay) < (100 - percentmana))
                {

                    if (percentmana > 40 || m_Mobile.Mana > 100 || 0.27 > Utility.RandomDouble())
                    {
                        SetWait(1 + Utility.Random(Utility.Random(m_Mobile.SuperAiDelayMax) + 1 + (int)(5 - (percentmana / 20))));  //random delay between doing an action, bigger chance if our mana is low
                    }
                    else if (percentmana < 10)
                    {
                        SetWait(21 + Utility.Random(25));
                    }
                }
                else if (m_Mobile.SuperAiDelayMax > 0 && Utility.RandomBool())
                {
                    SetWait(1 + Utility.Random(m_Mobile.SuperAiDelayMax));
                }

                CheckRun((Mobile)c);
            }
            else if (m_Mobile.Spell == null || !m_Mobile.Spell.IsCasting)
            {
                CheckRun((Mobile)c);
            }

            return true;
        }

        public override bool DoActionGuard()
        {
            m_Mobile.DebugSay("I am on guard");
            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("I am going to attack {0}", m_Mobile.FocusMob.Name);
                }

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }


            if (m_Mobile.Hits < (m_Mobile.HitsMax - 10) & 0.6 > Utility.RandomDouble())
            {
                DoHealingAction(false);
            }
            else if (0.1 > Utility.RandomDouble() && (m_Mobile.HitsPerc < 10 || m_Mobile.Hits < 30))
            {
                DoTeleportAction();
            }
            else if (m_Mobile.Mana > (m_Mobile.ManaMax - 30) && 0.1 > Utility.RandomDouble())
            {
                DoReveilAction();
            }

            return base.DoActionGuard();
        }


        public override bool DoActionFlee()
        {
            var c = m_Mobile.Combatant;

            if (c == null)
            {
                c = (Mobile)m_Mobile.FocusMob;
            }

            RunFrom((Mobile)c);


            if ((m_Mobile.Mana >= m_Mobile.ManaMax / 5) && (m_Mobile.Hits >= m_Mobile.HitsMax / 3))
            {
                m_Mobile.DebugSay("I am stronger now, my guard is up");
                Action = ActionType.Guard;
            }
            else if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("I am scared of {0}", m_Mobile.FocusMob.Name);
                }
            }

            if (0.01 > Utility.RandomDouble())
            {
                DoTeleportAction();
            }

            if (m_Mobile.Hits < m_Mobile.HitsMax && 0.02 > Utility.RandomDouble())
            {
                bool didaction = false;
                if (0.25 > Utility.RandomDouble())
                {
                    didaction = DoHealingAction(false);
                }

                if (!didaction)
                {
                    didaction = DoDefensiveAction(false);
                }
            }

            return true;
        }

        public Mobile FindDispelTarget(bool activeOnly)
        {
            if (m_Mobile.Deleted || m_Mobile.Int < 95 || CanDispel(m_Mobile) || m_Mobile.AutoDispel)
            {
                return null;
            }

            if (activeOnly)
            {
                List<AggressorInfo> aggressed = m_Mobile.Aggressed;
                List<AggressorInfo> aggressors = m_Mobile.Aggressors;

                Mobile active = null;
                double activePrio = 0.0;

                var comb = m_Mobile.Combatant;

                if (comb != null && !comb.Deleted && comb.Alive && !((Mobile)comb).IsDeadBondedPet && m_Mobile.InRange(comb, 12) && CanDispel((Mobile)comb))
                {
                    active = (Mobile)comb;
                    activePrio = m_Mobile.GetDistanceToSqrt(comb);

                    if (activePrio <= 2)
                    {
                        return active;
                    }
                }

                for (int i = 0; i < aggressed.Count; ++i)
                {
                    AggressorInfo info = aggressed[i];
                    Mobile m = info.Defender;

                    if (m != comb && m.Combatant == m_Mobile && m_Mobile.InRange(m, 12) && CanDispel(m))
                    {
                        double prio = m_Mobile.GetDistanceToSqrt(m);

                        if (active == null || prio < activePrio)
                        {
                            active = m;
                            activePrio = prio;

                            if (activePrio <= 2)
                            {
                                return active;
                            }
                        }
                    }
                }

                for (int i = 0; i < aggressors.Count; ++i)
                {
                    AggressorInfo info = aggressors[i];
                    Mobile m = info.Attacker;

                    if (m != comb && m.Combatant == m_Mobile && m_Mobile.InRange(m, 12) && CanDispel(m))
                    {
                        double prio = m_Mobile.GetDistanceToSqrt(m);

                        if (active == null || prio < activePrio)
                        {
                            active = m;
                            activePrio = prio;

                            if (activePrio <= 2)
                            {
                                return active;
                            }
                        }
                    }
                }

                return active;
            }
            else
            {
                Map map = m_Mobile.Map;

                if (map != null)
                {
                    Mobile active = null, inactive = null;
                    double actPrio = 0.0, inactPrio = 0.0;

                    var comb = m_Mobile.Combatant;

                    if (comb != null && !comb.Deleted && comb.Alive && !((Mobile)comb).IsDeadBondedPet && CanDispel((Mobile)comb))
                    {
                        active = inactive = (Mobile)comb;
                        actPrio = inactPrio = m_Mobile.GetDistanceToSqrt(comb);
                    }

                    foreach (Mobile m in m_Mobile.GetMobilesInRange(12))
                    {
                        if (m != m_Mobile && CanDispel(m))
                        {
                            double prio = m_Mobile.GetDistanceToSqrt(m);

                            if (!activeOnly && (inactive == null || prio < inactPrio))
                            {
                                inactive = m;
                                inactPrio = prio;
                            }

                            if ((m_Mobile.Combatant == m || m.Combatant == m_Mobile) && (active == null || prio < actPrio))
                            {
                                active = m;
                                actPrio = prio;
                            }
                        }
                    }

                    return active != null ? active : inactive;
                }
            }

            return null;
        }

        public bool CanDispel(Mobile m)
        {
            return (m is BaseCreature && ((BaseCreature)m).Summoned && m_Mobile.CanBeHarmful(m, false) && !((BaseCreature)m).IsAnimatedDead);
        }

        private static readonly int[] m_Offsets = new int[]
            {
                -1, -1,
                -1,  0,
                -1,  1,
                 0, -1,
                 0,  1,
                 1, -1,
                 1,  0,
                 1,  1,

                -2, -2,
                -2, -1,
                -2,  0,
                -2,  1,
                -2,  2,
                -1, -2,
                -1,  2,
                 0, -2,
                 0,  2,
                 1, -2,
                 1,  2,
                 2, -2,
                 2, -1,
                 2,  0,
                 2,  1,
                 2,  2
            };

        public void ProcessTarget(Target targ)
        {
            m_Mobile.DebugSay("target");
            bool isDispel = (targ is DispelSpell.InternalTarget || targ is MassDispelSpell.InternalTarget);
            //bool isParalyze = ( targ is ParalyzeSpell.InternalTarget );
            bool isTeleport = (targ is TeleportSpell.InternalTarget);
            bool isReveal = (targ is RevealSpell.InternalTarget || targ is SkillHandlers.DetectHidden.DetectHiddenTarget);
            bool teleportAway = false;

            var toTarget = m_Mobile.Combatant;


            //barding
            if (targ is SkillHandlers.Provocation.InternalFirstTarget && BardTarget != null)
            {
                m_Mobile.DebugSay("provocing {0}...", BardTarget.Name);
                targ.Invoke(m_Mobile, BardTarget);
                SetTargPending();//another target is coming so we set it now
                return;
            }
            else if (targ is SkillHandlers.Provocation.InternalSecondTarget && toTarget != null)
            {
                m_Mobile.DebugSay("provocing {0} to {1}", BardTarget.Name, toTarget);
                targ.Invoke(m_Mobile, toTarget);
                return;
            }


            if (isDispel)
            {
                toTarget = FindDispelTarget(false);

                if (!SmartAI && toTarget != null)
                {
                    RunTo((Mobile)toTarget);
                }
                else if (toTarget != null && m_Mobile.InRange(toTarget, 10))
                {
                    RunFrom((Mobile)toTarget);
                }
            }
            else if (isReveal) //target random land tile
            {
                Map map = m_Mobile.Map;
                int detectrange = targ.Range - 2;

                for (int i = 0; i < 30; ++i)
                {
                    Point3D randomPoint = new Point3D(m_Mobile.X - detectrange + Utility.Random(detectrange * 2 + 1), m_Mobile.Y - detectrange + Utility.Random(detectrange * 2 + 1), 0);

                    LandTarget lt = new LandTarget(randomPoint, map);

                    if (m_Mobile.InLOS(lt))
                    {
                        targ.Invoke(m_Mobile, new LandTarget(randomPoint, map));
                        return;
                    }
                }

                targ.Cancel(m_Mobile, TargetCancelType.Canceled);
            }
            else if (isTeleport)
            {
                toTarget = m_Mobile.Combatant;

                if (toTarget != null)
                {
                    teleportAway = (m_Mobile.Hits < 50 && toTarget.Hits > 50) || Action == ActionType.Flee;
                }

            }
            else
            {
                toTarget = m_Mobile.Combatant;

                if (toTarget != null)
                {
                    CheckRun((Mobile)toTarget);
                }
            }

            if ((targ.Flags & TargetFlags.Harmful) != 0 && toTarget != null)
            {
                if ((targ.Range == -1 || m_Mobile.InRange(toTarget, targ.Range)) && m_Mobile.CanSee(toTarget) && m_Mobile.InLOS(toTarget))
                {
                    targ.Invoke(m_Mobile, toTarget);
                }
                else if (isDispel)
                {
                    targ.Cancel(m_Mobile, TargetCancelType.Canceled);
                }
            }
            else if ((targ.Flags & TargetFlags.Beneficial) != 0)
            {
                targ.Invoke(m_Mobile, m_Mobile);
            }
            else if (!isTeleport && toTarget != null)
            {
                targ.Invoke(m_Mobile, toTarget);
            }
            else if (isTeleport)
            {
                Map map = m_Mobile.Map;

                if (map == null)
                {
                    targ.Cancel(m_Mobile, TargetCancelType.Canceled);
                    return;
                }

                int px, py;

                if (teleportAway && toTarget != null)
                {
                    int rx = m_Mobile.X - toTarget.X;
                    int ry = m_Mobile.Y - toTarget.Y;

                    double d = m_Mobile.GetDistanceToSqrt(toTarget);

                    px = toTarget.X + (int)(rx * (10 / d));
                    py = toTarget.Y + (int)(ry * (10 / d));
                }
                else if (toTarget != null)
                {
                    px = toTarget.X;
                    py = toTarget.Y;
                }
                else
                {
                    int range = 7;
                    if (targ != null)
                    {
                        range = targ.Range;
                    }

                    px = range - Utility.Random(range) + m_Mobile.X;
                    py = range - Utility.Random(range) + m_Mobile.Y;
                }

                for (int i = 0; i < m_Offsets.Length; i += 2)
                {
                    int x = m_Offsets[i], y = m_Offsets[i + 1];

                    Point3D p = new Point3D(px + x, py + y, 0);

                    LandTarget lt = new LandTarget(p, map);

                    if ((targ.Range == -1 || m_Mobile.InRange(p, targ.Range)) && m_Mobile.InLOS(lt) && map.CanSpawnMobile(px + x, py + y, lt.Z) && !SpellHelper.CheckMulti(p, map))
                    {
                        targ.Invoke(m_Mobile, lt);
                        return;
                    }
                }

                int teleRange = targ.Range;

                if (teleRange < 0)
                {
                    teleRange = 12;
                }

                for (int i = 0; i < 10; ++i)
                {
                    Point3D randomPoint = new Point3D(m_Mobile.X - teleRange + Utility.Random(teleRange * 2 + 1), m_Mobile.Y - teleRange + Utility.Random(teleRange * 2 + 1), 0);

                    LandTarget lt = new LandTarget(randomPoint, map);

                    if (m_Mobile.InLOS(lt) && map.CanSpawnMobile(lt.X, lt.Y, lt.Z) && !SpellHelper.CheckMulti(randomPoint, map))
                    {
                        targ.Invoke(m_Mobile, new LandTarget(randomPoint, map));
                        return;
                    }
                }

                targ.Cancel(m_Mobile, TargetCancelType.Canceled);
            }
            else
            {
                targ.Cancel(m_Mobile, TargetCancelType.Canceled);
            }

        }
    }
}
