namespace Server.Items;

public class PicnicBasket2 : BaseContainer
{
    [Constructable]
    public PicnicBasket2()
        : base(0xE7A)
    {
        Weight = 1.0;
    }

    public PicnicBasket2(Serial serial)
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
