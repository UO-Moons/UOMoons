using Server.Items;
using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Factions
{
	public class FactionBoardVendor : BaseFactionVendor
	{
		public FactionBoardVendor(Town town, Faction faction) : base(town, faction, "the LumberMan") // NOTE: title inconsistant, as OSI
		{
			SetSkill(SkillName.Carpentry, 85.0, 100.0);
			SetSkill(SkillName.Lumberjacking, 60.0, 83.0);
		}

		public override void InitSbInfo()
		{
			SbInfos.Add(new SBFactionBoard());
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new HalfApron());
		}

		public FactionBoardVendor(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	public class SBFactionBoard : SbInfo
	{
		private readonly List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
		private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

		public SBFactionBoard()
		{
		}

		public override IShopSellInfo SellInfo => m_SellInfo;
		public override List<GenericBuyInfo> BuyInfo => m_BuyInfo;

		public class InternalBuyInfo : List<GenericBuyInfo>
		{
			public InternalBuyInfo()
			{
				for (int i = 0; i < 5; ++i)
				{
					Add(new GenericBuyInfo(typeof(Board), 3, 20, 0x1BD7, 0));
				}
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			public InternalSellInfo()
			{
			}
		}
	}
}
