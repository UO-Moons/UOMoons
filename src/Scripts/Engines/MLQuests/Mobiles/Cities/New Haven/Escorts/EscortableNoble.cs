using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class EscortableNoble : TownEscortable
    {
        [Constructable]
        public EscortableNoble()
        {
            Title = "the noble";

            SetSkill(SkillName.Parry, 80.0, 100.0);
            SetSkill(SkillName.Swords, 80.0, 100.0);
            SetSkill(SkillName.Tactics, 80.0, 100.0);
        }

        public EscortableNoble(Serial serial)
            : base(serial)
        {
        }

        public override bool CanTeach => true;
        public override bool ClickTitle => false;
        public override void InitOutfit()
        {
            if (Female)
                AddItem(new FancyDress());
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

            if (!Female)
                AddItem(new Longsword());

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
