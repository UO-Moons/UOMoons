using System;

namespace Server.Misc
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DispellableAttributes : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class DispellableAttribute : Attribute
	{
	}
}
