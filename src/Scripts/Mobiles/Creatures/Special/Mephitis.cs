using Server.Engines.Champions;
using Server.Items;
using System;

namespace Server.Mobiles
{
	public class Mephitis : BaseChampion
	{
		public override ChampionSkullType SkullType => ChampionSkullType.Venom;

		public override Type[] UniqueList => new Type[] { typeof(Calm) };
		public override Type[] SharedList => new Type[] { typeof(OblivionsNeedle), typeof(ANecromancerShroud), typeof(EmbroideredOakLeafCloak), typeof(TheMostKnowledgePerson) };
		public override Type[] DecorativeList => new Type[] { typeof(Web), typeof(MonsterStatuette) };

		public override MonsterStatuetteType[] StatueTypes => new MonsterStatuetteType[] { MonsterStatuetteType.Spider };

		[Constructable]
		public Mephitis() : base(AIType.AI_Melee)
		{
			Body = 173;
			Name = "Mephitis";

			BaseSoundId = 0x183;

			SetStr(505, 1000);
			SetDex(102, 300);
			SetInt(402, 600);

			SetHits(3000);
			SetStam(105, 600);

			SetDamage(21, 33);

			SetDamageType(ResistanceType.Physical, 50);
			SetDamageType(ResistanceType.Poison, 50);

			SetResistance(ResistanceType.Physical, 75, 80);
			SetResistance(ResistanceType.Fire, 60, 70);
			SetResistance(ResistanceType.Cold, 60, 70);
			SetResistance(ResistanceType.Poison, 100);
			SetResistance(ResistanceType.Energy, 60, 70);

			SetSkill(SkillName.MagicResist, 70.7, 140.0);
			SetSkill(SkillName.Tactics, 97.6, 100.0);
			SetSkill(SkillName.Wrestling, 97.6, 100.0);

			Fame = 22500;
			Karma = -22500;

			VirtualArmor = 80;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.UltraRich, 4);
		}

		public override Poison PoisonImmune => Poison.Lethal;
		public override Poison HitPoison => Poison.Lethal;

		public override void OnDamagedBySpell(Mobile from)
		{
			if (from != null && from != this && 0.33 > Utility.RandomDouble())
			{
				WebAttack(this, from);
			}

			base.OnDamagedBySpell(from);
		}

		public override void OnGotMeleeAttack(Mobile attacker)
		{
			if (attacker != null && attacker != this && 0.33 > Utility.RandomDouble())
			{
				WebAttack(this, attacker);
			}

			base.OnGotMeleeAttack(attacker);
		}

		public static void WebAttack(Mobile from, Mobile to)
		{
			Map map = from.Map;

			if (map == null)
				return;

			int x = from.X + Utility.RandomMinMax(-1, 1);
			int y = from.Y + Utility.RandomMinMax(-1, 1);
			int z = from.Z;

			SelfDeletingItem web = new SelfDeletingItem(3812, "a web", 5);

			to.MovingEffect(from, 0xee6, 7, 1, false, false, 0x481, 0);
			to.Paralyze(TimeSpan.FromSeconds(6));
			to.SendMessage("You are are caught in a web");
			to.MoveToWorld(new Point3D(x, y, z), map);
			web.MoveToWorld(new Point3D(x, y, z), map);
			to.ApplyPoison(from, Poison.Lethal);
		}

		public Mephitis(Serial serial) : base(serial)
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
