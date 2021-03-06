using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.SkillHandlers;

public class DetectHidden
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.DetectHidden].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile src)
	{
		src.SendLocalizedMessage(500819);//Where will you search?
		src.Target = new DetectHiddenTarget();

		return TimeSpan.FromSeconds(6.0);
	}

	public class DetectHiddenTarget : Target
	{
		public DetectHiddenTarget()
			: base(12, true, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile src, object targ)
		{
			bool foundAnyone = false;

			Point3D p = targ switch
			{
				Mobile mob => mob.Location,
				Item item => item.Location,
				IPoint3D point => new Point3D(point),
				_ => src.Location
			};

			double srcSkill = src.Skills[SkillName.DetectHidden].Value;
			int range = (int)(srcSkill / 10.0);

			if (!src.CheckSkill(SkillName.DetectHidden, 0.0, 100.0))
				range /= 2;

			BaseHouse house = BaseHouse.FindHouseAt(p, src.Map, 16);

			bool inHouse = house != null && house.IsFriend(src);

			if (inHouse)
				range = 22;

			if (range > 0)
			{
				IPooledEnumerable inRange = src.Map.GetMobilesInRange(p, range);

				foreach (Mobile trg in inRange)
				{
					if (trg.Hidden && src != trg)
					{
						double ss = srcSkill + Utility.Random(21) - 10;
						double ts = trg.Skills[SkillName.Hiding].Value + Utility.Random(21) - 10;
						bool houseCheck = inHouse && house.IsInside(trg);

						if (src.AccessLevel >= trg.AccessLevel && (ss >= ts || houseCheck))
						{
							if ((trg is ShadowKnight && (trg.X != p.X || trg.Y != p.Y)) ||
							    (!houseCheck && !CanDetect(src, trg)))
							{
								continue;
							}

							trg.RevealingAction();
							trg.SendLocalizedMessage(500814); // You have been revealed!
							trg.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 500814, trg.NetState);
							foundAnyone = true;
						}
					}
				}

				inRange.Free();

				IPooledEnumerable itemsInRange = src.Map.GetItemsInRange(p, range);

				foreach (Item item in itemsInRange)
				{
					if (item is LibraryBookcase /*&& GoingGumshoeQuest3.CheckBookcase(src, item)*/)
					{
						foundAnyone = true;
					}
					else
					{
						if (item is not IRevealableItem dItem || (item.Visible && dItem.CheckWhenHidden))
						{
							continue;
						}

						if (dItem.CheckReveal(src))
						{
							dItem.OnRevealed(src);

							foundAnyone = true;
						}
					}
				}

				itemsInRange.Free();
			}

			if (!foundAnyone)
			{
				src.SendLocalizedMessage(500817); // You can see nothing hidden there.
			}
		}
	}
	public static void DoPassiveDetect(Mobile src)
	{
		if (src?.Map == null || src.Location == Point3D.Zero || src.IsStaff())
		{
			return;
		}

		double ss = src.Skills[SkillName.DetectHidden].Value;

		if (ss <= 0)
		{
			return;
		}

		IPooledEnumerable eable = src.Map.GetMobilesInRange(src.Location, 4);

		if (eable == null)
		{
			return;
		}

		foreach (Mobile m in eable)
		{
			if (m == null || m == src || m is ShadowKnight || !CanDetect(src, m))
			{
				continue;
			}

			double ts = (m.Skills[SkillName.Hiding].Value + m.Skills[SkillName.Stealth].Value) / 2;

			if (src.Race == Race.Elf)
			{
				ss += 20;
			}

			if (src.AccessLevel >= m.AccessLevel && Utility.Random(1000) < (ss - ts) + 1)
			{
				m.RevealingAction();
				m.SendLocalizedMessage(500814); // You have been revealed!
			}
		}

		eable.Free();

		eable = src.Map.GetItemsInRange(src.Location, 8);

		foreach (Item item in eable)
		{
			if (!item.Visible && item is IRevealableItem && ((IRevealableItem)item).CheckPassiveDetect(src))
			{
				src.SendLocalizedMessage(1153493); // Your keen senses detect something hidden in the area...
			}
		}

		eable.Free();
	}

	public static bool CanDetect(Mobile src, Mobile target)
	{
		if (src.Map == null || target.Map == null || !src.CanBeHarmful(target, false, false, true))
		{
			return false;
		}

		// No invulnerable NPC's
		if (src.Blessed || src is BaseCreature {IsInvulnerable: true})
		{
			return false;
		}

		if (target.Blessed || target is BaseCreature {IsInvulnerable: true})
		{
			return false;
		}

		// pet owner, guild/alliance, party
		if (!Spells.SpellHelper.ValidIndirectTarget(target, src))
		{
			return false;
		}

		// Checked aggressed/aggressors
		if (src.Aggressed.Any(x => x.Defender == target) || src.Aggressors.Any(x => x.Attacker == target))
		{
			return true;
		}

		// In Fel or Follow the same rules as indirect spells such as wither
		return src.Map.Rules == ZoneRules.FeluccaRules;
	}
}
