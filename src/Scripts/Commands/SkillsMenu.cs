using Server.Gumps;
using Server.Targeting;

namespace Server.Commands;

public class Skills
{
	public static void Initialize()
	{
		Register();
	}

	public static void Register()
	{
		CommandSystem.Register("Skills", AccessLevel.Counselor, Skills_OnCommand);
		CommandSystem.Register("ShowSkills", AccessLevel.Counselor, Skills_OnCommand);
		CommandSystem.Register("SkillsMenu", AccessLevel.Counselor, Skills_OnCommand);
	}

	private class SkillsTarget : Target
	{
		public SkillsTarget() : base(-1, true, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is Mobile mobile)
				from.SendGump(new SkillsGump(from, mobile));
		}
	}

	[Usage("Skills")]
	[Description("Opens a menu where you can view or edit skills of a targeted mobile.")]
	private static void Skills_OnCommand(CommandEventArgs e)
	{
		e.Mobile.Target = new SkillsTarget();
	}
}
