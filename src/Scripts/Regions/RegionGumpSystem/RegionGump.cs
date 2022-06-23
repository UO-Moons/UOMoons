using Server.Commands;
using Server.Gumps;

namespace Server.Scripts.Commands;

public class RegionGump
{
	public static void Initialize()
	{
		CommandSystem.Register("RegionGump", AccessLevel.Player, RegionGump_OnCommand);
		CommandSystem.Register("RG", AccessLevel.Player, RegionGump_OnCommand);
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
