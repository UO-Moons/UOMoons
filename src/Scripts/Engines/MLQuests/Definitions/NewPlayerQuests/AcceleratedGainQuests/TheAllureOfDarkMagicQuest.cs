using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests;

public class TheAllureOfDarkMagicQuest : BaseQuest
{
	public override bool DoneOnce => true;

	/* The Allure of Dark Magic */
	public override object Title => 1078036;

	/* Head East out of town and go to Old Haven. Cast Evil Omen and Pain Spike against monsters there until you have raised your 
    Necromancy skill to 50.<br><center>------</center><br>Welcome! I see you are allured by the dark magic of Necromancy. First, 
    you must prove yourself worthy of such knowledge. Undead currently occupy the town of Old Haven. Practice your harmful Necromancy 
    spells on them such as Evil Omen and Pain Spike.<br><br>Make sure you have plenty of reagents before embarking on your journey. 
    Reagents are required to cast Necromancy spells. You can purchase extra reagents from me, or you can find reagents growing in 
    the nearby wooded areas. You can see which reagents are required for each spell by looking in your spellbook.<br><br>Come back 
    to me once you feel that you are worthy of the rank of Apprentice Necromancer and I will reward you with the knowledge you desire. */
	public override object Description => 1078039;

	/* You are weak after all. Come back to me when you are ready to practice Necromancy. */
	public override object Refuse => 1078040;

	/* You have not achieved the rank of Apprentice Necromancer. Come back to me once you feel that you are worthy of the rank of 
    Apprentice Necromancer and I will reward you with the knowledge you desire. */
	public override object Uncomplete => 1078041;

	/* You have done well, my young apprentice. Behold! I now present to you the knowledge you desire. This spellbook contains all 
    the Necromancer spells. The power is intoxicating, isn't it? */
	public override object Complete => 1078043;

	public TheAllureOfDarkMagicQuest()
	{
		AddObjective(new ApprenticeObjective(SkillName.Necromancy, 50, "Old Haven Training", 1078037, 1078038));

		// 1078037 Your Necromancy potential is greatly enhanced while questing in this area.
		// 1078038 You are not in the quest area for Apprentice Necromancer. Your Necromancy potential is not enhanced here.

		AddReward(new BaseReward(typeof(CompleteNecromancerSpellbook), 1078052));
	}

	public override bool CanOffer()
	{
		PlayerMobile pm = Owner;
		if (pm.AcceleratedStart > DateTime.UtcNow)
		{
			Owner.SendLocalizedMessage(1077951); // You are already under the effect of an accelerated skillgain scroll.
			return false;
		}
		else
			return Owner.Skills.Necromancy.Base < 50;
	}

	public override void OnCompleted()
	{
		Owner.SendLocalizedMessage(1078042, null, 0x23); // You have achieved the rank of Apprentice Necromancer. Return to Mulcivikh in New Haven to receive the knowledge you desire.
		Owner.PlaySound(CompleteSound);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}