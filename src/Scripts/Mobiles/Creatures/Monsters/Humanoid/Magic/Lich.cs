using Server.Items;
using Server.Spells;

namespace Server.Mobiles
{
	[CorpseName("a liche's corpse")]
	public class Lich : BaseCreature
	{
		[Constructable]
		public Lich() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a lich";
			Body = 24;
			BaseSoundID = 0x3E9;

			SetStr(171, 200);
			SetDex(126, 145);
			SetInt(276, 305);

			SetHits(103, 120);

			SetDamage(24, 26);

			SetDamageType(ResistanceType.Physical, 10);
			SetDamageType(ResistanceType.Cold, 40);
			SetDamageType(ResistanceType.Energy, 50);

			SetResistance(ResistanceType.Physical, 40, 60);
			SetResistance(ResistanceType.Fire, 20, 30);
			SetResistance(ResistanceType.Cold, 50, 60);
			SetResistance(ResistanceType.Poison, 55, 65);
			SetResistance(ResistanceType.Energy, 40, 50);


			SetSkill(SkillName.Necromancy, 89, 99.1);
			SetSkill(SkillName.SpiritSpeak, 90.0, 99.0);

			SetSkill(SkillName.EvalInt, 100.0);
			SetSkill(SkillName.Magery, 70.1, 80.0);
			SetSkill(SkillName.Meditation, 85.1, 95.0);
			SetSkill(SkillName.MagicResist, 80.1, 100.0);
			SetSkill(SkillName.Tactics, 70.1, 90.0);

			Fame = 8000;
			Karma = -8000;

			VirtualArmor = 50;
			PackItem(new GnarledStaff());
			PackNecroReg(17, 24);
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.MedScrolls, 2);
		}

		public override OppositionGroup OppositionGroup => OppositionGroup.FeyAndUndead;

		public override bool CanRummageCorpses => true;
		public override bool BleedImmune => true;
		public override Poison PoisonImmune => Poison.Lethal;
		public override int TreasureMapLevel => 3;

		public override void OnDamagedBySpell(Mobile attacker)
		{
			base.OnDamagedBySpell(attacker);

			DoSpecialAbility(this, attacker);
		}

		public override void OnGotMeleeAttack(Mobile attacker)
		{
			base.OnGotMeleeAttack(attacker);

			DoSpecialAbility(this, attacker);
		}

		public static void DoSpecialAbility(BaseCreature from, Mobile target)
		{
			if (from == null || from.Summoned)
				return;

			if (0.2 >= Utility.RandomDouble()) // 20% chance to more ratmen
				SpawnMobiles(from, target);

			if (0.65 >= Utility.RandomDouble() && from.Hits < (from.HitsMax / 2) && !from.IsBodyMod) // the lich is low on life, polymorph into a Skeleton
				Polymorph(from);

			if (from.IsBodyMod && from.Hits >= (from.HitsMax / 2))
			{
				from.BodyMod = 0;
				from.HueMod = -1;
			}
		}

		public static void SpawnMobiles(Mobile from, Mobile target)
		{
			if (from == null)
				return;

			if (!Ability.TooManyCreatures(typeof(LichSkeleton), 5, from))
			{
				int count = Utility.RandomMinMax(1, 3);

				for (int i = 0; i < count; ++i)
				{
					BaseCreature bc = new LichSkeleton();
					bc.MoveToWorld(Ability.RandomCloseLocation(from), from.Map);
					bc.Combatant = target;
				}
			}
		}

		public static void Polymorph(Mobile m)
		{
			if (m.IsBodyMod || m.Mounted) //check mounted incase some GM hates the players
				return;

			m.BodyMod = 50;
			m.HueMod = 0;
			m.Name = "a skeleton";
		}

		public Lich(Serial serial) : base(serial)
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
