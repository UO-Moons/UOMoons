using Server.ContextMenus;
using Server.Engines.Craft;
using System;
using System.Collections.Generic;

namespace Server.Items;

public abstract class Food : BaseItem, IQuality
{
	[CommandProperty(AccessLevel.GameMaster)]
	private Mobile Poisoner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Poison Poison { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	protected int FillFactor { get; set; }

	public Food(int itemId) : this(1, itemId)
	{
	}

	public Food(int amount, int itemId) : base(itemId)
	{
		Stackable = true;
		Amount = amount;
		FillFactor = 1;
	}

	public Food(Serial serial) : base(serial)
	{
	}

	public override void OnAfterDuped(Item newItem)
	{
		if (newItem is not Food food)
			return;

		food.PlayerConstructed = PlayerConstructed;
		food.Poisoner = Poisoner;
		food.Poison = Poison;
		food.Quality = Quality;

		base.OnAfterDuped(newItem);
	}

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (from.Alive)
			list.Add(new EatEntry(from, this));
	}

	public override void AddCraftedProperties(ObjectPropertyList list)
	{
		if (Quality == ItemQuality.Exceptional)
		{
			list.Add(1060636); // Exceptional
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		if (Begged)
		{
			list.Add(1075129); // Acquired by begging
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!Movable)
			return;

		if (from.InRange(GetWorldLocation(), 1))
		{
			Eat(from);
		}
	}

	public virtual bool TryEat(Mobile from)
	{
		if (Deleted || !Movable || !from.CheckAlive() || !CheckItemUse(from))
			return false;

		return Eat(from);
	}

	public virtual bool Eat(Mobile from)
	{
		// Fill the Mobile with FillFactor
		if (CheckHunger(from))
		{
			// Play a random "eat" sound
			from.PlaySound(Utility.Random(0x3A, 3));

			if (from.Body.IsHuman && !from.Mounted)
				from.Animate(34, 5, 1, true, false, 0);

			if (Poison != null)
				from.ApplyPoison(Poisoner, Poison);

			Consume();

			EventSink.InvokeOnConsume(new OnConsumeEventArgs(from, this));

			return true;
		}

		return false;
	}

	public override bool WillStack(Mobile from, Item dropped)
	{
		return dropped is Food food && food.PlayerConstructed == PlayerConstructed && food.Quality == Quality && base.WillStack(from, food);
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		base.AddNameProperty(list);

		if (!string.IsNullOrEmpty(EngravedText))
		{
			list.Add(1072305, Utility.FixHtml(EngravedText)); // Engraved: ~1_INSCRIPTION~
		}
	}

	public virtual bool CheckHunger(Mobile from)
	{
		return FillHunger(from, FillFactor);
	}

	public static bool FillHunger(Mobile from, int fillFactor)
	{
		if (from.Hunger >= Settings.Configuration.Get<int>("Gameplay", "Hunger"))
		{
			from.SendLocalizedMessage(500867); // You are simply too full to eat any more!
			return false;
		}

		int iHunger = from.Hunger + fillFactor;

		if (from.Stam < from.StamMax)
			from.Stam += Utility.Random(6, 3) + fillFactor / 5;

		if (iHunger >= Settings.Configuration.Get<int>("Gameplay", "Hunger"))
		{
			from.Hunger = Settings.Configuration.Get<int>("Gameplay", "Hunger");
			from.SendLocalizedMessage(500872); // You manage to eat the food, but you are stuffed!
		}
		else
		{
			from.Hunger = iHunger;

			switch (iHunger)
			{
				case < 5:
					from.SendLocalizedMessage(500868); // You eat the food, but are still extremely hungry.
					break;
				case < 10:
					from.SendLocalizedMessage(500869); // You eat the food, and begin to feel more satiated.
					break;
				case < 15:
					from.SendLocalizedMessage(500870); // After eating the food, you feel much less hungry.
					break;
				default:
					from.SendLocalizedMessage(500871); // You feel quite full after consuming the food.
					break;
			}
		}

		return true;
	}

	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = (ItemQuality)quality;

		PlayerConstructed = true;

		return quality;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(Poisoner);
		Poison.Serialize(Poison, writer);
		writer.Write(FillFactor);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Poisoner = reader.ReadMobile();
				Poison = Poison.Deserialize(reader);
				FillFactor = reader.ReadInt();
				break;
			}
		}
	}
}

public class BreadLoaf : Food
{
	[Constructable]
	public BreadLoaf() : this(1)
	{
	}

	[Constructable]
	private BreadLoaf(int amount) : base(amount, 0x103B)
	{
		Weight = 1.0;
		FillFactor = 3;
	}

