using Server.Engines.VeteranRewards;

namespace Server.Items;

[Flipable]
public class RewardCloak : BaseCloak, IRewardItem
{
	private int m_LabelNumber;
	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsRewardItem { get; set; }
	[CommandProperty(AccessLevel.GameMaster)]
	public int Number { get => m_LabelNumber; set { m_LabelNumber = value; InvalidateProperties(); } }
	public override int LabelNumber => m_LabelNumber > 0 ? m_LabelNumber : base.LabelNumber;
	public override int BasePhysicalResistance => 3;

	public override void OnAdded(IEntity parent)
	{
		base.OnAdded(parent);
		if (parent is Mobile mobile)
			mobile.VirtualArmorMod += 2;
	}

	public override void OnRemoved(IEntity parent)
	{
		base.OnRemoved(parent);
		if (parent is Mobile mobile)
			mobile.VirtualArmorMod -= 2;
	}

	public override bool Dye(Mobile from, DyeTub sender)
	{
		from.SendLocalizedMessage(sender.FailMessage);
		return false;
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (Core.ML && IsRewardItem)
			list.Add(RewardSystem.GetRewardYearLabel(this, new object[] { Hue, m_LabelNumber })); // X Year Veteran Reward
	}

	public override bool CanEquip(Mobile m)
	{
		return !base.CanEquip(m) ? false : !IsRewardItem || RewardSystem.CheckIsUsableBy(m, this, new object[] { Hue, m_LabelNumber });
	}

	[Constructable]
	public RewardCloak() : this(0)
	{
	}

	[Constructable]
	public RewardCloak(int hue) : this(hue, 0)
	{
	}

	[Constructable]
	public RewardCloak(int hue, int labelNumber) : base(0x1515, hue)
	{
		Weight = 5.0;
		LootType = LootType.Blessed;
		m_LabelNumber = labelNumber;
	}

	public RewardCloak(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write(m_LabelNumber);
		writer.Write(IsRewardItem);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		int version = reader.ReadInt();
		switch (version)
		{
			case 0:
				{
					m_LabelNumber = reader.ReadInt();
					IsRewardItem = reader.ReadBool();
					break;
				}
		}
		if (Parent is Mobile mobile)
			mobile.VirtualArmorMod += 2;
	}
}
