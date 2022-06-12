using Server.Mobiles;
using System;

namespace Server.Engines
{
	public class CheckPlayer
	{
		private readonly Mobile m_Player;
		private readonly Mobile m_GM;
		private MacroCheckTimer m_Timer;
		private readonly DateTime m_Start;

		public CheckPlayer(Mobile player, Mobile gamemaster)
		{
			m_Player = player;
			m_GM = gamemaster;
			m_Start = DateTime.UtcNow;

			PlayerMobile pm = player as PlayerMobile;

			pm.LastMacroCheck = DateTime.UtcNow;
			player.SendGump(new MacroCheckGump(this));


			m_Timer = new MacroCheckTimer(this);
			m_Timer.Start();
		}

		public void PlayerRequest(bool isTrue)
		{
			if (m_Timer != null)
			{
				m_Timer.Stop();
				m_Timer = null;
			}

			m_Player.CloseGump(typeof(MacroCheckGump));
			TimeSpan clickbutton = DateTime.UtcNow - m_Start;

			if (isTrue)
				m_GM.SendMessage(0x40, "Player {0} correctly anwsered the call in {1} seconds.",
					m_Player.Name, TimeSpanFormat(clickbutton));
			else
				m_GM.SendMessage(0x20, "Player {0} failed to anwser the call within {1} seconds.", m_Player.Name,
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
			if (m_Timer != null)
			{
				m_Timer.Stop();
				m_Timer = null;
			}

			m_Player.CloseGump(typeof(MacroCheckGump));

			//				Server.Accounting.Account acc = m_Player.Account as Server.Accounting.Account;
			//
			//				if ( acc == null )
			//					return; // Char deleted, too bad
			//
			//				JailEntry jail = new JailEntry( m_Player , acc,  m_GM ,
			//			        TimeSpan.FromDays( 1 ) , "Bierne Makro", "Ukarany automatycznie przez AntiMacroSystem",
			//			                               true, true, ( m_Player.Race == RaceType.DarkElf) ? JailSystem.m_Jail[ 0 ] : JailSystem.m_Jail[ 1 ] );
			//
			//				JailSystem.Jailings.Add( jail );
			//				JailSystem.FinalizeJail( m_Player );

			m_GM.SendMessage("Player {0}, did not anwser within 1 minute and got locked in jail.",
				m_Player.Name);
		}
	}
}
