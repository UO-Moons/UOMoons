using Server.Multis;

namespace Server.ContextMenus;

public class EjectPlayerEntry : ContextMenuEntry
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mTarget;
	private readonly BaseHouse _mTargetHouse;

	public EjectPlayerEntry(Mobile from, Mobile target) : base(6206, 12)
	{
		_mFrom = from;
		_mTarget = target;
		_mTargetHouse = BaseHouse.FindHouseAt(_mTarget);
	}

	public override void OnClick()
	{
		if (!_mFrom.Alive || _mTargetHouse.Deleted || !_mTargetHouse.IsFriend(_mFrom))
			return;

		if (_mTarget != null)
		{
			_mTargetHouse.Kick(_mFrom, _mTarget);
		}
	}
}
