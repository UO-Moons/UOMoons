using Server.ContextMenus;
using Server.Engines.PartySystem;
using Server.Engines.Quests;
using Server.Engines.Quests.Haven;
using Server.Engines.XmlSpawner2;
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
using Server.Spells.Necromancy;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.Mobiles;

public partial class BaseCreature : BaseMobile, IHonorTarget
{
	#region Settings
	public const int MaxLoyalty = 100;
	public virtual bool ForceStayHome => false;
	public virtual bool ArcherRequireArrows => true;
	public virtual bool CanBeParagon => true;
	public virtual bool CanBeBlackRock => true;
	public virtual bool CanBeSupreme => true;
	public virtual int FactionSilverWorth => 30;
	public bool IsGolem => this is IRepairableMobile;
	public virtual bool HasManaOveride => false;
	public virtual FoodType FavoriteFood => FoodType.Meat;
	public virtual PackInstinct PackInstinct => PackInstinct.None;
	public virtual bool AllowMaleTamer => true;
	public virtual bool AllowFemaleTamer => true;
	public virtual bool SubdueBeforeTame => false;
	public virtual bool StatLossAfterTame => SubdueBeforeTame;
	public virtual bool ReduceSpeedWithDamage => true;
	public virtual bool IsSubdued => SubdueBeforeTame && (Hits < (HitsMax / 10));

	public virtual bool Commandable => true;

	public virtual Poison HitPoison => null;
	public virtual double HitPoisonChance => 0.5;
	public virtual Poison PoisonImmune => null;

	public virtual bool BardImmune => false;
	public virtual bool Unprovokable => BardImmune || IsDeadPet;
	public virtual bool Uncalmable => BardImmune || IsDeadPet;
	public virtual bool AreaPeaceImmune => BardImmune || IsDeadPet;

	public virtual bool BleedImmune => false;
	public virtual double BonusPetDamageScalar => 1.0;
	public virtual bool AllureImmunity => false;
	public virtual bool DeathAdderCharmable => false;
	public virtual bool GivesFameAndKarmaAward => true;

	//TODO: Find the pub 31 tweaks to the DispelDifficulty and apply them of course.
	public virtual double DispelDifficulty => 0.0;  // at this skill level we dispel 50% chance
	public virtual double DispelFocus => 20.0;  // at difficulty - focus we have 0%, at difficulty + focus we have 100%
	public virtual bool DisplayWeight => Backpack is PackAnimalsBackpack;

	public virtual double TeleportChance => 0.05;
	public virtual bool AttacksFocus => false;
	public virtual bool ShowSpellMantra => false;
	public virtual bool FreezeOnCast => ShowSpellMantra;
	public virtual bool CanFly => false;

	// Must be overriden in subclass to enable
	public virtual bool HasBreath => false;

	// Base damage given is: CurrentHitPoints * BreathDamageScalar
	public virtual double BreathDamageScalar => Core.AOS ? 0.16 : 0.05;

	// Min/max seconds until next breath
	public virtual double BreathMinDelay => 30.0;
	public virtual double BreathMaxDelay => 45.0;

	// Creature stops moving for 1.0 seconds while breathing
	public virtual double BreathStallTime => 1.0;

	// Effect is sent 1.3 seconds after BreathAngerSound and BreathAngerAnimation is played
	public virtual double BreathEffectDelay => 1.3;

	// Damage is given 1.0 seconds after effect is sent
	public virtual double BreathDamageDelay => 1.0;

	public virtual int BreathRange => RangePerception;

	// Damage types
	public virtual int BreathChaosDamage => 0;
	public virtual int BreathPhysicalDamage => 0;
	public virtual int BreathFireDamage => 100;
	public virtual int BreathColdDamage => 0;
	public virtual int BreathPoisonDamage => 0;
	public virtual int BreathEnergyDamage => 0;

	public virtual bool TaintedLifeAura => false;
	// Is immune to breath damages
	public virtual bool BreathImmune => false;

	// Effect details and sound
	public virtual int BreathEffectItemID => 0x36D4;
	public virtual int BreathEffectSpeed => 5;
	public virtual int BreathEffectDuration => 0;
	public virtual bool BreathEffectExplodes => false;
	public virtual bool BreathEffectFixedDir => false;
	public virtual int BreathEffectHue => 0;
	public virtual int BreathEffectRenderMode => 0;

	public virtual int BreathEffectSound => 0x227;

	// Anger sound/animations
	public virtual int BreathAngerSound => GetAngerSound();
	public virtual int BreathAngerAnimation => 12;
	public virtual bool GivesMlMinorArtifact => false;
	private static readonly bool PvMLogEnabled = Settings.Configuration.Get<bool>("Misc", "PvMLogEnabled");
	private static readonly bool m_BondingEnabled = Settings.Configuration.Get<bool>("Mobiles", "BondingEnabled", true);
	private static readonly TimeSpan m_BondingDelay = TimeSpan.FromDays(Settings.Configuration.Get<double>("Mobiles", "BondingDelay", 7.0));
	private static readonly TimeSpan m_BondingAbandonDelay = TimeSpan.FromDays(Settings.Configuration.Get<double>("Mobiles", "BondingAbandonDelay", 1.0));
	public virtual bool IsBondable => m_BondingEnabled && !Summoned && !_mAllured && !IsGolem;
	public virtual TimeSpan BondingDelay => m_BondingDelay;
	public virtual TimeSpan BondingAbandonDelay => m_BondingAbandonDelay;
	public override bool CanRegenHits => !IsDeadPet && base.CanRegenHits;
	public override bool CanRegenStam => !IsParagon && !IsDeadPet && base.CanRegenStam;
	public override bool CanRegenMana => !IsDeadPet && base.CanRegenMana;
	public override bool IsDeadBondedPet => IsDeadPet;
	public virtual bool HoldSmartSpawning => IsParagon || IsBlackRock || IsSupreme;
	public virtual double SwitchWeaponChance => Body.IsHuman ? 0.1 : 0.0;
	public virtual double SwitchWepSkillVal => 50;
	public virtual double WeaponAbilityChance => 0.4;
	public virtual int SuperAiIntensity => Fame < 10000 ? 30 : 60;  //bigger the number, more often it does AI abilities based on mana %
	public virtual int SuperAiDelayMax => IsSupreme ? 1 : 6;  //max delay between each AI action in seconds
	public virtual TimeSpan SuperAiHealingDelay => TimeSpan.FromSeconds(5);
	public virtual bool IsMlBoss => false;   //weather to drop ml arty or not
	public virtual bool NoFlee => false;
	public virtual double TargetTamerChance => 0.007;  //chance to attack tamer (per damage dealt)
	public virtual double TargetSwitchChance => 0.05;  //chance to switch target during combat
	public virtual double ArtyChance => 0.0;  //used in some arty systems to override base formula
	public virtual int ArtyChanceInt => (int)(ArtyChance * 100);  //int version of above, should not be overwritten
	public virtual int EntryAnimation => 12; //animations
	public virtual int ArcaneLevel => 0;  //arcane focus level
	private bool m_dispelonsummonerdeath = false;
	public virtual bool CanFlee => !IsParagon || !IsBlackRock || !IsSupreme;
	public virtual bool IsInvulnerable => false;
	public virtual bool CanStealth => false;
	public virtual bool SupportsRunAnimation => true;
	public const int MaxOwners = 5;
	public virtual double TreasureMapChance => TreasureMap.LootChance;
	public virtual int TreasureMapLevel => -1;
	public virtual bool IgnoreYoungProtection => false;
	public virtual TimeSpan ReacquireDelay => TimeSpan.FromSeconds(10.0);
	public virtual bool ReacquireOnMovement => false;
	public virtual bool AcquireOnApproach => IsParagon || IsBlackRock || IsSupreme || ApproachWait;
	public virtual int AcquireOnApproachRange => ApproachRange;
	public virtual bool AllowNewPetFriend => Friends == null || Friends.Count < 5;
	public virtual OppositionGroup OppositionGroup => null;
	public virtual Ethics.Ethic EthicAllegiance => null;
	public virtual Faction FactionAllegiance => null;
	public virtual InhumanSpeech SpeechType => null;
	public virtual bool PlayerRangeSensitive => CurrentWayPoint == null;  //If they are following a waypoint, they'll continue to follow it even if players aren't around
	public virtual bool ReturnsToHome => SeeksHome && (Home != Point3D.Zero) && !m_ReturnQueued && !Controlled && !Summoned;
	private static readonly bool EnableRummaging = true;
	private const double ChanceToRummage = 0.5; // 50%
	private const double MinutesToNextRummageMin = 1.0;
	private const double MinutesToNextRummageMax = 4.0;
	private const double MinutesToNextChanceMin = 0.25;
	private const double MinutesToNextChanceMax = 0.75;
	public virtual bool CanBreath => HasBreath && !Summoned;
	public virtual bool IsDispellable => Summoned && !IsAnimatedDead;
	public virtual bool CanHeal => false;
	public virtual bool CanHealOwner => false;
	public virtual double HealScalar => 1.0;
	public virtual int HealSound => 0x57;
	public virtual int HealStartRange => 2;
	public virtual int HealEndRange => RangePerception;
	public virtual double HealTrigger => 0.78;
	public virtual double HealDelay => 6.5;
	public virtual double HealInterval => 0.0;
	public virtual bool HealFully => true;
	public virtual double HealOwnerTrigger => 0.78;
	public virtual double HealOwnerDelay => 6.5;
	public virtual double HealOwnerInterval => 30.0;
	public virtual bool HealOwnerFully => false;
	private long m_NextHealTime = Core.TickCount;
	private long m_NextHealOwnerTime = Core.TickCount;
	private Timer m_HealTimer = null;
	public bool IsHealing => m_HealTimer != null;
	public virtual bool HasAura => false;
	public virtual TimeSpan AuraInterval => TimeSpan.FromSeconds(5);
	public virtual int AuraRange => 4;
	public virtual int AuraBaseDamage => 5;
	public virtual int AuraPhysicalDamage => 0;
	public virtual int AuraFireDamage => 100;
	public virtual int AuraColdDamage => 0;
	public virtual int AuraPoisonDamage => 0;
	public virtual int AuraEnergyDamage => 0;
	public virtual int AuraChaosDamage => 0;
	public virtual bool TeleportsTo => false;
	public virtual TimeSpan TeleportDuration => TimeSpan.FromSeconds(5);
	public virtual int TeleportRange => 16;
	public virtual double TeleportProb => 0.25;
	public virtual bool TeleportsPets => false;
	public virtual bool CanDetectHidden => Controlled && Skills.DetectHidden.Value > 0;
	public virtual int FindPlayerDelayBase => 15000 / Int;
	public virtual int FindPlayerDelayMax => 60;
	public virtual int FindPlayerDelayMin => 5;
	public virtual int FindPlayerDelayHigh => 10;
	public virtual int FindPlayerDelayLow => 9;
	#endregion

	#region Var declarations
	private AIType m_CurrentAI;         // The current AI
	private AIType m_DefaultAI;         // The default AI
	private int m_iTeam;                // Monster Team
	private double m_dCurrentSpeed;     // The current speed, lets say it could be changed by something;
	private Point3D m_pHome;                // The home position of the creature, used by some AI
	private readonly List<Type> m_arSpellAttack;     // List of attack spell/power
	private readonly List<Type> m_arSpellDefense;        // List of defensive spell/power
	private bool m_bControlled;     // Is controlled
	private Mobile m_ControlMaster; // My master
	private Point3D m_ControlDest;      // My target destination (patrol)
	private OrderType m_ControlOrder;       // My order
	private int m_Loyalty;
	private bool m_bTamable;
	private bool m_bSummoned;
	private Mobile m_SummonMaster;
	private int m_DamageMin = -1;
	private int m_DamageMax = -1;
	private int m_PhysicalResistance;
	private int m_FireResistance;
	private int m_ColdResistance;
	private int m_PoisonResistance;
	private int m_EnergyResistance;
	private bool m_IsStabled;
	private Mobile m_InitialFocus;
	private bool m_Paragon;
	public bool IsArenaMob { get; set; } = false;
	private int m_FailedReturnHome; /* return to home failure counter */
	private bool m_IsChampionSpawn;
	private bool m_IsSupreme;
	private bool m_BlackRock;
	private string _mEngravedText;
	private bool _mIsBonded;
	private long m_NextBreathTime;
	private bool _mAllured;
	/* until we are sure about who should be getting deleted, move them instead */
	/* On OSI, they despawn */
	private bool m_ReturnQueued;
	private long _NextDetect;
	private long m_NextTeleport;
	private long m_NextAura;
	private long m_NextRummageTime;
	#endregion

