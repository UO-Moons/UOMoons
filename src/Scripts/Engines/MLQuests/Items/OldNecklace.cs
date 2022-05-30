using System;

namespace Server.Items
{
    public class OldNecklace : BaseNecklace
    {
        [Constructable]
        public OldNecklace()
            : base(0x1085)
        {
            Hue = 0x222;
        }

        public OldNecklace(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1075525;// an old necklace
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
