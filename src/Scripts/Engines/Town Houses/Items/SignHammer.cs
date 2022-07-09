using Server.Multis;
using Server.Targeting;
using System.Collections;

namespace Server.Engines.TownHouses;

public enum HammerJob
{
	Flip,
	Swap
}

public class SignHammer : BaseItem
{
	private static readonly Hashtable STable = new();
	private static readonly ArrayList SList = new();

	public static void Initialize()
	{
		// Signs
		STable[0xB95] = 0xB96;
		STable[0xB96] = 0xB95;
		STable[0xBA3] = 0xBA4;
		STable[0xBA4] = 0xBA3;
		STable[0xBA5] = 0xBA6;
		STable[0xBA6] = 0xBA5;
		STable[0xBA7] = 0xBA8;
		STable[0xBA8] = 0xBA7;
		STable[0xBA9] = 0xBAA;
		STable[0xBAA] = 0xBA9;
		STable[0xBAB] = 0xBAC;
		STable[0xBAC] = 0xBAB;
		STable[0xBAD] = 0xBAE;
		STable[0xBAE] = 0xBAD;
		STable[0xBAF] = 0xBB0;
		STable[0xBB0] = 0xBAF;
		STable[0xBB1] = 0xBB2;
		STable[0xBB2] = 0xBB1;
		STable[0xBB3] = 0xBB4;
		STable[0xBB4] = 0xBB3;
		STable[0xBB5] = 0xBB6;
		STable[0xBB6] = 0xBB5;
		STable[0xBB7] = 0xBB8;
		STable[0xBB8] = 0xBB7;
		STable[0xBB9] = 0xBBA;
		STable[0xBBA] = 0xBB9;
		STable[0xBBB] = 0xBBC;
		STable[0xBBC] = 0xBBB;
		STable[0xBBD] = 0xBBE;
		STable[0xBBE] = 0xBBD;
		STable[0xBBF] = 0xBC0;
		STable[0xBC0] = 0xBBF;
		STable[0xBC1] = 0xBC2;
		STable[0xBC2] = 0xBC1;
		STable[0xBC3] = 0xBC4;
		STable[0xBC4] = 0xBC3;
		STable[0xBC5] = 0xBC6;
		STable[0xBC6] = 0xBC5;
		STable[0xBC7] = 0xBC8;
		STable[0xBC8] = 0xBC7;
		STable[0xBC9] = 0xBCA;
		STable[0xBCA] = 0xBC9;
		STable[0xBCB] = 0xBCC;
		STable[0xBCC] = 0xBCB;
		STable[0xBCD] = 0xBCE;
		STable[0xBCE] = 0xBCD;
		STable[0xBCF] = 0xBD0;
		STable[0xBD0] = 0xBCF;
		STable[0xBD1] = 0xBD2;
		STable[0xBD2] = 0xBD1;
		STable[0xBD3] = 0xBD4;
		STable[0xBD4] = 0xBD3;
		STable[0xBD5] = 0xBD6;
		STable[0xBD6] = 0xBD5;
		STable[0xBD7] = 0xBD8;
		STable[0xBD8] = 0xBD7;
		STable[0xBD9] = 0xBDA;
		STable[0xBDA] = 0xBD9;
		STable[0xBDB] = 0xBDC;
		STable[0xBDC] = 0xBDB;
		STable[0xBDD] = 0xBDE;
		STable[0xBDE] = 0xBDD;
		STable[0xBDF] = 0xBE0;
		STable[0xBE0] = 0xBDF;
		STable[0xBE1] = 0xBE2;
		STable[0xBE2] = 0xBE1;
		STable[0xBE3] = 0xBE4;
		STable[0xBE4] = 0xBE3;
		STable[0xBE5] = 0xBE6;
		STable[0xBE6] = 0xBE5;
		STable[0xBE7] = 0xBE8;
		STable[0xBE8] = 0xBE7;
		STable[0xBE9] = 0xBEA;
		STable[0xBEA] = 0xBE9;
		STable[0xBEB] = 0xBEC;
		STable[0xBEC] = 0xBEB;
		STable[0xBED] = 0xBEE;
		STable[0xBEE] = 0xBED;
		STable[0xBEF] = 0xBF0;
		STable[0xBF0] = 0xBEF;
		STable[0xBF1] = 0xBF2;
		STable[0xBF2] = 0xBF1;
		STable[0xBF3] = 0xBF4;
		STable[0xBF4] = 0xBF3;
		STable[0xBF5] = 0xBF6;
		STable[0xBF6] = 0xBF5;
		STable[0xBF7] = 0xBF8;
		STable[0xBF8] = 0xBF7;
		STable[0xBF9] = 0xBFA;
		STable[0xBFA] = 0xBF9;
		STable[0xBFB] = 0xBFC;
		STable[0xBFC] = 0xBFB;
		STable[0xBFD] = 0xBFE;
		STable[0xBFE] = 0xBFD;
		STable[0xBFF] = 0xC00;
		STable[0xC00] = 0xBFF;
		STable[0xC01] = 0xC02;
		STable[0xC02] = 0xC01;
		STable[0xC03] = 0xC04;
		STable[0xC04] = 0xC03;
		STable[0xC05] = 0xC06;
		STable[0xC06] = 0xC05;
		STable[0xC07] = 0xC08;
		STable[0xC08] = 0xC07;
		STable[0xC09] = 0xC0A;
		STable[0xC0A] = 0xC09;
		STable[0xC0B] = 0xC0C;
		STable[0xC0C] = 0xC0B;
		STable[0xC0D] = 0xC0E;
		STable[0xC0E] = 0xC0D;

		// Hangers
		STable[0xB97] = 0xB98;
		STable[0xB98] = 0xB97;
		STable[0xB99] = 0xB9A;
		STable[0xB9A] = 0xB99;
		STable[0xB9B] = 0xB9C;
		STable[0xB9C] = 0xB9B;
		STable[0xB9D] = 0xB9E;
		STable[0xB9E] = 0xB9D;
		STable[0xB9F] = 0xBA0;
		STable[0xBA0] = 0xB9F;
		STable[0xBA1] = 0xBA2;
		STable[0xBA2] = 0xBA1;

		// Hangers for swapping
		SList.Add(0xB97);
		SList.Add(0xB98);
		SList.Add(0xB99);
		SList.Add(0xB9A);
		SList.Add(0xB9B);
		SList.Add(0xB9C);
		SList.Add(0xB9D);
		SList.Add(0xB9E);
		SList.Add(0xB9F);
		SList.Add(0xBA0);
		SList.Add(0xBA1);
		SList.Add(0xBA2);
	}

