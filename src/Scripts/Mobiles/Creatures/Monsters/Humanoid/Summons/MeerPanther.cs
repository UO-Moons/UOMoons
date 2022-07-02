using System;
using Server.Items;

namespace Server.Mobiles
{
	public class MeerPanther : BaseSummoned
	{
		[Constructable]
		public MeerPanther() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Name = "a panther";
			Body = 0xD6;
			Hue = 0x901;
			BaseSoundId = 0x462;

			SetStr( 61, 85 );
			SetDex( 86, 105 );
			SetInt( 26, 50 );

			SetHits( 37, 51 );
			SetMana( 0 );

			SetDamage( 4, 12 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 20, 25 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 10, 15 );
			SetResistance( ResistanceType.Poison, 5, 10 );

			SetSkill( SkillName.MagicResist, 15.1, 30.0 );
			SetSkill( SkillName.Tactics, 50.1, 65.0 );
			SetSkill( SkillName.Wrestling, 50.1, 65.0 );

			Fame = 450;
			Karma = 0;

			VirtualArmor = 16;
		}

		public override PackInstinct PackInstinct{ get{ return PackInstinct.Feline; } }

		public MeerPanther(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
