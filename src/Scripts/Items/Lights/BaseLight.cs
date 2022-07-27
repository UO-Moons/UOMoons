using System;

namespace Server.Items;

public abstract class BaseLight : BaseItem
{
	private Timer m_Timer;
	private DateTime m_End;
	private bool m_Burning;
	private TimeSpan m_Duration = TimeSpan.Zero;

	public abstract int LitItemId { get; }

	public virtual int UnlitItemId => 0;
	public virtual int BurntOutItemId => 0;

	public virtual int LitSound => 0x47;
	public virtual int UnlitSound => 0x3be;
	public virtual int BurntOutSound => 0x4b8;

	public const bool Burnout = false;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Burning
	{
		get => m_Burning;
		init
		{
			if (m_Burning == value)
				return;
			m_Burning = true;
			DoTimer(m_Duration);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	private bool BurntOut { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Protected { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan Duration
	{
		get
		{
			if (m_Duration != TimeSpan.Zero && m_Burning)
			{
				return m_End - DateTime.UtcNow;
			}

			return m_Duration;
		}

		set => m_Duration = value;
	}

	[Constructable]
	public BaseLight(int itemId) : base(itemId)
	{
	}

	public BaseLight(Serial serial) : base(serial)
	{
	}

	public virtual void PlayLitSound()
	{
		if (LitSound == 0)
			return;

		Point3D loc = GetWorldLocation();
		Effects.PlaySound(loc, Map, LitSound);
	}

	public virtual void PlayUnlitSound()
	{
		int sound = UnlitSound;

		if (BurntOut && BurntOutSound != 0)
			sound = BurntOutSound;


		if (sound == 0)
			return;

		Point3D loc = GetWorldLocation();
		Effects.PlaySound(loc, Map, sound);
	}

	public virtual void Ignite()
	{
		if (BurntOut)
			return;

		PlayLitSound();

		m_Burning = true;
		ItemId = LitItemId;
		DoTimer(m_Duration);
	}

	public virtual void Douse()
	{
		m_Burning = false;

		if (BurntOut && BurntOutItemId != 0)
			ItemId = BurntOutItemId;
		else
			ItemId = UnlitItemId;

		if (BurntOut)
			m_Duration = TimeSpan.Zero;
		else if (m_Duration != TimeSpan.Zero)
			m_Duration = m_End - DateTime.UtcNow;

		m_Timer?.Stop();

		PlayUnlitSound();
	}

	public virtual void Burn()
	{
		BurntOut = true;
		Douse();
	}

	private void DoTimer(TimeSpan delay)
	{
		m_Duration = delay;

		m_Timer?.Stop();

		if (delay == TimeSpan.Zero)
			return;

		m_End = DateTime.UtcNow + delay;

		m_Timer = new InternalTimer(this, delay);
		m_Timer.Start();
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (BurntOut)
			return;

		if (Protected && from.AccessLevel == AccessLevel.Player)
			return;

		if (!from.InRange(GetWorldLocation(), 2))
			return;

		if (m_Burning)
		{
			if (UnlitItemId != 0)
				Douse();
		}
		else
		{
			Ignite();
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);
		writer.Write(BurntOut);
		writer.Write(m_Burning);
		writer.Write(m_Duration);
		writer.Write(Protected);

		if (m_Burning && m_Duration != TimeSpan.Zero)
			writer.WriteDeltaTime(m_End);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				BurntOut = reader.ReadBool();
				m_Burning = reader.ReadBool();
				m_Duration = reader.ReadTimeSpan();
				Protected = reader.ReadBool();

				if (m_Burning && m_Duration != TimeSpan.Zero)
					DoTimer(reader.ReadDeltaTime() - DateTime.UtcNow);

				break;
			}
		}
	}

	private class InternalTimer : Timer
	{
		private readonly BaseLight m_Light;

		public InternalTimer(BaseLight light, TimeSpan delay) : base(delay)
		{
			m_Light = light;
			Priority = TimerPriority.FiveSeconds;
		}

		protected override void OnTick()
		{
			if (m_Light is { Deleted: false })
				m_Light.Burn();
		}
	}
}
