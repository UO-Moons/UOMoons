namespace Server.Engines.TownHouses
{
	public class RentalLicense : BaseItem
	{
		private Mobile m_COwner;

		public Mobile Owner
		{
			get => m_COwner;
			set
			{
				m_COwner = value;
				InvalidateProperties();
			}
		}

		public RentalLicense() : base(0x14F0)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			if (m_COwner != null)
			{
				list.Add("a renter's license belonging to " + m_COwner.Name);
			}
			else
			{
				list.Add("a renter's license");
			}
		}

		public override void OnDoubleClick(Mobile m)
		{
			m_COwner ??= m;
		}

		public RentalLicense(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(m_COwner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();

			m_COwner = reader.ReadMobile();
		}
	}
}
