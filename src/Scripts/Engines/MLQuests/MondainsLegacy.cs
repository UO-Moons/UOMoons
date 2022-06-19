using System;
using System.IO;
using System.Linq;
using System.Xml;
using Server.Commands;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Targeting;

namespace Server;

public interface ICanBeElfOrHuman
{
	bool ElfOnly { get; set; }
}

public static class MondainsLegacy
{
	public static Type[] Artifacts { get; } = new Type[]
	{
		typeof(AegisOfGrace), typeof(BladeDance), typeof(BloodwoodSpirit), typeof(Bonesmasher),
		typeof(Boomstick), typeof(BrightsightLenses), typeof(FeyLeggings), typeof(FleshRipper),
		typeof(HelmOfSwiftness), typeof(PadsOfTheCuSidhe), typeof(QuiverOfRage), typeof(QuiverOfElements),
		typeof(RaedsGlory), typeof(RighteousAnger), typeof(RobeOfTheEclipse), typeof(RobeOfTheEquinox),
		typeof(SoulSeeker), typeof(TalonBite), typeof(TotemOfVoid), typeof(WildfireBow),
		typeof(Windsong)
	};

	private static bool _mPalaceOfParoxysmus;
	private static bool _mTwistedWeald;
	private static bool _mBlightedGrove;
	private static bool _mBedlam;
	private static bool _mPrismOfLight;
	private static bool _mCitadel;
	private static bool _mPaintedCaves;
	private static bool _mLabyrinth;
	private static bool _mSanctuary;
	private static bool _mStygianDragonLair;
	private static bool _mMedusasLair;
	private static bool _mSpellweaving;
	private static bool _mPublicDonations;

	public static bool PalaceOfParoxysmus
	{
		get => _mPalaceOfParoxysmus;
		set => _mPalaceOfParoxysmus = value;
	}

	public static bool TwistedWeald
	{
		get => _mTwistedWeald;
		set => _mTwistedWeald = value;
	}

	public static bool BlightedGrove
	{
		get => _mBlightedGrove;
		set => _mBlightedGrove = value;
	}

	public static bool Bedlam
	{
		get => _mBedlam;
		set => _mBedlam = value;
	}

	public static bool PrismOfLight
	{
		get => _mPrismOfLight;
		set => _mPrismOfLight = value;
	}

	public static bool Citadel
	{
		get => _mCitadel;
		set => _mCitadel = value;
	}

	public static bool PaintedCaves
	{
		get => _mPaintedCaves;
		set => _mPaintedCaves = value;
	}

	public static bool Labyrinth
	{
		get => _mLabyrinth;
		set => _mLabyrinth = value;
	}

	public static bool Sanctuary
	{
		get => _mSanctuary;
		set => _mSanctuary = value;
	}

	public static bool StygianDragonLair
	{
		get => _mStygianDragonLair;
		set => _mStygianDragonLair = value;
	}

	public static bool MedusasLair
	{
		get => _mMedusasLair;
		set => _mMedusasLair = value;
	}

	public static bool Spellweaving
	{
		get => _mSpellweaving;
		set => _mSpellweaving = value;
	}

	public static bool PublicDonations
	{
		get => _mPublicDonations;
		set => _mPublicDonations = value;
	}

	public static void Initialize()
	{
		CommandSystem.Register("DecorateML", AccessLevel.Administrator, DecorateML_OnCommand);
		//CommandSystem.Register("DecorateMLDelete", AccessLevel.Administrator, new CommandEventHandler(DecorateMLDelete_OnCommand));
		CommandSystem.Register("SettingsML", AccessLevel.Administrator, SettingsML_OnCommand);
		CommandSystem.Register("Quests", AccessLevel.GameMaster, Quests_OnCommand);

		LoadSettings();
	}

	public static bool FindItem(int x, int y, int z, Map map, int itemId)
	{
		return FindItem(new Point3D(x, y, z), map, itemId);
	}

	public static bool FindItem(int x, int y, int z, Map map, Item test)
	{
		return FindItem(new Point3D(x, y, z), map, test.ItemId);
	}

