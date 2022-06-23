using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Commands.Generic
{
	public sealed class SortExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new(40, "Order", -1, () => new SortExtension());

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private readonly List<OrderInfo> _mOrders;

		private IComparer _mComparer;

		public SortExtension()
		{
			_mOrders = new List<OrderInfo>();
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
				throw new Exception("The ordering extension may only be used in combination with an object conditional.");

			foreach (OrderInfo order in _mOrders)
			{
				order.Property.BindTo(baseType, PropertyAccess.Read);
				order.Property.CheckAccess(from);
			}

			if (assembly == null)
				assembly = new AssemblyEmitter("__dynamic", false);

			_mComparer = SortCompiler.Compile(assembly, baseType, _mOrders.ToArray());
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
				throw new Exception("Invalid ordering syntax.");

			if (Insensitive.Equals(arguments[offset], "by"))
			{
				++offset;
				--size;

				if (size < 1)
					throw new Exception("Invalid ordering syntax.");
			}

			int end = offset + size;

			while (offset < end)
			{
				string binding = arguments[offset++];

				bool isAscending = true;

				if (offset < end)
				{
					string next = arguments[offset];

					switch (next.ToLower())
					{
						case "+":
						case "up":
						case "asc":
						case "ascending":
							isAscending = true;
							++offset;
							break;

						case "-":
						case "down":
						case "desc":
						case "descending":
							isAscending = false;
							++offset;
							break;
					}
				}

				Property property = new(binding);

				_mOrders.Add(new OrderInfo(property, isAscending));
			}
		}

		public override void Filter(ArrayList list)
		{
			if (_mComparer == null)
				throw new InvalidOperationException("The extension must first be optimized.");

			list.Sort(_mComparer);
		}
	}
}
