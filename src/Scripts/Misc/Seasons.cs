using System;

namespace Server.Custom.Regions
{
	public class SeasonsTimer : Timer
	{
		private static readonly bool SeasonsEnabled = Settings.Configuration.Get<bool>("Misc", "SeasonsEnabled");
		public static void Initialize()
		{
			if (SeasonsEnabled)
			{
				new SeasonsTimer();
			}
		}
		public SeasonsTimer() : base(TimeSpan.FromSeconds(10), TimeSpan.FromDays(1))
		{
			Start();
		}

		protected override void OnTick()
		{
			switch (DateTime.Now.Month)
			{
				case 1:
				case 2:
				case 11:
				case 12:
					{
						Map.Felucca.Season = 2;
						Map.Trammel.Season = 2;
						Map.Ilshenar.Season = 2;
						Map.Malas.Season = 2;
						Console.WriteLine("Season: Fall");
					}
					break;
				case 3:
				case 4:
				case 5:
					{
						Map.Felucca.Season = 0;
						Map.Trammel.Season = 0;
						Map.Ilshenar.Season = 0;
						Map.Malas.Season = 0;
						Console.WriteLine("Season: Spring");
					}
					break;
				case 6:
				case 7:
				case 8:
					{
						Map.Felucca.Season = 1;
						Map.Trammel.Season = 1;
						Map.Ilshenar.Season = 1;
						Map.Malas.Season = 1;
						Console.WriteLine("Season: Summer");
					}
					break;
				case 9:
				case 10:
					{
						Map.Felucca.Season = 2;
						Map.Trammel.Season = 2;
						Map.Ilshenar.Season = 2;
						Map.Malas.Season = 2;
						Console.WriteLine("Season: Fall");
					}
					break;
				default:
					Console.WriteLine("Season: End of the world man, unknown month.");
					break;
			}
		}
	}
}
