using Server.Engines.Plants;
using Server.Items;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.Craft;

public enum CraftMarkOption
{
	MarkItem,
	DoNotMark,
	PromptForMark
}

public enum CraftQuestOption
{
	QuestItem,
	NonQuestItem
}
public class CraftContext
{
	public Mobile Owner { get; }
	public CraftSystem System { get; }

	public List<CraftItem> Items { get; }
	public int LastResourceIndex { get; set; }
	public int LastResourceIndex2 { get; set; }
	public int LastGroupIndex { get; set; }
	public bool DoNotColor { get; set; }
	public CraftMarkOption MarkOption { get; set; }
	public CraftQuestOption QuestOption { get; set; }
	public int MakeTotal { get; set; }
	public PlantHue RequiredPlantHue { get; set; }
	public PlantPigmentHue RequiredPigmentHue { get; set; }

	public CraftContext(Mobile owner, CraftSystem system)
	{
		Owner = owner;
		System = system;

		Items = new List<CraftItem>();
		LastResourceIndex = -1;
		LastResourceIndex2 = -1;
		LastGroupIndex = -1;

		QuestOption = CraftQuestOption.NonQuestItem;
		RequiredPlantHue = PlantHue.None;
		RequiredPigmentHue = PlantPigmentHue.None;

		Contexts.Add(this);
	}

	public CraftItem LastMade => Items.Count > 0 ? Items[0] : null;

	public void OnMade(CraftItem item)
	{
		Items.Remove(item);

		if (Items.Count == 10)
		{
			Items.RemoveAt(9);
		}

		Items.Insert(0, item);
	}

	public virtual void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(Owner);
		writer.Write(GetSystemIndex(System));
		writer.Write(LastResourceIndex);
		writer.Write(LastResourceIndex2);
		writer.Write(LastGroupIndex);
		writer.Write(DoNotColor);
		writer.Write((int)MarkOption);
		writer.Write((int)QuestOption);

