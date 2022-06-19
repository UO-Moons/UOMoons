using Server.Network;

namespace Server.Items
{
	[Flipable(0x1766, 0x1768)]
	public class Cloth : BaseItem, IScissorable, IDyable, ICommodity
	{
		[Constructable]
		public Cloth()
			: this(1)
		{
		}

		[Constructable]
		public Cloth(int amount)
			: base(0x1766)
		{
			Stackable = true;
			Amount = amount;
		}

		public Cloth(Serial serial)
			: base(serial)
		{
		}

		public override double DefaultWeight => 0.1;
		TextDefinition ICommodity.Description => LabelNumber;
		bool ICommodity.IsDeedable => true;
		public bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			Hue = sender.DyedHue;

			return true;
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

		public override void OnSingleClick(Mobile from)
		{
			int number = (Amount == 1) ? 1049124 : 1049123;

			from.Send(new MessageLocalized(Serial, ItemId, MessageType.Regular, 0x3B2, 3, number, "", Amount.ToString()));
		}

		public bool Scissor(Mobile from, Scissors scissors)
		{
			if (Deleted || !from.CanSee(this))
			{
				return false;
			}

			base.ScissorHelper(from, new Bandage(), 1);

			return true;
		}
	}

	public class CutUpCloth : BaseItem
	{
		public override int LabelNumber => 1044458;  // cut-up cloth

		[Constructable]
		public CutUpCloth()
			: base(0x1767)
		{
		}

		public CutUpCloth(Serial serial)
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
			_ = reader.ReadInt();
		}

		public void CutUp(Mobile from, Item[] items)
		{
			//Container backpack = from.Backpack;

			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] is BoltOfCloth boc)
				{
					boc.Scissor(from, null);
				}
			}
		}
	}

	public class CombineCloth : BaseItem
	{
		public override int LabelNumber => 1044459;  // combine cloth

		[Constructable]
		public CombineCloth()
			: base(0x1767)
		{
		}

		public CombineCloth(Serial serial)
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
			_ = reader.ReadInt();
		}

		public static bool CheckHue(int hue, int[] hues, out int count)
		{
			int result = 0;
			bool success = true;

			for (int i = 0; i < hues.Length; i++)
			{
				if (hues[i] == hue)
				{
					result = i;
					success = false;
				}
			}

			count = result;

			return success;
		}

		public static void Combine(Mobile from, Item[] items)
		{
			Container backpack = from.Backpack;

			int[] hues = new int[backpack.Items.Count];
			int[] amounts = new int[backpack.Items.Count];

			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] is Cloth c)
				{

					if (CheckHue(c.Hue, hues, out int count))
					{
						hues[i] = c.Hue;
						amounts[i] = c.Amount;
					}
					else
					{
						amounts[count] += c.Amount;
					}

					c.Delete();
				}
			}

			for (int i = 0; i < hues.Length; i++)
			{
				Cloth cloth = new Cloth
				{
					Hue = hues[i],
					Amount = amounts[i]
				};

				if (cloth.Amount > 0)
				{
					backpack.DropItem(cloth);
				}
				else
				{
					cloth.Delete();
				}
			}
		}
	}
}
