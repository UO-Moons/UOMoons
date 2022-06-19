using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.Champions;
using Server.Engines.Craft;
using Server.Engines.Help;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Misc;
using Server.Multis;
using Server.Network;
using Server.Regions;
using Server.SkillHandlers;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Fifth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Guilds;
using Server.Spells.First;

namespace Server.Mobiles
{
	#region Enums
	[Flags]
	public enum PlayerFlag // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
	{
		None = 0x00000000,
		Glassblowing = 0x00000001,
		Masonry = 0x00000002,
		SandMining = 0x00000004,
		StoneMining = 0x00000008,
		ToggleMiningStone = 0x00000010,
		KarmaLocked = 0x00000020,
		AutoRenewInsurance = 0x00000040,
		UseOwnFilter = 0x00000080,
		Unused = 0x00000100,
		PagingSquelched = 0x00000200,
		Young = 0x00000400,
		AcceptGuildInvites = 0x00000800,
		DisplayChampionTitle = 0x00001000,
		HasStatReward = 0x00002000,
		Bedlam = 0x00010000,
		LibraryFriend = 0x00020000,
		Spellweaving = 0x00040000,
		GemMining = 0x00080000,
		ToggleMiningGem = 0x00100000,
		BasketWeaving = 0x00200000,
		AbyssEntry = 0x00400000,
		ToggleClippings = 0x00800000,
		ToggleCutClippings = 0x01000000,
		ToggleCutReeds = 0x02000000,
		MechanicalLife = 0x04000000,
		Unusesd = 0x08000000,
		ToggleCutTopiaries = 0x10000000,
		HasValiantStatReward = 0x20000000,
		RefuseTrades = 0x40000000,
	}

	[Flags]
	public enum ExtendedPlayerFlag
	{
		Unused = 0x00000001,
		ToggleStoneOnly = 0x00000002,
		CanBuyCarpets = 0x00000004,
		VoidPool = 0x00000008,
		DisabledPvpWarning = 0x00000010,
	}

	public enum NpcGuild
	{
		None,
		MagesGuild,
		WarriorsGuild,
		ThievesGuild,
		RangersGuild,
		HealersGuild,
		MinersGuild,
		MerchantsGuild,
		TinkersGuild,
		TailorsGuild,
		FishermensGuild,
		BardsGuild,
		BlacksmithsGuild
	}

	public enum SolenFriendship
	{
		None,
		Red,
		Black
	}

	#endregion

	public partial class PlayerMobile : BaseMobile, IHonorTarget
	{
		private static readonly TimeSpan MKillShortTermDelay = TimeSpan.FromHours(Settings.Configuration.Get<double>("Gameplay", "KillShortTermDelay"));
		private static readonly TimeSpan MKillLongTermDelay = TimeSpan.FromHours(Settings.Configuration.Get<double>("Gameplay", "KillLongTermDelay"));

		public static List<PlayerMobile> Instances { get; }

		static PlayerMobile()
		{
			Instances = new List<PlayerMobile>(0x1000);
		}

		#region Mount Blocking
		public void SetMountBlock(BlockMountType type, TimeSpan duration, bool dismount)
		{
			if (dismount)
			{
				BaseMount.BaseDismount(this, this, type, duration, false);
			}
			else
			{
				BaseMount.SetMountPrevention(this, type, duration);
			}
		}
		#endregion

		#region Stygian Abyss
		public override void ToggleFlying()
		{
			if (Race != Race.Gargoyle)
				return;

			if (Frozen)
			{
				SendLocalizedMessage(1060170); // You cannot use this ability while frozen.
				return;
			}

			if (!Flying)
			{
				if (BeginAction(typeof(FlySpell)))
				{
					if (Spell is Spell)
						((Spell)Spell).Disturb(DisturbType.Unspecified, false, false);

					Spell spell = new FlySpell(this);
					spell.Cast();

					Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
				}
				else
				{
					LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
				}
			}
			else if (IsValidLandLocation(Location, Map))
			{
				if (BeginAction(typeof(FlySpell)))
				{
					if (Spell is Spell)
						((Spell)Spell).Disturb(DisturbType.Unspecified, false, false);

					Animate(AnimationType.Land, 0);
					Flying = false;
					BuffInfo.RemoveBuff(this, BuffIcon.Fly);

					Timer.DelayCall(TimeSpan.FromSeconds(3), () => EndAction(typeof(FlySpell)));
				}
				else
				{
					LocalOverheadMessage(MessageType.Regular, 0x3B2, 1075124); // You must wait before casting that spell again.
				}
			}
			else
				LocalOverheadMessage(MessageType.Regular, 0x3B2, 1113081); // You may not land here.
		}

		public static bool IsValidLandLocation(Point3D p, Map map)
		{
			return map.CanFit(p.X, p.Y, p.Z, 16, false, false);
		}
		#endregion

		private class CountAndTimeStamp
		{
			private int _mCount;

			public CountAndTimeStamp()
			{
			}

			public DateTime TimeStamp { get; private set; }
			public int Count
			{
				get => _mCount;
				set { _mCount = value; TimeStamp = DateTime.UtcNow; }
			}
		}

		private bool _mIgnoreMobiles; // IgnoreMobiles should be moved to Server.Mobiles
		private int _mNonAutoreinsuredItems; // number of items that could not be automatically reinsured because gold in bank was not enough
		private Guilds.RankDefinition _mGuildRank;
		private List<Mobile> _mAllFollowers;

		#region Getters & Setters

		public List<Mobile> RecentlyReported { get; set; }

		public DateTime LastMacroCheck { get; set; }

		public List<Mobile> AutoStabled { get; private set; }

		public bool NinjaWepCooldown { get; set; }

		public List<Mobile> AllFollowers
		{
			get { return _mAllFollowers ??= new List<Mobile>(); }
		}

