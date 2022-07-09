using Server.Commands;
using Server.Engines.PartySystem;
using Server.Factions;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Second;
using Server.Spells.Seventh;
using Server.Spells.Spellweaving;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.ConPVP;

public delegate void CountdownCallback(int count);

public class DuelContext
{
	public bool Rematch { get; private init; }
	public bool ReadyWait { get; private set; }
	public int ReadyCount { get; private set; }
	public bool Registered { get; private set; } = true;
	public bool Finished { get; private set; }
	public bool Started { get; private set; }

	public Mobile Initiator { get; }
	public ArrayList Participants { get; }
	public Ruleset Ruleset { get; private init; }
	public Arena Arena { get; private set; }

	private bool CantDoAnything(Mobile mob)
	{
		return EventGame != null && EventGame.CantDoAnything(mob);
	}

	public static bool IsFreeConsume(Mobile mob)
	{
		if (mob is not PlayerMobile pm || pm.DuelContext == null || pm.DuelContext.EventGame == null)
			return false;

		return pm.DuelContext.EventGame.FreeConsume;
	}

	public void DelayBounce(TimeSpan ts, Mobile mob, Container corpse)
	{
		Timer.DelayCall(ts, new TimerStateCallback(DelayBounce_Callback), new object[] { mob, corpse });
	}

	public static bool AllowSpecialMove(Mobile from, string name, SpecialMove move)
	{
		if (from is not PlayerMobile pm)
			return true;

		DuelContext dc = pm.DuelContext;

		return dc == null || dc.InstAllowSpecialMove(from, name, move);
	}

	public bool InstAllowSpecialMove(Mobile from, string name, SpecialMove move)
	{

		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (CantDoAnything(from))
			return false;

		string title = move switch
		{
			NinjaMove => "Bushido",
			SamuraiMove => "Ninjitsu",
			_ => null
		};

		if (title == null || name == null || Ruleset.GetOption(title, name))
			return true;

		from.SendMessage("The dueling ruleset prevents you from using this move.");
		return false;
	}

	public bool AllowSpellCast(Mobile from, Spell spell)
	{
		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (CantDoAnything(from))
			return false;

		if (spell is RecallSpell)
			from.SendMessage("You may not cast this spell.");

		string title = null;
		string option;

		switch (spell)
		{
			case ArcanistSpell:
				title = "Spellweaving";
				option = spell.Name;
				break;
			case PaladinSpell:
				title = "Chivalry";
				option = spell.Name;
				break;
			case NecromancerSpell:
				title = "Necromancy";
				option = spell.Name;
				break;
			case NinjaSpell:
				title = "Ninjitsu";
				option = spell.Name;
				break;
			case SamuraiSpell:
				title = "Bushido";
				option = spell.Name;
				break;
			case MagerySpell magerySpell:
				title = magerySpell.Circle switch
				{
					SpellCircle.First => "1st Circle",
					SpellCircle.Second => "2nd Circle",
					SpellCircle.Third => "3rd Circle",
					SpellCircle.Fourth => "4th Circle",
					SpellCircle.Fifth => "5th Circle",
					SpellCircle.Sixth => "6th Circle",
					SpellCircle.Seventh => "7th Circle",
					SpellCircle.Eighth => "8th Circle",
					_ => title
				};

				option = magerySpell.Name;
				break;
			default:
				title = "Other Spell";
				option = spell.Name;
				break;
		}

		if (title == null || option == null || Ruleset.GetOption(title, option))
			return true;

		from.SendMessage("The dueling ruleset prevents you from casting this spell.");
		return false;
	}

	public bool AllowItemEquip(Mobile from, Item item)
	{
		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (item is Dagger || CheckItemEquip(from, item))
			return true;

		from.SendMessage("The dueling ruleset prevents you from equiping this item.");
		return false;
	}

	public static bool AllowSpecialAbility(Mobile from, string name, bool message)
	{
		if (from is not PlayerMobile pm)
			return true;

		DuelContext dc = pm.DuelContext;

		return dc == null || dc.InstAllowSpecialAbility(from, name, message);
	}

	public bool InstAllowSpecialAbility(Mobile from, string name, bool message)
	{
		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (CantDoAnything(from))
			return false;

		if (Ruleset.GetOption("Combat Abilities", name))
			return true;

		if (message)
			from.SendMessage("The dueling ruleset prevents you from using this combat ability.");

		return false;
	}

	public bool CheckItemEquip(Mobile from, Item item)
	{
		switch (item)
		{
			case Fists when !Ruleset.GetOption("Weapons", "Wrestling"):
			case BaseArmor armor when (armor.ProtectionLevel > ArmorProtectionLevel.Regular && !Ruleset.GetOption("Armor", "Magical")) || (!Core.AOS && armor.Resource != armor.DefaultResource && !Ruleset.GetOption("Armor", "Colored")) || (armor is BaseShield && !Ruleset.GetOption("Armor", "Shields")):
			case BaseWeapon weapon when ((weapon.DamageLevel > WeaponDamageLevel.Regular || weapon.AccuracyLevel > WeaponAccuracyLevel.Regular) && !Ruleset.GetOption("Weapons", "Magical")) || (!Core.AOS && weapon.Resource != CraftResource.Iron && weapon.Resource != CraftResource.None && !Ruleset.GetOption("Weapons", "Runics")) || (weapon is BaseRanged && !Ruleset.GetOption("Weapons", "Ranged")) || (weapon is not BaseRanged && !Ruleset.GetOption("Weapons", "Melee")) || (weapon.PoisonCharges > 0 && weapon.Poison != null && !Ruleset.GetOption("Weapons", "Poisoned")) || (weapon is BaseWand && !Ruleset.GetOption("Items", "Wands")):
				return false;
			default:
				return true;
		}
	}

	public bool AllowSkillUse(Mobile from, SkillName skill)
	{
		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (CantDoAnything(from))
			return false;

		int id = (int)skill;

		if (id >= 0 && id < SkillInfo.Table.Length)
		{
			if (Ruleset.GetOption("Skills", SkillInfo.Table[id].Name))
				return true;
		}

		from.SendMessage("The dueling ruleset prevents you from using this skill.");
		return false;
	}

