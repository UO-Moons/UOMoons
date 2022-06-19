using Server.Network;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Gumps;

public class HumilityItemQuestGump : Gump
{
	private readonly HumilityQuestMobile _mMobile;
	private readonly WhosMostHumbleQuest _mQuest;
	private readonly int _mNpcIndex;

	public HumilityItemQuestGump(HumilityQuestMobile mobile, WhosMostHumbleQuest quest, int index) : base(50, 50)
	{
		_mMobile = mobile;
		_mQuest = quest;
		_mNpcIndex = index;

		AddBackground(0, 0, 350, 250, 2600);
		AddHtml(100, 25, 175, 16, $"{mobile.Name} {mobile.Title}", false, false);

		AddHtmlLocalized(40, 60, 270, 140, mobile.Greeting + 1, 1, false, true);
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (_mNpcIndex < 0 || _mNpcIndex >= _mQuest.Infos.Count)
		{
			return;
		}

		Mobile from = state.Mobile;

		int cliloc;
		string args;

		if (0.5 > Utility.RandomDouble() || _mNpcIndex == 6)
		{
			cliloc = _mMobile.Greeting + 2;
			args = $"#{_mQuest.Infos[_mNpcIndex].NeedsLoc}";
		}
		else
		{
			cliloc = _mMobile.Greeting + 3;
			args = $"#{_mQuest.Infos[_mNpcIndex].GivesLoc}";
		}

		_mMobile.SayTo(from, cliloc, args);
	}
}
