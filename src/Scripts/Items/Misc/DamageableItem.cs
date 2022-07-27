using Server.Mobiles;
using Server.Network;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public interface IDamageableItem : IDamageable
{
	bool CanDamage { get; }
	bool CheckHit(Mobile attacker);
	void OnHarmfulSpell(Mobile attacker);
}

public class DamageableItem : Item, IDamageableItem
{
	public enum ItemLevel
	{
		NotSet,
		VeryEasy,
		Easy,
		Average,
		Hard,
		VeryHard,
		Insane
	}

	private int _mHits;
	private int _mHitsMax;
	private int _mStartId;
	private int _mDestroyedId;
	private int _mHalfHitsId;
	private ItemLevel _mItemLevel;

	[CommandProperty(AccessLevel.GameMaster)]
	private ItemLevel Level
	{
		get => _mItemLevel;
		set
		{
			_mItemLevel = value;

			double bonus = (int)_mItemLevel * 100.0 * ((int)_mItemLevel * 5);

			HitsMax = (int)(100 + bonus);
			Hits = (int)(100 + bonus);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int IdStart
	{
		get => _mStartId;
		init
		{
			_mStartId = value < 0 ? 0 : value;

			if (!(_mHits >= _mHitsMax * IdChange)) return;
			if (ItemId != _mStartId)
				ItemId = _mStartId;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int IdHalfHits
	{
		get => _mHalfHitsId;
		init
		{
			_mHalfHitsId = value < 0 ? 0 : value;

			if (!(_mHits < _mHitsMax * IdChange)) return;
			if (ItemId != _mHalfHitsId)
				ItemId = _mHalfHitsId;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private int IdDestroyed
	{
		get => _mDestroyedId;
		init
		{
			if (value is < 0 or > int.MaxValue)
				_mDestroyedId = -1;
			else
				_mDestroyedId = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Hits
	{
		get => _mHits;
		set
		{
			if (value > HitsMax)
			{
				value = HitsMax;
			}

			if (_mHits != value)
			{
				int oldValue = _mHits;
				_mHits = value;
				UpdateDelta();
				OnHitsChange(oldValue);
			}

			int id = ItemId;

			if (_mHits >= _mHitsMax * IdChange && id != _mStartId)
			{
				ItemId = _mStartId;
				OnIDChange(id);
			}
			else if (_mHits <= _mHitsMax * IdChange && id == _mStartId)
			{
				ItemId = _mHalfHitsId;
				OnIDChange(id);
			}

			if (_mHits < 0)
			{
				Destroy();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitsMax
	{
		get => _mHitsMax;
		private set
		{
			_mHitsMax = value;

			if (Hits > _mHitsMax)
				Hits = _mHitsMax;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Destroyed { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ResistBasePhys { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ResistBaseFire { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ResistBaseCold { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ResistBasePoison { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ResistBaseEnergy { get; set; }

	private Dictionary<Mobile, int> DamageStore { get; set; }

	public virtual int HitEffect => -1;
	public virtual int DestroySound => 0x3B3;
	public virtual double IdChange => 0.5;
	public virtual bool DeleteOnDestroy => true;
	public virtual bool Alive => !Destroyed;
	public virtual bool CanDamage => true;

	public override int PhysicalResistance => ResistBasePhys;
	public override int FireResistance => ResistBaseFire;
	public override int ColdResistance => ResistBaseCold;
	public override int PoisonResistance => ResistBasePoison;
	public override int EnergyResistance => ResistBaseEnergy;

	public override bool ForceShowProperties => false;

	[Constructable]
	public DamageableItem(int startId)
		: this(startId, startId, -1)
	{
	}

	[Constructable]
	public DamageableItem(int startId, int halfId)
		: this(startId, halfId, -1)
	{
	}

	[Constructable]
	private DamageableItem(int startId, int halfId, int destroyId = -1)
		: base(startId)
	{
		Hue = 0;
		Movable = false;

		Level = ItemLevel.NotSet;

		IdStart = startId;
		IdHalfHits = halfId;
		IdDestroyed = destroyId;
	}

	int IDamageable.ComputeNotoriety(Mobile viewer) => GetNotoriety(viewer);

	public virtual int GetNotoriety(Mobile beholder)
	{
		return Notoriety.Compute(beholder, this);
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m.Warmode)
			m.Attack(this);
	}

	public virtual bool CheckHit(Mobile attacker)
	{
		return true; // Always hits
	}

	public virtual void OnHarmfulSpell(Mobile attacker)
	{
	}

	public virtual bool CheckReflect(int circle, IDamageable caster)
	{
		return false;
	}

	public override void OnStatsQuery(Mobile from)
	{
		if (from.Map == Map && from.InUpdateRange(this) && from.CanSee(this))
		{
			from.Send(new MobileStatusCompact(false, this));
		}
	}

	public virtual void UpdateDelta()
	{
		var eable = Map.GetClientsInRange(Location);

		Packet status = Packet.Acquire(new MobileHitsN(this));

		foreach (NetState ns in eable)
		{
			var beholder = ns.Mobile;

			if (beholder != null && beholder.CanSee(this))
			{
				ns.Send(status);
			}
		}

		Packet.Release(status);
		eable.Free();
	}

	public virtual void OnHitsChange(int oldhits)
	{
	}

	public virtual bool OnBeforeDestroyed()
	{
		return true;
	}

	public virtual void OnAfterDestroyed()
	{
	}

	public virtual int Damage(int amount, Mobile from)
	{
		if (!CanDamage && from.Combatant == this)
		{
			from.Combatant = null;
			return 0;
		}

		Hits -= amount;

		if (amount > 0)
			RegisterDamage(from, amount);

		if (HitEffect > 0)
			Effects.SendLocationEffect(Location, Map, HitEffect, 10, 5);

		SendDamagePacket(from, amount);

		OnDamage(amount, from, Hits < 0);

		return amount;
	}

	public virtual void SendDamagePacket(Mobile from, int amount)
	{
		NetState theirState = from?.NetState;

		if (theirState == null && from != null)
		{
			Mobile master = from.GetDamageMaster(null);

			if (master != null)
			{
				theirState = master.NetState;
			}
		}

		if (amount > 0 && theirState != null)
		{
			theirState.Send(new DamagePacket(this, amount));
		}
	}

	private void RegisterDamage(Mobile m, int damage)
	{
		if (m == null)
			return;

		DamageStore ??= new Dictionary<Mobile, int>();

		if ((m as BaseCreature)?.GetMaster() is PlayerMobile)
			m = ((BaseCreature)m).GetMaster();

		if (!DamageStore.ContainsKey(m))
			DamageStore[m] = 0;

		DamageStore[m] += damage;
	}

	public List<Mobile> GetLootingRights()
	{
		return DamageStore?.Keys.Where(m => DamageStore[m] > 0 && DamageStore[m] >= HitsMax / 16).ToList();
	}

	public virtual void OnDamage(int amount, Mobile from, bool willkill)
	{
	}

	private bool Destroy()
	{
		if (Deleted || Destroyed)
			return false;

		Effects.PlaySound(Location, Map, DestroySound);

		if (OnBeforeDestroyed())
		{
			if (DeleteOnDestroy)
			{
				Delete();
			}
			else if (_mDestroyedId >= 0)
			{
				ItemId = _mDestroyedId;

				Spawner?.Remove(this);
			}

			Destroyed = true;
			OnAfterDestroyed();

			return true;
		}

		return false;
	}

	public override void OnAfterDelete()
	{
		base.OnAfterDelete();

		DamageStore?.Clear();
	}

	public virtual void OnIDChange(int oldId)
	{
	}

	#region Effects/Sounds & Particles
	public void PlaySound(int soundId)
	{
		if (soundId == -1)
		{
			return;
		}

		if (Map != null)
		{
			Packet p = Packet.Acquire(new PlaySound(soundId, this));

			var eable = Map.GetClientsInRange(Location);

			foreach (NetState state in eable)
			{
				if (state.Mobile.CanSee(this))
				{
					state.Send(p);
				}
			}

			Packet.Release(p);

			eable.Free();
		}
	}

	public void MovingEffect(
		IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode)
	{
		Effects.SendMovingEffect(this, to, itemId, speed, duration, fixedDirection, explodes, hue, renderMode);
	}

	public void MovingEffect(IEntity to, int itemId, int speed, int duration, bool fixedDirection, bool explodes)
	{
		Effects.SendMovingEffect(this, to, itemId, speed, duration, fixedDirection, explodes, 0, 0);
	}

	public void MovingParticles(
		IEntity to,
		int itemId,
		int speed,
		int duration,
		bool fixedDirection,
		bool explodes,
		int hue,
		int renderMode,
		int effect,
		int explodeEffect,
		int explodeSound,
		EffectLayer layer,
		int unknown)
	{
		Effects.SendMovingParticles(
			this,
			to,
			itemId,
			speed,
			duration,
			fixedDirection,
			explodes,
			hue,
			renderMode,
			effect,
			explodeEffect,
			explodeSound,
			layer,
			unknown);
	}

	public void MovingParticles(
		IEntity to,
		int itemId,
		int speed,
		int duration,
		bool fixedDirection,
		bool explodes,
		int hue,
		int renderMode,
		int effect,
		int explodeEffect,
		int explodeSound,
		int unknown)
	{
		Effects.SendMovingParticles(
			this,
			to,
			itemId,
			speed,
			duration,
			fixedDirection,
			explodes,
			hue,
			renderMode,
			effect,
			explodeEffect,
			explodeSound,
			(EffectLayer)255,
			unknown);
	}

	public void MovingParticles(
		IEntity to,
		int itemId,
		int speed,
		int duration,
		bool fixedDirection,
		bool explodes,
		int effect,
		int explodeEffect,
		int explodeSound,
		int unknown)
	{
		Effects.SendMovingParticles(
			this, to, itemId, speed, duration, fixedDirection, explodes, effect, explodeEffect, explodeSound, unknown);
	}

	public void MovingParticles(
		IEntity to,
		int itemId,
		int speed,
		int duration,
		bool fixedDirection,
		bool explodes,
		int effect,
		int explodeEffect,
		int explodeSound)
	{
		Effects.SendMovingParticles(
			this, to, itemId, speed, duration, fixedDirection, explodes, 0, 0, effect, explodeEffect, explodeSound, 0);
	}

	public void FixedEffect(int itemId, int speed, int duration, int hue, int renderMode)
	{
		Effects.SendTargetEffect(this, itemId, speed, duration, hue, renderMode);
	}

	public void FixedEffect(int itemId, int speed, int duration)
	{
		Effects.SendTargetEffect(this, itemId, speed, duration, 0, 0);
	}

	public void FixedParticles(
		int itemId, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer, int unknown)
	{
		Effects.SendLocationParticles(this, itemId, speed, duration, hue, renderMode, effect, unknown);
	}

	public void FixedParticles(
		int itemId, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer)
	{
		Effects.SendLocationParticles(this, itemId, speed, duration, hue, renderMode, effect, 0);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, EffectLayer layer, int unknown)
	{
		Effects.SendLocationParticles(this, itemId, speed, duration, 0, 0, effect, unknown);
	}

	public void FixedParticles(int itemId, int speed, int duration, int effect, EffectLayer layer)
	{
		Effects.SendLocationParticles(this, itemId, speed, duration, 0, 0, effect, 0);
	}

	public void BoltEffect(int hue)
	{
		Effects.SendBoltEffect(this, true, hue);
	}
	#endregion

	public DamageableItem(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(_mStartId);
		writer.Write(_mHalfHitsId);
		writer.Write(_mDestroyedId);
		writer.Write((int)_mItemLevel);
		writer.Write(_mHits);
		writer.Write(_mHitsMax);
		writer.Write(Destroyed);

		writer.Write(ResistBasePhys);
		writer.Write(ResistBaseFire);
		writer.Write(ResistBaseCold);
		writer.Write(ResistBasePoison);
		writer.Write(ResistBaseEnergy);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		_mStartId = reader.ReadInt();
		_mHalfHitsId = reader.ReadInt();
		_mDestroyedId = reader.ReadInt();
		_mItemLevel = (ItemLevel)reader.ReadInt();
		_mHits = reader.ReadInt();
		_mHitsMax = reader.ReadInt();
		Destroyed = reader.ReadBool();

		ResistBasePhys = reader.ReadInt();
		ResistBaseFire = reader.ReadInt();
		ResistBaseCold = reader.ReadInt();
		ResistBasePoison = reader.ReadInt();
		ResistBaseEnergy = reader.ReadInt();
	}
}

public class TestDamageableItem : DamageableItem
{
	[Constructable]
	public TestDamageableItem(int itemid)
		: base(itemid, itemid)
	{
		Name = "Test Damageable Item";
	}

	public TestDamageableItem(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
