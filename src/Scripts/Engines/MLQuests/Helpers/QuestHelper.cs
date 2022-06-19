using System;
using System.Collections.Generic;
using System.Linq;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Regions;
using Server.Targeting;

namespace Server.Engines.Quests;

public class QuestHelper
{
	public static void Initialize()
	{
		EventSink.OnKilledBy += OnKilledBy;
	}

	public static void RemoveAcceleratedSkillgain(PlayerMobile from)
	{
		Region region = from.Region;

		while (region != null)
		{
			if ((region as ApprenticeRegion)?.Table[from] is BuffInfo)
			{
				BuffInfo.RemoveBuff(from, (BuffInfo)((ApprenticeRegion)region).Table[from]);
				((ApprenticeRegion)region).Table[from] = null;
			}

			region = region.Parent;
		}
	}

	public static BaseQuest RandomQuest(PlayerMobile from, Type[] quests, object quester)
	{
		return RandomQuest(from, quests, quester, quests is {Length: 1});
	}

	public static BaseQuest RandomQuest(PlayerMobile from, Type[] quests, object quester, bool message)
	{
		if (quests == null)
			return null;

		BaseQuest quest = null;

		if (quester is ITierQuester tierQuester)
		{
			quest = TierQuestInfo.RandomQuest(from, tierQuester);
		}
		else if (quests.Length > 0)
		{
			// give it 10 tries to generate quest
			for (var i = 0; i < 10; i++)
			{
				quest = Construct(quests[Utility.Random(quests.Length)]) as BaseQuest;
			}
		}

		if (quest == null) return null;
		quest.Owner = from;
		quest.Quester = quester;

		if (CanOffer(from, quest, quester, message))
		{
			return quest;
		}

		if (quester is not Mobile mobile || !message) return null;
		if (mobile is MondainQuester mondainQuester)
		{
			mondainQuester.OnOfferFailed();
		}
		else
		{
			mobile.Say(1080107); // I'm sorry, I have nothing for you at this time.
		}

		return null;
	}

	public static bool CanOffer(PlayerMobile from, BaseQuest quest, object quester, bool message)
	{
		if (!quest.CanOffer())
			return false;

		if (quest.ChainId != QuestChain.None)
		{
			// if a player wants to start quest chain (already started) again (not osi)
			if (from.Chains.ContainsKey(quest.ChainId) && FirstChainQuest(quest, quest.Quester))
			{
				return false;
			}
			// if player already has an active quest from the chain

			if (InChainProgress(from, quest))
			{
				return false;
			}
		}

		if (!Delayed(from, quest, quester, message))
			return false;

		for (var i = quest.Objectives.Count - 1; i >= 0; i--)
		{
			Type type = quest.Objectives[i].Type();

			if (type == null)
				continue;

			for (int j = from.Quests.Count - 1; j >= 0; j--)
			{
				BaseQuest pQuest = from.Quests[j];

				for (var k = pQuest.Objectives.Count - 1; k >= 0; k--)
				{
					BaseObjective obj = pQuest.Objectives[k];

					if (type == obj.Type() && (quest.ChainId == QuestChain.None || quest.ChainId == pQuest.ChainId))
						return false;
				}
			}
		}

		return true;
	}

	public static bool Delayed(PlayerMobile player, BaseQuest quest, object quester, bool message)
	{
		var restartInfo = GetRestartInfo(player, quest.GetType());

		if (restartInfo != null)
		{
			if (quest.DoneOnce)
			{
				if (message && quester is Mobile)
				{
					((Mobile)quester).Say(1075454); // I can not offer you the quest again.
				}

				return false;
			}

			DateTime endTime = restartInfo.RestartTime;

			if (DateTime.UtcNow < endTime)
			{
				if (!message || quester is not Mobile) return false;
				var ts = endTime - DateTime.UtcNow;
				string str;

				if (ts.TotalDays > 1)
					str = $"I cannot offer this quest again for about {ts.TotalDays} more days.";
				else if (ts.TotalHours > 1)
					str = $"I cannot offer this quest again for about {ts.TotalHours} more hours.";
				else if (ts.TotalMinutes > 1)
					str = $"I cannot offer this quest again for about {ts.TotalMinutes} more minutes.";
				else
					str = "I can offer this quest again very soon.";

				((Mobile)quester).SayTo(player, false, str);

				return false;
			}

			if (quest.RestartDelay > TimeSpan.Zero)
			{
				player.DoneQuests.Remove(restartInfo);
			}

			return true;
		}

		return true;
	}

