using Server.Items;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CPA = Server.CommandPropertyAttribute;

namespace Server.Commands;

public class Add
{
	public static void Initialize()
	{
		CommandSystem.Register("Tile", AccessLevel.GameMaster, Tile_OnCommand);
		CommandSystem.Register("TileRXYZ", AccessLevel.GameMaster, TileRXYZ_OnCommand);
		CommandSystem.Register("TileXYZ", AccessLevel.GameMaster, TileXYZ_OnCommand);
		CommandSystem.Register("TileZ", AccessLevel.GameMaster, TileZ_OnCommand);
		CommandSystem.Register("TileAvg", AccessLevel.GameMaster, TileAvg_OnCommand);

		CommandSystem.Register("Outline", AccessLevel.GameMaster, Outline_OnCommand);
		CommandSystem.Register("OutlineRXYZ", AccessLevel.GameMaster, OutlineRXYZ_OnCommand);
		CommandSystem.Register("OutlineXYZ", AccessLevel.GameMaster, OutlineXYZ_OnCommand);
		CommandSystem.Register("OutlineZ", AccessLevel.GameMaster, OutlineZ_OnCommand);
		CommandSystem.Register("OutlineAvg", AccessLevel.GameMaster, OutlineAvg_OnCommand);
	}

	public static void Invoke(Mobile from, Point3D start, Point3D end, string[] args)
	{
		Invoke(from, start, end, args, null, false, false);
	}

	public static void Invoke(Mobile from, Point3D start, Point3D end, string[] args, List<Container> packs)
	{
		Invoke(from, start, end, args, packs, false, false);
	}

	public static void Invoke(Mobile from, Point3D start, Point3D end, string[] args, List<Container> packs, bool outline, bool mapAvg)
	{
		StringBuilder sb = new();

		sb.Append($"{from.AccessLevel} {CommandLogging.Format(from)} building ");

		if (start == end)
			sb.Append($"at {start} in {from.Map}");
		else
			sb.Append($"from {start} to {end} in {from.Map}");

		sb.Append(":");

		for (int i = 0; i < args.Length; ++i)
			sb.Append($" \"{args[i]}\"");

		CommandLogging.WriteLine(from, sb.ToString());

		string name = args[0];

		FixArgs(ref args);

		string[,] props = null;

		for (int i = 0; i < args.Length; ++i)
		{
			if (Insensitive.Equals(args[i], "set"))
			{
				int remains = args.Length - i - 1;

				if (remains >= 2)
				{
					props = new string[remains / 2, 2];

					remains /= 2;

					for (int j = 0; j < remains; ++j)
					{
						props[j, 0] = args[i + (j * 2) + 1];
						props[j, 1] = args[i + (j * 2) + 2];
					}

					FixSetString(ref args, i);
				}

				break;
			}
		}

		Type type = Assembler.FindTypeByName(name);

		if (!IsEntity(type))
		{
			from.SendMessage("No type with that name was found.");
			return;
		}

		DateTime time = DateTime.UtcNow;

		int built = BuildObjects(from, type, start, end, args, props, packs, outline, mapAvg);

		if (built > 0)
			from.SendMessage("{0} object{1} generated in {2:F1} seconds.", built, built != 1 ? "s" : "", (DateTime.UtcNow - time).TotalSeconds);
		else
			SendUsage(type, from);
	}

	public static void FixSetString(ref string[] args, int index)
	{
		string[] old = args;
		args = new string[index];

		Array.Copy(old, 0, args, 0, index);
	}

	public static void FixArgs(ref string[] args)
	{
		string[] old = args;
		args = new string[args.Length - 1];

		Array.Copy(old, 1, args, 0, args.Length);
	}

	public static int BuildObjects(Mobile from, Type type, Point3D start, Point3D end, string[] args, string[,] props, List<Container> packs)
	{
		return BuildObjects(from, type, start, end, args, props, packs, false, false);
	}

