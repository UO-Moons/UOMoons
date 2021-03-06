using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbHairStylist : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo("special beard dye", typeof(SpecialBeardDye), 500000, 20, 0xE26, 0));
			Add(new GenericBuyInfo("special hair dye", typeof(SpecialHairDye), 500000, 20, 0xE26, 0));
			Add(new GenericBuyInfo("1041060", typeof(HairDye), 60, 20, 0xEFF, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(HairDye), 30);
			Add(typeof(SpecialBeardDye), 250000);
			Add(typeof(SpecialHairDye), 250000);
		}
	}
}
