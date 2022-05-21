using Server.Commands;
using Server.Targeting;
using System;

namespace Server.Items
{
	public class ItemFlags
	{
		public const int StealableFlag = 0x00200000;
		public const int TakenFlag = 0x00100000;

		public static void Initialize()
		{
			CommandSystem.Register("Stealable", AccessLevel.GameMaster, SetStealable_OnCommand);
			CommandSystem.Register("Flag", AccessLevel.GameMaster, GetFlag_OnCommand);
		}

		public static void SetStealable(Item target, bool value)
		{
			target?.SetSavedFlag(StealableFlag, value);
		}

		public static bool GetStealable(Item target)
		{
			return target != null && target.GetSavedFlag(StealableFlag);
		}

		public static void SetTaken(Item target, bool value)
		{
			target?.SetSavedFlag(TakenFlag, value);
		}

		public static bool GetTaken(Item target)
		{
			return target != null && target.GetSavedFlag(TakenFlag);
		}

		[Usage("Flag flagfield")]
		[Description("Gets the state of the specified SavedFlag on any item")]
		public static void GetFlag_OnCommand(CommandEventArgs e)
		{
			var flag = 0;
			var error = false;
			if (e.Arguments.Length > 0)
			{
				if (e.Arguments[0].StartsWith("0x"))
				{
					try
					{
						if (e.Arguments[0].Length > 2) flag = Convert.ToInt32(e.Arguments[0][2..], 16);
					} catch { error = true; }
				}
				else
				{
					try { flag = int.Parse(e.Arguments[0]); } catch { error = true; }
				}

			}
			if (!error)
			{
				e.Mobile.Target = new GetFlagTarget(e, flag);
			}
			else
			{
				try
				{
					e.Mobile.SendMessage(33, "Flag: Bad flagfield argument");
				}
				catch
				{
					// ignored
				}
			}
		}

		private class GetFlagTarget : Target
		{
			private CommandEventArgs m_E;
			private readonly int m_Flag;

			public GetFlagTarget(CommandEventArgs e, int flag) : base(30, false, TargetFlags.None)
			{
				m_E = e;
				m_Flag = flag;
			}
			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item item)
				{
					var state = item.GetSavedFlag(m_Flag);

					from.SendMessage("Flag (0x{0:X}) = {1}", m_Flag, state);
				}
				else
				{
					from.SendMessage("Must target an Item");
				}
			}
		}


		[Usage("Stealable [true/false]")]
		[Description("Sets/gets the stealable flag on any item")]
		public static void SetStealable_OnCommand(CommandEventArgs e)
		{
			var state = false;
			var error = false;
			if (e.Arguments.Length > 0)
			{
				try { state = bool.Parse(e.Arguments[0]); } catch { error = true; }

			}
			if (!error)
			{
				e.Mobile.Target = new SetStealableTarget(e, state);
			}

		}

		private class SetStealableTarget : Target
		{
			private CommandEventArgs m_E;
			private readonly bool m_State;
			private readonly bool m_Set;

			public SetStealableTarget(CommandEventArgs e, bool state) : base(30, false, TargetFlags.None)
			{
				m_E = e;
				m_State = state;
				if (e.Arguments.Length > 0)
				{
					m_Set = true;
				}
			}
			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Item item)
				{
					if (m_Set)
					{
						SetStealable(item, m_State);
					}

					var state = GetStealable(item);

					from.SendMessage("Stealable = {0}", state);

				}
				else
				{
					from.SendMessage("Must target an Item");
				}
			}
		}
	}
}
