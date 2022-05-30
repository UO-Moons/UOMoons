using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealKirin : EtherealMount
    {
        [Constructable]
        public EtherealKirin()
            : base(0x25A0, 0x3E9C, 0x3EAD, DefaultEtherealHue)
        { }

        public EtherealKirin(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1049746;  // Ethereal Ki-Rin Statuette
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
                NonTransparentMountedID = 0x3EAD;
                Transparent = true;
            }

            if (version == 1)
            {
                TransparentMountedHue = DefaultEtherealHue;
            }
        }
    }
}
