using Server.Gumps;
using Server.Network;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.ConPVP;

public class PreferencesController : BaseItem
{
	//[CommandProperty( AccessLevel.GameMaster )]
	public Preferences Preferences { get; set; }

	public override string DefaultName => "preferences controller";

	[Constructable]
	public PreferencesController() : base(0x1B7A)
	{
		Visible = false;
		Movable = false;

		Preferences = new Preferences();

		if (Preferences.Instance == null)
			Preferences.Instance = Preferences;
		else
			Delete();
	}

	public override void Delete()
	{
		if (Preferences.Instance != Preferences)
			base.Delete();
	}

	public PreferencesController(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		Preferences.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				Preferences = new Preferences(reader);
				Preferences.Instance = Preferences;
				break;
			}
		}
	}
}

public class Preferences
{
	private readonly Hashtable _table;

	public ArrayList Entries { get; }

	public PreferencesEntry Find(Mobile mob)
	{
		PreferencesEntry entry = (PreferencesEntry)_table[mob];

		if (entry == null)
		{
			_table[mob] = entry = new PreferencesEntry(mob, this);
			Entries.Add(entry);
		}

		return entry;
	}

	public static Preferences Instance { get; set; }

	public Preferences()
	{
		_table = new Hashtable();
		Entries = new ArrayList();
	}

	public Preferences(GenericReader reader)
	{
		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				int count = reader.ReadEncodedInt();

				_table = new Hashtable(count);
				Entries = new ArrayList(count);

				for (int i = 0; i < count; ++i)
				{
					PreferencesEntry entry = new(reader, this, version);

					if (entry.Mobile != null)
					{
						_table[entry.Mobile] = entry;
						Entries.Add(entry);
					}
				}

				break;
			}
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0); // version;

		writer.WriteEncodedInt(Entries.Count);

		for (int i = 0; i < Entries.Count; ++i)
			((PreferencesEntry)Entries[i])?.Serialize(writer);
	}
}

public class PreferencesEntry
{
	private readonly Preferences _preferences;

	public Mobile Mobile { get; }
	public ArrayList Disliked { get; }

	public PreferencesEntry(Mobile mob, Preferences prefs)
	{
		_preferences = prefs;
		Mobile = mob;
		Disliked = new ArrayList();
	}

	public PreferencesEntry(GenericReader reader, Preferences prefs, int version)
	{
		_preferences = prefs;

		switch (version)
		{
			case 0:
			{
				Mobile = reader.ReadMobile();

				int count = reader.ReadEncodedInt();

				Disliked = new ArrayList(count);

				for (int i = 0; i < count; ++i)
					Disliked.Add(reader.ReadString());

				break;
			}
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(Mobile);

		writer.WriteEncodedInt(Disliked.Count);

		for (int i = 0; i < Disliked.Count; ++i)
			writer.Write((string)Disliked[i]);
	}
}

public class PreferencesGump : Gump
{
	private readonly Mobile _from;
	private readonly PreferencesEntry _entry;

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_entry == null)
			return;

		if (info.ButtonID != 1)
			return;

		_entry.Disliked.Clear();

		List<Arena> arenas = Arena.Arenas;

		for (int i = 0; i < info.Switches.Length; ++i)
		{
			int idx = info.Switches[i];

			if (idx >= 0 && idx < arenas.Count)
				_entry.Disliked.Add(arenas[idx].Name);
		}
	}

	public PreferencesGump(Mobile from, Preferences prefs) : base(50, 50)
	{
		_from = from;
		_entry = prefs.Find(from);

		if (_entry == null)
			return;

		List<Arena> arenas = Arena.Arenas;

		AddPage(0);

		int height = 12 + 20 + (arenas.Count * 31) + 24 + 12;

		AddBackground(0, 0, 499 + 40 - 365, height, 0x2436);

		for (int i = 1; i < arenas.Count; i += 2)
			AddImageTiled(12, 32 + (i * 31), 475 + 40 - 365, 30, 0x2430);

		AddAlphaRegion(10, 10, 479 + 40 - 365, height - 20);

		AddColumnHeader(35, null);
		AddColumnHeader(115, "Arena");

		AddButton(499 + 40 - 365 - 12 - 63 - 4 - 63, height - 12 - 24, 247, 248, 1, GumpButtonType.Reply, 0);
		AddButton(499 + 40 - 365 - 12 - 63, height - 12 - 24, 241, 242, 2, GumpButtonType.Reply, 0);

		for (int i = 0; i < arenas.Count; ++i)
		{
			Arena ar = arenas[i];

			string name = ar.Name ?? "(no name)";

			int x = 12;
			int y = 32 + (i * 31);

			const int color = 0xCCFFCC;

			AddCheck(x + 3, y + 1, 9730, 9727, _entry.Disliked.Contains(name), i);
			x += 35;

			AddBorderedText(x + 5, y + 5, 115 - 5, name, color);
		}
	}

	private void AddBorderedText(int x, int y, int width, string text, int color)
	{
		/*AddColoredText( x - 1, y, width, text, borderColor );
		AddColoredText( x + 1, y, width, text, borderColor );
		AddColoredText( x, y - 1, width, text, borderColor );
		AddColoredText( x, y + 1, width, text, borderColor );*/
		/*AddColoredText( x - 1, y - 1, width, text, borderColor );
		AddColoredText( x + 1, y + 1, width, text, borderColor );*/
		AddColoredText(x, y, width, text, color);
	}

	private void AddColoredText(int x, int y, int width, string text, int color)
	{
		AddHtml(x, y, width, 20, color == 0 ? text : Color(text, color), false, false);
	}

	private int _columnX = 12;

	private void AddColumnHeader(int width, string name)
	{
		AddBackground(_columnX, 12, width, 20, 0x242C);
		AddImageTiled(_columnX + 2, 14, width - 4, 16, 0x2430);

		if (name != null)
			AddBorderedText(_columnX, 13, width, Center(name), 0xFFFFFF);

		_columnX += width;
	}
}
