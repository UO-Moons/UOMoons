namespace Server.Items;

public class GamblingStone : BaseItem
{
	private int m_GamblePot = 2500;

	[CommandProperty(AccessLevel.GameMaster)]
	public int GamblePot
	{
		get => m_GamblePot;
		set
		{
			m_GamblePot = value;
			InvalidateProperties();
		}
	}

	public override string DefaultName => "a gambling stone";

	[Constructable]
	public GamblingStone()
		: base(0xED4)
	{
		Movable = false;
		Hue = 0x56;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add("Jackpot: {0}gp", m_GamblePot);
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);
		LabelTo(from, "Jackpot: {0}gp", m_GamblePot);
	}

	public override void OnDoubleClick(Mobile from)
	{
		Container pack = from.Backpack;

		if (pack != null && pack.ConsumeTotal(typeof(Gold), 250))
		{
			m_GamblePot += 150;
			InvalidateProperties();

			int roll = Utility.Random(1200);

			switch (roll)
			{
				// Jackpot
				case 0:
				{
					const int maxCheck = 1000000;

					from.SendMessage(0x35, "You win the {0}gp jackpot!", m_GamblePot);

					while (m_GamblePot > maxCheck)
					{
						from.AddToBackpack(new BankCheck(maxCheck));

						m_GamblePot -= maxCheck;
					}

					from.AddToBackpack(new BankCheck(m_GamblePot));

					m_GamblePot = 2500;
					break;
				}
				// Chance for a regbag
				case <= 20:
					from.SendMessage(0x35, "You win a bag of reagents!");
					from.AddToBackpack(new BagOfReagents(50));
					break;
				// Chance for gold
				case <= 40:
					from.SendMessage(0x35, "You win 1500gp!");
					from.AddToBackpack(new BankCheck(1500));
					break;
				// Another chance for gold
				case <= 100:
					from.SendMessage(0x35, "You win 1000gp!");
					from.AddToBackpack(new BankCheck(1000));
					break;
				// Loser!
				default:
					from.SendMessage(0x22, "You lose!");
					break;
			}
		}
		else
		{
			from.SendMessage(0x22, "You need at least 250gp in your backpack to use this.");
		}
	}

	public GamblingStone(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(m_GamblePot);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		m_GamblePot = version switch
		{
			0 => reader.ReadInt(),
			_ => m_GamblePot
		};
	}
}
