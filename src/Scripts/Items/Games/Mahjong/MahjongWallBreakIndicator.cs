namespace Server.Engines.Mahjong;

public class MahjongWallBreakIndicator
{
	private static MahjongPieceDim GetDimensions(Point2D position)
	{
		return new MahjongPieceDim(position, 20, 20);
	}

	private readonly MahjongGame m_Game;
	private Point2D m_Position;

	public MahjongGame Game => m_Game;
	public Point2D Position => m_Position;

	public MahjongWallBreakIndicator(MahjongGame game, Point2D position)
	{
		m_Game = game;
		m_Position = position;
	}

	public MahjongPieceDim Dimensions => GetDimensions(m_Position);

	public void Move(Point2D position)
	{
		MahjongPieceDim dim = GetDimensions(position);

		if (!dim.IsValid())
			return;

		m_Position = position;

		m_Game.Players.SendGeneralPacket(true, true);
	}

	public void Save(GenericWriter writer)
	{
		writer.Write(0); // version

		writer.Write(m_Position);
	}

	public MahjongWallBreakIndicator(MahjongGame game, GenericReader reader)
	{
		m_Game = game;

		reader.ReadInt();

		m_Position = reader.ReadPoint2D();
	}
}
