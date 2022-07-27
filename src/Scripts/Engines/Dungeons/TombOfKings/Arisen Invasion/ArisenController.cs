using Server.Commands;
using Server.Mobiles;
using System;

namespace Server.Items
{
    public class ArisenController : Item
    {
	    private const int SpawnHour = 0; // Midnight
	    private const int DespawnHour = 4; // 4 AM

	    private class ArisenEntry
        {
	        private readonly TimeSpan m_MinDelay;
            private readonly TimeSpan m_MaxDelay;

            private Map Map { get; }

            private Point3D Location { get; }

            private string Creature { get; }

            private int Amount { get; }

            private int HomeRange { get; }

            private int SpawnRange { get; }

            public TimeSpan MinDelay => m_MinDelay;
            public TimeSpan MaxDelay => m_MaxDelay;

            public ArisenEntry(Map map, Point3D location, string creature, int amount, int homeRange, int spawnRange, TimeSpan minDelay, TimeSpan maxDelay)
            {
                Map = map;
                Location = location;
                Creature = creature;
                Amount = amount;
                HomeRange = homeRange;
                SpawnRange = spawnRange;
                m_MinDelay = minDelay;
                m_MaxDelay = maxDelay;
            }

            public XmlSpawner CreateSpawner()
            {
                XmlSpawner spawner = new(Amount, (int)m_MinDelay.TotalSeconds, (int)m_MaxDelay.TotalSeconds, 0, 20, 10, Creature);

                spawner.MoveToWorld(Location, Map);

                return spawner;
            }
        }

	    private static ArisenEntry[] Entries { get; } =
        {
	        new( Map.TerMur, new Point3D( 996, 3862, -42 ), "EffeteUndeadGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 15.0 ), TimeSpan.FromSeconds( 30.0 ) ),
	        new( Map.TerMur, new Point3D( 996, 3863, -42 ), "EffetePutridGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 30.0 ), TimeSpan.FromSeconds( 60.0 ) ),
	        new( Map.TerMur, new Point3D( 996, 3864, -42 ), "GargoyleShade",        2, 15, 10, TimeSpan.FromSeconds( 60.0 ), TimeSpan.FromSeconds( 90.0 ) ),

	        new( Map.TerMur, new Point3D( 996, 3892, -42 ), "EffeteUndeadGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 15.0 ), TimeSpan.FromSeconds( 30.0 ) ),
	        new( Map.TerMur, new Point3D( 996, 3893, -42 ), "EffetePutridGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 30.0 ), TimeSpan.FromSeconds( 60.0 ) ),

	        new( Map.TerMur, new Point3D( 996, 3917, -42 ), "EffeteUndeadGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 15.0 ), TimeSpan.FromSeconds( 30.0 ) ),
	        new( Map.TerMur, new Point3D( 996, 3918, -42 ), "EffetePutridGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 30.0 ), TimeSpan.FromSeconds( 60.0 ) ),

	        new( Map.TerMur, new Point3D( 996, 3951, -42 ), "EffeteUndeadGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 15.0 ), TimeSpan.FromSeconds( 30.0 ) ),
	        new( Map.TerMur, new Point3D( 996, 3952, -42 ), "EffetePutridGargoyle", 5, 20, 15, TimeSpan.FromSeconds( 30.0 ), TimeSpan.FromSeconds( 60.0 ) ),
	        new( Map.TerMur, new Point3D( 997, 3951, -42 ), "GargoyleShade",        2, 15, 10, TimeSpan.FromSeconds( 60.0 ), TimeSpan.FromSeconds( 90.0 ) ),
	        new( Map.TerMur, new Point3D( 997, 3951, -42 ), "PutridUndeadGargoyle", 1, 10,  5, TimeSpan.FromMinutes( 5.0 ),  TimeSpan.FromMinutes( 10.0 ) )
        };

	    private static ArisenController Instance { get; set; }

	    public static bool Create()
        {
            if (Instance is { Deleted: false })
                return false;

            Instance = new ArisenController();
            return true;
        }

        public static bool Remove()
        {
            if (Instance == null)
                return false;

            Instance.Delete();
            Instance = null;

            return true;
        }

        private InternalTimer m_SpawnTimer;
        private XmlSpawner[] m_Spawners;
        private bool m_Spawned;

        [CommandProperty(AccessLevel.Seer)] private bool ForceDeactivate { get; set; }

        private ArisenController()
            : base(1)
        {
            Name = "Arisen Controller - Internal";
            Movable = false;

            m_Spawners = new XmlSpawner[Entries.Length];

            for (int i = 0; i < Entries.Length; i++)
            {
                m_Spawners[i] = Entries[i].CreateSpawner();
                m_Spawners[i].SmartSpawning = true;
            }

            m_SpawnTimer = new InternalTimer(this);
            m_SpawnTimer.Start();
        }

        public override void OnDelete()
        {
            base.OnDelete();

            if (m_SpawnTimer != null)
            {
                m_SpawnTimer.Stop();
                m_SpawnTimer = null;
            }

            foreach (XmlSpawner spawner in m_Spawners)
                spawner.Delete();

            Instance = null;
        }

        private void OnTick()
        {
            // check time

            Clock.GetTime(Map.TerMur, 997, 3869, out var hours, out int _); // Holy City

            m_Spawned = hours is >= SpawnHour and < DespawnHour && !ForceDeactivate;

            foreach (XmlSpawner spawner in m_Spawners)
            {
                if (!m_Spawned)
                {
                    spawner.Reset();
                }
                else
                {
                    if (!spawner.Running)
                    {
                        spawner.Respawn();
                    }
                }
            }
        }

        private class InternalTimer : Timer
        {
            private readonly ArisenController m_Controller;

            public InternalTimer(ArisenController controller)
                : base(TimeSpan.Zero, TimeSpan.FromSeconds(5.0))
            {

                m_Controller = controller;
            }

            protected override void OnTick()
            {
                m_Controller.OnTick();
            }
        }

        public ArisenController(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(ForceDeactivate);

            writer.WriteEncodedInt(m_Spawners.Length);

            for (int i = 0; i < m_Spawners.Length; i++)
                writer.Write(m_Spawners[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            switch (version)
            {
                case 0:
                    {
                        ForceDeactivate = reader.ReadBool();
                        int length = reader.ReadEncodedInt();

                        m_Spawners = new XmlSpawner[length];

                        for (int i = 0; i < length; i++)
                        {
                            XmlSpawner spawner = reader.ReadItem<XmlSpawner>();

                            if (spawner == null)
                            {
                                spawner = Entries[i].CreateSpawner();
                                spawner.SmartSpawning = true;
                            }

                            m_Spawners[i] = spawner;
                        }

                        break;
                    }
            }

            Instance = this;

            m_SpawnTimer = new InternalTimer(this);
            m_SpawnTimer.Start();
        }
    }
}