	public static int BuildObjects(Mobile from, Type type, Point3D start, Point3D end, string[] args, string[,] props, List<Container> packs, bool outline, bool mapAvg)
	{
		Utility.FixPoints(ref start, ref end);

		PropertyInfo[] realProps = null;

		if (props != null)
		{
			realProps = new PropertyInfo[props.GetLength(0)];

			PropertyInfo[] allProps = type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

			for (int i = 0; i < realProps.Length; ++i)
			{
				PropertyInfo thisProp = null;

				string propName = props[i, 0];

				for (int j = 0; thisProp == null && j < allProps.Length; ++j)
				{
					if (Insensitive.Equals(propName, allProps[j].Name))
						thisProp = allProps[j];
				}

				if (thisProp == null)
				{
					from.SendMessage("Property not found: {0}", propName);
				}
				else
				{
					CPA attr = Properties.GetCpa(thisProp);

					if (attr == null)
						from.SendMessage("Property ({0}) not found.", propName);
					else if (from.AccessLevel < attr.WriteLevel)
						from.SendMessage("Setting this property ({0}) requires at least {1} access level.", propName, Mobile.GetAccessLevelName(attr.WriteLevel));
					else if (!thisProp.CanWrite || attr.ReadOnly)
						from.SendMessage("Property ({0}) is read only.", propName);
					else
						realProps[i] = thisProp;
				}
			}
		}

		ConstructorInfo[] ctors = type.GetConstructors();

		for (int i = 0; i < ctors.Length; ++i)
		{
			ConstructorInfo ctor = ctors[i];

			if (!IsConstructable(ctor, from.AccessLevel))
				continue;

			ParameterInfo[] paramList = ctor.GetParameters();

			if (args.Length == paramList.Length)
			{
				object[] paramValues = ParseValues(paramList, args);

				if (paramValues == null)
					continue;

				int built = Build(from, start, end, ctor, paramValues, props, realProps, packs, outline, mapAvg);

				if (built > 0)
					return built;
			}
		}

		return 0;
	}

	public static object[] ParseValues(ParameterInfo[] paramList, string[] args)
	{
		object[] values = new object[args.Length];

		for (int i = 0; i < args.Length; ++i)
		{
			object value = ParseValue(paramList[i].ParameterType, args[i]);

			if (value != null)
				values[i] = value;
			else
				return null;
		}

		return values;
	}

	public static object ParseValue(Type type, string value)
	{
		try
		{
			if (IsEnum(type))
			{
				return Enum.Parse(type, value, true);
			}

			if (IsType(type))
			{
				return Assembler.FindTypeByName(value);
			}

			if (IsParsable(type))
			{
				return ParseParsable(type, value);
			}

			object obj = value;

			if (value != null && value.StartsWith("0x"))
			{
				if (IsSignedNumeric(type))
					obj = Convert.ToInt64(value[2..], 16);
				else if (IsUnsignedNumeric(type))
					obj = Convert.ToUInt64(value[2..], 16);

				obj = Convert.ToInt32(value[2..], 16);
			}

			if (obj == null && !type.IsValueType)
				return null;
			return Convert.ChangeType(obj, type);
		}
		catch
		{
			return null;
		}
	}

	public static IEntity Build(Mobile from, ConstructorInfo ctor, object[] values, string[,] props, PropertyInfo[] realProps, ref bool sendError)
	{
		object built = ctor.Invoke(values);

		if (built != null && realProps != null)
		{
			bool hadError = false;

			for (int i = 0; i < realProps.Length; ++i)
			{
				if (realProps[i] == null)
					continue;

				string result = Properties.InternalSetValue(from, built, built, realProps[i], props[i, 1], props[i, 1], false);

				if (result != "Property has been set.")
				{
					if (sendError)
						from.SendMessage(result);

					hadError = true;
				}
			}

			if (hadError)
				sendError = false;
		}

		return (IEntity)built;
	}

	public static int Build(Mobile from, Point3D start, Point3D end, ConstructorInfo ctor, object[] values, string[,] props, PropertyInfo[] realProps, List<Container> packs)
	{
		return Build(from, start, end, ctor, values, props, realProps, packs, false, false);
	}

