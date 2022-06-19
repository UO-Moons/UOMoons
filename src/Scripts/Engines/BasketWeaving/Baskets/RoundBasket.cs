namespace Server.Items;

public class RoundBasket : BaseContainer
{
	public override int LabelNumber => 1112293;// round basket

	[Constructable]
    public RoundBasket()
        : base(0x990)
    {
        Weight = 1.0;
    }

    public RoundBasket(Serial serial)
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
