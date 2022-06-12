using System.Linq;
using Server.Spells;

namespace Server.Items
{
    public class LightningArrow : WeaponAbility
    {
        public LightningArrow()
        {
        }

        public override int BaseMana => 20;

        public override bool ConsumeAmmo => false;
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!Validate(attacker))
                return;

            ClearCurrentAbility(attacker);

            var map = attacker.Map;

            if (map == null)
                return;


            if (!(attacker.Weapon is BaseWeapon weapon))
                return;

            if (!CheckMana(attacker, true))
                return;

            IPooledEnumerable eable = defender.GetMobilesInRange(5);

            var targets = (from Mobile m in eable where m != defender && m != attacker && SpellHelper.ValidIndirectTarget(attacker, m) where m != null && !m.Deleted && m.Map == attacker.Map && m.Alive && attacker.CanSee(m) && attacker.CanBeHarmful(m) where attacker.InRange(m, weapon.MaxRange) && attacker.InLOS(m) select m).ToList();

            eable.Free();
            defender.BoltEffect(0);

            if (targets.Count > 0)
            {
                while (targets.Count > 2)
                {
                    targets.Remove(targets[Utility.Random(targets.Count)]);
                }

                foreach (var m in targets)
                {
                    m.BoltEffect(0);

                    AOS.Damage(m, attacker, Utility.RandomMinMax(29, 40), 0, 0, 0, 0, 100);
                }
            }

            ColUtility.Free(targets);
        }
    }
}
