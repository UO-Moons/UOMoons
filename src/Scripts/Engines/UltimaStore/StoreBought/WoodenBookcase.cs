namespace Server.Items;

[Furniture]
[Flipable(0x0A9D, 0x0A9E)]
public class WoodenBookcase : BaseContainer
{
	public override int LabelNumber => 1071102;  // Wooden Bookcase
	public override int DefaultGumpID => 0x4D;

	public bool IsEmpty => Items.Count == 0;

	[CommandProperty(AccessLevel.Decorator)]
	public override int ItemId
	{
		get => base.ItemId;
		set
		{
			if (!IsEmpty && value is 0x0A9D or 0x0A9E)
			{
				base.ItemId = value switch
				{
					0x0A9D => Utility.RandomList(0x0A97, 0x0A98, 0x0A9B),
					0x0A9E => Utility.RandomList(0x0A99, 0x0A9A, 0x0A9C),
					_ => base.ItemId
				};
			}
			else if (IsEmpty && value != 0x0A9D && value != 0x0A9E)
			{
				switch (value)
				{
					case 0x0A97:
					case 0x0A98:
					case 0x0A9B: base.ItemId = 0x0A9D; break;
					case 0x0A99:
					case 0x0A9A:
					case 0x0A9C: base.ItemId = 0x0A9E; break;
				}
			}
			else
			{
				base.ItemId = value;
			}
		}
	}

	[Constructable]
	public WoodenBookcase()
		: base(0x0A9D)
	{
	}

	public override void OnItemAdded(Item item)
	{
		base.OnItemAdded(item);

		if (ItemId is 0x0A9D or 0x0A9E)
		{
			ItemId = ItemId switch
			{
				0x0A9E => Utility.RandomList(0x0A99, 0x0A9A, 0x0A9C),
				_ => Utility.RandomList(0x0A97, 0x0A98, 0x0A9B),
			};
		}
	}

	public override void OnItemRemoved(Item item)
	{
		base.OnItemRemoved(item);

		if (IsEmpty && ItemId != 0x0A9D && ItemId != 0x0A9E)
		{
			base.ItemId = ItemId switch
			{
				0x0A99 or 0x0A9A or 0x0A9C => 0x0A9E,
				_ => 0x0A9D,
			};
		}
	}

	public void Flip()
	{
		ItemId = ItemId switch
		{
			0x0A97 => 0x0A99,
			0x0A98 => 0x0A9A,
			0x0A99 => 0x0A97,
			0x0A9A => 0x0A98,
			0x0A9B => 0x0A9C,
			0x0A9C => 0x0A9B,
			0x0A9D => 0x0A9E,
			0x0A9E => 0x0A9D,
			_ => ItemId
		};
	}

	public WoodenBookcase(Serial serial)
		: base(serial)
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
