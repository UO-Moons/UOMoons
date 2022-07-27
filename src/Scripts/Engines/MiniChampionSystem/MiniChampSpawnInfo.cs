using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.MiniChamps
{
    public class MiniChampSpawnInfo
    {
        private readonly MiniChamp m_Owner;
        public readonly List<Mobile> Creatures;

        public Type MonsterType { get; }
        public int Killed { get; set; }
        public int Spawned { get; set; }
        public int Required { get; }
        public int MaxSpawned => Required * 2 - 1;
        public bool Done => Killed >= Required;

        public MiniChampSpawnInfo(MiniChamp controller, MiniChampTypeInfo typeInfo)
        {
            m_Owner = controller;

            Required = typeInfo.Required;
            MonsterType = typeInfo.SpawnType;

            Creatures = new List<Mobile>();
            Killed = 0;
            Spawned = 0;
        }

        public bool Slice()
        {
            bool killed = false;
            List<Mobile> list = new(Creatures);

            for (int i = 0; i < list.Count; i++)
            {
                Mobile creature = list[i];

                if (creature == null || creature.Deleted)
                {
                    Creatures.Remove(creature);
                    Killed++;

                    killed = true;
                }
                else if (!creature.InRange(m_Owner.Location, m_Owner.SpawnRange + 10))
                {
                    // bring to home
                    Map map = m_Owner.Map;
                    Point3D loc = map.GetSpawnPosition(m_Owner.Location, m_Owner.SpawnRange);

                    creature.MoveToWorld(loc, map);
                }
            }

            ColUtility.Free(list);
            return killed;
        }

        public bool Respawn()
        {
            bool spawned = false;

            while (Creatures.Count < Required && Spawned < MaxSpawned)
            {
                BaseCreature bc = Activator.CreateInstance(MonsterType) as BaseCreature;

                Map map = m_Owner.Map;
                Point3D loc = map.GetSpawnPosition(m_Owner.Location, m_Owner.SpawnRange);

                if (m_Owner.BossSpawnPoint != Point3D.Zero)
                {
                    loc = m_Owner.BossSpawnPoint;
                }

                if (bc != null)
                {
	                bc.Home = m_Owner.Location;
	                bc.RangeHome = m_Owner.SpawnRange;
	                bc.Tamable = false;
	                bc.OnBeforeSpawn(loc, map);
	                bc.MoveToWorld(loc, map);

	                if (bc.Fame > Utility.Random(100000) || bc is BaseRenowned)
	                {
		                bc.IsRenowned = true;
						DropEssence(bc);
	                }

	                Creatures.Add(bc);
                }

                ++Spawned;

                spawned = true;
            }

            return spawned;
        }

        private void DropEssence(BaseCreature bc)
        {
            Type essenceType = MiniChampInfo.GetInfo(m_Owner.Type).EssenceType;

            Item essence = Loot.Construct(essenceType);

            if (essence != null)
            {
                bc.PackItem(essence);
            }
        }

        public void AddProperties(ObjectPropertyList list, int cliloc)
        {
            list.Add(cliloc, "{0}: Killed {1}/{2}, Spawned {3}/{4}",
                MonsterType.Name, Killed, Required, Spawned, MaxSpawned);
        }

        public void Serialize(GenericWriter writer)
        {
            writer.Write(m_Owner);
            writer.Write(Killed);
            writer.Write(Spawned);
            writer.Write(Required);
            writer.Write(MonsterType.FullName);
            writer.Write(Creatures);
        }

        public MiniChampSpawnInfo(GenericReader reader)
        {
            Creatures = new List<Mobile>();

            m_Owner = reader.ReadItem<MiniChamp>();
            Killed = reader.ReadInt();
            Spawned = reader.ReadInt();
            Required = reader.ReadInt();
            MonsterType = Assembler.FindTypeByFullName(reader.ReadString());
            Creatures = reader.ReadStrongMobileList();
        }
    }
}
