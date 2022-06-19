using Server.Engines.PartySystem;

namespace Server.ContextMenus;

public class AddToPartyEntry : ContextMenuEntry
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mTarget;

	public AddToPartyEntry(Mobile from, Mobile target) : base(0197, 12)
	{
		_mFrom = from;
		_mTarget = target;
	}

	public override void OnClick()
	{
		Party p = Party.Get(_mFrom);
		Party mp = Party.Get(_mTarget);

		if (_mFrom == _mTarget)
			_mFrom.SendLocalizedMessage(1005439); // You cannot add yourself to a party.
		else if (p != null && p.Leader != _mFrom)
			_mFrom.SendLocalizedMessage(1005453); // You may only add members to the party if you are the leader.
		else if (p != null && (p.Members.Count + p.Candidates.Count) >= Party.Capacity)
			_mFrom.SendLocalizedMessage(1008095); // You may only have 10 in your party (this includes candidates).
		else if (!_mTarget.Player)
			_mFrom.SendLocalizedMessage(1005444); // The creature ignores your offer.
		else if (mp != null && mp == p)
			_mFrom.SendLocalizedMessage(1005440); // This person is already in your party!
		else if (mp != null)
			_mFrom.SendLocalizedMessage(1005441); // This person is already in a party!
		else
			Party.Invite(_mFrom, _mTarget);
	}
}