	public static bool FindItem(Point3D p, Map map, int itemId)
	{
		IPooledEnumerable eable = map.GetItemsInRange(p);

		if (eable.Cast<Item>().Any(item => item.Z == p.Z && item.ItemId == itemId))
		{
			eable.Free();
			return true;
		}

		eable.Free();
		return false;
	}

	public static void LoadSettings()
	{
		if (!Directory.Exists("Data/Mondain's Legacy"))
			Directory.CreateDirectory("Data/Mondain's Legacy");

		if (!File.Exists("Data/Mondain's Legacy/Settings.xml"))
			File.Create("Data/Mondain's Legacy/Settings.xml");

		try
		{
			XmlDocument doc = new();
			doc.Load(Path.Combine(Core.BaseDirectory, "Data/Mondain's Legacy/Settings.xml"));

			XmlElement root = doc["Settings"];

			if (root == null)
				return;

			ReadNode(root, "PalaceOfParoxysmus", ref _mPalaceOfParoxysmus);
			ReadNode(root, "TwistedWeald", ref _mTwistedWeald);
			ReadNode(root, "BlightedGrove", ref _mBlightedGrove);
			ReadNode(root, "Bedlam", ref _mBedlam);
			ReadNode(root, "PrismOfLight", ref _mPrismOfLight);
			ReadNode(root, "Citadel", ref _mCitadel);
			ReadNode(root, "PaintedCaves", ref _mPaintedCaves);
			ReadNode(root, "Labyrinth", ref _mLabyrinth);
			ReadNode(root, "Sanctuary", ref _mSanctuary);
			ReadNode(root, "StygianDragonLair", ref _mStygianDragonLair);
			ReadNode(root, "MedusasLair", ref _mMedusasLair);
			ReadNode(root, "Spellweaving", ref _mSpellweaving);
			ReadNode(root, "PublicDonations", ref _mPublicDonations);
		}
		catch
		{
			// ignored
		}

		if (!Core.ML) return;
		if (!FindItem(new Point3D(1431, 1696, 0), Map.Trammel, 0x307F))
		{
			var addon = new ArcaneCircleAddon();
			addon.MoveToWorld(new Point3D(1431, 1696, 0), Map.Trammel);
		}

		if (!FindItem(new Point3D(1431, 1696, 0), Map.Felucca, 0x307F))
		{
			var addon = new ArcaneCircleAddon();
			addon.MoveToWorld(new Point3D(1431, 1696, 0), Map.Felucca);
		}
	}

	public static void SaveSetings()
	{
		if (!Directory.Exists("Data/Mondain's Legacy"))
			Directory.CreateDirectory("Data/Mondain's Legacy");

		if (!File.Exists("Data/Mondain's Legacy/Settings.xml"))
			File.Create("Data/Mondain's Legacy/Settings.xml");

		try
		{
			XmlDocument doc = new();
			doc.Load(Path.Combine(Core.BaseDirectory, "Data/Mondain's Legacy/Settings.xml"));

			XmlElement root = doc["Settings"];

			if (root == null)
				return;

			UpdateNode(root, "PalaceOfParoxysmus", _mPalaceOfParoxysmus);
			UpdateNode(root, "TwistedWeald", _mTwistedWeald);
			UpdateNode(root, "BlightedGrove", _mBlightedGrove);
			UpdateNode(root, "Bedlam", _mBedlam);
			UpdateNode(root, "PrismOfLight", _mPrismOfLight);
			UpdateNode(root, "Citadel", _mCitadel);
			UpdateNode(root, "PaintedCaves", _mPaintedCaves);
			UpdateNode(root, "Labyrinth", _mLabyrinth);
			UpdateNode(root, "Sanctuary", _mSanctuary);
			UpdateNode(root, "StygianDragonLair", _mStygianDragonLair);
			UpdateNode(root, "MedusasLair", _mMedusasLair);
			UpdateNode(root, "Spellweaving", _mSpellweaving);
			UpdateNode(root, "PublicDonations", _mPublicDonations);

			doc.Save("Data/Mondain's Legacy/Settings.xml");
		}
		catch (Exception e)
		{
			Console.WriteLine("Error while updating 'Settings.xml': {0}", e);
		}
	}

