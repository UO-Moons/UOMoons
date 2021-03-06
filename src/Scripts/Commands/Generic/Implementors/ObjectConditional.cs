using System;
using System.Collections.Generic;

namespace Server.Commands.Generic;

public sealed class ObjectConditional
{
	private static readonly Type TypeofItem = typeof(Item);
	private static readonly Type TypeofMobile = typeof(Mobile);
	private readonly ICondition[][] _mConditions;

	private IConditional[] _mConditionals;

	public Type Type { get; }

	public bool IsItem => (Type == null || Type == TypeofItem || Type.IsSubclassOf(TypeofItem));

	public bool IsMobile => (Type == null || Type == TypeofMobile || Type.IsSubclassOf(TypeofMobile));

	public static readonly ObjectConditional Empty = new(null, null);

	public bool HasCompiled => _mConditionals != null;

	public void Compile(ref AssemblyEmitter emitter)
	{
		if (emitter == null)
			emitter = new AssemblyEmitter("__dynamic", false);

		_mConditionals = new IConditional[_mConditions.Length];

		for (int i = 0; i < _mConditionals.Length; ++i)
			_mConditionals[i] = ConditionalCompiler.Compile(emitter, Type, _mConditions[i], i);
	}

	public bool CheckCondition(object obj)
	{
		if (Type == null)
			return true; // null type means no condition

		if (!HasCompiled)
		{
			AssemblyEmitter emitter = null;

			Compile(ref emitter);
		}

		for (int i = 0; i < _mConditionals.Length; ++i)
		{
			if (_mConditionals[i].Verify(obj))
				return true;
		}

		return false; // all conditions false
	}

	public static ObjectConditional Parse(Mobile from, ref string[] args)
	{
		string[] conditionArgs = null;

		for (int i = 0; i < args.Length; ++i)
		{
			if (Insensitive.Equals(args[i], "where"))
			{
				string[] origArgs = args;

				args = new string[i];

				for (int j = 0; j < args.Length; ++j)
					args[j] = origArgs[j];

				conditionArgs = new string[origArgs.Length - i - 1];

				for (int j = 0; j < conditionArgs.Length; ++j)
					conditionArgs[j] = origArgs[i + j + 1];

				break;
			}
		}

		return ParseDirect(from, conditionArgs, 0, conditionArgs.Length);
	}

	public static ObjectConditional ParseDirect(Mobile from, string[] args, int offset, int size)
	{
		if (args == null || size == 0)
			return Empty;

		int index = 0;

		Type objectType = Assembler.FindTypeByName(args[offset + index], true);

		if (objectType == null)
			throw new Exception($"No type with that name ({args[offset + index]}) was found.");

		++index;

		List<ICondition[]> conditions = new();
		List<ICondition> current = new()
		{
			TypeCondition.Default
		};

		while (index < size)
		{
			string cur = args[offset + index];

			bool inverse = false;

			if (Insensitive.Equals(cur, "not") || cur == "!")
			{
				inverse = true;
				++index;

				if (index >= size)
					throw new Exception("Improperly formatted object conditional.");
			}
			else if (Insensitive.Equals(cur, "or") || cur == "||")
			{
				if (current.Count > 1)
				{
					conditions.Add(current.ToArray());

					current.Clear();
					current.Add(TypeCondition.Default);
				}

				++index;

				continue;
			}

			string binding = args[offset + index];
			index++;

			if (index >= size)
				throw new Exception("Improperly formatted object conditional.");

			string oper = args[offset + index];
			index++;

			if (index >= size)
				throw new Exception("Improperly formatted object conditional.");

			string val = args[offset + index];
			index++;

			Property prop = new Property(binding);

			prop.BindTo(objectType, PropertyAccess.Read);
			prop.CheckAccess(from);

			ICondition condition = null;

			switch (oper)
			{
				#region Equality
				case "=":
				case "==":
				case "is":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Equal, val);
					break;

				case "!=":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.NotEqual, val);
					break;
				#endregion

				#region Relational
				case ">":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Greater, val);
					break;

				case "<":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.Lesser, val);
					break;

				case ">=":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.GreaterEqual, val);
					break;

				case "<=":
					condition = new ComparisonCondition(prop, inverse, ComparisonOperator.LesserEqual, val);
					break;
				#endregion

				#region Strings
				case "==~":
				case "~==":
				case "=~":
				case "~=":
				case "is~":
				case "~is":
					condition = new StringCondition(prop, inverse, StringOperator.Equal, val, true);
					break;

				case "!=~":
				case "~!=":
					condition = new StringCondition(prop, inverse, StringOperator.NotEqual, val, true);
					break;

				case "starts":
					condition = new StringCondition(prop, inverse, StringOperator.StartsWith, val, false);
					break;

				case "starts~":
				case "~starts":
					condition = new StringCondition(prop, inverse, StringOperator.StartsWith, val, true);
					break;

				case "ends":
					condition = new StringCondition(prop, inverse, StringOperator.EndsWith, val, false);
					break;

				case "ends~":
				case "~ends":
					condition = new StringCondition(prop, inverse, StringOperator.EndsWith, val, true);
					break;

				case "contains":
					condition = new StringCondition(prop, inverse, StringOperator.Contains, val, false);
					break;

				case "contains~":
				case "~contains":
					condition = new StringCondition(prop, inverse, StringOperator.Contains, val, true);
					break;
				#endregion
			}

			if (condition == null)
				throw new InvalidOperationException($"Unrecognized operator (\"{oper}\").");

			current.Add(condition);
		}

		conditions.Add(current.ToArray());

		return new ObjectConditional(objectType, conditions.ToArray());
	}

	public ObjectConditional(Type objectType, ICondition[][] conditions)
	{
		Type = objectType;
		_mConditions = conditions;
	}
}
