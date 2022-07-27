using Server.Items;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

[TypeAlias("Server.Mobiles.BaseSABosses")]
public abstract class BaseSABoss : BasePeerless
{
	public override bool GiveMlSpecial => false;

	Dictionary<Mobile, int> m_DamageEntries;
	public BaseSABoss(AIType aiType, FightMode fightMode, int rangePerception, int rangeFight, double activeSpeed, double passiveSpeed)
		: base(aiType, fightMode, rangePerception, rangeFight, activeSpeed, passiveSpeed)
	{
	}

	public BaseSABoss(Serial serial)
		: base(serial)
	{
	}

	public abstract Type[] UniqueSaList { get; }
	public abstract Type[] SharedSaList { get; }

	public virtual bool NoGoodies => false;

	public override bool DropPrimer => false;

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
	}

	public virtual void RegisterDamageTo(Mobile m)
	{
		if (m == null)
			return;

		foreach (DamageEntry de in m.DamageEntries)
		{
			Mobile damager = de.Damager;

			Mobile master = damager.GetDamageMaster(m);

			if (master != null)
				damager = master;

			RegisterDamage(damager, de.DamageGiven);
		}
	}

	private void RegisterDamage(Mobile from, int amount)
	{
		if (from is not { Player: true })
			return;

		if (m_DamageEntries.ContainsKey(from))
			m_DamageEntries[from] += amount;
		else
			m_DamageEntries.Add(from, amount);
	}

	private void AwardArtifact(Item artifact)
	{
		if (artifact == null)
			return;

		int totalDamage = 0;

		Dictionary<Mobile, int> validEntries = new();

		foreach (KeyValuePair<Mobile, int> kvp in m_DamageEntries)
		{
			if (IsEligible(kvp.Key, artifact))
			{
				validEntries.Add(kvp.Key, kvp.Value);
				totalDamage += kvp.Value;
			}
		}

		int randomDamage = Utility.RandomMinMax(1, totalDamage);

		totalDamage = 0;

		foreach (KeyValuePair<Mobile, int> kvp in m_DamageEntries)
		{
			totalDamage += kvp.Value;

			if (totalDamage <= randomDamage)
				continue;
			GiveArtifact(kvp.Key, artifact);
			break;
		}
	}

	private static void GiveArtifact(Mobile to, Item artifact)
	{
		if (to == null || artifact == null)
			return;

		Container pack = to.Backpack;

		if (pack == null || !pack.TryDropItem(to, artifact, false))
		{
			artifact.Delete();
		}
		else
		{
			to.SendLocalizedMessage(1062317); // For your valor in combating the fallen beast, a special artifact has been bestowed on you.
			to.PlaySound(0x5B4);
		}
	}

	private bool IsEligible(Mobile m, Item artifact)
	{
		return m.Player && m.Alive && m.InRange(Location, 32) && m.Backpack != null && m.Backpack.CheckHold(m, artifact, false);
	}

	private Item GetArtifact()
	{
		double random = Utility.RandomDouble();
		return random switch
		{
			<= 0.05 => CreateArtifact(UniqueSaList),
			<= 0.15 => CreateArtifact(SharedSaList),
			_ => null
		};
	}

	private static Item CreateArtifact(Type[] list)
	{
		if (list.Length == 0)
			return null;

		int random = Utility.Random(list.Length);

		Type type = list[random];

		Item artifact = Loot.Construct(type);

		return artifact;
	}

	public override bool OnBeforeDeath()
	{
		if (NoKillAwards) return base.OnBeforeDeath();
		m_DamageEntries = new Dictionary<Mobile, int>();

		RegisterDamageTo(this);
		AwardArtifact(GetArtifact());

		return base.OnBeforeDeath();
	}
}
