using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class SbHelmetArmor : SbInfo
{
	public override IShopSellInfo SellInfo { get; } = new InternalSellInfo();

	public override List<GenericBuyInfo> BuyInfo { get; } = new InternalBuyInfo();

	public class InternalBuyInfo : List<GenericBuyInfo>
	{
		public InternalBuyInfo()
		{
			Add(new GenericBuyInfo(typeof(PlateHelm), 21, 20, 0x1412, 0));
			Add(new GenericBuyInfo(typeof(CloseHelm), 18, 20, 0x1408, 0));
			Add(new GenericBuyInfo(typeof(CloseHelm), 18, 20, 0x1409, 0));
			Add(new GenericBuyInfo(typeof(Helmet), 31, 20, 0x140A, 0));
			Add(new GenericBuyInfo(typeof(Helmet), 18, 20, 0x140B, 0));
			Add(new GenericBuyInfo(typeof(NorseHelm), 18, 20, 0x140E, 0));
			Add(new GenericBuyInfo(typeof(NorseHelm), 18, 20, 0x140F, 0));
			Add(new GenericBuyInfo(typeof(Bascinet), 18, 20, 0x140C, 0));
			Add(new GenericBuyInfo(typeof(PlateHelm), 21, 20, 0x1419, 0));
		}
	}

	public class InternalSellInfo : GenericSellInfo
	{
		public InternalSellInfo()
		{
			Add(typeof(Bascinet), 9);
			Add(typeof(CloseHelm), 9);
			Add(typeof(Helmet), 9);
			Add(typeof(NorseHelm), 9);
			Add(typeof(PlateHelm), 10);
		}
	}
}
