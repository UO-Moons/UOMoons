using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class TylAriadne : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(ThouAndThineShieldQuest)
                };

        [Constructable]
        public TylAriadne()
            : base("Tyl Ariadne", "the Parrying Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.Parry, 120.0, 120.0);
            SetSkill(SkillName.Healing, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Meditation, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
        }

        public TylAriadne(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078140); // Want to learn how to parry blows?
        }

        public override void OnOfferFailed()
        {
            Say(1077772); // I cannot teach you, for you know all I can teach!
        }

        public override void InitBody()
        {
            Female = false;
            CantWalk = true;
            Race = Race.Elf;

            base.InitBody();
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new ElvenBoots(0x96D));

            Item item;

            item = new StuddedLegs();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedGloves();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedGorget();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedChest();
            item.Hue = 0x96D;
            AddItem(item);

            item = new StuddedArms();
            item.Hue = 0x96D;
            AddItem(item);

            item = new DiamondMace();
            item.Hue = 0x96D;
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
