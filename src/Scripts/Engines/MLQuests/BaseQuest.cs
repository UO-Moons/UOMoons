using System;
using System.Collections.Generic;
using Server.Mobiles;

namespace Server.Engines.Quests
{
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

        public virtual QuestChain ChainID => QuestChain.None;
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

        private Timer m_Timer;
        public List<BaseObjective> Objectives { get; private set; }
        public List<BaseReward> Rewards { get; private set; }
        public PlayerMobile Owner { get; set; }
        public Type QuesterType { get; set; }

        public BaseQuestItem StartingItem => m_Quester is BaseQuestItem ? (BaseQuestItem)m_Quester : null;
        public MondainQuester StartingMobile => m_Quester is MondainQuester ? (MondainQuester)m_Quester : null;

        private object m_Quester;
        public object Quester
        {
            get => m_Quester;
            set
            {
                m_Quester = value;

                if (m_Quester != null)
                    QuesterType = m_Quester.GetType();
            }
        }

        public bool Completed
        {
            get
            {
                for (int i = 0; i < Objectives.Count; i++)
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
                for (int i = 0; i < Objectives.Count; i++)
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
            if (m_Timer != null)
                return;

            m_Timer = Timer.DelayCall(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), new TimerCallback(Slice));
        }

        public void StopTimer()
        {
            if (m_Timer != null)
                m_Timer.Stop();

            m_Timer = null;
        }

        public virtual void Slice()
        {
            for (int i = 0; i < Objectives.Count; i++)
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
            if (ChainID != QuestChain.None && StartingMobile != null)
            {
                if (Owner.Chains.ContainsKey(ChainID))
                {
                    BaseChain chain = Owner.Chains[ChainID];

                    chain.CurrentQuest = GetType();
                    chain.Quester = StartingMobile.GetType();
                }
                else
                {
                    Owner.Chains.Add(ChainID, new BaseChain(GetType(), StartingMobile.GetType()));
                }
            }
        }

