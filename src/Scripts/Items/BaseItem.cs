using Server.Engines.Craft;
using Server.Items;
using System;

namespace Server
{
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
			Crafter = 0x00000008
		}

		private bool m_Identified;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Identified { get => m_Identified; set { m_Identified = value; InvalidateProperties(); } }

		private ItemQuality m_Quality = ItemQuality.Normal;
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual ItemQuality Quality { get => m_Quality; set { m_Quality = value; InvalidateProperties(); } }

		private string m_EngravedText;
		[CommandProperty(AccessLevel.GameMaster)]
		public string EngravedText { get => m_EngravedText; set { m_EngravedText = value; InvalidateProperties(); } }

		private bool m_PlayerConstructed;
		[CommandProperty(AccessLevel.GameMaster)]
		public bool PlayerConstructed { get => m_PlayerConstructed; set { m_PlayerConstructed = value; InvalidateProperties(); } }

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

		public BaseItem()
		{
			Quality = ItemQuality.Normal;
		}

		public BaseItem(int itemID) : base(itemID)
		{
		}

		public BaseItem(Serial serial) : base(serial)
		{
		}

		public virtual string GetNameString()
		{
			string name = Name;

			if (name == null)
				name = string.Format("#{0}", LabelNumber);

			return name;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			SaveFlag flags = SaveFlag.None;

			Utility.SetSaveFlag(ref flags, SaveFlag.Identified, m_Identified != false);
			Utility.SetSaveFlag(ref flags, SaveFlag.Quality, m_Quality != ItemQuality.Normal);
			Utility.SetSaveFlag(ref flags, SaveFlag.EngravedText, !string.IsNullOrEmpty(m_EngravedText));
			Utility.SetSaveFlag(ref flags, SaveFlag.PlayerConstructed, PlayerConstructed != false);
			Utility.SetSaveFlag(ref flags, SaveFlag.Crafter, m_Crafter != null);

			writer.WriteEncodedInt((int)flags);

			if (flags.HasFlag(SaveFlag.Identified))
				writer.Write(m_Identified);

			if (flags.HasFlag(SaveFlag.Quality))
				writer.WriteEncodedInt((int)m_Quality);

			if (flags.HasFlag(SaveFlag.EngravedText))
				writer.Write(m_EngravedText);

			if (flags.HasFlag(SaveFlag.Crafter))
				writer.Write(m_Crafter);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
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

						break;
					}
			}
		}
	}
}
