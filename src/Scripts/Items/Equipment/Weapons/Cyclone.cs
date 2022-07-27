namespace Server.Items
{
    public class Cyclone : BaseThrown
    {
        [Constructable]
        public Cyclone()
            : base(0x901)
        {
            Weight = 6.0;
            Layer = Layer.OneHanded;
        }

        public Cyclone(Serial serial)
            : base(serial)
        {
        }

        public override int MinThrowRange => 6;

        public override WeaponAbility PrimaryAbility => WeaponAbility.MovingShot;
        //public override WeaponAbility SecondaryAbility => WeaponAbility.InfusedThrow;
        public override int StrReq => 40;
        public override int MinDamageBase => 13;
        public override int MaxDamageBase => 17;
        public override float SpeedBase => 3.25f;

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
