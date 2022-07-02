using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Network;

namespace Server;

public enum StatCode
{
	Str,
	Dex,
	Int
}

public delegate TimeSpan SkillUseCallback(Mobile user);

public enum SkillLock : byte
{
	Up = 0,
	Down = 1,
	Locked = 2
}

public enum SkillName
{
	Alchemy = 0,
	Anatomy = 1,
	AnimalLore = 2,
	ItemID = 3,
	ArmsLore = 4,
	Parry = 5,
	Begging = 6,
	Blacksmith = 7,
	Fletching = 8,
	Peacemaking = 9,
	Camping = 10,
	Carpentry = 11,
	Cartography = 12,
	Cooking = 13,
	DetectHidden = 14,
	Discordance = 15,
	EvalInt = 16,
	Healing = 17,
	Fishing = 18,
	Forensics = 19,
	Herding = 20,
	Hiding = 21,
	Provocation = 22,
	Inscribe = 23,
	Lockpicking = 24,
	Magery = 25,
	MagicResist = 26,
	Tactics = 27,
	Snooping = 28,
	Musicianship = 29,
	Poisoning = 30,
	Archery = 31,
	SpiritSpeak = 32,
	Stealing = 33,
	Tailoring = 34,
	AnimalTaming = 35,
	TasteID = 36,
	Tinkering = 37,
	Tracking = 38,
	Veterinary = 39,
	Swords = 40,
	Macing = 41,
	Fencing = 42,
	Wrestling = 43,
	Lumberjacking = 44,
	Mining = 45,
	Meditation = 46,
	Stealth = 47,
	RemoveTrap = 48,
	Necromancy = 49,
	Focus = 50,
	Chivalry = 51,
	Bushido = 52,
	Ninjitsu = 53,
	Spellweaving = 54,
	Mysticism = 55,
	Imbuing = 56,
	Throwing = 57
}

[PropertyObject]
public class Skill
{
	private ushort _base;
	private ushort _cap;

	public override string ToString()
	{
		return $"[{Name}: {Base}]";
	}

	public Skill(Skills owner, SkillInfo info, GenericReader reader)
	{
		Owner = owner;
		Info = info;

		int version = reader.ReadByte();

		switch (version)
		{
			case 0:
			{
				_base = reader.ReadUShort();
				_cap = reader.ReadUShort();
				Lock = (SkillLock)reader.ReadByte();

				break;
			}
			case 0xFF:
			{
				_base = 0;
				_cap = 1000;
				Lock = SkillLock.Up;

				break;
			}
			default:
			{
				if ((version & 0xC0) == 0x00)
				{
					if ((version & 0x1) != 0)
					{
						_base = reader.ReadUShort();
					}

					if ((version & 0x2) != 0)
					{
						_cap = reader.ReadUShort();
					}
					else
					{
						_cap = 1000;
					}

					if ((version & 0x4) != 0)
					{
						Lock = (SkillLock)reader.ReadByte();
					}

					if ((version & 0x8) != 0)
					{
						VolumeLearned = reader.ReadInt();
					}

					if ((version & 0x10) != 0)
					{
						NextGgsGain = reader.ReadDateTime();
					}
				}

				break;
			}
		}

		if (Lock is < SkillLock.Up or > SkillLock.Locked)
		{
			Console.WriteLine("Bad skill lock -> {0}.{1}", owner.Owner, Lock);
			Lock = SkillLock.Up;
		}
	}

	public Skill(Skills owner, SkillInfo info, int baseValue, int cap, SkillLock skillLock)
	{
		Owner = owner;
		Info = info;
		_base = (ushort)baseValue;
		_cap = (ushort)cap;
		Lock = skillLock;
	}

	public void SetLockNoRelay(SkillLock skillLock)
	{
		if (skillLock is < SkillLock.Up or > SkillLock.Locked)
		{
			return;
		}

		Lock = skillLock;
	}

