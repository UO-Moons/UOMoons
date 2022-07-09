using System;
namespace Server;

public class DefensiveSpell
{
	public static void Nullify(Mobile from)
	{
		if (!from.CanBeginAction(typeof(DefensiveSpell)))
			new InternalTimer(from).Start();
	}

	private class InternalTimer : Timer
	{
		private readonly Mobile _mobile;

		public InternalTimer(Mobile m)
			: base(TimeSpan.FromMinutes(1.0))
		{
			_mobile = m;

			Priority = TimerPriority.OneSecond;
		}

		protected override void OnTick()
		{
			_mobile.EndAction(typeof(DefensiveSpell));
		}
	}
}
