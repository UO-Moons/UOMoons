namespace Server;

public class ResistanceMod
{
	private ResistanceType _type;
	private int _offset;

	public Mobile Owner { get; set; }

	public ResistanceType Type
	{
		get => _type;
		set
		{
			if (_type != value)
			{
				_type = value;

				Owner?.UpdateResistances();
			}
		}
	}

	public int Offset
	{
		get => _offset;
		set
		{
			if (_offset != value)
			{
				_offset = value;

				Owner?.UpdateResistances();
			}
		}
	}

	public ResistanceMod(ResistanceType type, int offset)
	{
		_type = type;
		_offset = offset;
	}
}
