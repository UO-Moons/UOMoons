namespace Server.Engines.TownHouses;

public sealed class RentalContractCopy : BaseItem
{
	private readonly RentalContract _cContract;

	public RentalContractCopy(RentalContract contract)
	{
		Name = "rental contract copy";
		ItemId = 0x14F0;
		_cContract = contract;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (_cContract == null || _cContract.Deleted)
		{
			Delete();
			return;
		}

		_cContract.OnDoubleClick(m);
	}

	public RentalContractCopy(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(1); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();
	}
}
