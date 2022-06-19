using Server.Items;
using System;

namespace Server.Mobiles
{
	[CorpseName("an ore elemental corpse")]
	public class ValoriteElemental : BaseCreature
	{
		private DateTime m_Delay = DateTime.UtcNow;

		[Constructable]
		public ValoriteElemental() : this(2)
		{
		}

		[Constructable]
		public ValoriteElemental(int oreAmount) : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			// TODO: Gas attack
			Name = "a valorite elemental";
			Body = 112;
			BaseSoundID = 268;

			SetStr(226, 255);
			SetDex(126, 145);
			SetInt(71, 92);

			SetHits(136, 153);

			SetDamage(28);

			SetDamageType(ResistanceType.Physical, 25);
			SetDamageType(ResistanceType.Fire, 25);
			SetDamageType(ResistanceType.Cold, 25);
			SetDamageType(ResistanceType.Energy, 25);

			SetResistance(ResistanceType.Physical, 65, 75);
			SetResistance(ResistanceType.Fire, 50, 60);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 50, 60);
			SetResistance(ResistanceType.Energy, 40, 50);

			SetSkill(SkillName.MagicResist, 50.1, 95.0);
			SetSkill(SkillName.Tactics, 60.1, 100.0);
			SetSkill(SkillName.Wrestling, 60.1, 100.0);

			Fame = 3500;
			Karma = -3500;

			VirtualArmor = 38;

			Item ore = new ValoriteOre(oreAmount)
			{
				ItemId = 0x19B9
			};
			PackItem(ore);
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.FilthyRich);
			AddLoot(LootPack.Gems, 4);
		}

		public override bool AutoDispel => true;
		public override bool BleedImmune => true;
		public override int TreasureMapLevel => 1;

		public override void AlterMeleeDamageTo(Mobile to, ref int damage)
		{
			if (0.5 >= Utility.RandomDouble())
			{
				Ability.DamageArmor(to, 1, 5);
			}
		}

		public override void AlterMeleeDamageFrom(Mobile from, ref int damage)
		{
			if (from != null)
			{
				int hitback = damage;
				AOS.Damage(from, this, hitback, 100, 0, 0, 0, 0);
			}

			if (from is BaseCreature bc)
			{
				if (bc.Controlled || bc.BardTarget == this)
					damage = 0; // Immune to pets and provoked creatures
			}
		}

		public override void CheckReflect(Mobile caster, ref bool reflect)
		{
			reflect = true; // Every spell is reflected back to the caster
		}

		public override void OnActionCombat()
		{
			if (DateTime.Now > m_Delay)
			{
				Ability.Aura(this, this, 0, 0, 3, 3, 4, "It exhails a cloud of noxious vapors");
				m_Delay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(5, 10));
			}
			base.OnActionCombat();
		}

		public ValoriteElemental(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
