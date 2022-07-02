using Server.Commands;
using Server.Commands.Generic;
using Server.Gumps;
using Server.Targeting;
using System;
using System.Reflection;
using CPA = Server.CommandPropertyAttribute;

namespace Server.Commands
{
	[Flags]
	public enum PropertyAccess
	{
		Read = 0x01,
		Write = 0x02,
		ReadWrite = Read | Write
	}

	public class Properties
	{
		public static void Initialize()
		{
			CommandSystem.Register("Props", AccessLevel.Counselor, Props_OnCommand);
		}

		private class PropsTarget : Target
		{
			public PropsTarget() : base(-1, true, TargetFlags.None)
			{
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (!BaseCommand.IsAccessible(from, o))
					from.SendMessage("That is not accessible.");
				else
					from.SendGump(new PropertiesGump(from, o));
			}
		}

		[Usage("Props [serial]")]
		[Description("Opens a menu where you can view and edit all properties of a targeted (or specified) object.")]
		private static void Props_OnCommand(CommandEventArgs e)
		{
			if (e.Length == 1)
			{
				IEntity ent = World.FindEntity(e.GetSerial(0));

				if (ent == null)
					e.Mobile.SendMessage("No object with that serial was found.");
				else if (!BaseCommand.IsAccessible(e.Mobile, ent))
					e.Mobile.SendMessage("That is not accessible.");
				else
					e.Mobile.SendGump(new PropertiesGump(e.Mobile, ent));
			}
			else
			{
				e.Mobile.Target = new PropsTarget();
			}
		}

		private static bool CiEqual(string l, string r)
		{
			return Insensitive.Equals(l, r);
		}

		private static readonly Type TypeofCpa = typeof(CPA);

		public static CPA GetCpa(PropertyInfo p)
		{
			object[] attrs = p.GetCustomAttributes(TypeofCpa, false);

			if (attrs.Length == 0)
				return null;

			return attrs[0] as CPA;
		}

		public static PropertyInfo[] GetPropertyInfoChain(Mobile from, Type type, string propertyString, PropertyAccess endAccess, ref string failReason)
		{
			string[] split = propertyString.Split('.');

			if (split.Length == 0)
				return null;

			PropertyInfo[] info = new PropertyInfo[split.Length];

			for (int i = 0; i < info.Length; ++i)
			{
				string propertyName = split[i];

				if (CiEqual(propertyName, "current"))
					continue;

				PropertyInfo[] props = type.GetProperties(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);

				bool isFinal = i == info.Length - 1;

				PropertyAccess access = endAccess;

				if (!isFinal)
					access |= PropertyAccess.Read;

				for (int j = 0; j < props.Length; ++j)
				{
					PropertyInfo p = props[j];

					if (CiEqual(p.Name, propertyName))
					{
						CPA attr = GetCpa(p);

						if (attr == null)
						{
							failReason = $"Property '{propertyName}' not found.";
							return null;
						}

						if ((access & PropertyAccess.Read) != 0 && from.AccessLevel < attr.ReadLevel)
						{
							failReason =
								$"You must be at least {Mobile.GetAccessLevelName(attr.ReadLevel)} to get the property '{propertyName}'.";

							return null;
						}

						if ((access & PropertyAccess.Write) != 0 && from.AccessLevel < attr.WriteLevel)
						{
							failReason =
								$"You must be at least {Mobile.GetAccessLevelName(attr.WriteLevel)} to set the property '{propertyName}'.";

							return null;
						}

						if ((access & PropertyAccess.Read) != 0 && !p.CanRead)
						{
							failReason = $"Property '{propertyName}' is write only.";
							return null;
						}

						if ((access & PropertyAccess.Write) != 0 && (!p.CanWrite || attr.ReadOnly) && isFinal)
						{
							failReason = $"Property '{propertyName}' is read only.";
							return null;
						}

						info[i] = p;
						type = p.PropertyType;
						break;
					}
				}

				if (info[i] == null)
				{
					failReason = $"Property '{propertyName}' not found.";
					return null;
				}
			}

			return info;
		}

