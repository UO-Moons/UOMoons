using Server.Engines.Quests;
using Server.Mobiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Services.Community_Collections;

public class CollectionsSystem
{
	private static readonly Dictionary<Collection, CollectionData> MCollections = new();
	private static List<BaseCollectionMobile> _mMobiles = new();
	private static readonly string MPath = Path.Combine("Saves", "CommunityCollections.bin");

	public static void Configure()
	{
		EventSink.OnWorldSave += EventSink_WorldSave;
		EventSink.OnWorldLoad += EventSink_WorldLoad;
	}

	public static void RegisterMobile(BaseCollectionMobile mob)
	{
		if (_mMobiles.Contains(mob)) return;
		_mMobiles.Add(mob);
		if (MCollections.ContainsKey(mob.CollectionId))
			mob.SetData(MCollections[mob.CollectionId]);
	}

	public static void UnregisterMobile(BaseCollectionMobile mob)
	{
		MCollections[mob.CollectionId] = mob.GetData();
		_mMobiles.Remove(mob);
	}

	private static void EventSink_WorldSave()
	{
		List<BaseCollectionMobile> newMobiles = _mMobiles.Where(mob => !mob.Deleted).ToList();
		_mMobiles = newMobiles;

		Persistence.Serialize(
			MPath,
			writer =>
			{
				writer.WriteMobileList(_mMobiles);
				writer.Write(_mMobiles.Count);
				foreach (BaseCollectionMobile mob in _mMobiles)
				{
					writer.Write((int)mob.CollectionId);
					CollectionData data = mob.GetData();
					data.Write(writer);
					MCollections[mob.CollectionId] = data;
				}
			});
	}

	private static void EventSink_WorldLoad()
	{
		Persistence.Deserialize(
			MPath,
			reader =>
			{
				_mMobiles.AddRange(reader.ReadMobileList().Cast<BaseCollectionMobile>());
				List<BaseCollectionMobile> mobs = new();
				mobs.AddRange(_mMobiles);

				int count = reader.ReadInt();
				for (var i = 0; i < count; ++i)
				{
					int collection = reader.ReadInt();
					CollectionData data = new();
					data.Read(reader);
					int toRemove = -1;
					foreach (var mob in mobs.Where(mob => mob.CollectionId == (Collection)collection))
					{
						mob.SetData(data);
						toRemove = mobs.IndexOf(mob);
						break;
					}
					if (toRemove >= 0)
						mobs.RemoveAt(toRemove);
				}
			});

	}
}

public class CollectionData
{
	public Collection Collection;
	public long Points;
	public long StartTier;
	public long NextTier;
	public long DailyDecay;
	public int Tier;
	public object DonationTitle;
	public List<List<object>> Tiers = new();

	public void Write(GenericWriter writer)
	{
		writer.Write(0); // version

		writer.Write((int)Collection);
		writer.Write(Points);
		writer.Write(StartTier);
		writer.Write(NextTier);
		writer.Write(DailyDecay);
		writer.Write(Tier);

		QuestWriter.Object(writer, DonationTitle);

		writer.Write(Tiers.Count);

		for (var i = 0; i < Tiers.Count; i++)
		{
			writer.Write(Tiers[i].Count);

			for (var j = 0; j < Tiers[i].Count; j++)
				QuestWriter.Object(writer, Tiers[i][j]);
		}
	}

	public void Read(GenericReader reader)
	{
		_ = reader.ReadInt();

		Collection = (Collection)reader.ReadInt();
		Points = reader.ReadLong();
		StartTier = reader.ReadLong();
		NextTier = reader.ReadLong();
		DailyDecay = reader.ReadLong();
		Tier = reader.ReadInt();

		DonationTitle = QuestReader.Object(reader);

		for (var i = reader.ReadInt(); i > 0; i--)
		{
			List<object> list = new();

			for (var j = reader.ReadInt(); j > 0; j--)
				list.Add(QuestReader.Object(reader));

			Tiers.Add(list);
		}
	}
}
