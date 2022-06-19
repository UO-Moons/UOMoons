using Server.Gumps;
using Server.Items;
using System;
using Server.Network;

namespace Server.Engines.Craft;

public class QueryMakersMarkGump : Gump
{
	private readonly int _mQuality;
	private readonly Mobile _mFrom;
	private readonly CraftItem _mCraftItem;
	private readonly CraftSystem _mCraftSystem;
	private readonly Type _mTypeRes;
	private readonly ITool _mTool;
	public QueryMakersMarkGump(int quality, Mobile from, CraftItem craftItem, CraftSystem craftSystem, Type typeRes, ITool tool)
		: base(100, 200)
	{
		from.CloseGump(typeof(QueryMakersMarkGump));

		_mQuality = quality;
		_mFrom = from;
		_mCraftItem = craftItem;
		_mCraftSystem = craftSystem;
		_mTypeRes = typeRes;
		_mTool = tool;

		AddPage(0);

		AddBackground(0, 0, 220, 170, 5054);
		AddBackground(10, 10, 200, 150, 3000);

		AddHtmlLocalized(20, 20, 180, 80, 1018317, false, false); // Do you wish to place your maker's mark on this item?

		AddHtmlLocalized(55, 100, 140, 25, 1011011, false, false); // CONTINUE
		AddButton(20, 100, 4005, 4007, 1, GumpButtonType.Reply, 0);

		AddHtmlLocalized(55, 125, 140, 25, 1011012, false, false); // CANCEL
		AddButton(20, 125, 4005, 4007, 0, GumpButtonType.Reply, 0);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		bool makersMark = info.ButtonID == 1;

		_mFrom.SendLocalizedMessage(makersMark ? 501808 : 501809);

		_mCraftItem.CompleteCraft(_mQuality, makersMark, _mFrom, _mCraftSystem, _mTypeRes, _mTool, null);
	}
}
