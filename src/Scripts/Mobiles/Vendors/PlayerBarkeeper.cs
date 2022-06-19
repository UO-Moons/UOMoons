using Server.ContextMenus;
using Server.Gumps;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Prompts;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public class ChangeRumorMessagePrompt : Prompt
{
	private readonly PlayerBarkeeper _mBarkeeper;
	private readonly int _mRumorIndex;

	public ChangeRumorMessagePrompt(PlayerBarkeeper barkeeper, int rumorIndex)
	{
		_mBarkeeper = barkeeper;
		_mRumorIndex = rumorIndex;
	}

	public override void OnCancel(Mobile from)
	{
		OnResponse(from, "");
	}

	public override void OnResponse(Mobile from, string text)
	{
		if (text.Length > 130)
			text = text[..130];

		_mBarkeeper.EndChangeRumor(from, _mRumorIndex, text);
	}
}

public class ChangeRumorKeywordPrompt : Prompt
{
	private readonly PlayerBarkeeper _mBarkeeper;
	private readonly int _mRumorIndex;

	public ChangeRumorKeywordPrompt(PlayerBarkeeper barkeeper, int rumorIndex)
	{
		_mBarkeeper = barkeeper;
		_mRumorIndex = rumorIndex;
	}

	public override void OnCancel(Mobile from)
	{
		OnResponse(from, "");
	}

	public override void OnResponse(Mobile from, string text)
	{
		if (text.Length > 130)
			text = text[..130];

		_mBarkeeper.EndChangeKeyword(from, _mRumorIndex, text);
	}
}

public class ChangeTipMessagePrompt : Prompt
{
	private readonly PlayerBarkeeper _mBarkeeper;

	public ChangeTipMessagePrompt(PlayerBarkeeper barkeeper)
	{
		_mBarkeeper = barkeeper;
	}

	public override void OnCancel(Mobile from)
	{
		OnResponse(from, "");
	}

	public override void OnResponse(Mobile from, string text)
	{
		if (text.Length > 130)
			text = text[..130];

		_mBarkeeper.EndChangeTip(from, text);
	}
}

public class BarkeeperRumor
{
	public string Message { get; set; }
	public string Keyword { get; set; }

	public BarkeeperRumor(string message, string keyword)
	{
		Message = message;
		Keyword = keyword;
	}

	public static BarkeeperRumor Deserialize(GenericReader reader)
	{
		return !reader.ReadBool() ? null : new BarkeeperRumor(reader.ReadString(), reader.ReadString());
	}

	public static void Serialize(GenericWriter writer, BarkeeperRumor rumor)
	{
		if (rumor == null)
		{
			writer.Write(false);
		}
		else
		{
			writer.Write(true);
			writer.Write(rumor.Message);
			writer.Write(rumor.Keyword);
		}
	}
}

public class ManageBarkeeperEntry : ContextMenuEntry
{
	private readonly Mobile _mFrom;
	private readonly PlayerBarkeeper _mBarkeeper;

	public ManageBarkeeperEntry(Mobile from, PlayerBarkeeper barkeeper) : base(6151, 12)
	{
		_mFrom = from;
		_mBarkeeper = barkeeper;
	}

	public override void OnClick()
	{
		_mBarkeeper.BeginManagement(_mFrom);
	}
}

