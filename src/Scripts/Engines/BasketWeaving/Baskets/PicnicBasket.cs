namespace Server.Items;

public class PicnicBasket : BaseContainer
{
	public override int LabelNumber => 1023706; // picnic basket

	[Constructable]
    public PicnicBasket()
        : base(0xE7A)
    {
        Weight = 1.0;
    }

    public PicnicBasket(Serial serial)
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
