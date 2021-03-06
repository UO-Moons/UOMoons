using Server.Items;
using Server.Network;
using Server.Spells;
using System;
using System.Collections;

namespace Server.Mobiles
{
	[CorpseName("a fan dancer corpse")]
	public class FanDancer : BaseCreature
	{
		public static int AbilityRange
		{
			get { return 10; }
		}

		private static int m_MinTime = 6;
		private static int m_MaxTime = 12;

		private DateTime m_NextAbilityTime;

		private ExpireTimer m_Timer;

		[Constructable]
		public FanDancer() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
		{
			Name = "a fan dancer";
			Body = 247;
			BaseSoundID = 0x372;

			SetStr(301, 375);
			SetDex(201, 255);
			SetInt(21, 25);

			SetHits(351, 430);

			SetDamage(12, 17);

			SetDamageType(ResistanceType.Physical, 70);
			SetDamageType(ResistanceType.Fire, 10);
			SetDamageType(ResistanceType.Cold, 10);
			SetDamageType(ResistanceType.Poison, 10);

			SetResistance(ResistanceType.Physical, 40, 60);
			SetResistance(ResistanceType.Fire, 50, 70);
			SetResistance(ResistanceType.Cold, 50, 70);
			SetResistance(ResistanceType.Poison, 50, 70);
			SetResistance(ResistanceType.Energy, 40, 60);

			SetSkill(SkillName.MagicResist, 100.1, 110.0);
			SetSkill(SkillName.Tactics, 85.1, 95.0);
			SetSkill(SkillName.Wrestling, 85.1, 95.0);
			SetSkill(SkillName.Anatomy, 85.1, 95.0);

			Fame = 9000;
			Karma = -9000;

			if (Utility.RandomDouble() < .33)
				PackItem(Engines.Plants.Seed.RandomBonsaiSeed());

			AddItem(new Tessen());

			if (0.02 >= Utility.RandomDouble())
				PackItem(new OrigamiPaper());
		}


		public override void GenerateLoot()
		{
			AddLoot(LootPack.FilthyRich);
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.Gems, 2);
		}

		public override bool Uncalmable => true;

		/* TODO: Repel Magic
		 * 10% chance of repelling a melee attack (why did they call it repel magic anyway?)
		 * Cliloc: 1070844
		 * Effect: damage is dealt to the attacker, no damage is taken by the fan dancer
		 */

		//public override void OnDamagedBySpell(Mobile attacker, Spell spell, int damage)
		//{
			//base.OnDamagedBySpell(attacker, spell, damage);

			//if (0.8 > Utility.RandomDouble() && !attacker.InRange(this, 1))
			//{
				/* Fan Throw
				 * Effect: - To: "0x57D4F5B" - ItemId: "0x27A3" - ItemIdName: "Tessen" - FromLocation: "(992 299, 24)" - ToLocation: "(992 308, 22)" - Speed: "10" - Duration: "0" - FixedDirection: "False" - Explode: "False" - Hue: "0x0" - Render: "0x0"
				 * Damage: 50-65
				 */
				//Effects.SendPacket(attacker, attacker.Map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, 0x27A3, Location, attacker.Location, 10, 0, false, false, 0, 0));
				//AOS.Damage(attacker, this, Utility.RandomMinMax(50, 65), 100, 0, 0, 0, 0);
			//}
		//}

		//public override void OnGotMeleeAttack(Mobile attacker)
		//{
			//base.OnGotMeleeAttack(attacker);

			//if (0.8 > Utility.RandomDouble() && !attacker.InRange(this, 1))
			//{
				/* Fan Throw
				 * Effect: - To: "0x57D4F5B" - ItemId: "0x27A3" - ItemIdName: "Tessen" - FromLocation: "(992 299, 24)" - ToLocation: "(992 308, 22)" - Speed: "10" - Duration: "0" - FixedDirection: "False" - Explode: "False" - Hue: "0x0" - Render: "0x0"
				 * Damage: 50-65
				 */
				//Effects.SendPacket(attacker, attacker.Map, new HuedEffect(EffectType.Moving, Serial.Zero, Serial.Zero, 0x27A3, Location, attacker.Location, 10, 0, false, false, 0, 0));
				//AOS.Damage(attacker, this, Utility.RandomMinMax(50, 65), 100, 0, 0, 0, 0);
			//}/
		//}

