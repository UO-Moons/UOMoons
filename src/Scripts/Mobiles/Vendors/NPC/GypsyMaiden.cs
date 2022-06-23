using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class GypsyMaiden : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	[Constructable]
	public GypsyMaiden() : base("the gypsy maiden")
	{
	}

	public override bool GetGender()
	{
		return true; // always female
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbProvisioner());
	}

	public override void InitOutfit()
	{
		base.InitOutfit();

		switch (Utility.Random(4))
		{
			case 0: SetWearable(new JesterHat(Utility.RandomBrightHue())); break;
			case 1: SetWearable(new Bandana(Utility.RandomBrightHue())); break;
			case 2: SetWearable(new SkullCap(Utility.RandomBrightHue())); break;
		}

		if (Utility.RandomBool())
			SetWearable(new HalfApron(Utility.RandomBrightHue()));

		Item item = FindItemOnLayer(Layer.Pants);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.OuterLegs);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();

		item = FindItemOnLayer(Layer.InnerLegs);

		if (item != null)
			item.Hue = Utility.RandomBrightHue();
	}

	public GypsyMaiden(Serial serial) : base(serial)
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
