using Server.Gumps;
using Server.Mobiles;
using Server.SkillHandlers;
using System;
using System.Collections.Generic;

namespace Server.Items;

public interface ISliderKit : IEntity
{
	int[] Order { get; }
	int Style { get; }

	void Complete(Mobile m);
}

public class SliderTrapTrainingKit : Item, ISliderKit, RemoveTrap.IRemoveTrapTrainingKit
{
	public override int LabelNumber => 1159016;  // Slider Trap Training Kit

	private int m_Style;

	public int[] Order { get; } = new int[9];

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Test
	{
		get => false;
		set
		{
			if (value)
			{
				TestOrder();
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Style
	{
		get => m_Style;
		private init
		{
			if (m_Style == value)
				return;
			m_Style = value;

			if (m_Style < 0)
				m_Style = 0;

			if (m_Style > 7)
				m_Style = 7;

			Reset();
		}
	}

	[Constructable]
	public SliderTrapTrainingKit()
		: base(41875)
	{
		m_Style = -1;
		Style = Utility.Random(8);
	}

	private void Reset()
	{
		for (int i = 0; i < Order.Length; i++)
		{
			Order[i] = 0;
		}

		int randomIndex = Utility.Random(Order.Length);
		int startId = 0x9CEE + m_Style * 9; // start at 1+ because the upper left picture is always omitted
		List<int> list = new();

		for (int i = 0; i < 8; i++)
		{
			list.Add(startId + i);
		}

		for (int i = 0; i < Order.Length; i++)
		{
			if (i == randomIndex)
				continue;

			int add = list[Utility.Random(list.Count)];
			Order[i] = add;
			list.Remove(add);
		}

		int invCount = 0;
		for (int i = 0; i < Order.Length - 1; i++)
		{
			for (int j = i + 1; j < Order.Length; j++)
			{
				if (Order[j] != 0 && Order[i] != 0 && Order[i] > Order[j])
					invCount++;
			}
		}

		if (invCount % 2 == 1)
		{
			if (randomIndex > 2)
			{
				(Order[0], Order[1]) = (Order[1], Order[0]);
			}
			else
			{
				(Order[^1], Order[^2]) = (Order[^2], Order[^1]);
			}
		}
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (IsChildOf(m.Backpack))
		{
			m.SendLocalizedMessage(1159008); // That appears to be trapped, using the remove trap skill would yield better results...
		}
		else
		{
			m.SendMessage("That is not your chest!");
		}
	}

	public void OnRemoveTrap(Mobile from)
	{
		if (from is PlayerMobile mobile)
		{
			BaseGump.SendGump(new SliderTrapGump(mobile, this));
		}
	}

	public void Complete(Mobile from)
	{
		from.SendLocalizedMessage(1159009); // You successfully disarm the trap!

		from.CheckTargetSkill(SkillName.RemoveTrap, this, 0, 100);

		Reset();
	}

	private void TestOrder()
	{
		int startId = 0x9CEE + m_Style * 9; // start at 1+ because the upper left picture is always omitted
		List<int> list = new();

		for (int i = 0; i < 8; i++)
		{
			list.Add(startId + i);
		}

		for (int i = 0; i < Order.Length; i++)
		{
			if (i != 1)
			{
				Order[i] = list[0];
				list.RemoveAt(0);
			}
			else
			{
				Order[i] = 0;
			}
		}
	}

	public SliderTrapTrainingKit(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(m_Style);

		for (int i = 0; i < Order.Length; i++)
		{
			writer.Write(Order[i]);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		m_Style = reader.ReadInt();

		for (int i = 0; i < Order.Length; i++)
		{
			Order[i] = reader.ReadInt();
		}
	}
}

public class SliderTrapGump : BaseGump
{
	private ISliderKit Kit { get; }
	private int[] Order => Kit.Order;

	public SliderTrapGump(PlayerMobile pm, ISliderKit kit)
		: base(pm, 100, 100)
	{
		pm.CloseGump(GetType());
		Kit = kit;
	}

	public override void AddGumpLayout()
	{
		AddBackground(0, 0, 270, 445, 0x6DB);
		AddImage(15, 20, 0x9CED + Kit.Style * 9);
		AddAlphaRegion(15, 20, 80, 133);

		for (int i = 0; i < Order.Length; i++)
		{
			int order = Order[i];

			if (order == 0)
				continue;

			int x = i % 3 == 0 ? 15 : i % 3 == 1 ? 95 : 175;
			int y = i <= 2 ? 20 : i <= 5 ? 153 : 286;

			AddButton(x, y, order, order, i + 1, GumpButtonType.Reply, 0);
		}
	}

	public override void OnResponse(RelayInfo info)
	{
		if (!Kit.Deleted && info.ButtonID is >= 1 and <= 9)
		{
			int pick = info.ButtonID - 1;
			int empty = Array.IndexOf(Order, 0);

			if (ValidMove(pick, empty))
			{
				User.SendSound(0x42);

				int id = Order[pick];
				Order[pick] = 0;
				Order[empty] = id;

				if (CheckSolution())
				{
					Kit.Complete(User);
				}
				else
				{
					Refresh();
				}
			}
			else
			{
				User.SendSound(0x051);
				Refresh();
			}
		}
	}

	private static bool ValidMove(int pick, int empty)
	{
		return pick switch
		{
			0 => empty is 1 or 3,
			1 => empty is 0 or 2 or 4,
			2 => empty is 1 or 5,
			3 => empty is 0 or 4 or 6,
			4 => empty is 1 or 3 or 5 or 7,
			5 => empty is 2 or 4 or 8,
			6 => empty is 3 or 7,
			7 => empty is 4 or 6 or 8,
			8 => empty is 5 or 7,
			_ => false
		};
	}

	private bool CheckSolution()
	{
		int start = 0x9CEE + Kit.Style * 9;

		return Order[0] == 0 &&
		       Order[1] == start &&
		       Order[2] == start + 1 &&
		       Order[3] == start + 2 &&
		       Order[4] == start + 3 &&
		       Order[5] == start + 4 &&
		       Order[6] == start + 5 &&
		       Order[7] == start + 6 &&
		       Order[8] == start + 7;
	}
}
