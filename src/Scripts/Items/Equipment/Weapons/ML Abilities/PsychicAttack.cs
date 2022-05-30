using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class PsychicAttack : WeaponAbility
    {
        public PsychicAttack()
        {
        }

        public override int BaseMana => 30;

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker) || !CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            attacker.SendLocalizedMessage(1074383); // Your shot sends forth a wave of psychic energy.
            defender.SendLocalizedMessage(1074384); // Your mind is attacked by psychic force!

            defender.FixedParticles(0x3789, 10, 25, 5032, EffectLayer.Head);
            defender.PlaySound(0x1F8);

            if (Registry.ContainsKey(defender))
            {
                if (!Registry[defender].DoneIncrease)
                {
                    Registry[defender].SpellDamageMalus *= 2;
                    Registry[defender].ManaCostMalus *= 2;
                }
            }
            else
                Registry[defender] = new PsychicAttackTimer(defender);

            BuffInfo.RemoveBuff(defender, BuffIcon.PsychicAttack);

            var args = $"{Registry[defender].SpellDamageMalus}\t{Registry[defender].ManaCostMalus}";
            BuffInfo.AddBuff(defender, new BuffInfo(BuffIcon.PsychicAttack, 1151296, 1151297, args));
        }

        public static Dictionary<Mobile, PsychicAttackTimer> Registry { get; } = new Dictionary<Mobile, PsychicAttackTimer>();

        public static void RemoveEffects(Mobile defender)
        {
            if (defender == null)
                return;

            BuffInfo.RemoveBuff(defender, BuffIcon.PsychicAttack);

            if (Registry.ContainsKey(defender))
                Registry.Remove(defender);

            defender.SendLocalizedMessage(1150292); // You recover from the effects of the psychic attack.
        }

        public class PsychicAttackTimer : Timer
        {
            private readonly Mobile m_Defender;
            private int m_SpellDamageMalus;
            private int m_ManaCostMalus;

            public int SpellDamageMalus { get => m_SpellDamageMalus; set { m_SpellDamageMalus = value; DoneIncrease = true; } }
            public int ManaCostMalus { get => m_ManaCostMalus; set { m_ManaCostMalus = value; DoneIncrease = true; } }
            public bool DoneIncrease { get; private set; }

            public PsychicAttackTimer(Mobile defender)
                : base(TimeSpan.FromSeconds(10))
            {
                m_Defender = defender;
                m_SpellDamageMalus = 15;
                m_ManaCostMalus = 15;
                DoneIncrease = false;
                Start();
            }

            protected override void OnTick()
            {
                RemoveEffects(m_Defender);
                Stop();
            }
        }
    }
}
