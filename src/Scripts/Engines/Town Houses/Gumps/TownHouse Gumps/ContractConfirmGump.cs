namespace Server.Engines.TownHouses;

public class ContractConfirmGump : GumpPlusLight
{
	private readonly RentalContract _cContract;

	public ContractConfirmGump(Mobile m, RentalContract rc) : base(m, 100, 100)
	{
		m.CloseGump(typeof(ContractConfirmGump));

		_cContract = rc;
	}

	protected override void BuildGump()
	{
		const int width = 300;
		var y = 0;

		if (_cContract.RentalClient == null)
		{
			AddHtml(0, 0 + 5, width, Html.Black + "<CENTER>Rent this House?");
		}
		else
		{
			AddHtml(0, 0 + 5, width, Html.Black + "<CENTER>Rental Agreement");
		}

		string text = string.Format("  I, {0}, agree to rent this property from {1} for the sum of {2} every {3}.  " +
		                            "The funds for this payment will be taken directly from my bank.  In the case where " +
		                            "I cannot pay this fee, the property will return to {1}.  I may cancel this agreement at any time by " +
		                            "demolishing the property.  {1} may also cancel this agreement at any time by either demolishing their " +
		                            "property or canceling the contract, in which case your security deposit will be returned.",
			_cContract.RentalClient == null ? "_____" : _cContract.RentalClient.Name,
			_cContract.RentalMaster.Name,
			_cContract.Free ? 0 : _cContract.Price,
			_cContract.PriceTypeShort.ToLower());

		text += "<BR>   Here is some more info reguarding this property:<BR>";

		text += $"<CENTER>Lockdowns: {_cContract.Locks}<BR>";
		text += $"Secures: {_cContract.Secures}<BR>";
		text += $"Floors: {((_cContract.MaxZ - _cContract.MinZ < 200) ? ((_cContract.MaxZ - _cContract.MinZ) / 20) + 1 : 1)}<BR>";
		text += $"Space: {_cContract.CalcVolume()} cubic units";

		AddHtml(40, y += 30, width - 60, 200, Html.Black + text, false, true);

		y += 200;

		if (_cContract.RentalClient == null)
		{
			AddHtml(60, y += 20, 60, Html.Black + "Preview");
			AddButton(40, y + 3, 0x837, 0x838, "Preview", Preview);

			var locsec = _cContract.ValidateLocSec();

			if (Owner != _cContract.RentalMaster && locsec)
			{
				AddHtml(width - 100, y, 60, Html.Black + "Accept");
				AddButton(width - 60, y + 3, 0x232C, 0x232D, "Accept", Accept);
			}
			else
			{
				AddImage(width - 60, y - 10, 0x232C);
			}

			if (!locsec)
			{
				Owner.SendMessage(Owner == _cContract.RentalMaster ? "You don't have the lockdowns or secures available for this contract." : "The owner of this contract cannot rent this property at this time.");
			}
		}
		else
		{
			if (Owner == _cContract.RentalMaster)
			{
				AddHtml(60, y += 20, 100, Html.Black + "Cancel Contract");
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
		_cContract.ClearPreview();
	}

	private void Preview()
	{
		_cContract.ShowAreaPreview(Owner);
		NewGump();
	}

	private void CancelContract()
	{
		if (Owner == _cContract.RentalClient)
		{
			_cContract.House.Delete();
		}
		else
		{
			_cContract.Delete();
		}
	}

	private void Accept()
	{
		if (!_cContract.ValidateLocSec())
		{
			Owner.SendMessage("The owner of this contract cannot rent this property at this time.");
			return;
		}

		_cContract.Purchase(Owner);

		if (!_cContract.Owned)
		{
			return;
		}

		_cContract.Visible = true;
		_cContract.RentalClient = Owner;
		_cContract.RentalClient.AddToBackpack(new RentalContractCopy(_cContract));
	}
}
