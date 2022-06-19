namespace Server.Items;

public class SquareBasket : BaseContainer
{
	public override int LabelNumber => 1112295;// square basket

	[Constructable]
    public SquareBasket()
        : base(0x24D5)
    {
        Weight = 1.0;
    }

    public SquareBasket(Serial serial)
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
