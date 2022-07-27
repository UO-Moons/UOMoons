using System;

namespace Server.Items;

public class CandleSkull : BaseLight
{
	public override int LitItemId
	{
		get
		{
			if (ItemId == 0x1583 || ItemId == 0x1854)
				return 0x1854;

			return 0x1858;
		}
	}

	public override int UnlitItemId
	{
		get
		{
			if (ItemId == 0x1853 || ItemId == 0x1584)
				return 0x1853;

			return 0x1857;
		}
	}

	[Constructable]
	public CandleSkull() : base(0x1853)
	{
		Duration = TimeSpan.Zero;

		Burning = false;
		Light = LightType.Circle150;
		Weight = 5.0;
	}

	public CandleSkull(Serial serial) : base(serial)
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
		reader.ReadInt();
	}
}
