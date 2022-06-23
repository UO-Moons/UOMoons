using Server.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Server.Commands;

public class Categorization
{
	private static CategoryEntry _mRootItems, _mRootMobiles;

	public static CategoryEntry Items
	{
		get
		{
			if (_mRootItems == null)
				Load();

			return _mRootItems;
		}
	}

	public static CategoryEntry Mobiles
	{
		get
		{
			if (_mRootMobiles == null)
				Load();

			return _mRootMobiles;
		}
	}

	public static void Initialize()
	{
		CommandSystem.Register("RebuildCategorization", AccessLevel.Administrator, RebuildCategorization_OnCommand);
	}

	[Usage("RebuildCategorization")]
	[Description("Rebuilds the categorization data file used by the Add command.")]
	public static void RebuildCategorization_OnCommand(CommandEventArgs e)
	{
		CategoryEntry root = new(null, "Add Menu", new[] { Items, Mobiles });

		Export(root, "Data/objects.xml", "Objects");

		e.Mobile.SendMessage("Categorization menu rebuilt.");
	}

	public static void RecurseFindCategories(CategoryEntry ce, ArrayList list)
	{
		list.Add(ce);

		for (int i = 0; i < ce.SubCategories.Length; ++i)
			RecurseFindCategories(ce.SubCategories[i], list);
	}

	public static void Export(CategoryEntry ce, string fileName, string title)
	{
		XmlTextWriter xml = new(fileName, System.Text.Encoding.UTF8)
		{
			Indentation = 1,
			IndentChar = '\t',
			Formatting = Formatting.Indented
		};

		xml.WriteStartDocument(true);

		RecurseExport(xml, ce);

		xml.Flush();
		xml.Close();
	}

	public static void RecurseExport(XmlTextWriter xml, CategoryEntry ce)
	{
		xml.WriteStartElement("category");

		xml.WriteAttributeString("title", ce.Title);

		ArrayList subCats = new(ce.SubCategories);

		subCats.Sort(new CategorySorter());

		for (int i = 0; i < subCats.Count; ++i)
			RecurseExport(xml, (CategoryEntry)subCats[i]);

		ce.Matched.Sort(new CategorySorter());

		for (int i = 0; i < ce.Matched.Count; ++i)
		{
			CategoryTypeEntry cte = (CategoryTypeEntry)ce.Matched[i];

			xml.WriteStartElement("object");

			if (cte != null)
			{
				xml.WriteAttributeString("type", cte.Type.ToString());

				object obj = cte.Object;

				if (obj is Item item)
				{
					int itemId = item.ItemId;

					if (item is BaseAddon addon && addon.Components.Count == 1)
						itemId = addon.Components[0].ItemId;

					if (itemId > TileData.MaxItemValue)
						itemId = 1;

					xml.WriteAttributeString("gfx", XmlConvert.ToString(itemId));

					int hue = item.Hue & 0x7FFF;

					if ((hue & 0x4000) != 0)
						hue = 0;

					if (hue != 0)
						xml.WriteAttributeString("hue", XmlConvert.ToString(hue));

					item.Delete();
				}
				else if (obj is Mobile mob)
				{
					int itemId = ShrinkTable.Lookup(mob, 1);

					xml.WriteAttributeString("gfx", XmlConvert.ToString(itemId));

					int hue = mob.Hue & 0x7FFF;

					if ((hue & 0x4000) != 0)
						hue = 0;

					if (hue != 0)
						xml.WriteAttributeString("hue", XmlConvert.ToString(hue));

					mob.Delete();
				}
			}

			xml.WriteEndElement();
		}

		xml.WriteEndElement();
	}

	public static void Load()
	{
		ArrayList types = new();

		AddTypes(Core.Assembly, types);

		for (int i = 0; i < Assembler.Assemblies.Length; ++i)
			AddTypes(Assembler.Assemblies[i], types);

		_mRootItems = Load(types, "Data/items.cfg");
		_mRootMobiles = Load(types, "Data/mobiles.cfg");
	}

	private static CategoryEntry Load(IList types, string config)
	{
		CategoryLine[] lines = CategoryLine.Load(config);

		if (lines.Length > 0)
		{
			int index = 0;
			CategoryEntry root = new(null, lines, ref index);

			Fill(root, types);

			return root;
		}

		return new CategoryEntry();
	}

	private static readonly Type TypeofItem = typeof(Item);
	private static readonly Type TypeofMobile = typeof(Mobile);
	private static readonly Type TypeofConstructable = typeof(ConstructableAttribute);

	private static bool IsConstructable(Type type)
	{
		if (!type.IsSubclassOf(TypeofItem) && !type.IsSubclassOf(TypeofMobile))
			return false;

		ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);

