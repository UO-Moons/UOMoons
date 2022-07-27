# UO Moons Version 4.0.0.2

1. Fixed Pigment Dye Colors.
2. Fixed Gargish Cloth Armor not being created correctly for females when crafted.
3. Added UO Store.
4. Added Monster Stealing.
5. Added All UO Store Items.
6. Fixed Wind Dungeon Entrance to require 70.0 and not 71.5 skill.
7. Fixed Regions Now have regions loaded though folder in Data/regions
8. Added New Magincia System disabled by defualt.
9. Refactored on going.
10. Fixed Vendor Inventor Bugs
11. Added Imbueing, Only Enabled by SA Expansion.
12. Added Reforging, Only Enabled by SA Expansion.
13. Added Some SA NPCS
14. Added Some SA Resources.
15. Added Some SA Quests.
16. Refactored Alot of Code.
17. Fixed many bugs to many to list.
18. Removed alot of code not needed or unused.
19. Fixed Dupe lines of code all over the place.
20. Added more helpers to help combine duped code in all kinds of places.
21. Rewrote all the imbueing and loot systems.
22. Rewrote the Dungeon Systems

# UO Moons Version 4.0.0.1

1. Added Hashed Wheel Timer Check out readme for details.
2. Fixed an issue with item/mobile creation where the event is triggered on a deleted object.
3. Updated Mobile Scripts.
- went through all scripts to update them to SetWearable.
4. Update to SetWearable.
- last item being set will be worn (if no override to OnEquip returns false.
- delete the item on the layer that is the same as the new item (if existend).
- used ifs to cap item.Movable, therefor no need to call to Random if <= 0 or >= 1.
5. Update base InitOutFit.
- replaced the Additem for Backpack with SetWearable, Movable is getting set this way too.
- altered the other SetWearable calls, to allow the items to be moved / dropped (as they should normally).
6. Added SetStats(str,dex,int); to basecreature.
7. Added SetDamage(resistsdamage ,normal damage); to basecreature.
8. Cleaned up basecreature and reorginazed.
9. Moved SetWearable to Basemobile.
10. Moved Creaturekilledby to eventsink.cs
11. Added Rare Name verifaction System defualt enabled is false.
12. Added Player History System Defualt Enabled is false.
13. Upgraded Version Control2.
14. Upgraded Server Files to be Current.
15. Added Texas Hold'm.


