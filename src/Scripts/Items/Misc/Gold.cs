using Server.Accounting;
using System;

namespace Server.Items;

public class Gold : BaseItem
{
	public override double DefaultWeight => Core.ML ? 0.02 / 3 : 0.02;

	[Constructable]
	public Gold() : this(1)
	{
	}

	[Constructable]
	public Gold(int amountFrom, int amountTo) : this(Utility.RandomMinMax(amountFrom, amountTo))
	{
	}

	[Constructable]
	public Gold(int amount) : base(0xEED)
	{
		Stackable = true;
		Amount = amount;
	}

	public Gold(Serial serial) : base(serial)
	{
	}

	public override int GetDropSound()
	{
		if (Amount <= 1)
			return 0x2E4;
		else if (Amount <= 5)
			return 0x2E5;
		else
			return 0x2E6;
	}

	protected override void OnAmountChange(int oldValue)
	{
		int newValue = Amount;

		UpdateTotal(this, TotalType.Gold, newValue - oldValue);
	}

	public override void OnAdded(IEntity parent)
	{
		base.OnAdded(parent);

		if (!AccountGold.Enabled)
		{
			return;
		}

		Mobile owner = null;
		SecureTradeInfo tradeInfo = null;

		Container root = parent as Container;

		while (root is { Parent: Container })
		{
			root = (Container)root.Parent;
		}

		parent = root ?? parent;

		switch (parent)
		{
			case SecureTradeContainer trade when AccountGold.ConvertOnTrade:
			{
				if (trade.Trade.From.Container == trade)
				{
					tradeInfo = trade.Trade.From;
					owner = tradeInfo.Mobile;
				}
				else if (trade.Trade.To.Container == trade)
				{
					tradeInfo = trade.Trade.To;
					owner = tradeInfo.Mobile;
				}

				break;
			}
			case BankBox box when AccountGold.ConvertOnBank:
				owner = box.Owner;
				break;
		}

		if (owner?.Account == null || !owner.Account.DepositGold(Amount))
		{
			return;
		}

		if (tradeInfo != null)
		{
			if (owner.NetState is { NewSecureTrading: false })
			{
				double total = Amount / Math.Max(1.0, Account.CurrencyThreshold);
				int plat = (int)Math.Truncate(total);
				int gold = (int)((total - plat) * Account.CurrencyThreshold);

				tradeInfo.Plat += plat;
				tradeInfo.Gold += gold;
			}

			tradeInfo.VirtualCheck?.UpdateTrade(tradeInfo.Mobile);
		}

		owner.SendLocalizedMessage(1042763, Amount.ToString("#,0"));

		Delete();

		((Container)parent).UpdateTotals();
	}

	public override int GetTotal(TotalType type)
	{
		int baseTotal = base.GetTotal(type);

		if (type == TotalType.Gold)
			baseTotal += Amount;

		return baseTotal;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}
}
