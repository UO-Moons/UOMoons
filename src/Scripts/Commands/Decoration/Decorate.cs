using Server.Engines.Quests.Haven;
using Server.Engines.Quests.Necro;
using Server.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Server.Engines.Champions;
using Server.Engines.Doom;
using Server.Engines.Exodus;
using Server.Engines.MiniChamps;
using Server.Engines.NewMagincia;
using Server.Engines.TombOfKings;
using Server.Factions;
using Server.Mobiles;

namespace Server.Commands;

public class Decorate
{
	private static readonly bool KhaldunEnabled = Settings.Configuration.Get<bool>("Dungeons", "KhaldunDungeon");
	private static readonly bool ExodusEnabled = Settings.Configuration.Get<bool>("Dungeons", "ExodusDungeon");
	private static readonly bool NavreysEnabled = Settings.Configuration.Get<bool>("Dungeons", "Navreys");
	private static readonly bool ExperimentalEnabled = Settings.Configuration.Get<bool>("Dungeons", "ExperimentalRoom");
	private static readonly bool PuzzleEnabled = Settings.Configuration.Get<bool>("Dungeons", "PuzzleRoom");
	private static readonly bool MazeEnabled = Settings.Configuration.Get<bool>("Dungeons", "MazeRoom");
	private static readonly bool StealableEnabled = Settings.Configuration.Get<bool>("Dungeons", "StealableArtifacts");
	private static readonly bool ToKEnabled = Settings.Configuration.Get<bool>("Dungeons", "TombOfKing");
	private static readonly bool LevelPuzzleEnabled = Settings.Configuration.Get<bool>("Dungeons", "LevelPuzzle");
	private static readonly bool GauntletEnabled = Settings.Configuration.Get<bool>("Dungeons", "GauntletPuzzle");
	private static readonly bool ArisenEnabled = Settings.Configuration.Get<bool>("Dungeons", "Arisen");
	private static readonly bool SecretContEnabled = Settings.Configuration.Get<bool>("Dungeons", "SecretCont");
	private static readonly bool MaginciaEnabled = Settings.Configuration.Get<bool>("World", "RuinedMagincia");
	private static readonly bool FactionsEnabled = Settings.Configuration.Get<bool>("World", "Factions");

	public static void Initialize()
	{
		CommandSystem.Register("Decorate", AccessLevel.Administrator, Decorate_OnCommand);
		CommandSystem.Register("TelGen", AccessLevel.Administrator, GenTeleporter.GenTeleporter_OnCommand);
		CommandSystem.Register("SignGen", AccessLevel.Administrator, SignParser.SignGen_OnCommand);
		CommandSystem.Register("RemoveStealArties", AccessLevel.Administrator, RemoveStealArties_OnCommand);
		CommandSystem.Register("ArisenDelete", AccessLevel.Administrator, ArisenDelete_OnCommand);
		CommandSystem.Register("ChampionInfo", AccessLevel.GameMaster, ChampionInfo_OnCommand);
		CommandSystem.Register("DelChampSpawns", AccessLevel.GameMaster, DelSpawns_OnCommand);
		CommandSystem.Register("GenChampSpawns", AccessLevel.GameMaster, ChampionSystem.GenSpawns_OnCommand);
		CommandSystem.Register("GenMiniChamp", AccessLevel.Administrator, MiniChamp.GenStoneRuins_OnCommand);
		CommandSystem.Register("ViewLottos", AccessLevel.GameMaster, NewMaginciaCommand.ViewLottos_OnCommand);
		CommandSystem.Register("GenNewMagincia", AccessLevel.GameMaster, NewMaginciaCommand.GenNewMagincia_OnCommand);
	}

