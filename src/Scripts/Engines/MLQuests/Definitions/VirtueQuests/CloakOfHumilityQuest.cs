using Server.Engines.Quests;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Quests;

public sealed class TheQuestionsQuest : BaseQuest
{
	public TheQuestionsQuest()
	{
		AddObjective(new QuestionAndAnswerObjective(4, EntryTable));
	}

	public override bool ShowDescription => false;

	public override QuestChain ChainId => QuestChain.CloakOfHumility;
	public override Type NextQuest => typeof(CommunityServiceMuseumQuest);

	public override object Title => 1075850;  // Know Thy Humility

	/*Greetings my friend! My name is Gareth, and I represent a group of citizens who wish to rejuvenate interest in our
     * kingdom's noble heritage. 'Tis our belief that one of Britannia's greatest triumphs was the institution of the Virtues,
     * neglected though they be now. To that end I have a set of tasks prepared for one who would follow a truly Humble path. 
     * Art thou interested in joining our effort?*/
	public override object Description => 1075675;

	//I wish that thou wouldest reconsider.
	public override object Refuse => 1075677;

	//Wonderful! First, let us see if thou art reading from the same roll of parchment as we are. *smiles*
	public override object Uncomplete => 1075676;

	/*Very good! I can see that ye hath more than just a passing interest in our work. There are many trials before thee, but 
     * I have every hope that ye shall have the diligence and fortitude to carry on to the very end. Before we begin, please
     * prepare thyself by thinking about the virtue of Humility. Ponder not only its symbols, but also its meanings. Once ye 
     * believe that thou art ready, speak with me again.*/
	public override object Complete => 1075714;

	/*Ah... no, that is not quite right. Truly, Humility is something that takes time and experience to understand. I wish to 
     * challenge thee to seek out more knowledge concerning this virtue, and tomorrow let us speak again about what thou hast 
     * learned.<br>*/
	public override object FailedMsg => 1075713;

	public override bool RenderObjective(MondainQuestGump g, bool offer)
	{
		// Quest Offer// Quest Log
		g.AddHtmlLocalized(130, 45, 270, 16, offer ? 1049010 : 1046026, 0xFFFFFF, false, false);

		g.AddButton(130, 430, 0x2EEF, 0x2EF1, (int)Buttons.PreviousPage, GumpButtonType.Reply, 0);
		g.AddButton(275, 430, 0x2EE9, 0x2EEB, (int)Buttons.NextPage, GumpButtonType.Reply, 0);

		g.AddHtmlObject(160, 70, 200, 40, Title, BaseQuestGump.DarkGreen, false, false);
		g.AddHtmlLocalized(98, 140, 312, 16, 1049073, 0x2710, false, false); // Objective:

		g.AddHtmlLocalized(98, 156, 312, 16, 1072208, 0x2710, false, false); // All of the following	

		int offset = 172;

		foreach (QuestionAndAnswerObjective obj in Objectives.OfType<QuestionAndAnswerObjective>())
		{
			var str = offer ? $"Answer {obj.MaxProgress} questions correctly." : $"Answer {obj.CurProgress}/{obj.MaxProgress} questions answered correctly.";

			g.AddHtmlObject(98, offset, 312, 16, str, BaseQuestGump.LightGreen, false, false);

			offset += 16;
		}

		return true;
	}

	public override void OnAccept()
	{
		base.OnAccept();
		Owner.SendGump(new QAndAGump(Owner, this));
	}

	public override void OnResign(bool chain)
	{
		base.OnResign(chain);
		CooldownTable[Owner] = DateTime.UtcNow + TimeSpan.FromHours(24);
	}

	public override bool CanOffer()
	{
		DefragCooldown();

		if (!CooldownTable.ContainsKey(Owner) || Owner.AccessLevel > AccessLevel.Player)
		{
			return base.CanOffer();
		}

		return false;
	}

