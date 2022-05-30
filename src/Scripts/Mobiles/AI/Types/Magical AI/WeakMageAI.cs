using System.Linq;
using Server.Spells;
using Server.Spells.Necromancy;
using Server.Items;

namespace Server.Mobiles
{
    public class WeakMageAI : MageAI
    {
        private const double ChanceToUseNecroSpells = 0.5;

        public WeakMageAI(BaseCreature m)
            : base(m)
        {
        }

        public override Spell ChooseSpell(IDamageable c)
        {
            Spell spell = CheckCastHealingSpell();

            if (spell != null)
                return spell;

            switch (Utility.Random(3))
            {
                default:
                case 0:
                case 1: // Curse them.
                    {
                        spell = GetRandomCurseSpell((Mobile)c);

                        break;
                    }
                case 2: // Drain some mana
                    {
                        spell = GetRandomManaDrainSpell();

                        break;
                    }
            }

            return spell;
        }

        public override Spell GetRandomHealingSpell()
        {
            return base.GetRandomHealingSpell(); // Use magery spells
        }

        public Spell GetRandomCurseSpell(Mobile c)
        {
            if (ChanceToUseNecroSpells > Utility.RandomDouble()) // Use necro spells
            {
                switch (Utility.Random(4))
                {
                    default:
                    case 0: return new CorpseSkinSpell(m_Mobile, null);
                    case 1: return new MindRotSpell(m_Mobile, null);
                    case 2: return new EvilOmenSpell(m_Mobile, null);
                    case 3: return new BloodOathSpell(m_Mobile, null);
                }
            }

            return base.GetRandomCurseSpell(); // Use magery spells
        }
    }
}