	public static QuestRestartInfo GetRestartInfo(PlayerMobile pm, Type quest)
	{
		return pm.DoneQuests.FirstOrDefault(ri => ri.QuestType == quest);
	}

	public static bool CheckDoneOnce(PlayerMobile player, BaseQuest quest, Mobile quester, bool message)
	{
		return quest.DoneOnce && CheckDoneOnce(player, quest.GetType(), quester, message);
	}

	public static bool CheckDoneOnce(PlayerMobile player, Type questType, Mobile quester, bool message)
	{
		if (player.DoneQuests.All(x => x.QuestType != questType)) return false;
		if (message && quester != null)
		{
			quester.SayTo(player, 1075454, 0x3B2); // I can not offer you the quest again.
		}

		return true;

	}

	public static bool TryReceiveQuestItem(PlayerMobile player, Type type, TimeSpan delay)
	{
		if (!type.IsSubclassOf(typeof(Item))) return false;
		var info = player.DoneQuests.FirstOrDefault(x => x.QuestType == type);

		if (info != null)
		{
			DateTime endTime = info.RestartTime;

			if (DateTime.UtcNow < endTime)
			{
				TimeSpan ts = endTime - DateTime.UtcNow;

				if (ts.Days > 0)
				{
					player.SendLocalizedMessage(1158377, string.Format("{0}\t{1}", ts.Days.ToString(), "day[s]"));
				}
				else if (ts.Hours > 0)
				{
					player.SendLocalizedMessage(1158377, string.Format("{0}\t{1}", ts.Hours.ToString(), "hour[s]"));
				}
				else
				{
					player.SendLocalizedMessage(1158377, string.Format("{0}\t{1}", ts.Minutes.ToString(), "minute[s]"));
				}

				return false;
			}

			info.Reset(delay);
		}
		else
		{
			player.DoneQuests.Add(new QuestRestartInfo(type, delay));
		}

		return true;

	}

	public static void Delay(PlayerMobile player, Type type, TimeSpan delay)
	{
		var restartInfo = GetRestartInfo(player, type);

		if (restartInfo != null)
		{
			restartInfo.Reset(delay);
			return;
		}

		player.DoneQuests.Add(new QuestRestartInfo(type, delay));
	}

	/// <summary>
	/// Called in BaseQuestItem.cs
	/// </summary>
	/// <param name="player"></param>
	/// <param name="quests"></param>
	/// <returns></returns>
	public static bool InProgress(PlayerMobile player, Type[] quests)
	{
		if (quests == null)
			return false;

		var quest = player.Quests.FirstOrDefault(q => quests.Any(questerType => questerType == q.GetType()));

		if (quest == null) return false;
		if (quest.Completed)
		{
			player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
		}
		else
		{
			player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
			quest.InProgress();
		}

		return true;

		/*for (int i = 0; i < quests.Length; i ++)
		{ 
		    for (int j = 0; j < player.Quests.Count; j ++)
		    {
		        BaseQuest quest = player.Quests[j];
				
		        if (quests[i].IsAssignableFrom(quest.GetType()))
		        {
		            if (quest.Completed)
		            {
		                player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
		            }
		            else
		            {
		                player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
		                quest.InProgress();
		            }
						
		            return true;
		        }
		    }
		}*/

	}

	/// <summary>
	/// Called in MondainQuester.cs
	/// </summary>
	/// <param name="player"></param>
	/// <param name="quester"></param>
	/// <returns></returns>
	public static bool InProgress(PlayerMobile player, Mobile quester)
	{
		var quest = player.Quests.FirstOrDefault(q => q.QuesterType == quester.GetType());

		if (quest == null) return false;
		if (quest.Completed)
		{
			if (quest.Complete == null && !AnyRewards(quest))
				quest.GiveRewards();
			else
				player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
		}
		else
		{
			player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
			quest.InProgress();
		}

		return true;

		/*for (int i = 0; i < player.Quests.Count; i ++)
		{
		    BaseQuest quest = player.Quests[i];
			
		    if (quest.Quester == null && quest.QuesterType == null)
		        continue;

		    if (quest.QuesterType == quester.GetType())
		    {
		        if (quest.Completed)		
		        {
		            if (quest.Complete == null && !AnyRewards(quest))
		                quest.GiveRewards();
		            else 
		                player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
		        }
		        else
		        {
		            player.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.InProgress, false));
		            quest.InProgress();
		        }
					
		        return true;
		    }
		}*/

	}

