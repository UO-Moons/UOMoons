using Server.Accounting;
using Server.Commands;
using Server.ContextMenus;
using Server.Guilds;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Menus;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Collections;
using System.Reflection;

namespace Server;

/// <summary>
/// Base class representing players, npcs, and creatures.
/// </summary>
[System.Runtime.InteropServices.ComVisible(true)]
public class Mobile : IHued, IComparable<Mobile>, ISerializable, ISpawnable, IDamageable
{
	#region Settings
	private static readonly int MConfigStatsCap = Settings.Configuration.Get<int>("Gameplay", "TotalStatCap");
	private static readonly int MConfigStrCap = Settings.Configuration.Get<int>("Gameplay", "StrCap");
	private static readonly int MConfigDexCap = Settings.Configuration.Get<int>("Gameplay", "DexCap");
	private static readonly int MConfigIntCap = Settings.Configuration.Get<int>("Gameplay", "IntCap");
	private static readonly int MConfigStrMaxCap = Settings.Configuration.Get<int>("Gameplay", "StrMaxCap");
	private static readonly int MConfigDexMaxCap = Settings.Configuration.Get<int>("Gameplay", "DexMaxCap");
	private static readonly int MConfigIntMaxCap = Settings.Configuration.Get<int>("Gameplay", "IntMaxCap");
	private static readonly int MConfigFollowersMax = Settings.Configuration.Get<int>("Gameplay", "FollowersMax");
	public static readonly int MurderKills = Settings.Configuration.Get<int>("Gameplay", "MurderKills");
	public static readonly int MaxStatValue = Math.Min(Settings.Configuration.Get<int>("Mobiles", "MaxStatValue"), int.MaxValue);
	private static readonly int MinResist = Settings.Configuration.Get<int>("Gameplay", "MinPlayerResistance");
	private static readonly int MaxResist = Settings.Configuration.Get<int>("Gameplay", "MaxPlayerResistance");
	private static readonly TimeSpan WarmodeSpamCatch = TimeSpan.FromSeconds(Core.SE ? 1.0 : 0.5);
	private static readonly TimeSpan WarmodeSpamDelay = TimeSpan.FromSeconds(Core.SE ? 4.0 : 2.0);
	private const int WarmodeCatchCount = 4; // Allow four warmode changes in 0.5 seconds, any more will be delay for two seconds
	//Duration of effect per second
	public const int EffectDurationPerSecond = 20;
	public virtual bool IsHallucinated => false;
	public virtual bool ViewOpl => ObjectPropertyList.Enabled;
	public virtual double RacialSkillBonus => 0;
	public virtual int BasePhysicalResistance => 0;
	public virtual int BaseFireResistance => 0;
	public virtual int BaseColdResistance => 0;
	public virtual int BasePoisonResistance => 0;
	public virtual int BaseEnergyResistance => 0;
	public virtual bool IsDeadBondedPet => false;
	#endregion

