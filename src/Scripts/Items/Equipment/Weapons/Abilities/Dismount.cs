using Server.Mobiles;
using Server.Network;
using Server.Spells.Ninjitsu;
using System;

namespace Server.Items
{
	/// <summary>
	/// Perfect for the foot-soldier, the Dismount special attack can unseat a mounted opponent.
	/// The fighter using this ability must be on his own two feet and not in the saddle of a steed
	/// (with one exception: players may use a lance to dismount other players while mounted).
	/// If it works, the target will be knocked off his own mount and will take some extra damage from the fall!
	/// </summary>
	public class Dismount : WeaponAbility
	{
		public Dismount()
		{
		}

		public override int BaseMana => Core.TOL ? 25 : 20;

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
			if (!Validate(attacker)) return;

			bool Lances = false;

			if ((attacker.Weapon is Lance) && (defender.Weapon is Lance ))
				Lances = true;

			if ((attacker.Mounted && !Lances) || attacker.Flying)
			{
				attacker.SendLocalizedMessage(1061283); // You cannot perform that attack while mounted or flying!
				return;
			}

			// if (defender is ChaosDragoon || defender is ChaosDragoonElite) return;

			ClearCurrentAbility(attacker);

			IMount mount = defender.Mount;

			if (mount == null && !defender.Flying && (!Core.ML || !Spells.Ninjitsu.AnimalForm.UnderTransformation(defender)))
			{
				attacker.SendLocalizedMessage(1060848); // This attack only works on mounted or flying targets
				return;
			}

			if (!CheckMana(attacker, true))
			{
				return;
			}

			if (Core.ML && attacker is LesserHiryu && 0.8 >= Utility.RandomDouble())
			{
				return; //Lesser Hiryu have an 80% chance of missing this attack
			}

			defender.PlaySound(0x140);
			defender.FixedParticles(0x3728, 10, 15, 9955, EffectLayer.Waist);

			int delay = Core.TOL && attacker.Weapon is BaseRanged ? 8 : 10;

			DoDismount(attacker, defender, BlockMountType.DismountRecovery, TimeSpan.FromSeconds(delay), true);

			if (!attacker.Mounted)
			{
				AOS.Damage(defender, attacker, Utility.RandomMinMax(15, 25), 100, 0, 0, 0, 0);
			}
		}

		/*
        public static void DoDismount(Mobile attacker, Mobile defender, IMount mount, int delay, BlockMountType type = BlockMountType.Dazed)
        {
            Dismount(attacker, defender, BlockMountType.DismountRecovery, TimeSpan.FromSeconds(delay), true);
        }
        */

		public static void DoDismount(Mobile dismounter, Mobile dismounted, BlockMountType blockmounttype, TimeSpan delay, bool message)
		{
			if (Core.ML && AnimalForm.UnderTransformation(dismounted))
			{
				AnimalForm.RemoveContext(dismounted, true);
				if (dismounted.Player)
					dismounted.SendLocalizedMessage(1114066, dismounter.Name); // ~1_NAME~ knocked you out of animal form!
			}
			else
			{
				if (!dismounted.Mounted) return;

				if (dismounted is Neira || dismounted is ChaosDragoon || dismounted is ChaosDragoonElite)
				{
					if (dismounter.Player)
						dismounter.SendLocalizedMessage(1042047); // You fail to knock the rider from its mount.
					return;
				}

				IMount mount = dismounted.Mount;

				if (mount != null)
				{
					if (dismounter is PlayerMobile)
					{
						dismounter.SendLocalizedMessage(1060082); // The force of your attack has dislodged them from their mount!
						((PlayerMobile)dismounter).SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(Core.TOL && dismounter.Weapon is BaseRanged ? 8 : 10), false);
					}

					mount.Rider = null;
					new BaseMount.DespawnTimer(mount, TimeSpan.FromMinutes(1.0)).Start();
					if (mount is Mobile newMob)
					{
						// if (dismounter.Aggressor) newMob.Aggressed = dismounter;
						newMob?.Attack(dismounter);
					}

					if (message)
					{
						if (dismounted.Flying)
						{
							if (dismounted.Player)
								dismounted.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113590, dismounter.Name); // You have been grounded by ~1_NAME~!

							if (!BaseMount.OnFlightPath(dismounted))
							{
								dismounted.Flying = false;
								dismounted.Freeze(TimeSpan.FromSeconds(1));
								dismounted.Animate(AnimationType.Land, 0);
								BuffInfo.RemoveBuff(dismounted, BuffIcon.Fly);
							}
						}
						else
						{
							// defender.SendLocalizedMessage(1060083); // You fall off of your mount and take damage!
							if (dismounted.Player)
								dismounted.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1049623, dismounter.Name); // You have been knocked off of your mount by ~1_NAME~!
						}
					}

					if (Core.ML)
					{
						if (dismounter is BaseCreature)
						{
							BaseCreature bc = dismounter as BaseCreature;

							if (bc.ControlMaster is PlayerMobile)
							{
								PlayerMobile pm = bc.ControlMaster as PlayerMobile;
								pm.SetMountBlock(BlockMountType.DismountRecovery, TimeSpan.FromSeconds(10.0), false);
							}
						}
					}

					if (delay != TimeSpan.MinValue)
					{
						BaseMount.SetMountPrevention(dismounted, mount, blockmounttype, delay);
					}
				}
			}
		}
	}
}
