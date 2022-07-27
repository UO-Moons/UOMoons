using Server.Commands;
using Server.Gumps;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.MiniChamps
{
    public class MiniChamp : Item
    {
	    private static readonly List<MiniChamp> Controllers = new();
        private const string TimerId = "MiniChampTimer";
        private const string RestartTimerId = "MiniChampRestartTimer";

        [Usage("GenMiniChamp")]
        [Description("MiniChampion Generator")]
        public static void GenStoneRuins_OnCommand(CommandEventArgs e)
        {
            foreach (MiniChamp controller in Controllers)
            {
                controller.Delete();
            }

            Map map = Map.TerMur;

            MiniChamp miniChamp = new MiniChamp
            {
                Type = MiniChampType.CrimsonVeins
            };
            miniChamp.MoveToWorld(new Point3D(974, 161, -10), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.AbyssalLair
            };
            miniChamp.MoveToWorld(new Point3D(987, 328, 11), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.DiscardedCavernClanRibbon
            };
            miniChamp.MoveToWorld(new Point3D(915, 501, -11), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.DiscardedCavernClanScratch
            };
            miniChamp.MoveToWorld(new Point3D(950, 552, -13), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.DiscardedCavernClanChitter
            };
            miniChamp.MoveToWorld(new Point3D(980, 491, -11), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.EnslavedGoblins
            };
            miniChamp.MoveToWorld(new Point3D(578, 799, -45), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.FairyDragonLair
            };
            miniChamp.MoveToWorld(new Point3D(887, 273, 4), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.FireTemple
            };
            miniChamp.MoveToWorld(new Point3D(546, 760, -91), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.LandsoftheLich
            };
            miniChamp.MoveToWorld(new Point3D(530, 658, 9), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.LavaCaldera
            };
            miniChamp.MoveToWorld(new Point3D(578, 900, -72), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.PassageofTears
            };
            miniChamp.MoveToWorld(new Point3D(684, 579, -14), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.SecretGarden
            };
            miniChamp.MoveToWorld(new Point3D(434, 701, 29), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                Type = MiniChampType.SkeletalDragon
            };
            miniChamp.MoveToWorld(new Point3D(677, 824, -108), map);
            miniChamp.Active = true;

            miniChamp = new MiniChamp
            {
                BossSpawnPoint = new Point3D(384, 1931, 50),
                Type = MiniChampType.MeraktusTheTormented
            };
            miniChamp.MoveToWorld(new Point3D(395, 1913, 12), Map.Malas);
            miniChamp.Active = true;

            e.Mobile.SendMessage("Created Mini Champion Spawns.");
        }

        private bool m_Active;
        private MiniChampType m_Type;
        private List<MiniChampSpawnInfo> m_Spawn;
        private List<Mobile> m_Despawns;
        private int m_Level;
        private int m_SpawnRange;

        [CommandProperty(AccessLevel.GameMaster)]
        public Point3D BossSpawnPoint { get; private set; }

        [Constructable]
        public MiniChamp()
            : base(0xBD2)
        {
            Movable = false;
            Visible = false;
            Name = "Mini Champion Controller";

            m_Despawns = new List<Mobile>();
            m_Spawn = new List<MiniChampSpawnInfo>();
            RestartDelay = TimeSpan.FromMinutes(5.0);
            m_SpawnRange = 30;
            BossSpawnPoint = Point3D.Zero;

            Controllers.Add(this);
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int SpawnRange
        {
            get => m_SpawnRange;
            set { m_SpawnRange = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public TimeSpan RestartDelay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public MiniChampType Type
        {
            get => m_Type;
            private init { m_Type = value; InvalidateProperties(); }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public bool Active
        {
            get => m_Active;
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int Level
        {
            get => m_Level;
            set { m_Level = value; InvalidateProperties(); }
        }

        public void Start()
        {
            if (m_Active || Deleted)
                return;

            m_Active = true;

            StartTimer();

            AdvanceLevel();
            InvalidateProperties();
        }

        public void Stop()
        {
            if (!m_Active || Deleted)
                return;

            m_Active = false;
            m_Level = 0;

            ClearSpawn();
            Despawn();

            TimerRegistry.RemoveFromRegistry(TimerId, this);
            TimerRegistry.RemoveFromRegistry(RestartTimerId, this);

            InvalidateProperties();
        }

        private void StartTimer()
        {
            TimerRegistry.Register(TimerId, this, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), false, spawner => spawner.OnSlice());
        }

        private void StartRestartTimer()
        {
            TimerRegistry.Register(RestartTimerId, this, RestartDelay, spawner => spawner.Start());
        }

        public void Despawn()
        {
            foreach (Mobile toDespawn in m_Despawns)
            {
                toDespawn.Delete();
            }

            m_Despawns.Clear();
        }

        public void OnSlice()
        {
            if (!m_Active || Deleted)
                return;

            bool changed = false;
            bool done = true;

            foreach (MiniChampSpawnInfo spawn in m_Spawn)
            {
                if (spawn.Slice() && !changed)
                {
                    changed = true;
                }

                if (!spawn.Done && done)
                {
                    done = false;
                }
            }

            if (done)
            {
                AdvanceLevel();
            }

            if (m_Active)
            {
	            foreach (var spawn in m_Spawn.Where(spawn => spawn.Respawn() && !changed))
	            {
		            changed = true;
	            }
            }

            if (done || changed)
            {
                InvalidateProperties();
            }
        }

        public void ClearSpawn()
        {
	        foreach (var creature in m_Spawn.SelectMany(spawn => spawn.Creatures))
	        {
		        m_Despawns.Add(creature);
	        }

	        m_Spawn.Clear();
        }

        public void AdvanceLevel()
        {
            Level++;

            MiniChampInfo info = MiniChampInfo.GetInfo(m_Type);
            MiniChampLevelInfo levelInfo = info.GetLevelInfo(Level);

            if (levelInfo != null && Level <= info.MaxLevel)
            {
	            ClearSpawn();

	            if (m_Type == MiniChampType.MeraktusTheTormented)
	            {
		            MinotaurShouts();
	            }

	            foreach (MiniChampTypeInfo type in levelInfo.Types)
	            {
		            m_Spawn.Add(new MiniChampSpawnInfo(this, type));
	            }

            }
            else // begin restart
            {
                Stop();

                StartRestartTimer();
            }
        }

        private void MinotaurShouts()
        {
            var cliloc = 0;

            switch (Level)
            {
                case 1:
                    return;
                case 2:
                    cliloc = 1073370;
                    break;
                case 3:
                    cliloc = 1073367;
                    break;
                case 4:
                    cliloc = 1073368;
                    break;
                case 5:
                    cliloc = 1073369;
                    break;
            }

            IPooledEnumerable eable = GetMobilesInRange(m_SpawnRange);

            foreach (Mobile x in eable)
            {
                if (x is PlayerMobile)
                    x.SendLocalizedMessage(cliloc);
            }

            eable.Free();
        }

        public override void GetProperties(ObjectPropertyList list)
        {
            base.GetProperties(list);

            list.Add(1060658, "Type\t{0}", m_Type); // ~1_val~: ~2_val~
            list.Add(1060661, "Spawn Range\t{0}", m_SpawnRange); // ~1_val~: ~2_val~

            if (m_Active)
            {
                MiniChampInfo info = MiniChampInfo.GetInfo(m_Type);

                list.Add(1060742); // active
                list.Add("Level {0} / {1}", Level, info != null ? info.MaxLevel.ToString() : "???"); // ~1_val~: ~2_val~

                for (var i = 0; i < m_Spawn.Count; i++)
                {
                    m_Spawn[i].AddProperties(list, i + 1150301);
                }
            }
            else
            {
                list.Add(1060743); // inactive
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            from.SendGump(new PropertiesGump(from, this));
        }

        public override void OnDelete()
        {
            Controllers.Remove(this);
            Stop();

            base.OnDelete();
        }

        public MiniChamp(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version

            writer.Write(BossSpawnPoint);
            writer.Write(m_Active);
            writer.Write((int)m_Type);
            writer.Write(m_Level);
            writer.Write(m_SpawnRange);
            writer.Write(RestartDelay);

            writer.Write(m_Spawn.Count);

            for (var i = 0; i < m_Spawn.Count; i++)
            {
                m_Spawn[i].Serialize(writer);
            }

            writer.Write(m_Despawns, true);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    {
                        m_Spawn = new List<MiniChampSpawnInfo>();

                        BossSpawnPoint = reader.ReadPoint3D();
                        m_Active = reader.ReadBool();
                        m_Type = (MiniChampType)reader.ReadInt();
                        m_Level = reader.ReadInt();
                        m_SpawnRange = reader.ReadInt();
                        RestartDelay = reader.ReadTimeSpan();

                        var spawnCount = reader.ReadInt();

                        for (var i = 0; i < spawnCount; i++)
                        {
                            m_Spawn.Add(new MiniChampSpawnInfo(reader));
                        }

                        m_Despawns = reader.ReadStrongMobileList();

                        if (m_Active)
                        {
                            StartTimer();
                        }
                        else
                        {
                            StartRestartTimer();
                        }

                        break;
                    }
            }

            Controllers.Add(this);
        }
    }
}