	public BreadLoaf(Serial serial) : base(serial)
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

public class Bacon : Food
{
	[Constructable]
	public Bacon() : this(1)
	{
	}

	[Constructable]
	private Bacon(int amount) : base(amount, 0x979)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public Bacon(Serial serial) : base(serial)
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

public class SlabOfBacon : Food
{
	[Constructable]
	public SlabOfBacon() : this(1)
	{
	}

	[Constructable]
	private SlabOfBacon(int amount) : base(amount, 0x976)
	{
		Weight = 1.0;
		FillFactor = 3;
	}

	public SlabOfBacon(Serial serial) : base(serial)
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

public class FishSteak : Food
{
	public override double DefaultWeight => 0.1;

	[Constructable]
	public FishSteak() : this(1)
	{
	}

	[Constructable]
	private FishSteak(int amount) : base(amount, 0x97B)
	{
		FillFactor = 3;
	}

	public FishSteak(Serial serial) : base(serial)
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

public class CheeseWheel : Food
{
	public override double DefaultWeight => 0.1;

	[Constructable]
	public CheeseWheel() : this(1)
	{
	}

	[Constructable]
	private CheeseWheel(int amount) : base(amount, 0x97E)
	{
		FillFactor = 3;
	}

	public CheeseWheel(Serial serial) : base(serial)
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

public class CheeseWedge : Food
{
	public override double DefaultWeight => 0.1;

	[Constructable]
	public CheeseWedge() : this(1)
	{
	}

	[Constructable]
	private CheeseWedge(int amount) : base(amount, 0x97D)
	{
		FillFactor = 3;
	}

	public CheeseWedge(Serial serial) : base(serial)
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

public class CheeseSlice : Food
{
	public override double DefaultWeight => 0.1;

	[Constructable]
	public CheeseSlice() : this(1)
	{
	}

	[Constructable]
	private CheeseSlice(int amount) : base(amount, 0x97C)
	{
		FillFactor = 1;
	}

	public CheeseSlice(Serial serial) : base(serial)
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

public class FrenchBread : Food
{
	[Constructable]
	public FrenchBread() : this(1)
	{
	}

	[Constructable]
	private FrenchBread(int amount) : base(amount, 0x98C)
	{
		Weight = 2.0;
		FillFactor = 3;
	}

	public FrenchBread(Serial serial) : base(serial)
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


public class FriedEggs : Food
{
	[Constructable]
	public FriedEggs() : this(1)
	{
	}

	[Constructable]
	private FriedEggs(int amount) : base(amount, 0x9B6)
	{
		Weight = 1.0;
		FillFactor = 4;
	}

	public FriedEggs(Serial serial) : base(serial)
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

public class CookedBird : Food
{
	[Constructable]
	public CookedBird() : this(1)
	{
	}

	[Constructable]
	private CookedBird(int amount) : base(amount, 0x9B7)
	{
		Weight = 1.0;
		FillFactor = 5;
	}

	public CookedBird(Serial serial) : base(serial)
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

public class RoastPig : Food
{
	[Constructable]
	public RoastPig() : this(1)
	{
	}

	[Constructable]
	private RoastPig(int amount) : base(amount, 0x9BB)
	{
		Weight = 45.0;
		FillFactor = 20;
	}

	public RoastPig(Serial serial) : base(serial)
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

public class Sausage : Food
{
	[Constructable]
	public Sausage() : this(1)
	{
	}

	[Constructable]
	private Sausage(int amount) : base(amount, 0x9C0)
	{
		Weight = 1.0;
		FillFactor = 4;
	}

	public Sausage(Serial serial) : base(serial)
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

public class Ham : Food
{
	[Constructable]
	public Ham() : this(1)
	{
	}

	[Constructable]
	private Ham(int amount) : base(amount, 0x9C9)
	{
		Weight = 1.0;
		FillFactor = 5;
	}

	public Ham(Serial serial) : base(serial)
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

public class Cake : Food
{
	[Constructable]
	public Cake() : base(0x9E9)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 10;
	}

	public Cake(Serial serial) : base(serial)
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

public class Ribs : Food
{
	[Constructable]
	public Ribs() : this(1)
	{
	}

	[Constructable]
	private Ribs(int amount) : base(amount, 0x9F2)
	{
		Weight = 1.0;
		FillFactor = 5;
	}

	public Ribs(Serial serial) : base(serial)
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

public class Cookies : Food
{
	[Constructable]
	public Cookies() : base(0x160b)
	{
		Stackable = Core.ML;
		Weight = 1.0;
		FillFactor = 4;
	}

	public Cookies(Serial serial) : base(serial)
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

public class Muffins : Food
{
	[Constructable]
	public Muffins() : base(0x9eb)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 4;
	}