		public static PropertyInfo GetPropertyInfo(Mobile from, ref object obj, string propertyName, PropertyAccess access, ref string failReason)
		{
			PropertyInfo[] chain = GetPropertyInfoChain(from, obj.GetType(), propertyName, access, ref failReason);

			return chain == null ? null : GetPropertyInfo(ref obj, chain, ref failReason);
		}

		public static PropertyInfo GetPropertyInfo(ref object obj, PropertyInfo[] chain, ref string failReason)
		{
			if (chain == null || chain.Length == 0)
			{
				failReason = "Property chain is empty.";
				return null;
			}

			for (int i = 0; i < chain.Length - 1; ++i)
			{
				if (chain[i] == null)
					continue;

				obj = chain[i].GetValue(obj, null);

				if (obj == null)
				{
					failReason = $"Property '{chain[i]}' is null.";
					return null;
				}
			}

			return chain[^1];
		}

		public static string GetValue(Mobile from, object o, string name)
		{
			string failReason = "";

			PropertyInfo[] chain = GetPropertyInfoChain(from, o.GetType(), name, PropertyAccess.Read, ref failReason);

			if (chain == null || chain.Length == 0)
				return failReason;

			PropertyInfo p = GetPropertyInfo(ref o, chain, ref failReason);

			return p == null ? failReason : InternalGetValue(o, p, chain);
		}

		public static string IncreaseValue(Mobile from, object o, string[] args)
		{
			_ = o.GetType();

			object[] realObjs = new object[args.Length / 2];
			PropertyInfo[] realProps = new PropertyInfo[args.Length / 2];
			int[] realValues = new int[args.Length / 2];

			bool positive = false, negative = false;

			for (int i = 0; i < realProps.Length; ++i)
			{
				string name = args[i * 2];

				try
				{
					string valueString = args[1 + (i * 2)];

					if (valueString.StartsWith("0x"))
					{
						realValues[i] = Convert.ToInt32(valueString.Substring(2), 16);
					}
					else
					{
						realValues[i] = Convert.ToInt32(valueString);
					}
				}
				catch
				{
					return "Offset value could not be parsed.";
				}

				switch (realValues[i])
				{
					case > 0:
						positive = true;
						break;
					case < 0:
						negative = true;
						break;
					default:
						return "Zero is not a valid value to offset.";
				}

				string failReason = null;
				realObjs[i] = o;
				realProps[i] = GetPropertyInfo(from, ref realObjs[i], name, PropertyAccess.ReadWrite, ref failReason);

				if (failReason != null)
					return failReason;

				if (realProps[i] == null)
					return "Property not found.";
			}

			for (int i = 0; i < realProps.Length; ++i)
			{
				object obj = realProps[i].GetValue(realObjs[i], null);

				if (!(obj is IConvertible))
					return "Property is not IConvertable.";

				try
				{
					long v = Convert.ToInt64(obj) + realValues[i];
					object toSet = Convert.ChangeType(v, realProps[i].PropertyType);
					realProps[i].SetValue(realObjs[i], toSet, null);

					EventSink.InvokeOnPropertyChanged(from, realProps[i], realObjs[i], obj, toSet);
				}
				catch
				{
					return "Value could not be converted";
				}
			}

			if (realProps.Length == 1)
			{
				return positive ? "The property has been increased." : "The property has been decreased.";
			}

			return positive switch
			{
				true when negative => "The properties have been changed.",
				true => "The properties have been increased.",
				_ => "The properties have been decreased."
			};
		}

