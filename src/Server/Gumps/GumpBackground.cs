using Server.Network;

namespace Server.Gumps
{
	public class GumpBackground : GumpEntry
	{
		private int _mX, _mY;
		private int _mWidth, _mHeight;
		private int _mGumpId;

		public int X
		{
			get => _mX;
			set => Delta(ref _mX, value);
		}

		public int Y
		{
			get => _mY;
			set => Delta(ref _mY, value);
		}

		public int Width
		{
			get => _mWidth;
			set => Delta(ref _mWidth, value);
		}

		public int Height
		{
			get => _mHeight;
			set => Delta(ref _mHeight, value);
		}

		public int GumpId
		{
			get => _mGumpId;
			set => Delta(ref _mGumpId, value);
		}

		public GumpBackground(int x, int y, int width, int height, int gumpId)
		{
			_mX = x;
			_mY = y;
			_mWidth = width;
			_mHeight = height;
			_mGumpId = gumpId;
		}

		public override string Compile()
		{
			return $"{{ resizepic {_mX} {_mY} {_mGumpId} {_mWidth} {_mHeight} }}";
		}

		private static readonly byte[] MLayoutName = Gump.StringToBuffer("resizepic");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(MLayoutName);
			disp.AppendLayout(_mX);
			disp.AppendLayout(_mY);
			disp.AppendLayout(_mGumpId);
			disp.AppendLayout(_mWidth);
			disp.AppendLayout(_mHeight);
		}
	}
}
