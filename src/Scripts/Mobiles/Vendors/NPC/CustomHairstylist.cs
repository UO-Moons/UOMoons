using Server.Gumps;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
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

	private static readonly HairstylistBuyInfo[] m_SellListElf = {
		new(
			1018357,
			50000,
			false,
			typeof(ChangeHairstyleGump),
			new[] {From, Vendor, Price, false, ChangeHairstyleEntry.HairEntriesElf}),
		new(
			1018359,
			50,
			false,
			typeof(ChangeHairHueGump),
			new[] {From, Vendor, Price, true, true, ChangeHairHueEntry.RegularEntries}),
		new(
			1018360,
			500000,
			false,
			typeof(ChangeHairHueGump),
			new[] {From, Vendor, Price, true, true, ChangeHairHueEntry.BrightEntries})
	};

	public override void VendorBuy(Mobile from)
	{
		if (from.Race == Race.Human)
		{
			from.SendGump(new HairstylistBuyGump(from, this, MSellList));
		}
		else if (from.Race == Race.Elf)
		{
			from.SendGump(new HairstylistBuyGump(from, this, m_SellListElf));
		}
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

		SetWearable(new Robe(Utility.RandomPinkHue()));
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

	public int Title { get; }
	public string TitleString { get; }
	public int Price { get; }
	public bool FacialHair { get; }
	public Type GumpType { get; }
	public object[] GumpArgs { get; }
}

public class HairstylistBuyGump : Gump
{
	private readonly Mobile _from;
	private readonly Mobile _vendor;
	private readonly HairstylistBuyInfo[] _sellList;

	public HairstylistBuyGump(Mobile from, Mobile vendor, HairstylistBuyInfo[] sellList)
		: base(50, 50)
	{
		_from = from;
		_vendor = vendor;
		_sellList = sellList;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		bool isFemale = from.Female || from.Body.IsFemale;

		int balance = Banker.GetBalance(from);
		int canAfford = sellList.Count(t => balance >= t.Price && (!t.FacialHair || !isFemale));

		AddPage(0);

		AddBackground(50, 10, 450, 100 + canAfford * 25, 2600);

		AddHtmlLocalized(100, 40, 350, 20, 1018356, false, false); // Choose your hairstyle change:

		int index = 0;

		for (int i = 0; i < sellList.Length; ++i)
		{
			if (balance >= sellList[i].Price && (!sellList[i].FacialHair || !isFemale))
			{
				if (sellList[i].TitleString != null)
				{
					AddHtml(140, 75 + index * 25, 300, 20, sellList[i].TitleString, false, false);
				}
				else
				{
					AddHtmlLocalized(140, 75 + index * 25, 300, 20, sellList[i].Title, false, false);
				}

				AddButton(100, 75 + index++ * 25, 4005, 4007, 1 + i, GumpButtonType.Reply, 0);
			}
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		int index = info.ButtonID - 1;

		if (index >= 0 && index < _sellList.Length)
		{
			HairstylistBuyInfo buyInfo = _sellList[index];

			int balance = Banker.GetBalance(_from);

			bool isFemale = _from.Female || _from.Body.IsFemale;

			if (buyInfo.FacialHair && isFemale)
			{
				// You cannot place facial hair on a woman!
				_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1010639, _from.NetState);
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
						{
							args[i] = _sellList[index].Price;
						}
						else if (origArgs[i] == CustomHairstylist.From)
						{
							args[i] = _from;
						}
						else if (origArgs[i] == CustomHairstylist.Vendor)
						{
							args[i] = _vendor;
						}
						else
						{
							args[i] = origArgs[i];
						}
					}

					Gump g = Activator.CreateInstance(buyInfo.GumpType, args) as Gump;

					_from.SendGump(g);
				}
				catch
				{
					// ignored
				}
			}
			else
			{
				// You cannot afford my services for that style.
				_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _from.NetState);
			}
		}
	}
}

