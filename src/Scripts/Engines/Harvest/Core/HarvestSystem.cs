using Server.Engines.Quests;
using Server.Engines.Quests.Hag;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Harvest;

public abstract class HarvestSystem
{
	public static void Configure()
	{
		EventSink.OnTargetByResourceMacro += TargetByResource;
	}

	protected HarvestSystem()
	{
		Definitions = new List<HarvestDefinition>();
	}

	public List<HarvestDefinition> Definitions { get; }

	public virtual bool CheckTool(Mobile from, Item tool)
	{
		bool wornOut = tool == null || tool.Deleted || tool is IUsesRemaining {UsesRemaining: <= 0};

		if (wornOut)
		{
			from.SendLocalizedMessage(1044038); // You have worn out your tool!
		}

		return !wornOut;
	}

	public virtual bool CheckHarvest(Mobile from, Item tool)
	{
		return CheckTool(from, tool);
	}

	public virtual bool CheckHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
	{
		return CheckTool(from, tool);
	}

	public virtual bool CheckRange(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
	{
		bool inRange = from.Map == map && from.InRange(loc, def.MaxRange);

		if (!inRange)
		{
			def.SendMessageTo(from, timed ? def.TimedOutOfRangeMessage : def.OutOfRangeMessage);
		}

		return inRange;
	}

	public virtual bool CheckResources(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
	{
		HarvestBank bank = def.GetBank(map, loc.X, loc.Y);
		bool available = bank != null && bank.Current >= def.ConsumedPerHarvest;

		if (!available)
		{
			def.SendMessageTo(from, timed ? def.DoubleHarvestMessage : def.NoResourcesMessage);
		}

		return available;
	}

	public virtual void OnBadHarvestTarget(Mobile from, Item tool, object toHarvest)
	{
	}

	public virtual object GetLock(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
	{
		/* Here we prevent multiple harvesting.
		* 
		* Some options:
		*  - 'return tool;' : This will allow the player to harvest more than once concurrently, but only if they use multiple tools. This seems to be as OSI.
		*  - 'return GetType();' : This will disallow multiple harvesting of the same type. That is, we couldn't mine more than once concurrently, but we could be both mining and lumberjacking.
		*  - 'return typeof( HarvestSystem );' : This will completely restrict concurrent harvesting.
		*/
		return tool;
	}

	public virtual void OnConcurrentHarvest(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
	{
	}

	public virtual void OnHarvestStarted(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
	{
	}

	public virtual bool BeginHarvesting(Mobile from, Item tool)
	{
		if (!CheckHarvest(from, tool))
		{
			return false;
		}

		EventSink.InvokeOnResourceHarvestAttempt(from, tool, this);
		from.Target = new HarvestTarget(tool, this);
		return true;
	}

	public virtual void FinishHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked)
	{
		from.EndAction(locked);

		if (!CheckHarvest(from, tool))
		{
			return;
		}

		if (!GetHarvestDetails(from, tool, toHarvest, out int tileId, out Map map, out Point3D loc))
		{
			OnBadHarvestTarget(from, tool, toHarvest);
			return;
		}

		if (!def.Validate(tileId) && !def.ValidateSpecial(tileId))
		{
			OnBadHarvestTarget(from, tool, toHarvest);
			return;
		}

		if (!CheckRange(from, tool, def, map, loc, true))
		{
			return;
		}

		if (!CheckResources(from, tool, def, map, loc, true))
		{
			return;
		}

		if (!CheckHarvest(from, tool, def, toHarvest))
		{
			return;
		}

		if (SpecialHarvest(from, tool, def, map, loc))
		{
			return;
		}

		HarvestBank bank = def.GetBank(map, loc.X, loc.Y);

		if (bank == null)
		{
			return;
		}

		HarvestVein vein = bank.Vein;

		if (vein != null)
		{
			vein = MutateVein(from, tool, def, bank, toHarvest, vein);
		}

		if (vein == null)
		{
			return;
		}

		HarvestResource primary = vein.PrimaryResource;
		HarvestResource fallback = vein.FallbackResource;
		HarvestResource resource = MutateResource(from, tool, def, map, loc, vein, primary, fallback);

		double skillBase = from.Skills[def.Skill].Base;

		Type type = null;

		if (CheckHarvestSkill(map, loc, from, resource, def))
		{
			type = GetResourceType(from, tool, def, map, loc, resource);

			if (type != null)
			{
				type = MutateType(type, from, tool, def, map, loc, resource);
			}

			if (type != null)
			{
				Item item = Construct(type, from, tool);

				if (item == null)
				{
					type = null;
				}
				else
				{
					int amount = def.ConsumedPerHarvest;
					int feluccaAmount = def.ConsumedPerFeluccaHarvest;

					if (item is BaseGranite)
					{
						feluccaAmount = 3;
					}

					//The whole harvest system is kludgy and I'm sure this is just adding to it.
					if (item.Stackable)
					{
						int racialAmount = (int)Math.Ceiling(amount * 1.1);
						int feluccaRacialAmount = (int)Math.Ceiling(feluccaAmount * 1.1);

						bool eligableForRacialBonus = (def.RaceBonus && from.Race == Race.Human);
						bool inFelucca = map == Map.Felucca;

						if (eligableForRacialBonus && inFelucca && bank.Current >= feluccaRacialAmount && 0.1 > Utility.RandomDouble())
						{
							item.Amount = feluccaRacialAmount;
						}
						else if (inFelucca && bank.Current >= feluccaAmount)
						{
							item.Amount = feluccaAmount;
						}
						else if (eligableForRacialBonus && bank.Current >= racialAmount && 0.1 > Utility.RandomDouble())
						{
							item.Amount = racialAmount;
						}
						else
						{
							item.Amount = amount;
						}
					}

					if (from.IsPlayer())
					{
						bank.Consume(amount, from);
					}

					if (Give(from, item, def.PlaceAtFeetIfFull))
					{
						SendSuccessTo(from, item, resource);
					}
					else
					{
						SendPackFullTo(from, item, def, resource);
						item.Delete();
					}

					BonusHarvestResource bonus = def.GetBonusResource();
					Item bonusItem = null;

					if (bonus is {Type: { }} && skillBase >= bonus.ReqSkill)
					{
						if (bonus.RequiredMap == null || bonus.RequiredMap == from.Map)
						{
							bonusItem = Construct(bonus.Type, from, tool);

							if (Give(from, bonusItem, true))    //Bonuses always allow placing at feet, even if pack is full irregrdless of def
							{
								bonus.SendSuccessTo(from);
							}
							else
							{
								bonusItem.Delete();
							}
						}
					}

					EventSink.InvokeOnResourceHarvestSuccess(from, tool, item, bonusItem, this);
				}

				#region High Seas
				OnToolUsed(from, tool, item != null);
				#endregion
			}

			// Siege rules will take into account axes and polearms used for lumberjacking
			if (tool is IUsesRemaining toolWithUses and (BaseHarvestTool or Pickaxe or SturdyPickaxe or GargoylesPickaxe))
			{
				toolWithUses.ShowUsesRemaining = true;

				if (toolWithUses.UsesRemaining > 0)
				{
					--toolWithUses.UsesRemaining;
				}

				if (toolWithUses.UsesRemaining < 1)
				{
					tool.Delete();
					def.SendMessageTo(from, def.ToolBrokeMessage);
				}
			}
		}

		if (type == null)
		{
			def.SendMessageTo(from, def.FailMessage);
		}

		OnHarvestFinished(from, tool, def, vein, bank, resource, toHarvest);
	}

	public virtual bool CheckHarvestSkill(Map map, Point3D loc, Mobile from, HarvestResource resource, HarvestDefinition def)
	{
		return from.Skills[def.Skill].Value >= resource.ReqSkill && from.CheckSkill(def.Skill, resource.MinSkill, resource.MaxSkill);
	}

	public virtual void OnToolUsed(Mobile from, Item tool, bool caughtSomething)
	{
	}

	public virtual void OnHarvestFinished(Mobile from, Item tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
	{
	}

	public virtual bool SpecialHarvest(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
	{
		return false;
	}

	public virtual Item Construct(Type type, Mobile from, Item tool)
	{
		try
		{
			return Activator.CreateInstance(type) as Item;
		}
		catch
		{
			return null;
		}
	}

	public virtual HarvestVein MutateVein(Mobile from, Item tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein)
	{
		return vein;
	}

	public virtual void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
	{
		resource.SendSuccessTo(from);
	}

	public virtual void SendPackFullTo(Mobile from, Item item, HarvestDefinition def, HarvestResource resource)
	{
		def.SendMessageTo(from, def.PackFullMessage);
	}

	public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
	{
		if (m.PlaceInBackpack(item))
		{
			return true;
		}

		if (!placeAtFeet)
		{
			return false;
		}

		Map map = m.Map;

		if (map == null || map == Map.Internal)
		{
			return false;
		}

		IPooledEnumerable eable = m.GetItemsInRange(0);

		List<Item> atFeet = eable.Cast<Item>().ToList();

		eable.Free();

		for (var i = 0; i < atFeet.Count; ++i)
		{
			Item check = atFeet[i];

			if (check.StackWith(m, item, false))
			{
				return true;
			}
		}

		ColUtility.Free(atFeet);

		item.MoveToWorld(m.Location, map);
		return true;
	}

	public virtual Type MutateType(Type type, Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
	{
		return from.Region.GetResource(type);
	}

	public virtual Type GetResourceType(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
	{
		return resource.Types.Length > 0 ? resource.Types[Utility.Random(resource.Types.Length)] : null;
	}

	public virtual HarvestResource MutateResource(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc, HarvestVein vein, HarvestResource primary, HarvestResource fallback)
	{
		bool racialBonus = def.RaceBonus && from.Race == Race.Elf;

		if (vein.ChanceToFallback > (Utility.RandomDouble() + (racialBonus ? .20 : 0)))
		{
			return fallback;
		}

		double skillValue = from.Skills[def.Skill].Value;

		if (fallback != null && (skillValue < primary.ReqSkill || skillValue < primary.MinSkill))
		{
			return fallback;
		}

		return primary;
	}

	public virtual bool OnHarvesting(Mobile from, Item tool, HarvestDefinition def, object toHarvest, object locked, bool last)
	{
		if (!CheckHarvest(from, tool))
		{
			from.EndAction(locked);
			return false;
		}

		if (!GetHarvestDetails(from, tool, toHarvest, out int tileId, out Map map, out Point3D loc))
		{
			from.EndAction(locked);
			OnBadHarvestTarget(from, tool, toHarvest);
			return false;
		}

		if (!def.Validate(tileId) && !def.ValidateSpecial(tileId))
		{
			from.EndAction(locked);
			OnBadHarvestTarget(from, tool, toHarvest);
			return false;
		}

		if (!CheckRange(from, tool, def, map, loc, true))
		{
			from.EndAction(locked);
			return false;
		}

		if (!CheckResources(from, tool, def, map, loc, true))
		{
			from.EndAction(locked);
			return false;
		}

		if (!CheckHarvest(from, tool, def, toHarvest))
		{
			from.EndAction(locked);
			return false;
		}

		DoHarvestingEffect(from, tool, def, map, loc);

		new HarvestSoundTimer(from, tool, this, def, toHarvest, locked, last).Start();

		return !last;
	}

	public virtual void DoHarvestingSound(Mobile from, Item tool, HarvestDefinition def, object toHarvest)
	{
		if (def.EffectSounds.Length > 0)
		{
			from.PlaySound(Utility.RandomList(def.EffectSounds));
		}
	}

	public virtual void DoHarvestingEffect(Mobile from, Item tool, HarvestDefinition def, Map map, Point3D loc)
	{
		from.Direction = from.GetDirectionTo(loc);

		if (from.Mounted) return;
		if (Core.SA)
		{
			from.Animate(AnimationType.Attack, Utility.RandomList(def.EffectActions));
		}
		else
		{
			from.Animate(Utility.RandomList(def.EffectActions), 5, 1, true, false, 0);
		}
	}

	public virtual HarvestDefinition GetDefinition(int tileId)
	{
		return GetDefinition(tileId, null);
	}

	public virtual HarvestDefinition GetDefinition(int tileId, Item tool)
	{
		HarvestDefinition def = null;

		for (int i = 0; def == null && i < Definitions.Count; ++i)
		{
			HarvestDefinition check = Definitions[i];

			if (check.Validate(tileId))
			{
				def = check;
			}
		}

		return def;
	}

	#region High Seas
	public virtual HarvestDefinition GetDefinitionFromSpecialTile(int tileId)
	{
		HarvestDefinition def = null;

		for (int i = 0; def == null && i < Definitions.Count; ++i)
		{
			HarvestDefinition check = Definitions[i];

			if (check.ValidateSpecial(tileId))
			{
				def = check;
			}
		}

		return def;
	}
	#endregion

	public virtual void StartHarvesting(Mobile from, Item tool, object toHarvest)
	{
		if (!CheckHarvest(from, tool))
		{
			return;
		}

		if (!GetHarvestDetails(from, tool, toHarvest, out int tileId, out Map map, out Point3D loc))
		{
			OnBadHarvestTarget(from, tool, toHarvest);
			return;
		}

		HarvestDefinition def = GetDefinition(tileId, tool);

		if (def == null)
		{
			OnBadHarvestTarget(from, tool, toHarvest);
			return;
		}

		if (!CheckRange(from, tool, def, map, loc, false))
		{
			return;
		}

		if (!CheckResources(from, tool, def, map, loc, false))
		{
			return;
		}

		if (!CheckHarvest(from, tool, def, toHarvest))
		{
			return;
		}

		object toLock = GetLock(from, tool, def, toHarvest);

		if (!from.BeginAction(toLock))
		{
			OnConcurrentHarvest(from, tool, def, toHarvest);
			return;
		}

		new HarvestTimer(from, tool, this, def, toHarvest, toLock).Start();
		OnHarvestStarted(from, tool, def, toHarvest);
	}

	public virtual bool GetHarvestDetails(Mobile from, Item tool, object toHarvest, out int tileId, out Map map, out Point3D loc)
	{
		switch (toHarvest)
		{
			case Static {Movable: false} @static:
			{
				tileId = (@static.ItemId & 0x3FFF) | 0x4000;
				map = @static.Map;
				loc = @static.GetWorldLocation();
				break;
			}
			case StaticTarget objtarg:
				tileId = (objtarg.ItemID & 0x3FFF) | 0x4000;
				map = from.Map;
				loc = objtarg.Location;
				break;
			case LandTarget obj:
				tileId = obj.TileID;
				map = from.Map;
				loc = obj.Location;
				break;
			default:
				tileId = 0;
				map = null;
				loc = Point3D.Zero;
				return false;
		}

		return map != null && map != Map.Internal;
	}

	#region Enhanced Client
	public static void TargetByResource(Mobile m, Item tool, short resourceType)
	{
		HarvestSystem system = null;
		HarvestDefinition def = null;

		if (tool is IHarvestTool harvestTool)
		{
			system = harvestTool.HarvestSystem;
		}

		if (system == null) return;
		switch (resourceType)
		{
			case 0: // ore
				if (system is Mining mining)
				{
					def = mining.OreAndStone;
				}

				break;
			case 1: // sand
				if (system is Mining mining1)
				{
					def = mining1.Sand;
				}

				break;
			case 2: // wood
				if (system is Lumberjacking lumberjacking)
				{
					def = lumberjacking.Definition;
				}

				break;
			case 3: // grave
				if (TryHarvestGrave(m))
				{
					return;
				}

				break;
			case 4: // red shrooms
				if (TryHarvestShrooms(m))
				{
					return;
				}

				break;
		}

		if (def != null && FindValidTile(m, def, out object toHarvest))
		{
			system.StartHarvesting(m, tool, toHarvest);
			return;
		}

		system.OnBadHarvestTarget(m, tool, new LandTarget(new Point3D(0, 0, 0), Map.Felucca));
	}

	private static bool FindValidTile(IEntity m, HarvestDefinition definition, out object toHarvest)
	{
		Map map = m.Map;
		toHarvest = null;

		if (map == null || map == Map.Internal)
		{
			return false;
		}

		for (int x = m.X - 1; x <= m.X + 1; x++)
		{
			for (int y = m.Y - 1; y <= m.Y + 1; y++)
			{
				StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);

				if (tiles.Length > 0)
				{
					foreach (var tile in tiles)
					{
						int id = (tile.Id & 0x3FFF) | 0x4000;

						if (!definition.Validate(id)) continue;
						toHarvest = new StaticTarget(new Point3D(x, y, tile.Z), tile.Id);
						return true;
					}
				}

				LandTile lt = map.Tiles.GetLandTile(x, y);

				if (!definition.Validate(lt.Id)) continue;
				toHarvest = new LandTarget(new Point3D(x, y, lt.Z), map);
				return true;
			}
		}

		return false;
	}

	public static bool TryHarvestGrave(Mobile m)
	{
		Map map = m.Map;

		if (map == null)
		{
			return false;
		}

		for (int x = m.X - 1; x <= m.X + 1; x++)
		{
			for (int y = m.Y - 1; y <= m.Y + 1; y++)
			{
				StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);

				foreach (var tile in tiles)
				{
					int itemId = tile.Id;

					if (itemId != 0xED3 && itemId != 0xEDF && itemId != 0xEE0 && itemId != 0xEE1 && itemId != 0xEE2 &&
					    itemId != 0xEE8) continue;
					if (m is not PlayerMobile player) continue;
					QuestSystem qs = player.Quest;

					if (qs is not WitchApprenticeQuest) continue;
					if (qs.FindObjective(typeof(FindIngredientObjective)) is not FindIngredientObjective obj ||
					    obj.Completed || obj.Ingredient != Ingredient.Bones) continue;
					player.SendLocalizedMessage(1055037); // You finish your grim work, finding some of the specific bones listed in the Hag's recipe.
					obj.Complete();

					return true;
				}
			}
		}

		return false;
	}

	public static bool TryHarvestShrooms(Mobile m)
	{
		Map map = m.Map;

		if (map == null)
		{
			return false;
		}

		for (int x = m.X - 1; x <= m.X + 1; x++)
		{
			for (int y = m.Y - 1; y <= m.Y + 1; y++)
			{
				StaticTile[] tiles = map.Tiles.GetStaticTiles(x, y, false);

				foreach (var tile in tiles)
				{
					int itemId = tile.Id;

					if (itemId != 0xD15 && itemId != 0xD16) continue;
					if (m is not PlayerMobile player) continue;
					QuestSystem qs = player.Quest;

					if (qs is not WitchApprenticeQuest) continue;
					if (qs.FindObjective(typeof(FindIngredientObjective)) is not FindIngredientObjective obj ||
					    obj.Completed || obj.Ingredient != Ingredient.RedMushrooms) continue;
					player.SendLocalizedMessage(1055036); // You slice a red cap mushroom from its stem.
					obj.Complete();

					return true;
				}
			}
		}

		return false;
	}

	#endregion
}
