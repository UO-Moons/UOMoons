using System;

namespace Server.Items;

public class TreasureChestLevel4 : LockableContainer
{
	private const int Level = 4;

	public override bool Decays => true;
	public override bool IsDecoContainer => false;
	public override TimeSpan DecayTime => TimeSpan.FromMinutes(Utility.Random(15, 60));
	public override int DefaultGumpID => 0x42;
	public override int DefaultDropSound => 0x42;
	public override Rectangle2D Bounds => new(18, 105, 144, 73);

	private void SetChestAppearance()
	{
		bool useFirstItemId = Utility.RandomBool();

		switch (Utility.Random(4))
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

			case 3:// Keg
				ItemId = 0xe7f;
				GumpID = 0x3e;
				break;
		}
	}

	[Constructable]
	public TreasureChestLevel4()
		: base(0xE41)
	{
		SetChestAppearance();
		Movable = false;

		TrapType = TrapType.ExplosionTrap;
		TrapPower = Level * Utility.Random(10, 25);
		Locked = true;

		RequiredSkill = 92;
		LockLevel = RequiredSkill - Utility.Random(1, 10);
		MaxLockLevel = RequiredSkill + Utility.Random(1, 10);

		// According to OSI, loot in level 4 chest is:
		//  Gold 500 - 900
		//  Reagents
		//  Scrolls
		//  Blank scrolls
		//  Potions
		//  Gems
		//  Magic Wand
		//  Magic weapon
		//  Magic armour
		//  Magic clothing (not implemented)
		//  Magic jewelry (not implemented)
		//  Crystal ball (not implemented)

		// Gold
		DropItem(new Gold(Utility.Random(200, 400)));

		// Reagents
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item reagentLoot = Loot.RandomReagent();
			reagentLoot.Amount = 12;
			DropItem(reagentLoot);
		}

		// Scrolls
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item scrollLoot = Loot.RandomScroll(0, 47, SpellbookType.Regular);
			scrollLoot.Amount = 16;
			DropItem(scrollLoot);
		}

		// Drop blank scrolls
		DropItem(new BlankScroll(Utility.Random(1, Level)));

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
			gemLoot.Amount = 12;
			DropItem(gemLoot);
		}

		// Magic Wand
		for (int i = Utility.Random(1, Level); i > 1; i--)
			DropItem(Loot.RandomWand());

		// Equipment
		for (int i = Utility.Random(1, Level); i > 1; i--)
		{
			Item item = Loot.RandomArmorOrShieldOrWeapon();
			if (item is BaseWeapon weapon)
			{
				weapon.DamageLevel = (WeaponDamageLevel)Utility.Random(Level);
				weapon.AccuracyLevel = (WeaponAccuracyLevel)Utility.Random(Level);
				weapon.DurabilityLevel = (DurabilityLevel)Utility.Random(Level);
				weapon.Quality = ItemQuality.Normal;
			}
			else if (item is BaseArmor armor)
			{
				armor.ProtectionLevel = (ArmorProtectionLevel)Utility.Random(Level);
				armor.Durability = (DurabilityLevel)Utility.Random(Level);
				armor.Quality = ItemQuality.Normal;
			}

			DropItem(item);
		}

		// Clothing
		for (int i = Utility.Random(1, 2); i > 1; i--)
			DropItem(Loot.RandomClothing());

		// Jewelry
		for (int i = Utility.Random(1, 2); i > 1; i--)
			DropItem(Loot.RandomJewelry());

		// Crystal ball (not implemented)
	}

	public TreasureChestLevel4(Serial serial)
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
