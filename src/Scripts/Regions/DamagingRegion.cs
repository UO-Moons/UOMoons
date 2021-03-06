using System;
using System.Collections.Generic;
using System.Xml;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Regions;

public class DamagingRegion : MondainRegion
{
	public Dictionary<Mobile, Timer> Table { get; private set; }

	public virtual int EnterMessage => 0;
	public virtual int EnterSound => 0;

	public virtual TimeSpan DamageInterval => TimeSpan.FromSeconds(1);

	public DamagingRegion(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	public override void OnEnter(Mobile m)
	{
		base.OnEnter(m);

		if (!CanDamage(m))
		{
			return;
		}

		if (EnterSound > 0)
		{
			m.PlaySound(EnterSound);
		}

		if (EnterMessage > 0)
		{
			m.SendLocalizedMessage(EnterMessage);
		}

		StartTimer(m);
	}

	public override void OnExit(Mobile m)
	{
		base.OnExit(m);
		StopTimer(m);
	}

	public override void OnLocationChanged(Mobile m, Point3D oldLocation)
	{
		base.OnLocationChanged(m, oldLocation);

		if (!Contains(m.Location))
		{
			StopTimer(m);
		}
		else if (!Contains(oldLocation))
		{
			StartTimer(m);
		}
	}

	protected void StartTimer(Mobile m)
	{
		Table ??= new Dictionary<Mobile, Timer>();


		if (Table.TryGetValue(m, out var t) && t != null)
		{
			t.Start();
		}
		else
		{
			Table[m] = Timer.DelayCall(DamageInterval, DamageInterval, Damage, m);
		}
	}

	protected void StopTimer(Mobile m)
	{
		if (Table == null)
		{
			Table = new Dictionary<Mobile, Timer>();
		}


		if (Table.TryGetValue(m, out Timer t))
		{
			t?.Stop();

			Table.Remove(m);
		}
	}

	public void Damage(Mobile m)
	{
		if (CanDamage(m))
		{
			OnDamage(m);
		}
		else
		{
			StopTimer(m);
		}
	}

	protected virtual void OnDamage(Mobile m)
	{
		m.RevealingAction();
	}

	public virtual bool CanDamage(Mobile m)
	{
		if (m.IsDeadBondedPet || !m.Alive || m.Blessed || m.Map != Map || !Contains(m.Location))
		{
			return false;
		}

		if (!m.Player && (m is not BaseCreature creature || creature.GetMaster() is not PlayerMobile))
		{
			return false;
		}

		if (m.IsStaff())
		{
			return false;
		}

		return true;
	}
}

public class CrystalField : DamagingRegion
{
	// An electric wind chills your blood, making it difficult to traverse the cave unharmed.
	public override int EnterMessage => 1072396;

	public override int EnterSound => 0x22F;

	public CrystalField(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		AOS.Damage(m, Utility.Random(2, 6), 0, 0, 100, 0, 0);
	}
}

public class IcyRiver : DamagingRegion
{
	public IcyRiver(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		var dmg = Utility.Random(2, 3);

		//if (m is PlayerMobile)
		//{
		//    dmg = (int)BalmOfProtection.HandleDamage((PlayerMobile)m, dmg);
		//}

		AOS.Damage(m, dmg, 0, 0, 100, 0, 0);
	}
}

public class PoisonedSemetery : DamagingRegion
{
	public override TimeSpan DamageInterval => TimeSpan.FromSeconds(5);

	public PoisonedSemetery(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		m.FixedParticles(0x36B0, 1, 14, 0x26BB, 0x3F, 0x7, EffectLayer.Waist);
		m.PlaySound(0x229);

		AOS.Damage(m, Utility.Random(2, 3), 0, 0, 0, 100, 0);
	}
}

public class PoisonedTree : DamagingRegion
{
	public override TimeSpan DamageInterval => TimeSpan.FromSeconds(1);

	public PoisonedTree(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		m.FixedEffect(0x374A, 1, 17);
		m.PlaySound(0x1E1);
		m.LocalOverheadMessage(MessageType.Regular, 0x21, 1074165); // You feel dizzy from a lack of clear air

		var mod = (int)(m.Str * 0.1);

		if (mod > 10)
		{
			mod = 10;
		}

		m.AddStatMod(new StatMod(StatType.Str, "Poisoned Tree Str", mod * -1, TimeSpan.FromSeconds(1)));

		mod = (int)(m.Int * 0.1);

		if (mod > 10)
		{
			mod = 10;
		}

		m.AddStatMod(new StatMod(StatType.Int, "Poisoned Tree Int", mod * -1, TimeSpan.FromSeconds(1)));
	}
}

public class ParoxysmusBossEntry : DamagingRegion
{
	public override TimeSpan DamageInterval => TimeSpan.FromSeconds(2);

	public ParoxysmusBossEntry(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	public override void OnEnter(Mobile m)
	{
		if (ParoxysmusAltar.IsUnderEffects(m))
		{
			m.SendLocalizedMessage(1074604); // The slimy ointment continues to protect you from the corrosive river.
		}
		else
		{
			m.MoveToWorld(new Point3D(6537, 506, -50), m.Map);
			m.Kill();
		}
	}

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		if (!ParoxysmusAltar.IsUnderEffects(m))
		{
			m.MoveToWorld(new Point3D(6537, 506, -50), m.Map);
			m.Kill();
		}
	}
}

public class AcidRiver : DamagingRegion
{
	public override TimeSpan DamageInterval => TimeSpan.FromSeconds(2);

	public AcidRiver(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		if (m.Location.X > 6484 && m.Location.Y > 500)
		{
			m.Kill();
		}
		else
		{
			m.FixedParticles(0x36B0, 1, 14, 0x26BB, 0x3F, 0x7, EffectLayer.Waist);
			m.PlaySound(0x229);

			var damage = 0;

			damage += (int)Math.Pow(m.Location.X - 6200, 0.5);
			damage += (int)Math.Pow(m.Location.Y - 330, 0.5);

			if (damage > 20)
			{
				// The acid river is much stronger here. You realize that allowing the acid to touch your flesh will surely kill you.
				m.SendLocalizedMessage(1074567);
			}
			else if (damage > 10)
			{
				// The acid river has gotten deeper. The concentration of acid is significantly stronger.
				m.SendLocalizedMessage(1074566);
			}
			else
			{
				// The acid river burns your skin.
				m.SendLocalizedMessage(1074565);
			}

			AOS.Damage(m, damage, 0, 0, 0, 100, 0);
		}
	}
}

public class TheLostCityEntry : DamagingRegion
{
	public override TimeSpan DamageInterval => TimeSpan.FromMilliseconds(500);

	public TheLostCityEntry(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{ }

	protected override void OnDamage(Mobile m)
	{
		base.OnDamage(m);

		if (m is Kodar)
			return;

		m.FixedParticles(0x36B0, 1, 14, 0x26BB, 0x3F, 0x7, EffectLayer.Waist);
		m.PlaySound(0x229);

		var damage = Utility.RandomMinMax(10, 20);

		AOS.Damage(m, damage, 0, 0, 0, 100, 0);
	}
}
