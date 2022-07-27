using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class ExperimentalRoomController : Item
{
	private static ExperimentalRoomController Instance { get; set; }

	private static Dictionary<Mobile, DateTime> _table;

	public ExperimentalRoomController() : base(7107)
	{
		_table = new Dictionary<Mobile, DateTime>();
		Visible = false;
		Movable = false;

		Instance = this;
	}

	public static void AddToTable(Mobile from)
	{
		if (from == null)
			return;

		_table[from] = DateTime.UtcNow + TimeSpan.FromHours(24);
	}

	public static bool IsInCooldown(Mobile from)
	{
		Defrag();

		return _table.ContainsKey(from);
	}

	private static void Defrag()
	{
		List<Mobile> list = (from kvp in _table where kvp.Value <= DateTime.UtcNow select kvp.Key).ToList();

		foreach (Mobile m in list)
			_table.Remove(m);
	}

	public static bool HasItem(Mobile from, Type type)
	{
		if (from == null || from.Backpack == null)
			return false;

		Item item = from.Backpack.FindItemByType(type);

		return item != null;
	}

	public ExperimentalRoomController(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		Defrag();
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		_table = new Dictionary<Mobile, DateTime>();

		Instance = this;
	}
}
