using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Plants;

public interface IPlantHue
{
	PlantHue PlantHue { get; set; }
	void InvalidatePlantHue();
}

public interface IPigmentHue
{
	PlantPigmentHue PigmentHue { get; set; }
}

[Flags]
public enum PlantPigmentHue
{
	None = 0,

	Plain = 0x1,

	Red = 0x2,
	Blue = 0x4,
	Yellow = 0x8,

	Purple = Red | Blue,
	Green = Blue | Yellow,
	Orange = Red | Yellow,

	Black = 0x10,
	White = 0x20,

	Pink = 0x40,
	Magenta = 0x80,
	Aqua = 0x100,
	FireRed = 0x200,

	BrightRed = Red | Bright,
	BrightBlue = Blue | Bright,
	BrightYellow = Yellow | Bright,

	BrightPurple = Purple | Bright,
	BrightGreen = Green | Bright,
	BrightOrange = Orange | Bright,

	DarkRed = Red | Dark,
	DarkBlue = Blue | Dark,
	DarkYellow = Yellow | Dark,

	DarkPurple = Purple | Dark,
	DarkGreen = Green | Dark,
	DarkOrange = Orange | Dark,

	IceRed = Red | Ice,
	IceBlue = Blue | Ice,
	IceYellow = Yellow | Ice,

	IcePurple = Purple | Ice,
	IceGreen = Green | Ice,
	IceOrange = Orange | Ice,

	IceBlack = Black | Ice,
	OffWhite = White | Bright,
	Metal = Black | Bright,

	Ice = 0x2000000,
	Dark = 0x4000000,
	Bright = 0x8000000,
}

public class PlantPigmentHueInfo
{
	private static readonly Dictionary<PlantPigmentHue, PlantPigmentHueInfo> MTable;

	static PlantPigmentHueInfo()
	{
		MTable = new Dictionary<PlantPigmentHue, PlantPigmentHueInfo>
		{
			[PlantPigmentHue.Plain] = new(PlantHue.Plain, 2101, 1060813, PlantPigmentHue.Plain),
			[PlantPigmentHue.Red] = new(PlantHue.Red, 1652, 1060814, PlantPigmentHue.Red),
			[PlantPigmentHue.Blue] = new(PlantHue.Blue, 2122, 1060815, PlantPigmentHue.Blue),
			[PlantPigmentHue.Yellow] = new(PlantHue.Yellow, 2125, 1060818, PlantPigmentHue.Yellow),
			[PlantPigmentHue.BrightRed] = new(PlantHue.BrightRed, 1646, 1060814, PlantPigmentHue.BrightRed),
			[PlantPigmentHue.BrightBlue] = new(PlantHue.BrightBlue, 1310, 1060815, PlantPigmentHue.BrightBlue),
			[PlantPigmentHue.BrightYellow] = new(PlantHue.BrightYellow, 253, 1060818, PlantPigmentHue.BrightYellow),
			[PlantPigmentHue.DarkRed] = new(PlantHue.Plain, 1141, 1112162, PlantPigmentHue.DarkRed),
			[PlantPigmentHue.DarkBlue] = new(PlantHue.Plain, 1317, 1112164, PlantPigmentHue.DarkBlue),
			[PlantPigmentHue.DarkYellow] = new(PlantHue.Plain, 2217, 1112165, PlantPigmentHue.DarkYellow),
			[PlantPigmentHue.IceRed] = new(PlantHue.Plain, 335, 1112169, PlantPigmentHue.IceRed),
			[PlantPigmentHue.IceBlue] = new(PlantHue.Plain, 1154, 1112168, PlantPigmentHue.IceBlue),
			[PlantPigmentHue.IceYellow] = new(PlantHue.Plain, 56, 1112171, PlantPigmentHue.IceYellow),
			[PlantPigmentHue.Purple] = new(PlantHue.Purple, 15, 1060816, PlantPigmentHue.Purple),
			[PlantPigmentHue.Green] = new(PlantHue.Green, 2128, 1060819, PlantPigmentHue.Green),
			[PlantPigmentHue.Orange] = new(PlantHue.Orange, 1128, 1060817, PlantPigmentHue.Orange),
			[PlantPigmentHue.BrightPurple] = new(PlantHue.BrightPurple, 316, 1060816, PlantPigmentHue.BrightPurple),
			[PlantPigmentHue.BrightGreen] = new(PlantHue.BrightGreen, 671, 1060819, PlantPigmentHue.BrightGreen),
			[PlantPigmentHue.BrightOrange] = new(PlantHue.BrightOrange, 1501, 1060817, PlantPigmentHue.BrightOrange),
			[PlantPigmentHue.DarkPurple] = new(PlantHue.Plain, 1254, 1113166, PlantPigmentHue.DarkPurple),
			[PlantPigmentHue.DarkGreen] = new(PlantHue.Plain, 1425, 1112163, PlantPigmentHue.DarkGreen),
			[PlantPigmentHue.DarkOrange] = new(PlantHue.Plain, 1509, 1112161, PlantPigmentHue.DarkOrange),
			[PlantPigmentHue.IcePurple] = new(PlantHue.Plain, 511, 1112172, PlantPigmentHue.IcePurple),
			[PlantPigmentHue.IceGreen] = new(PlantHue.Plain, 261, 1112167, PlantPigmentHue.IceGreen),
			[PlantPigmentHue.IceOrange] = new(PlantHue.Plain, 346, 1112170, PlantPigmentHue.IceOrange),
			[PlantPigmentHue.Black] = new(PlantHue.Black, 1175, 1060820, PlantPigmentHue.Black),
			[PlantPigmentHue.White] = new(PlantHue.White, 1150, 1060821, PlantPigmentHue.White),
			[PlantPigmentHue.IceBlack] = new(PlantHue.Plain, 2422, 1112988, PlantPigmentHue.IceBlack),
			[PlantPigmentHue.OffWhite] = new(PlantHue.Plain, 746, 1112224, PlantPigmentHue.OffWhite),
			[PlantPigmentHue.Metal] = new(PlantHue.Plain, 1105, 1015046, PlantPigmentHue.Metal),
			[PlantPigmentHue.Pink] = new(PlantHue.Pink, 341, 1061854, PlantPigmentHue.Pink),
			[PlantPigmentHue.Magenta] = new(PlantHue.Magenta, 1163, 1061852, PlantPigmentHue.Magenta),
			[PlantPigmentHue.Aqua] = new(PlantHue.Aqua, 391, 1061853, PlantPigmentHue.Aqua),
			[PlantPigmentHue.FireRed] = new(PlantHue.FireRed, 1358, 1061855, PlantPigmentHue.FireRed)
		};
	}

