//using Server.Engines.Despise;
//using Server.Engines.Shadowguard;
using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class RandomItemGenerator
    {
        public static bool Enabled => Core.SA;
        public static int FeluccaLuckBonus { get; private set; }
        public static int FeluccaBudgetBonus { get; private set; }

        private static int MaxBaseBudget { get; set; }
        private static int MinBaseBudget { get; set; }
        public static int MaxProps { get; private set; }

        public static int MaxAdjustedBudget { get; private set; }
        public static int MinAdjustedBudget { get; private set; }

        public static void Configure()
        {
            FeluccaLuckBonus = 1000;
            FeluccaBudgetBonus = 100;

            MaxBaseBudget = 700;
            MinBaseBudget = 150;
            MaxProps = 8;

            MaxAdjustedBudget = 1450;
            MinAdjustedBudget = 150;
        }

        private RandomItemGenerator()
        {
        }

        /// <summary>
        /// This is called for every item that is dropped to a loot pack.
        /// Change the conditions here to add/remove Random Item Drops with REGULAR loot.
        /// </summary>
        /// <param name="item">item to be mutated</param>
        /// <param name="killer">Mobile.LastKiller</param>
        /// <param name="victim">the victim</param>
        public static bool GenerateRandomItem(Item item, Mobile killer, BaseCreature victim)
        {
            if (Enabled)
                return RunicReforging.GenerateRandomItem(item, killer, victim);
            return false;
        }

        /// <summary>
        /// This is called in BaseRunicTool to ensure all items use the new system, as long as its not player made.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="luckChance">adjusted luck chance</param>
        /// <param name="attributeCount"></param>
        /// <param name="minIntensity"></param>
        /// <param name="maxIntensity"></param>
        public static void GenerateRandomItem(Item item, int luckChance, int attributeCount, int minIntensity, int maxIntensity)
        {
            var min = attributeCount * 2 * minIntensity;
            min += (int)(min * ((double)Utility.RandomMinMax(1, 4) / 10));

            int max = attributeCount * 2 * maxIntensity;
            max += (int)(max * ((double)Utility.RandomMinMax(1, 4) / 10));

            RunicReforging.GenerateRandomItem(item, luckChance, min, max);
        }

        /// <summary>
        /// 24000 is the normalized fame for MaxBaseBudget, ie Balron.  
        /// Called in BaseCreature.cs virtual BaseLootBudget Property
        /// </summary>
        /// <param name="bc">Creature to be evaluated</param>
        /// <returns></returns>
        private static int GetBaseBudget(BaseCreature bc)
        {
            if (bc is BaseRenowned)
                return MaxBaseBudget;

            return bc.Fame / (20500 / MaxBaseBudget);
        }

        public static int GetDifficultyFor(BaseCreature bc)
        {
            if (bc == null)
                return 0;

            var fame = bc.Fame;

            if (fame <= 0)
	            return 0;
            var budget = Math.Min(MaxBaseBudget, GetBaseBudget(bc));

            BossEntry.CheckBoss(bc, ref budget);

            return Math.Max(MinBaseBudget, budget);

        }
    }

    public class BossEntry
    {
	    private int Bonus { get; }
	    private List<Type> List { get; }

	    private BossEntry(int bonus, params Type[] list)
        {
            Bonus = bonus;
            List = list.ToList();
        }

        private static List<BossEntry> Entries { get; set; }

        public static void CheckBoss(BaseCreature bc, ref int budget)
        {
	        foreach (var entry in Entries.Where(entry => entry.List.FirstOrDefault(t => t == bc.GetType() || bc.GetType().IsSubclassOf(t)) != null))
	        {
		        budget += entry.Bonus;
		        return;
	        }
        }

        public static void Initialize()
        {
            Entries = new List<BossEntry>
            {
	            new(100, typeof(BaseRenowned), /*typeof(TRex), typeof(BaseShipCaptain),*/ typeof(Navrey)),
	            new(150, typeof(BaseChampion), typeof(Impaler), typeof(DarknightCreeper),
		            typeof(FleshRenderer),
		            typeof(ShadowKnight),
		            typeof(AbysmalHorror) /*, typeof(AdrianTheGloriousLord), typeof(AndrosTheDreadLord)*/),
	            new(250, typeof(BasePeerless), typeof(Harrower), typeof(DemonKnight)/*, typeof(ShadowguardBoss), typeof(Osiredon)*/)
            };

            //Entries.Add(
                //new BossEntry(350, typeof(ClockworkExodus), typeof(CoraTheSorceress), typeof(Charydbis), typeof(Zipactriotl), typeof(MyrmidexQueen)));
        }
    }
}
