using System;

namespace Server.Items;

public class TreasureChestLevel2 : LockableContainer
{
	private const int Level = 2;

	public override bool Decays => true;
	public override bool IsDecoContainer => false;
	public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));
	public override int DefaultGumpID => 0x42;
	public override int DefaultDropSound => 0x42;
	public override Rectangle2D Bounds => new(18, 105, 144, 73);

	private void SetChestAppearance()
	{
		bool useFirstItemId = Utility.RandomBool();

		switch (Utility.RandomList(0, 1, 2, 3, 4, 5, 6, 7))
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

			case 3:// Wooden Chest
				ItemId = useFirstItemId ? 0xe42 : 0xe43;
				GumpID = 0x49;
				break;

			case 4:// Metal Chest
				ItemId = useFirstItemId ? 0x9ab : 0xe7c;
				GumpID = 0x4A;
				break;

			case 5:// Metal Golden Chest
				ItemId = useFirstItemId ? 0xe40 : 0xe41;
				GumpID = 0x42;
				break;

			case 6:// Keg
				ItemId = 0xe7f;
				GumpID = 0x3e;
				break;

			case 7:// Barrel
				ItemId = 0xe77;
				GumpID = 0x3e;
				break;
		}
	}

	[Constructable]
	public TreasureChestLevel2()
		: base(0xE41)
	{
		SetChestAppearance();
		Movable = false;

		TrapType = TrapType.ExplosionTrap;
		TrapPower = Level * Utility.Random(1, 25);
		Locked = true;

		RequiredSkill = 72;
		LockLevel = RequiredSkill - Utility.Random(1, 10);
		MaxLockLevel = RequiredSkill + Utility.Random(1, 10);

		// According to OSI, loot in level 2 chest is:
		//  Gold 80 - 150
		//  Arrows 10
		//  Reagents
		//  Scrolls
		//  Potions
		//  Gems

		// Gold
		DropItem(new Gold(Utility.Random(70, 100)));

		// Drop bolts
		//DropItem( new Arrow( 10 ) );

		// Reagents
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item reagentLoot = Loot.RandomReagent();
			reagentLoot.Amount = Utility.Random(1, Level);
			DropItem(reagentLoot);
		}

		// Scrolls
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item scrollLoot = Loot.RandomScroll(0, 39, SpellbookType.Regular);
			scrollLoot.Amount = Utility.Random(1, 8);
			DropItem(scrollLoot);
		}

		// Potions
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item potionLoot = Loot.RandomPotion();
			DropItem(potionLoot);
		}

		// Gems
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item gemLoot = Loot.RandomGem();
			gemLoot.Amount = Utility.Random(1, 6);
			DropItem(gemLoot);
		}
	}

	public TreasureChestLevel2(Serial serial)
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
