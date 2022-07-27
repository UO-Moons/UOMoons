namespace Server.Items
{
    [Flipable(0x908, 0x4075)]
    public class GargishTalwar : BaseSword
    {
        [Constructable]
        public GargishTalwar()
            : base(0x908)
        {
            //Weight = 16.0;
        }

        public GargishTalwar(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility => WeaponAbility.WhirlwindAttack;
        public override WeaponAbility SecondaryAbility => WeaponAbility.Dismount;
        public override int StrReq => 40;
        public override int MinDamageBase => 16;
        public override int MaxDamageBase => 19;
        public override float SpeedBase => 3.50f;

        public override int DefHitSound => 0x237;
        public override int DefMissSound => 0x238;
        public override int InitMinHits => 31;
        public override int InitMaxHits => 80;

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
