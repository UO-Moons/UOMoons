namespace Server.Items
{
    public class OldRing : BaseRing
    {
        [Constructable]
        public OldRing()
            : base(0x108a)
        {
            Hue = 0x222;
        }

        public OldRing(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber => 1075524;// an old ring
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