	public bool AllowItemUse(Mobile from, Item item)
	{
		if (!StartedBeginCountdown)
			return true;

		DuelPlayer pl = Find(from);

		if (pl == null || pl.Eliminated)
			return true;

		if (item is not BaseRefreshPotion)
		{
			if (CantDoAnything(from))
				return false;
		}

		string title = null, option = null;

		if (item is BasePotion)
		{
			title = "Potions";

			switch (item)
			{
				case BaseAgilityPotion:
					option = "Agility";
					break;
				case BaseCurePotion:
					option = "Cure";
					break;
				case BaseHealPotion:
					option = "Heal";
					break;
				case NightSightPotion:
					option = "Nightsight";
					break;
				case BasePoisonPotion:
					option = "Poison";
					break;
				case BaseStrengthPotion:
					option = "Strength";
					break;
				case BaseExplosionPotion:
					option = "Explosion";
					break;
				case BaseRefreshPotion:
					option = "Refresh";
					break;
			}
		}
		else if (item is Bandage)
		{
			title = "Items";
			option = "Bandages";
		}
		else if (item is TrapableContainer container)
		{
			if (container.TrapType != TrapType.None)
			{
				title = "Items";
				option = "Trapped Containers";
			}
		}
		else if (item is Bola)
		{
			title = "Items";
			option = "Bolas";
		}
		else if (item is OrangePetals)
		{
			title = "Items";
			option = "Orange Petals";
		}
		else if (item is EtherealMount || item.Layer == Layer.Mount)
		{
			title = "Items";
			option = "Mounts";
		}
		else switch (item)
		{
			case LeatherNinjaBelt:
				title = "Items";
				option = "Shurikens";
				break;
			case Fukiya:
				title = "Items";
				option = "Fukiya Darts";
				break;
			case FireHorn:
				title = "Items";
				option = "Fire Horns";
				break;
			case BaseWand:
				title = "Items";
				option = "Wands";
				break;
		}

		if (title != null && option != null && StartedBeginCountdown && !Started)
		{
			from.SendMessage("You may not use this item before the duel begins.");
			return false;
		}

		switch (item)
		{
			case BasePotion and not BaseExplosionPotion and not BaseRefreshPotion when IsSuddenDeath:
				from.SendMessage(0x22, "You may not drink potions in sudden death.");
				return false;
			case Bandage when IsSuddenDeath:
				from.SendMessage(0x22, "You may not use bandages in sudden death.");
				return false;
		}

		if (title == null || option == null || Ruleset.GetOption(title, option))
			return true;

		from.SendMessage("The dueling ruleset prevents you from using this item.");
		return false;
	}

	private void DelayBounce_Callback(object state)
	{
		object[] states = (object[])state;
		Mobile mob = (Mobile)states[0];
		Container corpse = (Container)states[1];

		RemoveAggressions(mob);
		SendOutside(mob);
		Refresh(mob, corpse);
		Debuff(mob);
		CancelSpell(mob);
		mob.Frozen = false;
	}

	public void OnMapChanged(Mobile mob)
	{
		OnLocationChanged(mob);
	}

	public void OnLocationChanged(Mobile mob)
	{
		if (!Registered || !StartedBeginCountdown || Finished)
			return;

		Arena arena = Arena;

		if (arena == null || (mob.Map == arena.Facet && arena.Bounds.Contains(mob.Location)))
			return;

		DuelPlayer pl = Find(mob);

		if (pl == null || pl.Eliminated)
			return;

		if (mob.Map == Map.Internal)
		{
			// they've logged out

			if (mob.LogoutMap == arena.Facet && arena.Bounds.Contains(mob.LogoutLocation))
			{
				// they logged out inside the arena.. set them to eject on login

				mob.LogoutLocation = arena.Outside;
			}
		}

		pl.Eliminated = true;

		mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have forfeited your position in the duel.");
		mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false,
			$"{mob.Name} has forfeited by leaving the dueling arena.");

		Participant winner = CheckCompletion();