	public static bool AnyRewards(BaseQuest quest)
	{
		for (var i = 0; i < quest.Rewards.Count; i++)
		{
			BaseReward reward = quest.Rewards[i];

			if (reward.Type != null)
				return true;
		}

		return false;
	}

	public static bool DeliveryArrived(PlayerMobile player, BaseVendor vendor)
	{
		for (var i = 0; i < player.Quests.Count; i++)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = 0; j < quest.Objectives.Count; j++)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not DeliverObjective) continue;
				DeliverObjective deliver = (DeliverObjective)objective;

				if (!deliver.Update(vendor)) continue;
				if (!quest.Completed) return true;
				player.SendLocalizedMessage(1046258, null, 0x23); // Your quest is complete.												
				player.PlaySound(quest.CompleteSound);

				quest.OnCompleted();

				player.SendGump(vendor is MondainQuester quester
					? new MondainQuestGump(player, quest, MondainQuestGump.Section.Complete, false, true,
						quester)
					: new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));

				return true;
			}
		}

		return false;
	}

	public static bool QuestLimitReached(PlayerMobile player)
	{
		if (player.Quests.Count < 10) return false;
		player.SendLocalizedMessage(1075141); // You are too busy with other tasks at this time.
		return true;

	}

	public static bool FirstChainQuest(BaseQuest quest, object quester)
	{
		return quest != null && BaseChain.Chains[(int)quest.ChainId] != null && BaseChain.Chains[(int)quest.ChainId].Length > 0 && BaseChain.Chains[(int)quest.ChainId][0] == quest.GetType();
	}

	public static Type FindFirstChainQuest(BaseQuest quest)
	{
		if (quest == null || quest.ChainId == QuestChain.None || BaseChain.Chains[(int)quest.ChainId] == null || BaseChain.Chains[(int)quest.ChainId].Length == 0)
			return null;

		return BaseChain.Chains[(int)quest.ChainId][0];
	}

	public static bool InChainProgress(PlayerMobile pm, BaseQuest quest)
	{
		return pm.Quests.Any(q => q.ChainId != QuestChain.None && q.ChainId == quest.ChainId && q.GetType() != quest.GetType());
	}

	public static Region FindRegion(string name)
	{
		if (name == null)
			return null;

		if (Map.Trammel.Regions.TryGetValue(name, out var reg))
			return reg;

		if (Map.Felucca.Regions.TryGetValue(name, out reg))
			return reg;

		if (Map.Ilshenar.Regions.TryGetValue(name, out reg))
			return reg;

		if (Map.Malas.Regions.TryGetValue(name, out reg))
			return reg;

		if (Map.Tokuno.Regions.TryGetValue(name, out reg))
			return reg;

		return Map.TerMur.Regions.TryGetValue(name, out reg) ? reg : reg;
	}

	public static void CompleteQuest(PlayerMobile from, BaseQuest quest)
	{
		if (quest == null)
			return;

		for (var i = 0; i < quest.Objectives.Count; i++)
		{
			BaseObjective obj = quest.Objectives[i];

			obj.Complete();
		}

		from.SendLocalizedMessage(1046258, null, 0x23); // Your quest is complete.							
		from.SendGump(new MondainQuestGump(quest, MondainQuestGump.Section.Complete, false, true));
		from.PlaySound(quest.CompleteSound);

		quest.OnCompleted();
	}

	public static void DeleteItems(PlayerMobile from, Type itemType, int amount, bool questItem)
	{
		if (from.Backpack == null || itemType == null || amount <= 0)
			return;

		Item[] items = from.Backpack.FindItemsByType(itemType);

		int deleted = 0;

		for (var i = items.Length - 1; i >= 0 && deleted < amount; i--)
		{
			Item item = items[i];

			if (!item.QuestItem && questItem) continue;
			item.QuestItem = false;

			if (deleted + item.Amount > amount)
			{
				item.Amount -= amount - deleted;
				deleted += amount - deleted;
			}
			else
			{
				item.Delete();
				deleted += item.Amount;
			}
		}

		if (deleted < amount)
		{
			for (var i = from.Items.Count - 1; i >= 0 && deleted < amount; i--)
			{
				Item item = from.Items[i];

				if (!item.QuestItem && questItem) continue;
				if (!itemType.IsInstanceOfType(item)) continue;
				deleted += item.Amount;

				item.Delete();
			}
		}
	}

	public static void DeleteItems(BaseQuest quest)
	{
		for (var i = 0; i < quest.Objectives.Count; i++)
		{
			BaseObjective objective = quest.Objectives[i];

			DeleteItems(quest.Owner, objective.Type(), objective.MaxProgress, true);
			RemoveStatus(quest.Owner, objective.Type());
		}
	}

	public static bool TryDeleteItems(BaseQuest quest)
	{
		if (quest == null)
			return false;

		bool complete = false;

		for (int i = 0; i < quest.Objectives.Count && !complete; i++)
		{
			switch (quest.Objectives[i])
			{
				case ObtainObjective:
				{
					ObtainObjective obtain = (ObtainObjective)quest.Objectives[i];

					if (CountQuestItems(quest.Owner, obtain.Obtain) >= obtain.MaxProgress)
					{
						if (!quest.AllObjectives)
						{
							complete = true;
						}
					}
					else
					{
						return false;
					}

					break;
				}
				case DeliverObjective:
				{
					DeliverObjective deliver = (DeliverObjective)quest.Objectives[i];

					if (quest.StartingItem != null)
						continue;
					if (deliver.MaxProgress > CountQuestItems(quest.Owner, deliver.Delivery))
					{
						quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
						deliver.Fail();

						return false;
					}

					break;
				}
			}
		}

		DeleteItems(quest);

		return true;
	}

	public static int CountQuestItems(PlayerMobile from, Type type)
	{
		int count = 0;

		if (from.Backpack == null)
			return count;

		Item[] items = from.Backpack.FindItemsByType(type);

		for (var i = 0; i < items.Length; i++)
		{
			Item item = items[i];

			if (item.QuestItem)
				count += item.Amount;
		}

		return count;
	}

	public static int RemoveStatus(PlayerMobile from, Type type)
	{
		if (type == null)
			return 0;

		Item[] items = from.Backpack.FindItemsByType(type);

		int count = 0;

		for (var i = 0; i < items.Length; i++)
		{
			Item item = items[i];

			if (!item.QuestItem) continue;
			count += 1;
			item.QuestItem = false;
		}

		return count;
	}

	public static void RemoveStatus(PlayerMobile from, Item item)
	{
		for (var i = from.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = from.Quests[i];

			for (var j = quest.Objectives.Count - 1; j >= 0; j--)
			{
				switch (quest.Objectives[j])
				{
					case ObtainObjective:
					{
						ObtainObjective obtain = (ObtainObjective)quest.Objectives[j];

						if (obtain.Obtain != null && obtain.Obtain.IsInstanceOfType(item))
						{
							obtain.CurProgress -= item.Amount;
							item.QuestItem = false;
							from.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item. 	
							return;
						}

						break;
					}
					case DeliverObjective:
					{
						DeliverObjective deliver = (DeliverObjective)quest.Objectives[j];

						if (deliver.Delivery != null && deliver.Delivery.IsInstanceOfType(item))
						{
							from.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.							
							DeleteItems(from, deliver.Delivery, deliver.MaxProgress, false);
							deliver.Fail();
							item.Delete();
							return;
						}

						break;
					}
				}
			}
		}
	}

	public static void OnKilledBy(Mobile m, Mobile k)
	{
		if (m is PlayerMobile mobile)
		{
			CheckCreature(mobile, k);
		}
	}

	public static bool CheckCreature(PlayerMobile player, Mobile creature)
	{
		for (var i = player.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = quest.Objectives.Count - 1; j >= 0; j--)
			{
				if (quest.Objectives[j] is not SlayObjective) continue;
				SlayObjective slay = (SlayObjective)quest.Objectives[j];

				if (!slay.Update(creature)) continue;
				if (quest.Completed)
					quest.OnCompleted();
				else if (slay.Completed)
					player.PlaySound(quest.UpdateSound);

				return true;
			}
		}

		return false;
	}

	public static bool CheckItem(PlayerMobile player, Item item)
	{
		for (var i = player.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = quest.Objectives.Count - 1; j >= 0; j--)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not ObtainObjective obtain) continue;

				if (!obtain.Update(item)) continue;
				if (quest.Completed)
					quest.OnCompleted();
				else if (obtain.Completed)
					player.PlaySound(quest.UpdateSound);

				return true;
			}
		}

		return false;
	}

	public static bool CheckRewardItem(PlayerMobile player, Item item)
	{
		foreach (var quest in player.Quests.Where(q => q.Objectives.Any(obj => obj is ObtainObjective)))
		{
			foreach (var obtain in quest.Objectives.OfType<ObtainObjective>())
			{
				if (!obtain.IsObjective(item)) continue;
				obtain.CurProgress += item.Amount;
				quest.OnObjectiveUpdate(item);

				return true;
			}
		}

		return false;
	}

	public static bool CheckSkill(PlayerMobile player, Skill skill)
	{
		for (var i = player.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = quest.Objectives.Count - 1; j >= 0; j--)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not ApprenticeObjective apprentice) continue;

				if (!apprentice.Update(skill)) continue;
				if (quest.Completed)
					quest.OnCompleted();
				else if (apprentice.Completed)
					player.PlaySound(quest.UpdateSound);
			}
		}

		return false;
	}

	public static bool EnhancedSkill(PlayerMobile player, Skill skill)
	{
		if (player == null || player.Region == null || skill == null)
			return false;

		for (var i = player.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = player.Quests[i];

			for (var j = quest.Objectives.Count - 1; j >= 0; j--)
			{
				BaseObjective objective = quest.Objectives[j];

				if (objective is not ApprenticeObjective apprentice || apprentice.Completed) continue;

				if (apprentice.Region == null) continue;
				if (player.Region.IsPartOf(apprentice.Region) && skill.SkillName == apprentice.Skill)
					return true;
			}
		}

		return false;
	}

	public static object Construct(Type type)
	{
		if (type == null)
			return null;

		try
		{
			return Activator.CreateInstance(type);
		}
		catch
		{
			return null;
		}
	}

	public static void StartTimer(PlayerMobile player)
	{
		if (player?.Quests == null)
			return;

		for (var i = player.Quests.Count - 1; i >= 0; i--)
			player.Quests[i].StartTimer();
	}

	public static void StopTimer(PlayerMobile player)
	{
		if (player?.Quests == null)
			return;

		for (var i = player.Quests.Count - 1; i >= 0; i--)
			player.Quests[i].StopTimer();
	}

	public static void GetContextMenuEntries(List<ContextMenuEntry> list)
	{
		list?.Add(new SelectQuestItem());
	}

	public static bool FindCompletedQuest(PlayerMobile from, Type type, bool delete)
	{
		if (type == null)
			return false;

		for (var i = from.DoneQuests.Count - 1; i >= 0; i--)
		{
			QuestRestartInfo restartInfo = from.DoneQuests[i];

			if (restartInfo.QuestType != type) continue;
			if (delete)
				from.DoneQuests.RemoveAt(i);

			return true;
		}

		return false;
	}

	public static bool HasQuest<T>(PlayerMobile from) where T : BaseQuest
	{
		return GetQuest(from, typeof(T)) != null;
	}

	public static bool HasQuest(PlayerMobile from, Type t)
	{
		return GetQuest(from, t) != null;
	}

	public static BaseQuest GetQuest(PlayerMobile from, Type type)
	{
		if (type == null)
			return null;

		for (var i = from.Quests.Count - 1; i >= 0; i--)
		{
			BaseQuest quest = from.Quests[i];

			if (quest.GetType() == type)
				return quest;
		}

		return null;
	}

	public static T GetQuest<T>(PlayerMobile pm) where T : BaseQuest
	{
		return pm.Quests.FirstOrDefault(quest => quest.GetType() == typeof(T)) as T;
	}
}

public class SelectQuestItem : ContextMenuEntry
{
	public SelectQuestItem()
		: base(6169)
	{
	}

	public override void OnClick()
	{
		if (!Owner.From.Alive)
			return;

		Owner.From.SendLocalizedMessage(1072352); // Target the item you wish to toggle Quest Item status on <ESC> to cancel			
		Owner.From.BeginTarget(-1, false, TargetFlags.None, ToggleQuestItem_Callback);
	}

	private static void ToggleQuestItem_Callback(Mobile from, object obj)
	{
		if (from is not PlayerMobile player) return;

		if (obj is Item item)
		{
			if (item.Parent != null && item.Parent == player.Backpack)
			{
				if (!QuestHelper.CheckItem(player, item))
					player.SendLocalizedMessage(1072355, null, 0x23); // That item does not match any of your quest criteria
			}
			else
				player.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
		}
		else
			player.SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.

		player.BeginTarget(-1, false, TargetFlags.None, ToggleQuestItem_Callback);
	}
}
