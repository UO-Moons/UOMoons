namespace Server.Items;

public class WinnowingBasket : BaseContainer
{
    [Constructable]
    public WinnowingBasket()
        : base(0x1882)
    {
        Weight = 1.0;
    }

    public WinnowingBasket(Serial serial)
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
