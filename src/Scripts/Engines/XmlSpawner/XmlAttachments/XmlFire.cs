using Server.Items;
using Server.Spells;
using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlFire : XmlAttachment
	{
		private DateTime m_EndTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Damage { get; set; } = 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Refractory { get; set; } = TimeSpan.FromSeconds(5);

		[CommandProperty(AccessLevel.GameMaster)]
		public int Range { get; set; } = 5;

		// These are the various ways in which the message attachment can be constructed.
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlFire(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlFire(int damage)
		{
			Damage = damage;
		}

		[Attachable]
		public XmlFire(int damage, double refractory)
		{
			Damage = damage;
			Refractory = TimeSpan.FromSeconds(refractory);

		}

		[Attachable]
		public XmlFire(int damage, double refractory, double expiresin)
		{
			Damage = damage;
			Expiration = TimeSpan.FromMinutes(expiresin);
			Refractory = TimeSpan.FromSeconds(refractory);
		}


		// note that this method will be called when attached to either a mobile or a weapon
		// when attached to a weapon, only that weapon will do additional damage
		// when attached to a mobile, any weapon the mobile wields will do additional damage
		public override void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven)
		{
			// if it is still refractory then return
			if (DateTime.UtcNow < m_EndTime) return;

			int damage = 0;

			if (Damage > 0)
				damage = Utility.Random(Damage);

			if (defender != null && attacker != null && damage > 0)
			{
				attacker.MovingParticles(defender, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
				attacker.PlaySound(0x15E);

				SpellHelper.Damage(TimeSpan.Zero, defender, attacker, damage, 0, 100, 0, 0, 0);

				m_EndTime = DateTime.UtcNow + Refractory;
			}
		}

		public override bool HandlesOnMovement => true;

		public override void OnMovement(MovementEventArgs e)
		{
			base.OnMovement(e);

			if (e.Mobile == null || e.Mobile.AccessLevel > AccessLevel.Player)
				return;

			if (AttachedTo is Item item && (item.Parent == null) && Utility.InRange(e.Mobile.Location, item.Location, Range))
			{
				OnTrigger(null, e.Mobile);
			}
			else
				return;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write(Range);
			writer.Write(Damage);
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
					{
						Range = reader.ReadInt();
						Damage = reader.ReadInt();
						Refractory = reader.ReadTimeSpan();
						TimeSpan remaining = reader.ReadTimeSpan();
						m_EndTime = DateTime.UtcNow + remaining;
						break;
					}
			}
		}

		public override string OnIdentify(Mobile from)
		{
			string msg;
			if (Expiration > TimeSpan.Zero)
			{
				msg = $"Fire Damage {Damage} expires in {Expiration.TotalMinutes} mins";
			}
			else
			{
				msg = $"Fire Damage {Damage}";
			}

			if (Refractory > TimeSpan.Zero)
			{
				return $"{msg} : {Refractory.TotalSeconds} secs between uses";
			}
			else
				return msg;
		}

		public override void OnTrigger(object activator, Mobile m)
		{
			if (m == null)
				return;

			// if it is still refractory then return
			if (DateTime.UtcNow < m_EndTime) return;

			int damage = 0;

			if (Damage > 0)
				damage = Utility.Random(Damage);

			if (damage > 0)
			{
				m.MovingParticles(m, 0x36D4, 7, 0, false, true, 9502, 4019, 0x160);
				m.PlaySound(0x15E);
				SpellHelper.Damage(TimeSpan.Zero, m, damage, 0, 100, 0, 0, 0);
			}

			m_EndTime = DateTime.UtcNow + Refractory;

		}
	}
}
