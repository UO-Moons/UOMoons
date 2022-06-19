using Server.Network;

namespace Server.Gumps
{
	public abstract class GumpEntry
	{
		private Gump _mParent;

		protected void Delta(ref int var, int val)
		{
			if (var == val) return;
			var = val;

			if (_mParent != null)
			{
				Gump.Invalidate();
			}
		}

		protected void Delta(ref bool var, bool val)
		{
			if (var == val) return;
			var = val;

			if (_mParent != null)
			{
				Gump.Invalidate();
			}
		}

		protected void Delta(ref string var, string val)
		{
			if (var == val) return;
			var = val;

			if (_mParent != null)
			{
				Gump.Invalidate();
			}
		}

		public Gump Parent
		{
			get => _mParent;
			set
			{
				if (_mParent == value) return;
				_mParent?.Remove(this);

				_mParent = value;

				_mParent?.Add(this);
			}
		}

		public abstract string Compile();
		public abstract void AppendTo(IGumpWriter disp);
	}
}
