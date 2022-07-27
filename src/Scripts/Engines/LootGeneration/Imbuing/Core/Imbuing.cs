using Server.Commands;
using Server.Engines.Craft;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Spells.Mysticism;

namespace Server.SkillHandlers
{
    public class Imbuing
    {
        public static void Initialize()
        {
            SkillInfo.Table[(int)SkillName.Imbuing].Callback = OnUse;

            CommandSystem.Register("GetTotalWeight", AccessLevel.GameMaster, GetTotalWeight_OnCommand);
            CommandSystem.Register("GetTotalMods", AccessLevel.GameMaster, GetTotalMods_OnCommand);
        }

        /*private static void OnLogin(Mobile m)
        {
            if (!m.CanBeginAction(typeof(Imbuing)))
                m.EndAction(typeof(Imbuing));
        }*/

        private static Dictionary<Mobile, ImbuingContext> ContextTable { get; } = new();

        private static TimeSpan OnUse(Mobile from)
        {
            if (!from.Alive)
            {
                from.SendLocalizedMessage(500949); //You can't do that when you're dead.
            }
            else if (from is PlayerMobile mobile)
            {
                mobile.CloseGump(typeof(ImbuingGump));
                BaseGump.SendGump(new ImbuingGump(mobile));
                mobile.BeginAction(typeof(Imbuing));
            }

            return TimeSpan.FromSeconds(1.0);
        }

        public static ImbuingContext GetContext(Mobile m)
        {
	        if (ContextTable.ContainsKey(m))
		        return ContextTable[m];

	        ImbuingContext context = new(m);
            ContextTable[m] = context;
            return context;

        }

        public static void AddContext(Mobile from, ImbuingContext context)
        {
            ContextTable[from] = context;
        }

        public static bool CanImbueItem(Mobile from, Item item)
        {
            if (!CheckSoulForge(from, 2))
            {
                return false;
            }
            if (item == null || !item.IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1079575);  // The item must be in your backpack to imbue it.
            }
            else if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied)
            {
                from.SendLocalizedMessage(1080438);  // You cannot imbue a blessed item.
            }
            else switch (item)
            {
	            case BaseWeapon weapon when EnchantSpell.IsUnderSpellEffects(from, weapon):
		            from.SendLocalizedMessage(1080130);  // You cannot imbue an item that is currently enchanted.
		            break;
	            case BaseWeapon { FocusWeilder: { } }:
		            from.SendLocalizedMessage(1080444);  //You cannot imbue an item that is under the effects of the ninjitsu focus attack ability.
		            break;
	            default:
	            {
		            if (IsSpecialItem(item))
		            {
			            from.SendLocalizedMessage(1079576); // You cannot imbue this item.
		            }
		            else if (item.GetType() == typeof(JukaBow))
		            {
			            if (((JukaBow)item).IsModified)
				            from.SendLocalizedMessage(1079576); // You cannot imbue this item.
			            else
				            return true;
		            }
		            else if (item is BaseJewel and not BaseRing and not BaseBracelet)
		            {
			            from.SendLocalizedMessage(1079576); // You cannot imbue this item.
		            }
		            else if (IsInNonImbueList(item.GetType()))
		            {
			            from.SendLocalizedMessage(1079576); // You cannot imbue this item.
		            }
		            else
		            {
			            return true;
		            }

		            break;
	            }
            }

