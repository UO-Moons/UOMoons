using Server.Engines.Craft;
using Server.Network;

namespace Server.Items
{
	[Flipable(0x13E3, 0x13E4)]
	public class SmithHammer : BaseTool
	{
		public override CraftSystem CraftSystem => DefBlacksmithy.CraftSystem;

		[Constructable]
		public SmithHammer() : base(0x13E3)
		{
			Weight = 8.0;
			Layer = Layer.OneHanded;
			if (!Core.AOS)
			{
				m_Hits = Utility.RandomMinMax(31, 60);
				m_MaxHits = m_Hits;
			}
		}

		[Constructable]
		public SmithHammer(int uses) : base(uses, 0x13E3)
		{
			Weight = 8.0;
			Layer = Layer.OneHanded;
			if (!Core.AOS)
			{
				m_Hits = Utility.RandomMinMax(31, 60);
				m_MaxHits = m_Hits;
			}
		}

		public SmithHammer(Serial serial) : base(serial)
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
			int version = reader.ReadInt();
		}
	}
}
