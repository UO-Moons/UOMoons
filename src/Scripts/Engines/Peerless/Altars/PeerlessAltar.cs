using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Engines.PartySystem;

namespace Server.Items;

public class PeerlessKeyArray
{
	public Type Key { get; set; }
	public bool Active { get; set; }
}

public abstract class PeerlessAltar : Container
{
	public override bool IsPublicContainer => true;
	public override bool IsDecoContainer => false;

	public virtual TimeSpan TimeToSlay => TimeSpan.FromMinutes(90);
	public virtual TimeSpan DelayAfterBossSlain => TimeSpan.FromMinutes(15);

	public abstract int KeyCount { get; }
	public abstract MasterKey MasterKey { get; }

	private List<PeerlessKeyArray> _keyValidation;

	public abstract Type[] Keys { get; }
	public abstract BasePeerless Boss { get; }

	public abstract Rectangle2D[] BossBounds { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public BasePeerless Peerless { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D BossLocation { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D TeleportDest { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D ExitDest { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public DateTime Deadline { get; set; }

	[CommandProperty(AccessLevel.Counselor)]
	public bool ResetPeerless
	{
		get => false;
		set { if (value) FinishSequence(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int FighterCount => Fighters?.Count ?? 0;

	public List<Mobile> Fighters { get; set; }

	public List<Item> MasterKeys { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Owner { get; set; }

	/*public Mobile Summoner
	{
	    get
	    {
	        if (Fighters == null || Fighters.Count == 0)
	            return null;

	        return Fighters[0];
	    }
	}*/

	public PeerlessAltar(int itemId)
		: base(itemId)
	{
		Movable = false;

		Fighters = new List<Mobile>();
		MasterKeys = new List<Item>();
	}

	public PeerlessAltar(Serial serial)
		: base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.AccessLevel > AccessLevel.Player)
			base.OnDoubleClick(from);
	}

	public override bool CheckLift(Mobile from, Item item, ref LRReason reject)
	{
		if (from.AccessLevel > AccessLevel.Player)
			return base.CheckLift(from, item, ref reject);
		reject = LRReason.CannotLift;

		return false;
	}

	public override bool OnDragDrop(Mobile from, Item dropped)
	{
		if (Owner != null && Owner != from)
		{
			if (Peerless != null && Peerless.CheckAlive())
				from.SendLocalizedMessage(1075213); // The master of this realm has already been summoned and is engaged in combat.  Your opportunity will come after he has squashed the current batch of intruders!
			else
				from.SendLocalizedMessage(1072683, Owner.Name); // ~1_NAME~ has already activated the Prism, please wait...

			return false;
		}

		if (IsKey(dropped) && !MasterKeys.Any())
		{
			if (_keyValidation == null)
			{
				_keyValidation = new List<PeerlessKeyArray>();

				Keys.ToList().ForEach(x => _keyValidation.Add(new PeerlessKeyArray { Key = x, Active = false }));
			}

			if (_keyValidation.Any(x => x.Active))
			{
				if (_keyValidation.Any(x => x.Key == dropped.GetType() && x.Active == false))
				{
					_keyValidation.Find(s => s.Key == dropped.GetType())!.Active = true;
				}
				else
				{
					from.SendLocalizedMessage(1072682); // This is not the proper key.
					return false;
				}
			}
			else
			{
				Owner = from;
				KeyStartTimer(from);
				from.SendLocalizedMessage(1074575); // You have activated this object!
				_keyValidation.Find(s => s.Key == dropped.GetType())!.Active = true;
			}

			if (_keyValidation.Count(x => x.Active) == Keys.Count())
			{
				KeyStopTimer();

				from.SendLocalizedMessage(1072678); // You have awakened the master of this realm. You need to hurry to defeat it in time!
				BeginSequence(from);

				for (int k = 0; k < KeyCount; k++)
				{
					from.SendLocalizedMessage(1072680); // You have been given the key to the boss.

					MasterKey key = MasterKey;

					if (key != null)
					{
						key.Altar = this;
						key._Map = Map;

						if (!from.AddToBackpack(key))
							key.MoveToWorld(from.Location, from.Map);

						MasterKeys.Add(key);
					}
				}

				Timer.DelayCall(TimeSpan.FromSeconds(1), ClearContainer);
				_keyValidation = null;
			}
		}
		else
		{
			from.SendLocalizedMessage(1072682); // This is not the proper key.
			return false;
		}

		return base.OnDragDrop(from, dropped);
	}

	public virtual bool IsKey(Item item)
	{
		if (Keys == null || item == null)
			return false;

		bool isKey = false;

		// check if item is key	
		for (int i = 0; i < Keys.Length && !isKey; i++)
		{
			if (Keys[i].IsInstanceOfType(item))
				isKey = true;
		}

		// check if item is already in container			
		for (int i = 0; i < Items.Count && isKey; i++)
		{
			if (Items[i].GetType() == item.GetType())
				return false;
		}

		return isKey;
	}

	private Timer _mKeyResetTimer;

	public virtual void KeyStartTimer(Mobile from)
	{
		_mKeyResetTimer?.Stop();

		_mKeyResetTimer = Timer.DelayCall(TimeSpan.FromSeconds(30 * Keys.Count()), () =>
		{
			from.SendLocalizedMessage(1072679); // Your realm offering has reset. You will need to start over.

			if (Owner != null)
			{
				Owner = null;
			}

			_keyValidation = null;

			ClearContainer();
		});
	}

	public virtual void KeyStopTimer()
	{
		_mKeyResetTimer?.Stop();

		_mKeyResetTimer = null;
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(5); // version

		writer.Write(Owner);

		// version 4 remove pet table

		// version 3 remove IsAvailable

		// version 1
		writer.Write(Helpers != null);

		if (Helpers != null)
			writer.WriteMobileList(Helpers);

		// version 0			
		writer.Write(Peerless);
		writer.Write(BossLocation);
		writer.Write(TeleportDest);
		writer.Write(ExitDest);

		writer.Write(Deadline);

		// serialize master keys						
		writer.WriteItemList(MasterKeys);

		// serialize fighters							
		writer.WriteMobileList(Fighters);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 5:
			{
				Owner = reader.ReadMobile();
				goto case 4;
			}
			case 4:
			case 3:
				if (version < 5)
				{
					reader.ReadBool();
				}
				goto case 2;
			case 2:
			case 1:
				if (reader.ReadBool())
					Helpers = reader.ReadStrongMobileList<BaseCreature>();
				goto case 0;
			case 0:
				Peerless = reader.ReadMobile() as BasePeerless;
				BossLocation = reader.ReadPoint3D();
				TeleportDest = reader.ReadPoint3D();
				ExitDest = reader.ReadPoint3D();

				Deadline = reader.ReadDateTime();

				MasterKeys = reader.ReadStrongItemList();
				Fighters = reader.ReadStrongMobileList();

				if (version < 4)
				{
					int count = reader.ReadInt();

					for (int i = 0; i < count; i++)
					{
						reader.ReadMobile();
						reader.ReadStrongMobileList();
					}
				}

				if (version < 2)
					reader.ReadBool();

				if (Peerless == null && Helpers.Count > 0)
					Timer.DelayCall(TimeSpan.FromSeconds(30), CleanupHelpers);

				break;
		}


		if (Owner != null && Peerless == null)
		{
			FinishSequence();
		}
	}

	public virtual void ClearContainer()
	{
		for (int i = Items.Count - 1; i >= 0; --i)
		{
			if (i < Items.Count)
				Items[i].Delete();
		}
	}

	public virtual void AddFighter(Mobile fighter)
	{
		if (!Fighters.Contains(fighter))
			Fighters.Add(fighter);
	}

	public virtual void SendConfirmations(Mobile from)
	{
		Party party = Party.Get(from);

		if (party != null)
		{
			foreach (var m in party.Members.Select(info => info.Mobile))
			{
				if (m.InRange(from.Location, 25) && CanEnter(m))
				{
					m.SendGump(new ConfirmEntranceGump(this, from));
				}
			}
		}
		else
		{
			from.SendGump(new ConfirmEntranceGump(this, from));
		}
	}

	public virtual void BeginSequence(Mobile from)
	{
		SpawnBoss();
	}

	public virtual void SpawnBoss()
	{
		if (Peerless == null)
		{
			// spawn boss
			Peerless = Boss;

			if (Peerless == null)
				return;

			Peerless.Home = BossLocation;
			Peerless.RangeHome = 12;
			Peerless.MoveToWorld(BossLocation, Map);
			Peerless.Altar = this;

			StartSlayTimer();
		}
	}

	public void Enter(Mobile fighter)
	{
		if (CanEnter(fighter))
		{
			// teleport party member's pets
			if (fighter is PlayerMobile mobile)
			{
				foreach (var pet in mobile.AllFollowers.OfType<BaseCreature>().Where(pet => pet.Alive &&
					         pet.InRange(fighter.Location, 5) &&
					         pet is not BaseMount
					         {
						         Rider: { }
					         } &&
					         CanEnter(pet)))
				{
					pet.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
					pet.PlaySound(0x1FE);
					pet.MoveToWorld(TeleportDest, Map);
				}
			}

			// teleport party member
			fighter.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
			fighter.PlaySound(0x1FE);
			fighter.MoveToWorld(TeleportDest, Map);

			AddFighter(fighter);
		}
	}

	public virtual bool CanEnter(Mobile fighter)
	{
		return true;
	}

	public virtual bool CanEnter(BaseCreature pet)
	{
		return true;
	}

	public virtual void FinishSequence()
	{
		StopTimers();

		if (Owner != null)
		{
			Owner = null;
		}

		// delete peerless
		if (Peerless != null)
		{
			if (Peerless.Corpse is {Deleted: false})
				Peerless.Corpse.Delete();

			if (!Peerless.Deleted)
				Peerless.Delete();
		}

		// teleport party to exit if not already there
		if (Fighters != null)
		{
			Fighters.ForEach(Exit);
			Fighters.Clear();
		}

		// delete master keys
		if (MasterKeys != null)
		{
			MasterKeys.ForEach(x => x.Delete());
			MasterKeys.Clear();
		}

		// delete any remaining helpers
		CleanupHelpers();

		// reset summoner, boss		
		Peerless = null;

		Deadline = DateTime.MinValue;
	}

	public virtual void Exit(Mobile fighter)
	{
		if (fighter == null)
			return;

		// teleport fighter
		if (fighter.NetState == null && MobileIsInBossArea(fighter.LogoutLocation))
		{
			fighter.LogoutMap = this is CitadelAltar ? Map.Tokuno : Map;
			fighter.LogoutLocation = ExitDest;
		}
		else if (MobileIsInBossArea(fighter) && fighter.Map == Map)
		{
			fighter.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
			fighter.PlaySound(0x1FE);

			fighter.MoveToWorld(ExitDest, this is CitadelAltar ? Map.Tokuno : Map);
		}

		// teleport his pets
		if (fighter is PlayerMobile mobile)
		{
			foreach (var pet in mobile.AllFollowers.OfType<BaseCreature>().Where(pet => (pet.Alive || pet.IsBonded) &&
				         pet.Map != Map.Internal &&
				         MobileIsInBossArea(pet)))
			{
				if (pet is BaseMount mount)
				{
					if (mount.Rider != null && mount.Rider != fighter)
					{
						mount.Rider.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
						mount.Rider.PlaySound(0x1FE);

						mount.Rider.MoveToWorld(ExitDest, this is CitadelAltar ? Map.Tokuno : Map);

						continue;
					}

					if (mount.Rider != null)
						continue;
				}

				pet.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
				pet.PlaySound(0x1FE);

				pet.MoveToWorld(ExitDest, this is CitadelAltar ? Map.Tokuno : Map);
			}
		}

		Fighters.Remove(fighter);
		fighter.SendLocalizedMessage(1072677); // You have been transported out of this room.

		if (MasterKeys.Count == 0 && Fighters.Count == 0 && Owner != null)
		{
			StopTimers();

			Owner = null;

			if (Peerless != null)
			{
				if (Peerless.Corpse != null && !Peerless.Corpse.Deleted)
					Peerless.Corpse.Delete();

				if (!Peerless.Deleted)
					Peerless.Delete();
			}

			CleanupHelpers();

			// reset summoner, boss		
			Peerless = null;

			Deadline = DateTime.MinValue;
		}
	}

	public virtual void OnPeerlessDeath()
	{
		SendMessage(1072681); // The master of this realm has been slain! You may only stay here so long.

		StopSlayTimer();

		// delete master keys
		ColUtility.SafeDelete(MasterKeys);

		ColUtility.Free(MasterKeys);
		_mDeadlineTimer = Timer.DelayCall(DelayAfterBossSlain, FinishSequence);
	}

	public virtual bool MobileIsInBossArea(Mobile check)
	{
		return MobileIsInBossArea(check.Location);
	}

	public virtual bool MobileIsInBossArea(Point3D loc)
	{
		if (BossBounds == null || BossBounds.Length == 0)
			return true;

		return BossBounds.Any(rec => rec.Contains(loc));
	}

	public virtual void SendMessage(int message)
	{
		Fighters.ForEach(x => x.SendLocalizedMessage(message));
	}

	public virtual void SendMessage(int message, object param)
	{
		Fighters.ForEach(x => x.SendLocalizedMessage(message, param.ToString()));
	}

	private Timer _mSlayTimer;
	private Timer _mDeadlineTimer;

	public virtual void StopTimers()
	{
		StopSlayTimer();
		StopDeadlineTimer();
	}

	public virtual void StopDeadlineTimer()
	{
		_mDeadlineTimer?.Stop();

		_mDeadlineTimer = null;

		if (Owner != null)
		{
			Owner = null;
		}
	}

	public virtual void StopSlayTimer()
	{
		_mSlayTimer?.Stop();

		_mSlayTimer = null;
	}

	public virtual void StartSlayTimer()
	{
		_mSlayTimer?.Stop();

		if (TimeToSlay != TimeSpan.Zero)
			Deadline = DateTime.UtcNow + TimeToSlay;
		else
			Deadline = DateTime.UtcNow + TimeSpan.FromHours(1);

		_mSlayTimer = Timer.DelayCall(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5), new TimerCallback(DeadlineCheck));
		_mSlayTimer.Priority = TimerPriority.OneMinute;
	}

	public virtual void DeadlineCheck()
	{
		if (DateTime.UtcNow > Deadline)
		{
			SendMessage(1072258); // You failed to complete an objective in time!
			FinishSequence();
			return;
		}

		TimeSpan timeLeft = Deadline - DateTime.UtcNow;

		if (timeLeft < TimeSpan.FromMinutes(30))
			SendMessage(1075611, timeLeft.TotalSeconds);

		Fighters.ForEach(x =>
		{
			if (x is PlayerMobile {NetState: null} player)
			{
				TimeSpan offline = DateTime.UtcNow - player.LastOnline;

				if (offline > TimeSpan.FromMinutes(10))
					Exit(player);
			}
		});
	}

	#region Helpers

	public List<BaseCreature> Helpers { get; private set; } = new();

	public void AddHelper(BaseCreature helper)
	{
		if (helper != null && helper.Alive && !helper.Deleted)
			Helpers.Add(helper);
	}

	public bool AllHelpersDead()
	{
		for (int i = 0; i < Helpers.Count; i++)
		{
			BaseCreature c = Helpers[i];

			if (c.Alive)
				return false;
		}

		return true;
	}

	public void CleanupHelpers()
	{
		for (int i = 0; i < Helpers.Count; i++)
		{
			BaseCreature c = Helpers[i];

			if (c != null && c.Alive)
				c.Delete();
		}

		Helpers.Clear();
	}
	#endregion
}
