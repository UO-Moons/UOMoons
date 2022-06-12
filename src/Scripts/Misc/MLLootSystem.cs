using Server.Engines.Plants;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
    public class MLLootSystem
	{
        private static Mobile killer;
        private static Mobile victim;

        private static PlayerMobile pm;
        private static BaseCreature bc;

        public static int ComputeAmount(int amt)
        {
            if (victim.Map == Map.Felucca)
            {
                amt *= 4;
            }

            return amt;
        }

        public static int ComputeAmount()
        {
            return ComputeAmount(1);
        }

        public static void HandleKill(Mobile pvictim, Mobile pkiller)
        {
            if (pvictim == null || pkiller == null)
            {
                return;
            }

            victim = pvictim;
            killer = pkiller;

            if (killer is BaseCreature)
            {
                BaseCreature killerbc = killer as BaseCreature;
                if (killerbc.ControlMaster != null)
                {
                    killer = killerbc.ControlMaster;
                }
                else if (killerbc.SummonMaster != null)
                {
                    killer = killerbc.SummonMaster;
                }
            }

            pm = killer as PlayerMobile;
            bc = victim as BaseCreature;

            if (pm == null || bc == null || !killer.InRange(victim, 100))
            {
                return;
            }

            if (bc.Controlled || bc.Owners.Count > 0 || victim.Map != killer.Map)
            {
                return;
            }

            double luckfactor = 1;
            if (killer != null && killer is PlayerMobile)
            {
                luckfactor = 1 + (double)killer.Luck / 500;
            }

            if (luckfactor > 3)
            {
                luckfactor = 3;  //max luck of about 1500
            }

            int Fame = bc.Fame;
            if (Fame > 25000)
            {
                Fame = 25000;
            }

            double famefactor = 1 + ((double)Fame / 6000);


			//START DROPS:
			//Arcanist Scroll
			if ((0.02 * luckfactor * famefactor) > Utility.RandomDouble() && (bc is MougGuur || bc is Szavetra || bc is MLDryad || bc is Satyr || bc is Swoop))
            {
				for (int i = 0; i < Utility.RandomMinMax(0, 1); i++)
				{
					bc.PackItem(Loot.RandomScroll(0, Loot.ArcanistScrollTypes.Length, SpellbookType.Arcanist));
				}
			}

			if ((Utility.RandomDouble() < .60) && (bc is MLDryad))
			{
				bc.PackItem(Seed.RandomPeculiarSeed(1));
			}

			if ((Utility.RandomDouble() < 0.1) && (bc is Troglodyte))
			{
				bc.PackItem(new PrimitiveFetish());
			}

			//Protectors Essence
			if ((Utility.RandomDouble() < 0.4) && (bc is Protector))
			{
				bc.PackItem(new ProtectorsEssence(ComputeAmount(1)));
			}
			//ParrotItem
			if ((Utility.RandomDouble() < 0.1) && (bc is UnfrozenMummy || bc is Putrefier || bc is EnslavedSatyr || bc is InsaneDryad || bc is Saliva || bc is LadyJennifyr || bc is LadyMarai
				|| bc is MasterJonath || bc is MasterMikael || bc is LadyLissith || bc is LadySabrix || bc is Malefic || bc is Virulent))
			{
				bc.PackItem(new ParrotItem(ComputeAmount(1)));
			}
			if (bc is Saliva)
			{
				bc.PackItem(new SalivasFeather(ComputeAmount(1)));
			}

			if ((Utility.RandomDouble() < 0.4) && (bc is Tangle))
			{
				bc.PackItem(new TaintedSeeds(ComputeAmount(1)));
			}

			//Broken Crystals
			if ((Utility.RandomDouble() < 0.6) && (bc is UnfrozenMummy))
			{
				bc.PackItem(new BrokenCrystals(ComputeAmount(1)));
			}
			//Crystalline Fragments
			if ((Utility.RandomDouble() < 0.75) && (bc is CrystalVortex || bc is CrystalLatticeSeeker))
			{
				bc.PackItem(new CrystallineFragments(ComputeAmount(1)));
			}
			//Jagged Crystals
			if ((Utility.RandomDouble() < 0.6) && (bc is CrystalVortex))
			{
				bc.PackItem(new JaggedCrystals(ComputeAmount(1)));
			}
			//Crushed Crystals
			if ((Utility.RandomDouble() < 0.05) && (bc is CrystalSeaSerpent))
			{
				bc.PackItem(new CrushedCrystals(ComputeAmount(1)));
			}
			//Pieces Of Crystal
			if ((Utility.RandomDouble() < 0.07) && (bc is CrystalLatticeSeeker))
			{
				bc.PackItem(new PiecesOfCrystal(ComputeAmount(1)));
			}
			//Scattered Crystals
			if ((Utility.RandomDouble() < 0.4) && (bc is CrystalDaemon))
			{
				bc.PackItem(new ScatteredCrystals(ComputeAmount(1)));
			}
			//Icy Heart
			if ((Utility.RandomDouble() < 0.1) && (bc is CrystalSeaSerpent))
			{
				bc.PackItem(new IcyHeart(ComputeAmount(1)));
			}
			//Lucky Dagger
			if ((Utility.RandomDouble() < 0.1) && (bc is CrystalSeaSerpent))
			{
				bc.PackItem(new LuckyDagger(ComputeAmount(1)));
			}

			if ((Utility.RandomDouble() < 0.15) && (bc is LadyJennifyr || bc is LadyMarai || bc is MasterJonath || bc is MasterMikael || bc is SirPatrick))
			{
				bc.PackItem(new DisintegratingThesisNotes(ComputeAmount(1)));
			}

			if ((Utility.RandomDouble() < 0.05) && (bc is SirPatrick))
			{
				bc.PackItem(new AssassinChest());
			}

			if ((Utility.RandomDouble() < 0.02) && (bc is LadySabrix))
			{
				bc.PackItem(new SabrixsEye());
			}

			if ((Utility.RandomDouble() < 0.25) && (bc is LadySabrix))
			{
				switch (Utility.Random(2))
				{
					case 0: bc.PackItem(new PaladinArms()); break;
					case 1: bc.PackItem(new HunterLegs()); break;
				}
			}

			//Spleen Of The Putrefier
			if (bc is Putrefier)
			{
				bc.PackItem(new SpleenOfThePutrefier());
			}

			if (bc is Gnaw)
			{
				bc.PackItem(new GnawsFang());
			}

			if ((Utility.RandomDouble() < 0.25) && (bc is Irk))
			{
				bc.PackItem(new IrksBrain());
			}

			if ((Utility.RandomDouble() < 0.025) && (bc is Irk))
			{
				bc.PackItem(new PaladinGloves());
			}

			if ((Utility.RandomDouble() < 0.025) && (bc is LadyLissith))
			{
				bc.PackItem(new GreymistChest());
			}

			if ((Utility.RandomDouble() < 0.45) && (bc is LadyLissith))
			{
				bc.PackItem(new LissithsSilk());
			}

			if (bc is Grobu)
			{
				bc.PackItem(new GrobusFur());
			}

			if (bc is RedDeath)
			{
				bc.PackItem(new ResolvesBridle());
			}

			if (bc is Abscess)
			{
				bc.PackItem(new AbscessTail());
			}

			if (bc is Thrasher)
			{
				bc.PackItem(new ThrashersTail());
			}

			if (bc is Coil)
			{
				bc.PackItem(new CoilsFang());
			}

			if ((Utility.RandomDouble() < 0.4) && (bc is Tangle))
			{
				bc.PackItem(new TaintedSeeds(ComputeAmount(1)));
			}

			if (Utility.RandomDouble() < 0.025 && bc is Swoop)
			{
				switch (Utility.Random(18))
				{
					case 0: bc.PackItem(new AssassinChest()); break;
					case 1: bc.PackItem(new AssassinArms()); break;
					case 2: bc.PackItem(new DeathChest()); break;
					case 3: bc.PackItem(new MyrmidonArms()); break;
					case 4: bc.PackItem(new MyrmidonLegs()); break;
					case 5: bc.PackItem(new MyrmidonGorget()); break;
					case 6: bc.PackItem(new LeafweaveGloves()); break;
					case 7: bc.PackItem(new LeafweaveLegs()); break;
					case 8: bc.PackItem(new LeafweavePauldrons()); break;
					case 9: bc.PackItem(new PaladinGloves()); break;
					case 10: bc.PackItem(new PaladinGorget()); break;
					case 11: bc.PackItem(new PaladinArms()); break;
					case 12: bc.PackItem(new HunterArms()); break;
					case 13: bc.PackItem(new HunterGloves()); break;
					case 14: bc.PackItem(new HunterLegs()); break;
					case 15: bc.PackItem(new HunterChest()); break;
					case 16: bc.PackItem(new GreymistArms()); break;
					case 17: bc.PackItem(new GreymistGloves()); break;
				}
			}

			if (bc is Hydra)
			{
				bc.PackItem(new HydraScale());
			}

			if ((Utility.RandomDouble() < 0.025) && (bc is Virulent))
			{
				switch (Utility.Random(2))
				{
					case 0: bc.PackItem(new HunterLegs()); break;
					case 1: bc.PackItem(new MalekisHonor()); break;
				}
			}

			if (Utility.RandomDouble() < 0.025 && bc is Miasma)
			{
				switch (Utility.Random(16))
				{
					case 0: bc.PackItem(new MyrmidonGloves()); break;
					case 1: bc.PackItem(new MyrmidonGorget()); break;
					case 2: bc.PackItem(new MyrmidonLegs()); break;
					case 3: bc.PackItem(new MyrmidonArms()); break;
					case 4: bc.PackItem(new PaladinArms()); break;
					case 5: bc.PackItem(new PaladinGorget()); break;
					case 6: bc.PackItem(new LeafweaveLegs()); break;
					case 7: bc.PackItem(new DeathChest()); break;
					case 8: bc.PackItem(new DeathGloves()); break;
					case 9: bc.PackItem(new DeathLegs()); break;
					case 10: bc.PackItem(new GreymistGloves()); break;
					case 11: bc.PackItem(new GreymistArms()); break;
					case 12: bc.PackItem(new AssassinChest()); break;
					case 13: bc.PackItem(new AssassinArms()); break;
					case 14: bc.PackItem(new HunterGloves()); break;
					case 15: bc.PackItem(new HunterLegs()); break;
				}
			}

			if (killer != null && killer is PlayerMobile)
            {
                luckfactor = 1 + (double)killer.Luck / 1500;
            }

            if (luckfactor > 1)
            {
                luckfactor = 1;  //max luck of about 1500
            }

            if ((0.005 * luckfactor * famefactor) > Utility.RandomDouble() && (bc is CorrosiveSlime))
            {
                //bc.PackItem(new PotionOfRepair());
            }

            //TreasureMap System rework
            //if (bc is MeerMage)
            //bc.PackItem(new TreasureMap(5, Utility.RandomBool() ? Map.Felucca : Map.Trammel));

        }
    }
}
