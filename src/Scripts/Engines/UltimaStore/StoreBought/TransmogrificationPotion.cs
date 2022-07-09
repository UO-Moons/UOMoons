using Server.Gumps;
using Server.Network;
using Server.Targeting;

namespace Server.Items;

public class TransmogrificationPotion : BaseItem
{
	public override int LabelNumber => 1159501;  // Transmogrification Potion
        
	[CommandProperty(AccessLevel.GameMaster)]
	public Item SourceObject { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Item DestinationObject { get; set; }

	[Constructable]
	public TransmogrificationPotion()
		: base(0xA1E9)
	{
		Hue = 2741;
	}

	public TransmogrificationPotion(Serial serial)
		: base(serial)
	{
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.CloseGump(typeof(TransmogrificationPotionGump));
			from.SendGump(new TransmogrificationPotionGump(this));
		}
		else
		{
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
		}
	}

	//public bool CheckMagicalItem(Item item)
	//{
		//return Mannequin.GetProperty(item).Any(x => x.Value != 0);
	//}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		list.Add(1159503); // Robe Slot
	}

	public class TransmogrificationPotionGump : Gump
	{
		private readonly TransmogrificationPotion _item;

		public TransmogrificationPotionGump(TransmogrificationPotion item)
			: base(100, 100)
		{
			_item = item;

			AddPage(0);

			AddBackground(0, 0, 370, 520, 0x6DB);
			AddHtmlLocalized(85, 10, 200, 20, 1114513, "#1159501", 0x67D5, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
			AddItem(160, 50, 0x9D83);
			AddItem(145, 20, 0x376F);
			AddHtmlLocalized(10, 150, 350, 180, 1114513, "#1159496", 0x43FF, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
			AddButton(10, 339, 0x15E1, 0x15E5, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(35, 339, 150, 20, 1159494, 0x7FFF, false, false); // Set Source Object
			AddButton(185, 339, 0x15E1, 0x15E5, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(210, 339, 200, 20, 1159495, 0x7FFF, false, false); // Set Destination Object

			if (_item.SourceObject != null)
			{
				AddItem(50, 375, _item.SourceObject.ItemId, _item.SourceObject.Hue);
				AddItemProperty(_item.SourceObject.Serial);
			}

			if (_item.DestinationObject != null)
			{
				AddItem(250, 375, _item.DestinationObject.ItemId, _item.DestinationObject.Hue);
				AddItemProperty(_item.DestinationObject.Serial);
			}

			AddButton(150, 465, 0x47B, 0x47C, 3, GumpButtonType.Reply, 0);
			AddHtmlLocalized(137, 445, 100, 18, 1114513, "#1159497", 0x7E00, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
		}

		private class InternalTarget : Target
		{
			private readonly TransmogrificationPotion _item;
			private readonly bool _isSource;

			public InternalTarget(TransmogrificationPotion item, bool source)
				: base(12, true, TargetFlags.None)
			{
				_item = item;
				_isSource = source;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (_item.Deleted)
					return;

				if (!_item.IsChildOf(from.Backpack))
				{
					from.SendLocalizedMessage(1060640); // The item must be in your backpack to use it.
					return;
				}

				if (targeted is Item {Layer: Layer.OuterTorso} i)
				{
					if (!i.IsChildOf(from.Backpack))
					{
						from.SendLocalizedMessage(1080058); // This must be in your backpack to use it.
						return;
					}

					if (_item.DestinationObject != null && _item.SourceObject != null && _item.DestinationObject == _item.SourceObject)
					{
						from.SendLocalizedMessage(1159518); // You may not set the source and destination objects to the same object!
						return;
					}

					if (_isSource)
					{
						_item.SourceObject = i;
					}
					else
					{
						//if (_item.CheckMagicalItem(i))
						//{
						//	from.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
						//	return;
						//}
						//else
						//{
							_item.DestinationObject = i;
						//}
					}

					from.CloseGump(typeof(TransmogrificationPotionGump));
					from.SendGump(new TransmogrificationPotionGump(_item));
				}
				else
				{
					from.SendLocalizedMessage(1159500); // That is not a valid robe-slot item.
				}
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (_item.Deleted)
				return;

			Mobile m = sender.Mobile;

			switch (info.ButtonID)
			{
				case 0:
					break;
				case 1:
					m.SendLocalizedMessage(1159498); // Target the object that you wish to transfer properties FROM...
					m.Target = new InternalTarget(_item, true);
					break;
				case 2:
					m.SendLocalizedMessage(1159499); // Target the object you wish to transfer properties TO...
					m.Target = new InternalTarget(_item, false);
					break;
				case 3:
					if (!_item.IsChildOf(m.Backpack))
					{
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					if (_item.SourceObject == null || _item.DestinationObject == null)
					{
						m.SendLocalizedMessage(1159500); // That is not a valid robe-slot item.
						return;
					}

					if (!_item.SourceObject.IsChildOf(m.Backpack))
					{
						_item.SourceObject = null;
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					if (!_item.DestinationObject.IsChildOf(m.Backpack))
					{
						_item.DestinationObject = null;
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					//if (_Item.CheckMagicalItem(_Item.DestinationObject))
					//{
					//	m.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
					//	return;
					//}

					m.CloseGump(typeof(TransmogrificationPotionGump));
					m.SendGump(new TransmogrificationPotionGump(_item));

					m.CloseGump(typeof(TransmogrificationPotionConfirmGump));
					m.SendGump(new TransmogrificationPotionConfirmGump(_item));

					break;
			}
		}
	}

	public class TransmogrificationPotionConfirmGump : Gump
	{
		private readonly TransmogrificationPotion _item;

		public TransmogrificationPotionConfirmGump(TransmogrificationPotion item)
			: base(100, 100)
		{
			_item = item;

			AddPage(0);

			AddBackground(0, 0, 320, 245, 0x6DB);
			AddHtmlLocalized(65, 10, 200, 20, 1114513, "#1159501", 0x67D5, false, false); // <DIV ALIGN=CENTER>~1_TOKEN~</DIV>
			AddHtmlLocalized(15, 50, 295, 140, 1159502, 0x72ED, false, false); // You are about to transmogrify the items you have selected. The source object will be destroyed and the destination object will take on the properties of the source object.  Blessed status will be retained.  Are you sure you wish to proceed?  This process is final and cannot be undone.
			AddButton(30, 200, 0x867, 0x869, 1, GumpButtonType.Reply, 0);
			AddButton(265, 200, 0x867, 0x869, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(33, 180, 100, 50, 1046362, 0x7FFF, false, false); // Yes
			AddHtmlLocalized(273, 180, 100, 50, 1046363, 0x7FFF, false, false); // No
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			Mobile m = sender.Mobile;

			switch (info.ButtonID)
			{
				case 0:
				{
					break;
				}
				case 1:
				{
					if (!_item.IsChildOf(m.Backpack))
					{
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					if (_item.SourceObject == null || _item.DestinationObject == null)
					{
						m.SendLocalizedMessage(1159500); // That is not a valid robe-slot item.
						return;
					}

					if (!_item.SourceObject.IsChildOf(m.Backpack))
					{
						_item.SourceObject = null;
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					if (!_item.DestinationObject.IsChildOf(m.Backpack))
					{
						_item.DestinationObject = null;
						m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						return;
					}

					//if (_Item.CheckMagicalItem(_Item.DestinationObject))
					//{
					//	m.SendLocalizedMessage(1159504); // The destination item must be free of any magical properties.
					//	return;
					//}

					m.CloseGump(typeof(TransmogrificationPotionGump));

					m.PlaySound(491);

					_item.SourceObject.ItemId = _item.DestinationObject.ItemId;
					_item.SourceObject.Hue = _item.DestinationObject.Hue;
					_item.SourceObject.LootType = _item.DestinationObject.LootType;
					_item.SourceObject.Insured = _item.DestinationObject.Insured;

					_item.DestinationObject.Delete();
					_item.Delete();

					break;
				}
			}
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
		reader.ReadInt();
	}
}
