using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.Quests;

public class QuestWriter
{
	public static int Version(GenericWriter writer, int version)
	{
		if (writer == null)
		{
			return -1;
		}

		writer.Write(0x7FFFFFFF); // Preamble

		writer.Write(version);

		return version;
	}

	public static bool SubWrite(GenericWriter writer, Action<GenericWriter> serializer)
	{
		if (writer == null)
		{
			return false;
		}

		using var s = new MemoryStream();
		var w = new BinaryFileWriter(s, true);

		try
		{
			serializer(w);
		}
		catch (Exception e)
		{
			Console.WriteLine("Quest Save Failure: {0}", Utility.FormatDelegate(serializer));
			Console.WriteLine(e);

			writer.Write(0L);

			return false;
		}
		finally
		{
			w.Flush();
		}

		writer.Write(s.Length);

		s.Position = 0;

		while (s.Position < s.Length)
		{
			writer.Write((byte)s.ReadByte());
		}

		return true;
	}

	public static void Quests(GenericWriter writer, List<BaseQuest> quests)
	{
		if (writer == null)
		{
			return;
		}

		Version(writer, 0);

		if (quests == null)
		{
			writer.Write(0);
			return;
		}

		writer.Write(quests.Count);

		foreach (var quest in quests)
		{
			Type(writer, quest.GetType());

			SubWrite(writer, quest.Serialize);
		}
	}

	public static void Chains(GenericWriter writer, Dictionary<QuestChain, BaseChain> chains)
	{
		if (writer == null)
		{
			return;
		}

		Version(writer, 0);

		if (chains == null)
		{
			writer.Write(0);
			return;
		}

		writer.Write(chains.Count);

		foreach (var pair in chains)
		{
			writer.Write((int)pair.Key);

			Type(writer, pair.Value.CurrentQuest);
			Type(writer, pair.Value.Quester);
		}
	}

	public static void Object(GenericWriter writer, object obj)
	{
		if (writer == null)
		{
			return;
		}

		Version(writer, 0);

		switch (obj)
		{
			case int i:
				writer.Write((byte)0x1);
				writer.Write(i);
				break;
			case string s:
				writer.Write((byte)0x2);
				writer.Write(s);
				break;
			case Item item:
				writer.Write((byte)0x3);
				writer.Write(item);
				break;
			case Mobile mobile:
				writer.Write((byte)0x4);
				writer.Write(mobile);
				break;
			default:
				writer.Write((byte)0x0); // invalid
				break;
		}
	}

	public static void Type(GenericWriter writer, Type type)
	{
		writer?.Write(type?.FullName);
	}
}