	private static void DefragCooldown()
	{
		List<Mobile> toRemove = (from kvp in CooldownTable where kvp.Value < DateTime.UtcNow select kvp.Key).ToList();

		foreach (var m in toRemove.Where(m => CooldownTable.ContainsKey(m)))
		{
			CooldownTable.Remove(m);
		}
	}

	public static void Configure()
	{
		EntryTable[0] = new QuestionAndAnswerEntry(1075708, new object[] { 1075709 }, new object[] { 1075710, 1075711, 1075712 });  //<center>Finish this truism: Humility shows us...</center>
		EntryTable[1] = new QuestionAndAnswerEntry(1075678, new object[] { 1075679 }, new object[] { 1075680, 1075681, 1075682 });  //<center>What is the symbol of Humility?</center>
		EntryTable[2] = new QuestionAndAnswerEntry(1075683, new object[] { 1075685 }, new object[] { 1075684, 1075686, 1075687 });  //<center>What opposes Humility?</center>
		EntryTable[3] = new QuestionAndAnswerEntry(1075688, new object[] { 1075691 }, new object[] { 1075689, 1075690, 1075692 });  //<center>What is the color of Humility?</center>
		EntryTable[4] = new QuestionAndAnswerEntry(1075693, new object[] { 1075697 }, new object[] { 1075694, 1075695, 1075696 });  //<center>How doth one find Humility?</center>
		EntryTable[5] = new QuestionAndAnswerEntry(1075698, new object[] { 1075700 }, new object[] { 1075699, 1075601, 1075602 });  //<center>Which city embodies the need for Humility?</center>
		EntryTable[6] = new QuestionAndAnswerEntry(1075703, new object[] { 1075705 }, new object[] { 1075704, 1075706, 1075707 });  //<center>By name, which den of evil challenges one???s humility?</center>
	}

	public static QuestionAndAnswerEntry[] EntryTable { get; } = new QuestionAndAnswerEntry[7];

	public static Dictionary<Mobile, DateTime> CooldownTable { get; } = new();
}

public sealed class CommunityServiceMuseumQuest : BaseQuest
{
	public CommunityServiceMuseumQuest()
	{
		AddObjective(new CollectionsObtainObjective(typeof(ShepherdsCrookOfHumility), "Shepherd's Crook of Humility", 1));
	}

	public override QuestChain ChainId => QuestChain.CloakOfHumility;
	public override Type NextQuest => typeof(CommunityServiceZooQuest);

	//Community Service - Museum
	public override object Title => 1075716;

	/*'Tis time to help out the community of Britannia. Visit the Vesper Museum and donate to their collection, and eventually thou will
     * be able to receive a replica of the Shepherd's Crook of Humility. Once ye have it, return to me. Art thou willing to do this?*/
	public override object Description => 1075717;

	//I wish that thou wouldest reconsider.
	public override object Refuse => 1075719;

	//Hello my friend. The museum sitteth in southern Vesper. If ye go downstairs, ye will discover a small donation chest.
	//That is the place where ye should leave thy donation.
	public override object Uncomplete => 1075720;

	/*Terrific! The Museum is a worthy cause. Many will benefit from the inspiration and learning that thine donation hath supported.*/
	public override object Complete => 1075721;
}

public sealed class CommunityServiceZooQuest : BaseQuest
{
	public CommunityServiceZooQuest()
	{
		AddObjective(new CollectionsObtainObjective(typeof(ForTheLifeOfBritanniaSash), "Life of Britannia Sash", 1));
	}

	public override QuestChain ChainId => QuestChain.CloakOfHumility;
	public override Type NextQuest => typeof(CommunityServiceLibraryQuest);

	//Community Service ??? Zoo
	public override object Title => 1075722;

	/*Now, go on and donate to the Moonglow Zoo. Givest thou enough to receive a 'For the Life of Britannia' sash. Once ye have it, 
     * return it to me. Wilt thou continue?*/
	public override object Description => 1075723;

	//I wish that thou wouldest reconsider.
	public override object Refuse => 1075725;

