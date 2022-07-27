using Server.Mobiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items
{
    public class IngredientDropEntry
    {
	    private Type CreatureType { get; }

	    private bool DropMultiples { get; }

	    private string Region { get; }

	    private double Chance { get; }

	    private Type[] Ingredients { get; }

        public IngredientDropEntry(Type creature, bool dropMultiples, double chance, params Type[] ingredients)
            : this(creature, dropMultiples, null, chance, ingredients)
        {
        }

        public IngredientDropEntry(Type creature, bool dropMultiples, string region, double chance, params Type[] ingredients)
        {
            CreatureType = creature;
            Ingredients = ingredients;
            DropMultiples = dropMultiples;
            Region = region;
            Chance = chance;
        }

        private static List<IngredientDropEntry> IngredientTable { get; set; }

        public static void Initialize()
        {
            EventSink.OnCreatureDeath += OnCreatureDeath;

            IngredientTable = new List<IngredientDropEntry>
            {
	            // Imbuing Gems
	            new(typeof(AncientLichRenowned), true, 0.50, ImbuingGems),
	            new(typeof(DevourerRenowned), true, 0.50, ImbuingGems),
	            new(typeof(FireElementalRenowned), true, 0.50, ImbuingGems),
	            new(typeof(GrayGoblinMageRenowned), true, 0.50, ImbuingGems),
	            new(typeof(GreenGoblinAlchemistRenowned), true, 0.5, ImbuingGems),
	            new(typeof(PixieRenowned), true, 0.50, ImbuingGems),
	            new(typeof(RakktaviRenowned), true, 0.50, ImbuingGems),
	            new(typeof(SkeletalDragonRenowned), true, 0.50, ImbuingGems),
	            new(typeof(TikitaviRenowned), true, 0.50, ImbuingGems),
	            new(typeof(VitaviRenowned), true, 0.50, ImbuingGems),
	            //Bottle of Ichor/Spider Carapace
	            new(typeof(TrapdoorSpider), true, 0.05, typeof(SpiderCarapace)),
	            new(typeof(WolfSpider), true, 0.15, typeof(BottleIchor)),
	            new(typeof(SentinelSpider), true, 0.15, typeof(BottleIchor)),
	            new(typeof(Navrey), true, 0.50, typeof(BottleIchor), typeof(SpiderCarapace)),
	            //Reflective wolf eye
	            new(typeof(ClanSSW), true, 0.20, typeof(ReflectiveWolfEye)),
	            new(typeof(LeatherWolf), true, 0.20, typeof(ReflectiveWolfEye)),
	            //Faery Dust - drop from silver sapling mini champ
	            new(typeof(FairyDragon), true, "Abyss", 0.25, typeof(FaeryDust)),
	            new(typeof(Pixie), true, "Abyss", 0.25, typeof(FaeryDust)),
	            new(typeof(SAPixie), true, "Abyss", 0.25, typeof(FaeryDust)),
	            new(typeof(Wisp), true, "Abyss", 0.25, typeof(FaeryDust)),
	            new(typeof(DarkWisp), true, "Abyss", 0.25, typeof(FaeryDust)),
	            new(typeof(FairyDragon), true, "Abyss", 0.25, typeof(FeyWings)),
	            //Boura Pelt
	            new(typeof(RuddyBoura), true, 0.05, typeof(BouraPelt)),
	            new(typeof(LowlandBoura), true, 0.05, typeof(BouraPelt)),
	            new(typeof(HighPlainsBoura), true, 1.00, typeof(BouraPelt)),
	            //Silver snake skin
	            new(typeof(SilverSerpent), true, "TerMur", 0.10, typeof(SilverSnakeSkin)),
	            //Harpsichord Roll / Not an ingredient
	            new(typeof(BaseCreature), true, "TerMur", 0.01, typeof(HarpsichordRoll)),
	            //Void Orb/Vial of Vitriol
	            new(typeof(BaseVoidCreature), true, 0.05, typeof(VoidOrb)),
	            new(typeof(UnboundEnergyVortex), true, 0.25, typeof(VoidOrb), typeof(VialOfVitriol)),
	            new(typeof(AcidSlug), true, 0.10, typeof(VialOfVitriol)),
	            //Slith Tongue
	            new(typeof(Slith), true, 0.05, typeof(SlithTongue)),
	            new(typeof(StoneSlith), true, 0.05, typeof(SlithTongue)),
	            new(typeof(ToxicSlith), true, 0.05, typeof(SlithTongue)),
	            //Raptor Teeth
	            new(typeof(Raptor), true, 0.05, typeof(RaptorTeeth)),
	            //Daemon Claw
	            new(typeof(FireDaemon), true, 0.60, typeof(DaemonClaw)),
	            new(typeof(FireDaemonRenowned), true, 1.00, typeof(DaemonClaw)),
	            //Goblin Blood
	            new(typeof(GreenGoblin), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(GreenGoblinAlchemist), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(GreenGoblinScout), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(GrayGoblin), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(GrayGoblinKeeper), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(GrayGoblinMage), true, 0.10, typeof(GoblinBlood)),
	            new(typeof(EnslavedGoblinKeeper), true, 0.25, typeof(GoblinBlood)),
	            new(typeof(EnslavedGoblinMage), true, 0.25, typeof(GoblinBlood)),
	            new(typeof(EnslavedGoblinScout), true, 0.25, typeof(GoblinBlood)),
	            new(typeof(EnslavedGrayGoblin), true, 0.25, typeof(GoblinBlood)),
	            new(typeof(EnslavedGreenGoblin), true, 0.25, typeof(GoblinBlood)),
	            new(typeof(EnslavedGreenGoblinAlchemist), true, 0.25, typeof(GoblinBlood)),
	            //Lava Serpent Crust
	            new(typeof(LavaElemental), true, 0.25, typeof(LavaSerpentCrust)),
	            new(typeof(FireElementalRenowned), true, 1.00, typeof(LavaSerpentCrust)),
	            //Undying Flesh
	            new(typeof(UndeadGuardian), true, 0.10, typeof(UndyingFlesh)),
	            new(typeof(Niporailem), true, 1.0, typeof(UndyingFlesh)),
	            new(typeof(ChaosVortex), true, 0.25, typeof(UndyingFlesh)),
	            //Crystaline Blackrock
	            new(typeof(AgapiteElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(BronzeElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(CopperElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(GoldenElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(ShadowIronElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(ValoriteElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(VeriteElemental), true, 0.25, typeof(CrystallineBlackrock)),
	            new(typeof(ChaosVortex), true, 0.25, typeof(ChagaMushroom)),
	            new(typeof(BaseCreature), false, "Cavern of the Discarded", 0.05, typeof(DelicateScales),
		            typeof(ArcanicRuneStone), typeof(PowderedIron), typeof(EssenceBalance), typeof(CrushedGlass), typeof(CrystallineBlackrock),
		            typeof(ElvenFletching), typeof(CrystalShards), typeof(Lodestone), typeof(AbyssalCloth), typeof(SeedOfRenewal)),
	            new(typeof(BaseCreature), false, "Passage of Tears", 0.05, typeof(EssenceSingularity)),
	            new(typeof(BaseCreature), false, "Fairy Dragon Lair", 0.05, typeof(EssenceDiligence)),
	            new(typeof(BaseCreature), false, "Abyssal Lair", 0.05, typeof(EssenceAchievement)),
	            new(typeof(BaseCreature), false, "Crimson Veins", 0.05, typeof(EssencePrecision)),
	            new(typeof(BaseCreature), false, "Lava Caldera", 0.05, typeof(EssencePassion)),
	            new(typeof(BaseCreature), false, "Fire Temple Ruins", 0.05, typeof(EssenceOrder)),
	            new(typeof(BaseCreature), false, "Enslaved Goblins", 0.05, typeof(GoblinBlood), typeof(EssenceControl)),
	            new(typeof(BaseCreature), false, "Lands of the Lich", 0.05, typeof(EssenceDirection)),
	            new(typeof(BaseCreature), false, "Secret Garden", 0.05, typeof(EssenceFeeling)),
	            new(typeof(BaseCreature), false, "Skeletal Dragon", 0.05, typeof(EssencePersistence))
            };
        }

        private static void OnCreatureDeath(CreatureDeathEventArgs e)
        {
            BaseCreature bc = e.Creature as BaseCreature;
            Container c = e.Corpse;

            if (bc != null && c is { Deleted: false } && !bc.Controlled && !bc.Summoned)
            {
                CheckDrop(bc, c);
            }

            if (e.Killer is BaseVoidCreature creature)
            {
                creature.Mutate(VoidEvolution.Killing);
            }
        }

        private static void CheckDrop(BaseCreature bc, Container c)
        {
            if (IngredientTable != null)
            {
	            foreach (var entry in IngredientTable.Where(entry => entry != null))
	            {
		            if (entry.Region != null)
		            {
			            string reg = entry.Region;

			            switch (reg)
			            {
				            case "TerMur" when c.Map != Map.TerMur:
				            case "Abyss" when (c.Map != Map.TerMur || c.X < 235 || c.X > 1155 || c.Y < 40 || c.Y > 1040):
					            continue;
			            }

			            if (reg != "TerMur" && reg != "Abyss")
			            {
				            Region r = Server.Region.Find(c.Location, c.Map);

				            if (r == null || !r.IsPartOf(entry.Region))
					            continue;
			            }
		            }

		            if (bc.GetType() != entry.CreatureType && !bc.GetType().IsSubclassOf(entry.CreatureType))
		            {
			            continue;
		            }

		            double toBeat = entry.Chance;
		            List<Item> drops = new();

		            if (bc is BaseVoidCreature creature)
		            {
			            toBeat *= creature.Stage + 1;
		            }

		            if (entry.DropMultiples)
		            {
			            drops.AddRange(from type in entry.Ingredients where toBeat >= Utility.RandomDouble() select Loot.Construct(type) into drop where drop != null select drop);
		            }
		            else if (toBeat >= Utility.RandomDouble())
		            {
			            Item drop = Loot.Construct(entry.Ingredients);

			            if (drop != null)
				            drops.Add(drop);
		            }

		            foreach (Item item in drops)
		            {
			            c.DropItem(item);
		            }

		            ColUtility.Free(drops);
	            }
            }
        }

        public static readonly Type[] ImbuingGems =
        {
            typeof(FireRuby),
            typeof(WhitePearl),
            typeof(BlueDiamond),
            typeof(Turquoise)
        };
    }
}
