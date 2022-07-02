using System;

namespace Server;

[PropertyObject]
public class VirtueInfo
{
	public int[] Values { get; } = new int[8];

	public void Clear()
	{
		Array.Clear(Values, 0, Values.Length);
	}

	public int GetValue(int index)
	{
		return Values[index];
	}

	public void SetValue(int index, int value)
	{
		Values[index] = value;
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Humility { get => GetValue(0); set => SetValue(0, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Sacrifice { get => GetValue(1); set => SetValue(1, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Compassion { get => GetValue(2); set => SetValue(2, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Spirituality { get => GetValue(3); set => SetValue(3, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Valor { get => GetValue(4); set => SetValue(4, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Honor { get => GetValue(5); set => SetValue(5, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Justice { get => GetValue(6); set => SetValue(6, value); }

	[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
	public int Honesty { get => GetValue(7); set => SetValue(7, value); }

	public VirtueInfo()
	{ }

	public VirtueInfo(GenericReader reader)
	{
		int version = reader.ReadByte();

		switch (version)
		{
			case 0:
			{
				int mask = reader.ReadByte();

				if (mask != 0)
				{
					for (var i = 0; i < 8; ++i)
					{
						if ((mask & (1 << i)) != 0)
						{
							Values[i] = reader.ReadInt();
						}
					}
				}

				break;
			}
		}
	}

	public static void Serialize(GenericWriter writer, VirtueInfo info)
	{
		writer.Write((byte)0); // version

		var mask = 0;

		for (var i = 0; i < 8; ++i)
		{
			if (info.Values[i] != 0)
			{
				mask |= 1 << i;
			}
		}

		writer.Write((byte)mask);

		for (var i = 0; i < 8; ++i)
		{
			if (info.Values[i] != 0)
			{
				writer.Write(info.Values[i]);
			}
		}
	}
}
