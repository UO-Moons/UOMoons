using Server.Items;
using System;

namespace Server.Engines.XmlSpawner2
{
	public interface IXmlAttachment
	{
		ASerial Serial { get; }

		string Name { get; set; }

		TimeSpan Expiration { get; set; }

		DateTime ExpirationEnd { get; }

		DateTime CreationTime { get; }

		bool Deleted { get; }

		bool DoDelete { get; set; }

		bool CanActivateInBackpack { get; }

		bool CanActivateEquipped { get; }

		bool CanActivateInWorld { get; }

		bool HandlesOnSpeech { get; }

		void OnSpeech(SpeechEventArgs args);

		bool HandlesOnMovement { get; }

		void OnMovement(MovementEventArgs args);

		bool HandlesOnKill { get; }

		void OnKill(Mobile killed, Mobile killer);

		void OnBeforeKill(Mobile killed, Mobile killer);

		bool HandlesOnKilled { get; }

		void OnKilled(Mobile killed, Mobile killer);

		void OnBeforeKilled(Mobile killed, Mobile killer);

		/*
		bool HandlesOnSkillUse { get; }

		void OnSkillUse( Mobile m, Skill skill, bool success);
		*/

		object AttachedTo { get; set; }

		object OwnedBy { get; set; }

		bool CanEquip(Mobile from);

		void OnEquip(Mobile from);

		void OnRemoved(object parent);

		void OnAttach();

		void OnReattach();

		void OnUse(Mobile from);

		void OnUser(object target);

		bool BlockDefaultOnUse(Mobile from, object target);

		bool OnDragLift(Mobile from, Item item);

		string OnIdentify(Mobile from);

		string DisplayedProperties(Mobile from);

		void AddProperties(ObjectPropertyList list);

		string AttachedBy { get; }

		void OnDelete();

		void Delete();

		void InvalidateParentProperties();

		void SetAttachedBy(string name);

		void OnTrigger(object activator, Mobile from);

		void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven);

		int OnArmorHit(Mobile attacker, Mobile defender, Item armor, BaseWeapon weapon, int damageGiven);

		void Serialize(GenericWriter writer);

