using System;

namespace Server.Items;

public class HealingStone : BaseItem
{
	private int _lifeForce;
	private Timer _timer;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Caster { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int LifeForce { get => _lifeForce;
		set { _lifeForce = value; InvalidateProperties(); } }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxLifeForce { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxHeal { get; private set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxHealTotal { get; private set; }

	public override bool Nontransferable => true;

	[Constructable]
	public HealingStone(Mobile caster, int amount, int maxHeal) : base(0x4078)
	{
		Caster = caster;
		_lifeForce = amount;
		MaxHeal = maxHeal;

		MaxLifeForce = amount;
		MaxHealTotal = maxHeal;

		LootType = LootType.Blessed;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (!from.InRange(GetWorldLocation(), 1))
		{
			from.SendLocalizedMessage(502138); // That is too far away for you to use
			return;
		}

		if (from != Caster)
		{
		}
		else if (!BasePotion.HasFreeHand(from))
		{
			from.SendLocalizedMessage(1080116); // You must have a free hand to use a Healing Stone.
		}
		else if (from.Hits >= from.HitsMax && !from.Poisoned)
		{
			from.SendLocalizedMessage(1049547); //You are already at full health.
		}
		else if (from.BeginAction(typeof(HealingStone)))
		{
			if (MaxHeal > _lifeForce)
				MaxHeal = _lifeForce;

			if (from.Poisoned)
			{
				int toUse = Math.Min(120, from.Poison.RealLevel * 25);

				if (MaxLifeForce < toUse)
					from.SendLocalizedMessage(1115265); //Your Mysticism, Focus, or Imbuing Skills are not enough to use the heal stone to cure yourself.
				else if (_lifeForce < toUse)
				{
					from.SendLocalizedMessage(1115264); //Your healing stone does not have enough energy to remove the poison.
					LifeForce -= toUse / 3;
				}
				else
				{
					from.CurePoison(from);

					from.SendLocalizedMessage(500231); // You feel cured of poison!

					from.FixedEffect(0x373A, 10, 15);
					from.PlaySound(0x1E0);

					LifeForce -= toUse;
				}

				if (_lifeForce <= 0)
					Consume();

				Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(ReleaseHealLock), from);
				return;
			}

			int toHeal = Math.Min(MaxHeal, from.HitsMax - from.Hits);
			from.Heal(toHeal);
			Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(ReleaseHealLock), from);

			from.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
			from.PlaySound(0x202);

			LifeForce -= toHeal;
			MaxHeal = 1;

			if (_lifeForce <= 0)
			{
				from.SendLocalizedMessage(1115266); //The healing stone has used up all its energy and has been destroyed.
				Consume();
			}
			else
			{
				_timer?.Stop();

				_timer = new InternalTimer(this);
			}
		}
		else
			from.SendLocalizedMessage(1095172); // You must wait a few seconds before using another Healing Stone.
	}

	public void OnTick()
	{
		if (MaxHeal < MaxHealTotal)
		{
			int maxToHeal = MaxHealTotal - MaxHeal;
			MaxHeal += Math.Min(maxToHeal, MaxHealTotal / 15);

			if (MaxHeal > MaxHealTotal)
				MaxHeal = MaxHealTotal;
		}
	}

	private class InternalTimer : Timer
	{
		private readonly HealingStone _stone;
		private int _ticks;

		public InternalTimer(HealingStone stone) : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
		{
			_stone = stone;
			_ticks = 0;
			Start();
		}

		protected override void OnTick()
		{
			_ticks++;

			_stone.OnTick();

			if (_ticks >= 15)
				Stop();
		}
	}

	public override bool DropToWorld(Mobile from, Point3D p)
	{
		Delete();
		return false;
	}

	public override bool AllowSecureTrade(Mobile from, Mobile to, Mobile newOwner, bool accepted)
	{
		return false;
	}

	private static void ReleaseHealLock(object state)
	{
		((Mobile)state).EndAction(typeof(HealingStone));
	}

	public override void Delete()
	{
		if (_timer != null)
		{
			_timer.Stop();
			_timer = null;
		}

		base.Delete();
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1115274, _lifeForce.ToString());
	}

	public HealingStone(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);

		writer.Write(Caster);
		writer.Write(_lifeForce);
		writer.Write(MaxLifeForce);
		writer.Write(MaxHeal);
		writer.Write(MaxHealTotal);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
				Caster = reader.ReadMobile();
				_lifeForce = reader.ReadInt();
				MaxLifeForce = reader.ReadInt();
				MaxHeal = reader.ReadInt();
				MaxHealTotal = reader.ReadInt();
				break;
		}

		if (_lifeForce <= 0)
		{
			Delete();
		}
	}
}
