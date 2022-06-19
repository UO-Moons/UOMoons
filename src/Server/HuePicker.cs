using Server.Network;

namespace Server.HuePickers
{
	public class HuePicker
	{
		private static int _mNextSerial = 1;

		public int Serial { get; }

		public int ItemId { get; }

		public HuePicker(int itemId)
		{
			do
			{
				Serial = _mNextSerial++;
			} while (Serial == 0);

			ItemId = itemId;
		}

		public virtual void OnResponse(int hue)
		{
		}

		public void SendTo(NetState state)
		{
			state.Send(new DisplayHuePicker(this));
			state.AddHuePicker(this);
		}
	}
}
