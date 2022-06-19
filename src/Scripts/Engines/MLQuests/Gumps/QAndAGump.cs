using Server.Network;
using Server.Engines.Quests;
using System.Collections.Generic;

namespace Server.Gumps;

public class QAndAGump : Gump
{
	private const int FontColor = 0x000008;

	private readonly Mobile _mFrom;
	private readonly QuestionAndAnswerObjective _mObjective;
	private readonly BaseQuest _mQuest;

	public QAndAGump(Mobile owner, BaseQuest quest) : base(0, 0)
	{
		_mFrom = owner;
		_mQuest = quest;
		Closable = false;
		Disposable = false;

		foreach (BaseObjective objective in quest.Objectives)
		{
			if (objective is not QuestionAndAnswerObjective answerObjective) continue;
			_mObjective = answerObjective;
			break;
		}

		QuestionAndAnswerEntry entry = _mObjective?.GetRandomQandA();

		if (entry == null)
			return;

		AddPage(0);
		AddImage(0, 0, 1228);
		AddImage(40, 78, 95);
		AddImageTiled(49, 87, 301, 3, 96);
		AddImage(350, 78, 97);

		object answer = entry.Answers[Utility.Random(entry.Answers.Length)];

		List<object> selections = new(entry.WrongAnswers);
		var mIndex = Utility.Random(selections.Count);
		selections.Insert(mIndex, answer);

		AddHtmlLocalized(40, 40, 320, 40, entry.Question, FontColor, false, false); //question

		for (var i = 0; i < selections.Count; i++)
		{
			object selection = selections[i];

			AddButton(49, 104 + (i * 40), 2224, 2224, selection == answer ? 1 : 0, GumpButtonType.Reply, 0);

			if (selection is int selection1)
				AddHtmlLocalized(80, 102 + (i * 40), 200, 18, selection1, 0x0, false, false);
			else
				AddHtml(80, 102 + (i * 40), 200, 18, $"<BASEFONT COLOR=#{FontColor:X6}>{selection}</BASEFONT>", false, false);
		}
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (info.ButtonID == 1) //correct answer
		{
			_mObjective.Update(null);

			if (_mQuest.Completed)
			{
				_mQuest.OnCompleted();
				_mFrom.SendGump(new MondainQuestGump(_mQuest, MondainQuestGump.Section.Complete, false, true));
			}
			else
			{
				_mFrom.SendGump(new QAndAGump(_mFrom, _mQuest));
			}
		}
		else
		{
			_mFrom.SendGump(new MondainQuestGump(_mQuest, MondainQuestGump.Section.Failed, false, true));
			_mQuest.OnResign(false);
		}
	}
}
