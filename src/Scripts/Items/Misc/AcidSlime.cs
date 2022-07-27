using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public class AcidSlime : BaseItem
{
	private readonly TimeSpan m_Duration;
	private readonly int m_MinDamage;
	private readonly int m_MaxDamage;
	private readonly DateTime m_Created;
	private bool m_Drying;
	private readonly Timer m_Timer;

	[Constructable]
	public AcidSlime() : this(TimeSpan.FromSeconds(10.0), 5, 10)
	{
	}

	public override string DefaultName => "slime";

	[Constructable]
	public AcidSlime(TimeSpan duration, int minDamage, int maxDamage)
		: base(0x122A)
	{
		Hue = 0x3F;
		Movable = false;
		m_MinDamage = minDamage;
		m_MaxDamage = maxDamage;
		m_Created = DateTime.UtcNow;
		m_Duration = duration;
		m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), OnTick);
	}

	public override void OnAfterDelete()
	{
		if (m_Timer != null)
			m_Timer.Stop();
	}

	private void OnTick()
	{
		DateTime now = DateTime.UtcNow;
		TimeSpan age = now - m_Created;

		if (age > m_Duration)
		{
			Delete();
		}
		else
		{
			if (!m_Drying && age > (m_Duration - age))
			{
				m_Drying = true;
				ItemId = 0x122B;
			}

			List<Mobile> toDamage = (from m in GetMobilesInRange(0) let bc = m as BaseCreature where m.Alive && !m.IsDeadBondedPet && (bc == null || bc.Controlled || bc.Summoned) select m).ToList();

			for (int i = 0; i < toDamage.Count; i++)
				Damage(toDamage[i]);
		}
	}

	public override bool OnMoveOver(Mobile m)
	{
		Damage(m);
		return true;
	}

	private void Damage(Mobile m)
	{
		int damage = Utility.RandomMinMax(m_MinDamage, m_MaxDamage);
		if (Core.AOS)
			AOS.Damage(m, damage, 0, 0, 0, 100, 0);
		else
			m.Damage(damage);
	}

	public AcidSlime(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
	}

	public override void Deserialize(GenericReader reader)
	{
	}
}
