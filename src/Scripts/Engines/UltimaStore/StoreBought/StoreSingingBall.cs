using System;

namespace Server.Items;

public enum SbType
{
	DrunkWomans,
	DrunkMans,
	Bedlam,
	SosarianSteeds,
	BlueBoar
}

public class StoreSingingBall : SingingBall
{
	public override int LabelNumber => 1152323 + (int)Type;

	public SbType Type { get; set; }

	[Constructable]
	public StoreSingingBall()
		: base(0x468A)
	{
		var values = Enum.GetValues(typeof(SbType));
		Type = (SbType)values.GetValue(Utility.Random(values.Length))!;

		Weight = 1.0;
		LootType = LootType.Regular;
		SetHue();
	}

	private void SetHue()
	{
		Hue = Type switch
		{
			SbType.Bedlam => 2611,
			SbType.BlueBoar => 2514,
			SbType.DrunkMans => 2659,
			SbType.DrunkWomans => 2596,
			_ => 2554
		};
	}

	public override int SoundList()
	{
		int sound = Type switch
		{
			SbType.Bedlam => Utility.RandomList(897, 1005, 889, 1001, 1002, 1004, 1005, 894, 893, 889, 1003),
			SbType.BlueBoar => Utility.RandomList(1073, 1085, 811, 799, 1066, 794, 801, 1075, 803, 811, 1071),
			SbType.DrunkMans => Utility.RandomMinMax(1049, 1098),
			SbType.DrunkWomans => Utility.RandomMinMax(778, 823),
			_ => Utility.RandomList(1218, 751, 629, 1226, 1305, 1246, 1019, 1508, 674, 1241)
		};

		return sound;
	}

	public StoreSingingBall(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version

		writer.Write((int)Type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		Type = (SbType)reader.ReadInt();
	}
}
