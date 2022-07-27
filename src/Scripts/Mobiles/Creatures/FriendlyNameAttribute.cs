using System;

namespace Server.Mobiles;

[AttributeUsage(AttributeTargets.Class)]
public class FriendlyNameAttribute : Attribute
{
	public TextDefinition FriendlyName { get; }

	public FriendlyNameAttribute(TextDefinition friendlyName)
	{
		FriendlyName = friendlyName;
	}

	public static TextDefinition GetFriendlyNameFor(Type t)
	{
		if (!t.IsDefined(typeof(FriendlyNameAttribute), false))
			return t.Name;

		object[] objs = t.GetCustomAttributes(typeof(FriendlyNameAttribute), false);

		if (objs is not { Length: > 0 })
			return t.Name;

		if (objs[0] is FriendlyNameAttribute friendly) return friendly.FriendlyName;

		return t.Name;
	}
}
