using System;
using System.Collections;

namespace Server.Engines.PartySystem;

public class DeclineTimer : Timer
{
	private readonly Mobile _mMobile, _mLeader;

	private static readonly Hashtable MTable = new();

	public static void Start(Mobile m, Mobile leader)
	{
		DeclineTimer t = (DeclineTimer)MTable[m];

		t?.Stop();

		MTable[m] = t = new DeclineTimer(m, leader);
		t.Start();
	}

	private DeclineTimer(Mobile m, Mobile leader) : base(TimeSpan.FromSeconds(30.0))
	{
		_mMobile = m;
		_mLeader = leader;
	}

	protected override void OnTick()
	{
		MTable.Remove(_mMobile);

		if (_mMobile.Party == _mLeader && PartyCommands.Handler != null)
			PartyCommands.Handler.OnDecline(_mMobile, _mLeader);
	}
}
