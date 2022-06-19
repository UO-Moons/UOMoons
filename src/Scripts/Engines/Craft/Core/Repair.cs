using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;
using System;

namespace Server.Engines.Craft;

public interface IRepairable
{
	CraftSystem RepairSystem { get; }
}

public class Repair
{
	public static void Do(Mobile from, CraftSystem craftSystem, ITool tool)
	{
		from.Target = new CraftSystemTarget(craftSystem, tool);
		from.SendLocalizedMessage(1044276); // Target an item to repair.
	}

	public static void Do(Mobile from, CraftSystem craftSystem, RepairDeed deed)
	{
		from.Target = new CraftSystemTarget(craftSystem, deed);
		from.SendLocalizedMessage(1044276); // Target an item to repair.
	}

	public static void Do(Mobile from, CraftSystem craftSystem, RepairBenchAddon addon)
	{
		from.Target = new CraftSystemTarget(craftSystem, addon);
		from.SendLocalizedMessage(500436); // Select item to repair.
	}

	private class CraftSystemTarget : Target
	{
		private readonly CraftSystem _mCraftSystem;
		private readonly ITool _mTool;
		private readonly RepairDeed _mDeed;
		private readonly RepairBenchAddon _mAddon;

		public CraftSystemTarget(CraftSystem craftSystem, ITool tool)
			: base(10, false, TargetFlags.None)
		{
			_mCraftSystem = craftSystem;
			_mTool = tool;
		}

		public CraftSystemTarget(CraftSystem craftSystem, RepairDeed deed)
			: base(2, false, TargetFlags.None)
		{
			_mCraftSystem = craftSystem;
			_mDeed = deed;
		}

		public CraftSystemTarget(CraftSystem craftSystem, RepairBenchAddon addon)
			: base(2, false, TargetFlags.None)
		{
			_mCraftSystem = craftSystem;
			_mAddon = addon;
		}

		private static void EndMobileRepair(object state)
		{
			((Mobile)state).EndAction(typeof(IRepairableMobile));
		}

		private int GetWeakenChance(Mobile mob, SkillName skill, int curHits, int maxHits)
		{
			double value = 0;

			if (_mDeed != null)
			{
				value = _mDeed.SkillLevel;
			}
			else if (_mAddon != null)
			{
				value = _mAddon.Tools.Find(x => x.System == _mCraftSystem)!.SkillValue;
			}
			else
			{
				value = mob.Skills[skill].Value;
			}

			// 40% - (1% per hp lost) - (1% per 10 craft skill)
			return 40 + (maxHits - curHits) - (int)(value / 10);
		}

		private bool CheckWeaken(Mobile mob, SkillName skill, int curHits, int maxHits)
		{
			return GetWeakenChance(mob, skill, curHits, maxHits) > Utility.Random(100);
		}

		private int GetRepairDifficulty(int curHits, int maxHits)
		{
			return (maxHits - curHits) * 1250 / Math.Max(maxHits, 1) - 250;
		}

		private bool CheckRepairDifficulty(Mobile mob, SkillName skill, int curHits, int maxHits)
		{
			double difficulty = GetRepairDifficulty(curHits, maxHits) * 0.1;

			if (_mDeed != null)
			{
				double value = _mDeed.SkillLevel;
				double minSkill = difficulty - 25.0;
				double maxSkill = difficulty + 25;

				if (value < minSkill)
				{
					return false; // Too difficult
				}

				if (value >= maxSkill)
				{
					return true; // No challenge
				}

				double chance = (value - minSkill) / (maxSkill - minSkill);

				return chance >= Utility.RandomDouble();
			}
			else if (_mAddon != null)
			{
				double value = _mAddon.Tools.Find(x => x.System == _mCraftSystem)!.SkillValue;
				double minSkill = difficulty - 25.0;
				double maxSkill = difficulty + 25;

				if (value < minSkill)
				{
					return false; // Too difficult
				}

				if (value >= maxSkill)
				{
					return true; // No challenge
				}

				double chance = (value - minSkill) / (maxSkill - minSkill);

				return chance >= Utility.RandomDouble();
			}
			else
			{
				SkillLock sl = mob.Skills[SkillName.Tinkering].Lock;
				mob.Skills[SkillName.Tinkering].SetLockNoRelay(SkillLock.Locked);

				bool check = mob.CheckSkill(skill, difficulty - 25.0, difficulty + 25.0);

				mob.Skills[SkillName.Tinkering].SetLockNoRelay(sl);

				return check;
			}
		}

