using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealBeetle : EtherealMount
    {
        [Constructable]
        public EtherealBeetle()
            : base(0x260F, 0x3E97, 0x3EBC, DefaultEtherealHue)
        { }

        public EtherealBeetle(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1049748;  // Ethereal Beetle Statuette
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(2); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();

            if (version == 0)
            {
                NonTransparentMountedID = 0x3EBC;
                Transparent = true;
            }

            if (version == 1)
            {
                TransparentMountedHue = DefaultEtherealHue;
            }
        }
    }
}
