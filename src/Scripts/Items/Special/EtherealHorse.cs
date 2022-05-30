using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealHorse : EtherealMount
    {
        [Constructable]
        public EtherealHorse()
            : base(0x20DD, 0x3EAA, 0x3EA0)
        { }

        public EtherealHorse(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1041298;  // Ethereal Horse Statuette
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                NonTransparentMountedID = 0x3EA0;
                Transparent = true;
            }
        }
    }
}
