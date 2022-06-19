using System.Collections.Generic;

namespace Server.Mobiles
{
	public class TalkingJeweler : TalkingBaseVendor
	{
		private readonly List<SbInfo> m_SBInfos = new();
		protected override List<SbInfo> SbInfos => m_SBInfos;

		[Constructable]
		public TalkingJeweler() : base("the jeweler")
		{
			SetSkill(SkillName.ItemID, 64.0, 100.0);
		}

		public override void InitSbInfo()
		{
			m_SBInfos.Add(new SbJewel());
		}

		public TalkingJeweler(Serial serial) : base(serial)
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
}
