using System;

namespace Server.Items;

public delegate void SpinCallback(ISpinningWheel sender, Mobile from, int hue);

public interface ISpinningWheel
{
	bool Spinning { get; }
	void BeginSpin(SpinCallback callback, Mobile from, int hue);
}

public class SpinningwheelEastAddon : BaseAddon, ISpinningWheel
{
	public override BaseAddonDeed Deed => new SpinningwheelEastDeed();

	[Constructable]
	public SpinningwheelEastAddon()
	{
		AddComponent(new AddonComponent(0x1019), 0, 0, 0);
	}

	public SpinningwheelEastAddon(Serial serial) : base(serial)
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

	private Timer m_Timer;

	public override void OnComponentLoaded(AddonComponent c)
	{
		switch (c.ItemId)
		{
			case 0x1016:
			case 0x101A:
			case 0x101D:
			case 0x10A5: --c.ItemId; break;
		}
	}

	public bool Spinning => m_Timer != null;

	public void BeginSpin(SpinCallback callback, Mobile from, int hue)
	{
		m_Timer = new SpinTimer(this, callback, from, hue);
		m_Timer.Start();

		foreach (AddonComponent c in Components)
		{
			switch (c.ItemId)
			{
				case 0x1015:
				case 0x1019:
				case 0x101C:
				case 0x10A4: ++c.ItemId; break;
			}
		}
	}

	public void EndSpin(SpinCallback callback, Mobile from, int hue)
	{
		if (m_Timer != null)
			m_Timer.Stop();

		m_Timer = null;

		foreach (AddonComponent c in Components)
		{
			switch (c.ItemId)
			{
				case 0x1016:
				case 0x101A:
				case 0x101D:
				case 0x10A5: --c.ItemId; break;
			}
		}

		callback?.Invoke(this, from, hue);
	}

	private class SpinTimer : Timer
	{
		private readonly SpinningwheelEastAddon m_Wheel;
		private readonly SpinCallback m_Callback;
		private readonly Mobile m_From;
		private readonly int m_Hue;

		public SpinTimer(SpinningwheelEastAddon wheel, SpinCallback callback, Mobile from, int hue) : base(TimeSpan.FromSeconds(3.0))
		{
			m_Wheel = wheel;
			m_Callback = callback;
			m_From = from;
			m_Hue = hue;
			Priority = TimerPriority.TwoFiftyMs;
		}

		protected override void OnTick()
		{
			m_Wheel.EndSpin(m_Callback, m_From, m_Hue);
		}
	}
}

public class SpinningwheelEastDeed : BaseAddonDeed
{
	public override BaseAddon Addon => new SpinningwheelEastAddon();
	public override int LabelNumber => 1044341;  // spining wheel (east)

	[Constructable]
	public SpinningwheelEastDeed()
	{
	}

	public SpinningwheelEastDeed(Serial serial) : base(serial)
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