using Server.Commands;
using Server.Gumps;

namespace Server.Scripts.Commands
{
	public class GumpRegion
	{
		public static void Initialize()
		{
			CommandSystem.Register("GumpRegion", AccessLevel.Player, new CommandEventHandler(GumpRegion_OnCommand));
			CommandSystem.Register("GR", AccessLevel.Player, new CommandEventHandler(GumpRegion_OnCommand));
		}

		[Usage("GumpRegion || GR")]
		[Description("Decide if you want to have the region gump appear on entering a new region.")]
		public static void GumpRegion_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;
			if (from != null)
			{
				if (from.Map == Map.Felucca)
				{
					if (from.Region.Name == "Britain")
					{
						if (from.HasGump(typeof(FBritainRegion)))
							from.CloseGump(typeof(FBritainRegion));
						from.SendGump(new FBritainRegion());
						return;
					}
				}
				else
				{
					if (from.Region.Name == "Britain")
					{
						if (from.HasGump(typeof(BritainRegion)))
							from.CloseGump(typeof(BritainRegion));
						from.SendGump(new BritainRegion());
						return;
					}

					if (from.Region.Name == "Buccaneer's Den")
					{
						if (from.HasGump(typeof(BuccaneersDenRegion)))
							from.CloseGump(typeof(BuccaneersDenRegion));
						from.SendGump(new BuccaneersDenRegion());
						return;
					}

					if (from.Region.Name == "Cove")
					{
						if (from.HasGump(typeof(CoveRegion)))
							from.CloseGump(typeof(CoveRegion));
						from.SendGump(new CoveRegion());
						return;
					}

					if (from.Region.Name == "Delucia")
					{
						if (from.HasGump(typeof(DeluciaRegion)))
							from.CloseGump(typeof(DeluciaRegion));
						from.SendGump(new DeluciaRegion());
						return;
					}

					if (from.Region.Name == "Haven")
					{
						if (from.HasGump(typeof(HavenRegion)))
							from.CloseGump(typeof(HavenRegion));
						from.SendGump(new HavenRegion());
						return;
					}

					if (from.Region.Name == "Jhelom")
					{
						if (from.HasGump(typeof(JhelomRegion)))
							from.CloseGump(typeof(JhelomRegion));
						from.SendGump(new JhelomRegion());
						return;
					}

					if (from.Region.Name == "Magincia")
					{
						if (from.HasGump(typeof(MaginciaRegion)))
							from.CloseGump(typeof(MaginciaRegion));
						from.SendGump(new MaginciaRegion());
						return;
					}

					if (from.Region.Name == "Minoc")
					{
						if (from.HasGump(typeof(MinocRegion)))
							from.CloseGump(typeof(MinocRegion));
						from.SendGump(new MinocRegion());
						return;
					}

					if (from.Region.Name == "Moonglow")
					{
						if (from.HasGump(typeof(MoonglowRegion)))
							from.CloseGump(typeof(MoonglowRegion));
						from.SendGump(new MoonglowRegion());
						return;
					}

					if (from.Region.Name == "Nujel'm")
					{
						if (from.HasGump(typeof(NujelmRegion)))
							from.CloseGump(typeof(NujelmRegion));
						from.SendGump(new NujelmRegion());
						return;
					}

					if (from.Region.Name == "Papua")
					{
						if (from.HasGump(typeof(PapuaRegion)))
							from.CloseGump(typeof(PapuaRegion));
						from.SendGump(new PapuaRegion());
						return;
					}

					if (from.Region.Name == "Serpent's Hold")
					{
						if (from.HasGump(typeof(SerpentsHoldRegion)))
							from.CloseGump(typeof(SerpentsHoldRegion));
						from.SendGump(new SerpentsHoldRegion());
						return;
					}

					if (from.Region.Name == "Skara Brae")
					{
						if (from.HasGump(typeof(SkaraBraeRegion)))
							from.CloseGump(typeof(SkaraBraeRegion));
						from.SendGump(new SkaraBraeRegion());
						return;
					}

					if (from.Region.Name == "Trinsic")
					{
						if (from.HasGump(typeof(TrinsicRegion)))
							from.CloseGump(typeof(TrinsicRegion));
						from.SendGump(new TrinsicRegion());
						return;
					}

					if (from.Region.Name == "Vesper")
					{
						if (from.HasGump(typeof(VesperRegion)))
							from.CloseGump(typeof(VesperRegion));
						from.SendGump(new VesperRegion());
						return;
					}

					if (from.Region.Name == "Wind")
					{
						if (from.HasGump(typeof(WindRegion)))
							from.CloseGump(typeof(WindRegion));
						from.SendGump(new WindRegion());
						return;
					}

					if (from.Region.Name == "Yew")
					{
						if (from.HasGump(typeof(YewRegion)))
							from.CloseGump(typeof(YewRegion));
						from.SendGump(new YewRegion());
						return;
					}
				}
			}
			from.SendMessage("Region has not been defined yet!");
		}
	}
}
