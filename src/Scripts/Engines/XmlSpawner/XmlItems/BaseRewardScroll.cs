namespace Server.Items
{
	public abstract class BaseRewardScroll : Item
	{
		public override double DefaultWeight => 0.0;

		public BaseRewardScroll() : base(0x2D51)
		{
		}

		public BaseRewardScroll(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer); writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	public class RewardScrollDeed : Item
	{
		[Constructable]
		public RewardScrollDeed(int amount)
		{
		}

		[Constructable]
		public RewardScrollDeed() : this(1)
		{
			ItemID = 5360;
			Movable = true;
			Hue = 1165;
			Name = "Reward Scroll Deed";

		}

		public override void OnDoubleClick(Mobile from)
		{
			from.AddToBackpack(new RewardScroll());
			Delete();
		}
		public RewardScrollDeed(Serial serial) : base(serial)
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

	public class RewardScroll : BaseRewardScroll
	{
		[Constructable]
		public RewardScroll()
		{
			Stackable = true;
			Name = "Reward Scroll";
			Hue = 1165;
			LootType = LootType.Blessed;

		}

		public RewardScroll(Serial serial) : base(serial)
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
