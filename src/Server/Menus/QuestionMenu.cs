using Server.Network;

namespace Server.Menus;

public class QuestionMenu : IMenu
{
	private readonly int _serial;
	private static int _nextSerial;

	int IMenu.Serial => _serial;

	int IMenu.EntryLength => Answers.Length;

	public string Question { get; set; }

	public string[] Answers { get; }

	public QuestionMenu(string question, string[] answers)
	{
		Question = question;
		Answers = answers;

		do
		{
			_serial = ++_nextSerial;
			_serial &= 0x7FFFFFFF;
		} while (_serial == 0);
	}

	public virtual void OnCancel(NetState state)
	{
	}

	public virtual void OnResponse(NetState state, int index)
	{
	}

	public void SendTo(NetState state)
	{
		state.AddMenu(this);
		state.Send(new DisplayQuestionMenu(this));
	}
}
