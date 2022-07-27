using Server.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Mobiles;

public abstract class BaseRenowned : BaseCreature
{
	private Dictionary<Mobile, int> m_DamageEntries;

	public BaseRenowned(AIType aiType, FightMode mode = FightMode.Closest)
		: base(aiType, mode, 18, 1, 0.1, 0.2)
	{
	}

	public BaseRenowned(Serial serial)
		: base(serial)
	{
	}

	protected abstract Type[] UniqueSaList { get; }
	protected abstract Type[] SharedSaList { get; }

	public virtual bool NoGoodies => false;

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
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

	public void RegisterDamage(Mobile from, int amount)
	{
		if (from is not { Player: true })
			return;

		if (m_DamageEntries.ContainsKey(from))
			m_DamageEntries[from] += amount;
		else
			m_DamageEntries.Add(from, amount);
	}

	public void AwardArtifact(Item artifact)
	{
		if (artifact == null)
			return;

		int totalDamage = 0;

		Dictionary<Mobile, int> validEntries = new();

		foreach (var kvp in m_DamageEntries.Where(kvp => IsEligible(kvp.Key, artifact)))
		{
			validEntries.Add(kvp.Key, kvp.Value);
			totalDamage += kvp.Value;
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

	public bool IsEligible(Mobile m, Item artifact)
	{
		return m.Player && m.Alive && m.InRange(Location, 32) && m.Backpack != null && m.Backpack.CheckHold(m, artifact, false);
	}

	public Item GetArtifact()
	{
		double random = Utility.RandomDouble();

		return random switch
		{
			<= 0.05 => CreateArtifact(UniqueSaList),
			<= 0.15 => CreateArtifact(SharedSaList),
			_ => null
		};
	}

	public Item CreateArtifact(Type[] list)
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
		if (!NoKillAwards)
		{
			if (NoGoodies)
				return base.OnBeforeDeath();

			m_DamageEntries = new Dictionary<Mobile, int>();

			RegisterDamageTo(this);
			AwardArtifact(GetArtifact());
		}

		return base.OnBeforeDeath();
	}
}
