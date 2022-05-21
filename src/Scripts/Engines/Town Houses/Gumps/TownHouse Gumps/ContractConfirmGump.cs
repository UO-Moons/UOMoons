namespace Server.Engines.TownHouses
{
	public class ContractConfirmGump : GumpPlusLight
	{
		private readonly RentalContract m_CContract;

		public ContractConfirmGump(Mobile m, RentalContract rc) : base(m, 100, 100)
		{
			m.CloseGump(typeof(ContractConfirmGump));

			m_CContract = rc;
		}

		protected override void BuildGump()
		{
			const int width = 300;
			var y = 0;

			if (m_CContract.RentalClient == null)
			{
				AddHtml(0, y + 5, width, HTML.Black + "<CENTER>Rent this House?");
			}
			else
			{
				AddHtml(0, y + 5, width, HTML.Black + "<CENTER>Rental Agreement");
			}

			string text = string.Format("  I, {0}, agree to rent this property from {1} for the sum of {2} every {3}.  " +
				"The funds for this payment will be taken directly from my bank.  In the case where " +
				"I cannot pay this fee, the property will return to {1}.  I may cancel this agreement at any time by " +
				"demolishing the property.  {1} may also cancel this agreement at any time by either demolishing their " +
				"property or canceling the contract, in which case your security deposit will be returned.",
				m_CContract.RentalClient == null ? "_____" : m_CContract.RentalClient.Name,
				m_CContract.RentalMaster.Name,
				m_CContract.Free ? 0 : m_CContract.Price,
				m_CContract.PriceTypeShort.ToLower());

			text += "<BR>   Here is some more info reguarding this property:<BR>";

			text += $"<CENTER>Lockdowns: {m_CContract.Locks}<BR>";
			text += $"Secures: {m_CContract.Secures}<BR>";
			text += $"Floors: {((m_CContract.MaxZ - m_CContract.MinZ < 200) ? ((m_CContract.MaxZ - m_CContract.MinZ) / 20) + 1 : 1)}<BR>";
			text += $"Space: {m_CContract.CalcVolume()} cubic units";

			AddHtml(40, y += 30, width - 60, 200, HTML.Black + text, false, true);

			y += 200;

			if (m_CContract.RentalClient == null)
			{
				AddHtml(60, y += 20, 60, HTML.Black + "Preview");
				AddButton(40, y + 3, 0x837, 0x838, "Preview", Preview);

				var locsec = m_CContract.ValidateLocSec();

				if (Owner != m_CContract.RentalMaster && locsec)
				{
					AddHtml(width - 100, y, 60, HTML.Black + "Accept");
					AddButton(width - 60, y + 3, 0x232C, 0x232D, "Accept", Accept);
				}
				else
				{
					AddImage(width - 60, y - 10, 0x232C);
				}

				if (!locsec)
				{
					Owner.SendMessage(Owner == m_CContract.RentalMaster ? "You don't have the lockdowns or secures available for this contract." : "The owner of this contract cannot rent this property at this time.");
				}
			}
			else
			{
				if (Owner == m_CContract.RentalMaster)
				{
					AddHtml(60, y += 20, 100, HTML.Black + "Cancel Contract");
					AddButton(40, y + 3, 0x837, 0x838, "Cancel Contract", CancelContract);
				}
				else
				{
					AddImage(width - 60, y += 20, 0x232C);
				}
			}

			AddBackgroundZero(0, 0, width, y + 23, 0x24A4);
		}

		protected override void OnClose()
		{
			m_CContract.ClearPreview();
		}

		private void Preview()
		{
			m_CContract.ShowAreaPreview(Owner);
			NewGump();
		}

		private void CancelContract()
		{
			if (Owner == m_CContract.RentalClient)
			{
				m_CContract.House.Delete();
			}
			else
			{
				m_CContract.Delete();
			}
		}

		private void Accept()
		{
			if (!m_CContract.ValidateLocSec())
			{
				Owner.SendMessage("The owner of this contract cannot rent this property at this time.");
				return;
			}

			m_CContract.Purchase(Owner);

			if (!m_CContract.Owned)
			{
				return;
			}

			m_CContract.Visible = true;
			m_CContract.RentalClient = Owner;
			m_CContract.RentalClient.AddToBackpack(new RentalContractCopy(m_CContract));
		}
	}
}
