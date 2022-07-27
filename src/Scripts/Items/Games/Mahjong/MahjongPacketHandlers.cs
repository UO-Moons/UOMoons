using Server.Network;

namespace Server.Engines.Mahjong;

public delegate void OnMahjongPacketReceive(MahjongGame game, NetState state, PacketReader pvSrc);

public sealed class MahjongPacketHandlers
{
	private static readonly OnMahjongPacketReceive[] m_SubCommandDelegates = new OnMahjongPacketReceive[0x100];

	private static void RegisterSubCommand(int subCmd, OnMahjongPacketReceive onReceive)
	{
		m_SubCommandDelegates[subCmd] = onReceive;
	}

	private static OnMahjongPacketReceive GetSubCommandDelegate(int cmd)
	{
		return cmd is >= 0 and < 0x100 ? m_SubCommandDelegates[cmd] : null;
	}

	public static void Initialize()
	{
		PacketHandlers.Register(0xDA, 0, true, OnPacket);

		RegisterSubCommand(0x6, ExitGame);
		RegisterSubCommand(0xA, GivePoints);
		RegisterSubCommand(0xB, RollDice);
		RegisterSubCommand(0xC, BuildWalls);
		RegisterSubCommand(0xD, ResetScores);
		RegisterSubCommand(0xF, AssignDealer);
		RegisterSubCommand(0x10, OpenSeat);
		RegisterSubCommand(0x11, ChangeOption);
		RegisterSubCommand(0x15, MoveWallBreakIndicator);
		RegisterSubCommand(0x16, TogglePublicHand);
		RegisterSubCommand(0x17, MoveTile);
		RegisterSubCommand(0x18, MoveDealerIndicator);
	}

	private static void OnPacket(NetState state, PacketReader pvSrc)
	{
		MahjongGame game = World.FindItem(pvSrc.ReadSerial()) as MahjongGame;

		if (game != null)
			game.Players.CheckPlayers();

		pvSrc.ReadByte();

		int cmd = pvSrc.ReadByte();

		OnMahjongPacketReceive onReceive = GetSubCommandDelegate(cmd);

		if (onReceive != null)
		{
			onReceive(game, state, pvSrc);
		}
		else
		{
			pvSrc.Trace(state);
		}
	}

	private static MahjongPieceDirection GetDirection(int value)
	{
		return value switch
		{
			0 => MahjongPieceDirection.Up,
			1 => MahjongPieceDirection.Left,
			2 => MahjongPieceDirection.Down,
			_ => MahjongPieceDirection.Right
		};
	}

	private static MahjongWind GetWind(int value)
	{
		return value switch
		{
			0 => MahjongWind.North,
			1 => MahjongWind.East,
			2 => MahjongWind.South,
			_ => MahjongWind.West
		};
	}

	private static void ExitGame(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null)
			return;

		Mobile from = state.Mobile;

		game.Players.LeaveGame(from);
	}

	private static void GivePoints(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			return;

		int to = pvSrc.ReadByte();
		int amount = pvSrc.ReadInt32();

		game.Players.TransferScore(state.Mobile, to, amount);
	}

	private static void RollDice(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			return;

		game.Dices.RollDices(state.Mobile);
	}

	private static void BuildWalls(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		game.ResetWalls(state.Mobile);
	}

	private static void ResetScores(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		game.Players.ResetScores(MahjongGame.BaseScore);
	}

	private static void AssignDealer(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		int position = pvSrc.ReadByte();

		game.Players.AssignDealer(position);
	}

	private static void OpenSeat(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		int position = pvSrc.ReadByte();

		if (game.Players.GetPlayer(position) == state.Mobile)
			return;

		game.Players.OpenSeat(position);
	}

	private static void ChangeOption(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		pvSrc.ReadInt16();
		pvSrc.ReadByte();

		int options = pvSrc.ReadByte();

		game.ShowScores = (options & 0x1) != 0;
		game.SpectatorVision = (options & 0x2) != 0;
	}

	private static void MoveWallBreakIndicator(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		int y = pvSrc.ReadInt16();
		int x = pvSrc.ReadInt16();

		game.WallBreakIndicator.Move(new Point2D(x, y));
	}

	private static void TogglePublicHand(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			return;

		pvSrc.ReadInt16();
		pvSrc.ReadByte();

		bool publicHand = pvSrc.ReadBoolean();

		game.Players.SetPublic(game.Players.GetPlayerIndex(state.Mobile), publicHand);
	}

	private static void MoveTile(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			return;

		int number = pvSrc.ReadByte();

		if (number >= game.Tiles.Length)
			return;

		pvSrc.ReadByte(); // Current direction

		MahjongPieceDirection direction = GetDirection(pvSrc.ReadByte());

		pvSrc.ReadByte();

		bool flip = pvSrc.ReadBoolean();

		pvSrc.ReadInt16(); // Current Y
		pvSrc.ReadInt16(); // Current X

		pvSrc.ReadByte();

		int y = pvSrc.ReadInt16();
		int x = pvSrc.ReadInt16();

		pvSrc.ReadByte();

		game.Tiles[number].Move(new Point2D(x, y), direction, flip, game.Players.GetPlayerIndex(state.Mobile));
	}

	private static void MoveDealerIndicator(MahjongGame game, NetState state, PacketReader pvSrc)
	{
		if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			return;

		MahjongPieceDirection direction = GetDirection(pvSrc.ReadByte());

		MahjongWind wind = GetWind(pvSrc.ReadByte());

		int y = pvSrc.ReadInt16();
		int x = pvSrc.ReadInt16();

		game.DealerIndicator.Move(new Point2D(x, y), direction, wind);
	}
}
