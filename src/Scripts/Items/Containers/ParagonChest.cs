using Server.Misc;

namespace Server.Items;

[Flipable]
public class ParagonChest : LockableContainer
{
	private static readonly int[] m_ItemIDs = {
		0x9AB, 0xE40, 0xE41, 0xE7C
	};

	private static readonly int[] m_Hues = {
		0x0, 0x455, 0x47E, 0x89F, 0x8A5, 0x8AB,
		0x966, 0x96D, 0x972, 0x973, 0x979
	};

	private string m_Name;
	[Constructable]
	public ParagonChest(string name, int level) : base(Utility.RandomList(m_ItemIDs))
	{
		m_Name = name;
		Hue = Utility.RandomList(m_Hues);
		LootHelpers.Fill(this, level);
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);
		LabelTo(from, 1063449, m_Name);
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1063449, m_Name);
	}

	public void Flip()
	{
		ItemId = ItemId switch
		{
			0x9AB => 0xE7C,
			0xE7C => 0x9AB,
			0xE40 => 0xE41,
			0xE41 => 0xE40,
			_ => ItemId
		};
	}

	public ParagonChest(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(m_Name);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();

		m_Name = Utility.Intern(reader.ReadString());
	}
}
