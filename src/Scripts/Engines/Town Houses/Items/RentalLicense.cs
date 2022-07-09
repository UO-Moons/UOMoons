namespace Server.Engines.TownHouses;

public class RentalLicense : BaseItem
{
	private Mobile _cOwner;

	public Mobile Owner
	{
		get => _cOwner;
		set
		{
			_cOwner = value;
			InvalidateProperties();
		}
	}

	public RentalLicense() : base(0x14F0)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		if (_cOwner != null)
		{
			list.Add("a renter's license belonging to " + _cOwner.Name);
		}
		else
		{
			list.Add("a renter's license");
		}
	}

	public override void OnDoubleClick(Mobile m)
	{
		_cOwner ??= m;
	}

	public RentalLicense(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(_cOwner);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		_cOwner = reader.ReadMobile();
	}
}
