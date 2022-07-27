using Server.Factions;
using Server.Items;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.SkillHandlers;

public class RemoveTrap
{
	public interface IRemoveTrapTrainingKit
	{
		void OnRemoveTrap(Mobile m);
	}

	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.RemoveTrap].Callback = OnUse;
	}

	private static TimeSpan OnUse(Mobile m)
	{
		switch (Core.TOL)
		{
			case false when m.Skills[SkillName.Lockpicking].Value < 50:
				m.SendLocalizedMessage(502366); // You do not know enough about locks.  Become better at picking locks.
				break;
			case false when m.Skills[SkillName.DetectHidden].Value < 50:
				m.SendLocalizedMessage(502367); // You are not perceptive enough.  Become better at detect hidden.
				break;
			default:
				m.Target = new InternalTarget();

				m.SendLocalizedMessage(502368); // which trap will you attempt to disarm?
				break;
		}

		return TimeSpan.FromSeconds(10.0); // 10 second delay before begin able to re-use a skill
	}

	private class InternalTarget : Target
	{
		public InternalTarget()
			: base(2, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			switch (targeted)
			{
				case Mobile:
					from.SendLocalizedMessage(502816); // You feel that such an action would be inappropriate
					break;
				case IRemoveTrapTrainingKit kit:
					kit.OnRemoveTrap(from);
					break;
				case LockableContainer { Locked: true }:
					from.SendLocalizedMessage(501283); // That is locked.
					break;
				case TrapableContainer target:
				{
					from.Direction = from.GetDirectionTo(target);

					if (target.TrapType == TrapType.None)
					{
						from.SendLocalizedMessage(502373); // That doesn't appear to be trapped
					}
					else if (target is TreasureMapChest tChest && TreasureMapInfo.NewSystem)
					{
						if (tChest.Owner != from)
						{
							from.SendLocalizedMessage(1159010); // That is not your chest!
						}
						else if (IsDisarming(from))
						{
							from.SendLocalizedMessage(1159059); // You are already manipulating the trigger mechanism...
						}
						else if (IsBeingDisarmed(tChest))
						{
							from.SendLocalizedMessage(1159063); // That trap is already being disarmed.
						}
						else if (tChest.AncientGuardians.Any(g => !g.Deleted))
						{
							from.PrivateOverheadMessage(MessageType.Regular, 1150, 1159060, from.NetState); // *Your attempt fails as the the mechanism jams and you are attacked by an Ancient Chest Guardian!*
						}
						else
						{
							from.PlaySound(0x241);

							from.PrivateOverheadMessage(MessageType.Regular, 1150, 1159057, from.NetState); // *You delicately manipulate the trigger mechanism...*

							StartChestDisarmTimer(from, tChest);
						}
					}
					else
					{
						from.PlaySound(0x241);

						if (from.CheckTargetSkill(SkillName.RemoveTrap, target, target.TrapPower - 10, target.TrapPower + 10))
						{
							target.TrapPower = 0;
							target.TrapLevel = 0;
							target.TrapType = TrapType.None;
							target.InvalidateProperties();
							from.SendLocalizedMessage(502377); // You successfully render the trap harmless
						}
						else
						{
							from.SendLocalizedMessage(502372); // You fail to disarm the trap... but you don't set it off
						}
					}

					break;
				}
				/*else if (targeted is VvVTrap)
			{
				VvVTrap trap = targeted as VvVTrap;

				if (!ViceVsVirtueSystem.IsVvV(from))
				{
					from.SendLocalizedMessage(1155496); // This item can only be used by VvV participants!
				}
				else
				{
					if (from == trap.Owner || ((from.Skills[SkillName.RemoveTrap].Value - 80.0) / 20.0) > Utility.RandomDouble())
					{
						VvVTrapKit kit = new VvVTrapKit(trap.TrapType);
						trap.Delete();

						if (!from.AddToBackpack(kit))
							kit.MoveToWorld(from.Location, from.Map);

						if (trap.Owner != null && from != trap.Owner)
						{
							Guild fromG = from.Guild as Guild;
							Guild ownerG = trap.Owner.Guild as Guild;

							if (fromG != null && fromG != ownerG && !fromG.IsAlly(ownerG) && ViceVsVirtueSystem.Instance != null
								&& ViceVsVirtueSystem.Instance.Battle != null && ViceVsVirtueSystem.Instance.Battle.OnGoing)
							{
								ViceVsVirtueSystem.Instance.Battle.Update(from, UpdateType.Disarm);
							}
						}

						from.PrivateOverheadMessage(MessageType.Regular, 1154, 1155413, from.NetState);
					}
					else if (.1 > Utility.RandomDouble())
					{
						trap.Detonate(from);
					}
				}
			}*/
				case GoblinFloorTrap targ:
				{
					if (from.InRange(targ.Location, 3))
					{
						from.Direction = from.GetDirectionTo(targ);

						if (targ.Owner == null)
						{
							Item item = new FloorTrapComponent();

							if (from.Backpack == null || !from.Backpack.TryDropItem(from, item, false))
								item.MoveToWorld(from.Location, from.Map);
						}

						targ.Delete();
						from.SendLocalizedMessage(502377); // You successfully render the trap harmless
					}

					break;
				}
				case BaseFactionTrap trap:
				{
					Faction faction = Faction.Find(from);

					FactionTrapRemovalKit kit = from.Backpack?.FindItemByType(typeof(FactionTrapRemovalKit)) as FactionTrapRemovalKit;

					bool isOwner = trap.Placer == from || (trap.Faction != null && trap.Faction.IsCommander(from));

					if (faction == null)
					{
						from.SendLocalizedMessage(1010538); // You may not disarm faction traps unless you are in an opposing faction
					}
					else if (faction == trap.Faction && trap.Faction != null && !isOwner)
					{
						from.SendLocalizedMessage(1010537); // You may not disarm traps set by your own faction!
					}
					else if (!isOwner && kit == null)
					{
						from.SendLocalizedMessage(1042530); // You must have a trap removal kit at the base level of your pack to disarm a faction trap.
					}
					else
					{
						if ((Core.ML && isOwner) || (from.CheckTargetSkill(SkillName.RemoveTrap, trap, 80.0, 100.0) && from.CheckTargetSkill(SkillName.Tinkering, trap, 80.0, 100.0)))
						{
							from.PrivateOverheadMessage(MessageType.Regular, trap.MessageHue, trap.DisarmMessage, from.NetState);

							if (!isOwner)
							{
								int silver = faction.AwardSilver(from, trap.SilverFromDisarm);

								if (silver > 0)
									from.SendLocalizedMessage(1008113, true, silver.ToString("N0")); // You have been granted faction silver for removing the enemy trap :
							}

							trap.Delete();
						}
						else
						{
							from.SendLocalizedMessage(502372); // You fail to disarm the trap... but you don't set it off
						}

						if (!isOwner)
							kit.ConsumeCharge(from);
					}

					break;
				}
				default:
					from.SendLocalizedMessage(502373); // That does'nt appear to be trapped
					break;
			}
		}

		protected override void OnTargetOutOfRange(Mobile from, object targeted)
		{
			if (targeted is TreasureMapChest && TreasureMapInfo.NewSystem)
			{
				// put here to prevent abuse
				if (from.NextSkillTime > Core.TickCount)
				{
					from.NextSkillTime = Core.TickCount;
				}

				from.SendLocalizedMessage(1159058); // You are too far away from the chest to manipulate the trigger mechanism.
			}
			else
			{
				base.OnTargetOutOfRange(from, targeted);
			}
		}
	}

	private static Dictionary<Mobile, RemoveTrapTimer> _table;

	private static void StartChestDisarmTimer(Mobile from, TreasureMapChest chest)
	{
		_table ??= new Dictionary<Mobile, RemoveTrapTimer>();

		_table[from] = new RemoveTrapTimer(from, chest, from.Skills[SkillName.RemoveTrap].Value >= 100);
	}

	public static void EndChestDisarmTimer(Mobile from)
	{
		if (_table == null || !_table.ContainsKey(from))
			return;
		RemoveTrapTimer timer = _table[from];

		timer?.Stop();

		_table.Remove(from);

		if (_table.Count == 0)
		{
			_table = null;
		}
	}

	private static bool IsDisarming(Mobile from)
	{
		return _table != null && _table.ContainsKey(from);
	}

	private static bool IsBeingDisarmed(TreasureMapChest chest)
	{
		return _table != null && _table.Values.Any(timer => timer.Chest == chest);
	}
}

