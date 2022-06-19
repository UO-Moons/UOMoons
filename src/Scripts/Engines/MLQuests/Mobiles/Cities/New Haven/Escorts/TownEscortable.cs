using System;
using Server.Items;
using Server.Engines.Quests;

namespace Server.Mobiles;

public class TownEscortable : BaseEscort
{
	private static readonly Type[] MQuests = {
		typeof(EscortToYewQuest),
		typeof(EscortToVesperQuest),
		typeof(EscortToTrinsicQuest),
		typeof(EscortToSkaraQuest),
		typeof(EscortToSerpentsHoldQuest),
		typeof(EscortToNujelmQuest),
		typeof(EscortToMoonglowQuest),
		typeof(EscortToMinocQuest),
		typeof(EscortToMaginciaQuest),
		typeof(EscortToJhelomQuest),
		typeof(EscortToCoveQuest),
		typeof(EscortToBritainQuest)
	};

	private static readonly string[] MDestinations = {
		"Yew",
		"Vesper",
		"Trinsic",
		"Skara Brae",
		"Serpent's Hold",
		"Nujel'm",
		"Moonglow",
		"Minoc",
		"Magincia",
		"Jhelom",
		"Cove",
		"Britain"
	};

	private int _mQuest;

	public TownEscortable()
	{
		_mQuest = Utility.Random(MQuests.Length);
	}

	protected override void OnMapChange(Map oldMap)
	{
		base.OnMapChange(oldMap);

		if (MDestinations[_mQuest] == Region.Name)
		{
			_mQuest = RandomDestination();
		}
	}

	private int RandomDestination()
	{
		int random;

		do
		{
			random = Utility.Random(MDestinations.Length);
		}
		while (MDestinations[random] == Region.Find(Location, Map).Name);

		return random;
	}

	public TownEscortable(Serial serial)
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

		writer.WriteEncodedInt(1); // version

		writer.Write(_mQuest);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadEncodedInt();

		_mQuest = reader.ReadInt();

		if (version == 0 && MDestinations[_mQuest] == Region.Name)
		{
			_mQuest = RandomDestination();
			Console.WriteLine("Adjusting escort destination.");
		}
	}
}
