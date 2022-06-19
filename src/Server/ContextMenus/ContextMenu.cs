using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.ContextMenus;

/// <summary>
///     Represents the state of an active context menu. This includes who opened the menu, the menu's focus object, and a list of
///     <see
///         cref="ContextMenuEntry">
///         entries
///     </see>
///     that the menu is composed of.
///     <seealso cref="ContextMenuEntry" />
/// </summary>
public class ContextMenu : IDisposable
{
	public bool IsDisposed { get; private set; }

	/// <summary>
	///     Gets the <see cref="Mobile" /> who opened this ContextMenu.
	/// </summary>
	public Mobile From { get; private set; }

	/// <summary>
	///     Gets an object of the <see cref="Mobile" /> or <see cref="Item" /> for which this ContextMenu is on.
	/// </summary>
	public IEntity Target { get; private set; }

	/// <summary>
	///     Gets the list of <see cref="ContextMenuEntry">entries</see> contained in this ContextMenu.
	/// </summary>
	public ContextMenuEntry[] Entries { get; private set; }

	/// <summary>
	///     Instantiates a new ContextMenu instance.
	/// </summary>
	/// <param name="from">
	///     The <see cref="Mobile" /> who opened this ContextMenu.
	///     <seealso cref="From" />
	/// </param>
	/// <param name="target">
	///     The <see cref="Mobile" /> or <see cref="Item" /> for which this ContextMenu is on.
	///     <seealso cref="Target" />
	/// </param>
	public ContextMenu(Mobile from, IEntity target)
	{
		From = from;
		Target = target;

		var list = new List<ContextMenuEntry>();

		switch (target)
		{
			case Mobile mobile:
				mobile.GetContextMenuEntries(from, list);
				break;
			case Item item:
				item.GetContextMenuEntries(from, list);
				break;
		}

		//EventSink.InvokeContextMenu(new ContextMenuEventArgs(From, Target, list));

		foreach (var e in list)
		{
			e.Owner = this;
		}

		Entries = list.ToArray();

		list.Clear();
		list.TrimExcess();
	}

	~ContextMenu()
	{
		Dispose();
	}

	/// <summary>
	///     Returns true if this ContextMenu requires packet version 2.
	/// </summary>
	public bool RequiresNewPacket => Entries.Any(t => t.Number < 3000000 || t.Number > 3032767);

	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}

		IsDisposed = true;

		if (Entries != null)
		{
			foreach (var e in Entries.Where(e => e != null))
			{
				e.Dispose();
			}

			Entries = null;
		}

		if (From != null)
		{
			if (From.ContextMenu == this)
			{
				From.ContextMenu = null;
			}

			From = null;
		}

		Target = null;
	}

	public static bool Display(Mobile m, IEntity target)
	{
		if (m == null || target == null || m.Map != target.Map)
		{
			return false;
		}

		switch (target)
		{
			case Mobile when !Utility.InUpdateRange(m, target.Location):
			case Item item when !Utility.InUpdateRange(m, item.GetWorldLocation()):
				return false;
		}

		if (!m.CheckContextMenuDisplay(target))
		{
			return false;
		}

		var c = new ContextMenu(m, target);

		if (c.Entries.Length <= 0)
		{
			return false;
		}

		if (target is Item item1)
		{
			var root = item1.RootParent;

			if (root is Mobile mobile && root != m && mobile.AccessLevel >= m.AccessLevel)
			{
				foreach (var e in c.Entries.Where(e => !e.NonLocalUse))
				{
					e.Enabled = false;
				}
			}
		}

		m.ContextMenu = c;

		return true;
	}

	/// <summary>
	/// Returns the proper index of Enhanced Client Context Menu when sent from the icon on 
	/// the vendors status bar. Only known are Bank, Bulk Order Info and Bribe
	/// </summary>
	/// <param name="index">pre-described index sent by client. Must be 0x64 or higher</param>
	/// <returns>actual index of pre-desribed index from client</returns>
	public int GetIndexEc(int index)
	{
		int number = index switch
		{
			0x0078 => 3006105,
			0x0193 => 3006152,
			0x01A3 => 1152294,
			0x032A => 3000197,
			0x032B => 3000198,
			0x012D => 3006130,
			0x082 => 3006107,
			0x083 => 3006108,
			0x086 => 3006111,
			0x087 => 3006114,
			0x089 => 3006112,
			0x0140 => 1113797,
			0x025A => 3006205,
			0x025C => 3006207,
			0x0196 => 3006156,
			0x0194 => 3006154,
			0x0195 => 3006155,
			0x0321 => 3006169,
			0x01A0 => 1114299,
			0x01A2 => 3006201,
			0x0396 => 1115022,
			0x0393 => 1049594,
			0x0134 => 3006157,
			0x03F2 => 1152531,
			0x03F5 => 1154112,
			0x03F6 => 1154113,
			0x0334 => 3006168,
			_ => index
		};

		if (index >= 0x64)
		{
			for (int i = 0; i < Entries.Length; i++)
			{
				if (Entries[i].Number == number)
				{
					return i;
				}
			}
		}

		return index;
	}
}
