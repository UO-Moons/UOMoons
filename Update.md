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


