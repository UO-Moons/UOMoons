using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlDex : XmlAttachment
	{
		private readonly TimeSpan m_Duration = TimeSpan.FromSeconds(30.0);       // default 30 sec duration

		[CommandProperty(AccessLevel.GameMaster)]
		public int Value { get; set; } = 10;

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlDex(ASerial serial) : base(serial)
		{
		}


		[Attachable]
		public XmlDex()
		{
		}

		[Attachable]
		public XmlDex(int value)
		{
			Value = value;
		}

		[Attachable]
		public XmlDex(int value, double duration)
		{
			Value = value;
			m_Duration = TimeSpan.FromSeconds(duration);
		}

		public override void OnAttach()
		{
			base.OnAttach();

			// apply the mod
			if (AttachedTo is Mobile mobile)
			{
				mobile.AddStatMod(new StatMod(StatType.Dex, "XmlDex" + Name, Value, m_Duration));
			}
			// and then remove the attachment
			Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Delete));
			//Delete();
		}
	}
}
