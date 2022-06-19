using Server.Gumps;
using Server.Network;
using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class CustomHairstylist : BaseVendor
{
	protected override List<SbInfo> SbInfos { get; } = new();

	public override bool ClickTitle => false;

	public override bool IsActiveBuyer => false;
	public override bool IsActiveSeller => true;

	public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
	{
		return false;
	}

	public static readonly object From = new();
	public static readonly object Vendor = new();
	public static readonly object Price = new();

	private static readonly HairstylistBuyInfo[] MSellList = {
		new( 1018357, 50000, false, typeof( ChangeHairstyleGump ), new[]
			{ From, Vendor, Price, false, ChangeHairstyleEntry.HairEntries } ),
		new( 1018358, 50000, true, typeof( ChangeHairstyleGump ), new[]
			{ From, Vendor, Price, true, ChangeHairstyleEntry.BeardEntries } ),
		new( 1018359, 50, false, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, true, true, ChangeHairHueEntry.RegularEntries } ),
		new( 1018360, 500000, false, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, true, true, ChangeHairHueEntry.BrightEntries } ),
		new( 1018361, 30000, false, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, true, false, ChangeHairHueEntry.RegularEntries } ),
		new( 1018362, 30000, true, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, false, true, ChangeHairHueEntry.RegularEntries } ),
		new( 1018363, 500000, false, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, true, false, ChangeHairHueEntry.BrightEntries } ),
		new( 1018364, 500000, true, typeof( ChangeHairHueGump ), new[]
			{ From, Vendor, Price, false, true, ChangeHairHueEntry.BrightEntries } )
	};

	public override void VendorBuy(Mobile from)
	{
		from.SendGump(new HairstylistBuyGump(from, this, MSellList));
	}

	[Constructable]
	public CustomHairstylist() : base("the hairstylist")
	{
	}

	public override int GetHairHue()
	{
		return Utility.RandomBrightHue();
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new Robe(Utility.RandomPinkHue()));
	}

	public override void InitSbInfo()
	{
	}

	public CustomHairstylist(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class HairstylistBuyInfo
{
	public int Title { get; }
	public string TitleString { get; }
	public int Price { get; }
	public bool FacialHair { get; }
	public Type GumpType { get; }
	public object[] GumpArgs { get; }

	public HairstylistBuyInfo(int title, int price, bool facialHair, Type gumpType, object[] args)
	{
		Title = title;
		Price = price;
		FacialHair = facialHair;
		GumpType = gumpType;
		GumpArgs = args;
	}

	public HairstylistBuyInfo(string title, int price, bool facialHair, Type gumpType, object[] args)
	{
		TitleString = title;
		Price = price;
		FacialHair = facialHair;
		GumpType = gumpType;
		GumpArgs = args;
	}
}

public class HairstylistBuyGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mVendor;
	private readonly HairstylistBuyInfo[] _mSellList;

	public HairstylistBuyGump(Mobile from, Mobile vendor, HairstylistBuyInfo[] sellList) : base(50, 50)
	{
		_mFrom = from;
		_mVendor = vendor;
		_mSellList = sellList;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		bool isFemale = (from.Female || from.Body.IsFemale);

		int balance = Banker.GetBalance(from);
		int canAfford = 0;

		for (var i = 0; i < sellList.Length; ++i)
		{
			if (balance >= sellList[i].Price && (!sellList[i].FacialHair || !isFemale))
				++canAfford;
		}

		AddPage(0);

		AddBackground(50, 10, 450, 100 + (canAfford * 25), 2600);

		AddHtmlLocalized(100, 40, 350, 20, 1018356, false, false); // Choose your hairstyle change:

		int index = 0;

		for (int i = 0; i < sellList.Length; ++i)
		{
			if (balance < sellList[i].Price || (sellList[i].FacialHair && isFemale)) continue;
			if (sellList[i].TitleString != null)
				AddHtml(140, 75 + (index * 25), 300, 20, sellList[i].TitleString, false, false);
			else
				AddHtmlLocalized(140, 75 + (index * 25), 300, 20, sellList[i].Title, false, false);

			AddButton(100, 75 + (index++ * 25), 4005, 4007, 1 + i, GumpButtonType.Reply, 0);
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		int index = info.ButtonID - 1;

		if (index < 0 || index >= _mSellList.Length) return;
		HairstylistBuyInfo buyInfo = _mSellList[index];

		int balance = Banker.GetBalance(_mFrom);

		bool isFemale = (_mFrom.Female || _mFrom.Body.IsFemale);

		if (buyInfo.FacialHair && isFemale)
		{
			// You cannot place facial hair on a woman!
			_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1010639, _mFrom.NetState);
		}
		else if (balance >= buyInfo.Price)
		{
			try
			{
				object[] origArgs = buyInfo.GumpArgs;
				object[] args = new object[origArgs.Length];

				for (int i = 0; i < args.Length; ++i)
				{
					if (origArgs[i] == CustomHairstylist.Price)
						args[i] = _mSellList[index].Price;
					else if (origArgs[i] == CustomHairstylist.From)
						args[i] = _mFrom;
					else if (origArgs[i] == CustomHairstylist.Vendor)
						args[i] = _mVendor;
					else
						args[i] = origArgs[i];
				}

				Gump g = Activator.CreateInstance(buyInfo.GumpType, args) as Gump;

				_mFrom.SendGump(g);
			}
			catch
			{
				// ignored
			}
		}
		else
		{
			// You cannot afford my services for that style.
			_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _mFrom.NetState);
		}
	}
}

