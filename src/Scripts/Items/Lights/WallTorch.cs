using System;

namespace Server.Items
{
	[Flipable]
	public class WallTorch : BaseLight
	{
		public override int LitItemID
		{
			get
			{
				if (ItemId == 0xA05)
					return 0xA07;
				else
					return 0xA0C;
			}
		}

		public override int UnlitItemID
		{
			get
			{
				if (ItemId == 0xA07)
					return 0xA05;
				else
					return 0xA0A;
			}
		}

		[Constructable]
		public WallTorch() : base(0xA05)
		{
			Movable = false;
			Duration = TimeSpan.Zero; // Never burnt out
			Burning = false;
			Light = LightType.WestBig;
			Weight = 3.0;
		}

		public WallTorch(Serial serial) : base(serial)
		{
		}

		public void Flip()
		{
			if (Light == LightType.WestBig)
				Light = LightType.NorthBig;
			else if (Light == LightType.NorthBig)
				Light = LightType.WestBig;

			switch (ItemId)
			{
				case 0xA05: ItemId = 0xA0A; break;
				case 0xA07: ItemId = 0xA0C; break;

				case 0xA0A: ItemId = 0xA05; break;
				case 0xA0C: ItemId = 0xA07; break;
			}
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