		writer.Write(MakeTotal);
	}

	public CraftContext(GenericReader reader)
	{
		_ = reader.ReadInt();

		Items = new List<CraftItem>();

		Owner = reader.ReadMobile();
		int sysIndex = reader.ReadInt();
		LastResourceIndex = reader.ReadInt();
		LastResourceIndex2 = reader.ReadInt();
		LastGroupIndex = reader.ReadInt();
		DoNotColor = reader.ReadBool();
		MarkOption = (CraftMarkOption)reader.ReadInt();
		QuestOption = (CraftQuestOption)reader.ReadInt();

		MakeTotal = reader.ReadInt();

		System = GetCraftSystem(sysIndex);

		if (System == null || Owner == null) return;
		System.AddContext(Owner, this);
		Contexts.Add(this);
	}

	public int GetSystemIndex(CraftSystem system)
	{
		for (var i = 0; i < Systems.Length; i++)
		{
			if (Systems[i] == system)
			{
				return i;
			}
		}

		return -1;
	}

	public CraftSystem GetCraftSystem(int i)
	{
		if (i >= 0 && i < Systems.Length)
		{
			return Systems[i];
		}

		return null;
	}

	#region Serialize/Deserialize Persistence
	private static readonly string FilePath = Path.Combine("Saves", "CraftContext", "Contexts.bin");

	private static readonly List<CraftContext> Contexts = new();
	private static readonly Dictionary<Mobile, AnvilOfArtifactsEntry> AnvilEntries = new Dictionary<Mobile, AnvilOfArtifactsEntry>();
	public static CraftSystem[] Systems { get; } = new CraftSystem[11];

	public static void Configure()
	{
		Systems[0] = DefAlchemy.CraftSystem;
		Systems[1] = DefBlacksmithy.CraftSystem;
		Systems[2] = DefBowFletching.CraftSystem;
		Systems[3] = DefCarpentry.CraftSystem;
		Systems[4] = DefCartography.CraftSystem;
		Systems[5] = DefCooking.CraftSystem;
		Systems[6] = DefGlassblowing.CraftSystem;
		Systems[7] = DefInscription.CraftSystem;
		Systems[8] = DefMasonry.CraftSystem;
		Systems[9] = DefTailoring.CraftSystem;
		Systems[10] = DefTinkering.CraftSystem;

		EventSink.OnWorldSave += OnSave;
		EventSink.OnWorldLoad += OnLoad;
	}

	public static void OnSave()
	{
		Persistence.Serialize(
			FilePath,
			writer =>
			{
				writer.Write(0); // version

				writer.Write(Contexts.Count);
				Contexts.ForEach(c => c.Serialize(writer));
			});
	}

	public static void OnLoad()
	{
		Persistence.Deserialize(
			FilePath,
			reader =>
			{
				reader.ReadInt();

				int count = reader.ReadInt();
				for (var i = 0; i < count; i++)
				{
					new CraftContext(reader);
				}
			});
	}

	public static AnvilOfArtifactsEntry GetAnvilEntry(Mobile m)
	{
		return GetAnvilEntry(m, true);
	}

	public static AnvilOfArtifactsEntry GetAnvilEntry(Mobile m, bool create)
	{
		if (AnvilEntries.ContainsKey(m))
		{
			return AnvilEntries[m];
		}

		if (create)
		{
			var entry = new AnvilOfArtifactsEntry();

			AnvilEntries[m] = entry;

			return entry;
		}

		return null;
	}

	public static bool IsAnvilReady(Mobile m)
	{
		var entry = GetAnvilEntry(m, false);

		if (entry != null)
		{
			return entry.Ready;
		}

		return false;
	}

	public class AnvilOfArtifactsEntry
	{
		private AnvilofArtifactsAddon _Anvil;

		public Dictionary<ResistanceType, int> Exceptional { get; set; }
		public Dictionary<ResistanceType, int> Runic { get; set; }
		public AnvilofArtifactsAddon Anvil
		{
			get { return _Anvil; }
			set
			{
				_Anvil = value;

				if (_Anvil != null)
				{
					Ready = true;
				}
				else
				{
					Ready = false;
				}
			}
		}

		public bool Ready { get; set; }

		public AnvilOfArtifactsEntry()
		{
			Exceptional = CreateArray();
			Runic = CreateArray();

			Ready = false;
		}

		public void Clear(Mobile m)
		{
			var gump = m.FindGump<AnvilofArtifactsGump>();

			if (gump != null)
			{
				gump.Refresh();
			}

			Anvil = null;
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(0);

			writer.Write(Exceptional.Count);

			foreach (var kvp in Exceptional)
			{
				writer.Write((int)kvp.Key);
				writer.Write(kvp.Value);
			}

			writer.Write(Runic.Count);

			foreach (var kvp in Runic)
			{
				writer.Write((int)kvp.Key);
				writer.Write(kvp.Value);
			}

			writer.Write(Ready);
			writer.Write(_Anvil);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.ReadInt();

			var count = reader.ReadInt();

			for (int i = 0; i < count; i++)
			{
				Exceptional[(ResistanceType)reader.ReadInt()] = reader.ReadInt();
			}

			count = reader.ReadInt();

			for (int i = 0; i < count; i++)
			{
				Runic[(ResistanceType)reader.ReadInt()] = reader.ReadInt();
			}

			Ready = reader.ReadBool();
			_Anvil = reader.ReadItem<AnvilofArtifactsAddon>();
		}

		public static Dictionary<ResistanceType, int> CreateArray()
		{
			return new Dictionary<ResistanceType, int>
				{
					{ ResistanceType.Physical, 0 },
					{ ResistanceType.Fire, 0 },
					{ ResistanceType.Cold, 0 },
					{ ResistanceType.Poison, 0 },
					{ ResistanceType.Energy, 0 },
				};
		}
	}
	#endregion
}
