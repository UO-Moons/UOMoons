using System;

namespace Server.Engines.Quests
{
    public class QuestionAndAnswerEntry
    {
        private int m_Question;
        private object[] m_Answers;
        private object[] m_WrongAnswers;

        public int Question => m_Question;
        public object[] Answers => m_Answers;
        public object[] WrongAnswers => m_WrongAnswers;

        public QuestionAndAnswerEntry(int question, object[] answerText, object[] wrongAnswers)
        {
            m_Question = question;
            m_Answers = answerText;
            m_WrongAnswers = wrongAnswers;
        }
    }
}