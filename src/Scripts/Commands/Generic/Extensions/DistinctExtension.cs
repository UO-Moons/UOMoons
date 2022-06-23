using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Commands.Generic
{
	public sealed class DistinctExtension : BaseExtension
	{
		public static ExtensionInfo ExtInfo = new(30, "Distinct", -1, () => new DistinctExtension());

		public static void Initialize()
		{
			ExtensionInfo.Register(ExtInfo);
		}

		public override ExtensionInfo Info => ExtInfo;

		private readonly List<Property> _mProperties;

		private IComparer _mComparer;

		public DistinctExtension()
		{
			_mProperties = new List<Property>();
		}

		public override void Optimize(Mobile from, Type baseType, ref AssemblyEmitter assembly)
		{
			if (baseType == null)
				throw new Exception("Distinct extension may only be used in combination with an object conditional.");

			foreach (Property prop in _mProperties)
			{
				prop.BindTo(baseType, PropertyAccess.Read);
				prop.CheckAccess(from);
			}

			assembly ??= new AssemblyEmitter("__dynamic", false);

			_mComparer = DistinctCompiler.Compile(assembly, baseType, _mProperties.ToArray());
		}

		public override void Parse(Mobile from, string[] arguments, int offset, int size)
		{
			if (size < 1)
				throw new Exception("Invalid distinction syntax.");

			int end = offset + size;

			while (offset < end)
			{
				string binding = arguments[offset++];

				_mProperties.Add(new Property(binding));
			}
		}

		public override void Filter(ArrayList list)
		{
			if (_mComparer == null)
				throw new InvalidOperationException("The extension must first be optimized.");

			ArrayList copy = new(list);

			copy.Sort(_mComparer);

			list.Clear();

			object last = null;

			for (int i = 0; i < copy.Count; ++i)
			{
				object obj = copy[i];

				if (last == null || _mComparer.Compare(obj, last) != 0)
				{
					list.Add(obj);
					last = obj;
				}
			}
		}
	}
}