		private static string InternalGetValue(object o, PropertyInfo p, PropertyInfo[] chain = null)
		{
			Type type = p.PropertyType;

			object value = p.GetValue(o, null);
			string toString;

			if (value == null)
				toString = "null";
			else if (IsNumeric(type))
				toString = string.Format("{0} (0x{0:X})", value);
			else if (IsChar(type))
				toString = string.Format("'{0}' ({1} [0x{1:X}])", value, (int)value);
			else if (IsString(type))
				toString = (string)value == "null" ? @"@""null""" : $"\"{value}\"";
			else if (IsText(type))
				toString = ((TextDefinition)value).Format(false);
			else
				toString = value.ToString();

			if (chain == null)
				return $"{p.Name} = {toString}";

			string[] concat = new string[chain.Length * 2 + 1];

			for (int i = 0; i < chain.Length; ++i)
			{
				concat[(i * 2) + 0] = chain[i].Name;
				concat[(i * 2) + 1] = (i < (chain.Length - 1)) ? "." : " = ";
			}

			concat[^1] = toString;

			return string.Concat(concat);
		}

		public static string SetValue(Mobile from, object o, string name, string value)
		{
			object logObject = o;

			string failReason = "";
			PropertyInfo p = GetPropertyInfo(from, ref o, name, PropertyAccess.Write, ref failReason);

			if (p == null)
				return failReason;

			return InternalSetValue(from, logObject, o, p, name, value, true);
		}

		private static readonly Type TypeofSerial = typeof(Serial);

		private static bool IsSerial(Type t)
		{
			return t == TypeofSerial;
		}

		private static readonly Type TypeofType = typeof(Type);

		private static bool IsType(Type t)
		{
			return t == TypeofType;
		}

		private static readonly Type TypeofChar = typeof(char);

		private static bool IsChar(Type t)
		{
			return t == TypeofChar;
		}

		private static readonly Type TypeofString = typeof(string);

		private static bool IsString(Type t)
		{
			return t == TypeofString;
		}

		private static readonly Type TypeofText = typeof(TextDefinition);

		private static bool IsText(Type t)
		{
			return t == TypeofText;
		}

		private static bool IsEnum(Type t)
		{
			return t.IsEnum;
		}

		private static readonly Type TypeofTimeSpan = typeof(TimeSpan);
		private static readonly Type TypeofParsable = typeof(ParsableAttribute);

		private static bool IsParsable(Type t)
		{
			return (t == TypeofTimeSpan || t.IsDefined(TypeofParsable, false));
		}

		private static readonly Type[] MParseTypes = { typeof(string) };
		private static readonly object[] MParseParams = new object[1];

		private static object Parse(object o, Type t, string value)
		{
			MethodInfo method = t.GetMethod("Parse", MParseTypes);

			MParseParams[0] = value;

			return method.Invoke(o, MParseParams);
		}

		private static readonly Type[] MNumericTypes = {
				typeof( byte ), typeof( sbyte ),
				typeof( short ), typeof( ushort ),
				typeof( int ), typeof( uint ),
				typeof( long ), typeof( ulong )
			};

		private static bool IsNumeric(Type t)
		{
			return (Array.IndexOf(MNumericTypes, t) >= 0);
		}

