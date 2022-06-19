using Server.Network;

namespace Server.Gumps
{
	public enum GumpButtonType
	{
		Page = 0,
		Reply = 1
	}

	public class GumpButton : GumpEntry
	{
		private int _mX, _mY;
		private int _mId1, _mId2;
		private int _mButtonId;
		private GumpButtonType _mType;
		private int _mParam;

		public GumpButton(int x, int y, int normalId, int pressedId, int buttonId, GumpButtonType type, int param)
		{
			_mX = x;
			_mY = y;
			_mId1 = normalId;
			_mId2 = pressedId;
			_mButtonId = buttonId;
			_mType = type;
			_mParam = param;
		}

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

		public int NormalId
		{
			get => _mId1;
			set => Delta(ref _mId1, value);
		}

		public int PressedId
		{
			get => _mId2;
			set => Delta(ref _mId2, value);
		}

		public int ButtonId
		{
			get => _mButtonId;
			set => Delta(ref _mButtonId, value);
		}

		public GumpButtonType Type
		{
			get => _mType;
			set
			{
				if (_mType == value) return;
				_mType = value;

				Gump parent = Parent;

				if (parent != null)
				{
					Gump.Invalidate();
				}
			}
		}

		public int Param
		{
			get => _mParam;
			set => Delta(ref _mParam, value);
		}

		public override string Compile()
		{
			return $"{{ button {_mX} {_mY} {_mId1} {_mId2} {(int) _mType} {_mParam} {_mButtonId} }}";
		}

		private static readonly byte[] MLayoutName = Gump.StringToBuffer("button");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(MLayoutName);
			disp.AppendLayout(_mX);
			disp.AppendLayout(_mY);
			disp.AppendLayout(_mId1);
			disp.AppendLayout(_mId2);
			disp.AppendLayout((int)_mType);
			disp.AppendLayout(_mParam);
			disp.AppendLayout(_mButtonId);
		}
	}
}
