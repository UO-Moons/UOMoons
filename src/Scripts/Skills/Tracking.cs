using Server.Gumps;
using Server.Network;
using Server.Spells;
using Server.Spells.Necromancy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.SkillHandlers;

public class Tracking
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Tracking].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.SendLocalizedMessage(1011350); // What do you wish to track?

		m.CloseGump(typeof(TrackWhatGump));
		m.CloseGump(typeof(TrackWhoGump));
		m.SendGump(new TrackWhatGump(m));

		return TimeSpan.FromSeconds(10.0); // 10 second delay before begin able to re-use a skill
	}

	public class TrackingInfo
	{
		public Mobile MTracker;
		public Mobile MTarget;
		public Point2D MLocation;
		public Map MMap;

		public TrackingInfo(Mobile tracker, Mobile target)
		{
			MTracker = tracker;
			MTarget = target;
			MLocation = new Point2D(target.X, target.Y);
			MMap = target.Map;
		}
	}

	private static readonly Dictionary<Mobile, TrackingInfo> MTable = new();

	public static void AddInfo(Mobile tracker, Mobile target)
	{
		TrackingInfo info = new(tracker, target);
		MTable[tracker] = info;
	}

	public static double GetStalkingBonus(Mobile tracker, Mobile target)
	{
		MTable.TryGetValue(tracker, out TrackingInfo info);

		if (info == null || info.MTarget != target || info.MMap != target.Map)
			return 0.0;

		int xDelta = info.MLocation.X - target.X;
		int yDelta = info.MLocation.Y - target.Y;

		double bonus = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);

		MTable.Remove(tracker);    //Reset as of Pub 40, counting it as bg for Core.SE.

		return Core.ML ? Math.Min(bonus, 10 + tracker.Skills.Tracking.Value / 10) : bonus;
	}


	public static void ClearTrackingInfo(Mobile tracker)
	{
		MTable.Remove(tracker);
	}
}

public class TrackWhatGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly bool _mSuccess;

	public TrackWhatGump(Mobile from) : base(20, 30)
	{
		_mFrom = from;
		_mSuccess = from.CheckSkill(SkillName.Tracking, 0.0, 21.1);

		AddPage(0);

		AddBackground(0, 0, 440, 135, 5054);

		AddBackground(10, 10, 420, 75, 2620);
		AddBackground(10, 85, 420, 25, 3000);

		AddItem(20, 20, 9682);
		AddButton(20, 110, 4005, 4007, 1, GumpButtonType.Reply, 0);
		AddHtmlLocalized(20, 90, 100, 20, 1018087, false, false); // Animals

		AddItem(120, 20, 9607);
		AddButton(120, 110, 4005, 4007, 2, GumpButtonType.Reply, 0);
		AddHtmlLocalized(120, 90, 100, 20, 1018088, false, false); // Monsters

		AddItem(220, 20, 8454);
		AddButton(220, 110, 4005, 4007, 3, GumpButtonType.Reply, 0);
		AddHtmlLocalized(220, 90, 100, 20, 1018089, false, false); // Human NPCs

		AddItem(320, 20, 8455);
		AddButton(320, 110, 4005, 4007, 4, GumpButtonType.Reply, 0);
		AddHtmlLocalized(320, 90, 100, 20, 1018090, false, false); // Players
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (info.ButtonID is >= 1 and <= 4)
			TrackWhoGump.DisplayTo(_mSuccess, _mFrom, info.ButtonID - 1);
	}
}

public delegate bool TrackTypeDelegate(Mobile m);

