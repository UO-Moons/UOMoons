using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class NewHavenSeekerOfAdventure : NewHavenEscortable
    {
        [Constructable]
        public NewHavenSeekerOfAdventure()
        {
            Title = "the seeker of adventure";
        }

        public NewHavenSeekerOfAdventure(Serial serial)
            : base(serial)
        {
        }

        public override bool ClickTitle => false;
        public override void InitOutfit()
        {
            if (Female)
                AddItem(new FancyDress(GetRandomHue()));
            else
                AddItem(new FancyShirt(GetRandomHue()));

            int lowHue = GetRandomHue();

            AddItem(new ShortPants(lowHue));

            if (Female)
                AddItem(new ThighBoots(lowHue));
            else
                AddItem(new Boots(lowHue));

            if (!Female)
                AddItem(new BodySash(lowHue));

            AddItem(new Cloak(GetRandomHue()));

            AddItem(new Longsword());

            //PackItem(Loot.PackGold(100, 150));
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
