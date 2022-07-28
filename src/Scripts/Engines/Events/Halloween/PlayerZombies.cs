using Server.Items;
using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.Events
{
	public class HalloweenHauntings
	{
		public static Dictionary<PlayerMobile, ZombieSkeleton> ReAnimated { get => _ReAnimated; set => _ReAnimated = value; }

		public static Timer Timer;
		public static Timer ClearTimer;

		public static int TotalZombieLimit;
		public static int DeathQueueLimit;
		public static int QueueDelaySeconds;
		public static int QueueClearIntervalSeconds;

		public static Dictionary<PlayerMobile, ZombieSkeleton> _ReAnimated;

		public static List<PlayerMobile> DeathQueue;

		private static readonly Rectangle2D[] m_Cemetaries = {
			new(1272,3712,30,20), // Jhelom
			new(1337,1444,48,52), // Britain
			new(2424,1098,20,28), // Trinsic
			new(2728,840,54,54), // Vesper
			new(4528,1314,20,28), // Moonglow
			new(712,1104,30,22), // Yew
			new(5824,1464,22,6), // Fire Dungeon
			new(5224,3655,14,5), // T2A

			new(1272,3712,20,30), // Jhelom
			new(1337,1444,52,48), // Britain
			new(2424,1098,28,20), // Trinsic
			new(2728,840,54,54), // Vesper
			new(4528,1314,28,20), // Moonglow
			new(712,1104,22,30), // Yew
			new(5824,1464,6,22), // Fire Dungeon
			new(5224,3655,5,14), // T2A
		};

		public static void EventSink_PlayerDeath(Mobile m, Mobile killer, Container cont)
		{
			if (m is not ({ Deleted: false } and PlayerMobile player))
				return;

			if (Timer.Running && !DeathQueue.Contains(player) && DeathQueue.Count < DeathQueueLimit)
			{
				DeathQueue.Add(player);
			}
		}

		public static void Clear_Callback()
		{
			_ReAnimated.Clear();

			DeathQueue.Clear();

			if (DateTime.UtcNow <= TrickOrTreat.FinishHalloween)
			{
				ClearTimer.Stop();
			}
		}

		public static void Timer_Callback()
		{
			PlayerMobile player = null;

			if (DateTime.UtcNow <= TrickOrTreat.FinishHalloween)
			{
				for (int index = 0; DeathQueue.Count > 0 && index < DeathQueue.Count; index++)
				{
					if (_ReAnimated.ContainsKey(DeathQueue[index]))
						continue;

					player = DeathQueue[index];

					break;
				}

				if (player is { Deleted: false } && _ReAnimated.Count < TotalZombieLimit)
				{
					Map map = Utility.RandomBool() ? Map.Trammel : Map.Felucca;

					Point3D home = (GetRandomPointInRect(m_Cemetaries[Utility.Random(m_Cemetaries.Length)], map));

					if (!map.CanSpawnMobile(home))
						return;
					ZombieSkeleton zombieskel = new(player);

					_ReAnimated.Add(player, zombieskel);
					zombieskel.Home = home;
					zombieskel.RangeHome = 10;

					zombieskel.MoveToWorld(home, map);

					DeathQueue.Remove(player);
				}
			}
			else
			{
				Timer.Stop();
			}
		}

		private static Point3D GetRandomPointInRect(Rectangle2D rect, Map map)
		{
			int x = Utility.Random(rect.X, rect.Width);
			int y = Utility.Random(rect.Y, rect.Height);

			return new Point3D(x, y, map.GetAverageZ(x, y));
		}
	}

	public class PlayerBones : BaseContainer
	{
		[Constructable]
		public PlayerBones(string name)
			: base(Utility.RandomMinMax(0x0ECA, 0x0ED2))
		{
			Name = $"{name}'s bones";

			Hue = Utility.Random(10) switch
			{
				0 => 0xa09,
				1 => 0xa93,
				2 => 0xa47,
				_ => Hue
			};
		}

		public PlayerBones(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[CorpseName("a rotting corpse")]
	public class ZombieSkeleton : BaseCreature
	{
		private const string _Name = "Zombie Skeleton";

		private PlayerMobile m_DeadPlayer;

		public ZombieSkeleton()
			: this(null)
		{
		}

		public ZombieSkeleton(PlayerMobile player)
			: base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			m_DeadPlayer = player;

			Name = player != null ? $"{player.Name}'s {_Name}" : _Name;

			Body = 0x93;
			BaseSoundId = 0x1c3;

			SetStr(500);
			SetDex(500);
			SetInt(500);

			SetHits(2500);
			SetMana(500);
			SetStam(500);

			SetDamage(8, 18);

			SetDamageType(ResistanceType.Physical, 40);
			SetDamageType(ResistanceType.Cold, 60);

			SetResistance(ResistanceType.Fire, 50);
			SetResistance(ResistanceType.Energy, 50);
			SetResistance(ResistanceType.Physical, 50);
			SetResistance(ResistanceType.Cold, 50);
			SetResistance(ResistanceType.Poison, 50);

			SetSkill(SkillName.MagicResist, 65.1, 80.0);
			SetSkill(SkillName.Tactics, 95.1, 100);
			SetSkill(SkillName.Wrestling, 85.1, 95);

			Fame = 1000;
			Karma = -1000;

			VirtualArmor = 18;
		}

		public override void GenerateLoot()
		{
			switch (Utility.Random(10))
			{
				case 0: PackItem(new LeftArm()); break;
				case 1: PackItem(new RightArm()); break;
				case 2: PackItem(new Torso()); break;
				case 3: PackItem(new Bone()); break;
				case 4: PackItem(new RibCage()); break;
				case 5: if (m_DeadPlayer is { Deleted: false }) { PackItem(new PlayerBones(m_DeadPlayer.Name)); } break;
			}

			AddLoot(LootPack.Meager);
		}

		public override bool BleedImmune => true;

		public override Poison PoisonImmune => Poison.Regular;

		public ZombieSkeleton(Serial serial)
			: base(serial)
		{
		}

		public override void OnDelete()
		{
			if (HalloweenHauntings.ReAnimated == null)
				return;

			if (m_DeadPlayer is not { Deleted: false })
				return;

			if (HalloweenHauntings.ReAnimated.Count > 0 && HalloweenHauntings.ReAnimated.ContainsKey(m_DeadPlayer))
			{
				HalloweenHauntings.ReAnimated.Remove(m_DeadPlayer);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(m_DeadPlayer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			m_DeadPlayer = (PlayerMobile)reader.ReadMobile();
		}
	}
}
