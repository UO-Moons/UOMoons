namespace Server.Items;

public class SmallRoundBasket : BaseContainer
{
	public override int LabelNumber => 1112298;// small round basket

	[Constructable]
    public SmallRoundBasket()
        : base(0x24DD)
    {
        Weight = 1.0;
    }

    public SmallRoundBasket(Serial serial)
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
