using Server.Engines.Quests;
using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class Ryuichi : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof(TheArtOfStealthQuest)
	};

	public override bool IsActiveVendor => true;
	public override void InitSbInfo()
	{
		MSbInfos.Add(new SBKeeperOfNinjitsu());
	}

	[Constructable]
	public Ryuichi()
		: base("Ryuichi", "the Ninjitsu Instructor")
	{
		SetSkill(SkillName.Hiding, 120.0, 120.0);
		SetSkill(SkillName.Tracking, 120.0, 120.0);
		SetSkill(SkillName.Healing, 120.0, 120.0);
		SetSkill(SkillName.Tactics, 120.0, 120.0);
		SetSkill(SkillName.Fencing, 120.0, 120.0);
		SetSkill(SkillName.Stealth, 120.0, 120.0);
		SetSkill(SkillName.Ninjitsu, 120.0, 120.0);
	}

	public Ryuichi(Serial serial)
		: base(serial)
	{
	}

	public override void Advertise()
	{
		Say(1078155); // I can teach you Ninjitsu. The Art of Stealth.
	}

	public override void OnOfferFailed()
	{
		Say(1077772); // I cannot teach you, for you know all I can teach!
	}

	public override void InitBody()
	{
		Female = false;
		CantWalk = true;
		Race = Race.Human;

		base.InitBody();
	}

	public override void InitOutfit()
	{
		AddItem(new Backpack());
		AddItem(new SamuraiTabi());
		AddItem(new LeatherNinjaPants());
		AddItem(new LeatherNinjaHood());
		AddItem(new LeatherNinjaBelt());
		AddItem(new LeatherNinjaMitts());
		AddItem(new LeatherNinjaJacket());
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

public class SBKeeperOfNinjitsu : SbInfo
{
	private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
	private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();
	public SBKeeperOfNinjitsu()
	{
	}

	public override IShopSellInfo SellInfo => m_SellInfo;
	public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(BookOfNinjitsu), 500, 20, 0x23A0, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
		}
	}
}
