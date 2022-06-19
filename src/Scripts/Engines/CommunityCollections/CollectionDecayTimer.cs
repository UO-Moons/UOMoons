using System;

namespace Server;

public class CollectionDecayTimer : Timer
{
	private readonly IComunityCollection _mCollection;
	public CollectionDecayTimer(IComunityCollection collection, TimeSpan delay)
		: base(delay, TimeSpan.FromDays(1.0))
	{
		_mCollection = collection;
		Priority = TimerPriority.OneMinute;
	}

	protected override void OnTick()
	{
		if (_mCollection != null && _mCollection.DailyDecay > 0)
			_mCollection.Points -= _mCollection.DailyDecay;
	}
}
