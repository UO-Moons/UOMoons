using System;

namespace Server.Items;

public class TreasureChestLevel1 : LockableContainer
{
	private const int Level = 1;

	public override bool Decays => true;
	public override bool IsDecoContainer => false;
	public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));
	public override int DefaultGumpID => 0x42;
	public override int DefaultDropSound => 0x42;
	public override Rectangle2D Bounds => new(18, 105, 144, 73);

	private void SetChestAppearance()
	{
		bool useFirstItemId = Utility.RandomBool();

		switch (Utility.RandomList(0, 1, 2))
		{
			case 0:// Large Crate
				ItemId = useFirstItemId ? 0xe3c : 0xe3d;
				GumpID = 0x44;
				break;

			case 1:// Medium Crate
				ItemId = useFirstItemId ? 0xe3e : 0xe3f;
				GumpID = 0x44;
				break;

			case 2:// Small Crate
				ItemId = useFirstItemId ? 0x9a9 : 0xe7e;
				GumpID = 0x44;
				break;
		}
	}

	[Constructable]
	public TreasureChestLevel1()
		: base(0xE41)
	{
		SetChestAppearance();
		Movable = false;

		TrapType = TrapType.DartTrap;
		TrapPower = Level * Utility.Random(1, 25);
		Locked = true;

		RequiredSkill = 57;
		LockLevel = RequiredSkill - Utility.Random(1, 10);
		MaxLockLevel = RequiredSkill + Utility.Random(1, 10);

		// According to OSI, loot in level 1 chest is:
		//  Gold 25 - 50
		//  Bolts 10
		//  Gems
		//  Normal weapon
		//  Normal armour
		//  Normal clothing
		//  Normal jewelry

		// Gold
		DropItem(new Gold(Utility.Random(30, 100)));

		// Drop bolts
		//DropItem( new Bolt( 10 ) );

		// Gems
		if (Utility.RandomBool())
		{
			Item gemLoot = Loot.RandomGem();
			gemLoot.Amount = Utility.Random(1, 3);
			DropItem(gemLoot);
		}

		// Weapon
		if (Utility.RandomBool())
			DropItem(Loot.RandomWeapon());

		// Armour
		if (Utility.RandomBool())
			DropItem(Loot.RandomArmorOrShield());

		// Clothing
		if (Utility.RandomBool())
			DropItem(Loot.RandomClothing());

		// Jewelry
		if (Utility.RandomBool())
			DropItem(Loot.RandomJewelry());
	}

	public TreasureChestLevel1(Serial serial)
		: base(serial)
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
