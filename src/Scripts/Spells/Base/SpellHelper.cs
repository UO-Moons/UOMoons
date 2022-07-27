using Server.Engines.PartySystem;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Spells.Ninjitsu;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Server.Engines.ConPVP;
using Server.Spells.Mysticism;

namespace Server.Spells;

public enum TravelCheckType
{
	RecallFrom,
	RecallTo,
	GateFrom,
	GateTo,
	Mark,
	TeleportFrom,
	TeleportTo
}

public class SpellHelper
{

	#region Spell Focus and SDI Calculations
	private static readonly SkillName[] m_Schools =
	{
		SkillName.Magery,
		SkillName.AnimalTaming,
		SkillName.Musicianship,
		SkillName.Mysticism,
		SkillName.Spellweaving,
		SkillName.Chivalry,
		SkillName.Necromancy,
		SkillName.Bushido,
		SkillName.Ninjitsu
	};

	private static readonly SkillName[] m_TolSchools =
	{
		SkillName.Magery,
		SkillName.AnimalTaming,
		SkillName.Musicianship,
		SkillName.Mysticism,
		SkillName.Spellweaving,
		SkillName.Chivalry,
		SkillName.Necromancy,
		SkillName.Bushido,
		SkillName.Ninjitsu,
		SkillName.Parry
	};

	private static bool HasSpellFocus(Mobile m, SkillName focus)
	{
		SkillName[] list = Core.TOL ? m_TolSchools : m_Schools;

		return list.All(skill => skill == focus || !(m.Skills[skill].Value >= 30.0));
	}

	private static int PvPSpellDamageCap(Mobile m, SkillName castskill)
	{
		if (!Core.SA)
		{
			return 15;
		}

		if (HasSpellFocus(m, castskill))
		{
			return 30;
		}

		return Core.TOL ? 20 : 15;
	}

	public static int GetSpellDamageBonus(Mobile caster, IDamageable damageable, SkillName skill, bool playerVsPlayer)
	{
		int sdiBonus = AosAttributes.GetValue(caster, AosAttribute.SpellDamage);

		if (damageable is Mobile target)
		{
			if (RunedSashOfWarding.IsUnderEffects(target, WardingEffect.SpellDamage))
			{
				sdiBonus -= 10;
			}

			sdiBonus -= Block.GetSpellReduction(target);
		}

		// PvP spell damage increase cap of 15% from an item’s magic property, 30% if spell school focused.
		if (Core.SE && playerVsPlayer)
		{
			sdiBonus = Math.Min(sdiBonus, PvPSpellDamageCap(caster, skill));
		}

		return sdiBonus;
	}
	#endregion

	private static readonly TimeSpan AosDamageDelay = TimeSpan.FromSeconds(1.0);
	private static readonly TimeSpan OldDamageDelay = TimeSpan.FromSeconds(0.5);

	public const bool RestrictTravelCombat = true;
	public static bool RestrictRedTravel => false;

	private static TimeSpan GetDamageDelayForSpell(Spell sp)
	{
		if (!sp.DelayedDamage)
			return TimeSpan.Zero;

		return Core.AOS ? AosDamageDelay : OldDamageDelay;
	}

	public static bool CheckMulti(Point3D p, Map map, bool houses = true, int housingrange = 0)
	{
		if (map == null || map == Map.Internal)
			return false;

		Sector sector = map.GetSector(p.X, p.Y);

		for (int i = 0; i < sector.Multis.Count; ++i)
		{
			BaseMulti multi = sector.Multis[i];

			if (multi is BaseHouse bh)
			{
				if ((houses && bh.IsInside(p, 16)) || (housingrange > 0 && bh.InRange(p, housingrange)))
					return true;
			}
			else if (multi.Contains(p))
			{
				return true;
			}
		}

		return false;
	}

	public static void Turn(Mobile from, object to)
	{
		int d = -1;

		if (to is not IPoint3D target)
			return;

		if (target is Item item)
		{
			if (item.RootParent != from)
			{
				d = (int)from.GetDirectionTo(item.GetWorldLocation());
			}
		}
		else if (!Equals(from, target))
		{
			d = (int)from.GetDirectionTo(target);
		}

		if (d <= -1)
			return;

		from.Direction = (Direction)d;
		from.ProcessDelta();
	}

	public static bool CheckCombat(Mobile m)
	{
		if (m.Aggressed.Any(info => info.Defender.Player && DateTime.UtcNow - info.LastCombatTime < BaseMobile.CombatHeatDelay))
		{
			return true;
		}

		return Core.AOS && m.Aggressors.Any(info => info.Attacker.Player && DateTime.UtcNow - info.LastCombatTime < BaseMobile.CombatHeatDelay);
	}

	public static bool AdjustField(ref Point3D p, Map map, int height, bool mobsBlock)
	{
		if (map == null)
		{
			return false;
		}

		for (int offset = 0; offset < 25; ++offset)
		{
			Point3D loc = new(p.X, p.Y, p.Z - offset);

			if (map.CanFit(loc, height, true, mobsBlock))
			{
				p = loc;
				return true;
			}

			loc = new Point3D(p.X, p.Y, p.Z + offset);

			if (!map.CanFit(loc, height, true, mobsBlock))
				continue;

			p = loc;
			return true;
		}

		return false;
	}

	public static bool CheckField(Point3D p, Map map)
	{
		if (map == null)
		{
			return false;
		}

		IPooledEnumerable eable = map.GetItemsInRange(p, 0);

		if ((from Item item in eable select item.GetType()).Any(t => t.IsDefined(typeof(DispellableAttributes), false) || t.IsDefined(typeof(DispellableAttribute), true)))
		{
			eable.Free();
			return false;
		}

		eable.Free();
		return true;
	}