		private bool CheckDeed(Mobile from)
		{
			return _mDeed == null || _mDeed.Check(from);
		}

		private bool CheckSpecial(IEntity item)
		{
			return item is IRepairable repairable && repairable.RepairSystem == _mCraftSystem;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			bool usingDeed = _mDeed != null || _mAddon != null;
			bool toDelete = false;
			int number;

			double value = 0;

			if (_mDeed != null)
			{
				value = _mDeed.SkillLevel;
			}
			else if (_mAddon != null)
			{
				var tool = _mAddon.Tools.Find(x => x.System == _mCraftSystem);

				if (tool is {Charges: 0})
				{
					from.SendLocalizedMessage(1019073);// This item is out of charges.
					// m_Addon.Using = false;
					_mAddon.User = null;
					return;
				}

				if (tool != null) value = tool.SkillValue;
			}
			else
			{
				value = from.Skills[_mCraftSystem.MainSkill].Base;
			}

			if (_mCraftSystem is DefTinkering && targeted is IRepairableMobile mobile)
			{
				if (TryRepairMobile(from, mobile, usingDeed, out toDelete))
				{
					number = 1044279; // You repair the item.

					_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, mobile);
				}
				else
				{
					number = 500426; // You can't repair that.
				}
			}
			else if (targeted is Item item)
			{
				if (from.InRange(item.GetWorldLocation(), 2))
				{
					if (!CheckDeed(from))
					{
						if (_mAddon != null)
						{
							// m_Addon.Using = false;
							_mAddon.User = null;
						}

						return;
					}

					if (!AllowsRepair(item, _mCraftSystem))
					{
						from.SendLocalizedMessage(500426); // You can't repair that.

						if (_mAddon != null)
						{
							// m_Addon.Using = false;
							_mAddon.User = null;
						}

						return;
					}

					if (_mCraftSystem.CanCraft(from, _mTool, item.GetType()) == 1044267)
					{
						number = 1044282; // You must be near a forge and and anvil to repair items. * Yes, there are two and's *
					}
					else switch (item)
					{
						case BaseWeapon weapon:
						{
							SkillName skill = _mCraftSystem.MainSkill;
							int toWeaken = 0;

							if (Core.AOS)
							{
								toWeaken = 1;
							}
							else if (skill != SkillName.Tailoring)
							{
								toWeaken = value switch
								{
									>= 90.0 => 1,
									>= 70.0 => 2,
									_ => 3
								};
							}

							if (_mCraftSystem.CraftItems.SearchForSubclass(weapon.GetType()) == null && !CheckSpecial(weapon))
							{
								number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
							}
							else if (!weapon.IsChildOf(from.Backpack) && (!Core.ML || weapon.Parent != from))
							{
								number = 1044275; // The item must be in your backpack to repair it.
							}
							else if (!Core.AOS && weapon.PoisonCharges != 0)
							{
								number = 1005012; // You cannot repair an item while a caustic substance is on it.
							}
							else if (weapon.MaxHitPoints <= 0 || weapon.HitPoints == weapon.MaxHitPoints)
							{
								number = 1044281; // That item is in full repair
							}
							else if (weapon.MaxHitPoints <= toWeaken)
							{
								number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
							}
							else
							{
								if (CheckWeaken(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
								{
									weapon.MaxHitPoints -= toWeaken;
									weapon.HitPoints = Math.Max(0, weapon.HitPoints - toWeaken);
								}

								if (CheckRepairDifficulty(from, skill, weapon.HitPoints, weapon.MaxHitPoints))
								{
									number = 1044279; // You repair the item.
									_mCraftSystem.PlayCraftEffect(from);
									weapon.HitPoints = weapon.MaxHitPoints;

									_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, weapon);
								}
								else
								{
									number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
									_mCraftSystem.PlayCraftEffect(from);
								}

								toDelete = true;
							}

							break;
						}
						case BaseArmor armor:
						{
							SkillName skill = _mCraftSystem.MainSkill;
							int toWeaken = 0;

							if (Core.AOS)
							{
								toWeaken = 1;
							}
							else if (skill != SkillName.Tailoring)
							{
								toWeaken = value switch
								{
									>= 90.0 => 1,
									>= 70.0 => 2,
									_ => 3
								};
							}

							if (_mCraftSystem.CraftItems.SearchForSubclass(armor.GetType()) == null && !CheckSpecial(armor))
							{
								number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
							}
							else if (!armor.IsChildOf(from.Backpack) && (!Core.ML || armor.Parent != from))
							{
								number = 1044275; // The item must be in your backpack to repair it.
							}
							else if (armor.MaxHitPoints <= 0 || armor.HitPoints == armor.MaxHitPoints)
							{
								number = 1044281; // That item is in full repair
							}
							else if (armor.MaxHitPoints <= toWeaken)
							{
								number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
							}
							else
							{
								if (CheckWeaken(from, skill, armor.HitPoints, armor.MaxHitPoints))
								{
									armor.MaxHitPoints -= toWeaken;
									armor.HitPoints = Math.Max(0, armor.HitPoints - toWeaken);
								}

								if (CheckRepairDifficulty(from, skill, armor.HitPoints, armor.MaxHitPoints))
								{
									number = 1044279; // You repair the item.
									_mCraftSystem.PlayCraftEffect(from);
									armor.HitPoints = armor.MaxHitPoints;

									_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, armor);
								}
								else
								{
									number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
									_mCraftSystem.PlayCraftEffect(from);
								}

								toDelete = true;
							}

							break;
						}
						case BaseJewel jewel:
						{
							SkillName skill = _mCraftSystem.MainSkill;
							int toWeaken = 0;

							if (Core.AOS)
							{
								toWeaken = 1;
							}
							else if (skill != SkillName.Tailoring)
							{
								toWeaken = value switch
								{
									>= 90.0 => 1,
									>= 70.0 => 2,
									_ => 3
								};
							}

							if (_mCraftSystem.CraftItems.SearchForSubclass(jewel.GetType()) == null && !CheckSpecial(jewel))
							{
								number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
							}
							else if (!jewel.IsChildOf(from.Backpack))
							{
								number = 1044275; // The item must be in your backpack to repair it.
							}
							else if (jewel.MaxHitPoints <= 0 || jewel.HitPoints == jewel.MaxHitPoints)
							{
								number = 1044281; // That item is in full repair
							}
							else if (jewel.MaxHitPoints <= toWeaken)
							{
								number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
							}
							else
							{
								if (CheckWeaken(from, skill, jewel.HitPoints, jewel.MaxHitPoints))
								{
									jewel.MaxHitPoints -= toWeaken;
									jewel.HitPoints = Math.Max(0, jewel.HitPoints - toWeaken);
								}

								if (CheckRepairDifficulty(from, skill, jewel.HitPoints, jewel.MaxHitPoints))
								{
									number = 1044279; // You repair the item.
									_mCraftSystem.PlayCraftEffect(from);
									jewel.HitPoints = jewel.MaxHitPoints;

									_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, jewel);
								}
								else
								{
									number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
									_mCraftSystem.PlayCraftEffect(from);
								}

								toDelete = true;
							}

							break;
						}
						case BaseClothing clothing:
						{
							SkillName skill = _mCraftSystem.MainSkill;
							int toWeaken = 0;

							if (Core.AOS)
							{
								toWeaken = 1;
							}
							else if (skill != SkillName.Tailoring)
							{
								toWeaken = value switch
								{
									>= 90.0 => 1,
									>= 70.0 => 2,
									_ => 3
								};
							}

							if (_mCraftSystem.CraftItems.SearchForSubclass(clothing.GetType()) == null && !CheckSpecial(clothing))
							{
								number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
							}
							else if (!clothing.IsChildOf(from.Backpack) && (!Core.ML || clothing.Parent != from))
							{
								number = 1044275; // The item must be in your backpack to repair it.
							}
							else if (clothing.MaxHitPoints <= 0 || clothing.HitPoints == clothing.MaxHitPoints)
							{
								number = 1044281; // That item is in full repair
							}
							else if (clothing.MaxHitPoints <= toWeaken)
							{
								number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
							}
							else
							{
								if (CheckWeaken(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
								{
									clothing.MaxHitPoints -= toWeaken;
									clothing.HitPoints = Math.Max(0, clothing.HitPoints - toWeaken);
								}

								if (CheckRepairDifficulty(from, skill, clothing.HitPoints, clothing.MaxHitPoints))
								{
									number = 1044279; // You repair the item.
									_mCraftSystem.PlayCraftEffect(from);
									clothing.HitPoints = clothing.MaxHitPoints;

									_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, clothing);
								}
								else
								{
									number = usingDeed ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
									_mCraftSystem.PlayCraftEffect(from);
								}

								toDelete = true;
							}

							break;
						}
						case BaseTalisman talisman:
						{
							SkillName skill = _mCraftSystem.MainSkill;
							int toWeaken = 0;

							if (Core.AOS)
							{
								toWeaken = 1;
							}
							else if (skill != SkillName.Tailoring)
							{
								toWeaken = value switch
								{
									>= 90.0 => 1,
									>= 70.0 => 2,
									_ => 3
								};
							}

							if (_mCraftSystem is not DefTinkering)
							{
								number = usingDeed ? 1061136 : 1044277; // That item cannot be repaired. // You cannot repair that item with this type of repair contract.
							}
							else if (!talisman.IsChildOf(from.Backpack) && (!Core.ML || talisman.Parent != from))
							{
								number = 1044275; // The item must be in your backpack to repair it.
							}
							else if (talisman.MaxHitPoints <= 0 || talisman.HitPoints == talisman.MaxHitPoints)
							{
								number = 1044281; // That item is in full repair
							}
							else if (talisman.MaxHitPoints <= toWeaken)
							{
								number = 1044278; // That item has been repaired many times, and will break if repairs are attempted again.
							}
							else if (!talisman.CanRepair)// quick fix
							{
								number = 1044277; // That item cannot be repaired.
							}
							else
							{
								if (CheckWeaken(from, skill, talisman.HitPoints, talisman.MaxHitPoints))
								{
									talisman.MaxHitPoints -= toWeaken;
									talisman.HitPoints = Math.Max(0, talisman.HitPoints - toWeaken);
								}

								if (CheckRepairDifficulty(from, skill, talisman.HitPoints, talisman.MaxHitPoints))
								{
									number = 1044279; // You repair the item.
									_mCraftSystem.PlayCraftEffect(from);
									talisman.HitPoints = talisman.MaxHitPoints;

									_mCraftSystem.OnRepair(from, _mTool, _mDeed, _mAddon, talisman);
								}
								else
								{
									number = (usingDeed) ? 1061137 : 1044280; // You fail to repair the item. [And the contract is destroyed]
									_mCraftSystem.PlayCraftEffect(from);
								}

								toDelete = true;
							}

							break;
						}
						case BlankScroll scroll when !usingDeed:
						{
							SkillName skill = _mCraftSystem.MainSkill;

							if (from.Skills[skill].Value >= 50.0)
							{
								scroll.Consume(1);
								RepairDeed deed = new(RepairDeed.GetTypeFor(_mCraftSystem), from.Skills[skill].Value, from);
								from.AddToBackpack(deed);

								number = 500442; // You create the item and put it in your backpack.
							}
							else
							{
								number = 1047005; // You must be at least apprentice level to create a repair service contract.
							}

							break;
						}
						case BlankScroll:
							number = 1061136; // You cannot repair that item with this type of repair contract.
							break;
						default:
							number = 500426; // You can't repair that.
							break;
					}
				}
				else
				{
					number = 500446; // That is too far away.
				}
			}
			else
			{
				number = 500426; // You can't repair that.
			}

