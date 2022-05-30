using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class NewHavenMessenger : NewHavenEscortable
    {
        [Constructable]
        public NewHavenMessenger()
        {
            Title = "the messenger";
        }

        public NewHavenMessenger(Serial serial)
            : base(serial)
        {
        }

        public override bool ClickTitle => false;
        public override void InitOutfit()
        {
            if (Female)
                AddItem(new PlainDress());
            else
                AddItem(new Shirt(GetRandomHue()));

            int lowHue = GetRandomHue();

            AddItem(new ShortPants(lowHue));

            if (Female)
                AddItem(new Boots(lowHue));
            else
                AddItem(new Shoes(lowHue));

            switch (Utility.Random(4))
            {
                case 0:
                    AddItem(new ShortHair(Utility.RandomHairHue()));
                    break;
                case 1:
                    AddItem(new TwoPigTails(Utility.RandomHairHue()));
                    break;
                case 2:
                    AddItem(new ReceedingHair(Utility.RandomHairHue()));
                    break;
                case 3:
                    AddItem(new KrisnaHair(Utility.RandomHairHue()));
                    break;
            }

            //PackItem(Loot.PackGold(200, 250));
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
