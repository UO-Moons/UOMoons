using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Aelorn : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(CleansingOldHavenQuest)
                };

        public override bool IsActiveVendor => true;
        public override void InitSBInfo()
        {
            SBInfos.Add(new SBKeeperOfChivalry());
        }

        [Constructable]
        public Aelorn()
            : base("Aelorn", "the Chivalry Instructor")
        {
            SetSkill(SkillName.Anatomy, 120.0, 120.0);
            SetSkill(SkillName.MagicResist, 120.0, 120.0);
            SetSkill(SkillName.Tactics, 120.0, 120.0);
            SetSkill(SkillName.Swords, 120.0, 120.0);
            SetSkill(SkillName.Meditation, 120.0, 120.0);
            SetSkill(SkillName.Focus, 120.0, 120.0);
            SetSkill(SkillName.Chivalry, 120.0, 120.0);
        }

        public Aelorn(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078133); // Hail, friend. Want to live the life of a paladin?
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
            AddItem(new VikingSword());
            AddItem(new PlateChest());
            AddItem(new PlateLegs());
            AddItem(new PlateGloves());
            AddItem(new PlateArms());
            AddItem(new PlateGorget());
            AddItem(new OrderShield());
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