	public static void ReadNode(XmlElement root, string dungeon, ref bool val)
	{
		if (root == null)
			return;

		foreach (XmlElement element in root.SelectNodes(dungeon)!)
		{
			if (element.HasAttribute("active"))
				val = XmlConvert.ToBoolean(element.GetAttribute("active"));
		}
	}

	public static void UpdateNode(XmlElement root, string dungeon, bool val)
	{
		if (root == null)
			return;

		foreach (XmlElement element in root.SelectNodes(dungeon)!)
		{
			if (element.HasAttribute("active"))
				element.SetAttribute("active", XmlConvert.ToString(val));
		}
	}

	public static bool CheckArtifactChance(Mobile m, BaseCreature bc)
	{
		if (!Core.ML /*|| bc is BasePeerless*/) // Peerless drops to the corpse, this is handled elsewhere
			return false;

		return Paragon.CheckArtifactChance(m, bc);
	}

	public static void GiveArtifactTo(Mobile m)
	{
		if (Activator.CreateInstance(Artifacts[Utility.Random(Artifacts.Length)]) is not Item item)
			return;

		m.PlaySound(0x5B4);

		if (m.AddToBackpack(item))
		{
			m.SendLocalizedMessage(1072223); // An item has been placed in your backpack.
			m.SendLocalizedMessage(
				1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
		}
		else if (m.BankBox.TryDropItem(m, item, false))
		{
			m.SendLocalizedMessage(1072224); // An item has been placed in your bank box.
			m.SendLocalizedMessage(
				1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
		}
		else
		{
			// Item was placed at feet by m.AddToBackpack
			m.SendLocalizedMessage(
				1072523); // You find an artifact, but your backpack and bank are too full to hold it.
		}
	}

	public static void DropPeerlessMinor(Container peerlessCorpse)
	{
		Item item = Activator.CreateInstance(Artifacts[Utility.Random(Artifacts.Length)]) as Item;

		if (item is ICanBeElfOrHuman human)
			human.ElfOnly = false;

		peerlessCorpse.DropItem(item);
	}

	public static bool CheckMl(Mobile from)
	{
		return CheckMl(from, true);
	}

	public static bool CheckMl(Mobile from, bool message)
	{
		if (from == null || from.NetState == null)
			return false;

		if (from.NetState.SupportsExpansion(Expansion.ML))
			return true;

		if (message)
			from.SendLocalizedMessage(1072791); // You must upgrade to Mondain's Legacy in order to use that item.

		return false;
	}

	public static bool IsMlRegion(Region region)
	{
		return region.IsPartOf("Twisted Weald") ||
		       region.IsPartOf("Sanctuary") ||
		       region.IsPartOf("Prism of Light") ||
		       region.IsPartOf("TheCitadel") ||
		       region.IsPartOf("Bedlam") ||
		       region.IsPartOf("Blighted Grove") ||
		       region.IsPartOf("Painted Caves") ||
		       region.IsPartOf("Palace of Paroxysmus") ||
		       region.IsPartOf("Labyrinth");
	}

	//[Usage("DecorateMLDelete")]
	//[Description("Deletes Mondain's Legacy world decoration.")]
	//private static void DecorateMLDelete_OnCommand(CommandEventArgs e)
	//{
	//    //WeakEntityCollection.Delete("ml");
	//}

	[Usage("DecorateML")]
	[Description("Generates Mondain's Legacy world decoration.")]
	private static void DecorateML_OnCommand(CommandEventArgs e)
	{
		e.Mobile.SendMessage("Generating Mondain's Legacy world decoration, please wait.");

		Decorate.Generate("Data/Mondain's Legacy/Trammel", Map.Trammel);
		Decorate.Generate("Data/Mondain's Legacy/Felucca", Map.Felucca);
		Decorate.Generate("Data/Mondain's Legacy/Ilshenar", Map.Ilshenar);
		Decorate.Generate("Data/Mondain's Legacy/Malas", Map.Malas);
		Decorate.Generate("Data/Mondain's Legacy/Tokuno", Map.Tokuno);
		Decorate.Generate("Data/Mondain's Legacy/TerMur", Map.TerMur);

		PeerlessTeleporter tele;
		PrismOfLightPillar pillar;
		ParoxysmusIronGate gate;

		// Bedlam - Malas
		PeerlessAltar altar = new BedlamAltar();

		if (!FindItem(86, 1627, 0, Map.Malas, altar))
		{
			altar.MoveToWorld(new Point3D(86, 1627, 0), Map.Malas);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(99, 1617, 50), Map.Malas);
		}

		// Blighted Grove - Trammel
		altar = new BlightedGroveAltar();

		if (!FindItem(6502, 875, 0, Map.Trammel, altar))
		{
			altar.MoveToWorld(new Point3D(6502, 875, 0), Map.Trammel);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(6511, 949, 26), Map.Trammel);
		}

		// Blighted Grove - Felucca
		altar = new BlightedGroveAltar();

		if (!FindItem(6502, 875, 0, Map.Felucca, altar))
		{
			altar.MoveToWorld(new Point3D(6502, 875, 0), Map.Felucca);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(6511, 949, 26), Map.Felucca);
		}

