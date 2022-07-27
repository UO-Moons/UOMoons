using System;

namespace Server.Items;

public class TreasureChestLevel3 : LockableContainer
{
	private const int Level = 3;

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
			case 0:// Wooden Chest
				ItemId = useFirstItemId ? 0xe42 : 0xe43;
				GumpID = 0x49;
				break;

			case 1:// Metal Chest
				ItemId = useFirstItemId ? 0x9ab : 0xe7c;
				GumpID = 0x4A;
				break;

			case 2:// Metal Golden Chest
				ItemId = useFirstItemId ? 0xe40 : 0xe41;
				GumpID = 0x42;
				break;
		}
	}

	[Constructable]
	public TreasureChestLevel3()
		: base(0xE41)
	{
		SetChestAppearance();
		Movable = false;

		TrapType = TrapType.PoisonTrap;
		TrapPower = Level * Utility.Random(1, 25);
		Locked = true;

		RequiredSkill = 84;
		LockLevel = RequiredSkill - Utility.Random(1, 10);
		MaxLockLevel = RequiredSkill + Utility.Random(1, 10);

		// According to OSI, loot in level 3 chest is:
		//  Gold 250 - 350
		//  Arrows 10
		//  Reagents
		//  Scrolls
		//  Potions
		//  Gems
		//  Magic Wand
		//  Magic weapon
		//  Magic armour
		//  Magic clothing  (not implemented)
		//  Magic jewelry  (not implemented)

		// Gold
		DropItem(new Gold(Utility.Random(180, 240)));

		// Drop bolts
		//DropItem( new Arrow( 10 ) );

		// Reagents
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item reagentLoot = Loot.RandomReagent();
			reagentLoot.Amount = Utility.Random(1, 9);
			DropItem(reagentLoot);
		}

		// Scrolls
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item scrollLoot = Loot.RandomScroll(0, 47, SpellbookType.Regular);
			scrollLoot.Amount = Utility.Random(1, 12);
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
			gemLoot.Amount = Utility.Random(1, 9);
			DropItem(gemLoot);
		}

		// Magic Wand
		for (int i = Utility.Random(1, Level); i > 1; i--)
			DropItem(Loot.RandomWand());

		// Equipment
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item item = Loot.RandomArmorOrShieldOrWeapon();

			switch (item)
			{
				case BaseWeapon weapon:
					weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(Level);
					weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(Level);
					weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(Level);
					weapon.Quality = ItemQuality.Normal;
					break;
				case BaseArmor armor:
					armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(Level);
					armor.Durability = (DurabilityLevel)Utility.Random(Level);
					armor.Quality = ItemQuality.Normal;
					break;
			}

			DropItem(item);
		}

		// Clothing
		for (int i = Utility.Random(1, 2); i > 1; i--)
			DropItem(Loot.RandomClothing());

		// Jewelry
		for (int i = Utility.Random(1, 2); i > 1; i--)
			DropItem(Loot.RandomJewelry());
	}

	public TreasureChestLevel3(Serial serial)
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
