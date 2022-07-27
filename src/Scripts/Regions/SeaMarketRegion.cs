using Server.Commands;
using Server.Engines.Quests;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Server.Regions
{
    public class SeaMarketRegion : BaseRegion
    {
        private static readonly TimeSpan KickDuration = TimeSpan.FromMinutes(20);

        private static SeaMarketRegion _region1;
        private static SeaMarketRegion _region2;

        private static Timer _blabTimer;
        private static bool _restrictBoats;

        public static bool RestrictBoats
        {
            get => _restrictBoats;
            set
            {
                _restrictBoats = value;

                if (value)
                {
	                _region1?.StartTimer();

	                _region2?.StartTimer();
                }
                else
                {
	                _region1?.StopTimer();

	                _region2?.StopTimer();
                }
            }
        }

        public static Rectangle2D[] MarketBounds => m_Bounds;

        private static readonly Rectangle2D[] m_Bounds = {
            new Rectangle2D(4529, 2296, 45, 112),
        };

        private Timer m_Timer;

        private readonly Dictionary<BaseBoat, DateTime> m_BoatTable = new();

        private Dictionary<BaseBoat, DateTime> BoatTable => m_BoatTable;

        public SeaMarketRegion(XmlElement xml, Map map, Region parent)
            : base(xml, map, parent)
        {
        }

        public override void OnRegister()
        {
            if (_region1 == null)
            {
                _region1 = this;
            }
            else if (_region2 == null)
            {
                _region2 = this;
            }
        }

        public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
        {
            switch (type)
            {
                case TravelCheckType.RecallTo:
                case TravelCheckType.GateTo:
                    {
                        return BaseBoat.FindBoatAt(p, Map) != null;
                    }
                case TravelCheckType.Mark:
                    {
                        return false;
                    }
            }

            return base.CheckTravel(traveller, p, type);
        }

        public override bool AllowHousing(Mobile from, Point3D p)
        {
            return false;
        }

        #region Pirate Blabbing

        public static Dictionary<Mobile, DateTime> PirateBlabTable { get; } = new();

		private static readonly TimeSpan m_BlabDuration = TimeSpan.FromMinutes(1);

       /* public static void TryPirateBlab(Mobile from, Mobile npc)
        {
            if (m_PirateBlabTable.ContainsKey(from) && m_PirateBlabTable[from] > DateTime.UtcNow || BountyQuestSpawner.Bounties.Count <= 0)
                return;

            //Make of list of bounties on their map
            List<Mobile> bounties = new List<Mobile>();
            foreach (Mobile mob in BountyQuestSpawner.Bounties.Keys)
            {
                if (mob.Map == from.Map && mob is PirateCaptain && !bounties.Contains(mob))
                    bounties.Add(mob);
            }

            if (bounties.Count > 0)
            {
                Mobile bounty = bounties[Utility.Random(bounties.Count)];

                if (bounty != null)
                {
                    PirateCaptain capt = (PirateCaptain)bounty;

                    int xLong = 0, yLat = 0;
                    int xMins = 0, yMins = 0;
                    bool xEast = false, ySouth = false;
                    Point3D loc = capt.Location;
                    Map map = capt.Map;

                    string locArgs;
                    string combine;

                    if (Sextant.Format(loc, map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth))
                        locArgs = string.Format("{0}°{1}'{2},{3}°{4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W");
                    else
                        locArgs = "?????";

                    combine = string.Format("{0}\t{1}", capt.PirateName > -1 ? string.Format("#{0}", capt.PirateName) : capt.Name, locArgs);

                    int cliloc = Utility.RandomMinMax(1149856, 1149865);
                    npc.SayTo(from, cliloc, combine);

                    m_PirateBlabTable[from] = DateTime.UtcNow + m_BlabDuration;
                }
            }

            ColUtility.Free(bounties);
        }*/

       private static void CheckBlab_Callback()
        {
            CheckBabble(_region1);
            CheckBabble(_region2);
            CheckBabble(TokunoDocksRegion.Instance);
        }

        private static void CheckBabble(Region r)
        {
            if (r == null)
                return;

            /*foreach (Mobile player in r.AllMobiles.Where(p => p is PlayerMobile && p.Alive))
            {
                IPooledEnumerable eable = player.GetMobilesInRange(4);

                foreach (Mobile mob in eable)
                {
                    if (mob is BaseVendor || mob is MondainQuester || mob is GalleonPilot)
                    {
                        TryPirateBlab(player, mob);
                        break;
                    }
                }
                eable.Free();
            }*/
        }
        #endregion

        #region Boat Restriction

        private void StartTimer()
        {
	        m_Timer?.Stop();

            m_Timer = new InternalTimer(this);
            m_Timer.Start();
        }

        private void StopTimer()
        {
	        m_Timer?.Stop();

            m_Timer = null;
        }

        private List<BaseBoat> GetBoats()
        {
            List<BaseBoat> list = new();

            foreach (BaseBoat boat in AllMultis.OfType<BaseBoat>())
                list.Add(boat);

            return list;
        }

        private void OnTick()
        {
            if (!_restrictBoats)
            {
                StopTimer();
                return;
            }

            List<BaseBoat> boats = GetBoats();
            List<BaseBoat> toRemove = new();

            foreach (var (boat, moveBy) in m_BoatTable)
            {
	            if (boat == null || !boats.Contains(boat) || boat.Deleted)
                    toRemove.Add(boat);
                else if (DateTime.UtcNow >= moveBy && KickBoat(boat))
                    toRemove.Add(boat);
                else
	            {
		            if (boat.Owner is not { NetState: { } })
			            continue;
		            TimeSpan ts = moveBy - DateTime.UtcNow;

		            if ((int)ts.TotalMinutes > 10)
			            continue;
		            var rem = Math.Max(1, (int)ts.TotalMinutes);
		            boat.Owner.SendLocalizedMessage(1149787 + (rem - 1));
	            }
            }

            foreach (var boat in boats.Where(boat => !m_BoatTable.ContainsKey(boat) && !boat.IsMoving && boat.Owner is
                     {
	                     AccessLevel: < AccessLevel.Counselor
                     }))
            {
	            AddToTable(boat);
            }

            foreach (BaseBoat b in toRemove)
                m_BoatTable.Remove(b);

            ColUtility.Free(toRemove);
            ColUtility.Free(boats);
        }

        private void AddToTable(BaseBoat boat)
        {
            if (m_BoatTable.ContainsKey(boat))
                return;

            m_BoatTable.Add(boat, DateTime.UtcNow + KickDuration);

            if (boat.Owner is { NetState: { } })
                boat.Owner.SendMessage("You can only dock your boat here for {0} minutes.", (int)KickDuration.TotalMinutes);
        }

        private readonly Rectangle2D[] m_KickLocs = {
            new(m_Bounds[0].X - 100, m_Bounds[0].X - 100, 200 + m_Bounds[0].Width, 100),
            new(m_Bounds[0].X - 100, m_Bounds[0].Y, 100, m_Bounds[0].Height + 100),
            new(m_Bounds[0].X, m_Bounds[0].Y + m_Bounds[0].Height, m_Bounds[0].Width + 100, 100),
            new(m_Bounds[0].X + m_Bounds[0].Width, m_Bounds[0].Y, 100, m_Bounds[0].Height),
        };

        private bool KickBoat(BaseBoat boat)
        {
            if (boat == null || boat.Deleted)
                return false;

            for (var i = 0; i < 25; i++)
            {
                Rectangle2D rec = m_KickLocs[Utility.Random(m_KickLocs.Length)];

                var x = Utility.RandomMinMax(rec.X, rec.X + rec.Width);
                var y = Utility.RandomMinMax(rec.Y, rec.Y + rec.Height);
                var z = boat.Z;

                Point3D p = new(x, y, z);

                if (!boat.CanFit(p, boat.Map, boat.ItemId))
	                continue;

                boat.Teleport(x - boat.X, y - boat.Y, z - boat.Z);

                //if (boat.Owner != null && boat.Owner.NetState != null)
                //    boat.SendMessageToAllOnBoard(1149785); //A strong tide comes and carries your boat to deeper water.
                return true;
            }
            return false;
        }

        private class InternalTimer : Timer
        {
            private readonly SeaMarketRegion m_Region;

            public InternalTimer(SeaMarketRegion reg)
                : base(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))
            {
                m_Region = reg;
            }

            protected override void OnTick()
            {
	            m_Region?.OnTick();
            }
        }

        private static void StartTimers_Callback()
        {
            RestrictBoats = _restrictBoats;

            _blabTimer = Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), CheckBlab_Callback);
            _blabTimer.Priority = TimerPriority.OneSecond;
        }
        #endregion

        public static void Save(GenericWriter writer)
        {
            writer.Write(0);

            writer.Write(_restrictBoats);
        }

        public static void Load(GenericReader reader)
        {
            reader.ReadInt();

            _restrictBoats = reader.ReadBool();

            Timer.DelayCall(TimeSpan.FromSeconds(30), StartTimers_Callback);
        }

        public static void GetBoatInfo_OnCommand(CommandEventArgs e)
        {
            List<BaseBoat> boats = new List<BaseBoat>(_region1.BoatTable.Keys);
            List<DateTime> times = new List<DateTime>(_region1.BoatTable.Values);

            e.Mobile.SendMessage("========Boat Info for Felucca as Follows===========");
            e.Mobile.SendMessage("Boats: {0}", boats.Count);

            if (!_restrictBoats)
                e.Mobile.SendMessage("Boat restriction is Currently disabled.");

            Console.WriteLine("========Boat Info as Follows===========");
            Console.WriteLine("Boats: {0}", boats.Count);

            if (!_restrictBoats)
                Console.WriteLine("Boat restriction is Currently disabled.");

            for (int i = 0; i < boats.Count; i++)
            {
                BaseBoat boat = boats[i];

                if (boat == null || boat.Deleted)
                    continue;

                e.Mobile.SendMessage("Boat Name: {0}; Boat Owner: {1}; Expires: {2}", boat.ShipName, boat.Owner, times[i]);

                Console.WriteLine("Boat Name: {0}; Boat Owner: {1}; Expires: {2}", boat.ShipName, boat.Owner, times[i]);
            }

            boats.Clear();
            times.Clear();

            boats = new List<BaseBoat>(_region2.BoatTable.Keys);
            times = new List<DateTime>(_region2.BoatTable.Values);

            e.Mobile.SendMessage("========Boat Info for Trammel as Follows===========");
            e.Mobile.SendMessage("Boats: {0}", boats.Count);

            if (!_restrictBoats)
                e.Mobile.SendMessage("Boat restriction is Currently disabled.");

            Console.WriteLine("========Boat Info as Follows===========");
            Console.WriteLine("Boats: {0}", boats.Count);

            if (!_restrictBoats)
                Console.WriteLine("Boat restriction is Currently disabled.");

            for (int i = 0; i < boats.Count; i++)
            {
                BaseBoat boat = boats[i];

                if (boat == null || boat.Deleted)
                    continue;

                e.Mobile.SendMessage("Boat Name: {0}; Boat Owner: {1}; Expires: {2}", boat.ShipName, boat.Owner, times[i]);

                Console.WriteLine("Boat Name: {0}; Boat Owner: {1}; Expires: {2}", boat.ShipName, boat.Owner, times[i]);
            }
        }

        public static void SetRestriction_OnCommand(CommandEventArgs e)
        {
            if (_restrictBoats)
            {
                RestrictBoats = false;

                e.Mobile.SendMessage("Boat restriction in the sea market is no longer active.");
            }
            else
            {
                RestrictBoats = true;

                e.Mobile.SendMessage("Boat restriction in the sea market is now active.");
            }
        }
    }
}
