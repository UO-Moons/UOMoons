using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles
{
    public class Alefian : MondainQuester
    {
        public override Type[] Quests => new Type[]
                {
                    typeof(DefyingTheArcaneQuest)
                };

        [Constructable]
        public Alefian()
            : base("Alefian", "the Resisting Spells Instructor")
        {
            SetSkill(SkillName.EvalInt, 120.0, 120.0);
            SetSkill(SkillName.Inscribe, 120.0, 120.0);
            SetSkill(SkillName.Magery, 120.0, 120.0);
            SetSkill(SkillName.MagicResist, 120.0, 120.0);
            SetSkill(SkillName.Wrestling, 120.0, 120.0);
            SetSkill(SkillName.Meditation, 120.0, 120.0);
        }

        public Alefian(Serial serial)
            : base(serial)
        {
        }

        public override void Advertise()
        {
            Say(1078130); // A mage should learn how to resist spells.
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
            AddItem(new Robe());
            AddItem(new Sandals());
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
