using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Recaro : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(EnGuardeQuest)
                };

        [Constructable]
        public Recaro()
            : base("Recaro", "the Fencer Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Fencing, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public Recaro(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078187); // The art of fencing requires a dexterous hand, a quick wit and fleet feet.
        }

        public override void OnOfferFailed()
        {
            Say(1077772); // I cannot teach you, for you know all I can teach!
        }

        public override void InitBody()
        {
            Female = false;
            CantWalk = true;
            Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Shoes(0x455));
            AddItem(new WarFork());

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x455;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x455;
            AddItem(item);
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
