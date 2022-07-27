using Server.Items;
using Server.Mobiles;
using Server.SkillHandlers;
using System;
using System.Linq;

namespace Server.Gumps
{
    public class ImbueGump : BaseGump
    {
        private const int LabelColor = 0x7FFF;
        private const int IceHue = 0x481;
        private const int Green = 0x41;
        private const int Yellow = 0x36;
        private const int DarkYellow = 0x2E;
        private const int Red = 0x26;
        private readonly int m_Id;
        private int m_Value;
        private readonly Item m_Item;
        private int m_TotalItemWeight;
        private int m_TotalProps;
        private int m_MaxWeight;

        private ItemPropertyInfo m_Info;

        public ImbueGump(PlayerMobile pm, Item item, int id, int value)
            : base(pm)
        {
            pm.CloseGump(typeof(ImbuingGump));
            pm.CloseGump(typeof(ImbueSelectGump));
            pm.CloseGump(typeof(RunicReforgingGump));

            m_Id = id;
            m_Value = value;
            m_Item = item;
        }

        public override void AddGumpLayout()
        {
	        if (!Imbuing.CheckSoulForge(User, 2, out var bonus))
                return;

            ImbuingContext context = Imbuing.GetContext(User);

            if (!ItemPropertyInfo.Table.ContainsKey(m_Id))
                return;

            m_Info = ItemPropertyInfo.Table[m_Id];

            int minInt = ItemPropertyInfo.GetMinIntensity(m_Item, m_Id);
            int maxInt = ItemPropertyInfo.GetMaxIntensity(m_Item, m_Id, true);
            int weight = m_Info.Weight;
            int scale = ItemPropertyInfo.GetScale(m_Item, m_Id, false);
            int start = minInt - scale;

            if (m_Value < minInt)
            {
                m_Value = minInt;
            }

            if (m_Value > maxInt)
            {
                m_Value = maxInt;
            }

            double currentIntensity = Math.Floor((m_Value - start) / ((double)maxInt - start) * m_Info.Weight);

            // Set context
            context.LastImbued = m_Item;
            context.ImbueMod = m_Id;
            context.ImbueModVal = weight;

            // Current Mod Weight
            m_TotalItemWeight = Imbuing.GetTotalWeight(m_Item, m_Id, false, true);
            m_TotalProps = Imbuing.GetTotalMods(m_Item, m_Id);

            if (maxInt <= 1)
                currentIntensity = m_Info.Weight;

            int propWeight = (int)Math.Floor(weight / (double)maxInt * m_Value);

            // Maximum allowed Property Weight & Item Mod Count
            m_MaxWeight = Imbuing.GetMaxWeight(m_Item);

            // Times Item has been Imbued
            int timesImbued = Imbuing.TimesImbued(m_Item);

            // Check Ingredients needed at the current Intensity
            int gemAmount = Imbuing.GetGemAmount(m_Item, m_Id, m_Value);
            int primResAmount = Imbuing.GetPrimaryAmount(m_Item, m_Id, m_Value);
            int specResAmount = Imbuing.GetSpecialAmount(m_Item, m_Id, m_Value);

            AddPage(0);

            AddBackground(0, 0, 520, 440, 5054);
            AddImageTiled(10, 10, 500, 20, 2624);
            AddImageTiled(10, 40, 245, 140, 2624);
            AddImageTiled(265, 40, 245, 140, 2624);
            AddImageTiled(10, 190, 245, 140, 2624);
            AddImageTiled(265, 190, 245, 140, 2624);
            AddImageTiled(10, 340, 500, 60, 2624);
            AddImageTiled(10, 410, 500, 20, 2624);

            AddAlphaRegion(10, 10, 500, 420);

            AddHtmlLocalized(10, 12, 520, 20, 1079717, LabelColor, false, false); // <CENTER>IMBUING CONFIRMATION</CENTER>
            AddHtmlLocalized(50, 50, 250, 20, 1114269, LabelColor, false, false); // PROPERTY INFORMATION

            AddHtmlLocalized(25, 80, 390, 20, 1114270, LabelColor, false, false);  // Property:

            if (!m_Info.AttributeName.IsEmpty)
            {
                AddHtmlLocalized(95, 80, 150, 20, 1114057, m_Info.AttributeName.ToString(), LabelColor, false, false);
            }

            AddHtmlLocalized(25, 100, 390, 20, 1114271, LabelColor, false, false); // Replaces:
            TextDefinition replace = WhatReplacesWhat(m_Id, m_Item);

            if (!replace.IsEmpty)
            {
                AddHtmlLocalized(95, 100, 150, 20, 1114057, replace.ToString(), LabelColor, false, false);
            }

            // Weight Modifier
            AddHtmlLocalized(25, 120, 200, 20, 1114272, 0xFFFFFF, false, false); // Weight:
            AddLabel(95, 120, IceHue, $"{m_Info.Weight / 100.0:0.0}x");

            AddHtmlLocalized(25, 140, 200, 20, 1114273, LabelColor, false, false); // Intensity:
            AddLabel(95, 140, IceHue, $"{currentIntensity}%");

            // Materials needed
            AddHtmlLocalized(100, 200, 80, 20, 1044055, LabelColor, false, false); // <CENTER>MATERIALS</CENTER>

            AddHtmlLocalized(40, 220, 390, 20, 1114057, $"#{m_Info.PrimaryName.Number}", LabelColor, false, false);
            AddLabel(210, 220, IceHue, primResAmount.ToString());

            AddHtmlLocalized(40, 240, 390, 20, 1114057, $"#{m_Info.GemName.Number}", LabelColor, false, false);
            AddLabel(210, 240, IceHue, gemAmount.ToString());

            if (specResAmount > 0)
            {
                AddHtmlLocalized(40, 260, 390, 20, 1114057,
	                $"#{ItemPropertyInfo.GetSpecialResName(m_Item, m_Info).Number}", LabelColor, false, false);
                AddLabel(210, 260, IceHue, specResAmount.ToString());
            }

            // Mod Description
            AddHtmlLocalized(280, 55, 205, 115, m_Info.Description, LabelColor, false, false);

            AddHtmlLocalized(350, 200, 65, 20, 1113650, LabelColor, false, false); // RESULTS

            AddHtmlLocalized(280, 220, 140, 20, 1113645, LabelColor, false, false); // Properties:
            AddLabel(430, 220, GetColor(m_TotalProps + 1, 5), $"{m_TotalProps + 1}/{Imbuing.GetMaxProps()}");

            int projWeight = m_TotalItemWeight + propWeight;
            AddHtmlLocalized(280, 240, 260, 20, 1113646, LabelColor, false, false); // Total Property Weight:
            AddLabel(430, 240, GetColor(projWeight, m_MaxWeight), $"{projWeight}/{m_MaxWeight}");

            AddHtmlLocalized(280, 260, 200, 20, 1113647, LabelColor, false, false); // Times Imbued:
            AddLabel(430, 260, GetColor(timesImbued, 20), $"{timesImbued}/20");

            // ===== CALCULATE DIFFICULTY =====
            int truePropWeight = (int)(propWeight / (double)weight * 100);
            int trueTotalWeight = Imbuing.GetTotalWeight(m_Item, -1, true, true);

            double suc = Imbuing.GetSuccessChance(User, m_Item, trueTotalWeight, truePropWeight, bonus);

            AddHtmlLocalized(300, 300, 250, 20, 1044057, 0xFFFFFF, false, false); // Success Chance:
            AddLabel(420, 300, GetSuccessChanceHue(suc), $"{suc:0.0}%");

            // - Attribute Level
            if (maxInt > 1)
            {
                AddHtmlLocalized(235, 350, 200, 20, 1062300, LabelColor, false, false); // New Value:

                if (m_Id == 41) // - Mage Weapon Value ( i.e [Mage Weapon -25] )
                {
                    AddLabel(250, 370, IceHue, $"-{30 - m_Value}");
                }
                else if (m_Id is > 150 and < 184) // Skill Property
                {
                    AddLabel(m_Value > 9 ? 252 : 256, 370, IceHue, $"+{m_Value}");
                }
                else if (maxInt <= 8 || m_Id is 21 or 17) // - Show Property Value as just Number ( i.e [Mana Regen 2] )
                {
                    AddLabel(m_Value > 9 ? 252 : 256, 370, IceHue, $"{m_Value}"); // - Show Property Value as % ( i.e [Hit Fireball 25%] )
                }
                else
                {
                    int val = m_Value;

                    if (m_Id is >= 51 and <= 55)
                    {
                        int[] resistances = Imbuing.GetBaseResists(m_Item);

                        switch (m_Id)
                        {
                            case 51: val += resistances[0]; break;
                            case 52: val += resistances[1]; break;
                            case 53: val += resistances[2]; break;
                            case 54: val += resistances[3]; break;
                            case 55: val += resistances[4]; break;
                        }
                    }

                    AddLabel(val > 9 ? 252 : 256, 370, IceHue, $"{val}%");
                }

                // Buttons
                AddButton(179, 372, 0x1464, 0x1464, 10053, GumpButtonType.Reply, 0);
                AddButton(187, 372, 0x1466, 0x1466, 10053, GumpButtonType.Reply, 0);

                AddButton(199, 372, 0x1464, 0x1464, 10052, GumpButtonType.Reply, 0);
                AddButton(207, 372, 0x1466, 0x1466, 10052, GumpButtonType.Reply, 0);

                AddButton(221, 372, 0x1464, 0x1464, 10051, GumpButtonType.Reply, 0);
                AddButton(229, 372, 0x1466, 0x1466, 10051, GumpButtonType.Reply, 0);

                AddButton(280, 372, 0x1464, 0x1464, 10054, GumpButtonType.Reply, 0);
                AddButton(288, 372, 0x1466, 0x1466, 10054, GumpButtonType.Reply, 0);

                AddButton(300, 372, 0x1464, 0x1464, 10055, GumpButtonType.Reply, 0);
                AddButton(308, 372, 0x1466, 0x1466, 10055, GumpButtonType.Reply, 0);

                AddButton(320, 372, 0x1464, 0x1464, 10056, GumpButtonType.Reply, 0);
                AddButton(328, 372, 0x1466, 0x1466, 10056, GumpButtonType.Reply, 0);

                AddLabel(322, 370, 0, ">");
                AddLabel(326, 370, 0, ">");
                AddLabel(330, 370, 0, ">");

                AddLabel(304, 370, 0, ">");
                AddLabel(308, 370, 0, ">");

                AddLabel(286, 370, 0, ">");

                AddLabel(226, 370, 0, "<");

                AddLabel(203, 370, 0, "<");
                AddLabel(207, 370, 0, "<");

                AddLabel(181, 370, 0, "<");
                AddLabel(185, 370, 0, "<");
                AddLabel(189, 370, 0, "<");
            }

            AddButton(15, 410, 4005, 4007, 10099, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 412, 100, 20, 1114268, LabelColor, false, false); // Back 

            AddButton(390, 410, 4005, 4007, 10100, GumpButtonType.Reply, 0);
            AddHtmlLocalized(425, 412, 120, 18, 1114267, LabelColor, false, false); // Imbue Item
        }