	//Hello again. The zoo lies a short ways south of Moonglow. Close to the entrance thou wilt discover a small donation chest. 
	//That is where thou shouldest leave thy donation.
	public override object Uncomplete => 1075726;

	/*Wonderful! The Zoo is a very special place from which people young and old canst benefit. Thanks to thee, it can continue to thrive.*/
	public override object Complete => 1075727;
}

public sealed class CommunityServiceLibraryQuest : BaseQuest
{
	public CommunityServiceLibraryQuest()
	{
		AddObjective(new CollectionsObtainObjective(typeof(SpecialPrintingOfVirtue), "Special Painint of 'Virtue' Book", 1));
	}

	public override QuestChain ChainId => QuestChain.CloakOfHumility;
	public override Type NextQuest => typeof(WhosMostHumbleQuest);

	//Community Service ??? Library
	public override object Title => 1075728;

	/*I have one more charity for thee, my diligent friend. Go forth and donate to the Britain Library and do that which is necessary to receive 
     * a special printing of ???Virtue???, by Lord British. Once in hand, bring the book back with ye. Art thou ready?*/
	public override object Description => 1075729;

	//I wish that thou wouldest reconsider.
	public override object Refuse => 1075731;

	//Art thou having trouble? The Library lieth north of Castle British's gates. I believe the representatives in charge of the 
	//donations are easy enough to find. They await thy visit, amongst the many tomes of knowledge.
	public override object Uncomplete => 1075732;

	/*Very good! The library is of great import to the people of Britannia. Thou hath done a worthy deed and this is thy last 
     * required donation. I encourage thee to continue contributing to thine community, beyond the obligations of this endeavor.*/
	public override object Complete => 1075733;
}

public sealed class WhosMostHumbleQuest : BaseQuest
{
	private readonly List<Item> _mQuestItems = new();
	private readonly List<Mobile> _mGivenTo = new();

	public Dictionary<int, HumilityQuestMobileInfo> Infos { get; } = new();

	public override bool CanRefuseReward => true;

	public WhosMostHumbleQuest()
	{
		AddObjective(new ObtainObjective(typeof(IronChain), "Iron Chain", 1));
		AddReward(new BaseReward(typeof(GoldShield), "A Gold Shield"));
	}

	public override QuestChain ChainId => QuestChain.CloakOfHumility;

	//Who's Most Humble
	public override object Title => 1075734;

	/*Thou art challenged to find seven citizens spread out among the towns of Britannia: Skara Brae, Minoc, Britain, and 
     * one of the towns upon an isle at sea. Each citizen wilt reveal some thought concerning Humility. But who doth best 
     * exemplify the virtue? Here, thou needeth wear this plain grey cloak, for they wilt know ye by it. Wilt thou continue?*/
	public override object Description => 1075735;

	//'Tis a difficult quest, but well worth it. Wilt thou reconsider?
	public override object Refuse => 1075737;

	/*There art no less than seven 'humble citizens' spread across the Britannia proper. I know that they can be found in the
     * towns of Minoc, Skara Brae and Britain. Another may be upon an island at sea, the name of which escapes me at the moment. 
     * Thou needeth visit all seven to solve the puzzle. Be diligent, for they have a tendency to wander about.<BR><BR><br>Dost 
     * thou wear the plain grey cloak?*/
	public override object Uncomplete => 1075738;

	/*Noble friend, thou hast performed tremendously! On behalf of the Rise of Britannia I wish to reward thee with this golden
     * shield, a symbol of accomplishment and pride for the many things that thou hast done for our people.<BR><BR><br>Dost thou accept?*/
	public override object Complete => 1075782;

