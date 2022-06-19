using Server.Engines.PartySystem;

namespace Server.ContextMenus;

public class RemoveFromPartyEntry : ContextMenuEntry
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mTarget;

	public RemoveFromPartyEntry(Mobile from, Mobile target) : base(0198, 12)
	{
		_mFrom = from;
		_mTarget = target;
	}

	public override void OnClick()
	{
		Party p = Party.Get(_mFrom);

		if (p == null || p.Leader != _mFrom || !p.Contains(_mTarget))
			return;

		if (_mFrom == _mTarget)
			_mFrom.SendLocalizedMessage(1005446); // You may only remove yourself from a party if you are not the leader.
		else
			p.Remove(_mTarget);
	}
}
