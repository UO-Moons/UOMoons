using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Items
{
    public class Bladeweave : WeaponAbility
    {
        private class BladeWeaveRedirect
        {
            public readonly WeaponAbility NewAbility;
            public readonly int ClilocEntry;

            public BladeWeaveRedirect(WeaponAbility ability, int cliloc)
            {
                NewAbility = ability;
                ClilocEntry = cliloc;
            }
        }

        private static readonly Dictionary<Mobile, BladeWeaveRedirect> m_NewAttack = new Dictionary<Mobile, BladeWeaveRedirect>();

        public static bool BladeWeaving(Mobile attacker, out WeaponAbility a)
        {
            var success = m_NewAttack.TryGetValue(attacker, out var bwr);
            a = success ? bwr.NewAbility : null;

            return success;
        }

        public Bladeweave()
        {
        }

        public override int BaseMana => 30;

        public override bool OnBeforeSwing(Mobile attacker, Mobile defender)
        {
            if (!Validate(attacker) || !CheckMana(attacker, false))
                return false;

            int ran;
            if (attacker is BaseCreature creature /*&& PetTrainingHelper.CheckSecondarySkill(creature, SkillName.Bushido)*/)
            {
                ran = Utility.Random(9);
            }
            else
            {
                var canfeint = attacker.Skills[Feint.GetSecondarySkill(attacker)].Value >= Feint.GetRequiredSecondarySkill(attacker);
                var canblock = attacker.Skills[Block.GetSecondarySkill(attacker)].Value >= Block.GetRequiredSecondarySkill(attacker);

                if (canfeint && canblock)
                {
                    ran = Utility.Random(9);
                }
                else if (canblock)
                {
                    ran = Utility.Random(8);
                }
                else
                {
                    ran = Utility.RandomList(0, 1, 2, 3, 4, 5, 6, 8);
                }
            }

            switch (ran)
            {
                case 0:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(ArmorIgnore, 1028838);
                    break;
                case 1:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(BleedAttack, 1028839);
                    break;
                case 2:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(ConcussionBlow, 1028840);
                    break;
                case 3:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(CrushingBlow, 1028841);
                    break;
                case 4:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(DoubleStrike, 1028844);
                    break;
                case 5:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(MortalStrike, 1028846);
                    break;
                case 6:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(ParalyzingBlow, 1028848);
                    break;
                case 7:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(Block, 1028853);
                    break;
                case 8:
                    m_NewAttack[attacker] = new BladeWeaveRedirect(Feint, 1028857);
                    break;
                default:
                    // should never happen
                    return false;
            }

            return m_NewAttack[attacker].NewAbility.OnBeforeSwing(attacker, defender);
        }

        public override bool OnBeforeDamage(Mobile attacker, Mobile defender)
        {
            if (m_NewAttack.TryGetValue(attacker, out var bwr))
                return bwr.NewAbility.OnBeforeDamage(attacker, defender);
            else
                return base.OnBeforeDamage(attacker, defender);
        }

        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (CheckMana(attacker, false))
            {
                if (m_NewAttack.TryGetValue(attacker, out var bwr))
                {
                    attacker.SendLocalizedMessage(1072841, "#" + bwr.ClilocEntry.ToString());
                    bwr.NewAbility.OnHit(attacker, defender, damage);
                }
                else
                    base.OnHit(attacker, defender, damage);

                attacker.PlaySound(0x5BC);
                m_NewAttack.Remove(attacker);
                ClearCurrentAbility(attacker);
            }
        }

        public override void OnMiss(Mobile attacker, Mobile defender)
        {
            if (m_NewAttack.TryGetValue(attacker, out var bwr))
                bwr.NewAbility.OnMiss(attacker, defender);
            else
                base.OnMiss(attacker, defender);

            m_NewAttack.Remove(attacker);
        }
    }
}
