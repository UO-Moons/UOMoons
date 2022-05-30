using System;
using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;

namespace Server.Mobiles
{
    public class EtherealWarBoar : EtherealMount
    {
        public override int LabelNumber => 1159423;  // Ethereal War Boar Statuette

        [Constructable]
        public EtherealWarBoar()
            : base(0xA554, 0x3ED2, 0x3ED2, DefaultEtherealHue)
        {
            Transparent = false;
        }

        public EtherealWarBoar(Serial serial)
            : base(serial)
        {
        }

        public override bool Validate(Mobile from)
        {
            #region TOL
            if (from.NetState != null && !from.NetState.SupportsExpansion(Expansion.ML))
            {
                from.SendLocalizedMessage(1156139); // * You must upgrade to the Time of Legends in order to use this. *                               
                return false;
            }
            #endregion

            return base.Validate(from);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
