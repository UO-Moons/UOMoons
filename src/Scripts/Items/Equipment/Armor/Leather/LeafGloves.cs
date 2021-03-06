namespace Server.Items;

[Flipable]
public class LeafGloves : BaseArmor, IArcaneEquip
{
	public override Race RequiredRace => Race.Elf;
	public override int BasePhysicalResistance => 2;
	public override int BaseFireResistance => 3;
	public override int BaseColdResistance => 2;
	public override int BasePoisonResistance => 4;
	public override int BaseEnergyResistance => 4;
	public override int InitHits => Utility.RandomMinMax(30, 40);
	public override int StrReq => Core.AOS ? 10 : 10;
	public override int ArmorBase => 13;
	public override ArmorMaterialType MaterialType => ArmorMaterialType.Leather;
	public override CraftResource DefaultResource => CraftResource.RegularLeather;
	public override ArmorMeditationAllowance DefMedAllowance => ArmorMeditationAllowance.All;

	[Constructable]
	public LeafGloves() : base(0x2FC6)
	{
		Weight = 2.0;
	}

	public LeafGloves(Serial serial) : base(serial)
	{
	}

	#region Arcane Impl
	private int m_MaxArcaneCharges, m_CurArcaneCharges;

	[CommandProperty(AccessLevel.GameMaster)]
	public int MaxArcaneCharges
	{
		get { return m_MaxArcaneCharges; }
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
		get { return m_CurArcaneCharges; }
		set
		{
			m_CurArcaneCharges = value;
			InvalidateProperties();
			Update();
		}
	}

	public int TempHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsArcane => m_MaxArcaneCharges > 0 && m_CurArcaneCharges >= 0;

	public void Update()
	{
		if (IsArcane)
			ItemId = 0x26B0;
		else if (ItemId == 0x26B0)
			ItemId = 0x13C6;

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
			list.Add(1061837, "{0}\t{1}", m_CurArcaneCharges, m_MaxArcaneCharges); // arcane charges: ~1_val~ / ~2_val~
	}

	public void Flip()
	{
		if (ItemId == 0x13C6)
			ItemId = 0x13CE;
		else if (ItemId == 0x13CE)
			ItemId = 0x13C6;
	}
	#endregion

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
