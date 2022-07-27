using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.SkillHandlers
{
    public class ImbuingDefinition
    {
	    private object Attribute { get; }

	    private int AttributeName { get; }

	    private int Weight { get; }

	    private Type PrimaryRes { get; }

	    private Type GemRes { get; }

	    private Type SpecialRes { get; }

	    private int PrimaryName { get; }

	    private int GemName { get; }

	    private int SpecialName { get; }

	    private int MaxIntensity { get; }

	    private int IncAmount { get; }

	    private int Description { get; }

	    private bool Melee { get; }
	    private bool Ranged { get; }
	    private bool Armor { get; }
	    private bool Shield { get; }
	    private bool Jewels { get; }

        public ImbuingDefinition(object attribute, int attributeName, int weight, Type pRes, Type gRes, Type spRes, int mInt, int inc, int desc, bool melee = false, bool ranged = false, bool armor = false, bool shield = false, bool jewels = false)
        {
            Attribute = attribute;
            AttributeName = attributeName;
            Weight = weight;
            PrimaryRes = pRes;
            GemRes = gRes;
            SpecialRes = spRes;

            PrimaryName = GetLocalization(pRes);
            GemName = GetLocalization(gRes);
            SpecialName = GetLocalization(spRes);

            MaxIntensity = mInt;
            IncAmount = inc;
            Description = desc;

            Melee = melee;
            Ranged = ranged;
            Armor = armor;
            Shield = shield;
            Jewels = jewels;
        }

        private int GetLocalization(Type type)
        {
            if (type == null)
                return 0;

            if (type == typeof(Tourmaline)) return 1023864;
            if (type == typeof(Ruby)) return 1023859;
            if (type == typeof(Diamond)) return 1023878;
            if (type == typeof(Sapphire)) return 1023857;
            if (type == typeof(Citrine)) return 1023861;
            if (type == typeof(Emerald)) return 1023856;
            if (type == typeof(StarSapphire)) return 1023855;
            if (type == typeof(Amethyst)) return 1023862;

            if (type == typeof(RelicFragment)) return 1031699;
            if (type == typeof(EnchantedEssence)) return 1031698;
            if (type == typeof(MagicalResidue)) return 1031697;

            if (type == typeof(DarkSapphire)) return 1032690;
            if (type == typeof(Turquoise)) return 1032691;
            if (type == typeof(PerfectEmerald)) return 1032692;
            if (type == typeof(EcruCitrine)) return 1032693;
            if (type == typeof(WhitePearl)) return 1032694;
            if (type == typeof(FireRuby)) return 1032695;
            if (type == typeof(BlueDiamond)) return 1032696;
            if (type == typeof(BrilliantAmber)) return 1032697;

            if (type == typeof(ParasiticPlant)) return 1032688;
            if (type == typeof(LuminescentFungi)) return 1032689;

            LocBuffer ??= new Dictionary<Type, int>();

            if (LocBuffer.ContainsKey(type))
                return LocBuffer[type];

            var item = Loot.Construct(type);

            if (item != null)
            {
                LocBuffer[type] = item.LabelNumber;
                item.Delete();

                return LocBuffer[type]; ;
            }

            Console.WriteLine("Warning, missing name cliloc for type {0}.", type.Name);
            return -1;
        }

        private Dictionary<Type, int> LocBuffer { get; set; }
    }
}
