using System.Xml;
using Server.Network;
using Server.Spells;
using Server.Spells.Ninjitsu;

namespace Server.Regions;

public class TwistedWealdDesert : MondainRegion
{
	public TwistedWealdDesert(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{
	}

	public static void Initialize()
	{
		EventSink.OnLogin += Desert_OnLogin;
	}

	public override void OnEnter(Mobile m)
	{
		if (m.NetState != null &&
		    !TransformationSpellHelper.UnderTransformation(m, typeof(AnimalForm)) &&
		    m.AccessLevel < AccessLevel.GameMaster)
			m.SendSpeedControl(SpeedControlType.WalkSpeed);
	}

	public override void OnExit(Mobile m)
	{
		if (m.NetState != null &&
		    !TransformationSpellHelper.UnderTransformation(m, typeof(AnimalForm)) &&
		    (Core.SA || !TransformationSpellHelper.UnderTransformation(m, typeof(Server.Spells.Spellweaving.ReaperFormSpell))))
			m.SendSpeedControl(SpeedControlType.Disable);
	}

	private static void Desert_OnLogin(Mobile m)
	{
		if (m.Region.IsPartOf<TwistedWealdDesert>() && m.AccessLevel < AccessLevel.GameMaster)
			m.SendSpeedControl(SpeedControlType.WalkSpeed);
	}
}
