using System;
using System.Collections.Generic;

namespace Server.Engines.TombOfKings
{
    public class ChamberInfo
    {
	    public Point3D BarrierLocation { get; }

        public Point3D SwitchLocation { get; }

        public int SwitchId { get; }

        public ChamberInfo(Point3D barrierLoc, Point3D switchLoc, int switchId)
        {
            BarrierLocation = barrierLoc;
            SwitchLocation = switchLoc;
            SwitchId = switchId;
        }
    }

    public class Chamber
    {
        public static void Initialize()
        {
            // we should call it after deserialize the levers
            Timer.DelayCall(TimeSpan.Zero, Generate);
        }

        public static void Generate()
        {
            if (ChamberLever.Levers.Count == 0)
                return;

            foreach (ChamberInfo info in m_ChamberInfos)
                Chambers.Add(new Chamber(info));

            // randomize
            List<ChamberLever> levers = new(ChamberLever.Levers);

            foreach (Chamber chamber in Chambers)
            {
                int idx = Utility.Random(levers.Count);

                chamber.Lever = levers[idx];
                levers[idx].Chamber = chamber;
                levers.RemoveAt(idx);
            }
        }

        private static List<Chamber> Chambers { get; } = new();

        private static readonly ChamberInfo[] m_ChamberInfos = {
			// left side
			new( new Point3D( 15, 200, -5 ), new Point3D( 13, 195, 7 ), 0x1091 ),
            new( new Point3D( 15, 184, -5 ), new Point3D( 13, 179, 7 ), 0x1091 ),
            new( new Point3D( 15, 168, -5 ), new Point3D( 13, 163, 7 ), 0x1091 ),
            new( new Point3D( 15, 152, -5 ), new Point3D( 13, 147, 7 ), 0x1091 ),
            new( new Point3D( 15, 136, -5 ), new Point3D( 13, 131, 7 ), 0x1091 ),
            new( new Point3D( 15, 120, -5 ), new Point3D( 13, 115, 7 ), 0x1091 ),

			// right side
			new( new Point3D( 55, 200, -5 ), new Point3D( 56, 197, 7 ), 0x1090 ),
            new( new Point3D( 55, 184, -5 ), new Point3D( 56, 181, 7 ), 0x1090 ),
            new( new Point3D( 55, 168, -5 ), new Point3D( 56, 165, 7 ), 0x1090 ),
            new( new Point3D( 55, 152, -5 ), new Point3D( 56, 149, 7 ), 0x1090 ),
            new( new Point3D( 55, 136, -5 ), new Point3D( 56, 133, 7 ), 0x1090 ),
            new( new Point3D( 55, 120, -5 ), new Point3D( 56, 117, 7 ), 0x1090 ),
        };

        private ChamberSwitch Switch { get; }

        private ChamberBarrier Barrier { get; }

        private ChamberLever Lever { get; set; }

        public bool IsOpened()
        {
            return !Barrier.Active;
        }

        public void Open()
        {
            Barrier.Active = false;
            Lever.Switch();

            Timer.DelayCall(TimeSpan.FromMinutes(Utility.RandomMinMax(10, 15)), RestoreBarrier);
        }

        private void RestoreBarrier()
        {
            Barrier.Active = true;
            Lever.InvalidateProperties();
        }

        private Chamber(ChamberInfo info)
        {
            Switch = new ChamberSwitch(this, info.SwitchLocation, info.SwitchId);
            Barrier = new ChamberBarrier(info.BarrierLocation);
        }
    }
}
