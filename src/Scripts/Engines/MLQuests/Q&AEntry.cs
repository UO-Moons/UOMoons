namespace Server.Engines.Quests;

public class QuestionAndAnswerEntry
{
	public int Question { get; }
	public object[] Answers { get; }
	public object[] WrongAnswers { get; }

	public QuestionAndAnswerEntry(int question, object[] answerText, object[] wrongAnswers)
	{
		Question = question;
		Answers = answerText;
		WrongAnswers = wrongAnswers;
	}
}