	[Usage("Decorate")]
	[Description("Generates World Decoration.")]
	private static void Decorate_OnCommand(CommandEventArgs e)
	{
		_mobile = e.Mobile;
		_count = 0;

		_mobile.SendMessage("Generating World Decoration, please wait.");

		Generate("Data/Decoration/Britannia", Map.Trammel, Map.Felucca);
		Generate("Data/Decoration/Trammel", Map.Trammel);
		Generate("Data/Decoration/Felucca", Map.Felucca);
		Generate("Data/Decoration/Ilshenar", Map.Ilshenar);
		Generate("Data/Decoration/Malas", Map.Malas);
		Generate("Data/Decoration/Tokuno", Map.Tokuno);

		if (MaginciaEnabled)
		{
			Generate("Data/Decoration/RuinedMaginciaTram", Map.Trammel);
			Generate("Data/Decoration/RuinedMaginciaFel", Map.Felucca);
		}

		//SA Expansion Decorations
		Generate("Data/Decoration/Stygian Abyss/Ter Mur", Map.TerMur);
		Generate("Data/Decoration/Stygian Abyss/Trammel", Map.Trammel);
		Generate("Data/Decoration/Stygian Abyss/Felucca", Map.Felucca);

		if (FactionsEnabled)
		{
			_ = new FactionPersistance();

			List<Faction> factions = Faction.Factions;

			foreach (Faction faction in factions)
				Generator.Generate(faction);

			List<Town> towns = Town.Towns;

			foreach (Town town in towns)
				Generator.Generate(town);
		}

		if (KhaldunEnabled)
		{
			// Generate Morph Items
			CreateMorphItem(5459, 1416, 0, 0x1D0, 0x1, 1);
			CreateMorphItem(5460, 1416, 0, 0x1D0, 0x1, 1);
			CreateMorphItem(5459, 1416, 0, 0x1, 0x53D, 1);
			CreateMorphItem(5460, 1416, 0, 0x1, 0x53B, 1);

			CreateMorphItem(5459, 1425, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5459, 1426, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5459, 1427, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5460, 1425, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5460, 1426, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5460, 1427, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5461, 1427, 0, 0x1, 0x53B, 2);
			CreateMorphItem(5460, 1422, 0, 0x1, 0x544, 2);
			CreateMorphItem(5460, 1419, 0, 0x1, 0x545, 2);
			CreateMorphItem(5460, 1420, 0, 0x1, 0x545, 2);
			CreateMorphItem(5460, 1423, 0, 0x1, 0x545, 2);
			CreateMorphItem(5460, 1424, 0, 0x1, 0x545, 2);
			CreateMorphItem(5461, 1426, 0, 0x1, 0x545, 2);
			CreateMorphItem(5460, 1417, 0, 0x1, 0x546, 1);
			CreateMorphItem(5460, 1418, 0, 0x1, 0x546, 2);
			CreateMorphItem(5460, 1421, 0, 0x1, 0x546, 2);
			CreateMorphItem(5461, 1425, 0, 0x1, 0x548, 2);
			CreateMorphItem(5459, 1420, 0, 0x1, 0x54A, 2);
			CreateMorphItem(5459, 1421, 0, 0x1, 0x54A, 2);
			CreateMorphItem(5459, 1423, 0, 0x1, 0x54A, 2);
			CreateMorphItem(5459, 1418, 0, 0x1, 0x54B, 2);
			CreateMorphItem(5459, 1422, 0, 0x1, 0x54B, 2);
			CreateMorphItem(5459, 1417, 0, 0x1, 0x54C, 1);
			CreateMorphItem(5459, 1419, 0, 0x1, 0x54C, 2);
			CreateMorphItem(5459, 1424, 0, 0x1, 0x54C, 2);

			CreateMorphItem(5458, 1426, 0, 0x1, 0x1D1, 2);
			CreateMorphItem(5459, 1427, 0, 0x1, 0x1E3, 2);
			CreateMorphItem(5458, 1425, 3, 0x1, 0x1E4, 2);
			CreateMorphItem(5458, 1427, 6, 0x1, 0x1E5, 2);
			CreateMorphItem(5461, 1427, 0, 0x1, 0x1E8, 2);
			CreateMorphItem(5460, 1427, 0, 0x1, 0x1E9, 2);
			CreateMorphItem(5458, 1425, 0, 0x1, 0x1EA, 2);
			CreateMorphItem(5458, 1427, 0, 0x1, 0x1EA, 2);
			CreateMorphItem(5458, 1427, 3, 0x1, 0x1EA, 2);

			// Generate Approach Lights
			CreateApproachLight(5393, 1417, 0, 0x1857, 0x1858, LightType.Circle150);
			CreateApproachLight(5393, 1420, 0, 0x1857, 0x1858, LightType.Circle150);
			CreateApproachLight(5395, 1421, 0, 0x1857, 0x1858, LightType.Circle150);
			CreateApproachLight(5396, 1417, 0, 0x1857, 0x1858, LightType.Circle150);
			CreateApproachLight(5397, 1419, 0, 0x1857, 0x1858, LightType.Circle150);

			CreateApproachLight(5441, 1393, 5, 0x1F2B, 0x19BB, LightType.Circle225);
			CreateApproachLight(5446, 1393, 5, 0x1F2B, 0x19BB, LightType.Circle225);

			// Generate Sound Effects
			CreateSoundEffect(5425, 1489, 5, 0x102, 1);
			CreateSoundEffect(5425, 1491, 5, 0x102, 1);

			CreateSoundEffect(5449, 1499, 10, 0xF5, 1);
			CreateSoundEffect(5451, 1499, 10, 0xF5, 1);
			CreateSoundEffect(5453, 1499, 10, 0xF5, 1);

			CreateSoundEffect(5524, 1367, 0, 0x102, 1);

			CreateSoundEffect(5450, 1370, 0, 0x220, 2);
			CreateSoundEffect(5450, 1372, 0, 0x220, 2);

			CreateSoundEffect(5460, 1416, 0, 0x244, 2);

			CreateSoundEffect(5483, 1439, 5, 0x14, 3);

			// Generate Big Teleporter
			CreateBigTeleporterItem(5387, 1325, true);
			CreateBigTeleporterItem(5388, 1326, true);
			CreateBigTeleporterItem(5388, 1325, false);
			CreateBigTeleporterItem(5387, 1326, false);

			// Generate Central Khaldun entrance

			RaisableItem stone = TryCreateItem(5403, 1360, 0, new RaisableItem(0x788, 10, 0x477, 0x475, TimeSpan.FromMinutes(1.5))) as RaisableItem;
			RaisableItem door = TryCreateItem(5524, 1367, 0, new RaisableItem(0x1D0, 20, 0x477, 0x475, TimeSpan.FromMinutes(5.0))) as RaisableItem;

			if (TryCreateItem(5459, 1426, 10, new DisappearingRaiseSwitch()) is DisappearingRaiseSwitch sw) sw.RaisableItem = stone;
			if (TryCreateItem(5403, 1359, 0, new RaiseSwitch()) is RaiseSwitch lv) lv.RaisableItem = door;
		}

		if (StealableEnabled)
		{
			_mobile.SendMessage(StealableArtifactsSpawner.Create()
				? "Stealable artifacts spawner generated."
				: "Stealable artifacts spawner already present.");
		}

		if (SecretContEnabled)
		{
			MarkContainer.CreateMalasPassage(951, 546, -70, 1006, 994, -70, false, false);
			MarkContainer.CreateMalasPassage(914, 192, -79, 1019, 1062, -70, false, false);
			MarkContainer.CreateMalasPassage(1614, 143, -90, 1214, 1313, -90, false, false);
			MarkContainer.CreateMalasPassage(2176, 324, -90, 1554, 172, -90, false, false);
			MarkContainer.CreateMalasPassage(864, 812, -90, 1061, 1161, -70, false, false);
			MarkContainer.CreateMalasPassage(1051, 1434, -85, 1076, 1244, -70, false, true);
			MarkContainer.CreateMalasPassage(1326, 523, -87, 1201, 1554, -70, false, false);
			MarkContainer.CreateMalasPassage(424, 189, -1, 2333, 1501, -90, true, false);
			MarkContainer.CreateMalasPassage(1313, 1115, -85, 1183, 462, -45, false, false);
		}

		if (GauntletEnabled)
		{
			/* Begin healer room */
			GauntletSpawner.CreatePricedHealer(5000, 387, 400);
			GauntletSpawner.CreateTeleporter(390, 407, 394, 405);

			BaseDoor healerDoor = GauntletSpawner.CreateDoorSet(393, 404, true, 0x44E);

			healerDoor.Locked = true;
			healerDoor.KeyValue = Key.RandomValue();

			if (healerDoor.Link != null)
			{
				healerDoor.Link.Locked = true;
				healerDoor.Link.KeyValue = Key.RandomValue();
			}
			/* End healer room */

			/* Begin supply room */
			GauntletSpawner.CreateMorphItem(433, 371, 0x29F, 0x116, 3, 0x44E);
			GauntletSpawner.CreateMorphItem(433, 372, 0x29F, 0x115, 3, 0x44E);

			GauntletSpawner.CreateVarietyDealer(492, 369);

			for (int x = 434; x <= 478; ++x)
			{
				for (int y = 371; y <= 372; ++y)
				{
					Static item = new(0x524)
					{
						Hue = 1
					};
					item.MoveToWorld(new Point3D(x, y, -1), Map.Malas);
				}
			}
			/* End supply room */

			/* Begin gauntlet cycle */
			GauntletSpawner.CreateTeleporter(471, 428, 474, 428);
			GauntletSpawner.CreateTeleporter(462, 494, 462, 498);
			GauntletSpawner.CreateTeleporter(403, 502, 399, 506);
			GauntletSpawner.CreateTeleporter(357, 476, 356, 480);
			GauntletSpawner.CreateTeleporter(361, 433, 357, 434);

			GauntletSpawner sp1 = GauntletSpawner.CreateSpawner("DarknightCreeper", 491, 456, 473, 432, 417, 426, true, 473, 412, 39, 60);
			GauntletSpawner sp2 = GauntletSpawner.CreateSpawner("FleshRenderer", 482, 520, 468, 496, 426, 422, false, 448, 496, 56, 48);
			GauntletSpawner sp3 = GauntletSpawner.CreateSpawner("Impaler", 406, 538, 408, 504, 432, 430, false, 376, 504, 64, 48);
			GauntletSpawner sp4 = GauntletSpawner.CreateSpawner("ShadowKnight", 335, 512, 360, 478, 424, 439, false, 300, 478, 72, 64);
			GauntletSpawner sp5 = GauntletSpawner.CreateSpawner("AbysmalHorror", 326, 433, 360, 429, 416, 435, true, 300, 408, 60, 56);
			GauntletSpawner sp6 = GauntletSpawner.CreateSpawner("DemonKnight", 423, 430, 0, 0, 423, 430, true, 392, 392, 72, 96);

			sp1.Sequence = sp2;
			sp2.Sequence = sp3;
			sp3.Sequence = sp4;
			sp4.Sequence = sp5;
			sp5.Sequence = sp6;
			sp6.Sequence = sp1;

			sp1.State = GauntletSpawnerState.InProgress;
			/* End gauntlet cycle */

			/* Begin exit gate */
			ConfirmationMoongate gate = new()
			{
				Dispellable = false,

				Target = new Point3D(2350, 1270, -85),
				TargetMap = Map.Malas,

				GumpWidth = 420,
				GumpHeight = 280,

				MessageColor = 0x7F00,
				MessageNumber = 1062109, // You are about to exit Dungeon Doom.  Do you wish to continue?

				TitleColor = 0x7800,
				TitleNumber = 1062108, // Please verify...

				Hue = 0x44E
			};
			gate.MoveToWorld(new Point3D(433, 326, 4), Map.Malas);
			/* End exit gate */
		}

		if (LevelPuzzleEnabled)
		{
			if (Map.Malas.GetItemsInRange(LeverPuzzleController.LpCenter, 0).OfType<LeverPuzzleController>().Any())
			{
				e.Mobile.SendMessage("Lamp room puzzle already exists: please delete the existing controller first ...");
				return;
			}

			new LeverPuzzleController().MoveToWorld(LeverPuzzleController.LpCenter, Map.Malas);

			e.Mobile.SendMessage(!LeverPuzzleController._installed
				? "There was a problem generating the puzzle."
				: "Lamp room puzzle successfully generated.");
		}

		if (ArisenEnabled)
		{
			e.Mobile.SendMessage(ArisenController.Create()
				? "Arisen creatures spawner generated."
				: "Arisen creatures spawner already present.");
		}

		// Exodus Dungeon
		if (ExodusEnabled)
		{
			Generate("Data/Decoration/Exodus", Map.Ilshenar);

			if (VerLorRegController.IlshenarInstance == null)
			{
				VerLorRegController.IlshenarInstance = new VerLorRegController();
				VerLorRegController.IlshenarInstance.MoveToWorld(new Point3D(849, 648, -40), Map.Ilshenar);
			}
		}

		if (NavreysEnabled)
		{
			//Navreys Dungeon
			NavreysController.GenNavery(e.Mobile);
		}

		if (ExperimentalEnabled)
		{
			ExperimentalRoomController controller = new();
			controller.MoveToWorld(new Point3D(980, 1117, -42), Map.TerMur);

			//Room 0 to 1
			ExperimentalRoomDoor door = new(Room.RoomZero, DoorFacing.WestCcw);
			ExperimentalRoomBlocker blocker = new(Room.RoomZero);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(984, 1116, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(984, 1116, -42), Map.TerMur);

			door = new ExperimentalRoomDoor(Room.RoomZero, DoorFacing.EastCw);
			blocker = new ExperimentalRoomBlocker(Room.RoomZero);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(985, 1116, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(985, 1116, -42), Map.TerMur);

			//Room 1 to 2
			door = new ExperimentalRoomDoor(Room.RoomOne, DoorFacing.WestCcw);
			blocker = new ExperimentalRoomBlocker(Room.RoomOne);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(984, 1102, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(984, 1102, -42), Map.TerMur);

			door = new ExperimentalRoomDoor(Room.RoomOne, DoorFacing.EastCw);
			blocker = new ExperimentalRoomBlocker(Room.RoomOne);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(985, 1102, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(985, 1102, -42), Map.TerMur);

			//Room 2 to 3
			door = new ExperimentalRoomDoor(Room.RoomTwo, DoorFacing.WestCcw);
			blocker = new ExperimentalRoomBlocker(Room.RoomTwo);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(984, 1090, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(984, 1090, -42), Map.TerMur);

			door = new ExperimentalRoomDoor(Room.RoomTwo, DoorFacing.EastCw);
			blocker = new ExperimentalRoomBlocker(Room.RoomTwo);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(985, 1090, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(985, 1090, -42), Map.TerMur);

			//Room 3 to 4
			door = new ExperimentalRoomDoor(Room.RoomTwo, DoorFacing.WestCcw);
			blocker = new ExperimentalRoomBlocker(Room.RoomThree);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(984, 1072, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(984, 1072, -42), Map.TerMur);

			door = new ExperimentalRoomDoor(Room.RoomTwo, DoorFacing.EastCw);
			blocker = new ExperimentalRoomBlocker(Room.RoomThree);
			door.Hue = 1109;
			door.MoveToWorld(new Point3D(985, 1072, -42), Map.TerMur);
			blocker.MoveToWorld(new Point3D(985, 1072, -42), Map.TerMur);

			ExperimentalRoomChest chest = new();
			chest.MoveToWorld(new Point3D(984, 1064, -37), Map.TerMur);

			ExperimentalBook instr = new()
			{
				Movable = false
			};
			instr.MoveToWorld(new Point3D(995, 1114, -36), Map.TerMur);

			SecretDungeonDoor dd = new(DoorFacing.NorthCcw)
			{
				ClosedId = 87,
				OpenedId = 88
			};
			dd.MoveToWorld(new Point3D(1007, 1119, -42), Map.TerMur);

			LocalizedSign sign = new(3026, 1113407)
			{
				Movable = false
			}; // Experimental Room Access
			sign.MoveToWorld(new Point3D(980, 1119, -37), Map.TerMur);
		}

		if (PuzzleEnabled)
		{
			//Puzzle Room
			PuzzleBox box = new(PuzzleType.WestBox);
			box.MoveToWorld(new Point3D(1090, 1171, 11), Map.TerMur);

			box = new PuzzleBox(PuzzleType.EastBox);
			box.MoveToWorld(new Point3D(1104, 1171, 11), Map.TerMur);

			box = new PuzzleBox(PuzzleType.NorthBox);
			box.MoveToWorld(new Point3D(1097, 1163, 11), Map.TerMur);

			PuzzleBook book = new()
			{
				Movable = false
			};
			book.MoveToWorld(new Point3D(1109, 1153, -17), Map.TerMur);

			PuzzleRoomTeleporter tele = new()
			{
				PointDest = new Point3D(1097, 1173, 1),
				MapDest = Map.TerMur
			};
			tele.MoveToWorld(new Point3D(1097, 1175, 0), Map.TerMur);

			tele = new PuzzleRoomTeleporter
			{
				PointDest = new Point3D(1098, 1173, 1),
				MapDest = Map.TerMur
			};
			tele.MoveToWorld(new Point3D(1098, 1175, 0), Map.TerMur);

			MetalDoor2 door2 = new(DoorFacing.WestCcw)
			{
				Locked = true,
				KeyValue = 50000
			};
			door2.MoveToWorld(new Point3D(1097, 1174, 1), Map.TerMur);

			door2 = new MetalDoor2(DoorFacing.EastCw)
			{
				Locked = true,
				KeyValue = 50000
			};
			door2.MoveToWorld(new Point3D(1098, 1174, 1), Map.TerMur);

			Teleporter telep = new()
			{
				PointDest = new Point3D(1097, 1175, 0),
				MapDest = Map.TerMur
			};
			telep.MoveToWorld(new Point3D(1097, 1173, 1), Map.TerMur);

			telep = new Teleporter
			{
				PointDest = new Point3D(1098, 1175, 0),
				MapDest = Map.TerMur
			};
			telep.MoveToWorld(new Point3D(1098, 1173, 1), Map.TerMur);

			telep = new Teleporter
			{
				PointDest = new Point3D(996, 1117, -42),
				MapDest = Map.TerMur
			};
			telep.MoveToWorld(new Point3D(980, 1064, -42), Map.TerMur);

			Static sparkle = new(14138);
			sparkle.MoveToWorld(new Point3D(980, 1064, -42), Map.TerMur);
		}

		if (MazeEnabled)
		{
			//Maze of Death
			UnderworldPuzzleBox pBox = new();
			pBox.MoveToWorld(new Point3D(1068, 1026, -37), Map.TerMur);

			GoldenCompass compass = new();
			compass.MoveToWorld(new Point3D(1070, 1055, -34), Map.TerMur);

			Item map = new RolledMapOfTheUnderworld();
			map.MoveToWorld(new Point3D(1072, 1055, -36), Map.TerMur);
			map.Movable = false;

			FountainOfFortune f = new();
			f.MoveToWorld(new Point3D(1121, 957, -42), Map.TerMur);

			Item tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1121, 965, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1122, 965, -40), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1123, 965, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1124, 965, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1122, 964, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1123, 964, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1123, 963, -40), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1123, 962, -40), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1123, 961, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1122, 961, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1122, 960, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1121, 960, -41), Map.TerMur);

			tile = new InvisibleTile();
			tile.MoveToWorld(new Point3D(1121, 959, -41), Map.TerMur);

			GenerateRevealTiles();
			CheckCannoneers();
		}

		if (ToKEnabled)
		{
			// Bridge
			Static st = new(16880);
			st.MoveToWorld(new Point3D(36, 36, 0), Map.TerMur);

			st = new Static(16882);
			st.MoveToWorld(new Point3D(37, 36, 0), Map.TerMur);

			st = new Static(16883);
			st.MoveToWorld(new Point3D(38, 36, 0), Map.TerMur);

			st = new Static(16878);
			st.MoveToWorld(new Point3D(36, 35, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(37, 35, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(38, 35, 0), Map.TerMur);

			st = new Static(16878);
			st.MoveToWorld(new Point3D(36, 34, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(37, 34, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(38, 34, 0), Map.TerMur);

			st = new Static(16878);
			st.MoveToWorld(new Point3D(36, 33, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(37, 33, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(38, 33, 0), Map.TerMur);

			st = new Static(16878);
			st.MoveToWorld(new Point3D(36, 32, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(37, 32, 0), Map.TerMur);

			st = new Static(16884);
			st.MoveToWorld(new Point3D(38, 32, 0), Map.TerMur);

			st = new Static(16872);
			st.MoveToWorld(new Point3D(36, 31, 0), Map.TerMur);

			st = new Static(16873);
			st.MoveToWorld(new Point3D(37, 31, 0), Map.TerMur);

			st = new Static(16874);
			st.MoveToWorld(new Point3D(38, 31, 0), Map.TerMur);

			//Sacred Quest Blocker
			SacredQuestBlocker sq = new();
			sq.MoveToWorld(new Point3D(35, 38, 0), Map.TerMur);

			sq = new SacredQuestBlocker();
			sq.MoveToWorld(new Point3D(36, 38, 0), Map.TerMur);

			sq = new SacredQuestBlocker();
			sq.MoveToWorld(new Point3D(37, 38, 0), Map.TerMur);

			sq = new SacredQuestBlocker();
			sq.MoveToWorld(new Point3D(38, 38, 0), Map.TerMur);

			sq = new SacredQuestBlocker();
			sq.MoveToWorld(new Point3D(39, 38, 0), Map.TerMur);

			// Guardian
			XmlSpawner spawner = new(1, 300, 600, 0, 0, 0,
				"GargoyleDestroyer, /blessed/true/Frozen/true/Direction/West/Paralyzed/true/Hue/2401/Name/Guardian")
			{
				SmartSpawning = true
			};
			spawner.MoveToWorld(new Point3D(42, 38, 13), Map.TerMur);

			spawner = new XmlSpawner(1, 300, 600, 0, 0, 0,
				"GargoyleDestroyer, /blessed/true/Frozen/true/Direction/East/Paralyzed/true/Hue/2401/Name/Guardian")
			{
				SmartSpawning = true
			};
			spawner.MoveToWorld(new Point3D(33, 38, 13), Map.TerMur);

			// Teleporter
			ToKTeleporter t = new();
			t.MoveToWorld(new Point3D(21, 99, 1), Map.TerMur);

			st = new Static(14186); // sparkle
			st.MoveToWorld(new Point3D(21, 99, 1), Map.TerMur);

			st = new Static(18304); // door
			st.MoveToWorld(new Point3D(18, 99, 0), Map.TerMur);

			TombOfKingsSecretDoor door = new(18304);
			door.MoveToWorld(new Point3D(52, 99, 0), Map.TerMur);

			// Serpent's Breath
			BaseItem unused1 = new FlameOfOrder(new Point3D(28, 212, 3), Map.TerMur);
			BaseItem unused = new FlameOfChaos(new Point3D(43, 212, 3), Map.TerMur);

			st = new Static(3025)
			{
				Name = "Order Shall Steal The Serpent's Strength"
			};
			st.MoveToWorld(new Point3D(28, 208, 4), Map.TerMur);

			st = new Static(3025)
			{
				Name = "Chaos Shall Quell The Serpent's Wrath"
			};
			st.MoveToWorld(new Point3D(28, 208, 4), Map.TerMur);

			// Kings' Chambers
			ChamberLever.Generate();
			Chamber.Generate();
			ChamberSpawner.Generate();
		}

		_mobile.SendMessage("World generating complete. {0} items were generated.", _count);
	}

	[Usage("RemoveStealArties")]
	[Description("Removes the stealable artifacts spawner and every not yet stolen stealable artifacts.")]
	public static void RemoveStealArties_OnCommand(CommandEventArgs args)
	{
		Mobile from = args.Mobile;

		from.SendMessage(StealableArtifactsSpawner.Remove()
			? "Stealable artifacts spawner removed."
			: "Stealable artifacts spawner not present.");
	}

	[Usage("ArisenDelete")]
	[Description("Removes the Arisen creatures spawner.")]
	public static void ArisenDelete_OnCommand(CommandEventArgs args)
	{
		Mobile from = args.Mobile;

		from.SendMessage(ArisenController.Remove() ? "Arisen creatures spawner removed." : "Arisen creatures spawner not present.");
	}

	public static void DelSpawns_OnCommand(CommandEventArgs e)
	{
		ChampionSystem.RemoveSpawns();
		e.Mobile.SendMessage("Champ Spawns Removed!");
	}

	[Usage("ChampionInfo")]
	[Description("Opens a UI that displays information about the champion system")]
	private static void ChampionInfo_OnCommand(CommandEventArgs e)
	{
		if (!ChampionSystem.Enabled)
		{
			e.Mobile.SendMessage("The champion system is not enabled.");
			return;
		}
		if (ChampionSystem.AllSpawns.Count <= 0)
		{
			e.Mobile.SendMessage("The champion system is enabled but no altars exist");
			return;
		}
		e.Mobile.SendGump(new ChampionSystem.ChampionSystemGump());
	}

	public static void Generate(string folder, params Map[] maps)
	{
		if (!Directory.Exists(folder))
			return;

		string[] files = Directory.GetFiles(folder, "*.cfg");

		for (int i = 0; i < files.Length; ++i)
		{
			ArrayList list = DecorationList.ReadAll(files[i]);

			for (int j = 0; j < list.Count; ++j)
				_count += ((DecorationList)list[j])!.Generate(maps);
		}
	}

	private static Mobile _mobile;
	private static int _count;

	private static void GenerateRevealTiles()
	{
		Map map = Map.TerMur;

		for (int x = 1182; x <= 1192; x++)
		{
			for (int y = 1120; y <= 1134; y++)
			{
				if (map == null || !map.CanSpawnMobile(x, y, -42))
					continue;

				RevealTile t = new();
				t.MoveToWorld(new Point3D(x, y, -42), map);
			}
		}

		RevealTile tile = new();
		tile.MoveToWorld(new Point3D(1180, 883, 0), map);

		tile = new RevealTile();
		tile.MoveToWorld(new Point3D(1180, 882, 0), map);

		tile = new RevealTile();
		tile.MoveToWorld(new Point3D(1180, 881, 0), map);

		tile = new RevealTile();
		tile.MoveToWorld(new Point3D(1180, 880, 0), map);

		tile = new RevealTile();
		tile.MoveToWorld(new Point3D(1180, 879, 0), map);
	}

	private static void CheckCannoneers()
	{
		Cannon cannon = Map.TerMur.FindItem<Cannon>(new Point3D(1126, 1200, -2));

		if (cannon == null)
		{
			cannon = new Cannon(CannonDirection.North);
			cannon.MoveToWorld(new Point3D(1126, 1200, -2), Map.TerMur);
		}

		var cannoneer = Map.TerMur.FindMobile<MilitiaCanoneer>(new Point3D(1126, 1203, -2));

		if (cannoneer == null)
		{
			cannoneer = new MilitiaCanoneer();
			cannoneer.MoveToWorld(new Point3D(1126, 1203, -2), Map.TerMur);

		}

		cannon.Canoneer = cannoneer;

		cannon = Map.TerMur.FindItem<Cannon>(new Point3D(1131, 1200, -2));

		if (cannon == null)
		{
			cannon = new Cannon(CannonDirection.North);
			cannon.MoveToWorld(new Point3D(1131, 1200, -2), Map.TerMur);
		}

		cannoneer = Map.TerMur.FindMobile<MilitiaCanoneer>(new Point3D(1131, 1203, -2));

		if (cannoneer == null)
		{
			cannoneer = new MilitiaCanoneer();
			cannoneer.MoveToWorld(new Point3D(1131, 1203, -2), Map.TerMur);
		}

		cannon.Canoneer = cannoneer;
	}


	private static bool FindMorphItem(int x, int y, int z, int inactiveItemId, int activeItemId)
	{
		IPooledEnumerable eable = Map.Felucca.GetItemsInRange(new Point3D(x, y, z), 0);

		foreach (Item item in eable)
		{
			if (item is not MorphItem morphItem || item.Z != z || morphItem.InactiveItemID != inactiveItemId ||
				morphItem.ActiveItemID != activeItemId)
				continue;
			eable.Free();
			return true;
		}

		eable.Free();
		return false;
	}

	private static bool FindEffectController(int x, int y, int z)
	{
		IPooledEnumerable eable = Map.Felucca.GetItemsInRange(new Point3D(x, y, z), 0);

		if (eable.Cast<Item>().Any(item => item is EffectController && item.Z == z))
		{
			eable.Free();
			return true;
		}

		eable.Free();
		return false;
	}

	private static Item TryCreateItem(int x, int y, int z, Item srcItem)
	{
		IPooledEnumerable eable = Map.Felucca.GetItemsInBounds(new Rectangle2D(x, y, 1, 1));

		foreach (Item item in eable)
		{
			if (item.GetType() != srcItem.GetType())
				continue;

			eable.Free();
			srcItem.Delete();
			return item;
		}

		eable.Free();
		srcItem.MoveToWorld(new Point3D(x, y, z), Map.Felucca);
		_count++;

		return srcItem;
	}

	private static void CreateMorphItem(int x, int y, int z, int inactiveItemId, int activeItemId, int range)
	{
		if (FindMorphItem(x, y, z, inactiveItemId, activeItemId))
			return;

		MorphItem item = new(inactiveItemId, activeItemId, range, 3);

		item.MoveToWorld(new Point3D(x, y, z), Map.Felucca);
		_count++;
	}

	private static void CreateApproachLight(int x, int y, int z, int off, int on, LightType light)
	{
		if (FindMorphItem(x, y, z, off, on))
			return;

		MorphItem item = new(off, on, 2, 3)
		{
			Light = light
		};

		item.MoveToWorld(new Point3D(x, y, z), Map.Felucca);
		_count++;
	}

	private static void CreateSoundEffect(int x, int y, int z, int sound, int range)
	{
		if (FindEffectController(x, y, z))
			return;

		EffectController item = new()
		{
			SoundId = sound,
			TriggerType = EffectTriggerType.InRange,
			TriggerRange = range
		};

		item.MoveToWorld(new Point3D(x, y, z), Map.Felucca);
		_count++;
	}

	private static void CreateBigTeleporterItem(int x, int y, bool reverse)
	{
		if (FindMorphItem(x, y, 0, reverse ? 0x17DC : 0x17EE, reverse ? 0x17EE : 0x17DC))
			return;

		MorphItem item = new(reverse ? 0x17DC : 0x17EE, reverse ? 0x17EE : 0x17DC, 1, 3);

		item.MoveToWorld(new Point3D(x, y, 0), Map.Felucca);
		_count++;
	}

	private static bool FindItem(Point3D p, Map map)
	{
		IPooledEnumerable eable = map.GetItemsInRange(p, 0);

		if (eable.Cast<Item>().Any())
		{
			eable.Free();
			return true;
		}

		eable.Free();
		return false;
	}
}

public class DecorationList
{
	private Type m_Type;
	private int m_ItemId;
	private string[] m_Params;
	private ArrayList m_Entries;

	private DecorationList()
	{
	}

	private static readonly Type m_TypeofStatic = typeof(Static);
	private static readonly Type m_TypeofLocalizedStatic = typeof(LocalizedStatic);
	private static readonly Type m_TypeofBaseDoor = typeof(BaseDoor);
	private static readonly Type m_TypeofAnkhWest = typeof(AnkhWest);
	private static readonly Type m_TypeofAnkhNorth = typeof(AnkhNorth);
	private static readonly Type m_TypeofBeverage = typeof(BaseBeverage);
	private static readonly Type m_TypeofLocalizedSign = typeof(LocalizedSign);
	private static readonly Type m_TypeofMarkContainer = typeof(MarkContainer);
	private static readonly Type m_TypeofWarningItem = typeof(WarningItem);
	private static readonly Type m_TypeofHintItem = typeof(HintItem);
	private static readonly Type m_TypeofCannon = typeof(Cannon);
	private static readonly Type m_TypeofSerpentPillar = typeof(SerpentPillar);

	private Item Construct()
	{
		if (m_Type == null)
			return null;

		Item item;

		try
		{
			if (m_Type == m_TypeofStatic)
			{
				item = new Static(m_ItemId);
			}
			else if (m_Type == m_TypeofLocalizedStatic)
			{
				int labelNumber = 0;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (!m_Params[i].StartsWith("LabelNumber"))
						continue;

					int indexOf = m_Params[i].IndexOf('=');

					if (indexOf < 0)
						continue;
					labelNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					break;
				}

				item = new LocalizedStatic(m_ItemId, labelNumber);
			}
			else if (m_Type == m_TypeofLocalizedSign)
			{
				int labelNumber = 0;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (!m_Params[i].StartsWith("LabelNumber"))
						continue;
					int indexOf = m_Params[i].IndexOf('=');

					if (indexOf < 0)
						continue;
					labelNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					break;
				}

				item = new LocalizedSign(m_ItemId, labelNumber);
			}
			else if (m_Type == m_TypeofAnkhWest || m_Type == m_TypeofAnkhNorth)
			{
				bool bloodied = false;

				for (int i = 0; !bloodied && i < m_Params.Length; ++i)
					bloodied = m_Params[i] == "Bloodied";

				if (m_Type == m_TypeofAnkhWest)
					item = new AnkhWest(bloodied);
				else
					item = new AnkhNorth(bloodied);
			}
			else if (m_Type == m_TypeofMarkContainer)
			{
				bool bone = false;
				bool locked = false;
				Map map = Map.Malas;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					switch (m_Params[i])
					{
						case "Bone":
							bone = true;
							break;
						case "Locked":
							locked = true;
							break;
						default:
						{
							if (m_Params[i].StartsWith("TargetMap"))
							{
								int indexOf = m_Params[i].IndexOf('=');

								if (indexOf >= 0)
									map = Map.Parse(m_Params[i][++indexOf..]);
							}

							break;
						}
					}
				}

				MarkContainer mc = new(bone, locked)
				{
					TargetMap = map,
					Description = "strange location"
				};

				item = mc;
			}
			else if (m_Type == m_TypeofHintItem)
			{
				int range = 0;
				int messageNumber = 0;
				string messageString = null;
				int hintNumber = 0;
				string hintString = null;
				TimeSpan resetDelay = TimeSpan.Zero;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Range"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							range = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("WarningString"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							messageString = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("WarningNumber"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							messageNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("HintString"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							hintString = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("HintNumber"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							hintNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("ResetDelay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							resetDelay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
				}

				HintItem hi = new(m_ItemId, range, messageNumber, hintNumber)
				{
					WarningString = messageString,
					HintString = hintString,
					ResetDelay = resetDelay
				};

				item = hi;
			}
			else if (m_Type == m_TypeofWarningItem)
			{
				int range = 0;
				int messageNumber = 0;
				string messageString = null;
				TimeSpan resetDelay = TimeSpan.Zero;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Range"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							range = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("WarningString"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							messageString = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("WarningNumber"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							messageNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("ResetDelay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							resetDelay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
				}

				WarningItem wi = new(m_ItemId, range, messageNumber)
				{
					WarningString = messageString,
					ResetDelay = resetDelay
				};

				item = wi;
			}
			else if (m_Type == m_TypeofCannon)
			{
				CannonDirection direction = CannonDirection.North;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("CannonDirection"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							direction = (CannonDirection)Enum.Parse(typeof(CannonDirection), m_Params[i][++indexOf..], true);
					}
				}

				item = new Cannon(direction);
			}
			else if (m_Type == m_TypeofSerpentPillar)
			{
				string word = null;
				Rectangle2D destination = new();

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Word"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							word = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("DestStart"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							destination.Start = Point2D.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("DestEnd"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							destination.End = Point2D.Parse(m_Params[i][++indexOf..]);
					}
				}

				item = new SerpentPillar(word, destination);
			}
			else if (m_Type.IsSubclassOf(m_TypeofBeverage))
			{
				BeverageType content = BeverageType.Liquor;
				bool fill = false;

				for (int i = 0; !fill && i < m_Params.Length; ++i)
				{
					if (!m_Params[i].StartsWith("Content"))
						continue;
					int indexOf = m_Params[i].IndexOf('=');

					if (indexOf < 0)
						continue;
					content = (BeverageType)Enum.Parse(typeof(BeverageType), m_Params[i][++indexOf..], true);
					fill = true;
				}

				if (fill)
					item = (Item)Activator.CreateInstance(m_Type, content);
				else
					item = (Item)Activator.CreateInstance(m_Type);
			}
			else if (m_Type.IsSubclassOf(m_TypeofBaseDoor))
			{
				DoorFacing facing = DoorFacing.WestCw;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (!m_Params[i].StartsWith("Facing"))
						continue;
					int indexOf = m_Params[i].IndexOf('=');

					if (indexOf < 0)
						continue;
					facing = (DoorFacing)Enum.Parse(typeof(DoorFacing), m_Params[i][++indexOf..], true);
					break;
				}

				item = (Item)Activator.CreateInstance(m_Type, facing);
			}
			else
			{
				item = (Item)Activator.CreateInstance(m_Type);
			}
		}
		catch (Exception e)
		{
			throw new Exception($"Bad type: {m_Type}", e);
		}

		switch (item)
		{
			case BaseAddon addon when addon is MaabusCoffin coffin:
			{
				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("SpawnLocation"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							coffin.SpawnLocation = Point3D.Parse(m_Params[i][++indexOf..]);
					}
				}

				break;
			}
			case BaseAddon addon:
			{
				if (m_ItemId > 0)
				{
					List<AddonComponent> comps = addon.Components;

					for (int i = 0; i < comps.Count; ++i)
					{
						AddonComponent comp = comps[i];

						if (comp.Offset == Point3D.Zero)
							comp.ItemId = m_ItemId;
					}
				}

				break;
			}
			case BaseLight light:
			{
				bool unlit = false, unprotected = false;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (!unlit && m_Params[i] == "Unlit")
						unlit = true;
					else if (!unprotected && m_Params[i] == "Unprotected")
						unprotected = true;

					if (unlit && unprotected)
						break;
				}

				if (!unlit)
					light.Ignite();
				if (!unprotected)
					light.Protected = true;

				if (m_ItemId > 0)
					light.ItemId = m_ItemId;
				break;
			}
			case Spawner spawner:
			{
				Server.Mobiles.Spawner sp = spawner;

				sp.NextSpawn = TimeSpan.Zero;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Spawn"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.SpawnObjects.Add(new Mobiles.SpawnObject(m_Params[i][++indexOf..]));
					}
					else if (m_Params[i].StartsWith("MinDelay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.MinDelay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("MaxDelay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.MaxDelay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("NextSpawn"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.NextSpawn = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Count"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.MaxCount = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Team"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.Team = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("HomeRange"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.HomeRange = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Running"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.Running = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Group"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							sp.Group = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
				}

				break;
			}
			case RecallRune recallRune:
			{
				RecallRune rune = recallRune;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Description"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							rune.Description = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("Marked"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							rune.Marked = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("TargetMap"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							rune.TargetMap = Map.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Target"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							rune.Target = Point3D.Parse(m_Params[i][++indexOf..]);
					}
				}

				break;
			}
			case SkillTeleporter teleporter:
			{
				SkillTeleporter tp = teleporter;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Skill"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Skill = (SkillName)Enum.Parse(typeof(SkillName), m_Params[i][++indexOf..], true);
					}
					else if (m_Params[i].StartsWith("RequiredFixedPoint"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Required = Utility.ToInt32(m_Params[i][++indexOf..]) * 0.1;
					}
					else if (m_Params[i].StartsWith("Required"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Required = Utility.ToDouble(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("MessageString"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.MessageString = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("MessageNumber"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.MessageNumber = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("PointDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.PointDest = Point3D.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("MapDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.MapDest = Map.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Creatures"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Creatures = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SourceEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SourceEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("DestEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.DestEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SoundID"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SoundID = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Delay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Delay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
				}

				if (m_ItemId > 0)
					teleporter.ItemId = m_ItemId;
				break;
			}
			case KeywordTeleporter teleporter:
			{
				KeywordTeleporter tp = teleporter;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("Substring"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Substring = m_Params[i][++indexOf..];
					}
					else if (m_Params[i].StartsWith("Keyword"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Keyword = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Range"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Range = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("PointDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.PointDest = Point3D.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("MapDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.MapDest = Map.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Creatures"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Creatures = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SourceEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SourceEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("DestEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.DestEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SoundID"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SoundID = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Delay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Delay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
				}

				if (m_ItemId > 0)
					teleporter.ItemId = m_ItemId;
				break;
			}
			case Teleporter teleporter:
			{
				Teleporter tp = teleporter;

				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (m_Params[i].StartsWith("PointDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.PointDest = Point3D.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("MapDest"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.MapDest = Map.Parse(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Creatures"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Creatures = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SourceEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SourceEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("DestEffect"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.DestEffect = Utility.ToBoolean(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("SoundID"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.SoundID = Utility.ToInt32(m_Params[i][++indexOf..]);
					}
					else if (m_Params[i].StartsWith("Delay"))
					{
						int indexOf = m_Params[i].IndexOf('=');

						if (indexOf >= 0)
							tp.Delay = TimeSpan.Parse(m_Params[i][++indexOf..]);
					}
				}

				if (m_ItemId > 0)
					teleporter.ItemId = m_ItemId;
				break;
			}
			case FillableContainer container:
			{
				for (int i = 0; i < m_Params.Length; ++i)
				{
					if (!m_Params[i].StartsWith("ContentType"))
						continue;

					int indexOf = m_Params[i].IndexOf('=');

					if (indexOf >= 0)
						container.ContentType = (FillableContentType)Enum.Parse(typeof(FillableContentType), m_Params[i][++indexOf..], true);
				}

				if (m_ItemId > 0)
					container.ItemId = m_ItemId;
				break;
			}
			default:
			{
				if (m_ItemId > 0)
				{
					if (item != null)
						item.ItemId = m_ItemId;
				}

				break;
			}
		}

		item!.Movable = false;

		for (int i = 0; i < m_Params.Length; ++i)
		{
			if (m_Params[i].StartsWith("Light"))
			{
				int indexOf = m_Params[i].IndexOf('=');

				if (indexOf >= 0)
					item.Light = (LightType)Enum.Parse(typeof(LightType), m_Params[i][++indexOf..], true);
			}
			else if (m_Params[i].StartsWith("Hue"))
			{
				int indexOf = m_Params[i].IndexOf('=');

				if (indexOf < 0)

					continue;
				int hue = Utility.ToInt32(m_Params[i][++indexOf..]);

				if (item is DyeTub tub)
					tub.DyedHue = hue;
				else
					item.Hue = hue;
			}
			else if (m_Params[i].StartsWith("Name"))
			{
				int indexOf = m_Params[i].IndexOf('=');

				if (indexOf >= 0)
					item.Name = m_Params[i][++indexOf..];
			}
			else if (m_Params[i].StartsWith("Amount"))
			{
				int indexOf = m_Params[i].IndexOf('=');

				if (indexOf < 0)
					continue;

				// Must suppress stackable warnings

				bool wasStackable = item.Stackable;

				item.Stackable = true;
				item.Amount = Utility.ToInt32(m_Params[i][++indexOf..]);
				item.Stackable = wasStackable;
			}
		}

		return item;
	}

	private static readonly Queue m_DeleteQueue = new();

	private static bool FindItem(int x, int y, int z, Map map, Item srcItem)
	{
		int itemId = srcItem.ItemId;

		bool res = false;

		IPooledEnumerable eable;

		if (srcItem is BaseDoor)
		{
			eable = map.GetItemsInRange(new Point3D(x, y, z), 1);

			foreach (Item item in eable)
			{
				if (item is not BaseDoor bd)
					continue;

				Point3D p;
				int bdItemId;

				if (bd.Open)
				{
					p = new Point3D(bd.X - bd.Offset.X, bd.Y - bd.Offset.Y, bd.Z - bd.Offset.Z);
					bdItemId = bd.ClosedId;
				}
				else
				{
					p = bd.Location;
					bdItemId = bd.ItemId;
				}

				if (p.X != x || p.Y != y)
					continue;

				if (bd.Z == z && bdItemId == itemId)
					res = true;
				else if (Math.Abs(bd.Z - z) < 8)
					m_DeleteQueue.Enqueue(item);
			}
		}
		else if ((TileData.ItemTable[itemId & TileData.MaxItemValue].Flags & TileFlag.LightSource) != 0)
		{
			eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

			LightType lt = srcItem.Light;
			string srcName = srcItem.ItemData.Name;

			foreach (Item item in eable)
			{
				if (item.Z == z)
				{
					if (item.ItemId == itemId)
					{
						if (item.Light != lt)
							m_DeleteQueue.Enqueue(item);
						else
							res = true;
					}
					else if ((item.ItemData.Flags & TileFlag.LightSource) != 0 && item.ItemData.Name == srcName)
					{
						m_DeleteQueue.Enqueue(item);
					}
				}
			}
		}
		else if (srcItem is Teleporter || srcItem is FillableContainer || srcItem is BaseBook)
		{
			eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

			Type type = srcItem.GetType();

			foreach (Item item in eable)
			{
				if (item.Z == z && item.ItemId == itemId)
				{
					if (item.GetType() != type)
						m_DeleteQueue.Enqueue(item);
					else
						res = true;
				}
			}
		}
		else
		{
			eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

			if (eable.Cast<Item>().Any(item => item.Z == z && item.ItemId == itemId))
			{
				eable.Free();
				return true;
			}
		}

		eable.Free();

		while (m_DeleteQueue.Count > 0)
			((Item)m_DeleteQueue.Dequeue())?.Delete();

		return res;
	}

	public int Generate(Map[] maps)
	{
		int count = 0;

		Item item = null;

		for (int i = 0; i < m_Entries.Count; ++i)
		{
			DecorationEntry entry = (DecorationEntry)m_Entries[i];
			if (entry == null)
				continue;

			Point3D loc = entry.Location;
			string extra = entry.Extra;

			for (int j = 0; j < maps.Length; ++j)
			{
				item ??= Construct();

				if (item == null)
					continue;

				if (FindItem(loc.X, loc.Y, loc.Z, maps[j], item))
				{
				}
				else
				{
					item.MoveToWorld(loc, maps[j]);
					++count;

					switch (item)
					{
						case BaseDoor door:
						{
							IPooledEnumerable eable = maps[j].GetItemsInRange(loc, 1);

							Type itemType = door.GetType();

							foreach (Item link in eable)
							{
								if (link == item || link.Z != door.Z || link.GetType() != itemType)
									continue;

								door.Link = (BaseDoor)link;
								((BaseDoor)link).Link = door;
								break;
							}

							eable.Free();
							break;
						}
						case MarkContainer container:
							try { container.Target = Point3D.Parse(extra); }
							catch
							{
								// ignored
							}

							break;
					}

					item = null;
				}
			}
		}

		item?.Delete();

		return count;
	}

	public static ArrayList ReadAll(string path)
	{
		using StreamReader ip = new(path);
		ArrayList list = new();

		for (DecorationList v = Read(ip); v != null; v = Read(ip))
			list.Add(v);

		return list;
	}

	private static readonly string[] m_EmptyParams = Array.Empty<string>();

	private static DecorationList Read(StreamReader ip)
	{
		string line;

		while ((line = ip.ReadLine()) != null)
		{
			line = line.Trim();

			if (line.Length > 0 && !line.StartsWith("#"))
				break;
		}

		if (string.IsNullOrEmpty(line))
			return null;

		DecorationList list = new();

		int indexOf = line.IndexOf(' ');

		list.m_Type = Assembler.FindTypeByName(line[..indexOf++], true);

		if (list.m_Type == null)
			throw new ArgumentException($"Type not found for header: '{line}'");

		line = line[indexOf..];
		indexOf = line.IndexOf('(');
		if (indexOf >= 0)
		{
			list.m_ItemId = Utility.ToInt32(line[..(indexOf - 1)]);

			string parms = line[++indexOf..];

			if (line.EndsWith(")"))
				parms = parms[..^1];

			list.m_Params = parms.Split(';');

			for (int i = 0; i < list.m_Params.Length; ++i)
				list.m_Params[i] = list.m_Params[i].Trim();
		}
		else
		{
			list.m_ItemId = Utility.ToInt32(line);
			list.m_Params = m_EmptyParams;
		}

		list.m_Entries = new ArrayList();

		while ((line = ip.ReadLine()) != null)
		{
			line = line.Trim();

			if (line.Length == 0)
				break;

			if (line.StartsWith("#"))
				continue;

			list.m_Entries.Add(new DecorationEntry(line));
		}

		return list;
	}
}

public class DecorationEntry
{
	public Point3D Location { get; }
	public string Extra { get; }

	public DecorationEntry(string line)
	{

		Pop(out string x, ref line);
		Pop(out string y, ref line);
		Pop(out string z, ref line);

		Location = new Point3D(Utility.ToInt32(x), Utility.ToInt32(y), Utility.ToInt32(z));
		Extra = line;
	}

	private static void Pop(out string v, ref string line)
	{
		int space = line.IndexOf(' ');

		if (space >= 0)
		{
			v = line[..space++];
			line = line[space..];
		}
		else
		{
			v = line;
			line = "";
		}
	}
}