public class ChangeHairHueEntry
{
	public static readonly ChangeHairHueEntry[] BrightEntries = {
			new("*****", 12, 10), new("*****", 32, 5),
			new("*****", 38, 8), new("*****", 54, 3),
			new("*****", 62, 10), new("*****", 81, 2),
			new("*****", 89, 2), new("*****", 1153, 2)
		};

	public static readonly ChangeHairHueEntry[] RegularEntries = {
			new("*****", 1602, 26), new("*****", 1628, 27),
			new("*****", 1502, 32), new("*****", 1302, 32),
			new("*****", 1402, 32), new("*****", 1202, 24),
			new("*****", 2402, 29), new("*****", 2213, 6),
			new("*****", 1102, 8), new("*****", 1110, 8),
			new("*****", 1118, 16), new("*****", 1134, 16)
		};

	public ChangeHairHueEntry(string name, int[] hues)
	{
		Name = name;
		Hues = hues;
	}

	public ChangeHairHueEntry(string name, int start, int count)
	{
		Name = name;

		Hues = new int[count];

		for (int i = 0; i < count; ++i)
		{
			Hues[i] = start + i;
		}
	}

	public string Name { get; }

	public int[] Hues { get; }
}

public class ChangeHairHueGump : Gump
{
	private readonly Mobile _from;
	private readonly Mobile _vendor;
	private readonly int _price;
	private readonly bool _hair;
	private readonly bool _facialHair;
	private readonly ChangeHairHueEntry[] _entries;

	public ChangeHairHueGump(
		Mobile from, Mobile vendor, int price, bool hair, bool facialHair, ChangeHairHueEntry[] entries)
		: base(50, 50)
	{
		_from = from;
		_vendor = vendor;
		_price = price;
		_hair = hair;
		_facialHair = facialHair;
		_entries = entries;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		AddPage(0);

		AddBackground(100, 10, 350, 370, 2600);
		AddBackground(120, 54, 110, 270, 5100);

		AddHtmlLocalized(155, 25, 240, 30, 1011013, false, false); // <center>Hair Color Selection Menu</center>

		AddHtmlLocalized(150, 330, 220, 35, 1011014, false, false); // Dye my hair this color!
		AddButton(380, 330, 4005, 4007, 1, GumpButtonType.Reply, 0);

		for (int i = 0; i < entries.Length; ++i)
		{
			ChangeHairHueEntry entry = entries[i];

			AddLabel(130, 59 + i * 22, entry.Hues[0] - 1, entry.Name);
			AddButton(207, 60 + i * 22, 5224, 5224, 0, GumpButtonType.Page, 1 + i);
		}

		for (int i = 0; i < entries.Length; ++i)
		{
			ChangeHairHueEntry entry = entries[i];
			int[] hues = entry.Hues;
			string name = entry.Name;

			AddPage(1 + i);

			for (int j = 0; j < hues.Length; ++j)
			{
				AddLabel(278 + j / 16 * 80, 52 + j % 16 * 17, hues[j] - 1, name);
				AddRadio(260 + j / 16 * 80, 52 + j % 16 * 17, 210, 211, false, j * entries.Length + i);
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
				int index = switches[0] % _entries.Length;
				int offset = switches[0] / _entries.Length;

				if (index >= 0 && index < _entries.Length)
				{
					if (offset >= 0 && offset < _entries[index].Hues.Length)
					{
						if (_hair && _from.HairItemId > 0 || _facialHair && _from.FacialHairItemId > 0)
						{
							if (_price > 0 && !Banker.Withdraw(_from, _price))
							{
								_vendor?.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _from.NetState);
								// You cannot afford my services for that style.

								return;
							}

							int hue = _entries[index].Hues[offset];

							if (_hair)
							{
								_from.HairHue = hue;
							}

							if (_facialHair)
							{
								_from.FacialHairHue = hue;
							}
						}
						else
						{
							if (_vendor != null)
							{
								_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502623, _from.NetState); // You have no hair to dye and you cannot use this.
							}
							else
							{
								_from.SendLocalizedMessage(502623);
							}
						}
					}
				}
			}
			else
			{
				if (_vendor != null)
				{
					_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _from.NetState); // You decide not to change your hairstyle.
				}
				else
				{
					_from.SendLocalizedMessage(1013009);
				}
			}
		}
		else
		{
			// You decide not to change your hairstyle.
			_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _from.NetState);
		}
	}
}

