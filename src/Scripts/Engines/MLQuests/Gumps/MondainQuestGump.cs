using System;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Engines.Quests;

public enum Buttons
{
	Close,
	CloseQuest,
	RefuseQuest,
	ResignQuest,
	AcceptQuest,
	AcceptReward,
	PreviousPage,
	NextPage,
	Complete,
	CompleteQuest,
	RefuseReward
}

public sealed class MondainQuestGump : BaseQuestGump
{
	private const int ButtonOffset = 11;
	private readonly object _mQuester;
	private readonly PlayerMobile _mFrom;
	private readonly BaseQuest _mQuest;
	private readonly bool _mOffer;
	private readonly bool _mCompleted;
	private Section _mSection;

	public MondainQuestGump(PlayerMobile from)
		: this(from, null, Section.Main, false, false)
	{
	}

	public MondainQuestGump(BaseQuest quest)
		: this(quest, Section.Description, true)
	{
	}

	public MondainQuestGump(BaseQuest quest, Section section, bool offer)
		: this(null, quest, section, offer, false)
	{
	}

	public MondainQuestGump(BaseQuest quest, Section section, bool offer, bool completed)
		: this(null, quest, section, offer, completed)
	{
	}

	public MondainQuestGump(PlayerMobile owner, BaseQuest quest, Section section, bool offer, bool completed)
		: this(owner, quest, section, offer, completed, null)
	{
	}

	public MondainQuestGump(PlayerMobile owner, BaseQuest quest, Section section, bool offer, bool completed, object quester)
		: base(75, 25)
	{
		_mQuester = quester;
		_mQuest = quest;
		_mSection = section;
		_mOffer = offer;
		_mCompleted = completed;

		_mFrom = quest != null ? quest.Owner : owner;

		Closable = false;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);

		AddImageTiled(50, 20, 400, 460, 0x1404);
		AddImageTiled(50, 29, 30, 450, 0x28DC);
		AddImageTiled(34, 140, 17, 339, 0x242F);
		AddImage(48, 135, 0x28AB);
		AddImage(-16, 285, 0x28A2);
		AddImage(0, 10, 0x28B5);
		AddImage(25, 0, 0x28B4);
		AddImageTiled(83, 15, 350, 15, 0x280A);
		AddImage(34, 479, 0x2842);
		AddImage(442, 479, 0x2840);
		AddImageTiled(51, 479, 392, 17, 0x2775);
		AddImageTiled(415, 29, 44, 450, 0xA2D);
		AddImageTiled(415, 29, 30, 450, 0x28DC);
		AddImage(370, 50, 0x589);

		if ((int)_mFrom.AccessLevel > (int)AccessLevel.Counselor && quest != null)
		{
			AddButton(379, 60, 0x15A9, 0x15A9, (int)Buttons.CompleteQuest, GumpButtonType.Reply, 0);
		}
		else
		{
			AddImage(379, 60, _mQuest == null ? 0x15A9 : 0x1580);
		}

		AddImage(425, 0, 0x28C9);
		AddImage(90, 33, 0x232D);
		AddImageTiled(130, 65, 175, 1, 0x238D);