public class PlayerBarkeeper : BaseVendor
{
	private BaseHouse _mHouse;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; set; }

	public BaseHouse House
	{
		get => _mHouse;
		set
		{
			_mHouse?.PlayerBarkeepers.Remove(this);

			value?.PlayerBarkeepers.Add(this);

			_mHouse = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string TipMessage { get; set; }

	public override bool IsActiveBuyer => false;
	public override bool IsActiveSeller => _mSbInfos.Count > 0;

	public override bool DisallowAllMoves => true;
	public override bool NoHouseRestrictions => true;

	public BarkeeperRumor[] Rumors { get; private set; }

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.ThighBoots : VendorShoeType.Boots;

	public override bool GetGender()
	{
		return false; // always starts as male
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new HalfApron(Utility.RandomBrightHue()));

		Container pack = Backpack;

		pack?.Delete();
	}

	public override void InitBody()
	{
		base.InitBody();

		if (BodyValue == 0x340 || BodyValue == 0x402)
			Hue = 0;
		else
			Hue = 0x83F4; // hue is not random

		Container pack = Backpack;

		pack?.Delete();
	}

	public PlayerBarkeeper(Mobile owner, BaseHouse house) : base("the barkeeper")
	{
		Owner = owner;
		House = house;
		Rumors = new BarkeeperRumor[3];

		LoadSbInfo();
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		return InRange(from, 3) || base.HandlesOnSpeech(from);
	}

	private Timer _mNewsTimer;

	private void ShoutNews_Callback(object state)
	{
		object[] states = (object[])state;
		TownCrierEntry tce = (TownCrierEntry)states[0];
		int index = (int)states[1];

		if (index < 0 || index >= tce.Lines.Length)
		{
			_mNewsTimer?.Stop();

			_mNewsTimer = null;
		}
		else
		{
			PublicOverheadMessage(MessageType.Regular, 0x3B2, false, tce.Lines[index]);
			states[1] = index + 1;
		}
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		House = null;
	}

	public override bool OnBeforeDeath()
	{
		if (!base.OnBeforeDeath())
			return false;

		Item shoes = FindItemOnLayer(Layer.Shoes);

		if (shoes is Sandals)
			shoes.Hue = 0;

		return true;
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		base.OnSpeech(e);

		if (!e.Handled && InRange(e.Mobile, 3))
		{
			if (_mNewsTimer == null && e.HasKeyword(0x30)) // *news*
			{
				TownCrierEntry tce = GlobalTownCrierEntryList.Instance.GetRandomEntry();

				if (tce == null)
				{
					PublicOverheadMessage(MessageType.Regular, 0x3B2, 1005643); // I have no news at this time.
				}
				else
				{
					_mNewsTimer = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(3.0), new TimerStateCallback(ShoutNews_Callback), new object[] { tce, 0 });

					PublicOverheadMessage(MessageType.Regular, 0x3B2, 502978); // Some of the latest news!
				}
			}

			for (var i = 0; i < Rumors.Length; ++i)
			{
				BarkeeperRumor rumor = Rumors[i];

				string keyword = rumor?.Keyword;

				if (keyword == null || (keyword = keyword.Trim()).Length == 0)
					continue;

				if (!Insensitive.Equals(keyword, e.Speech)) continue;
				string message = rumor.Message;

				if (message == null || (message = message.Trim()).Length == 0)
					continue;

				PublicOverheadMessage(MessageType.Regular, 0x3B2, false, message);
			}
		}
	}

	public override bool CheckGold(Mobile from, Item dropped)
	{
		if (dropped is not Gold) return false;
		Gold g = (Gold)dropped;

		if (g.Amount > 50)
		{
			PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "I cannot accept so large a tip!", from.NetState);
		}
		else
		{
			string tip = TipMessage;

			if (tip == null || (tip = tip.Trim()).Length == 0)
			{
				PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "It would not be fair of me to take your money and not offer you information in return.", from.NetState);
			}
			else
			{
				Direction = GetDirectionTo(from);
				PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, tip, from.NetState);

				g.Delete();
				return true;
			}
		}

		return false;
	}

	public bool IsOwner(Mobile from)
	{
		if (from == null || from.Deleted || Deleted)
			return false;

		if (from.AccessLevel > AccessLevel.GameMaster)
			return true;

		return (Owner == from);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (IsOwner(from) && from.InLOS(this))
			list.Add(new ManageBarkeeperEntry(from, this));
	}

	public void BeginManagement(Mobile from)
	{
		if (!IsOwner(from))
			return;

		from.SendGump(new BarkeeperGump(from, this));
	}

	public void Dismiss()
	{
		Delete();
	}

	public void BeginChangeRumor(Mobile from, int index)
	{
		if (index < 0 || index >= Rumors.Length)
			return;

		from.Prompt = new ChangeRumorMessagePrompt(this, index);
		PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "Say what news you would like me to tell our guests.", from.NetState);
	}

	public void EndChangeRumor(Mobile from, int index, string text)
	{
		if (index < 0 || index >= Rumors.Length)
			return;

		if (Rumors[index] == null)
			Rumors[index] = new BarkeeperRumor(text, null);
		else
			Rumors[index].Message = text;

		from.Prompt = new ChangeRumorKeywordPrompt(this, index);
		PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "What keyword should a guest say to me to get this news?", from.NetState);
	}

	public void EndChangeKeyword(Mobile from, int index, string text)
	{
		if (index < 0 || index >= Rumors.Length)
			return;

		if (Rumors[index] == null)
			Rumors[index] = new BarkeeperRumor(null, text);
		else
			Rumors[index].Keyword = text;

		PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "I'll pass on the message.", from.NetState);
	}

	public void RemoveRumor(Mobile from, int index)
	{
		if (index < 0 || index >= Rumors.Length)
			return;

		Rumors[index] = null;
	}

	public void BeginChangeTip(Mobile from)
	{
		from.Prompt = new ChangeTipMessagePrompt(this);
		PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "Say what you want me to tell guests when they give me a good tip.", from.NetState);
	}

	public void EndChangeTip(Mobile from, string text)
	{
		TipMessage = text;
		PrivateOverheadMessage(MessageType.Regular, 0x3B2, false, "I'll say that to anyone who gives me a good tip.", from.NetState);
	}

	public void RemoveTip(Mobile from)
	{
		TipMessage = null;
	}

	public void BeginChangeTitle(Mobile from)
	{
		from.SendGump(new BarkeeperTitleGump(from, this));
	}

	public void EndChangeTitle(Mobile from, string title, bool vendor)
	{
		Title = title;

		LoadSbInfo();
	}

	public void CancelChangeTitle(Mobile from)
	{
		from.SendGump(new BarkeeperGump(from, this));
	}

	public void BeginChangeAppearance(Mobile from)
	{
		from.CloseGump(typeof(PlayerVendorCustomizeGump));
		from.SendGump(new PlayerVendorCustomizeGump(this, from));
	}

	public void ChangeGender(Mobile from)
	{
		Female = !Female;

		if (Female)
		{
			Body = 401;
			Name = NameList.RandomName("female");

			FacialHairItemID = 0;
		}
		else
		{
			Body = 400;
			Name = NameList.RandomName("male");
		}
	}

	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override void InitSbInfo()
	{
		if (Title is "the waiter" or "the barkeeper" or "the baker" or "the innkeeper" or "the chef")
		{
			if (_mSbInfos.Count == 0)
				_mSbInfos.Add(new SbPlayerBarkeeper());
		}
		else
		{
			_mSbInfos.Clear();
		}
	}

	public PlayerBarkeeper(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version;

		writer.Write(_mHouse);

		writer.Write(Owner);

		writer.WriteEncodedInt(Rumors.Length);

		for (var i = 0; i < Rumors.Length; ++i)
			BarkeeperRumor.Serialize(writer, Rumors[i]);

		writer.Write(TipMessage);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				House = (BaseHouse)reader.ReadItem();

				Owner = reader.ReadMobile();

				Rumors = new BarkeeperRumor[reader.ReadEncodedInt()];

				for (var i = 0; i < Rumors.Length; ++i)
					Rumors[i] = BarkeeperRumor.Deserialize(reader);

				TipMessage = reader.ReadString();

				break;
			}
		}

		if (version < 1)
			Timer.DelayCall(TimeSpan.Zero, UpgradeFromVersion0);
	}

	private void UpgradeFromVersion0()
	{
		House = BaseHouse.FindHouseAt(this);
	}
}

