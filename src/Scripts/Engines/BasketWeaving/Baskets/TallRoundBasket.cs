namespace Server.Items;

public class TallRoundBasket : BaseContainer
{
	public override int LabelNumber => 1112297;//Tall Round Basket

	[Constructable]
    public TallRoundBasket()
        : base(0x24D8)
    {
        Weight = 1.0;
    }

    public TallRoundBasket(Serial serial)
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
