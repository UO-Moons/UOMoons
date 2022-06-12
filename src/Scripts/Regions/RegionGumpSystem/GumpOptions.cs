using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Scripts.Commands
{
	public class RegionGump
	{
		public static void Initialize()
		{
			CommandSystem.Register("RegionGump", AccessLevel.Player, new CommandEventHandler(RegionGump_OnCommand));
			CommandSystem.Register("RG", AccessLevel.Player, new CommandEventHandler(RegionGump_OnCommand));
		}

		[Usage("RegionGump || RG")]
		[Description("Manual command to call the region gump and see what region your in.")]
		public static void RegionGump_OnCommand(CommandEventArgs e)
		{
			Mobile from = e.Mobile;

			if (from.HasGump(typeof(BritainRegion)))
				from.CloseGump(typeof(BritainRegion));

			if (from.HasGump(typeof(BuccaneersDenRegion)))
				from.CloseGump(typeof(BuccaneersDenRegion));

			if (from.HasGump(typeof(CoveRegion)))
				from.CloseGump(typeof(CoveRegion));

			if (from.HasGump(typeof(DeluciaRegion)))
				from.CloseGump(typeof(DeluciaRegion));

			if (from.HasGump(typeof(HavenRegion)))
				from.CloseGump(typeof(HavenRegion));

			if (from.HasGump(typeof(JhelomRegion)))
				from.CloseGump(typeof(JhelomRegion));

			if (from.HasGump(typeof(MaginciaRegion)))
				from.CloseGump(typeof(MaginciaRegion));

			if (from.HasGump(typeof(MinocRegion)))
				from.CloseGump(typeof(MinocRegion));

			if (from.HasGump(typeof(MoonglowRegion)))
				from.CloseGump(typeof(MoonglowRegion));

			if (from.HasGump(typeof(NujelmRegion)))
				from.CloseGump(typeof(NujelmRegion));

			if (from.HasGump(typeof(PapuaRegion)))
				from.CloseGump(typeof(PapuaRegion));

			if (from.HasGump(typeof(SerpentsHoldRegion)))
				from.CloseGump(typeof(SerpentsHoldRegion));

			if (from.HasGump(typeof(SkaraBraeRegion)))
				from.CloseGump(typeof(SkaraBraeRegion));

			if (from.HasGump(typeof(TrinsicRegion)))
				from.CloseGump(typeof(TrinsicRegion));

			if (from.HasGump(typeof(VesperRegion)))
				from.CloseGump(typeof(VesperRegion));

			if (from.HasGump(typeof(WindRegion)))
				from.CloseGump(typeof(WindRegion));

			if (from.HasGump(typeof(YewRegion)))
				from.CloseGump(typeof(YewRegion));

			if (from.HasGump(typeof(GumpOptions)))
				from.CloseGump(typeof(GumpOptions));
			from.SendGump(new GumpOptions(from));

		}
	}
}

namespace Server.Gumps
{
	public class GumpOptions : Gump
	{
		public GumpOptions(Mobile from) : base(0, 0)
		{
			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;
			AddPage(0);
			AddBackground(0, 29, 192, 168, 9200);
			AddImage(10, 41, 52);
			AddLabel(19, 123, 3, @"Region Gump (Auto/Man)");
			AddImage(82, 157, 113);
			AddButton(15, 163, 2111, 2112, 1, GumpButtonType.Reply, 0);
			AddImage(75, 56, 2529);
			AddButton(123, 162, 2114, 248, 2, GumpButtonType.Reply, 0);
			AddLabel(59, 44, 36, @"Gump Options");

		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile from = sender.Mobile;

			PlayerMobile From = from as PlayerMobile;

			//From.CloseGump(typeof(PernOptions));

			if (info.ButtonID == 1)
			{
				From.RegionGump = true;
				return;
			}
			if (info.ButtonID == 2)
			{
				From.RegionGump = false;
				return;
			}
		}
	}
}
