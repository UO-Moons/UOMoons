using Server.Items;

namespace Server.Engines.TombOfKings
{
    public class ChamberBarrier : Item
    {
        private Blocker m_Blocker;
        private LOSBlocker m_LosBlocker;

        public bool Active
        {
            get => Visible;
            set
            {
                if (Visible != value)
                {
                    if (Visible == value)
                    {
                        m_Blocker = new Blocker();
                        m_Blocker.MoveToWorld(Location, Map);

                        m_LosBlocker = new LOSBlocker();
                        m_LosBlocker.MoveToWorld(Location, Map);
                    }
                    else
                    {
                        m_Blocker.Delete();
                        m_Blocker = null;

                        m_LosBlocker.Delete();
                        m_LosBlocker = null;
                    }
                }
            }
        }

        public ChamberBarrier(Point3D loc)
            : base(0x3979)
        {
            Light = LightType.Circle150;

            Movable = false;
            MoveToWorld(loc, Map.TerMur);

            m_Blocker = new Blocker();
            m_Blocker.MoveToWorld(loc, Map.TerMur);

            m_LosBlocker = new LOSBlocker();
            m_LosBlocker.MoveToWorld(loc, Map.TerMur);
        }

        public ChamberBarrier(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Blocker != null);

            if (m_Blocker != null)
                writer.Write(m_Blocker);

            writer.Write(m_LosBlocker != null);

            if (m_LosBlocker != null)
                writer.Write(m_LosBlocker);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            if (reader.ReadBool())
            {
                m_Blocker = reader.ReadItem() as Blocker;

                if (m_Blocker != null)
                {
                    m_Blocker.Delete();
                }
            }

            if (reader.ReadBool())
            {
                m_LosBlocker = reader.ReadItem() as LOSBlocker;

                if (m_LosBlocker != null)
                {
                    m_LosBlocker.Delete();
                }
            }

            Delete();
        }
    }
}