public class TrackWhoGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly int _mRange;

	private static readonly TrackTypeDelegate[] MDelegates = {
		IsAnimal,
		IsMonster,
		IsHumanNpc,
		IsPlayer
	};

	private class InternalSorter : IComparer<Mobile>
	{
		private readonly Mobile _from;

		public InternalSorter(Mobile from)
		{
			_from = from;
		}

		public int Compare(Mobile x, Mobile y)
		{
			switch (x)
			{
				case null when y == null:
					return 0;
				case null:
					return -1;
			}

			return y == null ? 1 : _from.GetDistanceToSqrt(x).CompareTo(_from.GetDistanceToSqrt(y));
		}
	}

	public static void DisplayTo(bool success, Mobile from, int type)
	{
		if (!success)
		{
			from.SendLocalizedMessage(1018092); // You see no evidence of those in the area.
			return;
		}

		Map map = from.Map;

		if (map == null)
			return;

		TrackTypeDelegate check = MDelegates[type];

		from.CheckSkill(SkillName.Tracking, 21.1, 100.0); // Passive gain

		int range = 10 + (int)(from.Skills[SkillName.Tracking].Value / 10);

		List<Mobile> list = from.GetMobilesInRange(range).Where(m => m != from && (!Core.AOS || m.Alive) && (!m.Hidden || m.AccessLevel == AccessLevel.Player || from.AccessLevel > m.AccessLevel) && check(m) && CheckDifficulty(from, m)).ToList();

		if (list.Count > 0)
		{
			list.Sort(new InternalSorter(from));

			from.SendGump(new TrackWhoGump(from, list, range));
			from.SendLocalizedMessage(1018093); // Select the one you would like to track.
		}
		else
		{
			switch (type)
			{
				case 0:
					from.SendLocalizedMessage(502991); // You see no evidence of animals in the area.
					break;
				case 1:
					from.SendLocalizedMessage(502993); // You see no evidence of creatures in the area.
					break;
				default:
					from.SendLocalizedMessage(502995); // You see no evidence of people in the area.
					break;
			}
		}
	}

	// Tracking players uses tracking and detect hidden vs. hiding and stealth
	private static bool CheckDifficulty(Mobile from, Mobile m)
	{
		if (!Core.AOS || !m.Player)
			return true;



		int tracking = from.Skills[SkillName.Tracking].Fixed;
		int detectHidden = from.Skills[SkillName.DetectHidden].Fixed;

		if (Core.ML && m.Race == Race.Elf)
			tracking /= 2; //The 'Guide' says that it requires twice as Much tracking SKILL to track an elf.  Not the total difficulty to track.

		int hiding = m.Skills[SkillName.Hiding].Fixed;
		int stealth = m.Skills[SkillName.Stealth].Fixed;
		int divisor = hiding + stealth;

		// Necromancy forms affect tracking difficulty
		if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
			divisor -= 200;
		else if (TransformationSpellHelper.UnderTransformation(m, typeof(VampiricEmbraceSpell)) && divisor < 500)
			divisor = 500;
		else if (TransformationSpellHelper.UnderTransformation(m, typeof(WraithFormSpell)) && divisor <= 2000)
			divisor += 200;

		int chance;
		if (divisor > 0)
		{
			if (Core.SE)
				chance = 50 * (tracking * 2 + detectHidden) / divisor;
			else
				chance = 50 * (tracking + detectHidden + 10 * Utility.RandomMinMax(1, 20)) / divisor;
		}
		else
			chance = 100;

		return chance > Utility.Random(100);
	}

	private static bool IsAnimal(Mobile m)
	{
		return !m.Player && m.Body.IsAnimal;
	}

	private static bool IsMonster(Mobile m)
	{
		return (!m.Player && m.Body.IsMonster);
	}

	private static bool IsHumanNpc(Mobile m)
	{
		return !m.Player && m.Body.IsHuman;
	}

	private static bool IsPlayer(Mobile m)
	{
		return m.Player;
	}

	private readonly List<Mobile> _mList;

	private TrackWhoGump(Mobile from, List<Mobile> list, int range) : base(20, 30)
	{
		_mFrom = from;
		_mList = list;
		_mRange = range;

		AddPage(0);

		AddBackground(0, 0, 440, 155, 5054);

		AddBackground(10, 10, 420, 75, 2620);
		AddBackground(10, 85, 420, 45, 3000);

		if (list.Count > 4)
		{
			AddBackground(0, 155, 440, 155, 5054);

			AddBackground(10, 165, 420, 75, 2620);
			AddBackground(10, 240, 420, 45, 3000);

			if (list.Count > 8)
			{
				AddBackground(0, 310, 440, 155, 5054);

				AddBackground(10, 320, 420, 75, 2620);
				AddBackground(10, 395, 420, 45, 3000);
			}
		}

		for (int i = 0; i < list.Count && i < 12; ++i)
		{
			Mobile m = list[i];

			AddItem(20 + i % 4 * 100, 20 + i / 4 * 155, ShrinkTable.Lookup(m));
			AddButton(20 + i % 4 * 100, 130 + i / 4 * 155, 4005, 4007, i + 1, GumpButtonType.Reply, 0);

			if (m.Name != null)
				AddHtml(20 + i % 4 * 100, 90 + i / 4 * 155, 90, 40, m.Name, false, false);
		}
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		int index = info.ButtonID - 1;

		if (index >= 0 && index < _mList.Count && index < 12)
		{
			Mobile m = _mList[index];

			_mFrom.QuestArrow = new TrackArrow(_mFrom, m, _mRange * 2);

			if (Core.SE)
				Tracking.AddInfo(_mFrom, m);
		}
	}
}

public class TrackArrow : QuestArrow
{
	private Mobile _mFrom;
	private readonly Timer _mTimer;

	public TrackArrow(Mobile from, Mobile target, int range) : base(from, target)
	{
		_mFrom = from;
		_mTimer = new TrackTimer(from, target, range, this);
		_mTimer.Start();
	}

	public override void OnClick(bool rightClick)
	{
		if (rightClick)
		{
			Tracking.ClearTrackingInfo(_mFrom);

			_mFrom = null;

			Stop();
		}
	}

	public override void OnStop()
	{
		_mTimer.Stop();

		if (_mFrom != null)
		{
			Tracking.ClearTrackingInfo(_mFrom);

			_mFrom.SendLocalizedMessage(503177); // You have lost your quarry.
		}
	}
}

public class TrackTimer : Timer
{
	private readonly Mobile _mFrom, _mTarget;
	private readonly int _mRange;
	private int _mLastX, _mLastY;
	private readonly QuestArrow _mArrow;

	public TrackTimer(Mobile from, Mobile target, int range, QuestArrow arrow) : base(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(2.5))
	{
		_mFrom = from;
		_mTarget = target;
		_mRange = range;

		_mArrow = arrow;
	}

	protected override void OnTick()
	{
		if (!_mArrow.Running)
		{
			Stop();
			return;
		}

		if (_mFrom.NetState == null || _mFrom.Deleted || _mTarget.Deleted || _mFrom.Map != _mTarget.Map || !_mFrom.InRange(_mTarget, _mRange) || (_mTarget.Hidden && _mTarget.AccessLevel > _mFrom.AccessLevel))
		{
			_mArrow.Stop();
			Stop();
			return;
		}

		if (_mLastX != _mTarget.X || _mLastY != _mTarget.Y)
		{
			_mLastX = _mTarget.X;
			_mLastY = _mTarget.Y;

			_mArrow.Update();
		}
	}
}