public class ChangeHairstyleEntry
{
	public static readonly ChangeHairstyleEntry[] HairEntries = {
			new(50700, 70 - 137, 20 - 60, 0x203B),
			new(60710, 193 - 260, 18 - 60, 0x2045),
			new(50703, 316 - 383, 25 - 60, 0x2044),
			new(60701, 70 - 137, 75 - 125, 0x203C),
			new(60900, 193 - 260, 85 - 125, 0x2047),
			new(60713, 320 - 383, 85 - 125, 0x204A),
			new(60702, 70 - 137, 140 - 190, 0x203D),
			new(1836, 173 - 260, 128 - 190, 0x2049),
			new(1841, 60901, 315 - 383, 150 - 190, 0x2046, 0x2048),
			new(0, 0, 0, 0)
		};

	public static readonly ChangeHairstyleEntry[] BeardEntries = {
			new(50800, 120 - 187, 30 - 80, 0x2040),
			new(50904, 243 - 310, 33 - 80, 0x204B),
			new(50906, 120 - 187, 100 - 150, 0x204D),
			new(50801, 243 - 310, 95 - 150, 0x203E),
			new(50802, 120 - 187, 173 - 220, 0x203F),
			new(50905, 243 - 310, 165 - 220, 0x204C),
			new(50808, 120 - 187, 242 - 290, 0x2041), new(0, 0, 0, 0)
		};

	public static readonly ChangeHairstyleEntry[] HairEntriesElf = {
			new( 0xEDF5, 0xC6E5, 70 - 137,   20 -  60,  0x2FC0, 0x2FC0 ),
			new( 0xEDF6, 0xC6E6, 198 - 260,  18 -  60,  0x2FC1, 0x2FC1 ),
			new( 0xEDF7, 0xC6E7, 316 - 383,  20 -  60,  0x2FC2, 0x2FC2 ),
			new( 0xEDDC, 0xC6CC, 70 - 137,   80 - 125,  0x2FCE, 0x2FCE ),
			new( 0xEDDD, 0xC6CD, 193 - 260,  85 - 125,  0x2FCF, 0x2FCF ),
			new( 0xEDDF, 0xC6CF, 320 - 383,  85 - 125,  0x2FD1, 0x2FD1 ),
			new( 0xEDDA, 0xC6E4, 70 - 137,   147 - 190, 0x2FCC, 0x2FBF ),
			new( 0xEDDE, 0xC6CB, 196 - 260,  142 - 190, 0x2FD0, 0x2FCD ),
			new( -1, -1, -1, -1 ),
			new( 0, 0, 0, 0 )
	};

	public static readonly ChangeHairstyleEntry[] HairEntriesGargoyle = {
			new( 0x7A0, 0x76C, 47 - 137,   12 -  60,  0x4261, 0x4258  ),
			new( 0x7A1, 0x76D, 170 - 260,  12 -  60,  0x4262, 0x4259 ),
			new( 0x79E, 0x773, 295 - 383,  12 -  60,  0x4273, 0x425A ),
			new( 0x7A2, 0x76E, 50 - 137,   68 - 125,  0x4274, 0x425B ),
			new( 0x79F, 0x774, 172 - 260,  70 - 125,  0x4275, 0x425C ),
			new( 0x77C, 0x775, 295 - 383,  81 - 125,  0x42AA, 0x425D ),
			new( 0x77D, 0x776, 47 - 137,   142 - 190, 0x42AB, 0x425E ),
			new( 0x77E, 0x777, 172 - 260,  142 - 190, 0x42B1, 0x425F ),
			new( -1, -1, -1, -1 ),
			new( 0, 0, 0, 0 )
	};

