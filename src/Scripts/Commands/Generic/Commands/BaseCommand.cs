using Server.Gumps;
using System.Collections;

namespace Server.Commands.Generic;

public enum ObjectTypes
{
	Both,
	Items,
	Mobiles,
	All
}

public abstract class BaseCommand
{
	private readonly ArrayList _mResponses;
	private readonly ArrayList _mFailures;
	public bool ListOptimized { get; set; }
	public string[] Commands { get; set; }
	public string Usage { get; set; }
	public string Description { get; set; }
	public AccessLevel AccessLevel { get; set; }
	public ObjectTypes ObjectTypes { get; set; }
	public CommandSupport Supports { get; set; }

	public BaseCommand()
	{
		_mResponses = new ArrayList();
		_mFailures = new ArrayList();
	}

	public static bool IsAccessible(Mobile from, object obj)
	{
		if (from.AccessLevel >= AccessLevel.Administrator || obj == null)
			return true;

		Mobile mob;

		if (obj is Mobile mobile)
			mob = mobile;
		else if (obj is Item item)
			mob = item.RootParent as Mobile;
		else
			mob = null;

		if (mob == null || mob == from || from.AccessLevel > mob.AccessLevel)
			return true;

		return false;
	}

	public virtual void ExecuteList(CommandEventArgs e, ArrayList list)
	{
		for (int i = 0; i < list.Count; ++i)
			Execute(e, list[i]);
	}

	public virtual void Execute(CommandEventArgs e, object obj)
	{
	}

	public virtual bool ValidateArgs(BaseCommandImplementor impl, CommandEventArgs e)
	{
		return true;
	}

	private class MessageEntry
	{
		public readonly string MMessage;
		public int MCount;

		public MessageEntry(string message)
		{
			MMessage = message;
			MCount = 1;
		}

		public override string ToString()
		{
			return MCount > 1 ? $"{MMessage} ({MCount})" : MMessage;
		}
	}

	public void AddResponse(string message)
	{
		for (int i = 0; i < _mResponses.Count; ++i)
		{
			MessageEntry entry = (MessageEntry)_mResponses[i];

			if (entry != null && entry.MMessage == message)
			{
				++entry.MCount;
				return;
			}
		}

		if (_mResponses.Count == 10)
			return;

		_mResponses.Add(new MessageEntry(message));
	}

	public void AddResponse(Gump gump)
	{
		_mResponses.Add(gump);
	}

	public void LogFailure(string message)
	{
		for (int i = 0; i < _mFailures.Count; ++i)
		{
			MessageEntry entry = (MessageEntry)_mFailures[i];

			if (entry != null && entry.MMessage == message)
			{
				++entry.MCount;
				return;
			}
		}

		if (_mFailures.Count == 10)
			return;

		_mFailures.Add(new MessageEntry(message));
	}

	public void Flush(Mobile from, bool flushToLog)
	{
		if (_mResponses.Count > 0)
		{
			for (int i = 0; i < _mResponses.Count; ++i)
			{
				object obj = _mResponses[i];

				if (obj is MessageEntry entry)
				{
					from.SendMessage(entry.ToString());

					if (flushToLog)
						CommandLogging.WriteLine(from, entry.ToString());
				}
				else if (obj is Gump gump)
				{
					from.SendGump(gump);
				}
			}
		}
		else
		{
			for (int i = 0; i < _mFailures.Count; ++i)
				from.SendMessage(((MessageEntry)_mFailures[i])?.ToString());
		}

		_mResponses.Clear();
		_mFailures.Clear();
	}
}
