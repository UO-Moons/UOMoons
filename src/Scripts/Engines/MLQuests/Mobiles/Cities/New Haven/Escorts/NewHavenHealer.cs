using System;
using Server.Items;
using Server.Gumps;

namespace Server.Mobiles;

public class NewHavenHealer : NewHavenEscortable
{
	private static readonly TimeSpan ResurrectDelay = TimeSpan.FromSeconds(2.0);
	private DateTime m_NextResurrect;
	[Constructable]
	public NewHavenHealer()
		: base()
	{
		Title = "the wandering healer";

		Ai = AIType.AI_Mage;
		ActiveSpeed = 0.2;
		PassiveSpeed = 0.8;
		RangePerception = BaseCreature.DefaultRangePerception;
		FightMode = FightMode.Aggressor;

		SpeechHue = 0;

		SetStr(304, 400);
		SetDex(102, 150);
		SetInt(204, 300);

		SetDamage(10, 23);

		SetDamageType(ResistanceType.Physical, 100);

		SetResistance(ResistanceType.Physical, 40, 50);
		SetResistance(ResistanceType.Fire, 40, 50);
		SetResistance(ResistanceType.Cold, 40, 50);
		SetResistance(ResistanceType.Poison, 40, 50);
		SetResistance(ResistanceType.Energy, 40, 50);

		SetSkill(SkillName.Anatomy, 75.0, 97.5);
		SetSkill(SkillName.EvalInt, 82.0, 100.0);
		SetSkill(SkillName.Healing, 75.0, 97.5);
		SetSkill(SkillName.Magery, 82.0, 100.0);
		SetSkill(SkillName.MagicResist, 82.0, 100.0);
		SetSkill(SkillName.Tactics, 82.0, 100.0);
		SetSkill(SkillName.Camping, 80.0, 100.0);
		SetSkill(SkillName.Forensics, 80.0, 100.0);
		SetSkill(SkillName.SpiritSpeak, 80.0, 100.0);

		Fame = 1000;
		Karma = 10000;

		PackItem(new Bandage(Utility.RandomMinMax(5, 10)));
		PackItem(new HealPotion());
		PackItem(new CurePotion());
	}

	public NewHavenHealer(Serial serial)
		: base(serial)
	{
	}

	public override bool ClickTitle => false;
	public override bool CanTeach => true;
	public virtual bool HealsYoungPlayers => true;
	public override bool CheckTeach(SkillName skill, Mobile from)
	{
		if (!base.CheckTeach(skill, from))
			return false;

		return (skill == SkillName.Anatomy) ||
		       (skill == SkillName.Camping) ||
		       (skill == SkillName.Forensics) ||
		       (skill == SkillName.Healing) ||
		       (skill == SkillName.SpiritSpeak);
	}

	public override void InitOutfit()
	{
		AddItem(new Sandals(GetShoeHue()));
		AddItem(new Robe(Utility.RandomYellowHue()));
		AddItem(new GnarledStaff());
	}

	public virtual bool CheckResurrect(Mobile m)
	{
		if (m.Criminal)
		{
			Say(501222); // Thou art a criminal.  I shall not resurrect thee.
			return false;
		}
		else if (m.Murderer)
		{
			Say(501223); // Thou'rt not a decent and good person. I shall not resurrect thee.
			return false;
		}

		return true;
	}

	public virtual void OfferResurrection(Mobile m)
	{
		Direction = GetDirectionTo(m);
		Say(501224); // Thou hast strayed from the path of virtue, but thou still deservest a second chance.

		m.PlaySound(0x214);
		m.FixedEffect(0x376A, 10, 16);

		m.CloseGump(typeof(ResurrectGump));
		m.SendGump(new ResurrectGump(m, ResurrectMessage.Healer));
	}

	public virtual void OfferHeal(PlayerMobile m)
	{
		Direction = GetDirectionTo(m);

		if (m.CheckYoungHealTime())
		{
			Say(501229); // You look like you need some healing my child.

			m.PlaySound(0x1F2);
			m.FixedEffect(0x376A, 9, 32);

			m.Hits = m.HitsMax;
		}
		else
		{
			Say(501228); // I can do no more for you at this time.
		}
	}

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (!m.Frozen && DateTime.UtcNow >= m_NextResurrect && InRange(m, 4) && !InRange(oldLocation, 4) && InLOS(m))
		{
			if (!m.Alive)
			{
				m_NextResurrect = DateTime.UtcNow + ResurrectDelay;

				if (m.Map == null || !m.Map.CanFit(m.Location, 16, false, false))
				{
					m.SendLocalizedMessage(502391); // Thou can not be resurrected there!
				}
				else if (CheckResurrect(m))
				{
					OfferResurrection(m);
				}
			}
			else if (HealsYoungPlayers && m.Hits < m.HitsMax && m is PlayerMobile && ((PlayerMobile)m).Young)
			{
				OfferHeal((PlayerMobile)m);
			}
		}
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();
	}
}
