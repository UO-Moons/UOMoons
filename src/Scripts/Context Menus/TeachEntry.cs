using Server.Mobiles;

namespace Server.ContextMenus;

public class TeachEntry : ContextMenuEntry
{
	private readonly SkillName _mSkill;
	private readonly BaseCreature _mMobile;
	private readonly Mobile _mFrom;

	public TeachEntry(SkillName skill, BaseCreature m, Mobile from, bool enabled) : base(6000 + (int)skill)
	{
		_mSkill = skill;
		_mMobile = m;
		_mFrom = from;

		if (!enabled)
			Flags |= Network.CMEFlags.Disabled;
	}

	public override void OnClick()
	{
		if (!_mFrom.CheckAlive())
			return;

		_mMobile.Teach(_mSkill, _mFrom, 0, false);
	}
}
