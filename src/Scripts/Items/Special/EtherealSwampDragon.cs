using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealSwampDragon : EtherealMount
    {
        [Constructable]
        public EtherealSwampDragon()
            : base(0x2619, 0x3E98, 0x3EBD, DefaultEtherealHue, 0x851)
        { }

        public EtherealSwampDragon(Serial serial)
            : base(serial)
        { }

        public override int LabelNumber => 1049749;  // Ethereal Swamp Dragon Statuette
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
                NonTransparentMountedID = 0x3EBD;
                NonTransparentMountedHue = 0x851;
                Transparent = true;
            }
        }
    }
}