	public static readonly ChangeHairstyleEntry[] BeardEntriesGargoyle = {
			new( 0xC5E9, 120 - 187,  30 -  80, 0x42AD ),
			new( 0x770,  220 - 310,  23 -  80, 0x42AE ),
			new( 0xC5DA, 120 - 187, 100 - 150, 0x42AF ),
			new( 0xC5D7, 243 - 310,  95 - 150, 0x42B0 ),
			new( 0, 0, 0, 0 )
	};

	public int ItemIdMale { get; }
	public int ItemIdFemale { get; }
	public int GumpIdMale { get; }
	public int GumpIdFemale { get; }
	public int X { get; }
	public int Y { get; }

	public ChangeHairstyleEntry(int gumpId, int x, int y, int itemId)
		: this(gumpId, gumpId, x, y, itemId, itemId)
	{
	}

	public ChangeHairstyleEntry(int gumpIdFemale, int gumpIdMale, int x, int y, int itemIdFemale, int itemIdMale)
	{
		GumpIdMale = gumpIdMale;
		GumpIdFemale = gumpIdFemale;
		X = x;
		Y = y;
		ItemIdMale = itemIdMale;
		ItemIdFemale = itemIdFemale;
	}
}

public class ChangeHairstyleGump : Gump
{
	private readonly Mobile _from;
	private readonly Mobile _vendor;
	private readonly int _price;
	private readonly bool _facialHair;
	private readonly ChangeHairstyleEntry[] _entries;

	public bool Female;
	public GenderChangeToken Token;

	public ChangeHairstyleGump(Mobile from, Mobile vendor, int price, bool facialHair, ChangeHairstyleEntry[] entries)
		: this(from, vendor, price, facialHair, entries, null)
	{
	}

	public ChangeHairstyleGump(Mobile from, Mobile vendor, int price, bool facialHair, ChangeHairstyleEntry[] entries, GenderChangeToken token)
		: this(from.Female, from, vendor, price, facialHair, entries, token)
	{
	}

