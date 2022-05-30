using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealUnicorn : EtherealMount
    {
        [Constructable]
        public EtherealUnicorn()
            : base(0x25CE, 0x3E9B, 0x3EB4, DefaultEtherealHue)
        { }

        public EtherealUnicorn(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1049745;  // Ethereal Unicorn Statuette
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
                NonTransparentMountedID = 0x3EB4;
                Transparent = true;
            }

            if (version == 1)
            {
                TransparentMountedHue = DefaultEtherealHue;
            }
        }
    }
}
