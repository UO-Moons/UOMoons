using Server.Factions;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.SkillHandlers;

public class Stealing
{
	public static void Initialize()
	{
		SkillInfo.Table[33].Callback = OnUse;
	}

	public static readonly bool ClassicMode = false;
	public static readonly bool SuspendOnMurder = false;

	public static bool IsInGuild(Mobile m)
	{
		return m is PlayerMobile mobile && mobile.NpcGuild == NpcGuild.ThievesGuild;
	}

	public static bool IsInnocentTo(Mobile from, Mobile to)
	{
		return Notoriety.Compute(from, to) == Notoriety.Innocent;
	}

	private class StealingTarget : Target
	{
		private readonly Mobile _mThief;

		public StealingTarget(Mobile thief) : base(1, false, TargetFlags.None)
		{
			_mThief = thief;

			AllowNonlocal = true;
		}

		private Item TryStealItem(Item toSteal, ref bool caught)
		{
			Item stolen = null;

			object root = toSteal.RootParent;

			StealableArtifactsSpawner.StealableInstance si = null;
			if (toSteal.Parent == null || !toSteal.Movable)
				si = StealableArtifactsSpawner.GetStealableInstance(toSteal);

			if (!IsEmptyHanded(_mThief))
			{
				_mThief.SendLocalizedMessage(1005584); // Both hands must be free to steal.
			}
			else if (_mThief.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
			{
				_mThief.SendMessage("You may not steal in this area.");
			}
			else if (root is Mobile {Player: true} && !IsInGuild(_mThief))
			{
				_mThief.SendLocalizedMessage(1005596); // You must be in the thieves guild to steal from other players.
			}
			else if (SuspendOnMurder && root is Mobile mobile && mobile.Player && IsInGuild(_mThief) && _mThief.Kills > 0)
			{
				_mThief.SendLocalizedMessage(502706); // You are currently suspended from the thieves guild.
			}
			else switch (root)
			{
				case BaseVendor {IsInvulnerable: true}:
					_mThief.SendLocalizedMessage(1005598); // You can't steal from shopkeepers.
					break;
				case PlayerVendor:
					_mThief.SendLocalizedMessage(502709); // You can't steal from vendors.
					break;
				default:
				{
					if (!_mThief.CanSee(toSteal))
					{
						_mThief.SendLocalizedMessage(500237); // Target can not be seen.
					}
					else if (_mThief.Backpack == null || !_mThief.Backpack.CheckHold(_mThief, toSteal, false, true))
					{
						_mThief.SendLocalizedMessage(1048147); // Your backpack can't hold anything else.
					}
					#region Sigils
					else if (toSteal is Sigil sig)
					{
						PlayerState pl = PlayerState.Find(_mThief);
						Faction faction = pl?.Faction;

						if (!_mThief.InRange(sig.GetWorldLocation(), 1))
						{
							_mThief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
						}
						else if (root != null) // not on the ground
						{
							_mThief.SendLocalizedMessage(502710); // You can't steal that!
						}
						else if (faction != null)
						{
							if (!_mThief.CanBeginAction(typeof(IncognitoSpell)))
							{
								_mThief.SendLocalizedMessage(1010581); //	You cannot steal the sigil when you are incognito
							}
							else if (DisguiseTimers.IsDisguised(_mThief))
							{
								_mThief.SendLocalizedMessage(1010583); //	You cannot steal the sigil while disguised
							}
							else if (!_mThief.CanBeginAction(typeof(PolymorphSpell)))
							{
								_mThief.SendLocalizedMessage(1010582); //	You cannot steal the sigil while polymorphed
							}
							else if (TransformationSpellHelper.UnderTransformation(_mThief))
							{
								_mThief.SendLocalizedMessage(1061622); // You cannot steal the sigil while in that form.
							}
							else if (AnimalForm.UnderTransformation(_mThief))
							{
								_mThief.SendLocalizedMessage(1063222); // You cannot steal the sigil while mimicking an animal.
							}
							else if (pl.IsLeaving)
							{
								_mThief.SendLocalizedMessage(1005589); // You are currently quitting a faction and cannot steal the town sigil
							}
							else if (sig.IsBeingCorrupted && sig.LastMonolith.Faction == faction)
							{
								_mThief.SendLocalizedMessage(1005590); //	You cannot steal your own sigil
							}
							else if (sig.IsPurifying)
							{
								_mThief.SendLocalizedMessage(1005592); // You cannot steal this sigil until it has been purified
							}
							else if (_mThief.CheckTargetSkill(SkillName.Stealing, toSteal, 80.0, 80.0))
							{
								if (Sigil.ExistsOn(_mThief))
								{
									_mThief.SendLocalizedMessage(1010258); //	The sigil has gone back to its home location because you already have a sigil.
								}
								else if (_mThief.Backpack == null || !_mThief.Backpack.CheckHold(_mThief, sig, false, true))
								{
									_mThief.SendLocalizedMessage(1010259); //	The sigil has gone home because your backpack is full
								}
								else
								{
									if (sig.IsBeingCorrupted)
										sig.GraceStart = DateTime.UtcNow; // begin grace period

									_mThief.SendLocalizedMessage(1010586); // YOU STOLE THE SIGIL!!!   (woah, calm down now)

									if (sig.LastMonolith is {Sigil: { }})
									{
										sig.LastMonolith.Sigil = null;
										sig.LastStolen = DateTime.UtcNow;
									}

									return sig;
								}
							}
							else
							{
								_mThief.SendLocalizedMessage(1005594); //	You do not have enough skill to steal the sigil
							}
						}
						else
						{
							_mThief.SendLocalizedMessage(1005588); //	You must join a faction to do that
						}
					}
					#endregion
					else if (si == null && (toSteal.Parent == null || !toSteal.Movable))
					{
						_mThief.SendLocalizedMessage(502710); // You can't steal that!
					}
					else if (toSteal.LootType == LootType.Newbied || toSteal.CheckBlessed(root))
					{
						_mThief.SendLocalizedMessage(502710); // You can't steal that!
					}
					else if (Core.AOS && si == null && toSteal is Container)
					{
						_mThief.SendLocalizedMessage(502710); // You can't steal that!
					}
					else if (!_mThief.InRange(toSteal.GetWorldLocation(), 1))
					{
						_mThief.SendLocalizedMessage(502703); // You must be standing next to an item to steal it.
					}
					else if (si != null && _mThief.Skills[SkillName.Stealing].Value < 100.0)
					{
						_mThief.SendLocalizedMessage(1060025, 0x66D); // You're not skilled enough to attempt the theft of this item.
					}
					else if (toSteal.Parent is Mobile)
					{
						_mThief.SendLocalizedMessage(1005585); // You cannot steal items which are equiped.
					}
					else if (root == _mThief)
					{
						_mThief.SendLocalizedMessage(502704); // You catch yourself red-handed.
					}
					else if (root is Mobile && ((Mobile)root).AccessLevel > AccessLevel.Player)
					{
						_mThief.SendLocalizedMessage(502710); // You can't steal that!
					}
					else if (root is Mobile && !_mThief.CanBeHarmful((Mobile)root))
					{
					}
					else if (root is Corpse)
					{
						_mThief.SendLocalizedMessage(502710); // You can't steal that!
					}
					else
					{
						double w = toSteal.Weight + toSteal.TotalWeight;

						if (w > 10)
						{
							_mThief.SendMessage("That is too heavy to steal.");
						}
						else
						{
							if (toSteal.Stackable && toSteal.Amount > 1)
							{
								int maxAmount = (int)((_mThief.Skills[SkillName.Stealing].Value / 10.0) / toSteal.Weight);

								if (maxAmount < 1)
									maxAmount = 1;
								else if (maxAmount > toSteal.Amount)
									maxAmount = toSteal.Amount;

								int amount = Utility.RandomMinMax(1, maxAmount);

								if (amount >= toSteal.Amount)
								{
									int pileWeight = (int)Math.Ceiling(toSteal.Weight * toSteal.Amount);
									pileWeight *= 10;

									if (_mThief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
										stolen = toSteal;
								}
								else
								{
									int pileWeight = (int)Math.Ceiling(toSteal.Weight * amount);
									pileWeight *= 10;

									if (_mThief.CheckTargetSkill(SkillName.Stealing, toSteal, pileWeight - 22.5, pileWeight + 27.5))
									{
										stolen = Mobile.LiftItemDupe(toSteal, toSteal.Amount - amount) ?? toSteal;
									}
								}
							}
							else
							{
								int iw = (int)Math.Ceiling(w);
								iw *= 10;

								if (_mThief.CheckTargetSkill(SkillName.Stealing, toSteal, iw - 22.5, iw + 27.5))
									stolen = toSteal;
							}

							if (stolen != null)
							{
								_mThief.SendLocalizedMessage(502724); // You successfully steal the item.

								if (si != null)
								{
									toSteal.Movable = true;
									si.Item = null;
								}
							}
							else
							{
								_mThief.SendLocalizedMessage(502723); // You fail to steal the item.
							}

							caught = _mThief.Skills[SkillName.Stealing].Value < Utility.Random(150);
						}
					}

					break;
				}
			}

			return stolen;
		}

		protected override void OnTarget(Mobile from, object target)
		{
			from.RevealingAction();

			Item stolen = null;
			object root = null;
			bool caught = false;

			if (target is Item item)
			{
				root = item.RootParent;
				stolen = TryStealItem(item, ref caught);
			}
			else if (target is Mobile mobile)
			{
				Container pack = mobile.Backpack;

				if (pack != null && pack.Items.Count > 0)
				{
					int randomIndex = Utility.Random(pack.Items.Count);

					root = mobile;
					stolen = TryStealItem(pack.Items[randomIndex], ref caught);
				}
			}
			else
			{
				_mThief.SendLocalizedMessage(502710); // You can't steal that!
			}

			if (stolen != null)
			{
				from.AddToBackpack(stolen);

				if (!(stolen is Container || stolen.Stackable))
				{ // do not return stolen containers or stackable items
					StolenItem.Add(stolen, _mThief, root as Mobile);
				}
			}

			if (caught)
			{
				if (root == null)
				{
					_mThief.CriminalAction(false);
				}
				else if (root is Corpse && ((Corpse)root).IsCriminalAction(_mThief))
				{
					_mThief.CriminalAction(false);
				}
				else if (root is Mobile mobRoot)
				{
					if (!IsInGuild(mobRoot) && IsInnocentTo(_mThief, mobRoot))
						_mThief.CriminalAction(false);

					string message = $"You notice {_mThief.Name} trying to steal from {mobRoot.Name}.";

					foreach (NetState ns in _mThief.GetClientsInRange(8))
					{
						if (ns.Mobile != _mThief)
							ns.Mobile.SendMessage(message);
					}
				}
			}
			else if (root is Corpse corpse && corpse.IsCriminalAction(_mThief))
			{
				_mThief.CriminalAction(false);
			}

			if (root is Mobile {Player: true} root1 && _mThief is PlayerMobile pm && IsInnocentTo(pm, root1) && !IsInGuild(root1))
			{
				pm.PermaFlags.Add(root1);
				pm.Delta(MobileDelta.Noto);
			}
		}
	}

	public static bool IsEmptyHanded(Mobile from)
	{
		if (from.FindItemOnLayer(Layer.OneHanded) != null)
			return false;

		return from.FindItemOnLayer(Layer.TwoHanded) == null;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		if (!IsEmptyHanded(m))
		{
			m.SendLocalizedMessage(1005584); // Both hands must be free to steal.
		}
		else if (m.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone)))
		{
			m.SendMessage("You may not steal in this area.");
		}
		else
		{
			m.Target = new StealingTarget(m);
			m.RevealingAction();

			m.SendLocalizedMessage(502698); // Which item do you want to steal?
		}

