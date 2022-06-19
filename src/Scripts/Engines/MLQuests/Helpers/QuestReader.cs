using System;
using System.Collections.Generic;
using System.IO;
using Server.Mobiles;

namespace Server.Engines.Quests;

public class QuestReader
{
	public static int Version(GenericReader reader)
	{
		if (reader == null)
		{
			return -1;
		}

		if (reader.PeekInt() != 0x7FFFFFFF) return -1;
		reader.ReadInt(); // Preamble 0x7FFFFFFF

		return reader.ReadInt();

	}

	public static bool SubRead(GenericReader reader, Action<GenericReader> deserializer)
	{
		if (reader == null)
		{
			return false;
		}

		using var s = new MemoryStream();
		var length = reader.ReadLong();

		while (s.Length < length)
		{
			s.WriteByte(reader.ReadByte());
		}

		if (deserializer == null) return true;
		s.Position = 0;

		var r = new BinaryFileReader(new BinaryReader(s));

		try
		{
			deserializer(r);
		}
		catch (Exception e)
		{
			Console.WriteLine("Quest Load Failure: {0}", Utility.FormatDelegate(deserializer));
			Console.WriteLine(e);

			return false;
		}
		finally
		{
			r.Close();
		}

		return true;
	}

	public static List<BaseQuest> Quests(GenericReader reader, PlayerMobile player)
	{
		var quests = new List<BaseQuest>();

		if (reader == null)
		{
			return quests;
		}

		var version = Version(reader);

		var count = reader.ReadInt();

		for (var i = 0; i < count; i++)
		{
			if (Construct(reader) is not BaseQuest quest)
			{
				if (version >= 0)
				{
					SubRead(reader, null);
				}

				continue;
			}

			quest.Owner = player;

			if (version < 0)
			{
				quest.Deserialize(reader);
			}
			else if (!SubRead(reader, quest.Deserialize))
			{
				continue;
			}

			quests.Add(quest);
		}

		return quests;
	}

	public static Dictionary<QuestChain, BaseChain> Chains(GenericReader reader)
	{
		var chains = new Dictionary<QuestChain, BaseChain>();

		if (reader == null)
		{
			return chains;
		}

		Version(reader);

		var count = reader.ReadInt();

		for (var i = 0; i < count; i++)
		{
			var chain = reader.ReadInt();
			var quest = Type(reader);
			var quester = Type(reader);

			if (Enum.IsDefined(typeof(QuestChain), chain) && quest != null && quester != null)
			{
				chains[(QuestChain)chain] = new BaseChain(quest, quester);
			}
		}

		return chains;
	}

	public static object Object(GenericReader reader)
	{
		if (reader == null)
		{
			return null;
		}

		Version(reader);

		var type = reader.ReadByte();

		return type switch
		{
			0x0 => null // invalid
			,
			0x1 => reader.ReadInt(),
			0x2 => reader.ReadString(),
			0x3 => reader.ReadItem(),
			0x4 => reader.ReadMobile(),
			_ => null
		};
	}

	public static Type Type(GenericReader reader)
	{
		var type = reader?.ReadString();

		return type != null ? Assembler.FindTypeByFullName(type, false) : null;
	}

	public static object Construct(GenericReader reader)
	{
		var type = Type(reader);

		try
		{
			return Activator.CreateInstance(type);
		}
		catch
		{
			return null;
		}
	}
}
