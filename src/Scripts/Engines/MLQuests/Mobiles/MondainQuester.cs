using Server.Engines.Quests;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public abstract class MondainQuester : BaseVendor
{
	protected readonly List<SbInfo> MSbInfos = new();
	private DateTime _mSpoken;
	public MondainQuester()
		: base(null)
	{
		SpeechHue = 0x3B2;
	}

	public MondainQuester(string name)
		: this(name, null)
	{
	}

	public MondainQuester(string name, string title)
		: base(title)
	{
		Name = name;
		SpeechHue = 0x3B2;
	}

	public MondainQuester(Serial serial)
		: base(serial)
	{
	}

	public override void CheckMorph()
	{
		// Don't morph me!
	}
	public override bool IsActiveVendor => false;
	public override bool IsInvulnerable => true;
	public override bool DisallowAllMoves => false;
	public override bool ClickTitle => false;
	public override bool CanTeach => true;
	public virtual int AutoTalkRange => -1;
	public virtual int AutoSpeakRange => 10;
	public virtual TimeSpan SpeakDelay => TimeSpan.FromMinutes(1);
	public abstract Type[] Quests { get; }
	protected override List<SbInfo> SbInfos => MSbInfos;
	public override void InitSbInfo()
	{
	}

	public virtual void OnTalk(PlayerMobile player)
	{
		if (QuestHelper.DeliveryArrived(player, this))
			return;

		if (QuestHelper.InProgress(player, this))
			return;

		if (QuestHelper.QuestLimitReached(player))
			return;

		// check if this quester can offer any quest chain (already started)
		foreach (KeyValuePair<QuestChain, BaseChain> pair in player.Chains)
		{
			BaseChain chain = pair.Value;

			if (chain != null && chain.Quester != null && chain.Quester == GetType())
			{
				BaseQuest quest = QuestHelper.RandomQuest(player, new Type[] { chain.CurrentQuest }, this);

				if (quest != null)
				{
					_ = player.CloseGump(typeof(MondainQuestGump));
					_ = player.SendGump(new MondainQuestGump(quest));
					return;
				}
			}
		}

		BaseQuest questt = QuestHelper.RandomQuest(player, Quests, this);

		if (questt != null)
		{
			_ = player.CloseGump(typeof(MondainQuestGump));
			_ = player.SendGump(new MondainQuestGump(questt));
		}
	}

	public virtual void OnOfferFailed()
	{
		Say(1080107); // I'm sorry, I have nothing for you at this time.
	}

	public virtual void Advertise()
	{
		Say(Utility.RandomMinMax(1074183, 1074223));
	}

	public override bool CanBeDamaged()
	{
		return false;
	}

	public override void InitBody()
	{
		if (Race != null)
		{
			HairItemId = Race.RandomHair(Female);
			HairHue = Race.RandomHairHue();
			FacialHairItemId = Race.RandomFacialHair(Female);
			FacialHairHue = Race.RandomHairHue();
			Hue = Race.RandomSkinHue();
		}
	}

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (m.Alive && !m.Hidden && m is PlayerMobile mobile)
		{
			int range = AutoTalkRange;

			if (range >= 0 && InRange(m, range) && !InRange(oldLocation, range))
				OnTalk(mobile);

			range = AutoSpeakRange;

			if (InLOS(m) && range >= 0 && InRange(m, range) && !InRange(oldLocation, range) && DateTime.UtcNow >= _mSpoken + SpeakDelay)
			{
				if (Utility.Random(100) < 50)
					Advertise();

				_mSpoken = DateTime.UtcNow;
			}
		}
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m.Alive && m is PlayerMobile mobile)
			OnTalk(mobile);
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1072269); // Quest Giver
	}

	public void FocusTo(Mobile to)
	{
		QuestSystem.FocusTo(this, to);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		if (CantWalk)
			Frozen = true;
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		_mSpoken = DateTime.UtcNow;

		if (CantWalk)
			Frozen = true;
	}
}