	public HammerJob Job { get; set; }

	[Constructable]
	public SignHammer()
		: base(0x13E3)
	{
		Name = "Sign Hammer";
	}

	private static int GetFlipFor(int id)
	{
		return STable[id] == null ? id : (int)STable[id];
	}

	private static int GetNextSign(int id)
	{
		if (!SList.Contains(id))
		{
			return id;
		}

		var idx = SList.IndexOf(id);

		if (idx + 2 < SList.Count)
		{
			return (int)SList[idx + 2]!;
		}

		if (idx % 2 == 0)
		{
			return (int)SList[0]!;
		}

		return (int)SList[1]!;
	}

	public override void OnDoubleClick(Mobile m)
	{
		if (RootParent != m)
		{
			m.SendMessage("That item must be in your backpack to use.");
			return;
		}

		BaseHouse house = BaseHouse.FindHouseAt(m);

		if (m.AccessLevel == AccessLevel.Player && (house == null || house.Owner != m))
		{
			m.SendMessage("You have to be inside your house to use this.");
			return;
		}

		m.BeginTarget(3, false, TargetFlags.None, OnTarget);
	}

	private void OnTarget(Mobile m, object obj)
	{
		if (obj is SignHammer || obj is not Item item)
		{
			m.SendMessage("You cannot change that with this.");
			return;
		}

		if (item == this)
		{
			_ = new SignHammerGump(m, this);
			return;
		}

		if (Job == HammerJob.Flip)
		{
			int id = GetFlipFor(item.ItemId);

			if (id == item.ItemId)
			{
				m.SendMessage("You cannot change that with this.");
			}
			else
			{
				item.ItemId = id;
			}
		}
		else
		{
			var id = GetNextSign(item.ItemId);

			if (id == item.ItemId)
			{
				m.SendMessage("You cannot change that with this.");
			}
			else
			{
				item.ItemId = id;
			}
		}
	}

	public SignHammer(Serial serial)
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
		_ = reader.ReadInt();
	}
}

public sealed class SignHammerGump : GumpPlusLight
{
	private readonly SignHammer _cHammer;

	public SignHammerGump(Mobile m, SignHammer hammer) : base(m, 100, 100)
	{
		_cHammer = hammer;

		NewGump();
	}

	protected override void BuildGump()
	{
		AddBackground(0, 0, 200, 200, 2600);

		AddButton(50, 45, 2152, 2154, "Swap", Swap);
		AddHtml(90, 50, 70, "Swap Hanger");

		AddButton(50, 95, 2152, 2154, "Flip", Flip);
		AddHtml(90, 100, 70, "Flip Sign or Hanger");
	}

	private void Swap()
	{
		_cHammer.Job = HammerJob.Swap;
	}

	private void Flip()
	{
		_cHammer.Job = HammerJob.Flip;
	}
}
