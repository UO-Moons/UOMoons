using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealRidgeback : EtherealMount
    {
        [Constructable]
        public EtherealRidgeback()
            : base(0x2615, 0x3E9A, 0x3EBA, DefaultEtherealHue)
        { }

        public EtherealRidgeback(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1049747;  // Ethereal Ridgeback Statuette
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
                NonTransparentMountedID = 0x3EBA;
                Transparent = true;
            }

            if (version == 1)
            {
                TransparentMountedHue = DefaultEtherealHue;
            }
        }
    }
}
