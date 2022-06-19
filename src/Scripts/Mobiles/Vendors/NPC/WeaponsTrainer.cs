using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles;

public class WeaponsTrainer : BaseVendor
{
	public override bool IsActiveVendor => false;

	protected override List<SbInfo> SbInfos { get; } = new();

	public override void InitSbInfo()
	{
	}

	[Constructable]
	public WeaponsTrainer() : base( "the weapons trainer" )
	{
		Female = Utility.RandomBool();
		Body = Female ? 401 : 400;
		Name = NameList.RandomName( Female ? "female" : "male" );
		Hue = Utility.RandomSkinHue();
		SetStr( 96, 110 );
		SetDex( 91, 105 );
		SetInt( 71, 85 );
		Karma = Utility.RandomMinMax( 13, -45 );

			
		SetSkill( SkillName.Tactics, 75, 97.5 );
		SetSkill( SkillName.MagicResist, 65, 87.5 );
		SetSkill( SkillName.Parry, 75, 97.5 );
		SetSkill( SkillName.Swords, 67.5, 90 );
		SetSkill( SkillName.Macing, 67.5, 90 );
		SetSkill( SkillName.Fencing, 67.5, 90 );
		SetSkill( SkillName.Wrestling, 67.5, 90 );


		Item item;
		if ( !Female )
		{
			Utility.AssignRandomHair(this);
			Utility.AssignRandomFacialHair(this);
			item = new StuddedChest();
			AddItem( item );
			item = new StuddedLegs();
			AddItem( item );
			item = new StuddedArms();
			AddItem( item );
			item = new StuddedGloves();
			AddItem( item );
			item = Utility.RandomBool() ? new Boots() : new ThighBoots();
			AddItem( item );
			PackGold( 15, 100 );
		} else {
			Utility.AssignRandomHair(this);
			item = new StuddedChest();
			AddItem( item );
			item = new StuddedLegs();
			AddItem( item );
			item = new StuddedArms();
			AddItem( item );
			item = new StuddedGloves();
			AddItem( item );
			item = Utility.RandomBool() ? new Boots() : new ThighBoots();
			AddItem( item );
			PackGold( 15, 100 );
		}
	}

	public WeaponsTrainer( Serial serial ) : base( serial )
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
		reader.ReadInt();
	}
}