	public static bool CheckWater(Point3D p, Map map)
	{
		var landTile = map.Tiles.GetLandTile(p.X, p.Y);

		if (landTile.Z == p.Z && (landTile.Id is >= 168 and <= 171 || landTile.Id is >= 310 and <= 311))
		{
			return false;
		}

		var tiles = map.Tiles.GetStaticTiles(p.X, p.Y, true);

		return tiles.All(tile => tile.Z != p.Z || tile.Id is < 0x1796 or > 0x17B2);
	}

	public static bool CanRevealCaster(Mobile m)
	{
		return m is BaseCreature {Controlled: false};
	}

	public static void GetSurfaceTop(ref IPoint3D p)
	{
		switch (p)
		{
			case Item item:
				p = item.GetSurfaceTop();
				break;
			case StaticTarget t:
			{
				int z = t.Z;

				if ((t.Flags & TileFlag.Surface) == 0)
					z -= TileData.ItemTable[t.ItemID & TileData.MaxItemValue].CalcHeight;

				p = new Point3D(t.X, t.Y, z);
				break;
			}
		}
	}

	private static void RemoveStatOffsetCallback(object state)
	{
		if (state is not Mobile mobile)
		{
			return;
		}
		// This call has the side-effect of updating all stats
		mobile.CheckStatTimers();
	}

	public static bool AddStatOffset(Mobile m, StatType type, int offset, TimeSpan duration)
	{
		return offset switch
		{
			> 0 => AddStatBonus(m, m, type, offset, duration),
			< 0 => AddStatCurse(m, m, type, -offset, duration),
			_ => true
		};
	}

	public static void AddStatBonus(Mobile caster, Mobile target, bool blockSkill, StatType type)
	{
		AddStatBonus(caster, target, type, GetOffset(caster, target, type, false, blockSkill), GetDuration(caster, target));
	}

	private static bool AddStatBonus(Mobile caster, Mobile target, StatType type, int bonus, TimeSpan duration)
	{
		int offset = bonus;
		string name = $"[Magic] {type} Buff";
		StatMod mod = target.GetStatMod(name);

		if (mod != null)
		{
			offset = Math.Max(mod.Offset, offset);
		}

		target.AddStatMod(new StatMod(type, name, offset, duration));
		Timer.DelayCall(duration, RemoveStatOffsetCallback, target);

		return true;
	}

	public static int GetBuffOffset(Mobile m, StatType type)
	{
		string name = $"[Magic] {type} Buff";

		StatMod mod = m.GetStatMod(name);

		return mod?.Offset ?? 0;
	}

	public static void AddStatCurse(Mobile caster, Mobile target, StatType type, bool blockSkill, int offset)
	{
		AddStatCurse(caster, target, type, offset, TimeSpan.Zero);
	}

	public static void AddStatCurse(Mobile caster, Mobile target, StatType type, bool blockSkill = true)
	{
		AddStatCurse(caster, target, type, GetOffset(caster, target, type, true, blockSkill), TimeSpan.Zero);
	}

	private static bool AddStatCurse(Mobile caster, Mobile target, StatType type, int curse, TimeSpan duration)
	{
		int offset = curse;
		string name = $"[Magic] {type} Curse";

		StatMod mod = target.GetStatMod(name);

		if (mod != null)
		{
			offset = Math.Max(mod.Offset, offset);
		}

		offset *= -1;

		target.AddStatMod(new StatMod(type, name, offset, TimeSpan.Zero));
		return true;
	}

	public static TimeSpan GetDuration(Mobile caster, Mobile target)
	{
		return Core.AOS ? TimeSpan.FromSeconds(caster.Skills.EvalInt.Fixed / 50 + 1) * 6 : TimeSpan.FromSeconds(6 * caster.Skills[SkillName.Magery].Value * 1.2);
	}


	public static int GetCurseOffset(Mobile m, StatType type)
	{
		string name = $"[Magic] {type} Curse";

		StatMod mod = m.GetStatMod(name);

		return mod?.Offset ?? 0;
	}

	public static bool DisableSkillCheck { get; set; }

	public static double GetOffsetScalar(Mobile caster, Mobile target, bool curse)
	{
		double percent;

		if (curse)
			percent = 8 + caster.Skills.EvalInt.Fixed / 100 - target.Skills.MagicResist.Fixed / 100;
		else
			percent = 1 + caster.Skills.EvalInt.Fixed / 100;

		percent *= 0.01;

		if (percent < 0)
			percent = 0;

		return percent;
	}

	public static int GetOffset(Mobile caster, Mobile target, StatType type, bool curse, bool blockSkill)
	{
		if (!Core.AOS)
			return 1 + (int)(caster.Skills[SkillName.Magery].Value * 0.1);

		if (!blockSkill)
		{
			caster.CheckSkill(SkillName.EvalInt, 0.0, 120.0);

			if (curse)
				target.CheckSkill(SkillName.MagicResist, 0.0, 120.0);
		}

		double percent = GetOffsetScalar(caster, target, curse);

		return type switch
		{
			StatType.Str => (int)(target.RawStr * percent),
			StatType.Dex => (int)(target.RawDex * percent),
			StatType.Int => (int)(target.RawInt * percent),
			_ => 1 + (int)(caster.Skills[SkillName.Magery].Value * 0.1)
		};
	}

	private static Guild GetGuildFor(Mobile m)
	{
		Guild g = m.Guild as Guild;

		if (g != null || m is not BaseCreature c)
			return g;

		m = c.ControlMaster;

		if (m != null)
			g = m.Guild as Guild;

		if (g != null)
			return g;

		m = c.SummonMaster;

		if (m != null)
			g = m.Guild as Guild;

		return g;
	}

