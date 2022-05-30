using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealReptalon : EtherealMount
    {
        [Constructable]
        public EtherealReptalon()
            : base(0x2d95, 0x3e90, 0x3e90, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealReptalon(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1113812;  // Ethereal Reptalon Statuette
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