	public void Serialize(GenericWriter writer)
	{
		if (_base == 0 && _cap == 1000 && Lock == SkillLock.Up && VolumeLearned == 0 && NextGgsGain == DateTime.MinValue)
		{
			writer.Write((byte)0xFF); // default
		}
		else
		{
			int flags = 0x0;

			if (_base != 0)
			{
				flags |= 0x1;
			}

			if (_cap != 1000)
			{
				flags |= 0x2;
			}

			if (Lock != SkillLock.Up)
			{
				flags |= 0x4;
			}

			if (VolumeLearned != 0)
			{
				flags |= 0x8;
			}

			if (NextGgsGain != DateTime.MinValue)
			{
				flags |= 0x10;
			}

			writer.Write((byte)flags); // version

			if (_base != 0)
			{
				writer.Write((short)_base);
			}

			if (_cap != 1000)
			{
				writer.Write((short)_cap);
			}

			if (Lock != SkillLock.Up)
			{
				writer.Write((byte)Lock);
			}

			if (VolumeLearned != 0)
			{
				writer.Write(VolumeLearned);
			}

			if (NextGgsGain != DateTime.MinValue)
			{
				writer.Write(NextGgsGain);
			}
		}
	}

	public Skills Owner { get; }

	public SkillName SkillName => (SkillName)Info.SkillID;

	public int SkillID => Info.SkillID;

	[CommandProperty(AccessLevel.Counselor)]
	public string Name => Info.Name;

	public SkillInfo Info { get; }

	[CommandProperty(AccessLevel.Counselor)]
	public SkillLock Lock { get; private set; }

	[CommandProperty(AccessLevel.Counselor)]
	public int VolumeLearned
	{
		get;
		set;
	}

	[CommandProperty(AccessLevel.Counselor)]
	public DateTime NextGgsGain
	{
		get;
		set;
	}

