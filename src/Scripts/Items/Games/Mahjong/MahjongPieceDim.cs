namespace Server.Engines.Mahjong;

public struct MahjongPieceDim
{
	private Point2D m_Position;

	public Point2D Position => m_Position;
	private int Width { get; }

	private int Height { get; }

	public MahjongPieceDim(Point2D position, int width, int height)
	{
		m_Position = position;
		Width = width;
		Height = height;
	}

	public bool IsValid()
	{
		return m_Position.X >= 0 && m_Position.Y >= 0 && m_Position.X + Width <= 670 && m_Position.Y + Height <= 670;
	}

	public bool IsOverlapping(MahjongPieceDim dim)
	{
		return m_Position.X < dim.m_Position.X + dim.Width && m_Position.Y < dim.m_Position.Y + dim.Height && m_Position.X + Width > dim.m_Position.X && m_Position.Y + Height > dim.m_Position.Y;
	}

	public int GetHandArea()
	{
		if (m_Position.X + Width > 150 && m_Position.X < 520 && m_Position.Y < 35)
			return 0;

		if (m_Position.X + Width > 635 && m_Position.Y + Height > 150 && m_Position.Y < 520)
			return 1;

		if (m_Position.X + Width > 150 && m_Position.X < 520 && m_Position.Y + Height > 635)
			return 2;

		if (m_Position.X < 35 && m_Position.Y + Height > 150 && m_Position.Y < 520)
			return 3;

		return -1;
	}
}
