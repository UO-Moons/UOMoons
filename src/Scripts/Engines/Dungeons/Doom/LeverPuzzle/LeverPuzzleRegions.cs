using System.Collections.Generic;
using Server.Mobiles;
using Server.Regions;

namespace Server.Engines.Doom;

public class LampRoomRegion : BaseRegion
{
	private readonly LeverPuzzleController _controller;

	public LampRoomRegion(LeverPuzzleController controller)
		: base(null, Map.Malas, Find(LeverPuzzleController.LrEnter, Map.Malas), LeverPuzzleController.LrRect)
	{
		_controller = controller;
		Register();
	}

	public static void Initialize()
	{
		EventSink.OnLogin += OnLogin;
	}

	private static void OnLogin(Mobile m)
	{
		Rectangle2D rect = LeverPuzzleController.LrRect;
		if (m.X < rect.X || m.X > rect.X + 10 || m.Y < rect.Y || m.Y > rect.Y + 10 || m.Map != Map.Internal) return;
		Timer kick = new LeverPuzzleController.LampRoomKickTimer(m);
		kick.Start();
	}

	public override void OnEnter(Mobile m)
	{
		if (m is null or WandererOfTheVoid || m.AccessLevel > AccessLevel.Player)
			return;

		if (_controller.Successful != null)
		{
			switch (m)
			{
				case PlayerMobile when m == _controller.Successful:
				case BaseCreature bc when (bc.Controlled && bc.ControlMaster == _controller.Successful) || bc.Summoned:
					return;
			}
		}
		Timer kick = new LeverPuzzleController.LampRoomKickTimer(m);
		kick.Start();
	}

	public override void OnExit(Mobile m)
	{
		if (m != null && m == _controller.Successful)
			_controller.RemoveSuccessful();
	}

	public override void OnDeath(Mobile m)
	{
		if (m == null || m.Deleted || m is WandererOfTheVoid) return;
		Timer kick = new LeverPuzzleController.LampRoomKickTimer(m);
		kick.Start();
	}

	public override bool OnSkillUse(Mobile m, int skill) => _controller.Successful != null && (m.AccessLevel != AccessLevel.Player || m == _controller.Successful);
}

public class LeverPuzzleRegion : BaseRegion
{
	private readonly LeverPuzzleController _controller;
	private Mobile m_Occupant;

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Occupant => m_Occupant is {Alive: true} ? m_Occupant : null;

	public LeverPuzzleRegion(LeverPuzzleController controller, IReadOnlyList<int> loc)
		: base(null, Map.Malas, Find(LeverPuzzleController.LrEnter, Map.Malas), new Rectangle2D(loc[0], loc[1], 1, 1))
	{
		_controller = controller;
		Register();
	}

	public override void OnEnter(Mobile m)
	{
		if (m != null && m_Occupant == null && m is PlayerMobile && m.Alive)
			m_Occupant = m;
	}

	public override void OnExit(Mobile m)
	{
		if (m != null && m == m_Occupant)
			m_Occupant = null;
	}
}
