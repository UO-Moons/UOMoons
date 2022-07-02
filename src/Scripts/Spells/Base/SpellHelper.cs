using Server.Engines.PartySystem;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;
using Server.Spells.Fifth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Targeting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Server.Spells
{
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
		private static readonly SkillName[] _Schools =
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

		private static readonly SkillName[] _TOLSchools =
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

		public static bool HasSpellFocus(Mobile m, SkillName focus)
		{
			SkillName[] list = Core.TOL ? _TOLSchools : _Schools;

			foreach (SkillName skill in list)
			{
				if (skill != focus && m.Skills[skill].Value >= 30.0)
				{
					return false;
				}
			}

			return true;
		}

		public static int PvPSpellDamageCap(Mobile m, SkillName castskill)
		{
			if (!Core.SA)
			{
				return 15;
			}

			if (HasSpellFocus(m, castskill))
			{
				return 30;
			}
			else
			{
				return Core.TOL ? 20 : 15;
			}
		}

		public static int GetSpellDamageBonus(Mobile caster, IDamageable damageable, SkillName skill, bool playerVsPlayer)
		{
			Mobile target = damageable as Mobile;

			int sdiBonus = AosAttributes.GetValue(caster, AosAttribute.SpellDamage);

			if (target != null)
			{
				//if (RunedSashOfWarding.IsUnderEffects(target, WardingEffect.SpellDamage))
				//{
				//	sdiBonus -= 10;
				//}

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

		public static readonly bool RestrictTravelCombat = true;

		public static TimeSpan GetDamageDelayForSpell(Spell sp)
		{
			if (!sp.DelayedDamage)
				return TimeSpan.Zero;

			return Core.AOS ? AosDamageDelay : OldDamageDelay;
		}

		public static bool CheckMulti(Point3D p, Map map)
		{
			return CheckMulti(p, map, true, 0);
		}

		public static bool CheckMulti(Point3D p, Map map, bool houses)
		{
			return CheckMulti(p, map, houses, 0);
		}

		public static bool CheckMulti(Point3D p, Map map, bool houses, int housingrange)
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
			else if (from != target)
			{
				d = (int)from.GetDirectionTo(target);
			}

			if (d > -1)
			{
				from.Direction = (Direction)d;
				from.ProcessDelta();
			}
		}

		public static bool CheckCombat(Mobile m)
		{
			if (!RestrictTravelCombat)
				return false;

			for (int i = 0; i < m.Aggressed.Count; ++i)
			{
				AggressorInfo info = m.Aggressed[i];

				if (info.Defender.Player && (DateTime.UtcNow - info.LastCombatTime) < BaseMobile.CombatHeatDelay)
					return true;
			}

			if (Core.AOS)
			{
				for (int i = 0; i < m.Aggressors.Count; ++i)
				{
					AggressorInfo info = m.Aggressors[i];

					if (info.Attacker.Player && (DateTime.UtcNow - info.LastCombatTime) < BaseMobile.CombatHeatDelay)
						return true;
				}
			}

			return false;
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

				if (map.CanFit(loc, height, true, mobsBlock))
				{
					p = loc;
					return true;
				}
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

			foreach (Item item in eable)
			{
				Type t = item.GetType();

				if (t.IsDefined(typeof(DispellableAttributes), false) || t.IsDefined(typeof(DispellableAttribute), true))
				{
					eable.Free();
					return false;
				}
			}

			eable.Free();
			return true;
		}

		public static bool CheckWater(Point3D p, Map map)
		{
			var landTile = map.Tiles.GetLandTile(p.X, p.Y);

			if (landTile.Z == p.Z && ((landTile.Id >= 168 && landTile.Id <= 171) || (landTile.Id >= 310 && landTile.Id <= 311)))
			{
				return false;
			}

			var tiles = map.Tiles.GetStaticTiles(p.X, p.Y, true);

			foreach (var tile in tiles)
			{
				if (tile.Z == p.Z && tile.Id >= 0x1796 && tile.Id <= 0x17B2)
				{
					return false;
				}
			}

			return true;
		}

		public static bool CanRevealCaster(Mobile m)
		{
			if (m is BaseCreature bc)
			{
				if (!bc.Controlled)
					return true;
			}

			return false;
		}

		public static void GetSurfaceTop(ref IPoint3D p)
		{
			if (p is Item item)
			{
				p = item.GetSurfaceTop();
			}
			else if (p is StaticTarget t)
			{
				int z = t.Z;

				if ((t.Flags & TileFlag.Surface) == 0)
					z -= TileData.ItemTable[t.ItemID & TileData.MaxItemValue].CalcHeight;

				p = new Point3D(t.X, t.Y, z);
			}
		}

		protected static void RemoveStatOffsetCallback(object state)
		{
			if (state is not Mobile)
			{
				return;
			}
			// This call has the side-effect of updating all stats
			((Mobile)state).CheckStatTimers();
		}

		public static bool AddStatOffset(Mobile m, StatType type, int offset, TimeSpan duration)
		{
			if (offset > 0)
				return AddStatBonus(m, m, type, offset, duration);
			else if (offset < 0)
				return AddStatCurse(m, m, type, -offset, duration);

			return true;
		}

		public static bool AddStatBonus(Mobile caster, Mobile target, bool blockSkill, StatType type)
		{
			return AddStatBonus(caster, target, type, GetOffset(caster, target, type, false, blockSkill), GetDuration(caster, target));
		}

		public static bool AddStatBonus(Mobile caster, Mobile target, StatType type, int bonus, TimeSpan duration)
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

			return mod != null ? mod.Offset : 0;
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type)
		{
			return AddStatCurse(caster, target, type, true);
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type, bool blockSkill)
		{
			return AddStatCurse(caster, target, type, GetOffset(caster, target, type, true, blockSkill), TimeSpan.Zero);
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type, bool blockSkill, int offset)
		{
			return AddStatCurse(caster, target, type, offset, TimeSpan.Zero);
		}

		public static bool AddStatCurse(Mobile caster, Mobile target, StatType type, int curse, TimeSpan duration)
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
			if (Core.AOS)
				return TimeSpan.FromSeconds(((6 * caster.Skills.EvalInt.Fixed) / 50) + 1);

			return TimeSpan.FromSeconds(caster.Skills[SkillName.Magery].Value * 1.2);
		}


		public static int GetCurseOffset(Mobile m, StatType type)
		{
			string name = $"[Magic] {type} Curse";

			StatMod mod = m.GetStatMod(name);

			return mod != null ? mod.Offset : 0;
		}

		public static bool DisableSkillCheck { get; set; }

		public static double GetOffsetScalar(Mobile caster, Mobile target, bool curse)
		{
			double percent;

			if (curse)
				percent = 8 + (caster.Skills.EvalInt.Fixed / 100) - (target.Skills.MagicResist.Fixed / 100);
			else
				percent = 1 + (caster.Skills.EvalInt.Fixed / 100);

			percent *= 0.01;

			if (percent < 0)
				percent = 0;

			return percent;
		}

		public static int GetOffset(Mobile caster, Mobile target, StatType type, bool curse, bool blockSkill)
		{
			if (Core.AOS)
			{
				if (!blockSkill)
				{
					caster.CheckSkill(SkillName.EvalInt, 0.0, 120.0);

					if (curse)
						target.CheckSkill(SkillName.MagicResist, 0.0, 120.0);
				}

				double percent = GetOffsetScalar(caster, target, curse);

				switch (type)
				{
					case StatType.Str:
						return (int)(target.RawStr * percent);
					case StatType.Dex:
						return (int)(target.RawDex * percent);
					case StatType.Int:
						return (int)(target.RawInt * percent);
				}
			}

			return 1 + (int)(caster.Skills[SkillName.Magery].Value * 0.1);
		}

		public static Guild GetGuildFor(Mobile m)
		{
			Guild g = m.Guild as Guild;

			if (g == null && m is BaseCreature c)
			{
				m = c.ControlMaster;

				if (m != null)
					g = m.Guild as Guild;

				if (g == null)
				{
					m = c.SummonMaster;

					if (m != null)
						g = m.Guild as Guild;
				}
			}

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

			if (pmFrom == null && from is BaseCreature bcFrom && bcFrom.Summoned)
			{
				pmFrom = bcFrom.SummonMaster as PlayerMobile;
			}

			if (pmTarg == null && to is BaseCreature bcTarg && bcTarg.Summoned)
			{
				pmTarg = bcTarg.SummonMaster as PlayerMobile;
			}

			if (pmFrom != null && pmTarg != null)
			{
				if (pmFrom.DuelContext != null && pmFrom.DuelContext == pmTarg.DuelContext && pmFrom.DuelContext.Started && pmFrom.DuelPlayer != null && pmTarg.DuelPlayer != null)
					return (pmFrom.DuelPlayer.Participant != pmTarg.DuelPlayer.Participant);
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
				BaseCreature fromBC = creature1;
				BaseCreature toBC = creature2;

				if (fromBC.GetMaster() is BaseCreature)
				{
					fromBC = fromBC.GetMaster() as BaseCreature;
				}

				if (toBC.GetMaster() is BaseCreature)
				{
					toBC = toBC.GetMaster() as BaseCreature;
				}

				if (toBC.IsEnemy(fromBC))   //Natural Enemies
				{
					return true;
				}

				//All involved are monsters- no damage. If falls through this statement, normal noto rules apply
				if (!toBC.Controlled && !toBC.Summoned && !fromBC.Controlled && !fromBC.Summoned) //All involved are monsters- no damage
				{
					return false;
				}
			}

			if (to is BaseCreature creature && !creature.Controlled && creature.InitialInnocent)
				return true;

			int noto = Notoriety.Compute(from, to);

			return noto != Notoriety.Innocent || from.Murderer;
		}

		public static IEnumerable<IDamageable> AcquireIndirectTargets(Mobile caster, IPoint3D p, Map map, int range)
		{
			return AcquireIndirectTargets(caster, p, map, range, true);
		}

		public static IEnumerable<IDamageable> AcquireIndirectTargets(Mobile caster, IPoint3D p, Map map, int range, bool losCheck)
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

			double scale = 1.0 + ((caster.Skills[SkillName.Magery].Value - 100.0) / 200.0);

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
				else
				{
					int z = map.GetAverageZ(x, y);

					if (map.CanSpawnMobile(x, y, z))
					{
						p = new Point3D(x, y, z);
						return true;
					}
				}
			}

			return false;
		}

		public static void Configure()
		{
			Console.Write("Loading TravelRestrictions...");
			if (LoadTravelRestrictions())
				Console.WriteLine("done");
			else
				Console.WriteLine("failed");
		}
		/*
		public static void TravelRestrictions(XmlElement xml, Map map, Region parent)
		{
			XmlElement e = xml["TravelRestrictions"];
			foreach (XmlElement r in e.ChildNodes)//GetElementsByTagName("Region"))
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
				TravelRules t = new TravelRules();
				t.Validator = v;
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
		}*/

		public static bool LoadTravelRestrictions()
		{
			string filePath = Path.Combine("Data", "TravelRestrictions.xml");

			if (!File.Exists(filePath))
				return false;

			XmlDocument x = new();
			x.Load(filePath);

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

		private static readonly Dictionary<string, TravelRules> m_TravelRestrictions = new();
		private delegate bool TravelValidator(Map map, Point3D loc);

		public static void SendInvalidMessage(Mobile caster, TravelCheckType type)
		{
			if (type == TravelCheckType.RecallTo || type == TravelCheckType.GateTo)
				caster.SendLocalizedMessage(1019004); // You are not allowed to travel there.
			else if (type == TravelCheckType.TeleportTo)
				caster.SendLocalizedMessage(501035); // You cannot teleport from here to the destination.
			else
				caster.SendLocalizedMessage(501802); // Thy spell doth not appear to work...
		}

		public static bool CheckTravel(Mobile caster, TravelCheckType type)
		{
			return CheckTravel(caster, caster.Map, caster.Location, type);
		}

		public static bool CheckTravel(Map map, Point3D loc, TravelCheckType type)
		{
			return CheckTravel(null, map, loc, type);
		}

		private static Mobile m_TravelCaster;
		private static TravelCheckType m_TravelType;

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
					else if (caster.Region is GreenAcres)
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
			m_TravelCaster = caster;
			m_TravelType = type;

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

				if (m_TravelCaster != null && m_TravelCaster.Region != null)
				{
					if (m_TravelCaster.Region.IsPartOf("Blighted Grove") && loc.Z < -10)
					{
						isValid = false;
					}
				}

				if ((int)type <= 4 && (IsNewDungeon(caster.Map, caster.Location) || IsNewDungeon(map, loc)))
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

		public static bool IsWindLoc(Point3D loc)
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

		public static bool IsIlshenar(Map map, Point3D loc)
		{
			return map == Map.Ilshenar;
		}

		public static bool IsSolenHiveLoc(Point3D loc)
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

			return (map == Map.Felucca && x >= 5120 && y >= 2304 && x < 6144 && y < 4096);
		}

		public static bool IsAnyT2A(Map map, Point3D loc)
		{
			int x = loc.X, y = loc.Y;

			return ((map == Map.Trammel || map == Map.Felucca) && x >= 5120 && y >= 2304 && x < 6144 && y < 4096);
		}

		public static bool IsFeluccaDungeon(Map map, Point3D loc)
		{
			Region region = Region.Find(loc, map);
			return (region.IsPartOf(typeof(DungeonRegion)) && region.Map == Map.Felucca);
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
			if (Region.Find(loc, map).IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				if (m_TravelType == TravelCheckType.TeleportTo || m_TravelType == TravelCheckType.TeleportFrom)
				{
					if (m_TravelCaster is PlayerMobile pm && pm.DuelPlayer != null && !pm.DuelPlayer.Eliminated)
						return true;
				}

				return true;
			}
			#endregion

			return false;
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
			return (Region.Find(loc, map).IsPartOf(typeof(Engines.Champions.ChampionSpawnRegion)));
		}

		public static bool IsDoomFerry(Map map, Point3D loc)
		{
			if (map != Map.Malas)
				return false;

			int x = loc.X, y = loc.Y;

			if (x >= 426 && y >= 314 && x <= 430 && y <= 331)
				return true;

			if (x >= 406 && y >= 247 && x <= 410 && y <= 264)
				return true;

			return false;
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

			return (x >= 0 && y >= 0 && x < 256 && y < 256);
		}

		public static bool IsLampRoom(Map map, Point3D loc)
		{
			if (map != Map.Malas)
				return false;

			int x = loc.X, y = loc.Y;

			return (x >= 465 && y >= 92 && x < 474 && y < 102);
		}

		public static bool IsGuardianRoom(Map map, Point3D loc)
		{
			if (map != Map.Malas)
				return false;

			int x = loc.X, y = loc.Y;

			return (x >= 356 && y >= 5 && x < 375 && y < 25);
		}

		public static bool IsHeartwood(Map map, Point3D loc)
		{
			int x = loc.X, y = loc.Y;

			return (map == Map.Trammel || map == Map.Felucca) && (x >= 6911 && y >= 254 && x < 7167 && y < 511);
		}

		public static bool IsMLDungeon(Map map, Point3D loc)
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

		public static bool IsSAEntrance(Map map, Point3D loc)
		{
			return map == Map.TerMur && loc.X >= 1122 && loc.Y >= 1067 && loc.X <= 1144 && loc.Y <= 1086;
		}

		public static bool IsSADungeon(Map map, Point3D loc)
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
			if (map == Map.Felucca && loc.X >= 6975 && loc.X <= 7042 && loc.Y >= 2048 && loc.Y <= 2115)
			{
				return true;
			}

			return map == Map.TerMur && loc.X >= 0 && loc.X <= 1087 && loc.Y >= 1344 && loc.Y <= 2495;
		}

		public static bool IsNewDungeon(Map map, Point3D loc)
		{
			if (map == Map.Trammel && Core.SA)
			{
				Region r = Region.Find(loc, map);

				// Revamped Dungeons with specific rules
				if (r.Name == "Void Pool" || r.Name == "Wrong")
				{
					return true;
				}
			}

			return false;
		}

		public static bool IsInvalid(Map map, Point3D loc)
		{
			if (map == null || map == Map.Internal)
				return true;

			int x = loc.X, y = loc.Y;

			return (x < 0 || y < 0 || x >= map.Width || y >= map.Height);
		}

		//towns
		public static bool IsTown(IPoint3D loc, Mobile caster)
		{
			if (loc is Item item)
				loc = item.GetWorldLocation();

			return IsTown(new Point3D(loc), caster);
		}

		public static bool IsTown(Point3D loc, Mobile caster)
		{
			Map map = caster.Map;

			if (map == null)
				return false;

			#region Dueling
			Engines.ConPVP.SafeZone sz = (Engines.ConPVP.SafeZone)Region.Find(loc, map).GetRegion(typeof(Engines.ConPVP.SafeZone));

			if (sz != null)
			{
				PlayerMobile pm = (PlayerMobile)caster;

				if (pm == null || pm.DuelContext == null || !pm.DuelContext.Started || pm.DuelPlayer == null || pm.DuelPlayer.Eliminated)
					return true;
			}
			#endregion

			GuardedRegion reg = (GuardedRegion)Region.Find(loc, map).GetRegion(typeof(GuardedRegion));

			return (reg != null && !reg.IsDisabled());
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
			IDamageable c = caster as IDamageable;
			IDamageable t = target as IDamageable;

			bool reflect = CheckReflect(circle, ref c, ref t);

			if (c is Mobile)
			{
			}

			if (t is Mobile)
			{
				target = (Mobile)t;
			}

			return reflect;
		}

		public static bool CheckReflect(int circle, IDamageable caster, ref Mobile target)
		{
			IDamageable t = target as IDamageable;

			bool reflect = CheckReflect(circle, ref caster, ref t);

			if (t is Mobile)
			{
				caster = (Mobile)t;
			}

			return reflect;
		}

		public static bool CheckReflect(int circle, Mobile caster, ref IDamageable target)
		{
			IDamageable c = caster as IDamageable;

			bool reflect = CheckReflect(circle, ref c, ref target);

			if (c is Mobile)
			{
			}

			return reflect;
		}

		public static bool CheckReflect(int circle, ref Mobile caster, ref IDamageable target, DamageType type = DamageType.Spell)
		{
			IDamageable c = caster as IDamageable;

			bool reflect = CheckReflect(circle, ref c, ref target);

			if (c is Mobile)
			{
				caster = (Mobile)c;
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
				if (target != null && defender is Mobile)
				{
					Clone clone = MirrorImage.GetDeflect(target, (Mobile)defender);

					if (clone != null)
					{
						defender = clone;
						return false;
					}
				}
				//else if (defender is DamageableItem && ((DamageableItem)defender).CheckReflect(circle, source))
				//{
				//	IDamageable temp = source;
				//	source = defender;
				//	defender = temp;
				//	return true;
				//}
			}

			if (target == null || !(source is Mobile caster))
			{
				return false;
			}

			if (target.MagicDamageAbsorb > 0)
			{
				++circle;

				target.MagicDamageAbsorb -= circle;

				// This order isn't very intuitive, but you have to nullify reflect before target gets switched

				reflect = (target.MagicDamageAbsorb >= 0);

				if (target is BaseCreature creature)
					creature.CheckReflect(caster, ref reflect);

				if (target.MagicDamageAbsorb <= 0)
				{
					target.MagicDamageAbsorb = 0;
					DefensiveSpell.Nullify(target);
				}

				if (reflect)
				{
					target.FixedEffect(0x37B9, 10, 5);

					Mobile temp = caster;
					caster = target;
					target = temp;
				}
			}
			else if (target is BaseCreature creature)
			{
				reflect = false;

				creature.CheckReflect(caster, ref reflect);

				if (reflect)
				{
					target.FixedEffect(0x37B9, 10, 5);

					Mobile temp = caster;
					caster = target;
					target = temp;
				}
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
					Mobile m = creatures[i] as Mobile;

					if (m != null && ((BaseCreature)m).Summoned)
					{
						if (Utility.RandomBool() && amount > 0)
						{
							m.Delete();
							amount--;
						}
					}
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

		public static void Damage(Spell spell, TimeSpan delay, Mobile target, Mobile from, double damage)
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

			if (target is BaseMobile mobile && from != null && delay == TimeSpan.Zero)
			{
				mobile.OnHarmfulSpell(from, spell);
				mobile.OnDamagedBySpell(from, spell, iDamage);
			}
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

		public static void Damage(TimeSpan delay, IDamageable damageable, Mobile from, double damage, int phys, int fire, int cold, int pois, int nrgy, DfAlgorithm dfa)
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

				DamageType dtype = spell != null ? spell.SpellDamageType : DamageType.Spell;

				if (target != null)
				{
					target.Dfa = dfa;
				}

				int damageGiven = AOS.Damage(damageable, from, iDamage, phys, fire, cold, pois, nrgy, chaos, direct, dtype);

				//if (target != null)
				//{
				//	Spells.Mysticism.SpellPlagueSpell.OnMobileDamaged(target);
				//}

				if (target != null && target.Dfa != DfAlgorithm.Standard)
				{
					target.Dfa = DfAlgorithm.Standard;
				}

				//NegativeAttributes.OnCombatAction(from);

				//if (from != target)
				//{
				//	NegativeAttributes.OnCombatAction(target);
				//}

				if (from != null) // sanity check
				{
					AOS.DoLeech(damageGiven, from, target);
				}
			}
			else
			{
				new SpellDamageTimerAOS(spell, damageable, from, iDamage, phys, fire, cold, pois, nrgy, chaos, direct, delay, dfa).Start();
			}

			if (target is BaseMobile mobile && from != null && delay == TimeSpan.Zero)
			{
				mobile.OnHarmfulSpell(from, spell);
				mobile.OnDamagedBySpell(from, spell, iDamage);
			}
		}

		/*public static void DoLeech(int damageGiven, Mobile from, Mobile target)
		{
			TransformContext context = TransformationSpellHelper.GetContext(from);

			if (context != null) /* cleanup */
			/*{
				if (context.Type == typeof(WraithFormSpell))
				{
					int wraithLeech = (5 + (int)((15 * from.Skills.SpiritSpeak.Value) / 100)); // Wraith form gives 5-20% mana leech
					int manaLeech = AOS.Scale(damageGiven, wraithLeech);
					if (manaLeech != 0)
					{
						from.Mana += manaLeech;
						from.PlaySound(0x44D);
					}
				}
				else if (context.Type == typeof(VampiricEmbraceSpell))
				{
					from.Hits += AOS.Scale(damageGiven, 20);
					from.PlaySound(0x44D);
				}
			}
		}*/

		public static void Heal(int amount, Mobile target, Mobile from)
		{
			Heal(amount, target, from, true);
		}
		public static void Heal(int amount, Mobile target, Mobile from, bool message)
		{
			//TODO: All Healing *spells* go through ArcaneEmpowerment
			target.Heal(amount, from, message);
		}

		private class SpellDamageTimer : Timer
		{
			private readonly Mobile m_Target, m_From;
			private int m_Damage;
			private readonly Spell m_Spell;

			public SpellDamageTimer(Spell s, Mobile target, Mobile from, int damage, TimeSpan delay)
				: base(delay)
			{
				m_Target = target;
				m_From = from;
				m_Damage = damage;
				m_Spell = s;

				if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
					m_Spell.StartDelayedDamageContext(target, this);

				Priority = TimerPriority.TwentyFiveMs;
			}

			protected override void OnTick()
			{
				if (m_From is BaseMobile bc)
					bc.AlterSpellDamageTo(m_Target, ref m_Damage);

				if (m_Target is BaseMobile tbc)
					tbc.AlterSpellDamageFrom(m_From, ref m_Damage);

				m_Target.Damage(m_Damage);
				if (m_Spell != null)
					m_Spell.RemoveDelayedDamageContext(m_Target);
			}
		}

		public class SpellDamageTimerAOS : Timer
		{
			private IDamageable m_Target;
			private readonly Mobile m_From;
			private int m_Damage;
			private int m_Phys;
			private int m_Fire;
			private int m_Cold;
			private int m_Pois;
			private int m_Nrgy;
			private int m_Chaos;
			private int m_Direct;
			private DfAlgorithm m_DFA;

			public Spell Spell { get; }

			public SpellDamageTimerAOS(Spell s, IDamageable target, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, TimeSpan delay, DfAlgorithm dfa)
				: base(delay)
			{
				m_Target = target;
				m_From = from;
				m_Damage = damage;
				m_Phys = phys;
				m_Fire = fire;
				m_Cold = cold;
				m_Pois = pois;
				m_Nrgy = nrgy;
				m_Chaos = chaos;
				m_Direct = direct;
				m_DFA = dfa;
				Spell = s;

				if (Spell != null && Spell.DelayedDamage && !Spell.DelayedDamageStacking)
				{
					Spell.StartDelayedDamageContext(target, this);
				}

				Priority = TimerPriority.TwentyFiveMs;
			}

			protected override void OnTick()
			{
				Mobile target = m_Target as Mobile;

				if (m_From is BaseCreature && target != null)
				{
					((BaseCreature)m_From).AlterSpellDamageTo(target, ref m_Damage);
				}

				if (m_Target is BaseCreature creature && m_From != null)
				{
					creature.AlterSpellDamageFrom(m_From, ref m_Damage);
				}

				DamageType dtype = Spell != null ? Spell.SpellDamageType : DamageType.Spell;

				if (target != null)
				{
					target.Dfa = m_DFA;
				}

				int damageGiven = AOS.Damage(m_Target, m_From, m_Damage, m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy, m_Chaos, m_Direct, dtype);

				if (target != null && target.Dfa != DfAlgorithm.Standard)
				{
					target.Dfa = DfAlgorithm.Standard;
				}

				if (m_Target is BaseMobile bm && m_From != null)
				{
					bm.OnHarmfulSpell(m_From);
					bm.OnDamagedBySpell(m_From, Spell, damageGiven);
				}

				//if (target != null)
				//{
				//	Spells.Mysticism.SpellPlagueSpell.OnMobileDamaged(target);
				//}

				if (Spell != null)
				{
					Spell.RemoveDelayedDamageContext(m_Target);
				}

				//NegativeAttributes.OnCombatAction(m_From);

				//if (m_From != target)
				//{
				//	NegativeAttributes.OnCombatAction(target);
				//}
			}
		}

		/*private class SpellDamageTimerAOS : Timer
		{
			private readonly Mobile m_Target, m_From;
			private int m_Damage;
			private readonly int m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy;
			private readonly DFAlgorithm m_DFA;
			private readonly Spell m_Spell;

			public SpellDamageTimerAOS(Spell s, Mobile target, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, TimeSpan delay, DFAlgorithm dfa)
				: base(delay)
			{
				m_Target = target;
				m_From = from;
				m_Damage = damage;
				m_Phys = phys;
				m_Fire = fire;
				m_Cold = cold;
				m_Pois = pois;
				m_Nrgy = nrgy;
				m_DFA = dfa;
				m_Spell = s;
				if (m_Spell != null && m_Spell.DelayedDamage && !m_Spell.DelayedDamageStacking)
					m_Spell.StartDelayedDamageContext(target, this);

				Priority = TimerPriority.TwentyFiveMS;
			}

			protected override void OnTick()
			{
				if (m_From is BaseMobile bc && m_Target != null)
					bc.AlterSpellDamageTo(m_Target, ref m_Damage);

				if (m_Target is BaseMobile tbc && m_From != null)
					tbc.AlterSpellDamageFrom(m_From, ref m_Damage);

				WeightOverloading.DFA = m_DFA;

				int damageGiven = AOS.Damage(m_Target, m_From, m_Damage, m_Phys, m_Fire, m_Cold, m_Pois, m_Nrgy);

				if (m_From != null) // sanity check
				{
					DoLeech(damageGiven, m_From, m_Target);
				}

				WeightOverloading.DFA = DFAlgorithm.Standard;

				if (m_Target is BaseMobile bm && m_From != null)
				{
					bm.OnHarmfulSpell(m_From, m_Spell);
					bm.OnDamagedBySpell(m_From, m_Spell, damageGiven);
				}

				if (m_Spell != null)
					m_Spell.RemoveDelayedDamageContext(m_Target);
			}
		}*/
	}

	public class TransformationSpellHelper
	{
		#region Context Stuff
		private static readonly Dictionary<Mobile, TransformContext> m_Table = new();

		public static void AddContext(Mobile m, TransformContext context)
		{
			m_Table[m] = context;
		}

		public static void RemoveContext(Mobile m, bool resetGraphics)
		{
			TransformContext context = GetContext(m);

			if (context != null)
				RemoveContext(m, context, resetGraphics);
		}

		public static void RemoveContext(Mobile m, TransformContext context, bool resetGraphics)
		{
			if (!m_Table.ContainsKey(m)) return;
			m_Table.Remove(m);

			List<ResistanceMod> mods = context.Mods;

			for (int i = 0; i < mods.Count; ++i)
				m.RemoveResistanceMod(mods[i]);

			if (resetGraphics)
			{
				m.HueMod = -1;
				m.BodyMod = 0;
			}

			context.Timer.Stop();
			context.Spell.RemoveEffect(m);
		}

		public static TransformContext GetContext(Mobile m)
		{
			m_Table.TryGetValue(m, out var context);

			return context;
		}

		public static bool UnderTransformation(Mobile m)
		{
			return GetContext(m) != null;
		}

		public static bool UnderTransformation(Mobile m, Type type)
		{
			TransformContext context = GetContext(m);

			return context != null && context.Type == type;
		}

		public static void CheckCastSkill(Mobile m, TransformContext context)
		{
			if (context.Spell is Spell spell)
			{
				spell.GetCastSkills(out double min, out double max);

				if (m.Skills[spell.CastSkill].Value < min)
				{
					RemoveContext(m, context, true);
				}
			}
		}
		#endregion

		public static bool CheckCast(Mobile caster, Spell spell)
		{
			if (Factions.Sigil.ExistsOn(caster))
			{
				caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
				return false;
			}
			else if (!caster.CanBeginAction(typeof(PolymorphSpell)))
			{
				caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
				return false;
			}
			else if (AnimalForm.UnderTransformation(caster))
			{
				caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
				return false;
			}

			return true;
		}

		public static bool OnCast(Mobile caster, Spell spell)
		{
			if (spell is not ITransformationSpell transformSpell)
				return false;

			if (Factions.Sigil.ExistsOn(caster))
			{
				caster.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
			}
			else if (!caster.CanBeginAction(typeof(PolymorphSpell)))
			{
				caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
			}
			else if (DisguiseTimers.IsDisguised(caster))
			{
				caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
				return false;
			}
			else if (AnimalForm.UnderTransformation(caster))
			{
				caster.SendLocalizedMessage(1061091); // You cannot cast that spell in this form.
			}
			else if (!caster.CanBeginAction(typeof(IncognitoSpell)) || (caster.IsBodyMod && GetContext(caster) == null))
			{
				spell.DoFizzle();
			}
			else if (spell.CheckSequence())
			{
				TransformContext context = GetContext(caster);
				Type ourType = spell.GetType();

				bool wasTransformed = (context != null);
				bool ourTransform = (wasTransformed && context.Type == ourType);

				if (wasTransformed)
				{
					RemoveContext(caster, context, ourTransform);

					if (ourTransform)
					{
						caster.PlaySound(0xFA);
						caster.FixedParticles(0x3728, 1, 13, 5042, EffectLayer.Waist);
					}
				}

				if (!ourTransform)
				{
					List<ResistanceMod> mods = new();

					if (transformSpell.PhysResistOffset != 0)
						mods.Add(new ResistanceMod(ResistanceType.Physical, transformSpell.PhysResistOffset));

					if (transformSpell.FireResistOffset != 0)
						mods.Add(new ResistanceMod(ResistanceType.Fire, transformSpell.FireResistOffset));

					if (transformSpell.ColdResistOffset != 0)
						mods.Add(new ResistanceMod(ResistanceType.Cold, transformSpell.ColdResistOffset));

					if (transformSpell.PoisResistOffset != 0)
						mods.Add(new ResistanceMod(ResistanceType.Poison, transformSpell.PoisResistOffset));

					if (transformSpell.NrgyResistOffset != 0)
						mods.Add(new ResistanceMod(ResistanceType.Energy, transformSpell.NrgyResistOffset));

					if (!((Body)transformSpell.Body).IsHuman)
					{
						Mobiles.IMount mt = caster.Mount;

						if (mt != null)
							mt.Rider = null;
					}

					caster.BodyMod = transformSpell.Body;
					caster.HueMod = transformSpell.Hue;

					for (int i = 0; i < mods.Count; ++i)
						caster.AddResistanceMod(mods[i]);

					transformSpell.DoEffect(caster);

					Timer timer = new TransformTimer(caster, transformSpell);
					timer.Start();

					AddContext(caster, new TransformContext(timer, mods, ourType, transformSpell));
					return true;
				}
			}

			return false;
		}
	}

	public interface ITransformationSpell
	{
		int Body { get; }
		int Hue { get; }

		int PhysResistOffset { get; }
		int FireResistOffset { get; }
		int ColdResistOffset { get; }
		int PoisResistOffset { get; }
		int NrgyResistOffset { get; }

		double TickRate { get; }
		void OnTick(Mobile m);

		void DoEffect(Mobile m);
		void RemoveEffect(Mobile m);
	}

	public class TransformContext
	{
		public Timer Timer { get; }
		public List<ResistanceMod> Mods { get; }
		public Type Type { get; }
		public ITransformationSpell Spell { get; }

		public TransformContext(Timer timer, List<ResistanceMod> mods, Type type, ITransformationSpell spell)
		{
			Timer = timer;
			Mods = mods;
			Type = type;
			Spell = spell;
		}
	}

	public class TransformTimer : Timer
	{
		private readonly Mobile m_Mobile;
		private readonly ITransformationSpell m_Spell;

		public TransformTimer(Mobile from, ITransformationSpell spell)
			: base(TimeSpan.FromSeconds(spell.TickRate), TimeSpan.FromSeconds(spell.TickRate))
		{
			m_Mobile = from;
			m_Spell = spell;

			Priority = TimerPriority.TwoFiftyMs;
		}

		protected override void OnTick()
		{
			if (m_Mobile.Deleted || !m_Mobile.Alive || m_Mobile.Body != m_Spell.Body || m_Mobile.Hue != m_Spell.Hue)
			{
				TransformationSpellHelper.RemoveContext(m_Mobile, true);
				Stop();
			}
			else
			{
				m_Spell.OnTick(m_Mobile);
			}
		}
	}
}
