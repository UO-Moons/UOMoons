using Server.Gumps;
using Server.Mobiles;
using Server.SkillHandlers;
using System.Collections.Generic;

namespace Server.Items;

public class CircuitTrapTrainingKit : Item, ICircuitTrap, RemoveTrap.IRemoveTrapTrainingKit
{
	public override int LabelNumber => 1159014;  // Circuit Trap Training Kit

	public int GumpTitle => 1159005;  // <center>Trap Disarm Mechanism</center>
	public int GumpDescription => 1159006;  // // <center>disarm the trap</center>

	public CircuitCount Count { get; private set; }

	public List<int> Path { get; set; } = new();
	public List<int> Progress { get; set; } = new();

	public bool CanDecipher => false;

	[Constructable]
	public CircuitTrapTrainingKit()
		: base(41875)
	{
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (m.InRange(GetWorldLocation(), 1))
		{
			m.SendLocalizedMessage(1159008); // That appears to be trapped, using the remove trap skill would yield better results...
		}
	}

	public void OnRemoveTrap(Mobile m)
	{
		if (m is not PlayerMobile mobile)
			return;

		if (Path == null || Path.Count == 0)
		{
			double skill = mobile.Skills[SkillName.RemoveTrap].Base;

			Count = skill switch
			{
				< 80.0 => CircuitCount.Nine,
				< 100.0 => CircuitCount.Sixteen,
				_ => CircuitCount.TwentyFive
			};
		}

		BaseGump.SendGump(new CircuitTrapGump(mobile, this));
	}

	public void OnSelfClose(Mobile m)
	{
		Progress?.Clear();
	}

	public void OnProgress(Mobile m, int pick)
	{
		m.SendSound(0x1F4);
	}

	public void OnFailed(Mobile m)
	{
		m.SendLocalizedMessage(1159013); // You fail to disarm the trap and reset it.
	}

	public void OnComplete(Mobile m)
	{
		m.SendLocalizedMessage(1159009); // You successfully disarm the trap!

		m.CheckTargetSkill(SkillName.RemoveTrap, this, 0, 100);
	}

	public override void OnDelete()
	{
		if (RootParent is not Mobile m)
			return;

		if (m.HasGump(typeof(CircuitTrapGump)))
		{
			m.CloseGump(typeof(CircuitTrapGump));
		}
	}

	public CircuitTrapTrainingKit(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(1); // version

		writer.Write((int)Count);

		writer.Write(Path.Count);
		for (int i = 0; i < Path.Count; i++)
		{
			writer.Write(Path[i]);
		}

		writer.Write(Progress.Count);
		for (int i = 0; i < Progress.Count; i++)
		{
			writer.Write(Progress[i]);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 1:
				Count = (CircuitCount)reader.ReadInt();
				goto case 0;
			case 0:
				int count = reader.ReadInt();

				for (int i = 0; i < count; i++)
				{
					Path.Add(reader.ReadInt());
				}

				count = reader.ReadInt();

				for (int i = 0; i < count; i++)
				{
					Progress.Add(reader.ReadInt());
				}
				break;
		}

		if (version == 0)
		{
			Path.Clear();
			Progress.Clear();
		}
	}
}