	public int BaseFixedPoint
	{
		get => _base;
		set
		{
			value = value switch
			{
				< 0 => 0,
				>= 0x10000 => 0xFFFF,
				_ => value
			};

			ushort sv = (ushort)value;

			int oldBase = _base;

			if (_base != sv)
			{
				Owner.Total = Owner.Total - _base + sv;

				_base = sv;

				Owner.OnSkillChange(this);

				Mobile m = Owner.Owner;

				m?.OnSkillChange(SkillName, (double)oldBase / 10);
			}
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public double Base { get => _base / 10.0; set => BaseFixedPoint = (int)(value * 10.0); }

	public int CapFixedPoint
	{
		get => _cap;
		set
		{
			value = value switch
			{
				< 0 => 0,
				>= 0x10000 => 0xFFFF,
				_ => value
			};

			ushort sv = (ushort)value;

			if (_cap != sv)
			{
				_cap = sv;

				Owner.OnSkillChange(this);
			}
		}
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public double Cap
	{
		get => _cap / 10.0;
		set
		{
			_ = _cap / 10;

			CapFixedPoint = (int)(value * 10.0);

			//if (old != value && Owner.Owner != null)
			//{
			//EventSink.InvokeSkillCapChange(new SkillCapChangeEventArgs(Owner.Owner, this, old, value));
			//}
		}
	}

	public static bool UseStatMods { get; set; }

	public int Fixed => (int)(Value * 10);

	[CommandProperty(AccessLevel.Counselor)]
	public double Value
	{
		get
		{
			//There has to be this distinction between the racial values and not to account for gaining skills and these skills aren't displayed nor Totaled up.
			double value = NonRacialValue;

			double raceBonus = Owner.Owner.GetRacialSkillBonus(SkillName);

			if (raceBonus > value)
			{
				value = raceBonus;
			}

			return value;
		}
	}

	[CommandProperty(AccessLevel.Counselor)]
	public double NonRacialValue
	{
		get
		{
			double baseValue = Base;
			double inv = 100.0 - baseValue;

			if (inv < 0.0)
			{
				inv = 0.0;
			}

			inv /= 100.0;

			double statsOffset = (UseStatMods ? Owner.Owner.Str : Owner.Owner.RawStr) * Info.StrScale +
			                     (UseStatMods ? Owner.Owner.Dex : Owner.Owner.RawDex) * Info.DexScale +
			                     (UseStatMods ? Owner.Owner.Int : Owner.Owner.RawInt) * Info.IntScale;
			double statTotal = Info.StatTotal * inv;

			statsOffset *= inv;

			if (statsOffset > statTotal)
			{
				statsOffset = statTotal;
			}

			double value = baseValue + statsOffset;

			Owner.Owner.ValidateSkillMods();

			var mods = Owner.Owner.SkillMods;

			double bonusObey = 0.0, bonusNotObey = 0.0;

			for (int i = 0; i < mods.Count; ++i)
			{
				SkillMod mod = mods[i];

				if (mod.Skill == (SkillName)Info.SkillID)
				{
					if (mod.Relative)
					{
						if (mod.ObeyCap)
						{
							bonusObey += mod.Value;
						}
						else
						{
							bonusNotObey += mod.Value;
						}
					}
					else
					{
						bonusObey = 0.0;
						bonusNotObey = 0.0;
						value = mod.Value;
					}
				}
			}

			value += bonusNotObey;

			if (value < Cap)
			{
				value += bonusObey;

				if (value > Cap)
				{
					value = Cap;
				}
			}

			Owner.Owner.MutateSkill((SkillName)Info.SkillID, ref value);

			return value;
		}
	}

	public bool IsMastery => Info.IsMastery;

	public bool LearnMastery(int volume)
	{
		if (!IsMastery || HasLearnedVolume(volume))
			return false;

		VolumeLearned = volume;

		if (VolumeLearned > 3)
			VolumeLearned = 3;

		if (VolumeLearned < 0)
			VolumeLearned = 0;

		return true;
	}

	public bool HasLearnedVolume(int volume)
	{
		return VolumeLearned >= volume;
	}

	public bool HasLearnedMastery()
	{
		return VolumeLearned > 0;
	}

	public bool SetCurrent()
	{
		if (IsMastery)
		{
			Owner.CurrentMastery = (SkillName)Info.SkillID;
			return true;
		}

		return false;
	}

	public void Update()
	{
		Owner.OnSkillChange(this);
	}
}

public class SkillInfo
{
	public SkillInfo(
		int skillId,
		string name,
		double strScale,
		double dexScale,
		double intScale,
		string title,
		SkillUseCallback callback,
		double strGain,
		double dexGain,
		double intGain,
		double gainFactor,
		StatCode primary,
		StatCode secondary,
		bool mastery = false,
		bool usewhilecasting = false)
	{
		Name = name;
		Title = title;
		SkillID = skillId;
		StrScale = strScale / 100.0;
		DexScale = dexScale / 100.0;
		IntScale = intScale / 100.0;
		Callback = callback;
		StrGain = strGain;
		DexGain = dexGain;
		IntGain = intGain;
		GainFactor = gainFactor;
		Primary = primary;
		Secondary = secondary;
		IsMastery = mastery;
		UseWhileCasting = usewhilecasting;

		StatTotal = strScale + dexScale + intScale;
	}

	public StatCode Primary { get; private set; }
	public StatCode Secondary { get; private set; }

	public SkillUseCallback Callback { get; set; }
	public int SkillID { get; }
	public string Name { get; set; }
	public string Title { get; set; }
	public double StrScale { get; set; }
	public double DexScale { get; set; }
	public double IntScale { get; set; }
	public double StatTotal { get; set; }
	public double StrGain { get; set; }
	public double DexGain { get; set; }
	public double IntGain { get; set; }
	public double GainFactor { get; set; }
	public bool IsMastery { get; set; }
	public bool UseWhileCasting { get; set; }
	public int Localization => 1044060 + SkillID;

	public static SkillInfo[] Table { get; set; } = {
		new(0, "Alchemy", 0.0, 5.0, 5.0, "Alchemist", null, 0.0, 0.5, 0.5, 1.0, StatCode.Int, StatCode.Dex),
		new(1, "Anatomy", 0.0, 0.0, 0.0, "Biologist", null, 0.15, 0.15, 0.7, 1.0, StatCode.Int, StatCode.Str),
		new(2, "Animal Lore", 0.0, 0.0, 0.0, "Naturalist", null, 0.0, 0.0, 1.0, 1.0, StatCode.Int, StatCode.Str),
		new(3, "Item Identification", 0.0, 0.0, 0.0, "Merchant", null, 0.0, 0.0, 1.0, 1.0, StatCode.Int, StatCode.Dex),
		new(4, "Arms Lore", 0.0, 0.0, 0.0, "Weapon Master", null, 0.75, 0.15, 0.1, 1.0, StatCode.Int, StatCode.Str),
		new(5, "Parrying", 7.5, 2.5, 0.0, "Duelist", null, 0.75, 0.25, 0.0, 1.0, StatCode.Dex, StatCode.Str, true ),
		new(6, "Begging", 0.0, 0.0, 0.0, "Beggar", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(7, "Blacksmithy", 10.0, 0.0, 0.0, "Blacksmith", null, 1.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Dex),
		new(8, "Bowcraft/Fletching", 6.0, 16.0, 0.0, "Bowyer", null, 0.6, 1.6, 0.0, 1.0, StatCode.Dex, StatCode.Str),
		new(9, "Peacemaking", 0.0, 0.0, 0.0, "Pacifier", null, 0.0, 0.0, 0.0, 1.0, StatCode.Int, StatCode.Dex, true ),
		new(10, "Camping", 20.0, 15.0, 15.0, "Explorer", null, 2.0, 1.5, 1.5, 1.0, StatCode.Dex, StatCode.Int),
		new(11, "Carpentry", 20.0, 5.0, 0.0, "Carpenter", null, 2.0, 0.5, 0.0, 1.0, StatCode.Str, StatCode.Dex),
		new(12, "Cartography", 0.0, 7.5, 7.5, "Cartographer", null, 0.0, 0.75, 0.75, 1.0, StatCode.Int, StatCode.Dex),
		new(13, "Cooking", 0.0, 20.0, 30.0, "Chef", null, 0.0, 2.0, 3.0, 1.0, StatCode.Int, StatCode.Dex),
		new(14, "Detecting Hidden", 0.0, 0.0, 0.0, "Scout", null, 0.0, 0.4, 0.6, 1.0, StatCode.Int, StatCode.Dex),
		new(15, "Discordance", 0.0, 2.5, 2.5, "Demoralizer", null, 0.0, 0.25, 0.25, 1.0, StatCode.Dex, StatCode.Int, true ),
		new(16, "Evaluating Intelligence", 0.0, 0.0, 0.0, "Scholar", null, 0.0, 0.0, 1.0, 1.0, StatCode.Int, StatCode.Str),
		new(17, "Healing", 6.0, 6.0, 8.0, "Healer", null, 0.6, 0.6, 0.8, 1.0, StatCode.Int, StatCode.Dex),
		new(18, "Fishing", 0.0, 0.0, 0.0, "Fisherman", null, 0.5, 0.5, 0.0, 1.0, StatCode.Dex, StatCode.Str),
		new(19, "Forensic Evaluation", 0.0, 0.0, 0.0, "Detective", null, 0.0, 0.2, 0.8, 1.0, StatCode.Int, StatCode.Dex),
		new(20, "Herding", 16.25, 6.25, 2.5, "Shepherd", null, 1.625, 0.625, 0.25, 1.0, StatCode.Int, StatCode.Dex),
		new(21, "Hiding", 0.0, 0.0, 0.0, "Shade", null, 0.0, 0.8, 0.2, 1.0, StatCode.Dex, StatCode.Int),
		new(22, "Provocation", 0.0, 4.5, 0.5, "Rouser", null, 0.0, 0.45, 0.05, 1.0, StatCode.Int, StatCode.Dex, true ),
		new(23, "Inscription", 0.0, 2.0, 8.0, "Scribe", null, 0.0, 0.2, 0.8, 1.0, StatCode.Int, StatCode.Dex),
		new(24, "Lockpicking", 0.0, 25.0, 0.0, "Infiltrator", null, 0.0, 2.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(25, "Magery", 0.0, 0.0, 15.0, "Mage", null, 0.0, 0.0, 1.5, 1.0, StatCode.Int, StatCode.Str, true ),
		new(26, "Resisting Spells", 0.0, 0.0, 0.0, "Warder", null, 0.25, 0.25, 0.5, 1.0, StatCode.Str, StatCode.Dex),
		new(27, "Tactics", 0.0, 0.0, 0.0, "Tactician", null, 0.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Dex),
		new(28, "Snooping", 0.0, 25.0, 0.0, "Spy", null, 0.0, 2.5, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(29, "Musicianship", 0.0, 0.0, 0.0, "Bard", null, 0.0, 0.8, 0.2, 1.0, StatCode.Dex, StatCode.Int),
		new(30, "Poisoning", 0.0, 4.0, 16.0, "Assassin", null, 0.0, 0.4, 1.6, 1.0, StatCode.Int, StatCode.Dex, true ),
		new(31, "Archery", 2.5, 7.5, 0.0, "Archer", null, 0.25, 0.75, 0.0, 1.0, StatCode.Dex, StatCode.Str, true ),
		new(32, "Spirit Speak", 0.0, 0.0, 0.0, "Medium", null, 0.0, 0.0, 1.0, 1.0, StatCode.Int, StatCode.Str, false, true),
		new(33, "Stealing", 0.0, 10.0, 0.0, "Pickpocket", null, 0.0, 1.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(34, "Tailoring", 3.75, 16.25, 5.0, "Tailor", null, 0.38, 1.63, 0.5, 1.0, StatCode.Dex, StatCode.Int),
		new(35, "Animal Taming", 14.0, 2.0, 4.0, "Tamer", null, 1.4, 0.2, 0.4, 1.0, StatCode.Str, StatCode.Int, true ),
		new(36, "Taste Identification", 0.0, 0.0, 0.0, "Praegustator", null, 0.2, 0.0, 0.8, 1.0, StatCode.Int, StatCode.Str),
		new(37, "Tinkering", 5.0, 2.0, 3.0, "Tinker", null, 0.5, 0.2, 0.3, 1.0, StatCode.Dex, StatCode.Int),
		new(38, "Tracking", 0.0, 12.5, 12.5, "Ranger", null, 0.0, 1.25, 1.25, 1.0, StatCode.Int, StatCode.Dex),
		new(39, "Veterinary", 8.0, 4.0, 8.0, "Veterinarian", null, 0.8, 0.4, 0.8, 1.0, StatCode.Int, StatCode.Dex),
		new(40, "Swordsmanship", 7.5, 2.5, 0.0, "Swordsman", null, 0.75, 0.25, 0.0, 1.0, StatCode.Str, StatCode.Dex, true ),
		new(41, "Mace Fighting", 9.0, 1.0, 0.0, "Armsman", null, 0.9, 0.1, 0.0, 1.0, StatCode.Str, StatCode.Dex, true ),
		new(42, "Fencing", 4.5, 5.5, 0.0, "Fencer", null, 0.45, 0.55, 0.0, 1.0, StatCode.Dex, StatCode.Str, true ),
		new(43, "Wrestling", 9.0, 1.0, 0.0, "Wrestler", null, 0.9, 0.1, 0.0, 1.0, StatCode.Str, StatCode.Dex, true ),
		new(44, "Lumberjacking", 20.0, 0.0, 0.0, "Lumberjack", null, 2.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Dex),
		new(45, "Mining", 20.0, 0.0, 0.0, "Miner", null, 2.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Dex),
		new(46, "Meditation", 0.0, 0.0, 0.0, "Stoic", null, 0.0, 0.0, 0.0, 1.0, StatCode.Int, StatCode.Str),
		new(47, "Stealth", 0.0, 0.0, 0.0, "Rogue", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(48, "Remove Trap", 0.0, 0.0, 0.0, "Trap Specialist", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(49, "Necromancy", 0.0, 0.0, 0.0, "Necromancer", null, 0.0, 0.0, 0.0, 1.0, StatCode.Int, StatCode.Str, true ),
		new(50, "Focus", 0.0, 0.0, 0.0, "Driven", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Int),
		new(51, "Chivalry", 0.0, 0.0, 0.0, "Paladin", null, 0.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Int, true ),
		new(52, "Bushido", 0.0, 0.0, 0.0, "Samurai", null, 0.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Int, true ),
		new(53, "Ninjitsu", 0.0, 0.0, 0.0, "Ninja", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Int, true ),
		new(54, "Spellweaving", 0.0, 0.0, 0.0, "Arcanist", null, 0.0, 0.0, 0.0, 1.0, StatCode.Int, StatCode.Str, true),
		new(55, "Mysticism", 0.0, 0.0, 0.0, "Mystic", null, 0.0, 0.0, 0.0, 1.0, StatCode.Str, StatCode.Int, true ),
		new(56, "Imbuing", 0.0, 0.0, 0.0, "Artificer", null, 0.0, 0.0, 0.0, 1.0, StatCode.Int, StatCode.Str),
		new(57, "Throwing", 0.0, 0.0, 0.0, "Bladeweaver", null, 0.0, 0.0, 0.0, 1.0, StatCode.Dex, StatCode.Str, true ),
	};
}

[PropertyObject]
public class Skills : IEnumerable<Skill>
{
	private readonly Skill[] _skills;
	private Skill _highest;

	#region Skill Getters & Setters
	[CommandProperty(AccessLevel.Counselor)]
	public Skill Alchemy { get => this[SkillName.Alchemy]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Anatomy { get => this[SkillName.Anatomy]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill AnimalLore { get => this[SkillName.AnimalLore]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill ItemID { get => this[SkillName.ItemID]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill ArmsLore { get => this[SkillName.ArmsLore]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Parry { get => this[SkillName.Parry]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Begging { get => this[SkillName.Begging]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Blacksmith { get => this[SkillName.Blacksmith]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Fletching { get => this[SkillName.Fletching]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Peacemaking { get => this[SkillName.Peacemaking]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Camping { get => this[SkillName.Camping]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Carpentry { get => this[SkillName.Carpentry]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Cartography { get => this[SkillName.Cartography]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Cooking { get => this[SkillName.Cooking]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill DetectHidden { get => this[SkillName.DetectHidden]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Discordance { get => this[SkillName.Discordance]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill EvalInt { get => this[SkillName.EvalInt]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Healing { get => this[SkillName.Healing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Fishing { get => this[SkillName.Fishing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Forensics { get => this[SkillName.Forensics]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Herding { get => this[SkillName.Herding]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Hiding { get => this[SkillName.Hiding]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Provocation { get => this[SkillName.Provocation]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Inscribe { get => this[SkillName.Inscribe]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Lockpicking { get => this[SkillName.Lockpicking]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Magery { get => this[SkillName.Magery]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill MagicResist { get => this[SkillName.MagicResist]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Tactics { get => this[SkillName.Tactics]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Snooping { get => this[SkillName.Snooping]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Musicianship { get => this[SkillName.Musicianship]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Poisoning { get => this[SkillName.Poisoning]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Archery { get => this[SkillName.Archery]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill SpiritSpeak { get => this[SkillName.SpiritSpeak]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Stealing { get => this[SkillName.Stealing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Tailoring { get => this[SkillName.Tailoring]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill AnimalTaming { get => this[SkillName.AnimalTaming]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill TasteID { get => this[SkillName.TasteID]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Tinkering { get => this[SkillName.Tinkering]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Tracking { get => this[SkillName.Tracking]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Veterinary { get => this[SkillName.Veterinary]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Swords { get => this[SkillName.Swords]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Macing { get => this[SkillName.Macing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Fencing { get => this[SkillName.Fencing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Wrestling { get => this[SkillName.Wrestling]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Lumberjacking { get => this[SkillName.Lumberjacking]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Mining { get => this[SkillName.Mining]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Meditation { get => this[SkillName.Meditation]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Stealth { get => this[SkillName.Stealth]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill RemoveTrap { get => this[SkillName.RemoveTrap]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Necromancy { get => this[SkillName.Necromancy]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Focus { get => this[SkillName.Focus]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Chivalry { get => this[SkillName.Chivalry]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Bushido { get => this[SkillName.Bushido]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Ninjitsu { get => this[SkillName.Ninjitsu]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Spellweaving { get => this[SkillName.Spellweaving]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Mysticism { get => this[SkillName.Mysticism]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Imbuing { get => this[SkillName.Imbuing]; set { } }

	[CommandProperty(AccessLevel.Counselor)]
	public Skill Throwing { get => this[SkillName.Throwing]; set { } }
	#endregion

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Cap { get; set; }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public SkillName CurrentMastery
	{
		get;
		set;
	}

	public int Total { get; set; }
	public Mobile Owner { get; }
	public int Length => _skills.Length;

	public Skill this[SkillName name] => this[(int)name];

	public Skill this[int skillId]
	{
		get
		{
			if (skillId < 0 || skillId >= _skills.Length)
			{
				return null;
			}

			Skill sk = _skills[skillId];

			if (sk == null)
			{
				_skills[skillId] = sk = new Skill(this, SkillInfo.Table[skillId], 0, 1000, SkillLock.Up);
			}

			return sk;
		}
	}

	public override string ToString()
	{
		return "...";
	}

	public static bool UseSkill(Mobile from, SkillName name)
	{
		return UseSkill(from, (int)name);
	}

	public static bool UseSkill(Mobile from, int skillId)
	{
		if (!from.CheckAlive())
		{
			return false;
		}
		else if (!from.Region.OnSkillUse(from, skillId))
		{
			return false;
		}
		else if (!from.AllowSkillUse((SkillName)skillId))
		{
			return false;
		}

		if (skillId >= 0 && skillId < SkillInfo.Table.Length)
		{
			SkillInfo info = SkillInfo.Table[skillId];

			if (info.Callback != null)
			{
				if (Core.TickCount - from.NextSkillTime >= 0 && (info.UseWhileCasting || from.Spell == null))
				{
					from.DisruptiveAction();

					from.NextSkillTime = Core.TickCount + (int)info.Callback(from).TotalMilliseconds;

					return true;
				}
				else
				{
					from.SendSkillMessage();
				}
			}
			else
			{
				from.SendLocalizedMessage(500014); // That skill cannot be used directly.
			}
		}

		return false;
	}

	public Skill Highest
	{
		get
		{
			if (_highest == null)
			{
				Skill highest = null;
				int value = int.MinValue;

				for (int i = 0; i < _skills.Length; ++i)
				{
					Skill sk = _skills[i];

					if (sk != null && sk.BaseFixedPoint > value)
					{
						value = sk.BaseFixedPoint;
						highest = sk;
					}
				}

				if (highest == null && _skills.Length > 0)
				{
					highest = this[0];
				}

				_highest = highest;
			}

			return _highest;
		}
	}

	public void Serialize(GenericWriter writer)
	{
		Total = 0;

		writer.Write(4); // version
		writer.Write((int)CurrentMastery);

		writer.Write(Cap);
		writer.Write(_skills.Length);

		for (int i = 0; i < _skills.Length; ++i)
		{
			Skill sk = _skills[i];

			if (sk == null)
			{
				writer.Write((byte)0xFF);
			}
			else
			{
				sk.Serialize(writer);
				Total += sk.BaseFixedPoint;
			}
		}
	}

	public Skills(Mobile owner)
	{
		Owner = owner;
		Cap = Settings.Configuration.Get<int>("Gameplay", "SkillTotalCap");

		var info = SkillInfo.Table;

		_skills = new Skill[info.Length];

		//for ( int i = 0; i < info.Length; ++i )
		//	m_Skills[i] = new Skill( this, info[i], 0, 1000, SkillLock.Up );
	}

	public Skills(Mobile owner, GenericReader reader)
	{
		Owner = owner;

		int version = reader.ReadInt();

		switch (version)
		{
			case 4:
				CurrentMastery = (SkillName)reader.ReadInt();
				goto case 3;
			case 3:
			case 2:
			{
				Cap = reader.ReadInt();

				goto case 1;
			}
			case 1:
			{
				if (version < 2)
				{
					Cap = 7000;
				}

				if (version < 3)
				{
					/*m_Total =*/
					reader.ReadInt();
				}

				var info = SkillInfo.Table;

				_skills = new Skill[info.Length];

				int count = reader.ReadInt();

				for (int i = 0; i < count; ++i)
				{
					if (i < info.Length)
					{
						Skill sk = new Skill(this, info[i], reader);

						if (sk.BaseFixedPoint != 0 || sk.CapFixedPoint != 1000 || sk.Lock != SkillLock.Up || sk.VolumeLearned != 0)
						{
							_skills[i] = sk;
							Total += sk.BaseFixedPoint;
						}
					}
					else
					{
						new Skill(this, null, reader);
					}
				}

				//for ( int i = count; i < info.Length; ++i )
				//	m_Skills[i] = new Skill( this, info[i], 0, 1000, SkillLock.Up );

				break;
			}
			case 0:
			{
				reader.ReadInt();

				goto case 1;
			}
		}
	}

	public void OnSkillChange(Skill skill)
	{
		if (skill == _highest) // could be downgrading the skill, force a recalc
		{
			_highest = null;
		}
		else if (_highest != null && skill.BaseFixedPoint > _highest.BaseFixedPoint)
		{
			_highest = skill;
		}

		Owner.OnSkillInvalidated(skill);

		NetState ns = Owner.NetState;

		if (ns != null)
		{
			ns.Send(new SkillChange(skill));

			Owner.Delta(MobileDelta.Skills);
			Owner.ProcessDelta();
		}
	}

	public IEnumerator<Skill> GetEnumerator()
	{
		return _skills.Where(s => s != null).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _skills.Where(s => s != null).GetEnumerator();
	}
}