	public static bool ValidIndirectTarget(Mobile from, Mobile to)
	{
		if (from == to)
			return true;

		if (to.Hidden && to.AccessLevel > from.AccessLevel)
			return false;

		#region Dueling
		PlayerMobile pmFrom = from as PlayerMobile;
		PlayerMobile pmTarg = to as PlayerMobile;

		if (pmFrom == null && from is BaseCreature { Summoned: true } bcFrom)
		{
			pmFrom = bcFrom.SummonMaster as PlayerMobile;
		}

		if (pmTarg == null && to is BaseCreature { Summoned: true } bcTarg)
		{
			pmTarg = bcTarg.SummonMaster as PlayerMobile;
		}

		if (pmFrom != null && pmTarg != null)
		{
			if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null)
				return pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant;
		}
		#endregion

		Guild fromGuild = GetGuildFor(from);
		Guild toGuild = GetGuildFor(to);

		if (fromGuild != null && toGuild != null && (fromGuild == toGuild || fromGuild.IsAlly(toGuild)))
			return false;

		Party p = Party.Get(from);

		if (p != null && p.Contains(to))
			return false;

		if (to is BaseCreature bc)
		{
			if (bc.Controlled || bc.Summoned)
			{
				if (bc.ControlMaster == from || bc.SummonMaster == from)
					return false;

				if (p != null && (p.Contains(bc.ControlMaster) || p.Contains(bc.SummonMaster)))
					return false;
			}
		}

		if (from is BaseCreature c)
		{
			if (c.Controlled || c.Summoned)
			{
				if (c.ControlMaster == to || c.SummonMaster == to)
					return false;

				p = Party.Get(to);

				if (p != null && (p.Contains(c.ControlMaster) || p.Contains(c.SummonMaster)))
					return false;
			}
			else
			{
				if (to.Player || to is BaseCreature creature3 && (creature3.Controlled || creature3.Summoned) && creature3.GetMaster() is PlayerMobile)
				{
					return true;
				}
			}
		}

		// Non-enemy monsters will no longer flag area spells on each other
		if (from is BaseCreature creature1 && to is BaseCreature creature2)
		{
			BaseCreature fromBc = creature1;
			BaseCreature toBc = creature2;

			if (fromBc.GetMaster() is BaseCreature)
			{
				fromBc = fromBc.GetMaster() as BaseCreature;
			}

			if (toBc.GetMaster() is BaseCreature)
			{
				toBc = toBc.GetMaster() as BaseCreature;
			}

			if (toBc != null && toBc.IsEnemy(fromBc))   //Natural Enemies
			{
				return true;
			}

			//All involved are monsters- no damage. If falls through this statement, normal noto rules apply
			if (fromBc != null && toBc is {Controlled: false, Summoned: false} && !fromBc.Controlled && !fromBc.Summoned) //All involved are monsters- no damage
			{
				return false;
			}
		}

		if (to is BaseCreature {Controlled: false, InitialInnocent: true})
			return true;

		int noto = Notoriety.Compute(from, to);

