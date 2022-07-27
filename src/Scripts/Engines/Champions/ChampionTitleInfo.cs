using Server.Mobiles;
using System;
using System.Linq;

namespace Server.Engines.Champions;

[PropertyObject]
public class ChampionTitleInfo
{
	private static readonly TimeSpan LossDelay = TimeSpan.FromDays(1.0);
	private const int LossAmount = 90;

	private class TitleInfo
	{
		public int Value { get; set; }
		public DateTime LastDecay { get; set; }

		public TitleInfo()
		{
		}

		public TitleInfo(GenericReader reader)
		{
			int version = reader.ReadEncodedInt();

			switch (version)
			{
				case 0:
				{
					Value = reader.ReadEncodedInt();
					LastDecay = reader.ReadDateTime();
					break;
				}
			}
		}

		public static void Serialize(GenericWriter writer, TitleInfo info)
		{
			writer.WriteEncodedInt(0);

			writer.WriteEncodedInt(info.Value);
			writer.Write(info.LastDecay);
		}
	}

	private TitleInfo[] _mValues;

	private int GetValue(ChampionSpawnType type)
	{
		return GetValue((int)type);
	}

	private void SetValue(ChampionSpawnType type, int value)
	{
		SetValue((int)type, value);
	}

	public void Award(ChampionSpawnType type, int value)
	{
		Award((int)type, value);
	}

	public int GetValue(int index)
	{
		if (_mValues == null || index < 0 || index >= _mValues.Length)
			return 0;

		_mValues[index] ??= new TitleInfo();

		return _mValues[index].Value;
	}

	private DateTime GetLastDecay(int index)
	{
		if (_mValues == null || index < 0 || index >= _mValues.Length)
			return DateTime.MinValue;

		_mValues[index] ??= new TitleInfo();

		return _mValues[index].LastDecay;
	}

	private void SetValue(int index, int value)
	{
		_mValues ??= new TitleInfo[ChampionSpawnInfo.Table.Length];

		if (value < 0)
			value = 0;

		if (index < 0 || index >= _mValues.Length)
			return;

		_mValues[index] ??= new TitleInfo();

		_mValues[index].Value = value;
	}

	private void Award(int index, int value)
	{
		_mValues ??= new TitleInfo[ChampionSpawnInfo.Table.Length];

		if (index < 0 || index >= _mValues.Length || value <= 0)
			return;

		_mValues[index] ??= new TitleInfo();

		_mValues[index].Value += value;
	}

	private void Atrophy(int index, int value)
	{
		_mValues ??= new TitleInfo[ChampionSpawnInfo.Table.Length];

		if (index < 0 || index >= _mValues.Length || value <= 0)
			return;

		_mValues[index] ??= new TitleInfo();

		var before = _mValues[index].Value;

		if (_mValues[index].Value - value < 0)
			_mValues[index].Value = 0;
		else
			_mValues[index].Value -= value;

		if (before != _mValues[index].Value)
			_mValues[index].LastDecay = DateTime.UtcNow;
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Pestilence { get => GetValue(ChampionSpawnType.Pestilence); set => SetValue(ChampionSpawnType.Pestilence, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Abyss { get => GetValue(ChampionSpawnType.Abyss); set => SetValue(ChampionSpawnType.Abyss, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Arachnid { get => GetValue(ChampionSpawnType.Arachnid); set => SetValue(ChampionSpawnType.Arachnid, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ColdBlood { get => GetValue(ChampionSpawnType.ColdBlood); set => SetValue(ChampionSpawnType.ColdBlood, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ForestLord { get => GetValue(ChampionSpawnType.ForestLord); set => SetValue(ChampionSpawnType.ForestLord, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SleepingDragon { get => GetValue(ChampionSpawnType.SleepingDragon); set => SetValue(ChampionSpawnType.SleepingDragon, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int UnholyTerror { get => GetValue(ChampionSpawnType.UnholyTerror); set => SetValue(ChampionSpawnType.UnholyTerror, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int VerminHorde { get => GetValue(ChampionSpawnType.VerminHorde); set => SetValue(ChampionSpawnType.VerminHorde, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Harrower { get; private set; }

	public ChampionTitleInfo()
	{
	}

	public ChampionTitleInfo(GenericReader reader)
	{
		var version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				Harrower = reader.ReadEncodedInt();

				int length = reader.ReadEncodedInt();
				_mValues = new TitleInfo[length];

				for (var i = 0; i < length; i++)
				{
					_mValues[i] = new TitleInfo(reader);
				}

				if (_mValues.Length != ChampionSpawnInfo.Table.Length)
				{
					TitleInfo[] oldValues = _mValues;
					_mValues = new TitleInfo[ChampionSpawnInfo.Table.Length];

					for (var i = 0; i < _mValues.Length && i < oldValues.Length; i++)
					{
						_mValues[i] = oldValues[i];
					}
				}
				break;
			}
		}
	}

	public static void Serialize(GenericWriter writer, ChampionTitleInfo titles)
	{
		writer.WriteEncodedInt(0); // version

		writer.WriteEncodedInt(titles.Harrower);

		var length = titles._mValues.Length;
		writer.WriteEncodedInt(length);

		for (var i = 0; i < length; i++)
		{
			titles._mValues[i] ??= new TitleInfo();

			TitleInfo.Serialize(writer, titles._mValues[i]);
		}
	}

	public static void CheckAtrophy(PlayerMobile pm)
	{
		var t = pm.ChampionTitles;
		if (t == null)
			return;

		t._mValues ??= new TitleInfo[ChampionSpawnInfo.Table.Length];

		for (var i = 0; i < t._mValues.Length; i++)
		{
			if (t.GetLastDecay(i) + LossDelay < DateTime.UtcNow)
			{
				t.Atrophy(i, LossAmount);
			}
		}
	}

	public static void AwardHarrowerTitle(PlayerMobile pm)  //Called when killing a harrower.  Will give a minimum of 1 point.
	{
		var t = pm.ChampionTitles;
		if (t == null)
			return;

		t._mValues ??= new TitleInfo[ChampionSpawnInfo.Table.Length];

		var count = 1;

		for (var i = 0; i < t._mValues.Length; i++)
		{
			if (t._mValues[i].Value > 900)
				count++;
		}

		t.Harrower = Math.Max(count, t.Harrower);   //Harrower titles never decay.
	}

	public bool HasChampionTitle(PlayerMobile pm)
	{
		if (Harrower > 0)
			return true;

		return _mValues != null && _mValues.Any(info => info.Value > 300);
	}
}
