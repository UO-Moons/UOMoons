using Server.Items;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Engines.Distillation;

public class DistillationContext
{
	public Group LastGroup { get; set; }

	public Liquor LastLiquor { get; set; }

	public Yeast[] SelectedYeast { get; } = new Yeast[4];

	public bool MakeStrong { get; set; }

	public bool Mark { get; set; }

	public string Label { get; set; }

	public DistillationContext()
	{
		LastGroup = Group.WheatBased;
		LastLiquor = Liquor.None;
		MakeStrong = false;
		Mark = true;
		Label = null;
	}

	public bool YeastInUse(Yeast yeast)
	{
		return SelectedYeast.Any(y => y != null && y == yeast);
	}

	public void ClearYeasts()
	{
		for (int i = 0; i < SelectedYeast.Length; i++)
		{
			SelectedYeast[i] = null;
		}
	}

	public DistillationContext(GenericReader reader)
	{
		reader.ReadInt();

		LastGroup = (Group)reader.ReadInt();
		LastLiquor = (Liquor)reader.ReadInt();
		MakeStrong = reader.ReadBool();
		Mark = reader.ReadBool();
		Label = reader.ReadString();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write((int)LastGroup);
		writer.Write((int)LastLiquor);
		writer.Write(MakeStrong);
		writer.Write(Mark);
		writer.Write(Label);
	}

	#region Serialize/Deserialize Persistence
	private static readonly string FilePath = Path.Combine("Saves", "CraftContext", "DistillationContexts.bin");

	public static void Configure()
	{
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

				writer.Write(DistillationSystem.Contexts.Count);

				foreach (KeyValuePair<Mobile, DistillationContext> kvp in DistillationSystem.Contexts)
				{
					writer.Write(kvp.Key);
					kvp.Value.Serialize(writer);
				}
			});
	}

	public static void OnLoad()
	{
		Persistence.Deserialize(
			FilePath,
			reader =>
			{
				int version = reader.ReadInt();

				int count = reader.ReadInt();
				for (int i = 0; i < count; i++)
				{
					Mobile m = reader.ReadMobile();
					DistillationContext context = new DistillationContext(reader);

					if (m != null)
						DistillationSystem.Contexts[m] = context;
				}
			});
	}
	#endregion
}