        private static int GetColor(int value, int limit)
        {
            if (value < limit)
                return Green;

            if (value == limit)
                return Yellow;

            return Red;
        }

        private static int GetSuccessChanceHue(double suc)
        {
	        return suc switch
	        {
		        >= 100 => IceHue,
		        >= 80 => Green,
		        >= 50 => Yellow,
		        >= 10 => DarkYellow,
		        _ => Red
	        };
        }

        public override void OnResponse(RelayInfo info)
        {
            ImbuingContext context = Imbuing.GetContext(User);

            switch (info.ButtonID)
            {
                case 0: //Close
                    {
                        User.EndAction(typeof(Imbuing));
                        break;
                    }
                case 10051: // Decrease Mod Value [<]
                    {
                        m_Value = Math.Max(ItemPropertyInfo.GetMinIntensity(m_Item, m_Info.Id), m_Value - ItemPropertyInfo.GetScale(m_Item, m_Info.Id, false));
                        Refresh();

                        break;
                    }
                case 10052:// Decrease Mod Value [<<]
                    {
                        m_Value = Math.Max(ItemPropertyInfo.GetMinIntensity(m_Item, m_Info.Id), m_Value - 10);
                        Refresh();

                        break;
                    }
                case 10053:// Minimum Mod Value [<<<]
                    {
                        m_Value = ItemPropertyInfo.GetMinIntensity(m_Item, m_Info.Id);
                        Refresh();

                        break;
                    }
                case 10054: // Increase Mod Value [>]
                    {
                        m_Value = Math.Min(ItemPropertyInfo.GetMaxIntensity(m_Item, m_Info.Id, true), m_Value + ItemPropertyInfo.GetScale(m_Item, m_Info.Id, false));
                        Refresh();

                        break;
                    }
                case 10055: // Increase Mod Value [>>]
                    {
                        m_Value = Math.Min(ItemPropertyInfo.GetMaxIntensity(m_Item, m_Info.Id, true), m_Value + 10);
                        Refresh();

                        break;
                    }
                case 10056: // Maximum Mod Value [>>>]
                    {
                        m_Value = ItemPropertyInfo.GetMaxIntensity(m_Item, m_Info.Id, true);
                        Refresh();

                        break;
                    }

                case 10099: // Back
                    {
                        SendGump(new ImbueSelectGump(User, context.LastImbued));
                        break;
                    }
                case 10100:  // Imbue the Item
                    {
                        if (Imbuing.OnBeforeImbue(User, m_TotalProps, Imbuing.GetMaxProps(), m_TotalItemWeight, m_MaxWeight))
                        {
                            Imbuing.TryImbueItem(User, m_Item, m_Id, m_Value);
                            SendGumpDelayed(User);
                        }

                        break;
                    }
            }
        }

