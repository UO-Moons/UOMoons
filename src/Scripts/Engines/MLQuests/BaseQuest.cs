using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Quests;

public class BaseQuest
{
	public virtual bool AllObjectives => true;
	public virtual bool DoneOnce => false;
	public virtual TimeSpan RestartDelay => TimeSpan.Zero;
	public virtual bool ForceRemember => false;

	public virtual int AcceptSound => 0x5B4;
	public virtual int ResignSound => 0x5B3;
	public virtual int CompleteSound => 0x5B5;
	public virtual int UpdateSound => 0x5B6;

	public virtual int CompleteMessage => 1072273; // You've completed a quest!  Don't forget to collect your reward.

	public virtual QuestChain ChainId => QuestChain.None;
	public virtual Type NextQuest => null;

	public virtual object Title => null;
	public virtual object Description => null;
	public virtual object Refuse => null;
	public virtual object Uncomplete => null;
	public virtual object Complete => null;

	public virtual object FailedMsg => null;

	public virtual bool ShowDescription => true;
	public virtual bool ShowRewards => true;
	public virtual bool CanRefuseReward => false;

	private Timer _mTimer;
	public List<BaseObjective> Objectives { get; private set; }
	public List<BaseReward> Rewards { get; private set; }
	public PlayerMobile Owner { get; set; }
	public Type QuesterType { get; set; }

	public BaseQuestItem StartingItem => _mQuester is BaseQuestItem item ? item : null;
	public MondainQuester StartingMobile => _mQuester is MondainQuester quester ? quester : null;

	private object _mQuester;
	public object Quester
	{
		get => _mQuester;
		set
		{
			_mQuester = value;

			if (_mQuester != null)
				QuesterType = _mQuester.GetType();
		}
	}

	public bool Completed
	{
		get
		{
			for (var i = 0; i < Objectives.Count; i++)
			{
				if (Objectives[i].Completed)
				{
					if (!AllObjectives)
						return true;
				}
				else
				{
					if (AllObjectives)
						return false;
				}
			}

			return AllObjectives;
		}
	}

	public bool Failed
	{
		get
		{
			for (var i = 0; i < Objectives.Count; i++)
			{
				if (Objectives[i].Failed)
				{
					if (AllObjectives)
						return true;
				}
				else
				{
					if (!AllObjectives)
						return false;
				}
			}

			return !AllObjectives;
		}
	}

	public BaseQuest()
	{
		Objectives = new List<BaseObjective>();
		Rewards = new List<BaseReward>();
	}

