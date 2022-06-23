using Server.Engines.XmlSpawner2;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Items;

public class ItemIdentification
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.ItemID].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile from)
	{
		from.SendLocalizedMessage(500343); // What do you wish to appraise and identify?
		from.Target = new InternalTarget();

		return TimeSpan.FromSeconds(1.0);
	}

	[PlayerVendorTarget]
	private class InternalTarget : Target
	{
		public InternalTarget() : base(8, false, TargetFlags.None)
		{
			AllowNonlocal = true;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			switch (o)
			{
				case Item item when from.CheckTargetSkill(SkillName.ItemID, o, 0, 100):
				{
					switch (item)
					{
						case BaseWeapon weapon:
							weapon.Identified = true;
							break;
						case BaseArmor armor:
							armor.Identified = true;
							break;
					}

					if (!Core.AOS)
						item.OnSingleClick(from);
					break;
				}
				case Item:
					from.SendLocalizedMessage(500353); // You are not certain...
					break;
				case Mobile mob:
					mob.OnSingleClick(from);
					break;
				default:
					from.SendLocalizedMessage(500353); // You are not certain...
					break;
			}

			XmlAttach.RevealAttachments(from, o);
		}
	}
}