		// Palace of Paroxysmus - Trammel
		altar = new ParoxysmusAltar();

		if (!FindItem(6511, 506, -34, Map.Trammel, altar))
		{
			altar.MoveToWorld(new Point3D(6511, 506, -34), Map.Trammel);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(6518, 365, 46), Map.Trammel);
			gate = new ParoxysmusIronGate(altar);
			gate.MoveToWorld(new Point3D(6518, 492, -50), Map.Trammel);
		}

		// Palace of Paroxysmus - Felucca
		altar = new ParoxysmusAltar();

		if (!FindItem(6511, 506, -34, Map.Felucca, altar))
		{
			altar.MoveToWorld(new Point3D(6511, 506, -34), Map.Felucca);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(6518, 365, 46), Map.Felucca);
			gate = new ParoxysmusIronGate(altar);
			gate.MoveToWorld(new Point3D(6518, 492, -50), Map.Felucca);
		}

		// Prism of Light - Trammel
		altar = new PrismOfLightAltar();

		if (!FindItem(6509, 167, 6, Map.Trammel, altar))
		{
			altar.MoveToWorld(new Point3D(6509, 167, 6), Map.Trammel);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest,
				Visible = true,
				ItemId = 0xDDA
			};
			tele.MoveToWorld(new Point3D(6501, 137, -20), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6506, 167, 0), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6509, 164, 0), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6506, 164, 0), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6512, 167, 0), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6509, 170, 0), Map.Trammel);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6512, 170, 0), Map.Trammel);
		}

		// Prism of Light - Felucca
		altar = new PrismOfLightAltar();

		if (!FindItem(6509, 167, 6, Map.Felucca, altar))
		{
			altar.MoveToWorld(new Point3D(6509, 167, 6), Map.Felucca);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest,
				Visible = true,
				ItemId = 0xDDA
			};
			tele.MoveToWorld(new Point3D(6501, 137, -20), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6506, 167, 0), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6509, 164, 0), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x581);
			pillar.MoveToWorld(new Point3D(6506, 164, 0), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6512, 167, 0), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6509, 170, 0), Map.Felucca);

			pillar = new PrismOfLightPillar((PrismOfLightAltar) altar, 0x481);
			pillar.MoveToWorld(new Point3D(6512, 170, 0), Map.Felucca);
		}

		// The Citadel - Malas
		altar = new CitadelAltar();

		if (!FindItem(89, 1885, 0, Map.Malas, altar))
		{
			altar.MoveToWorld(new Point3D(89, 1885, 0), Map.Malas);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(111, 1955, 0), Map.Malas);
		}

		// Twisted Weald - Ilshenar
		altar = new TwistedWealdAltar();

		if (!FindItem(2170, 1255, -60, Map.Ilshenar, altar))
		{

			altar.MoveToWorld(new Point3D(2170, 1255, -60), Map.Ilshenar);
			tele = new PeerlessTeleporter(altar)
			{
				PointDest = altar.ExitDest
			};
			tele.MoveToWorld(new Point3D(2139, 1271, -57), Map.Ilshenar);
		}

		// Stygian Dragon Lair - Abyss
		/*StygianDragonPlatform sAltar = new StygianDragonPlatform();
   
		if (!FindItem(363, 157, 5, Map.TerMur, sAltar))
		{
		    WeakEntityCollection.Add("ml", sAltar);
		    sAltar.MoveToWorld(new Point3D(363, 157, 0), Map.TerMur);
   
		}
   
		//Medusa Lair - Abyss
		MedusaPlatform mAltar = new MedusaPlatform();
   
		if (!FindItem(822, 756, 56, Map.TerMur, mAltar))
		{
		    WeakEntityCollection.Add("ml", sAltar);
		    mAltar.MoveToWorld(new Point3D(822, 756, 56), Map.TerMur);
		}*/

		e.Mobile.SendMessage("Mondain's Legacy world generating complete.");
	}

	[Usage("SettingsML")]
	[Description("Mondain's Legacy Settings.")]
	private static void SettingsML_OnCommand(CommandEventArgs e)
	{
		e.Mobile.SendGump(new MondainsLegacyGump());
	}

	[Usage("Quests")]
	[Description("Pops up a quest list from targeted player.")]
	private static void Quests_OnCommand(CommandEventArgs e)
	{
		Mobile m = e.Mobile;
		m.SendMessage("Target a player to view their quests.");

		m.BeginTarget(-1, false, TargetFlags.None, delegate(Mobile from, object targeted)
		{
			if (targeted is PlayerMobile mobile)
				m.SendGump(new MondainQuestGump(mobile));
			else
				m.SendMessage("That is not a player!");
		});
	}
}

