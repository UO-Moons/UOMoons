namespace Server.Items
{
	public class SimpleMap : MapItem
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public int CurrentPin { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int NPins => Pins != null ? Pins.Count : 0;

		[CommandProperty(AccessLevel.GameMaster)]
		public Point2D PinLocation
		{
			set
			{
				// change the coordinates of the current pin
				if (Pins != null && CurrentPin > 0 && CurrentPin <= Pins.Count)
				{
					ConvertToMap(value.X, value.Y, out int mapx, out int mapy);
					Pins[CurrentPin - 1] = new Point2D(mapx, mapy);
				}
			}
			get
			{
				// get the coordinates of the current pin
				if (Pins != null && CurrentPin > 0 && CurrentPin <= Pins.Count)
				{
					ConvertToWorld(Pins[CurrentPin - 1].X, Pins[CurrentPin - 1].Y, out int mapx, out int mapy);
					return new Point2D(mapx, mapy);
				}
				else
				{
					return Point2D.Zero;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point2D NewPin
		{
			set
			{
				// add a new pin at the specified world coordinate
				AddWorldPin(value.X, value.Y);
				CurrentPin = NPins;
			}
			get
			{
				// return the last pin added to the Pins arraylist
				if (Pins != null && NPins > 0)
				{
					ConvertToWorld(Pins[NPins - 1].X, Pins[NPins - 1].Y, out int mapx, out int mapy);
					return new Point2D(mapx, mapy);
				}
				else
				{
					return Point2D.Zero;
				}
			}
		}


		[CommandProperty(AccessLevel.GameMaster)]
		public bool ClearAllPins
		{
			get => false;
			set { if (value) ClearPins(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PinRemove
		{
			set => RemovePin(value);
			get => 0;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile ShowTo
		{
			set
			{
				if (value != null)
				{
					//DisplayTo(value);
					OnDoubleClick(value);
				}
			}
			get => null;
		}


		[Constructable]
		public SimpleMap()
		{
			SetDisplay(0, 0, 5119, 4095, 400, 400);
		}

		public override int LabelNumber => 1025355;  // map

		public SimpleMap(Serial serial) : base(serial)
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
