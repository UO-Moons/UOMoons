namespace Server.Items;

public class MerchantsTrinket : GoldEarrings
{
	private bool _greater;
	private int _usesRemaining;

	[CommandProperty(AccessLevel.GameMaster)]
	public int Bonus => Greater ? 10 : 5;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Greater { get => _greater;
		set { _greater = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int UsesRemaining { get => _usesRemaining;
		set { _usesRemaining = value; InvalidateProperties(); } }

	public override int LabelNumber => 1071399; // Merchant's Trinket

	[Constructable]
	public MerchantsTrinket()
		: this(false)
	{
		LootType = LootType.Blessed;
	}

	[Constructable]
	public MerchantsTrinket(bool greater)
	{
		Greater = greater;
		LootType = LootType.Blessed;

		UsesRemaining = 90;
	}

	public MerchantsTrinket(Serial serial)
		: base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1071398, Bonus.ToString()); // Discount Rate of Vendor Charge: ~1_val~%
		list.Add(1159250); // Non-commission vendors only.
	}

	public override void AddWeightProperty(ObjectPropertyList list)
	{
		if (_usesRemaining > 0)
		{
			list.Add(1060584, _usesRemaining.ToString()); // uses remaining: ~1_val~
		}

		base.AddWeightProperty(list);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(_usesRemaining);
		writer.Write(_greater);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				_usesRemaining = reader.ReadInt();
				_greater = reader.ReadBool();
				break;
		}
	}
}
