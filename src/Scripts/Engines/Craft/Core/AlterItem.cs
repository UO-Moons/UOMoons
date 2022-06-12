using Server.Engines.VeteranRewards;
using Server.Items;
using Server.SkillHandlers;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Engines.Craft
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AlterableAttribute : Attribute
    {
        public Type CraftSystem { get; private set; }
        public Type AlteredType { get; private set; }
        public bool Inherit { get; private set; }

        public AlterableAttribute(Type craftSystem, Type alteredType, bool inherit = false)
        {
            CraftSystem = craftSystem;
            AlteredType = alteredType;
            Inherit = inherit;
        }

        /// <summary>
        /// this enables any craftable item where their parent class can be altered, can be altered too.
        /// This is mainly for the ML craftable artifacts.
        /// </summary>
        /// <returns></returns>
        public bool CheckInherit(Type original)
        {
            if (Inherit)
            {
                return true;
            }

            var system = CraftContext.Systems.FirstOrDefault(sys => sys.GetType() == CraftSystem);

            if (system != null)
            {
                return system.CraftItems.SearchFor(original) != null;
            }

            return false;
        }
    }

    public class AlterItem
    {
        public static void BeginTarget(Mobile from, CraftSystem system, ITool tool)
        {
            from.Target = new AlterItemTarget(system, tool);
            from.SendLocalizedMessage(1094730); //Target the item to altar
        }

        public static void BeginTarget(Mobile from, CraftSystem system, Item contract)
        {
            from.Target = new AlterItemTarget(system, contract);
            from.SendLocalizedMessage(1094730); //Target the item to altar
        }
    }

    public class AlterItemTarget : Target
    {
        private readonly CraftSystem m_System;
        private readonly ITool m_Tool;
        private readonly Item m_Contract;

        public AlterItemTarget(CraftSystem system, Item contract)
                : base(2, false, TargetFlags.None)
        {
            m_System = system;
            m_Contract = contract;
        }

        public AlterItemTarget(CraftSystem system, ITool tool)
            : base(1, false, TargetFlags.None)
        {
            m_System = system;
            m_Tool = tool;
        }

        private static AlterableAttribute GetAlterableAttribute(object o, bool inherit)
        {
            Type t = o.GetType();

            object[] attrs = t.GetCustomAttributes(typeof(AlterableAttribute), inherit);

            if (attrs != null && attrs.Length > 0)
            {
                if (attrs[0] is AlterableAttribute attr && (!inherit || attr.CheckInherit(t)))
                {
                    return attr;
                }
            }

            return null;
        }

        protected override void OnTarget(Mobile from, object o)
        {
            int number = -1;

            SkillName skill = m_System.MainSkill;
            double value = from.Skills[skill].Value;

            var alterInfo = GetAlterableAttribute(o, false);

            if (alterInfo == null)
            {
                alterInfo = GetAlterableAttribute(o, true);
            }

            if (o is not Item origItem || !origItem.IsChildOf(from.Backpack))
            {
                number = 1094729; // The item must be in your backpack for you to alter it.
            }
            else if (origItem is BlankScroll)
            {
                if (m_Contract == null)
                {
                    if (value >= 100.0)
                    {
                        Item contract = null;

                        if (skill == SkillName.Blacksmith)
                        {
                            contract = new AlterContract(RepairSkillType.Smithing, from);
                        }
                        else if (skill == SkillName.Carpentry)
                        {
                            contract = new AlterContract(RepairSkillType.Carpentry, from);
                        }
                        else if (skill == SkillName.Tailoring)
                        {
                            contract = new AlterContract(RepairSkillType.Tailoring, from);
                        }
                        else if (skill == SkillName.Tinkering)
                        {
                            contract = new AlterContract(RepairSkillType.Tinkering, from);
                        }

                        if (contract != null)
                        {
                            from.AddToBackpack(contract);

                            number = 1044154; // You create the item.

                            // Consume a blank scroll
                            origItem.Consume();
                        }
                    }
                    else
                    {
                        number = 1111869; // You must be at least grandmaster level to create an alter service contract.
                    }
                }
                else
                {
                    number = 1094728; // You may not alter that item.
                }
            }
            else if (alterInfo == null)
            {
                number = 1094728; // You may not alter that item.
            }
            else if (!IsAlterable(origItem))
            {
                number = 1094728; // You may not alter that item.
            }
            else if (alterInfo.CraftSystem != m_System.GetType())
            {
                if (m_Tool != null)
                {
                    // You may not alter that item.
                    number = 1094728;
                }
                else
                {
                    // You cannot alter that item with this type of alter contract.
                    number = 1094793;
                }
            }
            else if (m_Contract == null && value < 100.0)
            {
                number = 1111870; // You must be at least grandmaster level to alter an item.
            }
            else if (origItem is BaseWeapon weapon && weapon.EnchantedWeilder != null)
            {
                number = 1111849; // You cannot alter an item that is currently enchanted.
            }
            else
            {
                if (Activator.CreateInstance(alterInfo.AlteredType) is not Item newitem)
                {
                    return;
                }

                if (origItem is BaseWeapon weapon1 && newitem is BaseWeapon weapon2)
                {
					weapon2.Slayer = weapon1.Slayer;
					weapon2.Slayer2 = weapon1.Slayer2;
					weapon2.Slayer3 = weapon1.Slayer3;
					weapon2.Resource = weapon1.Resource;

                    if (weapon1.PlayerConstructed)
                    {
						weapon2.PlayerConstructed = true;
						weapon2.Crafter = weapon1.Crafter;
						weapon2.Quality = weapon1.Quality;
                    }
					weapon2.Altered = true;
                }
                else if (origItem is BaseArmor armor && newitem is BaseArmor armor1)
                {
					if (armor.PlayerConstructed)
                    {
						armor1.PlayerConstructed = true;
						armor1.Crafter = armor.Crafter;
						armor1.Quality = armor.Quality;
                    }
					armor1.Resource = armor.Resource;
					armor1.PhysicalBonus = armor.PhysicalBonus;
					armor1.FireBonus = armor.FireBonus;
					armor1.ColdBonus = armor.ColdBonus;
					armor1.PoisonBonus = armor.PoisonBonus;
					armor1.EnergyBonus = armor.EnergyBonus;
					armor1.Altered = true;
                }
                else if (origItem is BaseClothing clothing && newitem is BaseClothing clothing1)
                {
					if (clothing.PlayerConstructed)
                    {
						clothing1.PlayerConstructed = true;
						clothing1.Crafter = clothing.Crafter;
						clothing1.Quality = clothing.Quality;
                    }
					clothing1.Altered = true;
                }
                else if (origItem is BaseClothing clothing2 && newitem is BaseArmor armor2)
                {
					if (clothing2.PlayerConstructed)
                    {
                        int qual = (int)clothing2.Quality;
						armor2.PlayerConstructed = true;
						armor2.Crafter = clothing2.Crafter;
						armor2.Quality = (ItemQuality)qual;
                    }
					armor2.Altered = true;
                }
                else if (origItem is BaseQuiver && newitem is BaseArmor armor3)
                {
                    /*BaseQuiver oldquiver = (BaseQuiver)origItem;
                    BaseArmor newarmor = (BaseArmor)newitem;*/

                    armor3.Altered = true;
                }
                else
                {
                    return;
                }

                if (origItem.Name != null)
                {
                    newitem.Name = origItem.Name;
                }

				AlterResists(newitem, origItem);

                newitem.Hue = origItem.Hue;
                newitem.LootType = origItem.LootType;
                newitem.Insured = origItem.Insured;

                origItem.OnAfterDuped(newitem);
                newitem.Parent = null;

                if (origItem is IDurability durability && newitem is IDurability durability1)
                {
                    durability1.MaxHitPoints = durability.MaxHitPoints;
                    durability1.HitPoints = durability.HitPoints;
                }

                if (from.Backpack == null)
                {
                    newitem.MoveToWorld(from.Location, from.Map);
                }
                else
                {
                    from.Backpack.DropItem(newitem);
                }

                newitem.InvalidateProperties();

                if (m_Contract != null)
                {
                    m_Contract.Delete();
                }

                origItem.Delete();

                //EventSink.InvokeAlterItem(new AlterItemEventArgs(from, m_Tool is Item ? (Item)m_Tool : m_Contract, origItem, newitem));

                number = 1094727; // You have altered the item.
            }

            if (m_Tool != null)
            {
                from.SendGump(new CraftGump(from, m_System, m_Tool, number));
            }
            else
            {
                from.SendLocalizedMessage(number);
            }
        }

        private static void AlterResists(Item newItem, Item oldItem)
        {
        }

        private static bool RetainsName(Item item)
        {
            if (item is BaseGlasses || item is ElvenGlasses || item.IsArtifact)
            {
                return true;
            }

            if (item is IArtifact artifact && artifact.ArtifactRarity > 0)
            {
                return true;
            }

            return (item.LabelNumber >= 1073505 && item.LabelNumber <= 1073552) || (item.LabelNumber >= 1073111 && item.LabelNumber <= 1075040);
        }


        private static readonly Type[] ArmorType =
        {
            typeof(RingmailGloves),    typeof(RingmailGlovesOfMining),
            typeof(PlateGloves),   typeof(LeatherGloves)
        };

        private static bool IsAlterable(Item item)
        {
            if (item is BaseWeapon weapon)
            {
                if (weapon.SetID != SetItem.None || !weapon.CanAlter /*|| weapon.NegativeAttributes.Antique != 0*/)
                {
                    return false;
                }

                if ((weapon.RequiredRace != null && weapon.RequiredRace == Race.Gargoyle && !weapon.IsArtifact))
                {
                    return false;
                }
            }

            if (item is BaseArmor armor)
            {
                if (armor.SetID != SetItem.None || !armor.CanAlter /*|| armor.NegativeAttributes.Antique != 0*/)
                {
                    return false;
                }

                if ((armor.RequiredRace != null && armor.RequiredRace == Race.Gargoyle && !armor.IsArtifact))
                {
                    return false;
                }

                if (ArmorType.Any(t => t == armor.GetType()) && armor.Resource > CraftResource.Iron)
                    return false;

                /*
                if (armor is RingmailGlovesOfMining && armor.Resource > CraftResource.Iron)
                {
                    return false;
                }
                */
            }

            if (item is BaseClothing cloth)
            {
                if (cloth.SetID != SetItem.None || !cloth.CanAlter /*|| cloth.NegativeAttributes.Antique != 0*/)
                {
                    return false;
                }

                if ((cloth.RequiredRace != null && cloth.RequiredRace == Race.Gargoyle && !cloth.IsArtifact))
                {
                    return false;
                }
            }

            if (item is BaseQuiver quiver)
            {
                if (quiver.SetID != SetItem.None || !quiver.CanAlter)
                {
                    return false;
                }
            }

            if (item is IRewardItem)
            {
                return false;
            }

            return true;
        }
    }
}
