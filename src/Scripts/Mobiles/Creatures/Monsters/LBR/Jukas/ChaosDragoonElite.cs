using Server.Items;
using System;

namespace Server.Mobiles
{
	[CorpseName("a chaos dragoon elite corpse")]
	public class ChaosDragoonElite : BaseCreature
	{
		private DateTime m_Delay = DateTime.UtcNow;

		[Constructable]
		public ChaosDragoonElite()
			: base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.15, 0.4)
		{
			Name = "a chaos dragoon elite";
			Body = 0x190;
			Hue = Utility.RandomSkinHue();

			SetStr(276, 350);
			SetDex(66, 90);
			SetInt(126, 150);

			SetHits(276, 350);

			SetDamage(29, 34);

			SetDamageType(ResistanceType.Physical, 100);

			/*SetResistance(ResistanceType.Physical, 45, 55);
			SetResistance(ResistanceType.Fire, 15, 25);
			SetResistance(ResistanceType.Cold, 50);
			SetResistance(ResistanceType.Poison, 25, 35);
			SetResistance(ResistanceType.Energy, 25, 35);*/


			SetSkill(SkillName.Tactics, 80.1, 100.0);
			SetSkill(SkillName.MagicResist, 100.1, 110.0);
			SetSkill(SkillName.Anatomy, 80.1, 100.0);
			SetSkill(SkillName.Magery, 85.1, 100.0);
			SetSkill(SkillName.EvalInt, 85.1, 100.0);
			SetSkill(SkillName.Swords, 72.5, 95.0);
			SetSkill(SkillName.Fencing, 85.1, 100);
			SetSkill(SkillName.Macing, 85.1, 100);

			Fame = 8000;
			Karma = -8000;

			CraftResource res = CraftResource.None; ;

			switch (Utility.Random(6))
			{
				case 0: res = CraftResource.BlackScales; break;
				case 1: res = CraftResource.RedScales; break;
				case 2: res = CraftResource.BlueScales; break;
				case 3: res = CraftResource.YellowScales; break;
				case 4: res = CraftResource.GreenScales; break;
				case 5: res = CraftResource.WhiteScales; break;
			}

			BaseWeapon melee = null;

			switch (Utility.Random(3))
			{
				case 0: melee = new Kryss(); break;
				case 1: melee = new Broadsword(); break;
				case 2: melee = new Katana(); break;
			}

			melee.Movable = false;
			AddItem(melee);

			DragonChest Tunic = new DragonChest
			{
				Resource = res,
				Movable = false
			};
			AddItem(Tunic);

			DragonLegs Legs = new DragonLegs
			{
				Resource = res,
				Movable = false
			};
			AddItem(Legs);

			DragonArms Arms = new DragonArms
			{
				Resource = res,
				Movable = false
			};
			AddItem(Arms);

			DragonGloves Gloves = new DragonGloves
			{
				Resource = res,
				Movable = false
			};
			AddItem(Gloves);

			DragonHelm Helm = new DragonHelm
			{
				Resource = res,
				Movable = false
			};
			AddItem(Helm);

			ChaosShield shield = new ChaosShield
			{
				Movable = false
			};
			AddItem(shield);

			AddItem(new Boots(0x455));
			AddItem(new Shirt(Utility.RandomMetalHue()));

			int amount = Utility.RandomMinMax(1, 3);

			switch (res)
			{
				case CraftResource.BlackScales: AddItem(new BlackScales(amount)); break;
				case CraftResource.RedScales: AddItem(new RedScales(amount)); break;
				case CraftResource.BlueScales: AddItem(new BlueScales(amount)); break;
				case CraftResource.YellowScales: AddItem(new YellowScales(amount)); break;
				case CraftResource.GreenScales: AddItem(new GreenScales(amount)); break;
				case CraftResource.WhiteScales: AddItem(new WhiteScales(amount)); break;
			}
			switch (Utility.Random(9))
			{
				case 0: res = CraftResource.DullCopper; break;
				case 1: res = CraftResource.ShadowIron; break;
				case 2: res = CraftResource.Copper; break;
				case 3: res = CraftResource.Bronze; break;
				case 4: res = CraftResource.Gold; break;
				case 5: res = CraftResource.Agapite; break;
				case 6: res = CraftResource.Verite; break;
				case 7: res = CraftResource.Valorite; break;
				case 8: res = CraftResource.Iron; break;
			}

			SwampDragon mt = new SwampDragon
			{
				HasBarding = true,
				BardingResource = res
			};
			mt.BardingHP = mt.BardingMaxHP;
			mt.Rider = this;
		}

		public override int GetIdleSound()
		{
			return 0x2CE;
		}

		public override int GetDeathSound()
		{
			return 0x2CC;
		}

		public override int GetHurtSound()
		{
			return 0x2D1;
		}

		public override int GetAttackSound()
		{
			return 0x2C8;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Rich);
			AddLoot(LootPack.Gems);
		}

		public override bool HasBreath => true;
		public override bool AutoDispel => true;
		public override bool BardImmune => !Core.AOS;
		public override bool CanRummageCorpses => true;
		public override bool AlwaysMurderer => true;
		public override bool ShowFameTitle => false;

		public override bool OnBeforeDeath()
		{
			IMount mount = Mount;

			if (mount != null)
			{
				if (mount is SwampDragon)
					((SwampDragon)mount).HasBarding = false;

				mount.Rider = null;
			}

			return base.OnBeforeDeath();
		}

		public override void AlterMeleeDamageTo(Mobile to, ref int damage)
		{
			if (to is Dragon || to is WhiteWyrm || to is SwampDragon || to is Drake || to is Nightmare || to is Hiryu || to is LesserHiryu || to is Daemon)
				damage *= 3;
		}


		public override void OnGaveMeleeAttack(Mobile defender)
		{
			base.OnGaveMeleeAttack(defender);

			if (DateTime.UtcNow > m_Delay)
			{
				Ability.LowerStat(defender, 10, 20, 60, 120, 4);
				m_Delay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(10, 15));
			}
		}

		public ChaosDragoonElite(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}

		private class VirtualMount : IMount
		{
			private readonly VirtualMountItem m_Item;
			public VirtualMount(VirtualMountItem item)
			{
				m_Item = item;
			}

			public Mobile Rider
			{
				get
				{
					return m_Item.Rider;
				}
				set
				{
				}
			}
			public virtual void OnRiderDamaged(Mobile from, ref int amount, bool willKill)
			{
			}
		}

		private class VirtualMountItem : Item, IMountItem
		{
			private readonly VirtualMount m_Mount;
			private Mobile m_Rider;
			public VirtualMountItem(Mobile mob)
				: base(0x3EBE)
			{
				Layer = Layer.Mount;

				Movable = false;

				m_Rider = mob;
				m_Mount = new VirtualMount(this);
			}

			public VirtualMountItem(Serial serial)
				: base(serial)
			{
				m_Mount = new VirtualMount(this);
			}

			public Mobile Rider => m_Rider;
			public IMount Mount => m_Mount;
			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_Rider);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				int version = reader.ReadInt();

				m_Rider = reader.ReadMobile();

				if (m_Rider == null)
					Delete();
			}
		}
	}
}
