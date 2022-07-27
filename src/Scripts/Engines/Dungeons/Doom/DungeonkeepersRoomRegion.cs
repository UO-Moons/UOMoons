using Server.ContextMenus;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Doom
{
    [PropertyObject]
    public class DungeonkeepersRoomRegion : BaseRegion
    {
	    private static readonly bool Enabled = Settings.Configuration.Get<bool>("Dungeons", "DoomKeepersRoom");
		public static void Initialize()
        {
	        if (Enabled)
	        {
		        new DungeonkeepersRoomRegion();
	        }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        private GaryTheDungeonMaster Gary { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private Sapphired20 Dice { get; set; }

        private DisplayStatue[] Statues { get; set; }

        private Timer Timer { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private BaseDoor DoorOne { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private BaseDoor DoorTwo { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private DateTime NextRoll { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private BaseCreature Spawn { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        private int ForceRoll { get; set; }

        private static readonly Point3D m_GaryLoc = new(389, 8, 0);
        private static readonly Point3D m_DiceLoc = new(390, 8, 5);
        private static readonly Point3D m_RulesLoc = new(390, 9, 6);
        private static readonly Point3D m_SpawnLoc = new(396, 8, 4);
        private static readonly Point3D m_DoorOneLoc = new(395, 15, -1);
        private static readonly Point3D m_DoorTwoLoc = new(396, 15, -1);
        private static readonly Point3D[] m_StatueLocs = {
            new(393, 4, 5),
            new(395, 4 ,5),
            new(397, 4, 5)
        };
        private static readonly Rectangle2D[] m_Bounds =
        {
            new(388, 3, 16, 12)
        };

        private readonly Type[] m_MonsterList =
        {
            typeof(BoneDemon), typeof(SkeletalKnight), typeof(SkeletalMage), typeof(DarkGuardian), typeof(Devourer),
            typeof(FleshGolem), typeof(Gibberling), typeof(AncientLich), typeof(Lich), typeof(LichLord),
            typeof(Mummy), typeof(PatchworkSkeleton), typeof(Ravager), typeof(RestlessSoul), typeof(Dragon),
            typeof(SkeletalDragon), typeof(VampireBat), typeof(WailingBanshee), typeof(WandererOfTheVoid)
        };

        private static TimeSpan RollDelay => TimeSpan.FromMinutes(Utility.RandomMinMax(12, 15));

        private DungeonkeepersRoomRegion()
            : base("Dungeon Keeper's Room", Map.Malas, Find(m_GaryLoc, Map.Malas), m_Bounds)
        {
            Register();
            CheckStuff();
        }

        public override void OnRegister()
        {
            NextRoll = DateTime.UtcNow;
            Timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), OnTick);

            ForceRoll = -1;
        }

        public override void OnUnregister()
        {
	        if (Timer == null)
		        return;
	        Timer.Stop();
	        Timer = null;
        }

        private void OnTick()
        {
	        if (NextRoll >= DateTime.UtcNow || !AllPlayers.Any(p => p.Alive))
		        return;

	        DoRoll();
	        NextRoll = DateTime.UtcNow + RollDelay;
        }

        private void DoRoll()
        {
	        GaryTheDungeonMaster g = GetGary();
	        Sapphired20 d = GetDice();
	        int roll = ForceRoll >= 0 && ForceRoll < 20 ? ForceRoll : Utility.Random(20);

	        g.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1080099); // *Gary rolls the sapphire d20*

	        BaseDoor door1 = GetDoor1();
	        BaseDoor door2 = GetDoor2();

	        door1.Locked = true;
	        door2.Locked = true;

	        Timer.DelayCall(TimeSpan.FromMinutes(2), () =>
	        {
		        door1.Locked = false;
		        door2.Locked = false;
	        });

	        Timer.DelayCall(TimeSpan.FromSeconds(1), () =>
	        {
		        if (d != null)
		        {
			        d.Roll(roll);
		        }
		        else
		        {
			        foreach (Mobile m in AllPlayers)
			        {
				        m.SendMessage("- {0} -", (roll + 1).ToString());
			        }
		        }
	        });

	        Timer.DelayCall(TimeSpan.FromSeconds(2), () =>
	        {
		        Spawn = Activator.CreateInstance(m_MonsterList[roll]) as BaseCreature;
		        if (Spawn != null)
		        {
			        Spawn.Kills = 100;

			        if (Spawn is Dragon)
			        {
				        Spawn.Body = 155;
				        Spawn.CorpseNameOverride = "a rotting corpse";
			        }

			        Spawn.MoveToWorld(m_SpawnLoc, Map.Malas);
			        Spawn.Home = m_SpawnLoc;
			        Spawn.RangeHome = 7;
		        }

		        ChangeStatues();
	        });

	        ForceRoll = -1;
        }

        public void ChangeStatues()
        {
            foreach (DisplayStatue statue in GetStatues())
            {
                statue.AssignRandom();
            }
        }

        public override void OnDeath(Mobile m)
        {
            if (m == Spawn)
            {
                BaseDoor door1 = GetDoor1();
                BaseDoor door2 = GetDoor2();

                door1.Locked = false;
                door2.Locked = false;

                Spawn = null;
            }

            base.OnDeath(m);
        }

        public override void OnEnter(Mobile m)
        {
            GaryTheDungeonMaster g = GetGary();

            g.SayTo(m, 1080098); // Ah... visitors!
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, Spells.TravelCheckType type)
        {
            switch (type)
            {
                case Spells.TravelCheckType.Mark:
                case Spells.TravelCheckType.RecallTo:
                case Spells.TravelCheckType.RecallFrom:
                case Spells.TravelCheckType.GateTo:
                case Spells.TravelCheckType.GateFrom:
                    return false;
            }

            return base.CheckTravel(traveller, p, type);
        }

        private GaryTheDungeonMaster GetGary()
        {
	        if (Gary is { Deleted: false })
		        return Gary;

	        GaryTheDungeonMaster gary = null;
            IPooledEnumerable eable = Map.GetMobilesInBounds(m_Bounds[0]);

            foreach (Mobile m in eable)
            {
	            if (m is not GaryTheDungeonMaster master)
		            continue;

	            gary = master;
	            break;
            }

            eable.Free();

            if (gary != null)
            {
	            Gary = gary;
	            Gary.MoveToWorld(m_GaryLoc, Map.Malas);
            }
            else
            {
	            Gary = new GaryTheDungeonMaster();
	            Gary.MoveToWorld(m_GaryLoc, Map.Malas);
            }

            return Gary;
        }

        private Sapphired20 GetDice()
        {
	        if (Dice is { Deleted: false })
		        return Dice;

	        Sapphired20 dice = AllItems.OfType<Sapphired20>().FirstOrDefault(i => !i.Deleted);

            if (dice != null)
            {
	            Dice = dice;
	            Dice.MoveToWorld(m_DiceLoc, Map.Malas);
            }
            else
            {
	            Dice = new Sapphired20
	            {
		            Movable = false
	            };
	            Dice.MoveToWorld(m_DiceLoc, Map.Malas);
            }

            return Dice;
        }

        private DisplayStatue[] GetStatues()
        {
            if (Statues == null || Statues.Length != 3)
            {
                Statues = new DisplayStatue[3];
            }

            for (int i = 0; i < 3; i++)
            {
	            if (Statues[i] != null && !Statues[i].Deleted)
		            continue;

	            DisplayStatue s = AllItems.OfType<DisplayStatue>().FirstOrDefault(st => Array.IndexOf(Statues, st) == -1);

	            if (s == null)
	            {
		            Statues[i] = new DisplayStatue
		            {
			            Movable = false
		            };
		            Statues[i].MoveToWorld(m_StatueLocs[i], Map.Malas);
	            }
	            else
	            {
		            Statues[i] = s;
		            Statues[i].MoveToWorld(m_StatueLocs[i], Map.Malas);
	            }
            }

            return Statues;
        }

        private BaseDoor GetDoor1()
        {
	        if (DoorOne is { Deleted: false })
		        return DoorOne;
	        //BaseDoor door = this.AllItems.OfType<DarkWoodDoor>().FirstOrDefault(d => d.Location == _DoorOneLoc);
            Point3D p = m_DoorOneLoc;
            BaseDoor door = GetDoor(p) ?? GetDoor(new Point3D(p.X - 1, p.Y + 1, p.Z));

            if (door == null)
            {
	            DoorOne = new DarkWoodDoor(DoorFacing.WestCw);
	            DoorOne.MoveToWorld(m_DoorOneLoc, Map.Malas);
            }
            else
            {
	            DoorOne = door;
            }

            DoorOne.Locked = false;

            return DoorOne;
        }

        private BaseDoor GetDoor2()
        {
	        if (DoorTwo is { Deleted: false }) return DoorTwo;
	        //BaseDoor door = this.AllItems.OfType<DarkWoodDoor>().FirstOrDefault(d => d.Location == _DoorOneLoc);
            Point3D p = m_DoorTwoLoc;
            BaseDoor door = GetDoor(p) ?? GetDoor(new Point3D(p.X + 1, p.Y + 1, p.Z));

            if (door == null)
            {
	            DoorTwo = new DarkWoodDoor(DoorFacing.EastCcw);
	            DoorTwo.MoveToWorld(m_DoorTwoLoc, Map.Malas);
            }
            else
            {
	            DoorTwo = door;
            }

            DoorTwo.Locked = false;

            return DoorTwo;
        }

        private void CheckStuff()
        {
            GetGary();
            GetStatues();
            GetDice();
            GetDoor1();
            GetDoor2();

            if (!FindObject(typeof(UoBoard), m_RulesLoc))
            {
                UoBoard rules = new()
				{
                    Movable = false
                };
                rules.MoveToWorld(m_RulesLoc, Map.Malas);
            }

            Point3D p = new(390, 7, 2);

            if (!FindObject(typeof(Static), p))
            {
                Static books = new(0x1E22);
                books.MoveToWorld(p, Map.Malas);
            }

            if (FindObject(typeof(ScribesPen), p)) return;
            ScribesPen pen = new()
			{
	            ItemId = 4032,
	            Movable = false
            };
            pen.MoveToWorld(p, Map.Malas);
        }

        private bool FindObject(Type t, Point3D p)
        {
            IPooledEnumerable eable = Map.GetObjectsInRange(p, 0);

            foreach (object o in eable)
            {
                if (o.GetType() == t)
                {
                    eable.Free();
                    return true;
                }
            }

            eable.Free();
            return false;
        }

        private BaseDoor GetDoor(Point3D p)
        {
            IPooledEnumerable eable = Map.GetItemsInRange(p, 0);

            foreach (Item item in eable)
            {
	            if (item is not BaseDoor door)
		            continue;
	            eable.Free();
	            return door;
            }

            eable.Free();
            return null;
        }
    }

    public class Sapphired20 : Item
    {
        public override int LabelNumber => 1080096;  // Star Sapphire d20

        [Constructable]
        public Sapphired20()
            : base(0x3192)
        {
        }

        public override void OnDoubleClick(Mobile m)
        {
            if (GetRegion().IsPartOf<DungeonkeepersRoomRegion>())
            {
                m.SendLocalizedMessage(1080097); // You're blasted back in a blaze of light! This d20 is not yours to roll...

                m.Damage(Utility.RandomMinMax(20, 30));
            }
            else
            {
                Roll(Utility.Random(20));
            }
        }

        public void Roll(int roll)
        {
            PublicOverheadMessage(MessageType.Regular, 0x3B2, false, $"- {roll + 1} -");
        }

        public Sapphired20(Serial serial)
            : base(serial)
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

    public class DisplayStatue : Item
    {
        private MonsterStatuetteInfo m_Info;

        [CommandProperty(AccessLevel.GameMaster)]
        private MonsterStatuetteInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;

                if (ItemId != m_Info.ItemId)
                {
	                ItemId = m_Info.ItemId;
                }

                InvalidateProperties();
            }
        }

        public override int LabelNumber
        {
            get
            {
                if (m_Info == null)
                    return base.LabelNumber;

                return m_Info.LabelNumber;
            }
        }

        [Constructable]
        public DisplayStatue()
        {
            AssignRandom();

            Hue = 2958;
        }

        public void AssignRandom()
        {
            MonsterStatuetteInfo info;

            do
            {
                info = MonsterStatuetteInfo.Table[Utility.Random(MonsterStatuetteInfo.Table.Length)];
            }
            while (ItemId == info.ItemId);

            Info = info;
        }

        public DisplayStatue(Serial serial)
            : base(serial)
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

            AssignRandom();
        }
    }

    public class UoBoard : Item
    {
        private int m_Index;

        [CommandProperty(AccessLevel.GameMaster)]
        private int Index
        {
            get => m_Index;
            set
            {
                m_Index = value;

                if (m_Index < 0)
                    m_Index = 0;

                if (m_Index > 9)
                    m_Index = 0;
            }
        }

        public override int LabelNumber => 1080085;  // The Rulebook

        [Constructable]
        public UoBoard() : base(0xFAA)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 3))
            {
                int cliloc;

                if (m_Index == 0)
                {
                    cliloc = 1080095;
                }
                else
                {
                    cliloc = 1080086 + m_Index;
                }

                from.SendLocalizedMessage(cliloc);
                Index++;
            }
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> entries)
        {
            base.GetContextMenuEntries(from, entries);

            entries.Add(new SimpleContextMenuEntry(from, 3006162, _ =>
                {
                    Index = 0;
                }, 2));
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1062703); // Spectator Vision
        }

        public UoBoard(Serial serial)
            : base(serial)
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

    public class GaryTheDungeonMaster : BaseCreature
    {
        [CommandProperty(AccessLevel.GameMaster)]
        public DungeonkeepersRoomRegion RegionProps
        {
            get => Region as DungeonkeepersRoomRegion;
            set { }
        }

        public GaryTheDungeonMaster()
            : base(AIType.AI_Vendor, FightMode.None, 10, 1, .2, .4)
        {
            Blessed = true;
            Body = 0x190;
            Name = "Gary";
            Title = "the Dungeon Master";

            SetStr(150);
            SetInt(150);
            SetDex(150);

            SetWearable(new ShortPants(), 1024);
            SetWearable(new FancyShirt(), 680);
            SetWearable(new JinBaori());
            SetWearable(new Shoes());

            HairItemId = 8253;
            FacialHairItemId = 8267;
            Hue = Race.RandomSkinHue();

            CantWalk = true;
            SpeechHue = 0x3B2;
        }

        public GaryTheDungeonMaster(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            if (version == 0)
                Delete();
        }
    }

    public class GameMaster : BaseCreature
    {
        public GameMaster()
            : base(AIType.AI_Vendor, FightMode.None, 10, 1, .2, .4)
        {
            Blessed = true;
            Body = 0x190;
            Name = "Game Master";

            SetStr(150);
            SetInt(150);
            SetDex(150);

            SetWearable(new Robe(0x204F), 0x85);

            HairItemId = 8253;
            FacialHairItemId = 8267;
            Hue = Race.RandomSkinHue();

            CantWalk = true;
            SpeechHue = 0x3B2;
        }

        public GameMaster(Serial serial)
            : base(serial)
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

            Timer.DelayCall(TimeSpan.FromSeconds(10), Delete);
        }
    }
}
