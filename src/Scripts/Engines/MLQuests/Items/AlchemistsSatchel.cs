using Server.Engines.Quests;
using System;

namespace Server.Items
{
    public class AlchemistsSatchel : BaseContainer, IDyable
    {
        [Constructable]
        public AlchemistsSatchel()
            : base(0xE75)
        {
            Hue = BaseReward.SatchelHue();
            Layer = Layer.Backpack;
            Weight = 3.0;
            AddItem(new Bloodmoss(10));
            AddItem(new MortarPestle());
        }

        public AlchemistsSatchel(Serial serial)
            : base(serial)
        {
        }

        public bool Dye(Mobile from, DyeTub sender)
        {
            if (Deleted)
            {
                return false;
            }

            Hue = sender.DyedHue;

            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            _ = reader.ReadInt();
        }
    }
}