		return (ctor != null && ctor.IsDefined(TypeofConstructable, false));
	}

	private static void AddTypes(Assembly asm, IList types)
	{
		Type[] allTypes = asm.GetTypes();

		for (int i = 0; i < allTypes.Length; ++i)
		{
			Type type = allTypes[i];

			if (type.IsAbstract)
				continue;

			if (IsConstructable(type))
				types.Add(type);
		}
	}

	private static void Fill(CategoryEntry root, IList list)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			Type type = (Type)list[i];
			CategoryEntry match = GetDeepestMatch(root, type);

			if (match == null)
				continue;

			try
			{
				match.Matched.Add(new CategoryTypeEntry(type));
			}
			catch
			{
				// ignored
			}
		}
	}

	private static CategoryEntry GetDeepestMatch(CategoryEntry root, Type type)
	{
		if (!root.IsMatch(type))
			return null;

		for (int i = 0; i < root.SubCategories.Length; ++i)
		{
			CategoryEntry check = GetDeepestMatch(root.SubCategories[i], type);

			if (check != null)
				return check;
		}

		return root;
	}
}

public class CategorySorter : IComparer
{
	public int Compare(object x, object y)
	{
		var a = x switch
		{
			CategoryEntry entry => entry.Title,
			CategoryTypeEntry typeEntry => typeEntry.Type.Name,
			_ => null
		};

		var b = y switch
		{
			CategoryEntry entry1 => entry1.Title,
			CategoryTypeEntry typeEntry2 => typeEntry2.Type.Name,
			_ => null
		};

		switch (a)
		{
			case null when b == null:
				return 0;
			case null:
				return 1;
		}

		if (b == null)
			return -1;

		return string.Compare(a, b, StringComparison.Ordinal);
	}
}

public class CategoryTypeEntry
{
	public Type Type { get; }
	public object Object { get; }

	public CategoryTypeEntry(Type type)
	{
		Type = type;
		Object = Activator.CreateInstance(type);
	}
}

public class CategoryEntry
{
	public string Title { get; }
	public Type[] Matches { get; }
	public CategoryEntry Parent { get; }
	public CategoryEntry[] SubCategories { get; }
	public ArrayList Matched { get; }

	public CategoryEntry()
	{
		Title = "(empty)";
		Matches = Array.Empty<Type>();
		SubCategories = Array.Empty<CategoryEntry>();
		Matched = new ArrayList();
	}

	public CategoryEntry(CategoryEntry parent, string title, CategoryEntry[] subCats)
	{
		Parent = parent;
		Title = title;
		SubCategories = subCats;
		Matches = Array.Empty<Type>();
		Matched = new ArrayList();
	}

	public bool IsMatch(Type type)
	{
		bool isMatch = false;

		for (int i = 0; !isMatch && i < Matches.Length; ++i)
			isMatch = type == Matches[i] || type.IsSubclassOf(Matches[i]);

		return isMatch;
	}

	public CategoryEntry(CategoryEntry parent, IReadOnlyList<CategoryLine> lines, ref int index)
	{
		Parent = parent;

		string text = lines[index].Text;

		int start = text.IndexOf('(');

		if (start < 0)
			throw new FormatException($"Input string not correctly formatted ('{text}')");

		Title = text[..start].Trim();

		int end = text.IndexOf(')', ++start);

		if (end < start)
			throw new FormatException($"Input string not correctly formatted ('{text}')");

		text = text[start..end];
		string[] split = text.Split(';');

		ArrayList list = new();

		for (int i = 0; i < split.Length; ++i)
		{
			Type type = Assembler.FindTypeByName(split[i].Trim());

			if (type == null)
				Console.WriteLine("Match type not found ('{0}')", split[i].Trim());
			else
				list.Add(type);
		}

		Matches = (Type[])list.ToArray(typeof(Type));
		list.Clear();

		int ourIndentation = lines[index].Indentation;

		++index;

		while (index < lines.Count && lines[index].Indentation > ourIndentation)
			list.Add(new CategoryEntry(this, lines, ref index));

		SubCategories = (CategoryEntry[])list.ToArray(typeof(CategoryEntry));
		list.Clear();

		Matched = list;
	}
}

public class CategoryLine
{
	public int Indentation { get; }
	public string Text { get; }

	public CategoryLine(string input)
	{
		int index;

		for (index = 0; index < input.Length; ++index)
		{
			if (char.IsLetter(input, index))
				break;
		}

		if (index >= input.Length)
			throw new FormatException($"Input string not correctly formatted ('{input}')");

		Indentation = index;
		Text = input[index..];
	}

	public static CategoryLine[] Load(string path)
	{
		ArrayList list = new();

		if (File.Exists(path))
		{
			using StreamReader ip = new(path);

			while (ip.ReadLine() is { } line)
				list.Add(new CategoryLine(line));
		}

		return (CategoryLine[])list.ToArray(typeof(CategoryLine));
	}
}