		if (winner != null)
			Finish(winner);
	}

	private bool _yielding;

	public void OnDeath(Mobile mob, Container corpse)
	{
		if (!Registered || !Started)
			return;

		DuelPlayer pl = Find(mob);

		if (pl is {Eliminated: false})
		{
			if (EventGame != null && !EventGame.OnDeath(mob, corpse))
				return;

			pl.Eliminated = true;

			if (mob.Poison != null)
				mob.Poison = null;

			Requip(mob, corpse);
			DelayBounce(TimeSpan.FromSeconds(4.0), mob, corpse);

			Participant winner = CheckCompletion();

			if (winner != null)
			{
				Finish(winner);
			}
			else if (!_yielding)
			{
				mob.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have been defeated.");
				mob.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, $"{mob.Name} has been defeated.");
			}
		}
	}

	public bool CheckFull()
	{
		return Participants.Cast<Participant>().All(p => !p.HasOpenSlot);
	}

	public void Requip(Mobile from, Container cont)
	{
		if (cont is not Corpse corpse)
			return;

		List<Item> items = new(corpse.Items);

		bool gathered = false;
		bool didntFit = false;

		Container pack = from.Backpack;

		for (int i = 0; !didntFit && i < items.Count; ++i)
		{
			Item item = items[i];
			_ = item.Location;

			if (item.Layer is Layer.Hair or Layer.FacialHair || !item.Movable)
				continue;

			if (pack != null)
			{
				pack.DropItem(item);
				gathered = true;
			}
			else
			{
				didntFit = true;
			}
		}

		corpse.Carved = true;

		if (corpse.ItemId == 0x2006)
		{
			corpse.ProcessDelta();
			corpse.SendRemovePacket();
			corpse.ItemId = Utility.Random(0xECA, 9); // bone graphic
			corpse.Hue = 0;
			corpse.ProcessDelta();

			Mobile killer = from.FindMostRecentDamager(false);

			if (killer is {Player: true})
				killer.AddToBackpack(new Head(from, _Tournament == null ? HeadType.Duel : HeadType.Tournament, from.Name));
		}

		from.PlaySound(0x3E3);

		if (gathered)
		{
			from.SendLocalizedMessage(1062471); // You quickly gather all of your belongings.
		}
	}

	public static void Refresh(Mobile mob, Container cont)
	{
		if (!mob.Alive)
		{
			mob.Resurrect();


			if (mob.FindItemOnLayer(Layer.OuterTorso) is DeathRobe robe)
				robe.Delete();

			if (cont is Corpse corpse)
			{
				for (int i = 0; i < corpse.EquipItems.Count; ++i)
				{
					Item item = corpse.EquipItems[i];

					if (item.Movable && item.Layer != Layer.Hair && item.Layer != Layer.FacialHair && item.IsChildOf(mob.Backpack))
						mob.EquipItem(item);
				}
			}
		}

		mob.Hits = mob.HitsMax;
		mob.Stam = mob.StamMax;
		mob.Mana = mob.ManaMax;

		mob.Poison = null;
	}

	public void SendOutside(Mobile mob)
	{
		if (Arena == null)
			return;

		mob.Combatant = null;
		mob.MoveToWorld(Arena.Outside, Arena.Facet);
	}

	private Point3D _gatePoint;
	private Map _gateFacet;

	public void Finish(Participant winner)
	{
		if (Finished)
			return;

		EndAutoTie();
		StopSdTimers();

		Finished = true;

		for (int i = 0; i < winner.Players.Length; ++i)
		{
			DuelPlayer pl = winner.Players[i];

			if (pl is {Eliminated: false})
				DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
		}

		winner.Broadcast(0x59, null, winner.Players.Length == 1 ? "{0} has won the duel." : "{0} and {1} team have won the duel.", winner.Players.Length == 1 ? "You have won the duel." : "Your team has won the duel.");

		if (_Tournament != null && winner.TournyPart != null)
		{
			Match.Winner = winner.TournyPart;
			winner.TournyPart.WonMatch(Match);
			_Tournament.HandleWon(Arena, Match, winner.TournyPart);
		}

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant loser = (Participant)Participants[i];

			if (loser != winner)
			{
				loser.Broadcast(0x22, null, loser.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", loser.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");

				if (_Tournament != null && loser.TournyPart != null)
					loser.TournyPart.LostMatch(Match);
			}

			for (int j = 0; j < loser.Players.Length; ++j)
			{
				if (loser.Players[j] != null)
				{
					RemoveAggressions(loser.Players[j].Mobile);
					loser.Players[j].Mobile.Delta(MobileDelta.Noto);
					loser.Players[j].Mobile.CloseGump(typeof(BeginGump));

					if (_Tournament != null)
						loser.Players[j].Mobile.SendEverything();
				}
			}
		}

		if (IsOneVsOne)
		{
			DuelPlayer dp1 = ((Participant)Participants[0])?.Players[0];
			DuelPlayer dp2 = ((Participant)Participants[1])?.Players[0];

			if (dp1 != null && dp2 != null)
			{
				Award(dp1.Mobile, dp2.Mobile, dp1.Participant == winner);
				Award(dp2.Mobile, dp1.Mobile, dp2.Participant == winner);
			}
		}

		EventGame?.OnStop();

		Timer.DelayCall(TimeSpan.FromSeconds(9.0), UnregisterRematch);
	}

	public void Award(Mobile us, Mobile them, bool won)
	{
		Ladder ladder = Arena == null ? Ladder.Instance : Arena.AcquireLadder();

		if (ladder == null)
			return;

		LadderEntry ourEntry = ladder.Find(us);
		LadderEntry theirEntry = ladder.Find(them);

		if (ourEntry == null || theirEntry == null)
			return;

		int xpGain = Ladder.GetExperienceGain(ourEntry, theirEntry, won);

		if (xpGain == 0)
			return;

		if (_Tournament != null)
			xpGain *= xpGain > 0 ? 5 : 2;

		if (won)
			++ourEntry.Wins;
		else
			++ourEntry.Losses;

		int oldLevel = Ladder.GetLevel(ourEntry.Experience);

		ourEntry.Experience += xpGain;

		if (ourEntry.Experience < 0)
			ourEntry.Experience = 0;

		ladder.UpdateEntry(ourEntry);

		int newLevel = Ladder.GetLevel(ourEntry.Experience);

		if (newLevel > oldLevel)
			us.SendMessage(0x59, "You have achieved level {0}!", newLevel);
		else if (newLevel < oldLevel)
			us.SendMessage(0x22, "You have lost a level. You are now at {0}.", newLevel);
	}

	public void UnregisterRematch()
	{
		Unregister(true);
	}

	public void Unregister()
	{
		Unregister(false);
	}

	public void Unregister(bool queryRematch)
	{
		DestroyWall();

		if (!Registered)
			return;

		Registered = false;

		Arena?.Evict();

		StopSdTimers();

		Type[] types = { typeof(BeginGump), typeof(DuelContextGump), typeof(ParticipantGump), typeof(PickRulesetGump), typeof(ReadyGump), typeof(ReadyUpGump), typeof(RulesetGump) };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null)
					continue;

				if (pl.Mobile is PlayerMobile mobile)
					mobile.DuelPlayer = null;

				for (int k = 0; k < types.Length; ++k)
					pl.Mobile.CloseGump(types[k]);
			}
		}

		if (queryRematch && _Tournament == null)
			QueryRematch();
	}

	public void QueryRematch()
	{
		DuelContext dc = new(Initiator, Ruleset.Layout, false)
		{
			Ruleset = Ruleset,
			Rematch = true
		};

		dc.Participants.Clear();

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant oldPart = (Participant)Participants[i];
			if (oldPart != null)
			{
				Participant newPart = new(dc, oldPart.Players.Length);

				for (int j = 0; j < oldPart.Players.Length; ++j)
				{
					DuelPlayer oldPlayer = oldPart.Players[j];

					if (oldPlayer != null)
						newPart.Players[j] = new DuelPlayer(oldPlayer.Mobile, newPart);
				}

				dc.Participants.Add(newPart);
			}
		}

		dc.CloseAllGumps();
		dc.SendReadyUpGump();
	}

	public DuelPlayer Find(Mobile mob)
	{
		if (mob is PlayerMobile pm)
		{
			return pm.DuelContext == this ? pm.DuelPlayer : null;
		}

		return (from Participant p in Participants select p.Find(mob)).FirstOrDefault(pl => pl != null);
	}

	public bool IsAlly(Mobile m1, Mobile m2)
	{
		DuelPlayer pl1 = Find(m1);
		DuelPlayer pl2 = Find(m2);

		return pl1 != null && pl2 != null && pl1.Participant == pl2.Participant;
	}

	public Participant CheckCompletion()
	{
		Participant winner = null;

		bool hasWinner = false;
		int eliminated = 0;

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			if (p.Eliminated)
			{
				++eliminated;

				if (eliminated == Participants.Count - 1)
					hasWinner = true;
			}
			else
			{
				winner = p;
			}
		}

		if (hasWinner)
			return winner ?? (Participant)Participants[0];

		return null;
	}

	private Timer _countdown;

	public void StartCountdown(int count, CountdownCallback cb)
	{
		cb(count);
		_countdown = Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), count, new TimerStateCallback(Countdown_Callback), new object[] { count - 1, cb });
	}

	public void StopCountdown()
	{
		_countdown?.Stop();

		_countdown = null;
	}

	private void Countdown_Callback(object state)
	{
		object[] states = (object[])state;

		int count = (int)states[0];
		CountdownCallback cb = (CountdownCallback)states[1];

		if (count == 0)
		{
			_countdown?.Stop();

			_countdown = null;
		}

		cb(count);

		states[0] = count - 1;
	}

	private Timer _autoTieTimer;

	public bool Tied { get; private set; }
	public bool IsSuddenDeath { get; set; }

	private Timer _sdWarnTimer, _sdActivateTimer;

	public void StopSdTimers()
	{
		_sdWarnTimer?.Stop();

		_sdWarnTimer = null;

		_sdActivateTimer?.Stop();

		_sdActivateTimer = null;
	}

	public void StartSuddenDeath(TimeSpan timeUntilActive)
	{
		_sdWarnTimer?.Stop();

		_sdWarnTimer = Timer.DelayCall(TimeSpan.FromMinutes(timeUntilActive.TotalMinutes * 0.9), WarnSuddenDeath);

		_sdActivateTimer?.Stop();

		_sdActivateTimer = Timer.DelayCall(timeUntilActive, ActivateSuddenDeath);
	}

	public void WarnSuddenDeath()
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null || pl.Eliminated)
					continue;

				pl.Mobile.SendSound(0x1E1);
				pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
				pl.Mobile.SendMessage(0x22, "Sudden death will be active soon!");
			}
		}

		_Tournament?.Alert(Arena, "Sudden death will be active soon!");

		_sdWarnTimer?.Stop();

		_sdWarnTimer = null;
	}

	public static bool CheckSuddenDeath(Mobile mob)
	{
		if (mob is PlayerMobile pm)
		{
			if (pm.DuelPlayer is {Eliminated: false} && pm.DuelContext != null && pm.DuelContext.IsSuddenDeath)
				return true;
		}

		return false;
	}

	public void ActivateSuddenDeath()
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null || pl.Eliminated)
					continue;

				pl.Mobile.SendSound(0x1E1);
				pl.Mobile.SendMessage(0x22, "Warning! Warning! Warning!");
				pl.Mobile.SendMessage(0x22, "Sudden death has ACTIVATED. You are now unable to perform any beneficial actions.");
			}
		}

		_Tournament?.Alert(Arena, "Sudden death has been activated!");

		IsSuddenDeath = true;

		_sdActivateTimer?.Stop();

		_sdActivateTimer = null;
	}

	public void BeginAutoTie()
	{
		_autoTieTimer?.Stop();

		TimeSpan ts = _Tournament == null || _Tournament.TournyType == TournyType.Standard
			? AutoTieDelay
			: TimeSpan.FromMinutes(90.0);

		_autoTieTimer = Timer.DelayCall(ts, InvokeAutoTie);
	}

	public void EndAutoTie()
	{
		_autoTieTimer?.Stop();

		_autoTieTimer = null;
	}

	public void InvokeAutoTie()
	{
		_autoTieTimer = null;

		if (!Started || Finished)
			return;

		Tied = true;
		Finished = true;

		StopSdTimers();

		ArrayList remaining = new();

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			if (p.Eliminated)
			{
				p.Broadcast(0x22, null, p.Players.Length == 1 ? "{0} has lost the duel." : "{0} and {1} team have lost the duel.", p.Players.Length == 1 ? "You have lost the duel." : "Your team has lost the duel.");
			}
			else
			{
				p.Broadcast(0x59, null, p.Players.Length == 1 ? "{0} has tied the duel due to time expiration." : "{0} and {1} team have tied the duel due to time expiration.", p.Players.Length == 1 ? "You have tied the duel due to time expiration." : "Your team has tied the duel due to time expiration.");

				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer pl = p.Players[j];

					if (pl is {Eliminated: false})
						DelayBounce(TimeSpan.FromSeconds(8.0), pl.Mobile, null);
				}

				if (p.TournyPart != null)
					remaining.Add(p.TournyPart);
			}

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl != null)
				{
					pl.Mobile.Delta(MobileDelta.Noto);
					pl.Mobile.SendEverything();
				}
			}
		}

		_Tournament?.HandleTie(Arena, Match, remaining);

		Timer.DelayCall(TimeSpan.FromSeconds(10.0), Unregister);
	}

	public bool IsOneVsOne => Participants.Count == 2 && ((Participant)Participants[0])!.Players.Length == 1 && ((Participant)Participants[1])!.Players.Length == 1;

	public static void Initialize()
	{
		EventSink.OnSpeech += EventSink_Speech;
		EventSink.OnLogin += EventSink_Login;

		CommandSystem.Register("vli", AccessLevel.GameMaster, Vli_oc);
	}

	private static void Vli_oc(CommandEventArgs e)
	{
		e.Mobile.BeginTarget(-1, false, Targeting.TargetFlags.None, Vli_ot);
	}

	private static void Vli_ot(Mobile from, object obj)
	{
		if (obj is PlayerMobile pm)
		{
			if (Ladder.Instance == null)
				return;

			LadderEntry entry = Ladder.Instance.Find(pm);

			if (entry != null)
				from.SendGump(new PropertiesGump(from, entry));
		}
	}

	private static readonly TimeSpan CombatDelay = TimeSpan.FromSeconds(30.0);
	private static readonly TimeSpan AutoTieDelay = TimeSpan.FromMinutes(15.0);

	public static bool CheckCombat(Mobile m)
	{
		if (m.Aggressed.Any(info => info.Defender.Player && DateTime.UtcNow - info.LastCombatTime < CombatDelay))
		{
			return true;
		}

		return m.Aggressors.Any(info => info.Attacker.Player && DateTime.UtcNow - info.LastCombatTime < CombatDelay);
	}

	private static void EventSink_Login(Mobile m)
	{
		if (m is not PlayerMobile pm)
			return;

		DuelContext dc = pm.DuelContext;

		if (dc == null)
			return;

		switch (dc.ReadyWait)
		{
			case true when pm.DuelPlayer.Ready && !dc.Started && !dc.StartedBeginCountdown && !dc.Finished:
			{
				if (dc._Tournament == null)
					pm.SendGump(new ReadyGump(pm, dc, dc.ReadyCount));
				break;
			}
			case true when !dc.StartedBeginCountdown && !dc.Started && !dc.Finished:
			{
				if (dc._Tournament == null)
					pm.SendGump(new ReadyUpGump(pm, dc));
				break;
			}
			default:
			{
				if (dc.Initiator == pm && !dc.ReadyWait && !dc.StartedBeginCountdown && !dc.Started && !dc.Finished)
					pm.SendGump(new DuelContextGump(pm, dc));
				break;
			}
		}
	}

	private static void ViewLadder_OnTarget(Mobile from, object obj, object state)
	{
		switch (obj)
		{
			case PlayerMobile pm:
			{
				Ladder ladder = (Ladder)state;

				LadderEntry entry = ladder.Find(pm);

				if (entry == null)
					return; // sanity

				var text =
					$"{{0}} are ranked {LadderGump.Rank(entry.Index + 1)} at level {Ladder.GetLevel(entry.Experience)}.";

				pm.PrivateOverheadMessage(MessageType.Regular, pm.SpeechHue, true, string.Format(text, from == pm ? "You" : "They"), from.NetState);
				break;
			}
			case Mobile mob when mob.Body.IsHuman:
				mob.PrivateOverheadMessage(MessageType.Regular, mob.SpeechHue, false, "I'm not a duelist, and quite frankly, I resent the implication.", from.NetState);
				break;
			case Mobile mob:
				mob.PrivateOverheadMessage(MessageType.Regular, 0x3B2, true, "It's probably better than you.", from.NetState);
				break;
			default:
				from.SendMessage("That's not a player.");
				break;
		}
	}

	private static void EventSink_Speech(SpeechEventArgs e)
	{
		if (e.Handled)
			return;


		if (e.Mobile is not PlayerMobile pm)
			return;

		if (Insensitive.Contains(e.Speech, "i wish to duel"))
		{
			if (!pm.CheckAlive())
			{
			}
			else if (pm.Region.IsPartOf(typeof(Regions.Jail)))
			{
			}
			else if (CheckCombat(pm))
			{
				e.Mobile.SendMessage(0x22, "You have recently been in combat with another player and must wait before starting a duel.");
			}
			else if (pm.DuelContext != null)
			{
				e.Mobile.SendMessage(0x22,
					pm.DuelContext.Initiator == pm
						? "You have already started a duel."
						: "You have already been challenged in a duel.");
			}
			else if (TournamentController.IsActive)
			{
				e.Mobile.SendMessage(0x22, "You may not start a duel while a tournament is active.");
			}
			else
			{
				pm.SendGump(new DuelContextGump(pm, new DuelContext(pm, RulesetLayout.Root)));
				e.Handled = true;
			}
		}
		else if (Insensitive.Equals(e.Speech, "change arena preferences"))
		{
			if (!pm.CheckAlive())
			{
			}
			else
			{
				Preferences prefs = Preferences.Instance;

				if (prefs != null)
				{
					e.Mobile.CloseGump(typeof(PreferencesGump));
					e.Mobile.SendGump(new PreferencesGump(e.Mobile, prefs));
				}
			}
		}
		else if (Insensitive.Equals(e.Speech, "showladder"))
		{
			e.Blocked = true;
			if (!pm.CheckAlive())
			{
			}
			else
			{
				Ladder instance = Ladder.Instance;

				if (instance == null)
				{
					//pm.SendMessage( "Ladder not yet initialized." );
				}
				else
				{
					LadderEntry entry = instance.Find(pm);

					if (entry == null)
						return; // sanity

					string text =
						$"{{0}} {{1}} ranked {LadderGump.Rank(entry.Index + 1)} at level {Ladder.GetLevel(entry.Experience)}.";

					pm.LocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, string.Format(text, "You", "are"));
					pm.NonlocalOverheadMessage(MessageType.Regular, pm.SpeechHue, true, string.Format(text, pm.Name, "is"));

					//pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
					//pm.PublicOverheadMessage( MessageType.Regular, pm.SpeechHue, true, String.Format( "Level {0} with {1} win{2} and {3} loss{4}.", Ladder.GetLevel( entry.Experience ), entry.Wins, entry.Wins==1?"":"s", entry.Losses, entry.Losses==1?"":"es" ) );
				}
			}
		}
		else if (Insensitive.Equals(e.Speech, "viewladder"))
		{
			e.Blocked = true;

			if (!pm.CheckAlive())
			{
			}
			else
			{
				Ladder instance = Ladder.Instance;

				if (instance == null)
				{
					//pm.SendMessage( "Ladder not yet initialized." );
				}
				else
				{
					pm.SendMessage("Target a player to view their ranking and level.");
					pm.BeginTarget(16, false, Targeting.TargetFlags.None, new TargetStateCallback(ViewLadder_OnTarget), instance);
				}
			}
		}
		else if (Insensitive.Contains(e.Speech, "i yield"))
		{
			if (!pm.CheckAlive())
			{
			}
			else if (pm.DuelContext == null)
			{
			}
			else if (pm.DuelContext.Finished)
			{
				e.Mobile.SendMessage(0x22, "The duel is already finished.");
			}
			else if (!pm.DuelContext.Started)
			{
				DuelContext dc = pm.DuelContext;
				Mobile init = dc.Initiator;

				if (pm.DuelContext.StartedBeginCountdown)
				{
					e.Mobile.SendMessage(0x22, "The duel has not yet started.");
				}
				else
				{
					DuelPlayer pl = pm.DuelContext.Find(pm);

					if (pl == null)
						return;

					Participant p = pl.Participant;

					if (!pm.DuelContext.ReadyWait) // still setting stuff up
					{
						p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

						if (init == pm)
						{
							dc.Unregister();
						}
						else
						{
							p.Nullify(pl);
							pm.DuelPlayer = null;

							NetState ns = init.NetState;

							if (ns != null)
							{
								foreach (Gump g in ns.Gumps)
								{
									if (g is ParticipantGump pg)
									{
										if (pg.Participant == p)
										{
											init.SendGump(new ParticipantGump(init, dc, p));
											break;
										}
									}
									else if (g is DuelContextGump dcg)
									{
										if (dcg.Context == dc)
										{
											init.SendGump(new DuelContextGump(init, dc));
											break;
										}
									}
								}
							}
						}
					}
					else if (!pm.DuelContext.StartedReadyCountdown) // at ready stage
					{
						p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

						dc._yielding = true;
						dc.RejectReady(pm, null);
						dc._yielding = false;

						if (init == pm)
						{
							dc.Unregister();
						}
						else if (dc.Registered)
						{
							p.Nullify(pl);
							pm.DuelPlayer = null;

							NetState ns = init.NetState;

							if (ns != null)
							{
								bool send = true;

								foreach (Gump g in ns.Gumps)
								{
									if (g is ParticipantGump pg)
									{
										if (pg.Participant == p)
										{
											init.SendGump(new ParticipantGump(init, dc, p));
											send = false;
											break;
										}
									}
									else if (g is DuelContextGump dcg)
									{
										if (dcg.Context == dc)
										{
											init.SendGump(new DuelContextGump(init, dc));
											send = false;
											break;
										}
									}
								}

								if (send)
									init.SendGump(new DuelContextGump(init, dc));
							}
						}
					}
					else
					{
						if (pm.DuelContext._countdown != null)
							pm.DuelContext._countdown.Stop();
						pm.DuelContext._countdown = null;

						pm.DuelContext.StartedReadyCountdown = false;
						p.Broadcast(0x22, null, "{0} has yielded.", "You have yielded.");

						dc._yielding = true;
						dc.RejectReady(pm, null);
						dc._yielding = false;

						if (init == pm)
						{
							dc.Unregister();
						}
						else if (dc.Registered)
						{
							p.Nullify(pl);
							pm.DuelPlayer = null;

							NetState ns = init.NetState;

							if (ns != null)
							{
								bool send = true;

								foreach (Gump g in ns.Gumps)
								{
									if (g is ParticipantGump pg)
									{
										if (pg.Participant == p)
										{
											init.SendGump(new ParticipantGump(init, dc, p));
											send = false;
											break;
										}
									}
									else if (g is DuelContextGump gump)
									{
										if (gump.Context == dc)
										{
											init.SendGump(new DuelContextGump(init, dc));
											send = false;
											break;
										}
									}
								}

								if (send)
									init.SendGump(new DuelContextGump(init, dc));
							}
						}
					}
				}
			}
			else
			{
				DuelPlayer pl = pm.DuelContext.Find(pm);

				if (pl != null)
				{
					if (pm.DuelContext.IsOneVsOne)
					{
						e.Mobile.SendMessage(0x22, "You may not yield a 1 on 1 match.");
					}
					else if (pl.Eliminated)
					{
						e.Mobile.SendMessage(0x22, "You have already been eliminated.");
					}
					else
					{
						pm.LocalOverheadMessage(MessageType.Regular, 0x22, false, "You have yielded.");
						pm.NonlocalOverheadMessage(MessageType.Regular, 0x22, false, $"{pm.Name} has yielded.");

						pm.DuelContext._yielding = true;
						pm.Kill();
						pm.DuelContext._yielding = false;

						if (pm.Alive) // invul, ...
						{
							pl.Eliminated = true;

							pm.DuelContext.RemoveAggressions(pm);
							pm.DuelContext.SendOutside(pm);
							Refresh(pm, null);
							Debuff(pm);
							CancelSpell(pm);
							pm.Frozen = false;

							Participant winner = pm.DuelContext.CheckCompletion();

							if (winner != null)
								pm.DuelContext.Finish(winner);
						}
					}
				}
				else
				{
					e.Mobile.SendMessage(0x22, "BUG: Unable to find duel context.");
				}
			}
		}
	}

	public DuelContext(Mobile initiator, RulesetLayout layout) : this(initiator, layout, true)
	{
	}

	public DuelContext(Mobile initiator, RulesetLayout layout, bool addNew)
	{
		Initiator = initiator;
		Participants = new ArrayList();
		Ruleset = new Ruleset(layout);
		Ruleset.ApplyDefault(layout.Defaults[0]);

		if (addNew)
		{
			Participants.Add(new Participant(this, 1));
			Participants.Add(new Participant(this, 1));

			((Participant)Participants[0])?.Add(initiator);
		}
	}

	public void CloseAllGumps()
	{
		Type[] types = { typeof(DuelContextGump), typeof(ParticipantGump), typeof(RulesetGump) };
		_ = new[] { -1, -1, -1 };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			if (p != null)
			{
				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer pl = p.Players[j];

					if (pl == null)
						continue;

					Mobile mob = pl.Mobile;

					for (int k = 0; k < types.Length; ++k)
						mob.CloseGump(types[k]);
					//mob.CloseGump( types[k], defs[k] );
				}
			}
		}
	}

	public void RejectReady(Mobile rejector, string page)
	{
		if (StartedReadyCountdown)
			return; // sanity

		Type[] types = { typeof(DuelContextGump), typeof(ReadyUpGump), typeof(ReadyGump) };
		_ = new[] { -1, -1, -1 };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null)
					continue;

				pl.Ready = false;

				Mobile mob = pl.Mobile;

				if (page == null) // yield
				{
					if (mob != rejector)
						mob.SendMessage(0x22, "{0} has yielded.", rejector.Name);
				}
				else
				{
					if (mob == rejector)
						mob.SendMessage(0x22, "You have rejected the {0}.", Rematch ? "rematch" : page);
					else
						mob.SendMessage(0x22, "{0} has rejected the {1}.", rejector.Name, Rematch ? "rematch" : page);
				}

				for (int k = 0; k < types.Length; ++k)
					mob.CloseGump(types[k]);
				//mob.CloseGump( types[k], defs[k] );
			}
		}

		if (Rematch)
			Unregister();
		else if (!_yielding)
			Initiator.SendGump(new DuelContextGump(Initiator, this));

		ReadyWait = false;
		ReadyCount = 0;
	}

	public void SendReadyGump()
	{
		SendReadyGump(-1);
	}

	public static void Debuff(Mobile mob)
	{
		mob.RemoveStatMod("[Magic] Str Offset");
		mob.RemoveStatMod("[Magic] Dex Offset");
		mob.RemoveStatMod("[Magic] Int Offset");
		mob.RemoveStatMod("Concussion");
		mob.RemoveStatMod("blood-rose");
		mob.RemoveStatMod("clarity-potion");

		OrangePetals.RemoveContext(mob);

		mob.Paralyzed = false;
		mob.Hidden = false;

		if (!Core.AOS)
		{
			mob.MagicDamageAbsorb = 0;
			mob.MeleeDamageAbsorb = 0;
			ProtectionSpell.Registry.Remove(mob);

			ArchProtectionSpell.RemoveEntry(mob);

			mob.EndAction(typeof(DefensiveSpell));
		}

		TransformationSpellHelper.RemoveContext(mob, true);
		AnimalForm.RemoveContext(mob, true);

		if (DisguiseTimers.IsDisguised(mob))
			DisguiseTimers.StopTimer(mob);

		if (!mob.CanBeginAction(typeof(PolymorphSpell)))
		{
			mob.BodyMod = 0;
			mob.HueMod = -1;
			mob.EndAction(typeof(PolymorphSpell));
		}

		BaseEquipment.ValidateMobile(mob);

		mob.Hits = mob.HitsMax;
		mob.Stam = mob.StamMax;
		mob.Mana = mob.ManaMax;

		mob.Poison = null;
	}

	public static void CancelSpell(Mobile mob)
	{
		if (mob.Spell is Spell spell)
			spell.Disturb(DisturbType.Kill);

		mob.Target?.Cancel(mob);
	}

	public bool StartedBeginCountdown { get; private set; }
	public bool StartedReadyCountdown { get; private set; }

	private class InternalWall : BaseItem
	{
		public InternalWall() : base(0x80)
		{
			Movable = false;
		}

		public void Appear(Point3D loc, Map map)
		{
			MoveToWorld(loc, map);

			Effects.SendLocationParticles(this, 0x376A, 9, 10, 5025);
		}

		public InternalWall(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Delete();
		}
	}

	private readonly ArrayList _walls = new();

	public void DestroyWall()
	{
		for (int i = 0; i < _walls.Count; ++i)
		{
			((Item)_walls[i])?.Delete();
		}

		_walls.Clear();
	}

	public void CreateWall()
	{
		if (Arena == null)
			return;

		Point3D start = Arena.Points.EdgeWest;
		Point3D wall = Arena.Wall;

		int dx = start.X - wall.X;
		int dy = start.Y - wall.Y;
		int rx = dx - dy;
		int ry = dx + dy;

		bool eastToWest;

		if (rx >= 0 && ry >= 0)
			eastToWest = false;
		else if (rx >= 0)
			eastToWest = true;
		else if (ry >= 0)
			eastToWest = true;
		else
			eastToWest = false;

		Effects.PlaySound(wall, Arena.Facet, 0x1F6);

		for (int i = -1; i <= 1; ++i)
		{
			Point3D loc = new(eastToWest ? wall.X + i : wall.X, eastToWest ? wall.Y : wall.Y + i, wall.Z);

			InternalWall created = new();

			created.Appear(loc, Arena.Facet);

			_walls.Add(created);
		}
	}

	public void BuildParties()
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			if (p.Players.Length > 1)
			{
				ArrayList players = new();

				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer dp = p.Players[j];

					if (dp == null)
						continue;

					players.Add(dp.Mobile);
				}

				if (players.Count > 1)
				{
					for (int leaderIndex = 0; leaderIndex + 1 < players.Count; leaderIndex += Party.Capacity)
					{
						Mobile leader = (Mobile)players[leaderIndex];
						Party party = Party.Get(leader);

						if (party == null)
						{
							if (leader != null) leader.Party = party = new Party(leader);
						}
						else if (party.Leader != leader)
						{
							party.SendPublicMessage(leader, "I leave this party to fight in a duel.");
							party.Remove(leader);
							leader.Party = party = new Party(leader);
						}

						for (int j = leaderIndex + 1; j < players.Count && j < leaderIndex + Party.Capacity; ++j)
						{
							Mobile player = (Mobile)players[j];
							Party existing = Party.Get(player);

							if (existing == party)
								continue;

							if (party != null && party.Members.Count + party.Candidates.Count >= Party.Capacity)
							{
								player?.SendMessage(
									"You could not be added to the team party because it is at full capacity.");
								leader?.SendMessage(
									"{0} could not be added to the team party because it is at full capacity.");
							}
							else
							{
								if (existing != null)
								{
									existing.SendPublicMessage(player, "I leave this party to fight in a duel.");
									existing.Remove(player);
								}

								party?.OnAccept(player, true);
							}
						}
					}
				}
			}
		}
	}

	public void ClearIllegalItems()
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null)
					continue;

				ClearIllegalItems(pl.Mobile);
			}
		}
	}

	public void ClearIllegalItems(Mobile mob)
	{
		if (mob.StunReady && !AllowSpecialAbility(mob, "Stun", false))
			mob.StunReady = false;

		if (mob.DisarmReady && !AllowSpecialAbility(mob, "Disarm", false))
			mob.DisarmReady = false;

		Container pack = mob.Backpack;

		if (pack == null)
			return;

		for (int i = mob.Items.Count - 1; i >= 0; --i)
		{
			if (i >= mob.Items.Count)
				continue; // sanity

			Item item = mob.Items[i];

			if (!CheckItemEquip(mob, item))
			{
				pack.DropItem(item);

				switch (item)
				{
					case BaseWeapon:
						mob.SendLocalizedMessage(1062001, item.Name ?? "#" + item.LabelNumber); // You can no longer wield your ~1_WEAPON~
						break;
					case BaseArmor when !(item is BaseShield):
						mob.SendLocalizedMessage(1062002, item.Name ?? "#" + item.LabelNumber); // You can no longer wear your ~1_ARMOR~
						break;
					default:
						mob.SendLocalizedMessage(1062003, item.Name ?? "#" + item.LabelNumber); // You can no longer equip your ~1_SHIELD~
						break;
				}
			}
		}

		Item inHand = mob.Holding;

		if (inHand != null && !CheckItemEquip(mob, inHand))
		{
			mob.Holding = null;

			BounceInfo bi = inHand.GetBounce();

			if (bi.m_Parent == mob)
				pack.DropItem(inHand);
			else
				inHand.Bounce(mob);

			inHand.ClearBounce();
		}
	}

	public void SendBeginGump(int count)
	{
		if (!Registered || Finished)
			return;

		switch (count)
		{
			case 10:
				CreateWall();
				BuildParties();
				ClearIllegalItems();
				break;
			case 0:
				DestroyWall();
				break;
		}

		StartedBeginCountdown = true;

		if (count == 0)
		{
			Started = true;
			BeginAutoTie();
		}

		Type[] types = { typeof(ReadyGump), typeof(ReadyUpGump), typeof(BeginGump) };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null)
					continue;

				Mobile mob = pl.Mobile;

				if (count > 0)
				{
					if (count == 10)
						CloseAndSendGump(mob, new BeginGump(count), types);

					mob.Frozen = true;
				}
				else
				{
					mob.CloseGump(typeof(BeginGump));
					mob.Frozen = false;
				}
			}
		}
	}

	//private readonly ArrayList m_Entered = new();

	private class ReturnEntry
	{
		private DateTime _expire;

		public Mobile Mobile { get; }
		public Point3D Location { get; private set; }

		public Map Facet { get; private set; }

		public void Return()
		{
			if (Facet == Map.Internal || Facet == null)
				return;

			if (Mobile.Map == Map.Internal)
			{
				Mobile.LogoutLocation = Location;
				Mobile.LogoutMap = Facet;
			}
			else
			{
				Mobile.Location = Location;
				Mobile.Map = Facet;
			}
		}

		public ReturnEntry(Mobile mob)
		{
			Mobile = mob;

			Update();
		}

		public ReturnEntry(Mobile mob, Point3D loc, Map facet)
		{
			Mobile = mob;
			Location = loc;
			Facet = facet;
			_expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);
		}

		public bool Expired => DateTime.UtcNow >= _expire;

		public void Update()
		{
			_expire = DateTime.UtcNow + TimeSpan.FromMinutes(30.0);

			if (Mobile.Map == Map.Internal)
			{
				Facet = Mobile.LogoutMap;
				Location = Mobile.LogoutLocation;
			}
			else
			{
				Facet = Mobile.Map;
				Location = Mobile.Location;
			}
		}
	}

	private class ExitTeleporter : BaseItem
	{
		private ArrayList _entries;

		public override string DefaultName => "return teleporter";

		public ExitTeleporter() : base(0x1822)
		{
			_entries = new ArrayList();

			Hue = 0x482;
			Movable = false;
		}

		public void Register(Mobile mob)
		{
			ReturnEntry entry = Find(mob);

			if (entry != null)
			{
				entry.Update();
				return;
			}

			_entries.Add(new ReturnEntry(mob));
		}

		private ReturnEntry Find(IEntity mob)
		{
			for (int i = 0; i < _entries.Count; ++i)
			{
				ReturnEntry entry = (ReturnEntry)_entries[i];

				if (entry != null && entry.Mobile == mob)
					return entry;
				if (entry is {Expired: true})
					_entries.RemoveAt(i--);
			}

			return null;
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (!base.OnMoveOver(m))
				return false;

			ReturnEntry entry = Find(m);

			if (entry != null)
			{
				entry.Return();

				Effects.PlaySound(GetWorldLocation(), Map, 0x1FE);
				Effects.PlaySound(m.Location, m.Map, 0x1FE);

				_entries.Remove(entry);

				return false;
			}

			m.SendLocalizedMessage(1049383); // The teleporter doesn't seem to work for you.
			return true;
		}

		public ExitTeleporter(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.WriteEncodedInt(_entries.Count);

			for (int i = 0; i < _entries.Count; ++i)
			{
				ReturnEntry entry = (ReturnEntry)_entries[i];

				writer.Write(entry.Mobile);
				writer.Write(entry.Location);
				writer.Write(entry.Facet);

				if (entry.Expired)
					_entries.RemoveAt(i--);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
				{
					int count = reader.ReadEncodedInt();

					_entries = new ArrayList(count);

					for (int i = 0; i < count; ++i)
					{
						Mobile mob = reader.ReadMobile();
						Point3D loc = reader.ReadPoint3D();
						Map map = reader.ReadMap();

						_entries.Add(new ReturnEntry(mob, loc, map));
					}

					break;
				}
			}
		}
	}

	private class ArenaMoongate : ConfirmationMoongate
	{
		private readonly ExitTeleporter _teleporter;

		public override string DefaultName => "spectator moongate";

		public ArenaMoongate(Point3D target, Map map, ExitTeleporter tp) : base(target, map)
		{
			_teleporter = tp;

			ItemId = 0x1FD4;
			Dispellable = false;

			GumpWidth = 300;
			GumpHeight = 150;
			MessageColor = 0xFFC000;
			MessageString = "Are you sure you wish to spectate this duel?";
			TitleColor = 0x7800;
			TitleNumber = 1062051; // Gate Warning

			Timer.DelayCall(TimeSpan.FromSeconds(10.0), Delete);
		}

		public override void CheckGate(Mobile m, int range)
		{
			if (CheckCombat(m))
			{
				m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
			}
			else
			{
				base.CheckGate(m, range);
			}
		}

		public override void UseGate(Mobile m)
		{
			if (CheckCombat(m))
			{
				m.SendMessage(0x22, "You have recently been in combat with another player and cannot use this moongate.");
			}
			else
			{
				if (_teleporter != null && !_teleporter.Deleted)
					_teleporter.Register(m);

				base.UseGate(m);
			}
		}

		public void Appear(Point3D loc, Map map)
		{
			Effects.PlaySound(loc, map, 0x20E);
			MoveToWorld(loc, map);
		}

		public ArenaMoongate(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Delete();
		}
	}

	public void RemoveAggressions(Mobile mob)
	{
		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer dp = p.Players[j];

				if (dp == null || dp.Mobile == mob)
					continue;

				mob.RemoveAggressed(dp.Mobile);
				mob.RemoveAggressor(dp.Mobile);
				dp.Mobile.RemoveAggressed(mob);
				dp.Mobile.RemoveAggressor(mob);
			}
		}
	}

	public void SendReadyUpGump()
	{
		if (!Registered)
			return;

		ReadyWait = true;
		ReadyCount = -1;

		Type[] types = { typeof(ReadyUpGump) };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				Mobile mob = pl?.Mobile;

				if (mob != null)
				{
					if (_Tournament == null)
						CloseAndSendGump(mob, new ReadyUpGump(mob, this), types);
				}
			}
		}
	}

	public string ValidateStart()
	{
		if (_Tournament == null && TournamentController.IsActive)
			return "a tournament is active";

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer dp = p.Players[j];

				if (dp == null)
					return "a slot is empty";

				if (dp.Mobile.Region.IsPartOf(typeof(Regions.Jail)))
					return $"{dp.Mobile.Name} is in jail";

				if (Sigil.ExistsOn(dp.Mobile))
					return $"{dp.Mobile.Name} is holding a sigil";

				if (!dp.Mobile.Alive)
				{
					if (_Tournament == null)
						return $"{dp.Mobile.Name} is dead";
					dp.Mobile.Resurrect();
				}

				if (_Tournament == null && CheckCombat(dp.Mobile))
					return $"{dp.Mobile.Name} is in combat";

				if (dp.Mobile.Mounted)
				{
					IMount mount = dp.Mobile.Mount;

					if (_Tournament != null && mount != null)
						mount.Rider = null;
					else
						return $"{dp.Mobile.Name} is mounted";
				}
			}
		}

		return null;
	}

	public Arena OverrideArena;
	public Tournament _Tournament;
	public TournyMatch Match;
	public EventGame EventGame;

	public Tournament Tournament => _Tournament;

	public void SendReadyGump(int count)
	{
		if (!Registered)
			return;

		if (count != -1)
			StartedReadyCountdown = true;

		ReadyCount = count;

		if (count == 0)
		{
			string error = ValidateStart();

			if (error != null)
			{
				for (int i = 0; i < Participants.Count; ++i)
				{
					Participant p = (Participant)Participants[i];

					for (int j = 0; j < p.Players.Length; ++j)
					{
						DuelPlayer dp = p.Players[j];

						dp?.Mobile.SendMessage("The duel could not be started because {0}.", error);
					}
				}

				StartCountdown(10, SendReadyGump);

				return;
			}

			ReadyWait = false;

			List<Mobile> players = new();

			for (int i = 0; i < Participants.Count; ++i)
			{
				Participant p = (Participant)Participants[i];

				for (int j = 0; j < p.Players.Length; ++j)
				{
					DuelPlayer dp = p.Players[j];

					if (dp != null)
						players.Add(dp.Mobile);
				}
			}

			Arena arena = OverrideArena;

			if (arena == null)
				arena = Arena.FindArena(players);

			if (arena == null)
			{
				for (int i = 0; i < Participants.Count; ++i)
				{
					Participant p = (Participant)Participants[i];

					for (int j = 0; j < p.Players.Length; ++j)
					{
						DuelPlayer dp = p.Players[j];

						dp?.Mobile.SendMessage("The duel could not be started because there are no arenas. If you want to stop waiting for a free arena, yield the duel.");
					}
				}

				StartCountdown(10, SendReadyGump);
				return;
			}

			if (!arena.IsOccupied)
			{
				Arena = arena;

				if (Initiator.Map == Map.Internal)
				{
					_gatePoint = Initiator.LogoutLocation;
					_gateFacet = Initiator.LogoutMap;
				}
				else
				{
					_gatePoint = Initiator.Location;
					_gateFacet = Initiator.Map;
				}

				if (arena.Teleporter is not ExitTeleporter tp)
				{
					arena.Teleporter = tp = new ExitTeleporter();
					tp.MoveToWorld(arena.GateOut == Point3D.Zero ? arena.Outside : arena.GateOut, arena.Facet);
				}

				ArenaMoongate mg = new(arena.GateIn == Point3D.Zero ? arena.Outside : arena.GateIn, arena.Facet, tp);

				StartedBeginCountdown = true;

				for (int i = 0; i < Participants.Count; ++i)
				{
					Participant p = (Participant)Participants[i];

					for (int j = 0; j < p.Players.Length; ++j)
					{
						DuelPlayer pl = p.Players[j];

						if (pl == null)
							continue;

						tp.Register(pl.Mobile);

						pl.Mobile.Frozen = false; // reset timer just in case
						pl.Mobile.Frozen = true;

						Debuff(pl.Mobile);
						CancelSpell(pl.Mobile);

						pl.Mobile.Delta(MobileDelta.Noto);
					}

					arena.MoveInside(p.Players, i);
				}

				if (EventGame != null)
					EventGame.OnStart();

				StartCountdown(10, SendBeginGump);

				mg.Appear(_gatePoint, _gateFacet);
			}
			else
			{
				for (int i = 0; i < Participants.Count; ++i)
				{
					Participant p = (Participant)Participants[i];

					for (int j = 0; j < p.Players.Length; ++j)
					{
						DuelPlayer dp = p.Players[j];

						dp?.Mobile.SendMessage("The duel could not be started because all arenas are full. If you want to stop waiting for a free arena, yield the duel.");
					}
				}

				StartCountdown(10, SendReadyGump);
			}

			return;
		}

		ReadyWait = true;

		bool isAllReady = true;

		Type[] types = { typeof(ReadyGump) };

		for (int i = 0; i < Participants.Count; ++i)
		{
			Participant p = (Participant)Participants[i];

			for (int j = 0; j < p.Players.Length; ++j)
			{
				DuelPlayer pl = p.Players[j];

				if (pl == null)
					continue;

				Mobile mob = pl.Mobile;

				if (pl.Ready)
				{
					if (_Tournament == null)
						CloseAndSendGump(mob, new ReadyGump(mob, this, count), types);
				}
				else
				{
					isAllReady = false;
				}
			}
		}

		if (count == -1 && isAllReady)
			StartCountdown(3, SendReadyGump);
	}

	public static void CloseAndSendGump(Mobile mob, Gump g, params Type[] types)
	{
		CloseAndSendGump(mob.NetState, g, types);
	}

	public static void CloseAndSendGump(NetState ns, Gump g, params Type[] types)
	{
		Mobile mob = ns?.Mobile;

		if (mob != null)
		{
			foreach (var type in types)
			{
				mob.CloseGump(type);
			}

			mob.SendGump(g);
		}

		/*if ( ns == null )
			return;

		for ( int i = 0; i < types.Length; ++i )
			ns.Send( new CloseGump( Gump.GetTypeID( types[i] ), 0 ) );

		g.SendTo( ns );

		ns.AddGump( g );

		Packet[] packets = new Packet[types.Length + 1];

		for ( int i = 0; i < types.Length; ++i )
			packets[i] = new CloseGump( Gump.GetTypeID( types[i] ), 0 );

		packets[types.Length] = (Packet) typeof( Gump ).InvokeMember( "Compile", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, g, null, null );

		bool compress = ns.CompressionEnabled;
		ns.CompressionEnabled = false;
		ns.Send( BindPackets( compress, packets ) );
		ns.CompressionEnabled = compress;*/
	}

	/*public static Packet BindPackets( bool compress, params Packet[] packets )
	{
		if ( packets.Length == 0 )
			throw new ArgumentException( "No packets to bind", "packets" );

		byte[][] compiled = new byte[packets.Length][];
		int[] lengths = new int[packets.Length];

		int length = 0;

		for ( int i = 0; i < packets.Length; ++i )
		{
			compiled[i] = packets[i].Compile( compress, out lengths[i] );
			length += lengths[i];
		}

		return new BoundPackets( length, compiled, lengths );
	}

	private class BoundPackets : Packet
	{
		public BoundPackets( int length, byte[][] compiled, int[] lengths ) : base( 0, length )
		{
			m_Stream.Seek( 0, System.IO.SeekOrigin.Begin );

			for ( int i = 0; i < compiled.Length; ++i )
				m_Stream.Write( compiled[i], 0, lengths[i] );
		}
	}*/
}
