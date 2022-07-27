using System;

namespace Server.Items;

public class SmugglersToolBox : Item
{
	private int m_UsesRemaining;

	[CommandProperty(AccessLevel.GameMaster)]
	private int UsesRemaining { get => m_UsesRemaining;
		set { m_UsesRemaining = value; InvalidateProperties(); } }

	private DateTime NextRecharge { get; set; }

	public override int LabelNumber => 1071520;  // Smuggler's Tool Box

	[Constructable]
	public SmugglersToolBox()
		: base(0x1EB6)
	{
		Hue = 953;
		UsesRemaining = 10;

		NextRecharge = DateTime.UtcNow;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (IsChildOf(m.Backpack) && m_UsesRemaining > 0)
		{
			Lockpick lockpick = new(Utility.RandomMinMax(5, 12));

			if (m.Backpack == null || !m.Backpack.TryDropItem(m, lockpick, false))
			{
				m.SendLocalizedMessage(1077971); // Make room in your backpack first!
				lockpick.Delete();
			}
			else
			{
				m.SendLocalizedMessage(1071526); // You take some lockpicks from the tool box.
				UsesRemaining--;
			}
		}
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);
		list.Add(1060584, m_UsesRemaining.ToString());
	}

	public SmugglersToolBox(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version
		writer.Write(m_UsesRemaining);
		writer.Write(NextRecharge);

		if (NextRecharge >= DateTime.UtcNow)
			return;

		UsesRemaining = Math.Min(20, UsesRemaining + 1);
		NextRecharge = DateTime.UtcNow + TimeSpan.FromHours(24);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();
		m_UsesRemaining = reader.ReadInt();
		NextRecharge = reader.ReadDateTime();
	}
}