		void Deserialize(GenericReader reader);
	}

	public abstract class XmlAttachment : IXmlAttachment
	{
		// ----------------------------------------------
		// Private fields
		// ----------------------------------------------

		private object _mAttachedTo;

		private object _mOwnedBy;

		private string _mAttachedBy;

		private AttachmentTimer _mExpirationTimer;

		private TimeSpan _mExpiration = TimeSpan.Zero;     // no expiration by default

		// ----------------------------------------------
		// Public properties
		// ----------------------------------------------
		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime CreationTime { get; private set; }

		public bool Deleted { get; private set; }

		public bool DoDelete { get => false;
			set { if (value) Delete(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SerialValue => Serial.Value;

		public ASerial Serial { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Expiration
		{
			get
			{
				// if the expiration timer is running then return the remaining time
				if (_mExpirationTimer != null)
				{
					return ExpirationEnd - DateTime.UtcNow;
				}

				return _mExpiration;
			}
			set
			{
				_mExpiration = value;
				// if it is already attached to something then set the expiration timer
				if (_mAttachedTo != null)
				{
					DoTimer(_mExpiration);
				}
			}
		}

		public DateTime ExpirationEnd { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool CanActivateInBackpack => true;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool CanActivateEquipped => true;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool CanActivateInWorld => true;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool HandlesOnSpeech => false;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool HandlesOnMovement => false;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool HandlesOnKill => false;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool HandlesOnKilled => false;

		/*
		[CommandProperty( AccessLevel.GameMaster )]
		public virtual bool HandlesOnSkillUse { get{return false; } }
		*/

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual string Name { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual object Attached => _mAttachedTo;

		public virtual object AttachedTo { get => _mAttachedTo;
			set => _mAttachedTo = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual string AttachedBy => _mAttachedBy;

		public virtual object OwnedBy { get => _mOwnedBy;
			set => _mOwnedBy = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual object Owner => _mOwnedBy;

		// ----------------------------------------------
		// Private methods
		// ----------------------------------------------
		private void DoTimer(TimeSpan delay)
		{
			ExpirationEnd = DateTime.UtcNow + delay;

			_mExpirationTimer?.Stop();

			_mExpirationTimer = new AttachmentTimer(this, delay);
			_mExpirationTimer.Start();
		}

		// a timer that can be implement limited lifetime attachments
		private class AttachmentTimer : Timer
		{
			private readonly XmlAttachment m_Attachment;

			public AttachmentTimer(XmlAttachment attachment, TimeSpan delay)
				: base(delay)
			{
				Priority = TimerPriority.OneSecond;

				m_Attachment = attachment;
			}

			protected override void OnTick()
			{
				m_Attachment.Delete();
			}
		}

		// ----------------------------------------------
		// Constructors
		// ----------------------------------------------
		protected XmlAttachment()
		{
			CreationTime = DateTime.UtcNow;

			// get the next unique serial id
			Serial = ASerial.NewSerial();

			// register the attachment in the serial keyed dictionary
			XmlAttach.HashSerial(Serial, this);
		}

		// needed for deserialization
		protected XmlAttachment(ASerial serial)
		{
			Serial = serial;
		}

		// ----------------------------------------------
		// Public methods
		// ----------------------------------------------

		public static void Initialize()
		{
			XmlAttach.CleanUp();
		}

		public virtual bool CanEquip(Mobile from)
		{
			return true;
		}

		public virtual void OnEquip(Mobile from)
		{
		}

		public virtual void OnRemoved(object parent)
		{
		}

		public virtual void OnAttach()
		{
			// start up the expiration timer on attachment
			if (_mExpiration > TimeSpan.Zero)
				DoTimer(_mExpiration);
		}

		public virtual void OnReattach()
		{
		}

		public virtual void OnUse(Mobile from)
		{
		}

		public virtual void OnUser(object target)
		{
		}

		public virtual bool BlockDefaultOnUse(Mobile from, object target)
		{
			return false;
		}

		public virtual bool OnDragLift(Mobile from, Item item)
		{
			return true;
		}

		public void SetAttachedBy(string name)
		{
			_mAttachedBy = name;
		}

		public virtual void OnSpeech(SpeechEventArgs args)
		{
		}

		public virtual void OnMovement(MovementEventArgs args)
		{
		}

		public virtual void OnKill(Mobile killed, Mobile killer)
		{
		}

		public virtual void OnBeforeKill(Mobile killed, Mobile killer)
		{
		}

		public virtual void OnKilled(Mobile killed, Mobile killer)
		{
		}

		public virtual void OnBeforeKilled(Mobile killed, Mobile killer)
		{
		}

		/*
		public virtual void OnSkillUse( Mobile m, Skill skill, bool success)
		{
		}
		*/

		public virtual void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven)
		{
		}

		public virtual int OnArmorHit(Mobile attacker, Mobile defender, Item armor, BaseWeapon weapon, int damageGiven)
		{
			return 0;
		}

		public virtual string OnIdentify(Mobile from)
		{
			return null;
		}

		public virtual string DisplayedProperties(Mobile from)
		{
			return OnIdentify(from);
		}


		public virtual void AddProperties(ObjectPropertyList list)
		{
		}

		public void InvalidateParentProperties()
		{
			if (AttachedTo is Item item)
			{
				item.InvalidateProperties();
			}
		}

		public static void SafeItemDelete(Item item)
		{
			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(DeleteItemCallback), new object[] { item });

		}

		public static void DeleteItemCallback(object state)
		{
			object[] args = (object[])state;

			if (args[0] is Item item)
			{
				// delete the item
				item.Delete();
			}
		}

		public static void SafeMobileDelete(Mobile mob)
		{
			Timer.DelayCall(TimeSpan.Zero, new TimerStateCallback(DeleteMobileCallback), new object[] { mob });

		}

		public static void DeleteMobileCallback(object state)
		{
			object[] args = (object[])state;


			if (args[0] is Mobile mob)
			{
				// delete the mobile
				mob.Delete();
			}
		}

		public void Delete()
		{
			if (Deleted) return;

			Deleted = true;

			_mExpirationTimer?.Stop();

			OnDelete();

			// dereference the attachment object
			AttachedTo = null;
			OwnedBy = null;
		}

		public virtual void OnDelete()
		{
		}

		public virtual void OnTrigger(object activator, Mobile from)
		{
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(2);
			// version 2
			writer.Write(_mAttachedBy);
			switch (OwnedBy)
			{
				// version 1
				case Item item:
					writer.Write(0);
					writer.Write(item);
					break;
				case Mobile mobile:
					writer.Write(1);
					writer.Write(mobile);
					break;
				default:
					writer.Write(-1);
					break;
			}

			// version 0
			writer.Write(Name);
			// if there are any active timers, then serialize
			writer.Write(_mExpiration);
			if (_mExpirationTimer != null)
			{
				writer.Write(ExpirationEnd - DateTime.UtcNow);
			}
			else
			{
				writer.Write(TimeSpan.Zero);
			}
			writer.Write(CreationTime);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 2:
					_mAttachedBy = reader.ReadString();
					goto case 1;
				case 1:
					int owned = reader.ReadInt();
					OwnedBy = owned switch
					{
						0 => reader.ReadItem(),
						1 => reader.ReadMobile(),
						_ => null
					};

					goto case 0;
				case 0:
					// version 0
					Name = reader.ReadString();
					_mExpiration = reader.ReadTimeSpan();
					TimeSpan remaining = reader.ReadTimeSpan();

					if (remaining > TimeSpan.Zero)
						DoTimer(remaining);

					CreationTime = reader.ReadDateTime();
					break;
			}
		}
	}
}
