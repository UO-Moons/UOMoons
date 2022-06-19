using Server.Mobiles;

namespace Server.Items
{
	public class ParrotPerchAddon : BaseAddon
	{
		[Constructable]
		public ParrotPerchAddon()
			: this(null)
		{
		}

		public ParrotPerchAddon(PetParrot parrot)
		{
			AddComponent(new AddonComponent(0x2FB6), 0, 0, 0);
			Parrot = parrot;
		}

		public ParrotPerchAddon(Serial serial)
			: base(serial)
		{
		}

		public override BaseAddonDeed Deed => new ParrotPerchAddonDeed(Parrot);
		public override bool RetainDeedHue => true;
		[CommandProperty(AccessLevel.GameMaster)]
		public PetParrot Parrot { get; set; }

		public override void OnLocationChange(Point3D oldLocation)
		{
			base.OnLocationChange(oldLocation);

			if (Parrot != null)
				Parrot.Location = new Point3D(X, Y, Z + 12);
		}

		public override void OnMapChange()
		{
			base.OnMapChange();

			if (Parrot != null)
				Parrot.Map = Map;
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Parrot?.Internalize();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Parrot);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			Parrot = reader.ReadMobile() as PetParrot;
		}
	}

	public class ParrotPerchAddonDeed : BaseAddonDeed
	{
		private PetParrot _mParrot;
		private bool _mSafety;
		[Constructable]
		public ParrotPerchAddonDeed()
			: this(null)
		{
		}

		public ParrotPerchAddonDeed(PetParrot parrot)
		{
			LootType = LootType.Blessed;

			_mParrot = parrot;
		}

		public ParrotPerchAddonDeed(Serial serial)
			: base(serial)
		{
		}

		public override int LabelNumber => 1072619;// A deed for a Parrot Perch		
		public override BaseAddon Addon => new ParrotPerchAddon(_mParrot);

		[CommandProperty(AccessLevel.GameMaster)]
		public PetParrot Parrot
		{
			get => _mParrot;
			set
			{
				_mParrot = value;
				InvalidateProperties();
			}
		}
		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (_mParrot == null) return;
			if (_mParrot.Name != null)
				list.Add(1072624, _mParrot.Name); // Includes a pet Parrot named ~1_NAME~
			else
				list.Add(1072620); // Includes a pet Parrot

			int weeks = PetParrot.GetWeeks(_mParrot.Birth);

			switch (weeks)
			{
				case 1:
					list.Add(1072626); // 1 week old
					break;
				case > 1:
					list.Add(1072627, weeks.ToString()); // ~1_AGE~ weeks old
					break;
			}
		}

		public override void DeleteDeed()
		{
			_mSafety = true;

			base.DeleteDeed();
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (!_mSafety && _mParrot != null)
				_mParrot.Delete();

			_mSafety = false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(_mParrot);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			_mParrot = version switch
			{
				0 => reader.ReadMobile() as PetParrot,
				_ => _mParrot
			};
		}
	}
}