			if (!usingDeed)
			{
				CraftContext context = _mCraftSystem.GetContext(from);
				from.SendGump(new CraftGump(from, _mCraftSystem, _mTool, number));
			}
			else
			{
				if (_mAddon is {Deleted: false})
				{
					var tool = _mAddon.Tools.Find(x => x.System == _mCraftSystem);

					if (tool != null) tool.Charges--;

					from.SendGump(new RepairBenchGump(from, _mAddon));

					from.SendLocalizedMessage(number);
				}
				else
				{
					from.SendLocalizedMessage(number);

					if (toDelete)
					{
						if (_mDeed != null) _mDeed.Delete();
					}
				}
			}
		}

		protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
		{
			if (_mAddon is {Deleted: false})
			{
				from.SendGump(new RepairBenchGump(from, _mAddon));
			}
		}

		private bool TryRepairMobile(Mobile from, IRepairableMobile m, bool usingDeed, out bool toDelete)
		{
			int damage = m.HitsMax - m.Hits;
			BaseCreature bc = m as BaseCreature;
			toDelete = false;

			string name = bc != null ? bc.Name : "the creature";

			if (!from.InRange(m.Location, 2))
			{
				from.SendLocalizedMessage(1113612, name); // You must move closer to attempt to repair ~1_CREATURE~.
			}
			else if (bc != null && bc.IsDeadBondedPet)
			{
				from.SendLocalizedMessage(500426); // You can't repair that.
			}
			else if (damage <= 0)
			{
				from.SendLocalizedMessage(1113613, name); // ~1_CREATURE~ doesn't appear to be damaged.
			}
			else
			{
				double value = 0;

				if (_mDeed != null)
				{
					value = _mDeed.SkillLevel;
				}
				else if (_mAddon != null)
				{
					value = _mAddon.Tools.Find(x => x.System == _mCraftSystem)!.SkillValue;
				}
				else
				{
					value = from.Skills[SkillName.Tinkering].Value;
				}

				double skillValue = value;
				const double required = 0.1;

				if (skillValue < required)
				{
					from.SendLocalizedMessage(Math.Abs(required - 80.0) < 120.0 ? 1157049 : 1113614, name);
				}
				else if (!from.CanBeginAction(typeof(IRepairableMobile)))
				{
					from.SendLocalizedMessage(1113611, name); // You must wait a moment before attempting to repair ~1_CREATURE~ again.
				}
				else if (bc != null && bc.GetMaster() != null && bc.GetMaster() != from && !bc.GetMaster().InRange(from.Location, 10))
				{
					from.SendLocalizedMessage(1157045); // The pet's owner must be nearby to attempt repair.
				}
				else if (!from.CanBeBeneficial(bc, false, false))
				{
					from.SendLocalizedMessage(1001017); // You cannot perform beneficial acts on your target.
				}
				else
				{
					if (damage > (int)(skillValue * 0.6))
					{
						damage = (int)(skillValue * 0.6);
					}

					SkillLock sl = from.Skills[SkillName.Tinkering].Lock;
					from.Skills[SkillName.Tinkering].SetLockNoRelay(SkillLock.Locked);

					if (!from.CheckSkill(SkillName.Tinkering, 0.0, 100.0))
					{
						damage /= 6;
					}

					from.Skills[SkillName.Tinkering].SetLockNoRelay(sl);

					Container pack = from.Backpack;

					if (pack != null)
					{
						int v = pack.ConsumeUpTo(m.RepairResource, (damage + 4) / 5);

						if (v <= 0 && m is Golem)
						{
							v = pack.ConsumeUpTo(typeof(BronzeIngot), (damage + 4) / 5);
						}

						if (v > 0)
						{
							m.Hits += damage;

							from.SendLocalizedMessage(damage > 1 ? 1113616 : 1157030, name);// You repair ~1_CREATURE~.// You repair ~1_CREATURE~, but it barely helps.

							toDelete = true;
							double delay = 10 - skillValue / 16.65;

							from.BeginAction(typeof(IRepairableMobile));
							Timer.DelayCall(TimeSpan.FromSeconds(delay), new TimerStateCallback(EndMobileRepair), from);

							return true;
						}
						else if (m is Golem)
						{
							from.SendLocalizedMessage(1113615, name); // You need some iron or bronze ingots to repair the ~1_CREATURE~.
						}
						else
						{
							from.SendLocalizedMessage(1044037); // You do not have sufficient metal to make that.
						}
					}
					else
					{
						from.SendLocalizedMessage(1044037); // You do not have sufficient metal to make that.
					}
				}
			}

			return false;
		}
	}

	public static bool AllowsRepair(object targeted, CraftSystem system)
	{
		if (targeted is IFactionItem {FactionItemState: { }})
		{
			return false;
		}

		if (targeted is IRepairableMobile)
		{
			return true;
		}

		return targeted is BlankScroll or BaseArmor {CanRepair: true} or BaseWeapon {CanRepair: true} or BaseClothing {CanRepair: true} or BaseJewel {CanRepair: true} or BaseTalisman {CanRepair: true};
	}
}
