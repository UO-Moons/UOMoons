using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Quests;

public class BaseObjective
{
	private int _mCurProgress;
	private int _mSeconds;

	public BaseObjective()
		: this(1, 0)
	{
	}

	public BaseObjective(int maxProgress)
		: this(maxProgress, 0)
	{
	}

	public BaseObjective(int maxProgress, int seconds)
	{
		MaxProgress = maxProgress;
		_mSeconds = seconds;

		Timed = seconds > 0;
	}

	public BaseQuest Quest { get; set; }
	public int MaxProgress { get; set; }
	public int CurProgress
	{
		get => _mCurProgress;
		set
		{
			_mCurProgress = value;

			if (Completed)
				OnCompleted();

			if (_mCurProgress == -1)
				OnFailed();

			if (_mCurProgress < -1)
				_mCurProgress = -1;
		}
	}
	public int Seconds
	{
		get => _mSeconds;
		set
		{
			_mSeconds = value;

			if (_mSeconds < 0)
				_mSeconds = 0;
		}
	}
	public bool Timed { get; set; }
	public bool Completed => CurProgress >= MaxProgress;
	public bool Failed => CurProgress == -1;

	public virtual object ObjectiveDescription => null;

	public virtual void Complete()
	{
		CurProgress = MaxProgress;
	}

	public virtual void Fail()
	{
		CurProgress = -1;
	}

	public virtual void OnAccept()
	{
	}

	public virtual void OnCompleted()
	{
	}

	public virtual void OnFailed()
	{
	}

	public virtual Type Type()
	{
		return null;
	}

	public virtual bool Update(object obj)
	{
		return false;
	}

	public virtual void UpdateTime()
	{
		if (!Timed || Failed)
			return;

		if (Seconds > 0)
		{
			Seconds -= 1;
		}
		else if (!Completed)
		{
			Quest.Owner.SendLocalizedMessage(1072258); // You failed to complete an objective in time!

			Fail();
		}
	}

	public virtual void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0);

		writer.Write(_mCurProgress);
		writer.Write(_mSeconds);
	}

	public virtual void Deserialize(GenericReader reader)
	{
		_ = reader.ReadEncodedInt();

		_mCurProgress = reader.ReadInt();
		_mSeconds = reader.ReadInt();
	}
}

public class SlayObjective : BaseObjective
{
	public SlayObjective(Type creature, string name, int amount)
		: this(new[] { creature }, name, amount, null, 0)
	{
	}

	public SlayObjective(Type creature, string name, int amount, string region)
		: this(new[] { creature }, name, amount, region, 0)
	{
	}

	public SlayObjective(Type creature, string name, int amount, int seconds)
		: this(new[] { creature }, name, amount, null, seconds)
	{
	}

	public SlayObjective(string name, int amount, params Type[] creatures)
		: this(creatures, name, amount, null, 0)
	{
	}

	public SlayObjective(string name, int amount, string region, params Type[] creatures)
		: this(creatures, name, amount, region, 0)
	{
	}

	public SlayObjective(string name, int amount, int seconds, params Type[] creatures)
		: this(creatures, name, amount, null, seconds)
	{
	}

	public SlayObjective(Type[] creatures, string name, int amount, string region, int seconds)
		: base(amount, seconds)
	{
		Creatures = creatures;
		Name = name;

		if (region == null) return;
		Region = QuestHelper.FindRegion(region);

		if (Region == null)
			Console.WriteLine($"Invalid region name ('{region}') in '{GetType()}' objective!");
	}

	public Type[] Creatures { get; set; }
	public string Name { get; set; }
	public Region Region { get; set; }

	public virtual void OnKill(Mobile killed)
	{
		if (Completed)
			Quest.Owner.SendLocalizedMessage(1075050); // You have killed all the required quest creatures of this type.
		else
			Quest.Owner.SendLocalizedMessage(1075051, (MaxProgress - CurProgress).ToString()); // You have killed a quest creature. ~1_val~ more left.
	}

	public virtual bool IsObjective(Mobile mob)
	{
		if (Creatures == null)
			return false;

		if (Creatures.Any(type => type.IsInstanceOfType(mob)))
		{
			return Region == null || Region.Contains(mob.Location);
		}

		return false;
	}

