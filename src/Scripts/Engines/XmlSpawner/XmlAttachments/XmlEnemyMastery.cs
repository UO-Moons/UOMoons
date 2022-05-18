using Server.Items;
using Server.Mobiles;
using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlEnemyMastery : XmlAttachment
	{
		private string m_Enemy;
		private Type m_EnemyType;

		[CommandProperty(AccessLevel.GameMaster)]
		public int Chance { get; set; } = 20;

		[CommandProperty(AccessLevel.GameMaster)]
		public int PercentIncrease { get; set; } = 50;

		[CommandProperty(AccessLevel.GameMaster)]
		public string Enemy
		{
			get => m_Enemy;
			set
			{
				m_Enemy = value;
				// look up the type
				m_EnemyType = SpawnerType.GetType(m_Enemy);
			}
		}


		// These are the various ways in which the message attachment can be constructed.
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlEnemyMastery(ASerial serial) : base(serial)
		{
		}

		[Attachable]
		public XmlEnemyMastery(string enemy)
		{
			Enemy = enemy;
		}

		[Attachable]
		public XmlEnemyMastery(string enemy, int increase)
		{
			PercentIncrease = increase;
			Enemy = enemy;
		}

		[Attachable]
		public XmlEnemyMastery(string enemy, int chance, int increase)
		{
			Chance = chance;
			PercentIncrease = increase;
			Enemy = enemy;
		}

		[Attachable]
		public XmlEnemyMastery(string enemy, int chance, int increase, double expiresin)
		{
			Chance = chance;
			PercentIncrease = increase;
			Expiration = TimeSpan.FromMinutes(expiresin);
			Enemy = enemy;
		}

		public override void OnAttach()
		{
			base.OnAttach();

			if (AttachedTo is Mobile)
			{
				Mobile m = AttachedTo as Mobile;
				Effects.PlaySound(m, m.Map, 516);
				m.SendMessage(string.Format("You gain the power of Enemy Mastery over {0}", Enemy));
			}
		}


		// note that this method will be called when attached to either a mobile or a weapon
		// when attached to a weapon, only that weapon will do additional damage
		// when attached to a mobile, any weapon the mobile wields will do additional damage
		public override void OnWeaponHit(Mobile attacker, Mobile defender, BaseWeapon weapon, int damageGiven)
		{
			if (Chance <= 0 || Utility.Random(100) > Chance)
				return;

			if (defender != null && attacker != null && m_EnemyType != null)
			{

				// is the defender the correct type?
				if (defender.GetType() == m_EnemyType || defender.GetType().IsSubclassOf(m_EnemyType))
				{
					defender.Damage(damageGiven * PercentIncrease / 100, attacker);
				}
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			if (AttachedTo is Mobile)
			{
				Mobile m = AttachedTo as Mobile;
				if (!m.Deleted)
				{
					Effects.PlaySound(m, m.Map, 958);
					m.SendMessage(string.Format("Your power of Enemy Mastery over {0} fades..", Enemy));
				}
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			// version 0
			writer.Write(PercentIncrease);
			writer.Write(Chance);
			writer.Write(m_Enemy);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
			// version 0
			PercentIncrease = reader.ReadInt();
			Chance = reader.ReadInt();
			Enemy = reader.ReadString();
		}

		public override string OnIdentify(Mobile from)
		{
			string msg = Expiration > TimeSpan.Zero
				? $"Enemy Mastery : +{PercentIncrease}% damage vs {m_Enemy}, {Chance}%, hitchance expires in {Expiration.TotalMinutes} mins"
				: $"Enemy Mastery : +{PercentIncrease}% damage vs {m_Enemy}, {Chance}% hitchance";
			return msg;
		}
	}
}
