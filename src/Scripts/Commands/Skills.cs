using Server.Targeting;
using System;
using System.Globalization;

namespace Server.Commands;

public class SkillsCommand
{
	public static void Initialize()
	{
		CommandSystem.Register("SetSkill", AccessLevel.GameMaster, SetSkill_OnCommand);
		CommandSystem.Register("GetSkill", AccessLevel.GameMaster, GetSkill_OnCommand);
		CommandSystem.Register("SetAllSkills", AccessLevel.GameMaster, SetAllSkills_OnCommand);
	}

	[Usage("SetSkill <name> <value>")]
	[Description("Sets a skill value by name of a targeted mobile.")]
	public static void SetSkill_OnCommand(CommandEventArgs arg)
	{
		if (arg.Length != 2)
		{
			arg.Mobile.SendMessage("SetSkill <skill name> <value>");
		}
		else
		{
			if (Enum.TryParse(arg.GetString(0), true, out SkillName skill))
			{
				arg.Mobile.Target = new SkillTarget(skill, arg.GetDouble(1));
			}
			else
			{
				arg.Mobile.SendLocalizedMessage(1005631); // You have specified an invalid skill to set.
			}
		}
	}

	[Usage("SetAllSkills <name> <value>")]
	[Description("Sets all skill values of a targeted mobile.")]
	public static void SetAllSkills_OnCommand(CommandEventArgs arg)
	{
		if (arg.Length != 1)
		{
			arg.Mobile.SendMessage("SetAllSkills <value>");
		}
		else
		{
			arg.Mobile.Target = new AllSkillsTarget(arg.GetDouble(0));
		}
	}

	[Usage("GetSkill <name>")]
	[Description("Gets a skill value by name of a targeted mobile.")]
	public static void GetSkill_OnCommand(CommandEventArgs arg)
	{
		if (arg.Length != 1)
		{
			arg.Mobile.SendMessage("GetSkill <skill name>");
		}
		else
		{
			if (Enum.TryParse(arg.GetString(0), true, out SkillName skill))
			{
				arg.Mobile.Target = new SkillTarget(skill);
			}
			else
			{
				arg.Mobile.SendMessage("You have specified an invalid skill to get.");
			}
		}
	}

	public class AllSkillsTarget : Target
	{
		private readonly double _mValue;

		public AllSkillsTarget(double value) : base(-1, false, TargetFlags.None)
		{
			_mValue = value;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Mobile targ)
			{
				Server.Skills skills = targ.Skills;

				for (int i = 0; i < skills.Length; ++i)
					skills[i].Base = _mValue;

				CommandLogging.LogChangeProperty(from, targ, "EverySkill.Base", _mValue.ToString());
			}
			else
			{
				from.SendMessage("That does not have skills!");
			}
		}
	}

	public class SkillTarget : Target
	{
		private readonly bool _mSet;
		private readonly SkillName _mSkill;
		private readonly double _mValue;

		public SkillTarget(SkillName skill, double value) : base(-1, false, TargetFlags.None)
		{
			_mSet = true;
			_mSkill = skill;
			_mValue = value;
		}

		public SkillTarget(SkillName skill) : base(-1, false, TargetFlags.None)
		{
			_mSet = false;
			_mSkill = skill;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Mobile)
			{
				Mobile targ = (Mobile)targeted;
				Skill skill = targ.Skills[_mSkill];

				if (skill == null)
					return;

				if (_mSet)
				{
					skill.Base = _mValue;
					CommandLogging.LogChangeProperty(from, targ, $"{_mSkill}.Base", _mValue.ToString(CultureInfo.InvariantCulture));
				}

				from.SendMessage("{0} : {1} (Base: {2})", _mSkill, skill.Value, skill.Base);
			}
			else
			{
				from.SendMessage("That does not have skills!");
			}
		}
	}
}
