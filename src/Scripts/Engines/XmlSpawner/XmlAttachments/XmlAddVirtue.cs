using Server.Mobiles;
using System;

//using Server.Services.Virtues;

namespace Server.Engines.XmlSpawner2
{
	public class XmlAddVirtue : XmlAttachment
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public int Value { get; set; }
		public string Virtue { get; set; }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlAddVirtue(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlAddVirtue(string virtue, int value)
		{
			Value = value;
			Virtue = virtue;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Value);
			writer.Write(Virtue);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			Value = reader.ReadInt();
			Virtue = reader.ReadString();
		}

		public override void OnAttach()
		{
			base.OnAttach();

			// apply the mod
			if (AttachedTo is PlayerMobile)
			{
				// for players just add it immediately
				// lookup the virtue type
				VirtueName g = 0;
				bool valid = true;
				bool gainedPath = false;
				try
				{
					g = (VirtueName)Enum.Parse(typeof(VirtueName), Virtue, true);
				}
				catch
				{
					valid = false;
				}

				if (valid)
				{
					VirtueHelper.Award((Mobile)AttachedTo, g, Value, ref gainedPath);

					((Mobile)AttachedTo).SendMessage("Receive {0}", OnIdentify((Mobile)AttachedTo));

					if (gainedPath)
					{
						((Mobile)AttachedTo).SendMessage("You have gained a path in {0}", Virtue);
					}
				}
				else
				{
					((Mobile)AttachedTo).SendMessage("{0}: no such Virtue", Virtue);
				}
				// and then remove the attachment
				Timer.DelayCall(TimeSpan.Zero, new TimerCallback(Delete));
				//Delete();
			}
			else
			if (AttachedTo is Item)
			{
				// dont allow item attachments
				Delete();
			}
		}

		public override bool HandlesOnKilled => true;

		public override void OnKilled(Mobile killed, Mobile killer)
		{
			base.OnKilled(killed, killer);

			if (killer == null)
				return;

			VirtueName g = 0;
			bool valid = true;
			bool gainedPath = false;
			try
			{
				g = (VirtueName)Enum.Parse(typeof(VirtueName), Virtue, true);
			}
			catch
			{
				valid = false;
			}

			if (valid)
			{
				// give the killer the Virtue

				VirtueHelper.Award(killer, g, Value, ref gainedPath);

				if (gainedPath)
				{
					killer.SendMessage("You have gained a path in {0}", Virtue);
				}

				killer.SendMessage("Receive {0}", OnIdentify(killer));
			}
		}

		public override string OnIdentify(Mobile from)
		{

			return $"{Value} {Virtue} Virtue points";

		}
	}
}
