using Server.Gumps;
using Server.Spells.Fifth;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class HeritageQuestInfo
{
	public Type Quest { get; }
	public object Title { get; }

	public HeritageQuestInfo(Type quest, object title)
	{
		Quest = quest;
		Title = title;
	}

	public bool Check(PlayerMobile player)
	{
		return Check(player, false);
	}

	public bool Check(PlayerMobile player, bool delete)
	{
		int j = 0;

		while (j < player.DoneQuests.Count && player.DoneQuests[j].QuestType != Quest)
		{
			//if(player.Murderer && this.m_Quest == typeof(ResponsibilityQuest)  && player.DoneQuests[j].QuestType.IsSubclassOf(typeof(

			j += 1;
		}

		if (j == player.DoneQuests.Count)
			return false;
		if (delete)
			player.DoneQuests.RemoveAt(j);

		return true;
	}
}

public abstract class HeritageQuester : BaseVendor
{
	#region Vendor stuff

	protected override List<SbInfo> SbInfos { get; } = new();

	public override bool IsActiveVendor => false;
	public override void InitSbInfo()
	{
	}

	#endregion

	public virtual int AutoSpeakRange => 7;
	public virtual object ConfirmMessage => 0;
	public virtual object IncompleteMessage => 0;

	private bool _mBusy;
	private int _mIndex;

	public List<HeritageQuestInfo> Quests { get; private set; }

	public List<object> Objectives { get; private set; }

	public List<object> Story { get; private set; }

	public HeritageQuester()
		: this(null)
	{
	}

	public HeritageQuester(string name)
		: this(name, null)
	{
	}

	public HeritageQuester(string name, string title)
		: base(title)
	{
		Quests = new List<HeritageQuestInfo>();
		Objectives = new List<object>();
		Story = new List<object>();

		Initialize();

		Name = name;
		SpeechHue = 0x3B2;
	}

	public HeritageQuester(Serial serial)
		: base(serial)
	{
	}

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (m.Alive && !m.Hidden && m is PlayerMobile)
		{
			int range = AutoSpeakRange;

			if (range >= 0 && InRange(m, range) && !InRange(oldLocation, range))
				OnTalk(m);
		}
	}

	public override void OnDoubleClick(Mobile m)
	{
		Console.WriteLine(m.Items.Count);

		if (m.Alive)
			OnTalk(m);
	}

	public virtual void OnTalk(Mobile m)
	{
		if (m.Hidden || _mBusy || m.Race == Race)
		{
			m.SendLocalizedMessage(1074017); // He's too busy right now, so he ignores you.
			return;
		}

		_mBusy = true;
		_mIndex = 0;

		SpeechHue = Utility.RandomDyedHue();
		Say(m.Name);
		SpeechHue = 0x3B2;

		if (CheckCompleted(m))
			_ = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(10), Story.Count + 1, new TimerStateCallback(SayStory), m);
		else
		{
			List<object> incomplete = FindIncompleted(m);
			TimeSpan delay = TimeSpan.FromSeconds(2);

			if (incomplete.Count == Quests.Count + 1)
			{
				incomplete = Objectives;
				delay = TimeSpan.FromSeconds(10);
			}

			_ = Timer.DelayCall(TimeSpan.Zero, delay, incomplete.Count, new TimerStateCallback(SayInstructions), incomplete);
		}
	}

	public bool CheckCompleted(Mobile m)
	{
		return CheckCompleted(m, false);
	}

	public bool CheckCompleted(Mobile m, bool delete)
	{
		for (int i = 0; i < Quests.Count; i += 1)
		{
			HeritageQuestInfo info = Quests[i];

			if (!info.Check((PlayerMobile)m, delete))
				return false;
		}

		return true;
	}

	public List<object> FindIncompleted(Mobile m)
	{
		List<object> incomplete = new()
		{
			IncompleteMessage
		};

		for (int i = 0; i < Quests.Count; i += 1)
		{
			HeritageQuestInfo info = Quests[i];

			if (!info.Check((PlayerMobile)m))
				incomplete.Add(info.Title);
		}

		return incomplete;
	}

	private void SayInstructions(object args)
	{
		if (args is List<object> list)
			SayInstructions(list);
	}

	public void SayInstructions(List<object> incomplete)
	{
		Say(this, incomplete[_mIndex]);

		_mIndex += 1;

		if (_mIndex == incomplete.Count)
			_mBusy = false;
	}

	private void SayStory(object args)
	{
		if (args is Mobile mobile)
			SayStory(mobile);
	}

	public void SayStory(Mobile m)
	{
		if (_mIndex < Story.Count)
			Say(this, Story[_mIndex]);
		else
		{
			_mBusy = false;

			_ = m.CloseGump(typeof(ConfirmHeritageQuestGump));
			_ = m.SendGump(new ConfirmHeritageQuestGump(this));
		}

		_mIndex += 1;
	}

	#region Static
	private static readonly Dictionary<Mobile, HeritageQuester> m_Pending = new();

	public static void AddPending(Mobile m, HeritageQuester quester)
	{
		m_Pending[m] = quester;
	}

	public static void RemovePending(Mobile m)
	{
		if (m_Pending.ContainsKey(m))
		{
			_ = m_Pending.Remove(m);
		}
	}

	public static bool IsPending(Mobile m)
	{
		return m_Pending.ContainsKey(m) && m_Pending[m] != null;
	}

	public static HeritageQuester Pending(Mobile m)
	{
		return m_Pending.ContainsKey(m) ? m_Pending[m] : null;
	}

	public static void Say(Mobile m, object message)
	{
		if (message is int @int)
			m.Say(@int);
		else if (message is string @string)
			m.Say(@string);
	}

	public static bool Check(Mobile m)
	{
		if (!m.Alive)
			m.SendLocalizedMessage(1073646); // Only the living may proceed...			
		else if (m.Mounted)
			m.SendLocalizedMessage(1073647); // You may not continue while mounted...			
		else if (m.IsBodyMod || m.HueMod > 0 || !m.CanBeginAction(typeof(IncognitoSpell)))
			m.SendLocalizedMessage(1073648); // You may only proceed while in your original state...						
		else if (m.Spell != null && m.Spell.IsCasting)
			m.SendLocalizedMessage(1073649); // One may not proceed while embracing magic...			
		else if (IsUnburdened(m))
			m.SendLocalizedMessage(1073650); // To proceed you must be unburdened by equipment...
		else if (!m.NetState.SupportsExpansion(Expansion.ML))
			m.SendLocalizedMessage(1073651); // You must have Mondain's Legacy before proceeding...
		else if (m.Hits < m.HitsMax)
			m.SendLocalizedMessage(1073652); // You must be healthy to proceed...				
		else
			return true;

		return false;
	}

	public static bool IsUnburdened(Mobile m)
	{
		int count = m.Items.Count - 1;

		if (m.Backpack != null)
			count -= 1;

		return count > 0;
	}

	#endregion

	public virtual void Initialize()
	{
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

		Quests = new List<HeritageQuestInfo>();
		Objectives = new List<object>();
		Story = new List<object>();

		Initialize();
	}
}
