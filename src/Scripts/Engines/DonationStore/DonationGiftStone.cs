using Server.Accounting;
using Server.Gumps;

namespace Server.Items;

public class DonationGiftStone : Item
{
	public override string DefaultName => "Double click this stone to redeem your donation gift here";

	[Constructable]
	public DonationGiftStone() : base(0xED4)
	{
		Movable = false;
		Hue = 0x489;
	}

	public override void OnDoubleClick(Mobile from)
	{
		//check database for this player's account
		if (from.Account is Account account)
		{
			string accountName = account.Username;
		}

		from.SendGump(new DonationStoreGump(from));
	}

	public DonationGiftStone(Serial serial) : base(serial)
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
		_ = reader.ReadInt();
	}
}
