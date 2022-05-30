using System;
using System.Collections.Generic;
using System.Linq;
using Server.Spells;

namespace Server.Items
{
    public class ForceArrow : WeaponAbility
    {
        public ForceArrow()
        {
        }

        public override int BaseMana => 20;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1074381); // You fire an arrow of pure force.
            defender.SendLocalizedMessage(1074382); // You are struck by a force arrow!

            if (.4 > Utility.RandomDouble())
            {
                defender.Combatant = null;
                defender.Warmode = false;
            }

            ForceArrowInfo info = GetInfo(attacker, defender);

            if (info == null)
                BeginForceArrow(attacker, defender);
            else
            {
                if (info.Timer != null && info.Timer.Running)
                {
                    info.Timer.IncreaseExpiration();

                    BuffInfo.RemoveBuff(defender, BuffIcon.ForceArrow);
                    BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ForceArrow, 1151285, 1151286, info.DefenseChanceMalus.ToString()));
                }
            }


            if (defender.Spell is Spell spell && spell.IsCasting)
                spell.Disturb(DisturbType.Hurt, false, true);
        }

        private static readonly Dictionary<Mobile, List<ForceArrowInfo>> m_Table = new Dictionary<Mobile, List<ForceArrowInfo>>();

        public static void BeginForceArrow(Mobile attacker, Mobile defender)
        {
            var info = new ForceArrowInfo(attacker, defender);
            info.Timer = new ForceArrowTimer(info);

            if (!m_Table.ContainsKey(attacker))
                m_Table[attacker] = new List<ForceArrowInfo>();

            m_Table[attacker].Add(info);

            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.ForceArrow, 1151285, 1151286, info.DefenseChanceMalus.ToString()));
        }

        public static void EndForceArrow(ForceArrowInfo info)
        {
            if (info == null)
                return;

            Mobile attacker = info.Attacker;

            if (m_Table.ContainsKey(attacker) && m_Table[attacker].Contains(info))
            {
                m_Table[attacker].Remove(info);

                if (m_Table[attacker].Count == 0)
                    m_Table.Remove(attacker);
            }

            BuffInfo.RemoveBuff(info.Defender, BuffIcon.ForceArrow);
        }

        public static bool HasForceArrow(Mobile attacker, Mobile defender)
        {
            if (!m_Table.ContainsKey(attacker))
                return false;

            return m_Table[attacker].Any(info => info.Defender == defender);
        }

        public static ForceArrowInfo GetInfo(Mobile attacker, Mobile defender)
        {
            if (!m_Table.ContainsKey(attacker))
                return null;

            foreach (ForceArrowInfo info in m_Table[attacker])
            {
                if (info.Defender == defender)
                    return info;
            }

            return null;
        }

        public class ForceArrowInfo
        {
            public Mobile Attacker { get; }
            public Mobile Defender { get; }
            public ForceArrowTimer Timer { get; set; }
            public int DefenseChanceMalus { get; set; }

            public ForceArrowInfo(Mobile attacker, Mobile defender)
            {
                Attacker = attacker;
                Defender = defender;
                DefenseChanceMalus = 10;
            }
        }

        public class ForceArrowTimer : Timer
        {
            private readonly ForceArrowInfo m_Info;
            private DateTime m_Expires;

            public ForceArrowTimer(ForceArrowInfo info)
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1))
            {
                m_Info = info;
                Priority = TimerPriority.OneSecond;

                m_Expires = DateTime.UtcNow + TimeSpan.FromSeconds(10);

                Start();
            }

            protected override void OnTick()
            {
                switch (m_Expires < DateTime.UtcNow)
                {
                    case true:
                        Stop();
                        EndForceArrow(m_Info);
                        break;
                }
            }

            public void IncreaseExpiration()
            {
                m_Expires += TimeSpan.FromSeconds(2);

                m_Info.DefenseChanceMalus += 5;
            }
        }
    }
}
