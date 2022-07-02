namespace Server.Items
{
    [Flipable(0x903, 0x406E)]
    public class DiscMace : BaseBashing
    {
        [Constructable]
        public DiscMace()
            : base(0x903)
        {
        }

        public DiscMace(Serial serial)
            : base(serial)
        {
        }

        public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
        public override WeaponAbility SecondaryAbility => WeaponAbility.Disarm;
        public override int StrReq => 45;
        public override int MinDamageBase => 11;
        public override int MaxDamageBase => 15;
        public override float SpeedBase => 2.75f;

        public override int InitMinHits => 31;
        public override int InitMaxHits => 110;

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