public class BarkeeperTitleGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly PlayerBarkeeper _mBarkeeper;

	private class Entry
	{
		public readonly string MDescription;
		public readonly string MTitle;
		public readonly bool MVendor;

		public Entry(string desc) : this(desc, $"the {desc.ToLower()}", false)
		{
		}

		public Entry(string desc, bool vendor) : this(desc, $"the {desc.ToLower()}", vendor)
		{
		}

		public Entry(string desc, string title, bool vendor)
		{
			MDescription = desc;
			MTitle = title;
			MVendor = vendor;
		}
	}

	private static readonly Entry[] MEntries = {
		new( "Alchemist" ),
		new( "Animal Tamer" ),
		new( "Apothecary" ),
		new( "Artist" ),
		new( "Baker", true ),
		new( "Bard" ),
		new( "Barkeep", "the barkeeper", true ),
		new( "Beggar" ),
		new( "Blacksmith" ),
		new( "Bounty Hunter" ),
		new( "Brigand" ),
		new( "Butler" ),
		new( "Carpenter" ),
		new( "Chef", true ),
		new( "Commander" ),
		new( "Curator" ),
		new( "Drunkard" ),
		new( "Farmer" ),
		new( "Fisherman" ),
		new( "Gambler" ),
		new( "Gypsy" ),
		new( "Herald" ),
		new( "Herbalist" ),
		new( "Hermit" ),
		new( "Innkeeper", true ),
		new( "Jailor" ),
		new( "Jester" ),
		new( "Librarian" ),
		new( "Mage" ),
		new( "Mercenary" ),
		new( "Merchant" ),
		new( "Messenger" ),
		new( "Miner" ),
		new( "Monk" ),
		new( "Noble" ),
		new( "Paladin" ),
		new( "Peasant" ),
		new( "Pirate" ),
		new( "Prisoner" ),
		new( "Prophet" ),
		new( "Ranger" ),
		new( "Sage" ),
		new( "Sailor" ),
		new( "Scholar" ),
		new( "Scribe" ),
		new( "Sentry" ),
		new( "Servant" ),
		new( "Shepherd" ),
		new( "Soothsayer" ),
		new( "Stoic" ),
		new( "Storyteller" ),
		new( "Tailor" ),
		new( "Thief" ),
		new( "Tinker" ),
		new( "Town Crier" ),
		new( "Treasure Hunter" ),
		new( "Waiter", true ),
		new( "Warrior" ),
		new( "Watchman" ),
		new( "No Title", null, false )
	};

	private void RenderBackground()
	{
		AddPage(0);

		AddBackground(30, 40, 585, 410, 5054);

		AddImage(30, 40, 9251);
		AddImage(180, 40, 9251);
		AddImage(30, 40, 9253);
		AddImage(30, 130, 9253);
		AddImage(598, 40, 9255);
		AddImage(598, 130, 9255);
		AddImage(30, 433, 9257);
		AddImage(180, 433, 9257);
		AddImage(30, 40, 9250);
		AddImage(598, 40, 9252);
		AddImage(598, 433, 9258);
		AddImage(30, 433, 9256);

		AddItem(30, 40, 6816);
		AddItem(30, 125, 6817);
		AddItem(30, 233, 6817);
		AddItem(30, 341, 6817);
		AddItem(580, 40, 6814);
		AddItem(588, 125, 6815);
		AddItem(588, 233, 6815);
		AddItem(588, 341, 6815);

		AddImage(560, 20, 1417);
		AddItem(580, 44, 4033);

		AddBackground(183, 25, 280, 30, 5054);

		AddImage(180, 25, 10460);
		AddImage(434, 25, 10460);

		AddHtml(223, 32, 200, 40, "BARKEEP CUSTOMIZATION MENU", false, false);
		AddBackground(243, 433, 150, 30, 5054);

		AddImage(240, 433, 10460);
		AddImage(375, 433, 10460);

		AddImage(80, 398, 2151);
		AddItem(72, 406, 2543);

		AddHtml(110, 412, 180, 25, "sells food and drink", false, false);
	}

	private void RenderPage(IReadOnlyList<Entry> entries, int page)
	{
		AddPage(1 + page);

		AddHtml(430, 70, 180, 25, $"Page {page + 1} of {(entries.Count + 19) / 20}", false, false);

		for (int count = 0, i = page * 20; count < 20 && i < entries.Count; ++count, ++i)
		{
			Entry entry = entries[i];

			AddButton(80 + count / 10 * 260, 100 + count % 10 * 30, 4005, 4007, 2 + i, GumpButtonType.Reply, 0);
			AddHtml(120 + count / 10 * 260, 100 + count % 10 * 30, entry.MVendor ? 148 : 180, 25, entry.MDescription, true, false);

			if (entry.MVendor)
			{
				AddImage(270 + count / 10 * 260, 98 + count % 10 * 30, 2151);
				AddItem(262 + count / 10 * 260, 106 + count % 10 * 30, 2543);
			}
		}

		AddButton(340, 400, 4005, 4007, 0, GumpButtonType.Page, 1 + (page + 1) % ((entries.Count + 19) / 20));
		AddHtml(380, 400, 180, 25, "More Job Titles", false, false);

		AddButton(338, 437, 4014, 4016, 1, GumpButtonType.Reply, 0);
		AddHtml(290, 440, 35, 40, "Back", false, false);
	}

	public BarkeeperTitleGump(Mobile from, PlayerBarkeeper barkeeper) : base(0, 0)
	{
		_mFrom = from;
		_mBarkeeper = barkeeper;

		from.CloseGump(typeof(BarkeeperGump));
		from.CloseGump(typeof(BarkeeperTitleGump));

		Entry[] entries = MEntries;

		RenderBackground();

		int pageCount = (entries.Length + 19) / 20;

		for (var i = 0; i < pageCount; ++i)
			RenderPage(entries, i);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		int buttonId = info.ButtonID;

		if (buttonId <= 0) return;
		--buttonId;

		if (buttonId > 0)
		{
			--buttonId;

			if (buttonId < MEntries.Length)
				_mBarkeeper.EndChangeTitle(_mFrom, MEntries[buttonId].MTitle, MEntries[buttonId].MVendor);
		}
		else
		{
			_mBarkeeper.CancelChangeTitle(_mFrom);
		}
	}
}