	#region Get/Set
	[CommandProperty(AccessLevel.GameMaster)]
	public bool RemoveIfUntamed { get; set; }
	// used for deleting untamed creatures [in houses]
	[CommandProperty(AccessLevel.GameMaster)]
	public int RemoveStep { get; set; }
	// used for deleting untamed creatures [on save]
	[CommandProperty(AccessLevel.GameMaster)]
	public bool RemoveOnSave { get; set; }
	public bool IsAmbusher { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool HasGeneratedLoot { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool SeeksHome { get; set; }
	public int FollowRange { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public string CorpseNameOverride { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool Allured
	{
		get => _mAllured;
		set
		{
			_mAllured = value;

			if (value && Backpack != null)
			{
				ColUtility.SafeDelete(Backpack.Items);
			}
		}
	}
	[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
	public bool IsStabled
	{
		get => m_IsStabled;
		set
		{
			m_IsStabled = value;
			if (m_IsStabled)
				StopDeleteTimer();
		}
	}
	[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
	public Mobile StabledBy { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsPrisoner { get; set; }
	protected DateTime SummonEnd { get; set; }
	public UnsummonTimer UnsummonTimer { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual Spawner MySpawner
	{
		get
		{
			if (Spawner is Spawner spawner)
			{
				return spawner;
			}

			return null;
		}
		set
		{
		}
	}
	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile LastOwner
	{
		get
		{
			if (Owners == null || Owners.Count == 0)
				return null;

			return Owners[^1];
		}
	}
	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsBonded
	{
		get => _mIsBonded;
		set { _mIsBonded = value; InvalidateProperties(); }
	}
	public bool IsDeadPet { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime BondingBegin { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime OwnerAbandonTime { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public string EngravedText
	{
		get => _mEngravedText != null ? Utility.FixHtml(_mEngravedText) : null;
		set
		{
			_mEngravedText = value;
			InvalidateProperties();
		}
	}
	[CommandProperty(AccessLevel.GameMaster)]
	public bool Dispelonsummonerdeath
	{
		get => m_dispelonsummonerdeath;
		set { m_dispelonsummonerdeath = value; InvalidateProperties(); }
	}
	public DateTime EndFleeTime { get; set; }
	public BaseAI AIObject { get; private set; }
	public List<Mobile> Friends { get; private set; }
	public List<Mobile> Owners { get; private set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool NoKillAwards { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public bool NoLootOnDeath { get; set; }
	public bool GivenSpecialArtifact { get; set; }
	/* To save on cpu usage, UOMoons creatures only reacquire creatures under the following circumstances:
	 *  - 10 seconds have elapsed since the last time it tried
	 *  - The creature was attacked
	 *  - Some creatures, like dragons, will reacquire when they see someone move
	 *
	 * This functionality appears to be implemented on OSI as well
	 */
	public long NextReacquireTime { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ApproachWait { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ApproachRange { get; set; }
	public bool RecentSetControl { get; set; }
	public static bool Summoning { get; set; }
	#region Elemental Resistance/Damage
	public override int BasePhysicalResistance => m_PhysicalResistance;
	public override int BaseFireResistance => m_FireResistance;
	public override int BaseColdResistance => m_ColdResistance;
	public override int BasePoisonResistance => m_PoisonResistance;
	public override int BaseEnergyResistance => m_EnergyResistance;

	[CommandProperty(AccessLevel.GameMaster)]
	public int PhysicalResistanceSeed { get => m_PhysicalResistance; set { m_PhysicalResistance = value; UpdateResistances(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int FireResistSeed { get => m_FireResistance; set { m_FireResistance = value; UpdateResistances(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ColdResistSeed { get => m_ColdResistance; set { m_ColdResistance = value; UpdateResistances(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int PoisonResistSeed { get => m_PoisonResistance; set { m_PoisonResistance = value; UpdateResistances(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EnergyResistSeed { get => m_EnergyResistance; set { m_EnergyResistance = value; UpdateResistances(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int PhysicalDamage { get; set; } = 100;

	[CommandProperty(AccessLevel.GameMaster)]
	public int FireDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ColdDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int PoisonDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EnergyDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ChaosDamage { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DirectDamage { get; set; }
	#endregion
	#endregion

	#region regans
	public virtual int DefaultHitsRegen
	{
		get
		{
			int regen = 0;

			if (IsAnimatedDead)
			{
				regen = 4;
			}

			if (IsParagon)
			{
				regen += 40;
			}

			if (IsBlackRock)
			{
				regen += 30;
			}

			if (IsSupreme)
			{
				regen += 50;
			}

			//regen += HumilityVirtue.GetRegenBonus(this);

			return regen;
		}
	}

	public virtual int DefaultStamRegen
	{
		get
		{
			int regen = 0;

			if (IsParagon)
			{
				regen += 40;
			}

			if (IsBlackRock)
			{
				regen += 30;
			}

			if (IsSupreme)
			{
				regen += 50;
			}

			return regen;
		}
	}

	public virtual int DefaultManaRegen
	{
		get
		{
			int regen = 0;

			if (IsParagon)
			{
				regen += 40;
			}

			if (IsBlackRock)
			{
				regen += 30;
			}

			if (IsSupreme)
			{
				regen += 50;
			}

			return regen;
		}
	}
	#endregion

	#region Delete Previously Tamed Timer
	private DeleteTimer _mDeleteTimer;

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan DeleteTimeLeft
	{
		get
		{
			if (_mDeleteTimer is {Running: true})
				return _mDeleteTimer.Next - DateTime.UtcNow;

			return TimeSpan.Zero;
		}
	}

	private class DeleteTimer : Timer
	{
		private readonly Mobile m;

		public DeleteTimer(Mobile creature, TimeSpan delay) : base(delay)
		{
			m = creature;
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			m.Delete();
		}
	}

	public void BeginDeleteTimer()
	{
		if (this is not BaseEscortable && !Summoned && !Deleted && !IsStabled)
		{
			StopDeleteTimer();
			_mDeleteTimer = new DeleteTimer(this, TimeSpan.FromDays(3.0));
			_mDeleteTimer.Start();
		}
	}

	public void StopDeleteTimer()
	{
		if (_mDeleteTimer != null)
		{
			_mDeleteTimer.Stop();
			_mDeleteTimer = null;
		}
	}

	#endregion

	public virtual bool SwapWeapon(double skillreq)
	{
		if (Backpack == null)
		{
			return false;
		}

		BaseWeapon curwep = BaseWeapon.FindEquippedWeapon(this);

		Item[] itemsByType = Backpack.FindItemsByType(typeof(BaseWeapon), false);

		if (itemsByType.Length == 0)
		{
			return false;
		}
		BaseWeapon newwep = null;

		for (int i = 0; i < 1 + itemsByType.Length / 2; i++) //try to find a wep that matches skill requirements
		{
			Item tmp = itemsByType[Utility.Random(itemsByType.Length)];

			if (tmp is BaseWeapon weapon && weapon is not Fists)
			{
				newwep = weapon;
			}
			else
			{
				continue;
			}

			if (Skills[newwep.GetUsedSkill(this, true)].Value >= skillreq)
			{
				break;
			}
			newwep = null;
		}

		if (newwep != null)
		{
			Backpack.AddItem(curwep);
			EquipItem(newwep);
		}

		return true;
	}

	public bool HasLongRangeWep()
	{
		Item item = FindItemOnLayer(Layer.TwoHanded);

		if (item is BaseRanged)
		{
			return true;
		}

		//just in case, we'll check one handed as well
		item = FindItemOnLayer(Layer.OneHanded);

		if (item is BaseRanged)
		{
			return true;
		}

		return false;
	}

	public bool HasMeleeWep()
	{
		Item item = FindItemOnLayer(Layer.TwoHanded);

		if (item is BaseWeapon and not BaseRanged)
		{
			return true;
		}

		item = FindItemOnLayer(Layer.OneHanded);

		if (item is BaseWeapon and not BaseRanged)
		{
			return true;
		}

		return false;
	}

	public bool HasWep()
	{
		Item item = FindItemOnLayer(Layer.TwoHanded);

		if (item is BaseWeapon)
		{
			return true;
		}

		item = FindItemOnLayer(Layer.OneHanded);

		if (item is BaseWeapon)
		{
			return true;
		}

		return false;
	}

	public virtual WeaponAbility GetWeaponAbility()
	{
		return null;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsBlackRock
	{
		get => m_BlackRock;
		set
		{
			if (m_BlackRock == value)
			{
				return;
			}

			if (value)
			{
				BlackRockInfected.Convert(this);
			}
			else
			{
				BlackRockInfected.UnConvert(this);
			}

			m_BlackRock = value;

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsSupreme
	{
		get => m_IsSupreme;
		set
		{
			if (m_IsSupreme == value)
			{
				return;
			}

			if (value)
			{
				SupremeCreature.Convert(this);
			}
			else
			{
				SupremeCreature.UnConvert(this);
			}

			m_IsSupreme = value;

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsParagon
	{
		get => m_Paragon;
		set
		{
			if (m_Paragon == value)
				return;
			if (value)
				Paragon.Convert(this);
			else
				Paragon.UnConvert(this);

			m_Paragon = value;

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsChampionSpawn
	{
		get => m_IsChampionSpawn;
		set
		{
			if (m_IsChampionSpawn != value)
			{
				if (!m_IsChampionSpawn && value)
					SetToChampionSpawn();

				m_IsChampionSpawn = value;

				OnChampionSpawnChange();
			}
		}
	}

	protected virtual void OnChampionSpawnChange()
	{ }

	public virtual void SetToChampionSpawn()
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile InitialFocus
	{
		get
		{
			if (m_InitialFocus != null && (!m_InitialFocus.Alive || m_InitialFocus.Deleted))
			{
				m_InitialFocus = null;
			}

			return m_InitialFocus;
		}
		set => m_InitialFocus = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public override IDamageable Combatant
	{
		get => base.Combatant;
		set
		{
			Mobile initialFocus = InitialFocus;

			if (base.Combatant == null)
			{
				if (value is Mobile mobile && AttacksFocus)
				{
					InitialFocus = mobile;
				}
			}
			else if (AttacksFocus &&
					initialFocus != null &&
					value != initialFocus &&
					!initialFocus.Hidden &&
					Map == initialFocus.Map &&
					InRange(initialFocus.Location, RangePerception))
			{
				//Keeps focus
				base.Combatant = initialFocus;
				return;
			}

			base.Combatant = value;
		}
	}

	public static void SetTempSummon(Mobile summoner, Mobile tochange)
	{
		if (tochange is BaseCreature)
		{
			BaseCreature bc = tochange as BaseCreature;
			bc.ControlMaster = summoner;
			bc.Dispelonsummonerdeath = true;
			if (summoner is BaseCreature)
			{
				bc.Team = ((BaseCreature)summoner).Team;
			}
		}
	}

	public virtual void OnDrainLife(Mobile victim)
	{
	}

	#region Breath ability
	public virtual void BreathStart(Mobile target)
	{
		BreathStallMovement();
		BreathPlayAngerSound();
		BreathPlayAngerAnimation();

		Direction = GetDirectionTo(target);

		_ = Timer.DelayCall(TimeSpan.FromSeconds(BreathEffectDelay), new TimerStateCallback(BreathEffect_Callback), target);
	}

	public virtual void BreathStallMovement()
	{
		if (AIObject != null)
			AIObject.NextMove = Core.TickCount + (int)(BreathStallTime * 1000);
	}

	public virtual void BreathPlayAngerSound()
	{
		PlaySound(BreathAngerSound);
	}

	public virtual void BreathPlayAngerAnimation()
	{
		Animate(BreathAngerAnimation, 5, 1, true, false, 0);
	}

	public virtual void BreathEffect_Callback(object state)
	{
		Mobile target = (Mobile)state;

		if (!target.Alive || !CanBeHarmful(target))
			return;

		BreathPlayEffectSound();
		BreathPlayEffect(target);

		_ = Timer.DelayCall(TimeSpan.FromSeconds(BreathDamageDelay), new TimerStateCallback(BreathDamage_Callback), target);
	}

	public virtual void BreathPlayEffectSound()
	{
		PlaySound(BreathEffectSound);
	}

	public virtual void BreathPlayEffect(Mobile target)
	{
		Effects.SendMovingEffect(this, target, BreathEffectItemID,
			BreathEffectSpeed, BreathEffectDuration, BreathEffectFixedDir,
			BreathEffectExplodes, BreathEffectHue, BreathEffectRenderMode);
	}

	public virtual void BreathDamage_Callback(object state)
	{
		Mobile target = (Mobile)state;

		if (target is BaseCreature creature && creature.BreathImmune)
			return;

		if (CanBeHarmful(target))
		{
			DoHarmful(target);
			BreathDealDamage(target);
		}
	}

	public virtual void BreathDealDamage(Mobile target)
	{
		if (!Evasion.CheckSpellEvasion(target))
		{
			int physDamage = BreathPhysicalDamage;
			int fireDamage = BreathFireDamage;
			int coldDamage = BreathColdDamage;
			int poisDamage = BreathPoisonDamage;
			int nrgyDamage = BreathEnergyDamage;

			if (BreathChaosDamage > 0)
			{
				switch (Utility.Random(5))
				{
					case 0: physDamage += BreathChaosDamage; break;
					case 1: fireDamage += BreathChaosDamage; break;
					case 2: coldDamage += BreathChaosDamage; break;
					case 3: poisDamage += BreathChaosDamage; break;
					case 4: nrgyDamage += BreathChaosDamage; break;
				}
			}

			if (physDamage == 0 && fireDamage == 0 && coldDamage == 0 && poisDamage == 0 && nrgyDamage == 0)
			{
				target.Damage(BreathComputeDamage(), this);// Unresistable damage even in AOS
			}
			else
			{
				_ = AOS.Damage(target, this, BreathComputeDamage(), physDamage, fireDamage, coldDamage, poisDamage, nrgyDamage);
			}
		}
	}

	public virtual int BreathComputeDamage()
	{
		int damage = (int)(Hits * BreathDamageScalar);

		if (IsParagon)
			damage = (int)(damage / Paragon.HitsBuff);

		if (damage > 200)
			damage = 200;

		return damage;
	}
	#endregion

	#region Spill Acid
	public void SpillAcid(int Amount)
	{
		SpillAcid(null, Amount);
	}

	public void SpillAcid(Mobile target, int Amount)
	{
		if ((target != null && target.Map == null) || Map == null)
			return;

		for (int i = 0; i < Amount; ++i)
		{
			Point3D loc = Location;
			Map map = Map;
			Item acid = NewHarmfulItem();

			if (target != null && target.Map != null && Amount == 1)
			{
				loc = target.Location;
				map = target.Map;
			}
			else
			{
				bool validLocation = false;
				for (int j = 0; !validLocation && j < 10; ++j)
				{
					loc = new Point3D(
						loc.X + (Utility.Random(0, 3) - 2),
						loc.Y + (Utility.Random(0, 3) - 2),
						loc.Z);
					loc.Z = map.GetAverageZ(loc.X, loc.Y);
					validLocation = map.CanFit(loc, 16, false, false);
				}
			}
			acid.MoveToWorld(loc, map);
		}
	}

	/*
		Solen Style, override me for other mobiles/items:
		kappa+acidslime, grizzles+whatever, etc.
	*/

	public virtual Item NewHarmfulItem()
	{
		return new PoolOfAcid(TimeSpan.FromSeconds(10), 30, 30);
	}

	#endregion

	#region Flee!!!
	public virtual void StopFlee()
	{
		EndFleeTime = DateTime.MinValue;
	}

	public virtual bool CheckFlee()
	{
		if (EndFleeTime == DateTime.MinValue)
			return false;

		if (DateTime.UtcNow >= EndFleeTime)
		{
			StopFlee();
			return false;
		}

		return true;
	}

	public virtual void BeginFlee(TimeSpan maxDuration)
	{
		EndFleeTime = DateTime.UtcNow + maxDuration;
	}

	#endregion

	#region Friends
	public virtual bool IsPetFriend(Mobile m)
	{
		return Friends != null && Friends.Contains(m);
	}

	public virtual void AddPetFriend(Mobile m)
	{
		if (Friends == null)
			Friends = new List<Mobile>();

		Friends.Add(m);
	}

	public virtual void RemovePetFriend(Mobile m)
	{
		if (Friends != null)
			_ = Friends.Remove(m);
	}

	public virtual bool IsFriend(Mobile m)
	{
		OppositionGroup g = OppositionGroup;

		if (g != null && g.IsEnemy(this, m))
			return false;

		if (!(m is BaseCreature))
			return false;

		BaseCreature c = (BaseCreature)m;

		return (m_iTeam == c.m_iTeam && ((m_bSummoned || m_bControlled) == (c.m_bSummoned || c.m_bControlled))/* && c.Combatant != this */);
	}

	#endregion

	#region Allegiance
	public enum Allegiance
	{
		None,
		Ally,
		Enemy
	}

	public virtual Allegiance GetFactionAllegiance(Mobile mob)
	{
		if (mob == null || mob.Map != Faction.Facet || FactionAllegiance == null)
			return Allegiance.None;

		Faction fac = Faction.Find(mob, true);

		if (fac == null)
			return Allegiance.None;

		return (fac == FactionAllegiance ? Allegiance.Ally : Allegiance.Enemy);
	}

	public virtual Allegiance GetEthicAllegiance(Mobile mob)
	{
		if (mob == null || mob.Map != Faction.Facet || EthicAllegiance == null)
			return Allegiance.None;

		Ethics.Ethic ethic = Ethics.Ethic.Find(mob, true);

		if (ethic == null)
			return Allegiance.None;

		return (ethic == EthicAllegiance ? Allegiance.Ally : Allegiance.Enemy);
	}

	#endregion

	public virtual bool IsEnemy(Mobile m)
	{
		XmlIsEnemy a = (XmlIsEnemy)XmlAttach.FindAttachment(this, typeof(XmlIsEnemy));

		if (a != null)
		{
			return a.IsEnemy(m);
		}

		OppositionGroup g = OppositionGroup;

		if (g != null && g.IsEnemy(this, m))
			return true;

		if (m is BaseGuard || GetFactionAllegiance(m) == Allegiance.Ally)
			return false;

		Ethics.Ethic ourEthic = EthicAllegiance;
		Ethics.Player pl = Ethics.Player.Find(m, true);

		if (pl != null && pl.IsShielded && (ourEthic == null || ourEthic == pl.Ethic))
			return false;

		if (m is not BaseCreature || m is MilitiaFighter)
			return true;

		if (TransformationSpellHelper.UnderTransformation(m, typeof(EtherealVoyageSpell)) || m is PlayerMobile pm && pm.HonorActive)
			return false;

		BaseCreature c = (BaseCreature)m;

		//if ((FightMode == FightMode.Evil && m.Karma < 0) || (c.FightMode == FightMode.Evil && Karma < 0))
		//return true;
		// Are we a non-aggressive FightMode or are they an uncontrolled Summon?
		if (FightMode == FightMode.Aggressor || FightMode == FightMode.Evil || FightMode == FightMode.Good || (c != null && c.m_bSummoned && !c.m_bControlled && c.SummonMaster != null))
		{
			// Faction Opposed Players/Pets are my enemies
			if (GetFactionAllegiance(m) == Allegiance.Enemy)
			{
				return true;
			}

			// Ethic Opposed Players/Pets are my enemies
			if (GetEthicAllegiance(m) == Allegiance.Enemy)
			{
				return true;
			}

			// Negative Karma are my enemies
			if (FightMode == FightMode.Evil)
			{
				if (c != null && c.GetMaster() != null)
				{
					return c.GetMaster().Karma < 0;
				}

				return m.Karma < 0;
			}

			// Positive Karma are my enemies
			if (FightMode == FightMode.Good)
			{
				if (c != null && c.GetMaster() != null)
				{
					return c.GetMaster().Karma > 0;
				}

				return m.Karma > 0;
			}

			// Others are not my enemies
			return false;
		}

		//if (m is PlayerMobile pm)
		//{
		//    if (pm.Combatant != null && pm.KRStartingQuestStep == 24)
		//    {
		//        return true;
		//    }

		//   if (pm.KRStartingQuestStep > 0)
		//    {
		//        return false;
		//   }
		// }

		//if (!Controlled && BaseMaskOfDeathPotion.UnderEffect(m))
		//{
		//	return false;
		//}

		BaseCreature t = this;

		// Summons should have same rules as their master
		if (c.m_bSummoned && c.SummonMaster != null && c.SummonMaster is BaseCreature)
		{
			c = c.SummonMaster as BaseCreature;
		}

		// Summons should have same rules as their master
		if (t.m_bSummoned && t.SummonMaster != null && t.SummonMaster is BaseCreature)
		{
			t = t.SummonMaster as BaseCreature;
		}

		// Creatures on other teams are my enemies
		if (t.m_iTeam != c.m_iTeam)
		{
			return true;
		}

		return ((t.m_bSummoned && t.SummonMaster != null) || t.m_bControlled) != ((c.m_bSummoned && c.SummonMaster != null) || c.m_bControlled);
		//return m_iTeam != c.m_iTeam || ((m_bSummoned || m_bControlled) != (c.m_bSummoned || c.m_bControlled))/* || c.Combatant == this*/ ;
	}

	public override string ApplyNameSuffix(string suffix)
	{
		if (IsParagon && IsSupreme)
		{
			if (suffix.Length == 0)
			{
				suffix = "(Behemoth Paragon)";
			}
			else
			{
				suffix = string.Concat(suffix, " (Behemoth Paragon)");
			}
		}
		else if (IsParagon && !GivesMlMinorArtifact)
		{
			if (suffix.Length == 0)
			{
				suffix = "(Paragon)";
			}
			else
			{
				suffix = string.Concat(suffix, " (Paragon)");
			}
		}
		if (IsBlackRock && IsSupreme)
		{
			if (suffix.Length == 0)
			{
				suffix = "(Behemoth BlackRock Infected)";
			}
			else
			{
				suffix = string.Concat(suffix, " (Behemoth BlackRock Infected)");
			}
		}
		else if (IsBlackRock)
		{
			suffix = suffix.Length == 0 ? " (BlackRock Infected)" : string.Concat(suffix, " (BlackRock Infected)");
		}
		else if (IsSupreme)
		{
			if (suffix.Length == 0)
			{
				suffix = "(Behemoth)";
			}
			else
			{
				suffix = string.Concat(suffix, " (Behemoth)");
			}
		}

		return base.ApplyNameSuffix(suffix);
	}

	public virtual bool CheckControlChance(Mobile m)
	{
		if (GetControlChance(m) > Utility.RandomDouble())
		{
			Loyalty += 1;
			return true;
		}

		PlaySound(GetAngerSound());

		if (Body.IsAnimal)
			Animate(10, 5, 1, true, false, 0);
		else if (Body.IsMonster)
			Animate(18, 5, 1, true, false, 0);

		Loyalty -= 3;
		return false;
	}

	public virtual bool CanBeControlledBy(Mobile m)
	{
		return GetControlChance(m) > 0.0;
	}

	public double GetControlChance(Mobile m)
	{
		return GetControlChance(m, false);
	}

	public virtual double GetControlChance(Mobile m, bool useBaseSkill)
	{
		if (MinTameSkill <= 29.1 || m_bSummoned || m.AccessLevel >= AccessLevel.GameMaster)
			return 1.0;

		double dMinTameSkill = MinTameSkill;

		if (dMinTameSkill > -24.9 && AnimalTaming.CheckMastery(m, this))
			dMinTameSkill = -24.9;

		int taming = (int)((useBaseSkill ? m.Skills[SkillName.AnimalTaming].Base : m.Skills[SkillName.AnimalTaming].Value) * 10);
		int lore = (int)((useBaseSkill ? m.Skills[SkillName.AnimalLore].Base : m.Skills[SkillName.AnimalLore].Value) * 10);
		int chance = 700;
		int bonus;
		if (Core.ML)
		{
			int SkillBonus = taming - (int)(dMinTameSkill * 10);
			int LoreBonus = lore - (int)(dMinTameSkill * 10);

			int SkillMod = 6, LoreMod = 6;

			if (SkillBonus < 0)
				SkillMod = 28;
			if (LoreBonus < 0)
				LoreMod = 14;

			SkillBonus *= SkillMod;
			LoreBonus *= LoreMod;

			bonus = (SkillBonus + LoreBonus) / 2;
		}
		else
		{
			int difficulty = (int)(dMinTameSkill * 10);
			int weighted = ((taming * 4) + lore) / 5;
			bonus = weighted - difficulty;

			if (bonus <= 0)
				bonus *= 14;
			else
				bonus *= 6;
		}

		chance += bonus;

		if (chance >= 0 && chance < 200)
			chance = 200;
		else if (chance > 990)
			chance = 990;

		chance -= (MaxLoyalty - m_Loyalty) * 10;

		return ((double)chance / 1000);
	}

	private static readonly Type[] m_AnimateDeadTypes = new Type[]
		{
			typeof( MoundOfMaggots ), typeof( HellSteed ), typeof( SkeletalMount ),
			typeof( WailingBanshee ), typeof( Wraith ), typeof( SkeletalDragon ),
			typeof( LichLord ), typeof( FleshGolem ), typeof( Lich ),
			typeof( SkeletalKnight ), typeof( BoneKnight ), typeof( Mummy ),
			typeof( SkeletalMage ), typeof( BoneMagi ), typeof( PatchworkSkeleton )
		};

	public virtual bool IsAnimatedDead
	{
		get
		{
			if (!Summoned)
				return false;

			Type type = GetType();

			bool contains = false;

			for (int i = 0; !contains && i < m_AnimateDeadTypes.Length; ++i)
				contains = (type == m_AnimateDeadTypes[i]);

			return contains;
		}
	}

	public virtual bool IsNecroFamiliar
	{
		get
		{
			if (!Summoned)
				return false;

			if (m_ControlMaster != null && SummonFamiliarSpell.Table.Contains(m_ControlMaster))
				return SummonFamiliarSpell.Table[m_ControlMaster] == this;

			return false;
		}
	}

	/*public override void Damage(int amount, Mobile from)
	{
		int oldHits = Hits;

		if (Core.AOS && !Summoned && Controlled && 0.2 > Utility.RandomDouble())
			amount = (int)(amount * BonusPetDamageScalar);

		if (EvilOmenSpell.TryEndEffect(this))
			amount = (int)(amount * 1.25);

		Mobile oath = BloodOathSpell.GetBloodOath(from);

		if (oath == this)
		{
			amount = (int)(amount * 1.1);
			from.Damage(amount, from);
		}

		base.Damage(amount, from);

		if (SubdueBeforeTame && !Controlled)
		{
			if ((oldHits > (HitsMax / 10)) && (Hits <= (HitsMax / 10)))
				PublicOverheadMessage(MessageType.Regular, 0x3B2, false, "* The creature has been beaten into subjugation! *");
		}
	}*/

	public virtual bool DeleteCorpseOnDeath => !Core.AOS && m_bSummoned;

	public override void SetLocation(Point3D newLocation, bool isTeleport)
	{
		base.SetLocation(newLocation, isTeleport);

		if (isTeleport && AIObject != null)
			AIObject.OnTeleported();
	}

	public override void OnBeforeSpawn(Point3D location, Map m)
	{

		if (Utility.RandomDouble() < 0.30)
		{
			if (Paragon.CheckConvert(this, location, m))
			{
				IsParagon = true;
			}

			switch (Utility.Random(3))
			{
				case 0:
				case 1:
					if (BlackRockInfected.CheckConvert(this, location, m))
					{
						IsBlackRock = true;
					}
					break;
				case 2:
					if (SupremeCreature.CheckConvert(this, location, m))
					{
						IsSupreme = true;
					}
					break;
			}
		}

		//entry animation
		Pacify(this, DateTime.UtcNow + TimeSpan.FromSeconds(3));
		Animate(EntryAnimation, 16, 1, true, false, 0);
		base.OnBeforeSpawn(location, m);
	}

	public override ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
	{
		if (!Alive || IsDeadPet)
			return ApplyPoisonResult.Immune;

		if (EvilOmenSpell.TryEndEffect(this))
			poison = PoisonImpl.IncreaseLevel(poison);

		ApplyPoisonResult result = base.ApplyPoison(from, poison);

		if (from != null && result == ApplyPoisonResult.Poisoned && PoisonTimer is PoisonImpl.PoisonTimer)
			(PoisonTimer as PoisonImpl.PoisonTimer).From = from;

		return result;
	}

	public override bool CheckPoisonImmunity(Mobile from, Poison poison)
	{
		if (base.CheckPoisonImmunity(from, poison))
			return true;

		Poison p = PoisonImmune;
		XmlPoison xp = (XmlPoison)XmlAttach.FindAttachment(this, typeof(XmlPoison));

		if (m_Paragon)
			p = PoisonImpl.IncreaseLevel(p);

		if (xp != null)
		{
			p = xp.PoisonImmunity;
		}

		return p != null && p.Level >= poison.Level;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Loyalty
	{
		get => m_Loyalty;
		set => m_Loyalty = Math.Min(Math.Max(value, 0), MaxLoyalty);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public WayPoint CurrentWayPoint { get; set; } = null;

	[CommandProperty(AccessLevel.GameMaster)]
	public IPoint2D TargetLocation { get; set; } = null;

	public virtual Mobile ConstantFocus => null;

	public virtual bool DisallowAllMoves
	{
		get
		{
			XmlData x = (XmlData)XmlAttach.FindAttachment(this, typeof(XmlData), "NoSpecials");

			return x != null && x.Data == "True";
		}
	}

	public virtual bool InitialInnocent
	{
		get
		{
			XmlData x = (XmlData)XmlAttach.FindAttachment(this, typeof(XmlData), "Notoriety");

			return x != null && x.Data == "blue";
		}
	}

	public virtual bool AlwaysMurderer
	{
		get
		{
			XmlData x = (XmlData)XmlAttach.FindAttachment(this, typeof(XmlData), "Notoriety");

			return x != null && x.Data == "red";
		}
	}

	public virtual bool AlwaysAttackable
	{
		get
		{
			XmlData x = (XmlData)XmlAttach.FindAttachment(this, typeof(XmlData), "Notoriety");

			return x != null && x.Data == "gray";
		}
	}

	//public virtual bool DisallowAllMoves => false;

	//public virtual bool InitialInnocent => false;

	//public virtual bool AlwaysMurderer => false;

	//public virtual bool AlwaysAttackable => false;

	public virtual bool ForceNotoriety => false;

	public virtual bool UseSmartAI => false;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int DamageMin { get => m_DamageMin; set => m_DamageMin = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int DamageMax { get => m_DamageMax; set => m_DamageMax = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public override int HitsMax
	{
		get
		{
			if (HitsMaxSeed > 0)
			{
				int value = HitsMaxSeed + GetStatOffset(StatType.Str);

				if (value < 1)
					value = 1;
				else if (value > MaxStatValue)
					value = MaxStatValue;

				return value;
			}

			return Str;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitsMaxSeed { get; set; } = -1;

	[CommandProperty(AccessLevel.GameMaster)]
	public override int StamMax
	{
		get
		{
			if (StamMaxSeed > 0)
			{
				int value = StamMaxSeed + GetStatOffset(StatType.Dex);

				if (value < 1)
					value = 1;
				else if (value > MaxStatValue)
					value = MaxStatValue;

				return value;
			}

			return Dex;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int StamMaxSeed { get; set; } = -1;

	[CommandProperty(AccessLevel.GameMaster)]
	public override int ManaMax
	{
		get
		{
			if (ManaMaxSeed > 0)
			{
				int value = ManaMaxSeed + GetStatOffset(StatType.Int);

				if (value < 1)
					value = 1;
				else if (value > MaxStatValue)
					value = MaxStatValue;

				return value;
			}

			return Int;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ManaMaxSeed { get; set; } = -1;
	public virtual bool CanOpenDoors => !Body.IsAnimal && !Body.IsSea;
	public virtual bool CanMoveOverObstacles => Core.AOS || Body.IsMonster;
	public virtual bool CanDestroyObstacles => false;// to enable breaking of furniture, 'return CanMoveOverObstacles;'

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanMove { get; set; }

	public virtual bool CanCallGuards => !Deleted && Alive && !AlwaysMurderer && Kills < 5 && (Player || Body.IsHuman);

	public void Pacify(Mobile master, DateTime endtime)
	{
		BardMaster = master;
		BardPacified = true;
		BardEndTime = endtime;
	}

	public void Unpacify()
	{
		BardEndTime = DateTime.UtcNow;
		BardPacified = false;
	}

	public HonorContext ReceivedHonorContext { get; set; }

	/*

	Seems this actually was removed on OSI somewhere between the original bug report and now.
	We will call it ML, until we can get better information. I suspect it was on the OSI TC when
	originally it taken out of RunUO, and not implmented on OSIs production shards until more
	recently.  Either way, this is, or was, accurate OSI behavior, and just entirely
	removing it was incorrect.  OSI followers were distracted by being attacked well into
	AoS, at very least.

	*/

	public virtual bool CanBeDistracted => !Core.ML;

	public virtual void CheckDistracted(Mobile from)
	{
		if (Utility.RandomDouble() < .10)
		{
			ControlTarget = from;
			ControlOrder = OrderType.Attack;
			Combatant = from;
			Warmode = true;
		}
	}

	private DateTime lastsawmaster;
	private DateTime lastcheckedtamerlos;
	private int tamerhidingticks;

	[CommandProperty(AccessLevel.GameMaster)]
	public int MinutesSinceSawMaster
	{
		get
		{
			return (int)(DateTime.UtcNow.Subtract(lastsawmaster)).TotalMinutes;
		}
	}

	public void CheckSawTamer(bool uponly, int secsdelay)
	{
		//Testing code
		if (ControlMaster != null && (lastcheckedtamerlos + TimeSpan.FromSeconds(secsdelay)) < DateTime.UtcNow)
		{
			//Debugger.Write("petlos", "Checking {0} if {1} is in LOS...", this, ControlMaster);

			if ((!CanSee(ControlMaster) || !InLOS(ControlMaster) || !ControlMaster.Alive) && ((this is BaseMount mount && mount.Rider != ControlMaster) || this is not BaseMount))
			{
				//Debugger.Write("petlos", "Not in LOS");
				if (!uponly)
				{
					//Debugger.Write("petlos", "Dropping loyalty");
					Loyalty -= 0.12 > Utility.RandomDouble() && (Loyalty > 5) ? 1 : 0;
					if (tamerhidingticks < 10000)
						tamerhidingticks++;
				}
			}
			else
			{
				//Debugger.Write("petlos", "In LOS");
				if (tamerhidingticks > 25 && lastsawmaster < DateTime.UtcNow)
				{
					//Debugger.Write("petlos", "Decrementing penalty.");
					lastsawmaster += TimeSpan.FromSeconds(25);
					tamerhidingticks--;
				}
				else
				{
					//Debugger.Write("petlos", "Clearing penalty.");
					tamerhidingticks = 0;
					lastsawmaster = DateTime.UtcNow;
				}
			}
			lastcheckedtamerlos = DateTime.UtcNow;

			//Debugger.Write("petlos", "{0} loyalty: {1}  --  hide ticks: {2}  --  last saw master: {3}", this, Loyalty, tamerhidingticks, lastsawmaster);
		}
	}

	public virtual void OnBeforeDamage(Mobile from, ref int totalDamage, DamageType type)
	{
		if (type >= DamageType.Spell && RecentSetControl)
		{
			totalDamage = 0;
		}
	}

	public override void Bleed(Mobile attacker, int damage)
	{
		base.Bleed(attacker, damage, new Blood());
	}

	public override void OnDamage(int amount, Mobile from, bool willKill)
	{
		if (this is BaseVendor)
		{
			CheckHome();
		}

		if (BardPacified && (HitsMax - Hits) * 0.001 > Utility.RandomDouble())
			Unpacify();

		int disruptThreshold;
		//NPCs can use bandages too!
		if (!Core.AOS)
			disruptThreshold = 0;
		else if (from != null && from.Player)
			disruptThreshold = 18;
		else
			disruptThreshold = 25;

		if (amount > disruptThreshold)
		{
			BandageContext c = BandageContext.GetContext(this);

			if (c != null)
				c.Slip();
		}

		if (Confidence.IsRegenerating(this))
			Confidence.StopRegenerating(this);

		//WeightOverloading.FatigueOnDamage(this, amount);

		InhumanSpeech speechType = SpeechType;

		if (speechType != null && !willKill)
			speechType.OnDamage(this, amount);

		if (ReceivedHonorContext != null)
			ReceivedHonorContext.OnTargetDamaged(from, amount);

		CheckSawTamer(false, 1);

		if (!CombatLocked() && ControlMaster == null && SummonMaster == null && TargetTamerChance > 0 && from != null && from is BaseCreature creature && TargetTamerChance > Utility.RandomDouble())
		{
			Mobile tamer = null;

			if (creature.ControlMaster != null)
			{
				tamer = creature.ControlMaster;
			}
			else if (creature.SummonMaster != null)
			{
				tamer = creature.SummonMaster;
			}

			if (tamer != null && (!InRange(tamer, 3) || 0.01 > Utility.RandomDouble()) && InRange(tamer, 15))
			{
				//special case for those overpowered AWs:
				if (from is AncientWyrm)
				{
					LockCombat(tamer, TimeSpan.FromSeconds(25 + Utility.Random(45)));
				}
				else
				{
					LockCombat(tamer, TimeSpan.FromSeconds(3 + Utility.Random(6)));
				}

				DebugSay("Will attack {0} the tamer instead", tamer.Name);
			}
		}

		if (!willKill)
		{
			if (CanBeDistracted && ControlOrder == OrderType.Follow)
			{
				CheckDistracted(from);
			}
		}
		else if (from is PlayerMobile pm)
		{
			_ = Timer.DelayCall(TimeSpan.FromSeconds(10), new TimerCallback(pm.RecoverAmmo));
		}

		base.OnDamage(amount, from, willKill);
	}

	public override void OnDamagedBySpell(Mobile from, Spell spell, int damage)
	{
		base.OnDamagedBySpell(from, spell, damage);

		if (CanBeDistracted && ControlOrder == OrderType.Follow)
		{
			CheckDistracted(from);
		}
	}

	public override void AlterSpellDamageFrom(Mobile from, ref int damage)
	{
		if (TempDamageAbsorb > 0 /*&& VialofArmorEssence.UnderInfluence(this)*/)
			damage -= damage / TempDamageAbsorb;
	}

	public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
	{
		#region Mondain's Legacy
		if (from != null && from.Talisman is BaseTalisman talisman1)
		{
			BaseTalisman talisman = talisman1;

			if (talisman.Killer != null && talisman.Killer.Type != null)
			{
				Type type = talisman.Killer.Type;

				if (type.IsAssignableFrom(GetType()))
				{
					damage = (int)(damage * (1 + (double)talisman.Killer.Amount / 100));
				}
			}
		}
		#endregion

		if (TempDamageAbsorb > 0 /*&& VialofArmorEssence.UnderInfluence(this)*/)
			damage -= damage / TempDamageAbsorb;
	}

	public bool CheckHome()
	{
		if (Home == new Point3D(0, 0, 0))
		{
			return false;
		}

		if (GetDistanceToSqrt(Home) > RangeHome + 10 && !Controlled && LastOwner == null)
		{
			Location = Home;
			return true;
		}

		return false;

	}

	public override void AlterMeleeDamageTo(Mobile to, ref int damage)
	{
		if (TempDamageBonus > 0 /*&& TastyTreat.UnderInfluence(this)*/)
			damage += damage / TempDamageBonus;
	}

	public int TempDamageBonus { get; set; } = 0;
	public int TempDamageAbsorb { get; set; } = 0;

	public virtual void OnCarve(Mobile from, Corpse corpse, Item with)
	{
		int feathers = Feathers;
		int wool = Wool;
		int meat = Meat;
		int hides = Hides;
		int scales = Scales;
		int dragonblood = DragonBlood;

		bool special = with is HarvestersBlade;

		if ((feathers == 0 && wool == 0 && meat == 0 && hides == 0 && scales == 0) || Summoned || IsBonded || corpse.Animated)
		{
			if (corpse.Animated)
				corpse.SendLocalizedMessageTo(from, 500464); // Use this on corpses to carve away meat and hide
			else
				from.SendLocalizedMessage(500485); // You see nothing useful to carve from the corpse.
		}
		else
		{
			if (Core.ML && from.Race == Race.Human)
				hides = (int)Math.Ceiling(hides * 1.1); // 10% bonus only applies to hides, ore & logs

			if (corpse.Map == Map.Felucca)
			{
				feathers *= 2;
				wool *= 2;
				hides *= 2;

				if (Core.ML)
				{
					meat *= 2;
					scales *= 2;
				}
			}

			new Blood(0x122D).MoveToWorld(corpse.Location, corpse.Map);

			if (feathers != 0)
			{
				corpse.AddCarvedItem(new Feather(feathers), from);
				from.SendLocalizedMessage(500479); // You pluck the bird. The feathers are now on the corpse.
			}

			if (wool != 0)
			{
				Item w = new TaintedWool(wool);

				if (!Core.AOS || !special || !from.AddToBackpack(w))
				{
					corpse.AddCarvedItem(w, from);
					from.SendLocalizedMessage(500483); // You shear it, and the wool is now on the corpse.
				}
				else
				{
					from.SendLocalizedMessage(1114099); // You shear the creature and put the resources in your backpack.
				}
			}
			if (meat != 0)
			{
				Item m = MeatType switch
				{
					MeatType.Bird => new RawBird(meat),
					MeatType.LambLeg => new RawLambLeg(meat),
					_ => new RawRibs(meat),
				};
				if (!Core.AOS || !special || !from.AddToBackpack(m))
				{
					corpse.AddCarvedItem(m, from);
					from.SendLocalizedMessage(500467); // You carve some meat, which remains on the corpse.
				}
				else
				{
					from.SendLocalizedMessage(1114101); // You carve some meat and put it in your backpack.
				}
			}

			if (hides != 0)
			{
				var cutHides = (with is SkinningKnife && from.FindItemOnLayer(Layer.OneHanded) == with) || special || with is ButchersWarCleaver;

				Item leather;
				switch (HideType)
				{
					default:
					case HideType.Regular:
						if (cutHides)
						{
							leather = new Leather(hides);
						}
						else
						{
							leather = new Hides(hides);
						}

						break;
					case HideType.Spined:
						if (cutHides)
						{
							leather = new SpinedLeather(hides);
						}
						else
						{
							leather = new SpinedHides(hides);
						}

						break;
					case HideType.Horned:
						if (cutHides)
						{
							leather = new HornedLeather(hides);
						}
						else
						{
							leather = new HornedHides(hides);
						}

						break;
					case HideType.Barbed:
						if (cutHides)
						{
							leather = new BarbedLeather(hides);
						}
						else
						{
							leather = new BarbedHides(hides);
						}
						break;
				}

				if (!Core.AOS || !cutHides || !from.AddToBackpack(leather))
				{
					corpse.AddCarvedItem(leather, from);
					from.SendLocalizedMessage(500471); // You skin it, and the hides are now in the corpse.
				}
				else
				{
					from.SendLocalizedMessage(1073555); // You skin it and place the cut-up hides in your backpack.
				}
			}

			if (scales != 0)
			{
				ScaleType sc = ScaleType;
				List<Item> list = new();

				switch (sc)
				{
					default:
					case ScaleType.Red: list.Add(new RedScales(scales)); break;
					case ScaleType.Yellow: list.Add(new YellowScales(scales)); break;
					case ScaleType.Black: list.Add(new BlackScales(scales)); break;
					case ScaleType.Green: list.Add(new GreenScales(scales)); break;
					case ScaleType.White: list.Add(new WhiteScales(scales)); break;
					case ScaleType.Blue: list.Add(new BlueScales(scales)); break;
					case ScaleType.All:
						{
							list.Add(new RedScales(scales));
							list.Add(new YellowScales(scales));
							list.Add(new BlackScales(scales));
							list.Add(new GreenScales(scales));
							list.Add(new WhiteScales(scales));
							list.Add(new BlueScales(scales));
							break;
						}
				}

				if (Core.AOS && special)
				{
					bool allPack = true;
					bool anyPack = false;

					foreach (Item s in list)
					{
						//corpse.AddCarvedItem(s, from);
						if (!from.PlaceInBackpack(s))
						{
							corpse.AddCarvedItem(s, from);
							allPack = false;
						}
						else if (!anyPack)
						{
							anyPack = true;
						}
					}

					if (anyPack)
					{
						from.SendLocalizedMessage(1114098); // You cut away some scales and put them in your backpack.
					}

					if (!allPack)
					{
						from.SendLocalizedMessage(1079284); // You cut away some scales, but they remain on the corpse.
					}
				}
				else
				{
					foreach (Item s in list)
					{
						corpse.AddCarvedItem(s, from);
					}

					from.SendLocalizedMessage(1079284); // You cut away some scales, but they remain on the corpse.
				}

				ColUtility.Free(list);
			}

			if (dragonblood != 0)
			{
				Item dblood = new DragonBlood(dragonblood);

				if (!Core.AOS || !special || !from.AddToBackpack(dblood))
				{
					corpse.AddCarvedItem(dblood, from);
					from.SendLocalizedMessage(1094946); // Some blood is left on the corpse.
				}
				else
				{
					from.SendLocalizedMessage(1114100); // You take some blood off the corpse and put it in your backpack.
				}
			}

			corpse.Carved = true;

			if (corpse.IsCriminalAction(from))
				from.CriminalAction(true);
		}
	}

	public const int DefaultRangePerception = 16;
	public const int OldRangePerception = 10;

	public BaseCreature(AIType ai, FightMode mode, int iRangePerception, int iRangeFight, double dActiveSpeed, double dPassiveSpeed)
	{
		if (iRangePerception == OldRangePerception)
			iRangePerception = DefaultRangePerception;

		m_Loyalty = MaxLoyalty; // Wonderfully Happy
		PhysicalDamage = 100;
		CanMove = true;
		Hunger = 100;
		Thirst = 100;
		ApproachWait = false;
		ApproachRange = 10;
		m_CurrentAI = ai;
		m_DefaultAI = ai;

		RangePerception = iRangePerception;
		RangeFight = iRangeFight;

		FightMode = mode;

		m_iTeam = 0;

		_ = SpeedInfo.GetSpeeds(this, ref dActiveSpeed, ref dPassiveSpeed);

		ActiveSpeed = dActiveSpeed;
		PassiveSpeed = dPassiveSpeed;
		m_dCurrentSpeed = dPassiveSpeed;

		Debug = false;

		m_arSpellAttack = new List<Type>();
		m_arSpellDefense = new List<Type>();

		m_bControlled = false;
		m_ControlMaster = null;
		ControlTarget = null;
		m_ControlOrder = OrderType.None;

		m_bTamable = false;

		Owners = new List<Mobile>();

		NextReacquireTime = Core.TickCount + (int)ReacquireDelay.TotalMilliseconds;

		ChangeAIType(AI);

		InhumanSpeech speechType = SpeechType;

		if (speechType != null)
			speechType.OnConstruct(this);

		if (IsInvulnerable && !Core.AOS)
			NameHue = 0x35;

		GenerateLoot(true);
		//SkillsCap = 10000; //by default creatures have higher skill cap
	}

	public BaseCreature(Serial serial) : base(serial)
	{
		m_arSpellAttack = new List<Type>();
		m_arSpellDefense = new List<Type>();

		Debug = false;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		writer.Write((int)m_CurrentAI);
		writer.Write((int)m_DefaultAI);

		writer.Write(RangePerception);
		writer.Write(RangeFight);

		writer.Write(m_iTeam);

		writer.Write((double)ActiveSpeed);
		writer.Write((double)PassiveSpeed);
		writer.Write(m_dCurrentSpeed);

		writer.Write(m_pHome.X);
		writer.Write(m_pHome.Y);
		writer.Write(m_pHome.Z);

		// Version 1
		writer.Write(RangeHome);
		writer.Write(m_arSpellAttack.Count);

		int i;
		for (i = 0; i < m_arSpellAttack.Count; i++)
		{
			writer.Write(m_arSpellAttack[i].ToString());
		}

		writer.Write(m_arSpellDefense.Count);
		for (i = 0; i < m_arSpellDefense.Count; i++)
		{
			writer.Write(m_arSpellDefense[i].ToString());
		}

		// Version 2
		writer.Write((int)FightMode);

		writer.Write(m_bControlled);
		writer.Write(m_ControlMaster);
		writer.Write(ControlTarget is Mobile mobile ? mobile : null);
		writer.Write(m_ControlDest);
		writer.Write((int)m_ControlOrder);
		writer.Write((double)MinTameSkill);
		// Removed in version 9
		//writer.Write( (double) m_dMaxTameSkill );
		writer.Write(m_bTamable);
		writer.Write(m_bSummoned);

		if (m_bSummoned)
			writer.WriteDeltaTime(SummonEnd);

		writer.Write(ControlSlots);

		// Version 3
		writer.Write(m_Loyalty);

		// Version 4
		writer.Write(CurrentWayPoint);

		// Verison 5
		writer.Write(m_SummonMaster);

		// Version 6
		writer.Write(HitsMaxSeed);
		writer.Write(StamMaxSeed);
		writer.Write(ManaMaxSeed);
		writer.Write(m_DamageMin);
		writer.Write(m_DamageMax);

		// Version 7
		writer.Write(m_PhysicalResistance);
		writer.Write(PhysicalDamage);

		writer.Write(m_FireResistance);
		writer.Write(FireDamage);

		writer.Write(m_ColdResistance);
		writer.Write(ColdDamage);

		writer.Write(m_PoisonResistance);
		writer.Write(PoisonDamage);

		writer.Write(m_EnergyResistance);
		writer.Write(EnergyDamage);

		// Version 8
		writer.Write(Owners, true);

		// Version 10
		writer.Write(IsDeadPet);
		writer.Write(_mIsBonded);
		writer.Write(BondingBegin);
		writer.Write(OwnerAbandonTime);

		// Version 11
		writer.Write(HasGeneratedLoot);

		// Version 12
		writer.Write(m_Paragon);

		// Version 13
		writer.Write(Friends != null && Friends.Count > 0);

		if (Friends != null && Friends.Count > 0)
			writer.Write(Friends, true);

		// Version 14
		writer.Write(RemoveIfUntamed);
		writer.Write(RemoveStep);

		// Version 17
		if (IsStabled || (Controlled && ControlMaster != null))
			writer.Write(TimeSpan.Zero);
		else
			writer.Write(DeleteTimeLeft);

		// Version 18
		writer.Write(CorpseNameOverride);

		// Mondain's Legacy version 19
		writer.Write(_mAllured);

		writer.Write(_mEngravedText);

		writer.Write(Dispelonsummonerdeath);
		writer.Write(m_IsSupreme);
	}

	private static readonly double[] m_StandardActiveSpeeds = new double[]
		{
			0.175, 0.1, 0.15, 0.2, 0.25, 0.3, 0.4, 0.5, 0.6, 0.8
		};

	private static readonly double[] m_StandardPassiveSpeeds = new double[]
		{
			0.350, 0.2, 0.4, 0.5, 0.6, 0.8, 1.0, 1.2, 1.6, 2.0
		};

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				{
					m_CurrentAI = (AIType)reader.ReadInt();
					m_DefaultAI = (AIType)reader.ReadInt();

					RangePerception = reader.ReadInt();
					RangeFight = reader.ReadInt();

					m_iTeam = reader.ReadInt();

					ActiveSpeed = reader.ReadDouble();
					PassiveSpeed = reader.ReadDouble();
					m_dCurrentSpeed = reader.ReadDouble();

					if (RangePerception == OldRangePerception)
						RangePerception = DefaultRangePerception;

					m_pHome.X = reader.ReadInt();
					m_pHome.Y = reader.ReadInt();
					m_pHome.Z = reader.ReadInt();

					RangeHome = reader.ReadInt();


					int iCount = reader.ReadInt();
					for (int i = 0; i < iCount; i++)
					{
						string str = reader.ReadString();
						Type type = Type.GetType(str);

						if (type != null)
						{
							m_arSpellAttack.Add(type);
						}
					}

					iCount = reader.ReadInt();
					for (int i = 0; i < iCount; i++)
					{
						string str = reader.ReadString();
						Type type = Type.GetType(str);

						if (type != null)
						{
							m_arSpellDefense.Add(type);
						}
					}

					FightMode = (FightMode)reader.ReadInt();

					m_bControlled = reader.ReadBool();
					m_ControlMaster = reader.ReadMobile();
					ControlTarget = reader.ReadMobile();
					m_ControlDest = reader.ReadPoint3D();
					m_ControlOrder = (OrderType)reader.ReadInt();

					MinTameSkill = reader.ReadDouble();

					m_bTamable = reader.ReadBool();
					m_bSummoned = reader.ReadBool();

					if (m_bSummoned)
					{
						SummonEnd = reader.ReadDeltaTime();
						UnsummonTimer = new UnsummonTimer(m_ControlMaster, this, SummonEnd - DateTime.UtcNow);
						UnsummonTimer.Start();
					}

					ControlSlots = reader.ReadInt();

					m_Loyalty = reader.ReadInt();

					CurrentWayPoint = reader.ReadItem() as WayPoint;

					m_SummonMaster = reader.ReadMobile();

					HitsMaxSeed = reader.ReadInt();
					StamMaxSeed = reader.ReadInt();
					ManaMaxSeed = reader.ReadInt();
					m_DamageMin = reader.ReadInt();
					m_DamageMax = reader.ReadInt();

					m_PhysicalResistance = reader.ReadInt();
					PhysicalDamage = reader.ReadInt();

					m_FireResistance = reader.ReadInt();
					FireDamage = reader.ReadInt();

					m_ColdResistance = reader.ReadInt();
					ColdDamage = reader.ReadInt();

					m_PoisonResistance = reader.ReadInt();
					PoisonDamage = reader.ReadInt();

					m_EnergyResistance = reader.ReadInt();
					EnergyDamage = reader.ReadInt();

					Owners = reader.ReadStrongMobileList();

					IsDeadPet = reader.ReadBool();
					_mIsBonded = reader.ReadBool();
					BondingBegin = reader.ReadDateTime();
					OwnerAbandonTime = reader.ReadDateTime();
					HasGeneratedLoot = reader.ReadBool();
					m_Paragon = reader.ReadBool();

					if (reader.ReadBool())
						Friends = reader.ReadStrongMobileList();

					double activeSpeed = ActiveSpeed;
					double passiveSpeed = PassiveSpeed;

					_ = SpeedInfo.GetSpeeds(this, ref activeSpeed, ref passiveSpeed);

					bool isStandardActive = false;
					for (int i = 0; !isStandardActive && i < m_StandardActiveSpeeds.Length; ++i)
						isStandardActive = (ActiveSpeed == m_StandardActiveSpeeds[i]);

					bool isStandardPassive = false;
					for (int i = 0; !isStandardPassive && i < m_StandardPassiveSpeeds.Length; ++i)
						isStandardPassive = (PassiveSpeed == m_StandardPassiveSpeeds[i]);

					if (isStandardActive && m_dCurrentSpeed == ActiveSpeed)
						m_dCurrentSpeed = activeSpeed;
					else if (isStandardPassive && m_dCurrentSpeed == PassiveSpeed)
						m_dCurrentSpeed = passiveSpeed;

					if (isStandardActive && !m_Paragon)
						ActiveSpeed = activeSpeed;

					if (isStandardPassive && !m_Paragon)
						PassiveSpeed = passiveSpeed;


					RemoveIfUntamed = reader.ReadBool();
					RemoveStep = reader.ReadInt();

					TimeSpan deleteTime = reader.ReadTimeSpan();

					if (deleteTime > TimeSpan.Zero || LastOwner != null && !Controlled && !IsStabled)
					{
						if (deleteTime == TimeSpan.Zero)
							deleteTime = TimeSpan.FromDays(3.0);

						_mDeleteTimer = new DeleteTimer(this, deleteTime);
						_mDeleteTimer.Start();
					}

					CorpseNameOverride = reader.ReadString();
					_mAllured = reader.ReadBool();
					_mEngravedText = reader.ReadString();
					if (Core.AOS && NameHue == 0x35)
						NameHue = -1;

					Dispelonsummonerdeath = reader.ReadBool();
					m_IsSupreme = reader.ReadBool();

					CheckStatTimers();

					ChangeAIType(m_CurrentAI);

					AddFollowers();

					if (IsAnimatedDead)
						AnimateDeadSpell.Register(m_SummonMaster, this);

					break;
				}
		}
	}

	public virtual bool IsHumanInTown()
	{
		return (Body.IsHuman && Region.IsPartOf(typeof(Regions.GuardedRegion)));
	}

	public virtual bool CheckGold(Mobile from, Item dropped)
	{
		if (dropped is Gold gold)
			return OnGoldGiven(from, gold);

		return false;
	}

	public virtual bool OnGoldGiven(Mobile from, Gold dropped)
	{
		if (CheckTeachingMatch(from))
		{
			if (Teach(m_Teaching, from, dropped.Amount, true))
			{
				dropped.Delete();
				return true;
			}
		}
		else if (IsHumanInTown())
		{
			Direction = GetDirectionTo(from);

			int oldSpeechHue = SpeechHue;

			SpeechHue = 0x23F;
			SayTo(from, "Thou art giving me gold?");

			if (dropped.Amount >= 400)
				SayTo(from, "'Tis a noble gift.");
			else
				SayTo(from, "Money is always welcome.");

			SpeechHue = 0x3B2;
			SayTo(from, 501548); // I thank thee.

			SpeechHue = oldSpeechHue;

			dropped.Delete();
			return true;
		}

		return false;
	}

	public override bool ShouldCheckStatTimers => false;

	#region Food
	private static readonly Type[] m_Eggs = new Type[]
		{
			typeof( FriedEggs ), typeof( Eggs )
		};

	private static readonly Type[] m_Fish = new Type[]
		{
			typeof( FishSteak ), typeof( RawFishSteak )
		};

	private static readonly Type[] m_GrainsAndHay = new Type[]
		{
			typeof( BreadLoaf ), typeof( FrenchBread ), typeof( SheafOfHay )
		};

	private static readonly Type[] m_Meat = new Type[]
		{
			/* Cooked */
			typeof( Bacon ), typeof( CookedBird ), typeof( Sausage ),
			typeof( Ham ), typeof( Ribs ), typeof( LambLeg ),
			typeof( ChickenLeg ),

			/* Uncooked */
			typeof( RawBird ), typeof( RawRibs ), typeof( RawLambLeg ),
			typeof( RawChickenLeg ),

			/* Body Parts */
			typeof( Head ), typeof( LeftArm ), typeof( LeftLeg ),
			typeof( Torso ), typeof( RightArm ), typeof( RightLeg )
		};

	private static readonly Type[] m_FruitsAndVegies = new Type[]
		{
			typeof( HoneydewMelon ), typeof( YellowGourd ), typeof( GreenGourd ),
			typeof( Banana ), typeof( Bananas ), typeof( Lemon ), typeof( Lime ),
			typeof( Dates ), typeof( Grapes ), typeof( Peach ), typeof( Pear ),
			typeof( Apple ), typeof( Watermelon ), typeof( Squash ),
			typeof( Cantaloupe ), typeof( Carrot ), typeof( Cabbage ),
			typeof( Onion ), typeof( Lettuce ), typeof( Pumpkin )
		};

	private static readonly Type[] m_Gold = new Type[]
		{
			// white wyrms eat gold..
			typeof( Gold )
		};

	public virtual bool CheckFoodPreference(Item f)
	{
		if (CheckFoodPreference(f, FoodType.Eggs, m_Eggs))
			return true;

		if (CheckFoodPreference(f, FoodType.Fish, m_Fish))
			return true;

		if (CheckFoodPreference(f, FoodType.GrainsAndHay, m_GrainsAndHay))
			return true;

		if (CheckFoodPreference(f, FoodType.Meat, m_Meat))
			return true;

		if (CheckFoodPreference(f, FoodType.FruitsAndVegies, m_FruitsAndVegies))
			return true;

		if (CheckFoodPreference(f, FoodType.Gold, m_Gold))
			return true;

		return false;
	}

	public virtual bool CheckFoodPreference(Item fed, FoodType type, Type[] types)
	{
		if ((FavoriteFood & type) == 0)
			return false;

		Type fedType = fed.GetType();
		bool contains = false;

		for (int i = 0; !contains && i < types.Length; ++i)
			contains = (fedType == types[i]);

		return contains;
	}

	public virtual bool CheckFeed(Mobile from, Item dropped)
	{
		if (!IsDeadPet && Controlled && (ControlMaster == from || IsPetFriend(from)))
		{
			Item f = dropped;

			if (CheckFoodPreference(f))
			{
				int amount = f.Amount;

				if (amount > 0)
				{
					int stamGain;

					if (f is Gold)
						stamGain = amount - 50;
					else
						stamGain = (amount * 15) - 50;

					if (stamGain > 0)
						Stam += stamGain;

					if (Core.SE)
					{
						if (m_Loyalty < MaxLoyalty)
						{
							m_Loyalty = MaxLoyalty;
						}
					}
					else
					{
						for (int i = 0; i < amount; ++i)
						{
							if (m_Loyalty < MaxLoyalty && 0.5 >= Utility.RandomDouble())
							{
								m_Loyalty += 10;
							}
						}
					}

					/* if ( happier )*/    // looks like in OSI pets say they are happier even if they are at maximum loyalty
					SayTo(from, 502060); // Your pet looks happier.

					if (Body.IsAnimal)
						Animate(3, 5, 1, true, false, 0);
					else if (Body.IsMonster)
						Animate(17, 5, 1, true, false, 0);

					if (IsBondable && !IsBonded)
					{
						Mobile master = m_ControlMaster;

						if (master != null && master == from)   //So friends can't start the bonding process
						{
							if (MinTameSkill <= 29.1 || master.Skills[SkillName.AnimalTaming].Base >= MinTameSkill || OverrideBondingReqs() || (Core.ML && master.Skills[SkillName.AnimalTaming].Value >= MinTameSkill))
							{
								if (BondingBegin == DateTime.MinValue)
								{
									BondingBegin = DateTime.UtcNow;
								}
								else if ((BondingBegin + BondingDelay) <= DateTime.UtcNow)
								{
									IsBonded = true;
									BondingBegin = DateTime.MinValue;
									from.SendLocalizedMessage(1049666); // Your pet has bonded with you!
								}
							}
							else if (Core.ML)
							{
								from.SendLocalizedMessage(1075268); // Your pet cannot form a bond with you until your animal taming ability has risen.
							}
						}
					}

					dropped.Delete();
					return true;
				}
			}
		}

		return false;
	}

	#endregion

	public virtual bool OverrideBondingReqs()
	{
		return false;
	}

	public virtual bool CanAngerOnTame => false;

	#region OnAction[...]

	public virtual void OnActionWander()
	{
	}

	public virtual void OnActionCombat()
	{
	}

	public virtual void OnActionGuard()
	{
	}

	public virtual void OnActionFlee()
	{
	}

	public virtual void OnActionInteract()
	{
	}

	public virtual void OnActionBackoff()
	{
	}

	#endregion

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (CheckFeed(from, dropped))
			return true;
		else if (CheckGold(from, dropped))
			return true;

		return base.OnDragDrop(from, dropped);
	}

	protected virtual BaseAI ForcedAI => null;

	public void ChangeAIType(AIType NewAI)
	{
		if (AIObject != null)
			AIObject.m_Timer.Stop();

		if (ForcedAI != null)
		{
			AIObject = ForcedAI;
			return;
		}

		AIObject = null;

		switch (NewAI)
		{
			case AIType.AI_Melee:
				AIObject = new MeleeAI(this);
				break;
			case AIType.AI_Animal:
				AIObject = new AnimalAI(this);
				break;
			case AIType.BerserkAI:
				AIObject = new BerserkAI(this);
				break;
			case AIType.AI_Archer:
				AIObject = new ArcherAI(this);
				break;
			case AIType.AI_Healer:
				AIObject = new HealerAI(this);
				break;
			case AIType.AI_Vendor:
				AIObject = new VendorAI(this);
				break;
			case AIType.AI_Mage:
				AIObject = new MageAI(this);
				break;
			case AIType.PredatorAI:
				AIObject = new PredatorAI(this);
				//AIObject = new MeleeAI(this);
				break;
			case AIType.AI_Thief:
				AIObject = new ThiefAI(this);
				break;
			case AIType.AI_NecroMage:
				AIObject = new NecroMageAI(this);
				break;
			case AIType.OrcScoutAI:
				AIObject = new OrcScoutAI(this);
				break;
			case AIType.AI_Samurai:
				AIObject = new SamuraiAI(this);
				break;
			case AIType.AI_Ninja:
				AIObject = new NinjaAI(this);
				break;
			case AIType.AI_Spellweaving:
				AIObject = new SpellweavingAI(this);
				break;
			case AIType.AI_Mystic:
				AIObject = new MysticAI(this);
				break;
			case AIType.AI_Paladin:
				AIObject = new PaladinAI(this);
				break;
			case AIType.SpellbinderAI:
				AIObject = new SpellbinderAI(this);
				break;
			case AIType.AI_Necro:
				AIObject = new NecroAI(this);
				break;
			case AIType.ChampionMeleeAI:
				AIObject = new ChampionMeleeAI(this);
				break;
			case AIType.BoneDemonAI:
				AIObject = new BoneDemonAI(this);
				break;
			case AIType.BossMeleeAI:
				AIObject = new BossMeleeAI(this);
				break;
			case AIType.MephitisAI:
				AIObject = new MephitisAI(this);
				break;
			case AIType.ScalisAI:
				AIObject = new ScalisAI(this);
				break;
			case AIType.AdvancedArcherAI:
				AIObject = new AdvancedArcherAI(this);
				break;
			case AIType.AmbusherAI:
				AIObject = new AmbusherAI(this);
				break;
			case AIType.WeakMageAI:
				AIObject = new WeakMageAI(this);
				break;
			case AIType.CoreAI:
				AIObject = new CoreAI(this);
				break;
			case AIType.SuperAI:
				AIObject = new SuperAI(this);
				break;
			case AIType.AnimalSkittishAI:
				AIObject = new AnimalSkittishAI(this);
				break;
			case AIType.VampireAI:
				AIObject = new VampireAI(this);
				break;
		}
	}

	public void ChangeAIToDefault()
	{
		ChangeAIType(m_DefaultAI);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public AIType AI
	{
		get => m_CurrentAI;
		set
		{
			m_CurrentAI = value;

			if (m_CurrentAI == AIType.UseDefault)
			{
				m_CurrentAI = m_DefaultAI;
			}

			ChangeAIType(m_CurrentAI);
		}
	}

	[CommandProperty(AccessLevel.Administrator)]
	public bool Debug { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Team
	{
		get => m_iTeam;
		set
		{
			m_iTeam = value;

			OnTeamChange();
		}
	}

	public virtual void OnTeamChange()
	{
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public IDamageable FocusMob { get; set; } // Use focus mob instead of combatant, maybe we don't want to fight

	[CommandProperty(AccessLevel.GameMaster)]
	public FightMode FightMode { get; set; } // The style the mob uses

	[CommandProperty(AccessLevel.GameMaster)]
	public int RangePerception { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RangeFight { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RangeHome { get; set; } = 10; // The home range of the creature

	[CommandProperty(AccessLevel.GameMaster)]
	public double ActiveSpeed { get; set; }  // Timer speed when active

	[CommandProperty(AccessLevel.GameMaster)]
	public double PassiveSpeed { get; set; } // Timer speed when not active

	[CommandProperty(AccessLevel.GameMaster)]
	public double CurrentSpeed
	{
		get
		{
			if (TargetLocation != null)
				return 0.3;

			return m_dCurrentSpeed;
		}
		set
		{
			if (m_dCurrentSpeed != value)
			{
				m_dCurrentSpeed = value;

				if (AIObject != null)
					AIObject.OnCurrentSpeedChanged();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Home
	{
		get => m_pHome;
		set => m_pHome = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Controlled
	{
		get => m_bControlled;
		set
		{
			if (m_bControlled == value)
				return;

			m_bControlled = value;
			Delta(MobileDelta.Noto);

			InvalidateProperties();
		}
	}

	public override void RevealingAction()
	{
		Spells.Sixth.InvisibilitySpell.RemoveTimer(this);

		base.RevealingAction();
	}

	public void RemoveFollowers()
	{
		if (m_ControlMaster != null)
		{
			m_ControlMaster.Followers -= ControlSlots;
			if (m_ControlMaster is PlayerMobile pm)
			{
				_ = pm.AllFollowers.Remove(this);
				if (pm.AutoStabled.Contains(this))
					_ = pm.AutoStabled.Remove(this);
			}
		}
		else if (m_SummonMaster != null)
		{
			m_SummonMaster.Followers -= ControlSlots;
			if (m_SummonMaster is PlayerMobile pm)
			{
				_ = pm.AllFollowers.Remove(this);
			}
		}

		if (m_ControlMaster != null && m_ControlMaster.Followers < 0)
			m_ControlMaster.Followers = 0;

		if (m_SummonMaster != null && m_SummonMaster.Followers < 0)
			m_SummonMaster.Followers = 0;
	}

	public void AddFollowers()
	{
		if (m_ControlMaster != null)
		{
			m_ControlMaster.Followers += ControlSlots;
			if (m_ControlMaster is PlayerMobile pm)
			{
				pm.AllFollowers.Add(this);
			}
		}
		else if (m_SummonMaster != null)
		{
			m_SummonMaster.Followers += ControlSlots;
			if (m_SummonMaster is PlayerMobile pm)
			{
				pm.AllFollowers.Add(this);
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile ControlMaster
	{
		get => m_ControlMaster;
		set
		{
			if (m_ControlMaster == value || this == value)
				return;

			RemoveFollowers();
			m_ControlMaster = value;
			AddFollowers();
			if (m_ControlMaster != null)
				StopDeleteTimer();

			Delta(MobileDelta.Noto);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile SummonMaster
	{
		get => m_SummonMaster;
		set
		{
			if (m_SummonMaster == value || this == value)
				return;

			RemoveFollowers();
			m_SummonMaster = value;
			AddFollowers();

			Delta(MobileDelta.Noto);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public IDamageable ControlTarget { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D ControlDest
	{
		get => m_ControlDest;
		set => m_ControlDest = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual OrderType ControlOrder
	{
		get => m_ControlOrder;
		set
		{
			m_ControlOrder = value;

			if (_mAllured)
			{
				if (m_ControlOrder == OrderType.Release)
				{
					Say(502003); // Sorry, but no.
				}
				else if (m_ControlOrder != OrderType.None)
				{
					Say(1079120); // Very well.
				}
			}

			if (AIObject != null)
				AIObject.OnCurrentOrderChanged();

			InvalidateProperties();

			if (m_ControlMaster != null)
				m_ControlMaster.InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool BardProvoked { get; set; } = false;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool BardPacified { get; set; } = false;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile BardMaster { get; set; } = null;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile BardTarget { get; set; } = null;

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime BardEndTime { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public double MinTameSkill { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Tamable
	{
		get => m_bTamable && !(m_Paragon || m_BlackRock || m_IsSupreme);
		set => m_bTamable = value;
	}

	[CommandProperty(AccessLevel.Administrator)]
	public bool Summoned
	{
		get => m_bSummoned;
		set
		{
			if (m_bSummoned == value)
				return;

			NextReacquireTime = Core.TickCount;

			m_bSummoned = value;
			Delta(MobileDelta.Noto);

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.Administrator)]
	public int ControlSlots { get; set; } = 1;

	public virtual bool NoHouseRestrictions => false;
	public virtual bool IsHouseSummonable => false;

	#region Corpse Resources
	public virtual int Feathers => 0;
	public virtual int Wool => 0;
	public virtual int Fur => 0;

	public virtual MeatType MeatType => MeatType.Ribs;
	public virtual int Meat => 0;

	public virtual int Hides => 0;
	public virtual HideType HideType => HideType.Regular;

	public virtual int Scales => 0;
	public virtual ScaleType ScaleType => ScaleType.Red;
	public virtual int DragonBlood => 0;
	#endregion

	public virtual bool AutoDispel => false;
	public virtual double AutoDispelChance => ((Core.SE) ? .10 : 1.0);

	public virtual bool IsScaryToPets => false;
	public virtual bool IsScaredOfScaryThings => true;

	public virtual bool CanRummageCorpses => false;

	public override void OnGotMeleeAttack(Mobile attacker)
	{
		base.OnGotMeleeAttack(attacker);

		if (AutoDispel && attacker is BaseCreature creature && creature.IsDispellable && AutoDispelChance > Utility.RandomDouble())
			Dispel(attacker);
	}

	public virtual void Dispel(Mobile m)
	{
		Effects.SendLocationParticles(EffectItem.Create(m.Location, m.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
		Effects.PlaySound(m, m.Map, 0x201);

		m.Delete();
	}

	public virtual bool DeleteOnRelease => m_bSummoned;

	public override void OnGaveMeleeAttack(Mobile defender)
	{
		base.OnGaveMeleeAttack(defender);

		Poison p = HitPoison;

		if (m_Paragon)
			p = PoisonImpl.IncreaseLevel(p);

		if (p != null && HitPoisonChance >= Utility.RandomDouble())
		{
			_ = defender.ApplyPoison(this, p);

			if (Controlled)
				_ = CheckSkill(SkillName.Poisoning, 0, Skills[SkillName.Poisoning].Cap);
		}

		if (AutoDispel && defender is BaseCreature creature && creature.IsDispellable && AutoDispelChance > Utility.RandomDouble())
			Dispel(defender);
	}

	public override void OnAfterDelete()
	{
		if (AIObject != null)
		{
			if (AIObject.m_Timer != null)
				AIObject.m_Timer.Stop();

			AIObject = null;
		}

		if (_mDeleteTimer != null)
		{
			_mDeleteTimer.Stop();
			_mDeleteTimer = null;
		}

		FocusMob = null;

		if (IsAnimatedDead)
			AnimateDeadSpell.Unregister(m_SummonMaster, this);

		base.OnAfterDelete();
	}

	public void DebugSay(string text)
	{
		if (Debug)
			PublicOverheadMessage(MessageType.Regular, 41, false, text);
	}

	public void DebugSay(string format, params object[] args)
	{
		if (Debug)
			PublicOverheadMessage(MessageType.Regular, 41, false, string.Format(format, args));
	}

	/*
	 * This function can be overriden.. so a "Strongest" mobile, can have a different definition depending
	 * on who check for value
	 * -Could add a FightMode.Prefered
	 *
	 */

	public virtual double GetFightModeRanking(Mobile m, FightMode acqType, bool bPlayerOnly)
	{
		if ((bPlayerOnly && m.Player) || !bPlayerOnly)
		{
			return acqType switch
			{
				FightMode.Strongest => (m.Skills[SkillName.Tactics].Value + m.Str),//returns strongest mobile
				FightMode.Weakest => -m.Hits,// returns weakest mobile
				_ => -GetDistanceToSqrt(m),// returns closest mobile
			};
		}
		else
		{
			return double.MinValue;
		}
	}

	// Turn, - for left, + for right
	// Basic for now, needs work
	public virtual void Turn(int iTurnSteps)
	{
		int v = (int)Direction;

		Direction = (Direction)((((v & 0x7) + iTurnSteps) & 0x7) | (v & 0x80));
	}

	public virtual void TurnInternal(int iTurnSteps)
	{
		int v = (int)Direction;

		SetDirection((Direction)((((v & 0x7) + iTurnSteps) & 0x7) | (v & 0x80)));
	}

	public bool IsHurt()
	{
		return (Hits != HitsMax);
	}

	public double GetHomeDistance()
	{
		return GetDistanceToSqrt(m_pHome);
	}

	public virtual int GetTeamSize(int iRange)
	{
		int iCount = 0;

		foreach (Mobile m in GetMobilesInRange(iRange))
		{
			if (m is BaseCreature creature)
			{
				if (creature.Team == Team)
				{
					if (!m.Deleted)
					{
						if (m != this)
						{
							if (CanSee(m))
							{
								iCount++;
							}
						}
					}
				}
			}
		}

		return iCount;
	}

	private class TameEntry : ContextMenuEntry
	{
		private readonly BaseCreature m_Mobile;

		public TameEntry(Mobile from, BaseCreature creature) : base(6130, 6)
		{
			m_Mobile = creature;

			Enabled = Enabled && (from.Female ? creature.AllowFemaleTamer : creature.AllowMaleTamer);
		}

		public override void OnClick()
		{
			if (!Owner.From.CheckAlive())
				return;

			Owner.From.TargetLocked = true;
			SkillHandlers.AnimalTaming.DisableMessage = true;

			if (Owner.From.UseSkill(SkillName.AnimalTaming))
				Owner.From.Target.Invoke(Owner.From, m_Mobile);

			SkillHandlers.AnimalTaming.DisableMessage = false;
			Owner.From.TargetLocked = false;
		}
	}

	#region Teaching
	public virtual bool CanTeach => false;

	public virtual bool CheckTeach(SkillName skill, Mobile from)
	{
		if (!CanTeach)
			return false;

		if (skill == SkillName.Stealth && from.Skills[SkillName.Hiding].Base < Stealth.HidingRequirement)
			return false;

		if (skill == SkillName.RemoveTrap && (from.Skills[SkillName.Lockpicking].Base < 50.0 || from.Skills[SkillName.DetectHidden].Base < 50.0))
			return false;

		if (!Core.AOS && (skill == SkillName.Focus || skill == SkillName.Chivalry || skill == SkillName.Necromancy))
			return false;

		return true;
	}

	public enum TeachResult
	{
		Success,
		Failure,
		KnowsMoreThanMe,
		KnowsWhatIKnow,
		SkillNotRaisable,
		NotEnoughFreePoints
	}

	public virtual TeachResult CheckTeachSkills(SkillName skill, Mobile m, int maxPointsToLearn, ref int pointsToLearn, bool doTeach)
	{
		if (!CheckTeach(skill, m) || !m.CheckAlive())
			return TeachResult.Failure;

		Skill ourSkill = Skills[skill];
		Skill theirSkill = m.Skills[skill];

		if (ourSkill == null || theirSkill == null)
			return TeachResult.Failure;

		int baseToSet = ourSkill.BaseFixedPoint / 3;

		if (baseToSet > 420)
			baseToSet = 420;
		else if (baseToSet < 200)
			return TeachResult.Failure;

		if (baseToSet > theirSkill.CapFixedPoint)
			baseToSet = theirSkill.CapFixedPoint;

		pointsToLearn = baseToSet - theirSkill.BaseFixedPoint;

		if (maxPointsToLearn > 0 && pointsToLearn > maxPointsToLearn)
		{
			pointsToLearn = maxPointsToLearn;
			baseToSet = theirSkill.BaseFixedPoint + pointsToLearn;
		}

		if (pointsToLearn < 0)
			return TeachResult.KnowsMoreThanMe;

		if (pointsToLearn == 0)
			return TeachResult.KnowsWhatIKnow;

		if (theirSkill.Lock != SkillLock.Up)
			return TeachResult.SkillNotRaisable;

		int freePoints = m.Skills.Cap - m.Skills.Total;
		int freeablePoints = 0;

		if (freePoints < 0)
			freePoints = 0;

		for (int i = 0; (freePoints + freeablePoints) < pointsToLearn && i < m.Skills.Length; ++i)
		{
			Skill sk = m.Skills[i];

			if (sk == theirSkill || sk.Lock != SkillLock.Down)
				continue;

			freeablePoints += sk.BaseFixedPoint;
		}

		if ((freePoints + freeablePoints) == 0)
			return TeachResult.NotEnoughFreePoints;

		if ((freePoints + freeablePoints) < pointsToLearn)
		{
			pointsToLearn = freePoints + freeablePoints;
			baseToSet = theirSkill.BaseFixedPoint + pointsToLearn;
		}

		if (doTeach)
		{
			int need = pointsToLearn - freePoints;

			for (int i = 0; need > 0 && i < m.Skills.Length; ++i)
			{
				Skill sk = m.Skills[i];

				if (sk == theirSkill || sk.Lock != SkillLock.Down)
					continue;

				if (sk.BaseFixedPoint < need)
				{
					need -= sk.BaseFixedPoint;
					sk.BaseFixedPoint = 0;
				}
				else
				{
					sk.BaseFixedPoint -= need;
					need = 0;
				}
			}

			/* Sanity check */
			if (baseToSet > theirSkill.CapFixedPoint || (m.Skills.Total - theirSkill.BaseFixedPoint + baseToSet) > m.Skills.Cap)
				return TeachResult.NotEnoughFreePoints;

			theirSkill.BaseFixedPoint = baseToSet;
		}

		return TeachResult.Success;
	}

	public virtual bool CheckTeachingMatch(Mobile m)
	{
		if (m_Teaching == (SkillName)(-1))
			return false;

		if (m is PlayerMobile pm)
			return pm.Learning == m_Teaching;

		return true;
	}

	private SkillName m_Teaching = (SkillName)(-1);

	public virtual bool Teach(SkillName skill, Mobile m, int maxPointsToLearn, bool doTeach)
	{
		int pointsToLearn = 0;
		TeachResult res = CheckTeachSkills(skill, m, maxPointsToLearn, ref pointsToLearn, doTeach);

		switch (res)
		{
			case TeachResult.KnowsMoreThanMe:
				{
					Say(501508); // I cannot teach thee, for thou knowest more than I!
					break;
				}
			case TeachResult.KnowsWhatIKnow:
				{
					Say(501509); // I cannot teach thee, for thou knowest all I can teach!
					break;
				}
			case TeachResult.NotEnoughFreePoints:
			case TeachResult.SkillNotRaisable:
				{
					// Make sure this skill is marked to raise. If you are near the skill cap (700 points) you may need to lose some points in another skill first.
					m.SendLocalizedMessage(501510, 0x22);
					break;
				}
			case TeachResult.Success:
				{
					if (doTeach)
					{
						Say(501539); // Let me show thee something of how this is done.
						m.SendLocalizedMessage(501540); // Your skill level increases.

						m_Teaching = (SkillName)(-1);

						if (m is PlayerMobile pm)
							pm.Learning = (SkillName)(-1);
					}
					else
					{
						// I will teach thee all I know, if paid the amount in full.  The price is:
						Say(1019077, AffixType.Append, string.Format(" {0}", pointsToLearn), "");
						Say(1043108); // For less I shall teach thee less.

						m_Teaching = skill;

						if (m is PlayerMobile pm)
							pm.Learning = skill;
					}

					return true;
				}
		}

		return false;
	}

	#endregion

	public override void AggressiveAction(Mobile aggressor, bool criminal)
	{
		base.AggressiveAction(aggressor, criminal);

		if (ControlMaster != null)
			if (NotorietyHandlers.CheckAggressor(ControlMaster.Aggressors, aggressor))
				aggressor.Aggressors.Add(AggressorInfo.Create(this, aggressor, true));

		OrderType ct = m_ControlOrder;

		if (AIObject != null)
		{
			if (!Core.ML || (ct != OrderType.Follow && ct != OrderType.Stop && ct != OrderType.Stay))
			{
				AIObject.OnAggressiveAction(aggressor);
			}
			else
			{
				DebugSay("I'm being attacked but my master told me not to fight.");
				Warmode = false;
				return;
			}
		}

		StopFlee();

		ForceReacquire();

		if (!IsEnemy(aggressor))
		{
			Ethics.Player pl = Ethics.Player.Find(aggressor, true);

			if (pl != null && pl.IsShielded)
				pl.FinishShield();
		}

		if (aggressor.ChangingCombatant && (m_bControlled || m_bSummoned) && (ct == OrderType.Come || (!Core.ML && ct == OrderType.Stay) || ct == OrderType.Stop || ct == OrderType.None || ct == OrderType.Follow))
		{
			ControlTarget = aggressor;
			ControlOrder = OrderType.Attack;
		}
		else if (Combatant == null && !BardPacified)
		{
			Warmode = true;
			Combatant = aggressor;
		}
	}

	public override bool OnMoveOver(Mobile m)
	{
		if (m is BaseCreature creature && !creature.Controlled)
			return (!Alive || !m.Alive || IsDeadBondedPet || m.IsDeadBondedPet) || (Hidden && AccessLevel > AccessLevel.Player);
		#region Dueling
		if (Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)) && m is PlayerMobile pm)
		{
			if (pm.DuelContext == null || pm.DuelPlayer == null || !pm.DuelContext.Started || pm.DuelContext.Finished || pm.DuelPlayer.Eliminated)
				return true;
		}
		#endregion

		return base.OnMoveOver(m);
	}

	public virtual void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
	{
	}

	public virtual bool CanDrop => IsBonded;

	public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
	{
		base.GetContextMenuEntries(from, list);

		if (AIObject != null && Commandable)
			AIObject.GetContextMenuEntries(from, list);

		if (m_bTamable && !m_bControlled && from.Alive)
			list.Add(new TameEntry(from, this));

		AddCustomContextEntries(from, list);

		if (CanTeach && from.Alive)
		{
			Skills ourSkills = Skills;
			Skills theirSkills = from.Skills;

			for (int i = 0; i < ourSkills.Length && i < theirSkills.Length; ++i)
			{
				Skill skill = ourSkills[i];
				Skill theirSkill = theirSkills[i];

				if (skill != null && theirSkill != null && skill.Base >= 60.0 && CheckTeach(skill.SkillName, from))
				{
					int toTeach = skill.BaseFixedPoint / 3;

					if (toTeach > 420)
						toTeach = 420;

					list.Add(new TeachEntry((SkillName)i, this, from, (toTeach > theirSkill.BaseFixedPoint)));
				}
			}
		}
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		InhumanSpeech speechType = SpeechType;

		if (speechType != null && (speechType.Flags & IHSFlags.OnSpeech) != 0 && from.InRange(this, 3))
			return true;

		return AIObject != null && AIObject.HandlesOnSpeech(from) && from.InRange(this, RangePerception);
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		InhumanSpeech speechType = SpeechType;

		if (speechType != null && speechType.OnSpeech(this, e.Mobile, e.Speech))
			e.Handled = true;
		else if (!e.Handled && AIObject != null && e.Mobile.InRange(this, RangePerception))
			AIObject.OnSpeech(e);
	}

	public override bool IsHarmfulCriminal(IDamageable damageable)
	{
		Mobile target = damageable as Mobile;

		return (Controlled && target == m_ControlMaster) || (Summoned && target == m_SummonMaster) || (target is BaseCreature creature && creature.InitialInnocent && !creature.Controlled) || (target is PlayerMobile mobile && mobile.PermaFlags.Count > 0)
			? false
			: base.IsHarmfulCriminal(damageable);
	}

	public override void CriminalAction(bool message)
	{
		base.CriminalAction(message);

		if (Controlled || Summoned)
		{
			if (m_ControlMaster != null && m_ControlMaster.Player)
				m_ControlMaster.CriminalAction(false);
			else if (m_SummonMaster != null && m_SummonMaster.Player)
				m_SummonMaster.CriminalAction(false);
		}
	}

	public override void DoHarmful(IDamageable damageable, bool indirect)
	{
		if (RecentSetControl && GetMaster() == damageable as Mobile)
		{
			return;
		}

		base.DoHarmful(damageable, indirect);

		if (damageable is not Mobile target)
			return;

		if (target == this || target == m_ControlMaster || target == m_SummonMaster || (!Controlled && !Summoned))
			return;

		List<AggressorInfo> list = Aggressors;

		for (int i = 0; i < list.Count; ++i)
		{
			AggressorInfo ai = list[i];

			if (ai.Attacker == target)
				return;
		}

		list = Aggressed;

		for (int i = 0; i < list.Count; ++i)
		{
			AggressorInfo ai = list[i];

			if (ai.Defender == target)
			{
				if (m_ControlMaster != null && m_ControlMaster.Player && m_ControlMaster.CanBeHarmful(target, false))
					m_ControlMaster.DoHarmful(target, true);
				else if (m_SummonMaster != null && m_SummonMaster.Player && m_SummonMaster.CanBeHarmful(target, false))
					m_SummonMaster.DoHarmful(target, true);

				return;
			}
		}
	}

	private static Mobile m_NoDupeGuards;

	public static void ReleaseGuardDupeLock()
	{
		m_NoDupeGuards = null;
	}

	public void ReleaseGuardLock()
	{
		EndAction(typeof(GuardedRegion));
	}

	private DateTime m_IdleReleaseTime;

	public virtual bool CheckIdle()
	{
		if (Combatant != null)
			return false; // in combat.. not idling

		if (m_IdleReleaseTime > DateTime.MinValue)
		{
			// idling...

			if (DateTime.UtcNow >= m_IdleReleaseTime)
			{
				m_IdleReleaseTime = DateTime.MinValue;
				return false; // idle is over
			}

			return true; // still idling
		}

		if (95 > Utility.Random(100))
			return false; // not idling, but don't want to enter idle state

		m_IdleReleaseTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(15, 25));

		if (Body.IsHuman)
		{
			switch (Utility.Random(2))
			{
				case 0: CheckedAnimate(5, 5, 1, true, true, 1); break;
				case 1: CheckedAnimate(6, 5, 1, true, false, 1); break;
			}
		}
		else if (Body.IsAnimal)
		{
			switch (Utility.Random(3))
			{
				case 0: CheckedAnimate(3, 3, 1, true, false, 1); break;
				case 1: CheckedAnimate(9, 5, 1, true, false, 1); break;
				case 2: CheckedAnimate(10, 5, 1, true, false, 1); break;
			}
		}
		else if (Body.IsMonster)
		{
			switch (Utility.Random(2))
			{
				case 0: CheckedAnimate(17, 5, 1, true, false, 1); break;
				case 1: CheckedAnimate(18, 5, 1, true, false, 1); break;
			}
		}

		PlaySound(GetIdleSound());
		return true; // entered idle state
	}

	/*
		this way, due to the huge number of locations this will have to be changed
		Perhaps we can change this in the future when fixing game play is not the
		major issue.
	*/

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

	private void CheckAIActive()
	{
		Map map = Map;

		if (PlayerRangeSensitive && AIObject != null && map != null && map.GetSector(Location).Active)
			AIObject.Activate();
	}

	public override void OnCombatantChange()
	{
		base.OnCombatantChange();

		Warmode = (Combatant != null && !Combatant.Deleted && Combatant.Alive);

		if (CanFly && Warmode)
		{
			Flying = false;
		}
	}

	protected override void OnMapChange(Map oldMap)
	{
		CheckAIActive();

		base.OnMapChange(oldMap);
	}

	protected override void OnLocationChange(Point3D oldLocation)
	{
		CheckAIActive();

		base.OnLocationChange(oldLocation);
	}

	public virtual void ForceReacquire()
	{
		NextReacquireTime = Core.TickCount;
	}

	protected override bool OnMove(Direction d)
	{
		if (Hidden) //Hidden, let's try stealth
		{
			if (!Mounted && Skills.Stealth.Value >= 25.0 && CanStealth)
			{
				bool running = (d & Direction.Running) != 0;

				if (running)
				{
					if ((AllowedStealthSteps -= 2) <= 0)
					{
						RevealingAction();
					}
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
		}

		return true;
	}

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (AcquireOnApproach && (!Controlled && !Summoned) && FightMode != FightMode.Aggressor)
		{
			if (InRange(m.Location, AcquireOnApproachRange) && !InRange(oldLocation, AcquireOnApproachRange))
			{
				if (CanBeHarmful(m) && IsEnemy(m))
				{
					Combatant = FocusMob = m;

					if (AIObject != null)
					{
						_ = AIObject.MoveTo(m, true, 1);
					}

					DoHarmful(m);
				}
			}
		}
		else if (ReacquireOnMovement)
		{
			ForceReacquire();
		}

		InhumanSpeech speechType = SpeechType;

		if (speechType != null)
			speechType.OnMovement(this, m, oldLocation);

		/* Begin notice sound */
		if ((!m.Hidden || m.IsPlayer()) && m.Player && FightMode != FightMode.Aggressor && FightMode != FightMode.None && Combatant == null && !Controlled && !Summoned)
		{
			// If this creature defends itself but doesn't actively attack (animal) or
			// doesn't fight at all (vendor) then no notice sounds are played..
			// So, players are only notified of aggressive monsters

			// Monsters that are currently fighting are ignored

			// Controlled or summoned creatures are ignored

			if (InRange(m.Location, 18) && !InRange(oldLocation, 18))
			{
				if (Body.IsMonster)
					Animate(11, 5, 1, true, false, 1);

				PlaySound(GetAngerSound());
			}
		}
		/* End notice sound */

		if (m_NoDupeGuards == m)
			return;

		if (!Body.IsHuman || Murderer || AlwaysMurderer || AlwaysAttackable || !m.Murderer || !m.InRange(Location, 12) || !m.Alive)
			return;

		GuardedRegion guardedRegion = (GuardedRegion)Region.GetRegion(typeof(GuardedRegion));

		if (guardedRegion != null)
		{
			if (!guardedRegion.IsDisabled() && guardedRegion.IsGuardCandidate(m) && BeginAction(typeof(GuardedRegion)))
			{
				Say(1013037 + Utility.Random(16));
				guardedRegion.CallGuards(Location);

				_ = Timer.DelayCall(TimeSpan.FromSeconds(5.0), new TimerCallback(ReleaseGuardLock));

				m_NoDupeGuards = m;
				_ = Timer.DelayCall(TimeSpan.Zero, new TimerCallback(ReleaseGuardDupeLock));
			}
		}
	}

	public void AddSpellAttack(Type type)
	{
		m_arSpellAttack.Add(type);
	}

	public void AddSpellDefense(Type type)
	{
		m_arSpellDefense.Add(type);
	}

	public Spell GetAttackSpellRandom()
	{
		if (m_arSpellAttack.Count > 0)
		{
			Type type = m_arSpellAttack[Utility.Random(m_arSpellAttack.Count)];

			object[] args = { this, null };
			return Activator.CreateInstance(type, args) as Spell;
		}
		else
		{
			return null;
		}
	}

	public Spell GetDefenseSpellRandom()
	{
		if (m_arSpellDefense.Count > 0)
		{
			Type type = m_arSpellDefense[Utility.Random(m_arSpellDefense.Count)];

			object[] args = { this, null };
			return Activator.CreateInstance(type, args) as Spell;
		}
		else
		{
			return null;
		}
	}

	public Spell GetSpellSpecific(Type type)
	{
		int i;

		for (i = 0; i < m_arSpellAttack.Count; i++)
		{
			if (m_arSpellAttack[i] == type)
			{
				object[] args = { this, null };
				return Activator.CreateInstance(type, args) as Spell;
			}
		}

		for (i = 0; i < m_arSpellDefense.Count; i++)
		{
			if (m_arSpellDefense[i] == type)
			{
				object[] args = { this, null };
				return Activator.CreateInstance(type, args) as Spell;
			}
		}

		return null;
	}

	#region Set[...]
	public void SetDamage(int val)
	{
		m_DamageMin = val;
		m_DamageMax = val;
	}

	public void SetDamage(int min, int max)
	{
		m_DamageMin = min;
		m_DamageMax = max;
	}

	public void SetHits(int val)
	{
		if (val < 1000 && !Core.AOS)
			val = val * 100 / 60;

		HitsMaxSeed = val;
		Hits = HitsMax;
	}

	public void SetHits(int min, int max)
	{
		if (min < 1000 && !Core.AOS)
		{
			min = min * 100 / 60;
			max = max * 100 / 60;
		}

		HitsMaxSeed = Utility.RandomMinMax(min, max);
		Hits = HitsMax;
	}

	public void SetStam(int val)
	{
		StamMaxSeed = val;
		Stam = StamMax;
	}

	public void SetStam(int min, int max)
	{
		StamMaxSeed = Utility.RandomMinMax(min, max);
		Stam = StamMax;
	}

	public void SetMana(int val)
	{
		ManaMaxSeed = val;
		Mana = ManaMax;
	}

	public void SetMana(int min, int max)
	{
		ManaMaxSeed = Utility.RandomMinMax(min, max);
		Mana = ManaMax;
	}

	public void SetStr(int val)
	{
		RawStr = val;
		Hits = HitsMax;
	}

	public void SetStr(int min, int max)
	{
		RawStr = Utility.RandomMinMax(min, max);
		Hits = HitsMax;
	}

	public void SetDex(int val)
	{
		RawDex = val;
		Stam = StamMax;
	}

	public void SetDex(int min, int max)
	{
		RawDex = Utility.RandomMinMax(min, max);
		Stam = StamMax;
	}

	public void SetInt(int val)
	{
		RawInt = val;
		Mana = ManaMax;
	}

	public void SetInt(int min, int max)
	{
		RawInt = Utility.RandomMinMax(min, max);
		Mana = ManaMax;
	}
	/// <summary>
	///  These will set all three stats instead having to add more code lines to the npc.
	///  if you set anything to 0 the system automatic will give it a 1, because no npc can be set to 0 on stats.
	///  SetStats(1, 3, 5, 6, 7, 9);
	///  SetStats(1, 3, 0, 0, 7, 9);
	///  SetStats(0, 0, 0, 0, 7, 9);
	///  SetStats(1, 3, 5, 6, 7);
	///  SetStats(1, 3, 0, 0, 7);
	///  SetStats(0, 0, 0, 0, 7);
	///  SetStats(1, 3, 5, 6);
	///  SetStats(1, 3, 0, 6);
	///  SetStats(0, 0, 0, 6);
	///  SetStats(1, 3);
	///  SetStats(1);
	/// </summary>
	/// <param name="minstr"></param>
	/// <param name="maxstr"></param>
	/// <param name="mindex"></param>
	/// <param name="maxdex"></param>
	/// <param name="minint"></param>
	/// <param name="maxint"></param>
	public void SetStats(int minstr, int maxstr = 0, int mindex = 0, int maxdex = 0, int minint = 0, int maxint = 0)
	{
		if (maxstr > 0)
		{
			RawStr = Utility.RandomMinMax(minstr, maxstr);
			Hits = HitsMax;
		}
		else
		{
			RawStr = minstr;
			Hits = HitsMax;
		}

		if (mindex > 0 || maxdex > 0)
		{
			if (maxdex == 0)
				RawDex = mindex;
			else
				RawDex = Utility.RandomMinMax(mindex, maxdex);
			Stam = StamMax;
		}

		if (minint > 0 || maxint > 0)
		{
			if (maxint == 0)
				RawInt = minint;
			else
				RawInt = Utility.RandomMinMax(minint, maxint);
			Mana = ManaMax;
		}
	}

	public void SetDamageType(ResistanceType type, int min, int max)
	{
		SetDamageType(type, Utility.RandomMinMax(min, max));
	}

	public void SetDamageType(ResistanceType type, int val)
	{
		switch (type)
		{
			case ResistanceType.Physical: PhysicalDamage = val; break;
			case ResistanceType.Fire: FireDamage = val; break;
			case ResistanceType.Cold: ColdDamage = val; break;
			case ResistanceType.Poison: PoisonDamage = val; break;
			case ResistanceType.Energy: EnergyDamage = val; break;
		}
	}

	/// <summary>
	/// These will set the elemental damage and the normal damage. without havinge to set both
	/// min max dmg element, min,max damage
	/// SetDamage(ResistanceType.Physical, 50, 0, 13, 17);
	/// SetDamage(ResistanceType.Physical, 50, 0, 13);
	/// SetDamage(ResistanceType.Physical, 50, 60);
	/// SetDamage(ResistanceType.Physical, 50);
	/// </summary>
	/// <param name="type"></param>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <param name="dmgmin"></param>
	/// <param name="dmgmax"></param>
	public void SetDamage(ResistanceType type, int min, int max = 0, int dmgmin = 0, int dmgmax = 0)
	{
		if (dmgmin > 0 || dmgmax > 0)
		{
			m_DamageMin = dmgmin;
			if (dmgmax == 0)
				m_DamageMax = dmgmin;
			else
				m_DamageMax = dmgmax;
		}

		switch (type)
		{
			case ResistanceType.Physical:
				if (max > 0)
					PhysicalDamage = Utility.RandomMinMax(min, max);
				else
					PhysicalDamage = min;
				break;
			case ResistanceType.Fire:
				if (max > 0)
					FireDamage = Utility.RandomMinMax(min, max);
				else
					FireDamage = min;
				break;
			case ResistanceType.Cold:
				if (max > 0)
					ColdDamage = Utility.RandomMinMax(min, max);
				else
					ColdDamage = min;
				break;
			case ResistanceType.Poison:
				if (max > 0)
					PoisonDamage = Utility.RandomMinMax(min, max);
				else
					PoisonDamage = min;
				break;
			case ResistanceType.Energy:
				if (max > 0)
					EnergyDamage = Utility.RandomMinMax(min, max);
				else
					EnergyDamage = min;
				break;
		}
	}

	public void SetResist(ResistanceType type, int min, int max = 0)
	{
		if (max == 0) max = min;
		int val = (min == max || max == 0) ? min : Utility.RandomMinMax(min, max);

		switch (type)
		{
			case ResistanceType.Physical: m_PhysicalResistance = val; break;
			case ResistanceType.Fire: m_FireResistance = val; break;
			case ResistanceType.Cold: m_ColdResistance = val; break;
			case ResistanceType.Poison: m_PoisonResistance = val; break;
			case ResistanceType.Energy: m_EnergyResistance = val; break;
		}

		UpdateResistances();
	}

	public void SetResistance(ResistanceType type, int min, int max)
	{
		SetResistance(type, Utility.RandomMinMax(min, max));
	}

	public void SetResistance(ResistanceType type, int val)
	{
		switch (type)
		{
			case ResistanceType.Physical: m_PhysicalResistance = val; break;
			case ResistanceType.Fire: m_FireResistance = val; break;
			case ResistanceType.Cold: m_ColdResistance = val; break;
			case ResistanceType.Poison: m_PoisonResistance = val; break;
			case ResistanceType.Energy: m_EnergyResistance = val; break;
		}

		UpdateResistances();
	}

	public void SetSkill(SkillName name, double val)
	{
		Skills[name].BaseFixedPoint = (int)(val * 10);

		if (Skills[name].Base > Skills[name].Cap)
		{
			if (Core.SE)
				SkillsCap += (Skills[name].BaseFixedPoint - Skills[name].CapFixedPoint);

			Skills[name].Cap = Skills[name].Base;
		}
	}

	public void SetSkill(SkillName name, double min, double max)
	{
		int minFixed = (int)(min * 10);
		int maxFixed = (int)(max * 10);

		Skills[name].BaseFixedPoint = Utility.RandomMinMax(minFixed, maxFixed);

		if (Skills[name].Base > Skills[name].Cap)
		{
			if (Core.SE)
				SkillsCap += (Skills[name].BaseFixedPoint - Skills[name].CapFixedPoint);

			Skills[name].Cap = Skills[name].Base;
		}
	}

	public void SetFameLevel(int level)
	{
		switch (level)
		{
			case 1: Fame = Utility.RandomMinMax(0, 1249); break;
			case 2: Fame = Utility.RandomMinMax(1250, 2499); break;
			case 3: Fame = Utility.RandomMinMax(2500, 4999); break;
			case 4: Fame = Utility.RandomMinMax(5000, 9999); break;
			case 5: Fame = Utility.RandomMinMax(10000, 10000); break;
		}
	}

	public void SetKarmaLevel(int level)
	{
		switch (level)
		{
			case 0: Karma = -Utility.RandomMinMax(0, 624); break;
			case 1: Karma = -Utility.RandomMinMax(625, 1249); break;
			case 2: Karma = -Utility.RandomMinMax(1250, 2499); break;
			case 3: Karma = -Utility.RandomMinMax(2500, 4999); break;
			case 4: Karma = -Utility.RandomMinMax(5000, 9999); break;
			case 5: Karma = -Utility.RandomMinMax(10000, 10000); break;
		}
	}

	#endregion

	public static void Cap(ref int val, int min, int max)
	{
		if (val < min)
			val = min;
		else if (val > max)
			val = max;
	}

	#region Pack & Loot

	public void PackArcaneScroll(int min, int max)
	{
		PackArcaneScroll(Utility.RandomMinMax(min, max));
	}

	public void PackArcaneScroll(int amount)
	{
		for (int i = 0; i < amount; ++i)
			PackArcaneScroll();
	}

	public void PackArcaneScroll()
	{
		PackItem(Loot.Construct(Loot.ArcanistScrollTypes));
	}

	public void PackPotion()
	{
		PackItem(Loot.RandomPotion());
	}

	public void PackNecroScroll(int index)
	{
		if (!Core.AOS || 0.05 <= Utility.RandomDouble())
			return;

		PackItem(Loot.Construct(Loot.NecromancyScrollTypes, index));
	}

	public void PackScroll(int minCircle, int maxCircle)
	{
		PackScroll(Utility.RandomMinMax(minCircle, maxCircle));
	}

	public void PackScroll(int circle)
	{
		int min = (circle - 1) * 8;

		PackItem(Loot.RandomScroll(min, min + 7, SpellbookType.Regular));
	}

	public void PackMagicItems(int minLevel, int maxLevel)
	{
		PackMagicItems(minLevel, maxLevel, 0.30, 0.15);
	}

	public void PackMagicItems(int minLevel, int maxLevel, double armorChance, double weaponChance)
	{
		if (!PackArmor(minLevel, maxLevel, armorChance))
			_ = PackWeapon(minLevel, maxLevel, weaponChance);
	}

	public virtual void DropBackpack()
	{
		if (Backpack != null)
		{
			if (Backpack.Items.Count > 0)
			{
				Backpack b = new BaseCreatureBackpack(Name);

				List<Item> list = new(Backpack.Items);
				foreach (Item item in list)
				{
					b.DropItem(item);
				}

				BaseHouse house = BaseHouse.FindHouseAt(this);
				if (house != null)
					b.MoveToWorld(house.BanLocation, house.Map);
				else
					b.MoveToWorld(Location, Map);
			}
		}
	}

	protected bool MSpawning;
	protected int MKillersLuck;
	public int KillersLuck { get; protected set; }
	public LootStage LootStage { get; protected set; }
	public bool StealPackGenerated { get; protected set; }
	public bool HasBeenStolen { get; set; }

	public virtual void GenerateLoot(bool spawning)
	{
		GenerateLoot(spawning ? LootStage.Spawning : LootStage.Death);
	}

	public virtual void GenerateLoot(LootStage stage)
	{
		if (NoLootOnDeath || _mAllured)
			return;

		LootStage = stage;

		switch (stage)
		{
			case LootStage.Stolen:
				StealPackGenerated = true;
				break;
			case LootStage.Death:
				KillersLuck = LootPack.GetLuckChanceForKiller(this);
				break;
		}

		//MSpawning = spawning;

		//if (!spawning)
		//	MKillersLuck = LootPack.GetLuckChanceForKiller(this);

		GenerateLoot();

		if (m_Paragon)
		{
			switch (Fame)
			{
				case < 1250:
					AddLoot(LootPack.Meager);
					break;
				case < 2500:
					AddLoot(LootPack.Average);
					break;
				case < 5000:
					AddLoot(LootPack.Rich);
					break;
				case < 10000:
					AddLoot(LootPack.FilthyRich);
					break;
				default:
					AddLoot(LootPack.UltraRich);
					break;
			}
		}

		//MSpawning = false;
		KillersLuck = 0;
	}

	public virtual void GenerateLoot()
	{
	}

	public virtual void AddLoot(LootPack pack, int min, int max)
	{
		AddLoot(pack, Utility.RandomMinMax(min, max), 100.0);
	}

	public virtual void AddLoot(LootPack pack, int min, int max, double chance)
	{
		if (min > max)
			min = max;

		AddLoot(pack, Utility.RandomMinMax(min, max), chance);
	}

	public virtual void AddLoot(LootPack pack, int amount)
	{
		AddLoot(pack, amount, 100.0);
	}

	public virtual void AddLoot(LootPack pack, int amount, double chance)
	{
		for (int i = 0; i < amount; ++i)
		{
			AddLoot(pack, chance);
		}
	}

	public virtual void AddLoot(LootPack pack)
	{
		AddLoot(pack, 100.0);
	}

	public virtual void AddLoot(LootPack pack, double chance)
	{
		if (Summoned || pack == null || (chance < 100.0 && Utility.RandomDouble() > chance / 100))
		{
			return;
		}

		Container backpack = Backpack;

		if (backpack == null)
		{
			backpack = new Backpack
			{
				Movable = false
			};

			AddItem(backpack);
		}

		pack.Generate(this, backpack, MSpawning, MKillersLuck);
	}

	public bool PackArmor(int minLevel, int maxLevel)
	{
		return PackArmor(minLevel, maxLevel, 1.0);
	}

	public bool PackArmor(int minLevel, int maxLevel, double chance)
	{
		if (chance <= Utility.RandomDouble())
			return false;

		Cap(ref minLevel, 0, 5);
		Cap(ref maxLevel, 0, 5);

		if (Core.AOS)
		{
			Item item = Loot.RandomArmorOrShieldOrJewelry();

			if (item == null)
				return false;

			GetRandomAOSStats(minLevel, maxLevel, out int attributeCount, out int min, out int max);

			if (item is BaseArmor armor)
				BaseRunicTool.ApplyAttributesTo(armor, attributeCount, min, max);
			else if (item is BaseJewel jewel)
				BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);

			PackItem(item);
		}
		else
		{
			BaseArmor armor = Loot.RandomArmorOrShield();

			if (armor == null)
				return false;

			armor.ProtectionLevel = (ArmorProtectionLevel)RandomMinMaxScaled(minLevel, maxLevel);
			armor.Durability = (DurabilityLevel)RandomMinMaxScaled(minLevel, maxLevel);

			PackItem(armor);
		}

		return true;
	}

	public static void GetRandomAOSStats(int minLevel, int maxLevel, out int attributeCount, out int min, out int max)
	{
		int v = RandomMinMaxScaled(minLevel, maxLevel);

		switch (v)
		{
			case >= 5:
				attributeCount = Utility.RandomMinMax(2, 6);
				min = 20; max = 70;
				break;
			case 4:
				attributeCount = Utility.RandomMinMax(2, 4);
				min = 20; max = 50;
				break;
			case 3:
				attributeCount = Utility.RandomMinMax(2, 3);
				min = 20; max = 40;
				break;
			case 2:
				attributeCount = Utility.RandomMinMax(1, 2);
				min = 10; max = 30;
				break;
			default:
				attributeCount = 1;
				min = 10; max = 20;
				break;
		}
	}

	public static int RandomMinMaxScaled(int min, int max)
	{
		if (min == max)
			return min;

		if (min > max)
		{
			(max, min) = (min, max);
		}

		/* Example:
		 *    min: 1
		 *    max: 5
		 *  count: 5
		 *
		 * total = (5*5) + (4*4) + (3*3) + (2*2) + (1*1) = 25 + 16 + 9 + 4 + 1 = 55
		 *
		 * chance for min+0 : 25/55 : 45.45%
		 * chance for min+1 : 16/55 : 29.09%
		 * chance for min+2 :  9/55 : 16.36%
		 * chance for min+3 :  4/55 :  7.27%
		 * chance for min+4 :  1/55 :  1.81%
		 */

		int count = max - min + 1;
		int total = 0, toAdd = count;

		for (int i = 0; i < count; ++i, --toAdd)
			total += toAdd * toAdd;

		int rand = Utility.Random(total);
		toAdd = count;

		int val = min;

		for (int i = 0; i < count; ++i, --toAdd, ++val)
		{
			rand -= toAdd * toAdd;

			if (rand < 0)
				break;
		}

		return val;
	}

	public bool PackSlayer()
	{
		return PackSlayer(0.05);
	}

	public bool PackSlayer(double chance)
	{
		if (chance <= Utility.RandomDouble())
			return false;

		if (Utility.RandomBool())
		{
			BaseInstrument instrument = Loot.RandomInstrument();

			if (instrument != null)
			{
				instrument.Slayer = SlayerGroup.GetLootSlayerType(GetType());
				PackItem(instrument);
			}
		}
		else if (!Core.AOS)
		{
			BaseWeapon weapon = Loot.RandomWeapon();

			if (weapon != null)
			{
				weapon.Slayer = SlayerGroup.GetLootSlayerType(GetType());
				PackItem(weapon);
			}
		}

		return true;
	}

	public bool PackWeapon(int minLevel, int maxLevel)
	{
		return PackWeapon(minLevel, maxLevel, 1.0);
	}

	public bool PackWeapon(int minLevel, int maxLevel, double chance)
	{
		if (chance <= Utility.RandomDouble())
			return false;

		Cap(ref minLevel, 0, 5);
		Cap(ref maxLevel, 0, 5);

		if (Core.AOS)
		{
			Item item = Loot.RandomWeaponOrJewelry();

			if (item == null)
				return false;

			GetRandomAOSStats(minLevel, maxLevel, out int attributeCount, out int min, out int max);

			switch (item)
			{
				case BaseWeapon weapon:
					BaseRunicTool.ApplyAttributesTo(weapon, attributeCount, min, max);
					break;
				case BaseJewel jewel:
					BaseRunicTool.ApplyAttributesTo(jewel, attributeCount, min, max);
					break;
			}

			PackItem(item);
		}
		else
		{
			BaseWeapon weapon = Loot.RandomWeapon();

			if (weapon == null)
				return false;

			if (0.05 > Utility.RandomDouble())
				weapon.Slayer = SlayerName.Silver;

			weapon.DamageLevel = (WeaponDamageLevel)RandomMinMaxScaled(minLevel, maxLevel);
			weapon.AccuracyLevel = (WeaponAccuracyLevel)RandomMinMaxScaled(minLevel, maxLevel);
			weapon.DurabilityLevel = (DurabilityLevel)RandomMinMaxScaled(minLevel, maxLevel);

			PackItem(weapon);
		}

		return true;
	}

	public void PackGold(int amount)
	{
		if (amount > 0)
			PackItem(new Gold(amount));
	}

	public void PackGold(int min, int max)
	{
		PackGold(Utility.RandomMinMax(min, max));
	}

	public void PackStatue(int min, int max)
	{
		PackStatue(Utility.RandomMinMax(min, max));
	}

	public void PackStatue(int amount)
	{
		for (int i = 0; i < amount; ++i)
			PackStatue();
	}

	public void PackStatue()
	{
		PackItem(Loot.RandomStatue());
	}

	public void PackGem()
	{
		PackGem(1);
	}

	public void PackGem(int min, int max)
	{
		PackGem(Utility.RandomMinMax(min, max));
	}

	public void PackGem(int amount)
	{
		if (amount <= 0)
			return;

		Item gem = Loot.RandomGem();

		gem.Amount = amount;

		PackItem(gem);
	}

	public void PackNecroReg(int min, int max)
	{
		PackNecroReg(Utility.RandomMinMax(min, max));
	}

	public void PackNecroReg(int amount)
	{
		for (int i = 0; i < amount; ++i)
			PackNecroReg();
	}

	public void PackNecroReg()
	{
		if (!Core.AOS)
			return;

		PackItem(Loot.RandomNecromancyReagent());
	}

	public void PackReg(int min, int max)
	{
		PackReg(Utility.RandomMinMax(min, max));
	}

	public void PackReg(int amount)
	{
		if (amount <= 0)
			return;

		Item reg = Loot.RandomReagent();

		reg.Amount = amount;

		PackItem(reg);
	}

	public override void PackItem(Item item)
	{
		if (Summoned || item == null)
		{
			item?.Delete();

			return;
		}

		Container pack = Backpack;

		if (pack == null)
		{
			pack = new Backpack
			{
				Movable = false
			};

			AddItem(pack);
		}

		if (!item.Stackable || !pack.TryDropItem(this, item, false)) // try stack
		{
			pack.DropItem(item); // failed, drop it anyway
		}

		base.PackItem(item);
	}

	public override void WearItem(Item item, int hue = -1)
	{
		WearItem(item, 0.0, hue);
	}

	public void WearItem(Item item, double dropChance, int hue)
	{
		item.Movable = dropChance > Utility.RandomDouble();

		base.WearItem(item, hue);
	}
	#endregion

	public override void OnDoubleClick(Mobile from)
	{
		if (from.IsStaff() && !Body.IsHuman)
		{
			Container pack = Backpack;

			pack?.DisplayTo(from);
		}

		if (DeathAdderCharmable && from.CanBeHarmful(this, false))
		{
			if (SummonFamiliarSpell.Table[from] is DeathAdder da && !da.Deleted)
			{
				from.SendAsciiMessage("You charm the snake.  Select a target to attack.");
				from.Target = new DeathAdderCharmTarget(this);
			}
		}

		base.OnDoubleClick(from);
	}

	private class DeathAdderCharmTarget : Target
	{
		private readonly BaseCreature _mCharmed;

		public DeathAdderCharmTarget(BaseCreature charmed) : base(-1, false, TargetFlags.Harmful)
		{
			_mCharmed = charmed;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (!_mCharmed.DeathAdderCharmable || _mCharmed.Combatant != null || !from.CanBeHarmful(_mCharmed, false) || SummonFamiliarSpell.Table[from] is not DeathAdder da || da.Deleted || targeted is not Mobile targ || !from.CanBeHarmful(targ, false))
				return;

			from.RevealingAction();
			from.DoHarmful(targ, true);

			_mCharmed.Combatant = targ;

			if (_mCharmed.AIObject != null)
				_mCharmed.AIObject.Action = ActionType.Combat;
		}
	}

	public override void AddNameProperties(ObjectPropertyList list)
	{
		base.AddNameProperties(list);

		if (Core.ML)
		{
			if (Controlled && !string.IsNullOrEmpty(EngravedText))
			{
				list.Add(1157315, EngravedText); // <BASEFONT COLOR=#668cff>Branded: ~1_VAL~<BASEFONT COLOR=#FFFFFF>
			}

			if (DisplayWeight)
				list.Add(TotalWeight == 1 ? 1072788 : 1072789, TotalWeight.ToString()); // Weight: ~1_WEIGHT~ stones

			if (m_ControlOrder == OrderType.Guard)
				list.Add(1080078); // guarding
		}

		if (Summoned && !IsAnimatedDead && !IsNecroFamiliar && this is not Clone)
		{
			list.Add(1049646); // (summoned)
		}
		else if (Controlled && Commandable)
		{
			if (IsBonded)   //Intentional difference (showing ONLY bonded when bonded instead of bonded & tame)
				list.Add(1049608); // (bonded)
			else
				list.Add(502006); // (tame)
		}

		if (IsGolem)
			list.Add(1113697); // (Golem)

		if (IsAmbusher)
		{
			list.Add(1155480); // Ambusher
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		if (Controlled && Commandable)
		{
			int number;

			if (Summoned)
				number = 1049646; // (summoned)
			else if (IsBonded)
				number = 1049608; // (bonded)
			else
				number = 502006; // (tame)

			PrivateOverheadMessage(MessageType.Regular, 0x3B2, number, from.NetState);
		}

		base.OnSingleClick(from);
	}

	public override bool OnBeforeDeath()
	{
		Mobile killer = LastKiller;
		int treasureLevel = TreasureMapLevel;
		GetLootingRights();
		if (treasureLevel == 1 && Map == Map.Trammel && TreasureMap.IsInHavenIsland(this))
		{
			if (killer is BaseCreature creature)
				killer = creature.GetMaster();

			if (killer is PlayerMobile pm && pm.Young)
				treasureLevel = 0;
		}

		if (!Summoned && !NoKillAwards && !IsBonded && !NoLootOnDeath)
		{
			if (treasureLevel >= 0)
			{
				if (m_Paragon && Paragon.ChestChance > Utility.RandomDouble())
					PackItem(new ParagonChest(Name, treasureLevel));
				else if ((Map == Map.Felucca || Map == Map.Trammel) && TreasureMap.LootChance >= Utility.RandomDouble())
					PackItem(new TreasureMap(treasureLevel, Map));
			}

			if (m_Paragon && Paragon.ChocolateIngredientChance > Utility.RandomDouble())
			{
				switch (Utility.Random(4))
				{
					case 0: PackItem(new CocoaButter()); break;
					case 1: PackItem(new CocoaLiquor()); break;
					case 2: PackItem(new SackOfSugar()); break;
					case 3: PackItem(new Vanilla()); break;
				}
			}
		}

		if (!Summoned && !NoKillAwards && !HasGeneratedLoot && !NoLootOnDeath)
		{
			HasGeneratedLoot = true;
			GenerateLoot(false);
		}

		if (!NoKillAwards && Region.IsPartOf("Doom"))
		{
			int bones = Engines.Quests.Doom.TheSummoningQuest.GetDaemonBonesFor(this);

			if (bones > 0)
				PackItem(new DaemonBone(bones));
		}

		if (IsAnimatedDead)
			Effects.SendLocationEffect(Location, Map, 0x3728, 13, 1, 0x461, 4);

		InhumanSpeech speechType = SpeechType;

		if (speechType != null)
			speechType.OnDeath(this);

		if (ReceivedHonorContext != null)
			ReceivedHonorContext.OnTargetKilled();

		//ML Loot System 				
		MLLootSystem.HandleKill(this, killer);

		return base.OnBeforeDeath();
	}

	public static int ComputeBonusDamage(List<DamageEntry> list, Mobile m)
	{
		int bonus = 0;

		for (int i = list.Count - 1; i >= 0; --i)
		{
			DamageEntry de = list[i];

			if (de.Damager == m || de.Damager is not BaseCreature)
				continue;

			BaseCreature bc = (BaseCreature)de.Damager;

			Mobile master = bc.GetMaster();
			if (master == m)
				bonus += de.DamageGiven;
		}

		return bonus;
	}

	public Mobile GetMaster()
	{
		if (Controlled && ControlMaster != null)
			return ControlMaster;
		else if (Summoned && SummonMaster != null)
			return SummonMaster;

		return null;
	}

	public virtual bool IsMonster
	{
		get
		{
			if (!Controlled)
				return true;

			Mobile master = GetMaster();

			return master == null || (master is BaseCreature creature && !creature.Controlled);
		}
	}

	public virtual bool IsAggressiveMonster => IsMonster && (FightMode == FightMode.Closest ||
								 FightMode == FightMode.Strongest ||
								 FightMode == FightMode.Weakest ||
								 FightMode == FightMode.Good);

	private class FKEntry
	{
		public Mobile m_Mobile;
		public int m_Damage;

		public FKEntry(Mobile m, int damage)
		{
			m_Mobile = m;
			m_Damage = damage;
		}
	}

	public List<DamageStore> LootingRights { get; set; }

	public bool HasLootingRights(Mobile m)
	{
		return LootingRights?.FirstOrDefault(ds => ds.m_Mobile == m && ds.m_HasRight) != null;
	}

	public Mobile GetHighestDamager()
	{
		if (LootingRights == null || LootingRights.Count == 0)
			return null;

		return LootingRights[0].m_Mobile;
	}

	public bool IsHighestDamager(Mobile m)
	{
		return LootingRights != null && LootingRights.Count > 0 && LootingRights[0].m_Mobile == m;
	}

	public List<DamageStore> GetLootingRights()
	{
		if (LootingRights != null)
			return LootingRights;

		List<DamageEntry> damageEntries = DamageEntries;
		int hitsMax = HitsMax;

		List<DamageStore> rights = new();

		for (int i = damageEntries.Count - 1; i >= 0; --i)
		{
			if (i >= damageEntries.Count)
				continue;

			DamageEntry de = damageEntries[i];

			if (de.HasExpired)
			{
				damageEntries.RemoveAt(i);
				continue;
			}

			int damage = de.DamageGiven;

			List<DamageEntry> respList = de.Responsible;

			if (respList != null)
			{
				for (int j = 0; j < respList.Count; ++j)
				{
					DamageEntry subEntry = respList[j];
					Mobile master = subEntry.Damager;

					if (master == null || master.Deleted || !master.Player)
						continue;

					bool needNewSubEntry = true;

					for (int k = 0; needNewSubEntry && k < rights.Count; ++k)
					{
						DamageStore ds = rights[k];

						if (ds.m_Mobile == master)
						{
							ds.m_Damage += subEntry.DamageGiven;
							needNewSubEntry = false;
						}
					}

					if (needNewSubEntry)
						rights.Add(new DamageStore(master, subEntry.DamageGiven));

					damage -= subEntry.DamageGiven;
				}
			}

			Mobile m = de.Damager;

			if (m == null || m.Deleted || !m.Player)
				continue;

			if (damage <= 0)
				continue;

			bool needNewEntry = true;

			for (int j = 0; needNewEntry && j < rights.Count; ++j)
			{
				DamageStore ds = rights[j];

				if (ds.m_Mobile == m)
				{
					ds.m_Damage += damage;
					needNewEntry = false;
				}
			}

			if (needNewEntry)
				rights.Add(new DamageStore(m, damage));
		}

		if (rights.Count > 0)
		{
			rights[0].m_Damage = (int)(rights[0].m_Damage * 1.25);  //This would be the first valid person attacking it.  Gets a 25% bonus.  Per 1/19/07 Five on Friday

			if (rights.Count > 1)
				rights.Sort(); //Sort by damage

			int topDamage = rights[0].m_Damage;
			int minDamage;

			//if (Core.SA)
			//{
			//	minDamage = (int)(topDamage * 0.06);
			//}
			//else
			//{
				if (hitsMax >= 3000)
					minDamage = topDamage / 16;
				else if (hitsMax >= 1000)
					minDamage = topDamage / 8;
				else if (hitsMax >= 200)
					minDamage = topDamage / 4;
				else
					minDamage = topDamage / 2;
			//}

			//for (int i = 0; i < rights.Count; ++i)
			//{
			//	DamageStore ds = rights[i];
			//
			//	ds.m_HasRight = (ds.m_Damage >= minDamage);
			//}
			int totalDamage = 0; // check on when this was added.
			for (int i = 0; i < rights.Count; ++i)
			{
				DamageStore ds = rights[i];

				totalDamage += ds.m_Damage;
			}

			for (int i = 0; i < rights.Count; ++i)
			{
				DamageStore ds = rights[i];

				ds.m_HasRight = ds.m_Damage >= minDamage;

				if (totalDamage != 0)
					ds.DamagePercent = ds.m_Damage / totalDamage;
			}
		}

		LootingRights = rights;
		return rights;
	}

	public virtual void OnRelease(Mobile from)
	{
		if (_mAllured)
		{
			Timer.DelayCall(TimeSpan.FromSeconds(2), Delete);
		}
	}

	public override void OnItemLifted(Mobile from, Item item)
	{
		base.OnItemLifted(from, item);

		InvalidateProperties();
	}

	public override void OnKilledBy(Mobile mob)
	{
		base.OnKilledBy(mob);

		//if (m_Paragon && XmlParagon.CheckArtifactChance(mob, this))
		//{
		//	XmlParagon.GiveArtifactTo(mob, this);
		//}

		if (m_Paragon && Paragon.CheckArtifactChance(mob, this))
		{
			Paragon.GiveArtifactTo(mob);
		}

		if (GivesMlMinorArtifact)
		{
			if (MondainsLegacy.CheckArtifactChance(mob, this))
				MondainsLegacy.GiveArtifactTo(mob);
		}
		else if (m_Paragon)
		{
			if (Paragon.CheckArtifactChance(mob, this))
				Paragon.GiveArtifactTo(mob);
		}

		//if (mob is PlayerMobile)
		//{
		//	Reputation.BaseReputationGroup.OnKilledByEvent(this, (PlayerMobile)mob);
		//}

		if (PvMLogEnabled && mob is PlayerMobile pm)
		{
			BaseCreature bc = this;
			string path = Core.BaseDirectory;
			AppendPath(ref path, "Logs");
			AppendPath(ref path, "PvM");

			//path = Path.Combine(path, String.Format("Kills.log"));
			path = Path.Combine(path, $"Attacker.{pm.Name}.log");

			using StreamWriter sw = new StreamWriter(path, true);
			sw.WriteLine("######################################################");
			sw.WriteLine("Attacker: " + pm.Name);
			sw.WriteLine("Attacker's Account: " + pm.Account);
			sw.WriteLine("Monster: " + bc.Name);
			sw.WriteLine("Monster's Location of Death: " + bc.Location);
			sw.WriteLine("Monster's Time of Death: " + DateTime.UtcNow);
			sw.Close();
		}
		var e = new CreatureKilledByEventArgs(this, mob);
		EventSink.InvokeCreatureKilled(e);
		EventSink.InvokeOnKilledBy(this, mob);
	}

	public static void AppendPath(ref string path, string toAppend)
	{
		path = Path.Combine(path, toAppend);

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	public override void OnDeath(Container c)
	{
		MeerMage.StopEffect(this, false);

		if (IsBonded)
		{
			int sound = GetDeathSound();

			if (sound >= 0)
				Effects.PlaySound(this, Map, sound);

			Warmode = false;

			Poison = null;
			Combatant = null;

			BleedAttack.EndBleed(this, false);
			StrangleSpell.RemoveCurse(this);

			Hits = 0;
			Stam = 0;
			Mana = 0;

			IsDeadPet = true;
			ControlTarget = ControlMaster;
			ControlOrder = OrderType.Follow;

			ProcessDeltaQueue();
			SendIncomingPacket();
			SendIncomingPacket();

			List<AggressorInfo> aggressors = Aggressors;

			for (int i = 0; i < aggressors.Count; ++i)
			{
				AggressorInfo info = aggressors[i];

				if (info.Attacker.Combatant == this)
					info.Attacker.Combatant = null;
			}

			List<AggressorInfo> aggressed = Aggressed;

			for (int i = 0; i < aggressed.Count; ++i)
			{
				AggressorInfo info = aggressed[i];

				if (info.Defender.Combatant == this)
					info.Defender.Combatant = null;
			}

			Mobile owner = ControlMaster;

			if (owner == null || owner.Deleted || owner.Map != Map || !owner.InRange(this, 12) || !CanSee(owner) || !InLOS(owner))
			{
				if (OwnerAbandonTime == DateTime.MinValue)
					OwnerAbandonTime = DateTime.UtcNow;
			}
			else
			{
				OwnerAbandonTime = DateTime.MinValue;
			}

			GiftOfLifeSpell.HandleDeath(this);

			CheckStatTimers();
		}
		else
		{
			LootingRights = null;

			if (!Summoned && !NoKillAwards)
			{
				int totalFame = Fame / 100;
				int totalKarma = -Karma / 100;

				if (Map == Map.Felucca)
				{
					totalFame += totalFame / 10 * 3;
					totalKarma += totalKarma / 10 * 3;
				}

				List<DamageStore> list = GetLootingRights();
				List<Mobile> titles = new();
				List<int> fame = new();
				List<int> karma = new();

				bool givenQuestKill = false;
				bool givenFactionKill = false;
				bool givenToTKill = false;

				for (int i = 0; i < list.Count; ++i)
				{
					DamageStore ds = list[i];

					if (!ds.m_HasRight)
						continue;

					if (GivesFameAndKarmaAward)
					{
						Party party = Engines.PartySystem.Party.Get(ds.m_Mobile);

						if (party != null)
						{
							int divedFame = totalFame / party.Members.Count;
							int divedKarma = totalKarma / party.Members.Count;

							foreach (PartyMemberInfo info in party.Members)
							{
								if (info != null && info.Mobile != null)
								{
									int index = titles.IndexOf(info.Mobile);

									if (index == -1)
									{
										titles.Add(info.Mobile);
										fame.Add(divedFame);
										karma.Add(divedKarma);
									}
									else
									{
										fame[index] += divedFame;
										karma[index] += divedKarma;
									}
								}
							}
						}
						else
						{
							titles.Add(ds.m_Mobile);
							fame.Add(totalFame);
							karma.Add(totalKarma);
						}
					}

					OnKilledBy(ds.m_Mobile);

					//if (HumilityVirtue.IsInHunt(ds.m_Mobile) && Karma < 0)
					//{
					//	HumilityVirtue.RegisterKill(ds.m_Mobile, this, list.Count);
					//}

					if (!givenFactionKill)
					{
						givenFactionKill = true;
						Faction.HandleDeath(this, ds.m_Mobile);
					}

					Region region = ds.m_Mobile.Region;

					if (!givenToTKill && (Map == Map.Tokuno || region.IsPartOf("Yomotsu Mines") || region.IsPartOf("Fan Dancer's Dojo")))
					{
						givenToTKill = true;
						TreasuresOfTokuno.HandleKill(this, ds.m_Mobile);
					}

					if (ds.m_Mobile is PlayerMobile pm)
					{
						if (givenQuestKill)
							continue;

						QuestSystem qs = pm.Quest;

						if (qs != null)
						{
							qs.OnKill(this, c);
							givenQuestKill = true;
						}
					}

					//if (this is WeakSkeleton) uokr quest
					//{
					//	pm.CheckKRStartingQuestStep(19);
					//}
					XmlQuest.RegisterKill(this, ds.m_Mobile);
				}

				for (int i = 0; i < titles.Count; ++i)
				{
					Titles.AwardFame(titles[i], fame[i], true);
					Titles.AwardKarma(titles[i], karma[i], true);
				}
			}
			var e = new CreatureDeathEventArgs(this, LastKiller, c);

			EventSink.InvokeCreatureDeath(e);
			if (!c.Deleted)
			{
				int i;

				if (e.ClearCorpse)
				{
					i = c.Items.Count;

					while (--i >= 0)
					{
						if (i >= c.Items.Count)
						{
							continue;
						}

						var o = c.Items[i];

						if (o != null && !o.Deleted)
						{
							o.Delete();
						}
					}
				}

				i = e.ForcedLoot.Count;

				while (--i >= 0)
				{
					if (i >= e.ForcedLoot.Count)
					{
						continue;
					}

					var o = e.ForcedLoot[i];

					if (o != null && !o.Deleted)
					{
						c.DropItem(o);
					}
				}

				e.ClearLoot(false);
			}
			else
			{
				var i = e.ForcedLoot.Count;

				while (--i >= 0)
				{
					if (i >= e.ForcedLoot.Count)
					{
						continue;
					}

					var o = e.ForcedLoot[i];

					if (o != null && !o.Deleted)
					{
						o.Delete();
					}
				}

				e.ClearLoot(true);
			}

			//Pet Death Announce
			if (ControlMaster != null)
			{
				ControlMaster.SendGump(new PetDeathGump(this));
				ControlMaster.SendMessage(33, "WARNING! [PET DEATH!]", ToString());
			}

			base.OnDeath(c);

			if (DeleteCorpseOnDeath || IsArenaMob)
			{
				c.Delete();
			}

			if (e.PreventDefault)
			{
				return;
			}

			if (DeleteCorpseOnDeath && !e.PreventDelete)
			{
				c.Delete();
			}

			if (Summoned)
			{
				UnsummonTimer?.Stop();
			}
		}
	}

	public override void OnDelete()
	{
		Mobile m = m_ControlMaster;

		_ = SetControlMaster(null);
		SummonMaster = null;

		if (ReceivedHonorContext != null)
			ReceivedHonorContext.Cancel();

		base.OnDelete();

		if (m != null)
			m.InvalidateProperties();
	}

	public override bool CanBeHarmful(IDamageable damageable, bool message, bool ignoreOurBlessedness)
	{
		Mobile target = damageable as Mobile;

		if (RecentSetControl && GetMaster() == target)
		{
			return false;
		}

		if (target is BaseFactionGuard)
		{
			return false;
		}

		if ((target is BaseVendor && ((BaseVendor)target).IsInvulnerable) || target is PlayerVendor || target is TownCrier)
		{
			return false;
		}

		//if (damageable is IDamageableItem && !((IDamageableItem)damageable).CanDamage)
		//{
		//	return false;
		//}

		return base.CanBeHarmful(damageable, message, ignoreOurBlessedness);
	}

	public override bool CanBeRenamedBy(Mobile from)
	{
		bool ret = base.CanBeRenamedBy(from);

		if (Controlled && from == ControlMaster && !from.Region.IsPartOf(typeof(Jail)))
			ret = true;

		return ret;
	}

	public bool SetControlMaster(Mobile m)
	{
		if (m == null)
		{
			ControlMaster = null;
			Controlled = false;
			ControlTarget = null;
			ControlOrder = OrderType.None;
			Guild = null;

			Delta(MobileDelta.Noto);
		}
		else
		{
			ISpawner se = Spawner;
			if (se != null && se.UnlinkOnTaming)
			{
				Spawner.Remove(this);
				Spawner = null;
			}

			if (m.Followers + ControlSlots > m.FollowersMax)
			{
				m.SendLocalizedMessage(1049607); // You have too many followers to control that creature.
				return false;
			}

			CurrentWayPoint = null;//so tamed animals don't try to go back

			Home = Point3D.Zero;

			ControlMaster = m;
			Controlled = true;
			ControlTarget = null;
			ControlOrder = OrderType.Come;
			Guild = null;

			if (_mDeleteTimer != null)
			{
				_mDeleteTimer.Stop();
				_mDeleteTimer = null;
			}

			Delta(MobileDelta.Noto);
		}

		InvalidateProperties();

		return true;
	}

	public virtual void OnAfterTame(Mobile tamer)
	{
		if (StatLossAfterTame || Owners.Count == 0)
		{
			AnimalTaming.ScaleStats(this, 0.5);
		}
	}

	public override void OnRegionChange(Region Old, Region New)
	{
		base.OnRegionChange(Old, New);

		if (Controlled)
		{
			if (Spawner is SpawnEntry se && !se.UnlinkOnTaming && (New == null || !New.AcceptsSpawnsFrom(se.Region)))
			{
				Spawner.Remove(this);
				Spawner = null;
			}
		}
	}

	public virtual double GetDispelDifficulty()
	{
		double dif = DispelDifficulty;
		if (SummonMaster != null)
			dif += ArcaneEmpowermentSpell.GetDispellBonus(SummonMaster);
		return dif;
	}

	public static bool Summon(BaseCreature creature, Mobile caster, Point3D p, int sound, TimeSpan duration)
	{
		return Summon(creature, true, caster, p, sound, duration);
	}

	public static bool Summon(BaseCreature creature, bool controlled, Mobile caster, Point3D p, int sound, TimeSpan duration)
	{
		if (caster.Followers + creature.ControlSlots > caster.FollowersMax)
		{
			caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
			creature.Delete();
			return false;
		}

		Summoning = true;

		if (controlled)
			_ = creature.SetControlMaster(caster);

		creature.RangeHome = 10;
		creature.Summoned = true;

		creature.SummonMaster = caster;

		Container pack = creature.Backpack;

		if (pack != null)
		{
			for (int i = pack.Items.Count - 1; i >= 0; --i)
			{
				if (i >= pack.Items.Count)
					continue;

				pack.Items[i].Delete();
			}
		}

		creature.SetHits((int)Math.Floor(creature.HitsMax * (1 + ArcaneEmpowermentSpell.GetSpellBonus(caster, false) / 100.0)));

		new UnsummonTimer(caster, creature, duration).Start();
		creature.SummonEnd = DateTime.UtcNow + duration;

		creature.MoveToWorld(p, caster.Map);
		creature.OnAfterSpawn();

		Effects.PlaySound(p, creature.Map, sound);

		if (creature is EnergyVortex || creature is BladeSpirits)
		{
			SpellHelper.CheckSummonLimits(creature);
		}

		Summoning = false;

		return true;
	}

	#region Spawn Position
	public virtual Point3D GetSpawnPosition(int range)
	{
		return GetSpawnPosition(Location, Map, range);
	}

	public static Point3D GetSpawnPosition(Point3D from, Map map, int range)
	{
		if (map == null)
			return from;

		for (int i = 0; i < 10; i++)
		{
			int x = from.X + Utility.RandomMinMax(-range, range);
			int y = from.Y + Utility.RandomMinMax(-range, range);
			int z = map.GetAverageZ(x, y);

			Point3D p = new(x, y, from.Z);

			if (map.CanSpawnMobile(p) && map.LineOfSight(from, p))
				return p;

			p = new Point3D(x, y, z);

			if (map.CanSpawnMobile(p) && map.LineOfSight(from, p))
				return p;
		}

		return from;
	}
	#endregion

	#region Healing
	public virtual void HealStart(Mobile patient)
	{
		bool onSelf = patient == this;

		//DoBeneficial( patient );

		RevealingAction();

		if (!onSelf)
		{
			patient.RevealingAction();
			patient.SendLocalizedMessage(1008078, false, Name); //  : Attempting to heal you.
		}

		double seconds = (onSelf ? HealDelay : HealOwnerDelay) + (patient.Alive ? 0.0 : 5.0);

		m_HealTimer = Timer.DelayCall(TimeSpan.FromSeconds(seconds), new TimerStateCallback(Heal_Callback), patient);
	}

	private void Heal_Callback(object state)
	{
		if (state is Mobile mob)
			Heal(mob);
	}

	public virtual void Heal(Mobile patient)
	{
		if (!Alive || Map == Map.Internal || !CanBeBeneficial(patient, true, true) || patient.Map != Map || !InRange(patient, HealEndRange))
		{
			StopHeal();
			return;
		}

		bool onSelf = (patient == this);

		if (!patient.Alive)
		{
		}
		else if (patient.Poisoned)
		{
			int poisonLevel = patient.Poison.Level;

			double healing = Skills.Healing.Value;
			double anatomy = Skills.Anatomy.Value;
			double chance = (healing - 30.0) / 50.0 - poisonLevel * 0.1;

			if ((healing >= 60.0 && anatomy >= 60.0) && chance > Utility.RandomDouble())
			{
				if (patient.CurePoison(this))
				{
					patient.SendLocalizedMessage(1010059); // You have been cured of all poisons.

					_ = CheckSkill(SkillName.Healing, 0.0, 60.0 + poisonLevel * 10.0); // TODO: Verify formula
					_ = CheckSkill(SkillName.Anatomy, 0.0, 100.0);
				}
			}
		}
		else if (BleedAttack.IsBleeding(patient))
		{
			patient.SendLocalizedMessage(1060167); // The bleeding wounds have healed, you are no longer bleeding!
			BleedAttack.EndBleed(patient, false);
		}
		else
		{
			double healing = Skills.Healing.Value;
			double anatomy = Skills.Anatomy.Value;
			double chance = (healing + 10.0) / 100.0;

			if (chance > Utility.RandomDouble())
			{
				double min, max;

				min = (anatomy / 10.0) + (healing / 6.0) + 4.0;
				max = (anatomy / 8.0) + (healing / 3.0) + 4.0;

				if (onSelf)
					max += 10;

				double toHeal = min + (Utility.RandomDouble() * (max - min));

				toHeal *= HealScalar;

				patient.Heal((int)toHeal);

				_ = CheckSkill(SkillName.Healing, 0.0, 90.0);
				_ = CheckSkill(SkillName.Anatomy, 0.0, 100.0);
			}
		}

		HealEffect(patient);

		StopHeal();

		if ((onSelf && HealFully && Hits >= HealTrigger * HitsMax && Hits < HitsMax) || (!onSelf && HealOwnerFully && patient.Hits >= HealOwnerTrigger * patient.HitsMax && patient.Hits < patient.HitsMax))
			HealStart(patient);
	}

	public virtual void StopHeal()
	{
		if (m_HealTimer != null)
			m_HealTimer.Stop();

		m_HealTimer = null;
	}

	public virtual void HealEffect(Mobile patient)
	{
		patient.PlaySound(HealSound);
	}
	#endregion

	#region Damaging Aura
	public virtual void AuraDamage()
	{
		if (!Alive || IsDeadBondedPet)
			return;

		List<Mobile> list = new();

		foreach (Mobile m in GetMobilesInRange(AuraRange))
		{
			if (m == this || !CanBeHarmful(m, false) || (Core.AOS && !InLOS(m)))
				continue;

			if (m is BaseCreature bc)
			{
				if (bc.Controlled || bc.Summoned || bc.Team != Team)
					list.Add(m);
			}
			else if (m.Player)
			{
				list.Add(m);
			}
		}

		foreach (Mobile m in list)
		{
			_ = AOS.Damage(m, this, AuraBaseDamage, AuraPhysicalDamage, AuraFireDamage, AuraColdDamage, AuraPoisonDamage, AuraEnergyDamage, AuraChaosDamage);
			AuraEffect(m);
		}
	}

	public virtual void AuraEffect(Mobile m)
	{
	}
	#endregion

	#region TeleportTo
	public void TryTeleport()
	{
		if (Deleted)
			return;

		if (TeleportProb > Utility.RandomDouble())
		{
			Mobile toTeleport = GetTeleportTarget();

			if (toTeleport != null)
			{
				int offset = Utility.Random(8) * 2;

				Point3D to = Location;

				for (int i = 0; i < Helpers.Offsets.Length; i += 2)
				{
					int x = X + Helpers.Offsets[(offset + i) % Helpers.Offsets.Length];
					int y = Y + Helpers.Offsets[(offset + i + 1) % Helpers.Offsets.Length];

					if (Map.CanSpawnMobile(x, y, Z))
					{
						to = new Point3D(x, y, Z);
						break;
					}
					else
					{
						int z = Map.GetAverageZ(x, y);

						if (Map.CanSpawnMobile(x, y, z))
						{
							to = new Point3D(x, y, z);
							break;
						}
					}
				}

				Point3D from = toTeleport.Location;
				toTeleport.MoveToWorld(to, Map);
				SpellHelper.Turn(this, toTeleport);
				SpellHelper.Turn(toTeleport, this);
				toTeleport.ProcessDelta();
				Effects.SendLocationParticles(EffectItem.Create(from, Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);
				Effects.SendLocationParticles(EffectItem.Create(to, Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
				toTeleport.PlaySound(0x1FE);
				Combatant = toTeleport;
				OnAfterTeleport(toTeleport);
			}
		}
	}

	public virtual Mobile GetTeleportTarget()
	{
		IPooledEnumerable eable = GetMobilesInRange(TeleportRange);
		List<Mobile> list = new();

		foreach (Mobile m in eable)
		{
			bool isPet = m is BaseCreature creature && creature.GetMaster() is PlayerMobile;

			if (m != this && (m.Player || (TeleportsPets && isPet)) && CanBeHarmful(m) && CanSee(m))
			{
				list.Add(m);
			}
		}

		eable.Free();

		Mobile mob = null;

		if (list.Count > 0)
			mob = list[Utility.Random(list.Count)];

		ColUtility.Free(list);
		return mob;
	}

	public virtual void OnAfterTeleport(Mobile m)
	{
	}
	#endregion

	#region Detect Hidden
	public virtual void TryFindPlayer()
	{
		if (Deleted || Map == null)
		{
			return;
		}

		double srcSkill = Skills[SkillName.DetectHidden].Value;

		if (srcSkill <= 0)
		{
			return;
		}

		DetectHidden.OnUse(this);

		if (Target is DetectHidden.DetectHiddenTarget)
		{
			Target.Invoke(this, this);
			DebugSay("Checking for hidden players");
		}
		else
		{
			DebugSay("Failed Checking for hidden players");
		}
	}
	#endregion

	public virtual void OnThink()
	{
		long tc = Core.TickCount;

		if (Paralyzed || Frozen)
		{
			return;
		}

		if (EnableRummaging && CanRummageCorpses && !Summoned && !Controlled && tc - m_NextRummageTime >= 0)
		{
			double min, max;

			if (ChanceToRummage > Utility.RandomDouble() && Rummage())
			{
				min = MinutesToNextRummageMin;
				max = MinutesToNextRummageMax;
			}
			else
			{
				min = MinutesToNextChanceMin;
				max = MinutesToNextChanceMax;
			}

			double delay = min + (Utility.RandomDouble() * (max - min));
			m_NextRummageTime = tc + (int)TimeSpan.FromMinutes(delay).TotalMilliseconds;
		}

		if (Controlled && AIObject is SuperAI aI && Hits < (HitsMax - 10) && Combatant == null)
		{
			Target targ = Target;
			if (targ != null)
			{
				aI.ProcessTarget(targ);
			}
			else
			{
				aI.DoHealingAction(true);
			}
		}

		if (CanBreath && tc - m_NextBreathTime >= 0) // tested: controlled dragons do breath fire, what about summoned skeletal dragons?
		{
			if (Combatant is Mobile target && target.Alive && !target.IsDeadBondedPet && CanBeHarmful(target) && target.Map == Map && !IsDeadBondedPet && target.InRange(this, BreathRange) && InLOS(target) && !BardPacified)
			{
				if ((Core.TickCount - m_NextBreathTime) < 30000 && Utility.RandomBool())
				{
					BreathStart(target);
				}

				m_NextBreathTime = tc + (int)TimeSpan.FromSeconds(BreathMinDelay + (Utility.RandomDouble() * (BreathMaxDelay - BreathMinDelay))).TotalMilliseconds;
			}
		}

		if ((CanHeal || CanHealOwner) && Alive && !IsHealing && !BardPacified)
		{
			Mobile owner = ControlMaster;

			if (owner != null && CanHealOwner && tc - m_NextHealOwnerTime >= 0 && CanBeBeneficial(owner, true, true) && owner.Map == Map && InRange(owner, HealStartRange) && InLOS(owner) && owner.Hits < HealOwnerTrigger * owner.HitsMax)
			{
				HealStart(owner);

				m_NextHealOwnerTime = tc + (int)TimeSpan.FromSeconds(HealOwnerInterval).TotalMilliseconds;
			}
			else if (CanHeal && tc - m_NextHealTime >= 0 && CanBeBeneficial(this) && (Hits < HealTrigger * HitsMax || Poisoned))
			{
				HealStart(this);

				m_NextHealTime = tc + (int)TimeSpan.FromSeconds(HealInterval).TotalMilliseconds;
			}
		}

		if (ReturnsToHome && IsSpawnerBound() && !InRange(Home, RangeHome))
		{
			if ((Combatant == null) && (Warmode == false) && Utility.RandomDouble() < .10)  /* some throttling */
			{
				m_FailedReturnHome = !Move(GetDirectionTo(Home.X, Home.Y)) ? m_FailedReturnHome + 1 : 0;

				if (m_FailedReturnHome > 5)
				{
					SetLocation(Home, true);

					m_FailedReturnHome = 0;
				}
			}
		}
		else
		{
			m_FailedReturnHome = 0;
		}

		if (Combatant is Mobile && TeleportsTo && tc >= m_NextTeleport)
		{
			TryTeleport();
			m_NextTeleport = tc + (int)TeleportDuration.TotalMilliseconds;
		}

		if (CanDetectHidden && Core.TickCount >= _NextDetect)
		{
			TryFindPlayer();

			// Not exactly OSI style, approximation.
			int delay = FindPlayerDelayBase;

			if (delay > FindPlayerDelayMax)
			{
				delay = FindPlayerDelayMax; // 60s max at 250 int
			}
			else if (delay < FindPlayerDelayMin)
			{
				delay = FindPlayerDelayMin; // 5s min at 3000 int
			}

			int min = delay * (FindPlayerDelayLow / FindPlayerDelayHigh); // 13s at 1000 int, 33s at 400 int, 54s at <250 int
			int max = delay * (FindPlayerDelayHigh / FindPlayerDelayLow); // 16s at 1000 int, 41s at 400 int, 66s at <250 int

			_NextDetect = Core.TickCount +
				(int)TimeSpan.FromSeconds(Utility.RandomMinMax(min, max)).TotalMilliseconds;
		}

		if (HasAura && tc - m_NextAura >= 0)
		{
			AuraDamage();
			m_NextAura = tc + (int)AuraInterval.TotalMilliseconds;
		}

		if (Dispelonsummonerdeath && (m_SummonMaster == null || !m_SummonMaster.Alive) && (m_ControlMaster == null || !m_ControlMaster.Alive))
		{
			Dispel(this);
		}

		CheckSawTamer(true, 10);
	}

	public virtual bool Rummage()
	{
		Corpse toRummage = null;

		IPooledEnumerable eable = GetItemsInRange(2);
		foreach (Item item in eable)
		{
			if (item is Corpse corpse && item.Items.Count > 0)
			{
				toRummage = corpse;
				break;
			}
		}
		eable.Free();

		if (toRummage == null)
			return false;

		Container pack = Backpack;

		if (pack == null)
			return false;

		List<Item> items = toRummage.Items;

		for (int i = 0; i < items.Count; ++i)
		{
			Item item = items[Utility.Random(items.Count)];

			Lift(item, item.Amount, out bool rejected, out LRReason _);

			if (!rejected && Drop(this, new Point3D(-1, -1, 0)))
			{
				// *rummages through a corpse and takes an item*
				PublicOverheadMessage(MessageType.Emote, 0x3B2, 1008086);
				//TODO: Instancing of Rummaged stuff.
				return true;
			}
		}

		return false;
	}

	public override Mobile GetDamageMaster(Mobile damagee)
	{
		if (BardProvoked && damagee == BardTarget)
			return BardMaster;
		else if (m_bControlled && m_ControlMaster != null)
			return m_ControlMaster;
		else if (m_bSummoned && m_SummonMaster != null)
			return m_SummonMaster;

		return base.GetDamageMaster(damagee);
	}

	public void Provoke(Mobile master, Mobile target, bool bSuccess)
	{
		BardProvoked = true;

		if (!Core.ML)
		{
			PublicOverheadMessage(MessageType.Emote, EmoteHue, false, "*looks furious*");
		}

		if (bSuccess)
		{
			PlaySound(GetIdleSound());

			BardMaster = master;
			BardTarget = target;
			Combatant = target;
			BardEndTime = DateTime.UtcNow + TimeSpan.FromSeconds(30.0);

			if (target is BaseCreature t)
			{
				if (t.Unprovokable || (t.IsParagon && BaseInstrument.GetBaseDifficulty(t) >= 160.0))
					return;

				t.BardProvoked = true;

				t.BardMaster = master;
				t.BardTarget = this;
				t.Combatant = this;
				t.BardEndTime = DateTime.UtcNow + TimeSpan.FromSeconds(30.0);
			}
		}
		else
		{
			PlaySound(GetAngerSound());

			BardMaster = master;
			BardTarget = target;
		}
	}

	public bool FindMyName(string str, bool bWithAll)
	{
		int i, j;

		string name = Name;

		if (name == null || str.Length < name.Length)
			return false;

		string[] wordsString = str.Split(' ');
		string[] wordsName = name.Split(' ');

		for (j = 0; j < wordsName.Length; j++)
		{
			string wordName = wordsName[j];

			bool bFound = false;
			for (i = 0; i < wordsString.Length; i++)
			{
				string word = wordsString[i];

				if (Insensitive.Equals(word, wordName))
					bFound = true;

				if (bWithAll && Insensitive.Equals(word, "all"))
					return true;
			}

			if (!bFound)
				return false;
		}

		return true;
	}

	public static void TeleportPets(Mobile master, Point3D loc, Map map)
	{
		TeleportPets(master, loc, map, false);
	}

	public static void TeleportPets(Mobile master, Point3D loc, Map map, bool onlyBonded)
	{
		TeleportPets(master, loc, map, onlyBonded, 3, false);
	}

	public static void TeleportPets(Mobile master, Point3D loc, Map map, bool onlyBonded, int range, bool anycommand)
	{
		var move = new List<Mobile>();

		IPooledEnumerable eable = master.GetMobilesInRange(range);

		foreach (Mobile m in eable)
		{
			if (m is BaseCreature pet)
			{
				if (pet.Controlled && pet.ControlMaster == master)
				{
					if (!onlyBonded || pet.IsBonded)
					{
						if (pet.ControlOrder == OrderType.Guard || pet.ControlOrder == OrderType.Follow ||
							pet.ControlOrder == OrderType.Come)
						{
							move.Add(pet);
						}
						else if (anycommand)
						{
							move.Add(pet);
						}
					}
				}
			}
		}

		eable.Free();

		foreach (Mobile m in move)
		{
			m.MoveToWorld(loc, map);
		}

		ColUtility.Free(move);
	}

	public virtual void ResurrectPet()
	{
		if (!IsDeadPet)
			return;

		OnBeforeResurrect();

		Poison = null;

		Warmode = false;

		Hits = 10;
		Stam = StamMax;
		Mana = 0;

		ProcessDeltaQueue();

		IsDeadPet = false;

		Effects.SendPacket(Location, Map, new BondedStatus(0, Serial, 0));

		SendIncomingPacket();
		SendIncomingPacket();

		OnAfterResurrect();

		Mobile owner = ControlMaster;

		if (owner == null || owner.Deleted || owner.Map != Map || !owner.InRange(this, 12) || !CanSee(owner) || !InLOS(owner))
		{
			if (OwnerAbandonTime == DateTime.MinValue)
				OwnerAbandonTime = DateTime.UtcNow;
		}
		else
		{
			OwnerAbandonTime = DateTime.MinValue;
		}

		CheckStatTimers();
	}

	public override bool CanBeDamaged()
	{
		if (IsDeadPet || IsInvulnerable)
			return false;

		return base.CanBeDamaged();
	}

	/* until we are sure about who should be getting deleted, move them instead */
	/* On OSI, they despawn */
	private bool IsSpawnerBound()
	{
		if ((Map != null) && (Map != Map.Internal))
		{
			if (FightMode != FightMode.None && (RangeHome >= 0))
			{
				if (!Controlled && !Summoned)
				{
					if (Spawner != null && Spawner is Spawner && ((Spawner as Spawner).Map) == Map)
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	public override void OnSectorDeactivate()
	{
		if (!Deleted && ReturnsToHome && IsSpawnerBound() && !InRange(Home, RangeHome + 5))
		{
			_ = Timer.DelayCall(TimeSpan.FromSeconds(Utility.Random(45) + 15), new TimerCallback(GoHome_Callback));

			m_ReturnQueued = true;
		}
		else if (PlayerRangeSensitive && AIObject != null)
		{
			AIObject.Deactivate();
		}

		base.OnSectorDeactivate();
	}

	public void GoHome_Callback()
	{
		if (m_ReturnQueued && IsSpawnerBound())
		{
			if (!((Map.GetSector(X, Y)).Active))
			{
				SetLocation(Home, true);

				if (!((Map.GetSector(X, Y)).Active) && AIObject != null)
				{
					AIObject.Deactivate();
				}
			}
		}

		m_ReturnQueued = false;
	}

	public override void OnSectorActivate()
	{
		if (PlayerRangeSensitive && AIObject != null)
		{
			AIObject.Activate();
		}

		base.OnSectorActivate();
	}
}