		switch (_mSection)
		{
			case Section.Main:
				SecMain();
				break;
			case Section.Description:
				SecDescription();
				break;
			case Section.Objectives:
				SecObjectives();
				break;
			case Section.Rewards:
				SecRewards();
				break;
			case Section.Refuse:
				SecRefuse();
				break;
			case Section.Complete:
				SecComplete();
				break;
			case Section.InProgress:
				SecInProgress();
				break;
			case Section.Failed:
				SecFailed();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	public enum Section
	{
		Main,
		Description,
		Objectives,
		Rewards,
		Refuse,
		Complete,
		InProgress,
		Failed
	}

	public void SecMain()
	{
		if (_mFrom == null)
			return;

		AddHtmlLocalized(130, 45, 270, 16, 1046026, 0xFFFFFF, false, false); // Quest Log

		int offset = 140;

		for (var i = _mFrom.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = _mFrom.Quests[i];

			AddHtmlObject(98, offset, 270, 21, quest.Title, quest.Failed ? 0x3C00 : White, false, false);
			AddButton(368, offset, 0x26B0, 0x26B1, ButtonOffset + i, GumpButtonType.Reply, 0);

			offset += 21;
		}

		AddButton(313, 455, 0x2EEC, 0x2EEE, (int)Buttons.Close, GumpButtonType.Reply, 0);
	}

	public void SecDescription()
	{
		if (_mQuest == null)
			return;

		if (!_mQuest.RenderDescription(this, _mOffer))
		{
			AddHtmlLocalized(130, 45, 270, 16, _mOffer ? 1049010 : 1046026, 0xFFFFFF, false, false);

			if (_mQuest.Failed)
				AddHtmlLocalized(160, 80, 200, 32, 500039, 0x3C00, false, false); // Failed!

			AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);

			AddHtmlLocalized(98, 140, 312, 16, _mQuest.ChainId != QuestChain.None ? 1075024 : 1072202, 0x2710,
				false, false);

			AddHtmlObject(98, 156, 312, 180, _mQuest.Description, LightGreen, false, true);

			if (_mOffer)
			{
				AddButton(95, 455, 0x2EE0, 0x2EE2, (int)Buttons.AcceptQuest, GumpButtonType.Reply, 0);
				AddButton(313, 455, 0x2EF2, 0x2EF4, (int)Buttons.RefuseQuest, GumpButtonType.Reply, 0);
			}
			else
			{
				AddButton(95, 455, 0x2EF5, 0x2EF7, (int)Buttons.ResignQuest, GumpButtonType.Reply, 0);
				AddButton(313, 455, 0x2EEC, 0x2EEE, (int)Buttons.CloseQuest, GumpButtonType.Reply, 0);
			}

			if (_mQuest.ShowDescription)
				AddButton(275, 430, 0x2EE9, 0x2EEB, (int)Buttons.NextPage, GumpButtonType.Reply, 0);
		}
	}

	public void SecObjectives()
	{
		if (_mQuest == null)
			return;

		if (_mOffer)
		{
			AddButton(95, 455, 0x2EE0, 0x2EE2, (int)Buttons.AcceptQuest, GumpButtonType.Reply, 0);
			AddButton(313, 455, 0x2EF2, 0x2EF4, (int)Buttons.RefuseQuest, GumpButtonType.Reply, 0);
		}
		else
		{
			AddButton(95, 455, 0x2EF5, 0x2EF7, (int)Buttons.ResignQuest, GumpButtonType.Reply, 0);
			AddButton(313, 455, 0x2EEC, 0x2EEE, (int)Buttons.CloseQuest, GumpButtonType.Reply, 0);
		}

		if (_mQuest.RenderObjective(this, _mOffer)) return;
		AddHtmlLocalized(130, 45, 270, 16, _mOffer ? 1049010 : 1046026, 0xFFFFFF, false, false);

		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlLocalized(98, 140, 312, 16, 1049073, 0x2710, false, false); // Objective:

		AddHtmlLocalized(98, 156, 312, 16, _mQuest.AllObjectives ? 1072208 : 1072209, 0x2710, false, false);

		int offset = 172;

		for (var i = 0; i < _mQuest.Objectives.Count; i++)
		{
			BaseObjective objective = _mQuest.Objectives[i];

			switch (objective)
			{
				case SlayObjective slayObjective:
				{
					SlayObjective slay = slayObjective;

					if (slay != null)
					{
						AddHtmlLocalized(98, offset, 30, 16, 1072204, 0x15F90, false, false); // Slay	
						AddLabel(133, offset, 0x481, slay.MaxProgress + " " + slay.Name); // %count% + %name%

						offset += 16;

						if (_mOffer)
						{
							if (slay.Timed)
							{
								AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
								AddLabel(223, offset, 0x481, FormatSeconds(slay.Seconds)); // %est. time remaining%

								offset += 16;
							}
							continue;
						}

						if (slay.Region != null)
						{
							AddHtmlLocalized(103, offset, 312, 20, 1018327, 0x15F90, false, false); // Location
							AddHtmlObject(223, offset, 312, 20, slay.Region.Name, White, false, false); // %location%

							offset += 16;
						}

						AddHtmlLocalized(103, offset, 120, 16, 3000087, 0x15F90, false, false); // Total			
						AddLabel(223, offset, 0x481, slay.CurProgress.ToString());  // %current progress%

						offset += 16;

						if (ReturnTo() != null)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1074782, 0x15F90, false, false); // Return to	
							AddLabel(223, offset, 0x481, ReturnTo());  // %return to%		

							offset += 16;
						}

						if (slay.Timed)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
							AddLabel(223, offset, 0x481, FormatSeconds(slay.Seconds)); // %est. time remaining%

							offset += 16;
						}
					}

					break;
				}
				case ObtainObjective obtainObjective:
				{
					ObtainObjective obtain = obtainObjective;

					if (obtain != null)
					{
						AddHtmlLocalized(98, offset, 40, 16, 1072205, 0x15F90, false, false); // Obtain						
						AddLabel(143, offset, 0x481, obtain.MaxProgress + " " + obtain.Name); // %count% + %name%

						if (obtain.Image > 0)
							AddItem(350, offset, obtain.Image, obtain.Hue); // Image

						offset += 16;

						if (_mOffer)
						{
							if (obtain.Timed)
							{
								AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
								AddLabel(223, offset, 0x481, FormatSeconds(obtain.Seconds)); // %est. time remaining%

								offset += 16;
							}
							else if (obtain.Image > 0)
								offset += 16;

							continue;
						}
						AddHtmlLocalized(103, offset, 120, 16, 3000087, 0x15F90, false, false); // Total			
						AddLabel(223, offset, 0x481, obtain.CurProgress.ToString());    // %current progress%

						offset += 16;

						if (ReturnTo() != null)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1074782, 0x15F90, false, false); // Return to	
							AddLabel(223, offset, 0x481, ReturnTo());  // %return to%

							offset += 16;
						}