	public static int Build(Mobile from, Point3D start, Point3D end, ConstructorInfo ctor, object[] values, string[,] props, PropertyInfo[] realProps, List<Container> packs, bool outline, bool mapAvg)
	{
		try
		{
			Map map = from.Map;

			int width = end.X - start.X + 1;
			int height = end.Y - start.Y + 1;

			if (outline && (width < 3 || height < 3))
				outline = false;

			int objectCount;

			if (packs != null)
				objectCount = packs.Count;
			else if (outline)
				objectCount = (width + height - 2) * 2;
			else
				objectCount = width * height;

			if (objectCount >= 20)
				from.SendMessage("Constructing {0} objects, please wait.", objectCount);

			bool sendError = true;

			StringBuilder sb = new();
			sb.Append("Serials: ");

			if (packs != null)
			{
				for (int i = 0; i < packs.Count; ++i)
				{
					IEntity built = Build(from, ctor, values, props, realProps, ref sendError);

					sb.Append($"0x{built.Serial.Value:X}; ");

					switch (built)
					{
						case Item item:
						{
							Container pack = packs[i];
							pack.DropItem(item);
							break;
						}
						case Mobile mobile:
						{
							mobile.MoveToWorld(new Point3D(start.X, start.Y, start.Z), map);
							break;
						}
					}
				}
			}
			else
			{
				int z = start.Z;

				for (int x = start.X; x <= end.X; ++x)
				{
					for (int y = start.Y; y <= end.Y; ++y)
					{
						if (outline && x != start.X && x != end.X && y != start.Y && y != end.Y)
							continue;

						if (mapAvg)
							z = map.GetAverageZ(x, y);

						IEntity built = Build(from, ctor, values, props, realProps, ref sendError);

						sb.AppendFormat("0x{0:X}; ", built.Serial.Value);

						switch (built)
						{
							case Item item1:
							{
								item1.MoveToWorld(new Point3D(x, y, z), map);
								break;
							}
							case Mobile mobile:
							{
								mobile.MoveToWorld(new Point3D(x, y, z), map);
								break;
							}
						}
					}
				}
			}

			CommandLogging.WriteLine(from, sb.ToString());

			return objectCount;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex);
			return 0;
		}
	}

	public static void SendUsage(Type type, Mobile from)
	{
		ConstructorInfo[] ctors = type.GetConstructors();
		bool foundCtor = false;

		for (int i = 0; i < ctors.Length; ++i)
		{
			ConstructorInfo ctor = ctors[i];

			if (!IsConstructable(ctor, from.AccessLevel))
				continue;

			if (!foundCtor)
			{
				foundCtor = true;
				from.SendMessage("Usage:");
			}

			SendCtor(type, ctor, from);
		}

		if (!foundCtor)
			from.SendMessage("That type is not marked constructable.");
	}

	public static void SendCtor(Type type, ConstructorInfo ctor, Mobile from)
	{
		ParameterInfo[] paramList = ctor.GetParameters();

		StringBuilder sb = new();

		sb.Append(type.Name);

		for (int i = 0; i < paramList.Length; ++i)
		{
			if (i != 0)
				sb.Append(',');

			sb.Append(' ');

			sb.Append(paramList[i].ParameterType.Name);
			sb.Append(' ');
			sb.Append(paramList[i].Name);
		}

		from.SendMessage(sb.ToString());
	}

	public class AddTarget : Target
	{
		private readonly string[] m_Args;

		public AddTarget(string[] args) : base(-1, true, TargetFlags.None)
		{
			m_Args = args;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o is IPoint3D p)
			{
				p = p switch
				{
					Item item => item.GetWorldTop(),
					Mobile mobile => mobile.Location,
					_ => p
				};

				Point3D point = new(p);
				Add.Invoke(from, point, point, m_Args);
			}
		}
	}

	private enum TileZType
	{
		Start,
		Fixed,
		MapAverage
	}

	private class TileState
	{
		public readonly TileZType MZType;
		public readonly int MFixedZ;
		public readonly string[] MArgs;
		public readonly bool MOutline;

		public TileState(TileZType zType, int fixedZ, string[] args, bool outline)
		{
			MZType = zType;
			MFixedZ = fixedZ;
			MArgs = args;
			MOutline = outline;
		}
	}

	private static void TileBox_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
	{
		TileState ts = (TileState)state;
		bool mapAvg = false;

		switch (ts.MZType)
		{
			case TileZType.Fixed:
			{
				start.Z = end.Z = ts.MFixedZ;
				break;
			}
			case TileZType.MapAverage:
			{
				mapAvg = true;
				break;
			}
		}

		Invoke(from, start, end, ts.MArgs, null, ts.MOutline, mapAvg);
	}

	private static void Internal_OnCommand(CommandEventArgs e, bool outline)
	{
		if (e.Length >= 1)
			BoundingBoxPicker.Begin(e.Mobile, TileBox_Callback, new TileState(TileZType.Start, 0, e.Arguments, outline));
		else
			e.Mobile.SendMessage("Format: {0} <type> [params] [set {{<propertyName> <value> ...}}]", outline ? "Outline" : "Tile");
	}

	private static void InternalRXYZ_OnCommand(CommandEventArgs e, bool outline)
	{
		if (e.Length >= 6)
		{
			Point3D p = new(e.Mobile.X + e.GetInt32(0), e.Mobile.Y + e.GetInt32(1), e.Mobile.Z + e.GetInt32(4));
			Point3D p2 = new(p.X + e.GetInt32(2) - 1, p.Y + e.GetInt32(3) - 1, p.Z);

			string[] subArgs = new string[e.Length - 5];

			for (int i = 0; i < subArgs.Length; ++i)
				subArgs[i] = e.Arguments[i + 5];

			Invoke(e.Mobile, p, p2, subArgs, null, outline, false);
		}
		else
		{
			e.Mobile.SendMessage("Format: {0}RXYZ <x> <y> <w> <h> <z> <type> [params] [set {{<propertyName> <value> ...}}]", outline ? "Outline" : "Tile");
		}
	}

	private static void InternalXYZ_OnCommand(CommandEventArgs e, bool outline)
	{
		if (e.Length >= 6)
		{
			Point3D p = new(e.GetInt32(0), e.GetInt32(1), e.GetInt32(4));
			Point3D p2 = new(p.X + e.GetInt32(2) - 1, p.Y + e.GetInt32(3) - 1, e.GetInt32(4));

			string[] subArgs = new string[e.Length - 5];

			for (int i = 0; i < subArgs.Length; ++i)
				subArgs[i] = e.Arguments[i + 5];

			Invoke(e.Mobile, p, p2, subArgs, null, outline, false);
		}
		else
		{
			e.Mobile.SendMessage("Format: {0}XYZ <x> <y> <w> <h> <z> <type> [params] [set {{<propertyName> <value> ...}}]", outline ? "Outline" : "Tile");
		}
	}

	private static void InternalZ_OnCommand(CommandEventArgs e, bool outline)
	{
		if (e.Length >= 2)
		{
			string[] subArgs = new string[e.Length - 1];

			for (int i = 0; i < subArgs.Length; ++i)
				subArgs[i] = e.Arguments[i + 1];

			BoundingBoxPicker.Begin(e.Mobile, TileBox_Callback, new TileState(TileZType.Fixed, e.GetInt32(0), subArgs, outline));
		}
		else
		{
			e.Mobile.SendMessage("Format: {0}Z <z> <type> [params] [set {{<propertyName> <value> ...}}]", outline ? "Outline" : "Tile");
		}
	}

	private static void InternalAvg_OnCommand(CommandEventArgs e, bool outline)
	{
		if (e.Length >= 1)
			BoundingBoxPicker.Begin(e.Mobile, TileBox_Callback, new TileState(TileZType.MapAverage, 0, e.Arguments, outline));
		else
			e.Mobile.SendMessage("Format: {0}Avg <type> [params] [set {{<propertyName> <value> ...}}]", outline ? "Outline" : "Tile");
	}

	[Usage("Tile <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name into a targeted bounding box. Optional constructor parameters. Optional set property list.")]
	public static void Tile_OnCommand(CommandEventArgs e)
	{
		Internal_OnCommand(e, false);
	}

	[Usage("TileRXYZ <x> <y> <w> <h> <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name into a given bounding box, (x, y) parameters are relative to your characters position. Optional constructor parameters. Optional set property list.")]
	public static void TileRXYZ_OnCommand(CommandEventArgs e)
	{
		InternalRXYZ_OnCommand(e, false);
	}

	[Usage("TileXYZ <x> <y> <w> <h> <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name into a given bounding box. Optional constructor parameters. Optional set property list.")]
	public static void TileXYZ_OnCommand(CommandEventArgs e)
	{
		InternalXYZ_OnCommand(e, false);
	}

	[Usage("TileZ <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name into a targeted bounding box at a fixed Z location. Optional constructor parameters. Optional set property list.")]
	public static void TileZ_OnCommand(CommandEventArgs e)
	{
		InternalZ_OnCommand(e, false);
	}

	[Usage("TileAvg <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name into a targeted bounding box on the map's average Z elevation. Optional constructor parameters. Optional set property list.")]
	public static void TileAvg_OnCommand(CommandEventArgs e)
	{
		InternalAvg_OnCommand(e, false);
	}

	[Usage("Outline <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name around a targeted bounding box. Optional constructor parameters. Optional set property list.")]
	public static void Outline_OnCommand(CommandEventArgs e)
	{
		Internal_OnCommand(e, true);
	}

	[Usage("OutlineRXYZ <x> <y> <w> <h> <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name around a given bounding box, (x, y) parameters are relative to your characters position. Optional constructor parameters. Optional set property list.")]
	public static void OutlineRXYZ_OnCommand(CommandEventArgs e)
	{
		InternalRXYZ_OnCommand(e, true);
	}

	[Usage("OutlineXYZ <x> <y> <w> <h> <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name around a given bounding box. Optional constructor parameters. Optional set property list.")]
	public static void OutlineXYZ_OnCommand(CommandEventArgs e)
	{
		InternalXYZ_OnCommand(e, true);
	}

	[Usage("OutlineZ <z> <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name around a targeted bounding box at a fixed Z location. Optional constructor parameters. Optional set property list.")]
	public static void OutlineZ_OnCommand(CommandEventArgs e)
	{
		InternalZ_OnCommand(e, true);
	}

	[Usage("OutlineAvg <name> [params] [set {<propertyName> <value> ...}]")]
	[Description("Tiles an item or npc by name around a targeted bounding box on the map's average Z elevation. Optional constructor parameters. Optional set property list.")]
	public static void OutlineAvg_OnCommand(CommandEventArgs e)
	{
		InternalAvg_OnCommand(e, true);
	}

	private static readonly Type MEntityType = typeof(IEntity);

	public static bool IsEntity(Type t)
	{
		return MEntityType.IsAssignableFrom(t);
	}

	private static readonly Type MConstructableType = typeof(ConstructableAttribute);

	public static bool IsConstructable(ConstructorInfo ctor, AccessLevel accessLevel)
	{
		object[] attrs = ctor.GetCustomAttributes(MConstructableType, false);

		if (attrs.Length == 0)
			return false;

		return accessLevel >= ((ConstructableAttribute)attrs[0]).AccessLevel;
	}

	private static readonly Type MEnumType = typeof(Enum);

	public static bool IsEnum(Type type)
	{
		return type.IsSubclassOf(MEnumType);
	}

	private static readonly Type MTypeType = typeof(Type);

	public static bool IsType(Type type)
	{
		return (type == MTypeType || type.IsSubclassOf(MTypeType));
	}

	private static readonly Type MParsableType = typeof(ParsableAttribute);

	public static bool IsParsable(Type type)
	{
		return type.IsDefined(MParsableType, false);
	}

	private static readonly Type[] MParseTypes = { typeof(string) };
	private static readonly object[] MParseArgs = new object[1];

	public static object ParseParsable(Type type, string value)
	{
		MethodInfo method = type.GetMethod("Parse", MParseTypes);

		MParseArgs[0] = value;

		return method.Invoke(null, MParseArgs);
	}

	private static readonly Type[] MSignedNumerics = {
		typeof( long ),
		typeof( int ),
		typeof( short ),
		typeof( sbyte )
	};

	public static bool IsSignedNumeric(Type type)
	{
		for (int i = 0; i < MSignedNumerics.Length; ++i)
			if (type == MSignedNumerics[i])
				return true;

		return false;
	}

	private static readonly Type[] MUnsignedNumerics = {
		typeof( ulong ),
		typeof( uint ),
		typeof( ushort ),
		typeof( byte )
	};

	public static bool IsUnsignedNumeric(Type type)
	{
		for (int i = 0; i < MUnsignedNumerics.Length; ++i)
			if (type == MUnsignedNumerics[i])
				return true;

		return false;
	}
}
