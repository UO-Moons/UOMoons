using Server.Engines.Plants;

namespace Server.Items;

public class DryReeds : Item, IPlantHue
{
	private PlantHue _mPlantHue;

	[CommandProperty(AccessLevel.GameMaster)]
	public PlantHue PlantHue { get => _mPlantHue;
		set { _mPlantHue = value; InvalidatePlantHue(); InvalidateProperties(); } }

	public override int LabelNumber => 1112248;  //dry reeds

	public DryReeds(PlantHue hue)
		: base(0x1BD5)
	{
		PlantHue = hue;
		Stackable = true;
	}

	[Constructable]
	public DryReeds()
		: this(PlantHue.Plain)
	{
	}

	public void InvalidatePlantHue()
	{
		PlantHueInfo info = PlantHueInfo.GetInfo(_mPlantHue);

		if (info == null)
		{
			_mPlantHue = PlantHue.Plain;
			Hue = 0;
		}
		else
			Hue = info.Hue;

		InvalidateProperties();
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		PlantHueInfo info = PlantHueInfo.GetInfo(_mPlantHue);
		int cliloc;

		if (Amount > 1)
		{
			cliloc = info.IsBright() ? 1113273 : 1113275;
			list.Add(cliloc, $"{Amount}\t#{info.Name}");
		}
		else
		{
			cliloc = info.IsBright() ? 1112288 : 1112289;
			list.Add(cliloc, $"#{info.Name}");
		}
	}

	public override bool WillStack(Mobile from, Item dropped)
	{
		return dropped is IPlantHue hue && hue.PlantHue == _mPlantHue && base.WillStack(from, dropped);
	}

	public override void OnAfterDuped(Item newItem)
	{
		if (newItem is IPlantHue hue)
			hue.PlantHue = PlantHue;

		base.OnAfterDuped(newItem);
	}

	public DryReeds(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)_mPlantHue);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		_mPlantHue = (PlantHue)reader.ReadInt();
	}
}

public class SoftenedReeds : Item, IPlantHue
{
	private PlantHue _mPlantHue;

	[CommandProperty(AccessLevel.GameMaster)]
	public PlantHue PlantHue { get => _mPlantHue;
		set { _mPlantHue = value; InvalidatePlantHue(); InvalidateProperties(); } }

	public override int LabelNumber => 1112249;  //Softened reeds

	[Constructable]
	public SoftenedReeds()
		: this(PlantHue.Plain)
	{
	}

	public SoftenedReeds(PlantHue hue)
		: base(0x4006)
	{
		_mPlantHue = hue;
		InvalidatePlantHue();
		Stackable = true;
	}

	public void InvalidatePlantHue()
	{
		PlantHueInfo info = PlantHueInfo.GetInfo(_mPlantHue);

		if (info == null)
		{
			_mPlantHue = PlantHue.Plain;
			Hue = 0;
		}
		else
			Hue = info.Hue;

		InvalidateProperties();
	}

	public void InvalidateHue()
	{
		PlantHueInfo info = PlantHueInfo.GetInfo(Hue);
		_mPlantHue = info.PlantHue;
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{

		PlantHueInfo info = PlantHueInfo.GetInfo(_mPlantHue);
		int cliloc;

		if (Amount > 1)
		{
			cliloc = info.IsBright() ? 1113273 : 1113275;
			list.Add(cliloc, $"{Amount}\t#{info.Name}");
		}
		else
		{
			cliloc = info.IsBright() ? 1112288 : 1112289;
			list.Add(cliloc, $"#{info.Name}");
		}
	}

	public override bool WillStack(Mobile from, Item dropped)
	{
		return dropped is IPlantHue hue && hue.PlantHue == _mPlantHue && base.WillStack(from, dropped);
	}

	public override void OnAfterDuped(Item newItem)
	{
		if (newItem is IPlantHue hue)
			hue.PlantHue = PlantHue;

		base.OnAfterDuped(newItem);
	}

	public SoftenedReeds(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(2);

		writer.Write((int)_mPlantHue);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int v = reader.ReadInt();

		if (v > 1)
			_mPlantHue = (PlantHue)reader.ReadInt();
	}
}

public class CrystalGranules : Item
{
	public override int LabelNumber => 1112329;  // crystal granules

	[Constructable]
	public CrystalGranules()
		: base(16392)
	{
		Hue = 2625;
	}

	public CrystalGranules(Serial serial)
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
