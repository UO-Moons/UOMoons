using System;
using System.Collections.Generic;

namespace Server.Items
{
	public class DualWield : WeaponAbility
	{
		public DualWield()
		{
		}

		public static Dictionary<Mobile, DualWieldTimer> Registry { get; } = new Dictionary<Mobile, DualWieldTimer>();
		public override int BaseMana => 20;
		public static readonly TimeSpan Duration = TimeSpan.FromSeconds(8);

		public override SkillName GetSecondarySkill(Mobile from)
		{
			return from.Skills[SkillName.Ninjitsu].Base > from.Skills[SkillName.Bushido].Base ? SkillName.Ninjitsu : SkillName.Bushido;
		}

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker) || !CheckMana(attacker, true))
				return;

			if (HasRegistry(attacker))
			{
				var timer = Registry[attacker];

				if (timer.DualHitChance < .75)
				{
					timer.Expires += TimeSpan.FromSeconds(2);
					timer.DualHitChance += .25;

					BuffInfo.RemoveBuff(attacker, BuffIcon.DualWield);
					BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.DualWield, 1151294, 1151293, timer.Expires - DateTime.UtcNow, attacker, (timer.DualHitChance * 100).ToString()));

					attacker.SendLocalizedMessage(timer.DualHitChance == .75 ? 1150283 : 1150282); // Dual wield level increased to peak! : Dual wield level increased!
				}

				ClearCurrentAbility(attacker);
				return;
			}

			ClearCurrentAbility(attacker);
			attacker.SendLocalizedMessage(1150281); // You begin trying to strike with both your weapons at once.
			attacker.SendLocalizedMessage(1150284, true, Duration.TotalSeconds.ToString()); // Remaining Duration (seconds):

			DualWieldTimer t = new DualWieldTimer(attacker, .25);
			BuffInfo.AddBuff(attacker, new BuffInfo(BuffIcon.DualWield, 1151294, 1151293, Duration, attacker, "25"));

			Registry[attacker] = t;

			attacker.FixedParticles(0x3779, 1, 15, 0x7F6, 0x3E8, 3, EffectLayer.LeftHand);
			Effects.PlaySound(attacker.Location, attacker.Map, 0x524);
		}

		public static bool HasRegistry(Mobile attacker)
		{
			return Registry.ContainsKey(attacker);
		}

		public static void RemoveFromRegistry(Mobile from)
		{
			if (Registry.ContainsKey(from))
			{
				from.SendLocalizedMessage(1150285); // You no longer try to strike with both weapons at the same time.

				Registry[from].Stop();
				Registry.Remove(from);

				//if (from.Weapon is BaseWeapon)
					//((BaseWeapon)from.Weapon).ProcessingMultipleHits = false;
			}
		}

		/// <summary>
		/// Called from BaseWeapon, on successful hit
		/// </summary>
		/// <param name="from"></param>
		public static void DoHit(Mobile attacker, Mobile defender, int damage)
		{
			if (HasRegistry(attacker) && attacker.Weapon is BaseWeapon && Registry[attacker].DualHitChance > Utility.RandomDouble())
			{
				BaseWeapon wep = (BaseWeapon)attacker.Weapon;

				if (!Registry[attacker].SecondHit)
				{
					//wep.ProcessingMultipleHits = true;
					Registry[attacker].SecondHit = true;
					wep.OnHit(attacker, defender, .6);
					Registry[attacker].SecondHit = false;
				}
				//else if (wep.ProcessingMultipleHits)
				//{
				//	wep.EndDualWield = true;
				//}
			}
		}

		public class DualWieldTimer : Timer
		{
			public Mobile Owner { get; set; }
			public double DualHitChance { get; set; }
			public DateTime Expires { get; set; }
			public bool SecondHit { get; set; }

			private readonly TimeSpan Duration = TimeSpan.FromSeconds(8);

			public DualWieldTimer(Mobile owner, double dualHitChance)
				: base(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250))
			{
				Owner = owner;
				DualHitChance = dualHitChance;

				Expires = DateTime.UtcNow + Duration;

				Priority = TimerPriority.FiftyMs;
				Start();
			}

			protected override void OnTick()
			{
				if (DateTime.UtcNow > Expires)
				{
					RemoveFromRegistry(Owner);
				}
			}
		}
	}
}