						if (obtain.Timed)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
							AddLabel(223, offset, 0x481, FormatSeconds(obtain.Seconds)); // %est. time remaining%

							offset += 16;
						}
					}

					break;
				}
				case DeliverObjective deliverObjective:
				{
					DeliverObjective deliver = deliverObjective;

					if (deliver != null)
					{
						AddHtmlLocalized(98, offset, 40, 16, 1072207, 0x15F90, false, false); // Deliver						
						AddLabel(143, offset, 0x481, deliver.MaxProgress + " " + deliver.DeliveryName);     // %name%

						offset += 16;

						AddHtmlLocalized(103, offset, 120, 16, 1072379, 0x15F90, false, false); // Deliver to						
						AddLabel(223, offset, 0x481, deliver.DestName); // %deliver to%

						offset += 16;

						if (deliver.Timed)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
							AddLabel(223, offset, 0x481, FormatSeconds(deliver.Seconds)); // %est. time remaining%

							offset += 16;
						}
					}

					break;
				}
				case EscortObjective escortObjective:
				{
					EscortObjective escort = escortObjective;

					if (escort != null)
					{
						AddHtmlLocalized(98, offset, 312, 16, 1072206, 0x15F90, false, false); // Escort to

						if (escort.Label == 0)
						{
							AddHtmlObject(173, offset, 200, 16, escort.Region.Name, White, false, false);
						}
						else
						{
							AddHtmlLocalized(173, offset, 200, 16, escort.Label, 0xFFFFFF, false, false);
						}

						offset += 16;

						if (escort.Timed)
						{
							AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
							AddLabel(223, offset, 0x481, FormatSeconds(escort.Seconds)); // %est. time remaining%

							offset += 16;
						}
					}

					break;
				}
				case ApprenticeObjective apprenticeObjective:
				{
					ApprenticeObjective apprentice = apprenticeObjective;

					if (apprentice != null)
					{
						AddHtmlLocalized(98, offset, 200, 16, 1077485, "#" + (1044060 + (int)apprentice.Skill) + "\t" + apprentice.MaxProgress, 0x15F90, false, false); // Increase ~1_SKILL~ to ~2_VALUE~

						offset += 16;
					}

					break;
				}
				case SimpleObjective {Descriptions: { }} baseObjective:
				{
					SimpleObjective obj = baseObjective;

					for (var j = 0; j < obj.Descriptions.Count; j++)
					{
						offset += 16;
						AddLabel(98, offset, 0x481, obj.Descriptions[j]);
					}

					if (obj.Timed)
					{
						offset += 16;
						AddHtmlLocalized(103, offset, 120, 16, 1062379, 0x15F90, false, false); // Est. time remaining:
						AddLabel(223, offset, 0x481, FormatSeconds(obj.Seconds)); // %est. time remaining%
					}

					break;
				}
				default:
				{
					if (objective.ObjectiveDescription != null)
					{
						switch (objective.ObjectiveDescription)
						{
							case int description:
								AddHtmlLocalized(98, offset, 310, 300, description, 0x15F90, false, false);
								break;
							case string description:
								AddHtmlObject(98, offset, 310, 300, description, LightGreen, false, false);
								break;
						}
					}

					break;
				}
			}
		}

		AddButton(130, 430, 0x2EEF, 0x2EF1, (int)Buttons.PreviousPage, GumpButtonType.Reply, 0);
		AddButton(275, 430, 0x2EE9, 0x2EEB, (int)Buttons.NextPage, GumpButtonType.Reply, 0);
	}

	public void SecRewards()
	{
		if (_mQuest == null)
			return;

		AddHtmlLocalized(130, 45, 270, 16, _mOffer ? 1049010 : 1046026, 0xFFFFFF, false, false);

		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlLocalized(98, 140, 312, 16, 1072201, 0x2710, false, false); // Reward	

		int offset = 163;

		for (var i = 0; i < _mQuest.Rewards.Count; i++)
		{
			BaseReward reward = _mQuest.Rewards[i];

			if (reward == null) continue;
			AddImage(105, offset, 0x4B9);
			AddHtmlObject(133, offset, 280, _mQuest.Rewards.Count == 1 ? 100 : 16, reward.Name, LightGreen, false, false);

			offset += 16;
		}

		if (_mCompleted)
		{
			AddButton(95, 455, 0x2EE0, 0x2EE2, (int)Buttons.AcceptReward, GumpButtonType.Reply, 0);

			if (_mQuest.CanRefuseReward)
				AddButton(313, 430, 0x2EF2, 0x2EF4, (int)Buttons.RefuseReward, GumpButtonType.Reply, 0);
			else
				AddButton(313, 455, 0x2EE6, 0x2EE8, (int)Buttons.Close, GumpButtonType.Reply, 0);
		}
		else if (_mOffer)
		{
			AddButton(95, 455, 0x2EE0, 0x2EE2, (int)Buttons.AcceptQuest, GumpButtonType.Reply, 0);
			AddButton(313, 455, 0x2EF2, 0x2EF4, (int)Buttons.RefuseQuest, GumpButtonType.Reply, 0);
			AddButton(130, 430, 0x2EEF, 0x2EF1, (int)Buttons.PreviousPage, GumpButtonType.Reply, 0);
		}
		else
		{
			AddButton(95, 455, 0x2EF5, 0x2EF7, (int)Buttons.ResignQuest, GumpButtonType.Reply, 0);
			AddButton(313, 455, 0x2EEC, 0x2EEE, (int)Buttons.CloseQuest, GumpButtonType.Reply, 0);
			AddButton(130, 430, 0x2EEF, 0x2EF1, (int)Buttons.PreviousPage, GumpButtonType.Reply, 0);
		}
	}

	public void SecRefuse()
	{
		if (_mQuest == null)
			return;

		if (!_mOffer) return;
		AddHtmlLocalized(130, 45, 270, 16, 3006156, 0xFFFFFF, false, false); // Quest Conversation
		AddImage(140, 110, 0x4B9);
		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlObject(98, 140, 312, 180, _mQuest.Refuse, LightGreen, false, true);

		AddButton(313, 455, 0x2EE6, 0x2EE8, (int)Buttons.Close, GumpButtonType.Reply, 0);
	}

	public void SecInProgress()
	{
		if (_mQuest == null)
			return;

		AddHtmlLocalized(130, 45, 270, 16, 3006156, 0xFFFFFF, false, false); // Quest Conversation				
		AddImage(140, 110, 0x4B9);
		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlObject(98, 140, 312, 180, _mQuest.Uncomplete, LightGreen, false, true);

		AddButton(313, 455, 0x2EE6, 0x2EE8, (int)Buttons.Close, GumpButtonType.Reply, 0);
	}

	public void SecComplete()
	{
		if (_mQuest == null)
			return;

		if (_mQuest.Complete == null)
		{
			if (!QuestHelper.TryDeleteItems(_mQuest)) return;
			if (QuestHelper.AnyRewards(_mQuest))
			{
				_mSection = Section.Rewards;
				SecRewards();
			}
			else
				_mQuest.GiveRewards();

			return;
		}

		AddHtmlLocalized(130, 45, 270, 16, 3006156, 0xFFFFFF, false, false); // Quest Conversation
		AddImage(140, 110, 0x4B9);
		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlObject(98, 140, 312, 180, _mQuest.Complete, LightGreen, false, true);

		AddButton(313, 455, 0x2EE6, 0x2EE8, (int)Buttons.Close, GumpButtonType.Reply, 0);
		AddButton(95, 455, 0x2EE9, 0x2EEB, (int)Buttons.Complete, GumpButtonType.Reply, 0);
	}

	public void SecFailed()
	{
		if (_mQuest == null)
			return;

		object fail = _mQuest.FailedMsg ?? "You have failed to meet the conditions of the quest.";

		AddHtmlLocalized(130, 45, 270, 16, 3006156, 0xFFFFFF, false, false); // Quest Conversation				
		AddImage(140, 110, 0x4B9);
		AddHtmlObject(160, 70, 200, 40, _mQuest.Title, DarkGreen, false, false);
		AddHtmlObject(98, 140, 312, 240, fail, LightGreen, false, true);

		AddButton(313, 455, 0x2EE6, 0x2EE8, (int)Buttons.Close, GumpButtonType.Reply, 0);
	}

	public string FormatSeconds(int seconds)
	{
		int hours = seconds / 3600;

		seconds -= hours * 3600;

		int minutes = seconds / 60;

		seconds -= minutes * 60;

		if (hours > 0 && minutes > 0)
			return hours + ":" + minutes + ":" + seconds;
		if (minutes > 0)
			return minutes + ":" + seconds;
		return seconds.ToString();
	}

	public string ReturnTo()
	{
		if (_mQuest?.StartingMobile == null) return null;
		string returnTo = _mQuest.StartingMobile.Name;

		returnTo = _mQuest.StartingMobile.Region != null ? $"{returnTo} ({_mQuest.StartingMobile.Region.Name})"
			: $"{returnTo}";

		return returnTo;

	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		_mFrom?.CloseGump(typeof(MondainQuestGump));

		switch (info.ButtonID)
		{
			// close quest list
			case (int)Buttons.Close:
				break;
			// close quest
			case (int)Buttons.CloseQuest:
				_mFrom?.SendGump(new MondainQuestGump(_mFrom));
				break;
			// accept quest
			case (int)Buttons.AcceptQuest:
				if (_mOffer)
					_mQuest.OnAccept();
				break;
			// refuse quest
			case (int)Buttons.RefuseQuest:
				if (_mOffer)
				{
					_mQuest.OnRefuse();
					_mFrom?.SendGump(new MondainQuestGump(_mQuest, Section.Refuse, true));
				}
				break;
			// resign quest
			case (int)Buttons.ResignQuest:
				if (!_mOffer)
					_mFrom?.SendGump(new MondainResignGump(_mQuest));
				break;
			// accept reward
			case (int)Buttons.AcceptReward:
				if (!_mOffer && _mSection == Section.Rewards && _mCompleted)
					_mQuest.GiveRewards();
				break;
			// refuse reward
			case (int)Buttons.RefuseReward:
				if (!_mOffer && _mSection == Section.Rewards && _mCompleted)
					_mQuest.RefuseRewards();
				break;
			// previous page
			case (int)Buttons.PreviousPage:
				if (_mSection == Section.Objectives || (_mSection == Section.Rewards && !_mCompleted))
				{
					_mSection = (Section)((int)_mSection - 1);
					_mFrom?.SendGump(new MondainQuestGump(_mQuest, _mSection, _mOffer));
				}
				break;
			// next page
			case (int)Buttons.NextPage:
				if (_mSection == Section.Description || _mSection == Section.Objectives)
				{
					_mSection = (Section)((int)_mSection + 1);
					_mFrom?.SendGump(new MondainQuestGump(_mQuest, _mSection, _mOffer));
				}
				break;
			// player complete quest
			case (int)Buttons.Complete:
				if (!_mOffer && _mSection == Section.Complete)
				{
					if (!_mQuest.Completed)
						_mFrom?.SendLocalizedMessage(1074861); // You do not have everything you need!
					else
					{
						if (QuestHelper.TryDeleteItems(_mQuest))
						{
							if (_mQuester != null)
								_mQuest.Quester = _mQuester;

							if (!QuestHelper.AnyRewards(_mQuest))
								_mQuest.GiveRewards();
							else
								_mFrom?.SendGump(new MondainQuestGump(_mQuest, Section.Rewards, false, true));
						}
						else
						{
							_mFrom?.SendLocalizedMessage(1074861); // You do not have everything you need!
						}
					}
				}
				break;
			// admin complete quest
			case (int)Buttons.CompleteQuest:
				if (_mFrom != null && (int)_mFrom.AccessLevel > (int)AccessLevel.Counselor && _mQuest != null)
					QuestHelper.CompleteQuest(_mFrom, _mQuest);
				break;
			// show quest
			default:
				if (_mSection != Section.Main || info.ButtonID >= _mFrom?.Quests.Count + ButtonOffset || info.ButtonID < ButtonOffset)
					break;

				_mFrom?.SendGump(new MondainQuestGump(_mFrom.Quests[info.ButtonID - ButtonOffset],
					Section.Description, false));
				break;
		}
	}
}
