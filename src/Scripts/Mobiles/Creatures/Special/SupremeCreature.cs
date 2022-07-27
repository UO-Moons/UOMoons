using Server.Items;
using System;

namespace Server.Mobiles
{
    public class SupremeCreature
    {
        public static readonly Map[] Maps = new Map[]            // Maps that SupremeCreatures will spawn on
		{
            Map.Ilshenar,
            Map.Felucca,
            //Map.Trammel,
            Map.Malas,
            Map.Tokuno
        };

        public static readonly Type[] Artifacts = new Type[]
        {
                typeof(GoldBricks),
                typeof(PhillipsWoodenSteed),
                typeof(AlchemistsBauble),
                typeof(ArcticDeathDealer),
                typeof(BlazeOfDeath),
                typeof(BowOfTheJukaKing),
                typeof(BurglarsBandana),
                typeof(CavortingClub),
                typeof(EnchantedTitanLegBone),
                typeof(GwennosHarp),
                typeof(IolosLute),
                typeof(LunaLance),
                typeof(NightsKiss),
                typeof(NoxRangersHeavyCrossbow),
                typeof(OrcishVisage),
                typeof(PolarBearMask),
                typeof(ShieldOfInvulnerability),
                typeof(StaffOfPower),
                typeof(VioletCourage),
                typeof(HeartOfTheLion),
                typeof(WrathOfTheDryad),
                typeof(PixieSwatter),
                typeof(GlovesOfThePugilist)
        };

        public static readonly int Hue = 2732;        // supremecreature hue

        // Buffs
        public static readonly double HitsBuff = 1000;  //added
        public static readonly double HitsBuffMult = 3; //multiplied 		
        public static readonly double StrBuff = 1000; //added
        public static readonly double IntBuff = 5;  //multiplied
        public static readonly double ManaBuff = 500000; //added
        public static readonly double DexBuff = 10000; //added
        public static readonly double SkillsBuff = 1.30;
        public static readonly double FameBuff = 1.90;
        public static readonly double KarmaBuff = 1.90;
        public static readonly int DamageBuff = 7;


        public static void MaxOutSkill(Mobile bc, SkillName skill)
        {
            bc.Skills[skill].Base = bc.Skills[skill].Cap;
        }


        public static void Convert(BaseCreature bc)
        {
            if (bc.IsSupreme || !bc.CanBeSupreme)
                return;

            bc.Hue = Hue;

            if (bc.HitsMaxSeed >= 0)
                bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * HitsBuffMult + HitsBuff);

            bc.ManaMaxSeed += (int)ManaBuff;

            bc.RawStr = (int)(bc.RawStr + StrBuff);
            bc.RawInt = (int)(bc.RawInt * IntBuff);
            bc.RawDex = (int)(bc.RawDex + DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;


            MaxOutSkill(bc, SkillName.Magery);
            MaxOutSkill(bc, SkillName.Spellweaving);
            MaxOutSkill(bc, SkillName.Healing);
            MaxOutSkill(bc, SkillName.Anatomy);
            MaxOutSkill(bc, SkillName.Meditation);
            MaxOutSkill(bc, SkillName.Wrestling);
            MaxOutSkill(bc, SkillName.Tactics);
            MaxOutSkill(bc, SkillName.EvalInt);
            MaxOutSkill(bc, SkillName.Alchemy);


            bc.Ai = AIType.SuperAI;
            bc.FightMode = FightMode.Weakest;

            bc.DamageMin += DamageBuff;
            bc.DamageMax += DamageBuff;

            if (bc.Fame > 0)
                bc.Fame = (int)(bc.Fame * FameBuff);

            if (bc.Fame > 32000)
                bc.Fame = 32000;


            if (bc.Karma != 0)
            {
                bc.Karma = (int)(bc.Karma * KarmaBuff);

                if (Math.Abs(bc.Karma) > 32000)
                    bc.Karma = 32000 * Math.Sign(bc.Karma);
            }
        }

        public static void UnConvert(BaseCreature bc)
        {
            if (!bc.IsSupreme)
                return;

            bc.Hue = 0;

            if (bc.HitsMaxSeed >= 0)
                bc.HitsMaxSeed = (int)((bc.HitsMaxSeed - HitsBuff) / HitsBuffMult);

            bc.ManaMaxSeed = (int)(bc.ManaMaxSeed - ManaBuff);


            bc.RawStr = (int)(bc.RawStr - StrBuff);
            bc.RawInt = (int)(bc.RawInt / IntBuff);
            bc.RawDex = (int)(bc.RawDex - DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;

            //we let skills slip by, they stay the same (no way to really know what they previously were)

            bc.DamageMin -= DamageBuff;
            bc.DamageMax -= DamageBuff;

            if (bc.Fame > 0)
                bc.Fame = (int)(bc.Fame / FameBuff);
            if (bc.Karma != 0)
                bc.Karma = (int)(bc.Karma / KarmaBuff);
        }

        public static bool CheckConvert(BaseCreature bc)
        {
            return CheckConvert(bc, bc.Location, bc.Map);
        }

        public static bool CheckConvert(BaseCreature bc, Point3D location, Map m)
        {
            if (!Core.AOS)
                return false;

            if (Array.IndexOf(Maps, m) == -1)
                return false;

            if (bc is BaseChampion || bc is Harrower || bc is BaseVendor || bc is BaseEscortable || bc is MirrorImageClone || bc.IsSupreme || bc.IsBlackRock)
                return false;

            int fame = bc.Fame;

            if (fame > 32000)
                fame = 32000;

            double chance = 1 / Math.Round(20.0 - (fame / 3200));

            return chance > Utility.RandomDouble();
        }

        public static bool CheckArtifactChance(Mobile m, BaseCreature bc)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (!m.Alive)
            {
                return false;
            }

            double fame = bc.Fame;

            if (fame > 32000)
            {
                fame = 32000;
            }

            double chance = 1 / (Math.Max(10, 100 * (0.83 - Math.Round(Math.Log(Math.Round(fame / 6000, 3) + 0.001, 10), 3))) * (100 - Math.Sqrt(m.Luck)) / 100.0);

            return chance > Utility.RandomDouble();
        }

        public static void GiveArtifactTo(Mobile m)
        {
            Item item = (Item)Activator.CreateInstance(Artifacts[Utility.Random(Artifacts.Length)]);

            if (m.AddToBackpack(item))
                m.SendMessage("As a reward for slaying the mighty paragon, an artifact has been placed in your backpack.");
            else
                m.SendMessage("As your backpack is full, your reward for destroying the legendary paragon has been placed at your feet.");
        }
    }
}
