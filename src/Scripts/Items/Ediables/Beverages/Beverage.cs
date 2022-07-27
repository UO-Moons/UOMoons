using Server.Engines.Plants;
using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Engines.Quests.Matriarch;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections;
using Server.Engines.Craft;

namespace Server.Items;

public enum BeverageType
{
	Ale,
	Cider,
	Liquor,
	Milk,
	Wine,
	Water,
	Coffee,
	GreenTea,
	HotCocoa
}

public interface IHasQuantity
{
	int Quantity { get; set; }
}

public interface IWaterSource : IHasQuantity
{
}

[TypeAlias("Server.Items.BottleAle", "Server.Items.BottleLiquor", "Server.Items.BottleWine")]
public class BeverageBottle : BaseBeverage
{
	public override int BaseLabelNumber => 1042959;  // a bottle of Ale
	public override int MaxQuantity => 5;
	public override bool Fillable => false;

	public override int ComputeItemId()
	{
		if (!IsEmpty)
		{
			switch (Content)
			{
				case BeverageType.Ale: return 0x99F;
				case BeverageType.Cider: return 0x99F;
				case BeverageType.Liquor: return 0x99B;
				case BeverageType.Milk: return 0x99B;
				case BeverageType.Wine: return 0x9C7;
				case BeverageType.Water: return 0x99B;
			}
		}

		return 0;
	}

