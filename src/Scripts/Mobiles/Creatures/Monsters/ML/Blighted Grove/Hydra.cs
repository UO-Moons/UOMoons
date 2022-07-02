using Server.Items;

namespace Server.Mobiles
{
	[CorpseName("a hydra corpse")]
	public class Hydra : BaseCreature
	{
		[Constructable]
		public Hydra()
			: base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a hydra";
			Body = 0x109;
			BaseSoundId = 0x16A;

			SetStr(801, 828);
			SetDex(102, 118);
			SetInt(102, 120);

			SetHits(1480, 1500);

			SetDamage(21, 26);

			SetDamageType(ResistanceType.Physical, 60);
			SetDamageType(ResistanceType.Fire, 10);
			SetDamageType(ResistanceType.Cold, 10);
			SetDamageType(ResistanceType.Poison, 10);
			SetDamageType(ResistanceType.Energy, 10);

			SetResistance(ResistanceType.Physical, 65, 75);
			SetResistance(ResistanceType.Fire, 70, 85);
			SetResistance(ResistanceType.Cold, 25, 35);
			SetResistance(ResistanceType.Poison, 35, 43);
			SetResistance(ResistanceType.Energy, 36, 45);

			SetSkill(SkillName.Wrestling, 103.5, 117.4);
			SetSkill(SkillName.Tactics, 100.1, 109.8);
			SetSkill(SkillName.MagicResist, 85.5, 98.5);
			SetSkill(SkillName.Anatomy, 75.4, 79.8);

			// TODO: Fame/Karma
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.UltraRich, 3);
		}

		//public override void OnDeath(Container c)
		//{
		//	base.OnDeath(c);

		//	c.DropItem(new HydraScale());

			/*
			// TODO: uncomment once added
			if ( Utility.RandomDouble() < 0.2 )
				c.DropItem( new ParrotItem() );

			if ( Utility.RandomDouble() < 0.05 )
				c.DropItem( new ThorvaldsMedallion() );
			*/
		//}

		public override bool HasBreath => true;
		public override int Hides => 40;
		public override int Meat => 19;
		public override int TreasureMapLevel => 5;

		public Hydra(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
