namespace Server.Items;

public class Cloak : BaseCloak, IArcaneEquip
{
	#region Arcane Impl
	private int m_MaxArcaneCharges, m_CurArcaneCharges;

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxArcaneCharges
	{
		get => m_MaxArcaneCharges;
		set
		{
			m_MaxArcaneCharges = value;
			InvalidateProperties();
			Update();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int CurArcaneCharges
	{
		get => m_CurArcaneCharges;
		set
		{
			m_CurArcaneCharges = value;
			InvalidateProperties();
			Update();
		}
	}
	public int TempHue { get; set; }


	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsArcane => (m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0);

	public void Update()
	{
		if (IsArcane)
		{
			ItemID = 0x26AD;
		}
		else if (ItemID == 0x26AD)
		{
			ItemID = 0x1515;
		}

		if (IsArcane && CurArcaneCharges == 0)
		{
			TempHue = Hue;
			Hue = 0;
		}
	}

	public override void AddCraftedProperties(ObjectPropertyList list)
	{
		base.AddCraftedProperties(list);

		if (IsArcane)
		{
			list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
		}
	}

	public override void OnSingleClick(Mobile from)
	{
		base.OnSingleClick(from);

		if (IsArcane)
		{
			LabelTo(from, 1061837, string.Format("{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges));
		}
	}

	public void Flip()
	{
		if (ItemID == 0x1515)
		{
			ItemID = 0x1530;
		}
		else if (ItemID == 0x1530)
		{
			ItemID = 0x1515;
		}
	}

	#endregion

	[Constructable]
	public Cloak() : this(0)
	{
	}

	[Constructable]
	public Cloak(int hue) : base(0x1515, hue)
	{
	}

	public Cloak(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		if (IsArcane)
		{
			writer.Write(true);
			writer.Write(m_CurArcaneCharges);
			writer.Write(m_MaxArcaneCharges);
		}
		else
		{
			writer.Write(false);
		}
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();
		switch (version)
		{
			case 0:
				{
					if (reader.ReadBool())
					{
						m_CurArcaneCharges = reader.ReadInt();
						m_MaxArcaneCharges = reader.ReadInt();
						if (Hue == 2118)
							Hue = ArcaneGem.DefaultArcaneHue;
					}
					break;
				}
		}
	}
}