public class RemoveTrapTimer : Timer
{
	private Mobile From { get; }
	public TreasureMapChest Chest { get; }

	private DateTime SafetyEndTime { get; } // Used for 100 Remove Trap
	public int Stage { get; set; } // Used for 99.9- Remove Trap

	private bool GmRemover { get; }

	public RemoveTrapTimer(Mobile from, TreasureMapChest chest, bool gmRemover)
		: base(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
	{
		From = from;
		Chest = chest;
		GmRemover = gmRemover;

		if (gmRemover)
		{
			var duration = (TreasureLevel)chest.Level switch
			{
				TreasureLevel.Supply => TimeSpan.FromSeconds(60),
				TreasureLevel.Cache => TimeSpan.FromSeconds(180),
				TreasureLevel.Hoard => TimeSpan.FromSeconds(420),
				TreasureLevel.Trove => TimeSpan.FromSeconds(540),
				_ => TimeSpan.FromSeconds(20)
			};
			SafetyEndTime = Chest.DigTime + duration;
		}

		Start();
	}

	protected override void OnTick()
	{
		if (Chest.Deleted)
		{
			RemoveTrap.EndChestDisarmTimer(From);
		}

		if (!From.Alive)
		{
			From.SendLocalizedMessage(1159061); // Your ghostly fingers cannot manipulate the mechanism...
			RemoveTrap.EndChestDisarmTimer(From);
		}
		else if (!From.InRange(Chest.GetWorldLocation(), 16) || Chest.Deleted)
		{
			From.SendLocalizedMessage(1159058); // You are too far away from the chest to manipulate the trigger mechanism.
			RemoveTrap.EndChestDisarmTimer(From);
		}
		else if (GmRemover)
		{
			From.RevealingAction();

			if (SafetyEndTime < DateTime.UtcNow)
			{
				DisarmTrap();
			}
			else
			{
				if (From.CheckTargetSkill(SkillName.RemoveTrap, Chest, 80, 120 + (Chest.Level * 10)))
				{
					DisarmTrap();
				}
				else
				{
					Chest.SpawnAncientGuardian(From);
				}
			}

			RemoveTrap.EndChestDisarmTimer(From);
		}
		else
		{
			From.RevealingAction();

			double min = Math.Ceiling(From.Skills[SkillName.RemoveTrap].Value * .75);

			if (From.CheckTargetSkill(SkillName.RemoveTrap, Chest, min, min > 50 ? min + 50 : 100))
			{
				DisarmTrap();
				RemoveTrap.EndChestDisarmTimer(From);
			}
			else
			{
				Chest.SpawnAncientGuardian(From);

				if (From.Alive)
				{
					From.PrivateOverheadMessage(MessageType.Regular, 1150, 1159057, From.NetState); // *You delicately manipulate the trigger mechanism...*
				}
			}
		}
	}

	private void DisarmTrap()
	{
		Chest.TrapPower = 0;
		Chest.TrapLevel = 0;
		Chest.TrapType = TrapType.None;
		Chest.InvalidateProperties();

		From.PrivateOverheadMessage(MessageType.Regular, 1150, 1159009, From.NetState); // You successfully disarm the trap!
	}
}
