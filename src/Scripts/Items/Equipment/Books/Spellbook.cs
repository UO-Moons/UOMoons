using Server.Commands;
using Server.Engines.Craft;
using Server.Network;
using Server.Spells;
using Server.Targeting;
using System;
using System.Collections.Generic;
using Server.Multis;

namespace Server.Items;

public enum SpellbookType
{
	Invalid = -1,
	Regular,
	Necromancer,
	Paladin,
	Ninja,
	Samurai,
	Arcanist,
	Mystic,
}

public class Spellbook : BaseEquipment, ISpellbook, ICraftable, ISlayer, IWearableDurability
{
	private static readonly Dictionary<Mobile, List<Spellbook>> m_Table = new();

	private static readonly int[] m_LegendPropertyCounts = {
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 21/52 : 40%
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 1 property   : 15/52 : 29%
		2, 2, 2, 2, 2, 2, 2, 2, 2, 2, // 2 properties : 10/52 : 19%
		3, 3, 3, 3, 3, 3 // 3 properties :  6/52 : 12%
	};

	private static readonly int[] m_ElderPropertyCounts = {
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 15/34 : 44%
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // 1 property   : 10/34 : 29%
		2, 2, 2, 2, 2, 2, // 2 properties :  6/34 : 18%
		3, 3, 3 // 3 properties :  3/34 :  9%
	};

	private static readonly int[] m_GrandPropertyCounts = {
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // 0 properties : 10/20 : 50%
		1, 1, 1, 1, 1, 1, // 1 property   :  6/20 : 30%
		2, 2, 2, // 2 properties :  3/20 : 15%
		3 // 3 properties :  1/20 :  5%
	};

	private static readonly int[] m_MasterPropertyCounts = {
		0, 0, 0, 0, 0, 0, // 0 properties : 6/10 : 60%
		1, 1, 1, // 1 property   : 3/10 : 30%
		2 // 2 properties : 1/10 : 10%
	};

	private static readonly int[] m_AdeptPropertyCounts = {
		0, 0, 0, // 0 properties : 3/4 : 75%
		1 // 1 property   : 1/4 : 25%
	};

	private ulong m_Content;
	private SlayerName m_Slayer;
	private SlayerName m_Slayer2;
	public override bool DisplayWeight => false;
	public virtual SpellbookType SpellbookType => SpellbookType.Regular;
	public virtual int BookOffset => 0;
	public virtual int BookCount => 64;
	private int m_MaxHitPoints;
	private int m_HitPoints;
	public override bool CanFortify => false;

