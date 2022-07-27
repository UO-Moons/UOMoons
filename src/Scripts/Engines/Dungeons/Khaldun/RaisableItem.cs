using System;

namespace Server.Items
{
	public class RaisableItem : BaseItem
	{
		private int m_MaxElevation;

		[CommandProperty(AccessLevel.GameMaster)]
		private int MaxElevation
		{
			get => m_MaxElevation;
			set
			{
				m_MaxElevation = value switch
				{
					<= 0 => 0,
					>= 60 => 60,
					_ => value
				};
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		private int MoveSound { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		private int StopSound { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan CloseDelay { get; private set; }

		[Constructable]
		public RaisableItem(int itemId) : this(itemId, 20, -1, -1, TimeSpan.FromMinutes(1.0))
		{
		}

		[Constructable]
		public RaisableItem(int itemId, int maxElevation, TimeSpan closeDelay) : this(itemId, maxElevation, -1, -1, closeDelay)
		{
		}

		[Constructable]
		public RaisableItem(int itemId, int maxElevation, int moveSound, int stopSound, TimeSpan closeDelay) : base(itemId)
		{
			Movable = false;

			m_MaxElevation = maxElevation;
			MoveSound = moveSound;
			StopSound = stopSound;
			CloseDelay = closeDelay;
		}

		private int m_Elevation;
		private RaiseTimer m_RaiseTimer;

		public bool IsRaisable => m_RaiseTimer == null;

		public void Raise()
		{
			if (!IsRaisable)
				return;

			m_RaiseTimer = new RaiseTimer(this);
			m_RaiseTimer.Start();
		}

		private class RaiseTimer : Timer
		{
			private readonly RaisableItem m_Item;
			private readonly DateTime m_CloseTime;
			private bool m_Up;
			private int m_Step;

			public RaiseTimer(RaisableItem item) : base(TimeSpan.Zero, TimeSpan.FromSeconds(0.5))
			{
				m_Item = item;
				m_CloseTime = DateTime.UtcNow + item.CloseDelay;
				m_Up = true;

				Priority = TimerPriority.TenMs;
			}

			protected override void OnTick()
			{
				if (m_Item.Deleted)
				{
					Stop();
					return;
				}

				if (m_Step++ % 3 == 0)
				{
					if (m_Up)
					{
						m_Item.Z++;

						if (++m_Item.m_Elevation >= m_Item.MaxElevation)
						{
							Stop();

							if (m_Item.StopSound >= 0)
								Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.StopSound);

							m_Up = false;
							m_Step = 0;

							TimeSpan delay = m_CloseTime - DateTime.UtcNow;
							DelayCall(delay > TimeSpan.Zero ? delay : TimeSpan.Zero, Start);

							return;
						}
					}
					else
					{
						m_Item.Z--;

						if (--m_Item.m_Elevation <= 0)
						{
							Stop();

							if (m_Item.StopSound >= 0)
								Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.StopSound);

							m_Item.m_RaiseTimer = null;

							return;
						}
					}
				}

				if (m_Item.MoveSound >= 0)
					Effects.PlaySound(m_Item.Location, m_Item.Map, m_Item.MoveSound);
			}
		}

		public RaisableItem(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.WriteEncodedInt(m_MaxElevation);
			writer.WriteEncodedInt(MoveSound);
			writer.WriteEncodedInt(StopSound);
			writer.Write(CloseDelay);

			writer.WriteEncodedInt(m_Elevation);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadEncodedInt();

			m_MaxElevation = reader.ReadEncodedInt();
			MoveSound = reader.ReadEncodedInt();
			StopSound = reader.ReadEncodedInt();
			CloseDelay = reader.ReadTimeSpan();

			int elevation = reader.ReadEncodedInt();
			Z -= elevation;
		}
	}
}
