using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests;

public class SwiftAsAnArrowQuest : BaseQuest
{
	public override bool DoneOnce => true;

	/* Swift as an Arrow */
	public override object Title => 1078201;

	/* Head East out of town and go to Old Haven. While wielding your bow or crossbow, battle monster there until you have 
    raised your Archery skill to 50. Well met, friend. Imagine yourself in a distant grove of trees, You raise your bow, 
    take slow, careful aim, and with the twitch of a finger, you impale your prey with a deadly arrow. You look like you 
    would make a excellent archer, but you will need practice. There is no better way to practice Archery than when you 
    life is on the line. I have a challenge for you. Head East out of town and go to Old Haven. While wielding your bow 
    or crossbow, battle the undead that reside there. Make sure you bring a healthy supply of arrows (or bolts if you 
    prefer a crossbow). If you wish to purchase a bow, crossbow, arrows, or bolts, you can purchase them from me or the 
    Archery shop in town. You can also make your own arrows with the Bowcraft/Fletching skill. You will need fletcher's 
    tools, wood to turn into sharft's, and feathers to make arrows or bolts. Come back to me after you have achived the 
    rank of Apprentice Archer, and i will reward you with a fine Archery weapon. */
	public override object Description => 1078205;

	/* I understand that Archery may not be for you. Feel free to visit me in the future if you change your mind. */
	public override object Refuse => 1078206;

	/* You're doing great as an Archer! however, you need more practice. Head East out of town and go to Old Haven. come 
    back to me after you have acived the rank of Apprentice Archer. */
	public override object Uncomplete => 1078207;

	/* Congratulation! I want to reward you for your accomplishment. Take this composite bow. It is called " Heartseeker". 
    With it, you will shoot with swiftness, precision, and power. I hope "Heartseeker" serves you well. */
	public override object Complete => 1078209;

	public SwiftAsAnArrowQuest()
	{
		AddObjective(new ApprenticeObjective(SkillName.Archery, 50, "Old Haven Training", 1078203, 1078204));

		// 1078203 You feel more steady and dexterous here. Your Archery skill is enhanced in this area.
		// 1078204 You feel less steady and dexterous here. Your Archery learning potential is no longer enhanced.

		AddReward(new BaseReward(typeof(Heartseeker), 1078210));
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
			return Owner.Skills.Archery.Base < 50;
	}

	public override void OnCompleted()
	{
		Owner.SendLocalizedMessage(1078208, null, 0x23); // You have achieved the rank of Apprentice Archer. Return to Robyn in New Haven to claim your reward.
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