public class ChangeHairHueEntry
{
	public string Name { get; }
	public int[] Hues { get; }

	public ChangeHairHueEntry(string name, int[] hues)
	{
		Name = name;
		Hues = hues;
	}

	public ChangeHairHueEntry(string name, int start, int count)
	{
		Name = name;

		Hues = new int[count];

		for (var i = 0; i < count; ++i)
			Hues[i] = start + i;
	}

	public static readonly ChangeHairHueEntry[] BrightEntries = {
		new( "*****", 12, 10 ),
		new( "*****", 32, 5 ),
		new( "*****", 38, 8 ),
		new( "*****", 54, 3 ),
		new( "*****", 62, 10 ),
		new( "*****", 81, 2 ),
		new( "*****", 89, 2 ),
		new( "*****", 1153, 2 )
	};

	public static readonly ChangeHairHueEntry[] RegularEntries = {
		new( "*****", 1602, 26 ),
		new( "*****", 1628, 27 ),
		new( "*****", 1502, 32 ),
		new( "*****", 1302, 32 ),
		new( "*****", 1402, 32 ),
		new( "*****", 1202, 24 ),
		new( "*****", 2402, 29 ),
		new( "*****", 2213, 6 ),
		new( "*****", 1102, 8 ),
		new( "*****", 1110, 8 ),
		new( "*****", 1118, 16 ),
		new( "*****", 1134, 16 )
	};
}

