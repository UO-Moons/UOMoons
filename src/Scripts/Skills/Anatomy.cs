using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class Anatomy
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Anatomy].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.Target = new InternalTarget();

		m.SendLocalizedMessage(500321); // Whom shall I examine?

		return TimeSpan.FromSeconds(1.0);
	}

	private class InternalTarget : Target
	{
		public InternalTarget() : base(8, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (from == targeted)
			{
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 500324); // You know yourself quite well enough already.
			}
			else switch (targeted)
			{
				case TownCrier townCrier:
					townCrier.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500322, from.NetState); // This person looks fine to me, though he may have some news...
					break;
				case BaseVendor {IsInvulnerable: true} vendor:
					vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500326, from.NetState); // That can not be inspected.
					break;
				case Mobile targ:
				{
					int marginOfError = Math.Max(0, 25 - (int)(from.Skills[SkillName.Anatomy].Value / 4));

					int str = targ.Str + Utility.RandomMinMax(-marginOfError, +marginOfError);
					int dex = targ.Dex + Utility.RandomMinMax(-marginOfError, +marginOfError);
					int stm = ((targ.Stam * 100) / Math.Max(targ.StamMax, 1)) + Utility.RandomMinMax(-marginOfError, +marginOfError);

					int strMod = str / 10;
					int dexMod = dex / 10;
					int stmMod = stm / 10;

					strMod = strMod switch
					{
						< 0 => 0,
						> 10 => 10,
						_ => strMod
					};

					dexMod = dexMod switch
					{
						< 0 => 0,
						> 10 => 10,
						_ => dexMod
					};

					stmMod = stmMod switch
					{
						> 10 => 10,
						< 0 => 0,
						_ => stmMod
					};

					if (from.CheckTargetSkill(SkillName.Anatomy, targ, 0, 100))
					{
						targ.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1038045 + (strMod * 11) + dexMod, from.NetState); // That looks [strong] and [dexterous].

						if (from.Skills[SkillName.Anatomy].Base >= 65.0)
							targ.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1038303 + stmMod, from.NetState); // That being is at [10,20,...] percent endurance.
					}
					else
					{
						targ.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042666, from.NetState); // You can not quite get a sense of their physical characteristics.
					}

					break;
				}
				case Item item:
					item.SendLocalizedMessageTo(from, 500323); // Only living things have anatomies!
					break;
			}
		}
	}
}
