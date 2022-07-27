using Server.Items;
using System;

namespace Server
{
	public enum BookQuality
	{
		Regular,
		Exceptional
	}
	public abstract partial class BaseItem : Item, IEngravable
	{
		[Flags]
		private enum SaveFlag
		{
			None = 0x00000000,
			Identified = 0x00000001,
			Quality = 0x00000002,
			EngravedText = 0x00000004,
			PlayerConstructed = 0x00000006,
			Crafter = 0x00000008,
			Resource = 0x00000010,
			Bookquality = 0x00000020
		}

		private bool m_Identified;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Identified { get => m_Identified; set { m_Identified = value; InvalidateProperties(); } }

		private ItemQuality m_Quality = ItemQuality.Normal;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual ItemQuality Quality { get => m_Quality; set { m_Quality = value; InvalidateProperties(); } }

		private BookQuality m_BookQuality = BookQuality.Regular;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual BookQuality Bookquality { get => m_BookQuality; set { m_BookQuality = value; InvalidateProperties(); } }

		private string m_EngravedText;
		[CommandProperty(AccessLevel.GameMaster)]
		public string EngravedText { get => m_EngravedText; set { m_EngravedText = value; InvalidateProperties(); } }

		private bool m_PlayerConstructed;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed { get => m_PlayerConstructed; set { m_PlayerConstructed = value; InvalidateProperties(); } }

		private bool m_Begged;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Begged { get => m_Begged; init { m_Begged = value; InvalidateProperties(); } }

		private Mobile m_Crafter;
		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Crafter
		{
			get => m_Crafter;
			set
			{
				m_Crafter = value;
				InvalidateProperties();
			}
		}

		private CraftResource m_Resource;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual CraftResource Resource
		{
			get => m_Resource;
			set { m_Resource = value; Hue = CraftResources.GetHue(m_Resource); InvalidateProperties(); }
		}

		public virtual int Lifespan => 0;
		public virtual bool UseSeconds => true;
		[CommandProperty(AccessLevel.GameMaster)]
		public DecayingItemSocket DecayInfo => GetSocket<DecayingItemSocket>();

		[CommandProperty(AccessLevel.GameMaster)]
		public int TimeLeft
		{
			get
			{
				var socket = GetSocket<DecayingItemSocket>();

				return socket?.Remaining ?? 0;
			}
			set
			{
				var socket = GetSocket<DecayingItemSocket>();

				if (socket != null)
				{
					socket.Expires = DateTime.UtcNow + TimeSpan.FromSeconds(value);
				}
				else if (value > 0)
				{
					AttachSocket(new DecayingItemSocket(value, UseSeconds));
				}

				InvalidateProperties();
			}
		}

		public BaseItem()
		{
			Quality = ItemQuality.Normal;
			Crafter = null;

			if (Lifespan > 0)
			{
				AttachSocket(new DecayingItemSocket(Lifespan, UseSeconds));
			}
		}

		public BaseItem(int itemId) : base(itemId)
		{
		}

		public BaseItem(Serial serial) : base(serial)
		{
		}

		public virtual void Decay()
		{
			if (RootParent is Mobile parent)
			{
				if (Name == null)
					parent.SendLocalizedMessage(1072515, "#" + LabelNumber); // The ~1_name~ expired...
				else
					parent.SendLocalizedMessage(1072515, Name); // The ~1_name~ expired...

				Effects.SendLocationParticles(EffectItem.Create(parent.Location, parent.Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
				Effects.PlaySound(parent.Location, parent.Map, 0x201);
			}
			else
			{
				Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 8, 20, 5042);
				Effects.PlaySound(Location, Map, 0x201);
			}

			Delete();
		}

		public virtual void SendTimeRemainingMessage(Mobile to)
		{
			var socket = GetSocket<DecayingItemSocket>();

			if (socket != null && socket.Expires > DateTime.UtcNow)
			{
				to.SendLocalizedMessage(1072516,
					$"{Name ?? $"#{LabelNumber}"}\t{socket.Remaining}"); // ~1_name~ will expire in ~2_val~ seconds!
			}
		}

		public virtual string GetNameString()
		{
			string name = Name ?? $"#{LabelNumber}";

			return name;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
			writer.Write(m_Begged);
			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified);
			Utility.SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);
			Utility.SetSaveFlag(ref flags, SaveFlag.Bookquality, m_BookQuality != BookQuality.Regular);
			Utility.SetSaveFlag(ref flags, SaveFlag.EngravedText, !string.IsNullOrEmpty(m_EngravedText));
			Utility.SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, PlayerConstructed);
			Utility.SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);
			Utility.SetSaveFlag(ref flags, SaveFlag.Resource, m_Resource != CraftResource.Iron);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Identified))
				writer.Write(m_Identified);

			if (flags.HasFlag(SaveFlag.Quality))
				writer.WriteEncodedInt((int)m_Quality);

			if (flags.HasFlag(SaveFlag.EngravedText))
				writer.Write(m_EngravedText);

			if (flags.HasFlag(SaveFlag.Crafter))
				writer.Write(m_Crafter);

			if (flags.HasFlag(SaveFlag.Resource))
				writer.Write((int)m_Resource);

			if (flags.HasFlag(SaveFlag.Bookquality))
				writer.WriteEncodedInt((int)m_BookQuality);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Begged = reader.ReadBool();
						SaveFlag flags = (SaveFlag)reader.ReadEncodedInt();

						if (flags.HasFlag(SaveFlag.Identified))
							m_Identified = true;

						if (flags.HasFlag(SaveFlag.Quality))
							m_Quality = (ItemQuality)reader.ReadEncodedInt();
						else
							m_Quality = ItemQuality.Normal;

						if (flags.HasFlag(SaveFlag.EngravedText))
							m_EngravedText = reader.ReadString();

						if (flags.HasFlag(SaveFlag.PlayerConstructed))
						{
							PlayerConstructed = true;
						}

						if (flags.HasFlag(SaveFlag.Crafter))
							m_Crafter = reader.ReadMobile();

						if (flags.HasFlag(SaveFlag.Resource))
							m_Resource = (CraftResource)reader.ReadInt();
						else
							m_Resource = CraftResource.Iron;

						if (flags.HasFlag(SaveFlag.Bookquality))
							m_BookQuality = (BookQuality)reader.ReadEncodedInt();
						else
							m_BookQuality = BookQuality.Regular;

						break;
					}
			}
		}
	}
}
