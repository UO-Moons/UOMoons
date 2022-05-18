using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlAnimate : XmlAttachment
	{
		private DateTime m_EndTime;
		private LoopTimer m_Timer;

		// a serial constructor is REQUIRED
		public XmlAnimate(ASerial serial)
			: base(serial)
		{
		}

		[Attachable]
		public XmlAnimate()
		{
		}

		[Attachable]
		public XmlAnimate(int animation)
		{
			AnimationValue = animation;
		}

		[Attachable]
		public XmlAnimate(int animation, double refractory)
		{
			AnimationValue = animation;
			Refractory = TimeSpan.FromSeconds(refractory);
		}

		[Attachable]
		public XmlAnimate(int animation, int framecount, double refractory)
		{
			AnimationValue = animation;
			FrameCount = framecount;
			Refractory = TimeSpan.FromSeconds(refractory);
		}

		[Attachable]
		public XmlAnimate(int animation, double refractory, int loopcount, int loopdelay)
		{
			LoopCount = loopcount;
			LoopDelay = loopdelay;
			AnimationValue = animation;
			Refractory = TimeSpan.FromSeconds(refractory);
		}

		[Attachable]
		public XmlAnimate(int animation, int framecount, double refractory, int loopcount, int loopdelay)
		{
			LoopCount = loopcount;
			LoopDelay = loopdelay;
			AnimationValue = animation;
			FrameCount = framecount;
			Refractory = TimeSpan.FromSeconds(refractory);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int ProximityRange { get; set; } = 5;

		[CommandProperty(AccessLevel.GameMaster)]
		public int FrameCount { get; set; } = 7;

		[CommandProperty(AccessLevel.GameMaster)]
		public int RepeatCount { get; set; } = 1;

		[CommandProperty(AccessLevel.GameMaster)]
		public int AnimationDelay { get; set; } = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Repeat { get; set; } = false;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Forward { get; set; } = true;

		[CommandProperty(AccessLevel.GameMaster)]
		public int AnimationValue { get; set; } = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public string ActivationWord { get; set; } = null;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Refractory { get; set; } = TimeSpan.FromSeconds(5);

		[CommandProperty(AccessLevel.GameMaster)]
		public int LoopCount { get; set; } = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public int LoopDelay { get; set; } = 5;

		[CommandProperty(AccessLevel.GameMaster)]
		public int CurrentCount { get; set; } = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoAnimate
		{
			get => false;
			set
			{
				if (value == true)
					OnTrigger(null, null);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DoReset
		{
			get => false;
			set
			{
				if (value == true)
					Reset();
			}
		}
		// These are the various ways in which the message attachment can be constructed.
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments
		public override bool HandlesOnSpeech => ActivationWord != null;
		public override bool HandlesOnMovement => ProximityRange >= 0 && ActivationWord == null;
		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			// version 0
			writer.Write(CurrentCount);
			writer.Write(LoopCount);
			writer.Write(LoopDelay);
			writer.Write(ProximityRange);
			writer.Write(AnimationValue);
			writer.Write(FrameCount);
			writer.Write(RepeatCount);
			writer.Write(AnimationDelay);
			writer.Write(Forward);
			writer.Write(Repeat);
			writer.Write(ActivationWord);
			writer.Write(Refractory);
			writer.Write(m_EndTime - DateTime.UtcNow);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					// version 0
					CurrentCount = reader.ReadInt();
					LoopCount = reader.ReadInt();
					LoopDelay = reader.ReadInt();
					ProximityRange = reader.ReadInt();
					AnimationValue = reader.ReadInt();
					FrameCount = reader.ReadInt();
					RepeatCount = reader.ReadInt();
					AnimationDelay = reader.ReadInt();
					Forward = reader.ReadBool();
					Repeat = reader.ReadBool();
					ActivationWord = reader.ReadString();
					Refractory = reader.ReadTimeSpan();
					TimeSpan remaining = reader.ReadTimeSpan();
					m_EndTime = DateTime.UtcNow + remaining;
					break;
			}

			// restart any animation loops that were active
			if (CurrentCount > 0)
			{
				DoTimer(TimeSpan.FromSeconds(LoopDelay));
			}
		}

		public override string OnIdentify(Mobile from)
		{
			if (from == null || from.AccessLevel < AccessLevel.Counselor)
				return null;

			string msg = $"Animation #{AnimationValue},{FrameCount} : {Refractory.TotalSeconds} secs between uses";

			if (ActivationWord == null)
			{
				return msg;
			}
			else
			{
				return $"{msg} : trigger on '{ActivationWord}'";
			}
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			base.OnSpeech(e);

			if (e.Mobile == null || e.Mobile.AccessLevel > AccessLevel.Player)
				return;

			if (e.Speech == ActivationWord)
			{
				OnTrigger(null, e.Mobile);
			}
		}

		public override void OnMovement(MovementEventArgs e)
		{
			base.OnMovement(e);

			if (e.Mobile == null || e.Mobile.AccessLevel > AccessLevel.Player)
				return;

			if (AttachedTo is Item item && (item.Parent == null) && Utility.InRange(e.Mobile.Location, item.Location, ProximityRange))
			{
				OnTrigger(null, e.Mobile);
			}
			else
				return;
		}

		public override void OnAttach()
		{
			base.OnAttach();

			// only attach to mobiles
			if (AttachedTo is not Mobile)
			{
				Delete();
			}
		}

		public void Reset()
		{
			if (m_Timer != null)
				m_Timer.Stop();

			CurrentCount = 0;
			m_EndTime = DateTime.UtcNow;
		}

		public void Animate()
		{
			// play a animation
			if (AttachedTo is Mobile mobile && AnimationValue >= 0)
			{
				mobile.Animate(AnimationValue, FrameCount, RepeatCount, Forward, Repeat, AnimationDelay);
			}

			UpdateRefractory();

			CurrentCount--;
		}

		public void UpdateRefractory()
		{
			m_EndTime = DateTime.UtcNow + Refractory;
		}

		public override void OnTrigger(object activator, Mobile m)
		{
			if (DateTime.UtcNow < m_EndTime)
				return;

			if (LoopCount > 0)
			{
				CurrentCount = LoopCount;
				// check to make sure the timer is running
				DoTimer(TimeSpan.FromSeconds(LoopDelay));
			}
			else
			{
				Animate();
			}
		}

		private void DoTimer(TimeSpan delay)
		{
			if (m_Timer != null)
				m_Timer.Stop();

			m_Timer = new LoopTimer(this, delay);
			m_Timer.Start();
		}

		private class LoopTimer : Timer
		{
			public readonly TimeSpan m_delay;
			private readonly XmlAnimate m_attachment;
			public LoopTimer(XmlAnimate attachment, TimeSpan delay)
				: base(delay, delay)
			{
				Priority = TimerPriority.OneSecond;

				m_attachment = attachment;
				m_delay = delay;
			}

			protected override void OnTick()
			{
				if (m_attachment != null && !m_attachment.Deleted)
				{
					m_attachment.Animate();

					if (m_attachment.CurrentCount <= 0)
						Stop();
				}
				else
				{
					Stop();
				}
			}
		}
	}
}
