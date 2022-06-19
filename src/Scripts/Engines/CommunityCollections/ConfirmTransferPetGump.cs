using Server.Mobiles;
using Server.Network;

namespace Server.Gumps;

public class ConfirmTransferPetGump : Gump
{
	private readonly IComunityCollection _mCollection;
	private readonly Point3D _mLocation;
	private readonly BaseCreature _mPet;
	public ConfirmTransferPetGump(IComunityCollection collection, Point3D location, BaseCreature pet)
		: base(50, 50)
	{
		_mCollection = collection;
		_mLocation = location;
		_mPet = pet;

		Closable = true;
		Disposable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);
		AddBackground(0, 0, 270, 120, 0x13BE);

		AddHtmlLocalized(10, 10, 250, 75, 1073105, 0x0, true, false); // <div align=center>Are you sure you wish to transfer this pet away, with no possibility of recovery?</div>
		AddHtmlLocalized(55, 90, 75, 20, 1011011, 0x0, false, false); // CONTINUE
		AddHtmlLocalized(170, 90, 75, 20, 1011012, 0x0, false, false); // CANCEL

		AddButton(20, 90, 0xFA5, 0xFA7, (int)Buttons.Continue, GumpButtonType.Reply, 0);
		AddButton(135, 90, 0xFA5, 0xFA7, (int)Buttons.Cancel, GumpButtonType.Reply, 0);
	}

	private enum Buttons
	{
		Cancel,
		Continue,
	}
	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (_mCollection == null || _mPet == null || _mPet.Deleted || _mPet.ControlMaster != state.Mobile || !state.Mobile.InRange(_mLocation, 2))
			return;

		if (info.ButtonID == (int)Buttons.Continue && state.Mobile is PlayerMobile mobile)
			_mCollection.DonatePet(mobile, _mPet);
	}
}
