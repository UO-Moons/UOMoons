using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealLlama : EtherealMount
    {
        [Constructable]
        public EtherealLlama()
            : base(0x20F6, 0x3EAB, 0x3EA6)
        { }

        public EtherealLlama(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1041300;  // Ethereal Llama Statuette
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
                NonTransparentMountedID = 0x3EA6;
                Transparent = true;
            }
        }
    }
}
