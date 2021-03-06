using Server.Engines.Champions;
using System;
using System.Linq;

namespace Server.Items;

public sealed class ChampionSkull : BaseItem
{
	public static readonly ChampionSkullType[] Types = //
		Enum.GetValues(typeof(ChampionSkullType))
			.Cast<ChampionSkullType>()
			.Where(o => o != ChampionSkullType.None)
			.ToArray();

	public static ChampionSkullType RandomType => Types[Utility.Random(Types.Length)];

	private ChampionSkullType m_Type;

	[Constructable]
	public ChampionSkull()
		: this(RandomType)
	{ }

	[Constructable]
	public ChampionSkull(ChampionSkullType type)
		: base(0x1AE1)
	{
		m_Type = type;

		LootType = LootType.Cursed;

		Hue = type switch
		{
			ChampionSkullType.Power => 0x159,
			ChampionSkullType.Venom => 0x172,
			ChampionSkullType.Greed => 0x1EE,
			ChampionSkullType.Death => 0x025,
			ChampionSkullType.Pain => 0x035,
			_ => Hue
		};
	}

	public ChampionSkull(Serial serial)
		: base(serial)
	{ }

	[CommandProperty(AccessLevel.GameMaster)]
	public ChampionSkullType Type
	{
		get => m_Type;
		set
		{
			m_Type = value;
			InvalidateProperties();
		}
	}

	public override int LabelNumber => 1049479 + (int)m_Type;

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.Write((int)m_Type);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
		m_Type = (ChampionSkullType)reader.ReadInt();
	}
}
