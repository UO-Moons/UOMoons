using Server.Mobiles;
using Server.Spells;
using System;

namespace Server.Items
{
	public class WeaponEffects
	{
		#region Do<AoSEffect>
		public static void DoMagicArrow(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);
			double damage = BaseWeapon.GetAosSpellDamage(attacker, defender, 10, 1, 4);

			attacker.MovingParticles(defender, 0x36E4, 5, 0, false, true, 3006, 4006, 0);
			attacker.PlaySound(0x1E5);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoHarm(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = BaseWeapon.GetAosSpellDamage(attacker, defender, 17, 1, 5);

			if (!defender.InRange(attacker, 2))
				damage *= 0.25; // 1/4 damage at > 2 tile range
			else if (!defender.InRange(attacker, 1))
				damage *= 0.50; // 1/2 damage at 2 tile range

			defender.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
			defender.PlaySound(0x0FC);

			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 100, 0, 0);

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoFireball(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = BaseWeapon.GetAosSpellDamage(attacker, defender, 19, 1, 5);

			attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
			attacker.PlaySound(0x15E);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoLightning(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			double damage = BaseWeapon.GetAosSpellDamage(attacker, defender, 23, 1, 4);

			defender.BoltEffect(0);

			SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 0, 0, 0, 100);

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoDispel(Mobile attacker, Mobile defender)
		{
			bool dispellable = false;

			if (defender is BaseCreature creature)
				dispellable = creature.Summoned && !creature.IsAnimatedDead;

			if (!dispellable)
				return;

			if (!attacker.CanBeHarmful(defender, false))
				return;

			attacker.DoHarmful(defender);

			MagerySpell sp = new Spells.Sixth.DispelSpell(attacker, null);

			if (sp.CheckResisted(defender))
			{
				defender.FixedEffect(0x3779, 10, 20);
			}
			else
			{
				Effects.SendLocationParticles(EffectItem.Create(defender.Location, defender.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
				Effects.PlaySound(defender, defender.Map, 0x201);

				defender.Delete();
			}
		}

		public static void DoExplosion(Mobile attacker, Mobile defender)
		{
			if (!attacker.CanBeHarmful(defender, false))
			{
				return;
			}

			attacker.DoHarmful(defender);

			double damage = BaseWeapon.GetAosSpellDamage(attacker, defender, 40, 1, 5);

			defender.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
			defender.PlaySound(0x307);

			SpellHelper.Damage(TimeSpan.FromSeconds(1.0), defender, attacker, damage, 0, 100, 0, 0, 0);

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoHitVelocity(Mobile attacker, IDamageable damageable)
		{
			int bonus = (int)attacker.GetDistanceToSqrt(damageable);

			if (bonus > 0)
			{
				AOS.Damage(damageable, attacker, bonus * 3, 100, 0, 0, 0, 0);

				if (attacker.Player)
				{
					attacker.SendLocalizedMessage(1072794); // Your arrow hits its mark with velocity!
				}

				if (damageable is Mobile mobile && mobile.Player)
				{
					mobile.SendLocalizedMessage(1072795); // You have been hit by an arrow with velocity!
				}
			}

			if (attacker.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}

		public static void DoLowerAttack(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyAttack(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0xA, 3);
			}
		}

		public static void DoLowerDefense(Mobile from, Mobile defender)
		{
			if (HitLower.ApplyDefense(defender))
			{
				defender.PlaySound(0x28E);
				Effects.SendTargetEffect(defender, 0x37BE, 1, 4, 0x23, 3);
			}
		}

		public static void DoAreaAttack(Mobile from, Mobile defender, int damageGiven, int sound, int hue, int phys, int fire, int cold, int pois, int nrgy)
		{
			Map map = from.Map;

			if (map == null)
				return;

			int range = Core.ML ? 5 : 10;

			var list = SpellHelper.AcquireIndirectTargets(from, from, from.Map, range);

			var count = 0;

			foreach (var m in list)
			{
				++count;

				from.DoHarmful(m, true);
				m.FixedEffect(0x3779, 1, 15, hue, 0);
				AOS.Damage(m, from, damageGiven / 2, phys, fire, cold, pois, nrgy, Server.DamageType.SpellAOE);
			}

			if (count > 0)
			{
				Effects.PlaySound(from.Location, map, sound);
			}

			if (from.Weapon is BaseWeapon wep && wep.ProcessingMultipleHits)
			{
				wep.BlockHitEffects = true;
			}
		}
		#endregion
	}
}
