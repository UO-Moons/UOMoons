namespace Server.Items;

public class JewelryBoxFilter
{
	public bool IsDefault => !Ring && !Bracelet && !Earrings && !Necklace && !Talisman;

	public void Clear()
	{
		m_Ring = false;
		_mBracelet = false;
		_mEarrings = false;
		_mNecklace = false;
		_mTalisman = false;
	}

	private bool m_Ring;

	public bool Ring
	{
		get => m_Ring;
		set
		{
			if (value)
				Clear();

			m_Ring = value;
		}
	}

	private bool _mBracelet;
	public bool Bracelet
	{
		get => _mBracelet;
		set
		{
			if (value)
				Clear();

			_mBracelet = value;
		}
	}

	private bool _mEarrings;
	public bool Earrings
	{
		get => _mEarrings;
		set
		{
			if (value)
				Clear();

			_mEarrings = value;
		}
	}

	private bool _mNecklace;
	public bool Necklace
	{
		get => _mNecklace;
		set
		{
			if (value)
				Clear();

			_mNecklace = value;
		}
	}

	private bool _mTalisman;
	public bool Talisman
	{
		get => _mTalisman;
		set
		{
			if (value)
				Clear();

			_mTalisman = value;
		}
	}

	public JewelryBoxFilter()
	{
	}

	public JewelryBoxFilter(GenericReader reader)
	{
		var version = reader.ReadInt();

		switch (version)
		{
			case 1:
				Ring = reader.ReadBool();
				Bracelet = reader.ReadBool();
				Earrings = reader.ReadBool();
				Necklace = reader.ReadBool();
				Talisman = reader.ReadBool();
				break;
		}
	}

	public void Serialize(GenericWriter writer)
	{
		if (IsDefault)
		{
			writer.Write(0);
		}
		else
		{
			writer.Write(1);

			writer.Write(Ring);
			writer.Write(Bracelet);
			writer.Write(Earrings);
			writer.Write(Necklace);
			writer.Write(Talisman);
		}
	}
}
