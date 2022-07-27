using Server.Items;
using Server.Mobiles;
using Server.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Server.Engines.Doom
{
    public class DoomGuardianRegion : DungeonRegion
    {
	    private static DoomGuardianRegion Instance { get; set; }

        public static void Initialize()
        {
            Instance.CheckDoors();
        }

        private Timer m_Timer;

        private bool Active { get; set; }
        private List<DarkGuardian> Guardians { get; set; }
        private BaseDoor DoorOne { get; set; }
        private BaseDoor DoorTwo { get; set; }
        private DateTime NextActivate { get; set; }

        private bool CanActivate => NextActivate < DateTime.UtcNow;

        private static readonly Rectangle2D[] RegionBounds = { new(355, 5, 20, 20) };
        private static Rectangle2D _pentagramBounds = new(364, 14, 2, 2);
        private static readonly Point3D DoorOneLoc = new(355, 14, -1);
        private static readonly Point3D DoorTwoLoc = new(355, 15, -1);
        private static readonly Point3D KickLoc = new(344, 172, -1);
        private static readonly Point3D PentagramLoc = new(365, 15, -1);

        public DoomGuardianRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
            Instance = this;
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        public override void OnLocationChanged(Mobile m, Point3D oldLocation)
        {
            base.OnLocationChanged(m, oldLocation);

            if (!Active && CanActivate && m is PlayerMobile && m.AccessLevel < AccessLevel.Counselor && m.Alive)
            {
                for (int x = m.X - 3; x <= m.X + 3; x++)
                {
                    for (int y = m.Y - 3; y <= m.Y + 3; y++)
                    {
                        if (!Active && _pentagramBounds.Contains(new Point2D(x, y)))
                        {
                            Activate(m);
                            Active = true;
                            return;
                        }
                    }
                }
            }
        }

        private bool CheckReset()
        {
	        if (Guardians != null && Guardians.Count != 0 && !Guardians.All(x => x.Deleted) &&
	            PlayerCount != 0) return false;
	        Reset();
            return true;

        }

        public override void OnDeath(Mobile m)
        {
            if (Guardians != null && m is DarkGuardian guardian && Guardians.Contains(guardian))
            {
                Guardians.Remove(guardian);
            }

            if (m is PlayerMobile mobile && Active)
            {
                Timer.DelayCall(TimeSpan.FromSeconds(3), MoveDeadPlayer, mobile);
            }
        }

        private void MoveDeadPlayer(PlayerMobile pm)
        {
            if (pm.Region == this)
            {
                BaseCreature.TeleportPets(pm, KickLoc, Map.Malas);
                pm.MoveToWorld(KickLoc, Map.Malas);

                pm.Corpse?.MoveToWorld(KickLoc, Map.Malas);
            }

            if (!AllPlayers.Any(mob => mob.Alive))
            {
                Reset();
            }
        }

        private void Activate(IDamageable m)
        {
            if (Active)
                return;

            CheckDoors();

            DoorOne.Open = false;
            DoorTwo.Open = false;
            DoorOne.Locked = true;
            DoorTwo.Locked = true;

            Effects.PlaySound(DoorOne.Location, DoorOne.Map, 0x241);
            Effects.PlaySound(DoorTwo.Location, DoorTwo.Map, 0x241);

            Guardians ??= new List<DarkGuardian>();

            int count = 0;
            foreach (Mobile mob in AllMobiles.Where(mob => mob is PlayerMobile || (mob is BaseCreature creature && creature.GetMaster() != null && !creature.IsDeadBondedPet)))
            {
                if (mob.NetState != null)
                    mob.SendLocalizedMessage(1050000, "", 365); // The locks on the door click loudly and you begin to hear a faint hissing near the walls.

                if (mob.Alive)
                    count++;
            }

            count = Math.Max(1, count * 2);

            for (int i = 0; i < count; i++)
            {
                DarkGuardian guardian = new();

                int x = Utility.RandomMinMax(_pentagramBounds.X, _pentagramBounds.X + _pentagramBounds.Width);
                int y = Utility.RandomMinMax(_pentagramBounds.Y, _pentagramBounds.Y + _pentagramBounds.Height);
                int z = Map.Malas.GetAverageZ(x, y);

                guardian.MoveToWorld(new Point3D(x, y, z), Map.Malas);
                Guardians.Add(guardian);

                guardian.Combatant = m;
            }

            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        private void Reset()
        {
            if (!Active)
                return;

            if (m_Timer != null)
            {
                m_Timer.Stop();
                m_Timer = null;
            }

            DoorOne.Locked = false;
            DoorTwo.Locked = false;

            Active = false;
            NextActivate = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(15, 60));
        }

        private void CheckDoors()
        {
            if (DoorOne == null || DoorOne.Deleted)
            {
                if (!CheckDoor(DoorOneLoc, 1))
                {
                    DoorOne = new MetalDoor2(DoorFacing.NorthCw);
                    DoorOne.MoveToWorld(DoorOneLoc, Map.Malas);
                    DoorOne.KeyValue = 0;
                }
            }

            if (DoorTwo == null || DoorTwo.Deleted)
            {
                if (!CheckDoor(DoorTwoLoc, 2))
                {
                    DoorTwo = new MetalDoor2(DoorFacing.SouthCcw);
                    DoorTwo.MoveToWorld(DoorTwoLoc, Map.Malas);
                    DoorTwo.KeyValue = 0;
                }
            }

            if (DoorOne != null && DoorOne.Link != DoorTwo)
                DoorOne.Link = DoorTwo;

            if (DoorTwo != null && DoorTwo.Link != DoorOne)
                DoorTwo.Link = DoorOne;

            CheckPentagram();
        }

        private bool CheckDoor(Point3D p, int door)
        {
            IPooledEnumerable eable = Map.Malas.GetItemsInRange(p, 0);

            foreach (Item item in eable)
            {
	            if (item is not BaseDoor baseDoor)
		            continue;

	            eable.Free();

	            if (door == 1)
		            DoorOne = baseDoor;
	            else
		            DoorTwo = baseDoor;

	            return true;
            }

            eable.Free();
            return false;
        }

        private static void CheckPentagram()
        {
            IPooledEnumerable eable = Map.Malas.GetItemsInRange(PentagramLoc, 0);

            foreach (Item item in eable)
            {
                if (item is PentagramAddon)
                {
                    eable.Free();
                    return;
                }
            }

            eable.Free();

            PentagramAddon addon = new();
            addon.MoveToWorld(PentagramLoc, Map.Malas);
        }

        private class InternalTimer : Timer
        {
	        private DoomGuardianRegion Region { get; }
	        private DateTime NextGas { get; set; }

            public InternalTimer(DoomGuardianRegion reg)
                : base(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500))
            {
                Region = reg;
                NextGas = DateTime.UtcNow + TimeSpan.FromSeconds(3);
            }

            protected override void OnTick()
            {
                if (Region.CheckReset())
                {
                    Region.Reset();
                }
                else if (NextGas < DateTime.UtcNow)
                {
                    for (int i = 0; i < Utility.RandomMinMax(5, 12); i++)
                    {
                        Point3D p = Region.RandomSpawnLocation(0, true, false, Point3D.Zero, 0);
                        Effects.SendLocationEffect(p, Map.Malas, Utility.RandomList(0x113C, 0x1147, 0x11A8) - 2, 16, 3);
                    }

                    foreach (Mobile m in Region.AllMobiles.Where(m => m is PlayerMobile && m.Alive && m.AccessLevel < AccessLevel.Counselor && m.Poison == null))
                    {
                        m.ApplyPoison(m, Poison.Deadly);
                        m.SendSound(0x231);
                    }

                    NextGas = DateTime.UtcNow + TimeSpan.FromSeconds(3);
                }
            }
        }
    }
}
