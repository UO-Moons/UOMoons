using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealHiryu : EtherealMount
    {
        [Constructable]
        public EtherealHiryu()
            : base(0x276A, 0x3E94, 0x3E94, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealHiryu(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1113813;  // Ethereal Hiryu Statuette
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();

            if (version == 0)
            {
                Transparent = false;
            }
        }
    }
}
