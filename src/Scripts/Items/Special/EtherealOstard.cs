using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealOstard : EtherealMount
    {
        [Constructable]
        public EtherealOstard()
            : base(0x2135, 0x3EAC, 0x3EA5)
        { }

        public EtherealOstard(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1041299;  // Ethereal Ostard Statuette
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
                NonTransparentMountedID = 0x3EA5;
                Transparent = true;
            }
        }
    }
}
