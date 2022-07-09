using System;
using System.Collections.Generic;
using Server.Engines.Craft;

namespace Server.Items
{
	public class FurnitureContainer : BaseContainer, IResource, IQuality
	{
		private Mobile _crafter;
		private CraftResource _resource;
		private ItemQuality _quality;
		private bool _playerConstructed;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => _crafter;
			set
			{
				_crafter = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public ItemQuality Quality
		{
			get => _quality;
			set { _quality = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public CraftResource Resource
		{
			get => _resource;
			set
			{
				_resource = value;
				Hue = CraftResources.GetHue(_resource);
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed
		{
			get => _playerConstructed;
			set
			{
				_playerConstructed = value;
				InvalidateProperties();
			}
		}

		public FurnitureContainer(int id) : base(id)
		{
		}

		public override void AddCraftedProperties(ObjectPropertyList list)
		{
			if (_crafter != null)
			{
				list.Add(1050043, _crafter.Name); // crafted by ~1_NAME~
			}

			if (Quality == ItemQuality.Exceptional)
			{
				list.Add(1060636); // Exceptional
			}

			if (_resource > CraftResource.Iron)
			{
				list.Add(1114057, "#{0}", CraftResources.GetLocalizationNumber(_resource)); // ~1_val~
			}
		}

		public int OnCraft(int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, ITool tool, CraftItem craftItem, int resHue)
		{
			PlayerConstructed = true;

			Quality = (ItemQuality)quality;

			if (makersMark)
			{
				Crafter = from;
			}

			if (!craftItem.ForceNonExceptional)
			{
				typeRes ??= craftItem.Resources.GetAt(0).ItemType;

				Resource = CraftResources.GetFromType(typeRes);
			}

			return quality;
		}

		public FurnitureContainer(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);

			writer.Write(_playerConstructed);
			writer.Write((int)_resource);
			writer.Write((int)_quality);
			writer.Write(_crafter);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					_playerConstructed = reader.ReadBool();
					_resource = (CraftResource)reader.ReadInt();
					_quality = (ItemQuality)reader.ReadInt();
					_crafter = reader.ReadMobile();
					break;
			}
		}
	}

	[Furniture]
	[Flipable(0x2815, 0x2816)]
	public class TallCabinet : BaseContainer
	{
		[Constructable]
		public TallCabinet() : base(0x2815)
		{
			Weight = 1.0;
		}

		public TallCabinet(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0x2817, 0x2818)]
	public class ShortCabinet : BaseContainer
	{
		[Constructable]
		public ShortCabinet() : base(0x2817)
		{
			Weight = 1.0;
		}

		public ShortCabinet(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}


	[Furniture]
	[Flipable(0x2857, 0x2858)]
	public class RedArmoire : BaseContainer
	{
		[Constructable]
		public RedArmoire() : base(0x2857)
		{
			Weight = 1.0;
		}

		public RedArmoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0x285D, 0x285E)]
	public class CherryArmoire : BaseContainer
	{
		[Constructable]
		public CherryArmoire() : base(0x285D)
		{
			Weight = 1.0;
		}

		public CherryArmoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0x285B, 0x285C)]
	public class MapleArmoire : BaseContainer
	{
		[Constructable]
		public MapleArmoire() : base(0x285B)
		{
			Weight = 1.0;
		}

		public MapleArmoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0x2859, 0x285A)]
	public class ElegantArmoire : BaseContainer
	{
		[Constructable]
		public ElegantArmoire() : base(0x2859)
		{
			Weight = 1.0;
		}

		public ElegantArmoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0xa97, 0xa99, 0xa98, 0xa9a, 0xa9b, 0xa9c)]
	public class FullBookcase : BaseContainer
	{
		[Constructable]
		public FullBookcase() : base(0xA97)
		{
			Weight = 1.0;
		}

		public FullBookcase(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0xa9d, 0xa9e)]
	public class EmptyBookcase : BaseContainer
	{
		[Constructable]
		public EmptyBookcase() : base(0xA9D)
		{
		}

		public EmptyBookcase(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0xa2c, 0xa34)]
	public class Drawer : BaseContainer
	{
		[Constructable]
		public Drawer() : base(0xA2C)
		{
			Weight = 1.0;
		}

		public Drawer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0xa30, 0xa38)]
	public class FancyDrawer : BaseContainer
	{
		[Constructable]
		public FancyDrawer() : base(0xA30)
		{
			Weight = 1.0;
		}

		public FancyDrawer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();
		}
	}

	[Furniture]
	[Flipable(0xa4f, 0xa53)]
	public class Armoire : BaseContainer
	{
		[Constructable]
		public Armoire() : base(0xA4F)
		{
			Weight = 1.0;
		}

		public override void DisplayTo(Mobile m)
		{
			if (DynamicFurniture.Open(this, m))
				base.DisplayTo(m);
		}

		public Armoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			DynamicFurniture.Close(this);
		}
	}

	[Furniture]
	[Flipable(0xa4d, 0xa51)]
	public class FancyArmoire : BaseContainer
	{
		[Constructable]
		public FancyArmoire() : base(0xA4D)
		{
			Weight = 1.0;
		}

		public override void DisplayTo(Mobile m)
		{
			if (DynamicFurniture.Open(this, m))
				base.DisplayTo(m);
		}

		public FancyArmoire(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			reader.ReadInt();

			DynamicFurniture.Close(this);
		}
	}

	public class DynamicFurniture
	{
		private static readonly Dictionary<Container, Timer> m_Table = new();

		public static bool Open(Container c, Mobile m)
		{
			if (m_Table.ContainsKey(c))
			{
				c.SendRemovePacket();
				Close(c);
				c.Delta(ItemDelta.Update);
				c.ProcessDelta();
				return false;
			}

			if (c is Armoire or FancyArmoire)
			{
				Timer t = new FurnitureTimer(c, m);
				t.Start();
				m_Table[c] = t;

				c.ItemId = c.ItemId switch
				{
					0xA4D => 0xA4C,
					0xA4F => 0xA4E,
					0xA51 => 0xA50,
					0xA53 => 0xA52,
					_ => c.ItemId
				};
			}

			return true;
		}

		public static void Close(Container c)
		{

			m_Table.TryGetValue(c, out Timer t);

			if (t != null)
			{
				t.Stop();
				m_Table.Remove(c);
			}

			if (c is Armoire or FancyArmoire)
			{
				c.ItemId = c.ItemId switch
				{
					0xA4C => 0xA4D,
					0xA4E => 0xA4F,
					0xA50 => 0xA51,
					0xA52 => 0xA53,
					_ => c.ItemId
				};
			}
		}
	}

	public class FurnitureTimer : Timer
	{
		private readonly Container _container;
		private readonly Mobile _mobile;

		public FurnitureTimer(Container c, Mobile m) : base(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(0.5))
		{
			Priority = TimerPriority.TwoFiftyMs;

			_container = c;
			_mobile = m;
		}

		protected override void OnTick()
		{
			if (_mobile.Map != _container.Map || !_mobile.InRange(_container.GetWorldLocation(), 3))
				DynamicFurniture.Close(_container);
		}
	}
}
