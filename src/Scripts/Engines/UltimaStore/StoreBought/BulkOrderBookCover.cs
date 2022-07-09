using Server.Engines.BulkOrders;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public enum CoverType
{
	Normal,
	DullCopper,
	ShadowIron,
	Copper,
	Bronze,
	Gold,
	Agapite,
	Verite,
	Valorite,
	Spined,
	Horned,
	Barbed,
	Oak,
	Ash,
	Yew,
	Heartwood,
	Bloodwood,
	Frostwood,
	Alchemy,
	Blacksmith,
	Cooking,
	Fletching,
	Carpentry,
	Inscription,
	Tailoring,
	Tinkering
}

public class CoverInfo
{
	public static List<CoverInfo> Infos { get; private set; }

	public static void Initialize()
	{
		Infos = new List<CoverInfo>
		{
			new(CoverType.Normal, 1071097, 0),
			new(CoverType.DullCopper, 1071101, CraftResources.GetHue(CraftResource.DullCopper)),
			new(CoverType.ShadowIron, 1071107, CraftResources.GetHue(CraftResource.ShadowIron)),
			new(CoverType.Copper, 1071108, CraftResources.GetHue(CraftResource.Copper)),
			new(CoverType.Bronze, 1071109, CraftResources.GetHue(CraftResource.Bronze)),
			new(CoverType.Gold, 1071112, CraftResources.GetHue(CraftResource.Gold)),
			new(CoverType.Agapite, 1071113, CraftResources.GetHue(CraftResource.Agapite)),
			new(CoverType.Verite, 1071114, CraftResources.GetHue(CraftResource.Verite)),
			new(CoverType.Valorite, 1071115, CraftResources.GetHue(CraftResource.Valorite)),
			new(CoverType.Spined, 1071098, CraftResources.GetHue(CraftResource.SpinedLeather)),
			new(CoverType.Horned, 1071099, CraftResources.GetHue(CraftResource.HornedLeather)),
			new(CoverType.Barbed, 1071100, CraftResources.GetHue(CraftResource.BarbedLeather)),
			new(CoverType.Oak, 1071410, CraftResources.GetHue(CraftResource.OakWood)),
			new(CoverType.Ash, 1071411, CraftResources.GetHue(CraftResource.AshWood)),
			new(CoverType.Yew, 1071412, CraftResources.GetHue(CraftResource.YewWood)),
			new(CoverType.Heartwood, 1071413, CraftResources.GetHue(CraftResource.Heartwood)),
			new(CoverType.Bloodwood, 1071414, CraftResources.GetHue(CraftResource.Bloodwood)),
			new(CoverType.Frostwood, 1071415, CraftResources.GetHue(CraftResource.Frostwood)),
			new(CoverType.Alchemy, 1157605, 2505, 1002000),
			new(CoverType.Blacksmith, 1157605, 0x44E, 1157607),
			new(CoverType.Cooking, 1157605, 1169, 1002063),
			new(CoverType.Fletching, 1157605, 1425, 1002047),
			new(CoverType.Carpentry, 1157605, 1512, 1002054),
			new(CoverType.Inscription, 1157605, 2598, 1002090),
			new(CoverType.Tailoring, 1157605, 0x483, 1002155),
			new(CoverType.Tinkering, 1157605, 1109, 1002162)
		};
	}

	public CoverType Type { get; }
	public TextDefinition Label { get; }
	public int Hue { get; }
	public TextDefinition Args { get; }

	public CoverInfo(CoverType type, TextDefinition label, int hue, TextDefinition args = null)
	{
		Type = type;
		Label = label;
		Hue = hue;
		Args = args;
	}
}

public class BulkOrderBookCover : BaseItem
{
	private CoverType _coverType;
	private int _usesRemaining;

	[CommandProperty(AccessLevel.GameMaster)]
	public CoverType CoverType
	{
		get => _coverType;
		set
		{
			CoverType current = _coverType;

			if (current != value)
			{
				_coverType = value;
				InvalidateHue();
				InvalidateProperties();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int UsesRemaining { get => _usesRemaining;
		set { _usesRemaining = value; InvalidateProperties(); } }

	[Constructable]
	public BulkOrderBookCover(CoverType type)
		: this(type, 30)
	{
	}

	[Constructable]
	public BulkOrderBookCover(CoverType type, int uses)
		: base(0x2831)
	{
		UsesRemaining = uses;

		LootType = LootType.Blessed;
		CoverType = type;
	}

	public BulkOrderBookCover(Serial serial)
		: base(serial)
	{
	}

	public void InvalidateHue()
	{
		CoverInfo info = CoverInfo.Infos.FirstOrDefault(x => x.Type == _coverType);

		if (info != null)
		{
			Hue = info.Hue;
		}
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		CoverInfo info = CoverInfo.Infos.FirstOrDefault(x => x.Type == _coverType);

		if (info != null)
		{
			if (info.Args != null)
			{
				list.Add(1157605, info.Args.ToString()); // Bulk Order Cover (~1_HUE~)
			}
			else if (info.Label.Number > 0)
			{
				list.Add(info.Label.Number);
			}
			else
			{
				list.Add(1114057, info.Label.ToString()); // ~1_val~
			}
		}
		else
		{
			list.Add(1071097); // Bulk Order Cover (Normal)
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1060584, _usesRemaining.ToString()); // uses remaining: ~1_val~
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.SendLocalizedMessage(1071121); // Select the bulk order book you want to replace a cover.
			from.BeginTarget(-1, false, Targeting.TargetFlags.None, (m, targeted) =>
			{
				if (targeted is BulkOrderBook bob)
				{
					if (bob.IsChildOf(m.Backpack))
					{
						if (bob.Hue != Hue)
						{
							bob.Hue = Hue;
							UsesRemaining--;

							m.SendLocalizedMessage(1071119); // You have successfully given the bulk order book a new cover.
							m.PlaySound(0x048);

							if (UsesRemaining > 0)
								return;
							m.SendLocalizedMessage(1071120); // You have used up all the bulk order book covers.
							Delete();
						}
						else
						{
							m.SendLocalizedMessage(1071122); // You cannot cover it with same color.
						}
					}
					else
					{
						m.SendLocalizedMessage(1071117); // You cannot use this item for it.
					}
				}
				else
				{
					m.SendLocalizedMessage(1071118); // You can only cover a bulk order book with this item.
				}
			});
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(1);

		writer.Write(_usesRemaining);
		writer.Write((int)_coverType);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt();

		switch (version)
		{
			case 1:
				_usesRemaining = reader.ReadInt();
				_coverType = (CoverType)reader.ReadInt();
				break;
			case 0:
				_usesRemaining = 30;
				break;
		}
	}
}

public class BagOfBulkOrderCovers : Bag
{
	public override int LabelNumber => 1071116;  // Bag of bulk order covers

	public BagOfBulkOrderCovers(int start, int end)
	{
		for (var i = start; i <= end; i++)
		{
			if (i >= 0 && i < CoverInfo.Infos.Count)
			{
				DropItem(new BulkOrderBookCover(CoverInfo.Infos[i].Type));
			}
		}
	}

	public BagOfBulkOrderCovers(Serial serial)
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
