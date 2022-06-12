using System;
using Server;
using Server.Items;


namespace Server.Mobiles
{
	public class SDSkeletalMage : BaseSummoned
	{
		[Constructable]
		public SDSkeletalMage() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a skeletal mage";
			Body = 148;
			BaseSoundID = 451;

			SetStr( 76, 100 );
			SetDex( 56, 75 );
			SetInt( 186, 210 );

			SetHits( 46, 60 );

			SetDamage( 3, 7 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 35, 40 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 50, 60 );
			SetResistance( ResistanceType.Poison, 20, 30 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.EvalInt, 60.1, 70.0 );
			SetSkill( SkillName.Magery, 60.1, 70.0 );
			SetSkill( SkillName.MagicResist, 55.1, 70.0 );
			SetSkill( SkillName.Tactics, 45.1, 60.0 );
			SetSkill( SkillName.Wrestling, 45.1, 55.0 );

			Fame = 3000;
			Karma = -3000;

			VirtualArmor = 38;
		}

		public override Poison PoisonImmune{ get{ return Poison.Regular; } }

		public SDSkeletalMage( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
