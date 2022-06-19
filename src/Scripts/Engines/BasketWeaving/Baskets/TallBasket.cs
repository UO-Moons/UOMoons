namespace Server.Items;

public class TallBasket : BaseContainer
{
	public override int LabelNumber => 1112299;// tall basket

	[Constructable]
    public TallBasket()
        : base(0x24DB)
    {
        Weight = 1.0;
    }

    public TallBasket(Serial serial)
        : base(serial)
    {
    }

    public override void Serialize(GenericWriter writer)
    {
        base.Serialize(writer);
        writer.Write(0);
    }

    public override void Deserialize(GenericReader reader)
    {
	    base.Deserialize(reader);
	    reader.ReadInt();
    }
}
