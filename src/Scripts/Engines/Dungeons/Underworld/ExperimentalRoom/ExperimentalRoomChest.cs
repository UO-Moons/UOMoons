using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class ExperimentalRoomChest : MetalBox
{
	private Dictionary<Item, Mobile> m_Instancing;

	public override bool DisplayWeight => false;
	public override bool DisplaysContent => false;
	public override bool Decays => true;
	public override TimeSpan DecayTime => TimeSpan.FromMinutes(10.0);

	[Constructable]
	public ExperimentalRoomChest()
	{
		Movable = false;
		m_Instancing = new Dictionary<Item, Mobile>();
		LiftOverride = true;

	}

	public override void OnDoubleClick(Mobile from)
	{
		Container pack = from.Backpack;

		Item item = pack?.FindItemByType(typeof(ExperimentalGem));

		if (item is ExperimentalGem { Complete: true } gem)
		{
			gem.Delete();

			Item toDrop = GetRandomDrop();

			if (toDrop != null)
				AddItemFor(toDrop, from);
		}

		base.OnDoubleClick(from);
	}

	public override bool TryDropItem(Mobile from, Item dropped, bool message)
	{
		if (dropped is not ExperimentalGem { Complete: true } gem || !from.InRange(Location, 2))
			return false;

		gem.Delete();

		Item toDrop = GetRandomDrop();

		if (toDrop != null)
			AddItemFor(toDrop, from);

		base.OnDoubleClick(from);

		return false;
	}

	private void AddItemFor(Item item, Mobile mob)
	{
		if (item == null || mob == null)
			return;

		DropItem(item);
		item.SetLastMoved();

		m_Instancing ??= new Dictionary<Item, Mobile>();

		m_Instancing[item] = mob;
	}

	public override bool IsChildVisibleTo(Mobile m, Item child)
	{
		if (m.AccessLevel > AccessLevel.Player)
			return true;

		if (m_Instancing != null)
		{
			if (!m_Instancing.ContainsKey(child))
				return true;

			if (m_Instancing[child] == m)
				return true;
		}
		else
		{
			return true;
		}

		return false;
	}

	public override bool OnDecay()
	{
		List<Item> items = new(Items);

		foreach (var i in items.Where(i => i.Decays && i.LastMoved.Add(DecayTime) < DateTime.UtcNow))
		{
			i.Delete();

			if (m_Instancing.ContainsKey(i))
				m_Instancing.Remove(i);
		}

		return false;
	}

	public override void RemoveItem(Item item)
	{
		if (m_Instancing != null && m_Instancing.ContainsKey(item))
			m_Instancing.Remove(item);

		base.RemoveItem(item);
	}

	private static Item GetRandomDrop()
	{
		Item item = null;

		switch (Utility.Random(17))
		{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				item = new Stalagmite();
				break;
			case 7:
			case 8:
			case 9:
			case 10:
				item = new Flowstone();
				break;
			case 11:
				item = new CanvaslessEasel();
				break;
			case 12:
				item = new HangingChainmailLegs();
				break;
			case 13:
				item = new HangingRingmailTunic();
				break;
			case 14:
				item = new PluckedChicken();
				break;
			case 15:
				item = new ColorfulTapestry();
				break;
			case 16:
				item = new TwoStoryBanner();
				break;
		}

		return item;
	}

	public ExperimentalRoomChest(Serial serial) : base(serial)
	{

	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // ver
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		m_Instancing = new Dictionary<Item, Mobile>();
	}
}