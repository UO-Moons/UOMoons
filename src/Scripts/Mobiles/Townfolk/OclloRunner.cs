using System;
using Server;
using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
	public class OclloRunner : BaseCreature
	{
		[Constructable]
		public OclloRunner() : base( AIType.AI_Melee, FightMode.Aggressor, 10, 1, 0.45, 0.8 )
		{
			Female = Utility.RandomBool();
			Body = Female ? 401 : 400;
			Title = "the runner";
			Name = NameList.RandomName( Female ? "female" : "male" );
			Hue = Utility.RandomSkinHue();
			SetStr( 26, 40 );
			SetDex( 31, 45 );
			SetInt( 16, 30 );
			Karma = Utility.RandomMinMax( 13, -45 );

			
			SetSkill( SkillName.Tactics, 15, 37.5 );
			SetSkill( SkillName.MagicResist, 15, 37.5 );
			SetSkill( SkillName.Parry, 15, 37.5 );
			SetSkill( SkillName.Swords, 15, 37.5 );
			SetSkill( SkillName.Macing, 15, 37.5 );
			SetSkill( SkillName.Fencing, 15, 37.5 );
			SetSkill( SkillName.Wrestling, 15, 37.5 );


			Item item = null;
			if ( !Female )
			{
				Utility.AssignRandomHair(this);
				item.Hue = Utility.RandomHairHue();
				Utility.AssignRandomFacialHair(this);
				item = new Shirt();
				item.Hue = Utility.RandomNondyedHue();
				AddItem( item );
				item = new LongPants();
				item.Hue = Utility.RandomNondyedHue();
				AddItem( item );
				item = new Sandals();
				AddItem( item );
				LootPack.OldPoor.Generate(this, null, false, 0);
			} else {
				Utility.AssignRandomHair(this);
				item.Hue = Utility.RandomHairHue();
				item = new Shirt();
				item.Hue = Utility.RandomNondyedHue();
				AddItem( item );
				item = new Skirt();
				item.Hue = Utility.RandomNondyedHue();
				AddItem( item );
				item = new Sandals();
				AddItem( item );
				LootPack.OldPoor.Generate( this, null ,false, 0);
			}
		}

		public OclloRunner( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int)0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}

