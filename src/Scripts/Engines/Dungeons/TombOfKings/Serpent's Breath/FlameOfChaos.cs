using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class FlameOfChaos : BaseItem
{
        public override int LabelNumber => 1112128;  // Flame of Chaos

        private List<FireBarrier> m_Barriers;

        [Constructable]
        public FlameOfChaos(Point3D location, Map map)
            : base(0x19AB)
        {
            Movable = false;
            Light = LightType.Circle225;

            MoveToWorld(location, map);

            m_Barriers = new List<FireBarrier>(m_BarrierLocations.Length);

            for (int i = 0; i < m_BarrierLocations.Length; i++)
            {
                m_Barriers.Add(new FireBarrier(m_BarrierLocations[i], map));
            }
        }

        public override bool HandlesOnSpeech => true;

        public override void OnSpeech(SpeechEventArgs e)
        {
            string mantra = e.Speech.ToLower();

            if (!Visible || !e.Mobile.InRange(this, 2) || (mantra != "an-ord" && mantra != "anord")) return;
            Visible = false;

            foreach (FireBarrier barrier in m_Barriers)
            {
	            barrier.Active = false;
            }

            Timer.DelayCall(TimeSpan.FromMinutes(2.0), RestoreBarrier);
        }

        private void RestoreBarrier()
        {
            foreach (FireBarrier barrier in m_Barriers)
            {
                barrier.Active = true;
            }

            Visible = true;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            foreach (FireBarrier barrier in m_Barriers)
            {
                barrier.Delete();
            }
        }

        private static readonly Point3D[] m_BarrierLocations = {
            new( 33, 207, 0 ),
            new( 34, 207, 0 ),
            new( 35, 207, 0 ),
            new( 36, 207, 0 ),
            new( 37, 207, 0 ),

            new( 33, 206, 0 ),
            new( 34, 206, 0 ),
            new( 35, 206, 0 ),
            new( 36, 206, 0 ),
            new( 37, 206, 0 ),

            new( 33, 204, 0 ),
            new( 34, 204, 0 ),
            new( 35, 204, 0 ),
            new( 36, 204, 0 ),
            new( 37, 204, 0 ),

            new( 33, 203, 0 ),
            new( 34, 203, 0 ),
            new( 35, 203, 0 ),
            new( 36, 203, 0 ),
            new( 37, 203, 0 )
        };

        public FlameOfChaos(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version

            writer.Write(m_Barriers.Count);

            for (int i = 0; i < m_Barriers.Count; i++)
                writer.Write(m_Barriers[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            int amount = reader.ReadInt();

            m_Barriers = new List<FireBarrier>(amount);

            for (int i = 0; i < amount; i++)
                m_Barriers.Add(reader.ReadItem() as FireBarrier);

            if (!Visible)
                Timer.DelayCall(TimeSpan.Zero, RestoreBarrier);
        }
    }
}
