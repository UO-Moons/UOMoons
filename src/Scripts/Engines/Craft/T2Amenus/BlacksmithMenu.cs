using System;
using Server.Engines.Craft;
using Server.Network;
using Server.Items;

namespace Server.Menus.ItemLists
{
    public class BlacksmithMenu : ItemListMenu
    {
        private readonly Mobile _mMobile;
        private string _isFrom;
        private readonly BaseTool _mTool;
        private readonly ItemListEntry[] _mEntries;

        public BlacksmithMenu(Mobile m, ItemListEntry[] entries, string @is, BaseTool tool) : base("What would you like to make?", entries)
        {
            _mMobile = m;
            _isFrom = @is;
            _mTool = tool;
            _mEntries = entries;
        }

        //MAIN
        public static ItemListEntry[] Main(Mobile from)
        {
            bool shields = true;
            bool armor = true;
            bool weapons = true;
            int missing = 0;

            bool allRequiredSkills = true;

            //Shields
            var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(23).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(23).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //WoodenKiteShield
            {
                shields = false;
            }

            //Armor
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(0).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(0).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //RingmailGloves
            {
                armor = false;
            }

            //Weapons
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(26).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(26).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Dagger
            {
                weapons = false;
            }

            ItemListEntry[] entries = new ItemListEntry[4];

            entries[0] = new ItemListEntry("Repair", 4015, 0, 0);

            if (shields)
                entries[1] = new ItemListEntry("Shields", 7026, 0, 1);
            else
                missing++;// entries[1] = new ItemListEntry("", -1);

            if (armor)
                entries[2 - missing] = new ItemListEntry("Armor", 5141, 0, 2);
            else
                missing++;// entries[2] = new ItemListEntry("", -1);

            if (weapons)
                entries[3 - missing] = new ItemListEntry("Weapons", 5049, 0, 3);
            else
                missing++;// entries[3] = new ItemListEntry("", -1);

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //SHEILDS
        private static ItemListEntry[] Shields(Mobile from)
        {
	        Item item = null;
	        int missing = 0;

	        bool allRequiredSkills = true;

	        ItemListEntry[] entries = new ItemListEntry[6];

            for (int i = 0; i < 6; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 18).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 18).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 18).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("S", " S");
	                    name = name.Replace("K", " K");
	                    name = name.ToLower();
	                    var itemid = item.ItemId;
	                    if (itemid == 7033)
		                    itemid = 7032;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //WEAPONS
        private static ItemListEntry[] Weapons(Mobile from)
        {
            bool swords = true;
            bool axes = true;
            bool maces = true;
            bool polearms = true;
            int missing = 0;

            bool allRequiredSkills = true;

            //Swords
            var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(26).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(26).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Dagger
            {
                swords = false;
            }
            //Axes
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(34).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(34).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Double Axe
            {
                axes = false;
            }
            //Maces
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(45).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(45).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Mace
            {
                maces = false;
            }
            //Polearms
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(42).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(42).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Spear
            {
                polearms = false;
            }

            ItemListEntry[] entries = new ItemListEntry[4];

            if (swords)
                entries[0] = new ItemListEntry("Swords & Blades", 5049, 0, 0);
            else
                missing++;//entries[0] = new ItemListEntry("", -1);

            if (axes)
                entries[1 - missing] = new ItemListEntry("Axes", 3913, 0, 1);
            else
                missing++;//entries[1] = new ItemListEntry("", -1);

            if (maces)
                entries[2 - missing] = new ItemListEntry("Maces & Hammers", 5127, 0, 2);
            else
                missing++;//entries[2] = new ItemListEntry("", -1);

            if (polearms)
                entries[3-missing] = new ItemListEntry("Polearms", 3917, 0, 3);
            else
                missing++;//entries[3] = new ItemListEntry("", -1);

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //BLADES
        private static ItemListEntry[] Blades(Mobile from)
        {
	        Item item = null;

	        bool allRequiredSkills = true;
	        int missing = 0;

            ItemListEntry[] entries = new ItemListEntry[8];

            for (int i = 0; i < 8; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 24).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 24).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i+24).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //AXES
        private static ItemListEntry[] Axes(Mobile from)
        {
	        Item item = null;

	        bool allRequiredSkills = true;
	        int missing = 0;

            ItemListEntry[] entries = new ItemListEntry[7];

            for (int i = 0; i < 7; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 32).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 32).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if ((from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 32).ItemType).Resources.GetAt(0).Amount) && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("B", " B");
	                    name = name.Replace("H", " H");
	                    name = name.Replace("A", " A");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //MACES
        private static ItemListEntry[] Maces(Mobile from)
        {
	        Item item = null;
            int missing = 0;

            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[6];

            for (int i = 0; i < 6; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 43).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 43).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 43).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("P", " P");
	                    name = name.Replace("F", " F");
	                    name = name.Replace("M", " M");
	                    name = name.Replace("H", " H");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //POLEARMS
        private static ItemListEntry[] Polearms(Mobile from)
        {
	        Item item = null;
            int missing = 0;

            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[4];

            for (int i = 0; i < 4; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 39).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 39).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);


                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 39).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("S", " S");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //ARMOR
        private static ItemListEntry[] Armor(Mobile from)
        {
            bool platemail = true;
            bool chainmail = true;
            bool ringmail = true;
            bool helmets = true;
            int missing = 0;

            bool allRequiredSkills = true;

            //Platemail
            var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(9).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(9).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Gorget
            {
                platemail = false;
            }
            //Chainmail
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(5).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(5).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Legs
            {
                chainmail = false;
            }
            //Ringmail
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(0).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(0).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Gloves
            {
                ringmail = false;
            }
            //Helmets
            chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) < DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).ItemType).Resources.GetAt(0).Amount || (chance <= 0.0)) //Gloves
            {
                helmets = false;
            }

            ItemListEntry[] entries = new ItemListEntry[4];

            if (platemail)
                entries[0] = new ItemListEntry("Platemail", 5141, 0, 0);
            else
                missing++;// entries[0] = new ItemListEntry("", -1);

            if (chainmail)
                entries[1 - missing] = new ItemListEntry("Chainmail", 5055, 0, 1);
            else
                missing++;//entries[1] = new ItemListEntry("", -1);

            if (ringmail)
                entries[2 - missing] = new ItemListEntry("Ringmail", 5100, 0, 2);
            else
                missing++;//entries[2] = new ItemListEntry("", -1);

            if (helmets)
                entries[3 - missing] = new ItemListEntry("Helmets", 5138, 0, 3);
            else
            {
                missing++;//entries[3] = new ItemListEntry("", -1);
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //PLATEMAIL
        private static ItemListEntry[] Platemail(Mobile from)
        {
	        Item item = null;
            int missing = 0;

            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[6];

            for (int i = 0; i < 6; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 7).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 7).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 7).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("A", " A");
	                    name = name.Replace("G", " G");
	                    name = name.Replace("L", " L");
	                    name = name.Replace("C", " C");
	                    name = name.Replace("Female", "");
	                    name = name.Replace("Plate", "Platemail");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, (entries.Length - missing));
            return entries;
        }
        //CHAINMAIL
        private static ItemListEntry[] Chainmail(Mobile from)
        {
	        Item item = null;
            int missing = 0;

            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[2];

            for (int i = 0; i < 2; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 5).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 5).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 5).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("Chest", " Tunic");
	                    name = name.Replace("L", " L");
	                    name = name.Replace("Chain", " Chainmail");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, (entries.Length - missing));
            return entries;
        }

        //RINGMAIL
        private static ItemListEntry[] Ringmail(Mobile from)
        {
	        Item item = null;
            int missing = 0;
            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[4];

            for (int i = 0; i < 4; ++i)
            {
                var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i).ItemType;
                var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    var name = item.GetType().Name;
	                    name = name.Replace("Chest", "Tunic");
	                    name = name.Replace("T", " T");
	                    name = name.Replace("L", " L");
	                    name = name.Replace("S", " S");
	                    name = name.Replace("G", " G");
	                    name = name.Replace("A", " A");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    var itemid = item.ItemId;

	                    entries[i-missing] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        //HELMETS
        private static ItemListEntry[] Helmets(Mobile from)
        {
	        int itemid;
            string name;
            Item item = null;
            int missing = 0;

            bool allRequiredSkills = true;

            ItemListEntry[] entries = new ItemListEntry[6];

            //chainmail coif
            var type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).ItemType;
            var craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
            var chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);
            if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
            {
                type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).ItemType;
                try { item = Activator.CreateInstance(type) as Item; }
                catch
                {
	                // ignored
                }

                if (item != null)
                {
	                name = item.GetType().Name;
	                name = name.Replace("Chain", "Chainmail");
	                name = name.Replace("C", " C");
	                name = name.Trim();
	                name = name.ToLower();
	                itemid = item.ItemId;


	                entries[0] = new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, 4);
                }
            }
            else
            {
                missing++;// entries[0] = new ItemListEntry("", -1);
            }

            item?.Delete();

            //the rest
            for (int i = 1; i < 6; ++i)
            {
                type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 12).ItemType;
                craftResource = DefBlacksmithy.CraftSystem.CraftItems.SearchFor(type).Resources.GetAt(0);
                chance = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 12).GetSuccessChance(from, typeof(IronIngot), DefBlacksmithy.CraftSystem, false, ref allRequiredSkills);

                if (from.Backpack.GetAmount(typeof(IronIngot)) >= DefBlacksmithy.CraftSystem.CraftItems.SearchFor(DefBlacksmithy.CraftSystem.CraftItems.GetAt(i + 12).ItemType).Resources.GetAt(0).Amount && (chance > 0.0))
                {
                    item = null;
                    try { item = Activator.CreateInstance(type) as Item; }
                    catch
                    {
	                    // ignored
                    }

                    if (item != null)
                    {
	                    name = item.GetType().Name;
	                    name = name.Replace("C", " C");
	                    name = name.Replace("H", " H");
	                    name = name.Trim();
	                    name = name.ToLower();
	                    itemid = item.ItemId;

	                    entries[i - missing] =
		                    new ItemListEntry($"{name} ({craftResource.Amount} ingots)", itemid, 0, i);
                    }
                }
                else
                {
                    missing++;//entries[i-missing] = new ItemListEntry("", -1);
                }

                item?.Delete();
            }

            Array.Resize(ref entries, entries.Length - missing);
            return entries;
        }

        /*private static ItemListEntry[] Test()
        {
            Type type;
            int itemid;
            string name;
            Item item;

            ItemListEntry[] entries = new ItemListEntry[DefBlacksmithy.CraftSystem.CraftItems.Count];

            for (int i = 0; i < DefBlacksmithy.CraftSystem.CraftItems.Count; ++i)
            {
                type = DefBlacksmithy.CraftSystem.CraftItems.GetAt(i).ItemType;

                item = null;
                try { item = Activator.CreateInstance(type) as Item; }
                catch { }
                name = item.Name;
                itemid = item.ItemID;

                entries[i-missing] = new ItemListEntry(name, itemid, 0, i);

                if (item != null)
                    item.Delete();
            }

            return entries;
        }*/

        public override void OnResponse(NetState state, int index)
        {
            if (_isFrom == "Main")
            {
                if (_mEntries[index].CraftIndex == 0)
                {
                    Repair.Do(_mMobile, DefBlacksmithy.CraftSystem, _mTool);
                }
                if (_mEntries[index].CraftIndex == 1)
                {
                    _isFrom = "Shields";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Shields(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 2)
                {
                    _isFrom = "Armor";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Armor(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 3)
                {
                    _isFrom = "Weapons";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Weapons(_mMobile), _isFrom, _mTool));
                }
            }
            else if (_isFrom == "Shields")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 18).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 18).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 18).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 18));
            }
            else if (_isFrom == "Weapons")
            {
                if (_mEntries[index].CraftIndex == 0)
                {
                    _isFrom = "Blades";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Blades(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 1)
                {
                    _isFrom = "Axes";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Axes(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 2)
                {
                    _isFrom = "Maces";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Maces(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 3)
                {
                    _isFrom = "Polearms";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Polearms(_mMobile), _isFrom, _mTool));
                }

            }
            else if (_isFrom == "Blades")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 24).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 24).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 24).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 24));
            }
            else if (_isFrom == "Axes")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 32).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 32).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 32).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 32));
            }
            else if (_isFrom == "Maces")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 43).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 43).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 43).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 43));
            }
            else if (_isFrom == "Polearms")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 39).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 39).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 39).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 39));
            }
            else if (_isFrom == "Armor")
            {
                if (_mEntries[index].CraftIndex == 0)
                {
                    _isFrom = "Platemail";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Platemail(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 1)
                {
                    _isFrom = "Chainmail";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Chainmail(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 2)
                {
                    _isFrom = "Ringmail";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Ringmail(_mMobile), _isFrom, _mTool));
                }
                else if (_mEntries[index].CraftIndex == 3)
                {
                    _isFrom = "Helmets";
                    _mMobile.SendMenu(new BlacksmithMenu(_mMobile, Helmets(_mMobile), _isFrom, _mTool));
                }

            }
            else if (_isFrom == "Platemail")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 7).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 7).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 7).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 7));
            }
            else if (_isFrom == "Chainmail")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 5).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 5).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 5).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 5));
            }
            else if (_isFrom == "Ringmail")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex)).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex)).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex)).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex)));
            }
            else if (_isFrom == "Helmets")
            {
                Type type = null;

                CraftContext context = DefBlacksmithy.CraftSystem.GetContext(_mMobile);
                CraftSubResCol res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 12).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                int resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 12).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);

                //chainmail coif
                if (index == 0)
                {
                    res = (DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).UseSubRes2 ? DefBlacksmithy.CraftSystem.CraftSubRes2 : DefBlacksmithy.CraftSystem.CraftSubRes);
                    resIndex = (DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).UseSubRes2 ? context.LastResourceIndex2 : context.LastResourceIndex);
                }

                if (resIndex > -1)
                {
                    type = res.GetAt(resIndex).ItemType;
                }

                //chainmail coif
                if (index == 0)
                {
                    DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt(4).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt(4));
                }
                //the rest
                else
                {
                    DefBlacksmithy.CraftSystem.CreateItem(_mMobile, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 12).ItemType, type, _mTool, DefBlacksmithy.CraftSystem.CraftItems.GetAt((_mEntries[index].CraftIndex) + 12));
                }
            }

        }
    }
}
