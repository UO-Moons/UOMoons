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

	private static TimeSpan OnUse(Mobile m)
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
			switch (targeted)
			{
				case BaseWeapon weap when from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100):
				{
					if (weap.MaxHitPoints != 0)
					{
						var hp = (int)(weap.HitPoints / (double)weap.MaxHitPoints * 10);

						hp = hp switch
						{
							< 0 => 0,
							> 9 => 9,
							_ => hp
						};

						from.SendLocalizedMessage(1038285 + hp);
					}

					int damage = (weap.MaxDamage + weap.MinDamage) / 2;
					int hand = weap.Layer == Layer.OneHanded ? 0 : 1;

					if (damage < 3)
						damage = 0;
					else
						damage = (int)Math.Ceiling(Math.Min(damage, 30) / 5.0);

					WeaponType type = weap.Type;

					switch (type)
					{
						case WeaponType.Ranged:
							from.SendLocalizedMessage(1038224 + damage * 9);
							break;
						case WeaponType.Piercing:
							from.SendLocalizedMessage(1038218 + hand + damage * 9);
							break;
						case WeaponType.Slashing:
							from.SendLocalizedMessage(1038220 + hand + damage * 9);
							break;
						case WeaponType.Bashing:
							from.SendLocalizedMessage(1038222 + hand + damage * 9);
							break;
						default:
							from.SendLocalizedMessage(1038216 + hand + damage * 9);
							break;
					}

					if (weap.Poison != null && weap.PoisonCharges > 0)
						from.SendLocalizedMessage(1038284); // It appears to have poison smeared on it.
					break;
				}
				case BaseWeapon:
					from.SendLocalizedMessage(500353); // You are not certain...
					break;
				case BaseArmor arm when from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100):
				{
					if (arm.MaxHitPoints != 0)
					{
						var hp = (int)(arm.HitPoints / (double)arm.MaxHitPoints * 10);

						hp = hp switch
						{
							< 0 => 0,
							> 9 => 9,
							_ => hp
						};

						from.SendLocalizedMessage(1038285 + hp);
					}


					from.SendLocalizedMessage(1038295 + (int)Math.Ceiling(Math.Min(arm.ArmorRating, 35) / 5.0));
					break;
				}
				case BaseArmor:
					from.SendLocalizedMessage(500353); // You are not certain...
					break;
				case SwampDragon {HasBarding: true} dragon when from.CheckTargetSkill(SkillName.ArmsLore, targeted, 0, 100):
				{
					int perc = 4 * dragon.BardingHP / dragon.BardingMaxHP;

					perc = perc switch
					{
						< 0 => 0,
						> 4 => 4,
						_ => perc
					};

					dragon.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1053021 - perc, from.NetState);
					break;
				}
				case SwampDragon {HasBarding: true}:
					from.SendLocalizedMessage(500353); // You are not certain...
					break;
				default:
					from.SendLocalizedMessage(500352); // This is neither weapon nor armor.
					break;
			}
		}
	}
}
