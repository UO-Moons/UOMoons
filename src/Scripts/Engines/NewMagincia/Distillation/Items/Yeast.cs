namespace Server.Items;

public class Yeast : BaseItem
{
	private int _bacterialResistance;

	[CommandProperty(AccessLevel.GameMaster)]
	public int BacterialResistance
	{
		get => _bacterialResistance;
		set
		{
			_bacterialResistance = value;

			if (_bacterialResistance < 1) _bacterialResistance = 1;
			if (_bacterialResistance > 5) _bacterialResistance = 5;

			InvalidateProperties();
		}
	}

	public override int LabelNumber => 1150453;  // yeast

	[Constructable]
	public Yeast()
		: base(0xF00)
	{
		Hue = 1501;
		int ran = Utility.Random(100);

		_bacterialResistance = ran switch
		{
			<= 5 => 5,
			<= 10 => 4,
			<= 20 => 3,
			<= 40 => 2,
			_ => 1
		};
	}

	[Constructable]
	public Yeast(int resistance)
		: base(0xF00)
	{
		BacterialResistance = resistance;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1150455, GetResistanceLabel()); // Bacterial Resistance: ~1_VAL~
	}

	private string GetResistanceLabel()
	{
		return _bacterialResistance switch
		{
			4 => "+",
			3 => "+-",
			2 => "-",
			1 => "--",
			_ => "++",
		};
	}

	public Yeast(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(_bacterialResistance);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		_bacterialResistance = reader.ReadInt();
	}
}