	#region Get/Set
	public static bool DragEffects { get; set; } = true;
	[CommandProperty(AccessLevel.Administrator)]
	public bool AutoPageNotify { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public Race Race
	{
		get { return _race ??= Race.DefaultRace; }
		set
		{
			Race oldRace = Race;

			_race = value ?? Race.DefaultRace;

			Body = _race.Body(this);
			UpdateResistances();

			Delta(MobileDelta.Race);

			OnRaceChange(oldRace);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CharacterOut { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool PublicHouseContent { get; set; }

	public DfAlgorithm Dfa { get; set; }

	public List<string> SlayerVulnerabilities => MSlayerVulnerabilities;

	[CommandProperty(AccessLevel.Decorator)]
	public bool SpecialSlayerMechanics => MSpecialSlayerMechanics;

	public int[] Resistances { get; private set; }

	[CommandProperty(AccessLevel.Counselor)]
	public virtual int PhysicalResistance => GetResistance(ResistanceType.Physical);

	[CommandProperty(AccessLevel.Counselor)]
	public virtual int FireResistance => GetResistance(ResistanceType.Fire);

	[CommandProperty(AccessLevel.Counselor)]
	public virtual int ColdResistance => GetResistance(ResistanceType.Cold);

	[CommandProperty(AccessLevel.Counselor)]
	public virtual int PoisonResistance => GetResistance(ResistanceType.Poison);

	[CommandProperty(AccessLevel.Counselor)]
	public virtual int EnergyResistance => GetResistance(ResistanceType.Energy);

	public List<ResistanceMod> ResistanceMods { get; set; }

	public static int MinPlayerResistance { get; set; } = MinResist;
	public static int MaxPlayerResistance { get; set; } = MaxResist;

	public List<Mobile> Stabled { get; private set; }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public VirtueInfo Virtues { get => _virtues; set => _virtues = value; }

	public object Party { get; set; }
	public List<SkillMod> SkillMods { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int VirtualArmorMod
	{
		get => _virtualArmorMod;
		set
		{
			if (_virtualArmorMod != value)
			{
				_virtualArmorMod = value;

				Delta(MobileDelta.Armor);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int MeleeDamageAbsorb { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MagicDamageAbsorb { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SkillsTotal => _skills?.Total ?? 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public int SkillsCap
	{
		get => _skills?.Cap ?? 0;
		set
		{
			if (_skills != null)
				_skills.Cap = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BaseSoundId { get; set; }

	public long NextCombatTime { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int NameHue { get; set; } = -1;

	[CommandProperty(AccessLevel.GameMaster)]
	public int Hunger
	{
		get => _hunger;
		set
		{
			var oldValue = _hunger;

			if (oldValue != value)
			{
				_hunger = value;

				EventSink.InvokeHungerChanged(this, oldValue);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Thirst { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Bac { get; set; }

	public virtual int DefaultBloodHue => 0;

	public virtual bool HasBlood => Alive && BloodHue >= 0 && !Body.IsGhost && !Body.IsEquipment;

	private int _mBloodHue = -1;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int BloodHue
	{
		get
		{
			if (_mBloodHue < 0)
			{
				return DefaultBloodHue;
			}

			return _mBloodHue;
		}
		set => _mBloodHue = value;
	}

	/// <summary>
	/// Gets or sets the number of steps this player may take when hidden before being revealed.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int AllowedStealthSteps { get; set; }

	public long LastMoveTime { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool Paralyzed
	{
		get => _paralyzed;
		set
		{
			if (_paralyzed != value)
			{
				_paralyzed = value;
				Delta(MobileDelta.Flags);

				SendLocalizedMessage(_paralyzed ? 502381 : 502382);

				if (_paraTimer != null)
				{
					_paraTimer.Stop();
					_paraTimer = null;
				}
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool DisarmReady { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool StunReady { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool Frozen
	{
		get => _frozen;
		set
		{
			if (_frozen != value)
			{
				_frozen = value;
				Delta(MobileDelta.Flags);

				if (_frozenTimer != null)
				{
					_frozenTimer.Stop();
					_frozenTimer = null;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawStr" /> property.
	/// </summary>
	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public StatLockType StrLock
	{
		get => _mStrLock;
		set
		{
			if (_mStrLock != value)
			{
				_mStrLock = value;

				_netState?.Send(new StatLockInfo(this));
			}
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawDex" /> property.
	/// </summary>
	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public StatLockType DexLock
	{
		get => _mDexLock;
		set
		{
			if (_mDexLock != value)
			{
				_mDexLock = value;

				_netState?.Send(new StatLockInfo(this));
			}
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawInt" /> property.
	/// </summary>
	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public StatLockType IntLock
	{
		get => _mIntLock;
		set
		{
			if (_mIntLock != value)
			{
				_mIntLock = value;

				_netState?.Send(new StatLockInfo(this));
			}
		}
	}

	public long NextActionTime { get; set; }
	public long NextActionMessage { get; set; }
	public static int ActionMessageDelay { get; set; } = 125;
	public static bool GlobalRegenThroughPoison { get; set; } = true;
	public virtual bool RegenThroughPoison => GlobalRegenThroughPoison;
	public virtual bool CanRegenHits => Alive && (RegenThroughPoison || !Poisoned);
	public virtual bool CanRegenStam => Alive;
	public virtual bool CanRegenMana => Alive;
	public long NextSkillTime { get; set; }
	public List<AggressorInfo> Aggressors { get; private set; }
	public List<AggressorInfo> Aggressed { get; private set; }
	private int _mChangingCombatant;
	public bool ChangingCombatant => _mChangingCombatant > 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool GuardImmune { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int TotalGold => GetTotal(TotalType.Gold);

	[CommandProperty(AccessLevel.GameMaster)]
	public int TotalItems => GetTotal(TotalType.Items);

	[CommandProperty(AccessLevel.GameMaster)]
	public int TotalWeight => GetTotal(TotalType.Weight);

	[CommandProperty(AccessLevel.GameMaster)]
	public int TithingPoints
	{
		get => _tithingPoints;
		set
		{
			if (_tithingPoints != value)
			{
				_tithingPoints = value;

				Delta(MobileDelta.TithingPoints);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Followers
	{
		get => _followers;
		set
		{
			if (_followers != value)
			{
				_followers = value;

				Delta(MobileDelta.Followers);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int FollowersMax
	{
		get => _followersMax;
		set
		{
			if (_followersMax != value)
			{
				_followersMax = value;

				Delta(MobileDelta.Followers);
			}
		}
	}

	public bool TargetLocked { get; set; }
	public bool Pushing { get; set; }
	public static int WalkFoot { get; set; } = 400;
	public static int RunFoot { get; set; } = 200;
	public static int WalkMount { get; set; } = 200;
	public static int RunMount { get; set; } = 100;
	public static AccessLevel FwdAccessOverride { get; set; } = AccessLevel.Counselor;
	public static bool FwdEnabled { get; set; } = true;
	public static bool FwdUotdOverride { get; set; }
	public static int FwdMaxSteps { get; set; } = 4;

	[CommandProperty(AccessLevel.GameMaster, AccessLevel.Owner)]
	public IAccount Account { get; set; }

	public bool Deleted { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int VirtualArmor
	{
		get => _virtualArmor;
		set
		{
			if (_virtualArmor != value)
			{
				_virtualArmor = value;

				Delta(MobileDelta.Armor);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual double ArmorRating => 0.0;

	public virtual bool RetainPackLocsOnDeath => Core.AOS;
	public static char[] GhostChars { get; set; } = new char[] { 'o', 'O' };
	public static bool NoSpeechLos { get; set; }
	public static TimeSpan AutoManifestTimeout { get; set; } = TimeSpan.FromSeconds(5.0);

	[CommandProperty(AccessLevel.GameMaster)]
	public Container Corpse { get; set; }

	public static bool InsuranceEnabled { get; set; }
	public static int ActionDelay { get; set; } = 750;
	public static VisibleDamageType VisibleDamageType { get; set; }
	public List<DamageEntry> DamageEntries { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile LastKiller { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime LastKilled { get; set; }

	public static bool DefaultShowVisibleDamage { get; set; }
	public static bool DefaultCanSeeVisibleDamage { get; set; }
	public virtual bool ShowVisibleDamage => DefaultShowVisibleDamage;
	public virtual bool CanSeeVisibleDamage => DefaultCanSeeVisibleDamage;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Squelched { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime CreationTime { get; private set; }

	int ISerializable.TypeReference => MTypeRef;
	int ISerializable.SerialIdentity => _serial;

	[CommandProperty(AccessLevel.GameMaster)]
	public int LightLevel
	{
		get => _lightLevel;
		set
		{
			if (_lightLevel != value)
			{
				_lightLevel = value;

				CheckLightLevels(false);

				/*if ( m_NetState != null )
					m_NetState.Send( new PersonalLightLevel( this ) );*/
			}
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public string Profile { get; set; }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public bool ProfileLocked { get; set; }

	[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
	public bool Player
	{
		get => _player;
		set
		{
			_player = value;
			InvalidateProperties();

			if (!_player && _dex <= 100 && _combatTimer != null)
				_combatTimer.Priority = TimerPriority.FiftyMs;
			else if (_combatTimer != null)
				_combatTimer.Priority = TimerPriority.EveryTick;

			CheckStatTimers();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string Title
	{
		get => _title;
		set
		{
			_title = value;
			InvalidateProperties();
		}
	}

	public List<Item> Items { get; private set; }
	public virtual int MaxWeight => int.MaxValue;
	public static IWeapon DefaultWeapon { get; set; }

	[CommandProperty(AccessLevel.Counselor)]
	public Skills Skills
	{
		get => _skills;
		set => _skills = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IgnoreMobiles
	{
		get => _mIgnoreMobiles;
		set
		{
			if (_mIgnoreMobiles != value)
			{
				_mIgnoreMobiles = value;
				Delta(MobileDelta.Flags);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsStealthing { get; set; }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
	public AccessLevel AccessLevel
	{
		get => _accessLevel;
		set
		{
			AccessLevel oldValue = _accessLevel;

			if (oldValue != value)
			{
				_accessLevel = value;
				Delta(MobileDelta.Noto);
				InvalidateProperties();

				SendMessage("Your access level has been changed. You are now {0}.", GetAccessLevelName(value));

				ClearScreen();
				SendEverything();

				OnAccessLevelChanged(oldValue);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Blessed
	{
		get => _blessed;
		set
		{
			if (_blessed != value)
			{
				_blessed = value;
				Delta(MobileDelta.HealthbarYellow);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Fame
	{
		get => _fame;
		set
		{
			int oldValue = _fame;

			if (oldValue != value)
			{
				_fame = value;

				if (ShowFameTitle && (_player || _mBody.IsHuman) && oldValue >= 10000 != value >= 10000)
					InvalidateProperties();

				OnFameChange(oldValue);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Karma
	{
		get => _karma;
		set
		{
			int old = _karma;

			if (old != value)
			{
				_karma = value;
				OnKarmaChange(old);
			}
		}
	}
	#endregion

	#region Var declarations
	private Map _mMap;
	private Point3D _mLocation;
	private Direction _mDirection;
	private Body _mBody;
	private int _mHue;
	private Poison _mPoison;
	private BaseGuild _mGuild;
	private string _mGuildTitle;
	private bool _mCriminal;
	private string _mName;
	private int _deaths, _kills, _shortTermMurders;
	private string _language;
	private NetState _netState;
	private bool _female, _warmode, _hidden, _blessed, _flying;
	private int _statCap;
	private int _str, _dex, _int;
	private int _hits, _stam, _mana;
	private int _fame, _karma;
	private AccessLevel _accessLevel;
	private Skills _skills;
	private bool _player;
	private string _title;
	private int _lightLevel;
	private int _totalGold, _totalItems, _totalWeight;
	private ISpell _spell;
	private Target _target;
	private Prompt _prompt;
	private ContextMenu _contextMenu;
	private IDamageable _combatant;
	private bool _canHearGhosts;
	private int _tithingPoints;
	private bool _displayGuildTitle;
	private bool _displayGuildAbbr;
	private Timer _expireCombatant;
	private Timer _expireCriminal;
	private Timer _expireAggrTimer;
	private Timer _logoutTimer;
	private Timer _combatTimer;
	private Timer _manaTimer, _hitsTimer, _stamTimer;
	private bool _paralyzed;
	private ParalyzedTimer _paraTimer;
	private bool _frozen;
	private FrozenTimer _frozenTimer;
	private int _hunger;
	private Region _region;
	private int _virtualArmor;
	private int _followers, _followersMax;
	private List<object> _actions; // prefer List<object> over ArrayList for more specific profiling information
	private Queue<MovementRecord> _moveRecords;
	private int _warmodeChanges;
	private DateTime _nextWarmodeChange;
	private WarmodeTimer _warmodeTimer;
	private int _virtualArmorMod;
	private VirtueInfo _virtues;
	private Body _bodyMod;
	private Race _race;
	private readonly TemporalCache<object, object> _mLosRecent;
	protected List<string> MSlayerVulnerabilities = new();
	protected bool MSpecialSlayerMechanics;
	private StatLockType _mStrLock, _mDexLock, _mIntLock;
	private Item _mHolding;
	private DateTime _lockCombatTime; //time until we can change combatant
	private bool _mIgnoreMobiles;
	private long _endQueue;
	private static readonly List<IEntity> m_MoveList = new();
	private static readonly List<Mobile> m_MoveClientList = new();
	private Timer _autoManifestTimer;
	private static readonly object m_GhostMutateContext = new();
	private static readonly List<Mobile> m_Hears = new();
	private static readonly List<IEntity> m_OnSpeech = new();
	#endregion

	#region CompareTo(...)
	public int CompareTo(IEntity other)
	{
		if (other == null)
			return -1;

		return _serial.CompareTo(other.Serial);
	}

	public int CompareTo(Mobile other)
	{
		return CompareTo((IEntity)other);
	}

	public int CompareTo(object other)
	{
		if (other is null or IEntity)
			return CompareTo((IEntity)other);

		throw new ArgumentException("Bad IEntity in Mobiles");
	}
	#endregion

	#region Handlers

	public static FatigueHandler FatigueHandler { get; set; }

	public static AllowBeneficialHandler AllowBeneficialHandler { get; set; }

	public static AllowHarmfulHandler AllowHarmfulHandler { get; set; }

	public static SkillCheckTargetHandler SkillCheckTargetHandler { get; set; }

	public static SkillCheckLocationHandler SkillCheckLocationHandler { get; set; }

	public static SkillCheckDirectTargetHandler SkillCheckDirectTargetHandler { get; set; }

	public static SkillCheckDirectLocationHandler SkillCheckDirectLocationHandler { get; set; }

	public static AOSStatusHandler AosStatusHandler { get; set; }

	#endregion

	#region Regeneration
	public static RegenRateHandler HitsRegenRateHandler { get; set; }
	public static RegenRateHandler StamRegenRateHandler { get; set; }
	public static RegenRateHandler ManaRegenRateHandler { get; set; }

	public static TimeSpan DefaultHitsRate { get; set; }
	public static TimeSpan DefaultStamRate { get; set; }
	public static TimeSpan DefaultManaRate { get; set; }

	public static TimeSpan GetHitsRegenRate(Mobile m)
	{
		return HitsRegenRateHandler?.Invoke(m) ?? DefaultHitsRate;
	}

	public static TimeSpan GetStamRegenRate(Mobile m)
	{
		return StamRegenRateHandler?.Invoke(m) ?? DefaultStamRate;
	}

	public static TimeSpan GetManaRegenRate(Mobile m)
	{
		return ManaRegenRateHandler?.Invoke(m) ?? DefaultManaRate;
	}

	public virtual int HitsRegenBonus { get; set; }
	public virtual int ManaRegenBonus { get; set; }
	public virtual int StamRegenBonus { get; set; }

	public virtual int HitsRegenBaseValue => 1;
	public virtual int ManaRegenBaseValue => 1;
	public virtual int StamRegenBaseValue => 1;

	public virtual int GetHitsRegenValue()
	{
		return HitsRegenBaseValue + HitsRegenBonus;
	}

	public virtual int GetManaRegenValue()
	{
		return ManaRegenBaseValue + ManaRegenBonus;
	}

	public virtual int GetStamRegenValue()
	{
		return StamRegenBaseValue + StamRegenBonus;
	}

	#endregion

	private class MovementRecord
	{
		private long _mEnd;

		private static readonly Queue<MovementRecord> MInstancePool = new();

		public static MovementRecord NewInstance(long end)
		{
			MovementRecord r;

			if (MInstancePool.Count > 0)
			{
				r = MInstancePool.Dequeue();

				r._mEnd = end;
			}
			else
			{
				r = new MovementRecord(end);
			}

			return r;
		}

		private MovementRecord(long end)
		{
			_mEnd = end;
		}

		public bool Expired()
		{
			bool v = Core.TickCount - _mEnd >= 0;

			if (v)
				MInstancePool.Enqueue(this);

			return v;
		}
	}

	protected virtual void OnRaceChange(Race oldRace)
	{
	}

	public virtual double GetRacialSkillBonus(SkillName skill)
	{
		return RacialSkillBonus;
	}

	public virtual void MutateSkill(SkillName skill, ref double value)
	{ }

	public virtual short GetBody(Mobile toSend)
	{
		return (short)toSend.Body;
	}

	public virtual int GetHue(Mobile toSend)
	{
		return toSend.Hue;
	}

	public virtual void ComputeLightLevels(out int global, out int personal)
	{
		ComputeBaseLightLevels(out global, out personal);

		_region?.AlterLightLevel(this, ref global, ref personal);
	}

	public virtual void ComputeBaseLightLevels(out int global, out int personal)
	{
		global = 0;
		personal = _lightLevel;
	}

	public virtual void CheckLightLevels(bool forceResend)
	{
	}

	public virtual void UpdateResistances()
	{
		Resistances ??= new[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};

		bool delta = false;

		for (var i = 0; i < Resistances.Length; ++i)
		{
			if (Resistances[i] == int.MinValue) continue;
			Resistances[i] = int.MinValue;
			delta = true;
		}

		if (delta)
			Delta(MobileDelta.Resistances);
	}

	public virtual int GetResistance(ResistanceType type)
	{
		Resistances ??= new[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};

		var v = (int)type;

		if (v < 0 || v >= Resistances.Length)
			return 0;

		var res = Resistances[v];

		if (res != int.MinValue) return res;
		ComputeResistances();
		res = Resistances[v];

		return res;
	}

	public virtual void AddResistanceMod(ResistanceMod toAdd)
	{
		ResistanceMods ??= new List<ResistanceMod>();

		ResistanceMods.Add(toAdd);
		UpdateResistances();
	}

	public virtual void RemoveResistanceMod(ResistanceMod toRemove)
	{
		if (ResistanceMods != null)
		{
			_ = ResistanceMods.Remove(toRemove);

			if (ResistanceMods.Count == 0)
				ResistanceMods = null;
		}

		UpdateResistances();
	}

	public virtual void ComputeResistances()
	{
		Resistances ??= new[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};

		for (var i = 0; i < Resistances.Length; ++i)
			Resistances[i] = 0;

		Resistances[0] += BasePhysicalResistance;
		Resistances[1] += BaseFireResistance;
		Resistances[2] += BaseColdResistance;
		Resistances[3] += BasePoisonResistance;
		Resistances[4] += BaseEnergyResistance;

		for (var i = 0; ResistanceMods != null && i < ResistanceMods.Count; ++i)
		{
			ResistanceMod mod = ResistanceMods[i];
			var v = (int)mod.Type;

			if (v >= 0 && v < Resistances.Length)
				Resistances[v] += mod.Offset;
		}

		for (var i = 0; i < Items.Count; ++i)
		{
			Item item = Items[i];

			if (item.CheckPropertyConfliction(this))
				continue;

			Resistances[0] += item.PhysicalResistance;
			Resistances[1] += item.FireResistance;
			Resistances[2] += item.ColdResistance;
			Resistances[3] += item.PoisonResistance;
			Resistances[4] += item.EnergyResistance;
		}

		for (var i = 0; i < Resistances.Length; ++i)
		{
			var min = GetMinResistance((ResistanceType)i);
			var max = GetMaxResistance((ResistanceType)i);

			if (max < min)
				max = min;

			if (Resistances[i] > max)
				Resistances[i] = max;
			else if (Resistances[i] < min)
				Resistances[i] = min;
		}
	}

	public virtual int GetMinResistance(ResistanceType type)
	{
		if (_player)
		{
			return MinPlayerResistance;
		}

		return -100;
	}

	public virtual int GetMaxResistance(ResistanceType type)
	{
		return _player ? MaxPlayerResistance : 100;
	}

	public int GetAosStatus(int index)
	{
		return AosStatusHandler?.Invoke(this, index) ?? 0;
	}

	public virtual void SendPropertiesTo(Mobile from)
	{
		from.Send(PropertyList);
	}

	public virtual void OnAosSingleClick(Mobile from)
	{
		// XXX LOS BEGIN
		//Console.Write("Checking from: {0} --> {1} ", from.Name, this.Name );
		if (from._mLosRecent.ContainsKey(this))
		{
			//Console.WriteLine("... TOO SOON.");
			return;
		}
		//else Console.WriteLine("... SENDING PROPS.");
		// XXX LOS END
		ObjectPropertyList opl = PropertyList;

		if (opl.Header <= 0) return;
		int hue;

		if (NameHue != -1)
			hue = NameHue;
		else if (IsStaff())
			hue = 11;
		else
			hue = Notoriety.GetHue(Notoriety.Compute(from, this));

		from.Send(new MessageLocalized(_serial, Body, MessageType.Label, hue, 3, opl.Header, Name, opl.HeaderArgs));
	}

	public virtual string ApplyNameSuffix(string suffix)
	{
		return suffix;
	}

	public virtual void AddNameProperties(ObjectPropertyList list)
	{
		string name = Name ?? string.Empty;

		string prefix = "";

		if (ShowFameTitle && (_player || _mBody.IsHuman) && _fame >= 10000)
			prefix = _female ? "Lady" : "Lord";

		string suffix = "";

		if (PropertyTitle && Title is {Length: > 0})
			suffix = Title;

		suffix = ApplyNameSuffix(suffix);

		list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~
	}

	public virtual bool NewGuildDisplay => false;

	public virtual void GetProperties(ObjectPropertyList list)
	{
		AddNameProperties(list);

		Spawner?.GetSpawnProperties(this, list);
	}

	public virtual void GetChildProperties(ObjectPropertyList list, Item item)
	{
	}

	public virtual void GetChildNameProperties(ObjectPropertyList list, Item item)
	{
	}

	private void UpdateAggrExpire()
	{
		if (Deleted || (Aggressors.Count == 0 && Aggressed.Count == 0))
		{
			StopAggrExpire();
		}
		else if (_expireAggrTimer == null)
		{
			_expireAggrTimer = new ExpireAggressorsTimer(this);
			_expireAggrTimer.Start();
		}
	}

	private void StopAggrExpire()
	{
		_expireAggrTimer?.Stop();

		_expireAggrTimer = null;
	}

	private void CheckAggrExpire()
	{
		for (var i = Aggressors.Count - 1; i >= 0; --i)
		{
			if (i >= Aggressors.Count)
				continue;

			AggressorInfo info = Aggressors[i];

			if (info.Expired)
			{
				Mobile attacker = info.Attacker;
				attacker.RemoveAggressed(this);

				Aggressors.RemoveAt(i);
				info.Free();

				if (_netState != null && CanSee(attacker) && InUpdateRange(_mLocation, attacker._mLocation))
				{
					MobileIncoming.Send(_netState, attacker);
				}
			}
		}

		for (var i = Aggressed.Count - 1; i >= 0; --i)
		{
			if (i >= Aggressed.Count)
				continue;

			AggressorInfo info = Aggressed[i];

			if (info.Expired)
			{
				Mobile defender = info.Defender;
				defender.RemoveAggressor(this);

				Aggressed.RemoveAt(i);
				info.Free();

				if (_netState != null && CanSee(defender) && InUpdateRange(_mLocation, defender._mLocation))
				{
					MobileIncoming.Send(_netState, defender);
				}
			}
		}

		UpdateAggrExpire();
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="skill" /> changes in some way.
	/// </summary>
	public virtual void OnSkillInvalidated(Skill skill)
	{
	}

	public virtual void UpdateSkillMods()
	{
		ValidateSkillMods();

		for (var i = 0; i < SkillMods.Count; ++i)
		{
			SkillMod mod = SkillMods[i];

			Skill sk = _skills[mod.Skill];

			sk?.Update();
		}
	}

	public virtual void ValidateSkillMods()
	{
		for (var i = 0; i < SkillMods.Count;)
		{
			SkillMod mod = SkillMods[i];

			if (mod.CheckCondition())
				++i;
			else
				InternalRemoveSkillMod(mod);
		}
	}

	public virtual void AddSkillMod(SkillMod mod)
	{
		if (mod == null)
			return;

		ValidateSkillMods();

		if (!SkillMods.Contains(mod))
		{
			SkillMods.Add(mod);
			mod.Owner = this;

			Skill sk = _skills[mod.Skill];

			sk?.Update();
		}
	}

	public virtual void RemoveSkillMod(SkillMod mod)
	{
		if (mod == null)
			return;

		ValidateSkillMods();

		InternalRemoveSkillMod(mod);
	}

	private void InternalRemoveSkillMod(SkillMod mod)
	{
		if (SkillMods.Contains(mod))
		{
			_ = SkillMods.Remove(mod);
			mod.Owner = null;

			Skill sk = _skills[mod.Skill];

			sk?.Update();
		}
	}

	/// <summary>
	/// Overridable. Virtual event invoked when a client, <paramref name="from" />, invokes a 'help request' for the Mobile. Seemingly no longer functional in newer clients.
	/// </summary>
	public virtual void OnHelpRequest(Mobile from)
	{
	}

	public void DelayChangeWarmode(bool value)
	{
		if (_warmodeTimer != null)
		{
			_warmodeTimer.Value = value;
			return;
		}

		if (_warmode == value)
			return;

		DateTime now = DateTime.UtcNow, next = _nextWarmodeChange;

		if (now > next || _warmodeChanges == 0)
		{
			_warmodeChanges = 1;
			_nextWarmodeChange = now + WarmodeSpamCatch;
		}
		else if (_warmodeChanges == WarmodeCatchCount)
		{
			_warmodeTimer = new WarmodeTimer(this, value);
			_warmodeTimer.Start();

			return;
		}
		else
		{
			++_warmodeChanges;
		}

		Warmode = value;
	}

	public bool InLOS(Mobile target)
	{
		if (Deleted || _mMap == null)
			return false;

		if (target == this || IsStaff())
			return true;

		return _mMap.LineOfSight(this, target);
	}

	public bool InLOS(object target)
	{
		if (Deleted || _mMap == null)
			return false;
		if (target == this || IsStaff())
			return true;
		if (target is Item item && item.RootParent == this)
			return true;

		return _mMap.LineOfSight(this, target);
	}

	public bool InLOS(Point3D target)
	{
		if (Deleted || _mMap == null)
			return false;

		return IsStaff() || _mMap.LineOfSight(this, target);
	}

	public bool BeginAction(object toLock)
	{
		if (_actions == null)
		{
			_actions = new List<object>
			{
				toLock
			};

			return true;
		}

		if (!_actions.Contains(toLock))
		{
			_actions.Add(toLock);

			return true;
		}

		return false;
	}

	public bool CanBeginAction(object toLock)
	{
		return _actions == null || !_actions.Contains(toLock);
	}

	public void EndAction(object toLock)
	{
		if (_actions != null)
		{
			_ = _actions.Remove(toLock);

			if (_actions.Count == 0)
			{
				_actions = null;
			}
		}
	}

	public virtual TimeSpan GetLogoutDelay()
	{
		return Region.GetLogoutDelay(this);
	}

	public Item Holding
	{
		get => _mHolding;
		set
		{
			if (_mHolding != value)
			{
				if (_mHolding != null)
				{
					UpdateTotal(_mHolding, TotalType.Weight, -(_mHolding.TotalWeight + _mHolding.PileWeight));

					if (_mHolding.HeldBy == this)
						_mHolding.HeldBy = null;
				}

				if (value != null && _mHolding != null)
					DropHolding();

				_mHolding = value;

				if (_mHolding != null)
				{
					UpdateTotal(_mHolding, TotalType.Weight, _mHolding.TotalWeight + _mHolding.PileWeight);

					_mHolding.HeldBy ??= this;
				}
			}
		}
	}

	public virtual void Paralyze(TimeSpan duration)
	{
		if (!_paralyzed)
		{
			Paralyzed = true;

			_paraTimer = new ParalyzedTimer(this, duration);
			_paraTimer.Start();
		}
	}

	public virtual void Freeze(TimeSpan duration)
	{
		if (!_frozen)
		{
			Frozen = true;

			_frozenTimer = new FrozenTimer(this, duration);
			_frozenTimer.Start();
		}
	}

	public override string ToString() => $"0x{_serial.Value:X} \"{Name}\"";

	public virtual void SendSkillMessage()
	{
		if (NextActionMessage - Core.TickCount >= 0)
			return;

		NextActionMessage = Core.TickCount + ActionMessageDelay;

		SendLocalizedMessage(500118); // You must wait a few moments to use another skill.
	}

	public virtual void SendActionMessage()
	{
		if (NextActionMessage - Core.TickCount >= 0)
			return;

		NextActionMessage = Core.TickCount + ActionMessageDelay;

		SendLocalizedMessage(500119); // You must wait to perform another action.
	}

	public virtual void ClearHands()
	{
		ClearHand(FindItemOnLayer(Layer.OneHanded));
		ClearHand(FindItemOnLayer(Layer.TwoHanded));
	}

	public virtual void ClearHand(Item item)
	{
		if (item is {Movable: true} && !item.AllowEquipedCast(this))
		{
			Container pack = Backpack;

			if (pack == null)
				_ = AddToBackpack(item);
			else
				pack.DropItem(item);
		}
	}

	#region Timers
	private class AutoManifestTimer : Timer
	{
		private readonly Mobile _mobile;

		public AutoManifestTimer(Mobile m, TimeSpan delay)
			: base(delay)
		{
			_mobile = m;
		}

		protected override void OnTick()
		{
			if (!_mobile.Alive)
				_mobile.Warmode = false;
		}
	}

	private class ManaTimer : Timer
	{
		private readonly Mobile _mOwner;

		public ManaTimer(Mobile m)
			: base(GetManaRegenRate(m), GetManaRegenRate(m))
		{
			Priority = TimerPriority.FiftyMs;
			_mOwner = m;
		}

		protected override void OnTick()
		{
			if (_mOwner.CanRegenMana)
				_mOwner.Mana += _mOwner.GetManaRegenValue();

			Delay = Interval = Mobile.GetManaRegenRate(_mOwner);
		}
	}

	private class HitsTimer : Timer
	{
		private readonly Mobile _mOwner;

		public HitsTimer(Mobile m)
			: base(GetHitsRegenRate(m), GetHitsRegenRate(m))
		{
			Priority = TimerPriority.FiftyMs;
			_mOwner = m;
		}

		protected override void OnTick()
		{
			if (_mOwner.CanRegenHits)// m_Owner.Alive && !m_Owner.Poisoned )
				_mOwner.Hits += _mOwner.GetHitsRegenValue();

			Delay = Interval = GetHitsRegenRate(_mOwner);
		}
	}

	private class StamTimer : Timer
	{
		private readonly Mobile _mOwner;

		public StamTimer(Mobile m)
			: base(GetStamRegenRate(m), GetStamRegenRate(m))
		{
			Priority = TimerPriority.FiftyMs;
			_mOwner = m;
		}

		protected override void OnTick()
		{
			if (_mOwner.CanRegenStam)
				_mOwner.Stam += _mOwner.GetManaRegenValue();

			Delay = Interval = GetStamRegenRate(_mOwner);
		}
	}

	private class LogoutTimer : Timer
	{
		private readonly Mobile _mMobile;

		public LogoutTimer(Mobile m)
			: base(TimeSpan.FromDays(1.0))
		{
			Priority = TimerPriority.OneSecond;
			_mMobile = m;
		}

		protected override void OnTick()
		{
			if (_mMobile._mMap != Map.Internal)
			{
				EventSink.InvokeLogout(_mMobile);

				_mMobile.LogoutLocation = _mMobile._mLocation;
				_mMobile.LogoutMap = _mMobile._mMap;

				_mMobile.Internalize();
			}
		}
	}

	private class ParalyzedTimer : Timer
	{
		private readonly Mobile _mMobile;

		public ParalyzedTimer(Mobile m, TimeSpan duration)
			: base(duration)
		{
			Priority = TimerPriority.TwentyFiveMs;
			_mMobile = m;
		}

		protected override void OnTick()
		{
			_mMobile.Paralyzed = false;
		}
	}

	private class FrozenTimer : Timer
	{
		private readonly Mobile _mMobile;

		public FrozenTimer(Mobile m, TimeSpan duration)
			: base(duration)
		{
			Priority = TimerPriority.TwentyFiveMs;
			_mMobile = m;
		}

		protected override void OnTick()
		{
			_mMobile.Frozen = false;
		}
	}

	private class CombatTimer : Timer
	{
		private readonly Mobile _mMobile;

		public CombatTimer(Mobile m)
			: base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.01), 0)
		{
			_mMobile = m;

			if (!_mMobile._player && _mMobile._dex <= 100)
				Priority = TimerPriority.FiftyMs;
		}

		protected override void OnTick()
		{
			if (Core.TickCount - _mMobile.NextCombatTime >= 0)
			{
				IDamageable combatant = _mMobile.Combatant;

				// If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
				if (combatant == null || combatant.Deleted || _mMobile.Deleted || combatant.Map != _mMobile._mMap ||
				    !combatant.Alive || !_mMobile.Alive || !_mMobile.CanSee(combatant) || (combatant is Mobile mobile && mobile.IsDeadBondedPet) ||
				    _mMobile.IsDeadBondedPet)
				{
					_mMobile.Combatant = null;
					return;
				}

				IWeapon weapon = _mMobile.Weapon;

				if (!_mMobile.InRange(combatant, weapon.MaxRange))
				{
					return;
				}

				if (_mMobile.InLOS(combatant))
				{
					weapon.OnBeforeSwing(_mMobile, combatant); //OnBeforeSwing for checking in regards to being hidden and whatnot
					_mMobile.RevealingAction();
					_mMobile.NextCombatTime = Core.TickCount + (int)weapon.OnSwing(_mMobile, combatant).TotalMilliseconds;
				}
			}
		}
	}

	private class ExpireCombatantTimer : Timer
	{
		private readonly Mobile _mobile;

		public ExpireCombatantTimer(Mobile m)
			: base(TimeSpan.FromMinutes(1.0))
		{
			Priority = TimerPriority.FiveSeconds;
			_mobile = m;
		}

		protected override void OnTick()
		{
			_mobile.Combatant = null;
		}
	}

	//public static TimeSpan ExpireCriminalDelay { get; set; } = TimeSpan.FromMinutes(2.0);

	private class ExpireCriminalTimer : Timer
	{
		private readonly Mobile _mobile;

		public ExpireCriminalTimer(Mobile m)
			: base(TimeSpan.FromMinutes(2.0))
		{
			Priority = TimerPriority.FiveSeconds;
			_mobile = m;
		}

		protected override void OnTick()
		{
			_mobile.Criminal = false;
		}
	}

	private class ExpireAggressorsTimer : Timer
	{
		private readonly Mobile _mobile;

		public ExpireAggressorsTimer(Mobile m)
			: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
		{
			_mobile = m;
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			if (_mobile.Deleted || (_mobile.Aggressors.Count == 0 && _mobile.Aggressed.Count == 0))
				_mobile.StopAggrExpire();
			else
				_mobile.CheckAggrExpire();
		}
	}

	private class WarmodeTimer : Timer
	{
		private readonly Mobile _mobile;

		public bool Value { get; set; }

		public WarmodeTimer(Mobile m, bool value)
			: base(WarmodeSpamDelay)
		{
			_mobile = m;
			Value = value;
		}

		protected override void OnTick()
		{
			_mobile.Warmode = Value;
			_mobile._warmodeChanges = 0;

			_mobile._warmodeTimer = null;
		}
	}

	#endregion

	public virtual void Attack(IDamageable m)
	{
		if (CheckAttack(m))
		{
			Combatant = m;
			EventSink.InvokeOnMobileAttackRequest(this, m);
		}
	}

	public virtual bool CheckAttack(IDamageable m)
	{
		return InUpdateRange(m) && CanSee(m) && InLOS(m);
	}

	public void LockCombat(IDamageable comb, TimeSpan time)
	{
		if (comb == null)
		{
			return;
		}

		Combatant = comb;
		_lockCombatTime = DateTime.UtcNow + time;
	}

	public void LockCombat(TimeSpan time)
	{
		if (Combatant == null)
		{
			return;
		}

		LockCombat(Combatant, time);
	}

	public bool CombatLocked()
	{
		if (_lockCombatTime > DateTime.UtcNow)
		{
			if (Combatant == null || Combatant.Deleted || ((Mobile)Combatant).Hidden || !Combatant.Alive || ((Mobile)Combatant).IsDeadBondedPet || !InRange(Combatant, 15) || !InLOS(Combatant))
			{
				_lockCombatTime = DateTime.UtcNow;
				return false;
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Overridable. Gets or sets which Mobile that this Mobile is currently engaged in combat with.
	/// <seealso cref="OnCombatantChange" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual IDamageable Combatant
	{
		get => _combatant;
		set
		{
			if (Deleted || CombatLocked())
			{
				return;
			}

			if (_combatant != value && value != this)
			{
				IDamageable old = _combatant;

				++_mChangingCombatant;
				_combatant = value;

				if ((_combatant != null && !CanBeHarmful(_combatant, false)) || !Region.OnCombatantChange(this, old, _combatant))
				{
					_combatant = old;
					--_mChangingCombatant;
					return;
				}

				_netState?.Send(new ChangeCombatant(_combatant));

				if (_combatant == null)
				{
					_expireCombatant?.Stop();

					_combatTimer?.Stop();

					_expireCombatant = null;
					_combatTimer = null;
				}
				else
				{
					_expireCombatant ??= new ExpireCombatantTimer(this);

					_expireCombatant.Start();

					_combatTimer ??= new CombatTimer(this);

					_combatTimer.Start();
				}

				if (_combatant != null && CanBeHarmful(_combatant, false))
				{
					DoHarmful(_combatant);

					if (_combatant is Mobile mobile)
					{
						mobile.PlaySound(mobile.GetAngerSound());
					}
				}

				OnCombatantChange();
				--_mChangingCombatant;
			}
		}
	}

	/// <summary>
	/// Overridable. Virtual event invoked after the <see cref="Combatant" /> property has changed.
	/// <seealso cref="Combatant" />
	/// </summary>
	public virtual void OnCombatantChange()
	{
	}

	public double GetDistanceToSqrt(Point3D p)
	{
		int xDelta = _mLocation.m_X - p.m_X;
		int yDelta = _mLocation.m_Y - p.m_Y;

		return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
	}

	public double GetDistanceToSqrt(Mobile m)
	{
		int xDelta = _mLocation.m_X - m._mLocation.m_X;
		int yDelta = _mLocation.m_Y - m._mLocation.m_Y;

		return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
	}

	public double GetDistanceToSqrt(IPoint2D p)
	{
		int xDelta = _mLocation.m_X - p.X;
		int yDelta = _mLocation.m_Y - p.Y;

		return Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
	}

	public virtual void AggressiveAction(Mobile aggressor)
	{
		AggressiveAction(aggressor, false);
	}

	public virtual void AggressiveAction(Mobile aggressor, bool criminal)
	{
		if (aggressor == this)
			return;

		EventSink.InvokeAggressiveAction(this, aggressor, criminal);

		if (Combatant == aggressor)
		{
			if (_expireCombatant == null)
				_expireCombatant = new ExpireCombatantTimer(this);
			else
				_expireCombatant.Stop();

			_expireCombatant.Start();
		}

		bool addAggressor = true;

		List<AggressorInfo> list = Aggressors;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Attacker == aggressor)
			{
				info.Refresh();
				info.CriminalAggression = criminal;
				info.CanReportMurder = criminal;

				addAggressor = false;
			}
		}

		list = aggressor.Aggressors;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Attacker == this)
			{
				info.Refresh();

				addAggressor = false;
			}
		}

		bool addAggressed = true;

		list = Aggressed;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Defender == aggressor)
			{
				info.Refresh();

				addAggressed = false;
			}
		}

		list = aggressor.Aggressed;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Defender == this)
			{
				info.Refresh();
				info.CriminalAggression = criminal;
				info.CanReportMurder = criminal;

				addAggressed = false;
			}
		}

		bool setCombatant = false;

		if (addAggressor)
		{
			Aggressors.Add(AggressorInfo.Create(aggressor, this, criminal)); // new AggressorInfo( aggressor, this, criminal, true ) );

			if (CanSee(aggressor) && _netState != null)
			{
				MobileIncoming.Send(_netState, aggressor);
			}

			if (Combatant == null)
				setCombatant = true;

			UpdateAggrExpire();
		}

		if (addAggressed)
		{
			aggressor.Aggressed.Add(AggressorInfo.Create(aggressor, this, criminal)); // new AggressorInfo( aggressor, this, criminal, false ) );

			if (CanSee(aggressor) && _netState != null)
			{
				MobileIncoming.Send(_netState, aggressor);
			}

			if (Combatant == null)
				setCombatant = true;

			UpdateAggrExpire();
		}

		if (setCombatant)
			Combatant = aggressor;

		Region.OnAggressed(aggressor, this, criminal);
	}

	public void RemoveAggressed(Mobile aggressed)
	{
		if (Deleted)
			return;

		List<AggressorInfo> list = Aggressed;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Defender == aggressed)
			{
				Aggressed.RemoveAt(i);
				info.Free();

				if (_netState != null && CanSee(aggressed))
				{
					MobileIncoming.Send(_netState, aggressed);
				}

				break;
			}
		}

		UpdateAggrExpire();
	}

	public void RemoveAggressor(Mobile aggressor)
	{
		if (Deleted)
			return;

		List<AggressorInfo> list = Aggressors;

		for (var i = 0; i < list.Count; ++i)
		{
			AggressorInfo info = list[i];

			if (info.Attacker == aggressor)
			{
				Aggressors.RemoveAt(i);
				info.Free();

				if (_netState != null && CanSee(aggressor))
				{
					MobileIncoming.Send(_netState, aggressor);
				}

				break;
			}
		}

		UpdateAggrExpire();
	}

	public virtual int GetTotal(TotalType type)
	{
		return type switch
		{
			TotalType.Gold => _totalGold,
			TotalType.Items => _totalItems,
			TotalType.Weight => _totalWeight,
			_ => 0,
		};
	}

	public virtual void UpdateTotal(Item sender, TotalType type, int delta)
	{
		if (delta == 0 || sender.IsVirtualItem)
			return;

		switch (type)
		{
			case TotalType.Gold:
				_totalGold += delta;
				Delta(MobileDelta.Gold);
				break;
			case TotalType.Items:
				_totalItems += delta;
				break;
			case TotalType.Weight:
				_totalWeight += delta;
				Delta(MobileDelta.Weight);
				OnWeightChange(_totalWeight - delta);
				break;
		}
	}

	public virtual void UpdateTotals()
	{
		if (Items == null)
			return;

		var oldWeight = _totalWeight;

		_totalGold = 0;
		_totalItems = 0;
		_totalWeight = 0;

		for (var i = 0; i < Items.Count; ++i)
		{
			Item item = Items[i];

			item.UpdateTotals();

			if (item.IsVirtualItem)
				continue;

			_totalGold += item.TotalGold;
			_totalItems += item.TotalItems + 1;
			_totalWeight += item.TotalWeight + item.PileWeight;
		}

		if (_mHolding != null)
			_totalWeight += _mHolding.TotalWeight + _mHolding.PileWeight;

		if (_totalWeight != oldWeight)
			OnWeightChange(oldWeight);
	}

	public void ClearQuestArrow()
	{
		_mQuestArrow = null;
	}

	public void ClearTarget()
	{
		_target = null;
	}

	#region Targets
	private class SimpleTarget : Target
	{
		private readonly TargetCallback _mCallback;

		public SimpleTarget(int range, TargetFlags flags, bool allowGround, TargetCallback callback)
			: base(range, allowGround, flags)
		{
			_mCallback = callback;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			_mCallback?.Invoke(from, targeted);
		}
	}

	public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetCallback callback)
	{
		Target t = new SimpleTarget(range, flags, allowGround, callback);

		Target = t;

		return t;
	}

	private class SimpleStateTarget : Target
	{
		private readonly TargetStateCallback _mCallback;
		private readonly object _mState;

		public SimpleStateTarget(int range, TargetFlags flags, bool allowGround, TargetStateCallback callback, object state)
			: base(range, allowGround, flags)
		{
			_mCallback = callback;
			_mState = state;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			_mCallback?.Invoke(from, targeted, _mState);
		}
	}

	public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetStateCallback callback, object state)
	{
		Target t = new SimpleStateTarget(range, flags, allowGround, callback, state);

		Target = t;

		return t;
	}

	private class SimpleStateTarget<T> : Target
	{
		private readonly TargetStateCallback<T> _callback;
		private readonly T _state;

		public SimpleStateTarget(int range, TargetFlags flags, bool allowGround, TargetStateCallback<T> callback, T state)
			: base(range, allowGround, flags)
		{
			_callback = callback;
			_state = state;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			_callback?.Invoke(from, targeted, _state);
		}
	}
	public Target BeginTarget<T>(int range, bool allowGround, TargetFlags flags, TargetStateCallback<T> callback, T state)
	{
		Target t = new SimpleStateTarget<T>(range, flags, allowGround, callback, state);

		Target = t;

		return t;
	}

	public Target Target
	{
		get => _target;
		set
		{
			Target oldTarget = _target;

			if (oldTarget == value)
				return;

			_target = null;

			if (oldTarget != null && value != null)
				oldTarget.Cancel(this, TargetCancelType.Overriden);

			_target = value;

			if (value != null && _netState != null && !TargetLocked)
				_netState.Send(value.GetPacketFor(_netState));

			OnTargetChange();
		}
	}
	#endregion

	/// <summary>
	/// Overridable. Virtual event invoked after the <see cref="Target">Target property</see> has changed.
	/// </summary>
	protected virtual void OnTargetChange()
	{
	}

	public ContextMenu ContextMenu
	{
		get => _contextMenu;
		set
		{
			_contextMenu = value;

			if (_contextMenu != null && _netState != null)
			{
				// Old packet is preferred until assistants catch up
				if (_netState.NewHaven && _contextMenu.RequiresNewPacket)
					_ = Send(new DisplayContextMenu(_contextMenu));
				else
					_ = Send(new DisplayContextMenuOld(_contextMenu));
			}
		}
	}

	public virtual bool CheckContextMenuDisplay(IEntity target)
	{
		return true;
	}

	#region Prompts
	private class SimplePrompt : Prompt
	{
		private readonly PromptCallback _callback;
		private readonly PromptCallback _cancelCallback;
		private readonly bool _callbackHandlesCancel;

		public SimplePrompt(PromptCallback callback, PromptCallback cancelCallback)
		{
			_callback = callback;
			_cancelCallback = cancelCallback;
		}

		public SimplePrompt(PromptCallback callback, bool callbackHandlesCancel = false)
		{
			_callback = callback;
			_callbackHandlesCancel = callbackHandlesCancel;
		}

		public override void OnResponse(Mobile from, string text)
		{
			_callback?.Invoke(from, text);
		}

		public override void OnCancel(Mobile from)
		{
			if (_callbackHandlesCancel && _callback != null)
				_callback(from, "");
			else _cancelCallback?.Invoke(from, "");
		}
	}

	public Prompt BeginPrompt(PromptCallback callback, PromptCallback cancelCallback)
	{
		Prompt p = new SimplePrompt(callback, cancelCallback);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt(PromptCallback callback, bool callbackHandlesCancel)
	{
		Prompt p = new SimplePrompt(callback, callbackHandlesCancel);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt(PromptCallback callback)
	{
		return BeginPrompt(callback, false);
	}

	private class SimpleStatePrompt : Prompt
	{
		private readonly PromptStateCallback _callback;
		private readonly PromptStateCallback _cancelCallback;
		private readonly bool _callbackHandlesCancel;
		private readonly object _state;

		public SimpleStatePrompt(PromptStateCallback callback, PromptStateCallback cancelCallback, object state)
		{
			_callback = callback;
			_cancelCallback = cancelCallback;
			_state = state;
		}
		public SimpleStatePrompt(PromptStateCallback callback, bool callbackHandlesCancel, object state)
		{
			_callback = callback;
			_state = state;
			_callbackHandlesCancel = callbackHandlesCancel;
		}
		public SimpleStatePrompt(PromptStateCallback callback, object state)
			: this(callback, false, state)
		{
		}

		public override void OnResponse(Mobile from, string text)
		{
			_callback?.Invoke(from, text, _state);
		}

		public override void OnCancel(Mobile from)
		{
			if (_callbackHandlesCancel && _callback != null)
				_callback(from, "", _state);
			else _cancelCallback?.Invoke(from, "", _state);
		}
	}
	public Prompt BeginPrompt(PromptStateCallback callback, PromptStateCallback cancelCallback, object state)
	{
		Prompt p = new SimpleStatePrompt(callback, cancelCallback, state);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt(PromptStateCallback callback, bool callbackHandlesCancel, object state)
	{
		Prompt p = new SimpleStatePrompt(callback, callbackHandlesCancel, state);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt(PromptStateCallback callback, object state)
	{
		return BeginPrompt(callback, false, state);
	}

	private class SimpleStatePrompt<T> : Prompt
	{
		private readonly PromptStateCallback<T> _callback;
		private readonly PromptStateCallback<T> _cancelCallback;
		private readonly bool _callbackHandlesCancel;
		private readonly T _state;

		public SimpleStatePrompt(PromptStateCallback<T> callback, PromptStateCallback<T> cancelCallback, T state)
		{
			_callback = callback;
			_cancelCallback = cancelCallback;
			_state = state;
		}
		public SimpleStatePrompt(PromptStateCallback<T> callback, bool callbackHandlesCancel, T state)
		{
			_callback = callback;
			_state = state;
			_callbackHandlesCancel = callbackHandlesCancel;
		}
		public SimpleStatePrompt(PromptStateCallback<T> callback, T state)
			: this(callback, false, state)
		{
		}

		public override void OnResponse(Mobile from, string text)
		{
			_callback?.Invoke(from, text, _state);
		}

		public override void OnCancel(Mobile from)
		{
			if (_callbackHandlesCancel && _callback != null)
				_callback(from, "", _state);
			else _cancelCallback?.Invoke(from, "", _state);
		}
	}

	public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, PromptStateCallback<T> cancelCallback, T state)
	{
		Prompt p = new SimpleStatePrompt<T>(callback, cancelCallback, state);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, bool callbackHandlesCancel, T state)
	{
		Prompt p = new SimpleStatePrompt<T>(callback, callbackHandlesCancel, state);

		Prompt = p;
		return p;
	}

	public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, T state)
	{
		return BeginPrompt(callback, false, state);
	}

	public Prompt Prompt
	{
		get => _prompt;
		set
		{
			Prompt oldPrompt = _prompt;
			Prompt newPrompt = value;

			if (oldPrompt == newPrompt)
				return;

			_prompt = null;

			if (oldPrompt != null && newPrompt != null)
				oldPrompt.OnCancel(this);

			_prompt = newPrompt;

			if (newPrompt != null)
			{
				newPrompt.SendTo(this);
			}
		}
	}
	#endregion

	private bool InternalOnMove(Direction d)
	{
		if (!OnMove(d))
			return false;

		MovementEventArgs e = MovementEventArgs.Create(this, d);

		EventSink.InvokeMovement(e);

		bool ret = !e.Blocked;

		e.Free();

		return ret;
	}

	/// <summary>
	/// Overridable. Event invoked before the Mobile <see cref="Move">moves</see>.
	/// </summary>
	/// <returns>True if the move is allowed, false if not.</returns>
	protected virtual bool OnMove(Direction d)
	{
		if (_hidden && _accessLevel == AccessLevel.Player)
		{
			if (AllowedStealthSteps-- <= 0 || (d & Direction.Running) != 0 || Mounted)
				RevealingAction();
		}

		return true;
	}

	private static readonly Packet[][] MMovingPacketCache = {//added two new packets for hul
		new Packet[8],
		new Packet[8],
		new Packet[8],
		new Packet[8]
	};

	public virtual void ClearFastwalkStack()
	{
		if (_moveRecords is {Count: > 0})
			_moveRecords.Clear();

		_endQueue = Core.TickCount;
	}

	public virtual bool CheckMovement(Direction d, out int newZ)
	{
		return Movement.Movement.CheckMovement(this, d, out newZ);
	}

	public virtual bool Move(Direction d)
	{
		if (Deleted)
			return false;

		BankBox box = FindBankNoCreate();

		if (box is {Opened: true})
			box.Close();

		Point3D newLocation = _mLocation;
		Point3D oldLocation = newLocation;

		if ((_mDirection & Direction.Mask) == (d & Direction.Mask))
		{
			// We are actually moving (not just a direction change)

			if (_spell != null && !_spell.OnCasterMoving(d))
				return false;

			if (_paralyzed || _frozen)
			{
				SendLocalizedMessage(500111); // You are frozen and can not move.

				return false;
			}

			if (CheckMovement(d, out int newZ))
			{
				int x = oldLocation.m_X, y = oldLocation.m_Y;
				int oldX = x, oldY = y;
				int oldZ = oldLocation.m_Z;

				switch (d & Direction.Mask)
				{
					case Direction.North:
						--y;
						break;
					case Direction.Right:
						++x;
						--y;
						break;
					case Direction.East:
						++x;
						break;
					case Direction.Down:
						++x;
						++y;
						break;
					case Direction.South:
						++y;
						break;
					case Direction.Left:
						--x;
						++y;
						break;
					case Direction.West:
						--x;
						break;
					case Direction.Up:
						--x;
						--y;
						break;
				}

				newLocation.m_X = x;
				newLocation.m_Y = y;
				newLocation.m_Z = newZ;

				Pushing = false;

				Map map = _mMap;

				if (map != null)
				{
					Sector oldSector = map.GetSector(oldX, oldY);
					Sector newSector = map.GetSector(x, y);

					if (oldSector != newSector)
					{
						if (oldSector.Mobiles.Any(m => m != this && m.X == oldX && m.Y == oldY && m.Z + 15 > oldZ && oldZ + 15 > m.Z && !m.OnMoveOff(this)))
						{
							return false;
						}

						if (oldSector.Items.Any(item => item.AtWorldPoint(oldX, oldY) && (item.Z == oldZ || (item.Z + item.ItemData.Height > oldZ && oldZ + 15 > item.Z)) && !item.OnMoveOff(this)))
						{
							return false;
						}

						if (newSector.Mobiles.Any(m => m.X == x && m.Y == y && m.Z + 15 > newZ && newZ + 15 > m.Z && !m.OnMoveOver(this)))
						{
							return false;
						}

						if (newSector.Items.Any(item => item.AtWorldPoint(x, y) && (item.Z == newZ || (item.Z + item.ItemData.Height > newZ && newZ + 15 > item.Z)) && !item.OnMoveOver(this)))
						{
							return false;
						}
					}
					else
					{
						for (int i = 0; i < oldSector.Mobiles.Count; ++i)
						{
							Mobile m = oldSector.Mobiles[i];

							if (m != this && m.X == oldX && m.Y == oldY && m.Z + 15 > oldZ && oldZ + 15 > m.Z && !m.OnMoveOff(this))
								return false;
							if (m.X == x && m.Y == y && m.Z + 15 > newZ && newZ + 15 > m.Z && !m.OnMoveOver(this))
								return false;
						}

						for (int i = 0; i < oldSector.Items.Count; ++i)
						{
							Item item = oldSector.Items[i];

							if (item.AtWorldPoint(oldX, oldY) && (item.Z == oldZ || (item.Z + item.ItemData.Height > oldZ && oldZ + 15 > item.Z)) && !item.OnMoveOff(this))
								return false;
							if (item.AtWorldPoint(x, y) && (item.Z == newZ || (item.Z + item.ItemData.Height > newZ && newZ + 15 > item.Z)) && !item.OnMoveOver(this))
								return false;
						}
					}

					if (!Region.CanMove(this, d, newLocation, oldLocation, _mMap))
						return false;
				}
				else
				{
					return false;
				}

				if (!InternalOnMove(d))
					return false;

				if (FwdEnabled && _netState != null && _accessLevel < FwdAccessOverride && (!FwdUotdOverride || !_netState.IsUOTDClient))
				{
					_moveRecords ??= new Queue<MovementRecord>(6);

					while (_moveRecords.Count > 0)
					{
						MovementRecord r = _moveRecords.Peek();

						if (r.Expired())
							_ = _moveRecords.Dequeue();
						else
							break;
					}

					if (_moveRecords.Count >= FwdMaxSteps)
					{
						FastWalkEventArgs fw = new(_netState);
						EventSink.InvokeFastWalk(fw);

						if (fw.Blocked)
							return false;
					}

					var delay = ComputeMovementSpeed(d);

					long end;

					if (_moveRecords.Count > 0)
						end = _endQueue + delay;
					else
						end = Core.TickCount + delay;

					_moveRecords.Enqueue(MovementRecord.NewInstance(end));

					_endQueue = end;
				}

				LastMoveTime = Core.TickCount;
			}
			else
			{
				return false;
			}

			DisruptiveAction();
		}

		_netState?.Send(MovementAck.Instantiate(_netState.Sequence, this));//new MovementAck( m_NetState.Sequence, this ) );

		SetLocation(newLocation, false);
		SetDirection(d);

		if (_mMap != null)
		{
			IPooledEnumerable<IEntity> eable = _mMap.GetObjectsInRange(_mLocation, Map.GlobalMaxUpdateRange);

			foreach (IEntity o in eable)
			{
				if (o == this)
					continue;

				switch (o)
				{
					case Mobile mob:
					{
						if (mob.NetState != null)
							m_MoveClientList.Add(mob);
						m_MoveList.Add(o);
						break;
					}
					case Item item:
					{
						if (item.HandlesOnMovement)
							m_MoveList.Add(item);
						break;
					}
				}
			}

			eable.Free();

			Packet[][] cache = MMovingPacketCache;

			/*for( int i = 0; i < cache.Length; ++i )
				for( int j = 0; j < cache[i].Length; ++j )
					Packet.Release( ref cache[i][j] );*/

			foreach (Mobile m in m_MoveClientList)
			{
				NetState ns = m.NetState;

				if (ns != null && InUpdateRange(_mLocation, m._mLocation) && m.CanSee(this))
				{
					if (ns.StygianAbyss)
					{
						Packet p;
						var noto = Notoriety.Compute(m, this);

						if (m.IsHallucinated)
						{
							p = cache[2][noto];
							if (p == null)
								cache[2][noto] = p = Packet.Acquire(new MobileMoving(this, noto));
						}
						else
						{
							p = cache[0][noto];
							if (p == null)
								cache[0][noto] = p = Packet.Acquire(new MobileMoving(this, noto));
						}
						ns.Send(p);
					}
					else
					{
						Packet p;
						var noto = Notoriety.Compute(m, this);

						if (m.IsHallucinated)
						{
							p = cache[3][noto];
							if (p == null)
								cache[3][noto] = p = Packet.Acquire(new MobileMovingOld(this, noto));
						}
						else
						{
							p = cache[1][noto];
							if (p == null)
								cache[1][noto] = p = Packet.Acquire(new MobileMovingOld(this, noto));
						}

						ns.Send(p);
					}
				}
			}

			for (var i = 0; i < cache.Length; ++i)
			for (var j = 0; j < cache[i].Length; ++j)
				Packet.Release(ref cache[i][j]);

			for (var i = 0; i < m_MoveList.Count; ++i)
			{
				IEntity o = m_MoveList[i];

				switch (o)
				{
					case Mobile mob:
						mob.OnMovement(this, oldLocation);
						break;
					case Item item:
						item.OnMovement(this, oldLocation);
						break;
				}
			}

			if (m_MoveList.Count > 0)
				m_MoveList.Clear();

			if (m_MoveClientList.Count > 0)
				m_MoveClientList.Clear();
		}

		OnAfterMove(oldLocation);
		return true;
	}

	public virtual void OnAfterMove(Point3D oldLocation)
	{
	}

	public int ComputeMovementSpeed()
	{
		return ComputeMovementSpeed(Direction, false);
	}

	public int ComputeMovementSpeed(Direction dir)
	{
		return ComputeMovementSpeed(dir, true);
	}

	public virtual int ComputeMovementSpeed(Direction dir, bool checkTurning)
	{
		int delay;

		if (Mounted)
			delay = (dir & Direction.Running) != 0 ? RunMount : WalkMount;
		else
			delay = (dir & Direction.Running) != 0 ? RunFoot : WalkFoot;

		return delay;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when a Mobile <paramref name="m" /> moves off this Mobile.
	/// </summary>
	/// <returns>True if the move is allowed, false if not.</returns>
	public virtual bool OnMoveOff(Mobile m)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Event invoked when a Mobile <paramref name="m" /> moves over this Mobile.
	/// </summary>
	/// <returns>True if the move is allowed, false if not.</returns>
	public virtual bool OnMoveOver(Mobile m)
	{
		if (_mMap == null || Deleted)
			return true;

		return m.CheckShove(this);
	}

	public virtual bool CheckShove(Mobile shoved)
	{
		if (!_mIgnoreMobiles && (_mMap.Rules & ZoneRules.FreeMovement) == 0)
		{
			if (!shoved.Alive || !Alive || shoved.IsDeadBondedPet || IsDeadBondedPet)
				return true;
			if (shoved._hidden && shoved._accessLevel > AccessLevel.Player)
				return true;

			if (!Pushing)
			{
				Pushing = true;

				int number;

				if (AccessLevel > AccessLevel.Player)
				{
					number = shoved._hidden ? 1019041 : 1019040;
				}
				else
				{
					if (Stam == StamMax)
					{
						number = shoved._hidden ? 1019043 : 1019042;
						Stam -= 10;

						RevealingAction();
					}
					else
					{
						return false;
					}
				}

				SendLocalizedMessage(number);
			}
		}
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile sees another Mobile, <paramref name="m" />, move.
	/// </summary>
	public virtual void OnMovement(Mobile m, Point3D oldLocation)
	{
	}

	public ISpell Spell
	{
		get => _spell;
		set
		{
			if (_spell != null && value != null)
				Console.WriteLine("Warning: Spell has been overwritten");

			_spell = value;
		}
	}


	public virtual void CriminalAction(bool message)
	{
		if (Deleted)
			return;

		Criminal = true;

		Region.OnCriminalAction(this, message);
	}

	public virtual bool IsPlayer() => Utilities.IsPlayer(this);
	public virtual bool IsStaff() => Utilities.IsStaff(this);
	public virtual bool IsAdministrator() => Utilities.IsAdministrator(this);
	public virtual bool IsCounselor() => Utilities.IsCounselor(this);
	public virtual bool IsDecorator() => Utilities.IsDecorator(this);
	public virtual bool IsDeveloper() => Utilities.IsDeveloper(this);
	public virtual bool IsGameMaster() => Utilities.IsGameMaster(this);
	public virtual bool IsSeer() => Utilities.IsSeer(this);
	public virtual bool IsVip() => Utilities.IsVip(this);
	public virtual bool IsOwner() => Utilities.IsOwner(this);
	public virtual bool IsSnoop(Mobile from) => from != this;

	/// <summary>
	/// Overridable. Any call to <see cref="Resurrect" /> will silently fail if this method returns false.
	/// <seealso cref="Resurrect" />
	/// </summary>
	public virtual bool CheckResurrect() => true;

	/// <summary>
	/// Overridable. Event invoked before the Mobile is <see cref="Resurrect">resurrected</see>.
	/// <seealso cref="Resurrect" />
	/// </summary>
	public virtual void OnBeforeResurrect()
	{
	}

	/// <summary>
	/// Overridable. Event invoked after the Mobile is <see cref="Resurrect">resurrected</see>.
	/// <seealso cref="Resurrect" />
	/// </summary>
	public virtual void OnAfterResurrect()
	{
		EventSink.InvokeOnMobileResurrect(this);
	}

	public virtual void Resurrect()
	{
		if (!Alive)
		{
			if (!Region.OnResurrect(this))
				return;

			if (!CheckResurrect())
				return;

			OnBeforeResurrect();

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
				box.Close();

			Poison = null;

			Warmode = false;

			Hits = 10;
			Stam = StamMax;
			Mana = 0;

			BodyMod = 0;
			Body = Race.AliveBody(this);

			ProcessDeltaQueue();

			for (var i = Items.Count - 1; i >= 0; --i)
			{
				if (i >= Items.Count)
					continue;

				Item item = Items[i];

				if (item.ItemId == 0x204E)
					item.Delete();
			}

			SendIncomingPacket();
			SendIncomingPacket();

			OnAfterResurrect();

			//Send( new DeathStatus( false ) );
		}
	}

	public void DropHolding()
	{
		Item holding = _mHolding;

		if (holding != null)
		{
			if (!holding.Deleted && holding.HeldBy == this && holding.Map == Map.Internal)
				_ = AddToBackpack(holding);

			Holding = null;
			holding.ClearBounce();
		}
	}

	public virtual void Delete()
	{
		if (Deleted)
			return;
		if (!World.OnDelete(this))
			return;

		_netState?.CancelAllTrades();

		_netState?.Dispose();

		DropHolding();

		Region.OnRegionChange(this, _region, null);

		_region = null;
		//Is the above line REALLY needed?  The old Region system did NOT have said line
		//and worked fine, because of this a LOT of extra checks have to be done everywhere...
		//I guess this should be there for Garbage collection purposes, but, still, is it /really/ needed?

		OnDelete();

		for (var i = Items.Count - 1; i >= 0; --i)
			if (i < Items.Count)
				Items[i].OnParentDeleted(this);

		for (var i = 0; i < Stabled.Count; i++)
			Stabled[i].Delete();

		SendRemovePacket();

		_mGuild?.OnDelete(this);

		Deleted = true;

		if (_mMap != null)
		{
			_mMap.OnLeave(this);
			_mMap = null;
		}

		_hair = null;
		_facialHair = null;
		_mMountItem = null;
		_face = null;

		World.RemoveMobile(this);

		OnAfterDelete();

		FreeCache();
	}

	/// <summary>
	/// Overridable. Virtual event invoked before the Mobile is deleted.
	/// </summary>
	public virtual void OnDelete()
	{
		if (Spawner != null)
		{
			Spawner.Remove(this);
			Spawner = null;
		}
	}

	/// <summary>
	/// Overridable. Returns true if the player is alive, false if otherwise. By default, this is computed by: <c>!Deleted &amp;&amp; (!Player || !Body.IsGhost)</c>
	/// </summary>
	[CommandProperty(AccessLevel.Counselor)]
	public virtual bool Alive => !Deleted && (!_player || !_mBody.IsGhost);

	public virtual bool CheckSpellCast(ISpell spell)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile casts a <paramref name="spell" />.
	/// </summary>
	/// <param name="spell"></param>
	public virtual void OnSpellCast(ISpell spell)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked after <see cref="TotalWeight" /> changes.
	/// </summary>
	public virtual void OnWeightChange(int oldValue)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the <see cref="Skill.Base" /> or <see cref="Skill.BaseFixedPoint" /> property of <paramref name="skill" /> changes.
	/// </summary>
	public virtual void OnSkillChange(SkillName skill, double oldBase)
	{
	}

	/// <summary>
	/// Overridable. Invoked after the mobile is deleted. When overriden, be sure to call the base method.
	/// </summary>
	public virtual void OnAfterDelete()
	{
		StopAggrExpire();

		CheckAggrExpire();

		PoisonTimer?.Stop();

		_hitsTimer?.Stop();

		_stamTimer?.Stop();

		_manaTimer?.Stop();

		_combatTimer?.Stop();

		_expireCombatant?.Stop();

		_logoutTimer?.Stop();

		_expireCriminal?.Stop();

		_warmodeTimer?.Stop();

		_paraTimer?.Stop();

		_frozenTimer?.Stop();

		_autoManifestTimer?.Stop();

		EventSink.InvokeOnMobileDeleted(this);
	}

	public virtual bool AllowSkillUse(SkillName name) => true;

	public virtual bool UseSkill(SkillName name) => Skills.UseSkill(this, name);

	public virtual bool UseSkill(int skillId) => Skills.UseSkill(this, skillId);

	public static CreateCorpseHandler CreateCorpseHandler { get; set; }

	public virtual DeathMoveResult GetParentMoveResultFor(Item item) => item.OnParentDeath(this);

	public virtual DeathMoveResult GetInventoryMoveResultFor(Item item) => item.OnInventoryDeath(this);

	public virtual void Kill()
	{
		if (!CanBeDamaged() || !Alive || IsDeadBondedPet || Deleted || !Region.OnBeforeDeath(this) || !OnBeforeDeath())
			return;

		BankBox box = FindBankNoCreate();

		if (box is {Opened: true})
			box.Close();

		_netState?.CancelAllTrades();

		_spell?.OnCasterKilled();
		//m_Spell.Disturb( DisturbType.Kill );

		_target?.Cancel(this, TargetCancelType.Canceled);

		DisruptiveAction();

		Warmode = false;

		DropHolding();

		Hits = 0;
		Stam = 0;
		Mana = 0;

		Poison = null;
		Combatant = null;

		if (Paralyzed)
		{
			Paralyzed = false;

			_paraTimer?.Stop();
		}

		if (Frozen)
		{
			Frozen = false;

			_frozenTimer?.Stop();
		}

		List<Item> content = new();
		List<Item> equip = new();
		List<Item> moveToPack = new();

		List<Item> itemsCopy = new(Items);

		Container pack = Backpack;

		for (var i = 0; i < itemsCopy.Count; ++i)
		{
			Item item = itemsCopy[i];

			if (item == pack)
				continue;

			DeathMoveResult res = GetParentMoveResultFor(item);

			switch (res)
			{
				case DeathMoveResult.MoveToCorpse:
				{
					content.Add(item);
					equip.Add(item);
					break;
				}
				case DeathMoveResult.MoveToBackpack:
				{
					moveToPack.Add(item);
					break;
				}
			}
		}

		if (pack != null)
		{
			List<Item> packCopy = new(pack.Items);

			for (var i = 0; i < packCopy.Count; ++i)
			{
				Item item = packCopy[i];

				DeathMoveResult res = GetInventoryMoveResultFor(item);

				if (res == DeathMoveResult.MoveToCorpse)
					content.Add(item);
				else
					moveToPack.Add(item);
			}

			for (var i = 0; i < moveToPack.Count; ++i)
			{
				Item item = moveToPack[i];

				if (RetainPackLocsOnDeath && item.Parent == pack)
					continue;

				pack.DropItem(item);
			}
		}

		HairInfo hair = null;
		if (_hair != null)
			hair = new HairInfo(_hair.ItemID, _hair.Hue);

		FacialHairInfo facialhair = null;
		if (_facialHair != null)
			facialhair = new FacialHairInfo(_facialHair.ItemID, _facialHair.Hue);

		Container c = CreateCorpseHandler?.Invoke(this, hair, facialhair, content, equip);

		if (_mMap != null)
		{
			Packet animPacket = null;
			//Packet remPacket = null;//RemovePacket; possible fix check later

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state != _netState)
				{
					animPacket ??= Packet.Acquire(new DeathAnimation(this, c));

					state.Send(animPacket);

					if (!state.Mobile.CanSee(this))
					{
						//if (remPacket == null)
						//	remPacket = RemovePacket;
						// XXX LOS BEGIN
						state.Mobile.RemoveLos(this);
						// XXX LOS END
						state.Send(RemovePacket);
					}
				}
			}

			Packet.Release(animPacket);

			eable.Free();
		}

		Region.OnDeath(this);
		OnDeath(c);
	}

	/// <summary>
	/// Overridable. Event invoked before the Mobile is <see cref="Kill">killed</see>.
	/// <seealso cref="Kill" />
	/// <seealso cref="OnDeath" />
	/// </summary>
	/// <returns>True to continue with death, false to override it.</returns>
	public virtual bool OnBeforeDeath() => true;

	/// <summary>
	/// Overridable. Event invoked after the Mobile is <see cref="Kill">killed</see>. Primarily, this method is responsible for deleting an NPC or turning a PC into a ghost.
	/// <seealso cref="Kill" />
	/// <seealso cref="OnBeforeDeath" />
	/// </summary>
	public virtual void OnDeath(Container c)
	{
		EventSink.InvokeOnMobileDeath(this, LastKiller, c);

		int sound = GetDeathSound();

		if (sound >= 0)
			Effects.PlaySound(this, Map, sound);

		if (!_player)
		{
			Delete();
		}
		else
		{
			_ = Send(DeathStatus.Instantiate(true));

			Warmode = false;

			BodyMod = 0;
			//Body = this.Female ? 0x193 : 0x192;
			Body = Race.GhostBody(this);

			Item deathShroud = new(0x204E)
			{
				Movable = false,
				Layer = Layer.OuterTorso
			};

			AddItem(deathShroud);

			_ = Items.Remove(deathShroud);
			Items.Insert(0, deathShroud);

			Poison = null;
			Combatant = null;

			Hits = 0;
			Stam = 0;
			Mana = 0;

			ProcessDeltaQueue();

			_ = Send(DeathStatus.Instantiate(false));

			CheckStatTimers();
		}
	}

	#region Get*Sound
	public virtual int GetAngerSound()
	{
		if (BaseSoundId != 0)
			return BaseSoundId;

		return -1;
	}

	public virtual int GetIdleSound()
	{
		if (BaseSoundId != 0)
			return BaseSoundId + 1;

		return -1;
	}

	public virtual int GetAttackSound()
	{
		if (BaseSoundId != 0)
			return BaseSoundId + 2;

		return -1;
	}

	public virtual int GetHurtSound()
	{
		if (BaseSoundId != 0)
			return BaseSoundId + 3;

		return -1;
	}

	public virtual int GetDeathSound()
	{
		if (BaseSoundId != 0)
		{
			return BaseSoundId + 4;
		}

		if (_mBody.IsHuman)
		{
			return Utility.Random(_female ? 0x314 : 0x423, _female ? 4 : 5);
		}

		return -1;
	}
	#endregion

	public virtual bool CheckTarget(Mobile from, Target targ, object targeted)
	{
		return true;
	}

	public virtual void Use(Item item)
	{
		if (item == null || item.Deleted || item.QuestItem || Deleted)
			return;

		DisruptiveAction();

		if (_spell != null && !_spell.OnCasterUsingObject(item))
			return;

		object root = item.RootParent;
		bool okay = false;

		if (!InUpdateRange(item.GetWorldLocation()))
			item.OnDoubleClickOutOfRange(this);
		else if (!CanSee(item))
			item.OnDoubleClickCantSee(this);
		else if (!item.IsAccessibleTo(this))
		{
			Region reg = Region.Find(item.GetWorldLocation(), item.Map);

			if (reg == null || !reg.SendInaccessibleMessage(item, this))
				item.OnDoubleClickNotAccessible(this);
		}
		else if (!CheckAlive(false))
			item.OnDoubleClickDead(this);
		else if (item.InSecureTrade)
			item.OnDoubleClickSecureTrade(this);
		else if (!AllowItemUse(item))
		{// okay = false;
		}
		else if (!item.CheckItemUse(this, item))
		{// okay = false;
		}
		else if (root is Mobile mob && mob.IsSnoop(this))
			item.OnSnoop(this);
		else if (Region.OnDoubleClick(this, item))
			okay = true;

		if (okay)
		{
			if (!item.Deleted)
				item.OnItemUsed(this, item);

			if (!item.Deleted)
				item.OnDoubleClick(this);
		}
	}

	public virtual void Use(Mobile m)
	{
		if (m == null || m.Deleted || Deleted)
			return;

		DisruptiveAction();

		if (_spell != null && !_spell.OnCasterUsingObject(m))
			return;

		if (!InUpdateRange(m))
			m.OnDoubleClickOutOfRange(this);
		else if (!CanSee(m))
			m.OnDoubleClickCantSee(this);
		else if (!CheckAlive(false))
			m.OnDoubleClickDead(this);
		else if (Region.OnDoubleClick(this, m) && !m.Deleted)
			m.OnDoubleClick(this);
	}

	public virtual void Lift(Item item, int amount, out bool rejected, out LRReason reject)
	{
		rejected = true;
		reject = LRReason.Inspecific;

		if (item == null)
			return;

		Mobile from = this;
		NetState state = _netState;

		if (from.AccessLevel >= AccessLevel.GameMaster || Core.TickCount - from.NextActionTime >= 0)
		{
			if (from.CheckAlive())
			{
				from.DisruptiveAction();

				if (from.Holding != null)
				{
					reject = LRReason.AreHolding;
				}
				else if (from.IsPlayer() && !from.InRange(item.GetWorldLocation(), 2))
				{
					reject = LRReason.OutOfRange;
				}
				else if (!from.CanSee(item) || !from.InLOS(item))
				{
					reject = LRReason.OutOfSight;
				}
				else if (!item.VerifyMove(from))
				{
					reject = LRReason.CannotLift;
				}
				else if (!item.IsAccessibleTo(from))
				{
					reject = LRReason.CannotLift;
				}
				else if (item.Nontransferable && amount != item.Amount)
				{
					if (item.QuestItem)
						from.SendLocalizedMessage(1074868); // Stacks of quest items cannot be unstacked.

					reject = LRReason.CannotLift;
				}
				else if (!item.CheckLift(from, item, ref reject))
				{
				}
				else
				{
					object root = item.RootParent;

					if (root is Mobile mob && !mob.CheckNonlocalLift(from, item))
					{
						reject = LRReason.TryToSteal;
					}
					else if (!from.OnDragLift(item) || !item.OnDragLift(from))
					{
						reject = LRReason.Inspecific;
					}
					else if (!from.CheckAlive())
					{
						reject = LRReason.Inspecific;
					}
					else
					{
						item.SetLastMoved();
						var itemGrid = item.GridLocation;

						if (item.Spawner != null)
						{
							item.Spawner.Remove(item);
							item.Spawner = null;
						}

						if (amount < 1)
							amount = 1;

						if (amount > item.Amount)
							amount = item.Amount;

						var oldAmount = item.Amount;
						Item oldStack = null;

						if (amount < oldAmount)
						{
							oldStack = LiftItemDupe(item, amount);
						}
						//item.Amount = amount; //Set in LiftItemDupe

						//if (amount < oldAmount)
						//_ = LiftItemDupe(item, amount);
						//item.Dupe( oldAmount - amount );

						Map map = from.Map;

						if (DragEffects && map != null && (root == null || root is Item))
						{
							IPooledEnumerable<NetState> eable = map.GetClientsInRange(from.Location);
							Packet p = null;

							foreach (NetState ns in eable)
							{
								if (ns.Mobile != from && ns.Mobile.CanSee(from) && ns.Mobile.InLOS(from) && ns.Mobile.CanSee(root))
								{
									if (p == null)
									{
										IEntity src = root == null ? new Entity(Serial.Zero, item.Location, map) : new Entity(((Item)root).Serial, ((Item)root).Location, map);

										p = Packet.Acquire(new DragEffect(src, from, item.ItemId, item.Hue, amount));
									}

									ns.Send(p);
								}
							}

							Packet.Release(p);

							eable.Free();
						}

						Point3D fixLoc = item.Location;
						Map fixMap = item.Map;
						bool shouldFix = item.Parent == null;

						item.RecordBounce(this, oldStack);
						item.OnItemLifted(from, item);
						item.Internalize();

						from.Holding = item;

						item.GridLocation = 0;

						if (oldStack != null)
						{
							oldStack.GridLocation = itemGrid;
						}

						var liftSound = item.GetLiftSound(from);

						if (liftSound != -1)
							_ = from.Send(new PlaySound(liftSound, from));

						from.NextActionTime = Core.TickCount + ActionDelay;

						if (fixMap != null && shouldFix)
							fixMap.FixColumn(fixLoc.m_X, fixLoc.m_Y);

						reject = LRReason.Inspecific;
						rejected = false;
					}
				}
			}
			else
			{
				reject = LRReason.Inspecific;
			}
		}
		else
		{
			SendActionMessage();
			reject = LRReason.Inspecific;
		}

		if (rejected && state != null)
		{
			state.Send(new LiftRej(reject));

			if (item.Deleted)
				return;

			switch (item.Parent)
			{
				case Item:
					ContainerContentUpdate.Send(state, item);
					break;
				case Mobile:
					state.Send(new EquipUpdate(item));
					break;
				default:
					item.SendInfoTo(state);
					break;
			}

			if (ObjectPropertyList.Enabled && item.Parent != null)
				state.Send(item.OplPacket);
		}
	}

	public static Item LiftItemDupe(Item oldItem, int amount)
	{
		Item item;
		try
		{
			item = (Item)Activator.CreateInstance(oldItem.GetType());
		}
		catch
		{
			Console.WriteLine("Warning: 0x{0:X}: Item must have a zero paramater constructor to be separated from a stack. '{1}'.", oldItem.Serial.Value, oldItem.GetType().Name);
			return null;
		}

		if (item == null)
			return null;
		item.Visible = oldItem.Visible;
		item.Movable = oldItem.Movable;
		item.LootType = oldItem.LootType;
		item.Direction = oldItem.Direction;
		item.Hue = oldItem.Hue;
		item.ItemId = oldItem.ItemId;
		item.Location = oldItem.Location;
		item.Layer = oldItem.Layer;
		item.Name = oldItem.Name;
		item.Weight = oldItem.Weight;

		item.Amount = oldItem.Amount - amount;
		item.Map = oldItem.Map;

		oldItem.Amount = amount;
		oldItem.OnAfterDuped(item);

		switch (oldItem.Parent)
		{
			case Mobile mobile:
				mobile.AddItem(item);
				break;
			case Item item1:
				item1.AddItem(item);
				break;
		}

		item.Delta(ItemDelta.Update);

		return item;

	}

	public virtual void SendDropEffect(Item item)
	{
		if (DragEffects && !item.Deleted)
		{
			Map map = _mMap;
			object root = item.RootParent;

			if (map != null && (root == null || root is Item))
			{
				IPooledEnumerable<NetState> eable = map.GetClientsInRange(_mLocation);
				Packet p = null;

				foreach (NetState ns in eable)
				{
					if (ns.StygianAbyss)
						continue;

					if (ns.Mobile != this && ns.Mobile.CanSee(this) && ns.Mobile.InLOS(this) && ns.Mobile.CanSee(root))
					{
						if (p == null)
						{
							IEntity trg;

							trg = root == null ? new Entity(Serial.Zero, item.Location, map) : new Entity(((Item)root).Serial, ((Item)root).Location, map);

							p = Packet.Acquire(new DragEffect(this, trg, item.ItemId, item.Hue, item.Amount));
						}

						ns.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}
	}

	public virtual bool Drop(Item to, Point3D loc)
	{
		Mobile from = this;
		Item item = from.Holding;

		bool valid = item != null && item.HeldBy == from && item.Map == Map.Internal;

		from.Holding = null;

		if (!valid)
		{
			return false;
		}

		bool bounced = true;

		item.SetLastMoved();

		if (to == null || !item.DropToItem(from, to, loc))
			item.Bounce(from);
		else
			bounced = false;

		item.ClearBounce();

		if (!bounced)
			SendDropEffect(item);

		return !bounced;
	}

	public virtual bool Drop(Point3D loc)
	{
		Mobile from = this;
		Item item = from.Holding;

		bool valid = item != null && item.HeldBy == from && item.Map == Map.Internal;

		from.Holding = null;

		if (!valid)
		{
			return false;
		}

		bool bounced = true;

		item.SetLastMoved();

		if (!item.DropToWorld(from, loc))
			item.Bounce(from);
		else
			bounced = false;

		item.ClearBounce();

		if (!bounced)
			SendDropEffect(item);

		return !bounced;
	}

	public virtual bool Drop(Mobile to, Point3D loc)
	{
		Mobile from = this;
		Item item = from.Holding;

		bool valid = item != null && item.HeldBy == from && item.Map == Map.Internal;

		from.Holding = null;

		if (!valid)
		{
			return false;
		}

		bool bounced = true;

		item.SetLastMoved();

		if (to == null || !item.DropToMobile(from, to, loc))
			item.Bounce(from);
		else
			bounced = false;

		item.ClearBounce();

		if (!bounced)
			SendDropEffect(item);

		return !bounced;
	}

	public virtual bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
	{
		if (Alive)
			return false;

		StringBuilder sb = new(text.Length, text.Length);

		for (int i = 0; i < text.Length; ++i)
		{
			_ = sb.Append(text[i] != ' ' ? GhostChars[Utility.Random(GhostChars.Length)] : ' ');
		}

		text = sb.ToString();
		context = m_GhostMutateContext;
		return true;
	}

	public virtual void Manifest(TimeSpan delay)
	{
		Warmode = true;

		if (_autoManifestTimer == null)
			_autoManifestTimer = new AutoManifestTimer(this, delay);
		else
			_autoManifestTimer.Stop();

		_autoManifestTimer.Start();
	}

	public virtual bool CheckSpeechManifest()
	{
		if (Alive)
			return false;

		TimeSpan delay = AutoManifestTimeout;

		if (delay > TimeSpan.Zero && (!Warmode || _autoManifestTimer != null))
		{
			Manifest(delay);
			return true;
		}

		return false;
	}

	public virtual bool CheckHearsMutatedSpeech(Mobile m, object context)
	{
		if (context == m_GhostMutateContext)
			return m.Alive && !m.CanHearGhosts;

		return true;
	}

	private void AddSpeechItemsFrom(List<IEntity> list, Container cont)
	{
		for (var i = 0; i < cont.Items.Count; ++i)
		{
			Item item = cont.Items[i];

			if (item.HandlesOnSpeech)
				list.Add(item);

			if (item is Container container)
				AddSpeechItemsFrom(list, container);
		}
	}

	private class LocationComparer : IComparer<IEntity>
	{
		private static LocationComparer _instance;

		public static LocationComparer GetInstance(IEntity relativeTo)
		{
			if (_instance == null)
				_instance = new LocationComparer(relativeTo);
			else
				_instance.RelativeTo = relativeTo;

			return _instance;
		}

		private IEntity RelativeTo { get; set; }

		public LocationComparer(IEntity relativeTo)
		{
			RelativeTo = relativeTo;
		}

		private int GetDistance(IPoint3D p)
		{
			var x = RelativeTo.X - p.X;
			var y = RelativeTo.Y - p.Y;
			var z = RelativeTo.Z - p.Z;

			x *= 11;
			y *= 11;

			return x * x + y * y + z * z;
		}

		public int Compare(IEntity x, IEntity y)
		{
			return GetDistance(x) - GetDistance(y);
		}
	}

	#region Get*InRange

	public IPooledEnumerable<Item> GetItemsInRange(int range)
	{
		Map map = _mMap;

		return map == null ? Map.NullEnumerable<Item>.Instance : map.GetItemsInRange(_mLocation, range);
	}

	public IPooledEnumerable<IEntity> GetObjectsInRange(int range)
	{
		Map map = _mMap;

		return map == null ? Map.NullEnumerable<IEntity>.Instance : map.GetObjectsInRange(_mLocation, range);
	}

	public IPooledEnumerable<Mobile> GetMobilesInRange(int range)
	{
		Map map = _mMap;

		return map == null ? Map.NullEnumerable<Mobile>.Instance : map.GetMobilesInRange(_mLocation, range);
	}

	public IPooledEnumerable<NetState> GetClientsInRange(int range)
	{
		Map map = _mMap;

		return map == null ? Map.NullEnumerable<NetState>.Instance : map.GetClientsInRange(_mLocation, range);
	}

	#endregion


	public virtual void DoSpeech(string text, int[] keywords, MessageType type, int hue)
	{
		if (Deleted || CommandSystem.Handle(this, text, type))
			return;

		var range = 15;

		switch (type)
		{
			case MessageType.Regular:
				SpeechHue = hue;
				break;
			case MessageType.Emote:
				EmoteHue = hue;
				break;
			case MessageType.Whisper:
				WhisperHue = hue;
				range = 1;
				break;
			case MessageType.Yell:
				YellHue = hue;
				range = 18;
				break;
			default:
				type = MessageType.Regular;
				break;
		}

		SpeechEventArgs regArgs = new(this, text, type, hue, keywords);

		EventSink.InvokeSpeech(regArgs);
		Region.OnSpeech(regArgs);
		OnSaid(regArgs);

		if (regArgs.Blocked)
			return;

		text = regArgs.Speech;

		if (string.IsNullOrEmpty(text))
			return;

		List<Mobile> hears = m_Hears;
		List<IEntity> onSpeech = m_OnSpeech;

		if (_mMap != null)
		{
			IPooledEnumerable<IEntity> eable = _mMap.GetObjectsInRange(_mLocation, range);

			foreach (IEntity o in eable)
			{
				if (o is Mobile heard)
				{
					if (heard.CanSee(this) && (NoSpeechLos || !heard.Player || heard.InLOS(this)))
					{
						if (heard._netState != null)
							hears.Add(heard);

						if (heard.HandlesOnSpeech(this))
							onSpeech.Add(heard);

						for (int i = 0; i < heard.Items.Count; ++i)
						{
							Item item = heard.Items[i];

							if (item.HandlesOnSpeech)
								onSpeech.Add(item);

							if (item is Container container)
								AddSpeechItemsFrom(onSpeech, container);
						}
					}
				}
				else if (o is Item item)
				{
					if (item.HandlesOnSpeech)
						onSpeech.Add(o);

					if (o is Container container)
						AddSpeechItemsFrom(onSpeech, container);
				}
			}

			eable.Free();

			object mutateContext = null;
			string mutatedText = text;
			SpeechEventArgs mutatedArgs = null;

			if (MutateSpeech(hears, ref mutatedText, ref mutateContext))
				mutatedArgs = new SpeechEventArgs(this, mutatedText, type, hue, Array.Empty<int>());

			_ = CheckSpeechManifest();

			ProcessDelta();

			Packet regp = null;
			Packet mutp = null;

			// TODO: Should this be sorted like onSpeech is below?

			for (int i = 0; i < hears.Count; ++i)
			{
				Mobile heard = hears[i];

				if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
				{
					heard.OnSpeech(regArgs);

					NetState ns = heard.NetState;

					if (ns != null)
					{
						regp ??= Packet.Acquire(new UnicodeMessage(_serial, Body, type, hue, 3, _language, Name, text));

						ns.Send(regp);
					}
				}
				else
				{
					heard.OnSpeech(mutatedArgs);

					NetState ns = heard.NetState;

					if (ns != null)
					{
						mutp ??= Packet.Acquire(new UnicodeMessage(_serial, Body, type, hue, 3, _language, Name, mutatedText));

						ns.Send(mutp);
					}
				}
			}

			Packet.Release(regp);
			Packet.Release(mutp);

			if (onSpeech.Count > 1)
				onSpeech.Sort(LocationComparer.GetInstance(this));

			for (int i = 0; i < onSpeech.Count; ++i)
			{
				IEntity obj = onSpeech[i];

				if (obj is Mobile heard)
				{
					if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
						heard.OnSpeech(regArgs);
					else
						heard.OnSpeech(mutatedArgs);
				}
				else
				{
					Item item = (Item)obj;

					item.OnSpeech(regArgs);
				}
			}

			if (m_Hears.Count > 0)
				m_Hears.Clear();

			if (m_OnSpeech.Count > 0)
				m_OnSpeech.Clear();
		}
	}

	public static Mobile GetDamagerFrom(DamageEntry de)
	{
		return de?.Damager;
	}

	public Mobile FindMostRecentDamager(bool allowSelf)
	{
		return GetDamagerFrom(FindMostRecentDamageEntry(allowSelf));
	}

	public DamageEntry FindMostRecentDamageEntry(bool allowSelf)
	{
		for (var i = DamageEntries.Count - 1; i >= 0; --i)
		{
			if (i >= DamageEntries.Count)
			{
				continue;
			}

			DamageEntry de = DamageEntries[i];

			if (de.HasExpired)
			{
				DamageEntries.RemoveAt(i);
			}
			else if (allowSelf || de.Damager != this)
			{
				return de;
			}
		}

		return null;
	}

	public Mobile FindLeastRecentDamager(bool allowSelf)
	{
		return GetDamagerFrom(FindLeastRecentDamageEntry(allowSelf));
	}

	public DamageEntry FindLeastRecentDamageEntry(bool allowSelf)
	{
		for (var i = 0; i < DamageEntries.Count; ++i)
		{
			if (i < 0)
			{
				continue;
			}

			DamageEntry de = DamageEntries[i];

			if (de.HasExpired)
			{
				DamageEntries.RemoveAt(i);
				--i;
			}
			else if (allowSelf || de.Damager != this)
			{
				return de;
			}
		}

		return null;
	}

	public Mobile FindMostTotalDamager(bool allowSelf)
	{
		return GetDamagerFrom(FindMostTotalDamageEntry(allowSelf));
	}

	public DamageEntry FindMostTotalDamageEntry(bool allowSelf)
	{
		DamageEntry mostTotal = null;

		for (var i = DamageEntries.Count - 1; i >= 0; --i)
		{
			if (i >= DamageEntries.Count)
			{
				continue;
			}

			DamageEntry de = DamageEntries[i];

			if (de.HasExpired)
			{
				DamageEntries.RemoveAt(i);
			}
			else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven > mostTotal.DamageGiven))
			{
				mostTotal = de;
			}
		}

		return mostTotal;
	}

	public Mobile FindLeastTotalDamager(bool allowSelf)
	{
		return GetDamagerFrom(FindLeastTotalDamageEntry(allowSelf));
	}

	public DamageEntry FindLeastTotalDamageEntry(bool allowSelf)
	{
		DamageEntry mostTotal = null;

		for (var i = DamageEntries.Count - 1; i >= 0; --i)
		{
			if (i >= DamageEntries.Count)
			{
				continue;
			}

			DamageEntry de = DamageEntries[i];

			if (de.HasExpired)
			{
				DamageEntries.RemoveAt(i);
			}
			else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven < mostTotal.DamageGiven))
			{
				mostTotal = de;
			}
		}

		return mostTotal;
	}

	public DamageEntry FindDamageEntryFor(Mobile m)
	{
		for (var i = DamageEntries.Count - 1; i >= 0; --i)
		{
			if (i >= DamageEntries.Count)
			{
				continue;
			}

			DamageEntry de = DamageEntries[i];

			if (de.HasExpired)
			{
				DamageEntries.RemoveAt(i);
			}
			else if (de.Damager == m)
			{
				return de;
			}
		}

		return null;
	}

	public virtual Mobile GetDamageMaster(Mobile damagee) => null;

	public virtual DamageEntry RegisterDamage(int amount, Mobile from)
	{
		DamageEntry de = FindDamageEntryFor(from);

		if (de == null)
		{
			de = new DamageEntry(from);
		}

		de.DamageGiven += amount;
		de.LastDamage = DateTime.UtcNow;

		DamageEntries.Remove(de);
		DamageEntries.Add(de);

		Mobile master = from.GetDamageMaster(this);

		if (master != null)
		{
			var list = de.Responsible;

			if (list == null)
			{
				de.Responsible = list = new List<DamageEntry>();
			}

			DamageEntry resp = list.FirstOrDefault(check => check.Damager == master);

			if (resp == null)
			{
				list.Add(resp = new DamageEntry(master));
			}

			resp.DamageGiven += amount;
			resp.LastDamage = DateTime.UtcNow;
		}

		return de;
	}

	public virtual void Bleed(Mobile attacker, int damage)
	{
	}

	public virtual void Bleed(Mobile attacker, int damage, Item blooditem)
	{
		if (damage <= 2)
		{
			return;
		}

		Direction d = GetDirectionTo(attacker);

		var maxCount = damage / 15;

		maxCount = maxCount switch
		{
			< 1 => 1,
			> 4 => 4,
			_ => maxCount
		};

		for (var i = 0; i < Utility.Random(1, maxCount); ++i)
		{
			var x = X;
			var y = Y;

			switch (d)
			{
				case Direction.North:
					x += Utility.Random(-1, 3);
					y += Utility.Random(2);
					break;
				case Direction.East:
					y += Utility.Random(-1, 3);
					x += Utility.Random(-1, 2);
					break;
				case Direction.West:
					y += Utility.Random(-1, 3);
					x += Utility.Random(2);
					break;
				case Direction.South:
					x += Utility.Random(-1, 3);
					y += Utility.Random(-1, 2);
					break;
				case Direction.Up:
					x += Utility.Random(2);
					y += Utility.Random(2);
					break;
				case Direction.Down:
					x += Utility.Random(-1, 2);
					y += Utility.Random(-1, 2);
					break;
				case Direction.Left:
					x += Utility.Random(2);
					y += Utility.Random(-1, 2);
					break;
				case Direction.Right:
					x += Utility.Random(-1, 2);
					y += Utility.Random(2);
					break;
			}

			blooditem.MoveToWorld(new Point3D(x, y, Z), Map);
		}
	}

	/// <summary>
	/// Overridable. Get hit block chance
	/// </summary>
	/// <returns></returns>
	public virtual double GetHitBlockChance() => 0;

	/// <summary>
	/// Overridable. Get spell damage bonus of the mobile
	/// </summary>
	/// <returns></returns>
	public virtual int GetSpellDamageBonus(bool inPvP) => 0;

	/// <summary>
	/// Overridable. Get spell cast speed bonus
	/// </summary>
	/// <returns></returns>
	public virtual int GetSpellCastSpeedBonus(SkillName castSkill) => 0;

	/// <summary>
	/// Overridable. Get damage bonus of the mobile
	/// </summary>
	/// <returns></returns>
	public virtual int GetDamageBonus() => 0;

	/// <summary>
	/// Overridable. Get damage bonus of attack speed
	/// </summary>
	/// <returns></returns>
	public virtual int GetAttackSpeedBonus() => 0;

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile is <see cref="Damage">damaged</see>. It is called before <see cref="Hits">hit points</see> are lowered or the Mobile is <see cref="Kill">killed</see>.
	/// <seealso cref="Damage" />
	/// <seealso cref="Hits" />
	/// <seealso cref="Kill" />
	/// </summary>
	public virtual void OnDamage(int amount, Mobile from, bool willKill)
	{
		EventSink.InvokeOnMobileDamage(this, amount, from, willKill);
	}

	public virtual bool CanBeDamaged() => !_blessed;
	public virtual int Damage(int amount) => Damage(amount, null);
	public virtual int Damage(int amount, Mobile from) => Damage(amount, from, true);
	public virtual int Damage(int amount, Mobile from, bool informMount) => Damage(amount, from, informMount, true);

	public virtual int Damage(int amount, Mobile from, bool informMount, bool checkDisrupt)
	{
		if (!CanBeDamaged() || Deleted)
		{
			return 0;
		}

		if (!Region.OnDamage(this, ref amount))
		{
			return 0;
		}

		if (amount > 0)
		{
			var oldHits = Hits;
			var newHits = oldHits - amount;

			if (checkDisrupt && Spell != null)
			{
				Spell.OnCasterHurt();
			}

			if (from != null)
			{
				RegisterDamage(amount, from);
			}

			DisruptiveAction();

			Paralyzed = false;

			SendDamagePacket(from, amount);
			OnDamage(amount, from, newHits < 0);

			IMount m = Mount;

			if (m != null && informMount)
			{
				{
					var temp = amount;
					m.OnRiderDamaged(from, ref amount, newHits < 0);
					if (temp > amount)
					{
						var absorbed = temp - amount;
						newHits += absorbed;
					}
				}
			}

			SendDamagePacket(from, amount);
			OnDamage(amount, from, newHits < 0);

			if (newHits < 0)
			{
				LastKiller = from;

				Hits = 0;

				if (oldHits >= 0)
				{
					Kill();
				}
			}
			else
			{
				FatigueHandler(this, amount, Dfa);

				Hits = newHits;
			}
		}

		return amount;
	}

	public virtual void SendDamagePacket(Mobile from, int amount)
	{
		switch (VisibleDamageType)
		{
			case VisibleDamageType.Related:
			{
				SendVisibleDamageRelated(from, amount);
				break;
			}
			case VisibleDamageType.Everyone:
			{
				SendVisibleDamageEveryone(amount);
				break;
			}
			case VisibleDamageType.Selective:
			{
				SendVisibleDamageSelective(from, amount);
				break;
			}
		}
	}

	public void SendVisibleDamageRelated(Mobile from, int amount)
	{
		NetState ourState = _netState, theirState = from?._netState;

		if (ourState == null)
		{
			Mobile master = GetDamageMaster(from);

			if (master != null)
				ourState = master._netState;
		}

		if (theirState == null && from != null)
		{
			Mobile master = from.GetDamageMaster(this);

			if (master != null)
				theirState = master._netState;
		}

		if (amount > 0 && (ourState != null || theirState != null))
		{
			Packet p = null;// = new DamagePacket( this, amount );

			if (ourState != null)
			{
				if (ourState.DamagePacket)
					p = Packet.Acquire(new DamagePacket(this, amount));
				else
					p = Packet.Acquire(new DamagePacketOld(this, amount));

				ourState.Send(p);
			}

			if (theirState != null && theirState != ourState)
			{
				bool newPacket = theirState.DamagePacket;

				if (newPacket && (p == null || p is not DamagePacket))
				{
					Packet.Release(p);
					p = Packet.Acquire(new DamagePacket(this, amount));
				}
				else if (!newPacket && (p == null || p is not DamagePacketOld))
				{
					Packet.Release(p);
					p = Packet.Acquire(new DamagePacketOld(this, amount));
				}

				theirState.Send(p);
			}

			Packet.Release(p);
		}
	}

	public void SendVisibleDamageEveryone(int amount)
	{
		if (amount < 0)
		{
			return;
		}

		Map map = _mMap;

		if (map == null)
		{
			return;
		}

		var eable = map.GetClientsInRange(_mLocation);

		Packet pNew = null;
		Packet pOld = null;

		foreach (NetState ns in eable)
		{
			if (ns.Mobile.CanSee(this))
			{
				if (ns.DamagePacket)
				{
					pNew ??= Packet.Acquire(new DamagePacket(this, amount));

					ns.Send(pNew);
				}
				else
				{
					pOld ??= Packet.Acquire(new DamagePacketOld(this, amount));

					ns.Send(pOld);
				}
			}
		}

		Packet.Release(pNew);
		Packet.Release(pOld);

		eable.Free();
	}

	public void SendVisibleDamageSelective(Mobile from, int amount)
	{
		NetState ourState = _netState, theirState = from?._netState;

		Mobile damager = from;
		Mobile damaged = this;

		if (ourState == null)
		{
			Mobile master = GetDamageMaster(from);

			if (master != null)
			{
				damaged = master;
				ourState = master._netState;
			}
		}

		if (theirState == null && from != null)
		{
			Mobile master = from.GetDamageMaster(this);

			if (master != null)
			{
				if (!damaged.ShowVisibleDamage)
					damager = master;
				else
					theirState = master._netState;
			}
		}

		if (amount > 0 && (ourState != null || theirState != null))
		{
			if (damaged.CanSeeVisibleDamage && ourState != null)
			{
				if (ourState.DamagePacket)
					ourState.Send(new DamagePacket(this, amount));
				else
					ourState.Send(new DamagePacketOld(this, amount));
			}

			if (theirState != null && theirState != ourState && damager.CanSeeVisibleDamage)
			{
				if (theirState.DamagePacket)
					theirState.Send(new DamagePacket(this, amount));
				else
					theirState.Send(new DamagePacketOld(this, amount));
			}
		}
	}

	public void Heal(int amount)
	{
		Heal(amount, this, true);
	}

	public void Heal(int amount, Mobile from)
	{
		Heal(amount, from, true);
	}

	public void Heal(int amount, Mobile from, bool message)
	{
		if (!Alive || IsDeadBondedPet)
			return;

		if (!Region.OnHeal(this, ref amount))
			return;

		OnHeal(ref amount, from);

		if (Hits + amount > HitsMax)
		{
			amount = HitsMax - Hits;
		}

		EventSink.InvokeOnMobileHeal(this, from, amount);

		Hits += amount;

		if (message && amount > 0 && _netState != null)
			_netState.Send(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Label, 0x3B2, 3, 1008158, "", AffixType.Append | AffixType.System, amount.ToString(), ""));
	}

	public virtual void OnHeal(ref int amount, Mobile from)
	{
	}

	public virtual void Deserialize(GenericReader reader)
	{
		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				_deaths = reader.ReadInt();
				_mBloodHue = reader.ReadInt();
				StrCap = reader.ReadInt();
				DexCap = reader.ReadInt();
				IntCap = reader.ReadInt();
				StrMaxCap = reader.ReadInt();
				DexMaxCap = reader.ReadInt();
				IntMaxCap = reader.ReadInt();
				_mIgnoreMobiles = reader.ReadBool();
				GuardImmune = reader.ReadBool();
				LastStrGain = reader.ReadDeltaTime();
				LastIntGain = reader.ReadDeltaTime();
				LastDexGain = reader.ReadDeltaTime();

				byte hairflag = reader.ReadByte();

				if ((hairflag & 0x01) != 0)
					_hair = new HairInfo(reader);
				if ((hairflag & 0x02) != 0)
					_facialHair = new FacialHairInfo(reader);
				if ((hairflag & 0x04) != 0)
				{
					_face = new FaceInfo(reader);
				}

				_race = reader.ReadRace();

				_tithingPoints = reader.ReadInt();

				Corpse = reader.ReadItem() as Container;

				CreationTime = reader.ReadDateTime();

				Stabled = reader.ReadStrongMobileList();

				CantWalk = reader.ReadBool();

				_virtues = new VirtueInfo(reader);

				Thirst = reader.ReadInt();
				Bac = reader.ReadInt();

				_shortTermMurders = reader.ReadInt();

				_followersMax = reader.ReadInt();

				MagicDamageAbsorb = reader.ReadInt();

				GuildFealty = reader.ReadMobile();

				_mGuild = reader.ReadGuild();
				_displayGuildAbbr = reader.ReadBool();
				_displayGuildTitle = reader.ReadBool();

				CanSwim = reader.ReadBool();

				Squelched = reader.ReadBool();

				_mHolding = reader.ReadItem();

				_virtualArmor = reader.ReadInt();

				BaseSoundId = reader.ReadInt();

				DisarmReady = reader.ReadBool();
				StunReady = reader.ReadBool();

				_statCap = reader.ReadInt();

				NameHue = reader.ReadInt();

				_hunger = reader.ReadInt();

				_mLocation = reader.ReadPoint3D();
				_mBody = new Body(reader.ReadInt());
				_mName = reader.ReadString();
				_mGuildTitle = reader.ReadString();
				_mCriminal = reader.ReadBool();
				_kills = reader.ReadInt();
				SpeechHue = reader.ReadInt();
				EmoteHue = reader.ReadInt();
				WhisperHue = reader.ReadInt();
				YellHue = reader.ReadInt();
				_language = reader.ReadString();
				_female = reader.ReadBool();
				_warmode = reader.ReadBool();
				_hidden = reader.ReadBool();
				_mDirection = (Direction)reader.ReadByte();
				_mHue = reader.ReadInt();
				_str = reader.ReadInt();
				_dex = reader.ReadInt();
				_int = reader.ReadInt();
				_hits = reader.ReadInt();
				_stam = reader.ReadInt();
				_mana = reader.ReadInt();
				_mMap = reader.ReadMap();
				_blessed = reader.ReadBool();
				_fame = reader.ReadInt();
				_karma = reader.ReadInt();
				_accessLevel = (AccessLevel)reader.ReadByte();

				_skills = new Skills(this, reader);

				Items = reader.ReadStrongItemList();

				_player = reader.ReadBool();
				_title = reader.ReadString();
				Profile = reader.ReadString();
				ProfileLocked = reader.ReadBool();

				AutoPageNotify = reader.ReadBool();

				LogoutLocation = reader.ReadPoint3D();
				LogoutMap = reader.ReadMap();

				_mStrLock = (StatLockType)reader.ReadByte();
				_mDexLock = (StatLockType)reader.ReadByte();
				_mIntLock = (StatLockType)reader.ReadByte();

				StatMods = new List<StatMod>();
				SkillMods = new List<SkillMod>();

				if (_player && _mMap != Map.Internal)
				{
					LogoutLocation = _mLocation;
					LogoutMap = _mMap;

					_mMap = Map.Internal;
				}

				_mMap?.OnEnter(this);

				if (_mCriminal)
				{
					_expireCriminal ??= new ExpireCriminalTimer(this);

					_expireCriminal.Start();
				}

				if (ShouldCheckStatTimers)
					CheckStatTimers();

				if (!_player && _dex <= 100 && _combatTimer != null)
					_combatTimer.Priority = TimerPriority.FiftyMs;
				else if (_combatTimer != null)
					_combatTimer.Priority = TimerPriority.EveryTick;

				UpdateRegion();

				UpdateResistances();

				break;
			}
		}

		if (!_player)
			Utility.Intern(ref _mName);

		Utility.Intern(ref _title);
		Utility.Intern(ref _language);
	}

	public void ConvertHair()
	{
		Item hair;

		if ((hair = FindItemOnLayer(Layer.Hair)) != null)
		{
			HairItemId = hair.ItemId;
			HairHue = hair.Hue;
			hair.Delete();
		}

		if ((hair = FindItemOnLayer(Layer.FacialHair)) != null)
		{
			FacialHairItemId = hair.ItemId;
			FacialHairHue = hair.Hue;
			hair.Delete();
		}
	}

	public virtual bool ShouldCheckStatTimers => true;

	public virtual void CheckStatTimers()
	{
		if (Deleted)
			return;

		if (Hits < HitsMax)
		{
			if (CanRegenHits)
			{
				_hitsTimer ??= new HitsTimer(this);

				_hitsTimer.Start();
			}
			else
			{
				_hitsTimer?.Stop();
			}
		}
		else
		{
			Hits = HitsMax;
		}

		if (Stam < StamMax)
		{
			if (CanRegenStam)
			{
				_stamTimer ??= new StamTimer(this);

				_stamTimer.Start();
			}
			else
			{
				_stamTimer?.Stop();
			}
		}
		else
		{
			Stam = StamMax;
		}

		if (Mana < ManaMax)
		{
			if (CanRegenMana)
			{
				_manaTimer ??= new ManaTimer(this);

				_manaTimer.Start();
			}
			else
			{
				_manaTimer?.Stop();
			}
		}
		else
		{
			Mana = ManaMax;
		}
	}

	public virtual void ResetStatTimers()
	{
		_hitsTimer?.Stop();

		if (CanRegenHits && Hits < HitsMax)
		{
			_hitsTimer ??= new HitsTimer(this);
			_hitsTimer.Start();
		}

		_stamTimer?.Stop();

		if (CanRegenStam && Stam < StamMax)
		{
			_stamTimer ??= new StamTimer(this);
			_stamTimer.Start();
		}

		_manaTimer?.Stop();

		if (CanRegenMana && Mana < ManaMax)
		{
			_manaTimer ??= new ManaTimer(this);
			_manaTimer.Start();
		}
	}

	public virtual void Serialize(GenericWriter writer)
	{
		writer.Write(0);

		writer.Write(_deaths);
		writer.Write(_mBloodHue);
		writer.Write(StrCap);
		writer.Write(DexCap);
		writer.Write(IntCap);
		writer.Write(StrMaxCap);
		writer.Write(DexMaxCap);
		writer.Write(IntMaxCap);
		writer.Write(_mIgnoreMobiles);
		writer.Write(GuardImmune);
		writer.WriteDeltaTime(LastStrGain);
		writer.WriteDeltaTime(LastIntGain);
		writer.WriteDeltaTime(LastDexGain);

		byte hairflag = 0x00;

		if (_hair != null)
			hairflag |= 0x01;
		if (_facialHair != null)
			hairflag |= 0x02;
		if (_face != null)
		{
			hairflag |= 0x04;
		}

		writer.Write(hairflag);

		if ((hairflag & 0x01) != 0)
			_hair.Serialize(writer);
		if ((hairflag & 0x02) != 0)
			_facialHair.Serialize(writer);
		if ((hairflag & 0x04) != 0)
		{
			if (_face != null)
			{
				_face.Serialize(writer);
			}
		}

		writer.Write(Race);

		writer.Write(_tithingPoints);

		writer.Write(Corpse);

		writer.Write(CreationTime);

		writer.Write(Stabled, true);

		writer.Write(CantWalk);

		VirtueInfo.Serialize(writer, _virtues);

		writer.Write(Thirst);
		writer.Write(Bac);

		writer.Write(_shortTermMurders);

		writer.Write(_followersMax);

		writer.Write(MagicDamageAbsorb);

		writer.Write(GuildFealty);

		writer.Write(_mGuild);
		writer.Write(_displayGuildAbbr);
		writer.Write(_displayGuildTitle);

		writer.Write(CanSwim);

		writer.Write(Squelched);

		writer.Write(_mHolding);

		writer.Write(_virtualArmor);

		writer.Write(BaseSoundId);

		writer.Write(DisarmReady);
		writer.Write(StunReady);

		writer.Write(_statCap);

		writer.Write(NameHue);

		writer.Write(_hunger);

		writer.Write(_mLocation);
		writer.Write(_mBody);
		writer.Write(_mName);
		writer.Write(_mGuildTitle);
		writer.Write(_mCriminal);
		writer.Write(_kills);
		writer.Write(SpeechHue);
		writer.Write(EmoteHue);
		writer.Write(WhisperHue);
		writer.Write(YellHue);
		writer.Write(_language);
		writer.Write(_female);
		writer.Write(_warmode);
		writer.Write(_hidden);
		writer.Write((byte)_mDirection);
		writer.Write(_mHue);
		writer.Write(_str);
		writer.Write(_dex);
		writer.Write(_int);
		writer.Write(_hits);
		writer.Write(_stam);
		writer.Write(_mana);

		writer.Write(_mMap);

		writer.Write(_blessed);
		writer.Write(_fame);
		writer.Write(_karma);
		writer.Write((byte)_accessLevel);
		_skills.Serialize(writer);

		writer.Write(Items);

		writer.Write(_player);
		writer.Write(_title);
		writer.Write(Profile);
		writer.Write(ProfileLocked);
		writer.Write(AutoPageNotify);

		writer.Write(LogoutLocation);
		writer.Write(LogoutMap);

		writer.Write((byte)_mStrLock);
		writer.Write((byte)_mDexLock);
		writer.Write((byte)_mIntLock);
	}

	private static readonly string[] m_AccessLevelNames = {
		"a player",
		"a VIP",
		"a counselor",
		"a decorator",
		"a Spawner",
		"a game master",
		"a seer",
		"an administrator",
		"a developer",
		"an owner"
	};

	public static string GetAccessLevelName(AccessLevel level) => m_AccessLevelNames[(int)level];

	public virtual bool CanPaperdollBeOpenedBy(Mobile from) => Body.IsHuman || Body.IsGhost || IsBodyMod;

	public virtual void GetChildContextMenuEntries(Mobile from, List<ContextMenuEntry> list, Item item)
	{
	}

	public virtual void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		if (Deleted)
			return;

		if (CanPaperdollBeOpenedBy(from))
			list.Add(new PaperdollEntry(this));

		if (from == this && Backpack != null && CanSee(Backpack) && CheckAlive(false))
			list.Add(new OpenBackpackEntry(this));
	}

	public void Internalize()
	{
		Map = Map.Internal;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="AddItem">added</see> from the Mobile, such as when it is equiped.
	/// <seealso cref="Items" />
	/// <seealso cref="OnItemRemoved" />
	/// </summary>
	public virtual void OnItemAdded(Item item)
	{
		EventSink.InvokeOnMobileItemEquip(this, item);
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="RemoveItem">removed</see> from the Mobile.
	/// <seealso cref="Items" />
	/// <seealso cref="OnItemAdded" />
	/// </summary>
	public virtual void OnItemRemoved(Item item)
	{
		EventSink.InvokeOnMobileItemRemoved(this, item);
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="item" /> is becomes a child of the Mobile; it's worn or contained at some level of the Mobile's <see cref="Mobile.Backpack">backpack</see> or <see cref="Mobile.BankBox">bank box</see>
	/// <seealso cref="OnSubItemRemoved" />
	/// <seealso cref="OnItemAdded" />
	/// </summary>
	public virtual void OnSubItemAdded(Item item)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="item" /> is removed from the Mobile, its <see cref="Mobile.Backpack">backpack</see>, or its <see cref="Mobile.BankBox">bank box</see>.
	/// <seealso cref="OnSubItemAdded" />
	/// <seealso cref="OnItemRemoved" />
	/// </summary>
	public virtual void OnSubItemRemoved(Item item)
	{
	}

	public virtual void OnItemBounceCleared(Item item)
	{
	}

	public virtual void OnSubItemBounceCleared(Item item)
	{
	}

	public virtual void OnItemObtained(Item item)
	{
		if (item != _backpack && item != _mBankBox)
		{
			EventSink.InvokeOnItemObtained(this, item);
		}
	}

	public void AddItem(Item item, int hue)
	{
		item.Hue = hue;
		AddItem(item);
	}

	public void AddItem(Item item)
	{
		if (item == null || item.Deleted)
			return;

		if (item.Parent == this)
			return;
		switch (item.Parent)
		{
			case Mobile mobile:
				mobile.RemoveItem(item);
				break;
			case Item parentItem:
				parentItem.RemoveItem(item);
				break;
			default:
				item.SendRemovePacket();
				break;
		}

		item.Parent = this;
		item.Map = _mMap;

		Items.Add(item);

		if (!item.IsVirtualItem)
		{
			UpdateTotal(item, TotalType.Gold, item.TotalGold);
			UpdateTotal(item, TotalType.Items, item.TotalItems + 1);
			UpdateTotal(item, TotalType.Weight, item.TotalWeight + item.PileWeight);
		}

		item.Delta(ItemDelta.Update);

		item.OnAdded(this);
		OnItemAdded(item);

		if (item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
		    item.PoisonResistance != 0 || item.EnergyResistance != 0)
			UpdateResistances();
	}

	public void RemoveItem(Item item)
	{
		if (item == null || Items == null)
			return;

		if (Items.Contains(item))
		{
			item.SendRemovePacket();

			//int oldCount = m_Items.Count;

			_ = Items.Remove(item);

			if (!item.IsVirtualItem)
			{
				UpdateTotal(item, TotalType.Gold, -item.TotalGold);
				UpdateTotal(item, TotalType.Items, -(item.TotalItems + 1));
				UpdateTotal(item, TotalType.Weight, -(item.TotalWeight + item.PileWeight));
			}

			item.Parent = null;

			item.OnRemoved(this);
			OnItemRemoved(item);

			if (item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
			    item.PoisonResistance != 0 || item.EnergyResistance != 0)
				UpdateResistances();
		}
	}

	public virtual void Animate(AnimationType type, int action)
	{
		Map map = _mMap;

		if (map != null)
		{
			ProcessDelta();

			Packet p = null;

			var eable = map.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this))
				{
					state.Mobile.ProcessDelta();

					p = Packet.Acquire(new NewMobileAnimation(this, type, action, Utility.Random(0, 60)));

					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public virtual void Animate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
	{
		Map map = _mMap;

		if (map != null)
		{
			ProcessDelta();

			Packet p = null;
			//Packet pNew = null;

			IPooledEnumerable<NetState> eable = map.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this))
				{
					state.Mobile.ProcessDelta();

					if (p == null)
					{
						#region SA
						if (Body.IsGargoyle)
						{
							frameCount = 10;

							if (Flying)
							{
								switch (action)
								{
									case >= 9 and <= 11:
										action = 71;
										break;
									case >= 12 and <= 14:
										action = 72;
										break;
									case 20:
										action = 77;
										break;
									case 31:
										action = 71;
										break;
									case 34:
										action = 78;
										break;
									case >= 200 and <= 259:
									case >= 260 and <= 270:
										action = 75;
										break;
								}
							}
							else
							{
								action = action switch
								{
									>= 200 and <= 259 => 17,
									>= 260 and <= 270 => 16,
									_ => action
								};
							}
						}
						#endregion

						p = Packet.Acquire(new MobileAnimation(this, action, frameCount, repeatCount, forward, repeat, delay));
					}

					state.Send(p);
					//}
				}
			}

			Packet.Release(p);
			//Packet.Release( pNew );

			eable.Free();
		}
	}

	public void SendSound(int soundId)
	{
		if (soundId != -1 && _netState != null)
			_ = Send(new PlaySound(soundId, this));
	}

	public void SendSound(int soundId, IPoint3D p)
	{
		if (soundId != -1 && _netState != null)
			_ = Send(new PlaySound(soundId, p));
	}

	public void PlaySound(int soundId)
	{
		if (soundId == -1)
			return;

		if (_mMap != null)
		{
			Packet p = Packet.Acquire(new PlaySound(soundId, this));

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public virtual void OnAccessLevelChanged(AccessLevel oldLevel)
	{
	}

	public virtual void OnFameChange(int oldValue)
	{
		EventSink.InvokeOnFameChange(this, oldValue, _fame);
	}

	public virtual void OnKarmaChange(int oldValue)
	{
		EventSink.InvokeOnKarmaChange(this, oldValue, _karma);
	}

	// Mobile did something which should unhide him
	public virtual void RevealingAction()
	{
		if (_hidden && IsPlayer())
			Hidden = false;

		IsStealthing = false;

		DisruptiveAction(); // Anything that unhides you will also distrupt meditation
	}

	#region Say/SayTo/Emote/Whisper/Yell
	public void SayTo(Mobile to, bool ascii, string text)
	{
		PrivateOverheadMessage(MessageType.Regular, SpeechHue, ascii, text, to.NetState);
	}

	public void SayTo(Mobile to, string text)
	{
		SayTo(to, false, text);
	}

	public void SayTo(Mobile to, string format, params object[] args)
	{
		SayTo(to, false, string.Format(format, args));
	}

	public void SayTo(Mobile to, bool ascii, string format, params object[] args)
	{
		SayTo(to, ascii, string.Format(format, args));
	}

	public void SayTo(Mobile to, int number)
	{
		_ = to.Send(new MessageLocalized(_serial, Body, MessageType.Regular, SpeechHue, 3, number, Name, ""));
	}

	public void SayTo(Mobile to, int number, string args)
	{
		_ = to.Send(new MessageLocalized(_serial, Body, MessageType.Regular, SpeechHue, 3, number, Name, args));
	}

	public void SayTo(Mobile to, int number, int hue)
	{
		PrivateOverheadMessage(MessageType.Regular, hue, number, to.NetState);
	}

	public void SayTo(Mobile to, int number, string args, int hue)
	{
		PrivateOverheadMessage(MessageType.Regular, hue, number, args, to.NetState);
	}

	public void SayTo(Mobile to, int hue, string text, string args)
	{
		SayTo(to, text, args, hue, false);
	}

	public void SayTo(Mobile to, int hue, string text, string args, bool ascii)
	{
		PrivateOverheadMessage(MessageType.Regular, hue, ascii, string.Format(text, args), to.NetState);
	}

	public void Say(bool ascii, string text)
	{
		PublicOverheadMessage(MessageType.Regular, SpeechHue, ascii, text);
	}

	public void Say(string text)
	{
		PublicOverheadMessage(MessageType.Regular, SpeechHue, false, text);
	}

	public void Say(string format, params object[] args)
	{
		Say(string.Format(format, args));
	}

	public void Say(int number, AffixType type, string affix, string args)
	{
		PublicOverheadMessage(MessageType.Regular, SpeechHue, number, type, affix, args);
	}

	public void Say(int number)
	{
		Say(number, "");
	}

	public void Say(int number, string args)
	{
		PublicOverheadMessage(MessageType.Regular, SpeechHue, number, args);
	}

	public void Emote(string text)
	{
		PublicOverheadMessage(MessageType.Emote, EmoteHue, false, text);
	}

	public void Emote(string format, params object[] args)
	{
		Emote(string.Format(format, args));
	}

	public void Emote(int number)
	{
		Emote(number, "");
	}

	public void Emote(int number, string args)
	{
		PublicOverheadMessage(MessageType.Emote, EmoteHue, number, args);
	}

	public void Whisper(string text)
	{
		PublicOverheadMessage(MessageType.Whisper, WhisperHue, false, text);
	}

	public void Whisper(string format, params object[] args)
	{
		Whisper(string.Format(format, args));
	}

	public void Whisper(int number)
	{
		Whisper(number, "");
	}

	public void Whisper(int number, string args)
	{
		PublicOverheadMessage(MessageType.Whisper, WhisperHue, number, args);
	}

	public void Yell(string text)
	{
		PublicOverheadMessage(MessageType.Yell, YellHue, false, text);
	}

	public void Yell(string format, params object[] args)
	{
		Yell(string.Format(format, args));
	}

	public void Yell(int number)
	{
		Yell(number, "");
	}

	public void Yell(int number, string args)
	{
		PublicOverheadMessage(MessageType.Yell, YellHue, number, args);
	}
	#endregion

	public void SendRemovePacket()
	{
		SendRemovePacket(true);
	}

	public void SendRemovePacket(bool everyone)
	{
		if (_mMap != null)
		{
			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state != _netState && (everyone || !state.Mobile.CanSee(this)))
				{
					state.Mobile.RemoveLos(this);
					state.Send(RemovePacket);
				}
			}

			eable.Free();
		}
	}

	public void ClearScreen()
	{
		ClearScreen(0, Map.GlobalRadarRange);
	}

	public void ClearScreen(int minRange, int maxRange)
	{
		if (_mMap != null && _mMap != Map.Internal)
		{
			Utility.FixRange(ref minRange, ref maxRange);

			var ns = _netState;

			if (ns != null)
			{
				var eable = _mMap.GetObjectsInRange(_mLocation, maxRange);

				foreach (var o in eable)
				{
					if (minRange > 0 && InRange(o, minRange))
					{
						continue;
					}

					// XXX LOS BEGIN
					RemoveLos(o);
					// XXX LOS END

					if (o is Mobile m)
					{
						if (m != this && InUpdateRange(m))
						{
							ns.Send(m.RemovePacket);
						}
					}
					else if (o is Item item && InUpdateRange(item))
					{
						ns.Send(item.RemovePacket);
					}
				}

				eable.Free();
			}
		}
	}

	/*public void ClearScreen()
	{
		NetState ns = m_NetState;

		if (_mMap != null && ns != null)
		{
			IPooledEnumerable<IEntity> eable = _mMap.GetObjectsInRange(_mLocation, Map.GlobalMaxUpdateRange);

			foreach (IEntity o in eable)
			{
				// XXX LOS BEGIN
				RemoveLos(o);
				// XXX LOS END
				if (o is Mobile m)
				{
					if (m != this && m.InUpdateRange(_mLocation, m._mLocation))
						ns.Send(m.RemovePacket);
				}
				else if (o is Item item)
				{
					if (InRange(item.Location, item.GetUpdateRange(this)))
						ns.Send(item.RemovePacket);
				}
			}

			eable.Free();
		}
	}*/

	public virtual bool SendSpeedControl(SpeedControlType type)
	{
		return Send(new SpeedControl(type));
	}

	public bool Send(Packet p)
	{
		return Send(p, false);
	}

	public bool Send(Packet p, bool throwOnOffline)
	{
		if (_netState != null)
		{
			_netState.Send(p);
			return true;
		}

		if (throwOnOffline)
		{
			throw new MobileNotConnectedException(this, "Packet could not be sent.");
		}
		return false;
	}

	#region Gumps/Menus

	public bool SendHuePicker(HuePicker p)
	{
		return SendHuePicker(p, false);
	}

	public bool SendHuePicker(HuePicker p, bool throwOnOffline)
	{
		if (_netState != null)
		{
			p.SendTo(_netState);
			return true;
		}

		if (throwOnOffline)
		{
			throw new MobileNotConnectedException(this, "Hue picker could not be sent.");
		}

		return false;
	}

	public Gump FindGump(Type type)
	{
		NetState ns = _netState;

		return ns?.Gumps.FirstOrDefault(gump => type.IsAssignableFrom(gump.GetType()));
	}

	public TGump FindGump<TGump>() where TGump : Gump
	{
		return FindGump(typeof(TGump)) as TGump;
	}

	public bool CloseGump(Type type)
	{
		if (_netState != null)
		{
			Gump gump = FindGump(type);

			if (gump != null)
			{
				_netState.Send(new CloseGump(gump.TypeId, 0));

				_netState.RemoveGump(gump);

				gump.OnServerClose(_netState);
			}

			return true;
		}

		return false;
	}

	//[Obsolete("Use CloseGump( Type ) instead.")]
	//public bool CloseGump(Type type, int buttonId)
	//{
	//	return CloseGump(type);
	//}

	//[Obsolete("Use CloseGump( Type ) instead.")]
	//public bool CloseGump(Type type, int buttonId, bool throwOnOffline)
	//{
	//	return CloseGump(type);
	//}

	public bool CloseAllGumps()
	{
		NetState ns = _netState;

		if (ns != null)
		{
			List<Gump> gumps = new(ns.Gumps);

			ns.ClearGumps();

			foreach (Gump gump in gumps)
			{
				ns.Send(new CloseGump(gump.TypeId, 0));

				gump.OnServerClose(ns);
			}

			return true;
		}

		return false;
	}

	//[Obsolete("Use CloseAllGumps() instead.", false)]
	//public bool CloseAllGumps(bool throwOnOffline)
	//{
	//	return CloseAllGumps();
	//}

	public bool HasGump(Type type)
	{
		return FindGump(type) != null;
	}

	//[Obsolete("Use HasGump( Type ) instead.", false)]
	//public bool HasGump(Type type, bool throwOnOffline)
	//{
	//	return HasGump(type);
	//}

	public bool SendGump(Gump g)
	{
		return SendGump(g, false);
	}

	public bool SendGump(Gump g, bool throwOnOffline)
	{
		if (_netState != null)
		{
			g.SendTo(_netState);
			return true;
		}

		if (throwOnOffline)
		{
			throw new MobileNotConnectedException(this, "Gump could not be sent.");
		}

		return false;
	}

	public bool SendMenu(IMenu m)
	{
		return SendMenu(m, false);
	}

	public bool SendMenu(IMenu m, bool throwOnOffline)
	{
		if (_netState != null)
		{
			m.SendTo(_netState);
			return true;
		}
		else if (throwOnOffline)
		{
			throw new MobileNotConnectedException(this, "Menu could not be sent.");
		}
		else
		{
			return false;
		}
	}

	#endregion

	/// <summary>
	/// Overridable. Event invoked before the Mobile says something.
	/// <seealso cref="DoSpeech" />
	/// </summary>
	public virtual void OnSaid(SpeechEventArgs e)
	{
		if (Squelched)
		{
			if (Core.ML)
				SendLocalizedMessage(500168); // You can not say anything, you have been muted.
			else
				SendMessage("You can not say anything, you have been squelched.");

			e.Blocked = true;
		}

		if (!e.Blocked)
			RevealingAction();
	}

	public virtual bool HandlesOnSpeech(Mobile from) => false;

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile hears speech. This event will only be invoked if <see cref="HandlesOnSpeech" /> returns true.
	/// <seealso cref="DoSpeech" />
	/// </summary>
	public virtual void OnSpeech(SpeechEventArgs e)
	{
	}

	public void SendEverything()
	{
		SendEverything(0, Map.GlobalRadarRange);
	}

	public void SendEverything(int minRange, int maxRange)
	{
		if (_mMap != null && _mMap != Map.Internal)
		{
			Utility.FixRange(ref minRange, ref maxRange);

			var ns = _netState;

			if (ns != null)
			{
				var eable = _mMap.GetObjectsInRange(_mLocation, maxRange);

				foreach (var o in eable)
				{
					if (minRange > 0 && InRange(o, minRange))
					{
						continue;
					}

					if (o is Item item)
					{
						if (CanSee(item) && InUpdateRange(item))
						{
							item.SendInfoTo(ns);
						}
					}
					else if (o is Mobile m)
					{
						if (m == this || (CanSee(m) && InUpdateRange(m)))
						{
							MobileIncoming.Send(ns, m);

							if (ns.IsEnhancedClient)
							{
								ns.Send(new HealthbarPoisonEC(m));
								ns.Send(new HealthbarYellowEC(m));
							}
							else if (ns.StygianAbyss)
							{
								ns.Send(new HealthbarPoison(m));
								ns.Send(new HealthbarYellow(m));
							}

							if (m.IsDeadBondedPet)
							{
								ns.Send(new BondedStatus(0, m._serial, 1));
							}

							if (ViewOpl)
							{
								ns.Send(m.OplPacket);
							}
						}
					}
				}

				eable.Free();
			}
		}
	}
	/*
	public void SendEverything()
	{
		NetState ns = m_NetState;

		if (_mMap != null && ns != null)
		{
			IPooledEnumerable<IEntity> eable = _mMap.GetObjectsInRange(_mLocation, Map.GlobalMaxUpdateRange);

			foreach (IEntity o in eable)
			{
				if (o is Item item)
				{
					if (CanSee(item) && InRange(item.Location, item.GetUpdateRange(this)))
						item.SendInfoTo(ns);
				}
				else if (o is Mobile m)
				{
					if (CanSee(m) && m.InUpdateRange(_mLocation, m._mLocation))
					{
						ns.Send(MobileIncoming.Create(ns, this, m));

						if (ns.StygianAbyss)
						{
							if (m.Poisoned)
								ns.Send(new HealthbarPoison(m));

							if (m.Blessed || m.YellowHealthbar)
								ns.Send(new HealthbarYellow(m));
						}

						if (m.IsDeadBondedPet)
							ns.Send(new BondedStatus(0, m.m_Serial, 1));

						if (ObjectPropertyList.Enabled)
						{
							ns.Send(m.OplPacket);

							//foreach ( Item item in m.m_Items )
							//	ns.Send( item.OPLPacket );
						}
					}
				}
			}

			eable.Free();
		}
	}*/

	public virtual void OnUpdateRangeChanged(int oldRange, int newRange)
	{
		if (oldRange > newRange)
		{
			ClearScreen(newRange, oldRange);
			SendEverything(0, newRange);
		}
		else if (oldRange < newRange)
		{
			SendEverything(oldRange, newRange);
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public Map Map
	{
		get => _mMap;
		set
		{
			if (Deleted)
				return;

			if (_mMap != value)
			{
				_netState?.ValidateAllTrades();

				Map oldMap = _mMap;

				if (_mMap != null)
				{
					_mMap.OnLeave(this);

					ClearScreen();
					SendRemovePacket();
				}

				for (var i = 0; i < Items.Count; ++i)
					Items[i].Map = value;

				_mMap = value;

				UpdateRegion();

				_mMap?.OnEnter(this);

				/*NetState ns = m_NetState;

				if (ns != null && _mMap != null)
				{
					ns.Sequence = 0;
					ns.Send(new MapChange(this));
					ns.Send(new MapPatches());
					ns.Send(SeasonChange.Instantiate(GetSeason(), true));

					if (ns.StygianAbyss)
						ns.Send(new MobileUpdate(this));
					else
						ns.Send(new MobileUpdateOld(this));

					ClearFastwalkStack();
				}

				if (ns != null)
				{
					if (_mMap != null)
						ns.Send(new ServerChange(this, _mMap));

					ns.Sequence = 0;
					ClearFastwalkStack();

					ns.Send(MobileIncoming.Create(ns, this, this));

					if (ns.StygianAbyss)
					{
						ns.Send(new MobileUpdate(this));
						CheckLightLevels(true);
						ns.Send(new MobileUpdate(this));
					}
					else
					{
						ns.Send(new MobileUpdateOld(this));
						CheckLightLevels(true);
						ns.Send(new MobileUpdateOld(this));
					}
				}

				SendEverything();
				SendIncomingPacket();

				if (ns != null)
				{
					ns.Sequence = 0;
					ClearFastwalkStack();

					ns.Send(MobileIncoming.Create(ns, this, this));

					if (ns.StygianAbyss)
					{
						ns.Send(SupportedFeatures.Instantiate(ns));
						ns.Send(new MobileUpdate(this));
						ns.Send(new MobileAttributes(this));
					}
					else
					{
						ns.Send(SupportedFeatures.Instantiate(ns));
						ns.Send(new MobileUpdateOld(this));
						ns.Send(new MobileAttributes(this));
					}
				}*/
				SendMapUpdates(true, true);

				OnMapChange(oldMap);
			}
		}
	}

	public void UpdateRegion()
	{
		if (Deleted)
			return;

		Region newRegion = Region.Find(_mLocation, _mMap);

		if (newRegion != _region)
		{
			Region.OnRegionChange(this, _region, newRegion);

			_region = newRegion;
			OnRegionChange(_region, newRegion);
		}
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <see cref="Map" /> changes.
	/// </summary>
	protected virtual void OnMapChange(Map oldMap)
	{
	}

	#region Beneficial Checks/Actions

	public virtual bool IsInBeneficialZone()
	{
		if (Region != null && !Region.Rules.HasFlag(ZoneRules.BeneficialRestrictions))
			return true;

		return Map != null && !Map.Rules.HasFlag(ZoneRules.BeneficialRestrictions);
	}

	public virtual bool CanBeBeneficial(Mobile target) => CanBeBeneficial(target, true, false);

	public virtual bool CanBeBeneficial(Mobile target, bool message) => CanBeBeneficial(target, message, false);

	public virtual bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
	{
		if (target == null)
			return false;

		if (Deleted || target.Deleted || !Alive || IsDeadBondedPet || (!allowDead && (!target.Alive || target.IsDeadBondedPet)))
		{
			if (message)
				SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.

			return false;
		}

		if (target == this)
			return true;

		if ( /*m_Player &&*/ !Region.AllowBeneficial(this, target))
		{
			// TODO: Pets
			//if ( !(target.m_Player || target.Body.IsHuman || target.Body.IsAnimal) )
			//{
			if (message)
				SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.

			return false;
			//}
		}

		return true;
	}

	public virtual bool IsBeneficialCriminal(Mobile target)
	{
		if (this == target)
			return false;

		var n = Notoriety.Compute(this, target);

		return n == Notoriety.Criminal || n == Notoriety.Murderer;
	}

	/// <summary>
	/// Overridable. Event invoked when the Mobile <see cref="DoBeneficial">does a beneficial action</see>.
	/// </summary>
	public virtual void OnBeneficialAction(Mobile target, bool isCriminal)
	{
		if (isCriminal)
			CriminalAction(false);
	}

	public virtual void DoBeneficial(Mobile target)
	{
		if (target == null)
			return;

		OnBeneficialAction(target, IsBeneficialCriminal(target));

		Region.OnBeneficialAction(this, target);
		target.Region.OnGotBeneficialAction(this, target);
	}

	public virtual bool BeneficialCheck(Mobile target)
	{
		if (CanBeBeneficial(target, true))
		{
			DoBeneficial(target);
			return true;
		}

		return false;
	}

	#endregion

	#region Harmful Checks/Actions
	public virtual bool IsInHarmfulZone()
	{
		if (Region != null && !Region.Rules.HasFlag(ZoneRules.HarmfulRestrictions))
			return true;

		return Map != null && !Map.Rules.HasFlag(ZoneRules.HarmfulRestrictions);
	}

	public virtual bool CanBeHarmful(IDamageable target) => CanBeHarmful(target, true);

	public virtual bool CanBeHarmful(IDamageable target, bool message) => CanBeHarmful(target, message, false);

	public virtual bool CanBeHarmful(IDamageable target, bool message, bool ignoreOurBlessedness) => CanBeHarmful(target, message, ignoreOurBlessedness, false);

	public virtual bool CanBeHarmful(IDamageable target, bool message, bool ignoreOurBlessedness, bool ignorePeaceCheck)
	{
		if (target == null)
		{
			return false;
		}

		if (Deleted || (!ignoreOurBlessedness && _blessed) || !Alive || IsDeadBondedPet || target.Deleted)
		{
			if (message)
			{
				SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
			}

			return false;
		}

		if (target is Mobile mobile)
		{
			if (mobile._blessed || !mobile.Alive || mobile.IsDeadBondedPet)
			{
				if (message)
				{
					SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
				}

				return false;
			}

			if (!mobile.CanBeHarmedBy(this, message))
			{
				return false;
			}
		}

		if (target == this)
		{
			return true;
		}

		// TODO: Pets
		if ( /*m_Player &&*/ !Region.AllowHarmful(this, target))
			//(target.m_Player || target.Body.IsHuman) && !Region.AllowHarmful( this, target )  )
		{
			if (message)
			{
				SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
			}

			return false;
		}

		return true;
	}

	public virtual bool CanBeHarmedBy(Mobile from, bool message) => true;

	public virtual bool IsHarmfulCriminal(IDamageable target)
	{
		if (this == target)
			return false;

		return Notoriety.Compute(this, target) == Notoriety.Innocent;
	}

	/// <summary>
	/// Overridable. Event invoked when the Mobile <see cref="DoHarmful">does a harmful action</see>.
	/// </summary>
	public virtual void OnHarmfulAction(IDamageable target, bool isCriminal)
	{
		if (isCriminal)
			CriminalAction(false);
	}

	public virtual void DoHarmful(IDamageable target)
	{
		DoHarmful(target, false);
	}

	public virtual void DoHarmful(IDamageable target, bool indirect)
	{
		if (target == null || Deleted)
			return;

		bool isCriminal = IsHarmfulCriminal(target);

		OnHarmfulAction(target, isCriminal);

		if (target is Mobile mobile)
			mobile.AggressiveAction(this, isCriminal);

		Region.OnDidHarmful(this, target);

		switch (target)
		{
			case Mobile mobile1:
				mobile1.Region.OnGotHarmful(this, target);
				break;
			case Item:
				Region.Find(target.Location, target.Map).OnGotHarmful(this, target);
				break;
		}

		if (!indirect)
			Combatant = target;

		if (_expireCombatant == null)
			_expireCombatant = new ExpireCombatantTimer(this);
		else
			_expireCombatant.Stop();

		_expireCombatant.Start();
	}

	public virtual bool HarmfulCheck(IDamageable target)
	{
		if (CanBeHarmful(target))
		{
			DoHarmful(target);
			return true;
		}

		return false;
	}

	#endregion

	#region Stats

	/// <summary>
	/// Gets a list of all <see cref="StatMod">StatMod's</see> currently active for the Mobile.
	/// </summary>
	public List<StatMod> StatMods { get; private set; }

	public bool RemoveStatMod(StatMod mod)
	{
		if (StatMods.Contains(mod))
		{
			_ = StatMods.Remove(mod);
			CheckStatTimers();
			Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
			return true;
		}
		return false;
	}

	public bool RemoveStatMod(string name)
	{
		StatMod statsMod = StatMods.FirstOrDefault(stat => stat.Name == name);
		if (statsMod != null)
		{
			_ = RemoveStatMod(statsMod);
		}

		return statsMod != null;
	}

	public StatMod GetStatMod(string name)
	{
		return StatMods.FirstOrDefault(stat => stat.Name == name);
	}

	public void AddStatMod(StatMod mod)
	{
		StatMod statsMod = StatMods.FirstOrDefault(stat => stat.Name == mod.Name);
		if (statsMod != null)
		{
			Delta(MobileDelta.Stat | GetStatDelta(statsMod.Type));
			_ = StatMods.Remove(statsMod);
		}

		StatMods.Add(mod);
		Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
		CheckStatTimers();
	}

	private static MobileDelta GetStatDelta(StatType type)
	{
		MobileDelta delta = 0;

		if ((type & StatType.Str) != 0)
			delta |= MobileDelta.Hits;

		if ((type & StatType.Dex) != 0)
			delta |= MobileDelta.Stam;

		if ((type & StatType.Int) != 0)
			delta |= MobileDelta.Mana;

		return delta;
	}

	/// <summary>
	/// Computes the total modified offset for the specified stat type. Expired <see cref="StatMod" /> instances are removed.
	/// </summary>
	public int GetStatOffset(StatType type)
	{
		int offset = 0;

		for (int i = 0; i < StatMods.Count; ++i)
		{
			StatMod mod = StatMods[i];

			if (mod.HasElapsed())
			{
				StatMods.RemoveAt(i);
				Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
				CheckStatTimers();

				--i;
			}
			else if ((mod.Type & type) != 0)
			{
				offset += mod.Offset;
			}
		}

		return offset;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the <see cref="RawStr" /> changes.
	/// <seealso cref="RawStr" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	public virtual void OnRawStrChange(int oldValue)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <see cref="RawDex" /> changes.
	/// <seealso cref="RawDex" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	public virtual void OnRawDexChange(int oldValue)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the <see cref="RawInt" /> changes.
	/// <seealso cref="RawInt" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	public virtual void OnRawIntChange(int oldValue)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the <see cref="RawStr" />, <see cref="RawDex" />, or <see cref="RawInt" /> changes.
	/// <seealso cref="OnRawStrChange" />
	/// <seealso cref="OnRawDexChange" />
	/// <seealso cref="OnRawIntChange" />
	/// </summary>
	public virtual void OnRawStatChange(StatType stat, int oldValue)
	{
	}

	/// <summary>
	/// Gets or sets the base, unmodified, strength of the Mobile. Ranges from 1 to 65000, inclusive.
	/// <seealso cref="Str" />
	/// <seealso cref="StatMod" />
	/// <seealso cref="OnRawStrChange" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int RawStr
	{
		get => _str;
		set
		{
			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			if (_str != value)
			{
				int oldValue = _str;

				_str = value;
				Delta(MobileDelta.Stat | MobileDelta.Hits);

				EventSink.InvokeOnStatGainChange(this, StatType.Str, oldValue, _str);

				if (Hits < HitsMax)
				{
					if (_hitsTimer == null)
						_hitsTimer = new HitsTimer(this);

					_hitsTimer.Start();
				}
				else if (Hits > HitsMax)
				{
					Hits = HitsMax;
				}

				OnRawStrChange(oldValue);
				OnRawStatChange(StatType.Str, oldValue);
			}
		}
	}

	/// <summary>
	/// Gets or sets the effective strength of the Mobile. This is the sum of the <see cref="RawStr" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
	/// <seealso cref="RawStr" />
	/// <seealso cref="StatMod" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Str
	{
		get
		{
			int value = _str + GetStatOffset(StatType.Str);

			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			return value;
		}
		set
		{
			if (StatMods.Count == 0)
				RawStr = value;
		}
	}

	/// <summary>
	/// Gets or sets the base, unmodified, dexterity of the Mobile. Ranges from 1 to 65000, inclusive.
	/// <seealso cref="Dex" />
	/// <seealso cref="StatMod" />
	/// <seealso cref="OnRawDexChange" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int RawDex
	{
		get => _dex;
		set
		{
			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			if (_dex != value)
			{
				int oldValue = _dex;

				_dex = value;
				Delta(MobileDelta.Stat | MobileDelta.Stam);

				EventSink.InvokeOnStatGainChange(this, StatType.Dex, oldValue, _dex);

				if (Stam < StamMax)
				{
					if (_stamTimer == null)
						_stamTimer = new StamTimer(this);

					_stamTimer.Start();
				}
				else if (Stam > StamMax)
				{
					Stam = StamMax;
				}

				OnRawDexChange(oldValue);
				OnRawStatChange(StatType.Dex, oldValue);
			}
		}
	}

	/// <summary>
	/// Gets or sets the effective dexterity of the Mobile. This is the sum of the <see cref="RawDex" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
	/// <seealso cref="RawDex" />
	/// <seealso cref="StatMod" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Dex
	{
		get
		{
			var value = _dex + GetStatOffset(StatType.Dex);

			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			return value;
		}
		set
		{
			if (StatMods.Count == 0)
				RawDex = value;
		}
	}

	/// <summary>
	/// Gets or sets the base, unmodified, intelligence of the Mobile. Ranges from 1 to 65000, inclusive.
	/// <seealso cref="Int" />
	/// <seealso cref="StatMod" />
	/// <seealso cref="OnRawIntChange" />
	/// <seealso cref="OnRawStatChange" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int RawInt
	{
		get => _int;
		set
		{
			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			if (_int != value)
			{
				var oldValue = _int;

				_int = value;
				Delta(MobileDelta.Stat | MobileDelta.Mana);

				EventSink.InvokeOnStatGainChange(this, StatType.Int, oldValue, _int);

				if (Mana < ManaMax)
				{
					_manaTimer ??= new ManaTimer(this);

					_manaTimer.Start();
				}
				else if (Mana > ManaMax)
				{
					Mana = ManaMax;
				}

				OnRawIntChange(oldValue);
				OnRawStatChange(StatType.Int, oldValue);
			}
		}
	}

	/// <summary>
	/// Gets or sets the effective intelligence of the Mobile. This is the sum of the <see cref="RawInt" /> plus any additional modifiers. Any attempts to set this value when under the influence of a <see cref="StatMod" /> will result in no change. It ranges from 1 to 65000, inclusive.
	/// <seealso cref="RawInt" />
	/// <seealso cref="StatMod" />
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Int
	{
		get
		{
			var value = _int + GetStatOffset(StatType.Int);

			if (value < 1)
				value = 1;
			else if (value > MaxStatValue)
				value = MaxStatValue;

			return value;
		}
		set
		{
			if (StatMods.Count == 0)
				RawInt = value;
		}
	}

	public virtual void OnHitsChange(int oldValue)
	{
	}

	public virtual void OnStamChange(int oldValue)
	{
	}

	public virtual void OnManaChange(int oldValue)
	{
	}

	/// <summary>
	/// Gets or sets the current hit point of the Mobile. This value ranges from 0 to <see cref="HitsMax" />, inclusive. When set to the value of <see cref="HitsMax" />, the <see cref="AggressorInfo.CanReportMurder">CanReportMurder</see> flag of all aggressors is reset to false, and the list of damage entries is cleared.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int Hits
	{
		get => _hits;
		set
		{
			if (Deleted)
				return;

			if (value < 0)
			{
				value = 0;
			}
			else if (value >= HitsMax)
			{
				value = HitsMax;

				_hitsTimer?.Stop();

				for (var i = 0; i < Aggressors.Count; i++) //reset reports on full HP
					Aggressors[i].CanReportMurder = false;

				if (DamageEntries.Count > 0)
					DamageEntries.Clear(); // reset damage entries on full HP
			}

			if (value < HitsMax)
			{
				if (CanRegenHits)
				{
					_hitsTimer ??= new HitsTimer(this);

					_hitsTimer.Start();
				}
				else
				{
					_hitsTimer?.Stop();
				}
			}

			if (_hits != value)
			{
				var oldValue = _hits;
				_hits = value;
				Delta(MobileDelta.Hits);
				OnHitsChange(oldValue);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitsPerc
	{
		get
		{
			int val = (int)(Hits / (double)HitsMax * 100);
			val = val switch
			{
				< 0 => 0,
				> 100 => 100,
				_ => val
			};

			return val;
		}
		set
		{
			value = value switch
			{
				< 0 => 0,
				> 100 => 100,
				_ => value
			};

			Hits = (int)((double)value / 100 * HitsMax);
		}
	}

	public int HitsMaxBonus; //non serialized bonus added to hits max (can be used for certain spells and stuff)
	public int HitsMaxPenalty;//same as above, but minused

	/// <summary>
	/// Overridable. Gets the maximum hit point of the Mobile. By default, this returns: <c>50 + (<see cref="Str" /> / 2)</c>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int HitsMax => 50 + Str / 2;

	/// <summary>
	/// Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="StamMax" />, inclusive.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int Stam
	{
		get => _stam;
		set
		{
			if (Deleted)
				return;

			if (value < 0)
			{
				value = 0;
			}
			else if (value >= StamMax)
			{
				value = StamMax;

				_stamTimer?.Stop();
			}

			if (value < StamMax)
			{
				if (CanRegenStam)
				{
					_stamTimer ??= new StamTimer(this);

					_stamTimer.Start();
				}
				else
				{
					_stamTimer?.Stop();
				}
			}

			if (_stam != value)
			{
				int oldValue = _stam;
				_stam = value;
				Delta(MobileDelta.Stam);
				OnStamChange(oldValue);
			}
		}
	}

	/// <summary>
	/// Overridable. Gets the maximum stamina of the Mobile. By default, this returns: <c><see cref="Dex" /></c>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int StamMax => Dex;

	/// <summary>
	/// Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="ManaMax" />, inclusive.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int Mana
	{
		get => _mana;
		set
		{
			if (Deleted)
				return;

			if (value < 0)
			{
				value = 0;
			}
			else if (value >= ManaMax)
			{
				value = ManaMax;

				_manaTimer?.Stop();

				if (Meditating)
				{
					Meditating = false;
					SendLocalizedMessage(501846); // You are at peace.
					OnFinishMeditation();
				}
			}

			if (value < ManaMax)
			{
				if (CanRegenMana)
				{
					_manaTimer ??= new ManaTimer(this);

					_manaTimer.Start();
				}
				else
				{
					_manaTimer?.Stop();
				}
			}

			if (_mana != value)
			{
				var oldValue = _mana;
				_mana = value;
				Delta(MobileDelta.Mana);
				OnManaChange(oldValue);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ManaPerc
	{
		get
		{
			int val = (int)(Mana / (double)ManaMax * 100);
			val = val switch
			{
				< 0 => 0,
				> 100 => 100,
				_ => val
			};

			return val;
		}
		set
		{
			value = value switch
			{
				< 0 => 0,
				> 100 => 100,
				_ => value
			};

			Mana = (int)((double)value / 100 * ManaMax);
		}
	}

	public int ManaMaxBonus; //non serialized bonus added to mana max (can be used for certain spells and stuff)
	public int ManaMaxPenalty; //same as above, but minused

	/// <summary>
	/// Overridable. Gets the maximum mana of the Mobile. By default, this returns: <c><see cref="Int" /></c>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ManaMax => Int;

	/// <summary>
	/// Overridable. Called when a mobile finish meditation.
	/// </summary>
	public virtual void OnFinishMeditation()
	{
	}

	#endregion

	//custom notoriety calculator for area spells or other harmful things that can hit an indirect target
	public virtual bool ShouldHarm(Mobile target)
	{
		if (target == null || target == this)
		{
			return false;
		}

		if (!target.Alive || target.IsDeadBondedPet || target.Blessed)
		{
			return false;
		}

		return target.AccessLevel == AccessLevel.Player;

		//this function should be called up in child classes before calling the child version, if its true we continue executing child version, otherwise return false - so we return true by default
	}

	public virtual int Luck => 0;

	public virtual int HuedItemID => _female ? 0x2107 : 0x2106;

	private int _hueMod = -1;

	[Hue, CommandProperty(AccessLevel.GameMaster)]
	public int HueMod
	{
		get => _hueMod;
		set
		{
			if (_hueMod != value)
			{
				_hueMod = value;

				Delta(MobileDelta.Hue);
			}
		}
	}

	[Hue, CommandProperty(AccessLevel.Decorator)]
	public virtual int Hue
	{
		get
		{
			if (_hueMod != -1)
			{
				return _hueMod;
			}

			return BodyHue;
		}
		set => BodyHue = value;
	}

	[Hue, CommandProperty(AccessLevel.Decorator)]
	public int BodyHue
	{
		get => _mHue;
		set
		{
			var oldHue = _mHue;

			if (oldHue != value)
			{
				_mHue = value;

				Delta(MobileDelta.Hue);
			}
		}
	}


	public void SetDirection(Direction dir)
	{
		_mDirection = dir;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Direction Direction
	{
		get => _mDirection;
		set
		{
			if (_mDirection != value)
			{
				_mDirection = value;

				Delta(MobileDelta.Direction);
				//ProcessDelta();
			}
		}
	}

	public virtual int GetSeason()
	{
		return _mMap?.Season ?? 1;
	}

	int IDamageable.ComputeNotoriety(Mobile viewer)
	{
		return GetNotoriety(viewer);
	}

	public virtual int GetNotoriety(Mobile beholder)
	{
		return Notoriety.Compute(beholder, this);
	}

	public virtual int GetPacketFlags()
	{
		var flags = 0x0;

		if (_paralyzed || _frozen)
			flags |= 0x01;

		if (_female)
			flags |= 0x02;

		if (_flying)
			flags |= 0x04;

		if (_blessed || _yellowHealthbar)
			flags |= 0x08;

		if (_warmode)
			flags |= 0x40;

		if (_hidden)
			flags |= 0x80;

		if (_mIgnoreMobiles)
		{
			flags |= 0x10;
		}

		return flags;
	}

	// Pre-7.0.0.0 Packet Flags
	public virtual int GetOldPacketFlags()
	{
		var flags = 0x0;

		if (_paralyzed || _frozen)
			flags |= 0x01;

		if (_female)
			flags |= 0x02;

		if (_mPoison != null)
			flags |= 0x04;

		if (_blessed || _yellowHealthbar)
			flags |= 0x08;

		if (_warmode)
			flags |= 0x40;

		if (_hidden)
			flags |= 0x80;

		if (_mIgnoreMobiles)
		{
			flags |= 0x10;
		}

		return flags;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Female
	{
		get => _female;
		set
		{
			if (_female != value)
			{
				_female = value;
				Delta(MobileDelta.Flags);
				OnGenderChanged(!_female);
			}
		}
	}

	public virtual void OnGenderChanged(bool oldFemale)
	{
		EventSink.InvokeOnGenderChange(this, oldFemale, _female);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Flying
	{
		get => _flying;
		set
		{
			if (_flying != value)
			{
				_flying = value;
				Delta(MobileDelta.Flags);
			}
		}
	}

	public virtual void ToggleFlying()
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool Warmode
	{
		get => _warmode;
		set
		{
			if (Deleted)
				return;

			if (_warmode != value)
			{
				if (_autoManifestTimer != null)
				{
					_autoManifestTimer.Stop();
					_autoManifestTimer = null;
				}

				_warmode = value;
				Delta(MobileDelta.Flags);

				if (_netState != null)
					_ = Send(SetWarMode.Instantiate(value));

				if (!_warmode)
					Combatant = null;

				if (!Alive)
				{
					if (value)
						Delta(MobileDelta.GhostUpdate);
					else
						SendRemovePacket(false);
				}

				OnWarmodeChanged();
			}
		}
	}

	/// <summary>
	/// Overridable. Virtual event invoked after the Warmode property has changed.
	/// </summary>
	public virtual void OnWarmodeChanged()
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool Hidden
	{
		get => _hidden;
		set
		{
			if (_hidden != value)
			{
				_hidden = value;
				//Delta( MobileDelta.Flags );

				OnHiddenChanged();
			}
		}
	}

	public virtual void OnHiddenChanged()
	{
		AllowedStealthSteps = 0;

		if (_mMap != null)
		{
			Packet p = null;//MORE TESTING
			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (!state.Mobile.CanSee(this))
				{
					p ??= RemovePacket;
					// XXX LOS BEGIN
					state.Mobile.RemoveLos(this);
					// XXX LOS END
					state.Send(RemovePacket);
				}
				else
				{
					MobileIncoming.Send(state, this);

					if (IsDeadBondedPet)
						state.Send(new BondedStatus(0, _serial, 1));

					if (ObjectPropertyList.Enabled)
					{
						state.Send(OplPacket);

						//foreach ( Item item in m_Items )
						//	state.Send( item.OPLPacket );
					}
				}
			}

			eable.Free();
		}
	}

	public virtual void OnConnected()
	{
	}

	public virtual void OnDisconnected()
	{
	}

	public virtual void OnNetStateChanged()
	{
	}

	[CommandProperty(AccessLevel.GameMaster, AccessLevel.Owner)]
	public NetState NetState
	{
		get
		{
			if (_netState is {Socket: null, IsDisposing: false})
			{
				NetState = null;
			}

			return _netState;
		}
		set
		{
			if (_netState != value)
			{
				_mMap?.OnClientChange(_netState, value, this);

				_target?.Cancel(this, TargetCancelType.Disconnected);

				if (_mQuestArrow != null)
					QuestArrow = null;

				_spell?.OnConnectionChanged();

				//if ( m_Spell != null )
				//	m_Spell.FinishSequence();

				_netState?.CancelAllTrades();

				BankBox box = FindBankNoCreate();

				if (box is {Opened: true})
					box.Close();

				// REMOVED:
				//m_Actions.Clear();

				_netState = value;

				if (_netState == null)
				{
					OnDisconnected();
					EventSink.InvokeDisconnected(this);

					// Disconnected, start the logout timer

					if (_logoutTimer == null)
						_logoutTimer = new LogoutTimer(this);
					else
						_logoutTimer.Stop();

					_logoutTimer.Delay = GetLogoutDelay();
					_logoutTimer.Start();
				}
				else
				{
					OnConnected();
					EventSink.InvokeConnected(this);

					// Connected, stop the logout timer and if needed, move to the world

					if (_logoutTimer != null)
						_logoutTimer.Stop();

					_logoutTimer = null;

					if (_mMap == Map.Internal && LogoutMap != null)
					{
						CharacterOut = true;
						Map = LogoutMap;
						Location = LogoutLocation;
					}
					else
					{
						CharacterOut = false;
					}
				}

				for (int i = Items.Count - 1; i >= 0; --i)
				{
					if (i >= Items.Count)
						continue;

					Item item = Items[i];

					if (item is SecureTradeContainer)
					{
						for (int j = item.Items.Count - 1; j >= 0; --j)
						{
							if (j < item.Items.Count)
							{
								item.Items[j].OnSecureTrade(this, this, this, false);
								_ = AddToBackpack(item.Items[j]);
							}
						}

						_ = Timer.DelayCall(TimeSpan.Zero, delegate { item.Delete(); });
					}
				}

				DropHolding();
				OnNetStateChanged();
			}
		}
	}

	public virtual bool CanSee(object o)
	{
		return o switch
		{
			Item item => CanSee(item),
			Mobile mob => CanSee(mob),
			_ => true
		};
	}

	public virtual bool CanSee(Item item)
	{
		bool canSee = true;

		if (Deleted || item.Deleted || _mMap == Map.Internal || item.Map == Map.Internal || _mMap != item.Map)
		{
			canSee = false;
		}
		else if (item.Parent != null)
		{
			canSee = item.Parent switch
			{
				Item item1 when !CanSee(item1) => false,
				Item => IsStaff() || item.Visible,
				Mobile mobile when !CanSee(mobile) => false,
				Mobile => IsStaff() || item.Visible,
				_ => true
			};
		}
		else if (item is BankBox)
		{
			if (item is BankBox box && IsPlayer() && (box.Owner != this || !box.Opened))
				canSee = false;
		}
		else if (item is SecureTradeContainer container)
		{
			SecureTrade trade = container.Trade;

			if (trade != null && trade.From.Mobile != this && trade.To.Mobile != this)
				canSee = false;
		}
		else if (!Utility.InRange(Location, item.Location, 15))
		{
			canSee = false;
		}
		else
		{
			canSee = IsStaff() || (item.Visible && CheckLos(item));
		}

		//Console.WriteLine("LOS: \"{0}\" sees \"{1}\", ID={2} ? {3}", this.Name, item.Name, item.ItemID, canSee );
		return canSee;

		/*if (m_Map == Map.Internal)
			return false;
		else if (item.Map == Map.Internal)
			return false;

		if (item.Parent != null)
		{
			if (item.Parent is Item parent)
			{
				if (!(CanSee(parent) && parent.IsChildVisibleTo(this, item)))
					return false;
			}
			else if (item.Parent is Mobile parentMob)
			{
				if (!CanSee(parentMob))
					return false;
			}
		}

		if (item is BankBox box)
		{
			if (box != null && m_AccessLevel <= AccessLevel.Counselor && (box.Owner != this || !box.Opened))
				return false;
		}
		else if (item is SecureTradeContainer secureTrade)
		{
			SecureTrade trade = secureTrade.Trade;

			if (trade != null && trade.From.Mobile != this && trade.To.Mobile != this)
				return false;
		}

		return !item.Deleted && item.Map == m_Map && (item.Visible || m_AccessLevel > AccessLevel.Counselor);*/
	}

	public virtual bool CanSee(Mobile m)
	{
		/*
		if (Deleted || m.Deleted || m_Map == Map.Internal || m.m_Map == Map.Internal)
			return false;

		return this == m || (
			m.m_Map == m_Map &&
			(!m.Hidden || (m_AccessLevel != AccessLevel.Player && (m_AccessLevel >= m.AccessLevel || m_AccessLevel >= AccessLevel.Administrator))) &&
			((m.Alive || (Core.SE && Skills.SpiritSpeak.Value >= 100.0)) || !Alive || m_AccessLevel > AccessLevel.Player || m.Warmode));
		*/
		//bool canSee = true; changed to the canSee without the true.
		bool canSee;
		if (this == m)
		{
			canSee = true;
		}
		else if (!Utility.InRange(Location, m.Location, 15) || Deleted || m.Deleted || _mMap == Map.Internal || m._mMap == Map.Internal || _mMap != m._mMap)
		{
			canSee = false;
		}
		else if (AccessLevel > AccessLevel.Player && (AccessLevel >= m.AccessLevel || AccessLevel >= AccessLevel.Administrator))
		{
			canSee = true;
		}
		else// hidden cant be seen regardless // account for ghosts and what not
			canSee = !m.Hidden &&(m.Alive || Core.SE && Skills.SpiritSpeak.Value >= 100.0 || !Alive || m.Warmode) && CheckLos(m);

		//Console.WriteLine("LOS: \"{0}\" sees \"{1}\" ? {2}", this.Name, m.Name, canSee );
		//if( AccessLevel = AccessLevel.Player || LOS.Config.GetInstance().LosForMobs )
		//if( canSee && addLos ) AddLos( m );

		return canSee;
	}

	//------------------------------------------------------------------------------
	//  CheckLos() -- this is where the real line of sight checking is actually
	//     done.
	//------------------------------------------------------------------------------
	// XXX LOS BEGIN
	public bool CheckLos(IEntity o)
	{
		//if( this.NetState == null )
		//Console.WriteLine( "Checking LOS for {0} --> {1}", this.Name, o is Mobile ? ((Mobile)o).Name : ((Item)o).Name );

		Point3D viewer = Location;
		Point3D target = o.Location;

		//if( o is Item ) Console.WriteLine( "Checking LOS for item {0},{1},{2} --> {3},{4},{5} \"{6}\"", 
		//    viewer.X, viewer.Y, viewer.Z, target.X, target.Y, target.Z, o.GetType().Name
		//    );

		//  CHECK NO UP IN CAN_SEE, No need here
		//
		// if ( !Utility.InRange( viewer, target, 15 ) )                          
		// {
		//     return false; // LOS stops resolving at a range of 15
		// }

		if (o is Item item)
		{
			//--------------------------------------------------------------
			//  Immovable items always visible
			//--------------------------------------------------------------

			if (!item.Movable)
			{
				if (LOS.Values.Corpse.IsInstanceOfType(item))
				{
					PropertyInfo info = LOS.Values.Corpse.GetProperty("Owner");

					if (info != null)
					{
						Mobile owner = (Mobile)info.GetValue(item, null);

						if (owner == this)
							return true;
					}
				}
				else
					return true;
			}

			//--------------------------------------------------------------
			//  Not lossed items always visible
			//--------------------------------------------------------------

			if (LOS.Config.GetInstance().NotLossed(item.ItemId | 0x4000))
			{
				return true;
			}

			//--------------------------------------------------------------
			//  All items are visible if we're not lossing them.
			//--------------------------------------------------------------

			if (!LOS.Config.GetInstance().Items)
			{
				return true;
			}
		}
		else if (o is Mobile)
		{
			if (Player)
			{
				if (!LOS.Config.GetInstance().Mobiles)
				{
					return true;  // if mobile is off, we can see all mobiles
				}
			}
			else
			{
				if (!LOS.Config.GetInstance().LosForMobs)
				{
					return true;  // if this is an npc mob, and los for npcs if off, the npc mob sees all
				}

				if (Utility.InRange(viewer, target, 7))
				{
					return true;
				}
			}
		}

		//------------------------------------------------------------------
		// if LOS is not on on this facet, don't use LOS
		//------------------------------------------------------------------

		if (!LOS.Config.GetInstance().FacetOn(Map.Name))
		{
			return true;
		}

		//------------------------------------------------------------------
		// if LOS is off, don't use LOS
		//------------------------------------------------------------------

		if (!LOS.Config.GetInstance().On)
		{
			return true;
		}

		//------------------------------------------------------------------
		//  Maybe perform symmetric los; this creates balance: if I can't LOS you,
		//  you can't LOS me. This has nothing to do with hidden or other characteristics
		//  at this point, just basic LOS. This can be used to remedy certain
		//  defects of the LOS system that some might regard as unfair...
		//------------------------------------------------------------------

		if (LOS.Config.GetInstance().Symmetric)
		{
			return Map.LOS.Visible(viewer, target) && Map.LOS.Visible(target, viewer);
		}
		else
		{
			return Map.LOS.Visible(viewer, target);
		}
	}

	//------------------------------------------------------------------------------
	//  InvalidateLos() -- this function sends a remove packet to mobiles that
	//    have just gone out of our line of site. This needs to be done because the
	//    client "dead reckons" (stores last position) mobiles by default.
	//------------------------------------------------------------------------------

	public void InvalidateLos()
	{
		if (LosCurrent.Count > 0)
		{
			Dictionary<IEntity, IEntity> culls = new();

			foreach (IEntity o in LosCurrent.Keys)
			{
				if (o is Mobile m)
				{
					if (!CanSee(m))
						culls.Add(m, m);
				}
				else if (o is Item i)
				{
					if (!CanSee(i))
						culls.Add(i, i);
				}
			}

			foreach (IEntity o in culls.Keys)
			{
				RemoveLos(o);
				Packet p = o is Mobile mobile ? mobile.RemovePacket : ((Item)o).RemovePacket;
				NetState state = NetState;
				state?.Send(p);
			}
		}
	}


	public bool InLos(IEntity o)
	{
		return LosCurrent.ContainsKey(o);
	}

	public void AddLos(IEntity o)
	{
		if (_netState == null)
			return; // things without a netstate cannot benefit from LOS list optimization
		if (!LosCurrent.ContainsKey(o))
			LosCurrent.Add(o, o);
	}

	public void RemoveLos(IEntity o)
	{
		if (_netState == null)
			return; // things without a netstate cannot benefit from LOS list optimization
		if (LosCurrent.ContainsKey(o))
			LosCurrent.Remove(o);
	}

	// XXX LOS END

	public virtual bool CanBeRenamedBy(Mobile from)
	{
		return from.IsStaff() && from._accessLevel > _accessLevel;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string Language
	{
		get => _language;
		set => _language = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SpeechHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EmoteHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int WhisperHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int YellHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public string GuildTitle
	{
		get => _mGuildTitle;
		set
		{
			string old = _mGuildTitle;

			if (old != value)
			{
				_mGuildTitle = value;

				if (_mGuild != null && !_mGuild.Disbanded && _mGuildTitle != null)
					SendLocalizedMessage(1018026, true, _mGuildTitle); // Your guild title has changed :

				InvalidateProperties();

				OnGuildTitleChange(old);
			}
		}
	}

	public virtual void OnGuildTitleChange(string oldTitle)
	{
	}

	[CommandProperty(AccessLevel.Decorator)]
	public bool DisplayGuildAbbr
	{
		get => _displayGuildAbbr;
		set
		{
			_displayGuildAbbr = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool DisplayGuildTitle
	{
		get => _displayGuildTitle;
		set
		{
			_displayGuildTitle = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile GuildFealty { get; set; }

	private string _nameMod;

	[CommandProperty(AccessLevel.GameMaster)]
	public string NameMod
	{
		get => _nameMod;
		set
		{
			if (_nameMod != value)
			{
				_nameMod = value;
				Delta(MobileDelta.Name);
				InvalidateProperties();
			}
		}
	}

	private bool _yellowHealthbar;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool YellowHealthbar
	{
		get => _yellowHealthbar;
		set
		{
			_yellowHealthbar = value;
			Delta(MobileDelta.HealthbarYellow);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public string RawName
	{
		get => _mName;
		set => Name = value;
	}

	[CommandProperty(AccessLevel.Decorator)]
	public virtual string TitleName => _mName;

	[CommandProperty(AccessLevel.GameMaster)]
	public string Name
	{
		get => _nameMod ?? _mName;
		set
		{
			if (_mName != value) // I'm leaving out the && m_NameMod == null
			{
				string oldName = _mName;
				_mName = value;
				OnAfterNameChange(oldName, _mName);
				Delta(MobileDelta.Name);
				InvalidateProperties();
			}
		}
	}

	public virtual void OnAfterNameChange(string oldName, string newName)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime LastStrGain { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime LastIntGain { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime LastDexGain { get; set; }

	public DateTime LastStatGain
	{
		get
		{
			DateTime d = LastStrGain;

			if (LastIntGain > d)
				d = LastIntGain;

			if (LastDexGain > d)
				d = LastDexGain;

			return d;
		}
		set
		{
			LastStrGain = value;
			LastIntGain = value;
			LastDexGain = value;
		}
	}

	public BaseGuild Guild
	{
		get => _mGuild;
		set
		{
			BaseGuild old = _mGuild;

			if (old != value)
			{
				if (value == null)
					GuildTitle = null;

				_mGuild = value;

				Delta(MobileDelta.Noto);
				InvalidateProperties();

				OnGuildChange(old);
			}
		}
	}

	public virtual void OnGuildChange(BaseGuild oldGuild)
	{
	}

	#region Poison/Curing

	public Timer PoisonTimer { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Poison Poison
	{
		get => _mPoison;
		set
		{
			/*if ( m_Poison != value && (m_Poison == null || value == null || m_Poison.Level < value.Level) )
			{*/
			_mPoison = value;
			Delta(MobileDelta.HealthbarPoison);

			if (PoisonTimer != null)
			{
				PoisonTimer.Stop();
				PoisonTimer = null;
			}

			if (_mPoison != null)
			{
				PoisonTimer = _mPoison.ConstructTimer(this);

				if (PoisonTimer != null)
					PoisonTimer.Start();
			}

			CheckStatTimers();
			/*}*/
		}
	}

	/// <summary>
	/// Overridable. Event invoked when a call to <see cref="ApplyPoison" /> failed because <see cref="CheckPoisonImmunity" /> returned false: the Mobile was resistant to the poison. By default, this broadcasts an overhead message: * The poison seems to have no effect. *
	/// <seealso cref="CheckPoisonImmunity" />
	/// <seealso cref="ApplyPoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual void OnPoisonImmunity(Mobile from, Poison poison)
	{
		PublicOverheadMessage(MessageType.Emote, 0x3B2, 1005534); // * The poison seems to have no effect. *
	}

	/// <summary>
	/// Overridable. Virtual event invoked when a call to <see cref="ApplyPoison" /> failed because <see cref="CheckHigherPoison" /> returned false: the Mobile was already poisoned by an equal or greater strength poison.
	/// <seealso cref="CheckHigherPoison" />
	/// <seealso cref="ApplyPoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual void OnHigherPoison(Mobile from, Poison poison)
	{
	}

	/// <summary>
	/// Overridable. Event invoked when a call to <see cref="ApplyPoison" /> succeeded. By default, this broadcasts an overhead message varying by the level of the poison. Example: * Zippy begins to spasm uncontrollably. *
	/// <seealso cref="ApplyPoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual void OnPoisoned(Mobile from, Poison poison, Poison oldPoison)
	{
		if (poison != null)
		{
			LocalOverheadMessage(MessageType.Regular, 0x21, 1042857 + poison.Level * 2);
			NonlocalOverheadMessage(MessageType.Regular, 0x21, 1042858 + poison.Level * 2, Name);
		}
	}

	/// <summary>
	/// Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is immune to some <see cref="Poison" />. If true, <see cref="OnPoisonImmunity" /> will be invoked and <see cref="ApplyPoisonResult.Immune" /> is returned.
	/// <seealso cref="OnPoisonImmunity" />
	/// <seealso cref="ApplyPoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual bool CheckPoisonImmunity(Mobile from, Poison poison)
	{
		return false;
	}

	/// <summary>
	/// Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is already poisoned by some <see cref="Poison" /> of equal or greater strength. If true, <see cref="OnHigherPoison" /> will be invoked and <see cref="ApplyPoisonResult.HigherPoisonActive" /> is returned.
	/// <seealso cref="OnHigherPoison" />
	/// <seealso cref="ApplyPoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual bool CheckHigherPoison(Mobile from, Poison poison)
	{
		return _mPoison != null && _mPoison.Level >= poison.Level;
	}

	/// <summary>
	/// Overridable. Attempts to apply poison to the Mobile. Checks are made such that no <see cref="CheckHigherPoison">higher poison is active</see> and that the Mobile is not <see cref="CheckPoisonImmunity">immune to the poison</see>. Provided those assertions are true, the <paramref name="poison" /> is applied and <see cref="OnPoisoned" /> is invoked.
	/// <seealso cref="Poison" />
	/// <seealso cref="CurePoison" />
	/// </summary>
	/// <returns>One of four possible values:
	/// <list type="table">
	/// <item>
	/// <term><see cref="ApplyPoisonResult.Cured">Cured</see></term>
	/// <description>The <paramref name="poison" /> parameter was null and so <see cref="CurePoison" /> was invoked.</description>
	/// </item>
	/// <item>
	/// <term><see cref="ApplyPoisonResult.HigherPoisonActive">HigherPoisonActive</see></term>
	/// <description>The call to <see cref="CheckHigherPoison" /> returned false.</description>
	/// </item>
	/// <item>
	/// <term><see cref="ApplyPoisonResult.Immune">Immune</see></term>
	/// <description>The call to <see cref="CheckPoisonImmunity" /> returned false.</description>
	/// </item>
	/// <item>
	/// <term><see cref="ApplyPoisonResult.Poisoned">Poisoned</see></term>
	/// <description>The <paramref name="poison" /> was successfully applied.</description>
	/// </item>
	/// </list>
	/// </returns>
	public virtual ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
	{
		if (poison == null)
		{
			_ = CurePoison(from);
			return ApplyPoisonResult.Cured;
		}

		if (CheckHigherPoison(from, poison))
		{
			OnHigherPoison(from, poison);
			return ApplyPoisonResult.HigherPoisonActive;
		}

		if (CheckPoisonImmunity(from, poison))
		{
			OnPoisonImmunity(from, poison);
			return ApplyPoisonResult.Immune;
		}

		Poison oldPoison = _mPoison;
		Poison = poison;

		OnPoisoned(from, poison, oldPoison);

		return ApplyPoisonResult.Poisoned;
	}

	/// <summary>
	/// Overridable. Called from <see cref="CurePoison" />, this method checks to see that the Mobile can be cured of <see cref="Poison" />
	/// <seealso cref="CurePoison" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual bool CheckCure(Mobile from)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> succeeded.
	/// <seealso cref="CurePoison" />
	/// <seealso cref="CheckCure" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual void OnCured(Mobile from, Poison oldPoison)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> failed.
	/// <seealso cref="CurePoison" />
	/// <seealso cref="CheckCure" />
	/// <seealso cref="Poison" />
	/// </summary>
	public virtual void OnFailedCure(Mobile from)
	{
	}

	/// <summary>
	/// Overridable. Attempts to cure any poison that is currently active.
	/// </summary>
	/// <returns>True if poison was cured, false if otherwise.</returns>
	public virtual bool CurePoison(Mobile from)
	{
		if (CheckCure(from))
		{
			Poison oldPoison = _mPoison;
			Poison = null;

			OnCured(from, oldPoison);

			return true;
		}

		OnFailedCure(from);

		return false;
	}

	#endregion

	public ISpawner Spawner { get; set; }
	public Region WalkRegion { get; set; }

	public virtual void OnBeforeSpawn(Point3D location, Map m)
	{
	}

	public virtual void OnAfterSpawn()
	{
	}

	protected virtual void OnCreate()
	{ }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Poisoned => _mPoison != null;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsBodyMod => _bodyMod.BodyId != 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public Body BodyMod
	{
		get => _bodyMod;
		set
		{
			if (_bodyMod != value)
			{
				_bodyMod = value;

				Delta(MobileDelta.Body);
				InvalidateProperties();

				CheckStatTimers();
			}
		}
	}

	private static readonly int[] m_InvalidBodies = {
		32,
		95,
		156,
		197,
		198,
	};

	[Body, CommandProperty(AccessLevel.GameMaster)]
	public Body Body
	{
		get => IsBodyMod ? _bodyMod : _mBody;
		set
		{
			if (_mBody != value && !IsBodyMod)
			{
				_mBody = SafeBody(value);

				Delta(MobileDelta.Body);
				InvalidateProperties();

				CheckStatTimers();
			}
		}
	}

	public virtual int SafeBody(int body)
	{
		var delta = -1;

		for (var i = 0; delta < 0 && i < m_InvalidBodies.Length; ++i)
			delta = m_InvalidBodies[i] - body;

		return delta != 0 ? body : 0;
	}

	[Body, CommandProperty(AccessLevel.GameMaster)]
	public int BodyValue
	{
		get => Body.BodyId;
		set => Body = value;
	}
	private Serial _serial;
	[CommandProperty(AccessLevel.Counselor, true)]
	public Serial Serial => _serial;

	internal void NewSerial()
	{
		_serial = Serial.NewMobile;
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public Point3D Location
	{
		get => _mLocation;
		set => SetLocation(value, true);
	}

	/* Logout:
	 *
	 * When a client logs into mobile x
	 *  - if ( x is Internalized ) move x to logout location and map
	 *
	 * When a client attached to a mobile disconnects
	 *  - LogoutTimer is started
	 *	   - Delay is taken from Region.GetLogoutDelay to allow insta-logout regions.
	 *     - OnTick : Location and map are stored, and mobile is internalized
	 *
	 * Some things to consider:
	 *  - An internalized person getting killed (say, by poison). Where does the body go?
	 *  - Regions now have a GetLogoutDelay( Mobile m ); virtual function (see above)
	 */
	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public Point3D LogoutLocation { get; set; }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public Map LogoutMap { get; set; }

	public Region Region
	{
		get
		{
			if (_region == null)
				return Map == null ? Map.Internal.DefaultRegion : Map.DefaultRegion;
			return _region;
		}
	}

	public void FreeCache()
	{
		Packet.Release(ref _mRemovePacket);
		Packet.Release(ref _mPropertyList);
		Packet.Release(ref _mOplPacket);
	}

	private Packet _mRemovePacket;
	private readonly object _rpLock = new();

	public Packet RemovePacket
	{
		get
		{
			if (_mRemovePacket == null)
			{
				lock (_rpLock)
				{
					if (_mRemovePacket == null)
					{
						_mRemovePacket = new RemoveMobile(this);
						_mRemovePacket.SetStatic();
					}
				}
			}

			return _mRemovePacket;
		}
	}

	private Packet _mOplPacket;
	private readonly object _oplLock = new();

	public Packet OplPacket
	{
		get
		{
			if (_mOplPacket == null)
			{
				lock (_oplLock)
				{
					if (_mOplPacket == null)
					{
						_mOplPacket = new OplInfo(PropertyList);
						_mOplPacket.SetStatic();
					}
				}
			}

			return _mOplPacket;
		}
	}

	private ObjectPropertyList _mPropertyList;

	public ObjectPropertyList PropertyList
	{
		get
		{
			if (_mPropertyList == null)
			{
				_mPropertyList = new ObjectPropertyList(this);

				GetProperties(_mPropertyList);

				_mPropertyList.Terminate();
				_mPropertyList.SetStatic();
			}

			return _mPropertyList;
		}
	}

	public void ClearProperties()
	{
		Packet.Release(ref _mPropertyList);
		Packet.Release(ref _mOplPacket);
	}

	public void InvalidateProperties()
	{
		if (!ObjectPropertyList.Enabled)
			return;

		if (_mMap != null && _mMap != Map.Internal && !World.Loading)
		{
			ObjectPropertyList oldList = _mPropertyList;
			Packet.Release(ref _mPropertyList);
			ObjectPropertyList newList = PropertyList;

			if (oldList == null || oldList.Hash != newList.Hash)
			{
				Packet.Release(ref _mOplPacket);
				Delta(MobileDelta.Properties);
			}
		}
		else
		{
			ClearProperties();
		}
	}

	private int _mSolidHueOverride = -1;

	[CommandProperty(AccessLevel.GameMaster)]
	public int SolidHueOverride
	{
		get => _mSolidHueOverride;
		set { if (_mSolidHueOverride == value) return; _mSolidHueOverride = value; Delta(MobileDelta.Hue | MobileDelta.Body); }
	}

	public virtual void SendMapUpdates(bool global, bool extra)
	{
		var ns = _netState;

		if (ns != null)
		{
			ns.Sequence = 0;

			ClearFastwalkStack();

			SupportedFeatures.Send(ns);

			if (_mMap != null)
			{
				MapChange.Send(ns);
				MapPatches.Send(ns);

				SeasonChange.Send(ns, true);

				ns.Send(new ServerChange(this, _mMap));
			}

			CheckLightLevels(extra);

			MobileIncoming.Send(ns, this);
			MobileUpdate.Send(ns, this);

			ns.Send(new MobileAttributes(this));
		}

		if (global)
		{
			SendIncomingPacket();
		}

		SendEverything();
	}

	/*public virtual void MoveToWorld(Point3D newLocation, Map map)
	{
		if (Deleted)
			return;

		if (_mMap == map)
		{
			SetLocation(newLocation, true);
			return;
		}

		BankBox box = FindBankNoCreate();

		if (box is {Opened: true})
			box.Close();

		Point3D oldLocation = _mLocation;
		Map oldMap = _mMap;
		if (oldMap != null)
		{
			oldMap.OnLeave(this);

			ClearScreen();
			SendRemovePacket();
		}

		for (int i = 0; i < Items.Count; ++i)
			Items[i].Map = map;

		_mMap = map;

		_mLocation = newLocation;

		NetState ns = m_NetState;

		if (_mMap != null)
		{
			_mMap.OnEnter(this);

			UpdateRegion();

			if (ns != null && _mMap != null)
			{
				ns.Sequence = 0;
				ns.Send(new MapChange(this));
				ns.Send(new MapPatches());
				ns.Send(SeasonChange.Instantiate(GetSeason(), true));

				if (ns.StygianAbyss)
					ns.Send(new MobileUpdate(this));
				else
					ns.Send(new MobileUpdateOld(this));

				ClearFastwalkStack();
			}
		}
		else
		{
			UpdateRegion();
		}

		if (ns != null)
		{
			if (_mMap != null)
				_ = Send(new ServerChange(this, _mMap));

			ns.Sequence = 0;
			ClearFastwalkStack();

			ns.Send(MobileIncoming.Create(ns, this, this));

			if (ns.StygianAbyss)
			{
				ns.Send(new MobileUpdate(this));
				CheckLightLevels(true);
				ns.Send(new MobileUpdate(this));
			}
			else
			{
				ns.Send(new MobileUpdateOld(this));
				CheckLightLevels(true);
				ns.Send(new MobileUpdateOld(this));
			}
		}

		SendEverything();
		SendIncomingPacket();

		if (ns != null)
		{
			ns.Sequence = 0;
			ClearFastwalkStack();

			ns.Send(MobileIncoming.Create(ns, this, this));

			if (ns.StygianAbyss)
			{
				ns.Send(SupportedFeatures.Instantiate(ns));
				ns.Send(new MobileUpdate(this));
				ns.Send(new MobileAttributes(this));
			}
			else
			{
				ns.Send(SupportedFeatures.Instantiate(ns));
				ns.Send(new MobileUpdateOld(this));
				ns.Send(new MobileAttributes(this));
			}
		}

		OnMapChange(oldMap);
		OnLocationChange(oldLocation);

		m_Region?.OnLocationChanged(this, oldLocation);
	}*/
	public virtual void MoveToWorld(Point3D newLocation, Map map)
	{
		if (Deleted)
		{
			return;
		}

		if (_mMap == map)
		{
			SetLocation(newLocation, true);
			return;
		}

		if (AccessLevel <= AccessLevel.Counselor)
		{
			var box = FindBankNoCreate();

			if (box is {Opened: true})
			{
				box.Close();
			}
		}

		var oldLocation = _mLocation;
		var oldMap = _mMap;

		if (oldMap != null)
		{
			oldMap.OnLeave(this);

			ClearScreen();
			SendRemovePacket();
		}

		var i = Items.Count;

		while (--i >= 0)
		{
			if (i < Items.Count)
			{
				Items[i].Map = map;
			}
		}

		_mMap = map;
		_mLocation = newLocation;

		_mMap?.OnEnter(this);

		UpdateRegion();
		SendMapUpdates(true, true);

		NotifyLocationChange(oldMap, oldLocation);
	}

	public virtual void NotifyLocationChange(Map oldMap, Point3D oldLocation)
	{
		if (oldMap == _mMap && oldLocation == _mLocation)
			return;

		if (oldMap != null)
		{
			var obj = oldMap.GetObjectsInRange(oldLocation, 0);

			foreach (var o in obj)
			{
				switch (o)
				{
					case Item item:
						item.OnLeaveLocation(this);
						break;
					case Mobile mob:
						mob.OnLeaveLocation(this);
						break;
				}
			}

			obj.Free();
		}

		if (_mMap != null)
		{
			var obj = _mMap.GetObjectsInRange(_mLocation, 0);

			foreach (var o in obj)
			{
				switch (o)
				{
					case Item item:
						item.OnEnterLocation(this);
						break;
					case Mobile mob:
						mob.OnEnterLocation(this);
						break;
				}
			}

			obj.Free();
		}

		if (oldMap != _mMap)
			OnMapChange(oldMap);

		OnLocationChange(oldLocation);

		Region?.OnLocationChanged(this, oldLocation);
	}

	public virtual void SetLocation(Point3D newLocation, bool isTeleport)
	{
		if (Deleted)
			return;

		Point3D oldLocation = _mLocation;

		if (oldLocation != newLocation)
		{
			_mLocation = newLocation;
			UpdateRegion();

			if (AccessLevel <= AccessLevel.Counselor)
			{
				var box = FindBankNoCreate();

				if (box is {Opened: true})
				{
					box.Close();
				}
			}

			_netState?.ValidateAllTrades();

			_mMap?.OnMove(oldLocation, this);

			if (isTeleport && _netState != null && (!_netState.HighSeas || !NoMoveHs))
			{
				_netState.Sequence = 0;

				MobileUpdate.Send(_netState, this);

				ClearFastwalkStack();

				EventSink.InvokeOnTeleportMovement(this, oldLocation, newLocation);
			}

			Map map = _mMap;

			if (map != null)
			{

				// XXX LOS BEGIN
				if (_netState != null)
					InvalidateLos(); // things without netstate do not benefit from Los mgmt
				// XXX LOS END
				// First, send a remove message to everyone who can no longer see us. (inOldRange && !inNewRange)
				//Packet removeThis = null;
				IPooledEnumerable<NetState> eable = map.GetClientsInRange(oldLocation);

				foreach (NetState ns in eable)
				{
					//if (ns != m_NetState && !Utility.InUpdateRange(newLocation, ns.Mobile.Location))
					//{
					//	ns.Send(RemovePacket);
					//}
					// XXX LOS BEGIN

					//------------------------------------------------------------------
					//  This code isn't really that different from the above. It's just been broken out
					//  so that it could be worked with and debugged more easily.
					//------------------------------------------------------------------
					var m = ns.Mobile;
					if  (ns != _netState && (!m.InUpdateRange(newLocation, ns.Mobile.Location) || !ns.Mobile.CanSee(this)))
					{
						//if (removeThis == null)
						//	removeThis = RemovePacket;

						ns.Mobile.RemoveLos(this);
						ns.Send(RemovePacket);
						//ns.Send(removeThis);
					}
					// XXX LOS END
				}

				eable.Free();

				var hbpPacket = Packet.Acquire(new HealthbarPoison(this));
				var hbyPacket = Packet.Acquire(new HealthbarYellow(this));

				var hbpKrPacket = Packet.Acquire(new HealthbarPoisonEC(this));
				var hbyKrPacket = Packet.Acquire(new HealthbarYellowEC(this));

				NetState ourState = _netState;

				// Check to see if we are attached to a client
				if (ourState != null)
				{
					IPooledEnumerable<IEntity> eeable = map.GetObjectsInRange(newLocation, Map.GlobalMaxUpdateRange);

					// We are attached to a client, so it's a bit more complex. We need to send new items and people to ourself, and ourself to other clients

					foreach (IEntity o in eeable)
					{
						if (o is Item item)
						{
							//int range = item.GetUpdateRange(this);
							//Point3D loc = item.Location;

							//if (!Utility.InRange(oldLocation, loc, range) && Utility.InRange(newLocation, loc, range) && CanSee(item))
							//item.SendInfoTo(ourState);
							// XXX LOS BEGIN
							if (CanSee(item))
							{
								item.SendInfoTo(ourState);
								AddLos(item); // optimization
							}
							// XXX LOS END
						}
						else if (o != this && o is Mobile m)
						{
							if (!m.InUpdateRange(newLocation, m._mLocation))
								continue;

							/*bool inOldRange = Utility.InUpdateRange(oldLocation, m.m_Location);

							if (m.m_NetState != null && ((isTeleport && (!m.m_NetState.HighSeas || !NoMoveHS)) || !inOldRange) && m.CanSee(this))
							{
								m.m_NetState.Send(MobileIncoming.Create(m.m_NetState, m, this));

								if (m.m_NetState.StygianAbyss)
								{
									//if ( m_Poison != null )
									m.m_NetState.Send(new HealthbarPoison(this));

									//if ( m_Blessed || m_YellowHealthbar )
									m.m_NetState.Send(new HealthbarYellow(this));
								}

								if (IsDeadBondedPet)
									m.m_NetState.Send(new BondedStatus(0, m_Serial, 1));

								if (ObjectPropertyList.Enabled)
								{
									m.m_NetState.Send(OPLPacket);

									//foreach ( Item item in m_Items )
									//	m.m_NetState.Send( item.OPLPacket );
								}
							}*/

							/*
if (!inOldRange && CanSee(m))
{
ourState.Send(MobileIncoming.Create(ourState, this, m));

if (ourState.StygianAbyss)
{
	//if ( m.Poisoned )
	ourState.Send(new HealthbarPoison(m));

	//if ( m.Blessed || m.YellowHealthbar )
	ourState.Send(new HealthbarYellow(m));
}

if (m.IsDeadBondedPet)
	ourState.Send(new BondedStatus(0, m.m_Serial, 1));

if (ObjectPropertyList.Enabled)
{
	ourState.Send(m.OPLPacket);

	//foreach ( Item item in m.m_Items )
	//	ourState.Send( item.OPLPacket );
}
}*/

							// XXX LOS BEGIN
							//if( m == this ) // XXX NEVER TRUE AT THIS POINT JK
							//if( isTeleport && m.m_NetState != null && m.CanSee( this ) )

							//----------------------------------------------
							// other players; send their update of my move
							//----------------------------------------------

							if (m._netState != null && (isTeleport && !m._netState.HighSeas || !NoMoveHs) && !InLos(this) && m.CanSee(this))
							{
								if (LOS.Config.GetInstance().SquelchNames > 0)
									_mLosRecent.Update(m, m);

								m.AddLos(this); // optimization

								MobileIncoming.Send(m._netState, this);

								if (m._netState.IsEnhancedClient)
								{
									m._netState.Send(hbpKrPacket);
									m._netState.Send(hbyKrPacket);
								}
								else if (m._netState.StygianAbyss)
								{
									m._netState.Send(hbpPacket);
									m._netState.Send(hbyPacket);
								}

								if (IsDeadBondedPet)
									ourState.Send(new BondedStatus(0, _serial, 1));

								if (ObjectPropertyList.Enabled)
								{
									ourState.Send(OplPacket);
								}
							}

							//----------------------------------------------
							// this player; update my view based on my move
							//----------------------------------------------

							if (!InLos(m) && CanSee(m))
							{
								if (LOS.Config.GetInstance().SquelchNames > 0)
									_mLosRecent.Update(m, m);

								AddLos(m); // optimization

								MobileIncoming.Send(ourState, m);

								if (ourState.IsEnhancedClient)
								{
									ourState.Send(new HealthbarPoisonEC(m));
									ourState.Send(new HealthbarYellowEC(m));
								}
								else if (ourState.StygianAbyss)
								{
									ourState.Send(new HealthbarPoison(m));
									ourState.Send(new HealthbarYellow(m));
								}

								if (m.IsDeadBondedPet)
									ourState.Send(new BondedStatus(0, m._serial, 1));

								if (ObjectPropertyList.Enabled)
								{
									ourState.Send(m.OplPacket);
								}
							}
							// XXX LOS END
						}
					}

					eeable.Free();
				}
				else
				{
					eable = map.GetClientsInRange(newLocation);

					// We're not attached to a client, so simply send an Incoming
					foreach (NetState ns in eable)
					{
						//if (((isTeleport && (!ns.HighSeas || !NoMoveHS)) || !Utility.InUpdateRange(oldLocation, ns.Mobile.Location)) && ns.Mobile.CanSee(this))
						//{
						// XXX LOS BEGIN
						if ((isTeleport && (!ns.HighSeas || !NoMoveHs)) || !ns.Mobile.InLos(this) && ns.Mobile.CanSee(this))
						{
							if (LOS.Config.GetInstance().SquelchNames > 0)
								_mLosRecent.Update(ns.Mobile, ns.Mobile);

							ns.Mobile.AddLos(this); // optimization
							// XXX LOS END

							MobileIncoming.Send(ns, this);

							if (ns.IsEnhancedClient)
							{
								ns.Send(hbpKrPacket);
								ns.Send(hbyKrPacket);
							}
							else if (ns.StygianAbyss)
							{
								ns.Send(hbpPacket);
								ns.Send(hbyPacket);
							}
							//if (ns.StygianAbyss)
							//{
							//if ( m_Poison != null )
							//	ns.Send(new HealthbarPoison(this));

							//if ( m_Blessed || m_YellowHealthbar )
							//	ns.Send(new HealthbarYellow(this));
							//}

							if (IsDeadBondedPet)
								ns.Send(new BondedStatus(0, _serial, 1));

							if (ObjectPropertyList.Enabled)
							{
								ns.Send(OplPacket);

								//foreach ( Item item in m_Items )
								//	ns.Send( item.OPLPacket );
							}
						}
					}

					eable.Free();
				}
				Packet.Release(ref hbpKrPacket);
				Packet.Release(ref hbyKrPacket);
				Packet.Release(ref hbpPacket);
				Packet.Release(ref hbyPacket);
			}
			NotifyLocationChange(map, oldLocation);
			//OnLocationChange(oldLocation);

			//Region.OnLocationChanged(this, oldLocation);
		}
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <see cref="Location" /> changes.
	/// </summary>
	protected virtual void OnLocationChange(Point3D oldLocation)
	{
		var items = Items;

		if (items == null)
		{
			return;
		}

		var i = items.Count;

		while (--i >= 0)
		{
			if (i < items.Count)
			{
				items[i]?.OnParentLocationChange(oldLocation);
			}
		}
	}

	public virtual void OnEnterLocation(Mobile m)
	{
	}

	public virtual void OnLeaveLocation(Mobile m)
	{
	}

	#region Hair & Face
	private HairInfo _hair;
	private FacialHairInfo _facialHair;
	private FaceInfo _face;

	[CommandProperty(AccessLevel.GameMaster)]
	public int HairItemId
	{
		get => _hair?.ItemID ?? 0;
		set
		{
			if (_hair == null && value > 0)
				_hair = new HairInfo(value);
			else if (value <= 0)
				_hair = null;
			else if (_hair != null) _hair.ItemID = value;

			Delta(MobileDelta.Hair);
		}
	}

	//		[CommandProperty( AccessLevel.GameMaster )]
	//		public int HairSerial { get { return HairInfo.FakeSerial( this ); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int FacialHairItemId
	{
		get => _facialHair?.ItemID ?? 0;
		set
		{
			if (_facialHair == null && value > 0)
				_facialHair = new FacialHairInfo(value);
			else if (value <= 0)
				_facialHair = null;
			else if (_facialHair != null) _facialHair.ItemID = value;

			Delta(MobileDelta.FacialHair);
		}
	}

	//		[CommandProperty( AccessLevel.GameMaster )]
	//		public int FacialHairSerial { get { return FacialHairInfo.FakeSerial( this ); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HairHue
	{
		get => _hair?.Hue ?? 0;
		set
		{
			if (_hair != null)
			{
				_hair.Hue = value;
				Delta(MobileDelta.Hair);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int FacialHairHue
	{
		get => _facialHair?.Hue ?? 0;
		set
		{
			if (_facialHair != null)
			{
				_facialHair.Hue = value;
				Delta(MobileDelta.FacialHair);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int FaceItemId
	{
		get => _face?.ItemID ?? 0;
		set
		{
			if (_face == null && value > 0)
			{
				_face = new FaceInfo(value);
			}
			else if (value <= 0)
			{
				_face = null;
			}
			else
			{
				if (_face != null) _face.ItemID = value;
			}

			Delta(MobileDelta.Face);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int FaceHue
	{
		get => _face?.Hue ?? Hue;
		set
		{
			if (_face != null)
			{
				_face.Hue = value;
				Delta(MobileDelta.Face);
			}
		}
	}
	#endregion

	public bool HasFreeHand()
	{
		return FindItemOnLayer(Layer.TwoHanded) == null;
	}

	private IWeapon _mWeapon;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual IWeapon Weapon
	{
		get
		{
			if (_mWeapon is Item {Deleted: false} item && item.Parent == this && CanSee(item))
				return _mWeapon;

			_mWeapon = null;

			item = FindItemOnLayer(Layer.OneHanded) ?? FindItemOnLayer(Layer.TwoHanded);

			if (item is IWeapon weapon)
				return _mWeapon = weapon;
			return GetDefaultWeapon();
		}
	}

	public virtual IWeapon GetDefaultWeapon()
	{
		return DefaultWeapon;
	}

	private BankBox _mBankBox;

	[CommandProperty(AccessLevel.GameMaster)]
	public BankBox BankBox
	{
		get
		{
			if (_mBankBox is {Deleted: false} && _mBankBox.Parent == this)
				return _mBankBox;

			_mBankBox = FindItemOnLayer(Layer.Bank) as BankBox;

			if (_mBankBox == null)
				AddItem(_mBankBox = new BankBox(this));

			return _mBankBox;
		}
	}

	public BankBox FindBankNoCreate()
	{
		if (_mBankBox is {Deleted: false} && _mBankBox.Parent == this)
			return _mBankBox;

		_mBankBox = FindItemOnLayer(Layer.Bank) as BankBox;

		return _mBankBox;
	}

	private Container _backpack;

	[CommandProperty(AccessLevel.GameMaster)]
	public Container Backpack
	{
		get
		{
			if (_backpack is {Deleted: false} && _backpack.Parent == this)
			{
				return _backpack;
			}

			return _backpack = FindItemOnLayer(Layer.Backpack) as Container;
		}
	}

	public virtual bool KeepsItemsOnDeath => _accessLevel > AccessLevel.Player;

	public Item FindItemOnLayer(Layer layer)
	{
		List<Item> eq = Items;
		var count = eq.Count;

		for (var i = 0; i < count; ++i)
		{
			Item item = eq[i];

			if (!item.Deleted && item.Layer == layer)
			{
				return item;
			}
		}

		return null;
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int X
	{
		get => _mLocation.m_X;
		set => Location = new Point3D(value, _mLocation.m_Y, _mLocation.m_Z);
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Y
	{
		get => _mLocation.m_Y;
		set => Location = new Point3D(_mLocation.m_X, value, _mLocation.m_Z);
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Z
	{
		get => _mLocation.m_Z;
		set => Location = new Point3D(_mLocation.m_X, _mLocation.m_Y, value);
	}

	#region Effects & Particles

	public void MovingEffect(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode)
	{
		Effects.SendMovingEffect(this, to, itemId, speed, duration, fixedDirection, explodes, hue, renderMode);
	}

	public void MovingEffect(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes)
	{
		Effects.SendMovingEffect(this, to, itemId, speed, duration, fixedDirection, explodes, 0, 0);
	}

	public void MovingParticles(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode, int effect, int explodeEffect, int explodeSound, EffectLayer layer, int unknown)
	{
		Effects.SendMovingParticles(this, to, itemId, speed, duration, fixedDirection, explodes, hue, renderMode, effect, explodeEffect, explodeSound, layer, unknown);
	}

	public void MovingParticles(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode, int effect, int explodeEffect, int explodeSound, int unknown)
	{
		Effects.SendMovingParticles(this, to, itemId, speed, duration, fixedDirection, explodes, hue, renderMode, effect, explodeEffect, explodeSound, (EffectLayer)255, unknown);
	}

	public void MovingParticles(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int effect, int explodeEffect, int explodeSound, int unknown)
	{
		Effects.SendMovingParticles(this, to, itemId, speed, duration, fixedDirection, explodes, effect, explodeEffect, explodeSound, unknown);
	}

	public void MovingParticles(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int effect, int explodeEffect, int explodeSound)
	{
		Effects.SendMovingParticles(this, to, itemId, speed, duration, fixedDirection, explodes, 0, 0, effect, explodeEffect, explodeSound, 0);
	}

	public void FixedEffect(int itemId, int speed, int duration, int hue, int renderMode)
	{
		Effects.SendTargetEffect(this, itemId, speed, duration, hue, renderMode);
	}

	public void FixedEffect(int itemId, int seconds)
	{
		FixedEffect(itemId, 0, seconds * EffectDurationPerSecond, 0, 0);
	}

	public void FixedEffect(int itemId, float seconds)
	{
		FixedEffect(itemId, 0, (int)seconds * EffectDurationPerSecond, 0, 0);
	}

	public void FixedEffect(int itemId, int speed, float seconds)
	{
		Effects.SendTargetEffect(this, itemId, speed, (int)seconds * EffectDurationPerSecond, 0, 0);
	}

	public void FixedEffect(int itemId, int speed, int duration)
	{
		Effects.SendTargetEffect(this, itemId, speed, duration, 0, 0);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer, int unknown)
	{
		Effects.SendTargetParticles(this, itemId, speed, duration, hue, renderMode, effect, layer, unknown);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer)
	{
		Effects.SendTargetParticles(this, itemId, speed, duration, hue, renderMode, effect, layer, 0);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, EffectLayer layer, int unknown)
	{
		Effects.SendTargetParticles(this, itemId, speed, duration, 0, 0, effect, layer, unknown);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, EffectLayer layer)
	{
		Effects.SendTargetParticles(this, itemId, speed, duration, 0, 0, effect, layer, 0);
	}

	public void BoltEffect(int hue)
	{
		Effects.SendBoltEffect(this, true, hue);
	}

	#endregion

	public void SendIncomingPacket()
	{
		if (_mMap != null)
		{
			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this))
				{
					MobileIncoming.Send(state, this);

					if (state.StygianAbyss)
					{
						if (_mPoison != null)
						{
							if (state.IsEnhancedClient)
							{
								state.Send(new HealthbarPoisonEC(this));
							}
							else
							{
								state.Send(new HealthbarPoison(this));
							}
						}

						if (_blessed || _yellowHealthbar)
						{
							if (state.IsEnhancedClient)
							{
								state.Send(new HealthbarYellowEC(this));
							}
							else
							{
								state.Send(new HealthbarYellow(this));
							}
						}
					}

					if (IsDeadBondedPet)
						state.Send(new BondedStatus(0, _serial, 1));

					if (ObjectPropertyList.Enabled)
					{
						state.Send(OplPacket);

						//foreach ( Item item in m_Items )
						//	state.Send( item.OPLPacket );
					}
				}
			}

			eable.Free();
		}
	}

	public bool PlaceInBackpack(Item item)
	{
		if (item.Deleted)
			return false;

		Container pack = Backpack;

		return pack != null && pack.TryDropItem(this, item, false);
	}

	public bool AddToBackpack(Item item)
	{
		if (item.Deleted)
			return false;

		if (!PlaceInBackpack(item))
		{
			Point3D loc = _mLocation;
			Map map = _mMap;

			if ((map == null || map == Map.Internal) && LogoutMap != null)
			{
				loc = LogoutLocation;
				map = LogoutMap;
			}

			item.MoveToWorld(loc, map);
			return false;
		}

		return true;
	}

	public virtual bool CheckLift(Mobile from, Item item, ref LRReason reject)
	{
		return true;
	}

	public virtual bool CheckNonlocalLift(Mobile from, Item item)
	{
		return from == this || (from.AccessLevel > AccessLevel && from.AccessLevel >= AccessLevel.GameMaster);
	}

	public bool HasTrade
	{
		get
		{
			if (_netState != null)
				return _netState.Trades.Count > 0;

			return false;
		}
	}

	public virtual bool CheckTrade(Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, bool checkWeight, int plusItems, int plusWeight)
	{
		return true;
	}

	public bool OpenTrade(Mobile from)
	{
		return OpenTrade(from, null);
	}

	public virtual bool OpenTrade(Mobile from, Item offer)
	{
		if (!from.Player || !Player || !from.Alive || !Alive)
		{
			return false;
		}

		NetState ourState = _netState;
		NetState theirState = from._netState;

		if (ourState == null || theirState == null)
		{
			return false;
		}

		SecureTradeContainer cont = theirState.FindTradeContainer(this);

		if (!from.CheckTrade(this, offer, cont, true, true, true, 0, 0))
		{
			return false;
		}

		if (cont == null)
		{
			cont = theirState.AddTrade(ourState);
		}

		if (offer != null)
		{
			cont.DropItem(offer);
		}

		return true;
	}

	/// <summary>
	/// Overridable. Event invoked when a Mobile (<paramref name="from" />) drops an <see cref="Item"><paramref name="dropped" /></see> onto the Mobile.
	/// </summary>
	public virtual bool OnDragDrop(Mobile from, Item dropped)
	{
		if (from == this)
		{
			Container pack = Backpack;

			return pack != null && dropped.DropToItem(from, pack, new Point3D(-1, -1, 0));
		}

		return from.InRange(Location, 2) && OpenTrade(from, dropped);
	}

	public virtual bool CheckEquip(Item item)
	{
		return Items.All(t => !t.CheckConflictingLayer(this, item, item.Layer) && !item.CheckConflictingLayer(this, t, t.Layer));
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to wear <paramref name="item" />.
	/// </summary>
	/// <returns>True if the request is accepted, false if otherwise.</returns>
	public virtual bool OnEquip(Item item)
	{
		// For some reason OSI allows equipping quest items, but they are unmarked in the process
		if (item.QuestItem)
		{
			item.QuestItem = false;
			SendLocalizedMessage(1074769); // An item must be in your backpack (and not in a container within) to be toggled as a quest item.
		}

		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to lift <paramref name="item" />.
	/// </summary>
	/// <returns>True if the lift is allowed, false if otherwise.</returns>
	/// <example>
	/// The following example demonstrates usage. It will disallow any attempts to pick up a pick axe if the Mobile does not have enough strength.
	/// <code>
	/// public override bool OnDragLift( Item item )
	/// {
	///		if ( item is Pickaxe &amp;&amp; this.Str &lt; 60 )
	///		{
	///			SendMessage( "That is too heavy for you to lift." );
	///			return false;
	///		}
	///
	///		return base.OnDragLift( item );
	/// }</code>
	/// </example>
	public virtual bool OnDragLift(Item item)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into a <see cref="Container"><paramref name="container" /></see>.
	/// </summary>
	/// <returns>True if the drop is allowed, false if otherwise.</returns>
	public virtual bool OnDroppedItemInto(Item item, Container container, Point3D loc)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> directly onto another <see cref="Item" />, <paramref name="target" />. This is the case of stacking items.
	/// </summary>
	/// <returns>True if the drop is allowed, false if otherwise.</returns>
	public virtual bool OnDroppedItemOnto(Item item, Item target)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into another <see cref="Item" />, <paramref name="target" />. The target item is most likely a <see cref="Container" />.
	/// </summary>
	/// <returns>True if the drop is allowed, false if otherwise.</returns>
	public virtual bool OnDroppedItemToItem(Item item, Item target, Point3D loc)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to give <paramref name="item" /> to a Mobile (<paramref name="target" />).
	/// </summary>
	/// <returns>True if the drop is allowed, false if otherwise.</returns>
	public virtual bool OnDroppedItemToMobile(Item item, Mobile target)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> to the world at a <see cref="Point3D"><paramref name="location" /></see>.
	/// </summary>
	/// <returns>True if the drop is allowed, false if otherwise.</returns>
	public virtual bool OnDroppedItemToWorld(Item item, Point3D location)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event when <paramref name="from" /> successfully uses <paramref name="item" /> while it's on this Mobile.
	/// <seealso cref="Item.OnItemUsed" />
	/// </summary>
	public virtual void OnItemUsed(Mobile from, Item item)
	{
		EventSink.InvokeOnItemUse(from, item);
	}

	public virtual bool CheckNonlocalDrop(Mobile from, Item item, Item target)
	{
		return from == this || (from.AccessLevel > AccessLevel && from.AccessLevel >= AccessLevel.GameMaster);
	}

	public virtual bool CheckItemUse(Mobile from, Item item)
	{
		return true;
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <paramref name="from" /> successfully lifts <paramref name="item" /> from this Mobile.
	/// <seealso cref="Item.OnItemLifted" />
	/// </summary>
	public virtual void OnItemLifted(Mobile from, Item item)
	{
	}

	public virtual bool AllowItemUse(Item item)
	{
		return true;
	}

	public virtual bool AllowEquipFrom(Mobile mob)
	{
		return mob == this || (mob.AccessLevel >= AccessLevel.GameMaster && mob.AccessLevel > AccessLevel);
	}

	public virtual void EquipItem(Item item, int hue)
	{
		item.Hue = hue;
		_ = EquipItem(item);
	}

	public virtual bool EquipItem(Item item)
	{
		if (item == null || item.Deleted || !item.CanEquip(this))
			return false;

		if (CheckEquip(item) && OnEquip(item) && item.OnEquip(this))
		{
			if (_spell != null && !_spell.OnCasterEquiping(item))
				return false;

			//if ( m_Spell != null && m_Spell.State == SpellState.Casting )
			//	m_Spell.Disturb( DisturbType.EquipRequest );

			AddItem(item);
			return true;
		}

		return false;
	}

	internal int MTypeRef;

	public Mobile(Serial serial)
	{
		_region = Map.Internal.DefaultRegion;
		_serial = serial;
		Aggressors = new List<AggressorInfo>();
		Aggressed = new List<AggressorInfo>();
		NextSkillTime = Core.TickCount;
		DamageEntries = new List<DamageEntry>();

		// XXX LOS BEGIN
		// Need to fix. Player isn't set at construct
		//if( Player || LOS.Config.GetInstance().LosForMobs )
		//{
		LosCurrent = new Dictionary<IEntity, IEntity>();//object
		int razorSuppress = LOS.Config.GetInstance().SquelchNames;
		_mLosRecent = new TemporalCache<object, object>(250, razorSuppress > 0 ? razorSuppress : 0);
		//}
		// XXX LOS END
		Type ourType = GetType();
		MTypeRef = World.m_MobileTypes.IndexOf(ourType);

		if (MTypeRef == -1)
		{
			World.m_MobileTypes.Add(ourType);
			MTypeRef = World.m_MobileTypes.Count - 1;
		}

		Init();
	}

	public Mobile()
	{
		_region = Map.Internal.DefaultRegion;
		_serial = Serial.NewMobile;
		// XXX LOS BEGIN

		// Need to fix. Player isn't set at construct
		//if( Player || LOS.Config.GetInstance().LosForMobs )
		//{
		LosCurrent = new Dictionary<IEntity, IEntity>();//object
		int razorSuppress = LOS.Config.GetInstance().SquelchNames;
		_mLosRecent = new TemporalCache<object, object>(250, razorSuppress > 0 ? razorSuppress : 0);
		//}
		// XXX LOS END

		DefaultMobileInit();

		World.AddMobile(this);

		Type ourType = GetType();
		MTypeRef = World.m_MobileTypes.IndexOf(ourType);

		if (MTypeRef == -1)
		{
			World.m_MobileTypes.Add(ourType);
			MTypeRef = World.m_MobileTypes.Count - 1;
		}

		//_ = Timer.DelayCall(EventSink.InvokeOnMobileCreated, this);

		Timer.DelayCall(() =>
		{
			if (!Deleted)
			{
				EventSink.InvokeOnMobileCreated(this);
				if (!Deleted)
				{
					//m_InternalCanRegen = true;
					OnCreate();
				}
			}
		});
	}

	public void DefaultMobileInit()
	{
		_statCap = MConfigStatsCap;
		StrCap = MConfigStrCap;
		DexCap = MConfigDexCap;
		IntCap = MConfigIntCap;
		StrMaxCap = MConfigStrMaxCap;
		DexMaxCap = MConfigDexMaxCap;
		IntMaxCap = MConfigIntMaxCap;
		_followersMax = MConfigFollowersMax;
		_skills = new Skills(this);
		Items = new List<Item>();
		StatMods = new List<StatMod>();
		SkillMods = new List<SkillMod>();
		Map = Map.Internal;
		AutoPageNotify = true;
		Aggressors = new List<AggressorInfo>();
		Aggressed = new List<AggressorInfo>();
		_virtues = new VirtueInfo();
		Stabled = new List<Mobile>();
		DamageEntries = new List<DamageEntry>();

		NextSkillTime = Core.TickCount;
		CreationTime = DateTime.UtcNow;

		Init();
	}

	/// <summary>
	/// Overridable. Event invoked when the mobile is initialize
	/// </summary>
	public virtual void Init()
	{
	}

	private static readonly Queue<Mobile> MDeltaQueue = new();
	private static readonly Queue<Mobile> MDeltaQueueR = new();

	private bool _mInDeltaQueue;
	private MobileDelta _mDeltaFlags;

	public virtual void Delta(MobileDelta flag)
	{
		if (_mMap == null || _mMap == Map.Internal || Deleted)
			return;

		_mDeltaFlags |= flag;

		if (!_mInDeltaQueue)
		{
			_mInDeltaQueue = true;

			if (_processing)
			{
				lock (MDeltaQueueR)
				{
					MDeltaQueueR.Enqueue(this);

					try
					{
						using StreamWriter op = new("delta-recursion.log", true);
						op.WriteLine("# {0}", DateTime.UtcNow);
						op.WriteLine(new System.Diagnostics.StackTrace());
						op.WriteLine();
					}
					catch
					{
						// ignored
					}
				}
			}
			else
			{
				MDeltaQueue.Enqueue(this);
			}
		}

		Core.Set();
	}

	public bool NoMoveHs { get; set; }

	#region GetDirectionTo[..]

	public Direction GetDirectionTo(int x, int y)
	{
		var dx = _mLocation.m_X - x;
		var dy = _mLocation.m_Y - y;

		var rx = (dx - dy) * 44;
		var ry = (dx + dy) * 44;

		var ax = Math.Abs(rx);
		var ay = Math.Abs(ry);

		Direction ret;

		if ((ay >> 1) - ax >= 0)
			ret = ry > 0 ? Direction.Up : Direction.Down;
		else if ((ax >> 1) - ay >= 0)
			ret = rx > 0 ? Direction.Left : Direction.Right;
		else
			ret = rx switch
			{
				>= 0 when ry >= 0 => Direction.West,
				>= 0 when true => Direction.South,
				< 0 when ry < 0 => Direction.East,
				_ => Direction.North
			};

		return ret;
	}

	public Direction GetDirectionTo(Point2D p)
	{
		return GetDirectionTo(p.m_X, p.m_Y);
	}

	public Direction GetDirectionTo(Point3D p)
	{
		return GetDirectionTo(p.m_X, p.m_Y);
	}

	public Direction GetDirectionTo(IPoint2D p)
	{
		return p == null ? Direction.North : GetDirectionTo(p.X, p.Y);
	}

	#endregion

	public virtual void ProcessDelta()
	{
		Mobile m = this;

		var delta = m._mDeltaFlags;

		if (delta == MobileDelta.None)
			return;

		MobileDelta attrs = delta & MobileDelta.Attributes;

		m._mDeltaFlags = MobileDelta.None;
		m._mInDeltaQueue = false;

		bool sendHits = false, sendStam = false, sendMana = false, sendAll = false, sendAny = false;
		bool sendIncoming = false, sendNonlocalIncoming = false;
		bool sendUpdate = false, sendRemove = false;
		bool sendPublicStats = false, sendPrivateStats = false;
		bool sendMoving = false, sendNonlocalMoving = false;

		bool sendOplUpdate = ObjectPropertyList.Enabled && (delta & MobileDelta.Properties) != 0;

		bool sendHair = false, sendFacialHair = false, removeHair = false, removeFacialHair = false, sendFace = false, removeFace = false;

		bool sendHealthbarPoison = false, sendHealthbarYellow = false;

		if (attrs != MobileDelta.None)
		{
			sendAny = true;

			if (attrs == MobileDelta.Attributes)
			{
				sendAll = true;
			}
			else
			{
				sendHits = (attrs & MobileDelta.Hits) != 0;
				sendStam = (attrs & MobileDelta.Stam) != 0;
				sendMana = (attrs & MobileDelta.Mana) != 0;
			}
		}

		if ((delta & MobileDelta.GhostUpdate) != 0)
		{
			sendNonlocalIncoming = true;
		}

		if ((delta & MobileDelta.Hue) != 0)
		{
			sendNonlocalIncoming = true;
			sendUpdate = true;
			sendRemove = true;
		}

		if ((delta & MobileDelta.Direction) != 0)
		{
			sendNonlocalMoving = true;
			sendUpdate = true;
		}

		if ((delta & MobileDelta.Body) != 0)
		{
			sendUpdate = true;
			sendIncoming = true;
		}

		if ((delta & (MobileDelta.Flags | MobileDelta.Noto)) != 0)
		{
			sendMoving = true;
		}

		if ((delta & MobileDelta.HealthbarPoison) != 0)
		{
			sendHealthbarPoison = true;
		}

		if ((delta & MobileDelta.HealthbarYellow) != 0)
		{
			sendHealthbarYellow = true;
		}

		if ((delta & MobileDelta.Name) != 0)
		{
			sendAll = false;
			sendHits = false;
			sendAny = sendStam || sendMana;
			sendPublicStats = true;
		}

		if ((delta & (MobileDelta.WeaponDamage | MobileDelta.Resistances | MobileDelta.Stat |
		              MobileDelta.Weight | MobileDelta.Gold | MobileDelta.Armor | MobileDelta.StatCap |
		              MobileDelta.Followers | MobileDelta.TithingPoints | MobileDelta.Race)) != 0)
		{
			sendPrivateStats = true;
		}

		if ((delta & MobileDelta.Hair) != 0)
		{
			if (m.HairItemId <= 0)
				removeHair = true;

			sendHair = true;
		}

		if ((delta & MobileDelta.FacialHair) != 0)
		{
			if (m.FacialHairItemId <= 0)
				removeFacialHair = true;

			sendFacialHair = true;
		}

		if ((delta & MobileDelta.Face) != 0)
		{
			if (m.FaceItemId <= 0)
			{
				removeFace = true;
			}

			sendFace = true;
		}

		Packet[][] cache = { new Packet[8], new Packet[8] };

		NetState ourState = m._netState;

		if (ourState != null)
		{
			if (sendUpdate)
			{
				ourState.Sequence = 0;

				MobileUpdate.Send(ourState, m);

				ClearFastwalkStack();
			}

			if (sendIncoming)
				MobileIncoming.Send(ourState, m);

			if (ourState.StygianAbyss)
			{
				if (sendMoving)
				{
					int noto = Notoriety.Compute(m, m);
					ourState.Send(cache[0][noto] = Packet.Acquire(new MobileMoving(m, noto)));
				}

				if (sendHealthbarPoison)
				{
					if (ourState.IsEnhancedClient)
					{
						ourState.Send(new HealthbarPoisonEC(m));
					}
					else
					{
						ourState.Send(new HealthbarPoison(m));
					}
				}

				if (sendHealthbarYellow)
				{
					if (ourState.IsEnhancedClient)
					{
						ourState.Send(new HealthbarYellowEC(m));
					}
					else
					{
						ourState.Send(new HealthbarYellow(m));
					}
				}
			}
			else
			{
				if (sendMoving || sendHealthbarPoison || sendHealthbarYellow)
				{
					int noto = Notoriety.Compute(m, m);
					ourState.Send(cache[1][noto] = Packet.Acquire(new MobileMovingOld(m, noto)));
				}
			}

			if (sendPublicStats || sendPrivateStats)
			{
				ourState.Send(new MobileStatusExtended(m, _netState));
			}
			else if (sendAll)
			{
				ourState.Send(new MobileAttributes(m));
			}
			else if (sendAny)
			{
				if (sendHits)
					ourState.Send(new MobileHits(m));

				if (sendStam)
					ourState.Send(new MobileStam(m));

				if (sendMana)
					ourState.Send(new MobileMana(m));
			}

			if (sendStam || sendMana)
			{
				IParty ip = Party as IParty;

				if (ip != null && sendStam)
					ip.OnStamChanged(this);

				if (ip != null && sendMana)
					ip.OnManaChanged(this);
			}

			if (sendHair)
			{
				if (removeHair)
					ourState.Send(new RemoveHair(m));
				else
					ourState.Send(new HairEquipUpdate(m));
			}

			if (sendFacialHair)
			{
				if (removeFacialHair)
					ourState.Send(new RemoveFacialHair(m));
				else
					ourState.Send(new FacialHairEquipUpdate(m));
			}

			if (sendFace && ourState.IsEnhancedClient)
			{
				if (removeFace)
				{
					ourState.Send(new RemoveFace(m));
				}
				else
				{
					ourState.Send(new RemoveFace(m));
					ourState.Send(new FaceEquipUpdate(m));
				}
			}

			if (sendOplUpdate)
				ourState.Send(OplPacket);
		}

		sendMoving = sendMoving || sendNonlocalMoving;
		sendIncoming = sendIncoming || sendNonlocalIncoming;
		sendHits = sendHits || sendAll;

		if (m._mMap != null && (sendRemove || sendIncoming || sendPublicStats || sendHits || sendMoving || sendOplUpdate || sendHair || sendFacialHair || sendHealthbarPoison || sendHealthbarYellow || sendFace))
		{
			Packet hitsPacket = null;
			Packet statPacketTrue = null;
			Packet statPacketFalse = null;
			Packet deadPacket = null;
			Packet hairPacket = null;
			Packet facialhairPacket = null;
			Packet hbpPacket = null;
			Packet hbyPacket = null;
			Packet hbpPacketEc = null;
			Packet hbyPacketEc = null;
			Packet faceRemovePacket = null;
			Packet faceSendPacket = null;

			IPooledEnumerable<NetState> eable = m.Map.GetClientsInRange(m._mLocation);

			foreach (NetState state in eable)
			{
				var beholder = state.Mobile;

				if (beholder != m && beholder.CanSee(m))
				{
					if (sendRemove)
						// XXX LOS BEGIN
						//state.Send( m.RemovePacket );
					{
						state.Mobile.RemoveLos(this);
						state.Send(m.RemovePacket);
					}
					// XXX LOS END

					if (sendIncoming)
					{
						MobileIncoming.Send(state, m);

						if (m.IsDeadBondedPet)
						{
							if (deadPacket == null)
								deadPacket = Packet.Acquire(new BondedStatus(0, m._serial, 1));

							state.Send(deadPacket);
						}
					}

					if (state.StygianAbyss)
					{
						if (sendMoving)
						{
							int noto = Notoriety.Compute(beholder, m);

							Packet p = cache[0][noto];

							if (p == null)
								cache[0][noto] = p = Packet.Acquire(new MobileMoving(m, noto));

							state.Send(p);
						}

						if (sendHealthbarPoison)
						{
							if (state.IsEnhancedClient)
							{
								if (hbpPacketEc == null)
								{
									hbpPacketEc = Packet.Acquire(new HealthbarPoisonEC(m));
								}

								state.Send(hbpPacketEc);
							}
							else
							{
								if (hbpPacket == null)
								{
									hbpPacket = Packet.Acquire(new HealthbarPoison(m));
								}

								state.Send(hbpPacket);
							}

							//state.Send(hbpPacket);
							//state.Send(hbpPacketEC);
						}

						if (sendHealthbarYellow)
						{
							if (state.IsEnhancedClient)
							{
								if (hbyPacketEc == null)
								{
									hbyPacketEc = Packet.Acquire(new HealthbarYellowEC(m));
								}

								state.Send(hbyPacketEc);
							}
							else
							{
								if (hbyPacket == null)
								{
									hbyPacket = Packet.Acquire(new HealthbarYellow(m));
								}

								state.Send(hbyPacket);
							}

							//state.Send(hbyPacket);
							//state.Send(hbyPacketEC);
						}
					}
					else
					{
						if (sendMoving || sendHealthbarPoison || sendHealthbarYellow)
						{
							int noto = Notoriety.Compute(beholder, m);

							Packet p = cache[1][noto];

							if (p == null)
								cache[1][noto] = p = Packet.Acquire(new MobileMovingOld(m, noto));

							state.Send(p);
						}
					}

					if (sendPublicStats)
					{
						if (m.CanBeRenamedBy(beholder))
						{
							statPacketTrue ??= Packet.Acquire(new MobileStatusCompact(true, m));

							state.Send(statPacketTrue);
						}
						else
						{
							statPacketFalse ??= Packet.Acquire(new MobileStatusCompact(false, m));

							state.Send(statPacketFalse);
						}
					}
					else if (sendHits)
					{
						hitsPacket ??= Packet.Acquire(new MobileHitsN(m));

						state.Send(hitsPacket);
					}

					if (sendHair)
					{
						if (hairPacket == null)
						{
							hairPacket = removeHair ? Packet.Acquire(new RemoveHair(m)) : Packet.Acquire(new HairEquipUpdate(m));
						}

						state.Send(hairPacket);
					}

					if (sendFacialHair)
					{
						if (facialhairPacket == null)
						{
							facialhairPacket = removeFacialHair ? Packet.Acquire(new RemoveFacialHair(m)) : Packet.Acquire(new FacialHairEquipUpdate(m));
						}

						state.Send(facialhairPacket);
					}

					if (sendFace && state.IsEnhancedClient)
					{
						if (faceRemovePacket == null)
						{
							faceRemovePacket = Packet.Acquire(new RemoveFace(m));

							if (!removeFace)
							{
								faceSendPacket = Packet.Acquire(new FaceEquipUpdate(m));
							}
						}

						state.Send(faceRemovePacket);

						if (!removeFace)
						{
							state.Send(faceSendPacket);
						}
					}

					if (sendOplUpdate)
						state.Send(OplPacket);
				}
			}

			Packet.Release(hitsPacket);
			Packet.Release(statPacketTrue);
			Packet.Release(statPacketFalse);
			Packet.Release(deadPacket);
			Packet.Release(hairPacket);
			Packet.Release(facialhairPacket);
			Packet.Release(hbpPacket);
			Packet.Release(hbyPacket);
			Packet.Release(hbpPacketEc);
			Packet.Release(hbyPacketEc);
			Packet.Release(faceRemovePacket);
			Packet.Release(faceSendPacket);

			eable.Free();
		}

		if (sendMoving || sendNonlocalMoving || sendHealthbarPoison || sendHealthbarYellow)
		{
			for (var i = 0; i < cache.Length; ++i)
			for (var j = 0; j < cache[i].Length; ++j)
				Packet.Release(ref cache[i][j]);
		}
	}

	private static bool _processing;

	public static void ProcessDeltaQueue()
	{
		_processing = true;

		if (MDeltaQueue.Count >= 512)
		{
			_ = Parallel.ForEach(MDeltaQueue, m => m.ProcessDelta());
			MDeltaQueue.Clear();
		}
		else
		{
			while (MDeltaQueue.Count > 0) MDeltaQueue.Dequeue().ProcessDelta();
		}

		_processing = false;

		lock (MDeltaQueueR)
		{
			while (MDeltaQueueR.Count > 0) MDeltaQueueR.Dequeue().ProcessDelta();
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Deaths
	{
		get => _deaths;
		set
		{
			if (_deaths != value)
			{
				var oldValue = _deaths;

				_deaths = Math.Max(0, value);

				OnDeathsChange(oldValue);
			}
		}
	}

	public virtual void OnDeathsChange(int oldValue)
	{ }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Kills
	{
		get => _kills;
		set
		{
			var oldValue = _kills;

			if (_kills != value)
			{
				_kills = value;

				if (_kills < 0)
					_kills = 0;

				if (oldValue >= MurderKills != _kills >= MurderKills)
				{
					Delta(MobileDelta.Noto);
					InvalidateProperties();
				}

				OnKillsChange(oldValue);
			}
		}
	}

	public virtual void OnKillsChange(int oldValue)
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ShortTermMurders
	{
		get => _shortTermMurders;
		set
		{
			if (_shortTermMurders != value)
			{
				_shortTermMurders = value;

				if (_shortTermMurders < 0)
					_shortTermMurders = 0;
			}
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public bool Criminal
	{
		get => _mCriminal;
		set
		{
			if (_mCriminal != value)
			{
				_mCriminal = value;
				Delta(MobileDelta.Noto);
				InvalidateProperties();
			}

			if (_mCriminal)
			{
				if (_expireCriminal == null)
					_expireCriminal = new ExpireCriminalTimer(this);
				else
					_expireCriminal.Stop();

				_expireCriminal.Start();
			}
			else if (_expireCriminal != null)
			{
				_expireCriminal.Stop();
				_expireCriminal = null;
			}
		}
	}

	[CommandProperty(AccessLevel.Counselor, true)]
	public virtual bool Murderer => _kills >= MurderKills;

	public bool CheckAlive()
	{
		return CheckAlive(true);
	}

	public bool CheckAlive(bool message)
	{
		if (!Alive)
		{
			if (message)
				LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019048); // I am dead and cannot do that.

			return false;
		}
		else
		{
			return true;
		}
	}

	#region Overhead messages

	public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text)
	{
		PublicOverheadMessage(type, hue, ascii, text, true);
	}

	public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text, bool noLineOfSight)
	{
		if (_mMap != null)
		{
			Packet p;
			if (ascii)
				p = new AsciiMessage(_serial, Body, type, hue, 3, Name, text);
			else
				p = new UnicodeMessage(_serial, Body, type, hue, 3, _language, Name, text);

			p.Acquire();

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public void PublicOverheadMessage(MessageType type, int hue, int number)
	{
		PublicOverheadMessage(type, hue, number, "", true);
	}

	public void PublicOverheadMessage(MessageType type, int hue, int number, string args)
	{
		PublicOverheadMessage(type, hue, number, args, true);
	}

	public void PublicOverheadMessage(MessageType type, int hue, int number, string args, bool noLineOfSight)
	{
		if (_mMap != null)
		{
			Packet p = Packet.Acquire(new MessageLocalized(_serial, Body, type, hue, 3, number, Name, args));

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public void PublicOverheadMessage(MessageType type, int hue, int number, AffixType affixType, string affix, string args)
	{
		PublicOverheadMessage(type, hue, number, affixType, affix, args, true);
	}

	public void PublicOverheadMessage(MessageType type, int hue, int number, AffixType affixType, string affix, string args, bool noLineOfSight)
	{
		if (_mMap != null)
		{
			Packet p = Packet.Acquire(new MessageLocalizedAffix(_serial, Body, type, hue, 3, number, Name, affixType, affix, args));

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public void PrivateOverheadMessage(MessageType type, int hue, bool ascii, string text, NetState state)
	{
		if (state == null)
			return;

		if (ascii)
			state.Send(new AsciiMessage(_serial, Body, type, hue, 3, Name, text));
		else
			state.Send(new UnicodeMessage(_serial, Body, type, hue, 3, _language, Name, text));
	}

	public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state)
	{
		PrivateOverheadMessage(type, hue, number, "", state);
	}

	public void PrivateOverheadMessage(MessageType type, int hue, int number, string args, NetState state)
	{
		state?.Send(new MessageLocalized(_serial, Body, type, hue, 3, number, Name, args));
	}

	public void LocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
	{
		NetState ns = _netState;

		if (ns != null)
		{
			if (ascii)
				ns.Send(new AsciiMessage(_serial, Body, type, hue, 3, Name, text));
			else
				ns.Send(new UnicodeMessage(_serial, Body, type, hue, 3, _language, Name, text));
		}
	}

	public void LocalOverheadMessage(MessageType type, int hue, int number)
	{
		LocalOverheadMessage(type, hue, number, "");
	}

	public void LocalOverheadMessage(MessageType type, int hue, int number, string args)
	{
		NetState ns = _netState;

		ns?.Send(new MessageLocalized(_serial, Body, type, hue, 3, number, Name, args));
	}

	public void NonlocalOverheadMessage(MessageType type, int hue, int number)
	{
		NonlocalOverheadMessage(type, hue, number, "");
	}

	public void NonlocalOverheadMessage(MessageType type, int hue, int number, string args)
	{
		if (_mMap != null)
		{
			Packet p = Packet.Acquire(new MessageLocalized(_serial, Body, type, hue, 3, number, Name, args));

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state != _netState && state.Mobile.CanSee(this))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public void NonlocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
	{
		if (_mMap != null)
		{
			Packet p;
			if (ascii)
				p = new AsciiMessage(_serial, Body, type, hue, 3, Name, text);
			else
				p = new UnicodeMessage(_serial, Body, type, hue, 3, Language, Name, text);

			p.Acquire();

			IPooledEnumerable<NetState> eable = _mMap.GetClientsInRange(_mLocation);

			foreach (NetState state in eable)
			{
				if (state != _netState && state.Mobile.CanSee(this))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	#endregion

	#region SendLocalizedMessage

	public void SendLocalizedMessage(int number)
	{
		NetState ns = _netState;

		ns?.Send(MessageLocalized.InstantiateGeneric(number));
	}

	public void SendLocalizedMessage(int number, string args)
	{
		SendLocalizedMessage(number, args, 0x3B2);
	}

	public void SendLocalizedMessage(int number, int hue)
	{
		SendLocalizedMessage(number, null, hue);
	}

	public void SendLocalizedMessage(int number, string args, int hue)
	{
		if (hue == 0x3B2 && string.IsNullOrEmpty(args))
		{
			NetState ns = _netState;

			ns?.Send(MessageLocalized.InstantiateGeneric(number));
		}
		else
		{
			NetState ns = _netState;

			ns?.Send(new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", args));
		}
	}

	public void SendLocalizedMessage(int number, bool append, string affix)
	{
		SendLocalizedMessage(number, append, affix, "", 0x3B2);
	}

	public void SendLocalizedMessage(int number, bool append, string affix, string args)
	{
		SendLocalizedMessage(number, append, affix, args, 0x3B2);
	}

	public void SendLocalizedMessage(int number, bool append, string affix, string args, int hue)
	{
		NetState ns = _netState;

		ns?.Send(new MessageLocalizedAffix(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", (append ? AffixType.Append : AffixType.Prepend) | AffixType.System, affix, args));
	}

	#endregion

	public void LaunchBrowser(string url)
	{
		_netState?.LaunchBrowser(url);
	}

	#region Send[ASCII]Message

	public void SendMessage(string text)
	{
		SendMessage(0x3B2, text);
	}

	public void SendMessage(string format, params object[] args)
	{
		SendMessage(0x3B2, string.Format(format, args));
	}

	public void SendMessage(int hue, string text)
	{
		NetState ns = _netState;

		ns?.Send(new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text));
	}

	public void SendMessage(int hue, string format, params object[] args)
	{
		SendMessage(hue, string.Format(format, args));
	}

	public void SendAsciiMessage(string text)
	{
		SendAsciiMessage(0x3B2, text);
	}

	public void SendAsciiMessage(string format, params object[] args)
	{
		SendAsciiMessage(0x3B2, string.Format(format, args));
	}

	public void SendAsciiMessage(int hue, string text)
	{
		NetState ns = _netState;

		ns?.Send(new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text));
	}

	public void SendAsciiMessage(int hue, string format, params object[] args)
	{
		SendAsciiMessage(hue, string.Format(format, args));
	}

	#endregion

	#region InRange
	public bool InRange(IPoint2D p, int range)
	{
		return Utility.InRange(_mLocation, p, range);
	}

	public bool InUpdateRange(IPoint2D p)
	{
		if (p is Item item)
			return Utility.InRange(_mLocation, item.GetWorldLocation(), item.GetUpdateRange(this));

		return Utility.InRange(_mLocation, p, _netState?.UpdateRange ?? Map.GlobalUpdateRange);
	}

	public bool InUpdateRange(Point2D p, IPoint2D o)
	{
		if (o is Item i)
			return Utility.InRange(p, i.GetWorldLocation(), i.GetUpdateRange(this));

		return Utility.InRange(p, o, _netState?.UpdateRange ?? Map.GlobalUpdateRange);
	}

	public bool InUpdateRange(Point3D p, IPoint2D o)
	{
		if (o is Item i)
			return Utility.InRange(p, i.GetWorldLocation(), i.GetUpdateRange(this));

		return Utility.InRange(p, o, _netState?.UpdateRange ?? Map.GlobalUpdateRange);
	}
	#endregion

	public void InitStats(int str, int dex, int intel)
	{
		_str = str;
		_dex = dex;
		_int = intel;

		Hits = HitsMax;
		Stam = StamMax;
		Mana = ManaMax;

		Delta(MobileDelta.Stat | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
	}

	public virtual void DisplayPaperdollTo(Mobile to)
	{
		EventSink.InvokePaperdollRequest(to, this);
	}

	public static bool DisableDismountInWarmode { get; set; }

	#region OnDoubleClick[..]

	/// <summary>
	/// Overridable. Event invoked when the Mobile is double clicked. By default, this method can either dismount or open the paperdoll.
	/// <seealso cref="CanPaperdollBeOpenedBy" />
	/// <seealso cref="DisplayPaperdollTo" />
	/// </summary>
	public virtual void OnDoubleClick(Mobile from)
	{
		if (this == from && (!DisableDismountInWarmode || !_warmode))
		{
			IMount mount = Mount;

			if (mount != null)
			{
				mount.Rider = null;
				return;
			}
		}

		if (CanPaperdollBeOpenedBy(from))
			DisplayPaperdollTo(from);
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile is double clicked by someone who is over 18 tiles away.
	/// <seealso cref="OnDoubleClick" />
	/// </summary>
	public virtual void OnDoubleClickOutOfRange(Mobile from)
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the Mobile is double clicked by someone who can no longer see the Mobile. This may happen, for example, using 'Last Object' after the Mobile has hidden.
	/// <seealso cref="OnDoubleClick" />
	/// </summary>
	public virtual void OnDoubleClickCantSee(Mobile from)
	{
	}

	/// <summary>
	/// Overridable. Event invoked when the Mobile is double clicked by someone who is not alive. Similar to <see cref="OnDoubleClick" />, this method will show the paperdoll. It does not, however, provide any dismount functionality.
	/// <seealso cref="OnDoubleClick" />
	/// </summary>
	public virtual void OnDoubleClickDead(Mobile from)
	{
		if (CanPaperdollBeOpenedBy(from))
			DisplayPaperdollTo(from);
	}

	#endregion

	/// <summary>
	/// Overridable. Event invoked when the Mobile requests to open his own paperdoll via the 'Open Paperdoll' macro.
	/// </summary>
	public virtual void OnPaperdollRequest()
	{
		if (CanPaperdollBeOpenedBy(this))
			DisplayPaperdollTo(this);
	}

	public static int BodyWeight { get; set; } = 14;

	/// <summary>
	/// Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's stats.
	/// </summary>
	/// <param name="from"></param>
	public virtual void OnStatsQuery(Mobile from)
	{
		if (from.Map == Map && from.InUpdateRange(from) && from.CanSee(this))
			MobileStatus.Send(from.NetState, this);

		if (from == this)
			_ = Send(new StatLockInfo(this));

		if (Party is IParty ip)
			ip.OnStatsQuery(from, this);
	}

	/// <summary>
	/// Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's skills.
	/// </summary>
	public virtual void OnSkillsQuery(Mobile from)
	{
		if (from == this)
			_ = Send(new SkillUpdate(_skills));
	}

	/// <summary>
	/// Overridable. Virtual event invoked when <see cref="Region" /> changes.
	/// </summary>
	public virtual void OnRegionChange(Region old, Region @new)
	{
	}

	private Item _mMountItem;

	[CommandProperty(AccessLevel.GameMaster)]
	public IMount Mount
	{
		get
		{
			IMountItem mountItem = null;

			if (_mMountItem is {Deleted: false} && _mMountItem.Parent == this)
				mountItem = (IMountItem)_mMountItem;

			if (mountItem == null)
				_mMountItem = (mountItem = FindItemOnLayer(Layer.Mount) as IMountItem) as Item;

			return mountItem?.Mount;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Mounted => Mount != null;

	private QuestArrow _mQuestArrow;

	public QuestArrow QuestArrow
	{
		get => _mQuestArrow;
		set
		{
			if (_mQuestArrow != value)
			{
				_mQuestArrow?.Stop();

				_mQuestArrow = value;
			}
		}
	}

	public virtual bool CanTarget => true;
	public virtual bool ClickTitle => true;

	public virtual bool PropertyTitle => !OldPropertyTitles || ClickTitle;

	public static bool DisableHiddenSelfClick { get; set; } = true;
	public static bool AsciiClickMessage { get; set; } = true;
	public static bool GuildClickMessage { get; set; } = true;
	public static bool OldPropertyTitles { get; set; }

	public virtual bool ShowFameTitle => true; //(m_Player || m_Body.IsHuman) && m_Fame >= 10000; }
	public virtual bool DisplayAccessTitle => false;

	/// <summary>
	/// Overridable. Event invoked when the Mobile is single clicked.
	/// </summary>
	public virtual void OnSingleClick(Mobile from)
	{
		// XXX LOS BEGIN
		//Console.Write("Checking from: {0} --> {1} ", from.Name, this.Name );
		if (from._mLosRecent.ContainsKey(this))
		{
			//Console.WriteLine("... TOO SOON.");
			return;
		}
		//else Console.WriteLine("... SENDING PROPS.");
		// XXX LOS END
		if (Deleted)
			return;
		if (IsPlayer() && DisableHiddenSelfClick && Hidden && from == this)
			return;

		if (GuildClickMessage)
		{
			BaseGuild guild = _mGuild;

			if (guild != null && (_displayGuildTitle || (_player && guild.Type != GuildType.Regular)))
			{
				string title = GuildTitle;
				string type;

				title = title == null ? "" : title.Trim();

				if (guild.Type >= 0 && (int)guild.Type < GuildTypes.Length)
					type = GuildTypes[(int)guild.Type];
				else
					type = "";

				string text = string.Format(title.Length <= 0 ? "[{1}]{2}" : "[{0}, {1}]{2}", title, guild.Abbreviation, type);

				PrivateOverheadMessage(MessageType.Regular, SpeechHue, true, text, from.NetState);
			}
		}

		int hue;

		if (NameHue != -1)
			hue = NameHue;
		else if (AccessLevel > AccessLevel.Player)
			hue = 11;
		else
			hue = Notoriety.GetHue(Notoriety.Compute(from, this));

		string name = Name ?? string.Empty;

		string prefix = "";

		if (ShowFameTitle && (_player || _mBody.IsHuman) && _fame >= 10000)
			prefix = _female ? "Lady" : "Lord";

		string suffix = "";

		if (ClickTitle && Title != null && Title.Length > 0)
			suffix = Title;

		suffix = ApplyNameSuffix(suffix);

		string val = prefix.Length switch
		{
			> 0 when suffix.Length > 0 => string.Concat(prefix, " ", name, " ", suffix),
			> 0 => string.Concat(prefix, " ", name),
			_ => suffix.Length > 0 ? string.Concat(name, " ", suffix) : name
		};

		PrivateOverheadMessage(MessageType.Label, hue, AsciiClickMessage, val, from.NetState);
	}

	public static string[] GuildTypes = {
		"",
		" (Chaos)",
		" (Order)"
	};

	public bool CheckSkill(SkillName skill, double minSkill, double maxSkill)
	{
		return SkillCheckLocationHandler != null && SkillCheckLocationHandler(this, skill, minSkill, maxSkill);
	}

	public bool CheckSkill(SkillName skill, double chance)
	{
		return SkillCheckDirectLocationHandler != null && SkillCheckDirectLocationHandler(this, skill, chance);
	}

	public bool CheckTargetSkill(SkillName skill, object target, double minSkill, double maxSkill)
	{
		return SkillCheckTargetHandler != null && SkillCheckTargetHandler(this, skill, target, minSkill, maxSkill);
	}

	public bool CheckTargetSkill(SkillName skill, object target, double chance)
	{
		return SkillCheckDirectTargetHandler != null && SkillCheckDirectTargetHandler(this, skill, target, chance);
	}

	public virtual void DisruptiveAction()
	{
		if (!Meditating) return;
		Meditating = false;
		SendLocalizedMessage(500134); // You stop meditating.
	}

	#region Armor
	public Item ShieldArmor => FindItemOnLayer(Layer.TwoHanded);

	public Item NeckArmor => FindItemOnLayer(Layer.Neck);

	public Item HandArmor => FindItemOnLayer(Layer.Gloves);

	public Item HeadArmor => FindItemOnLayer(Layer.Helm);

	public Item ArmsArmor => FindItemOnLayer(Layer.Arms);

	public Item LegsArmor
	{
		get
		{
			Item ar = FindItemOnLayer(Layer.InnerLegs) ?? FindItemOnLayer(Layer.Pants);

			return ar;
		}
	}

	public Item ChestArmor
	{
		get
		{
			Item ar = FindItemOnLayer(Layer.InnerTorso) ?? FindItemOnLayer(Layer.Shirt);

			return ar;
		}
	}

	public Item Talisman => FindItemOnLayer(Layer.Talisman);
	#endregion

	/// <summary>
	///     Gets or sets the maximum attainable value for <see cref="RawStr" />, <see cref="RawDex" />, and <see cref="RawInt" />.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public int StatCap
	{
		get => _statCap;
		set
		{
			if (_statCap != value)
			{
				var old = _statCap;

				_statCap = value;

				if (old != _statCap)
				{
					EventSink.InvokeOnStatCapChange(this, old, _statCap);
				}

				Delta(MobileDelta.StatCap);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int StrCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DexCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int StrMaxCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DexMaxCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int IntMaxCap { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool Meditating { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanSwim { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CantWalk { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanHearGhosts
	{
		get => _canHearGhosts || AccessLevel >= AccessLevel.Counselor;
		set => _canHearGhosts = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int RawStatTotal => RawStr + RawDex + RawInt;

	public long NextSpellTime { get; set; }

	// XXX LOS BEGIN
	public Dictionary<IEntity, IEntity> LosCurrent { get; }//object
	//public TemporalCache<Object,Object> LosRecent { get { return m_LosRecent; } }
	// XXX LOS END

	/// <summary>
	/// Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Activate">activated</see>.
	/// </summary>
	public virtual void OnSectorActivate()
	{
	}

	/// <summary>
	/// Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Deactivate">deactivated</see>.
	/// </summary>
	public virtual void OnSectorDeactivate()
	{
	}
}
