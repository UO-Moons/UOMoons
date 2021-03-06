using System.Collections.Generic;
using System.IO;
using Server.Mobiles;

namespace Server.Engines.Quests;

public static class MondainQuestData
{
	public static readonly string FilePath = Path.Combine("Saves/Quests", "MLQuests.bin");

	public static Dictionary<PlayerMobile, List<BaseQuest>> QuestData { get; set; }
	public static Dictionary<PlayerMobile, Dictionary<QuestChain, BaseChain>> ChainData { get; set; }

	public static List<BaseQuest> GetQuests(PlayerMobile pm)
	{
		if (!QuestData.ContainsKey(pm))
		{
			QuestData[pm] = new List<BaseQuest>();
		}

		return QuestData[pm];
	}

	public static Dictionary<QuestChain, BaseChain> GetChains(PlayerMobile pm)
	{
		if (!ChainData.ContainsKey(pm))
		{
			ChainData[pm] = new Dictionary<QuestChain, BaseChain>();
		}

		return ChainData[pm];
	}

	public static void AddQuest(PlayerMobile pm, BaseQuest q)
	{
		if (!QuestData.ContainsKey(pm) || QuestData[pm] == null)
			QuestData[pm] = new List<BaseQuest>();

		QuestData[pm].Add(q);
	}

	public static void AddChain(PlayerMobile pm, QuestChain id, BaseChain chain)
	{
		if (pm == null)
			return;

		if (!ChainData.ContainsKey(pm) || ChainData[pm] == null)
			ChainData[pm] = new Dictionary<QuestChain, BaseChain>();

		ChainData[pm].Add(id, chain);
	}

	public static void RemoveQuest(PlayerMobile pm, BaseQuest quest)
	{
		if (!QuestData.ContainsKey(pm) || !QuestData[pm].Contains(quest)) return;
		QuestData[pm].Remove(quest);

		if (QuestData[pm].Count == 0)
			QuestData.Remove(pm);
	}

	public static void RemoveChain(PlayerMobile pm, QuestChain chain)
	{
		if (!ChainData.ContainsKey(pm) || !ChainData[pm].ContainsKey(chain)) return;
		ChainData[pm].Remove(chain);

		if (ChainData[pm].Count == 0)
			ChainData.Remove(pm);
	}

	public static void Configure()
	{
		EventSink.OnWorldSave += OnSave;
		EventSink.OnWorldLoad += OnLoad;

		QuestData = new Dictionary<PlayerMobile, List<BaseQuest>>();
		ChainData = new Dictionary<PlayerMobile, Dictionary<QuestChain, BaseChain>>();
	}

	public static void OnSave()
	{
		Persistence.Serialize(
			FilePath,
			writer =>
			{
				writer.Write(0);

				writer.Write(QuestData.Count);
				foreach (var kvp in QuestData)
				{
					writer.Write(kvp.Key);
					QuestWriter.Quests(writer, kvp.Value);
				}

				writer.Write(ChainData.Count);
				foreach (var kvp in ChainData)
				{
					writer.Write(kvp.Key);
					QuestWriter.Chains(writer, kvp.Value);
				}

				TierQuestInfo.Save(writer);
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
				for (var i = 0; i < count; i++)
				{
					PlayerMobile pm = reader.ReadMobile() as PlayerMobile;

					List<BaseQuest> quests = QuestReader.Quests(reader, pm);

					if (pm != null)
						QuestData[pm] = quests;
				}

				count = reader.ReadInt();
				for (var i = 0; i < count; i++)
				{
					Dictionary<QuestChain, BaseChain> dic = QuestReader.Chains(reader);

					if (reader.ReadMobile() is PlayerMobile pm)
						ChainData[pm] = dic;
				}

				TierQuestInfo.Load(reader);
			});
	}
}