		return TimeSpan.FromSeconds(10.0);
	}
}

public class StolenItem
{
	public static readonly TimeSpan StealTime = TimeSpan.FromMinutes(2.0);

	public Item Stolen { get; }
	public Mobile Thief { get; }
	public Mobile Victim { get; }
	public DateTime Expires { get; private set; }

	public bool IsExpired => (DateTime.UtcNow >= Expires);

	public StolenItem(Item stolen, Mobile thief, Mobile victim)
	{
		Stolen = stolen;
		Thief = thief;
		Victim = victim;

		Expires = DateTime.UtcNow + StealTime;
	}

	private static readonly Queue<StolenItem> MQueue = new();

	public static void Add(Item item, Mobile thief, Mobile victim)
	{
		Clean();

		MQueue.Enqueue(new StolenItem(item, thief, victim));
	}

	public static bool IsStolen(Item item)
	{
		Mobile victim = null;

		return IsStolen(item, ref victim);
	}

	public static bool IsStolen(Item item, ref Mobile victim)
	{
		Clean();

		foreach (var si in MQueue.Where(si => si.Stolen == item && !si.IsExpired))
		{
			victim = si.Victim;
			return true;
		}

		return false;
	}

	public static void ReturnOnDeath(Mobile killed, Container corpse)
	{
		Clean();

		foreach (var si in MQueue.Where(si => si.Stolen.RootParent == corpse && si.Victim != null && !si.IsExpired))
		{
			// the item that was stolen is returned to you.// the item that was stolen from you falls to the ground.
			si.Victim.SendLocalizedMessage(si.Victim.AddToBackpack(si.Stolen) ? 1010464 : 1010463);

			si.Expires = DateTime.UtcNow;
		}
	}

	public static void Clean()
	{
		while (MQueue.Count > 0)
		{
			StolenItem si = MQueue.Peek();

			if (si.IsExpired)
				MQueue.Dequeue();
			else
				break;
		}
	}
}