	[Constructable]
	public BeverageBottle(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public BeverageBottle(Serial serial)
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

public class Shochu : BaseBeverage
{
	public override int LabelNumber => 1075497; // Shochu
	public override int MaxQuantity => 5;

	[Constructable]
	public Shochu()
	{
		Hue = 700;
		LootType = LootType.Blessed;
	}

	public override int ComputeItemId()
	{
		return 0x1956;
	}

	public Shochu(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version
	}
	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}

public class Jug : BaseBeverage
{
	public override int BaseLabelNumber => 1042965;  // a jug of Ale
	public override int MaxQuantity => 10;
	public override bool Fillable => false;

	public override int ComputeItemId()
	{
		if (!IsEmpty)
			return 0x9C8;

		return 0;
	}

	[Constructable]
	public Jug(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public Jug(Serial serial)
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

public class HotCocoaMug : CeramicMug
{
	[Constructable]
	public HotCocoaMug()
		: base(BeverageType.HotCocoa)
	{
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (Quantity > 0 && Content == BeverageType.HotCocoa)
		{
			list.Add(1049515, "#1155738"); // a mug of Hot Cocoa
		}
		else
		{
			base.AddNameProperty(list);
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (Quantity > 0 && Content == BeverageType.HotCocoa)
		{
			from.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1155739); // *You sip from the mug*
			Pour_OnTarget(from, from);
		}
		else
		{
			base.OnDoubleClick(from);
		}
	}

	public HotCocoaMug(Serial serial)
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

public class BasketOfGreenTeaMug : CeramicMug
{
	[Constructable]
	public BasketOfGreenTeaMug()
		: base(BeverageType.GreenTea)
	{
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (Quantity > 0 && Content == BeverageType.GreenTea)
		{
			list.Add(1049515, "#1030315"); // a mug of Basket of Green Tea
		}
		else
		{
			base.AddNameProperty(list);
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (Quantity > 0 && Content == BeverageType.GreenTea)
		{
			from.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1155739); // *You sip from the mug*
			Pour_OnTarget(from, from);
		}
		else
		{
			base.OnDoubleClick(from);
		}
	}

	public BasketOfGreenTeaMug(Serial serial)
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

public class CoffeeMug : CeramicMug
{
	[Constructable]
	public CoffeeMug()
		: base(BeverageType.Coffee)
	{
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (Quantity > 0 && Content == BeverageType.Coffee)
		{
			list.Add(1049515, "#1155737"); // a mug of Coffee
		}
		else
		{
			base.AddNameProperty(list);
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (Quantity > 0 && Content == BeverageType.Coffee)
		{
			from.PublicOverheadMessage(MessageType.Regular, 0x3B2, 1155739); // *You sip from the mug*
			Pour_OnTarget(from, from);
		}
		else
		{
			base.OnDoubleClick(from);
		}
	}

	public CoffeeMug(Serial serial)
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

public class CeramicMug : BaseBeverage
{
	public override int BaseLabelNumber => 1042982;  // a ceramic mug of Ale
	public override int MaxQuantity => 1;

	public override int ComputeItemId()
	{
		return ItemId switch
		{
			>= 0x995 and <= 0x999 => ItemId,
			0x9CA => ItemId,
			_ => 0x995
		};
	}

	[Constructable]
	public CeramicMug()
	{
		Weight = 1.0;
	}

	[Constructable]
	public CeramicMug(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public CeramicMug(Serial serial)
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

public class PewterMug : BaseBeverage
{
	public override int BaseLabelNumber => 1042994;  // a pewter mug with Ale
	public override int MaxQuantity => 1;

	public override int ComputeItemId()
	{
		if (ItemId is >= 0xFFF and <= 0x1002)
			return ItemId;

		return 0xFFF;
	}

	[Constructable]
	public PewterMug()
	{
		Weight = 1.0;
	}

	[Constructable]
	public PewterMug(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public PewterMug(Serial serial)
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

public class Goblet : BaseBeverage
{
	public override int BaseLabelNumber => 1043000;  // a goblet of Ale
	public override int MaxQuantity => 1;

	public override int ComputeItemId()
	{
		if (ItemId == 0x99A || ItemId == 0x9B3 || ItemId == 0x9BF || ItemId == 0x9CB)
			return ItemId;

		return 0x99A;
	}

	[Constructable]
	public Goblet()
	{
		Weight = 1.0;
	}

	[Constructable]
	public Goblet(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public Goblet(Serial serial)
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

[TypeAlias("Server.Items.MugAle", "Server.Items.GlassCider", "Server.Items.GlassLiquor",
	"Server.Items.GlassMilk", "Server.Items.GlassWine", "Server.Items.GlassWater")]
public class GlassMug : BaseBeverage
{
	public override int EmptyLabelNumber => 1022456;  // mug
	public override int BaseLabelNumber => 1042976;  // a mug of Ale
	public override int MaxQuantity => 5;

	public override int ComputeItemId()
	{
		if (IsEmpty)
			return ItemId >= 0x1F81 && ItemId <= 0x1F84 ? ItemId : 0x1F81;

		return Content switch
		{
			BeverageType.Ale => ItemId == 0x9EF ? 0x9EF : 0x9EE,
			BeverageType.Cider => ItemId >= 0x1F7D && ItemId <= 0x1F80 ? ItemId : 0x1F7D,
			BeverageType.Liquor => ItemId >= 0x1F85 && ItemId <= 0x1F88 ? ItemId : 0x1F85,
			BeverageType.Milk => ItemId >= 0x1F89 && ItemId <= 0x1F8C ? ItemId : 0x1F89,
			BeverageType.Wine => ItemId >= 0x1F8D && ItemId <= 0x1F90 ? ItemId : 0x1F8D,
			BeverageType.Water => ItemId >= 0x1F91 && ItemId <= 0x1F94 ? ItemId : 0x1F91,
			_ => 0
		};
	}

	[Constructable]
	public GlassMug()
	{
		Weight = 1.0;
	}

	[Constructable]
	public GlassMug(BeverageType type)
		: base(type)
	{
		Weight = 1.0;
	}

	public GlassMug(Serial serial)
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

[TypeAlias("Server.Items.PitcherAle", "Server.Items.PitcherCider", "Server.Items.PitcherLiquor",
	"Server.Items.PitcherMilk", "Server.Items.PitcherWine", "Server.Items.PitcherWater",
	"Server.Items.GlassPitcher")]
public class Pitcher : BaseBeverage
{
	public override int BaseLabelNumber => 1048128;  // a Pitcher of Ale
	public override int MaxQuantity => 5;

	public override int ComputeItemId()
	{
		if (IsEmpty)
		{
			if (ItemId == 0x9A7 || ItemId == 0xFF7)
				return ItemId;

			return 0xFF6;
		}

		switch (Content)
		{
			case BeverageType.Ale:
			{
				return ItemId == 0x1F96 ? ItemId : 0x1F95;
			}
			case BeverageType.Cider:
			{
				return ItemId == 0x1F98 ? ItemId : 0x1F97;
			}
			case BeverageType.Liquor:
			{
				return ItemId == 0x1F9A ? ItemId : 0x1F99;
			}
			case BeverageType.Milk:
			{
				return ItemId == 0x9AD ? ItemId : 0x9F0;
			}
			case BeverageType.Wine:
			{
				return ItemId == 0x1F9C ? ItemId : 0x1F9B;
			}
			case BeverageType.Water:
			{
				if (ItemId == 0xFF8 || ItemId == 0xFF9 || ItemId == 0x1F9E)
					return ItemId;

				return 0x1F9D;
			}
		}

		return 0;
	}

	[Constructable]
	public Pitcher()
	{
		Weight = 2.0;
	}

	[Constructable]
	public Pitcher(BeverageType type)
		: base(type)
	{
		Weight = 2.0;
	}

	public Pitcher(Serial serial)
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
		if (CheckType("PitcherWater") || CheckType("GlassPitcher"))
			InternalDeserialize(reader, false);
		else
			InternalDeserialize(reader, true);

		reader.ReadInt();
	}
}

public abstract class BaseBeverage : BaseItem, IHasQuantity, IResource, IQuality
{
	private BeverageType m_Content;
	private int m_Quantity;

	public override int LabelNumber
	{
		get
		{
			int num = BaseLabelNumber;

			if (IsEmpty || num == 0)
				return EmptyLabelNumber;

			return BaseLabelNumber + (int)m_Content;
		}
	}

	public virtual bool ShowQuantity => MaxQuantity > 1;
	public virtual bool Fillable => true;
	public virtual bool Pourable => true;

	public virtual int EmptyLabelNumber => base.LabelNumber;
	public virtual int BaseLabelNumber => 0;

	public abstract int MaxQuantity { get; }

	public abstract int ComputeItemId();

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsEmpty => m_Quantity <= 0;

	[CommandProperty(AccessLevel.GameMaster)]
	private bool ContainsAlchohol => !IsEmpty && m_Content != BeverageType.Milk && m_Content != BeverageType.Water;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsFull => m_Quantity >= MaxQuantity;

	[CommandProperty(AccessLevel.GameMaster)]
	public Poison Poison { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Poisoner { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public BeverageType Content
	{
		get => m_Content;
		set
		{
			m_Content = value;

			InvalidateProperties();

			int itemId = ComputeItemId();

			if (itemId > 0)
				ItemId = itemId;
			else
				Delete();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Quantity
	{
		get => m_Quantity;
		set
		{
			if (value < 0)
				value = 0;
			else if (value > MaxQuantity)
				value = MaxQuantity;

			m_Quantity = value;

			QuantityChanged();
			InvalidateProperties();

			int itemId = ComputeItemId();

			if (itemId > 0)
				ItemId = itemId;
			else
				Delete();
		}
	}

	public virtual int GetQuantityDescription()
	{
		int perc = m_Quantity * 100 / MaxQuantity;

		return perc switch
		{
			<= 0 => 1042975,
			<= 33 => 1042974,
			<= 66 => 1042973,
			_ => 1042972
		};
	}

	public virtual void QuantityChanged()
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (ShowQuantity)
			list.Add(GetQuantityDescription());
	}

	public override void AddNameProperty(ObjectPropertyList list)
	{
		if (Resource > CraftResource.Iron)
		{
			list.Add(1053099, "#{0}\t{1}", CraftResources.GetLocalizationNumber(Resource), $"#{LabelNumber}"); // ~1_oretype~ ~2_armortype~
		}
		else
		{
			base.AddNameProperty(list);
		}
	}

	public override void AddCraftedProperties(ObjectPropertyList list)
	{
		if (Crafter != null)
		{
			list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
		}

		if (Quality == ItemQuality.Exceptional)
		{
			list.Add(1060636); // Exceptional
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		if (ShowQuantity)
			LabelTo(from, GetQuantityDescription());
	}

	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		Quality = (ItemQuality)quality;

		if (makersMark)
			Crafter = from;

		if (craftItem.ForceNonExceptional)
			return quality;

		typeRes ??= craftItem.Resources.GetAt(0).ItemType;

		Resource = CraftResources.GetFromType(typeRes);

		PlayerConstructed = true;

		return quality;
	}

	public virtual bool ValidateUse(Mobile from, bool message)
	{
		if (Deleted)
			return false;

		if (!Movable && !Fillable)
		{
			Multis.BaseHouse house = Multis.BaseHouse.FindHouseAt(this);

			if (house == null || !house.IsLockedDown(this))
			{
				if (message)
					from.SendLocalizedMessage(502946, 0x59); // That belongs to someone else.

				return false;
			}
		}

		if (from.Map != Map || !from.InRange(GetWorldLocation(), 2) || !from.InLOS(this))
		{
			if (message)
				from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.

			return false;
		}

		return true;
	}

	public virtual void Fill_OnTarget(Mobile from, object targ)
	{
		if (!IsEmpty || !Fillable || !ValidateUse(from, false))
			return;

		switch (targ)
		{
			case BaseBeverage bev when bev.IsEmpty || !bev.ValidateUse(from, true):
				return;
			case BaseBeverage bev:
			{
				Content = bev.Content;
				Poison = bev.Poison;
				Poisoner = bev.Poisoner;

				if (bev.Quantity > MaxQuantity)
				{
					Quantity = MaxQuantity;
					bev.Quantity -= MaxQuantity;
				}
				else
				{
					Quantity += bev.Quantity;
					bev.Quantity = 0;
				}

				break;
			}
			case BaseWaterContainer container:
			{
				BaseWaterContainer bwc = container;

				if (Quantity == 0 || (Content == BeverageType.Water && !IsFull))
				{
					int iNeed = Math.Min(MaxQuantity - Quantity, bwc.Quantity);

					if (iNeed > 0 && !bwc.IsEmpty && !IsFull)
					{
						bwc.Quantity -= iNeed;
						Quantity += iNeed;
						Content = BeverageType.Water;

						from.PlaySound(0x4E);
					}
				}

				break;
			}
			case Item item1:
			{
				Item item = item1;

				var src = item as IWaterSource;

				if (src == null && item is AddonComponent component)
					src = component.Addon as IWaterSource;

				if (src == null || src.Quantity <= 0)
				{
					if (item is DecorativeWishingWell dw)
					{
						dw.CheckWaterSource(from, this);
					}

					/*if (item.ItemId >= 0xB41 && item.ItemId <= 0xB44)
				{
					Caddellite.CheckWaterSource(from, this, item);
				}*/

					return;
				}

				if (from.Map != item.Map || !from.InRange(item.GetWorldLocation(), 2) || !from.InLOS(item))
				{
					from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
					return;
				}

				Content = BeverageType.Water;
				Poison = null;
				Poisoner = null;

				if (src.Quantity > MaxQuantity)
				{
					Quantity = MaxQuantity;
					src.Quantity -= MaxQuantity;
				}
				else
				{
					Quantity += src.Quantity;
					src.Quantity = 0;
				}

				if (src is not WaterContainerComponent)
				{
					from.SendLocalizedMessage(1010089); // You fill the container with water.
				}

				break;
			}
			case Cow cow1:
			{
				if (cow1.TryMilk(from))
				{
					Content = BeverageType.Milk;
					Quantity = MaxQuantity;
					from.SendLocalizedMessage(1080197); // You fill the container with milk.
				}

				break;
			}
			case LandTarget target:
			{
				int tileId = target.TileID;

				if (from is PlayerMobile player)
				{
					QuestSystem qs = player.Quest;

					if (qs is WitchApprenticeQuest)
					{
						if (qs.FindObjective(typeof(FindIngredientObjective)) is FindIngredientObjective
						    {
							    Completed: false, Ingredient: Ingredient.SwampWater
						    } obj)
						{
							bool contains = false;

							for (int i = 0; !contains && i < m_SwampTiles.Length; i += 2)
								contains = tileId >= m_SwampTiles[i] && tileId <= m_SwampTiles[i + 1];

							if (contains)
							{
								Delete();

								player.SendLocalizedMessage(
									1055035); // You dip the container into the disgusting swamp water, collecting enough for the Hag's vile stew.
								obj.Complete();
							}
						}
					}
				}

				break;
			}
		}
	}

	private static readonly int[] m_SwampTiles = {
		0x9C4, 0x9EB,
		0x3D65, 0x3D65,
		0x3DC0, 0x3DD9,
		0x3DDB, 0x3DDC,
		0x3DDE, 0x3EF0,
		0x3FF6, 0x3FF6,
		0x3FFC, 0x3FFE,
	};

	#region Effects of achohol
	private static readonly Hashtable m_Table = new();

	public static void Initialize()
	{
		EventSink.OnLogin += EventSink_Login;
	}

	private static void EventSink_Login(Mobile mob)
	{
		CheckHeaveTimer(mob);
	}

	public static void CheckHeaveTimer(Mobile from)
	{
		if (from.Bac > 0 && from.Map != Map.Internal && !from.Deleted)
		{
			Timer t = (Timer)m_Table[from];

			if (t != null)
				return;
			if (from.Bac > 60)
				from.Bac = 60;

			t = new HeaveTimer(from);
			t.Start();

			m_Table[from] = t;
		}
		else
		{
			Timer t = (Timer)m_Table[from];

			if (t == null)
				return;
			t.Stop();
			m_Table.Remove(from);

			from.SendLocalizedMessage(500850); // You feel sober.
		}
	}

	private class HeaveTimer : Timer
	{
		private readonly Mobile m_Drunk;

		public HeaveTimer(Mobile drunk)
			: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
		{
			m_Drunk = drunk;

			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			if (m_Drunk.Deleted || m_Drunk.Map == Map.Internal)
			{
				Stop();
				m_Table.Remove(m_Drunk);
			}
			else if (m_Drunk.Alive)
			{
				if (m_Drunk.Bac > 60)
					m_Drunk.Bac = 60;

				// chance to get sober
				if (10 > Utility.Random(100))
					--m_Drunk.Bac;

				// lose some stats
				m_Drunk.Stam -= 1;
				m_Drunk.Mana -= 1;

				if (Utility.Random(1, 4) == 1)
				{
					if (!m_Drunk.Mounted)
					{
						// turn in a random direction
						m_Drunk.Direction = (Direction)Utility.Random(8);

						// heave
						m_Drunk.Animate(32, 5, 1, true, false, 0);
					}

					// *hic*
					m_Drunk.PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, 500849);
				}

				if (m_Drunk.Bac > 0)
					return;
				Stop();
				m_Table.Remove(m_Drunk);

				m_Drunk.SendLocalizedMessage(500850); // You feel sober.
			}
		}
	}

	#endregion

	public virtual void Pour_OnTarget(Mobile from, object targ)
	{
		if (IsEmpty || !Pourable || !ValidateUse(from, false))
			return;

		if (targ is BaseBeverage)
		{
			BaseBeverage bev = (BaseBeverage)targ;

			if (!bev.ValidateUse(from, true))
				return;

			if (bev.IsFull && bev.Content == Content)
			{
				from.SendLocalizedMessage(500848); // Couldn't pour it there.  It was already full.
			}
			else if (!bev.IsEmpty)
			{
				from.SendLocalizedMessage(500846); // Can't pour it there.
			}
			else
			{
				bev.Content = Content;
				bev.Poison = Poison;
				bev.Poisoner = Poisoner;

				if (Quantity > bev.MaxQuantity)
				{
					bev.Quantity = bev.MaxQuantity;
					Quantity -= bev.MaxQuantity;
				}
				else
				{
					bev.Quantity += Quantity;
					Quantity = 0;
				}

				from.PlaySound(0x4E);
			}
		}
		else if (targ is WaterContainerComponent)
		{
			WaterContainerComponent component = (WaterContainerComponent)targ;

			if (component.IsFull)
			{
				from.SendLocalizedMessage(500848); // Couldn't pour it there.  It was already full.
			}
			else
			{
				component.Quantity += Quantity;
				Quantity = 0;
			}

			from.PlaySound(0x4E);
		}
		else if (from == targ)
		{
			if (from.Thirst < 20)
				from.Thirst += 1;

			if (ContainsAlchohol)
			{
				int bac = 0;

				switch (Content)
				{
					case BeverageType.Ale: bac = 1; break;
					case BeverageType.Wine: bac = 2; break;
					case BeverageType.Cider: bac = 3; break;
					case BeverageType.Liquor: bac = 4; break;
				}

				from.Bac += bac;

				if (from.Bac > 60)
					from.Bac = 60;

				CheckHeaveTimer(from);
			}

			from.PlaySound(Utility.RandomList(0x30, 0x2D6));

			if (Poison != null)
				from.ApplyPoison(Poisoner, Poison);

			--Quantity;
		}
		else if (targ is BaseWaterContainer)
		{
			BaseWaterContainer bwc = targ as BaseWaterContainer;

			if (Content != BeverageType.Water)
			{
				from.SendLocalizedMessage(500842); // Can't pour that in there.
			}
			else if (bwc.Items.Count != 0)
			{
				from.SendLocalizedMessage(500841); // That has something in it.
			}
			else
			{
				int itNeeds = Math.Min(bwc.MaxQuantity - bwc.Quantity, Quantity);

				if (itNeeds > 0)
				{
					bwc.Quantity += itNeeds;
					Quantity -= itNeeds;

					from.PlaySound(0x4E);
				}
			}
		}
		else if (targ is PlantItem)
		{
			((PlantItem)targ).Pour(from, this);
		}
		else if (targ is ChickenLizardEgg)
		{
			((ChickenLizardEgg)targ).Pour(from, this);
		}
		else if (targ is WaterElemental)
		{
			if (this is Pitcher && Content == BeverageType.Water)
			{
				EndlessDecanter.HandleThrow(this, (WaterElemental)targ, from);
			}
		}
		else if (this is Pitcher && Content == BeverageType.Water)
		{
			if (targ is FillableBarrel)
			{
				((FillableBarrel)targ).Pour(from, this);
			}
			else if (targ is Barrel)
			{
				((Barrel)targ).Pour(from, this);
			}
		}
		else if (targ is AddonComponent &&
		         (((AddonComponent)targ).Addon is WaterVatEast || ((AddonComponent)targ).Addon is WaterVatSouth) &&
		         Content == BeverageType.Water)
		{
			PlayerMobile player = from as PlayerMobile;

			if (player != null)
			{
				SolenMatriarchQuest qs = player.Quest as SolenMatriarchQuest;

				if (qs != null)
				{
					QuestObjective obj = qs.FindObjective(typeof(GatherWaterObjective));

					if (obj != null && !obj.Completed)
					{
						BaseAddon vat = ((AddonComponent)targ).Addon;

						if (vat.X > 5784 && vat.X < 5814 && vat.Y > 1903 && vat.Y < 1934 &&
						    ((qs.RedSolen && vat.Map == Map.Trammel) || (!qs.RedSolen && vat.Map == Map.Felucca)))
						{
							if (obj.CurProgress + Quantity > obj.MaxProgress)
							{
								int delta = obj.MaxProgress - obj.CurProgress;

								Quantity -= delta;
								obj.CurProgress = obj.MaxProgress;
							}
							else
							{
								obj.CurProgress += Quantity;
								Quantity = 0;
							}
						}
					}
				}
			}
		}
		else
		{
			from.SendLocalizedMessage(500846); // Can't pour it there.
		}
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsEmpty)
		{
			if (!Fillable || !ValidateUse(from, true))
				return;

			from.BeginTarget(-1, true, TargetFlags.None, new TargetCallback(Fill_OnTarget));
			SendLocalizedMessageTo(from, 500837); // Fill from what?
		}
		else if (Pourable && ValidateUse(from, true))
		{
			from.BeginTarget(-1, true, TargetFlags.None, new TargetCallback(Pour_OnTarget));
			from.SendLocalizedMessage(1010086); // What do you want to use this on?
		}
	}

	public static bool ConsumeTotal(Container pack, BeverageType content, int quantity)
	{
		return ConsumeTotal(pack, typeof(BaseBeverage), content, quantity);
	}

	public static bool ConsumeTotal(Container pack, Type itemType, BeverageType content, int quantity)
	{
		Item[] items = pack.FindItemsByType(itemType);

		// First pass, compute total
		int total = 0;

		for (int i = 0; i < items.Length; ++i)
		{
			BaseBeverage bev = items[i] as BaseBeverage;

			if (bev != null && bev.Content == content && !bev.IsEmpty)
				total += bev.Quantity;
		}

		if (total >= quantity)
		{
			// We've enough, so consume it

			int need = quantity;

			for (int i = 0; i < items.Length; ++i)
			{
				BaseBeverage bev = items[i] as BaseBeverage;

				if (bev == null || bev.Content != content || bev.IsEmpty)
					continue;

				int theirQuantity = bev.Quantity;

				if (theirQuantity < need)
				{
					bev.Quantity = 0;
					need -= theirQuantity;
				}
				else
				{
					bev.Quantity -= need;
					return true;
				}
			}
		}

		return false;
	}

	public BaseBeverage()
	{
		ItemId = ComputeItemId();
	}

	public BaseBeverage(BeverageType type)
	{
		m_Content = type;
		m_Quantity = MaxQuantity;
		ItemId = ComputeItemId();
	}

	public BaseBeverage(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(Poisoner);

		Poison.Serialize(Poison, writer);
		writer.Write((int)m_Content);
		writer.Write(m_Quantity);
	}

	protected bool CheckType(string name)
	{
		return World.LoadingType == string.Format("Server.Items.{0}", name);
	}

	public override void Deserialize(GenericReader reader)
	{
		InternalDeserialize(reader, true);
	}

	protected void InternalDeserialize(GenericReader reader, bool read)
	{
		base.Deserialize(reader);

		if (!read)
			return;

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Poisoner = reader.ReadMobile();

				Poison = Poison.Deserialize(reader);
				m_Content = (BeverageType)reader.ReadInt();
				m_Quantity = reader.ReadInt();
				break;
			}
		}
	}
}
