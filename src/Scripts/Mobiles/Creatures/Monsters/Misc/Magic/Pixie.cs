using Server.Items;
using System;

namespace Server.Mobiles
{
	[CorpseName("a pixie corpse")]
	public class Pixie : BaseCreature
	{
		public override bool InitialInnocent => true;

		[Constructable]
		public Pixie() : base(AIType.AI_Mage, FightMode.Evil, 10, 1, 0.2, 0.4)
		{
			Name = NameList.RandomName("pixie");
			Body = 128;
			BaseSoundID = 0x467;

			SetStr(21, 30);
			SetDex(301, 400);
			SetInt(201, 250);

			SetHits(13, 18);

			SetDamage(9, 15);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 80, 90);
			SetResistance(ResistanceType.Fire, 40, 50);
			SetResistance(ResistanceType.Cold, 40, 50);
			SetResistance(ResistanceType.Poison, 40, 50);
			SetResistance(ResistanceType.Energy, 40, 50);

			SetSkill(SkillName.EvalInt, 90.1, 100.0);
			SetSkill(SkillName.Magery, 90.1, 100.0);
			SetSkill(SkillName.Meditation, 90.1, 100.0);
			SetSkill(SkillName.MagicResist, 100.5, 150.0);
			SetSkill(SkillName.Tactics, 10.1, 20.0);
			SetSkill(SkillName.Wrestling, 10.1, 12.5);

			Fame = 7000;
			Karma = 7000;

			VirtualArmor = 100;
			if (0.02 > Utility.RandomDouble())
				PackStatue();
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.LowScrolls);
			AddLoot(LootPack.Gems, 2);

			if (0.08 > Utility.RandomDouble())
				switch (Utility.Random(11))
				{
					default:
					case 0: PackItem(new StatueSouth()); break;
					case 1: PackItem(new StatueSouth2()); break;
					case 2: PackItem(new StatueNorth()); break;
					case 3: PackItem(new StatueWest()); break;
					case 4: PackItem(new StatueEast()); break;
					case 5: PackItem(new StatueEast2()); break;
					case 6: PackItem(new StatueSouthEast()); break;
					case 7: PackItem(new BustSouth()); break;
					case 8: PackItem(new BustEast()); break;
					case 9: PackItem(new StatuePegasus()); break;
					case 10: PackItem(new StatuePegasus2()); break;
				};
		}

		public override void OnDeath(Container c)
		{
			base.OnDeath(c);

			if (Utility.RandomDouble() < 0.35)
				c.DropItem(new PixieLeg());
		}

		public override HideType HideType => HideType.Spined;
		public override int Hides => 5;
		public override int Meat => 1;

		public override bool OnBeforeDeath()
		{
			if (Combatant != null)
			{
				Mobile targ = (Mobile)Combatant;

				switch (Utility.Random(5))
				{
					default:
					case 0: Bless(targ); break;
					case 1: Curse(targ); break;
					case 2: GHeal(this); break;
					case 3: FlameStrike(this, targ); break;
					case 4: Poison2(this, targ); break;
				};
			}
			return base.OnBeforeDeath();
		}

		public static void Bless(Mobile target)
		{
			if (target.GetStatMod("Pixie Bless1") != null) return;
			if (target.GetStatMod("Pixie Bless2") != null) return;
			if (target.GetStatMod("Pixie Bless3") != null) return;
			int offset = 5 + Utility.Random(5);
			target.AddStatMod(new StatMod(StatType.Str, "Pixie Bless1", offset, TimeSpan.FromSeconds(Utility.Random(60))));
			target.AddStatMod(new StatMod(StatType.Dex, "Pixie Bless2", offset, TimeSpan.FromSeconds(Utility.Random(60))));
			target.AddStatMod(new StatMod(StatType.Int, "Pixie Bless3", offset, TimeSpan.FromSeconds(Utility.Random(60))));
		}

		public static void Curse(Mobile target)
		{
			if (target.GetStatMod("Pixie Curse1") != null) return;
			if (target.GetStatMod("Pixie Curse2") != null) return;
			if (target.GetStatMod("Pixie Curse3") != null) return;
			int offset = 5 + Utility.Random(5);
			target.AddStatMod(new StatMod(StatType.Str, "Pixie Curse1", -offset, TimeSpan.FromSeconds(Utility.Random(60))));
			target.AddStatMod(new StatMod(StatType.Dex, "Pixie Curse2", -offset, TimeSpan.FromSeconds(Utility.Random(60))));
			target.AddStatMod(new StatMod(StatType.Int, "Pixie Curse3", -offset, TimeSpan.FromSeconds(Utility.Random(60))));
		}

		public static void GHeal(Mobile target)
		{
			target.Heal(Utility.RandomMinMax(20, 40));
			target.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
			target.PlaySound(0x202);
		}

		public static void FlameStrike(Mobile from, Mobile target)
		{
			int FSdamage = Utility.RandomMinMax(20, 40);
			AOS.Damage(target, from, FSdamage, 0, 100, 0, 0, 0);
			target.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
			target.PlaySound(0x208);
		}

		public static void Poison2(Mobile from, Mobile target)
		{
			target.ApplyPoison(from, Poison.Greater);
			target.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
		}

		public Pixie(Serial serial) : base(serial)
		{
		}

		public override OppositionGroup OppositionGroup => OppositionGroup.FeyAndUndead;

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
