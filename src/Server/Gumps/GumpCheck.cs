using Server.Network;

namespace Server.Gumps
{
	public class GumpCheck : GumpEntry
	{
		private int _mX, _mY;
		private int _mId1, _mId2;
		private bool _mInitialState;
		private int _mSwitchId;

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

		public int InactiveId
		{
			get => _mId1;
			set => Delta(ref _mId1, value);
		}

		public int ActiveId
		{
			get => _mId2;
			set => Delta(ref _mId2, value);
		}

		public bool InitialState
		{
			get => _mInitialState;
			set => Delta(ref _mInitialState, value);
		}

		public int SwitchId
		{
			get => _mSwitchId;
			set => Delta(ref _mSwitchId, value);
		}

		public GumpCheck(int x, int y, int inactiveId, int activeId, bool initialState, int switchId)
		{
			_mX = x;
			_mY = y;
			_mId1 = inactiveId;
			_mId2 = activeId;
			_mInitialState = initialState;
			_mSwitchId = switchId;
		}

		public override string Compile()
		{
			return $"{{ checkbox {_mX} {_mY} {_mId1} {_mId2} {(_mInitialState ? 1 : 0)} {_mSwitchId} }}";
		}

		private static readonly byte[] MLayoutName = Gump.StringToBuffer("checkbox");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(MLayoutName);
			disp.AppendLayout(_mX);
			disp.AppendLayout(_mY);
			disp.AppendLayout(_mId1);
			disp.AppendLayout(_mId2);
			disp.AppendLayout(_mInitialState);
			disp.AppendLayout(_mSwitchId);

			disp.Switches++;
		}
	}
}
