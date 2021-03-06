using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.Quests
{
	public class ChannelingTheSupernaturalQuest : BaseQuest
	{
		public override bool DoneOnce => true;

		/* Channeling the Supernatural */
		public override object Title => 1078044;

		/* Head East out of town and go to Old Haven. Use Spirit Speak and channel energy from either yourself or nearby corpses 
        there. You can also cast Necromancy spells as well to raise Spirit Speak. Do these activities until you have raised your 
        Spirit Speak skill to 50.<br><center>------</center><br>How do you do? Channeling the supernatural through Spirit Speak 
        allows you heal your wounds. Such channeling expends your mana, so be mindful of this. Spirit Speak enhances the potency 
        of your Necromancy spells. The channeling powers of a Medium are quite useful when practicing the dark magic of Necromancy.
        <br><br>It is best to practice Spirit Speak where there are a lot of corpses. Head East out of town and go to Old Haven. 
        Undead currently reside there. Use Spirit Speak and channel energy from either yourself or nearby corpses. You can also 
        cast Necromancy spells as well to raise Spirit Speak.<br><br>Come back to me once you feel that you are worthy of the 
        rank of Apprentice Medium and I will reward you with something useful. */
		public override object Description => 1078047;

		/* Channeling the supernatural isn't for everyone. It is a dark art. See me if you ever wish to pursue the life of a Medium. */
		public override object Refuse => 1078048;

		/* Back so soon? You have not achieved the rank of Apprentice Medium. Come back to me once you feel that you are worthy of 
        the rank of Apprentice Medium and I will reward you with something useful. */
		public override object Uncomplete => 1078049;

		/* Well done! Channeling the supernatural is taxing, indeed. As promised, I will reward you with this bag of Necromancer 
        reagents. You will need these if you wish to also pursue the dark magic of Necromancy. Good journey to you. */
		public override object Complete => 1078051;

		public ChannelingTheSupernaturalQuest()
			: base()
		{
			AddObjective(new ApprenticeObjective(SkillName.SpiritSpeak, 50, "Old Haven Training", 1078045, 1078046));

			// 1078045 You ability to channel the supernatural is greatly enhanced while questing in this area.
			// 1078046 You are not in the quest area for Apprentice Medium. Your ability to channel the supernatural potential is not enhanced here.

			AddReward(new BaseReward(typeof(BagOfNecroReagents), 1078053));
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
				return Owner.Skills.SpiritSpeak.Base < 50;
		}

		public override void OnCompleted()
		{
			Owner.SendLocalizedMessage(1078050, null, 0x23); // You have achieved the rank of Apprentice Medium. Return to Morganna in New Haven to receive your reward.
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
}
