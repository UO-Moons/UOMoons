using Server.Mobiles;
using System;

namespace Server.Engines;

public class CheckPlayer
{
	private readonly Mobile _player;
	private readonly Mobile _gm;
	private MacroCheckTimer _timer;
	private readonly DateTime _start;

	public CheckPlayer(Mobile player, Mobile gamemaster)
	{
		_player = player;
		_gm = gamemaster;
		_start = DateTime.UtcNow;

		if (player is PlayerMobile pm)
			pm.LastMacroCheck = DateTime.UtcNow;
		player.SendGump(new MacroCheckGump(this));


		_timer = new MacroCheckTimer(this);
		_timer.Start();
	}

	public void PlayerRequest(bool isTrue)
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}

		_player.CloseGump(typeof(MacroCheckGump));
		TimeSpan clickbutton = DateTime.UtcNow - _start;

		if (isTrue)
			_gm.SendMessage(0x40, "Player {0} correctly answered the call in {1} seconds.",
				_player.Name, TimeSpanFormat(clickbutton));
		else
			_gm.SendMessage(0x20, "Player {0} failed to answer the call within {1} seconds.", _player.Name,
				TimeSpanFormat(clickbutton));
	}

	public void PlayerRequest()
	{
		PlayerRequest(true);
	}


	public static string TimeSpanFormat(TimeSpan time)
	{
		return $"{time.Seconds}";
	}

	public void TimeOut()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}

		_player.CloseGump(typeof(MacroCheckGump));

		//				Server.Accounting.Account acc = m_Player.Account as Server.Accounting.Account;
		//
		//				if ( acc == null )
		//					return; // Char deleted, too bad
		//
		//				JailEntry jail = new JailEntry( m_Player , acc,  m_GM ,
		//			        TimeSpan.FromDays( 1 ) , "Passive Macro", "Punished automatically by AntiMacroSystem",
		//			                               true, true, ( m_Player.Race == RaceType.DarkElf) ? JailSystem.m_Jail[ 0 ] : JailSystem.m_Jail[ 1 ] );
		//
		//				JailSystem.Jailings.Add( jail );
		//				JailSystem.FinalizeJail( m_Player );

		_gm.SendMessage("Player {0}, did not answer within 1 minute and got locked in jail.",
			_player.Name);
	}
}
