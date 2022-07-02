namespace Server.Items
{
    public class Boomerang : BaseThrown
    {
        [Constructable]
        public Boomerang()
            : base(0x8FF)
        {
            Weight = 4.0;
            Layer = Layer.OneHanded;
        }

        public Boomerang(Serial serial)
            : base(serial)
        {
        }

        public override int MinThrowRange => 4;

        //public override WeaponAbility PrimaryAbility => WeaponAbility.MysticArc;
        public override WeaponAbility SecondaryAbility => WeaponAbility.ConcussionBlow;
        public override int StrReq => 25;
        public override int MinDamageBase => 11;
        public override int MaxDamageBase => 15;
        public override float SpeedBase => 2.75f;

        public override int InitMinHits => 31;
        public override int InitMaxHits => 60;

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