public class BarkeeperGump : Gump
{
	private readonly Mobile _mFrom;
	private readonly PlayerBarkeeper _mBarkeeper;

	public void RenderBackground()
	{
		AddPage(0);

		AddBackground(30, 40, 585, 410, 5054);

		AddImage(30, 40, 9251);
		AddImage(180, 40, 9251);
		AddImage(30, 40, 9253);
		AddImage(30, 130, 9253);
		AddImage(598, 40, 9255);
		AddImage(598, 130, 9255);
		AddImage(30, 433, 9257);
		AddImage(180, 433, 9257);
		AddImage(30, 40, 9250);
		AddImage(598, 40, 9252);
		AddImage(598, 433, 9258);
		AddImage(30, 433, 9256);

		AddItem(30, 40, 6816);
		AddItem(30, 125, 6817);
		AddItem(30, 233, 6817);
		AddItem(30, 341, 6817);
		AddItem(580, 40, 6814);
		AddItem(588, 125, 6815);
		AddItem(588, 233, 6815);
		AddItem(588, 341, 6815);

		AddBackground(183, 25, 280, 30, 5054);

		AddImage(180, 25, 10460);
		AddImage(434, 25, 10460);
		AddImage(560, 20, 1417);

		AddHtml(223, 32, 200, 40, "BARKEEP CUSTOMIZATION MENU", false, false);
		AddBackground(243, 433, 150, 30, 5054);

		AddImage(240, 433, 10460);
		AddImage(375, 433, 10460);
	}