public class MondainsLegacyGump : Gump
{
	public MondainsLegacyGump()
		: base(50, 50)
	{
		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);
		AddBackground(0, 0, 308, 390, 0x2454);

		// title
		AddLabel(125, 10, 150, "Settings");
		AddImage(256, 5, 0x9E1);

		// dungeons			
		AddButton(20, 60, MondainsLegacy.PalaceOfParoxysmus ? 0x939 : 0x938,
			MondainsLegacy.PalaceOfParoxysmus ? 0x939 : 0x938, 1, GumpButtonType.Reply, 0);
		AddButton(20, 85, MondainsLegacy.TwistedWeald ? 0x939 : 0x938, MondainsLegacy.TwistedWeald ? 0x939 : 0x938, 2,
			GumpButtonType.Reply, 0);
		AddButton(20, 110, MondainsLegacy.BlightedGrove ? 0x939 : 0x938, MondainsLegacy.BlightedGrove ? 0x939 : 0x938,
			3, GumpButtonType.Reply, 0);
		AddButton(20, 135, MondainsLegacy.Bedlam ? 0x939 : 0x938, MondainsLegacy.Bedlam ? 0x939 : 0x938, 4,
			GumpButtonType.Reply, 0);
		AddButton(20, 160, MondainsLegacy.PrismOfLight ? 0x939 : 0x938, MondainsLegacy.PrismOfLight ? 0x939 : 0x938, 5,
			GumpButtonType.Reply, 0);
		AddButton(20, 185, MondainsLegacy.Citadel ? 0x939 : 0x938, MondainsLegacy.Citadel ? 0x939 : 0x938, 6,
			GumpButtonType.Reply, 0);
		AddButton(20, 210, MondainsLegacy.PaintedCaves ? 0x939 : 0x938, MondainsLegacy.PaintedCaves ? 0x939 : 0x938, 7,
			GumpButtonType.Reply, 0);
		AddButton(20, 235, MondainsLegacy.Labyrinth ? 0x939 : 0x938, MondainsLegacy.Labyrinth ? 0x939 : 0x938, 8,
			GumpButtonType.Reply, 0);
		AddButton(20, 260, MondainsLegacy.Sanctuary ? 0x939 : 0x938, MondainsLegacy.Sanctuary ? 0x939 : 0x938, 9,
			GumpButtonType.Reply, 0);
		AddButton(20, 285, MondainsLegacy.StygianDragonLair ? 0x939 : 0x938,
			MondainsLegacy.StygianDragonLair ? 0x939 : 0x938, 10, GumpButtonType.Reply, 0);
		AddButton(20, 310, MondainsLegacy.MedusasLair ? 0x939 : 0x938, MondainsLegacy.MedusasLair ? 0x939 : 0x938, 11,
			GumpButtonType.Reply, 0);
		AddButton(20, 335, MondainsLegacy.Spellweaving ? 0x939 : 0x938, MondainsLegacy.Spellweaving ? 0x939 : 0x938, 12,
			GumpButtonType.Reply, 0);
		AddButton(20, 360, MondainsLegacy.PublicDonations ? 0x939 : 0x938,
			MondainsLegacy.PublicDonations ? 0x939 : 0x938, 13, GumpButtonType.Reply, 0);

