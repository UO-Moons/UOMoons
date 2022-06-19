using System;

namespace Server.Engines.Harvest;

public class HarvestBank
{
	private readonly int _mMaximum;
	private int _mCurrent;
	private DateTime _mNextRespawn;
	private HarvestVein _mVein, _mDefaultVein;
	public HarvestBank(HarvestDefinition def, HarvestVein defaultVein)
	{
		_mMaximum = Utility.RandomMinMax(def.MinTotal, def.MaxTotal);
		_mCurrent = _mMaximum;
		_mDefaultVein = defaultVein;
		_mVein = _mDefaultVein;

		Definition = def;
	}

	public HarvestDefinition Definition { get; }
	public int Current
	{
		get
		{
			CheckRespawn();
			return _mCurrent;
		}
	}
	public HarvestVein Vein
	{
		get
		{
			CheckRespawn();
			return _mVein;
		}
		set => _mVein = value;
	}
	public HarvestVein DefaultVein
	{
		get
		{
			CheckRespawn();
			return _mDefaultVein;
		}
	}
	public void CheckRespawn()
	{
		if (_mCurrent == _mMaximum || _mNextRespawn > DateTime.UtcNow)
		{
			return;
		}

		_mCurrent = _mMaximum;

		if (Definition.RandomizeVeins)
		{
			_mDefaultVein = Definition.GetVeinFrom(Utility.RandomDouble());
		}

		_mVein = _mDefaultVein;
	}

	public void Consume(int amount, Mobile from)
	{
		CheckRespawn();

		if (_mCurrent == _mMaximum)
		{
			double min = Definition.MinRespawn.TotalMinutes;
			double max = Definition.MaxRespawn.TotalMinutes;
			double rnd = Utility.RandomDouble();

			_mCurrent = _mMaximum - amount;

			double minutes = min + (rnd * (max - min));
			if (Definition.RaceBonus && from.Race == Race.Elf)    //def.RaceBonus = Core.ML
			{
				minutes *= .75;    //25% off the time.  
			}

			_mNextRespawn = DateTime.UtcNow + TimeSpan.FromMinutes(minutes);
		}
		else
		{
			_mCurrent -= amount;
		}

		if (_mCurrent < 0)
		{
			_mCurrent = 0;
		}
	}
}