public class ChangeHairHueGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mVendor;
	private readonly int _mPrice;
	private readonly bool _mHair;
	private readonly bool _mFacialHair;
	private readonly ChangeHairHueEntry[] _mEntries;

	public ChangeHairHueGump(Mobile from, Mobile vendor, int price, bool hair, bool facialHair, ChangeHairHueEntry[] entries) : base(50, 50)
	{
		_mFrom = from;
		_mVendor = vendor;
		_mPrice = price;
		_mHair = hair;
		_mFacialHair = facialHair;
		_mEntries = entries;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		AddPage(0);

		AddBackground(100, 10, 350, 370, 2600);
		AddBackground(120, 54, 110, 270, 5100);

		AddHtmlLocalized(155, 25, 240, 30, 1011013, false, false); // <center>Hair Color Selection Menu</center>

		AddHtmlLocalized(150, 330, 220, 35, 1011014, false, false); // Dye my hair this color!
		AddButton(380, 330, 4005, 4007, 1, GumpButtonType.Reply, 0);

		for (var i = 0; i < entries.Length; ++i)
		{
			ChangeHairHueEntry entry = entries[i];

			AddLabel(130, 59 + (i * 22), entry.Hues[0] - 1, entry.Name);
			AddButton(207, 60 + (i * 22), 5224, 5224, 0, GumpButtonType.Page, 1 + i);
		}

		for (var i = 0; i < entries.Length; ++i)
		{
			ChangeHairHueEntry entry = entries[i];
			int[] hues = entry.Hues;
			string name = entry.Name;

			AddPage(1 + i);

			for (var j = 0; j < hues.Length; ++j)
			{
				AddLabel(278 + ((j / 16) * 80), 52 + ((j % 16) * 17), hues[j] - 1, name);
				AddRadio(260 + ((j / 16) * 80), 52 + ((j % 16) * 17), 210, 211, false, (j * entries.Length) + i);
			}
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (info.ButtonID == 1)
		{
			int[] switches = info.Switches;

			if (switches.Length > 0)
			{
				int index = switches[0] % _mEntries.Length;
				int offset = switches[0] / _mEntries.Length;

				if (index < 0 || index >= _mEntries.Length) return;
				if (offset >= 0 && offset < _mEntries[index].Hues.Length)
				{
					if (_mHair && _mFrom.HairItemID > 0 || _mFacialHair && _mFrom.FacialHairItemID > 0)
					{
						if (!Banker.Withdraw(_mFrom, _mPrice))
						{
							_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _mFrom.NetState); // You cannot afford my services for that style.
							return;
						}

						int hue = _mEntries[index].Hues[offset];

						if (_mHair)
							_mFrom.HairHue = hue;

						if (_mFacialHair)
							_mFrom.FacialHairHue = hue;
					}
					else
						_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502623, _mFrom.NetState); // You have no hair to dye and you cannot use this.
				}
			}
			else
			{
				// You decide not to change your hairstyle.
				_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _mFrom.NetState);
			}
		}
		else
		{
			// You decide not to change your hairstyle.
			_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _mFrom.NetState);
		}
	}
}

public class ChangeHairstyleEntry
{
	public int ItemId { get; }
	public int GumpId { get; }
	public int X { get; }
	public int Y { get; }

	public ChangeHairstyleEntry(int gumpId, int x, int y, int itemId)
	{
		GumpId = gumpId;
		X = x;
		Y = y;
		ItemId = itemId;
	}

	public static readonly ChangeHairstyleEntry[] HairEntries = {
		new( 50700,  70 - 137,  20 -  60, 0x203B ),
		new( 60710, 193 - 260,  18 -  60, 0x2045 ),
		new( 50703, 316 - 383,  25 -  60, 0x2044 ),
		new( 60708,  70 - 137,  75 - 125, 0x203C ),
		new( 60900, 193 - 260,  85 - 125, 0x2047 ),
		new( 60713, 320 - 383,  85 - 125, 0x204A ),
		new( 60702,  70 - 137, 140 - 190, 0x203D ),
		new( 60707, 193 - 260, 140 - 190, 0x2049 ),
		new( 60901, 315 - 383, 150 - 190, 0x2048 ),
		new( 0, 0, 0, 0 )
	};

	public static readonly ChangeHairstyleEntry[] BeardEntries = {
		new( 50800, 120 - 187,  30 -  80, 0x2040 ),
		new( 50904, 243 - 310,  33 -  80, 0x204B ),
		new( 50906, 120 - 187, 100 - 150, 0x204D ),
		new( 50801, 243 - 310,  95 - 150, 0x203E ),
		new( 50802, 120 - 187, 173 - 220, 0x203F ),
		new( 50905, 243 - 310, 165 - 220, 0x204C ),
		new( 50808, 120 - 187, 242 - 290, 0x2041 ),
		new( 0, 0, 0, 0 )
	};
}