	public override bool Update(object obj)
	{
		if (obj is not Mobile mob) return false;
		if (!IsObjective(mob)) return false;
		if (!Completed)
			CurProgress += 1;

		OnKill(mob);
		return true;

	}

	public override Type Type()
	{
		return Creatures is {Length: > 0} ? Creatures[0] : null;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class ObtainObjective : BaseObjective
{
	public ObtainObjective(Type obtain, string name, int amount)
		: this(obtain, name, amount, 0, 0)
	{
	}

	public ObtainObjective(Type obtain, string name, int amount, int image)
		: this(obtain, name, amount, image, 0)
	{
	}

	public ObtainObjective(Type obtain, string name, int amount, int image, int seconds)
		: this(obtain, name, amount, image, seconds, 0)
	{
	}

	public ObtainObjective(Type obtain, string name, int amount, int image, int seconds, int hue)
		: base(amount, seconds)
	{
		Obtain = obtain;
		Name = name;
		Image = image;
		Hue = hue;
	}

	public Type Obtain { get; set; }
	public string Name { get; set; }
	public int Image { get; set; }
	public int Hue { get; set; }
	public override bool Update(object obj)
	{
		if (obj is Item obtained)
		{
			if (IsObjective(obtained))
			{
				if (!obtained.QuestItem)
				{
					CurProgress += obtained.Amount;

					obtained.QuestItem = true;
					Quest.Owner.SendLocalizedMessage(1072353); // You set the item to Quest Item status

					Quest.OnObjectiveUpdate(obtained);
				}
				else
				{
					CurProgress -= obtained.Amount;

					obtained.QuestItem = false;
					Quest.Owner.SendLocalizedMessage(1072354); // You remove Quest Item status from the item
				}

				return true;
			}
		}

		return false;
	}

	public virtual bool IsObjective(Item item)
	{
		return Obtain != null && Obtain.IsInstanceOfType(item);
	}

	public override Type Type()
	{
		return Obtain;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class DeliverObjective : BaseObjective
{
	public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName)
		: this(delivery, deliveryName, amount, destination, destName, 0)
	{
	}

	public DeliverObjective(Type delivery, string deliveryName, int amount, Type destination, string destName, int seconds)
		: base(amount, seconds)
	{
		Delivery = delivery;
		DeliveryName = deliveryName;

		Destination = destination;
		DestName = destName;
	}

	public Type Delivery { get; set; }
	public string DeliveryName { get; set; }
	public Type Destination { get; set; }
	public string DestName { get; set; }
	public override void OnAccept()
	{
		if (Quest.StartingItem != null)
		{
			Quest.StartingItem.QuestItem = true;
			return;
		}

		int amount = MaxProgress;

		while (amount > 0 && !Failed)
		{
			if (QuestHelper.Construct(Delivery) is not Item item)
			{
				Fail();
				break;
			}

			if (item.Stackable)
			{
				item.Amount = amount;
				amount = 1;
			}

			if (!Quest.Owner.PlaceInBackpack(item))
			{
				Quest.Owner.SendLocalizedMessage(503200); // You do not have room in your backpack for 
				Quest.Owner.SendLocalizedMessage(1075574); // Could not create all the necessary items. Your quest has not advanced.

				Fail();

				break;
			}

			item.QuestItem = true;

			amount -= 1;
		}

		if (!Failed) return;
		QuestHelper.DeleteItems(Quest.Owner, Delivery, MaxProgress - amount, false);

		Quest.RemoveQuest();
	}

	public override bool Update(object obj)
	{
		if (Delivery == null || Destination == null)
			return false;

		if (Failed)
		{
			Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.
			return false;
		}

		if (obj is not BaseVendor) return false;
		if (Quest.StartingItem != null)
		{
			Complete();
			return true;
		}

		if (!Destination.IsInstanceOfType(obj)) return false;
		if (MaxProgress < QuestHelper.CountQuestItems(Quest.Owner, Delivery))
		{
			Quest.Owner.SendLocalizedMessage(1074813);  // You have failed to complete your delivery.						
			Fail();
		}
		else
			Complete();

		return true;

	}

	public override Type Type()
	{
		return Delivery;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class EscortObjective : BaseObjective
{
	public Region Region { get; set; }
	public int Fame { get; set; }
	public int Compassion { get; set; }
	public int Label { get; set; }

	public EscortObjective(string region)
		: this(region, 10, 200, 0, 0)
	{
	}

	public EscortObjective(int label, string region)
		: this(region, 10, 200, 0, label)
	{
	}

	public EscortObjective(string region, int fame)
		: this(region, fame, 200)
	{
	}

	public EscortObjective(string region, int fame, int compassion)
		: this(region, fame, compassion, 0, 0)
	{
	}

	public EscortObjective(string region, int fame, int compassion, int seconds, int label)
		: base(1, seconds)
	{
		Region = QuestHelper.FindRegion(region);
		Fame = fame;
		Compassion = compassion;
		Label = label;

		if (Region == null)
			Console.WriteLine($"Invalid region name ('{region}') in '{GetType()}' objective!");
	}

	public override void OnCompleted()
	{
		if (Quest != null && Quest.Owner != null && Quest.Owner.Murderer && Quest.Owner.DoneQuests.FirstOrDefault(info => info.QuestType == typeof(ResponsibilityQuest)) == null)
		{
			QuestHelper.Delay(Quest.Owner, typeof(ResponsibilityQuest), Quest.RestartDelay);
		}

		base.OnCompleted();
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class ApprenticeObjective : BaseObjective
{
	public ApprenticeObjective(SkillName skill, int cap)
		: this(skill, cap, null, null, null)
	{
	}

	public ApprenticeObjective(SkillName skill, int cap, string region, object enterRegion, object leaveRegion)
		: base(cap)
	{
		Skill = skill;

		if (region != null)
		{
			Region = QuestHelper.FindRegion(region);
			Enter = enterRegion;
			Leave = leaveRegion;

			if (Region == null)
				Console.WriteLine($"Invalid region name ('{region}') in '{GetType()}' objective!");
		}
	}

	public SkillName Skill { get; set; }
	public Region Region { get; set; }
	public object Enter { get; set; }
	public object Leave { get; set; }
	public override bool Update(object obj)
	{
		if (Completed)
			return false;

		if (obj is not Skill skill) return false;
		if (skill.SkillName != Skill)
			return false;

		if (!(Quest.Owner.Skills[Skill].Base >= MaxProgress)) return false;
		Complete();
		return true;

	}

	public override void OnAccept()
	{
		Region region = Quest.Owner.Region;

		while (region != null)
		{
			if (region is ApprenticeRegion)
				region.OnEnter(Quest.Owner);

			region = region.Parent;
		}
	}

	public override void OnCompleted()
	{
		QuestHelper.RemoveAcceleratedSkillgain(Quest.Owner);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class QuestionAndAnswerObjective : BaseObjective
{
	private int _currentIndex;

	private readonly List<int> _mDone = new();
	private readonly QuestionAndAnswerEntry[] _mEntryTable;

	public virtual QuestionAndAnswerEntry[] EntryTable => _mEntryTable;

	public QuestionAndAnswerObjective(int count, QuestionAndAnswerEntry[] table)
		: base(count)
	{
		_mEntryTable = table;
		_currentIndex = -1;
	}

	public QuestionAndAnswerEntry GetRandomQandA()
	{
		if (_mEntryTable == null || _mEntryTable.Length == 0 || _mEntryTable.Length - _mDone.Count <= 0)
			return null;

		if (_currentIndex >= 0 && _currentIndex < _mEntryTable.Length)
		{
			return _mEntryTable[_currentIndex];
		}

		int ran;

		do
		{
			ran = Utility.Random(_mEntryTable.Length);
		}
		while (_mDone.Contains(ran));

		_currentIndex = ran;
		return _mEntryTable[ran];
	}

	public override bool Update(object obj)
	{
		_mDone.Add(_currentIndex);
		_currentIndex = -1;

		if (!Completed)
			CurProgress++;

		return true;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(1);

		writer.Write(_currentIndex);

		writer.Write(_mDone.Count);
		for (var i = 0; i < _mDone.Count; i++)
			writer.Write(_mDone[i]);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		if (version > 0)
		{
			_currentIndex = reader.ReadInt();
		}

		int c = reader.ReadInt();
		for (var i = 0; i < c; i++)
			_mDone.Add(reader.ReadInt());
	}
}
