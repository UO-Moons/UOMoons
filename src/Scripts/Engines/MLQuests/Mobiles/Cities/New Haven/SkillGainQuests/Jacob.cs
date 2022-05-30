using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Jacob : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(TheDeluciansLostMineQuest)
                };

        [Constructable]
        public Jacob()
            : base("Jacob Waltzt", "the Miner Instructor")
        {
            SetSkill(SkillName.ArmsLore, 120.0, 120.0);
            SetSkill(SkillName.Blacksmith, 120.0, 120.0);
            SetSkill(SkillName.Magery, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Tinkering, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Mining, 120.0, 120.0);
        }

        public Jacob(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078124); // You there! I can use some help mining these rocks!
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
            AddItem(new Pickaxe());
            AddItem(new Boots());
            AddItem(new ShortPants(0x370));
            AddItem(new Shirt(0x966));
            AddItem(new WideBrimHat(0x966));
            AddItem(new HalfApron(0x1BB));
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