            return false;
        }

        public static bool OnBeforeImbue(Mobile from, Item item, int id)
        {
            return OnBeforeImbue(from, GetTotalMods(item, id), GetMaxProps(), GetTotalWeight(item, id, false, true), GetMaxWeight(item));
        }

        public static bool OnBeforeImbue(Mobile from, int totalprops, int maxprops, int totalitemweight, int maxweight)
        {
	        if (totalprops < maxprops && totalitemweight <= maxweight)
		        return true;

	        from.SendLocalizedMessage(1079772); // You cannot imbue this item with any more item properties.
            from.CloseGump(typeof(ImbueGump));
            from.EndAction(typeof(Imbuing));
            return false;

        }

        public static bool CanUnravelItem(Mobile from, Item item, bool message = true)
        {
            if (!CheckSoulForge(from, 2, false, checkqueen: false))
            {
                from.SendLocalizedMessage(1080433); // You must be near a soulforge to magically unravel an item.
            }
            else if (!item.IsChildOf(from.Backpack))
            {
                if (message)
                    from.SendLocalizedMessage(1080424);  // The item must be in your backpack to magically unravel it.
            }
            else if (item.LootType == LootType.Blessed || item.LootType == LootType.Newbied)
            {
                if (message)
                    from.SendLocalizedMessage(1080421);  // You cannot unravel the magic of a blessed item.
            }
            else if (!(item is BaseWeapon || item is BaseArmor || item is BaseJewel || item is BaseHat))
            {
                if (message)
                    from.SendLocalizedMessage(1080425); // You cannot magically unravel this item.
            }
            else switch (item)
            {
	            case BaseWeapon weapon when EnchantSpell.IsUnderSpellEffects(from, weapon):
	            {
		            if (message)
			            from.SendLocalizedMessage(1080427);  // You cannot magically unravel an item that is currently enchanted.
		            break;
	            }
	            case BaseWeapon { FocusWeilder: { } }:
	            {
		            if (message)
			            from.SendLocalizedMessage(1080445); //You cannot magically unravel an item that is under the effects of the ninjitsu focus attack ability.
		            break;
	            }
	            default:
		            return true;
            }

            return false;
        }

        private static bool IsSpecialItem(Item item)
        {
            if (item == null)
                return true;

            if (IsSpecialImbuable(item))
                return false;

            if (item.IsArtifact)
                return true;

            if (RunicReforging.GetArtifactRarity(item) > 0)
                return true;

            if (NonCraftableImbuable(item))
                return false;

            return CraftItem.GetCraftItem(item) == null;
        }

        private static bool IsSpecialImbuable(Item item)
        {
            return IsSpecialImbuable(item.GetType());
        }

        private static bool IsSpecialImbuable(Type type)
        {
	        return m_SpecialImbuable.Any(i => i == type) || type.IsSubclassOf(typeof(BaseGlovesOfMining));
        }

        private static readonly Type[] m_SpecialImbuable =
        {
            /*typeof(ClockworkLeggings), typeof(GargishClockworkLeggings), typeof(OrcishKinMask), typeof(SavageMask), typeof(VirtuososArmbands),
            typeof(VirtuososCap), typeof(VirtuososCollar), typeof(VirtuososEarpieces), typeof(VirtuososKidGloves), typeof(VirtuososKilt),
            typeof(VirtuososNecklace), typeof(VirtuososTunic), typeof(BestialArms), typeof(BestialEarrings), typeof(BestialGloves), typeof(BestialGorget),
            typeof(BestialHelm), typeof(BestialKilt), typeof(BestialLegs), typeof(BestialNecklace), typeof(BarbedWhip), typeof(BladedWhip),
            typeof(SpikedWhip), typeof(SkullGnarledStaff), typeof(GargishSkullGnarledStaff), typeof(SkullLongsword), typeof(GargishSkullLongsword), typeof(JukaBow),
            typeof(SlayerLongbow), typeof(JackOLanternHelm)*/
        };

        private static readonly Type[] m_NonCraftables =
        {
            typeof(SilverRing), typeof(SilverBracelet)
        };

        private static bool NonCraftableImbuable(Item item)
        {
            if (item is BaseWand)
                return true;

            Type type = item.GetType();

            return m_NonCraftables.Any(t => t == type);
        }

        public static double GetSuccessChance(Mobile from, Item item, int totalItemIntensity, int propintensity, double bonus)
        {
            double skill = from.Skills[SkillName.Imbuing].Value;
            double resultWeight = totalItemIntensity + propintensity;

            double e = Math.E;
            double a, b, c, w;
            double i = item is BaseJewel ? 0.9162 : 1.0;

            // - Racial Bonus - SA ONLY -
            if (from.Race == Race.Gargoyle)
            {
                a = 1362.281555;
                b = 66.32801518;
                c = 235.2223147;
                w = -1481.037561;
            }
            else
            {
                a = 1554.96118;
                b = 53.81743328;
                c = 230.0038452;
                w = -1664.857794;
            }

            // Royal City Bonus, Fluctuation - Removed as EA doesn't seem to fluctuate despite stratics posts
            /*if (bonus == 0.02)
            {
                if (totalItemIntensity < 295)
                {
                    bonus = (double)Utility.RandomMinMax(190, 210) / 100.0;
                }
            }*/

            return Math.Max(0, Math.Round(Math.Floor(20 * skill + 10 * a * Math.Pow(e, b / (resultWeight + c)) + 10 * w - 2400) / 1000 * i + bonus, 3) * 100);
        }

        public static int GetQualityBonus(Item item)
        {
	        if (item is not IQuality quality)
		        return 0;
	        if (quality.Quality == ItemQuality.Exceptional)
		        return 20;

	        return quality.PlayerConstructed ? 10 : 0;
        }

        /// <summary>
        /// Imbues Item with selected id
        /// </summary>
        /// <param name="from">Player Imbuing</param>
        /// <param name="i">Item to be imbued</param>
        /// <param name="id">id to be imbued, see m_Table</param>
        /// <param name="value">value for id</param>
        public static void TryImbueItem(Mobile from, Item i, int id, int value)
        {
	        if (!CheckSoulForge(from, 2, out var bonus))
                return;

            ImbuingContext context = GetContext(from);

            context.LastImbued = i;
            context.ImbueMod = id;
            context.ImbueModInt = value;

            ItemPropertyInfo def = ItemPropertyInfo.GetInfo(id);

            if (def == null)
                return;

            Type gem = def.GemRes;
            Type primary = def.PrimaryRes;
            Type special = ItemPropertyInfo.GetSpecialRes(i, id, def.SpecialRes);

            context.ImbueModVal = def.Weight;

            var gemAmount = GetGemAmount(i, id, value);
            var primResAmount = GetPrimaryAmount(i, id, value);
            var specResAmount = GetSpecialAmount(i, id, value);

            if (from.AccessLevel < AccessLevel.Counselor &&
                (from.Backpack == null || from.Backpack.GetAmount(gem) < gemAmount ||
                from.Backpack.GetAmount(primary) < primResAmount ||
                from.Backpack.GetAmount(special) < specResAmount))
                from.SendLocalizedMessage(1079773); //You do not have enough resources to imbue this item.     
            else
            {
                var maxWeight = GetMaxWeight(i);

                var trueWeight = GetTotalWeight(i, id, true, true);
                var imbuingWeight = GetTotalWeight(i, id, false, true);
                GetTotalMods(i, id);
                var maxint = ItemPropertyInfo.GetMaxIntensity(i, id, true);

                var propImbuingweight = (int)(def.Weight / (double)maxint * value);
                var propTrueWeight = (int)(propImbuingweight / (double)def.Weight * 100);

                if (imbuingWeight + propImbuingweight > maxWeight)
                {
                    from.SendLocalizedMessage(1079772); // You cannot imbue this item with any more item properties.
                    from.CloseGump(typeof(ImbueGump));
                    return;
                }

                double skill = from.Skills[SkillName.Imbuing].Value;
                double success = GetSuccessChance(from, i, trueWeight, propTrueWeight, bonus);

                if (TimesImbued(i) < 20 && skill < from.Skills[SkillName.Imbuing].Cap)
                {
                    double s = Math.Min(100, success);
                    double mins = 120 - s * 1.2;
                    double maxs = Math.Max(120 / (s / 100), skill);

                    from.CheckSkill(SkillName.Imbuing, mins, maxs);
                }

                success /= 100;

                Effects.SendPacket(from, from.Map, new GraphicalEffect(EffectType.FixedFrom, from.Serial, Serial.Zero, 0x375A, from.Location, from.Location, 1, 17, true, false));
                Effects.SendTargetParticles(from, 0, 1, 0, 0x1593, EffectLayer.Waist);

                if (success >= Utility.RandomDouble() || id < 0 || id > 180)
                {
                    if (from.AccessLevel < AccessLevel.Counselor)
                    {
                        from.Backpack.ConsumeTotal(gem, gemAmount);
                        from.Backpack.ConsumeTotal(primary, primResAmount);

                        if (specResAmount > 0)
                            from.Backpack.ConsumeTotal(special, specResAmount);
                    }


                    ImbueItem(from, i, id, value);
                }
                else
                {
                    // This is consumed regardless of success/fail
                    if (from.AccessLevel < AccessLevel.Counselor)
                    {
                        from.Backpack.ConsumeTotal(primary, primResAmount);
                    }

                    from.SendLocalizedMessage(1079774); // You attempt to imbue the item, but fail.
                    from.PlaySound(0x1E4);
                }
            }

            from.EndAction(typeof(Imbuing));
        }

        private static void ImbueItem(Mobile from, Item item, int id, int value)
        {
            from.SendLocalizedMessage(1079775); // You successfully imbue the item!
            from.PlaySound(0x1EB);

            switch (item)
            {
	            case BaseWeapon wep:
		            switch (id)
		            {
			            // New property replaces the old one, so lets set them all to 0
			            case >= 30 and <= 34:
				            wep.WeaponAttributes.HitPhysicalArea = 0;
				            wep.WeaponAttributes.HitFireArea = 0;
				            wep.WeaponAttributes.HitColdArea = 0;
				            wep.WeaponAttributes.HitPoisonArea = 0;
				            wep.WeaponAttributes.HitEnergyArea = 0;
				            break;
			            case >= 35 and <= 39:
				            wep.WeaponAttributes.HitMagicArrow = 0;
				            wep.WeaponAttributes.HitHarm = 0;
				            wep.WeaponAttributes.HitFireball = 0;
				            wep.WeaponAttributes.HitLightning = 0;
				            wep.WeaponAttributes.HitDispel = 0;
				            break;
		            }

		            break;
	            case BaseJewel baseJewel when id is >= 151 and <= 183:
	            {
		            BaseJewel jewel = baseJewel;
		            SkillName[] group = GetSkillGroup((SkillName)ItemPropertyInfo.GetAttribute(id));

		            //Removes skill bonus if that group already exists on the item
		            for (int j = 0; j < 5; j++)
		            {
			            if (jewel.SkillBonuses.GetBonus(j) > 0 && group.Any(sk => sk == jewel.SkillBonuses.GetSkill(j)))
			            {
				            jewel.SkillBonuses.SetBonus(j, 0.0);
				            jewel.SkillBonuses.SetSkill(j, SkillName.Alchemy);
			            }
		            }

		            break;
	            }
            }

            SetProperty(item, id, value);

            // Sets DImodded, which is used in BaseWeapon
            if (item is BaseWeapon weapon && id == 12 && !weapon.DImodded)
            {
                weapon.DImodded = true;
            }

            // removes nom-imbued Imbuing value, which changes the way the items total weight is calculated
            if (id is >= 51 and <= 55)
            {
	            switch (item)
	            {
		            case BaseArmor arm:
			            switch (id)
			            {
				            case 51: arm.PhysNonImbuing = 0; break;
				            case 52: arm.FireNonImbuing = 0; break;
				            case 53: arm.ColdNonImbuing = 0; break;
				            case 54: arm.PoisonNonImbuing = 0; break;
				            case 55: arm.EnergyNonImbuing = 0; break;
			            }

			            break;
		            case BaseClothing hat:
			            switch (id)
			            {
				            case 51: hat.PhysNonImbuing = 0; break;
				            case 52: hat.FireNonImbuing = 0; break;
				            case 53: hat.ColdNonImbuing = 0; break;
				            case 54: hat.PoisonNonImbuing = 0; break;
				            case 55: hat.EnergyNonImbuing = 0; break;
			            }

			            break;
	            }
            }

            if (item is IImbuableEquipement imbuable)
            {
	            imbuable.OnAfterImbued(from, id, value);
	            imbuable.TimesImbued++;
            }

            // jewels get hits set to 255
            if (item is BaseJewel { MaxHitPoints: <= 0 } jewel1)
            {
	            jewel1.MaxHitPoints = 255;
	            jewel1.HitPoints = 255;
            }

			// Removes self repair
			AosArmorAttributes armorAttrs = RunicReforging.GetAosArmorAttributes(item);

            if (armorAttrs != null)
            {
                armorAttrs.SelfRepair = 0;
            }
            else
            {
                AosWeaponAttributes wepAttrs = RunicReforging.GetAosWeaponAttributes(item);

                if (wepAttrs != null)
                {
                    wepAttrs.SelfRepair = 0;
                }
            }
        }

        public static void SetProperty(Item item, int id, int value)
        {
            object prop = ItemPropertyInfo.GetAttribute(id);

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            switch (prop)
		            {
			            case AosAttribute attr when attr == AosAttribute.SpellChanneling:
			            {
				            weapon.Attributes.SpellChanneling = value;

				            if (value > 0 && weapon.Attributes.CastSpeed >= 0)
					            weapon.Attributes.CastSpeed -= 1;
				            break;
			            }
			            case AosAttribute.CastSpeed:
				            weapon.Attributes.CastSpeed += value;
				            break;
			            case AosAttribute attr:
				            weapon.Attributes[attr] = value;
				            break;
			            case AosWeaponAttribute attribute:
				            weapon.WeaponAttributes[attribute] = value;
				            break;
			            case SlayerName name:
				            weapon.Slayer = name;
				            break;
			            case SAAbsorptionAttribute attribute:
				            weapon.AbsorptionAttributes[attribute] = value;
				            break;
			            case AosElementAttribute attribute:
			            {
				            weapon.GetDamageTypes(null, out var phys, out _, out _, out _, out _, out _, out _);

				            value = Math.Min(phys, value);

				            weapon.AosElementDamages[attribute] = value;
				            weapon.Hue = weapon.GetElementalDamageHue();
				            break;
			            }
			            case string s when weapon is BaseRanged ranged && s == "WeaponVelocity":
				            ranged.Velocity = value;
				            break;
		            }

		            break;
	            }
	            case BaseShield baseShield:
	            {
		            if (prop is AosWeaponAttribute.DurabilityBonus)
		            {
			            prop = AosArmorAttribute.DurabilityBonus;
		            }

		            switch (prop)
		            {
			            case AosAttribute attribute:
			            {
				            switch (attribute)
				            {
					            case AosAttribute.SpellChanneling:
					            {
						            baseShield.Attributes.SpellChanneling = value;

						            if (value > 0 && baseShield.Attributes.CastSpeed >= 0)
							            baseShield.Attributes.CastSpeed -= 1;
						            break;
					            }
					            case AosAttribute.CastSpeed:
						            baseShield.Attributes.CastSpeed += value;
						            break;
					            default:
						            baseShield.Attributes[attribute] = value;
						            break;
				            }

				            break;
			            }
			            case AosElementAttribute attribute:
			            {
				            switch (attribute)
				            {
					            case AosElementAttribute.Physical: baseShield.PhysicalBonus = value; break;
					            case AosElementAttribute.Fire: baseShield.FireBonus = value; break;
					            case AosElementAttribute.Cold: baseShield.ColdBonus = value; break;
					            case AosElementAttribute.Poison: baseShield.PoisonBonus = value; break;
					            case AosElementAttribute.Energy: baseShield.EnergyBonus = value; break;
				            }

				            break;
			            }
			            case SAAbsorptionAttribute attribute:
				            baseShield.AbsorptionAttributes[attribute] = value;
				            break;
			            case AosArmorAttribute attribute:
				            baseShield.ArmorAttributes[attribute] = value;
				            break;
		            }

		            break;
	            }
	            case BaseArmor armor:
	            {
		            if (prop is AosWeaponAttribute.DurabilityBonus)
		            {
			            prop = AosArmorAttribute.DurabilityBonus;
		            }

		            switch (prop)
		            {
			            case AosAttribute attribute:
				            armor.Attributes[attribute] = value;
				            break;
			            case AosElementAttribute attribute:
			            {
				            switch (attribute)
				            {
					            case AosElementAttribute.Physical: armor.PhysicalBonus = value; break;
					            case AosElementAttribute.Fire: armor.FireBonus = value; break;
					            case AosElementAttribute.Cold: armor.ColdBonus = value; break;
					            case AosElementAttribute.Poison: armor.PoisonBonus = value; break;
					            case AosElementAttribute.Energy: armor.EnergyBonus = value; break;
				            }

				            break;
			            }
			            case SAAbsorptionAttribute attribute:
				            armor.AbsorptionAttributes[attribute] = value;
				            break;
			            case AosArmorAttribute attribute:
				            armor.ArmorAttributes[attribute] = value;
				            break;
		            }

		            break;
	            }
	            case BaseClothing baseClothing:
	            {
		            switch (prop)
		            {
			            case AosAttribute attribute:
				            baseClothing.Attributes[attribute] = value;
				            break;
			            case SAAbsorptionAttribute attribute:
				            baseClothing.SAAbsorptionAttributes[attribute] = value;
				            break;
			            case AosElementAttribute attribute:
			            {
				            switch (attribute)
				            {
					            case AosElementAttribute.Physical: baseClothing.Resistances.Physical = value; break;
					            case AosElementAttribute.Fire: baseClothing.Resistances.Fire = value; break;
					            case AosElementAttribute.Cold: baseClothing.Resistances.Cold = value; break;
					            case AosElementAttribute.Poison: baseClothing.Resistances.Poison = value; break;
					            case AosElementAttribute.Energy: baseClothing.Resistances.Energy = value; break;
				            }

				            break;
			            }
		            }

		            break;
	            }
	            case BaseJewel baseJewel:
	            {
		            switch (prop)
		            {
			            case AosAttribute attribute:
				            baseJewel.Attributes[attribute] = value;
				            break;
			            case SAAbsorptionAttribute attribute:
				            baseJewel.AbsorptionAttributes[attribute] = value;
				            break;
			            case AosElementAttribute attribute:
			            {
				            switch (attribute)
				            {
					            case AosElementAttribute.Physical: baseJewel.Resistances.Physical = value; break;
					            case AosElementAttribute.Fire: baseJewel.Resistances.Fire = value; break;
					            case AosElementAttribute.Cold: baseJewel.Resistances.Cold = value; break;
					            case AosElementAttribute.Poison: baseJewel.Resistances.Poison = value; break;
					            case AosElementAttribute.Energy: baseJewel.Resistances.Energy = value; break;
				            }

				            break;
			            }
			            case SkillName name:
			            {
				            AosSkillBonuses bonuses = baseJewel.SkillBonuses;

				            int index = GetAvailableSkillIndex(bonuses);

				            if (index is >= 0 and <= 4)
				            {
					            bonuses.SetValues(index, name, value);
				            }

				            break;
			            }
		            }

		            break;
	            }
            }

            item.InvalidateProperties();
        }

        public static bool UnravelItem(Mobile from, Item item, bool message = true)
        {
	        if (!CheckSoulForge(from, 2, out var bonus))
                return false;

            int weight = GetTotalWeight(item, -1, false, true);

            if (weight <= 0)
            {
                if (message)
                {
                    // You cannot magically unravel this item. It appears to possess little or no magic.
                    from.SendLocalizedMessage(1080437);
                }

                return false;
            }

			_ = GetContext(from);

			Type resType = null;
            int resAmount = Math.Max(1, weight / 100);

            bool success = false;

            if (weight >= 480 - bonus)
            {
                if (from.Skills[SkillName.Imbuing].Value < 95.0)
                {
                    if (message)
                    {
                        // Your Imbuing skill is not high enough to magically unravel this item.
                        from.SendLocalizedMessage(1080434);
                    }

                    return false;
                }

                if (from.CheckSkill(SkillName.Imbuing, 90.1, 120.0))
                {
                    success = true;
                    resType = typeof(RelicFragment);
                    resAmount = 1;
                }
                else if (from.CheckSkill(SkillName.Imbuing, 45.0, 95.0))
                {
                    success = true;
                    resType = typeof(EnchantedEssence);
                    resAmount = Math.Max(1, resAmount - Utility.Random(3));
                }
            }
            else if (weight > 200 - bonus && weight < 480 - bonus)
            {
                if (from.Skills[SkillName.Imbuing].Value < 45.0)
                {
                    if (message)
                    {
                        // Your Imbuing skill is not high enough to magically unravel this item.
                        from.SendLocalizedMessage(1080434);
                    }

                    return false;
                }

                if (from.CheckSkill(SkillName.Imbuing, 45.0, 95.0))
                {
                    success = true;
                    resType = typeof(EnchantedEssence);
                    resAmount = Math.Max(1, resAmount);
                }
                else if (from.CheckSkill(SkillName.Imbuing, 0.0, 45.0))
                {
                    success = true;
                    resType = typeof(MagicalResidue);
                    resAmount = Math.Max(1, resAmount + Utility.Random(2));
                }
            }
            else if (weight <= 200 - bonus)
            {
                if (from.CheckSkill(SkillName.Imbuing, 0.0, 45.0))
                {
                    success = true;
                    resType = typeof(MagicalResidue);
                    resAmount = Math.Max(1, resAmount + Utility.Random(2));
                }
            }
            else
            {
                if (message)
                {
                    // You cannot magically unravel this item. It appears to possess little or no magic.
                    from.SendLocalizedMessage(1080437);
                }

                return false;
            }

            if (!success)
            {
                return false;
            }

            while (resAmount > 0)
            {
	            if (Activator.CreateInstance(resType) is not Item res)
                {
                    break;
                }

                if (res.Stackable)
                {
                    res.Amount = Math.Max(1, Math.Min(60000, resAmount));
                }

                resAmount -= res.Amount;

                from.AddToBackpack(res);
            }

            item.Delete();

            return true;
        }

        public static int GetMaxWeight(object item)
        {
            int maxWeight = 450;

            if (item is IQuality { Quality: ItemQuality.Exceptional })
                maxWeight += 50;

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            switch (weapon)
		            {
			            case BaseThrown:
				            maxWeight += 0;
				            break;
			            case BaseRanged:
				            maxWeight += 50;
				            break;
			            default:
			            {
				            if (weapon.Layer == Layer.TwoHanded)
					            maxWeight += 100;
				            break;
			            }
		            }

		            break;
	            }
	            case BaseJewel:
		            maxWeight = 500;
		            break;
            }

            return maxWeight;
        }

        public static int GetMaxProps()
        {
            return 5;
        }

        public static int GetGemAmount(Item item, int id, int value)
        {
            var max = ItemPropertyInfo.GetMaxIntensity(item, id, true);
            var inc = ItemPropertyInfo.GetScale(item, id, false);

            if (max == 1 && inc == 0)
                return 10;

            double v = Math.Floor(value / ((double)max / 10));

            if (v > 10) v = 10;
            if (v < 1) v = 1;

            return (int)v;
        }

        public static int GetPrimaryAmount(Item item, int id, int value)
        {
            var max = ItemPropertyInfo.GetMaxIntensity(item, id, true);
            var inc = ItemPropertyInfo.GetScale(item, id, false);

            //if (item is BaseJewel && id == 12)
            //    max /= 2;

            if (max == 1 && inc == 0)
                return 5;

            double v = Math.Floor(value / (max / 5.0));

            if (v > 5) v = 5;
            if (v < 1) v = 1;

            return (int)v;
        }

        public static int GetSpecialAmount(Item item, int id, int value)
        {
            var max = ItemPropertyInfo.GetMaxIntensity(item, id, true);

            var intensity = (int)(value / (double)max * 100);

            return intensity switch
            {
	            >= 100 => 10,
	            >= 1 and > 90 => intensity - 90,
	            _ => 0
            };
        }

        [Usage("GetTotalMods")]
        [Description("Displays the total mods, ie AOS attributes for the targeted item.")]
        private static void GetTotalMods_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(12, false, TargetFlags.None, GetTotalMods_OnTarget);
            e.Mobile.SendMessage("Target the item to get total AOS Attributes.");
        }

        private static void GetTotalMods_OnTarget(Mobile from, object targeted)
        {
            if (targeted is Item item)
            {
                var ids = GetTotalMods(item);

                item.LabelTo(from, $"Total Mods: {ids}");
            }
            else
                from.SendMessage("That is not an item!");
        }

        public static int GetTotalMods(Item item, int id = -1)
        {
            var total = 0;
            var prop = ItemPropertyInfo.GetAttribute(id);

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            foreach (int i in Enum.GetValues(typeof(AosAttribute)))
		            {
			            AosAttribute attr = (AosAttribute)i;

			            if (!ItemPropertyInfo.ValidateProperty(attr))
				            continue;

			            switch (weapon.Attributes[attr])
			            {
				            case > 0:
				            {
					            if (prop is not AosAttribute attribute || attribute != attr)
						            total++;
					            break;
				            }
				            case 0 when attr == AosAttribute.CastSpeed && weapon.Attributes[AosAttribute.SpellChanneling] > 0:
				            {
					            if (prop is not AosAttribute attribute || attribute != attr)
						            total++;
					            break;
				            }
			            }
		            }

		            total += GetSkillBonuses(weapon.SkillBonuses, prop);

		            foreach (long i in Enum.GetValues(typeof(AosWeaponAttribute)))
		            {
			            AosWeaponAttribute attr = (AosWeaponAttribute)i;

			            if (!ItemPropertyInfo.ValidateProperty(attr))
				            continue;

			            if (weapon.WeaponAttributes[attr] <= 0)
				            continue;

			            if (IsHitAreaOrSpell(attr, id))
				            continue;

			            if (prop is not AosWeaponAttribute attribute || attribute != attr)
				            total++;
		            }

		            foreach (int i in Enum.GetValues(typeof(ExtendedWeaponAttribute)))
		            {
			            ExtendedWeaponAttribute attr = (ExtendedWeaponAttribute)i;

			            if (!ItemPropertyInfo.ValidateProperty(attr))
				            continue;

			            if (weapon.ExtendedWeaponAttributes[attr] <= 0)
				            continue;

			            if (prop is not ExtendedWeaponAttribute attribute || attribute != attr)
				            total++;
		            }

		            if (weapon.Slayer != SlayerName.None && (prop is not SlayerName name || name != weapon.Slayer))
			            total++;

		            if (weapon.Slayer2 != SlayerName.None)
			            total++;

		            if (weapon.Slayer3 != TalismanSlayerName.None)
			            total++;

		            total += (from int i in Enum.GetValues(typeof(SAAbsorptionAttribute)) select (SAAbsorptionAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where weapon.AbsorptionAttributes[attr] > 0 select attr).Count(attr => prop is not SAAbsorptionAttribute attribute || attribute != attr);

		            if (weapon is BaseRanged ranged && prop is not string)
		            {
			            if (ranged.Velocity > 0 && id != 60)
				            total++;
		            }

		            if (weapon.SearingWeapon)
			            total++;
		            break;
	            }
	            case BaseArmor baseArmor:
	            {
		            foreach (int i in Enum.GetValues(typeof(AosAttribute)))
		            {
			            AosAttribute attr = (AosAttribute)i;

			            if (!ItemPropertyInfo.ValidateProperty(attr))
				            continue;

			            switch (baseArmor.Attributes[attr])
			            {
				            case > 0:
				            {
					            if (prop is not AosAttribute attribute || attribute != attr)
						            total++;
					            break;
				            }
				            case 0 when attr == AosAttribute.CastSpeed && baseArmor.Attributes[AosAttribute.SpellChanneling] > 0:
				            {
					            if (prop is not AosAttribute attribute || attribute == attr)
						            total++;
					            break;
				            }
			            }
		            }

		            total += GetSkillBonuses(baseArmor.SkillBonuses, prop);

		            if (baseArmor.PhysicalBonus > baseArmor.PhysNonImbuing && id != 51) { total++; }
		            if (baseArmor.FireBonus > baseArmor.FireNonImbuing && id != 52) { total++; }
		            if (baseArmor.ColdBonus > baseArmor.ColdNonImbuing && id != 53) { total++; }
		            if (baseArmor.PoisonBonus > baseArmor.PoisonNonImbuing && id != 54) { total++; }
		            if (baseArmor.EnergyBonus > baseArmor.EnergyNonImbuing && id != 55) { total++; }

		            foreach (int i in Enum.GetValues(typeof(AosArmorAttribute)))
		            {
			            AosArmorAttribute attr = (AosArmorAttribute)i;

			            if (!ItemPropertyInfo.ValidateProperty(attr))
				            continue;

			            if (baseArmor.ArmorAttributes[attr] <= 0)
				            continue;

			            if (prop is not AosArmorAttribute attribute || attribute != attr)
				            total++;
		            }


		            total += (from int i in Enum.GetValues(typeof(SAAbsorptionAttribute)) select (SAAbsorptionAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where baseArmor.AbsorptionAttributes[attr] > 0 select attr).Count(attr => prop is not SAAbsorptionAttribute attribute || attribute != attr);

		            break;
	            }
	            case BaseJewel jewel:
	            {
		            BaseJewel j = jewel;

		            total += (from int i in Enum.GetValues(typeof(AosAttribute)) select (AosAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where j.Attributes[attr] > 0 select attr).Count(attr => prop is not AosAttribute attribute || attribute != attr);

		            total += (from int i in Enum.GetValues(typeof(SAAbsorptionAttribute)) select (SAAbsorptionAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where j.AbsorptionAttributes[attr] > 0 select attr).Count(attr => prop is not SAAbsorptionAttribute attribute || attribute != attr);

		            total += GetSkillBonuses(j.SkillBonuses, prop);

		            if (j.Resistances.Physical > 0 && id != 51) { total++; }
		            if (j.Resistances.Fire > 0 && id != 52) { total++; }
		            if (j.Resistances.Cold > 0 && id != 53) { total++; }
		            if (j.Resistances.Poison > 0 && id != 54) { total++; }
		            if (j.Resistances.Energy > 0 && id != 55) { total++; }

		            break;
	            }
	            case BaseClothing baseClothing:
	            {
		            BaseClothing clothing = baseClothing;

		            total += (from int i in Enum.GetValues(typeof(AosAttribute)) select (AosAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where clothing.Attributes[attr] > 0 select attr).Count(attr => prop is not AosAttribute attribute || attribute != attr);

		            total += (from int i in Enum.GetValues(typeof(SAAbsorptionAttribute)) select (SAAbsorptionAttribute)i into attr where ItemPropertyInfo.ValidateProperty(attr) where clothing.SAAbsorptionAttributes[attr] > 0 select attr).Count(attr => prop is not SAAbsorptionAttribute attribute || attribute != attr);

		            total += GetSkillBonuses(clothing.SkillBonuses, prop);

		            if (clothing.Resistances.Physical > clothing.PhysNonImbuing && id != 51) { total++; }
		            if (clothing.Resistances.Fire > clothing.FireNonImbuing && id != 52) { total++; }
		            if (clothing.Resistances.Cold > clothing.ColdNonImbuing && id != 53) { total++; }
		            if (clothing.Resistances.Poison > clothing.PoisonNonImbuing && id != 54) { total++; }
		            if (clothing.Resistances.Energy > clothing.EnergyNonImbuing && id != 55) { total++; }

		            break;
	            }
            }

            Type type = item.GetType();

            if (IsDerivedArmorOrClothing(type))
            {
                int[] resists = null;

                if (ResistBuffer != null && ResistBuffer.ContainsKey(type))
                {
                    resists = ResistBuffer[type];
                }
                else
                {
                    Type baseType = type.BaseType;

                    if (IsDerivedArmorOrClothing(baseType))
                    {
                        Item temp = Loot.Construct(baseType);

                        if (temp != null)
                        {
                            resists = new int[5];

                            resists[0] = GetBaseResistBonus(item, AosElementAttribute.Physical) - GetBaseResistBonus(temp, AosElementAttribute.Physical);
                            resists[1] = GetBaseResistBonus(item, AosElementAttribute.Fire) - GetBaseResistBonus(temp, AosElementAttribute.Fire);
                            resists[2] = GetBaseResistBonus(item, AosElementAttribute.Cold) - GetBaseResistBonus(temp, AosElementAttribute.Cold);
                            resists[3] = GetBaseResistBonus(item, AosElementAttribute.Poison) - GetBaseResistBonus(temp, AosElementAttribute.Poison);
                            resists[4] = GetBaseResistBonus(item, AosElementAttribute.Energy) - GetBaseResistBonus(temp, AosElementAttribute.Energy);

                            ResistBuffer ??= new Dictionary<Type, int[]>();

                            ResistBuffer[type] = resists;
                            temp.Delete();
                        }
                    }
                }

                if (resists != null)
                {
	                total += resists.Where((t, i) => id != 51 + i && t > 0).Count();
                }
            }

            return total;
        }

        private static bool IsHitAreaOrSpell(AosWeaponAttribute attr, int id)
        {
	        return attr switch
	        {
		        >= AosWeaponAttribute.HitMagicArrow and <= AosWeaponAttribute.HitDispel => id is >= 35 and <= 39,
		        >= AosWeaponAttribute.HitColdArea and <= AosWeaponAttribute.HitPhysicalArea => id is >= 30 and <= 34,
		        _ => false
	        };
        }

        /*private static bool IsInSkillGroup(SkillName skill, int index)
        {
            if (index < 0 || index >= m_SkillGroups.Length)
                return false;

            foreach (SkillName name in m_SkillGroups[index])
            {
                if (name == skill)
                    return true;
            }
            return false;
        }*/

        private static int GetSkillBonuses(AosSkillBonuses bonus, object prop)
        {
            var id = 0;

            for (var j = 0; j < 5; j++)
            {
	            if (!(bonus.GetBonus(j) > 0))
		            continue;

	            if (prop is not SkillName name || !IsInSkillGroup(bonus.GetSkill(j), name))
		            id += 1;
            }

            return id;
        }

        [Usage("GetTotalWeight")]
        [Description("Displays the total imbuing weight of the targeted item.")]
        public static void GetTotalWeight_OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(12, false, TargetFlags.None, GetTotalWeight_OnTarget);
            e.Mobile.SendMessage("Target the item to get total imbuing weight.");
        }

        public static void GetTotalWeight_OnTarget(Mobile from, object targeted)
        {
            if (targeted is Item item)
            {
                int w = GetTotalWeight(item, -1, false, true);
                item.LabelTo(from, $"Imbuing Weight: {w}");
                w = GetTotalWeight(item, -1, false, false);
                item.LabelTo(from, $"Loot Weight: {w}");
                w = GetTotalWeight(item, -1, true, true);
                item.LabelTo(from, $"True Weight: {w}");
            }
            else
                from.SendMessage("That is not an item!");
        }

        private static Dictionary<Type, int[]> ResistBuffer { get; set; }

        public static int GetTotalWeight(Item item, int id, bool trueWeight, bool imbuing)
        {
            double weight = 0;

            AosAttributes aosAttrs = RunicReforging.GetAosAttributes(item);
            AosWeaponAttributes wepAttrs = RunicReforging.GetAosWeaponAttributes(item);
            SAAbsorptionAttributes saAttrs = RunicReforging.GetSaAbsorptionAttributes(item);
            AosArmorAttributes armorAttrs = RunicReforging.GetAosArmorAttributes(item);
            AosElementAttributes resistAttrs = RunicReforging.GetElementalAttributes(item);
            ExtendedWeaponAttributes extattrs = RunicReforging.GetExtendedWeaponAttributes(item);

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            if (weapon.Slayer != SlayerName.None)
			            weight += GetIntensityForAttribute(weapon, weapon.Slayer, id, 1, trueWeight, imbuing);

		            if (weapon.Slayer2 != SlayerName.None)
			            weight += GetIntensityForAttribute(weapon, weapon.Slayer2, id, 1, trueWeight, imbuing);

		            if (weapon.Slayer3 != TalismanSlayerName.None)
			            weight += GetIntensityForAttribute(weapon, weapon.Slayer3, id, 1, trueWeight, imbuing);

		            if (weapon.SearingWeapon)
			            weight += GetIntensityForAttribute(weapon, "SearingWeapon", id, 1, trueWeight, imbuing);

		            if (weapon is BaseRanged { Velocity: > 0 } ranged) weight += GetIntensityForAttribute(weapon, "WeaponVelocity", id, ranged.Velocity, trueWeight, imbuing);

		            break;
	            }
	            case BaseArmor armor:
	            {
		            if (armor.PhysicalBonus > armor.PhysNonImbuing) { if (id != 51) { weight += 100.0 / 15 * (armor.PhysicalBonus - armor.PhysNonImbuing); } }
		            if (armor.FireBonus > armor.FireNonImbuing) { if (id != 52) { weight += 100.0 / 15 * (armor.FireBonus - armor.FireNonImbuing); } }
		            if (armor.ColdBonus > armor.ColdNonImbuing) { if (id != 53) { weight += 100.0 / 15 * (armor.ColdBonus - armor.ColdNonImbuing); } }
		            if (armor.PoisonBonus > armor.PoisonNonImbuing) { if (id != 54) { weight += 100.0 / 15 * (armor.PoisonBonus - armor.PoisonNonImbuing); } }
		            if (armor.EnergyBonus > armor.EnergyNonImbuing) { if (id != 55) { weight += 100.0 / 15 * (armor.EnergyBonus - armor.EnergyNonImbuing); } }

		            break;
	            }
            }

            Type type = item.GetType();

            if (IsDerivedArmorOrClothing(type))
            {
                int[] resists = null;

                if (ResistBuffer != null && ResistBuffer.ContainsKey(type))
                {
                    resists = ResistBuffer[type];
                }
                else
                {
                    Type baseType = type.BaseType;

                    if (IsDerivedArmorOrClothing(baseType))
                    {
                        Item temp = Loot.Construct(baseType);

                        if (temp != null)
                        {
                            resists = new int[5];

                            resists[0] = GetBaseResistBonus(item, AosElementAttribute.Physical) - GetBaseResistBonus(temp, AosElementAttribute.Physical);
                            resists[1] = GetBaseResistBonus(item, AosElementAttribute.Fire) - GetBaseResistBonus(temp, AosElementAttribute.Fire);
                            resists[2] = GetBaseResistBonus(item, AosElementAttribute.Cold) - GetBaseResistBonus(temp, AosElementAttribute.Cold);
                            resists[3] = GetBaseResistBonus(item, AosElementAttribute.Poison) - GetBaseResistBonus(temp, AosElementAttribute.Poison);
                            resists[4] = GetBaseResistBonus(item, AosElementAttribute.Energy) - GetBaseResistBonus(temp, AosElementAttribute.Energy);

                            ResistBuffer ??= new Dictionary<Type, int[]>();

                            ResistBuffer[type] = resists;
                            temp.Delete();
                        }
                    }
                }

                if (resists != null)
                {
	                weight += resists.Where((t, i) => id != 51 + i && t > 0).Sum(t => 100.0 / 15 * t);
                }
            }

            if (aosAttrs != null) weight = Enum.GetValues(typeof(AosAttribute)).Cast<int>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (AosAttribute)i, id, aosAttrs[(AosAttribute)i], trueWeight, imbuing));

            if (wepAttrs != null) weight = Enum.GetValues(typeof(AosWeaponAttribute)).Cast<long>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (AosWeaponAttribute)i, id, wepAttrs[(AosWeaponAttribute)i], trueWeight, imbuing));

            if (saAttrs != null) weight = Enum.GetValues(typeof(SAAbsorptionAttribute)).Cast<int>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (SAAbsorptionAttribute)i, id, saAttrs[(SAAbsorptionAttribute)i], trueWeight, imbuing));

            if (armorAttrs != null) weight = Enum.GetValues(typeof(AosArmorAttribute)).Cast<int>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (AosArmorAttribute)i, id, armorAttrs[(AosArmorAttribute)i], trueWeight, imbuing));

            if (resistAttrs != null && item is not BaseWeapon) weight = Enum.GetValues(typeof(AosElementAttribute)).Cast<int>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (AosElementAttribute)i, id, resistAttrs[(AosElementAttribute)i], trueWeight, imbuing));

            if (extattrs != null) weight = Enum.GetValues(typeof(ExtendedWeaponAttribute)).Cast<int>().Aggregate(weight, (current, i) => current + GetIntensityForAttribute(item, (ExtendedWeaponAttribute)i, id, extattrs[(ExtendedWeaponAttribute)i], trueWeight, imbuing));

            weight += CheckSkillBonuses(item, id, trueWeight, imbuing);

            return (int)weight;
        }

        public static int[] GetBaseResists(Item item)
        {
            int[] resists;

            // Special items base resist don't count as a property or weight. Once that resist is imbued, 
            // it then uses the base class resistance as the base resistance. EA is stupid.
            if (item is IImbuableEquipement equipement && IsSpecialImbuable(item))
            {
                resists = equipement.BaseResists;
            }
            else
            {
                resists = new int[5];

                resists[0] = GetBaseResistBonus(item, AosElementAttribute.Physical);
                resists[1] = GetBaseResistBonus(item, AosElementAttribute.Fire);
                resists[2] = GetBaseResistBonus(item, AosElementAttribute.Cold);
                resists[3] = GetBaseResistBonus(item, AosElementAttribute.Poison);
                resists[4] = GetBaseResistBonus(item, AosElementAttribute.Energy);
            }

            return resists;
        }

        private static int GetBaseResistBonus(Item item, AosElementAttribute resist)
        {
            switch (resist)
            {
                case AosElementAttribute.Physical:
                    {
                        switch (item)
                        {
	                        case BaseArmor armor:
		                        return armor.BasePhysicalResistance;
	                        case BaseClothing clothing:
		                        return clothing.BasePhysicalResistance;
                        }

                        break;
                    }
                case AosElementAttribute.Fire:
                    {
                        switch (item)
                        {
	                        case BaseArmor armor:
		                        return armor.BaseFireResistance;
	                        case BaseClothing clothing:
		                        return clothing.BaseFireResistance;
                        }

                        break;
                    }
                case AosElementAttribute.Cold:
                    {
                        switch (item)
                        {
	                        case BaseArmor armor:
		                        return armor.BaseColdResistance;
	                        case BaseClothing clothing:
		                        return clothing.BaseColdResistance;
                        }

                        break;
                    }
                case AosElementAttribute.Poison:
                    {
                        switch (item)
                        {
	                        case BaseArmor armor:
		                        return armor.BasePoisonResistance;
	                        case BaseClothing clothing:
		                        return clothing.BasePoisonResistance;
                        }

                        break;
                    }
                case AosElementAttribute.Energy:
                    {
                        switch (item)
                        {
	                        case BaseArmor armor:
		                        return armor.BaseEnergyResistance;
	                        case BaseClothing clothing:
		                        return clothing.BaseEnergyResistance;
                        }

                        break;
                    }
            }

            return 0;
        }

        /// <summary>
        /// This is for special items such as artifacts, if you ever so chose to imbue them on your server. Without
        /// massive edits, this should never come back as true.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsDerivedArmorOrClothing(Type type)
        {
            if (IsSpecialImbuable(type))
            {
                return false;
            }

            return (type.IsSubclassOf(typeof(BaseClothing)) || type.IsSubclassOf(typeof(BaseArmor))) &&
                type != typeof(BaseHat) &&
                type != typeof(BaseShield) &&
                type != typeof(Item) &&
                type != typeof(BaseOuterTorso) &&
                type != typeof(BaseMiddleTorso) &&
                type != typeof(BaseOuterLegs) &&
                type != typeof(BasePants) &&
                type != typeof(BaseShirt) &&
                type != typeof(BaseWaist) &&
                type != typeof(BaseShoes) &&
                type != typeof(BaseCloak);
        }

        private static int CheckSkillBonuses(Item item, int check, bool trueWeight, bool imbuing)
        {
            double weight = 0;

            AosSkillBonuses skills = RunicReforging.GetAosSkillBonuses(item);

            if (skills == null)
	            return (int)weight;

            var id = -1;

            if (item is BaseJewel)
            {
	            id = check;
            }

            // Place Holder. THis is in case the skill weight/max intensity every changes
            var totalWeight = trueWeight ? 100 : ItemPropertyInfo.GetWeight(151);
            var maxInt = ItemPropertyInfo.GetMaxIntensity(item, 151, imbuing);

            for (var i = 0; i < 5; i++)
            {
	            double bonus = skills.GetBonus(i);

	            if (!(bonus > 0))
		            continue;
	            var attr = ItemPropertyInfo.GetAttribute(id);

	            if (attr is not SkillName name || !IsInSkillGroup(skills.GetSkill(i), name))
	            {
		            weight += totalWeight / maxInt * bonus;
	            }
            }

            return (int)weight;
        }

        public static SkillName[] PossibleSkills { get; } = {
	        SkillName.Swords,
	        SkillName.Fencing,
	        SkillName.Macing,
	        SkillName.Archery,
	        SkillName.Wrestling,
	        SkillName.Parry,
	        SkillName.Tactics,
	        SkillName.Anatomy,
	        SkillName.Healing,
	        SkillName.Magery,
	        SkillName.Meditation,
	        SkillName.EvalInt,
	        SkillName.MagicResist,
	        SkillName.AnimalTaming,
	        SkillName.AnimalLore,
	        SkillName.Veterinary,
	        SkillName.Musicianship,
	        SkillName.Provocation,
	        SkillName.Discordance,
	        SkillName.Peacemaking,
	        SkillName.Chivalry,
	        SkillName.Focus,
	        SkillName.Necromancy,
	        SkillName.Stealing,
	        SkillName.Stealth,
	        SkillName.SpiritSpeak,
	        SkillName.Bushido,
	        SkillName.Ninjitsu,
	        SkillName.Throwing,
	        SkillName.Mysticism
        };

        private static readonly SkillName[][] m_SkillGroups = {
            new[] { SkillName.Fencing, SkillName.Macing, SkillName.Swords, SkillName.Musicianship, SkillName.Magery },
            new[] { SkillName.Wrestling, SkillName.AnimalTaming, SkillName.SpiritSpeak, SkillName.Tactics, SkillName.Provocation },
            new[] { SkillName.Focus, SkillName.Parry, SkillName.Stealth, SkillName.Meditation, SkillName.AnimalLore, SkillName.Discordance },
            new[] { SkillName.Mysticism, SkillName.Bushido, SkillName.Necromancy, SkillName.Veterinary, SkillName.Stealing, SkillName.EvalInt, SkillName.Anatomy },
            new[] { SkillName.Peacemaking, SkillName.Ninjitsu, SkillName.Chivalry, SkillName.Archery, SkillName.MagicResist, SkillName.Healing, SkillName.Throwing }
        };

        public static SkillName[] GetSkillGroup(SkillName skill)
        {
            return m_SkillGroups.FirstOrDefault(list => list.Any(sk => sk == skill));
        }

        private static int GetAvailableSkillIndex(AosSkillBonuses skills)
        {
            for (var i = 0; i < 5; i++)
            {
                if (skills.GetBonus(i) == 0)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int GetSkillGroupIndex(SkillName skill)
        {
            for (var i = 0; i < m_SkillGroups.Length; i++)
            {
                if (m_SkillGroups[i].Any(sk => sk == skill))
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsInSkillGroup(SkillName one, SkillName two)
        {
            var skillGroup1 = GetSkillGroupIndex(one);
            var skillGroup2 = GetSkillGroupIndex(two);

            return skillGroup1 != -1 && skillGroup2 != -1 && skillGroup1 == skillGroup2;
        }

        public static bool CheckSoulForge(Mobile from, int range, out double bonus)
        {
            return CheckSoulForge(from, range, true, true, out bonus);
        }

        public static bool CheckSoulForge(Mobile from, int range, bool message)
        {
	        return CheckSoulForge(from, range, message, false, out _);
        }

        public static bool CheckSoulForge(Mobile from, int range, bool message = true, bool checkqueen = true)
        {
	        return CheckSoulForge(from, range, message, checkqueen, out _);
        }

        private static bool CheckSoulForge(Mobile from, int range, bool message, bool checkqueen, out double bonus)
        {
            PlayerMobile m = from as PlayerMobile;
            bonus = 0.0;

            ImbuingContext context = GetContext(m);
            Map map = from.Map;

            if (map == null)
                return false;

            bool isForge = false;

            IPooledEnumerable eable = map.GetItemsInRange(from.Location, range);

            foreach (Item item in eable)
            {
                if (item.ItemId is >= 0x4277 and <= 0x4286 || item.ItemId is >= 0x4263 and <= 0x4272 || item.ItemId is >= 17607 and <= 17610)
                {
                    isForge = true;
                    break;
                }
            }

            eable.Free();

            if (!isForge)
            {
                if (message)
                    from.SendLocalizedMessage(1079787); // You must be near a soulforge to imbue an item.

                return false;
            }

            if (checkqueen)
            {
                if (from.Region != null && from.Region.IsPartOf("Queen's Palace"))
                {
                    /*if (!Engines.Points.PointsSystem.QueensLoyalty.IsNoble(from))
                    {
                        if (message)
                        {
                            from.SendLocalizedMessage(1113736); // You must rise to the rank of noble in the eyes of the Gargoyle Queen before her majesty will allow you to use this soulforge.
                        }

                        return false;
                    }
                    else*/
                    //{
                        bonus = 0.06;
                    //}
                }
                else if (from.Region != null && from.Region.IsPartOf("Royal City"))
                {
                    bonus = 0.02;
                }
            }

            return true;
        }

        public static Type[] IngredTypes { get; } = {
	        typeof(MagicalResidue),     typeof(EnchantedEssence),       typeof(RelicFragment),

	        typeof(SeedOfRenewal),      typeof(ChagaMushroom),          typeof(CrystalShards),
	        typeof(BottleIchor),        typeof(ReflectiveWolfEye),      typeof(FaeryDust),
	        typeof(BouraPelt),          typeof(SilverSnakeSkin),        typeof(ArcanicRuneStone),
	        typeof(SlithTongue),        typeof(VoidOrb),                typeof(RaptorTeeth),
	        typeof(SpiderCarapace),     typeof(DaemonClaw),             typeof(VialOfVitriol),
	        typeof(GoblinBlood),        typeof(LavaSerpentCrust),       typeof(UndyingFlesh),
	        typeof(CrushedGlass),       typeof(CrystallineBlackrock),   typeof(PowderedIron),
	        typeof(ElvenFletching),     typeof(DelicateScales),

	        typeof(EssenceSingularity), typeof(EssenceBalance),       typeof(EssencePassion),
	        typeof(EssenceDirection),   typeof(EssencePrecision),       typeof(EssenceControl),
	        typeof(EssenceDiligence),   typeof(EssenceAchievement),     typeof(EssenceFeeling),
	        typeof(EssenceOrder),

	        typeof(ParasiticPlant),   typeof(LuminescentFungi),
	        typeof(FireRuby),           typeof(WhitePearl),             typeof(BlueDiamond),
	        typeof(Turquoise)
        };


        public static bool IsInNonImbueList(Type itemType)
        {
	        return m_CannotImbue.Any(type => type == itemType);
        }

        private static readonly Type[] m_CannotImbue = {
            typeof(GargishLeatherWingArmor), typeof(GargishClothWingArmor)
        };

        public static int GetValueForId(Item item, int id)
        {
            object attr = ItemPropertyInfo.GetAttribute(id);

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            if (id == 16 && weapon.Attributes.SpellChanneling > 0)
			            return weapon.Attributes[AosAttribute.CastSpeed] + 1;

		            switch (attr)
		            {
			            case AosAttribute attribute:
				            return weapon.Attributes[attribute];
			            case AosWeaponAttribute attribute:
				            return weapon.WeaponAttributes[attribute];
			            case ExtendedWeaponAttribute attribute:
				            return weapon.ExtendedWeaponAttributes[attribute];
			            case SAAbsorptionAttribute attribute:
				            return weapon.AbsorptionAttributes[attribute];
			            case SlayerName name when weapon.Slayer == name:
				            return 1;
			            default:
			            {
				            switch (id)
				            {
					            case 60 when weapon is BaseRanged ranged:
						            return ranged.Velocity;
					            case 62:
						            return weapon.SearingWeapon ? 1 : 0;
					            default:
					            {
						            if (attr is AosElementAttribute ele)
						            {
							            switch (ele)
							            {
								            case AosElementAttribute.Physical: return weapon.WeaponAttributes.ResistPhysicalBonus;
								            case AosElementAttribute.Fire: return weapon.WeaponAttributes.ResistFireBonus;
								            case AosElementAttribute.Cold: return weapon.WeaponAttributes.ResistColdBonus;
								            case AosElementAttribute.Poison: return weapon.WeaponAttributes.ResistPoisonBonus;
								            case AosElementAttribute.Energy: return weapon.WeaponAttributes.ResistEnergyBonus;
							            }
						            }

						            break;
					            }
				            }

				            break;
			            }
		            }

		            break;
	            }
	            case BaseArmor armor:
	            {
		            if (armor is BaseShield && id == 16 && armor.Attributes.SpellChanneling > 0)
			            return armor.Attributes[AosAttribute.CastSpeed] + 1;

		            switch (attr)
		            {
			            case AosAttribute attribute:
				            return armor.Attributes[attribute];
			            case AosArmorAttribute attribute:
				            return armor.ArmorAttributes[attribute];
			            case SAAbsorptionAttribute attribute:
				            return armor.AbsorptionAttributes[attribute];
			            case AosElementAttribute attribute:
			            {
				            var value = 0;

				            switch (attribute)
				            {
					            case AosElementAttribute.Physical: value = armor.PhysicalBonus; break;
					            case AosElementAttribute.Fire: value = armor.FireBonus; break;
					            case AosElementAttribute.Cold: value = armor.ColdBonus; break;
					            case AosElementAttribute.Poison: value = armor.PoisonBonus; break;
					            case AosElementAttribute.Energy: value = armor.EnergyBonus; break;
				            }

				            if (value > 0)
				            {
					            return value;
				            }

				            break;
			            }
		            }

		            break;
	            }
	            case BaseClothing clothing:
	            {
		            switch (attr)
		            {
			            case AosAttribute attribute:
				            return clothing.Attributes[attribute];
			            case AosElementAttribute attribute:
			            {
				            var value = clothing.Resistances[attribute];

				            if (value > 0)
				            {
					            return value;
				            }

				            break;
			            }
			            case AosArmorAttribute attribute:
				            return clothing.ClothingAttributes[attribute];
			            case SAAbsorptionAttribute attribute:
				            return clothing.SAAbsorptionAttributes[attribute];
		            }

		            break;
	            }
	            case BaseJewel jewel:
	            {
		            switch (attr)
		            {
			            case AosAttribute attribute:
				            return jewel.Attributes[attribute];
			            case AosElementAttribute attribute:
				            return jewel.Resistances[attribute];
			            case SAAbsorptionAttribute attribute:
				            return jewel.AbsorptionAttributes[attribute];
			            case SkillName name:
			            {
				            if (jewel.SkillBonuses.Skill_1_Name == name)
					            return (int)jewel.SkillBonuses.Skill_1_Value;

				            if (jewel.SkillBonuses.Skill_2_Name == name)
					            return (int)jewel.SkillBonuses.Skill_2_Value;

				            if (jewel.SkillBonuses.Skill_3_Name == name)
					            return (int)jewel.SkillBonuses.Skill_3_Value;

				            if (jewel.SkillBonuses.Skill_4_Name == name)
					            return (int)jewel.SkillBonuses.Skill_4_Value;

				            if (jewel.SkillBonuses.Skill_5_Name == name)
					            return (int)jewel.SkillBonuses.Skill_5_Value;
				            break;
			            }
		            }

		            break;
	            }
            }

            Type type = item.GetType();

            if (id is >= 51 and <= 55 && IsDerivedArmorOrClothing(type))
            {
                int[] resists = null;

                if (ResistBuffer != null && ResistBuffer.ContainsKey(type))
                {
                    resists = ResistBuffer[type];
                }
                else
                {
                    Type baseType = type.BaseType;

                    if (IsDerivedArmorOrClothing(baseType))
                    {
                        Item temp = Loot.Construct(baseType);

                        if (temp != null)
                        {
                            resists = new int[5];

                            resists[0] = GetBaseResistBonus(item, AosElementAttribute.Physical) - GetBaseResistBonus(temp, AosElementAttribute.Physical);
                            resists[1] = GetBaseResistBonus(item, AosElementAttribute.Fire) - GetBaseResistBonus(temp, AosElementAttribute.Fire);
                            resists[2] = GetBaseResistBonus(item, AosElementAttribute.Cold) - GetBaseResistBonus(temp, AosElementAttribute.Cold);
                            resists[3] = GetBaseResistBonus(item, AosElementAttribute.Poison) - GetBaseResistBonus(temp, AosElementAttribute.Poison);
                            resists[4] = GetBaseResistBonus(item, AosElementAttribute.Energy) - GetBaseResistBonus(temp, AosElementAttribute.Energy);

                            ResistBuffer ??= new Dictionary<Type, int[]>();

                            ResistBuffer[type] = resists;
                            temp.Delete();
                        }
                    }
                }

                if (resists is { Length: 5 })
                {
                    return resists[id - 51];
                }
            }

            return 0;
        }

        public static int GetIntensityForAttribute(Item item, object attr, int checkId, int value, bool trueWeight = false)
        {
            return GetIntensityForId(item, ItemPropertyInfo.GetId(attr), checkId, value, trueWeight, true);
        }

        private static int GetIntensityForAttribute(Item item, object attr, int checkId, int value, bool trueWeight, bool imbuing)
        {
            return GetIntensityForId(item, ItemPropertyInfo.GetId(attr), checkId, value, trueWeight, imbuing);
        }

        public static int GetIntensityForId(Item item, int id, int checkId, int value, bool trueWeight = false)
        {
            return GetIntensityForId(item, id, checkId, value, trueWeight, true);
        }

        private static int GetIntensityForId(Item item, int id, int checkId, int value, bool trueWeight, bool imbuing)
        {
            // This is terribly clunky, however we're accommodating 1 out of 50+ attributes that acts differently
            if (value <= 0 && id != 16)
            {
                return 0;
            }

            if (id == 61 && item is not BaseRanged)
            {
                id = 63;
            }

            if (item is BaseWeapon or BaseShield && id == 16)
            {
                AosAttributes attrs = RunicReforging.GetAosAttributes(item);

                if (attrs is { SpellChanneling: > 0 })
                    value++;
            }

            if (value <= 0)
                return 0;

            if (id == checkId)
	            return 0;
            int weight = trueWeight ? 100 : ItemPropertyInfo.GetWeight(id);

            if (weight == 0)
            {
	            return 0;
            }

            int max = ItemPropertyInfo.GetMaxIntensity(item, id, imbuing);

            return (int)((double)weight / max * value);

        }

        public static bool CanImbueProperty(Mobile from, Item item, int id)
        {
            ItemPropertyInfo info = ItemPropertyInfo.GetInfo(id);
            bool canImbue = false;

            if (info != null)
            {
	            switch (item)
	            {
		            case BaseRanged when info.CanImbue(ItemType.Ranged):
			            canImbue = true;
			            break;
		            case BaseWeapon:
		            {
			            if (info.CanImbue(ItemType.Melee))
			            {
				            canImbue = true;
			            }

			            break;
		            }
		            case BaseShield when info.CanImbue(ItemType.Shield):
			            canImbue = true;
			            break;
		            case BaseArmor:
		            {
			            if (info.CanImbue(ItemType.Armor))
			            {
				            canImbue = true;
			            }

			            break;
		            }
		            case BaseJewel when info.CanImbue(ItemType.Jewel):
			            canImbue = true;
			            break;
	            }
            }

            if (canImbue)
	            return true;

            from.CloseGump(typeof(ImbueGump));
            from.SendLocalizedMessage(1114291); // You cannot imbue the last property on that item.

            return false;
        }

        public static int TimesImbued(Item item)
        {
	        return item is IImbuableEquipement equipement ? equipement.TimesImbued : 0;
        }
    }
}