	public void RenderCategories()
	{
		AddPage(1);

		AddButton(130, 120, 4005, 4007, 0, GumpButtonType.Page, 2);
		AddHtml(170, 120, 200, 40, "Message Control", false, false);

		AddButton(130, 200, 4005, 4007, 0, GumpButtonType.Page, 8);
		AddHtml(170, 200, 200, 40, "Customize your barkeep", false, false);

		AddButton(130, 280, 4005, 4007, 0, GumpButtonType.Page, 3);
		AddHtml(170, 280, 200, 40, "Dismiss your barkeep", false, false);

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Reply, 0);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(574, 43, 5360);
	}

	public void RenderMessageManagement()
	{
		AddPage(2);

		AddButton(130, 120, 4005, 4007, 0, GumpButtonType.Page, 4);
		AddHtml(170, 120, 380, 20, "Add or change a message and keyword", false, false);

		AddButton(130, 200, 4005, 4007, 0, GumpButtonType.Page, 5);
		AddHtml(170, 200, 380, 20, "Remove a message and keyword from your barkeep", false, false);

		AddButton(130, 280, 4005, 4007, 0, GumpButtonType.Page, 6);
		AddHtml(170, 280, 380, 20, "Add or change your barkeeper's tip message", false, false);

		AddButton(130, 360, 4005, 4007, 0, GumpButtonType.Page, 7);
		AddHtml(170, 360, 380, 20, "Delete your barkeepers tip message", false, false);

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 1);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(580, 46, 4030);
	}

	public void RenderDismissConfirmation()
	{
		AddPage(3);

		AddHtml(170, 160, 380, 20, "Are you sure you want to dismiss your barkeeper?", false, false);

		AddButton(205, 280, 4005, 4007, GetButtonId(0, 0), GumpButtonType.Reply, 0);
		AddHtml(240, 280, 100, 20, @"Yes", false, false);

		AddButton(395, 280, 4005, 4007, 0, GumpButtonType.Reply, 0);
		AddHtml(430, 280, 100, 20, "No", false, false);

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 1);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(574, 43, 5360);
		AddItem(584, 34, 6579);
	}

	public void RenderMessageManagement_Message_AddOrChange()
	{
		AddPage(4);

		AddHtml(250, 60, 500, 25, "Add or change a message", false, false);

		BarkeeperRumor[] rumors = _mBarkeeper.Rumors;

		for (int i = 0; i < rumors.Length; ++i)
		{
			BarkeeperRumor rumor = rumors[i];

			AddHtml(100, 70 + i * 120, 50, 20, "Message", false, false);
			AddHtml(100, 90 + i * 120, 450, 40, rumor == null ? "No current message" : rumor.Message, true, false);
			AddHtml(100, 130 + i * 120, 50, 20, "Keyword", false, false);
			AddHtml(100, 150 + i * 120, 450, 40, rumor == null ? "None" : rumor.Keyword, true, false);

			AddButton(60, 90 + i * 120, 4005, 4007, GetButtonId(1, i), GumpButtonType.Reply, 0);
		}

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 2);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(580, 46, 4030);
	}

	public void RenderMessageManagement_Message_Remove()
	{
		AddPage(5);

		AddHtml(190, 60, 500, 25, "Choose the message you would like to remove", false, false);

		BarkeeperRumor[] rumors = _mBarkeeper.Rumors;

		for (int i = 0; i < rumors.Length; ++i)
		{
			BarkeeperRumor rumor = rumors[i];

			AddHtml(100, 70 + i * 120, 50, 20, "Message", false, false);
			AddHtml(100, 90 + i * 120, 450, 40, rumor == null ? "No current message" : rumor.Message, true, false);
			AddHtml(100, 130 + i * 120, 50, 20, "Keyword", false, false);
			AddHtml(100, 150 + i * 120, 450, 40, rumor == null ? "None" : rumor.Keyword, true, false);

			AddButton(60, 90 + i * 120, 4005, 4007, GetButtonId(2, i), GumpButtonType.Reply, 0);
		}

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 2);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(580, 46, 4030);
	}

	private static int GetButtonId(int type, int index)
	{
		return 1 + index * 6 + type;
	}

	private void RenderMessageManagement_Tip_AddOrChange()
	{
		AddPage(6);

		AddHtml(250, 95, 500, 20, "Change this tip message", false, false);
		AddHtml(100, 190, 50, 20, "Message", false, false);
		AddHtml(100, 210, 450, 40, _mBarkeeper.TipMessage ?? "No current message", true, false);

		AddButton(60, 210, 4005, 4007, GetButtonId(3, 0), GumpButtonType.Reply, 0);

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 2);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(580, 46, 4030);
	}

	private void RenderMessageManagement_Tip_Remove()
	{
		AddPage(7);

		AddHtml(250, 95, 500, 20, "Remove this tip message", false, false);
		AddHtml(100, 190, 50, 20, "Message", false, false);
		AddHtml(100, 210, 450, 40, _mBarkeeper.TipMessage ?? "No current message", true, false);

		AddButton(60, 210, 4005, 4007, GetButtonId(4, 0), GumpButtonType.Reply, 0);

		AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 2);
		AddHtml(290, 440, 35, 40, "Back", false, false);

		AddItem(580, 46, 4030);
	}

	private void RenderAppearanceCategories()
	{
		AddPage(8);

		AddButton(130, 120, 4005, 4007, GetButtonId(5, 0), GumpButtonType.Reply, 0);
		AddHtml(170, 120, 120, 20, "Title", false, false);

		if (_mBarkeeper.BodyValue != 0x340 && _mBarkeeper.BodyValue != 0x402)
		{
			AddButton(130, 200, 4005, 4007, GetButtonId(5, 1), GumpButtonType.Reply, 0);
			AddHtml(170, 200, 120, 20, "Appearance", false, false);

			AddButton(130, 280, 4005, 4007, GetButtonId(5, 2), GumpButtonType.Reply, 0);
			AddHtml(170, 280, 120, 20, "Male / Female", false, false);

			AddButton(338, 437, 4014, 4016, 0, GumpButtonType.Page, 1);
			AddHtml(290, 440, 35, 40, "Back", false, false);
		}

		AddItem(580, 44, 4033);
	}

	public BarkeeperGump(Mobile from, PlayerBarkeeper barkeeper) : base(0, 0)
	{
		_mFrom = from;
		_mBarkeeper = barkeeper;

		from.CloseGump(typeof(BarkeeperGump));
		from.CloseGump(typeof(BarkeeperTitleGump));

		RenderBackground();
		RenderCategories();
		RenderMessageManagement();
		RenderDismissConfirmation();
		RenderMessageManagement_Message_AddOrChange();
		RenderMessageManagement_Message_Remove();
		RenderMessageManagement_Tip_AddOrChange();
		RenderMessageManagement_Tip_Remove();
		RenderAppearanceCategories();
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (!_mBarkeeper.IsOwner(_mFrom))
			return;

		int index = info.ButtonID - 1;

		if (index < 0)
			return;

		int type = index % 6;
		index /= 6;

		switch (type)
		{
			case 0: // Controls
			{
				switch (index)
				{
					case 0: // Dismiss
					{
						_mBarkeeper.Dismiss();
						break;
					}
				}

				break;
			}
			case 1: // Change message
			{
				_mBarkeeper.BeginChangeRumor(_mFrom, index);
				break;
			}
			case 2: // Remove message
			{
				_mBarkeeper.RemoveRumor(_mFrom, index);
				break;
			}
			case 3: // Change tip
			{
				_mBarkeeper.BeginChangeTip(_mFrom);
				break;
			}
			case 4: // Remove tip
			{
				_mBarkeeper.RemoveTip(_mFrom);
				break;
			}
			case 5: // Appearance category selection
			{
				switch (index)
				{
					case 0: _mBarkeeper.BeginChangeTitle(_mFrom); break;
					case 1: _mBarkeeper.BeginChangeAppearance(_mFrom); break;
					case 2: _mBarkeeper.ChangeGender(_mFrom); break;
				}

				break;
			}
		}
	}
}