public class ChangeHairstyleGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly Mobile _mVendor;
	private readonly int _mPrice;
	private readonly bool _mFacialHair;
	private readonly ChangeHairstyleEntry[] _mEntries;

	public ChangeHairstyleGump(Mobile from, Mobile vendor, int price, bool facialHair, ChangeHairstyleEntry[] entries) : base(50, 50)
	{
		_mFrom = from;
		_mVendor = vendor;
		_mPrice = price;
		_mFacialHair = facialHair;
		_mEntries = entries;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		int tableWidth = _mFacialHair ? 2 : 3;
		int tableHeight = (entries.Length + tableWidth - (_mFacialHair ? 1 : 2)) / tableWidth;
		int offsetWidth = 123;
		int offsetHeight = _mFacialHair ? 70 : 65;

		AddPage(0);

		AddBackground(0, 0, 81 + tableWidth * offsetWidth, 105 + tableHeight * offsetHeight, 2600);

		AddButton(45, 45 + tableHeight * offsetHeight, 4005, 4007, 1, GumpButtonType.Reply, 0);
		AddHtmlLocalized(77, 45 + tableHeight * offsetHeight, 90, 35, 1006044, false, false); // Ok

		AddButton(81 + tableWidth * offsetWidth - 180, 45 + tableHeight * offsetHeight, 4005, 4007, 0, GumpButtonType.Reply, 0);
		AddHtmlLocalized(81 + tableWidth * offsetWidth - 148, 45 + tableHeight * offsetHeight, 90, 35, 1006045, false, false); // Cancel

		if (!facialHair)
			AddHtmlLocalized(50, 15, 350, 20, 1018353, false, false); // <center>New Hairstyle</center>
		else
			AddHtmlLocalized(55, 15, 200, 20, 1018354, false, false); // <center>New Beard</center>

		for (var i = 0; i < entries.Length; ++i)
		{
			int xTable = i % tableWidth;
			int yTable = i / tableWidth;

			if (entries[i].GumpId != 0)
			{
				AddRadio(40 + xTable * offsetWidth, 70 + yTable * offsetHeight, 208, 209, false, i);
				AddBackground(87 + xTable * offsetWidth, 50 + yTable * offsetHeight, 50, 50, 2620);
				AddImage(87 + xTable * offsetWidth + entries[i].X, 50 + yTable * offsetHeight + entries[i].Y, entries[i].GumpId);
			}
			else if (!facialHair)
			{
				AddRadio(40 + (xTable + 1) * offsetWidth, 240, 208, 209, false, i);
				AddHtmlLocalized(60 + ((xTable + 1) * offsetWidth), 240, 85, 35, 1011064, false, false); // Bald
			}
			else
			{
				AddRadio(40 + xTable * offsetWidth, 70 + yTable * offsetHeight, 208, 209, false, i);
				AddHtmlLocalized(60 + xTable * offsetWidth, 70 + yTable * offsetHeight, 85, 35, 1011064, false, false); // Bald
			}
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_mFacialHair && (_mFrom.Female || _mFrom.Body.IsFemale))
			return;

		if (_mFrom.Race == Race.Elf)
		{
			_mFrom.SendMessage("This isn't implemented for elves yet.  Sorry!");
			return;
		}

		if (info.ButtonID == 1)
		{
			int[] switches = info.Switches;

			if (switches.Length > 0)
			{
				int index = switches[0];

				if (index < 0 || index >= _mEntries.Length) return;
				ChangeHairstyleEntry entry = _mEntries[index];

				if (_mFrom is PlayerMobile mobile)
					mobile.SetHairMods(-1, -1);

				int hairId = _mFrom.HairItemID;
				int facialHairId = _mFrom.FacialHairItemID;

				if (entry.ItemId == 0)
				{
					if (_mFacialHair ? facialHairId == 0 : hairId == 0)
						return;

					if (Banker.Withdraw(_mFrom, _mPrice))
					{
						if (_mFacialHair)
							_mFrom.FacialHairItemID = 0;
						else
							_mFrom.HairItemID = 0;
					}
					else
						_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _mFrom.NetState); // You cannot afford my services for that style.
				}
				else
				{
					if (_mFacialHair)
					{
						if (facialHairId > 0 && facialHairId == entry.ItemId)
							return;
					}
					else
					{
						if (hairId > 0 && hairId == entry.ItemId)
							return;
					}

					if (Banker.Withdraw(_mFrom, _mPrice))
					{
						if (_mFacialHair)
							_mFrom.FacialHairItemID = entry.ItemId;
						else
							_mFrom.HairItemID = entry.ItemId;
					}
					else
						_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _mFrom.NetState); // You cannot afford my services for that style.
				}
			}
			else
			{
				// You decide not to change your hairstyle.
				_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _mFrom.NetState);
			}
		}
		else
		{
			// You decide not to change your hairstyle.
			_mVendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _mFrom.NetState);
		}
	}
}