		public static string ConstructFromString(Type type, object obj, string value, ref object constructed)
		{
			object toSet;
			bool isSerial = IsSerial(type);

			if (isSerial) // mutate into int32
				type = MNumericTypes[4];

			if (value == "(-null-)" && !type.IsValueType)
				value = null;

			if (IsEnum(type))
			{
				try
				{
					toSet = Enum.Parse(type, value, true);
				}
				catch
				{
					return "That is not a valid enumeration member.";
				}
			}
			else if (IsType(type))
			{
				try
				{
					toSet = Assembler.FindTypeByName(value);

					if (toSet == null)
						return "No type with that name was found.";
				}
				catch
				{
					return "No type with that name was found.";
				}
			}
			else if (IsParsable(type))
			{
				try
				{
					toSet = Parse(obj, type, value);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}
			else if (value == null)
			{
				toSet = null;
			}
			else if (value.StartsWith("0x") && IsNumeric(type))
			{
				try
				{
					toSet = Convert.ChangeType(Convert.ToUInt64(value.Substring(2), 16), type);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}
			else
			{
				try
				{
					toSet = Convert.ChangeType(value, type);
				}
				catch
				{
					return "That is not properly formatted.";
				}
			}

			if (isSerial) // mutate back
			{
				toSet = new Serial(Convert.ToInt32(toSet));
			}

			constructed = toSet;
			return null;
		}

		public static string SetDirect(Mobile from, object logObject, object obj, PropertyInfo prop, string givenName, object toSet, bool shouldLog)
		{
			try
			{
				if (toSet is AccessLevel newLevel)
				{
					AccessLevel reqLevel = newLevel switch
					{
						AccessLevel.Administrator => AccessLevel.Developer,
						>= AccessLevel.Developer => AccessLevel.Owner,
						_ => AccessLevel.Administrator
					};

					if (from.AccessLevel < reqLevel)
						return "You do not have access to that level.";
				}

				if (shouldLog)
					CommandLogging.LogChangeProperty(from, logObject, givenName, toSet == null ? "(-null-)" : toSet.ToString());

				object oldValue = prop.GetValue(obj, null);
				prop.SetValue(obj, toSet, null);

				EventSink.InvokeOnPropertyChanged(from, prop, obj, oldValue, toSet);

				return "Property has been set.";
			}
			catch
			{
				return "An exception was caught, the property may not be set.";
			}
		}

		public static string SetDirect(object obj, PropertyInfo prop, object toSet)
		{
			try
			{
				if (toSet is AccessLevel)
				{
					return "You do not have access to that level.";
				}

				object oldValue = prop.GetValue(obj, null);
				prop.SetValue(obj, toSet, null);

				EventSink.InvokeOnPropertyChanged(null, prop, obj, oldValue, toSet);

				return "Property has been set.";
			}
			catch
			{
				return "An exception was caught, the property may not be set.";
			}
		}

		public static string InternalSetValue(Mobile from, object logobj, object o, PropertyInfo p, string pname, string value, bool shouldLog)
		{
			object toSet = null;
			string result = ConstructFromString(p.PropertyType, o, value, ref toSet);

			if (result != null)
				return result;

			return SetDirect(from, logobj, o, p, pname, toSet, shouldLog);
		}

		public static string InternalSetValue(object o, PropertyInfo p, string value)
		{
			object toSet = null;
			string result = ConstructFromString(p.PropertyType, o, value, ref toSet);

			if (result != null)
				return result;

			return SetDirect(o, p, toSet);
		}
	}
}

namespace Server
{
	public abstract class PropertyException : ApplicationException
	{
		protected Property MProperty;

		public Property Property => MProperty;

		public PropertyException(Property property, string message)
			: base(message)
		{
			MProperty = property;
		}
	}

	public abstract class BindingException : PropertyException
	{
		public BindingException(Property property, string message)
			: base(property, message)
		{
		}
	}

	public sealed class NotYetBoundException : BindingException
	{
		public NotYetBoundException(Property property)
			: base(property, "Property has not yet been bound.")
		{
		}
	}

	public sealed class AlreadyBoundException : BindingException
	{
		public AlreadyBoundException(Property property)
			: base(property, "Property has already been bound.")
		{
		}
	}

	public sealed class UnknownPropertyException : BindingException
	{
		public UnknownPropertyException(Property property, string current)
			: base(property, $"Property '{current}' not found.")
		{
		}
	}

	public sealed class ReadOnlyException : BindingException
	{
		public ReadOnlyException(Property property)
			: base(property, "Property is read-only.")
		{
		}
	}

	public sealed class WriteOnlyException : BindingException
	{
		public WriteOnlyException(Property property)
			: base(property, "Property is write-only.")
		{
		}
	}

	public abstract class AccessException : PropertyException
	{
		public AccessException(Property property, string message)
			: base(property, message)
		{
		}
	}

	public sealed class InternalAccessException : AccessException
	{
		public InternalAccessException(Property property)
			: base(property, "Property is internal.")
		{
		}
	}

