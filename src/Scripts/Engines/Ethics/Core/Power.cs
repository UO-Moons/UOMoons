namespace Server.Ethics
{
	public abstract class Power
	{
		protected PowerDefinition _Definition;

		public PowerDefinition Definition => _Definition;

		public virtual bool CheckInvoke(Player from)
		{
			if (!from.Mobile.CheckAlive())
				return false;

			if (from.Power < _Definition.Power)
			{
				from.Mobile.LocalOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, false, "You lack the power to invoke this ability.");
				return false;
			}

			return true;
		}

		public abstract void BeginInvoke(Player from);

		public virtual void FinishInvoke(Player from)
		{
			from.Power -= _Definition.Power;
		}
	}
}
