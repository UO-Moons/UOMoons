namespace Server.Engines.Mahjong;

public class MahjongDealerIndicator
{
	private static MahjongPieceDim GetDimensions(Point2D position, MahjongPieceDirection direction)
	{
		return direction is MahjongPieceDirection.Up or MahjongPieceDirection.Down ? new MahjongPieceDim(position, 40, 20) : new MahjongPieceDim(position, 20, 40);
	}

	private readonly MahjongGame m_Game;
	private Point2D m_Position;
	private MahjongPieceDirection m_Direction;
	private MahjongWind m_Wind;

	public MahjongGame Game => m_Game;
	public Point2D Position => m_Position;
	public MahjongPieceDirection Direction => m_Direction;
	public MahjongWind Wind => m_Wind;

	public MahjongDealerIndicator(MahjongGame game, Point2D position, MahjongPieceDirection direction, MahjongWind wind)
	{
		m_Game = game;
		m_Position = position;
		m_Direction = direction;
		m_Wind = wind;
	}

	public MahjongPieceDim Dimensions => GetDimensions(m_Position, m_Direction);

	public void Move(Point2D position, MahjongPieceDirection direction, MahjongWind wind)
	{
		MahjongPieceDim dim = GetDimensions(position, direction);

		if (!dim.IsValid())
			return;

		m_Position = position;
		m_Direction = direction;
		m_Wind = wind;

		m_Game.Players.SendGeneralPacket(true, true);
	}

	public void Save(GenericWriter writer)
	{
		writer.Write(0); // version

		writer.Write(m_Position);
		writer.Write((int)m_Direction);
		writer.Write((int)m_Wind);
	}

	public MahjongDealerIndicator(MahjongGame game, GenericReader reader)
	{
		m_Game = game;

		reader.ReadInt();

		m_Position = reader.ReadPoint2D();
		m_Direction = (MahjongPieceDirection)reader.ReadInt();
		m_Wind = (MahjongWind)reader.ReadInt();
	}
}
