namespace Server.Items
{
    public class SoulGlaive : BaseThrown
    {
        [Constructable]
        public SoulGlaive()
            : base(0x090A)
        {
            Weight = 8.0;
            Layer = Layer.OneHanded;
        }

        public SoulGlaive(Serial serial)
            : base(serial)
        {
        }

        public override int MinThrowRange => 8;

        public override WeaponAbility PrimaryAbility => WeaponAbility.ArmorIgnore;
        public override WeaponAbility SecondaryAbility => WeaponAbility.MortalStrike;
        public override int StrReq => 60;
        public override int MinDamageBase => 16;
        public override int MaxDamageBase => 20;
        public override float SpeedBase => 4.00f;

        public override int InitMinHits => 31;
        public override int InitMaxHits => 65;

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
