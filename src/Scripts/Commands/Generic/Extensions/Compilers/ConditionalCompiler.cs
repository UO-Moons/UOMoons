using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace Server.Commands.Generic
{
	public interface IConditional
	{
		bool Verify(object obj);
	}

	public interface ICondition
	{
		// Invoked during the constructor
		void Construct(TypeBuilder typeBuilder, ILGenerator il, int index);

		// Target object will be loaded on the stack
		void Compile(MethodEmitter emitter);
	}

	public sealed class TypeCondition : ICondition
	{
		public static TypeCondition Default = new();

		void ICondition.Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
		{
		}

		void ICondition.Compile(MethodEmitter emitter)
		{
			// The object was safely cast to be the conditionals type
			// If it's null, then the type cast didn't work...

			emitter.LoadNull();
			emitter.Compare(OpCodes.Ceq);
			emitter.LogicalNot();
		}
	}

	public sealed class PropertyValue
	{
		public Type Type { get; }
		public object Value { get; private set; }
		public FieldInfo Field { get; private set; }
		public bool HasField => (Field != null);

		public PropertyValue(Type type, object value)
		{
			Type = type;
			Value = value;
		}

		public void Load(MethodEmitter method)
		{
			if (Field != null)
			{
				method.LoadArgument(0);
				method.LoadField(Field);
			}
			else if (Value == null)
			{
				method.LoadNull(Type);
			}
			else
			{
				switch (Value)
				{
					case int value:
						method.Load(value);
						break;
					case long l:
						method.Load(l);
						break;
					case float f:
						method.Load(f);
						break;
					case double d:
						method.Load(d);
						break;
					case char c:
						method.Load(c);
						break;
					case bool b:
						method.Load(b);
						break;
					case string s:
						method.Load(s);
						break;
					case Enum @enum:
						method.Load(@enum);
						break;
					default:
						throw new InvalidOperationException("Unrecognized comparison value.");
				}
			}
		}

		public void Acquire(TypeBuilder typeBuilder, ILGenerator il, string fieldName)
		{
			if (Value is string toParse)
			{
				if (!Type.IsValueType && toParse == "null")
				{
					Value = null;
				}
				else if (Type == typeof(string))
				{
					if (toParse == @"@""null""")
						toParse = "null";

					Value = toParse;
				}
				else if (Type.IsEnum)
				{
					Value = Enum.Parse(Type, toParse, true);
				}
				else
				{
					MethodInfo parseMethod;
					object[] parseArgs;

					MethodInfo parseNumber = Type.GetMethod(
						"Parse",
						BindingFlags.Public | BindingFlags.Static,
						null,
						new[] { typeof(string), typeof(NumberStyles) },
						null
					);

					if (parseNumber != null)
					{
						NumberStyles style = NumberStyles.Integer;

						if (Insensitive.StartsWith(toParse, "0x"))
						{
							style = NumberStyles.HexNumber;
							toParse = toParse[2..];
						}

						parseMethod = parseNumber;
						parseArgs = new object[] { toParse, style };
					}
					else
					{
						MethodInfo parseGeneral = Type.GetMethod(
							"Parse",
							BindingFlags.Public | BindingFlags.Static,
							null,
							new[] { typeof(string) },
							null
						);

						parseMethod = parseGeneral;
						parseArgs = new object[] {toParse};
					}

					if (parseMethod != null)
					{
						Value = parseMethod.Invoke(null, parseArgs);

						if (!Type.IsPrimitive)
						{
							Field = typeBuilder.DefineField(
								fieldName,
								Type,
								FieldAttributes.Private | FieldAttributes.InitOnly
							);

							il.Emit(OpCodes.Ldarg_0);

							il.Emit(OpCodes.Ldstr, toParse);

							if (parseArgs.Length == 2) // dirty evil hack :-(
								il.Emit(OpCodes.Ldc_I4, (int)parseArgs[1]);

							il.Emit(OpCodes.Call, parseMethod);
							il.Emit(OpCodes.Stfld, Field);
						}
					}
					else
					{
						throw new InvalidOperationException(
							$"Unable to convert string \"{Value}\" into type '{Type}'."
						);
					}
				}
			}
		}
	}

	public abstract class PropertyCondition : ICondition
	{
		protected Property MProperty;
		protected bool MNot;

		public PropertyCondition(Property property, bool not)
		{
			MProperty = property;
			MNot = not;
		}

		public abstract void Construct(TypeBuilder typeBuilder, ILGenerator il, int index);

		public abstract void Compile(MethodEmitter emitter);
	}

	public enum StringOperator
	{
		Equal,
		NotEqual,

		Contains,

		StartsWith,
		EndsWith
	}

	public sealed class StringCondition : PropertyCondition
	{
		private readonly StringOperator _mOperator;
		private readonly PropertyValue _mValue;

		private readonly bool _mIgnoreCase;

		public StringCondition(Property property, bool not, StringOperator op, object value, bool ignoreCase)
			: base(property, not)
		{
			_mOperator = op;
			_mValue = new PropertyValue(property.Type, value);

			_mIgnoreCase = ignoreCase;
		}

		public override void Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
		{
			_mValue.Acquire(typeBuilder, il, "v" + index);
		}

		public override void Compile(MethodEmitter emitter)
		{
			bool inverse = false;

			string methodName;

			switch (_mOperator)
			{
				case StringOperator.Equal:
					methodName = "Equals";
					break;

				case StringOperator.NotEqual:
					methodName = "Equals";
					inverse = true;
					break;

				case StringOperator.Contains:
					methodName = "Contains";
					break;

				case StringOperator.StartsWith:
					methodName = "StartsWith";
					break;

				case StringOperator.EndsWith:
					methodName = "EndsWith";
					break;

				default:
					throw new InvalidOperationException("Invalid string comparison operator.");
			}

			if (_mIgnoreCase || methodName == "Equals")
			{
				Type type = (_mIgnoreCase ? typeof(Insensitive) : typeof(string));

				emitter.BeginCall(
					type.GetMethod(
						methodName,
						BindingFlags.Public | BindingFlags.Static,
						null,
						new[]
						{
							typeof( string ),
							typeof( string )
						},
						null
					)
				);

				emitter.Chain(MProperty);
				_mValue.Load(emitter);

				emitter.FinishCall();
			}
			else
			{
				Label notNull = emitter.CreateLabel();
				Label moveOn = emitter.CreateLabel();

				LocalBuilder temp = emitter.AcquireTemp(MProperty.Type);

				emitter.Chain(MProperty);

				emitter.StoreLocal(temp);
				emitter.LoadLocal(temp);

				emitter.BranchIfTrue(notNull);

				emitter.Load(false);
				emitter.Pop();
				emitter.Branch(moveOn);

				emitter.MarkLabel(notNull);
				emitter.LoadLocal(temp);

				emitter.BeginCall(
					typeof(string).GetMethod(
						methodName,
						BindingFlags.Public | BindingFlags.Instance,
						null,
						new[]
						{
							typeof( string )
						},
						null
					)
				);

				_mValue.Load(emitter);

				emitter.FinishCall();

				emitter.MarkLabel(moveOn);
			}

			if (MNot != inverse)
				emitter.LogicalNot();
		}
	}

	public enum ComparisonOperator
	{
		Equal,
		NotEqual,
		Greater,
		GreaterEqual,
		Lesser,
		LesserEqual
	}

	public sealed class ComparisonCondition : PropertyCondition
	{
		private readonly ComparisonOperator _mOperator;
		private readonly PropertyValue _mValue;

		public ComparisonCondition(Property property, bool not, ComparisonOperator op, object value)
			: base(property, not)
		{
			_mOperator = op;
			_mValue = new PropertyValue(property.Type, value);
		}

		public override void Construct(TypeBuilder typeBuilder, ILGenerator il, int index)
		{
			_mValue.Acquire(typeBuilder, il, "v" + index);
		}

		public override void Compile(MethodEmitter emitter)
		{
			emitter.Chain(MProperty);

			bool inverse = false;

			bool couldCompare =
			emitter.CompareTo(1, delegate ()
		   {
			   _mValue.Load(emitter);
		   });

			if (couldCompare)
			{
				emitter.Load(0);

				switch (_mOperator)
				{
					case ComparisonOperator.Equal:
						emitter.Compare(OpCodes.Ceq);
						break;

					case ComparisonOperator.NotEqual:
						emitter.Compare(OpCodes.Ceq);
						inverse = true;
						break;

					case ComparisonOperator.Greater:
						emitter.Compare(OpCodes.Cgt);
						break;

					case ComparisonOperator.GreaterEqual:
						emitter.Compare(OpCodes.Clt);
						inverse = true;
						break;

					case ComparisonOperator.Lesser:
						emitter.Compare(OpCodes.Clt);
						break;

					case ComparisonOperator.LesserEqual:
						emitter.Compare(OpCodes.Cgt);
						inverse = true;
						break;

					default:
						throw new InvalidOperationException("Invalid comparison operator.");
				}
			}
			else
			{
				// This type is -not- comparable
				// We can only support == and != operations

				_mValue.Load(emitter);

				switch (_mOperator)
				{
					case ComparisonOperator.Equal:
						emitter.Compare(OpCodes.Ceq);
						break;

					case ComparisonOperator.NotEqual:
						emitter.Compare(OpCodes.Ceq);
						inverse = true;
						break;

					case ComparisonOperator.Greater:
					case ComparisonOperator.GreaterEqual:
					case ComparisonOperator.Lesser:
					case ComparisonOperator.LesserEqual:
						throw new InvalidOperationException("Property does not support relational comparisons.");

					default:
						throw new InvalidOperationException("Invalid operator.");
				}
			}

			if (MNot != inverse)
				emitter.LogicalNot();
		}
	}

	public static class ConditionalCompiler
	{
		public static IConditional Compile(AssemblyEmitter assembly, Type objectType, ICondition[] conditions, int index)
		{
			TypeBuilder typeBuilder = assembly.DefineType(
					"__conditional" + index,
					TypeAttributes.Public,
					typeof(object)
				);

			#region Constructor
			{
				ConstructorBuilder ctor = typeBuilder.DefineConstructor(
						MethodAttributes.Public,
						CallingConventions.Standard,
						Type.EmptyTypes
					);

				ILGenerator il = ctor.GetILGenerator();

				// : base()
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);

				for (int i = 0; i < conditions.Length; ++i)
					conditions[i].Construct(typeBuilder, il, i);

				// return;
				il.Emit(OpCodes.Ret);
			}
			#endregion

			#region IComparer
			typeBuilder.AddInterfaceImplementation(typeof(IConditional));

			#region Compare
			{
				MethodEmitter emitter = new(typeBuilder);

				emitter.Define(
					/*  name  */ "Verify",
					/*  attr  */ MethodAttributes.Public | MethodAttributes.Virtual,
					/* return */ typeof(bool),
					/* params */ new Type[] { typeof(object) });

				LocalBuilder obj = emitter.CreateLocal(objectType);
				LocalBuilder eq = emitter.CreateLocal(typeof(bool));

				emitter.LoadArgument(1);
				emitter.CastAs(objectType);
				emitter.StoreLocal(obj);

				Label done = emitter.CreateLabel();

				for (int i = 0; i < conditions.Length; ++i)
				{
					if (i > 0)
					{
						emitter.LoadLocal(eq);

						emitter.BranchIfFalse(done);
					}

					emitter.LoadLocal(obj);

					conditions[i].Compile(emitter);

					emitter.StoreLocal(eq);
				}

				emitter.MarkLabel(done);

				emitter.LoadLocal(eq);

				emitter.Return();

				typeBuilder.DefineMethodOverride(
						emitter.Method,
						typeof(IConditional).GetMethod(
							"Verify",
							new[]
								{
									typeof( object )
								}
						)
					);
			}
			#endregion
			#endregion

			Type conditionalType = typeBuilder.CreateType();

			return (IConditional)Activator.CreateInstance(conditionalType);
		}
	}
}
