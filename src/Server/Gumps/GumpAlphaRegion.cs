using Server.Network;

namespace Server.Gumps
{
	public class GumpAlphaRegion : GumpEntry
	{
		private int _mX, _mY;
		private int _mWidth, _mHeight;

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

		public GumpAlphaRegion(int x, int y, int width, int height)
		{
			_mX = x;
			_mY = y;
			_mWidth = width;
			_mHeight = height;
		}

		public override string Compile()
		{
			return $"{{ checkertrans {_mX} {_mY} {_mWidth} {_mHeight} }}";
		}

		private static readonly byte[] MLayoutName = Gump.StringToBuffer("checkertrans");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(MLayoutName);
			disp.AppendLayout(_mX);
			disp.AppendLayout(_mY);
			disp.AppendLayout(_mWidth);
			disp.AppendLayout(_mHeight);
		}
	}
}
