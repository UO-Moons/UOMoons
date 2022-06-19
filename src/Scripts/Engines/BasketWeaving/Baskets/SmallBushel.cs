namespace Server.Items;

public class SmallBushel : BaseContainer
{
	public override int LabelNumber => 1112337;// small bushel

	[Constructable]
    public SmallBushel()
        : base(0x09B1)
    {
        Weight = 1.0;
    }

    public SmallBushel(Serial serial)
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
