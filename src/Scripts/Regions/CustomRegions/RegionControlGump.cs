using Server.Commands;
using Server.Items;
using Server.Network;

namespace Server.Gumps;

public class RegionControlGump : Gump
{
	private readonly RegionControl _mController;
	public RegionControlGump(RegionControl r) : base(25, 50)
	{
		_mController = r;

		Closable = true;
		Dragable = true;
		Resizable = false;

		AddPage(0);
		//x, y, width, high
		AddBackground(23, 32, 412, 186, 9270);
		AddAlphaRegion(19, 29, 418, 193);

		AddLabel(55, 60, 1152, "CUSTOM REGIONS For UO Moons");

		AddLabel(75, 90, 1152, "Add Region Area");
		AddButton(55, 92, 0x845, 0x846, 3, GumpButtonType.Reply, 0);

		AddLabel(75, 110, 1152, "Edit Restricted Spells");
		AddButton(55, 112, 0x845, 0x846, 1, GumpButtonType.Reply, 0);

		AddLabel(75, 130, 1152, "Edit Restricted Skills");
		AddButton(55, 132, 0x845, 0x846, 2, GumpButtonType.Reply, 0);

		AddLabel(75, 150, 1152, "Edit Other Properties");
		AddButton(55, 152, 0x845, 0x846, 4, GumpButtonType.Reply, 0);

		AddLabel(75, 170, 1152, "See Region Bounds");
		AddButton(55, 172, 0x845, 0x846, 5, GumpButtonType.Reply, 0);

		AddImage(353, 54, 3953);
		AddImage(353, 180, 3955);
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_mController == null || _mController.Deleted)
		{
			return;
		}

		Mobile m = sender.Mobile;
		string prefix = CommandSystem.Prefix;

		switch (info.ButtonID)
		{
			case 1:
			{
				//m_Controller.SendRestrictGump( m, RestrictType.Spells );
				m.CloseGump(typeof(SpellRestrictGump));
				m.SendGump(new SpellRestrictGump(_mController.RestrictedSpells));

				m.CloseGump(typeof(RegionControlGump));
				m.SendGump(new RegionControlGump(_mController));
				break;
			}
			case 2:
			{
				//m_Controller.SendRestrictGump( m, RestrictType.Skills );

				m.CloseGump(typeof(SkillRestrictGump));
				m.SendGump(new SkillRestrictGump(_mController.RestrictedSkills));

				m.CloseGump(typeof(RegionControlGump));
				m.SendGump(new RegionControlGump(_mController));
				break;
			}
			case 3:
			{
				m.CloseGump(typeof(RegionControlGump));
				m.SendGump(new RegionControlGump(_mController));

				m.CloseGump(typeof(RemoveAreaGump));
				m.SendGump(new RemoveAreaGump(_mController));

				_mController.ChooseArea(m);
				break;
			}
			case 4:
			{
				m.SendGump(new PropertiesGump(m, _mController));
				m.CloseGump(typeof(RegionControlGump));
				m.SendGump(new RegionControlGump(_mController));
				m.CloseGump(typeof(RemoveAreaGump));
				m.SendGump(new RemoveAreaGump(_mController));
				break;
			}
			case 5:
			{
				CommandSystem.Handle(m, $"{prefix}RegionBounds");
				m.CloseGump(typeof(RegionControlGump));
				m.SendGump(new RegionControlGump(_mController));
				m.CloseGump(typeof(RemoveAreaGump));
				m.SendGump(new RemoveAreaGump(_mController));
				break;
			}
		}
	}
}
