using Server.Network;

namespace Server.Gumps
{
	public class GumpGroup : GumpEntry
	{
		private int _mGroup;

		public GumpGroup(int group)
		{
			_mGroup = group;
		}

		public int Group
		{
			get => _mGroup;
			set => Delta(ref _mGroup, value);
		}

		public override string Compile()
		{
			return $"{{ group {_mGroup} }}";
		}

		private static readonly byte[] MLayoutName = Gump.StringToBuffer("group");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(MLayoutName);
			disp.AppendLayout(_mGroup);
		}
	}
}
