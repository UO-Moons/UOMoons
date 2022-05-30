using Server.Items;

namespace Server.Mobiles
{
    public class AdvancedArcherAI : BaseAI
    {
        private bool WasTooNear;
        private bool WasHiding;
        private bool IsSetUp;

        public AdvancedArcherAI(BaseCreature m)
            : base(m)
        {
        }

        public override bool DoActionWander()
        {
            m_Mobile.DebugSay("I have no combatant");

            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                    m_Mobile.DebugSay("I have detected {0} and I will attack", m_Mobile.FocusMob.Name);

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }
            else
            {
                return base.DoActionWander();
            }

            return true;
        }

        public override bool DoActionCombat()
        {
            IDamageable combatant = m_Mobile.Combatant;
            if (combatant == null || combatant.Deleted || m_Mobile.Deleted ||
                !combatant.Alive || !m_Mobile.Alive || !m_Mobile.CanSee(combatant) || (combatant is Mobile && ((Mobile)combatant).IsDeadBondedPet) ||
                m_Mobile.IsDeadBondedPet)
            {
                m_Mobile.DebugSay("My combatant is deleted");
                Action = ActionType.Guard;
                return true;
            }
            if (m_Mobile.Combatant != null)
            {
                //RANGE HERE
                int maxrange = m_Mobile.Weapon.MaxRange;
                //minrange is divided by 2 when used
                int minrange = m_Mobile.RangeFight * 2;
                if (minrange < m_Mobile.Weapon.MaxRange)
                {
                    //don't fight too close
                    minrange = m_Mobile.Weapon.MaxRange;
                }
                if (maxrange > m_Mobile.RangePerception)
                {
                    //can't fight if you can't see
                    maxrange = m_Mobile.RangePerception;
                    minrange = maxrange;
                }
                else if (minrange > m_Mobile.RangePerception)
                {
                    //can't fight if you can't see
                    minrange = m_Mobile.RangePerception;
                }
                if (maxrange < 1)
                {
                    //can't fight on top
                    maxrange = 1;
                    minrange = 2;
                }
                else if (minrange < 2)
                {
                    //can't fight on top
                    minrange = 2;
                }
                int absminrange = minrange;
                if (absminrange > 4)
                    absminrange = 4;
                //RANGE STOP
                //DECIDE ACTION
                if ((int)m_Mobile.GetDistanceToSqrt(m_Mobile.Combatant) >= maxrange)
                {
                    WasTooNear = false;
                    WasHiding = false;
                    IsSetUp = false;
                    if ((int)m_Mobile.GetDistanceToSqrt(m_Mobile.Combatant) > m_Mobile.RangePerception + 1)
                    {
                        if (m_Mobile.Debug)
                            m_Mobile.DebugSay("I have lost {0}", m_Mobile.Combatant.Name);

                        m_Mobile.Combatant = null;
                        Action = ActionType.Guard;
                        return true;
                    }
                }
                else if ((int)m_Mobile.GetDistanceToSqrt(m_Mobile.Combatant) <= minrange / 2)
                {
                    if (WasHiding == true)
                    {
                        if ((int)m_Mobile.GetDistanceToSqrt(m_Mobile.Combatant) >= absminrange / 2)
                        {
                            WasHiding = false;
                            IsSetUp = true;
                        }
                    }
                    else if (IsSetUp == true)
                    {
                        if ((int)m_Mobile.GetDistanceToSqrt(m_Mobile.Combatant) < absminrange / 2)
                        {
                            WasTooNear = true;
                            IsSetUp = false;
                        }
                    }
                    else
                    {
                        WasTooNear = true;
                    }

                }
                else if (IsSetUp == true)
                {
                    WasTooNear = false;
                    IsSetUp = false;
                }
                else
                {
                    WasHiding = false;
                }

                //ACTION DECIDED
                //TAKE ACTION
                if (!m_Mobile.InLOS(m_Mobile.Combatant))
                {
                    if (m_Mobile.Debug)
                        m_Mobile.DebugSay("I can't see {0}", m_Mobile.Combatant.Name);
                    WalkMobileRange(m_Mobile.Combatant, 1, true, 1, 1);
                    WasHiding = true;
                    IsSetUp = false;
                }
                else if (IsSetUp == true)
                {
                    if (m_Mobile.Debug)
                        m_Mobile.DebugSay("I'm fighting the chicken {0}", m_Mobile.Combatant.Name);
                    WalkMobileRange(m_Mobile.Combatant, 1, true, absminrange / 2, maxrange);
                }
                else if (WasHiding == true)
                {
                    if (m_Mobile.Debug)
                        m_Mobile.DebugSay("I found {0} !", m_Mobile.Combatant.Name);
                    WalkMobileRange(m_Mobile.Combatant, 1, true, absminrange / 2, minrange / 2);
                }
                else if (WasTooNear == true)
                {
                    if (m_Mobile.Debug)
                        m_Mobile.DebugSay("I am too near {0}", m_Mobile.Combatant.Name);
                    WalkMobileRange(m_Mobile.Combatant, 1, true, maxrange, maxrange + 1);
                }
                else
                {
                    if (m_Mobile.Debug)
                        m_Mobile.DebugSay("I am not too near {0}", m_Mobile.Combatant.Name);
                    WalkMobileRange(m_Mobile.Combatant, 1, true, minrange / 2, maxrange);
                    // Be sure to face the combatant
                    m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant.Location);
                }
                if (Core.TickCount - m_Mobile.LastMoveTime > 1000)
                {
                    // Be sure to face the combatant
                    m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant.Location);
                }
                //STOP ACTION
            }

            // When we have no ammo, we flee
            Container pack = m_Mobile.Backpack;

            if (pack == null || pack.FindItemByType(typeof(Arrow)) == null)
            {
                Action = ActionType.Flee;
                return true;
            }


            // At 20% we should check if we must leave
            if (m_Mobile.Hits < m_Mobile.HitsMax * 20 / 100 && m_Mobile.CanFlee)
            {
                bool bFlee = false;
                // if my current hits are more than my opponent, i don't care
                if (m_Mobile.Combatant != null && m_Mobile.Hits < m_Mobile.Combatant.Hits)
                {
                    int iDiff = m_Mobile.Combatant.Hits - m_Mobile.Hits;

                    if (Utility.Random(0, 100) > 10 + iDiff) // 10% to flee + the diff of hits
                    {
                        bFlee = true;
                    }
                }
                else if (m_Mobile.Combatant != null && m_Mobile.Hits >= m_Mobile.Combatant.Hits)
                {
                    if (Utility.Random(0, 100) > 10) // 10% to flee
                    {
                        bFlee = true;
                    }
                }

                if (bFlee)
                {
                    Action = ActionType.Flee;
                }
            }

            return true;
        }

        public override bool DoActionGuard()
        {
            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                    m_Mobile.DebugSay("I have detected {0}, attacking", m_Mobile.FocusMob.Name);

                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }
            else
            {
                base.DoActionGuard();
            }

            return true;
        }
    }
}