	public virtual int InitMinHits => 0;
	public virtual int InitMaxHits => 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public AosSkillBonuses SkillBonuses { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public NegativeAttributes NegativeAttributes { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SpellCount { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public ulong Content
	{
		get => m_Content;
		set
		{
			if (m_Content != value)
			{
				m_Content = value;

				SpellCount = 0;

				while (value > 0)
				{
					SpellCount += (int)(value & 0x1);
					value >>= 1;
				}

				InvalidateProperties();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public SlayerName Slayer
	{
		get => m_Slayer;
		set
		{
			m_Slayer = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public SlayerName Slayer2
	{
		get => m_Slayer2;
		set
		{
			m_Slayer2 = value;
			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitPoints
	{
		get => m_HitPoints;
		set
		{
			if (m_HitPoints == value)
			{
				return;
			}

			if (value > m_MaxHitPoints)
			{
				value = m_MaxHitPoints;
			}

			m_HitPoints = value;

			InvalidateProperties();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxHitPoints
	{
		get => m_MaxHitPoints;
		set
		{
			m_MaxHitPoints = value;
			InvalidateProperties();
		}
	}

	[Constructable]
	public Spellbook()
		: this(0)
	{ }

	public Spellbook(ulong content, int itemId = 0xEFA)
		: base(itemId)
	{
		SkillBonuses = new AosSkillBonuses(this);
		NegativeAttributes = new NegativeAttributes(this);

		Weight = Core.AOS ? 0.0 : 3.0;
		Layer = Layer.OneHanded;
		LootType = LootType.Blessed;

		Content = content;
	}

	public Spellbook(Serial serial)
		: base(serial)
	{ }

	public virtual void ScaleDurability()
	{
	}

	public virtual void UnscaleDurability()
	{
	}

	public virtual int OnHit(BaseWeapon weap, int damage)
	{
		if (m_MaxHitPoints == 0)
			return damage;

		int chance = NegativeAttributes.Antique > 0 ? 50 : 25;

		if (chance <= Utility.Random(100))
			return damage;

		if (m_HitPoints >= 1)
		{
			HitPoints--;
		}
		else if (m_MaxHitPoints > 0)
		{
			MaxHitPoints--;

			if (Parent is Mobile mobile)
				mobile.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.

			if (m_MaxHitPoints == 0)
			{
				Delete();
			}
		}

		return damage;
	}

	public static void Initialize()
	{
		EventSink.OnOpenSpellbookRequest += EventSink_OpenSpellbookRequest;
		EventSink.OnCastSpellRequest += EventSink_CastSpellRequest;
		EventSink.TargetedSpell += Targeted_Spell;
		CommandSystem.Register("AllSpells", AccessLevel.GameMaster, AllSpells_OnCommand);
	}
	#region Enhanced Client
	private static void Targeted_Spell(TargetedSpellEventArgs e)
	{
		try
		{
			Mobile from = e.Mobile;

			if (!DesignContext.Check(from))
			{
				return; // They are customizing
			}

			int spellId = e.SpellID;

			Spellbook book = Find(from, spellId);

			if (book != null && book.HasSpell(spellId))
			{
				SpecialMove move = SpellRegistry.GetSpecialMove(spellId);

				if (move != null)
				{
					SpecialMove.SetCurrentMove(from, move);
				}
				/*
				else if (e.Target != null)
				{
					Mobile to = World.FindMobile(e.Target.Serial);
					Item toI = World.FindItem(e.Target.Serial);
					Spell spell = SpellRegistry.NewSpell(spellID, from, null);

					 (spell != null && !Spells.SkillMasteries.MasteryInfo.IsPassiveMastery(spellID))
					{
						if (to != null)
						{
							spell.InstantTarget = to;
						}
						else if (toI != null)
						{
							spell.InstantTarget = toI as IDamageableItem;
						}

						spell.Cast();
					}
				}*/
			}
			else
			{
				from.SendLocalizedMessage(500015); // You do not have that spell!
			}
		}
		catch
		{
		}
	}
	#endregion

	[Usage("AllSpells")]
	[Description("Completely fills a targeted spellbook with scrolls.")]
	private static void AllSpells_OnCommand(CommandEventArgs e)
	{
		e.Mobile.BeginTarget(-1, false, TargetFlags.None, AllSpells_OnTarget);
		e.Mobile.SendMessage("Target the spellbook to fill.");
	}

	private static void AllSpells_OnTarget(Mobile from, object obj)
	{
		if (obj is Spellbook book)
		{
			if (book.BookCount == 64)
				book.Content = ulong.MaxValue;
			else
				book.Content = (1ul << book.BookCount) - 1;

			from.SendMessage("The spellbook has been filled.");

			CommandLogging.WriteLine(from, "{0} {1} filling spellbook {2}", from.AccessLevel, CommandLogging.Format(from), CommandLogging.Format(book));
		}
		else
		{
			from.BeginTarget(-1, false, TargetFlags.None, AllSpells_OnTarget);
			from.SendMessage("That is not a spellbook. Try again.");
		}
	}

	private static void EventSink_OpenSpellbookRequest(Mobile from, int type)
	{
		if (!DesignContext.Check(from))
			return; // They are customizing

		SpellbookType bookType = type switch
		{
			2 => SpellbookType.Necromancer,
			3 => SpellbookType.Paladin,
			4 => SpellbookType.Ninja,
			5 => SpellbookType.Samurai,
			6 => SpellbookType.Arcanist,
			7 => SpellbookType.Mystic,
			_ => SpellbookType.Regular,
		};

		Spellbook book = Find(from, -1, bookType);

		book?.DisplayTo(from);
	}

	private static void EventSink_CastSpellRequest(Mobile from, int spellId, ISpellbook spellbook)
	{
		if (!Multis.DesignContext.Check(from))
			return; // They are customizing

		if (spellbook is not Spellbook book || !book.HasSpell(spellId))
			book = Find(from, spellId);

		if (book != null && book.HasSpell(spellId))
		{
			SpecialMove move = SpellRegistry.GetSpecialMove(spellId);

			if (move != null)
			{
				SpecialMove.SetCurrentMove(from, move);
			}
			else
			{
				Spell spell = SpellRegistry.NewSpell(spellId, from, null);

				if (spell != null)
					spell.Cast();
				else
					from.SendLocalizedMessage(502345); // This spell has been temporarily disabled.
			}
		}
		else
		{
			from.SendLocalizedMessage(500015); // You do not have that spell!
		}
	}

	public static SpellbookType GetTypeForSpell(int spellId)
	{
		return spellId switch
		{
			>= 0 and < 64 => SpellbookType.Regular,
			>= 100 and < 117 => SpellbookType.Necromancer,
			>= 200 and < 210 => SpellbookType.Paladin,
			>= 400 and < 406 => SpellbookType.Samurai,
			>= 500 and < 508 => SpellbookType.Ninja,
			>= 600 and < 617 => SpellbookType.Arcanist,
			>= 677 and < 693 => SpellbookType.Mystic,
			_ => SpellbookType.Invalid
		};
	}

	public static Spellbook FindRegular(Mobile from)
	{
		return Find(from, -1, SpellbookType.Regular);
	}

	public static Spellbook FindNecromancer(Mobile from)
	{
		return Find(from, -1, SpellbookType.Necromancer);
	}

	public static Spellbook FindPaladin(Mobile from)
	{
		return Find(from, -1, SpellbookType.Paladin);
	}

	public static Spellbook FindSamurai(Mobile from)
	{
		return Find(from, -1, SpellbookType.Samurai);
	}

	public static Spellbook FindNinja(Mobile from)
	{
		return Find(from, -1, SpellbookType.Ninja);
	}

	public static Spellbook FindArcanist(Mobile from)
	{
		return Find(from, -1, SpellbookType.Arcanist);
	}

	public static Spellbook FindMystic(Mobile from)
	{
		return Find(from, -1, SpellbookType.Mystic);
	}

	public static Spellbook Find(Mobile from, int spellId)
	{
		return Find(from, spellId, GetTypeForSpell(spellId));
	}

	public static Spellbook Find(Mobile from, int spellId, SpellbookType type)
	{
		if (from == null)
			return null;

		if (from.Deleted)
		{
			m_Table.Remove(from);
			return null;
		}

		m_Table.TryGetValue(from, out List<Spellbook> list);

		bool searchAgain = false;

		if (list == null)
			m_Table[from] = list = FindAllSpellbooks(from);
		else
			searchAgain = true;

		Spellbook book = FindSpellbookInList(list, from, spellId, type);

		if (book != null || !searchAgain)
			return book;

		m_Table[from] = list = FindAllSpellbooks(from);

		book = FindSpellbookInList(list, from, spellId, type);

		return book;
	}

	public static Spellbook FindSpellbookInList(List<Spellbook> list, Mobile from, int spellId, SpellbookType type)
	{
		Container pack = from.Backpack;

		for (int i = list.Count - 1; i >= 0; --i)
		{
			if (i >= list.Count)
				continue;

			Spellbook book = list[i];

			if (!book.Deleted && (book.Parent == from || (pack != null && book.Parent == pack)) && ValidateSpellbook(book, spellId, type))
				return book;

			list.RemoveAt(i);
		}

		return null;
	}

	public static List<Spellbook> FindAllSpellbooks(Mobile from)
	{
		List<Spellbook> list = new();

		Item item = from.FindItemOnLayer(Layer.OneHanded);

		if (item is Spellbook spellbook)
			list.Add(spellbook);

		Container pack = from.Backpack;

		if (pack == null)
			return list;

		for (int i = 0; i < pack.Items.Count; ++i)
		{
			item = pack.Items[i];

			if (item is Spellbook spb)
				list.Add(spb);
		}

		return list;
	}

	public static Spellbook FindEquippedSpellbook(Mobile from)
	{
		return from.FindItemOnLayer(Layer.OneHanded) as Spellbook;
	}

	public static bool ValidateSpellbook(Spellbook book, int spellId, SpellbookType type)
	{
		return book.SpellbookType == type && (spellId == -1 || book.HasSpell(spellId));
	}

	public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
	{
		return Ethics.Ethic.CheckTrade(from, to, newOwner, this) && base.AllowSecureTrade(from, to, newOwner, accepted);
	}

	public override bool CanEquip(Mobile from)
	{
		if (!Ethics.Ethic.CheckEquip(from, this))
		{
			return false;
		}

		if (Owner != null && Owner != from)
		{
			from.SendLocalizedMessage(501023); // You must be the owner to use this item.
			return false;
		}

		return from.CanBeginAction(typeof(BaseWeapon)) && base.CanEquip(from);
	}

	public override bool AllowEquipedCast(Mobile from)
	{
		return true;
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (dropped is not SpellScroll scroll1 || dropped.Amount != 1)
			return false;

		SpellbookType type = GetTypeForSpell(scroll1.SpellID);

		if (type != SpellbookType)
		{
			return false;
		}

		if (HasSpell(scroll1.SpellID))
		{
			from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
			return false;
		}

		int val = scroll1.SpellID - BookOffset;

		if (val < 0 || val >= BookCount)
			return false;

		from.Send(new PlaySound(0x249, GetWorldLocation()));
		m_Content |= (ulong)1 << val;
		++SpellCount;

		if (dropped.Amount > 1)
		{
			dropped.Amount--;
			return base.OnDragDrop(from, dropped);
		}

		InvalidateProperties();
		scroll1.Delete();
		return true;

	}

	public override void OnAfterDuped(Item newItem)
	{
		base.OnAfterDuped(newItem);

		if (newItem is not Spellbook book)
			return;
		book.SkillBonuses = new AosSkillBonuses(newItem, SkillBonuses);
		book.NegativeAttributes = new NegativeAttributes(newItem, NegativeAttributes);
	}

	public override void OnAdded(IEntity parent)
	{
		if (!Core.AOS || parent is not Mobile from)
			return;

		SkillBonuses.AddTo(from);

		int strBonus = Attributes.BonusStr;
		int dexBonus = Attributes.BonusDex;
		int intBonus = Attributes.BonusInt;

		if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
		{
			string modName = Serial.ToString();

			if (strBonus != 0)
				from.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

			if (dexBonus != 0)
				from.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

			if (intBonus != 0)
				from.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
		}

		from.CheckStatTimers();
	}

	public override void OnRemoved(IEntity parent)
	{
		if (!Core.AOS || parent is not Mobile from)
			return;

		SkillBonuses.Remove();
		/*
			if (HasSocket<Caddellite>())
			{
				Caddellite.UpdateBuff(from);
			}*/

		RemoveStatBonuses(from);

		from.CheckStatTimers();
	}

	public bool HasSpell(int spellId)
	{
		spellId -= BookOffset;

		return spellId >= 0 && spellId < BookCount && (m_Content & ((ulong)1 << spellId)) != 0;
	}

	public void DisplayTo(Mobile to)
	{
		// The client must know about the spellbook or it will crash!

		NetState ns = to.NetState;

		if (ns == null)
			return;

		switch (Parent)
		{
			case null:
				SendInfoTo(ns, to.ViewOpl);
				break;
			case Item:
				ContainerContentUpdate.Send(ns, this);
				break;
			case Mobile:
				to.Send(new EquipUpdate(this));
				break;
		}

		DisplaySpellbook.Send(ns, this);
		SpellbookContent.Send(ns, this, BookOffset + 1, m_Content);
	}

	public override bool DisplayLootType => Core.AOS;

	public override void AddNameProperties(ObjectPropertyList list)
	{
		base.AddNameProperties(list);

		if (Bookquality == BookQuality.Exceptional)
		{
			list.Add(1063341); // exceptional
		}

		if (EngravedText != null)
		{
			list.Add(1072305, Utility.FixHtml(EngravedText)); // Engraved: ~1_INSCRIPTION~
		}

		if (Crafter != null)
		{
			list.Add(1050043, Crafter.TitleName); // crafted by ~1_NAME~
		}

		//if (IsVvVItem)
		//{
		//	list.Add(1154937); // VvV Item
		//}

		if (OwnerName != null)
		{
			list.Add(1153213, OwnerName);
		}

		if (NegativeAttributes != null)
		{
			NegativeAttributes.GetProperties(list, this);
		}

		SkillBonuses.GetProperties(list);

		if (m_Slayer != SlayerName.None)
		{
			SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer);
			if (entry != null)
			{
				list.Add(entry.Title);
			}
		}

		if (m_Slayer2 != SlayerName.None)
		{
			SlayerEntry entry = SlayerGroup.GetEntryByName(m_Slayer2);
			if (entry != null)
			{
				list.Add(entry.Title);
			}
		}

		//if (HasSocket<Caddellite>())
		//{
		//	list.Add(1158662); // Caddellite Infused
		//}

		int prop;

		if (Attributes.SpellChanneling != 0)
		{
			list.Add(1060482); // spell channeling
		}

		if (Attributes.NightSight != 0)
		{
			list.Add(1060441); // night sight
		}

		if ((prop = Attributes.BonusStr) != 0)
		{
			list.Add(1060485, prop.ToString()); // strength bonus ~1_val~
		}

		if ((prop = Attributes.BonusDex) != 0)
		{
			list.Add(1060409, prop.ToString()); // dexterity bonus ~1_val~
		}

		if ((prop = Attributes.BonusInt) != 0)
		{
			list.Add(1060432, prop.ToString()); // intelligence bonus ~1_val~
		}

		if ((prop = Attributes.BonusHits) != 0)
		{
			list.Add(1060431, prop.ToString()); // hit point increase ~1_val~
		}

		if ((prop = Attributes.BonusStam) != 0)
		{
			list.Add(1060484, prop.ToString()); // stamina increase ~1_val~
		}

		if ((prop = Attributes.BonusMana) != 0)
		{
			list.Add(1060439, prop.ToString()); // mana increase ~1_val~
		}

		if ((prop = Attributes.RegenHits) != 0)
		{
			list.Add(1060444, prop.ToString()); // hit point regeneration ~1_val~
		}

		if ((prop = Attributes.RegenStam) != 0)
		{
			list.Add(1060443, prop.ToString()); // stamina regeneration ~1_val~
		}

		if ((prop = Attributes.RegenMana) != 0)
		{
			list.Add(1060440, prop.ToString()); // mana regeneration ~1_val~
		}

		if ((prop = Attributes.Luck) != 0)
		{
			list.Add(1060436, prop.ToString()); // luck ~1_val~
		}

		if ((prop = Attributes.EnhancePotions) != 0)
		{
			list.Add(1060411, prop.ToString()); // enhance potions ~1_val~%
		}

		if ((prop = Attributes.ReflectPhysical) != 0)
		{
			list.Add(1060442, prop.ToString()); // reflect physical damage ~1_val~%
		}

		if ((prop = Attributes.AttackChance) != 0)
		{
			list.Add(1060415, prop.ToString()); // hit chance increase ~1_val~%
		}

		if ((prop = Attributes.WeaponSpeed) != 0)
		{
			list.Add(1060486, prop.ToString()); // swing speed increase ~1_val~%
		}

		if ((prop = Attributes.WeaponDamage) != 0)
		{
			list.Add(1060401, prop.ToString()); // damage increase ~1_val~%
		}

		if ((prop = Attributes.DefendChance) != 0)
		{
			list.Add(1060408, prop.ToString()); // defense chance increase ~1_val~%
		}

		if ((prop = Attributes.CastRecovery) != 0)
		{
			list.Add(1060412, prop.ToString()); // faster cast recovery ~1_val~
		}

		if ((prop = Attributes.CastSpeed) != 0)
		{
			list.Add(1060413, prop.ToString()); // faster casting ~1_val~
		}

		if ((prop = Attributes.SpellDamage) != 0)
		{
			list.Add(1060483, prop.ToString()); // spell damage increase ~1_val~%
		}

		if ((prop = Attributes.LowerManaCost) != 0)
		{
			list.Add(1060433, prop.ToString()); // lower mana cost ~1_val~%
		}

		if ((prop = Attributes.LowerRegCost) != 0)
		{
			list.Add(1060434, prop.ToString()); // lower reagent cost ~1_val~%
		}

		if ((prop = Attributes.IncreasedKarmaLoss) != 0)
		{
			list.Add(1075210, prop.ToString()); // Increased Karma Loss ~1val~%
		}

		AddProperty(list);

		list.Add(1042886, SpellCount.ToString()); // ~1_NUMBERS_OF_SPELLS~ Spells

		if (m_MaxHitPoints > 0)
			list.Add(1060639, "{0}\t{1}", m_HitPoints, m_MaxHitPoints); // durability ~1_val~ / ~2_val~
	}

	public virtual void AddProperty(ObjectPropertyList list)
	{
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		if (Crafter != null)
			LabelTo(from, 1050043, Crafter.Name); // crafted by ~1_NAME~

		LabelTo(from, 1042886, SpellCount.ToString());
	}

	public override void OnDoubleClick(Mobile from)
	{
		Container pack = from.Backpack;

		if (Parent == from || (pack != null && Parent == pack))
			DisplayTo(from);
		else
			from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		NegativeAttributes.Serialize(writer);

		writer.Write(m_HitPoints);
		writer.Write(m_MaxHitPoints);
		writer.Write((int)m_Slayer);
		writer.Write((int)m_Slayer2);
		SkillBonuses.Serialize(writer);
		writer.Write(m_Content);
		writer.Write(SpellCount);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				NegativeAttributes = new NegativeAttributes(this, reader);
				m_MaxHitPoints = reader.ReadInt();
				m_HitPoints = reader.ReadInt();
				m_Slayer = (SlayerName)reader.ReadInt();
				m_Slayer2 = (SlayerName)reader.ReadInt();
				SkillBonuses = new AosSkillBonuses(this, reader);
				m_Content = reader.ReadULong();
				SpellCount = reader.ReadInt();
				break;
			}
		}

		SkillBonuses ??= new AosSkillBonuses(this);

		NegativeAttributes ??= new NegativeAttributes(this);

		if (Core.AOS && Parent is Mobile parentMobile)
			SkillBonuses.AddTo(parentMobile);

		int strBonus = Attributes.BonusStr;
		int dexBonus = Attributes.BonusDex;
		int intBonus = Attributes.BonusInt;

		if (Parent is Mobile m && (strBonus != 0 || dexBonus != 0 || intBonus != 0))
		{
			string modName = Serial.ToString();

			if (strBonus != 0)
				m.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

			if (dexBonus != 0)
				m.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

			if (intBonus != 0)
				m.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
		}

		if (Parent is Mobile mob)
			mob.CheckStatTimers();
	}

	public virtual int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
	{
		int magery = from.Skills.Magery.BaseFixedPoint;

		if (magery >= 800)
		{
			int[] propertyCounts;
			int minIntensity;
			int maxIntensity;

			switch (magery)
			{
				case >= 1000:
					propertyCounts = magery switch
					{
						>= 1200 => m_LegendPropertyCounts,
						>= 1100 => m_ElderPropertyCounts,
						_ => m_GrandPropertyCounts
					};

					minIntensity = 55;
					maxIntensity = 75;
					break;
				case >= 900:
					propertyCounts = m_MasterPropertyCounts;
					minIntensity = 25;
					maxIntensity = 45;
					break;
				default:
					propertyCounts = m_AdeptPropertyCounts;
					minIntensity = 0;
					maxIntensity = 15;
					break;
			}

			int propertyCount = propertyCounts[Utility.Random(propertyCounts.Length)];

			if (from.FindItemOnLayer(Layer.Talisman) is GuaranteedSpellbookImprovementTalisman { Charges: > 0 } talisman)
			{
				propertyCount++;
				talisman.Charges--;

				from.SendLocalizedMessage(1157210); // Your talisman magically improves your spellbook.

				if (talisman.Charges <= 0)
				{
					from.SendLocalizedMessage(1157211); // Your talisman has been destroyed.
					talisman.Delete();
				}
			}

			BaseRunicTool.ApplyAttributesTo(this, true, 0, propertyCount, minIntensity, maxIntensity);
		}

		if (makersMark)
			Crafter = from;

		Bookquality = (BookQuality)(quality - 1);

		return quality;
	}
}
