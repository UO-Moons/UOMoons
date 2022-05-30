using System;

namespace Server.Items
{
    public class PeppercornFishsteak : Food
    {
        public override ItemQuality Quality { get => ItemQuality.Normal; set { } }

        public override double DefaultWeight => 0.1;

        [Constructable]
        public PeppercornFishsteak()
           : this(1)
        {
        }

        [Constructable]
        public PeppercornFishsteak(int amount)
            : base(amount, 0x97B)
        {
            FillFactor = 3;
        }

        public PeppercornFishsteak(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1075557;// peppercorn fishsteak
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }
}
