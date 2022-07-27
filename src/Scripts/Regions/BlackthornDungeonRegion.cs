using Server.Mobiles;
using Server.Regions;
using Server.Spells;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Ninjitsu;
using System;
using System.Xml;

namespace Server.Engines.Blackthorn;

public class BlackthornDungeon : DungeonRegion
{
	private static readonly Point3D[] m_RandomLocations =
	{
		new(6459, 2781, 0),
		new(6451, 2781, 0),
		new(6443, 2781, 0),
		new(6409, 2792, 0),
		new(6356, 2781, 0),
		new(6272, 2702, 0),
		new(6272, 2656, 0),
		new(6456, 2623, 0),
	};

	public BlackthornDungeon(XmlElement xml, Map map, Region parent)
		: base(xml, map, parent)
	{
		//Timer.DelayCall(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5), OnTick);
	}
	/*
    public void OnTick()
    {
        if (!Fellowship.ForsakenFoesEvent.Instance.Running)
        {
            foreach (Mobile m in AllPlayers.Where(m => m is PlayerMobile && m.AccessLevel < AccessLevel.Counselor))
            {
                if (m.Hidden)
                    m.RevealingAction();

                if (m.Y > 2575 && m.LastMoveTime + 120000 < Core.TickCount)
                    MoveLocation(m);
            }
        }
    }*/

	public void MoveLocation(Mobile m)
	{
		Point3D p = m_RandomLocations[Utility.Random(m_RandomLocations.Length)];

		m.MoveToWorld(p, Map);

		for (int x = m.X - 1; x <= m.X + 1; x++)
		{
			for (int y = m.Y - 1; y <= m.Y + 1; y++)
			{
				Effects.SendLocationEffect(new Point3D(x, y, m.Z), m.Map, Utility.RandomList(14120, 4518, 14133), 16, 1, 1166, 0);
			}
		}

		Effects.PlaySound(m.Location, m.Map, 0x231);
		m.LocalOverheadMessage(Network.MessageType.Regular, 0x22, 500855); // You are enveloped by a noxious gas cloud!                
		m.ApplyPoison(m, Poison.Lethal);

		IPooledEnumerable eable = Map.GetMobilesInRange(m.Location, 12);

		foreach (Mobile mob in eable)
		{
			if (mob.Combatant != null || mob is not BaseCreature creature || creature.GetMaster() != null ||
			    !creature.CanBeHarmful(m)) continue;
			if (creature.InLOS(m))
				Timer.DelayCall(() => mob.Combatant = m);
			else
				creature.AIObject.MoveTo(creature, true, 1);
		}

		eable.Free();

		m.LastMoveTime = Core.TickCount;
	}

	public override bool CheckTravel(Mobile traveller, Point3D p, TravelCheckType type)
	{
		if (traveller.AccessLevel > AccessLevel.Player)
			return true;

		return type > TravelCheckType.Mark;
	}

	public override void OnDeath(Mobile m)
	{
		//if (m is BaseCreature && Map == Map.Trammel && InvasionController.TramInstance != null)
		//   InvasionController.TramInstance.OnDeath(m as BaseCreature);

		//if (m is BaseCreature && Map == Map.Felucca && InvasionController.FelInstance != null)
		//    InvasionController.FelInstance.OnDeath(m as BaseCreature);
	}
}