        public static void SendGumpDelayed(PlayerMobile pm)
        {
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                SendGump(new ImbuingGump(pm));
            });
        }

        // =========== Check if Chosen Attribute Replaces Another =================
        private static TextDefinition WhatReplacesWhat(int id, Item item)
        {
            if (Imbuing.GetValueForId(item, id) > 0)
            {
                return ItemPropertyInfo.GetAttributeName(id);
            }

            switch (item)
            {
	            case BaseWeapon weapon:
	            {
		            switch (id)
		            {
			            // Slayers replace Slayers
			            case >= 101 and <= 127 when weapon.Slayer != SlayerName.None:
				            return GetNameForAttribute(weapon.Slayer);
			            case >= 101 and <= 127 when weapon.Slayer2 != SlayerName.None:
				            return GetNameForAttribute(weapon.Slayer2);
			            // OnHitEffect replace OnHitEffect
			            case >= 35 and <= 39 when weapon.WeaponAttributes.HitMagicArrow > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitMagicArrow);
			            case >= 35 and <= 39 when weapon.WeaponAttributes.HitHarm > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitHarm);
			            case >= 35 and <= 39 when weapon.WeaponAttributes.HitFireball > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitFireball);
			            case >= 35 and <= 39 when weapon.WeaponAttributes.HitLightning > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitLightning);
			            case >= 35 and <= 39 when weapon.WeaponAttributes.HitDispel > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitDispel);
			            // OnHitArea replace OnHitArea
			            case >= 30 and <= 34 when weapon.WeaponAttributes.HitPhysicalArea > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitPhysicalArea);
			            case >= 30 and <= 34 when weapon.WeaponAttributes.HitColdArea > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitFireArea);
			            case >= 30 and <= 34 when weapon.WeaponAttributes.HitFireArea > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitColdArea);
			            case >= 30 and <= 34 when weapon.WeaponAttributes.HitPoisonArea > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitPoisonArea);
			            case >= 30 and <= 34 when weapon.WeaponAttributes.HitEnergyArea > 0:
				            return GetNameForAttribute(AosWeaponAttribute.HitEnergyArea);
		            }

		            break;
	            }
	            case BaseJewel baseJewel:
	            {
		            if (id is >= 151 and <= 183)
		            {
			            AosSkillBonuses bonuses = baseJewel.SkillBonuses;
			            SkillName[] group = Imbuing.GetSkillGroup((SkillName)ItemPropertyInfo.GetAttribute(id));

			            for (var i = 0; i < 5; i++)
			            {
				            if (bonuses.GetBonus(i) > 0 && group.Any(sk => sk == bonuses.GetSkill(i)))
				            {
					            return GetNameForAttribute(bonuses.GetSkill(i));
				            }
			            }
		            }

		            break;
	            }
            }

            return null;
        }

        private static TextDefinition GetNameForAttribute(object attribute)
        {
            if (attribute is AosArmorAttribute.LowerStatReq)
                attribute = AosWeaponAttribute.LowerStatReq;

            if (attribute is AosArmorAttribute.DurabilityBonus)
                attribute = AosWeaponAttribute.DurabilityBonus;

            foreach (ItemPropertyInfo info in ItemPropertyInfo.Table.Values)
            {
                switch (attribute)
                {
	                case SlayerName name when info.Attribute is SlayerName slayerName && name == slayerName:
		                return info.AttributeName;
	                case AosAttribute aosAttribute when info.Attribute is AosAttribute infoAttribute && aosAttribute == infoAttribute:
		                return info.AttributeName;
	                case AosWeaponAttribute weaponAttribute when info.Attribute is AosWeaponAttribute infoAttribute && weaponAttribute == infoAttribute:
		                return info.AttributeName;
	                case SkillName name when info.Attribute is SkillName skillName && name == skillName:
		                return info.AttributeName;
                }

                if (info.Attribute == attribute)
                    return info.AttributeName;
            }

            return null;
        }
    }
}