	public abstract class ClearanceException : AccessException
	{
		protected AccessLevel MPlayerAccess;
		protected AccessLevel MNeededAccess;

		public AccessLevel PlayerAccess => MPlayerAccess;

		public AccessLevel NeededAccess => MNeededAccess;

		public ClearanceException(Property property, AccessLevel playerAccess, AccessLevel neededAccess, string accessType)
			: base(property,
				$"You must be at least {Mobile.GetAccessLevelName(neededAccess)} to {accessType} this property.")
		{
		}
	}

	public sealed class ReadAccessException : ClearanceException
	{
		public ReadAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
			: base(property, playerAccess, neededAccess, "read")
		{
		}
	}

	public sealed class WriteAccessException : ClearanceException
	{
		public WriteAccessException(Property property, AccessLevel playerAccess, AccessLevel neededAccess)
			: base(property, playerAccess, neededAccess, "write")
		{
		}
	}

	public sealed class Property
	{
		private PropertyInfo[] _mChain;

		public string Binding { get; }
		public bool IsBound => (_mChain != null);
		public PropertyAccess Access { get; private set; }

		public PropertyInfo[] Chain
		{
			get
			{
				if (!IsBound)
					throw new NotYetBoundException(this);

				return _mChain;
			}
		}

		public Type Type
		{
			get
			{
				if (!IsBound)
					throw new NotYetBoundException(this);

				return _mChain[^1].PropertyType;
			}
		}

		public bool CheckAccess(Mobile from)
		{
			if (!IsBound)
				throw new NotYetBoundException(this);

			for (int i = 0; i < _mChain.Length; ++i)
			{
				PropertyInfo prop = _mChain[i];

				bool isFinal = i == _mChain.Length - 1;

				PropertyAccess access = Access;

				if (!isFinal)
					access |= PropertyAccess.Read;

				CPA security = Properties.GetCpa(prop);

				if (security == null)
					throw new InternalAccessException(this);

				if ((access & PropertyAccess.Read) != 0 && from.AccessLevel < security.ReadLevel)
					throw new ReadAccessException(this, from.AccessLevel, security.ReadLevel);

				if ((access & PropertyAccess.Write) != 0 && (from.AccessLevel < security.WriteLevel || security.ReadOnly))
					throw new WriteAccessException(this, from.AccessLevel, security.ReadLevel);
			}

			return true;
		}

		public void BindTo(Type objectType, PropertyAccess desiredAccess)
		{
			if (IsBound)
				throw new AlreadyBoundException(this);

			string[] split = Binding.Split('.');

			PropertyInfo[] chain = new PropertyInfo[split.Length];

			for (int i = 0; i < split.Length; ++i)
			{
				bool isFinal = (i == (chain.Length - 1));

				chain[i] = objectType.GetProperty(split[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

				if (chain[i] == null)
					throw new UnknownPropertyException(this, split[i]);

				objectType = chain[i].PropertyType;

				PropertyAccess access = desiredAccess;

				if (!isFinal)
					access |= PropertyAccess.Read;

				if ((access & PropertyAccess.Read) != 0 && !chain[i].CanRead)
					throw new WriteOnlyException(this);

				if ((access & PropertyAccess.Write) != 0 && !chain[i].CanWrite)
					throw new ReadOnlyException(this);
			}

			Access = desiredAccess;
			_mChain = chain;
		}

		public Property(string binding)
		{
			Binding = binding;
		}

		public Property(PropertyInfo[] chain)
		{
			_mChain = chain;
		}

		public override string ToString()
		{
			if (!IsBound)
				return Binding;

			string[] toJoin = new string[_mChain.Length];

			for (int i = 0; i < toJoin.Length; ++i)
				toJoin[i] = _mChain[i].Name;

			return string.Join(".", toJoin);
		}

		public static Property Parse(Type type, string binding, PropertyAccess access)
		{
			Property prop = new(binding);

			prop.BindTo(type, access);

			return prop;
		}
	}
}
