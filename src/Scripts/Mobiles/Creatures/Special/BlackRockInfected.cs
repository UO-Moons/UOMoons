using System;
using Server.Items;

namespace Server.Mobiles
{
    public class BlackRockInfected
    {
        public static readonly double InfectedChestChance = .10;

        public static readonly Map[] Maps = new Map[]            // Maps that Blackrock Infected will spawn on
		{
            Map.Felucca,
            Map.Trammel
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

        private class BlackRockStamRegen : Timer
        {
            private readonly BaseCreature m_Owner;

            public BlackRockStamRegen(Mobile m)
                : base(TimeSpan.FromSeconds(.5), TimeSpan.FromSeconds(2))
            {
                Priority = TimerPriority.FiftyMs;
                m_Owner = m as BaseCreature;
            }

            protected override void OnTick()
            {
                if (!m_Owner.Deleted && m_Owner.IsBlackRock && m_Owner.Map != Map.Internal)
                {
                    m_Owner.Stam++;

                    Delay = Interval = (m_Owner.Stam < (m_Owner.StamMax * .75)) ? TimeSpan.FromSeconds(.5) : TimeSpan.FromSeconds(2);
                }
                else
                {
                    Stop();
                }
            }
        }

        // Buffs
        public static readonly double HitsBuff = 1.0;
        public static readonly double StrBuff = 2.05;
        public static readonly double IntBuff = 2.20;
        public static readonly double DexBuff = 2.20;
        public static readonly double SkillsBuff = 2.20;
        public static readonly double SpeedBuff = 2.20;
        public static readonly double FameBuff = 2.40;
        public static readonly double KarmaBuff = 2.40;
        public static readonly int DamageBuff = 10;

        public static void Convert(BaseCreature bc)
        {
            if (bc.IsBlackRock || !bc.CanBeBlackRock)
            {
                return;
            }

            bc.Hue = 1175;

            if (bc.HitsMaxSeed >= 0)
            {
                bc.HitsMaxSeed = (int)(bc.HitsMaxSeed * HitsBuff);
            }

            bc.RawStr = (int)(bc.RawStr * StrBuff);
            bc.RawInt = (int)(bc.RawInt * IntBuff);
            bc.RawDex = (int)(bc.RawDex * DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;

            for (int i = 0; i < bc.Skills.Length; i++)
            {
                Skill skill = (Skill)bc.Skills[i];

                if (skill.Base > 0.0)
                {
                    skill.Base *= SkillsBuff;
                }
            }

            bc.PassiveSpeed /= SpeedBuff;
            bc.ActiveSpeed /= SpeedBuff;
            bc.CurrentSpeed = bc.PassiveSpeed;

            bc.DamageMin += DamageBuff;
            bc.DamageMax += DamageBuff;

            if (bc.Fame > 0)
            {
                bc.Fame = (int)(bc.Fame * FameBuff);
            }

            if (bc.Fame > 32000)
            {
                bc.Fame = 32000;
            }

            // TODO: Mana regeneration rate = Sqrt( buffedFame ) / 4

            if (bc.Karma != 0)
            {
                bc.Karma = (int)(bc.Karma * KarmaBuff);

                if (Math.Abs(bc.Karma) > 32000)
                {
                    bc.Karma = 32000 * Math.Sign(bc.Karma);
                }
            }

            new BlackRockStamRegen(bc).Start();
        }

        public static void UnConvert(BaseCreature bc)
        {
            if (!bc.IsBlackRock)
            {
                return;
            }

            bc.Hue = 0;

            if (bc.HitsMaxSeed >= 0)
            {
                bc.HitsMaxSeed = (int)(bc.HitsMaxSeed / HitsBuff);
            }

            bc.RawStr = (int)(bc.RawStr / StrBuff);
            bc.RawInt = (int)(bc.RawInt / IntBuff);
            bc.RawDex = (int)(bc.RawDex / DexBuff);

            bc.Hits = bc.HitsMax;
            bc.Mana = bc.ManaMax;
            bc.Stam = bc.StamMax;

            for (int i = 0; i < bc.Skills.Length; i++)
            {
                Skill skill = (Skill)bc.Skills[i];

                if (skill.Base > 0.0)
                {
                    skill.Base /= SkillsBuff;
                }
            }

            bc.PassiveSpeed *= SpeedBuff;
            bc.ActiveSpeed *= SpeedBuff;

            bc.DamageMin -= DamageBuff;
            bc.DamageMax -= DamageBuff;

            if (bc.Fame > 0)
            {
                bc.Fame = (int)(bc.Fame / FameBuff);
            }

            if (bc.Karma != 0)
            {
                bc.Karma = (int)(bc.Karma / KarmaBuff);
            }
        }

        public static bool CheckConvert(BaseCreature bc)
        {
            return CheckConvert(bc, bc.Location, bc.Map);
        }

        public static bool CheckConvert(BaseCreature bc, Point3D location, Map m)
        {
            if (!Core.AOS)
            {
                return false;
            }

            if (Array.IndexOf(Maps, m) == -1)
            {
                return false;
            }

            if (bc is BaseChampion || bc is Harrower || bc is BaseVendor || bc is BaseEscortable || bc.Summoned || bc.Controlled || bc is Clone || bc.IsBlackRock)
            {
                return false;
            }

            if (bc is DreadHorn || bc is MonstrousInterredGrizzle || bc is Travesty || bc is ChiefParoxysmus || bc is LadyMelisande || bc is ShimmeringEffusion || bc.IsParagon)
            {
                return false;
            }

            int fame = bc.Fame;

            if (fame > 32000)
            {
                fame = 32000;
            }

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