	public ChangeHairstyleGump(bool female, Mobile from, Mobile vendor, int price, bool facialHair, ChangeHairstyleEntry[] entries, GenderChangeToken token)
		: base(50, 50)
	{
		_from = from;
		_vendor = vendor;
		_price = price;
		_facialHair = facialHair;
		_entries = entries;
		Female = female;

		Token = token;

		from.CloseGump(typeof(HairstylistBuyGump));
		from.CloseGump(typeof(ChangeHairHueGump));
		from.CloseGump(typeof(ChangeHairstyleGump));

		int tableWidth = _facialHair ? 2 : 3;
		int tableHeight = (entries.Length + tableWidth - (_facialHair ? 1 : 2)) / tableWidth;
		const int offsetWidth = 123;
		int offsetHeight = _facialHair ? 70 : 65;

		AddPage(0);

		AddBackground(0, 0, 81 + tableWidth * offsetWidth, 145 + tableHeight * offsetHeight, 2600);

		AddButton(45, 90 + tableHeight * offsetHeight, 4005, 4007, 1, GumpButtonType.Reply, 0);
		AddHtmlLocalized(77, 90 + tableHeight * offsetHeight, 90, 35, 1006044, false, false); // Ok

		AddButton(
			90 + tableWidth * offsetWidth - 180, 85 + tableHeight * offsetHeight, 4005, 4007, 0, GumpButtonType.Reply, 0);
		AddHtmlLocalized(
			90 + tableWidth * offsetWidth - 148, 85 + tableHeight * offsetHeight, 90, 35, 1006045, false, false); // Cancel

		if (!facialHair)
		{
			AddHtmlLocalized(50, 15, 350, 20, 1018353, false, false); // <center>New Hairstyle</center>
		}
		else
		{
			AddHtmlLocalized(55, 15, 200, 20, 1018354, false, false); // <center>New Beard</center>
		}

		for (int i = 0; i < entries.Length; ++i)
		{
			int xTable = i % tableWidth;
			int yTable = i / tableWidth;
			int gumpId = female ? entries[i].GumpIdFemale : entries[i].GumpIdMale;

			if (gumpId == -1)
				continue;

			if (gumpId != 0)
			{
				AddRadio(40 + xTable * offsetWidth, 70 + yTable * offsetHeight, 208, 209, false, i);
				AddBackground(87 + xTable * offsetWidth, 50 + yTable * offsetHeight, 50, 50, 2620);

				int x = entries[i].X;
				int y = entries[i].Y;

				if (gumpId == 1841)
				{
					x -= 17;
					y -= 17;
				}

				AddImage(87 + xTable * offsetWidth + x, 50 + yTable * offsetHeight + y, gumpId);
			}
			else if (!facialHair)
			{
				AddRadio(40 + xTable * offsetWidth, 240, 208, 209, false, i);
				AddHtmlLocalized(60 + xTable * offsetWidth, 240, 200, 40, 1011064, false, false); // Bald
			}
			else
			{
				AddRadio(40 + xTable * offsetWidth, 70 + yTable * offsetHeight, 208, 209, false, i);
				//AddHtmlLocalized(60 + (xTable * offsetWidth), 70 + (yTable * offsetHeight), 85, 35, 1011064, false, false); // Bald
			}
		}
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (!_facialHair || !Female)
		{
			if (info.ButtonID == 1)
			{
				int[] switches = info.Switches;

				if (switches.Length > 0)
				{
					int index = switches[0];
					bool female = Female;

					if (index >= 0 && index < _entries.Length)
					{
						ChangeHairstyleEntry entry = _entries[index];

						if (_from is PlayerMobile mobile)
						{
							mobile.SetHairMods(-1, -1);
						}

						int hairId = _from.HairItemId;
						int facialHairId = _from.FacialHairItemId;
						int itemId = female ? entry.ItemIdFemale : entry.ItemIdMale;

						if (itemId == 0)
						{
							bool invalid = _facialHair ? facialHairId == 0 : hairId == 0;

							if (!invalid)
							{
								if (Token != null)
								{
									Token.OnChangeHairstyle(_from, _facialHair, 0);
									return;
								}

								if (Banker.Withdraw(_from, _price, true))
								{
									if (_facialHair)
									{
										_from.FacialHairItemId = 0;
									}
									else
									{
										_from.HairItemId = 0;
									}
								}
								else
								{
									if (_vendor != null)
									{
										_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _from.NetState);
										// You cannot afford my services for that style.
									}
									else
									{
										_from.SendLocalizedMessage(1042293);
									}
								}
							}
						}
						else
						{
							bool invalid = _facialHair ? facialHairId > 0 && facialHairId == itemId : hairId > 0 && hairId == itemId;

							if (!invalid)
							{
								if (_price <= 0 || Banker.Withdraw(_from, _price))
								{
									if (Token != null)
									{
										Token.OnChangeHairstyle(_from, _facialHair, itemId);
										return;
									}

									if (_facialHair)
									{
										var old = _from.FacialHairItemId;

										_from.FacialHairItemId = itemId;

										if (old == 0)
										{
											_from.FacialHairHue = _from.HairHue;
										}
									}
									else
									{
										_from.HairItemId = itemId;
									}
								}
								else
								{
									if (_vendor != null)
									{
										_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042293, _from.NetState);
										// You cannot afford my services for that style.
									}
									else
									{
										_from.SendLocalizedMessage(1042293);
									}
								}
							}
						}
					}
				}
				else
				{
					if (_vendor != null)
					{
						// You decide not to change your hairstyle.
						_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _from.NetState);
					}
					else
					{
						_from.SendLocalizedMessage(1013009); // You decide not to change your hairstyle. 
					}
				}
			}
			else
			{
				if (_vendor != null)
				{
					// You decide not to change your hairstyle.
					_vendor.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1013009, _from.NetState);
				}
				else
				{
					_from.SendLocalizedMessage(1013009); // You decide not to change your hairstyle. 
				}
			}
		}

		Token?.OnFailedHairstyle(_from, _facialHair);
	}
}
