using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class EscortableMerchant : TownEscortable
    {
        [Constructable]
        public EscortableMerchant()
        {
            Title = "the merchant";
            SetSkill(SkillName.ItemID, 55.0, 78.0);
            SetSkill(SkillName.ArmsLore, 55, 78);
        }

        public EscortableMerchant(Serial serial)
            : base(serial)
        {
        }

        public override bool CanTeach => true;
        public override bool ClickTitle => false;
        public override void InitOutfit()
        {
            if (Female)
                AddItem(new PlainDress());
            else
                AddItem(new Shirt(GetRandomHue()));

            int lowHue = GetRandomHue();

            AddItem(new ThighBoots());

            AddItem(new LongPants(lowHue));

            if (!Female)
                AddItem(new BodySash(lowHue));

            PackItem(Loot.PackGold(200, 250));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
}