	public override void OnAccept()
	{
		base.OnAccept();

		Owner.SendGump(new QuestInfoGump(1075736)); // Excellent. When thou hast satisfied the needs of the most humble, thou wilt be given an item meant for me. Take this <B>brass ring</B> to start ye on the way.

		Item cloak = new GreyCloak();
		Item ring = new BrassRing();

		_mQuestItems.Add(cloak);
		_mQuestItems.Add(ring);

		Owner.Backpack.DropItem(cloak);
		Owner.Backpack.DropItem(ring);

		List<Type> itemTypes = new(HumilityQuestMobileInfo.ItemTypes);
		List<Type> mobTypes = new(HumilityQuestMobileInfo.MobileTypes);

		for (int i = 0; i < 25; i++)
		{
			int ran = Utility.RandomMinMax(1, itemTypes.Count - 2);

			Type t = itemTypes[ran];
			itemTypes.Remove(t);
			itemTypes.Insert(Utility.RandomMinMax(1, itemTypes.Count - 2), t);
		}

		for (int i = 0; i < 25; i++)
		{
			int ran = Utility.RandomMinMax(0, mobTypes.Count - 2);

			if (ran > 0)
			{
				Type t = mobTypes[ran];
				mobTypes.Remove(t);
				mobTypes.Insert(Utility.RandomMinMax(1, mobTypes.Count - 2), t);
			}
		}

		for (int i = 0; i < mobTypes.Count; i++)
		{
			int mobIndex = HumilityQuestMobileInfo.GetNPCIndex(mobTypes[i]);
			int need = i;
			int give = need + 1;

			Type needs = itemTypes[need];
			Type gives = itemTypes[give];

			Infos[mobIndex] = new HumilityQuestMobileInfo(needs, gives, HumilityQuestMobileInfo.GetLoc(needs), HumilityQuestMobileInfo.GetLoc(gives));
		}
	}

	public override void OnResign(bool chain)
	{
		foreach (var item in _mQuestItems.Where(item => item != null && !item.Deleted))
		{
			item.Delete();
		}

		base.OnResign(chain);
	}

	public override void GiveRewards()
	{
		foreach (var item in _mQuestItems.Where(item => item != null && !item.Deleted))
		{
			item.Delete();
		}

		Owner.SendGump(new QuestInfoGump(1075783));

		base.GiveRewards();
	}

	public override void RefuseRewards()
	{
		foreach (Item item in _mQuestItems)
		{
			if (item is GreyCloak)
			{
				/*((GreyCloak)item).Owner = Owner;*/
			}
			else if (item != null && !item.Deleted)
			{
				item.Delete();
			}
		}

		Owner.SendGump(new QuestInfoGump(1075784));

		base.RefuseRewards();
	}

	public void AddQuestItem(Item item, Mobile from)
	{
		if (!_mQuestItems.Contains(item))
		{
			_mQuestItems.Add(item);
		}

		OnGivenTo(from);
	}

	public void RemoveQuestItem(Item item)
	{
		if (_mQuestItems.Contains(item))
		{
			_mQuestItems.Remove(item);
		}
	}

	public void OnGivenTo(Mobile m)
	{
		_mGivenTo.Add(m);
	}

	public bool HasGivenTo(Mobile m)
	{
		return _mGivenTo.Contains(m);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(_mQuestItems.Count);
		foreach (Item item in _mQuestItems)
		{
			writer.Write(item);
		}

		writer.Write(Infos.Count);

		foreach (KeyValuePair<int, HumilityQuestMobileInfo> kvp in Infos)
		{
			writer.Write(kvp.Key);
			kvp.Value.Serialize(writer);
		}

		writer.Write(_mGivenTo.Count);
		foreach (Mobile m in _mGivenTo)
		{
			writer.Write(m);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		int count = reader.ReadInt();
		for (int i = 0; i < count; i++)
		{
			Item item = reader.ReadItem();

			if (item is {Deleted: false})
			{
				_mQuestItems.Add(item);
			}
		}

		count = reader.ReadInt();
		for (int i = 0; i < count; i++)
		{
			int mobIndex = reader.ReadInt();
			Infos[mobIndex] = new HumilityQuestMobileInfo(reader);
		}

		count = reader.ReadInt();
		for (int i = 0; i < count; i++)
		{
			Mobile m = reader.ReadMobile();
			if (m != null)
			{
				_mGivenTo.Add(m);
			}
		}
	}
}
