namespace Server.Items
{
    [Flipable(0x900, 0x4071)]
    public class StoneWarSword : BaseSword
    {
        [Constructable]
        public StoneWarSword()
            : base(0x900)
        {
            //Weight = 6.0;
        }

        public StoneWarSword(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
        public override WeaponAbility SecondaryAbility => WeaponAbility.ParalyzingBlow;
        public override int StrReq => 40;
        public override int MinDamageBase => 15;
        public override int MaxDamageBase => 19;
        public override float SpeedBase => 3.75f;

        public override int DefMissSound => 0x23A;
        public override int InitMinHits => 31;
        public override int InitMaxHits => 100;

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