		//public override void OnGaveMeleeAttack(Mobile defender)
		//{
			//base.OnGaveMeleeAttack(defender);

			//if (!IsFanned(defender) && 0.05 > Utility.RandomDouble())
			//{
				/* Fanning Fire
				 * Graphic: Type: "3" From: "0x57D4F5B" To: "0x0" ItemId: "0x3709" ItemIdName: "fire column" FromLocation: "(994 325, 16)" ToLocation: "(994 325, 16)" Speed: "10" Duration: "30" FixedDirection: "True" Explode: "False" Hue: "0x0" RenderMode: "0x0" Effect: "0x34" ExplodeEffect: "0x1" ExplodeSound: "0x0" Serial: "0x57D4F5B" Layer: "5" Unknown: "0x0"
				 * Sound: 0x208
				 * Start cliloc: 1070833
				 * Effect: Fire res -10% for 10 seconds
				 * Damage: 35-45, 100% fire
				 * End cliloc: 1070834
				 * Effect does not stack
				 */

				//defender.SendLocalizedMessage(1070833); // The creature fans you with fire, reducing your resistance to fire attacks.

				//int effect = -(defender.FireResistance / 10);

				//ResistanceMod mod = new ResistanceMod(ResistanceType.Fire, effect);

				//defender.FixedParticles(0x37B9, 10, 30, 0x34, EffectLayer.RightFoot);
				//defender.PlaySound(0x208);

				// This should be done in place of the normal attack damage.
				//AOS.Damage( defender, this, Utility.RandomMinMax( 35, 45 ), 0, 100, 0, 0, 0 );

				//defender.AddResistanceMod(mod);

				//ExpireTimer timer = new ExpireTimer(defender, mod, TimeSpan.FromSeconds(10.0));
				//timer.Start();
				//m_Table[defender] = timer;
			//}
		//}

		//private static readonly Hashtable m_Table = new Hashtable();

		/*public bool IsFanned(Mobile m)
		{
			return m_Table.Contains(m);
		}

		private class ExpireTimer : Timer
		{
			private readonly Mobile m_Mobile;
			private readonly ResistanceMod m_Mod;

			public ExpireTimer(Mobile m, ResistanceMod mod, TimeSpan delay) : base(delay)
			{
				m_Mobile = m;
				m_Mod = mod;
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				m_Mobile.SendLocalizedMessage(1070834); // Your resistance to fire attacks has returned.
				m_Mobile.RemoveResistanceMod(m_Mod);
				Stop();
				m_Table.Remove(m_Mobile);
			}
		}*/

		public void LowerFireResist(Mobile target)
		{
			if (BaseAttackHelperSE.IsUnderEffect(target, BaseAttackHelperSE.m_FanDancerRMT)) return;

			TimeSpan duration = TimeSpan.FromSeconds(121 - (int)(target.Skills[SkillName.MagicResist].Value));
			int effect = -(target.FireResistance / 10);
			ResistanceMod[] mod = new ResistanceMod[1]
			{
				new ResistanceMod( ResistanceType.Fire, -25 )
			};

			target.SendLocalizedMessage(1070833); // The creature fans you with fire, reducing your resistance to fire attacks.

			BaseAttackHelperSE.LowerResistanceAttack(this, ref m_Timer, duration, target, mod, BaseAttackHelperSE.m_FanDancerRMT);
			BuffInfo.AddBuff(target, new BuffInfo(BuffIcon.FanDancerFanFire, 1153787, 1153817, TimeSpan.FromSeconds(10.0), target, effect));

		}

		public override void OnThink()
		{
			if (DateTime.UtcNow >= m_NextAbilityTime)
			{
				if (Utility.RandomBool())
				{
					ThrowingTessenSE tessen = new ThrowingTessenSE(this);

					tessen.ThrowIt();
				}
				else
				{
					Mobile target = BaseAttackHelperSE.GetRandomAttacker(this, FanDancer.AbilityRange);

					if (target != null)
						LowerFireResist(target);
				}

				m_NextAbilityTime = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(m_MinTime, m_MaxTime));
			}

			base.OnThink();
		}

		public FanDancer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
