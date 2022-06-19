using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbScribe : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(ScribesPen), 8, 20, 0xFBF, 0));
			Add(new GenericBuyInfo(typeof(BlankScroll), 5, 999, 0x0E34, 0));
			Add(new GenericBuyInfo(typeof(ScribesPen), 8, 20, 0xFC0, 0));
			Add(new GenericBuyInfo(typeof(BrownBook), 15, 10, 0xFEF, 0));
			Add(new GenericBuyInfo(typeof(TanBook), 15, 10, 0xFF0, 0));
			Add(new GenericBuyInfo(typeof(BlueBook), 15, 10, 0xFF2, 0));
			//Add( new GenericBuyInfo( "1041267", typeof( Runebook ), 3500, 10, 0xEFA, 0x461 ) );
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(ScribesPen), 4);
			Add(typeof(BrownBook), 7);
			Add(typeof(TanBook), 7);
			Add(typeof(BlueBook), 7);
			Add(typeof(BlankScroll), 3);
		}
	}
}
