using System.Collections.Generic;
using Server.Gumps;
using System.IO;

namespace Server.Items;

public class ReforgingContext
{
	public Dictionary<BaseTool, ReforgingInfo> Contexts { get; }

	private ReforgingContext(Mobile m)
	{
		Contexts = new Dictionary<BaseTool, ReforgingInfo>();

		ReforgingContexts[m] = this;
	}

	private ReforgingContext(GenericReader reader)
	{
		Contexts = new Dictionary<BaseTool, ReforgingInfo>();

		var version = reader.ReadInt();

		switch (version)
		{
			case 1:
				var count = reader.ReadInt();

				for (var i = 0; i < count; i++)
				{
					var info = new ReforgingInfo((ReforgingOption)reader.ReadInt(), (ReforgedPrefix)reader.ReadInt(), (ReforgedSuffix)reader.ReadInt());

					if (reader.ReadItem() is BaseTool tool)
					{
						Contexts[tool] = info;
					}
				}
				break;
			case 0:
				reader.ReadInt();
				reader.ReadInt();

				var count2 = reader.ReadInt();
				for (var i = 0; i < count2; i++)
				{
					var option = (ReforgingOption)reader.ReadInt();

					if (reader.ReadItem() is BaseTool tool)
						Contexts[tool] = new ReforgingInfo(option);
				}
				break;
		}
	}

	private void Serialize(GenericWriter writer)
	{
		writer.Write(1);

		writer.Write(Contexts.Count);
		foreach (var kvp in Contexts)
		{
			writer.Write(kvp.Key);
			writer.Write((int)kvp.Value.Options);
			writer.Write((int)kvp.Value.Prefix);
			writer.Write((int)kvp.Value.Suffix);
		}
	}

	#region Serialize/Deserialize Persistence
	private static readonly string FilePath = Path.Combine("Saves", "CraftContext", "ReforgingContexts.bin");

	private static Dictionary<Mobile, ReforgingContext> ReforgingContexts { get; set; }

	public static ReforgingContext GetContext(Mobile m)
	{
		if (ReforgingContexts.ContainsKey(m))
		{
			return ReforgingContexts[m];
		}

		return new ReforgingContext(m);
	}

	public static void Configure()
	{
		EventSink.OnWorldSave += OnSave;
		EventSink.OnWorldLoad += OnLoad;
	}

	private static void OnSave()
	{
		Persistence.Serialize(
			FilePath,
			writer =>
			{
				writer.Write(0); // version

				writer.Write(ReforgingContexts.Count);

				foreach (var kvp in ReforgingContexts)
				{
					writer.Write(kvp.Key);
					kvp.Value.Serialize(writer);
				}
			});
	}

	private static void OnLoad()
	{
		ReforgingContexts = new Dictionary<Mobile, ReforgingContext>();

		Persistence.Deserialize(
			FilePath,
			reader =>
			{
				reader.ReadInt();

				var count = reader.ReadInt();
				for (var i = 0; i < count; i++)
				{
					var m = reader.ReadMobile();
					var context = new ReforgingContext(reader);

					if (m != null)
						ReforgingContexts[m] = context;
				}
			});
	}
	#endregion
}

public class ReforgingInfo
{
	public ReforgedPrefix Prefix { get; set; }
	public ReforgedSuffix Suffix { get; set; }
	public ReforgingOption Options { get; set; }

	public ReforgingInfo()
	{
	}

	public ReforgingInfo(ReforgingOption option)
	{
		Options = option;
	}

	public ReforgingInfo(ReforgingOption option, ReforgedPrefix prefix, ReforgedSuffix suffix)
	{
		Options = option;
		Prefix = prefix;
		Suffix = suffix;
	}
}
