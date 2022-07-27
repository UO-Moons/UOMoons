
using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
	/// <summary>
	/// Gain a defensive advantage over your primary opponent for a short time.
	/// </summary>
	public class Feint : WeaponAbility
	{
		public static Dictionary<Mobile, FeintTimer> Registry { get; } = new();

		public override int BaseMana => 30;

		public override SkillName GetSecondarySkill(Mobile from)
		{
			return from.Skills[SkillName.Ninjitsu].Base > from.Skills[SkillName.Bushido].Base ? SkillName.Ninjitsu : SkillName.Bushido;
		}

		public static bool IsUnderEffect(Mobile mob)
		{
			if (mob == null)
				return false;

			return Registry.ContainsKey(mob);
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			if (Registry.ContainsKey(attacker))
			{
				if (Registry[attacker] != null)
					Registry[attacker].Stop();

				Registry.Remove(attacker);
			}

			ClearCurrentAbility(attacker);

			attacker.SendLocalizedMessage(1063360); // You baffle your target with a feint!
			defender.SendLocalizedMessage(1063361); // You were deceived by an attacker's feint!

			attacker.FixedParticles(0x3728, 1, 13, 0x7F3, 0x962, 0, EffectLayer.Waist);
			double skill = Math.Max(attacker.Skills[SkillName.Ninjitsu].Value, attacker.Skills[SkillName.Bushido].Value);

			int bonus = (int)(20.0 + 3.0 * (skill - 50.0) / 7.0);

			FeintTimer t = new FeintTimer(attacker, defender, bonus);   //20-50 % decrease

			t.Start();
			Registry.Add(defender, t);

			string args = $"{defender.Name}\t{bonus}";
			BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.Feint, 1151308, 1151307, TimeSpan.FromSeconds(6), attacker, args));
		}

		public class FeintTimer : Timer
		{
			public Mobile Owner { get; }
			public Mobile Enemy { get; }

			public int DamageReduction { get; }

			public FeintTimer(Mobile owner, Mobile enemy, int _DamageReduction)
				: base(TimeSpan.FromSeconds(6.0))
			{
				Owner = owner;
				Enemy = enemy;
				DamageReduction = _DamageReduction;
				Priority = TimerPriority.FiftyMs;
			}

			protected override void OnTick()
			{
				Registry.Remove(Owner);
			}
		}
	}
}