	public Muffins(Serial serial) : base(serial)
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

[TypeAlias("Server.Items.Pizza")]
public class CheesePizza : Food
{
	public override int LabelNumber => 1044516;  // cheese pizza

	[Constructable]
	public CheesePizza() : base(0x1040)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 6;
	}

	public CheesePizza(Serial serial) : base(serial)
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

public class SausagePizza : Food
{
	public override int LabelNumber => 1044517;  // sausage pizza

	[Constructable]
	public SausagePizza() : base(0x1040)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 6;
	}

	public SausagePizza(Serial serial) : base(serial)
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

public class FruitPie : Food
{
	public override int LabelNumber => 1041346;  // baked fruit pie

	[Constructable]
	public FruitPie() : base(0x1041)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 5;
	}

	public FruitPie(Serial serial) : base(serial)
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

public class MeatPie : Food
{
	public override int LabelNumber => 1041347;  // baked meat pie

	[Constructable]
	public MeatPie() : base(0x1041)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 5;
	}

	public MeatPie(Serial serial) : base(serial)
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

public class PumpkinPie : Food
{
	public override int LabelNumber => 1041348;  // baked pumpkin pie

	[Constructable]
	public PumpkinPie() : base(0x1041)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 5;
	}

	public PumpkinPie(Serial serial) : base(serial)
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

public class ApplePie : Food
{
	public override int LabelNumber => 1041343;  // baked apple pie

	[Constructable]
	public ApplePie() : base(0x1041)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 5;
	}

	public ApplePie(Serial serial) : base(serial)
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

public class PeachCobbler : Food
{
	public override int LabelNumber => 1041344;  // baked peach cobbler

	[Constructable]
	public PeachCobbler() : base(0x1041)
	{
		Stackable = false;
		Weight = 1.0;
		FillFactor = 5;
	}

	public PeachCobbler(Serial serial) : base(serial)
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

public class Quiche : Food
{
	public override int LabelNumber => 1041345;  // baked quiche

	[Constructable]
	public Quiche() : base(0x1041)
	{
		Stackable = Core.ML;
		Weight = 1.0;
		FillFactor = 5;
	}

	public Quiche(Serial serial) : base(serial)
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

public class LambLeg : Food
{
	[Constructable]
	public LambLeg() : this(1)
	{
	}

	[Constructable]
	private LambLeg(int amount) : base(amount, 0x160a)
	{
		Weight = 2.0;
		FillFactor = 5;
	}

	public LambLeg(Serial serial) : base(serial)
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

public class ChickenLeg : Food
{
	[Constructable]
	public ChickenLeg() : this(1)
	{
	}

	[Constructable]
	private ChickenLeg(int amount) : base(amount, 0x1608)
	{
		Weight = 1.0;
		FillFactor = 4;
	}

	public ChickenLeg(Serial serial) : base(serial)
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

[Flipable(0xC74, 0xC75)]
public class HoneydewMelon : Food
{
	[Constructable]
	public HoneydewMelon() : this(1)
	{
	}

	[Constructable]
	private HoneydewMelon(int amount) : base(amount, 0xC74)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public HoneydewMelon(Serial serial) : base(serial)
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

[Flipable(0xC64, 0xC65)]
public class YellowGourd : Food
{
	[Constructable]
	public YellowGourd() : this(1)
	{
	}

	[Constructable]
	private YellowGourd(int amount) : base(amount, 0xC64)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public YellowGourd(Serial serial) : base(serial)
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

[Flipable(0xC66, 0xC67)]
public class GreenGourd : Food
{
	[Constructable]
	public GreenGourd() : this(1)
	{
	}

	[Constructable]
	private GreenGourd(int amount) : base(amount, 0xC66)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public GreenGourd(Serial serial) : base(serial)
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

[Flipable(0xC7F, 0xC81)]
public class EarOfCorn : Food
{
	[Constructable]
	public EarOfCorn() : this(1)
	{
	}

	[Constructable]
	private EarOfCorn(int amount) : base(amount, 0xC81)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public EarOfCorn(Serial serial) : base(serial)
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

public class Turnip : Food
{
	[Constructable]
	public Turnip() : this(1)
	{
	}

	[Constructable]
	private Turnip(int amount) : base(amount, 0xD3A)
	{
		Weight = 1.0;
		FillFactor = 1;
	}

	public Turnip(Serial serial) : base(serial)
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

public class SheafOfHay : BaseItem
{
	[Constructable]
	public SheafOfHay() : base(0xF36)
	{
		Weight = 10.0;
	}

	public SheafOfHay(Serial serial) : base(serial)
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