		public Server.Guilds.RankDefinition GuildRank
		{
			get => AccessLevel >= AccessLevel.GameMaster ? Server.Guilds.RankDefinition.Leader : _mGuildRank;
			set => _mGuildRank = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int GuildMessageHue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AllianceMessageHue { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Profession { get; set; }

		public int StepsTaken { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public NpcGuild NpcGuild { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NpcGuildJoinTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextBodTurnInTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastOnline { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public long LastMoved => LastMoveTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NpcGuildGameTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToTItemsTurnedIn { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToTTotalMonsterFame { get; set; }

		public int ExecutesLightningStrike { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RegionGump { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoSaveGump { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ToothAche
		{
			get => CandyCane.GetToothAche(this);
			set => CandyCane.SetToothAche(this, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool MechanicalLife { get => GetFlag(PlayerFlag.MechanicalLife); set => SetFlag(PlayerFlag.MechanicalLife, value); }

		#endregion

		#region PlayerFlags
		public PlayerFlag Flags { get; set; }
		public ExtendedPlayerFlag ExtendedFlags { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PagingSquelched
		{
			get => GetFlag(PlayerFlag.PagingSquelched);
			set => SetFlag(PlayerFlag.PagingSquelched, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Glassblowing
		{
			get => GetFlag(PlayerFlag.Glassblowing);
			set => SetFlag(PlayerFlag.Glassblowing, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Masonry
		{
			get => GetFlag(PlayerFlag.Masonry);
			set => SetFlag(PlayerFlag.Masonry, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SandMining
		{
			get => GetFlag(PlayerFlag.SandMining);
			set => SetFlag(PlayerFlag.SandMining, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StoneMining
		{
			get => GetFlag(PlayerFlag.StoneMining);
			set => SetFlag(PlayerFlag.StoneMining, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool GemMining { get => GetFlag(PlayerFlag.GemMining); set => SetFlag(PlayerFlag.GemMining, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleMiningGem { get => GetFlag(PlayerFlag.ToggleMiningGem); set => SetFlag(PlayerFlag.ToggleMiningGem, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool BasketWeaving { get => GetFlag(PlayerFlag.BasketWeaving); set => SetFlag(PlayerFlag.BasketWeaving, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleMiningStone
		{
			get => GetFlag(PlayerFlag.ToggleMiningStone);
			set => SetFlag(PlayerFlag.ToggleMiningStone, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool KarmaLocked
		{
			get => GetFlag(PlayerFlag.KarmaLocked);
			set => SetFlag(PlayerFlag.KarmaLocked, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AutoRenewInsurance
		{
			get => GetFlag(PlayerFlag.AutoRenewInsurance);
			set => SetFlag(PlayerFlag.AutoRenewInsurance, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool UseOwnFilter
		{
			get => GetFlag(PlayerFlag.UseOwnFilter);
			set => SetFlag(PlayerFlag.UseOwnFilter, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool AcceptGuildInvites
		{
			get => GetFlag(PlayerFlag.AcceptGuildInvites);
			set => SetFlag(PlayerFlag.AcceptGuildInvites, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool HasStatReward
		{
			get => GetFlag(PlayerFlag.HasStatReward);
			set => SetFlag(PlayerFlag.HasStatReward, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RefuseTrades
		{
			get => GetFlag(PlayerFlag.RefuseTrades);
			set => SetFlag(PlayerFlag.RefuseTrades, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanBuyCarpets
		{
			get => GetFlag(ExtendedPlayerFlag.CanBuyCarpets);
			set => SetFlag(ExtendedPlayerFlag.CanBuyCarpets, value);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleStoneOnly
		{
			get => GetFlag(ExtendedPlayerFlag.ToggleStoneOnly);
			set => SetFlag(ExtendedPlayerFlag.ToggleStoneOnly, value);
		}
		#endregion

		#region UOLRB
		#region Plant system
		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleClippings { get => GetFlag(PlayerFlag.ToggleClippings); set => SetFlag(PlayerFlag.ToggleClippings, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleCutReeds { get => GetFlag(PlayerFlag.ToggleCutReeds); set => SetFlag(PlayerFlag.ToggleCutReeds, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleCutClippings { get => GetFlag(PlayerFlag.ToggleCutClippings); set => SetFlag(PlayerFlag.ToggleCutClippings, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ToggleCutTopiaries { get => GetFlag(PlayerFlag.ToggleCutTopiaries); set => SetFlag(PlayerFlag.ToggleCutTopiaries, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SsNextSeed { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SsSeedExpire { get; set; }

		public Point3D SsSeedLocation { get; set; }

		public Map SsSeedMap { get; set; }
		#endregion
		#endregion

		#region Auto Arrow Recovery
		public Dictionary<Type, int> RecoverableAmmo { get; } = new();

		public void RecoverAmmo()
		{
			if (!Core.SE || !Alive) return;
			foreach (KeyValuePair<Type, int> kvp in RecoverableAmmo)
			{
				if (kvp.Value <= 0) continue;
				Item ammo = null;

				try
				{
					ammo = Activator.CreateInstance(kvp.Key) as Item;
				}
				catch
				{
					// ignored
				}

				if (ammo == null) continue;
				string name = ammo.Name;
				ammo.Amount = kvp.Value;

				if (name == null)
				{
					name = ammo switch
					{
						Arrow => "arrow",
						CrossBowBolt => "bolt",
						_ => name
					};
				}

				if (name != null && ammo.Amount > 1)
					name = $"{name}s";

				name ??= $"#{ammo.LabelNumber}";

				PlaceInBackpack(ammo);
				SendLocalizedMessage(1073504, $"{ammo.Amount}\t{name}"); // You recover ~1_NUM~ ~2_AMMO~.
			}

			RecoverableAmmo.Clear();
		}

		#endregion

		#region Mondain's Legacy
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Bedlam { get => GetFlag(PlayerFlag.Bedlam); set => SetFlag(PlayerFlag.Bedlam, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool LibraryFriend { get => GetFlag(PlayerFlag.LibraryFriend); set => SetFlag(PlayerFlag.LibraryFriend, value); }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Spellweaving { get => GetFlag(PlayerFlag.Spellweaving); set => SetFlag(PlayerFlag.Spellweaving, value); }
		#endregion

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AnkhNextUse { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan DisguiseTimeLeft => DisguiseTimers.TimeRemaining(this);

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime PeacedUntil { get; set; }

		#region Scroll of Alacrity
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime AcceleratedStart { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SkillName AcceleratedSkill { get; set; }
		#endregion

		private bool _mHallucinate;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Hallucinating
		{
			get => _mHallucinate;
			set
			{
				if (_mHallucinate == value)
					return;

				if (value)
				{
					_mHallucinate = true;
					SendEverything();
				}
				else
				{
					_mHallucinate = false;
					SendEverything();
				}

				InvalidateProperties();
			}
		}

		public override bool IsHallucinated => _mHallucinate;

		private int _mBounty;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Bounty
		{
			get => _mBounty;
			set
			{
				if (_mBounty != value)
				{
					if (_mBounty < value)
						NextBountyDecay = DateTime.UtcNow + TimeSpan.FromDays(1.0);
					_mBounty = value;
				}
				BountyBoard.Update(this);

			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextBountyDecay { get; set; }

		public DateTime LastLogin { get; set; }

		public override short GetBody(Mobile toSend)
		{
			if (_mHallucinate)
			{
				return (short)m_BodyArray[new Random(_halluRandomSeed + toSend.Serial.Value).Next(0, 59)];
			}

			return (short)toSend.Body;
		}


		public override int GetHue(Mobile toSend)
		{
			return _mHallucinate ? new Random(_halluRandomSeed + toSend.Serial.Value).Next(2, 1200) : toSend.Hue;
		}

		private static int _halluRandomSeed = Utility.Random(1073741822);

		private class ReseedTimer : Timer
		{
			public ReseedTimer() : base(TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(20))
			{
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				_halluRandomSeed = Utility.Random(1073741822);
			}
		}

		private static readonly int[] m_BodyArray = new int[60] { 1, 2, 3, 4, 5, 6, 8, 10, 11, 12, 13, 14, 15, 16, 21, 22, 24, 26, 28, 29, 30, 31, 33, 39, 42, 46, 47, 48, 50, 51, 52, 53, 58, 62, 64, 70, 72, 75, 80, 81, 85, 86, 88, 90, 97, 101, 104, 123, 134, 145, 149, 150, 151, 152, 154, 157, 400, 401, 605, 606 };

		public static Direction GetDirection4(Point3D from, Point3D to)
		{
			int dx = from.X - to.X;
			int dy = from.Y - to.Y;

			int rx = dx - dy;
			int ry = dx + dy;

			Direction ret = rx switch
			{
				>= 0 when ry >= 0 => Direction.West,
				>= 0 when ry < 0 => Direction.South,
				< 0 when ry < 0 => Direction.East,
				_ => Direction.North
			};

			return ret;
		}

		public override bool OnDroppedItemToWorld(Item item, Point3D location)
		{
			if (!base.OnDroppedItemToWorld(item, location))
				return false;

			if (Core.AOS)
			{
				IPooledEnumerable mobiles = Map.GetMobilesInRange(location, 0);

				if (mobiles.Cast<Mobile>().Any(m => m.Z >= location.Z && m.Z < location.Z + 16 && (!m.Hidden || m.AccessLevel == AccessLevel.Player)))
				{
					mobiles.Free();
					return false;
				}

				mobiles.Free();
			}

			BounceInfo bi = item.GetBounce();

			if (bi == null)
				return true;

			Type type = item.GetType();

			if (!type.IsDefined(typeof(FurnitureAttribute), true) &&
			    !type.IsDefined(typeof(DynamicFlipingAttribute), true)) return true;
			object[] objs = type.GetCustomAttributes(typeof(FlipableAttribute), true);

			if (objs.Length <= 0) return true;

			if (objs[0] is not FlipableAttribute fp) return true;

			int[] itemIDs = fp.ItemIDs;

			Point3D oldWorldLoc = bi.m_WorldLoc;
			Point3D newWorldLoc = location;

			if (oldWorldLoc.X == newWorldLoc.X && oldWorldLoc.Y == newWorldLoc.Y) return true;
			Direction dir = GetDirection4(oldWorldLoc, newWorldLoc);

			switch (itemIDs.Length)
			{
				case 2:
					switch (dir)
					{
						case Direction.North:
						case Direction.South: item.ItemId = itemIDs[0]; break;
						case Direction.East:
						case Direction.West: item.ItemId = itemIDs[1]; break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					break;
				case 4:
					item.ItemId = dir switch
					{
						Direction.South => itemIDs[0],
						Direction.East => itemIDs[1],
						Direction.North => itemIDs[2],
						Direction.West => itemIDs[3],
						_ => item.ItemId
					};

					break;
			}

			return true;
		}

		public override int GetPacketFlags()
		{
			int flags = base.GetPacketFlags();

			if (_mIgnoreMobiles)
				flags |= 0x10;

			return flags;
		}

		public override int GetOldPacketFlags()
		{
			int flags = base.GetOldPacketFlags();

			if (_mIgnoreMobiles)
				flags |= 0x10;

			return flags;
		}

		public bool GetFlag(PlayerFlag flag)
		{
			return (Flags & flag) != 0;
		}

		public void SetFlag(PlayerFlag flag, bool value)
		{
			if (value)
				Flags |= flag;
			else
				Flags &= ~flag;
		}

		public bool GetFlag(ExtendedPlayerFlag flag)
		{
			return ((ExtendedFlags & flag) != 0);
		}

		public void SetFlag(ExtendedPlayerFlag flag, bool value)
		{
			if (value)
			{
				ExtendedFlags |= flag;
			}
			else
			{
				ExtendedFlags &= ~flag;
			}
		}

		public DesignContext DesignContext { get; set; }

		public static void Initialize()
		{
			if (FastwalkPrevention)
				PacketHandlers.RegisterThrottler(0x02, MovementThrottle_Callback);

			EventSink.OnLogin += OnLogin;
			EventSink.OnLogout += OnLogout;
			EventSink.OnConnected += EventSink_Connected;
			EventSink.OnDisconnected += EventSink_Disconnected;

			#region Enchanced Client
			//EventSink.TargetedSkill += Targeted_Skill;
			//EventSink.EquipMacro += EquipMacro;
			//EventSink.UnequipMacro += UnequipMacro;
			#endregion

			if (Core.SE)
			{
				Timer.DelayCall(TimeSpan.Zero, CheckPets);
			}
		}

		#region Enhanced Client
		/*private static void Targeted_Skill(TargetedSkillEventArgs e)
		{
			Mobile from = e.Mobile;
			int SkillId = e.SkillID;
			IEntity target = e.Target;

			if (from == null || target == null)
				return;

			from.TargetLocked = true;

			if (e.SkillID == 35)
			{
				AnimalTaming.DisableMessage = true;
				AnimalTaming.DeferredTarget = false;
			}

			if (from.UseSkill(e.SkillID) && from.Target != null)
			{
				from.Target.Invoke(from, target);
			}

			if (e.SkillID == 35)
			{
				AnimalTaming.DeferredTarget = true;
				AnimalTaming.DisableMessage = false;
			}

			from.TargetLocked = false;
		}

		public static void EquipMacro(EquipMacroEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
			{
				Container pack = pm.Backpack;

				e.List.ForEach(serial =>
				{
					Item item = pack.Items.FirstOrDefault(i => i.Serial == serial);

					if (item != null)
					{
						Item toMove = pm.FindItemOnLayer(item.Layer);

						if (toMove != null)
						{
							//pack.DropItem(toMove);
							toMove.Internalize();

							if (!pm.EquipItem(item))
							{
								pm.EquipItem(toMove);
							}
							else
							{
								pack.DropItem(toMove);
							}
						}
						else
						{
							pm.EquipItem(item);
						}
					}
				});
			}
		}

		public static void UnequipMacro(UnequipMacroEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm != null && pm.Backpack != null && pm.Alive && e.List != null && e.List.Count > 0)
			{
				Container pack = pm.Backpack;

				List<Item> worn = new List<Item>(pm.Items);

				foreach (var item in worn)
				{
					if (e.List.Contains((int)item.Layer))
					{
						pack.TryDropItem(pm, item, false);
					}
				}

				ColUtility.Free(worn);
			}
		}*/
		#endregion

		private static void CheckPets()
		{
			foreach (var m in World.Mobiles.Values)
			{
				if (m is not PlayerMobile pm) continue;
				if (((!pm.Mounted || (pm.Mount != null && pm.Mount is EtherealMount)) && pm.AllFollowers.Count > pm.AutoStabled.Count) ||
				    (pm.Mounted && pm.AllFollowers.Count > pm.AutoStabled.Count + 1))
				{
					pm.AutoStablePets(); /* autostable checks summons, et al: no need here */
				}
			}
		}

		private MountBlock _mMountBlock;

		public BlockMountType MountBlockReason => (CheckBlock(_mMountBlock)) ? _mMountBlock.MType : BlockMountType.None;

		private static bool CheckBlock(MountBlock block)
		{
			return block is not null && block.MTimer.Running;
		}

		private class MountBlock
		{
			public readonly BlockMountType MType;
			public readonly Timer MTimer;

			public MountBlock(TimeSpan duration, BlockMountType type, Mobile mobile)
			{
				MType = type;

				MTimer = Timer.DelayCall(duration, RemoveBlock, mobile);
			}

			private static void RemoveBlock(Mobile mobile)
			{
				((PlayerMobile) mobile)._mMountBlock = null;
			}
		}

		public override void OnSkillInvalidated(Skill skill)
		{
			if (Core.AOS && skill.SkillName == SkillName.MagicResist)
				UpdateResistances();
		}

		public override int GetMaxResistance(ResistanceType type)
		{
			if (AccessLevel > AccessLevel.Player)
				return 100;

			int max = base.GetMaxResistance(type);

			if (type != ResistanceType.Physical && 60 < max && Spells.Fourth.CurseSpell.UnderEffect(this))
				max = 60;

			if (Core.ML && Race == Race.Elf && type == ResistanceType.Energy)
				max += 5; //Intended to go after the 60 max from curse

			return max;
		}

		protected override void OnRaceChange(Race oldRace)
		{
			ValidateEquipment();
			UpdateResistances();
		}

		public override int MaxWeight => (Core.ML && Race == Race.Human ? 100 : 40) + (int)(3.5 * Str);

		private int _mLastGlobalLight = -1, _mLastPersonalLight = -1;

		public override void OnNetStateChanged()
		{
			_mLastGlobalLight = -1;
			_mLastPersonalLight = -1;
		}

		public override void ComputeBaseLightLevels(out int global, out int personal)
		{
			global = LightCycle.ComputeLevelFor(this);

			bool racialNightSight = Core.ML && Race == Race.Elf;

			if (LightLevel < 21 && (AosAttributes.GetValue(this, AosAttribute.NightSight) > 0 || racialNightSight))
				personal = 21;
			else
				personal = LightLevel;
		}

		public override void CheckLightLevels(bool forceResend)
		{
			NetState ns = NetState;

			if (ns == null)
				return;

			ComputeLightLevels(out int global, out int personal);

			if (!forceResend)
				forceResend = (global != _mLastGlobalLight || personal != _mLastPersonalLight);

			if (!forceResend)
				return;

			_mLastGlobalLight = global;
			_mLastPersonalLight = personal;

			ns.Send(GlobalLightLevel.Instantiate(global));
			ns.Send(new PersonalLightLevel(this, personal));
		}

		public override int GetMinResistance(ResistanceType type)
		{
			int magicResist = (int)(Skills[SkillName.MagicResist].Value * 10);
			int min = int.MinValue;

			if (magicResist >= 1000)
				min = 40 + ((magicResist - 1000) / 50);
			else if (magicResist >= 400)
				min = (magicResist - 400) / 15;

			if (min > MaxPlayerResistance)
				min = MaxPlayerResistance;

			int baseMin = base.GetMinResistance(type);

			if (min < baseMin)
				min = baseMin;

			return min;
		}

		public override void OnManaChange(int oldValue)
		{
			base.OnManaChange(oldValue);
			if (ExecutesLightningStrike <= 0) return;
			if (Mana < ExecutesLightningStrike)
			{
				SpecialMove.ClearCurrentMove(this);
			}
		}

		private static void OnLogin(Mobile from)
		{
			CheckAtrophies(from);

			if (AccountHandler.LockdownLevel > AccessLevel.Player)
			{
				string notice;

				if (from.Account is not Account acct || !acct.HasAccess(from.NetState))
				{
					notice = from.IsPlayer() ? "The server is currently under lockdown. No players are allowed to log in at this time." : "The server is currently under lockdown. You do not have sufficient access level to connect.";

					Timer.DelayCall(TimeSpan.FromSeconds(1.0), new TimerStateCallback(Disconnect), from);
				}
				else if (from.AccessLevel >= AccessLevel.Administrator)
				{
					notice = "The server is currently under lockdown. As you are an administrator, you may change this from the [Admin gump.";
				}
				else
				{
					notice = "The server is currently under lockdown. You have sufficient access level to connect.";
				}

				from.SendGump(new NoticeGump(1060637, 30720, notice, 0xFFC000, 300, 140, null, null));
				return;
			}

			PlayerMobile pm = from as PlayerMobile;

			pm?.ClaimAutoStabledPets();

			if (pm == null || !pm.Young || from.Account is not Account {Young: true} acc) return;
			TimeSpan ts = Accounting.Account.YoungDuration - acc.TotalGameTime;
			int hours = Math.Max((int)ts.TotalHours, 0);

			pm.SendAsciiMessage("You will enjoy the benefits and relatively safe status of a young player for {0} more hour{1}.", hours, hours != 1 ? "s" : "");
		}

		private bool _mNoDeltaRecursion;

		public void ValidateEquipment()
		{
			if (_mNoDeltaRecursion || Map == null || Map == Map.Internal)
				return;

			if (Items == null)
				return;

			_mNoDeltaRecursion = true;
			Timer.DelayCall(TimeSpan.Zero, ValidateEquipment_Sandbox);
		}

		private void ValidateEquipment_Sandbox()
		{
			try
			{
				if (Map == null || Map == Map.Internal)
					return;

				List<Item> items = Items;

				if (items == null)
					return;

				bool moved = false;

				int str = Str;
				int dex = Dex;
				int intel = Int;
				int factionItemCount = 0;

				Mobile from = this;

				Ethics.Ethic ethic = Ethics.Ethic.Find(from);

				for (int i = items.Count - 1; i >= 0; --i)
				{
					if (i >= items.Count)
						continue;

					Item item = items[i];
					bool drop = false;

					if ((item.SavedFlags & 0x100) != 0)
					{
						if (item.Hue != Ethics.Ethic.Hero.Definition.PrimaryHue)
						{
							item.SavedFlags &= ~0x100;
						}
						else if (ethic != Ethics.Ethic.Hero)
						{
							from.AddToBackpack(item);
							moved = true;
							continue;
						}
					}
					else if ((item.SavedFlags & 0x200) != 0)
					{
						if (item.Hue != Ethics.Ethic.Evil.Definition.PrimaryHue)
						{
							item.SavedFlags &= ~0x200;
						}
						else if (ethic != Ethics.Ethic.Evil)
						{
							from.AddToBackpack(item);
							moved = true;
							continue;
						}
					}

					if (!RaceDefinitions.ValidateEquipment(from, item, false))
					{
						drop = true;
					}

					switch (item)
					{
						case BaseWeapon weapon:
						{
							if (!drop)
							{
								if (dex < weapon.DexRequirement)
									drop = true;
								else if (str < AOS.Scale(weapon.StrRequirement, 100 - weapon.GetLowerStatReq()))
									drop = true;
								else if (intel < weapon.IntRequirement)
									drop = true;
								else if (weapon.RequiredRace != null && weapon.RequiredRace != Race)
									drop = true;
							}

							if (drop)
							{
								string name = weapon.Name ?? $"#{weapon.LabelNumber}";

								from.SendLocalizedMessage(1062001, name); // You can no longer wield your ~1_WEAPON~
								from.AddToBackpack(weapon);
								moved = true;
							}

							break;
						}
						case BaseArmor armor:
						{
							if (!drop)
							{
								drop = true;
							}
							else
							{
								if (!armor.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
								{
									drop = true;
								}
								else if (!armor.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
								{
									drop = true;
								}
								else if (armor.RequiredRace != null && armor.RequiredRace != Race)
								{
									drop = true;
								}
								else
								{
									int strBonus = armor.ComputeStatBonus(StatType.Str), strReq = armor.ComputeStatReq(StatType.Str);
									int dexBonus = armor.ComputeStatBonus(StatType.Dex), dexReq = armor.ComputeStatReq(StatType.Dex);
									int intBonus = armor.ComputeStatBonus(StatType.Int), intReq = armor.ComputeStatReq(StatType.Int);

									if (dex < dexReq || (dex + dexBonus) < 1)
										drop = true;
									else if (str < strReq || (str + strBonus) < 1)
										drop = true;
									else if (intel < intReq || (intel + intBonus) < 1)
										drop = true;
								}
							}

							if (drop)
							{
								string name = armor.Name ?? $"#{armor.LabelNumber}";

								from.SendLocalizedMessage(armor is BaseShield ? 1062003 : 1062002, name);

								from.AddToBackpack(armor);
								moved = true;
							}

							break;
						}
						case BaseClothing clothing:
						{
							if (!drop)
							{

								if (!clothing.AllowMaleWearer && !from.Female && from.AccessLevel < AccessLevel.GameMaster)
								{
									drop = true;
								}
								else if (!clothing.AllowFemaleWearer && from.Female && from.AccessLevel < AccessLevel.GameMaster)
								{
									drop = true;
								}
								else if (clothing.RequiredRace != null && clothing.RequiredRace != Race)
								{
									drop = true;
								}
								else
								{
									int strBonus = clothing.ComputeStatBonus(StatType.Str);
									int strReq = clothing.ComputeStatReq(StatType.Str);

									if (str < strReq || (str + strBonus) < 1)
										drop = true;
								}
							}

							if (drop)
							{
								string name = clothing.Name ?? $"#{clothing.LabelNumber}";

								from.SendLocalizedMessage(1062002, name); // You can no longer wear your ~1_ARMOR~

								from.AddToBackpack(clothing);
								moved = true;
							}

							break;
						}
					}

					FactionItem factionItem = FactionItem.Find(item);

					if (factionItem == null) continue;
					Faction ourFaction = Faction.Find(this);

					if (ourFaction == null || ourFaction != factionItem.Faction)
						drop = true;
					else if (++factionItemCount > FactionItem.GetMaxWearables(this))
						drop = true;

					if (!drop) continue;
					from.AddToBackpack(item);
					moved = true;
				}

				if (moved)
					from.SendLocalizedMessage(500647); // Some equipment has been moved to your backpack.
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				_mNoDeltaRecursion = false;
			}
		}

		public override void Delta(MobileDelta flag)
		{
			base.Delta(flag);

			if ((flag & MobileDelta.Stat) != 0)
				ValidateEquipment();
		}

		private static void Disconnect(object state)
		{
			NetState ns = ((Mobile)state).NetState;

			ns?.Dispose();
		}

		private static void OnLogout(Mobile from)
		{
			if (from is PlayerMobile pm)
				pm.AutoStablePets();
		}

		private static void EventSink_Connected(Mobile from)
		{
			if (from is PlayerMobile pm)
			{
				pm.SessionStart = DateTime.UtcNow;

				pm.Quest?.StartTimer();

				QuestHelper.StartTimer(pm);

				pm.BedrollLogout = false;
				pm.LastOnline = DateTime.UtcNow;
			}

			DisguiseTimers.StartTimer(from);

			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(ClearSpecialMovesCallback), from);

			if (from.Account is not Account acc || !acc.Young || acc.YoungTimer != null) return;
			acc.YoungTimer = new YoungTimer(acc);
			acc.YoungTimer.Start();
		}

		private static void ClearSpecialMovesCallback(object state)
		{
			Mobile from = (Mobile)state;
			SpecialMove.ClearAllMoves(from);
		}

		private static void EventSink_Disconnected(Mobile from)
		{
			DesignContext context = DesignContext.Find(from);

			if (context != null)
			{
				/* Client disconnected
				 *  - Remove design context
				 *  - Eject all from house
				 *  - Restore relocated entities
				 */

				// Remove design context
				DesignContext.Remove(from);

				// Eject all from house
				from.RevealingAction();

				foreach (Item item in context.Foundation.GetItems())
				{
					item.Location = context.Foundation.BanLocation;
				}

				foreach (Mobile mobile in context.Foundation.GetMobiles())
				{
					mobile.Location = context.Foundation.BanLocation;
				}

				// Restore relocated entities
				context.Foundation.RestoreRelocatedEntities();
			}

			PlayerMobile pm = from as PlayerMobile;

			if (pm != null)
			{
				pm._mGameTime += DateTime.UtcNow - pm.SessionStart;

				if (pm.Quest != null)
					pm.Quest.StopTimer();

				QuestHelper.StopTimer(pm);

				pm.SpeechLog = null;
				pm.LastOnline = DateTime.UtcNow;
			}

			DisguiseTimers.StopTimer(from);

			Account acc = from.Account as Account;

			if (acc != null && acc.YoungTimer != null)
			{
				acc.YoungTimer.Stop();
				acc.YoungTimer = null;
			}

			if (acc != null && pm != null)
			{
				acc.TotalGameTime += DateTime.UtcNow - pm.SessionStart;
			}
		}

		public override void RevealingAction()
		{
			if (DesignContext != null)
				return;

			Spells.Sixth.InvisibilitySpell.RemoveTimer(this);

			base.RevealingAction();

			IsStealthing = false; // IsStealthing should be moved to Server.Mobiles
		}

		public override void OnHiddenChanged()
		{
			base.OnHiddenChanged();

			RemoveBuff(BuffIcon.Invisibility);  //Always remove, default to the hiding icon EXCEPT in the invis spell where it's explicitly set

			if (!Hidden)
			{
				RemoveBuff(BuffIcon.HidingAndOrStealth);
			}
			else// if( !InvisibilitySpell.HasTimer( this ) )
			{
				BuffInfo.AddBuff(this, new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655)); //Hidden/Stealthing & You Are Hidden
			}
		}

		public override void OnSubItemAdded(Item item)
		{
			if (AccessLevel < AccessLevel.GameMaster && item.IsChildOf(Backpack))
			{
				int maxWeight = WeightOverloading.GetMaxWeight(this);
				int curWeight = Mobile.BodyWeight + TotalWeight;

				if (curWeight > maxWeight)
					SendLocalizedMessage(1019035, true, $" : {curWeight} / {maxWeight}");
			}

			base.OnSubItemAdded(item);
		}

		public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness, bool ignorePeaceCheck)
		{
			Mobile target = damageable as Mobile;

			if (DesignContext != null || (target is PlayerMobile pm && pm.DesignContext != null))
				return false;

			#region Mondain's Legacy
			if (Peaced && !ignorePeaceCheck)
			{
				//!+ TODO: message
				return false;
			}
			#endregion

			if ((target is not BaseCreature creature || !creature.IsInvulnerable) && target is not PlayerVendor &&
			    target is not TownCrier)
				return base.CanBeHarmful(damageable, message, ignoreOurBlessedness, ignorePeaceCheck);
			if (!message) return false;
			if (target.Title == null)
				SendMessage("{0} cannot be harmed.", target.Name);
			else
				SendMessage("{0} {1} cannot be harmed.", target.Name, target.Title);

			return false;

			/*if (damageable is IDamageableItem && !((IDamageableItem)damageable).CanDamage)
			{
				if (message)
					SendMessage("That cannot be harmed.");

				return false;
			}*/ //High Seas Item

		}

		public override bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
		{
			if (DesignContext != null || (target is PlayerMobile pm && pm.DesignContext != null))
				return false;

			return base.CanBeBeneficial(target, message, allowDead);
		}

		public override bool CheckContextMenuDisplay(IEntity target)
		{
			return DesignContext == null;
		}

		public override void OnItemAdded(Item item)
		{
			base.OnItemAdded(item);

			if (item is BaseArmor || item is BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

			if (NetState != null)
				CheckLightLevels(false);
		}

		public override void OnItemRemoved(Item item)
		{
			base.OnItemRemoved(item);

			if (item is BaseArmor or BaseWeapon)
			{
				Hits = Hits;
				Stam = Stam;
				Mana = Mana;
			}

			if (NetState != null)
				CheckLightLevels(false);
		}

		public override double ArmorRating
		{
			get
			{
				//BaseArmor ar;
				double rating = 0.0;

				AddArmorRating(ref rating, NeckArmor);
				AddArmorRating(ref rating, HandArmor);
				AddArmorRating(ref rating, HeadArmor);
				AddArmorRating(ref rating, ArmsArmor);
				AddArmorRating(ref rating, LegsArmor);
				AddArmorRating(ref rating, ChestArmor);
				AddArmorRating(ref rating, ShieldArmor);

				return VirtualArmor + VirtualArmorMod + rating;
			}
		}

		private static void AddArmorRating(ref double rating, Item armor)
		{
			if (armor is BaseArmor ar && (!Core.AOS || ar.ArmorAttributes.MageArmor == 0))
				rating += ar.ArmorRatingScaled;
		}

		#region [Stats]Max
		[CommandProperty(AccessLevel.GameMaster)]
		public override int HitsMax
		{
			get
			{
				int strBase;
				int strOffs = GetStatOffset(StatType.Str);

				if (Core.AOS)
				{
					strBase = Str; //this.Str already includes GetStatOffset/str
					strOffs = AosAttributes.GetValue(this, AosAttribute.BonusHits);

					if (Core.ML && strOffs > 25 && AccessLevel <= AccessLevel.Player)
						strOffs = 25;

					if (AnimalForm.UnderTransformation(this, typeof(BakeKitsune)) || AnimalForm.UnderTransformation(this, typeof(GreyWolf)))
						strOffs += 20;
				}
				else
				{
					strBase = RawStr;
				}

				return strBase / 2 + 50 + strOffs;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int StamMax => base.StamMax + AosAttributes.GetValue(this, AosAttribute.BonusStam);

		[CommandProperty(AccessLevel.GameMaster)]
		public override int ManaMax => base.ManaMax + AosAttributes.GetValue(this, AosAttribute.BonusMana) + ((Core.ML && Race == Race.Elf) ? 20 : 0);
		#endregion

		#region Stat Getters/Setters

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Str
		{
			get
			{
				if (Core.ML && IsPlayer())
					return Math.Min(base.Str, 150);

				return base.Str;
			}
			set => base.Str = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Int
		{
			get
			{
				if (Core.ML && IsPlayer())
					return Math.Min(base.Int, 150);

				return base.Int;
			}
			set => base.Int = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Dex
		{
			get
			{
				if (Core.ML && IsPlayer())
					return Math.Min(base.Dex, 150);

				return base.Dex;
			}
			set => base.Dex = value;
		}

		#endregion

		public override bool Move(Direction d)
		{
			NetState ns = NetState;

			if (ns != null)
			{
				if (HasGump(typeof(ResurrectGump)))
				{
					if (Alive)
					{
						CloseGump(typeof(ResurrectGump));
					}
					else
					{
						SendLocalizedMessage(500111); // You are frozen and cannot move.
						return false;
					}
				}
			}

			int speed = ComputeMovementSpeed(d);

			if (!Alive)
				Server.Movement.MovementImpl.IgnoreMovableImpassables = true;

			var res = base.Move(d);

			Server.Movement.MovementImpl.IgnoreMovableImpassables = false;

			if (!res)
				return false;

			_mNextMovementTime += speed;

			return true;
		}

		public override bool CheckMovement(Direction d, out int newZ)
		{
			DesignContext context = DesignContext;

			if (context == null)
				return base.CheckMovement(d, out newZ);

			HouseFoundation foundation = context.Foundation;

			newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int newX = X, newY = Y;
			Movement.Movement.Offset(d, ref newX, ref newY);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			return newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map;
		}

		public override bool AllowItemUse(Item item)
		{
			#region Dueling
			if (DuelContext != null && !DuelContext.AllowItemUse(this, item))
				return false;
			#endregion

			return DesignContext.Check(this);
		}

		public SkillName[] AnimalFormRestrictedSkills { get; } = new[]
		{
			SkillName.ArmsLore, SkillName.Begging, SkillName.Discordance, SkillName.Forensics,
			SkillName.Inscribe, SkillName.ItemID, SkillName.Meditation, SkillName.Peacemaking,
			SkillName.Provocation, SkillName.RemoveTrap, SkillName.SpiritSpeak, SkillName.Stealing,
			SkillName.TasteID
		};

		public override bool AllowSkillUse(SkillName skill)
		{
			if (AnimalForm.UnderTransformation(this))
			{
				for (int i = 0; i < AnimalFormRestrictedSkills.Length; i++)
				{
					if (AnimalFormRestrictedSkills[i] != skill) continue;
					SendLocalizedMessage(1070771); // You cannot use that skill in this form.
					return false;
				}
			}

			#region Dueling
			if (DuelContext != null && !DuelContext.AllowSkillUse(this, skill))
				return false;
			#endregion

			return DesignContext.Check(this);
		}

		private bool _mLastProtectedMessage;
		private int _mNextProtectionCheck = 10;

		public virtual void RecheckTownProtection()
		{
			_mNextProtectionCheck = 10;

			Regions.GuardedRegion reg = (Regions.GuardedRegion)Region.GetRegion(typeof(Regions.GuardedRegion));
			bool isProtected = (reg != null && !reg.IsDisabled());

			if (isProtected == _mLastProtectedMessage) return;
			SendLocalizedMessage(isProtected ? 500112 : 500113);

			_mLastProtectedMessage = isProtected;
		}

		public override void MoveToWorld(Point3D loc, Map map)
		{
			base.MoveToWorld(loc, map);

			RecheckTownProtection();
		}

		public override void SetLocation(Point3D loc, bool isTeleport)
		{
			if (!isTeleport && AccessLevel == AccessLevel.Player)
			{
				// moving, not teleporting
				int zDrop = Location.Z - loc.Z;

				if (zDrop > 20) // we fell more than one story
					Hits -= zDrop / 20 * 10 - 5; // deal some damage; does not kill, disrupt, etc
			}

			base.SetLocation(loc, isTeleport);

			if (isTeleport || --_mNextProtectionCheck == 0)
				RecheckTownProtection();
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from == this)
			{
				Quest?.GetContextMenuEntries(list);

				if (Alive)
				{
					if (InsuranceEnabled)
					{
						if (Core.SA)
							list.Add(new CallbackEntry(1114299, OpenItemInsuranceMenu)); // Open Item Insurance Menu

						list.Add(new CallbackEntry(6201, ToggleItemInsurance)); // Toggle Item Insurance

						if (!Core.SA)
						{
							if (AutoRenewInsurance)
								list.Add(new CallbackEntry(6202, CancelRenewInventoryInsurance)); // Cancel Renewing Inventory Insurance
							else
								list.Add(new CallbackEntry(6200, AutoRenewInventoryInsurance)); // Auto Renew Inventory Insurance
						}
					}

					if (Core.ML)
					{
						QuestHelper.GetContextMenuEntries(list);

						if (!Core.SA && RewardTitles.Count > 0)
						{
							list.Add(new CallbackEntry(6229, ShowChangeTitle));
						}

						if (Quest != null)
						{
							Quest.GetContextMenuEntries(list);
						}
					}
				}

				BaseHouse house = BaseHouse.FindHouseAt(this);

				if (house != null)
				{
					if (Alive && house.InternalizedVendors.Count > 0 && house.IsOwner(this))
						list.Add(new CallbackEntry(6204, GetVendor));

					if (house.IsAosRules && !Region.IsPartOf(typeof(Engines.ConPVP.SafeZone))) // Dueling
						list.Add(new CallbackEntry(6207, LeaveHouse));
				}

				if (JusticeProtectors.Count > 0)
					list.Add(new CallbackEntry(6157, CancelProtection));

				if (Alive)
					list.Add(new CallbackEntry(6210, ToggleChampionTitleDisplay));

				if (Core.HS)
				{
					NetState ns = from.NetState;

					if (ns != null && ns.ExtendedStatus)
						list.Add(new CallbackEntry(RefuseTrades ? 1154112 : 1154113, ToggleTrades)); // Allow Trades / Refuse Trades
				}
			}
			else
			{
				if (Core.TOL && from.InRange(this, 2))
				{
					list.Add(new CallbackEntry(1077728, () => OpenTrade(from))); // Trade
				}

				if (Alive && Core.Expansion >= Expansion.AOS)
				{
					Party theirParty = from.Party as Party;
					Party ourParty = Party as Party;

					if (theirParty == null && ourParty == null)
					{
						list.Add(new AddToPartyEntry(from, this));
					}
					else if (theirParty != null && theirParty.Leader == from)
					{
						if (ourParty == null)
						{
							list.Add(new AddToPartyEntry(from, this));
						}
						else if (ourParty == theirParty)
						{
							list.Add(new RemoveFromPartyEntry(from, this));
						}
					}
				}

				BaseHouse curhouse = BaseHouse.FindHouseAt(this);

				if (curhouse == null) return;
				if (Alive && Core.Expansion >= Expansion.AOS && curhouse.IsAosRules && curhouse.IsFriend(from))
					list.Add(new EjectPlayerEntry(from, this));
			}
		}

		private void CancelProtection()
		{
			for (var i = 0; i < JusticeProtectors.Count; ++i)
			{
				Mobile prot = JusticeProtectors[i];

				string args = $"{Name}\t{prot.Name}";

				prot.SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
				SendLocalizedMessage(1049371, args); // The protective relationship between ~1_PLAYER1~ and ~2_PLAYER2~ has been ended.
			}

			JusticeProtectors.Clear();
		}

		#region Insurance

		private static int GetInsuranceCost(Item item)
		{
			return 600; // TODO
		}

		private void ToggleItemInsurance()
		{
			if (!CheckAlive())
				return;

			BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
			SendLocalizedMessage(1060868); // Target the item you wish to toggle insurance status on <ESC> to cancel
		}

		private bool CanInsure(Item item)
		{
			if ((item is Container && item is not BaseQuiver) || item is BagOfSending or KeyRing or PotionKeg or Sigil)
				return false;

			if (item.Stackable)
				return false;

			if (item.LootType == LootType.Cursed)
				return false;

			if (item.ItemId == 0x204E) // death shroud
				return false;

			if (item.Layer == Layer.Mount)
				return false;

			if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied || item.BlessedFor == this)
			{
				//SendLocalizedMessage( 1060870, "", 0x23 ); // That item is blessed and does not need to be insured
				return false;
			}

			return true;
		}

		private void ToggleItemInsurance_Callback(Mobile from, object obj)
		{
			if (!CheckAlive())
				return;

			ToggleItemInsurance_Callback(from, obj as Item, true);
		}

		private void ToggleItemInsurance_Callback(Mobile from, Item item, bool target)
		{
			if (item == null || !item.IsChildOf(this))
			{
				if (target)
					BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);

				SendLocalizedMessage(1060871, 0x23); // You can only insure items that you have equipped or that are in your backpack
			}
			else if (item.Insured)
			{
				item.Insured = false;

				SendLocalizedMessage(1060874, 0x35); // You cancel the insurance on the item

				if (target)
				{
					BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
					SendLocalizedMessage(1060868, 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
				}
			}
			else if (!CanInsure(item))
			{
				if (target)
					BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);

				SendLocalizedMessage(1060869, 0x23); // You cannot insure that
			}
			else
			{
				if (!item.PayedInsurance)
				{
					int cost = GetInsuranceCost(item);

					if (Banker.Withdraw(from, cost))
					{
						SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
						item.PayedInsurance = true;
					}
					else
					{
						SendLocalizedMessage(1061079, 0x23); // You lack the funds to purchase the insurance
						return;
					}
				}

				item.Insured = true;

				SendLocalizedMessage(1060873, 0x23); // You have insured the item

				if (target)
				{
					BeginTarget(-1, false, TargetFlags.None, ToggleItemInsurance_Callback);
					SendLocalizedMessage(1060868, 0x23); // Target the item you wish to toggle insurance status on <ESC> to cancel
				}
			}
		}

		private void AutoRenewInventoryInsurance()
		{
			if (!CheckAlive())
				return;

			SendLocalizedMessage(1060881, 0x23); // You have selected to automatically reinsure all insured items upon death
			AutoRenewInsurance = true;
		}

		private void CancelRenewInventoryInsurance()
		{
			if (!CheckAlive())
				return;

			if (Core.SE)
			{
				if (!HasGump(typeof(CancelRenewInventoryInsuranceGump)))
					SendGump(new CancelRenewInventoryInsuranceGump(this, null));
			}
			else
			{
				SendLocalizedMessage(1061075, 0x23); // You have cancelled automatically reinsuring all insured items upon death
				AutoRenewInsurance = false;
			}
		}

		private class CancelRenewInventoryInsuranceGump : Gump
		{
			private readonly PlayerMobile _mPlayer;
			private readonly ItemInsuranceMenuGump _mInsuranceGump;

			public CancelRenewInventoryInsuranceGump(PlayerMobile player, ItemInsuranceMenuGump insuranceGump) : base(250, 200)
			{
				_mPlayer = player;
				_mInsuranceGump = insuranceGump;

				AddBackground(0, 0, 240, 142, 0x13BE);
				AddImageTiled(6, 6, 228, 100, 0xA40);
				AddImageTiled(6, 116, 228, 20, 0xA40);
				AddAlphaRegion(6, 6, 228, 142);

				AddHtmlLocalized(8, 8, 228, 100, 1071021, 0x7FFF, false, false); // You are about to disable inventory insurance auto-renewal.

				AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(148, 118, 450, 20, 1071022, 0x7FFF, false, false); // DISABLE IT!
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (!_mPlayer.CheckAlive())
					return;

				if (info.ButtonID == 1)
				{
					_mPlayer.SendLocalizedMessage(1061075, 0x23); // You have cancelled automatically reinsuring all insured items upon death
					_mPlayer.AutoRenewInsurance = false;
				}
				else
				{
					_mPlayer.SendLocalizedMessage(1042021); // Cancelled.
				}

				if (_mInsuranceGump != null)
					_mPlayer.SendGump(_mInsuranceGump.NewInstance());
			}
		}

		private void OpenItemInsuranceMenu()
		{
			if (!CheckAlive())
				return;

			List<Item> items = Items.Where(DisplayInItemInsuranceGump).ToList();

			Container pack = Backpack;

			if (pack != null)
				items.AddRange(pack.FindItemsByType<Item>(true, DisplayInItemInsuranceGump));

			// TODO: Investigate item sorting

			CloseGump(typeof(ItemInsuranceMenuGump));

			if (items.Count == 0)
				SendLocalizedMessage(1114915, 0x35); // None of your current items meet the requirements for insurance.
			else
				SendGump(new ItemInsuranceMenuGump(this, items.ToArray()));
		}

		private bool DisplayInItemInsuranceGump(Item item)
		{
			return ((item.Visible || AccessLevel >= AccessLevel.GameMaster) && (item.Insured || CanInsure(item)));
		}

		private class ItemInsuranceMenuGump : Gump
		{
			private readonly PlayerMobile _mFrom;
			private readonly Item[] _mItems;
			private readonly bool[] _mInsure;
			private readonly int _mPage;

			public ItemInsuranceMenuGump(PlayerMobile from, Item[] items, bool[] insure = null, int page = 0)
				: base(25, 50)
			{
				_mFrom = from;
				_mItems = items;

				if (insure == null)
				{
					insure = new bool[items.Length];

					for (int i = 0; i < items.Length; ++i)
						insure[i] = items[i].Insured;
				}

				_mInsure = insure;
				_mPage = page;

				AddPage(0);

				AddBackground(0, 0, 520, 510, 0x13BE);
				AddImageTiled(10, 10, 500, 30, 0xA40);
				AddImageTiled(10, 50, 500, 355, 0xA40);
				AddImageTiled(10, 415, 500, 80, 0xA40);
				AddAlphaRegion(10, 10, 500, 485);

				AddButton(15, 470, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(50, 472, 80, 20, 1011012, 0x7FFF, false, false); // CANCEL

				if (from.AutoRenewInsurance)
					AddButton(360, 10, 9723, 9724, 1, GumpButtonType.Reply, 0);
				else
					AddButton(360, 10, 9720, 9722, 1, GumpButtonType.Reply, 0);

				AddHtmlLocalized(395, 14, 105, 20, 1114122, 0x7FFF, false, false); // AUTO REINSURE

				AddButton(395, 470, 0xFA5, 0xFA6, 2, GumpButtonType.Reply, 0);
				AddHtmlLocalized(430, 472, 50, 20, 1006044, 0x7FFF, false, false); // OK

				AddHtmlLocalized(10, 14, 150, 20, 1114121, 0x7FFF, false, false); // <CENTER>ITEM INSURANCE MENU</CENTER>

				AddHtmlLocalized(45, 54, 70, 20, 1062214, 0x7FFF, false, false); // Item
				AddHtmlLocalized(250, 54, 70, 20, 1061038, 0x7FFF, false, false); // Cost
				AddHtmlLocalized(400, 54, 70, 20, 1114311, 0x7FFF, false, false); // Insured

				int balance = Banker.GetBalance(from);
				int cost = 0;

				for (int i = 0; i < items.Length; ++i)
				{
					if (insure[i])
						cost += GetInsuranceCost(items[i]);
				}

				AddHtmlLocalized(15, 420, 300, 20, 1114310, 0x7FFF, false, false); // GOLD AVAILABLE:
				AddLabel(215, 420, 0x481, balance.ToString());
				AddHtmlLocalized(15, 435, 300, 20, 1114123, 0x7FFF, false, false); // TOTAL COST OF INSURANCE:
				AddLabel(215, 435, 0x481, cost.ToString());

				if (cost != 0)
				{
					AddHtmlLocalized(15, 450, 300, 20, 1114125, 0x7FFF, false, false); // NUMBER OF DEATHS PAYABLE:
					AddLabel(215, 450, 0x481, (balance / cost).ToString());
				}

				for (int i = page * 4, y = 72; i < (page + 1) * 4 && i < items.Length; ++i, y += 75)
				{
					Item item = items[i];
					Rectangle2D b = ItemBounds.Table[item.ItemId];

					AddImageTiledButton(40, y, 0x918, 0x918, 0, GumpButtonType.Page, 0, item.ItemId, item.Hue, 40 - b.Width / 2 - b.X, 30 - b.Height / 2 - b.Y);
					AddItemProperty(item.Serial);

					if (insure[i])
					{
						AddButton(400, y, 9723, 9724, 100 + i, GumpButtonType.Reply, 0);
						AddLabel(250, y, 0x481, GetInsuranceCost(item).ToString());
					}
					else
					{
						AddButton(400, y, 9720, 9722, 100 + i, GumpButtonType.Reply, 0);
						AddLabel(250, y, 0x66C, GetInsuranceCost(item).ToString());
					}
				}

				if (page >= 1)
				{
					AddButton(15, 380, 0xFAE, 0xFAF, 3, GumpButtonType.Reply, 0);
					AddHtmlLocalized(50, 380, 450, 20, 1044044, 0x7FFF, false, false); // PREV PAGE
				}

				if ((page + 1) * 4 < items.Length)
				{
					AddButton(400, 380, 0xFA5, 0xFA7, 4, GumpButtonType.Reply, 0);
					AddHtmlLocalized(435, 380, 70, 20, 1044045, 0x7FFF, false, false); // NEXT PAGE
				}
			}

			public ItemInsuranceMenuGump NewInstance()
			{
				return new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage);
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (info.ButtonID == 0 || !_mFrom.CheckAlive())
					return;

				switch (info.ButtonID)
				{
					case 1: // Auto Reinsure
						{
							if (_mFrom.AutoRenewInsurance)
							{
								if (!_mFrom.HasGump(typeof(CancelRenewInventoryInsuranceGump)))
									_mFrom.SendGump(new CancelRenewInventoryInsuranceGump(_mFrom, this));
							}
							else
							{
								_mFrom.AutoRenewInventoryInsurance();
								_mFrom.SendGump(new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage));
							}

							break;
						}
					case 2: // OK
						{
							_mFrom.SendGump(new ItemInsuranceMenuConfirmGump(_mFrom, _mItems, _mInsure, _mPage));

							break;
						}
					case 3: // Prev
						{
							if (_mPage >= 1)
								_mFrom.SendGump(new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage - 1));

							break;
						}
					case 4: // Next
						{
							if ((_mPage + 1) * 4 < _mItems.Length)
								_mFrom.SendGump(new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage + 1));

							break;
						}
					default:
						{
							int idx = info.ButtonID - 100;

							if (idx >= 0 && idx < _mItems.Length)
								_mInsure[idx] = !_mInsure[idx];

							_mFrom.SendGump(new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage));

							break;
						}
				}
			}
		}

		private class ItemInsuranceMenuConfirmGump : Gump
		{
			private readonly PlayerMobile _mFrom;
			private readonly Item[] _mItems;
			private readonly bool[] _mInsure;
			private readonly int _mPage;

			public ItemInsuranceMenuConfirmGump(PlayerMobile from, Item[] items, bool[] insure, int page)
				: base(250, 200)
			{
				_mFrom = from;
				_mItems = items;
				_mInsure = insure;
				_mPage = page;

				AddBackground(0, 0, 240, 142, 0x13BE);
				AddImageTiled(6, 6, 228, 100, 0xA40);
				AddImageTiled(6, 116, 228, 20, 0xA40);
				AddAlphaRegion(6, 6, 228, 142);

				AddHtmlLocalized(8, 8, 228, 100, 1114300, 0x7FFF, false, false); // Do you wish to insure all newly selected items?

				AddButton(6, 116, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40, 118, 450, 20, 1060051, 0x7FFF, false, false); // CANCEL

				AddButton(114, 116, 0xFA5, 0xFA7, 1, GumpButtonType.Reply, 0);
				AddHtmlLocalized(148, 118, 450, 20, 1073996, 0x7FFF, false, false); // ACCEPT
			}

			public override void OnResponse(NetState sender, RelayInfo info)
			{
				if (!_mFrom.CheckAlive())
					return;

				if (info.ButtonID == 1)
				{
					for (int i = 0; i < _mItems.Length; ++i)
					{
						Item item = _mItems[i];

						if (item.Insured != _mInsure[i])
							_mFrom.ToggleItemInsurance_Callback(_mFrom, item, false);
					}
				}
				else
				{
					_mFrom.SendLocalizedMessage(1042021); // Cancelled.
					_mFrom.SendGump(new ItemInsuranceMenuGump(_mFrom, _mItems, _mInsure, _mPage));
				}
			}
		}

		#endregion

		private void ToggleTrades()
		{
			RefuseTrades = !RefuseTrades;
		}

		private void GetVendor()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (!CheckAlive() || house == null || !house.IsOwner(this) || house.InternalizedVendors.Count <= 0) return;
			CloseGump(typeof(ReclaimVendorGump));
			SendGump(new ReclaimVendorGump(house));
		}

		private void LeaveHouse()
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);

			if (house != null)
				Location = house.BanLocation;
		}

		private delegate void ContextCallback();

		private class CallbackEntry : ContextMenuEntry
		{
			private readonly ContextCallback _mCallback;

			public CallbackEntry(int number, ContextCallback callback) : this(number, -1, callback)
			{
			}

			public CallbackEntry(int number, int range, ContextCallback callback) : base(number, range)
			{
				_mCallback = callback;
			}

			public override void OnClick()
			{
				_mCallback?.Invoke();
			}
		}

		public override void DisruptiveAction()
		{
			if (Meditating)
			{
				RemoveBuff(BuffIcon.ActiveMeditation);
			}

			base.DisruptiveAction();
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (this == from && !Warmode)
			{
				IMount mount = Mount;

				if (mount != null && !DesignContext.Check(this))
					return;
			}

			base.OnDoubleClick(from);
		}

		public override void DisplayPaperdollTo(Mobile to)
		{
			if (DesignContext.Check(this))
				base.DisplayPaperdollTo(to);
		}

		private static bool _mNoRecursion;

		public override bool CheckEquip(Item item)
		{
			if (!base.CheckEquip(item))
				return false;

			#region Dueling
			if (DuelContext != null && !DuelContext.AllowItemEquip(this, item))
				return false;
			#endregion

			#region Factions
			FactionItem factionItem = FactionItem.Find(item);

			if (factionItem != null)
			{
				Faction faction = Faction.Find(this);

				if (faction == null)
				{
					SendLocalizedMessage(1010371); // You cannot equip a faction item!
					return false;
				}
				else if (faction != factionItem.Faction)
				{
					SendLocalizedMessage(1010372); // You cannot equip an opposing faction's item!
					return false;
				}
				else
				{
					int maxWearables = FactionItem.GetMaxWearables(this);

					for (int i = 0; i < Items.Count; ++i)
					{
						Item equiped = Items[i];

						if (item == equiped || FactionItem.Find(equiped) == null) continue;
						if (--maxWearables != 0) continue;
						SendLocalizedMessage(1010373); // You do not have enough rank to equip more faction items!
						return false;
					}
				}
			}
			#endregion

			if (AccessLevel < AccessLevel.GameMaster && item.Layer != Layer.Mount && HasTrade)
			{
				BounceInfo bounce = item.GetBounce();

				if (bounce != null)
				{
					if (bounce.m_Parent is Item parent)
					{
						if (parent == Backpack || parent.IsChildOf(Backpack))
							return true;
					}
					else if (bounce.m_Parent == this)
					{
						return true;
					}
				}

				SendLocalizedMessage(1004042); // You can only equip what you are already carrying while you have a trade pending.
				return false;
			}

			return true;
		}

		public override bool OnDragLift(Item item)
		{
			if ((item as IPromotionalToken)?.GumpType == null) return base.OnDragLift(item);
			Type t = ((IPromotionalToken)item).GumpType;

			if (HasGump(t))
				CloseGump(t);

			return base.OnDragLift(item);
		}

		public override bool CheckTrade(Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			int msgNum = 0;

			if (cont == null)
			{
				if (to.Holding != null)
					msgNum = 1062727; // You cannot trade with someone who is dragging something.
				else if (HasTrade)
					msgNum = 1062781; // You are already trading with someone else!
				else if (to.HasTrade)
					msgNum = 1062779; // That person is already involved in a trade
				else if (to is PlayerMobile pm && pm.RefuseTrades)
					msgNum = 1154111; // ~1_NAME~ is refusing all trades.
			}

			if (msgNum == 0 && item != null)
			{
				if (cont != null)
				{
					plusItems += cont.TotalItems;
					plusWeight += cont.TotalWeight;
				}

				if (Backpack == null || !Backpack.CheckHold(this, item, false, checkItems, plusItems, plusWeight))
					msgNum = 1004040; // You would not be able to hold this if the trade failed.
				else if (to.Backpack == null || !to.Backpack.CheckHold(to, item, false, checkItems, plusItems, plusWeight))
					msgNum = 1004039; // The recipient of this trade would not be able to carry this.
				else
					msgNum = CheckContentForTrade(item);
			}

			if (msgNum == 0) return true;
			if (!message) return false;
			if (msgNum == 1154111)
				SendLocalizedMessage(msgNum, to.Name);
			else
				SendLocalizedMessage(msgNum);

			return false;

		}

		private static int CheckContentForTrade(Item item)
		{
			if (item is TrapableContainer tContainer && tContainer.TrapType != TrapType.None)
				return 1004044; // You may not trade trapped items.

			if (StolenItem.IsStolen(item))
				return 1004043; // You may not trade recently stolen items.

			if (item is not Container) return 0;
			return item.Items.Select(CheckContentForTrade).FirstOrDefault(msg => msg != 0);
		}

		public override bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (!base.CheckNonlocalDrop(from, item, target))
				return false;

			if (from.AccessLevel >= AccessLevel.GameMaster)
				return true;

			Container pack = Backpack;
			if (from != this || !HasTrade || (target != pack && !target.IsChildOf(pack))) return true;
			BounceInfo bounce = item.GetBounce();

			if (bounce is {m_Parent: Item parent})
			{
				if (parent == pack || parent.IsChildOf(pack))
					return true;
			}

			SendLocalizedMessage(1004041); // You can't do that while you have a trade pending.
			return false;

		}

		protected override void OnLocationChange(Point3D oldLocation)
		{
			CheckLightLevels(false);

			#region Dueling

			DuelContext?.OnLocationChanged(this);
			#endregion

			DesignContext context = DesignContext;

			if (context == null || _mNoRecursion)
				return;

			_mNoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			int newX = X, newY = Y;
			int newZ = foundation.Z + HouseFoundation.GetLevelZ(context.Level, context.Foundation);

			int startX = foundation.X + foundation.Components.Min.X + 1;
			int startY = foundation.Y + foundation.Components.Min.Y + 1;
			int endX = startX + foundation.Components.Width - 1;
			int endY = startY + foundation.Components.Height - 2;

			if (newX >= startX && newY >= startY && newX < endX && newY < endY && Map == foundation.Map)
			{
				if (Z != newZ)
					Location = new Point3D(X, Y, newZ);

				_mNoRecursion = false;
				return;
			}

			Location = new Point3D(foundation.X, foundation.Y, newZ);
			Map = foundation.Map;

			_mNoRecursion = false;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m is BaseCreature {Controlled: false})
				return (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet) || (Hidden && AccessLevel > AccessLevel.Player);

			#region Dueling

			if (!Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)) || m is not PlayerMobile pm)
				return base.OnMoveOver(m);
			if (pm.DuelContext == null || pm.DuelPlayer == null || !pm.DuelContext.Started || pm.DuelContext.Finished || pm.DuelPlayer.Eliminated)
				return true;
			#endregion

			return base.OnMoveOver(m);
		}

		public override bool CheckShove(Mobile shoved)
		{
			if (_mIgnoreMobiles || TransformationSpellHelper.UnderTransformation(shoved, typeof(WraithFormSpell)))
				return true;
			else
				return base.CheckShove(shoved);
		}

		protected override void OnMapChange(Map oldMap)
		{
			if ((Map != Faction.Facet && oldMap == Faction.Facet) || (Map == Faction.Facet && oldMap != Faction.Facet))
				InvalidateProperties();

			#region Dueling

			DuelContext?.OnMapChanged(this);
			#endregion

			DesignContext context = DesignContext;

			if (context == null || _mNoRecursion)
				return;

			_mNoRecursion = true;

			HouseFoundation foundation = context.Foundation;

			if (Map != foundation.Map)
				Map = foundation.Map;

			_mNoRecursion = false;
		}

		public override void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			if (SentHonorContext != null)
				SentHonorContext.OnSourceBeneficialAction(target);

			base.OnBeneficialAction(target, isCriminal);
		}

		public override void OnDamage(int amount, Mobile from, bool willKill)
		{
			int disruptThreshold;

			if (!Core.AOS)
				disruptThreshold = 0;
			else if (from != null && from.Player)
				disruptThreshold = 18;
			else
				disruptThreshold = 25;

			if (amount > disruptThreshold)
			{
				BandageContext c = BandageContext.GetContext(this);

				c?.Slip();
			}

			if (Confidence.IsRegenerating(this))
				Confidence.StopRegenerating(this);

			if (ReceivedHonorContext != null)
				ReceivedHonorContext.OnTargetDamaged(from, amount);
			if (SentHonorContext != null)
				SentHonorContext.OnSourceDamaged(from, amount);

			if (willKill && from is PlayerMobile pm)
				Timer.DelayCall(TimeSpan.FromSeconds(10), pm.RecoverAmmo);

			#region Mondain's Legacy
			if (InvisibilityPotion.HasTimer(this))
			{
				InvisibilityPotion.Iterrupt(this);
			}
			#endregion

			//UndertakersStaff.TryRemoveTimer(this);

			base.OnDamage(amount, from, willKill);
		}

		public override void Resurrect()
		{
			bool wasAlive = Alive;

			base.Resurrect();

			if (!Alive || wasAlive) return;
			Item deathRobe = new DeathRobe();

			if (!EquipItem(deathRobe))
				deathRobe.Delete();
		}

		public override double RacialSkillBonus
		{
			get
			{
				if (Core.ML && Race == Race.Human)
					return 20.0;

				return 0;
			}
		}

		public override void OnWarmodeChanged()
		{
			if (!Warmode)
				Timer.DelayCall(TimeSpan.FromSeconds(10), RecoverAmmo);
		}

		private Mobile _mInsuranceAward;
		private int _mInsuranceCost;
		private int _mInsuranceBonus;

		public List<Item> EquipSnapshot { get; private set; }

		private bool FindItems_Callback(Item item)
		{
			if (!item.Deleted && (item.LootType == LootType.Blessed || item.Insured))
			{
				if (Backpack != item.Parent)
				{
					return true;
				}
			}
			return false;
		}

		public override bool OnBeforeDeath()
		{
			NetState state = NetState;

			if (state != null)
				state.CancelAllTrades();

			DropHolding();

			if (Core.AOS && Backpack != null && !Backpack.Deleted)
			{
				List<Item> ilist = Backpack.FindItemsByType<Item>(FindItems_Callback);

				for (var i = 0; i < ilist.Count; i++)
				{
					Backpack.AddItem(ilist[i]);
				}
			}

			EquipSnapshot = new List<Item>(Items);

			_mNonAutoreinsuredItems = 0;
			_mInsuranceCost = 0;
			_mInsuranceAward = base.FindMostRecentDamager(false);

			if (_mInsuranceAward is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();

				if (master != null)
					_mInsuranceAward = master;
			}

			if (_mInsuranceAward != null && (!_mInsuranceAward.Player || _mInsuranceAward == this))
				_mInsuranceAward = null;

			if (_mInsuranceAward is PlayerMobile pm)
				pm._mInsuranceBonus = 0;

			if (ReceivedHonorContext != null)
				ReceivedHonorContext.OnTargetKilled();
			if (SentHonorContext != null)
				HonorContext.OnSourceKilled();

			RecoverAmmo();

			return base.OnBeforeDeath();
		}

		private bool CheckInsuranceOnDeath(Item item)
		{
			if (!InsuranceEnabled || !item.Insured) return false;

			#region Dueling
			if (_mDuelPlayer != null && DuelContext != null && DuelContext.Registered && DuelContext.Started && !_mDuelPlayer.Eliminated)
				return true;
			#endregion

			if (AutoRenewInsurance)
			{
				int cost = GetInsuranceCost(item);

				if (_mInsuranceAward != null)
					cost /= 2;

				if (Banker.Withdraw(this, cost))
				{
					_mInsuranceCost += cost;
					item.PayedInsurance = true;
					SendLocalizedMessage(1060398, cost.ToString()); // ~1_AMOUNT~ gold has been withdrawn from your bank box.
				}
				else
				{
					SendLocalizedMessage(1061079, 0x23); // You lack the funds to purchase the insurance
					item.PayedInsurance = false;
					item.Insured = false;
					_mNonAutoreinsuredItems++;
				}
			}
			else
			{
				item.PayedInsurance = false;
				item.Insured = false;
			}

			if (_mInsuranceAward == null) return true;
			if (!Banker.Deposit(_mInsuranceAward, 300)) return true;
			if (_mInsuranceAward is PlayerMobile pm)
				pm._mInsuranceBonus += 300;

			return true;

		}

		public override DeathMoveResult GetParentMoveResultFor(Item item)
		{
			// It seems all items are unmarked on death, even blessed/insured ones
			if (item.QuestItem)
				item.QuestItem = false;

			if (CheckInsuranceOnDeath(item))
				return DeathMoveResult.MoveToBackpack;

			DeathMoveResult res = base.GetParentMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
				res = DeathMoveResult.MoveToBackpack;

			return res;
		}

		public override DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			// It seems all items are unmarked on death, even blessed/insured ones
			if (item.QuestItem)
				item.QuestItem = false;

			if (CheckInsuranceOnDeath(item))
				return DeathMoveResult.MoveToBackpack;

			DeathMoveResult res = base.GetInventoryMoveResultFor(item);

			if (res == DeathMoveResult.MoveToCorpse && item.Movable && Young)
				res = DeathMoveResult.MoveToBackpack;

			return res;
		}

		public override void OnDeath(Container c)
		{
			if (_mNonAutoreinsuredItems > 0)
			{
				SendLocalizedMessage(1061115);
			}

			base.OnDeath(c);

			EquipSnapshot = null;

			HueMod = -1;
			NameMod = null;
			SavagePaintExpiration = TimeSpan.Zero;

			SetHairMods(-1, -1);

			PolymorphSpell.StopTimer(this);
			IncognitoSpell.StopTimer(this);
			DisguiseTimers.RemoveTimer(this);
			WeakenSpell.RemoveEffects(this);
			ClumsySpell.RemoveEffects(this);
			FeeblemindSpell.RemoveEffects(this);
			EndAction(typeof(PolymorphSpell));
			EndAction(typeof(IncognitoSpell));

			MeerMage.StopEffect(this, false);

			#region Stygian Abyss
			if (Flying)
			{
				Flying = false;
				BuffInfo.RemoveBuff(this, BuffIcon.Fly);
			}
			#endregion

			SkillHandlers.StolenItem.ReturnOnDeath(this, c);

			if (PermaFlags.Count > 0)
			{
				PermaFlags.Clear();

				if (c is Corpse corpse)
					corpse.Criminal = true;

				if (SkillHandlers.Stealing.ClassicMode)
					Criminal = true;
			}

			if (Murderer && DateTime.UtcNow >= _mNextJustAward)
			{
				Mobile m = FindMostRecentDamager(false);

				if (m is BaseCreature baseCreature)
					m = baseCreature.GetMaster();

				if (m != null && m is PlayerMobile && m != this)
				{
					bool gainedPath = false;

					int pointsToGain = 0;

					pointsToGain += (int)Math.Sqrt(GameTime.TotalSeconds * 4);
					pointsToGain *= 5;
					pointsToGain += (int)Math.Pow(Skills.Total / 250, 2);

					if (VirtueHelper.Award(m, VirtueName.Justice, pointsToGain, ref gainedPath))
					{
						if (gainedPath)
							m.SendLocalizedMessage(1049367); // You have gained a path in Justice!
						else
							m.SendLocalizedMessage(1049363); // You have gained in Justice.

						m.FixedParticles(0x375A, 9, 20, 5027, EffectLayer.Waist);
						m.PlaySound(0x1F7);

						_mNextJustAward = DateTime.UtcNow + TimeSpan.FromMinutes(pointsToGain / 3);
					}
				}
			}

			if (_mInsuranceAward is PlayerMobile {_mInsuranceBonus: > 0} pm) pm.SendLocalizedMessage(1060397, pm._mInsuranceBonus.ToString()); // ~1_AMOUNT~ gold has been deposited into your bank box.

			Mobile killer = FindMostRecentDamager(true);

			OnKilledBy(killer);

			if (killer is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();
				if (master != null)
					killer = master;
			}

			if (Young && DuelContext == null)
			{
				if (YoungDeathTeleport())
					Timer.DelayCall(TimeSpan.FromSeconds(2.5), SendYoungDeathNotice);
			}

			if (DuelContext == null || !DuelContext.Registered || !DuelContext.Started || _mDuelPlayer == null || _mDuelPlayer.Eliminated)
				Faction.HandleDeath(this, killer);

			Guilds.Guild.HandleDeath(this, killer);

			#region Dueling
			if (DuelContext != null)
				DuelContext.OnDeath(this, c);
			#endregion

			if (_mBuffTable == null) return;
			List<BuffInfo> list = _mBuffTable.Values.Where(buff => !buff.RetainThroughDeath).ToList();

			for (var i = 0; i < list.Count; i++)
			{
				RemoveBuff(list[i]);
			}
		}

		#region Stuck Menu
		private DateTime[] _mStuckMenuUses;

		public bool CanUseStuckMenu()
		{
			if (_mStuckMenuUses == null)
			{
				return true;
			}
			else
			{
				for (var i = 0; i < _mStuckMenuUses.Length; ++i)
				{
					if ((DateTime.UtcNow - _mStuckMenuUses[i]) > TimeSpan.FromDays(1.0))
					{
						return true;
					}
				}

				return false;
			}
		}

		public void UsedStuckMenu()
		{
			_mStuckMenuUses ??= new DateTime[2];

			for (var i = 0; i < _mStuckMenuUses.Length; ++i)
			{
				if ((DateTime.UtcNow - _mStuckMenuUses[i]) <= TimeSpan.FromDays(1.0)) continue;
				_mStuckMenuUses[i] = DateTime.UtcNow;
				return;
			}
		}
		#endregion

		private readonly Hashtable _mAntiMacroTable;
		private TimeSpan _mGameTime;
		private TimeSpan _mShortTermElapse;
		private TimeSpan _mLongTermElapse;
		private DateTime _mNextSmithBulkOrder;
		private DateTime _mNextTailorBulkOrder;
		private DateTime _mSavagePaintExpiration;

		public SkillName Learning { get; set; } = (SkillName)(-1);

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan SavagePaintExpiration
		{
			get
			{
				TimeSpan ts = _mSavagePaintExpiration - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set => _mSavagePaintExpiration = DateTime.UtcNow + value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextSmithBulkOrder
		{
			get
			{
				TimeSpan ts = _mNextSmithBulkOrder - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try { _mNextSmithBulkOrder = DateTime.UtcNow + value; }
				catch
				{
					// ignored
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan NextTailorBulkOrder
		{
			get
			{
				TimeSpan ts = _mNextTailorBulkOrder - DateTime.UtcNow;

				if (ts < TimeSpan.Zero)
					ts = TimeSpan.Zero;

				return ts;
			}
			set
			{
				try { _mNextTailorBulkOrder = DateTime.UtcNow + value; }
				catch
				{
					// ignored
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastEscortTime { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastPetBallTime { get; set; }

		public PlayerMobile()
		{
			Instances.Add(this);
			AutoStabled = new List<Mobile>();

			VisibilityList = new List<Mobile>();
			PermaFlags = new List<Mobile>();
			_mAntiMacroTable = new Hashtable();
			RecentlyReported = new List<Mobile>();

			#region Mondain's Legacy Quests
			DoneQuests = new List<QuestRestartInfo>();
			Collections = new Dictionary<Collection, int>();
			RewardTitles = new List<object>();
			PeacedUntil = DateTime.UtcNow;
			#endregion

			BobFilter = new Engines.BulkOrders.BOBFilter();

			_mGameTime = TimeSpan.Zero;
			_mShortTermElapse = MKillShortTermDelay;
			_mLongTermElapse = MKillLongTermDelay;

			JusticeProtectors = new List<Mobile>();
			_mGuildRank = Guilds.RankDefinition.Lowest;

			_mChampionTitles = new ChampionTitleInfo();
		}

		public override bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
		{
			if (Alive)
				return false;

			if (Core.ML && Skills[SkillName.SpiritSpeak].Value >= 100.0)
				return false;

			if (!Core.AOS) return base.MutateSpeech(hears, ref text, ref context);
			for (var i = 0; i < hears.Count; ++i)
			{
				Mobile m = hears[i];

				if (m != this && m.Skills[SkillName.SpiritSpeak].Value >= 100.0)
					return false;
			}

			return base.MutateSpeech(hears, ref text, ref context);
		}

		public override void DoSpeech(string text, int[] keywords, MessageType type, int hue)
		{
			if (Guilds.Guild.NewGuildSystem && type is MessageType.Guild or MessageType.Alliance)
			{
				if (Guild is not Guilds.Guild g)
				{
					SendLocalizedMessage(1063142); // You are not in a guild!
				}
				else if (type == MessageType.Alliance)
				{
					if (g.Alliance != null && g.Alliance.IsMember(g))
					{
						//g.Alliance.AllianceTextMessage( hue, "[Alliance][{0}]: {1}", this.Name, text );
						g.Alliance.AllianceChat(this, text);
						SendToStaffMessage(this, "[Alliance]: {0}", text);

						AllianceMessageHue = hue;
					}
					else
					{
						SendLocalizedMessage(1071020); // You are not in an alliance!
					}
				}
				else    //Type == MessageType.Guild
				{
					GuildMessageHue = hue;

					g.GuildChat(this, text);
					SendToStaffMessage(this, "[Guild]: {0}", text);
				}
			}
			else
			{
				base.DoSpeech(text, keywords, type, hue);
			}
		}

		private static void SendToStaffMessage(Mobile from, string text)
		{
			Packet p = null;

			foreach (NetState ns in from.GetClientsInRange(8))
			{
				Mobile mob = ns.Mobile;

				if (mob == null || mob.AccessLevel < AccessLevel.GameMaster ||
				    mob.AccessLevel <= from.AccessLevel) continue;
				p ??= Packet.Acquire(new UnicodeMessage(from.Serial, from.Body, MessageType.Regular, from.SpeechHue, 3, from.Language,
					from.Name, text));

				ns.Send(p);
			}

			Packet.Release(p);
		}

		private static void SendToStaffMessage(Mobile from, string format, params object[] args)
		{
			SendToStaffMessage(from, string.Format(format, args));
		}

		/*public override void Damage(int amount, Mobile from)
		{
			if (EvilOmenSpell.TryEndEffect(this))
				amount = (int)(amount * 1.25);

			Mobile oath = BloodOathSpell.GetBloodOath(from);

			/* Per EA's UO Herald Pub48 (ML):
			 * ((resist spellsx10)/20 + 10=percentage of damage resisted)
			 */

			/*if (oath == this)
			{
				amount = (int)(amount * 1.1);

				if (amount > 35 && from is PlayerMobile)  /* capped @ 35, seems no expansion */
				/*{
					amount = 35;
				}

				if (Core.ML)
				{
					from.Damage((int)(amount * (1 - (((from.Skills.MagicResist.Value * .5) + 10) / 100))), this);
				}
				else
				{
					from.Damage(amount, this);
				}
			}

			if (from != null && Talisman is BaseTalisman talisman)
			{
				if (talisman.Protection != null && talisman.Protection.Type != null)
				{
					Type type = talisman.Protection.Type;

					if (type.IsAssignableFrom(from.GetType()))
						amount = (int)(amount * (1 - (double)talisman.Protection.Amount / 100));
				}
			}

			base.Damage(amount, from);
		}*/

		#region Poison

		public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
		{
			if (!Alive)
				return ApplyPoisonResult.Immune;

			if (EvilOmenSpell.TryEndEffect(this))
				poison = PoisonImpl.IncreaseLevel(poison);

			ApplyPoisonResult result = base.ApplyPoison(from, poison);

			if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer)
				((PoisonImpl.PoisonTimer) PoisonTimer).From = from;

			return result;
		}

		public override bool CheckPoisonImmunity(Mobile from, Poison poison)
		{
			if (Young && (DuelContext == null || !DuelContext.Started || DuelContext.Finished))
				return true;

			return base.CheckPoisonImmunity(from, poison);
		}

		public override void OnPoisonImmunity(Mobile from, Poison poison)
		{
			if (Young && (DuelContext == null || !DuelContext.Started || DuelContext.Finished))
				SendLocalizedMessage(502808); // You would have been poisoned, were you not new to the land of Britannia. Be careful in the future.
			else
				base.OnPoisonImmunity(from, poison);
		}

		#endregion

		public PlayerMobile(Serial s) : base(s)
		{
			Instances.Add(this);
			VisibilityList = new List<Mobile>();
			_mAntiMacroTable = new Hashtable();
		}

		public List<Mobile> VisibilityList { get; }

		public List<Mobile> PermaFlags { get; private set; }

		public override int Luck => AosAttributes.GetValue(this, AosAttribute.Luck);

		public override bool IsHarmfulCriminal(IDamageable damageable)
		{
			Mobile target = damageable as Mobile;

			if (Stealing.ClassicMode && target is PlayerMobile && ((PlayerMobile)target).PermaFlags.Count > 0)
			{
				int noto = Notoriety.Compute(this, target);

				if (noto == Notoriety.Innocent)
					target.Delta(MobileDelta.Noto);

				return false;
			}

			if (target is BaseCreature {InitialInnocent: true, Controlled: false})
				return false;

			if (Core.ML && target is BaseCreature {Controlled: true} targetBc && this == targetBc.ControlMaster)
				return false;

			if (target is BaseCreature {Summoned: true} creature && creature.SummonMaster == this)
			{
				return false;
			}

			return base.IsHarmfulCriminal(damageable);
		}

		public bool AntiMacroCheck(Skill skill, object obj)
		{
			if (obj == null || _mAntiMacroTable == null || AccessLevel != AccessLevel.Player)
				return true;

			Hashtable tbl = (Hashtable)_mAntiMacroTable[skill];
			if (tbl == null)
				_mAntiMacroTable[skill] = tbl = new Hashtable();

			CountAndTimeStamp count = (CountAndTimeStamp)tbl[obj];
			if (count != null)
			{
				if (count.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow)
				{
					count.Count = 1;
					return true;
				}
				else
				{
					++count.Count;
					if (count.Count <= SkillCheck.Allowance)
						return true;
					else
						return false;
				}
			}
			else
			{
				tbl[obj] = count = new CountAndTimeStamp();
				count.Count = 1;

				return true;
			}
		}

		public Engines.BulkOrders.BOBFilter BobFilter { get; private set; }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						LastLogin = reader.ReadDateTime();
						NextBountyDecay = reader.ReadDateTime();
						_mBounty = reader.ReadInt();
						RegionGump = reader.ReadBool();
						AutoSaveGump = reader.ReadBool();
						ExtendedFlags = (ExtendedPlayerFlag)reader.ReadInt();
						Collections = new Dictionary<Collection, int>();
						RewardTitles = new List<object>();

						for (int i = reader.ReadInt(); i > 0; i--)
						{
							Collections.Add((Collection)reader.ReadInt(), reader.ReadInt());
						}

						for (int i = reader.ReadInt(); i > 0; i--)
						{
							RewardTitles.Add(QuestReader.Object(reader));
						}

						SelectedTitle = reader.ReadInt();

						if (reader.ReadBool())
						{
							_mStuckMenuUses = new DateTime[reader.ReadInt()];

							for (int i = 0; i < _mStuckMenuUses.Length; ++i)
							{
								_mStuckMenuUses[i] = reader.ReadDateTime();
							}
						}
						else
						{
							_mStuckMenuUses = null;
						}
						PeacedUntil = reader.ReadDateTime();

						AnkhNextUse = reader.ReadDateTime();

						AutoStabled = reader.ReadStrongMobileList();

						int recipeCount = reader.ReadInt();

						if (recipeCount > 0)
						{
							_mAcquiredRecipes = new Dictionary<int, bool>();

							for (int i = 0; i < recipeCount; i++)
							{
								int r = reader.ReadInt();
								if (reader.ReadBool())  //Don't add in recipies which we haven't gotten or have been removed
									_mAcquiredRecipes.Add(r, true);
							}
						}

						LastHonorLoss = reader.ReadDeltaTime();

						_mChampionTitles = new ChampionTitleInfo(reader);

						LastValorLoss = reader.ReadDateTime();

						ToTItemsTurnedIn = reader.ReadEncodedInt();
						ToTTotalMonsterFame = reader.ReadInt();

						AllianceMessageHue = reader.ReadEncodedInt();
						GuildMessageHue = reader.ReadEncodedInt();

						int rank = reader.ReadEncodedInt();
						int maxRank = Guilds.RankDefinition.Ranks.Length - 1;
						if (rank > maxRank)
							rank = maxRank;

						_mGuildRank = Guilds.RankDefinition.Ranks[rank];
						LastOnline = reader.ReadDateTime();

						SolenFriendship = (SolenFriendship)reader.ReadEncodedInt();

						Quest = QuestSerializer.DeserializeQuest(reader);

						if (Quest != null)
							Quest.From = this;

						int count = reader.ReadEncodedInt();

						if (count > 0)
						{
							DoneQuests = new List<QuestRestartInfo>();

							for (int i = 0; i < count; ++i)
							{
								Type questType = QuestSerializer.ReadType(QuestSystem.QuestTypes, reader);

								var restartTime = reader.ReadDateTime();

								DoneQuests.Add(new QuestRestartInfo(questType, restartTime));
							}
						}

						Profession = reader.ReadEncodedInt();

						LastCompassionLoss = reader.ReadDeltaTime();

						CompassionGains = reader.ReadEncodedInt();

						if (CompassionGains > 0)
							NextCompassionDay = reader.ReadDeltaTime();

						BobFilter = new Engines.BulkOrders.BOBFilter(reader);

						if (reader.ReadBool())
						{
							_mHairModId = reader.ReadInt();
							_mHairModHue = reader.ReadInt();
							_mBeardModId = reader.ReadInt();
							_mBeardModHue = reader.ReadInt();
						}

						SavagePaintExpiration = reader.ReadTimeSpan();

						if (SavagePaintExpiration > TimeSpan.Zero)
						{
							BodyMod = Female ? 184 : 183;
							HueMod = 0;
						}

						NpcGuild = (NpcGuild)reader.ReadInt();
						NpcGuildJoinTime = reader.ReadDateTime();
						NpcGuildGameTime = reader.ReadTimeSpan();

						PermaFlags = reader.ReadStrongMobileList();

						NextTailorBulkOrder = reader.ReadTimeSpan();

						NextSmithBulkOrder = reader.ReadTimeSpan();

						LastJusticeLoss = reader.ReadDeltaTime();
						JusticeProtectors = reader.ReadStrongMobileList();

						LastSacrificeGain = reader.ReadDeltaTime();
						LastSacrificeLoss = reader.ReadDeltaTime();
						AvailableResurrects = reader.ReadInt();

						Flags = (PlayerFlag)reader.ReadInt();

						_mLongTermElapse = reader.ReadTimeSpan();
						_mShortTermElapse = reader.ReadTimeSpan();
						_mGameTime = reader.ReadTimeSpan();

						break;
					}
			}

			RecentlyReported ??= new List<Mobile>();

			PermaFlags ??= new List<Mobile>();

			JusticeProtectors ??= new List<Mobile>();

			BobFilter ??= new Engines.BulkOrders.BOBFilter();

			_mGuildRank ??= Guilds.RankDefinition.Member;

			if (LastOnline == DateTime.MinValue && Account != null)
				LastOnline = ((Account)Account).LastLogin;

			_mChampionTitles ??= new ChampionTitleInfo();

			if (AccessLevel > AccessLevel.Player)
				_mIgnoreMobiles = true;

			foreach (Mobile pet in Stabled)
			{
				if (pet is not BaseCreature bc) continue;
				bc.IsStabled = true;
				bc.StabledBy = this;
			}

			#region Mondain's Legacy
			DoneQuests ??= new List<QuestRestartInfo>();

			Collections ??= new Dictionary<Collection, int>();

			RewardTitles ??= new List<object>();
			#endregion

			if (NextBountyDecay == DateTime.MinValue)
			{
				if (LastLogin != DateTime.MinValue)
					NextBountyDecay = LastLogin + TimeSpan.FromDays(1.0);
			}

			while (_mBounty > 0 && NextBountyDecay < DateTime.UtcNow)
			{
				_mBounty -= 100;
				NextBountyDecay += TimeSpan.FromDays(1.0);
			}

			if (_mBounty <= 0)
			{
				_mBounty = 0;
				Kills = 0;
			}

			if (_mBounty > 0 && _mBounty > BountyBoard.LowestBounty)
				BountyBoard.Update(this);

			CheckAtrophies(this);

			if (Hidden) //Hiding is the only buff where it has an effect that's serialized.
				AddBuff(new BuffInfo(BuffIcon.HidingAndOrStealth, 1075655));
		}

		public override void Serialize(GenericWriter writer)
		{
			//cleanup our anti-macro table
			foreach (Hashtable t in _mAntiMacroTable.Values)
			{
				List<CountAndTimeStamp> remove = t.Values.Cast<CountAndTimeStamp>().Where(time => time.TimeStamp + SkillCheck.AntiMacroExpire <= DateTime.UtcNow).ToList();

				for (var i = 0; i < remove.Count; ++i)
					t.Remove(remove[i]);
			}

			if (NextBountyDecay != DateTime.MinValue)
			{
				bool update = false;
				while (_mBounty > 0 && NextBountyDecay < DateTime.UtcNow)
				{
					update = true;
					_mBounty -= 100;
					NextBountyDecay += TimeSpan.FromDays(1.0);
				}

				if (_mBounty < 0)
					_mBounty = 0;

				if (update)
					BountyBoard.Update(this);
			}

			CheckKillDecay();

			CheckAtrophies(this);

			base.Serialize(writer);

			writer.Write(0); // version
			writer.Write(LastLogin);
			writer.Write(NextBountyDecay);
			writer.Write(_mBounty);
			writer.Write(AutoSaveGump);
			writer.Write(RegionGump);
			writer.Write((int)ExtendedFlags);
			#region Mondain's Legacy
			if (Collections == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(Collections.Count);

				foreach (var pair in Collections)
				{
					writer.Write((int)pair.Key);
					writer.Write(pair.Value);
				}
			}

			if (RewardTitles == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(RewardTitles.Count);

				for (var i = 0; i < RewardTitles.Count; i++)
				{
					QuestWriter.Object(writer, RewardTitles[i]);
				}
			}

			writer.Write(SelectedTitle);
			#endregion

			if (_mStuckMenuUses != null)
			{
				writer.Write(true);

				writer.Write(_mStuckMenuUses.Length);

				for (var i = 0; i < _mStuckMenuUses.Length; ++i)
				{
					writer.Write(_mStuckMenuUses[i]);
				}
			}
			else
			{
				writer.Write(false);
			}

			writer.Write(PeacedUntil);
			writer.Write(AnkhNextUse);
			writer.Write(AutoStabled, true);

			if (_mAcquiredRecipes == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(_mAcquiredRecipes.Count);

				foreach (KeyValuePair<int, bool> kvp in _mAcquiredRecipes)
				{
					writer.Write(kvp.Key);
					writer.Write(kvp.Value);
				}
			}

			writer.WriteDeltaTime(LastHonorLoss);

			ChampionTitleInfo.Serialize(writer, _mChampionTitles);

			writer.Write(LastValorLoss);
			writer.WriteEncodedInt(ToTItemsTurnedIn);
			writer.Write(ToTTotalMonsterFame);    //This ain't going to be a small #.

			writer.WriteEncodedInt(AllianceMessageHue);
			writer.WriteEncodedInt(GuildMessageHue);

			writer.WriteEncodedInt(_mGuildRank.Rank);
			writer.Write(LastOnline);

			writer.WriteEncodedInt((int)SolenFriendship);

			QuestSerializer.Serialize(Quest, writer);

			if (DoneQuests == null)
			{
				writer.WriteEncodedInt(0);
			}
			else
			{
				writer.WriteEncodedInt(DoneQuests.Count);

				for (var i = 0; i < DoneQuests.Count; ++i)
				{
					QuestRestartInfo restartInfo = DoneQuests[i];

					QuestSerializer.Write(restartInfo.QuestType, QuestSystem.QuestTypes, writer);
					writer.Write(restartInfo.RestartTime);
				}
			}

			writer.WriteEncodedInt(Profession);

			writer.WriteDeltaTime(LastCompassionLoss);

			writer.WriteEncodedInt(CompassionGains);

			if (CompassionGains > 0)
				writer.WriteDeltaTime(NextCompassionDay);

			BobFilter.Serialize(writer);

			bool useMods = (_mHairModId != -1 || _mBeardModId != -1);

			writer.Write(useMods);

			if (useMods)
			{
				writer.Write(_mHairModId);
				writer.Write(_mHairModHue);
				writer.Write(_mBeardModId);
				writer.Write(_mBeardModHue);
			}

			writer.Write(SavagePaintExpiration);

			writer.Write((int)NpcGuild);
			writer.Write(NpcGuildJoinTime);
			writer.Write(NpcGuildGameTime);

			writer.Write(PermaFlags, true);

			writer.Write(NextTailorBulkOrder);

			writer.Write(NextSmithBulkOrder);

			writer.WriteDeltaTime(LastJusticeLoss);
			writer.Write(JusticeProtectors, true);

			writer.WriteDeltaTime(LastSacrificeGain);
			writer.WriteDeltaTime(LastSacrificeLoss);
			writer.Write(AvailableResurrects);

			writer.Write((int)Flags);

			writer.Write(_mLongTermElapse);
			writer.Write(_mShortTermElapse);
			writer.Write(GameTime);
		}

		public static void CheckAtrophies(Mobile m)
		{
			SacrificeVirtue.CheckAtrophy(m);
			JusticeVirtue.CheckAtrophy(m);
			CompassionVirtue.CheckAtrophy(m);
			ValorVirtue.CheckAtrophy(m);

			if (m is PlayerMobile mobile)
				ChampionTitleInfo.CheckAtrophy(mobile);
		}

		public void CheckKillDecay()
		{
			if (_mShortTermElapse < GameTime)
			{
				_mShortTermElapse += TimeSpan.FromHours(8);
				if (ShortTermMurders > 0)
					--ShortTermMurders;
			}

			if (_mLongTermElapse < GameTime)
			{
				_mLongTermElapse += TimeSpan.FromHours(40);
				if (Kills > 0)
					--Kills;
			}
		}

		public void ResetKillTime()
		{
			_mShortTermElapse = GameTime + TimeSpan.FromHours(8);
			_mLongTermElapse = GameTime + TimeSpan.FromHours(40);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SessionStart { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan GameTime
		{
			get
			{
				if (NetState != null)
					return _mGameTime + (DateTime.UtcNow - SessionStart);
				else
					return _mGameTime;
			}
		}

		public override bool CanSee(Mobile m)
		{
			if (m is IConditionalVisibility && !((IConditionalVisibility)m).CanBeSeenBy(this))
				return false;

			if (m is CharacterStatue statue)
				statue.OnRequestedAnimation(this);

			if (m is PlayerMobile pm && pm.VisibilityList.Contains(this))
				return true;

			if (DuelContext == null || _mDuelPlayer == null || DuelContext.Finished ||
			    DuelContext.m_Tournament == null || _mDuelPlayer.Eliminated) return base.CanSee(m);
			Mobile owner = m;

			if (owner is BaseCreature bc)
			{
				Mobile master = bc.GetMaster();

				if (master != null)
					owner = master;
			}

			if (m.AccessLevel == AccessLevel.Player && owner is PlayerMobile pmOwner && pmOwner.DuelContext != DuelContext)
			{
				return false;
			}

			return base.CanSee(m);
		}

		public virtual void CheckedAnimate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			if (!Mounted)
			{
				base.Animate(action, frameCount, repeatCount, forward, repeat, delay);
			}
		}

		public override void Animate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			base.Animate(action, frameCount, repeatCount, forward, repeat, delay);
		}

		public override bool CanSee(Item item)
		{
			if (item is IConditionalVisibility && !((IConditionalVisibility)item).CanBeSeenBy(this))
				return false;

			if (DesignContext != null && DesignContext.Foundation.IsHiddenToCustomizer(this, item))
			{
				return false;
			}

			if (IsPlayer())
			{
				Region r = item.GetRegion();

				if (r is BaseRegion region && !region.CanSee(this, item))
				{
					return false;
				}
			}

			return base.CanSee(item);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Faction faction = Faction.Find(this);

			faction?.RemoveMember(this);

			BaseHouse.HandleDeletion(this);

			DisguiseTimers.RemoveTimer(this);
		}

		public override bool NewGuildDisplay => Server.Guilds.Guild.NewGuildSystem;

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (AccessLevel > AccessLevel.Player)
				list.Add(1060847, "{0}\t{1}", "Shard", Enum.GetName(typeof(AccessLevel), AccessLevel));

			#region Mondain's Legacy Titles
			if (Core.ML && RewardTitles != null && SelectedTitle > -1)
			{
				if (SelectedTitle < RewardTitles.Count)
				{
					switch (RewardTitles[SelectedTitle])
					{
						case int:
							//if ((int)RewardTitles[SelectedTitle] == 1154017 && CityLoyaltySystem.HasCustomTitle(this, out cust))
							//{
							//	list.Add(1154017, cust); // ~1_TITLE~ of ~2_CITY~
							//}
							//else
							list.Add((int)RewardTitles[SelectedTitle]);
							break;
						case string @string:
							list.Add(1070722, @string);
							break;
					}
				}
			}
			#endregion

			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					Faction faction = pl.Faction;

					if (faction.Commander == this)
						list.Add(1042733, faction.Definition.PropName); // Commanding Lord of the ~1_FACTION_NAME~
					else if (pl.Sheriff != null)
						list.Add(1042734, "{0}\t{1}", pl.Sheriff.Definition.FriendlyName, faction.Definition.PropName); // The Sheriff of  ~1_CITY~, ~2_FACTION_NAME~
					else if (pl.Finance != null)
						list.Add(1042735, "{0}\t{1}", pl.Finance.Definition.FriendlyName, faction.Definition.PropName); // The Finance Minister of ~1_CITY~, ~2_FACTION_NAME~
					else if (pl.MerchantTitle != MerchantTitle.None)
						list.Add(1060776, "{0}\t{1}", MerchantTitles.GetInfo(pl.MerchantTitle).Title, faction.Definition.PropName); // ~1_val~, ~2_val~
					else
						list.Add(1060776, "{0}\t{1}", pl.Rank.Title, faction.Definition.PropName); // ~1_val~, ~2_val~
				}
			}

			if (!Core.ML) return;
			for (var i = AllFollowers.Count - 1; i >= 0; i--)
			{
				if (AllFollowers[i] is not BaseCreature c || c.ControlOrder != OrderType.Guard) continue;
				list.Add(501129); // guarded
				break;
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			if (Map == Faction.Facet)
			{
				PlayerState pl = PlayerState.Find(this);

				if (pl != null)
				{
					string text;
					bool ascii = false;

					Faction faction = pl.Faction;

					if (faction.Commander == this)
						text = string.Concat(Female ? "(Commanding Lady of the " : "(Commanding Lord of the ", faction.Definition.FriendlyName, ")");
					else if (pl.Sheriff != null)
						text = string.Concat("(The Sheriff of ", pl.Sheriff.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					else if (pl.Finance != null)
						text = string.Concat("(The Finance Minister of ", pl.Finance.Definition.FriendlyName, ", ", faction.Definition.FriendlyName, ")");
					else
					{
						ascii = true;

						if (pl.MerchantTitle != MerchantTitle.None)
							text = string.Concat("(", MerchantTitles.GetInfo(pl.MerchantTitle).Title.String, ", ", faction.Definition.FriendlyName, ")");
						else
							text = string.Concat("(", pl.Rank.Title.String, ", ", faction.Definition.FriendlyName, ")");
					}

					int hue = (Faction.Find(from) == faction ? 98 : 38);

					PrivateOverheadMessage(MessageType.Label, hue, ascii, text, from.NetState);
				}
			}

			base.OnSingleClick(from);
		}

		protected override bool OnMove(Direction d)
		{
			if (!Core.SE)
				return base.OnMove(d);

			if (AccessLevel != AccessLevel.Player)
				return true;

			if (!Hidden || DesignContext.Find(this) != null) return true;
			if (!Mounted && Skills.Stealth.Value >= 25.0)
			{
				bool running = (d & Direction.Running) != 0;

				if (running)
				{
					if ((AllowedStealthSteps -= 2) <= 0)
						RevealingAction();
				}
				else if (AllowedStealthSteps-- <= 0)
				{
					Stealth.OnUse(this);
				}
			}
			else
			{
				RevealingAction();
			}

			return true;
		}

		public bool BedrollLogout { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public override bool Paralyzed
		{
			get => base.Paralyzed;
			set
			{
				base.Paralyzed = value;

				if (value)
					AddBuff(new BuffInfo(BuffIcon.Paralyze, 1075827));  //Paralyze/You are frozen and can not move
				else
					RemoveBuff(BuffIcon.Paralyze);
			}
		}


		[CommandProperty(AccessLevel.GameMaster)]
		public override bool Meditating
		{
			get => base.Meditating;
			set
			{
				base.Meditating = value;
				if (value == false)
				{
					RemoveBuff(BuffIcon.ActiveMeditation);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Ethics.Player EthicPlayer { get; set; }

		public PlayerState FactionPlayerState { get; set; }

		#region Dueling
		private Engines.ConPVP.DuelPlayer _mDuelPlayer;

		public Engines.ConPVP.DuelContext DuelContext { get; private set; }

		public Engines.ConPVP.DuelPlayer DuelPlayer
		{
			get => _mDuelPlayer;
			set
			{
				bool wasInTourny = (DuelContext != null && !DuelContext.Finished && DuelContext.m_Tournament != null);

				_mDuelPlayer = value;

				if (_mDuelPlayer == null)
					DuelContext = null;
				else
					DuelContext = _mDuelPlayer.Participant.Context;

				bool isInTourny = (DuelContext != null && !DuelContext.Finished && DuelContext.m_Tournament != null);

				if (wasInTourny != isInTourny)
					SendEverything();
			}
		}
		#endregion

		public QuestSystem Quest { get; set; }

		public List<QuestRestartInfo> DoneQuests { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public SolenFriendship SolenFriendship { get; set; }

		#region Mondain's Legacy Quests
		public List<BaseQuest> Quests => MondainQuestData.GetQuests(this);

		public Dictionary<QuestChain, BaseChain> Chains => MondainQuestData.GetChains(this);

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Peaced => PeacedUntil > DateTime.UtcNow;

		public Dictionary<Collection, int> Collections { get; private set; }

		public List<object> RewardTitles { get; private set; }

		public int SelectedTitle { get; private set; }

		public bool RemoveRewardTitle(object o, bool silent)
		{
			if (!RewardTitles.Contains(o)) return false;
			int i = RewardTitles.IndexOf(o);

			if (i == SelectedTitle)
				SelectRewardTitle(-1, silent);
			else if (i > SelectedTitle)
				SelectRewardTitle(SelectedTitle - 1, silent);

			RewardTitles.Remove(o);

			return true;

		}

		public int GetCollectionPoints(Collection collection)
		{
			Collections ??= new Dictionary<Collection, int>();

			int points = 0;

			if (Collections.ContainsKey(collection))
			{
				Collections.TryGetValue(collection, out points);
			}

			return points;
		}

		public void AddCollectionPoints(Collection collection, int points)
		{
			Collections ??= new Dictionary<Collection, int>();

			if (Collections.ContainsKey(collection))
			{
				Collections[collection] += points;
			}
			else
			{
				Collections.Add(collection, points);
			}
		}

		public void SelectRewardTitle(int num, bool silent = false)
		{
			if (num == -1)
			{
				SelectedTitle = num;

				if (!silent)
					SendLocalizedMessage(1074010); // You elect to hide your Reward Title.
			}
			else if (num < RewardTitles.Count && num >= -1)
			{
				if (SelectedTitle != num)
				{
					SelectedTitle = num;

					switch (RewardTitles[num])
					{
						case int when !silent:
							SendLocalizedMessage(1074008, "#" + (int)RewardTitles[num]);
							// You change your Reward Title to "~1_TITLE~".
							break;
						case string when !silent:
							SendLocalizedMessage(1074008, (string)RewardTitles[num]); // You change your Reward Title to "~1_TITLE~".
							break;
					}
				}
				else if (!silent)
				{
					SendLocalizedMessage(1074009); // You decide to leave your title as it is.
				}
			}

			InvalidateProperties();
		}

		public bool AddRewardTitle(object title)
		{
			RewardTitles ??= new List<object>();

			if (title == null || RewardTitles.Contains(title)) return false;
			RewardTitles.Add(title);

			InvalidateProperties();
			return true;

		}

		public void ShowChangeTitle()
		{
			SendGump(new SelectTitleGump(this, SelectedTitle));
		}
		#endregion

		#region Titles
		private string m_FameKarmaTitle;
		private string m_PaperdollSkillTitle;
		private string m_SubtitleSkillTitle;
		private string m_CurrentChampTitle;
		private string m_OverheadTitle;
		private int m_CurrentVeteranTitle;

		public string FameKarmaTitle
		{
			get => m_FameKarmaTitle;
			set { m_FameKarmaTitle = value; InvalidateProperties(); }
		}

		public string PaperdollSkillTitle
		{
			get => m_PaperdollSkillTitle;
			set { m_PaperdollSkillTitle = value; InvalidateProperties(); }
		}

		public string SubtitleSkillTitle
		{
			get => m_SubtitleSkillTitle;
			set { m_SubtitleSkillTitle = value; InvalidateProperties(); }
		}

		public string CurrentChampTitle
		{
			get => m_CurrentChampTitle;
			set { m_CurrentChampTitle = value; InvalidateProperties(); }
		}

		public string OverheadTitle
		{
			get => m_OverheadTitle;
			set { m_OverheadTitle = value; InvalidateProperties(); }
		}

		public int CurrentVeteranTitle
		{
			get => m_CurrentVeteranTitle;
			set { m_CurrentVeteranTitle = value; InvalidateProperties(); }
		}

		public override bool DisplayAccessTitle
		{
			get
			{
				switch (AccessLevel)
				{
					case AccessLevel.VIP:
					case AccessLevel.Counselor:
					case AccessLevel.GameMaster:
					case AccessLevel.Seer:
						return true;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			string prefix = "";

			if (ShowFameTitle && Fame >= 10000)
			{
				prefix = Female ? "Lady" : "Lord";
			}

			string suffix = "";

			if (PropertyTitle && Title is {Length: > 0})
			{
				suffix = Title;
			}

			BaseGuild guild = Guild;
			//bool vvv = ViceVsVirtueSystem.IsVvV(this) && (ViceVsVirtueSystem.EnhancedRules || Map == Faction.Facet);

			/*if (m_OverheadTitle != null)
			{
				if (vvv)
				{
					suffix = "[VvV]";
				}
				else
				{
					int loc = Utility.ToInt32(m_OverheadTitle);

					if (loc > 0)
					{
						if (CityLoyaltySystem.ApplyCityTitle(this, list, prefix, loc))
							return;
					}
					else if (suffix.Length > 0)
					{
						suffix = string.Format("{0} {1}", suffix, m_OverheadTitle);
					}
					else
					{
						suffix = string.Format("{0}", m_OverheadTitle);
					}
				}
			}
			else*/ if (guild != null && DisplayGuildAbbr)
			{
				//if (vvv)
				//{
				//	suffix = $"[{Utility.FixHtml(guild.Abbreviation)}] [VvV]";
				//}
				//else
				if (suffix.Length > 0)
				{
					suffix = $"{suffix} [{Utility.FixHtml(guild.Abbreviation)}]";
				}
				else
				{
					suffix = $"[{Utility.FixHtml(guild.Abbreviation)}]";
				}
			}
			//else if (vvv)
			//{
			//	suffix = "[VvV]";
			//}

			suffix = ApplyNameSuffix(suffix);
			string name = Name;

			list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

			if (guild != null && (DisplayGuildTitle || guild.Type != GuildType.Regular))
			{
				string title = GuildTitle;

				if (title == null)
				{
					title = "";
				}
				else
				{
					title = title.Trim();
				}

				if (title.Length > 0)
				{
					list.Add("{0}, {1}", Utility.FixHtml(title), Utility.FixHtml(guild.Name));
				}
			}
		}

		public override void OnAfterNameChange(string oldName, string newName)
		{
			if (m_FameKarmaTitle != null)
			{
				FameKarmaTitle = FameKarmaTitle.Replace(oldName, newName);
			}
		}
		#endregion

		public override void OnKillsChange(int oldValue)
		{
			if (!Young || Kills <= oldValue) return;
			if (Account is Account acc)
				acc.RemoveYoungStatus(0);

		}

		public override void OnGenderChanged(bool oldFemale)
		{
			base.OnGenderChanged(oldFemale);
		}

		public override void OnGuildChange(Guilds.BaseGuild oldGuild)
		{
			base.OnGuildChange(oldGuild);
		}

		public override void OnGuildTitleChange(string oldTitle)
		{
			base.OnGuildTitleChange(oldTitle);
		}

		public override void OnKarmaChange(int oldValue)
		{
			base.OnKarmaChange(oldValue);
			//EpiphanyHelper.OnKarmaChange(this);
		}

		public override void OnFameChange(int oldValue)
		{
			base.OnFameChange(oldValue);
		}

		public override void OnSkillChange(SkillName skill, double oldBase)
		{
			if (!Young) return;
			if (SkillsTotal < 4500 || (Core.AOS || !(Skills[skill].Base >= 80.0))) return;
			Account acc = Account as Account;

			acc?.RemoveYoungStatus(1019036);
			// You have successfully obtained a respectable skill level, and have outgrown your status as a young player!

			TransformContext context = TransformationSpellHelper.GetContext(this);

			if (context != null)
			{
				TransformationSpellHelper.CheckCastSkill(this, context);
			}
		}

		public override void OnAccessLevelChanged(AccessLevel oldLevel)
		{
			IgnoreMobiles = !IsPlayer();
		}

		public override void OnRawStatChange(StatType stat, int oldValue)
		{
		}

		public override void OnDelete()
		{
			ReceivedHonorContext?.Cancel();
			SentHonorContext?.Cancel();
		}

		#region Fastwalk Prevention

		private const bool FastwalkPrevention = true; // Is fastwalk prevention enabled?
		private const int FastwalkThreshold = 400; // Fastwalk prevention will become active after 0.4 seconds

		private long _mNextMovementTime;
		private bool _mHasMoved;

		public virtual bool UsesFastwalkPrevention => IsPlayer();

		public override int ComputeMovementSpeed(Direction dir, bool checkTurning)
		{
			if (checkTurning && (dir & Direction.Mask) != (Direction & Direction.Mask))
				return Mobile.RunMount; // We are NOT actually moving (just a direction change)

			TransformContext context = TransformationSpellHelper.GetContext(this);

			if (context != null && context.Type == typeof(ReaperFormSpell))
				return Mobile.WalkFoot;

			bool running = ((dir & Direction.Running) != 0);

			bool onHorse = (Mount != null);

			AnimalFormContext animalContext = AnimalForm.GetContext(this);

			if (onHorse || animalContext is {SpeedBoost: true})
				return running ? RunMount : WalkMount;

			return (running ? RunFoot : WalkFoot);
		}

		public static bool MovementThrottle_Callback(NetState ns)
		{
			if (ns.Mobile is not PlayerMobile pm || !pm.UsesFastwalkPrevention)
				return true;

			if (!pm._mHasMoved)
			{
				// has not yet moved
				pm._mNextMovementTime = Core.TickCount;
				pm._mHasMoved = true;
				return true;
			}

			long ts = pm._mNextMovementTime - Core.TickCount;

			if (ts >= 0) return (ts < FastwalkThreshold);
			// been a while since we've last moved
			pm._mNextMovementTime = Core.TickCount;
			return true;

		}

		#endregion

		#region Enemy of One
		private Type _mEnemyOfOneType;

		public Type EnemyOfOneType
		{
			get => _mEnemyOfOneType;
			set
			{
				Type oldType = _mEnemyOfOneType;
				Type newType = value;

				if (oldType == newType)
					return;

				_mEnemyOfOneType = value;

				DeltaEnemies(oldType, newType);
			}
		}

		public bool WaitingForEnemy { get; set; }

		private void DeltaEnemies(Type oldType, Type newType)
		{
			foreach (Mobile m in GetMobilesInRange(Map.GlobalUpdateRange))
			{
				Type t = m.GetType();

				if (t != oldType && t != newType) continue;
				NetState ns = NetState;

				if (ns == null) continue;
				if (ns.StygianAbyss)
				{
					ns.Send(new MobileMoving(m, Notoriety.Compute(this, m)));
				}
				else
				{
					ns.Send(new MobileMovingOld(m, Notoriety.Compute(this, m)));
				}
			}
		}

		#endregion

		#region Hair and beard mods
		private int _mHairModId = -1, _mHairModHue;
		private int _mBeardModId = -1, _mBeardModHue;

		public void SetHairMods(int hairId, int beardId)
		{
			if (hairId == -1)
				InternalRestoreHair(true, ref _mHairModId, ref _mHairModHue);
			else if (hairId != -2)
				InternalChangeHair(true, hairId, ref _mHairModId, ref _mHairModHue);

			if (beardId == -1)
				InternalRestoreHair(false, ref _mBeardModId, ref _mBeardModHue);
			else if (beardId != -2)
				InternalChangeHair(false, beardId, ref _mBeardModId, ref _mBeardModHue);
		}

		private void CreateHair(bool hair, int id, int hue)
		{
			if (hair)
			{
				//TODO Verification?
				HairItemID = id;
				HairHue = hue;
			}
			else
			{
				FacialHairItemID = id;
				FacialHairHue = hue;
			}
		}

		private void InternalRestoreHair(bool hair, ref int id, ref int hue)
		{
			if (id == -1)
				return;

			if (hair)
				HairItemID = 0;
			else
				FacialHairItemID = 0;

			//if( id != 0 )
			CreateHair(hair, id, hue);

			id = -1;
			hue = 0;
		}

		private void InternalChangeHair(bool hair, int id, ref int storeID, ref int storeHue)
		{
			if (storeID == -1)
			{
				storeID = hair ? HairItemID : FacialHairItemID;
				storeHue = hair ? HairHue : FacialHairHue;
			}
			CreateHair(hair, id, 0);
		}

		#endregion

		#region Virtues

		public DateTime LastSacrificeGain { get; set; }
		public DateTime LastSacrificeLoss { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AvailableResurrects { get; set; }

		private DateTime _mNextJustAward;

		public DateTime LastJusticeLoss { get; set; }
		public List<Mobile> JusticeProtectors { get; set; }

		public DateTime LastCompassionLoss { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime NextCompassionDay { get; set; }
		[CommandProperty(AccessLevel.GameMaster)]
		public int CompassionGains { get; set; }

		public DateTime LastValorLoss { get; set; }

		public DateTime MHontime;

		public DateTime LastHonorLoss { get; set; }
		public DateTime LastHonorUse { get; set; }
		public bool HonorActive { get; set; }
		public HonorContext ReceivedHonorContext { get; set; }
		public HonorContext SentHonorContext { get; set; }
		#endregion

		#region Young system
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Young
		{
			get => GetFlag(PlayerFlag.Young);
			set { SetFlag(PlayerFlag.Young, value); InvalidateProperties(); }
		}

		public override string ApplyNameSuffix(string suffix)
		{
			if (Young)
			{
				suffix = suffix.Length == 0 ? "(Young)" : string.Concat(suffix, " (Young)");
			}

			#region Ethics
			if (EthicPlayer != null)
			{
				suffix = suffix.Length == 0 ? EthicPlayer.Ethic.Definition.Adjunct.String : string.Concat(suffix, " ", EthicPlayer.Ethic.Definition.Adjunct.String);
			}
			#endregion

			if (!Core.ML || Map != Faction.Facet) return base.ApplyNameSuffix(suffix);
			Faction faction = Faction.Find(this);

			if (faction == null) return base.ApplyNameSuffix(suffix);
			string adjunct = $"[{faction.Definition.Abbreviation}]";
			suffix = suffix.Length == 0 ? adjunct : string.Concat(suffix, " ", adjunct);

			return base.ApplyNameSuffix(suffix);
		}

		public override TimeSpan GetLogoutDelay()
		{
			if (Young || BedrollLogout || TestCenter.Enabled)
				return TimeSpan.Zero;

			return base.GetLogoutDelay();
		}

		private DateTime _mLastYoungMessage = DateTime.MinValue;

		public bool CheckYoungProtection(Mobile from)
		{
			if (!Young)
				return false;

			if (Region is BaseRegion baseRegion && !baseRegion.YoungProtected)
				return false;

			if (from is BaseCreature bc && bc.IgnoreYoungProtection)
				return false;

			if (Quest != null && Quest.IgnoreYoungProtection(from))
				return false;

			if (DateTime.UtcNow - _mLastYoungMessage <= TimeSpan.FromMinutes(1.0)) return true;
			_mLastYoungMessage = DateTime.UtcNow;
			SendLocalizedMessage(1019067); // A monster looks at you menacingly but does not attack.  You would be under attack now if not for your status as a new citizen of Britannia.

			return true;
		}

		private DateTime _mLastYoungHeal = DateTime.MinValue;

		public bool CheckYoungHealTime()
		{
			if (DateTime.UtcNow - _mLastYoungHeal <= TimeSpan.FromMinutes(5.0)) return false;
			_mLastYoungHeal = DateTime.UtcNow;
			return true;

		}

		private static readonly Point3D[] MTrammelDeathDestinations = {
				new( 1481, 1612, 20 ),
				new( 2708, 2153,  0 ),
				new( 2249, 1230,  0 ),
				new( 5197, 3994, 37 ),
				new( 1412, 3793,  0 ),
				new( 3688, 2232, 20 ),
				new( 2578,  604,  0 ),
				new( 4397, 1089,  0 ),
				new( 5741, 3218, -2 ),
				new( 2996, 3441, 15 ),
				new(  624, 2225,  0 ),
				new( 1916, 2814,  0 ),
				new( 2929,  854,  0 ),
				new(  545,  967,  0 ),
				new( 3665, 2587,  0 )
			};

		private static readonly Point3D[] MIlshenarDeathDestinations = {
				new( 1216,  468, -13 ),
				new(  723, 1367, -60 ),
				new(  745,  725, -28 ),
				new(  281, 1017,   0 ),
				new(  986, 1011, -32 ),
				new( 1175, 1287, -30 ),
				new( 1533, 1341,  -3 ),
				new(  529,  217, -44 ),
				new( 1722,  219,  96 )
			};

		private static readonly Point3D[] MMalasDeathDestinations = {
				new( 2079, 1376, -70 ),
				new(  944,  519, -71 )
			};

		private static readonly Point3D[] MTokunoDeathDestinations = {
				new( 1166,  801, 27 ),
				new(  782, 1228, 25 ),
				new(  268,  624, 15 )
			};

		public bool YoungDeathTeleport()
		{
			if (Region.IsPartOf(typeof(Jail))
				|| Region.IsPartOf("Samurai start location")
				|| Region.IsPartOf("Ninja start location")
				|| Region.IsPartOf("Ninja cave"))
				return false;

			Point3D loc;
			Map map;

			DungeonRegion dungeon = (DungeonRegion)Region.GetRegion(typeof(DungeonRegion));
			if (dungeon != null && dungeon.EntranceLocation != Point3D.Zero)
			{
				loc = dungeon.EntranceLocation;
				map = dungeon.EntranceMap;
			}
			else
			{
				loc = Location;
				map = Map;
			}

			Point3D[] list;

			if (map == Map.Trammel)
				list = MTrammelDeathDestinations;
			else if (map == Map.Ilshenar)
				list = MIlshenarDeathDestinations;
			else if (map == Map.Malas)
				list = MMalasDeathDestinations;
			else if (map == Map.Tokuno)
				list = MTokunoDeathDestinations;
			else
				return false;

			Point3D dest = Point3D.Zero;
			int sqDistance = int.MaxValue;

			for (var i = 0; i < list.Length; i++)
			{
				Point3D curDest = list[i];

				int width = loc.X - curDest.X;
				int height = loc.Y - curDest.Y;
				int curSqDistance = width * width + height * height;

				if (curSqDistance < sqDistance)
				{
					dest = curDest;
					sqDistance = curSqDistance;
				}
			}

			MoveToWorld(dest, map);
			return true;
		}

		private void SendYoungDeathNotice()
		{
			SendGump(new YoungDeathNotice());
		}

		#endregion

		#region Speech log

		public SpeechLog SpeechLog { get; private set; }

		public override void OnSpeech(SpeechEventArgs e)
		{
			if (!SpeechLog.Enabled || NetState == null) return;
			SpeechLog ??= new SpeechLog();

			SpeechLog.Add(e.Mobile, e.Speech);
		}

		#endregion

		#region Champion Titles
		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisplayChampionTitle
		{
			get => GetFlag(PlayerFlag.DisplayChampionTitle);
			set => SetFlag(PlayerFlag.DisplayChampionTitle, value);
		}

		private ChampionTitleInfo _mChampionTitles;

		[CommandProperty(AccessLevel.GameMaster)]
		public ChampionTitleInfo ChampionTitles { get => _mChampionTitles; set { } }

		private void ToggleChampionTitleDisplay()
		{
			if (!CheckAlive())
				return;

			SendLocalizedMessage(DisplayChampionTitle ? 1062419 : 1062418, 0x23);

			DisplayChampionTitle = !DisplayChampionTitle;
		}

		#endregion

		#region Recipes

		private Dictionary<int, bool> _mAcquiredRecipes;

		public virtual bool HasRecipe(Recipe r)
		{
			if (r == null)
				return false;

			return HasRecipe(r.Id);
		}

		public virtual bool HasRecipe(int recipeId)
		{
			if (_mAcquiredRecipes != null && _mAcquiredRecipes.ContainsKey(recipeId))
				return _mAcquiredRecipes[recipeId];

			return false;
		}

		public virtual void AcquireRecipe(Recipe r)
		{
			if (r != null)
				AcquireRecipe(r.Id);
		}

		public virtual void AcquireRecipe(int recipeId)
		{
			if (_mAcquiredRecipes == null)
				_mAcquiredRecipes = new Dictionary<int, bool>();

			_mAcquiredRecipes[recipeId] = true;
		}

		public virtual void ResetRecipes()
		{
			_mAcquiredRecipes = null;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int KnownRecipes => _mAcquiredRecipes?.Count ?? 0;

		#endregion

		#region Buff Icons

		public void ResendBuffs()
		{
			if (!BuffInfo.Enabled || _mBuffTable == null)
				return;

			NetState state = NetState;

			if (state is not {BuffIcon: true}) return;
			foreach (BuffInfo info in _mBuffTable.Values)
			{
				state.Send(new AddBuffPacket(this, info));
			}
		}

		private Dictionary<BuffIcon, BuffInfo> _mBuffTable;

		public void AddBuff(BuffInfo b)
		{
			if (!BuffInfo.Enabled || b == null)
				return;

			RemoveBuff(b);  //Check & subsequently remove the old one.

			_mBuffTable ??= new Dictionary<BuffIcon, BuffInfo>();

			_mBuffTable.Add(b.ID, b);

			NetState state = NetState;

			if (state != null && state.BuffIcon)
			{
				state.Send(new AddBuffPacket(this, b));
			}
		}

		public void RemoveBuff(BuffInfo b)
		{
			if (b == null)
				return;

			RemoveBuff(b.ID);
		}

		public void RemoveBuff(BuffIcon b)
		{
			if (_mBuffTable == null || !_mBuffTable.ContainsKey(b))
				return;

			BuffInfo info = _mBuffTable[b];

			if (info.Timer != null && info.Timer.Running)
				info.Timer.Stop();

			_mBuffTable.Remove(b);

			NetState state = NetState;

			if (state is {BuffIcon: true})
			{
				state.Send(new RemoveBuffPacket(this, b));
			}

			if (_mBuffTable.Count <= 0)
				_mBuffTable = null;
		}

		#endregion

		public void AutoStablePets()
		{
			if (!Core.SE || AllFollowers.Count <= 0) return;
			for (var i = _mAllFollowers.Count - 1; i >= 0; --i)
			{
				if (AllFollowers[i] is not BaseCreature pet || pet.ControlMaster == null)
					continue;

				if (pet.Summoned)
				{
					if (pet.Map != Map)
					{
						pet.PlaySound(pet.GetAngerSound());
						Timer.DelayCall(TimeSpan.Zero, new TimerCallback(pet.Delete));
					}
					continue;
				}

				switch (pet)
				{
					case IMount {Rider: { }}:
					case PackLlama or PackHorse or Beetle when pet.Backpack != null && pet.Backpack.Items.Count > 0:
					case BaseEscortable:
						continue;
				}

				pet.ControlTarget = null;
				pet.ControlOrder = OrderType.Stay;
				pet.Internalize();

				pet.SetControlMaster(null);
				pet.SummonMaster = null;

				pet.IsStabled = true;
				pet.StabledBy = this;

				pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy

				Stabled.Add(pet);
				AutoStabled.Add(pet);
			}
		}

		public void ClaimAutoStabledPets()
		{
			if (!Core.SE || AutoStabled.Count <= 0)
				return;

			if (!Alive)
			{
				SendLocalizedMessage(1076251); // Your pet was unable to join you while you are a ghost.  Please re-login once you have ressurected to claim your pets.
				return;
			}

			for (var i = AutoStabled.Count - 1; i >= 0; --i)
			{
				BaseCreature pet = AutoStabled[i] as BaseCreature;

				if (pet == null || pet.Deleted)
				{
					pet.IsStabled = false;
					pet.StabledBy = null;

					if (Stabled.Contains(pet))
						Stabled.Remove(pet);

					continue;
				}

				if (Followers + pet.ControlSlots <= FollowersMax)
				{
					pet.SetControlMaster(this);

					if (pet.Summoned)
						pet.SummonMaster = this;

					pet.ControlTarget = this;
					pet.ControlOrder = OrderType.Follow;

					pet.MoveToWorld(Location, Map);

					pet.IsStabled = false;
					pet.StabledBy = null;

					pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully Happy

					if (Stabled.Contains(pet))
						Stabled.Remove(pet);
				}
				else
				{
					SendLocalizedMessage(1049612, pet.Name); // ~1_NAME~ remained in the stables because you have too many followers.
				}
			}

			AutoStabled.Clear();
		}
	}
}
