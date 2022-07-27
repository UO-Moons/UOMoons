using Server.Mobiles;
using System;

namespace Server.Items;

public class NiporailemsTreasure : Item
{
	public override int LabelNumber => ItemId == 0xEEF
		? 1112113  // Niporailem's Treasure
		: 1112115; // Treasure Sand

	private bool m_CanSpawn;

	public NiporailemsTreasure()
		: base(0xEEF)
	{
		Weight = 25.0;

		var timer = new NiporailemsTreasureTimer(this);
		timer.Start();

		m_CanSpawn = true;
	}

	private void TurnToSand()
	{
		ItemId = 0x11EA + Utility.Random(1);
		m_CanSpawn = false;
	}

	public override bool OnDroppedToWorld(Mobile from, Point3D p)
	{
		if (!base.OnDroppedToWorld(from, p))
		{
			return false;
		}

		if (m_CanSpawn)
		{
			int amount = Utility.Random(3); // 0-2

			for (int i = 0; i < amount; i++)
			{
				Mobile summon;

				if (Utility.RandomBool())
				{
					summon = new CursedMetallicKnight();
				}
				else
				{
					summon = new CursedMetallicMage();
				}

				summon.MoveToWorld(p, from.Map);
			}
		}
		from.SendLocalizedMessage(1112111); // To steal my gold? To give it freely!

		TurnToSand();

		return true;
	}

	public NiporailemsTreasure(Serial serial)
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
		reader.ReadInt();
	}

	private class NiporailemsTreasureTimer : Timer
	{
		private readonly NiporailemsTreasure m_Owner;

		public NiporailemsTreasureTimer(NiporailemsTreasure owner)
			: base(TimeSpan.FromSeconds(60.0))
		{

			m_Owner = owner;
		}

		protected override void OnTick()
		{
			if (!m_Owner.Deleted)
			{
				m_Owner.TurnToSand();
			}
		}
	}
}

public class TreasureSand : Item
{
	public override int LabelNumber => 1112115;  // Treasure Sand

	public TreasureSand()
		: base(0x11EA + Utility.Random(1))
	{
		Weight = 1.0;
	}

	public TreasureSand(Serial serial)
		: base(serial)
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
		reader.ReadInt();
	}
}
