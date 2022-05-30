using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Robyn : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(SwiftAsAnArrowQuest)
                };

        public override void InitSBInfo()
        {
            SBInfos.Add(new SBRanger());
        }

        [Constructable]
        public Robyn()
            : base("Robyn", "the Archer Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Fletching, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Archery, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public Robyn(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078202); // Archery requires a steady aim and dexterous fingers.
        }

        public override void OnOfferFailed()
        {
            Say(1077772); // I cannot teach you, for you know all I can teach!
        }

        public override void InitBody()
        {
            Female = true;
            CantWalk = true;
            Race = Race.Human;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Boots(0x592));
            AddItem(new Cloak(0x592));
            AddItem(new Bandana(0x592));
            AddItem(new CompositeBow());

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x592;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x592;
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