		return noto != Notoriety.Innocent || from.Murderer;
	}

	public static IEnumerable<IDamageable> AcquireIndirectTargets(Mobile caster, IPoint3D p, Map map, int range, bool losCheck = true)
	{
		if (map == null)
		{
			yield break;
		}

		IPooledEnumerable eable = map.GetObjectsInRange(new Point3D(p), range);

		foreach (var id in eable.OfType<IDamageable>())
		{
			if (id == caster || !id.Alive || (losCheck && !caster.InLOS(id)) || !caster.CanBeHarmful(id, false) || id is Mobile mobile && !ValidIndirectTarget(caster, mobile))
			{
				continue;
			}

			yield return id;
		}

		eable.Free();
	}

	public static void Summon(BaseCreature creature, Mobile caster, int sound, TimeSpan duration, bool scaleDuration, bool scaleStats)
	{
		Map map = caster.Map;

		if (map == null)
			return;

		double scale = 1.0 + (caster.Skills[SkillName.Magery].Value - 100.0) / 200.0;

		if (scaleDuration)
			duration = TimeSpan.FromSeconds(duration.TotalSeconds * scale);

		if (scaleStats)
		{
			creature.RawStr = (int)(creature.RawStr * scale);
			creature.Hits = creature.HitsMax;

			creature.RawDex = (int)(creature.RawDex * scale);
			creature.Stam = creature.StamMax;

			creature.RawInt = (int)(creature.RawInt * scale);
			creature.Mana = creature.ManaMax;
		}

		Point3D p = new(caster);

		if (FindValidSpawnLocation(map, ref p, true))
		{
			BaseCreature.Summon(creature, caster, p, sound, duration);
		}
		else
		{
			creature.Delete();
			caster.SendLocalizedMessage(501942); // That location is blocked.
		}
	}

	public static bool FindValidSpawnLocation(Map map, ref Point3D p, bool surroundingsOnly)
	{
		if (map == null)    //sanity
			return false;

		if (!surroundingsOnly)
		{
			if (map.CanSpawnMobile(p))  //p's fine.
			{
				p = new Point3D(p);
				return true;
			}

			int z = map.GetAverageZ(p.X, p.Y);

			if (map.CanSpawnMobile(p.X, p.Y, z))
			{
				p = new Point3D(p.X, p.Y, z);
				return true;
			}
		}

		int offset = Utility.Random(8) * 2;

		for (int i = 0; i < Helpers.Offsets.Length; i += 2)
		{
			int x = p.X + Helpers.Offsets[(offset + i) % Helpers.Offsets.Length];
			int y = p.Y + Helpers.Offsets[(offset + i + 1) % Helpers.Offsets.Length];

			if (map.CanSpawnMobile(x, y, p.Z))
			{
				p = new Point3D(x, y, p.Z);
				return true;
			}

			int z = map.GetAverageZ(x, y);

			if (!map.CanSpawnMobile(x, y, z))
				continue;

			p = new Point3D(x, y, z);
			return true;
		}

		return false;
	}

	public static void Configure()
	{
		Console.Write("Loading TravelRestrictions...");
		Console.WriteLine(LoadTravelRestrictions() ? "done" : "failed");
	}

	private static readonly string TravelDirectory = Path.Combine(Core.BaseDirectory, "Data", "TravelRestrictions.xml");

	public static bool LoadTravelRestrictions()
	{
		if (!File.Exists(TravelDirectory))
		{
			Utility.WriteConsole(ConsoleColor.Red, $"TravelRestrictions: Directory not found:\n > {TravelDirectory}");
			return false;
		}

		XmlDocument x = new();
		x.Load(TravelDirectory);

		try
		{
			XmlElement e = x["TravelRestrictions"];

			if (e == null)
				return false;

			foreach (XmlElement r in e.GetElementsByTagName("Region"))
			{
				if (!r.HasAttribute("Name"))
				{
					Console.WriteLine("Warning: Missing 'Name' attribute in TravelRestrictions.xml");
					continue;
				}

				string name = r.GetAttribute("Name");

				if (m_TravelRestrictions.ContainsKey(name))
				{
					Console.WriteLine("Warning: Duplicate name '{0}' in TravelRestrictions.xml", name);
					continue;
				}

				if (!r.HasAttribute("Delegate"))
				{
					Console.WriteLine("Warning: Missing 'Delegate' attribute in TravelRestrictions.xml");
					continue;
				}

				string d = r.GetAttribute("Delegate");

				MethodInfo m = typeof(SpellHelper).GetMethod(d);
				if (m == null)
				{
					Console.WriteLine("Warning: TravelRestrictions.xml Delegate '{0}' not found in SpellHelper", d);
					continue;
				}

				TravelValidator v = (TravelValidator)Delegate.CreateDelegate(typeof(TravelValidator), m);
				TravelRules t = new()
				{
					Validator = v
				};
				m_TravelRestrictions[name] = t;

				foreach (XmlElement rule in r)
				{
					switch (rule.Name.ToLower())
					{
						case "recallfrom": t.RecallFrom = Utility.ToBoolean(rule.InnerText); break;
						case "recallto": t.RecallTo = Utility.ToBoolean(rule.InnerText); break;
						case "gatefrom": t.GateFrom = Utility.ToBoolean(rule.InnerText); break;
						case "gateto": t.GateTo = Utility.ToBoolean(rule.InnerText); break;
						case "mark": t.Mark = Utility.ToBoolean(rule.InnerText); break;
						case "teleportfrom": t.TeleportFrom = Utility.ToBoolean(rule.InnerText); break;
						case "teleportto": t.TeleportTo = Utility.ToBoolean(rule.InnerText); break;
						default: Console.WriteLine("Warning: Unknown element '{0}' in TravelRestrictions.xml", rule.Name); break;
					}
				}
			}
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return false;
		}

		return true;
	}

	private struct TravelRules
	{
		public TravelValidator Validator;

		public bool RecallFrom, RecallTo, GateFrom, GateTo, Mark, TeleportFrom, TeleportTo;

		public bool Allow(TravelCheckType t) => t switch
		{
			TravelCheckType.RecallFrom => RecallFrom,
			TravelCheckType.RecallTo => RecallTo,
			TravelCheckType.GateFrom => GateFrom,
			TravelCheckType.GateTo => GateTo,
			TravelCheckType.Mark => Mark,
			TravelCheckType.TeleportFrom => TeleportFrom,
			TravelCheckType.TeleportTo => TeleportTo,
			_ => false,
		};
	}

	private static Dictionary<string, TravelRules> m_TravelRestrictions = new();

	private delegate bool TravelValidator(Map map, Point3D loc);

	public static void SendInvalidMessage(Mobile caster, TravelCheckType type)
	{
		switch (type)
		{
			case TravelCheckType.RecallTo or TravelCheckType.GateTo:
				caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
				break;
			case TravelCheckType.TeleportTo:
				caster.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
				break;
			default:
				caster.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
				break;
		}
	}

	public static bool CheckTravel(Mobile caster, TravelCheckType type)
	{
		return CheckTravel(caster, caster.Map, caster.Location, type);
	}

	public static bool CheckTravel(Map map, Point3D loc, TravelCheckType type)
	{
		return CheckTravel(null, map, loc, type);
	}

	private static Mobile _travelCaster;
	private static TravelCheckType _travelType;

	public static bool CheckTravel(Mobile caster, Map map, Point3D loc, TravelCheckType type)
	{
		if (IsInvalid(map, loc)) // null, internal, out of bounds
		{
			if (caster != null)
				SendInvalidMessage(caster, type);

			return false;
		}

		if (caster != null)
		{
			if (caster.IsPlayer())
			{
				// Jail region
				if (caster.Region.IsPartOf<Jail>())
				{
					caster.SendLocalizedMessage(1114345); // You'll need a better jailbreak plan than that!
					return false;
				}

				if (caster.Region is GreenAcres)
				{
					caster.SendLocalizedMessage(502360); // You cannot teleport into that area.
					return false;
				}
			}

			// Always allow monsters to teleport
			if (caster is BaseCreature creature && (type == TravelCheckType.TeleportTo || type == TravelCheckType.TeleportFrom))
			{
				if (!creature.Controlled && !creature.Summoned)
				{
					return true;
				}
			}
		}
		_travelCaster = caster;
		_travelType = type;

		int v = (int)type;
		bool isValid = true;

		if (caster != null)
		{
			if (Region.Find(loc, map) is BaseRegion destination && !destination.CheckTravel(caster, loc, type))
			{
				isValid = false;
			}

			if (isValid && Region.Find(caster.Location, map) is BaseRegion current && !current.CheckTravel(caster, loc, type))
			{
				isValid = false;
			}

			if (_travelCaster is { Region: { } })
			{
				if (_travelCaster.Region.IsPartOf("Blighted Grove") && loc.Z < -10)
				{
					isValid = false;
				}
			}

			if (v <= 4 && (IsNewDungeon(caster.Map, caster.Location) || IsNewDungeon(map, loc)))
			{
				isValid = false;
			}
		}

		foreach (KeyValuePair<string, TravelRules> r in m_TravelRestrictions)
		{
			isValid = r.Value.Allow(type) || !r.Value.Validator(map, loc);

			if (!isValid && caster != null)
			{
				break;
			}
		}

		if (!isValid && caster != null)
			SendInvalidMessage(caster, type);

		return isValid;
	}

	private static bool IsWindLoc(Point3D loc)
	{
		int x = loc.X, y = loc.Y;

		return x >= 5120 && y >= 0 && x < 5376 && y < 256;
	}

	public static bool IsFeluccaWind(Map map, Point3D loc)
	{
		return map == Map.Felucca && IsWindLoc(loc);
	}

	public static bool IsTrammelWind(Map map, Point3D loc)
	{
		return map == Map.Trammel && IsWindLoc(loc);
	}

	public static bool IsIlshenar(Map map)
	{
		return map == Map.Ilshenar;
	}

	private static bool IsSolenHiveLoc(Point3D loc)
	{
		int x = loc.X, y = loc.Y;

		return x >= 5640 && y >= 1776 && x < 5935 && y < 2039;
	}

	public static bool IsTrammelSolenHive(Map map, Point3D loc)
	{
		return map == Map.Trammel && IsSolenHiveLoc(loc);
	}

	public static bool IsFeluccaSolenHive(Map map, Point3D loc)
	{
		return map == Map.Felucca && IsSolenHiveLoc(loc);
	}

	public static bool IsFeluccaT2A(Map map, Point3D loc)
	{
		int x = loc.X, y = loc.Y;

		return map == Map.Felucca && x >= 5120 && y >= 2304 && x < 6144 && y < 4096;
	}

	public static bool IsAnyT2A(Map map, Point3D loc)
	{
		int x = loc.X, y = loc.Y;

		return (map == Map.Trammel || map == Map.Felucca) && x >= 5120 && y >= 2304 && x < 6144 && y < 4096;
	}

	public static bool IsFeluccaDungeon(Map map, Point3D loc)
	{
		Region region = Region.Find(loc, map);
		return region.IsPartOf(typeof(DungeonRegion)) && region.Map == Map.Felucca;
	}

	public static bool IsKhaldun(Map map, Point3D loc)
	{
		return Region.Find(loc, map).Name == "Khaldun";
	}

	public static bool IsCrystalCave(Map map, Point3D loc)
	{
		if (map != Map.Malas || loc.Z >= -80)
			return false;

		int x = loc.X, y = loc.Y;

		return (x >= 1182 && y >= 437 && x < 1211 && y < 470)
		       || (x >= 1156 && y >= 470 && x < 1211 && y < 503)
		       || (x >= 1176 && y >= 503 && x < 1208 && y < 509)
		       || (x >= 1188 && y >= 509 && x < 1201 && y < 513);
	}

	public static bool IsSafeZone(Map map, Point3D loc)
	{
		#region Duels

		if (!Region.Find(loc, map).IsPartOf(typeof(SafeZone)))
			return false;

		if (_travelType is not (TravelCheckType.TeleportTo or TravelCheckType.TeleportFrom))
			return true;

		if (_travelCaster is PlayerMobile {DuelPlayer.Eliminated: false})
			return true;

		return true;
		#endregion

	}

	public static bool IsFactionStronghold(Map map, Point3D loc)
	{
		/*// Teleporting is allowed, but only for faction members
		if ( !Core.AOS && m_TravelCaster != null && (m_TravelType == TravelCheckType.TeleportTo || m_TravelType == TravelCheckType.TeleportFrom) )
		{
			if ( Factions.Faction.Find( m_TravelCaster, true, true ) != null )
				return false;
		}*/

		return Region.Find(loc, map).IsPartOf(typeof(Factions.StrongholdRegion));
	}

	public static bool IsChampionSpawn(Map map, Point3D loc)
	{
		return Region.Find(loc, map).IsPartOf(typeof(Engines.Champions.ChampionSpawnRegion));
	}

	public static bool IsDoomFerry(Map map, Point3D loc)
	{
		if (map != Map.Malas)
			return false;

		int x = loc.X, y = loc.Y;

		switch (x)
		{
			case >= 426 when y >= 314 && x <= 430 && y <= 331:
			case >= 406 when y >= 247 && x <= 410 && y <= 264:
				return true;
			default:
				return false;
		}
	}

	public static bool IsTokunoDungeon(Map map, Point3D loc)
	{
		//The tokuno dungeons are really inside malas
		if (map != Map.Malas)
			return false;

		int x = loc.X, y = loc.Y;
		_ = loc.Z;

		bool r1 = x >= 0 && y >= 0 && x <= 128 && y <= 128;
		bool r2 = x >= 45 && y >= 320 && x < 195 && y < 710;

		return r1 || r2;
	}

	public static bool IsDoomGauntlet(Map map, Point3D loc)
	{
		if (map != Map.Malas)
			return false;

		int x = loc.X - 256, y = loc.Y - 304;

		return x >= 0 && y >= 0 && x < 256 && y < 256;
	}

	public static bool IsLampRoom(Map map, Point3D loc)
	{
		if (map != Map.Malas)
			return false;

		int x = loc.X, y = loc.Y;

		return x >= 465 && y >= 92 && x < 474 && y < 102;
	}

	public static bool IsGuardianRoom(Map map, Point3D loc)
	{
		if (map != Map.Malas)
			return false;

		int x = loc.X, y = loc.Y;

		return x >= 356 && y >= 5 && x < 375 && y < 25;
	}

	public static bool IsHeartwood(Map map, Point3D loc)
	{
		int x = loc.X, y = loc.Y;

		return (map == Map.Trammel || map == Map.Felucca) && x >= 6911 && y >= 254 && x < 7167 && y < 511;
	}

	public static bool IsMlDungeon(Map map, Point3D loc)
	{
		return MondainsLegacy.IsMlRegion(Region.Find(loc, map));
	}

	public static bool IsTombOfKings(Map map, Point3D loc)
	{
		return Region.Find(loc, map).IsPartOf(typeof(TombOfKingsRegion));
	}

	public static bool IsMazeOfDeath(Map map, Point3D loc)
	{
		return Region.Find(loc, map).IsPartOf(typeof(TombOfKingsRegion));
		//	return Region.Find(loc, map).IsPartOf(typeof(MazeOfDeathRegion));
	}

	public static bool IsSaEntrance(Map map, Point3D loc)
	{
		return map == Map.TerMur && loc.X >= 1122 && loc.Y >= 1067 && loc.X <= 1144 && loc.Y <= 1086;
	}

	public static bool IsSaDungeon(Map map, Point3D loc)
	{
		if (map != Map.TerMur)
		{
			return false;
		}

		Region region = Region.Find(loc, map);
		return region.IsPartOf(typeof(DungeonRegion)) && !region.IsPartOf(typeof(TombOfKingsRegion));
	}

	public static bool IsEodon(Map map, Point3D loc)
	{
		if (map == Map.Felucca && loc.X is >= 6975 and <= 7042 && loc.Y is >= 2048 and <= 2115)
		{
			return true;
		}

		return map == Map.TerMur && loc.X is >= 0 and <= 1087 && loc.Y is >= 1344 and <= 2495;
	}

	private static bool IsNewDungeon(Map map, Point3D loc)
	{
		if (map != Map.Trammel || !Core.SA)
			return false;

		Region r = Region.Find(loc, map);

		// Revamped Dungeons with specific rules
		return r.Name is "Void Pool" or "Wrong";
	}

	private static bool IsInvalid(Map map, Point3D loc)
	{
		if (map == null || map == Map.Internal)
			return true;

		int x = loc.X, y = loc.Y;

		return x < 0 || y < 0 || x >= map.Width || y >= map.Height;
	}

	//towns
	public static bool IsTown(IPoint3D loc, Mobile caster)
	{
		if (loc is Item item)
			loc = item.GetWorldLocation();

		return IsTown(new Point3D(loc), caster);
	}

	private static bool IsTown(Point3D loc, Mobile caster)
	{
		Map map = caster.Map;

		if (map == null)
			return false;

		#region Dueling
		SafeZone sz = (SafeZone)Region.Find(loc, map).GetRegion(typeof(SafeZone));

		if (sz != null)
		{
			PlayerMobile pm = (PlayerMobile)caster;

			if (pm.DuelContext is not {Started: true} || pm.DuelPlayer == null || pm.DuelPlayer.Eliminated)
				return true;
		}
		#endregion

		GuardedRegion reg = (GuardedRegion)Region.Find(loc, map).GetRegion(typeof(GuardedRegion));

		return reg != null && !reg.IsDisabled();
	}

	public static bool CheckTown(IPoint3D loc, Mobile caster)
	{
		if (loc is Item item)
			loc = item.GetWorldLocation();

		return CheckTown(new Point3D(loc), caster);
	}

	public static bool CheckTown(Point3D loc, Mobile caster)
	{
		if (IsTown(loc, caster))
		{
			caster.SendLocalizedMessage(500946); // You cannot cast this in town!
			return false;
		}

		return true;
	}

	//magic reflection
	public static bool CheckReflect(int circle, Mobile caster, ref Mobile target)
	{
		IDamageable c = caster;
		IDamageable t = target;

		bool reflect = CheckReflect(circle, ref c, ref t);

		if (c is Mobile)
		{
		}

		if (t is Mobile mobile)
		{
			target = mobile;
		}

		return reflect;
	}

	public static bool CheckReflect(int circle, IDamageable caster, ref Mobile target)
	{
		IDamageable t = target;

		bool reflect = CheckReflect(circle, ref caster, ref t);

		if (t is Mobile mobile)
		{
			caster = mobile;
		}

		return reflect;
	}

	public static bool CheckReflect(int circle, Mobile caster, ref IDamageable target)
	{
		IDamageable c = caster;

		bool reflect = CheckReflect(circle, ref c, ref target);

		if (c is Mobile)
		{
		}

		return reflect;
	}

	public static bool CheckReflect(int circle, ref Mobile caster, ref IDamageable target, DamageType type = DamageType.Spell)
	{
		IDamageable c = caster;

		bool reflect = CheckReflect(circle, ref c, ref target);

		if (c is Mobile mobile)
		{
			caster = mobile;
		}

		return reflect;
	}

	public static bool CheckReflect(int circle, ref Mobile caster, ref Mobile target)
	{
		return CheckReflect(circle, caster, ref target);
	}

	public static bool CheckReflect(int circle, ref IDamageable source, ref IDamageable defender, DamageType type = DamageType.Spell)
	{
		bool reflect = false;
		Mobile target = defender as Mobile;

		if (Core.AOS && type >= DamageType.Spell)
		{
			if (target != null)
			{
				MirrorImageClone clone = MirrorImage.GetDeflect(target, (Mobile)defender);

				if (clone != null)
				{
					defender = clone;
					return false;
				}
			}
			else if (defender is DamageableItem && ((DamageableItem)defender).CheckReflect(circle, source))
			{
				(source, defender) = (defender, source);
				return true;
			}
		}

		if (target == null || source is not Mobile caster)
		{
			return false;
		}

		if (target.MagicDamageAbsorb > 0)
		{
			++circle;

			target.MagicDamageAbsorb -= circle;

			// This order isn't very intuitive, but you have to nullify reflect before target gets switched

			reflect = target.MagicDamageAbsorb >= 0;

			if (target is BaseCreature creature)
				creature.CheckReflect(caster, ref reflect);

			if (target.MagicDamageAbsorb <= 0)
			{
				target.MagicDamageAbsorb = 0;
				DefensiveSpell.Nullify(target);
			}

			if (!reflect)
				return false;

			target.FixedEffect(0x37B9, 10, 5);
		}
		else if (target is BaseCreature creature)
		{
			creature.CheckReflect(caster, ref reflect);

			if (!reflect)
				return false;

			target.FixedEffect(0x37B9, 10, 5);
		}
		return reflect;
	}

	private static readonly int m_SummonArea = Settings.Configuration.Get<int>("Spells", "SummonArea");
	private static readonly int m_SummonLimit = Settings.Configuration.Get<int>("Spells", "SummonLimit");

	public static void CheckSummonLimits(BaseCreature creature)
	{
		ArrayList creatures = new();

		//int limit = 6; // 6 creatures
		//int range = 5; // per 5x5 area

		var eable = creature.GetMobilesInRange(m_SummonArea);

		foreach (Mobile mobile in eable)
		{
			if (mobile != null && mobile.GetType() == creature.GetType())
			{
				creatures.Add(mobile);
			}
		}

		int amount = 0;

		if (creatures.Count > m_SummonLimit)
		{
			amount = creatures.Count - m_SummonLimit;
		}

		while (amount > 0)
		{
			for (int i = 0; i < creatures.Count; i++)
			{
				if (creatures[i] is not Mobile m || !((BaseCreature)m).Summoned)
					continue;

				if (!Utility.RandomBool() || amount <= 0)
					continue;

				m.Delete();
				amount--;
			}
		}
	}

	public static void Damage(Spell spell, Mobile target, double damage)
	{
		TimeSpan ts = GetDamageDelayForSpell(spell);

		Damage(spell, ts, target, spell.Caster, damage);
	}

	public static void Damage(TimeSpan delay, Mobile target, double damage)
	{
		Damage(delay, target, null, damage);
	}

	public static void Damage(TimeSpan delay, Mobile target, Mobile from, double damage)
	{
		Damage(null, delay, target, from, damage);
	}

	private static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage)
	{
		int iDamage = (int)damage;

		if (delay == TimeSpan.Zero)
		{
			if (from is BaseMobile creature)
				creature.AlterSpellDamageTo(target, ref iDamage);

			if (target is BaseMobile targetCreature)
				targetCreature.AlterSpellDamageFrom(from, ref iDamage);

			target.Damage(iDamage, from);
		}
		else
		{
			new SpellDamageTimer(spell, target, from, iDamage, delay).Start();
		}

		if (target is not BaseMobile mobile || from == null || delay != TimeSpan.Zero)
			return;

		mobile.OnHarmfulSpell(from, spell);
		mobile.OnDamagedBySpell(from, spell, iDamage);
	}

	public static void Damage(Spell spell, IDamageable damageable, double damage, int phys, int fire, int cold, int pois, int nrgy)
	{
		TimeSpan ts = GetDamageDelayForSpell(spell);

		Damage(spell, ts, damageable, spell.Caster, damage, phys, fire, cold, pois, nrgy, DfAlgorithm.Standard);
	}

	public static void Damage(Spell spell, IDamageable damageable, double damage, int phys, int fire, int cold, int pois, int nrgy, DfAlgorithm dfa)
	{
		TimeSpan ts = GetDamageDelayForSpell(spell);

		Damage(spell, ts, damageable, spell.Caster, damage, phys, fire, cold, pois, nrgy, dfa);
	}

	public static void Damage(Spell spell, IDamageable damageable, double damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct)
	{
		TimeSpan ts = GetDamageDelayForSpell(spell);

		Damage(spell, ts, damageable, spell.Caster, damage, phys, fire, cold, pois, nrgy, DfAlgorithm.Standard, chaos, direct);
	}

	public static void Damage(TimeSpan delay, IDamageable damageable, double damage, int phys, int fire, int cold, int pois, int nrgy)
	{
		Damage(delay, damageable, null, damage, phys, fire, cold, pois, nrgy);
	}

	public static void Damage(TimeSpan delay, IDamageable damageable, double damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct)
	{
		Damage(null, delay, damageable, null, damage, phys, fire, cold, pois, nrgy, DfAlgorithm.Standard, chaos, direct);
	}

	public static void Damage(TimeSpan delay, IDamageable damageable, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy)
	{
		Damage(delay, damageable, from, damage, phys, fire, cold, pois, nrgy, DfAlgorithm.Standard);
	}

	private static void Damage(TimeSpan delay, IDamageable damageable, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DfAlgorithm dfa)
	{
		Damage(null, delay, damageable, from, damage, phys, fire, cold, pois, nrgy, dfa);
	}

	public static void Damage(Spell spell, TimeSpan delay, IDamageable damageable, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DfAlgorithm dfa, int chaos = 0, int direct = 0)
	{
		Mobile target = damageable as Mobile;
		int iDamage = (int)damage;

		if (delay == TimeSpan.Zero)
		{
			if (from is BaseMobile bc)
				bc.AlterSpellDamageTo(target, ref iDamage);

			if (target is BaseMobile tbc)
				tbc.AlterSpellDamageFrom(from, ref iDamage);

			DamageType dtype = spell?.SpellDamageType ?? DamageType.Spell;

			if (target != null)
			{
				target.Dfa = dfa;
			}

			int damageGiven = AOS.Damage(damageable, from, iDamage, phys, fire, cold, pois, nrgy, chaos, direct, dtype);

			if (target != null)
			{
				SpellPlagueSpell.OnMobileDamaged(target);
			}

			if (target != null && target.Dfa != DfAlgorithm.Standard)
			{
				target.Dfa = DfAlgorithm.Standard;
			}

			NegativeAttributes.OnCombatAction(from);

			if (from != target)
			{
				NegativeAttributes.OnCombatAction(target);
			}

			if (from != null) // sanity check
			{
				AOS.DoLeech(damageGiven, from, target);
			}
		}
		else
		{
			new SpellDamageTimerAos(spell, damageable, from, iDamage, phys, fire, cold, pois, nrgy, chaos, direct, delay, dfa).Start();
		}

		if (target is not BaseMobile mobile || from == null || delay != TimeSpan.Zero)
			return;

		mobile.OnHarmfulSpell(from, spell);
		mobile.OnDamagedBySpell(from, spell, iDamage);
	}

	public static void Heal(int amount, Mobile target, Mobile from, bool message = true)
	{
		//TODO: All Healing *spells* go through ArcaneEmpowerment
		target.Heal(amount, from, message);
	}

	private class SpellDamageTimer : Timer
	{
		private readonly Mobile _target, _from;
		private int _damage;
		private readonly Spell _spell;

		public SpellDamageTimer(Spell s, Mobile target, Mobile from, int damage, TimeSpan delay)
			: base(delay)
		{
			_target = target;
			_from = from;
			_damage = damage;
			_spell = s;

			if (_spell is {DelayedDamage: true, DelayedDamageStacking: false})
				_spell.StartDelayedDamageContext(target, this);

			Priority = TimerPriority.TwentyFiveMs;
		}

		protected override void OnTick()
		{
			if (_from is BaseMobile bc)
				bc.AlterSpellDamageTo(_target, ref _damage);

			if (_target is BaseMobile tbc)
				tbc.AlterSpellDamageFrom(_from, ref _damage);

			_target.Damage(_damage);
			_spell?.RemoveDelayedDamageContext(_target);
		}
	}

	private class SpellDamageTimerAos : Timer
	{
		private readonly IDamageable _target;
		private readonly Mobile _from;
		private int _damage;
		private readonly int _phys;
		private readonly int _fire;
		private readonly int _cold;
		private readonly int _pois;
		private readonly int _nrgy;
		private readonly int _chaos;
		private readonly int _direct;
		private readonly DfAlgorithm _dfa;

		private Spell Spell { get; }

		public SpellDamageTimerAos(Spell s, IDamageable target, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, TimeSpan delay, DfAlgorithm dfa)
			: base(delay)
		{
			_target = target;
			_from = from;
			_damage = damage;
			_phys = phys;
			_fire = fire;
			_cold = cold;
			_pois = pois;
			_nrgy = nrgy;
			_chaos = chaos;
			_direct = direct;
			_dfa = dfa;
			Spell = s;

			if (Spell is {DelayedDamage: true, DelayedDamageStacking: false})
			{
				Spell.StartDelayedDamageContext(target, this);
			}

			Priority = TimerPriority.TwentyFiveMs;
		}

		protected override void OnTick()
		{
			Mobile target = _target as Mobile;

			if (_from is BaseCreature baseCreature && target != null)
			{
				baseCreature.AlterSpellDamageTo(target, ref _damage);
			}

			if (_target is BaseCreature creature && _from != null)
			{
				creature.AlterSpellDamageFrom(_from, ref _damage);
			}

			DamageType dtype = Spell?.SpellDamageType ?? DamageType.Spell;

			if (target != null)
			{
				target.Dfa = _dfa;
			}

			int damageGiven = AOS.Damage(_target, _from, _damage, _phys, _fire, _cold, _pois, _nrgy, _chaos, _direct, dtype);

			if (target != null && target.Dfa != DfAlgorithm.Standard)
			{
				target.Dfa = DfAlgorithm.Standard;
			}

			if (_target is BaseMobile bm && _from != null)
			{
				bm.OnHarmfulSpell(_from);
				bm.OnDamagedBySpell(_from, Spell, damageGiven);
			}

			if (target != null)
			{
				SpellPlagueSpell.OnMobileDamaged(target);
			}

			Spell?.RemoveDelayedDamageContext(_target);

			NegativeAttributes.OnCombatAction(_from);

			if (_from != target)
			{
				NegativeAttributes.OnCombatAction(target);
			}
		}
	}
}