		AddLabel(45, 56, 0x226, "Palace of Paroxysmus");
		AddLabel(45, 81, 0x226, "Twisted Weald");
		AddLabel(45, 106, 0x226, "Blighted Grove");
		AddLabel(45, 131, 0x226, "Bedlam");
		AddLabel(45, 156, 0x226, "Prism of Light");
		AddLabel(45, 181, 0x226, "The Citadel");
		AddLabel(45, 206, 0x226, "Painted Caves");
		AddLabel(45, 231, 0x226, "Labyrinth");
		AddLabel(45, 256, 0x226, "Sanctuary");
		AddLabel(45, 281, 0x226, "StygianDragonLair");
		AddLabel(45, 306, 0x226, "MedusasLair");
		AddLabel(45, 331, 0x226, "Spellweaving");
		AddLabel(45, 356, 0x226, "PublicDonations");

		// legend
		AddLabel(243, 205, 0x226, "Legend:");

		AddImage(218, 235, 0x938);
		AddLabel(243, 231, 0x226, "disabled");
		AddImage(218, 260, 0x939);
		AddLabel(243, 256, 0x226, "enabled");
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case 0:
				MondainsLegacy.SaveSetings();
				break;
			case 1:
				MondainsLegacy.PalaceOfParoxysmus = !MondainsLegacy.PalaceOfParoxysmus;
				break;
			case 2:
				MondainsLegacy.TwistedWeald = !MondainsLegacy.TwistedWeald;
				break;
			case 3:
				MondainsLegacy.BlightedGrove = !MondainsLegacy.BlightedGrove;
				break;
			case 4:
				MondainsLegacy.Bedlam = !MondainsLegacy.Bedlam;
				break;
			case 5:
				MondainsLegacy.PrismOfLight = !MondainsLegacy.PrismOfLight;
				break;
			case 6:
				MondainsLegacy.Citadel = !MondainsLegacy.Citadel;
				break;
			case 7:
				MondainsLegacy.PaintedCaves = !MondainsLegacy.PaintedCaves;
				break;
			case 8:
				MondainsLegacy.Labyrinth = !MondainsLegacy.Labyrinth;
				break;
			case 9:
				MondainsLegacy.Sanctuary = !MondainsLegacy.Sanctuary;
				break;
			case 10:
				MondainsLegacy.StygianDragonLair = !MondainsLegacy.StygianDragonLair;
				break;
			case 11:
				MondainsLegacy.MedusasLair = !MondainsLegacy.MedusasLair;
				break;
			case 12:
				MondainsLegacy.Spellweaving = !MondainsLegacy.Spellweaving;
				break;
			case 13:
				MondainsLegacy.PublicDonations = !MondainsLegacy.PublicDonations;
				break;
		}

		if (info.ButtonID > 0)
			sender.Mobile.SendGump(new MondainsLegacyGump());
	}
}