	private PlantPigmentHueInfo(PlantHue planthue, int hue, int name, PlantPigmentHue pigmentHue)
	{
		PlantHue = planthue;
		Hue = hue;
		Name = name;
		PlantPigmentHue = pigmentHue;
	}

	public PlantHue PlantHue { get; }

	public int Hue { get; }

	public int Name { get; }

	public PlantPigmentHue PlantPigmentHue { get; }

	public static PlantPigmentHue HueFromPlantHue(PlantHue hue)
	{
		if (hue == PlantHue.None || hue == PlantHue.Plain)
			return PlantPigmentHue.Plain;

		foreach (var kvp in MTable.Where(kvp => kvp.Value.PlantHue == hue))
		{
			return kvp.Key;
		}

		return PlantPigmentHue.Plain;
	}

	public static PlantPigmentHueInfo GetInfo(PlantPigmentHue hue)
	{
		return !MTable.ContainsKey(hue) ? MTable[PlantPigmentHue.Plain] : MTable[hue];
	}

	public static bool IsMixable(PlantPigmentHue hue)
	{
		return hue <= PlantPigmentHue.White && hue != PlantPigmentHue.None;
	}

	public static bool IsBright(PlantPigmentHue hue)
	{
		return (hue & PlantPigmentHue.Bright) != PlantPigmentHue.None;
	}

	public static bool IsPrimary(PlantPigmentHue hue)
	{
		return hue is PlantPigmentHue.Red or PlantPigmentHue.Blue or PlantPigmentHue.Yellow;
	}

	public static PlantPigmentHue Mix(PlantPigmentHue first, PlantPigmentHue second)
	{
		if (!IsMixable(first) || !IsMixable(second))
			return PlantPigmentHue.None;

		if (first == second && first is PlantPigmentHue.Plain or PlantPigmentHue.Black or PlantPigmentHue.White)
			return PlantPigmentHue.None;

		if (first == second)
			return second | PlantPigmentHue.Bright;

		if (first == PlantPigmentHue.Plain)
			return second | PlantPigmentHue.Bright;
		if (second == PlantPigmentHue.Plain)
			return first | PlantPigmentHue.Bright;

		if (first == PlantPigmentHue.White)
			return second | PlantPigmentHue.Ice;
		if (second == PlantPigmentHue.White)
			return first | PlantPigmentHue.Ice;

		if (first == PlantPigmentHue.Black)
			return second | PlantPigmentHue.Dark;
		if (second == PlantPigmentHue.Black)
			return first | PlantPigmentHue.Dark;

		bool firstPrimary = IsPrimary(first);
		bool secondPrimary = IsPrimary(second);

		return firstPrimary switch
		{
			true when secondPrimary => first | second,
			//
			// not sure after this point
			// 
			// the remaining combinations to precess are (orange,purple,green with
			// any of red, blue, yellow, orange, purple, green)
			// the code below is temporary until proper mixed hues can be confirmed
			// 
			// mixing table on stratics seems incorrect because the table is not symmetrical
			// 
			true when true => first,
			false when secondPrimary => second,
			_ => first & second
		};
	}

	public bool IsMixable()
	{
		return IsMixable(PlantPigmentHue);
	}

	public bool IsBright()
	{
		return IsBright(PlantPigmentHue);
	}

	public bool IsPrimary()
	{
		return IsPrimary(PlantPigmentHue);
	}
}
