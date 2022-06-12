using Server.Engines.Quests;
using Server.Mobiles;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Services.Community_Collections
{
	public class CollectionsSystem
	{
		private static readonly Dictionary<Collection, CollectionData> m_Collections = new();
		private static List<BaseCollectionMobile> m_Mobiles = new();
		private static readonly string m_Path = Path.Combine("Saves", "CommunityCollections.bin");

		public static void Configure()
		{
			EventSink.OnWorldSave += EventSink_WorldSave;
			EventSink.OnWorldLoad += EventSink_WorldLoad;
		}

		public static void RegisterMobile(BaseCollectionMobile mob)
		{
			if (!m_Mobiles.Contains(mob))
			{
				m_Mobiles.Add(mob);
				if (m_Collections.ContainsKey(mob.CollectionID))
					mob.SetData(m_Collections[mob.CollectionID]);
			}
		}

		public static void UnregisterMobile(BaseCollectionMobile mob)
		{
			m_Collections[mob.CollectionID] = mob.GetData();
			m_Mobiles.Remove(mob);
		}

		private static void EventSink_WorldSave()
		{
			List<BaseCollectionMobile> newMobiles = new();
			foreach (BaseCollectionMobile mob in m_Mobiles)
			{
				if (!mob.Deleted)
					newMobiles.Add(mob);
			}
			m_Mobiles = newMobiles;

			Persistence.Serialize(
				m_Path,
				writer =>
				{
					writer.WriteMobileList(m_Mobiles);
					writer.Write(m_Mobiles.Count);
					foreach (BaseCollectionMobile mob in m_Mobiles)
					{
						writer.Write((int)mob.CollectionID);
						CollectionData data = mob.GetData();
						data.Write(writer);
						m_Collections[mob.CollectionID] = data;
					}
				});
		}

		private static void EventSink_WorldLoad()
		{
			Persistence.Deserialize(
				m_Path,
				reader =>
				{
					m_Mobiles.AddRange(reader.ReadMobileList().Cast<BaseCollectionMobile>());
					List<BaseCollectionMobile> mobs = new();
					mobs.AddRange(m_Mobiles);

					int count = reader.ReadInt();
					for (int i = 0; i < count; ++i)
					{
						int collection = reader.ReadInt();
						CollectionData data = new();
						data.Read(reader);
						int toRemove = -1;
						foreach (BaseCollectionMobile mob in mobs)
						{
							if (mob.CollectionID == (Collection)collection)
							{
								mob.SetData(data);
								toRemove = mobs.IndexOf(mob);
								break;
							}
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

			for (int i = 0; i < Tiers.Count; i++)
			{
				writer.Write(Tiers[i].Count);

				for (int j = 0; j < Tiers[i].Count; j++)
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

			for (int i = reader.ReadInt(); i > 0; i--)
			{
				List<object> list = new();

				for (int j = reader.ReadInt(); j > 0; j--)
					list.Add(QuestReader.Object(reader));

				Tiers.Add(list);
			}
		}
	}
}