        public virtual void OnAccept()
        {
            Owner.PlaySound(AcceptSound);
            Owner.SendLocalizedMessage(1049019); // You have accepted the Quest.
            Owner.Quests.Add(this);

            // give items if any		
            for (int i = 0; i < Objectives.Count; i++)
            {
                BaseObjective objective = Objectives[i];

                objective.OnAccept();
            }

            if (m_Quester is BaseEscort)
            {
                BaseEscort escort = (BaseEscort)m_Quester;

                if (escort.SetControlMaster(Owner))
                {
                    escort.Quest = this;
                    escort.LastSeenEscorter = DateTime.UtcNow;
                    escort.StartFollow();
                    escort.AddHash(Owner);

                    Region region = escort.GetDestination();

                    if (region != null)
                        escort.Say(1042806, region.Name); // Lead on! Payment will be made when we arrive at ~1_DESTINATION~!
                    else
                        escort.Say(1042806, "destination"); // Lead on! Payment will be made when we arrive at ~1_DESTINATION~!

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
            for (int i = 0; i < Objectives.Count; i++)
            {
                if (Objectives[i] is ObtainObjective)
                {
                    ObtainObjective obtain = (ObtainObjective)Objectives[i];

                    QuestHelper.RemoveStatus(Owner, obtain.Obtain);
                }
                else if (Objectives[i] is DeliverObjective)
                {
                    DeliverObjective deliver = (DeliverObjective)Objectives[i];

                    QuestHelper.DeleteItems(Owner, deliver.Delivery, deliver.MaxProgress, true);
                }
            }

            // delete escorter
            if (m_Quester is BaseEscort escort)
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
            for (int i = 0; i < Rewards.Count; i++)
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

                if (reward != null)
                {
                    if (reward.Stackable)
                    {
                        reward.Amount = Rewards[i].Amount;
                        Rewards[i].Amount = 1;
                    }

                    for (int j = 0; j < Rewards[i].Amount; j++)
                    {
                        if (!Owner.PlaceInBackpack(reward))
                        {
                            reward.MoveToWorld(Owner.Location, Owner.Map);
                        }

                        if (Rewards[i].Name is int)
                            Owner.SendLocalizedMessage(1074360, "#" + (int)Rewards[i].Name); // You receive a reward: ~1_REWARD~
                        else if (Rewards[i].Name is string)
                            Owner.SendLocalizedMessage(1074360, (string)Rewards[i].Name); // You receive a reward: ~1_REWARD~

                        // already marked, we need to see if this gives progress to another quest.
                        if (reward.QuestItem)
                        {
                            QuestHelper.CheckRewardItem(Owner, reward);
                        }
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
                BaseQuest quest = QuestHelper.RandomQuest(Owner, new Type[] { NextQuest }, StartingMobile);

                if (quest != null && quest.ChainID == ChainID)
                    Owner.SendGump(new MondainQuestGump(quest));
            }

            if (this is ITierQuest)
            {
                TierQuestInfo.CompleteQuest(Owner, (ITierQuest)this);
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
            if (NextQuest != null)
            {
                BaseQuest quest = QuestHelper.RandomQuest(Owner, new Type[] { NextQuest }, StartingMobile);

                if (quest != null && quest.ChainID == ChainID)
                    Owner.SendGump(new MondainQuestGump(quest));
            }
        }

        public virtual void AddObjective(BaseObjective objective)
        {
            if (Objectives == null)
                Objectives = new List<BaseObjective>();

            if (objective != null)
            {
                objective.Quest = this;
                Objectives.Add(objective);
            }
        }

        public virtual void AddReward(BaseReward reward)
        {
            if (Rewards == null)
                Rewards = new List<BaseReward>();

            if (reward != null)
            {
                reward.Quest = this;
                Rewards.Add(reward);
            }
        }

        public virtual void RemoveQuest()
        {
            RemoveQuest(false);
        }

        public virtual void RemoveQuest(bool removeChain)
        {
            StopTimer();

            if (removeChain)
                Owner.Chains.Remove(ChainID);

            if (Completed && (RestartDelay > TimeSpan.Zero || ForceRemember || DoneOnce) && NextQuest == null/*&& Owner.AccessLevel == AccessLevel.Player*/)
            {
                Type type = GetType();

                if (ChainID != QuestChain.None)
                    type = QuestHelper.FindFirstChainQuest(this);

                QuestHelper.Delay(Owner, type, RestartDelay);
            }

            QuestHelper.RemoveAcceleratedSkillgain(Owner);

            if (Owner.Quests.Contains(this))
            {
                Owner.Quests.Remove(this);
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
            writer.Write(QuesterType == null ? null : QuesterType.Name);

            if (m_Quester == null)
                writer.Write(0x0);
            else if (m_Quester is Mobile)
            {
                writer.Write(0x1);
                writer.Write((Mobile)m_Quester);
            }
            else if (m_Quester is Item)
            {
                writer.Write(0x2);
                writer.Write((Item)m_Quester);
            }

            for (int i = 0; i < Objectives.Count; i++)
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

            switch (reader.ReadInt())
            {
                case 0x0:
                    m_Quester = null;
                    break;
                case 0x1:
                    m_Quester = reader.ReadMobile() as MondainQuester;
                    break;
                case 0x2:
                    m_Quester = reader.ReadItem() as BaseQuestItem;
                    break;
            }

            if (m_Quester is BaseEscort escort)
            {
                escort.Quest = this;
            }
            else if (m_Quester is BaseQuestItem item)
            {
                item.Quest = this;
            }

            if (version == 0 && m_Quester != null)
            {
                QuesterType = m_Quester.GetType();
            }

            for (int i = 0; i < Objectives.Count; i++)
            {
                BaseObjective objective = Objectives[i];
                objective.Deserialize(reader);
            }
        }
    }
}
