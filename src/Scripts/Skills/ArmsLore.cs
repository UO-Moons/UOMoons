using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class ArmsLore
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.ArmsLore].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.Target = new InternalTarget();

		m.SendLocalizedMessage(500349); // What item do you wish to get information about?

		return TimeSpan.FromSeconds(1.0);
	}

	[PlayerVendorTarget]
	private class InternalTarget : Target
	{
		public InternalTarget() : base(2, false, TargetFlags.None)
		{
			AllowNonlocal = true;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is BaseWeapon weap)
			{
				if (from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100))
				{
					if (weap.MaxHitPoints != 0)
					{
						int hp = (int)((weap.HitPoints / (double)weap.MaxHitPoints) * 10);

						hp = hp switch
						{
							< 0 => 0,
							> 9 => 9,
							_ => hp
						};

						from.SendLocalizedMessage(1038285 + hp);
					}

					int damage = (weap.MaxDamage + weap.MinDamage) / 2;
					int hand = (weap.Layer == Layer.OneHanded ? 0 : 1);

					if (damage < 3)
						damage = 0;
					else
						damage = (int)Math.Ceiling(Math.Min(damage, 30) / 5.0);
					/*
				else if ( damage < 6 )
					damage = 1;
				else if ( damage < 11 )
					damage = 2;
				else if ( damage < 16 )
					damage = 3;
				else if ( damage < 21 )
					damage = 4;
				else if ( damage < 26 )
					damage = 5;
				else
					damage = 6;
					 * */

					WeaponType type = weap.Type;

					switch (type)
					{
						case WeaponType.Ranged:
							from.SendLocalizedMessage(1038224 + (damage * 9));
							break;
						case WeaponType.Piercing:
							from.SendLocalizedMessage(1038218 + hand + (damage * 9));
							break;
						case WeaponType.Slashing:
							from.SendLocalizedMessage(1038220 + hand + (damage * 9));
							break;
						case WeaponType.Bashing:
							from.SendLocalizedMessage(1038222 + hand + (damage * 9));
							break;
						default:
							from.SendLocalizedMessage(1038216 + hand + (damage * 9));
							break;
					}

					if (weap.Poison != null && weap.PoisonCharges > 0)
						from.SendLocalizedMessage(1038284); // It appears to have poison smeared on it.
				}
				else
				{
					from.SendLocalizedMessage(500353); // You are not certain...
				}
			}
			else if (targeted is BaseArmor arm)
			{
				if (from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100))
				{
					if (arm.MaxHitPoints != 0)
					{
						int hp = (int)((arm.HitPoints / (double)arm.MaxHitPoints) * 10);

						hp = hp switch
						{
							< 0 => 0,
							> 9 => 9,
							_ => hp
						};

						from.SendLocalizedMessage(1038285 + hp);
					}


					from.SendLocalizedMessage(1038295 + (int)Math.Ceiling(Math.Min(arm.ArmorRating, 35) / 5.0));
					/*
					if ( arm.ArmorRating < 1 )
						from.SendLocalizedMessage( 1038295 ); // This armor offers no defense against attackers.
					else if ( arm.ArmorRating < 6 )
						from.SendLocalizedMessage( 1038296 ); // This armor provides almost no protection.
					else if ( arm.ArmorRating < 11 )
						from.SendLocalizedMessage( 1038297 ); // This armor provides very little protection.
					else if ( arm.ArmorRating < 16 )
						from.SendLocalizedMessage( 1038298 ); // This armor offers some protection against blows.
					else if ( arm.ArmorRating < 21 )
						from.SendLocalizedMessage( 1038299 ); // This armor serves as sturdy protection.
					else if ( arm.ArmorRating < 26 )
						from.SendLocalizedMessage( 1038300 ); // This armor is a superior defense against attack.
					else if ( arm.ArmorRating < 31 )
						from.SendLocalizedMessage( 1038301 ); // This armor offers excellent protection.
					else
						from.SendLocalizedMessage( 1038302 ); // This armor is superbly crafted to provide maximum protection.
					 * */
				}
				else
				{
					from.SendLocalizedMessage(500353); // You are not certain...
				}
			}
			else if (targeted is SwampDragon {HasBarding: true} dragon)
			{
				if (from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100))
				{
					int perc = 4 * dragon.BardingHP / dragon.BardingMaxHP;

					perc = perc switch
					{
						< 0 => 0,
						> 4 => 4,
						_ => perc
					};

					dragon.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1053021 - perc, from.NetState);
				}
				else
				{
					from.SendLocalizedMessage(500353); // You are not certain...
				}
			}
			else
			{
				from.SendLocalizedMessage(500352); // This is neither weapon nor armor.
			}
		}
	}
}