	public void StartTimer()
	{
		if (_mTimer != null)
			return;

		_mTimer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), Slice);
	}

	public void StopTimer()
	{
		_mTimer?.Stop();

		_mTimer = null;
	}

	public virtual void Slice()
	{
		for (var i = 0; i < Objectives.Count; i++)
		{
			BaseObjective obj = Objectives[i];

			obj.UpdateTime();
		}
	}

	public virtual void OnObjectiveUpdate(Item item)
	{
	}

	public virtual bool CanOffer()
	{
		return true;
	}

	public virtual void UpdateChain()
	{
		if (ChainId == QuestChain.None || StartingMobile == null) return;
		if (Owner.Chains.ContainsKey(ChainId))
		{
			BaseChain chain = Owner.Chains[ChainId];

			chain.CurrentQuest = GetType();
			chain.Quester = StartingMobile.GetType();
		}
		else
		{
			Owner.Chains.Add(ChainId, new BaseChain(GetType(), StartingMobile.GetType()));
		}
	}

	public virtual void OnAccept()
	{
		Owner.PlaySound(AcceptSound);
		Owner.SendLocalizedMessage(1049019); // You have accepted the Quest.
		Owner.Quests.Add(this);

		// give items if any		
		for (var i = 0; i < Objectives.Count; i++)
		{
			BaseObjective objective = Objectives[i];

			objective.OnAccept();
		}

		if (_mQuester is BaseEscort escort)
		{
			if (escort.SetControlMaster(Owner))
			{
				escort.Quest = this;
				escort.LastSeenEscorter = DateTime.UtcNow;
				escort.StartFollow();
				escort.AddHash(Owner);

				Region region = escort.GetDestination();

				escort.Say(1042806, region != null ? region.Name : "destination");// Lead on! Payment will be made when we arrive at ~1_DESTINATION~!

				Owner.LastEscortTime = DateTime.UtcNow;
			}
		}

		// tick tack	
		StartTimer();
	}

	public virtual void OnRefuse()
	{
		if (!QuestHelper.FirstChainQuest(this, Quester))
			UpdateChain();
	}

	public virtual void OnResign(bool resignChain)
	{
		Owner.PlaySound(ResignSound);

		// update chain
		if (!resignChain && !QuestHelper.FirstChainQuest(this, Quester))
			UpdateChain();

		// delete items	that were given on quest start
		for (var i = 0; i < Objectives.Count; i++)
		{
			switch (Objectives[i])
			{
				case ObtainObjective obtain:
					_ = QuestHelper.RemoveStatus(Owner, obtain.Obtain);
					break;
				case DeliverObjective deliver:
					QuestHelper.DeleteItems(Owner, deliver.Delivery, deliver.MaxProgress, true);
					break;
			}
		}

		// delete escorter
		if (_mQuester is BaseEscort escort)
		{
			escort.Say(1005653); // Hmmm.  I seem to have lost my master.
			escort.PlaySound(0x5B3);
			escort.BeginDelete(Owner);
		}

		RemoveQuest(resignChain);
	}

	public virtual void InProgress()
	{
	}

	public virtual void OnCompleted()
	{
		Owner.SendLocalizedMessage(CompleteMessage, null, 0x23); // You've completed a quest!  Don't forget to collect your reward.							
		Owner.PlaySound(CompleteSound);
	}

	public virtual void GiveRewards()
	{
		// give rewards
		for (var i = 0; i < Rewards.Count; i++)
		{
			Type type = Rewards[i].Type;

			Rewards[i].GiveReward();

			if (type == null)
				continue;

			Item reward;

			try
			{
				reward = Activator.CreateInstance(type) as Item;
			}
			catch
			{
				reward = null;
			}

			if (reward == null) continue;
			if (reward.Stackable)
			{
				reward.Amount = Rewards[i].Amount;
				Rewards[i].Amount = 1;
			}

			for (var j = 0; j < Rewards[i].Amount; j++)
			{
				if (!Owner.PlaceInBackpack(reward))
				{
					reward.MoveToWorld(Owner.Location, Owner.Map);
				}

				switch (Rewards[i].Name)
				{
					case int @int:
						Owner.SendLocalizedMessage(1074360, "#" + @int); // You receive a reward: ~1_REWARD~
						break;
					case string @string:
						Owner.SendLocalizedMessage(1074360, @string); // You receive a reward: ~1_REWARD~
						break;
				}

				// already marked, we need to see if this gives progress to another quest.
				if (reward.QuestItem)
				{
					_ = QuestHelper.CheckRewardItem(Owner, reward);
				}
			}
		}

		// remove quest
		if (NextQuest == null)
			RemoveQuest(true);
		else
			RemoveQuest();

		// offer next quest if present
		if (NextQuest != null)
		{
			BaseQuest quest = QuestHelper.RandomQuest(Owner, new[] { NextQuest }, StartingMobile);

			if (quest != null && quest.ChainId == ChainId)
				_ = Owner.SendGump(new MondainQuestGump(quest));
		}

		if (this is ITierQuest quest1)
		{
			TierQuestInfo.CompleteQuest(Owner, quest1);
		}

		//EventSink.InvokeOnQuestComplete = QuestCompleteEventArgs;
		EventSink.InvokeOnQuestComplete(Owner, GetType());
	}

	public virtual void RefuseRewards()
	{
		// remove quest
		if (NextQuest == null)
			RemoveQuest(true);
		else
			RemoveQuest();

		// offer next quest if present
		if (NextQuest == null) return;
		BaseQuest quest = QuestHelper.RandomQuest(Owner, new[] { NextQuest }, StartingMobile);

		if (quest != null && quest.ChainId == ChainId)
			_ = Owner.SendGump(new MondainQuestGump(quest));
	}

	public virtual void AddObjective(BaseObjective objective)
	{
		Objectives ??= new List<BaseObjective>();

		if (objective == null) return;
		objective.Quest = this;
		Objectives.Add(objective);
	}

	public virtual void AddReward(BaseReward reward)
	{
		Rewards ??= new List<BaseReward>();

		if (reward == null) return;
		reward.Quest = this;
		Rewards.Add(reward);
	}

	public virtual void RemoveQuest()
	{
		RemoveQuest(false);
	}

	public virtual void RemoveQuest(bool removeChain)
	{
		StopTimer();

		if (removeChain)
			_ = Owner.Chains.Remove(ChainId);

		if (Completed && (RestartDelay > TimeSpan.Zero || ForceRemember || DoneOnce) && NextQuest == null/*&& Owner.AccessLevel == AccessLevel.Player*/)
		{
			Type type = GetType();

			if (ChainId != QuestChain.None)
				type = QuestHelper.FindFirstChainQuest(this);

			QuestHelper.Delay(Owner, type, RestartDelay);
		}

		QuestHelper.RemoveAcceleratedSkillgain(Owner);

		if (Owner.Quests.Contains(this))
		{
			_ = Owner.Quests.Remove(this);
		}
	}

	public virtual bool RenderDescription(MondainQuestGump g, bool offer)
	{
		return false;
	}

	public virtual bool RenderObjective(MondainQuestGump g, bool offer)
	{
		return false;
	}

	public virtual void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(1);
		writer.Write(QuesterType?.Name);

		switch (_mQuester)
		{
			case null:
				writer.Write(0x0);
				break;
			case Mobile mobile:
				writer.Write(0x1);
				writer.Write(mobile);
				break;
			case Item item:
				writer.Write(0x2);
				writer.Write(item);
				break;
		}

		for (var i = 0; i < Objectives.Count; i++)
		{
			BaseObjective objective = Objectives[i];
			objective.Serialize(writer);
		}
	}

	public virtual void Deserialize(GenericReader reader)
	{
		int version = reader.ReadEncodedInt();

		if (version > 0)
		{
			string questerType = reader.ReadString();

			if (questerType != null)
				QuesterType = Assembler.FindTypeByName(questerType);
		}

		_mQuester = reader.ReadInt() switch
		{
			0x0 => null,
			0x1 => reader.ReadMobile() as MondainQuester,
			0x2 => reader.ReadItem() as BaseQuestItem,
			_ => _mQuester
		};

		switch (_mQuester)
		{
			case BaseEscort escort:
				escort.Quest = this;
				break;
			case BaseQuestItem item:
				item.Quest = this;
				break;
		}

		if (version == 0 && _mQuester != null)
		{
			QuesterType = _mQuester.GetType();
		}

		for (var i = 0; i < Objectives.Count; i++)
		{
			BaseObjective objective = Objectives[i];
			objective.Deserialize(reader);
		}
	}
}
