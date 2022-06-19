using System;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class NewHavenEscortable : BaseEscort
{
	private static readonly Type[] MQuests = new[]
	{
		typeof(NewHavenAlchemistEscortQuest),
		typeof(NewHavenBardEscortQuest),
		typeof(NewHavenWarriorEscortQuest),
		typeof(NewHavenTailorEscortQuest),
		typeof(NewHavenCarpenterEscortQuest),
		typeof(NewHavenMapmakerEscortQuest),
		typeof(NewHavenMageEscortQuest),
		typeof(NewHavenInnEscortQuest),
		typeof(NewHavenFarmEscortQuest),
		typeof(NewHavenDocksEscortQuest),
		typeof(NewHavenBowyerEscortQuest),
		typeof(NewHavenBankEscortQuest)
	};

	private static readonly string[] MDestinations = new[]
	{
		"the New Haven Alchemist",
		"the New Haven Bard",
		"the New Haven Warrior",
		"the New Haven Tailor",
		"the New Haven Carpenter",
		"the New Haven Mapmaker",
		"the New Haven Mage",
		"the New Haven Inn",
		"the New Haven Farm",
		"the New Haven Docks",
		"the New Haven Bowyer",
		"the New Haven Bank"
	};

	private int _mQuest;

	public NewHavenEscortable()
		: this(Utility.Random(12))
	{
	}

	public NewHavenEscortable(int quest)
	{
		_mQuest = quest;
	}

	public NewHavenEscortable(Serial serial)
		: base(serial)
	{
	}

	public override Type[] Quests => new[] { MQuests[_mQuest] };
	public override void Advertise()
	{
		Say(Utility.RandomMinMax(1072301, 1072303));
	}

	public override Region GetDestination()
	{
		return QuestHelper.FindRegion(MDestinations[_mQuest]);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.WriteEncodedInt(0);
		writer.Write(_mQuest);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadEncodedInt();
		_mQuest = reader.ReadInt();
	}
}
