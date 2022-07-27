using Server.Mobiles;
using System;

namespace Server.Items;

public class DeathVortexTrap : BaseTrap
{
	private Timer m_Timer;

	[Constructable]
	public DeathVortexTrap()
		: base(0x3789)
	{
		Hue = 2070;
		Movable = false;

		m_Timer = new InternalTimer(this);
		m_Timer.Start();
	}

	public DeathVortexTrap(Serial serial)
		: base(serial)
	{
	}

	public override void OnDelete()
	{
		m_Timer.Stop();

		base.OnDelete();
	}

	protected override bool PassivelyTriggered => true;
	protected override TimeSpan PassiveTriggerDelay => TimeSpan.FromSeconds(2.0);
	protected override int PassiveTriggerRange => 3;
	protected override TimeSpan ResetDelay => TimeSpan.FromSeconds(0.2);

	protected override void OnTrigger(Mobile from)
	{
		if (from.IsStaff())
			return;

		if (from.Alive && CheckRange(from.Location, 1) && from is not ClockworkExodus)
			StamManaDrain(from);
	}

	private static void StamManaDrain(Mobile defender)
	{
		switch (Utility.Random(2)) // 50%/50% for stamina leech or mana leech
		{
			case 0:
			{
				if (defender.Alive)
				{
					int manaToLeech = (int)(defender.Mana * 0.6); // defender loses 1/2 of their mana
					defender.Mana -= manaToLeech;
				}
				break;
			}
			case 1:
			{
				if (defender.Alive)
				{
					int stamToLeech = (int)(defender.Stam * 0.7); // defender loses 9/10 of their stamina
					defender.Stam -= stamToLeech;
				}
				break;
			}
		}

		defender.SendLocalizedMessage(1152694, "", 0x22); // Your life force is drained by the death vortex! 
	}


	private class InternalTimer : Timer
	{
		private readonly DeathVortexTrap m_Item;

		public InternalTimer(DeathVortexTrap item) : base(TimeSpan.FromSeconds(15.0))
		{
			m_Item = item;

			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			if (m_Item != null)
				m_Item.Delete();

			Stop();
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		m_Timer = new InternalTimer(this);
		m_Timer.Start();
	}
}
