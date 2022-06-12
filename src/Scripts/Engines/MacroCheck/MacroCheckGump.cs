using Server.Gumps;
using Server.Network;

namespace Server.Engines
{
	public class MacroCheckGump : Gump
	{
		private readonly CheckPlayer m_Check;

		public MacroCheckGump(CheckPlayer check) : base(Utility.RandomMinMax(10, 80), Utility.RandomMinMax(10, 50))
		{
			m_Check = check;

			Closable = false;
			Disposable = true;
			Dragable = false;
			Resizable = false;
			AddPage(0);
			int ktory = Utility.RandomMinMax(0, 2);
			if (ktory == 0)
			{
				AddImage(71, 80, 2200);
				int random3 = Utility.RandomMinMax(0, 7);
				int i;
				for (i = 0; i <= 3; i++)
				{
					if (random3 == i)
					{
						AddButton(120 + (i * 30), 230, 2225 + i, 2225 + i, 1, GumpButtonType.Reply, 0);
					}
					else
					{
						AddButton(120 + (i * 30), 230, 2225 + i, 2225 + i, 2, GumpButtonType.Reply, 0);
					}
				}

				for (i = 4; i <= 7; i++)
				{
					if (random3 == i)
					{
						AddButton(270 + ((i - 4) * 30), 230, 2225 + i, 2225 + i, 1, GumpButtonType.Reply, 0);
					}
					else
					{
						AddButton(270 + ((i - 4) * 30), 230, 2225 + i, 2225 + i, 2, GumpButtonType.Reply, 0);
					}
				}

				AddLabel(274, 140, 32, "Select a Number " + (random3 + 1));
				AddLabel(130, 100, 32, "Macro");
				AddLabel(300, 100, 32, "Check");
				AddLabel(120, 140, 32, "Let the gods know,");
				AddLabel(120, 160, 32, "you're alive!");
			}
			else if (ktory == 1)
			{
				AddBackground(71, 80, 400, 120, 2620);
				int random2 = Utility.RandomMinMax(0, 11);
				for (int i = 0; i <= 11; i++)
				{
					if (i == random2)
					{
						AddButton(85 + (i * 30), 100, 1155, 1155, 1, GumpButtonType.Reply, 0);
					}
					else
					{
						AddButton(85 + (i * 30), 100, 1152, 1152, 2, GumpButtonType.Reply, 0);
					}
				}

				AddLabel(180, 140, 32, "Let the gods know you are");
				AddLabel(180, 160, 32, "alive by pressing a button!");
			}
			else if (ktory == 2)
			{
				AddBackground(71, 80, 300, 350, 2620);
				AddImage(96, 100, 104);
				int random = Utility.RandomMinMax(0, 7);
				int[] id = new int[8];
				id[0] = 2;
				id[1] = 2;
				id[2] = 2;
				id[3] = 2;
				id[4] = 2;
				id[5] = 2;
				id[6] = 2;
				id[7] = 2;
				id[random] = 1;
				AddButton(188, 107, 112, 112, id[0], GumpButtonType.Reply, 0); // miecz            
				AddButton(251, 130, 107, 107, id[1], GumpButtonType.Reply, 0); // kielich            
				AddButton(277, 191, 105, 105, id[2], GumpButtonType.Reply, 0); // serce            
				AddButton(251, 255, 109, 109, id[3], GumpButtonType.Reply, 0); // waga            
				AddButton(188, 281, 106, 106, id[4], GumpButtonType.Reply, 0); // reka            
				AddButton(127, 255, 111, 111, id[5], GumpButtonType.Reply, 0); // krzyz            
				AddButton(101, 194, 110, 110, id[6], GumpButtonType.Reply, 0); // kropla            
				AddButton(127, 130, 108, 108, id[7], GumpButtonType.Reply, 0); // laska        
				int strzalkaID = 4500 + random;
				AddImage(198, 202, strzalkaID);
				AddLabel(135, 329, 32, "Macro");
				AddLabel(265, 329, 32, "Check");
				AddLabel(120, 370, 32, "Let the gods know you are alive");
				AddLabel(120, 390, 32, "by pressing the button!");
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			int val = info.ButtonID;

			if (val == 1)
				m_Check.PlayerRequest(true);
			else
				m_Check.PlayerRequest(false);
		}
	}
}
