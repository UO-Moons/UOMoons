using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.NewMagincia;

public class CommodityBrokerEntry
{
	public Type CommodityType { get; }

	public CommodityBroker Broker { get; }

	public int ItemId { get; }

	public int Label { get; }

	public int SellPricePer { get; set; }

	public int BuyPricePer { get; set; }

	public int BuyLimit { get; set; }

	public int SellLimit { get; set; }

	public int Stock { get; set; }

	public int ActualSellLimit => Stock < SellLimit ? Stock : SellLimit;

	public int ActualBuyLimit
	{
		get
		{
			if (Broker != null && Broker.BankBalance < BuyLimit * BuyPricePer && BuyPricePer > 0)
				return Broker.BankBalance / BuyPricePer;

			int limit = BuyLimit - Stock;

			return limit <= 0 ? 0 : limit;
		}
	}

	/*	SellPricePer - price per unit the broker is selling to players for
	 *	BuyPricePer - price per unit the broker is buying from players for
	 *	BuyAtLimit - Limit a commodity must go below before it will buy that particular commodity
	 *	
	 *	SellAtLimit - Limit a commodity must go above before it will sell that particular commodity
	 *	BuyLimit - Limit (in units) it will buy from players
	 *	SellLimit - Limit (in units) it will sell to players
	 */
	public CommodityBrokerEntry(Item item, CommodityBroker broker, int amount)
	{
		CommodityType = item.GetType();
		ItemId = item.ItemId;
		Broker = broker;
		Stock = amount;

		if (item is ICommodity commodity)
			Label = commodity.Description;
		else
			Label = item.LabelNumber;
	}

	/// <summary>
	/// Player buys, the vendor is selling
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public bool PlayerCanBuy(int amount)
	{
		return (SellLimit == 0 || amount <= ActualSellLimit) && Stock > 0 &&  SellPricePer > 0;
	}

	/// <summary>
	/// Player sells, the vendor is buying
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public bool PlayerCanSell(int amount)
	{
		return (BuyLimit == 0 || amount <= ActualBuyLimit) && BuyPricePer > 0 && BuyPricePer <= Broker.BankBalance;
	}

	public CommodityBrokerEntry(GenericReader reader)
	{
		reader.ReadInt();

		CommodityType = Assembler.FindTypeByName(reader.ReadString());
		Label = reader.ReadInt();
		Broker = reader.ReadMobile() as CommodityBroker;
		ItemId = reader.ReadInt();
		SellPricePer = reader.ReadInt();
		BuyPricePer = reader.ReadInt();
		BuyLimit = reader.ReadInt();
		SellLimit = reader.ReadInt();
		Stock = reader.ReadInt();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(CommodityType.Name);
		writer.Write(Label);
		writer.Write(Broker);
		writer.Write(ItemId);
		writer.Write(SellPricePer);
		writer.Write(BuyPricePer);
		writer.Write(BuyLimit);
		writer.Write(SellLimit);
		writer.Write(Stock);
	}
}

public class PetBrokerEntry
{
	public BaseCreature Pet { get; }

	public int SalePrice { get; set; }

	public string TypeName { get; set; }

	public static Dictionary<Type, string> NameBuffer { get; } = new();

	public static readonly int DefaultPrice = 1000;

	public PetBrokerEntry(BaseCreature pet) : this(pet, DefaultPrice)
	{
	}

	public PetBrokerEntry(BaseCreature pet, int price)
	{
		Pet = pet;
		SalePrice = price;
		TypeName = GetOriginalName(pet);
	}

	public static string GetOriginalName(BaseCreature bc)
	{
		if (bc == null)
			return null;

		Type t = bc.GetType();

		if (NameBuffer.ContainsKey(t))
			return NameBuffer[t];

		if (Activator.CreateInstance(t) is BaseCreature c)
		{
			c.Delete();
			AddToBuffer(t, c.Name);
			return c.Name;
		}

		return t.Name;
	}

	public static void AddToBuffer(Type type, string s)
	{
		if (!string.IsNullOrEmpty(s) && !NameBuffer.ContainsKey(type))
			NameBuffer[type] = s;
	}

	public void Internalize()
	{
		if (Pet.Map == Map.Internal)
			return;

		Pet.ControlTarget = null;
		Pet.ControlOrder = OrderType.Stay;
		Pet.Internalize();

		Pet.SetControlMaster(null);
		Pet.SummonMaster = null;

		Pet.IsStabled = true;
		Pet.Loyalty = BaseCreature.MaxLoyalty;

		Pet.Home = Point3D.Zero;
		Pet.RangeHome = 10;
		Pet.Blessed = false;
	}

	public PetBrokerEntry(GenericReader reader)
	{
		reader.ReadInt();

		Pet = reader.ReadMobile() as BaseCreature;
		SalePrice = reader.ReadInt();
		TypeName = reader.ReadString();

		if (Pet != null)
		{
			AddToBuffer(Pet.GetType(), TypeName);

			Pet.IsStabled = true;

			Timer.DelayCall(TimeSpan.FromSeconds(10), Internalize);
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(Pet);
		writer.Write(SalePrice);
		writer.Write(TypeName);
	}
}
