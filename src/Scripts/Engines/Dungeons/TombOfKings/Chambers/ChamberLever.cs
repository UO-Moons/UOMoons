using System.Collections.Generic;

namespace Server.Engines.TombOfKings
{
    public class ChamberLever : Item
    {
        public static void Generate()
        {
            foreach (Point3D loc in m_LeverLocations)
            {
                ChamberLever item = new(loc);

                Levers.Add(item);
            }
        }

        private static readonly Point3D[] m_LeverLocations = {
            new( 25, 229, 2 ),
            new( 25, 227, 2 ),
            new( 25, 225, 2 ),

            new( 25, 221, 2 ),
            new( 25, 219, 2 ),
            new( 25, 217, 2 ),

            new( 45, 229, 2 ),
            new( 45, 227, 2 ),
            new( 45, 225, 2 ),

            new( 45, 221, 2 ),
            new( 45, 219, 2 ),
            new( 45, 217, 2 ),
        };

        public static List<ChamberLever> Levers { get; } = new();

        private Chamber m_Chamber;

        public Chamber Chamber
        {
            get => m_Chamber;
            set
            {
                m_Chamber = value;
                InvalidateProperties();
            }
        }

        private bool IsUsable()
        {
            if (m_Chamber == null)
                return false;

            return !m_Chamber.IsOpened();
        }

        private ChamberLever(Point3D loc)
            : base(Utility.RandomBool() ? 0x108C : 0x108E)
        {
            Movable = false;
            MoveToWorld(loc, Map.TerMur);
        }

        public ChamberLever(Serial serial)
            : base(serial)
        {
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
	        list.Add(IsUsable() ? 1112130 : 1112129);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (IsUsable() && from.InRange(this, 1))
                m_Chamber.Open();
        }

        public void Switch()
        {
            ItemId = ItemId == 0x108C ? 0x108E : 0x108C;

            Effects.PlaySound(Location, Map, 0x3E8);

            InvalidateProperties();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            Levers.Add(this);
        }
    }
}
