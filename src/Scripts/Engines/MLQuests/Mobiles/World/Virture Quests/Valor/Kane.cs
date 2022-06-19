using Server.Engines.Quests;
using Server.Items;
using System;

namespace Server.Mobiles;

public class Kane : MondainQuester
{
	public override Type[] Quests => new Type[]
	{
		typeof( DoughtyWarriorsQuest )
	};

	[Constructable]
	public Kane()
		: base("Kane", "the Master of Arms")
	{
	}

	public Kane(Serial serial)
		: base(serial)
	{
	}

	public override void InitBody()
	{
		InitStats(100, 100, 25);
		Female = false;
		Race = Race.Human;
		HairItemID = 0x203C;
		HairHue = 0x3B3;
	}

	public override void InitOutfit()
	{
		AddItem(new PlateArms());
		AddItem(new PlateChest());
		AddItem(new PlateGloves());
		AddItem(new StuddedGorget());
		AddItem(new PlateLegs());

		switch (Utility.Random(4))
		{
			case 0: AddItem(new PlateHelm()); break;
			case 1: AddItem(new NorseHelm()); break;
			case 2: AddItem(new CloseHelm()); break;
			case 3: AddItem(new Helmet()); break;
		}

		switch (Utility.Random(3))
		{
			case 0: AddItem(new BodySash(0x482)); break;
			case 1: AddItem(new Doublet(0x482)); break;
			case 2: AddItem(new Tunic(0x482)); break;
		}

		AddItem(new Broadsword());

		Item shield = new MetalKiteShield
		{
			Hue = Utility.RandomNondyedHue()
		};

		AddItem(shield);

		switch (Utility.Random(2))
		{
			case 0: AddItem(new Boots()); break;
			case 1: AddItem(new ThighBoots()); break;
		}

		PackItem(Loot.PackGold(100, 200));
		Blessed = true;
	}

	/*
    public static Kane TramInstance { get; set; }
    public static Kane FelInstance { get; set; }

    public static void Initialize()
    {
        if (TramInstance == null)
        {
            TramInstance = new Kane();
            var p = new Point3D(2027, 2740, 50);

            TramInstance.MoveToWorld(p, Map.Trammel);
            TramInstance.Home = p;
            TramInstance.RangeHome = 5;
        }

        if (FelInstance == null)
        {
            FelInstance = new Kane();
            var p = new Point3D(2027, 2740, 50);

            FelInstance.MoveToWorld(p, Map.Felucca);
            FelInstance.Home = p;
            FelInstance.RangeHome = 5;
        }
    }
    */

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