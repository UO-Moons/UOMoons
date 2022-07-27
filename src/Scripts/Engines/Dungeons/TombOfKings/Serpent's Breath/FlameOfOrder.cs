using System;
using System.Collections.Generic;

namespace Server.Items
{
    public class FlameOfOrder : BaseItem
    {
        public override int LabelNumber => 1112127;  // Flame of Order

        private List<EnergyBarrier> m_Barriers;
        private List<Blocker> m_Blockers;
        private List<LOSBlocker> m_LosBlockers;
        private List<SbMessageTrigger> m_MsgTriggers;

        [Constructable]
        public FlameOfOrder(Point3D location, Map map)
            : base(0x19AB)
        {
            Movable = false;
            Light = LightType.Circle225;

            MoveToWorld(location, map);

            m_Barriers = new List<EnergyBarrier>(m_BarrierLocations.Length);
            m_Blockers = new List<Blocker>(m_BarrierLocations.Length);
            m_LosBlockers = new List<LOSBlocker>(m_BarrierLocations.Length);
            m_MsgTriggers = new List<SbMessageTrigger>(m_MsgTriggerLocations.Length);

            foreach (Point3D loc in m_BarrierLocations)
            {
                m_Barriers.Add(new EnergyBarrier(loc, map));

                Blocker blocker = new Blocker();
                blocker.MoveToWorld(loc, map);
                m_Blockers.Add(blocker);

                LOSBlocker losblocker = new LOSBlocker();
                losblocker.MoveToWorld(loc, map);
                m_LosBlockers.Add(losblocker);
            }

            foreach (Point3D loc in m_MsgTriggerLocations)
            {
                SbMessageTrigger trigger = new SbMessageTrigger(this);
                trigger.MoveToWorld(loc, map);
                m_MsgTriggers.Add(trigger);
            }
        }

        public override bool HandlesOnSpeech => true;

        public override void OnSpeech(SpeechEventArgs e)
        {
            string mantra = e.Speech.ToLower();

            if (!Visible || !e.Mobile.InRange(this, 2) || mantra != "ord") return;
            Visible = false;

            foreach (EnergyBarrier barrier in m_Barriers)
	            barrier.Active = false;

            foreach (Blocker blocker in m_Blockers)
	            blocker.Delete();

            foreach (LOSBlocker losblocker in m_LosBlockers)
	            losblocker.Delete();

            m_Blockers.Clear();
            m_LosBlockers.Clear();

            Timer.DelayCall(TimeSpan.FromMinutes(2.0), RestoreBarrier);
        }

        private void RestoreBarrier()
        {
            foreach (EnergyBarrier barrier in m_Barriers)
                barrier.Active = true;

            foreach (Point3D loc in m_BarrierLocations)
            {
                Blocker blocker = new();
                blocker.MoveToWorld(loc, Map);
                m_Blockers.Add(blocker);

                LOSBlocker losblocker = new();
                losblocker.MoveToWorld(loc, Map);
                m_LosBlockers.Add(losblocker);
            }

            Visible = true;
        }

        public override void OnAfterDelete()
        {
            base.OnAfterDelete();

            foreach (Blocker blocker in m_Blockers)
            {
                blocker.Delete();
            }

            foreach (LOSBlocker losblocker in m_LosBlockers)
            {
                losblocker.Delete();
            }

            foreach (SbMessageTrigger trigger in m_MsgTriggers)
            {
                trigger.Delete();
            }

            foreach (EnergyBarrier barrier in m_Barriers)
            {
                barrier.Delete();
            }
        }

        private static readonly Point3D[] m_BarrierLocations = {
            new( 33, 205, 0 ),
            new( 34, 205, 0 ),
            new( 35, 205, 0 ),
            new( 36, 205, 0 ),
            new( 37, 205, 0 )
        };

        private static readonly Point3D[] m_MsgTriggerLocations = {
            new( 33, 203, 0 ),
            new( 34, 203, 0 ),
            new( 35, 203, 0 ),
            new( 36, 203, 0 ),
            new( 37, 203, 0 )
        };

        public FlameOfOrder(Serial serial)
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

            writer.Write(m_Blockers.Count);

            for (int i = 0; i < m_Blockers.Count; i++)
                writer.Write(m_Blockers[i]);

            writer.Write(m_LosBlockers.Count);

            for (int i = 0; i < m_LosBlockers.Count; i++)
                writer.Write(m_LosBlockers[i]);

            writer.Write(m_MsgTriggers.Count);

            for (int i = 0; i < m_MsgTriggers.Count; i++)
                writer.Write(m_MsgTriggers[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();

            // barrier
            int amount = reader.ReadInt();

            m_Barriers = new List<EnergyBarrier>(amount);

            for (int i = 0; i < amount; i++)
                m_Barriers.Add(reader.ReadItem() as EnergyBarrier);

            // blockers
            amount = reader.ReadInt();

            m_Blockers = new List<Blocker>(amount);

            for (int i = 0; i < amount; i++)
                m_Blockers.Add(reader.ReadItem() as Blocker);

            amount = reader.ReadInt();

            m_LosBlockers = new List<LOSBlocker>(amount);

            for (int i = 0; i < amount; i++)
                m_LosBlockers.Add(reader.ReadItem() as LOSBlocker);

            // msg triggers
            amount = reader.ReadInt();

            m_MsgTriggers = new List<SbMessageTrigger>(amount);

            for (int i = 0; i < amount; i++)
                m_MsgTriggers.Add(reader.ReadItem() as SbMessageTrigger);

            if (!Visible)
                Timer.DelayCall(TimeSpan.Zero, RestoreBarrier);
        }
    }
}
