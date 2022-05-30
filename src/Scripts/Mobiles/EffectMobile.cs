using System;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class EffectMobile : BaseMobile
    {
        public static readonly TimeSpan DefaultDuration = TimeSpan.FromSeconds(1.0);
        private static readonly List<EffectMobile> m_Free = new List<EffectMobile>();

        public EffectMobile(Serial serial)
            : base(serial)
        {
        }

        private EffectMobile()
        {
            CantWalk = true;
            // Hidden = true;
            Blessed = true;
        }

        public override void Kill()
        {
            base.Kill();
        }

        public override int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
        {
            return 0;
        }

        public static EffectMobile Create(Point3D p, Map map, TimeSpan duration)
        {
            EffectMobile mobile = null;

            for (int i = m_Free.Count - 1; mobile == null && i >= 0; --i) // We reuse new entries first so decay works better
            {
                EffectMobile free = m_Free[i];

                m_Free.RemoveAt(i);

                if (!free.Deleted && free.Map == Map.Internal)
                {
                    mobile = free;
                }
            }

            if (mobile == null)
            {
                mobile = new EffectMobile();
            }

            mobile.MoveToWorld(p, map);
            mobile.BeginFree(duration);

            return mobile;
        }

        public void BeginFree(TimeSpan duration)
        {
            new FreeTimer(this, duration).Start();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
            Delete();
        }

        private class FreeTimer : Timer
        {
            private readonly EffectMobile m_Mobile;
            public FreeTimer(EffectMobile mobile, TimeSpan delay)
                : base(delay)
            {
                m_Mobile = mobile;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Mobile.Internalize();
                m_Free.Add(m_Mobile);
            }
        }
    }
}
