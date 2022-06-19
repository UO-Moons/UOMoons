using System;
using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles;

public class Scribe : BaseVendor
{
	private readonly List<SbInfo> _mSbInfos = new();
	protected override List<SbInfo> SbInfos => _mSbInfos;

	public override NpcGuild NpcGuild => NpcGuild.MagesGuild;

	private DateTime _mNextShush;
	public static readonly TimeSpan ShushDelay = TimeSpan.FromMinutes(1);

	[Constructable]
	public Scribe() : base("the scribe")
	{
		Job = JobFragment.scholar;
		Karma = Utility.RandomMinMax(13, -45);
		SetSkill(SkillName.EvalInt, 60.0, 83.0);
		SetSkill(SkillName.Inscribe, 90.0, 100.0);
	}

	public override void InitSbInfo()
	{
		_mSbInfos.Add(new SbScribe());
	}

	public override VendorShoeType ShoeType => Utility.RandomBool() ? VendorShoeType.Shoes : VendorShoeType.Sandals;

	public override void InitOutfit()
	{
		base.InitOutfit();

		AddItem(new Robe(Utility.RandomNeutralHue()));
	}

	public override bool HandlesOnSpeech(Mobile from)
	{
		return from.Player;
	}

	public override void OnSpeech(SpeechEventArgs e)
	{
		base.OnSpeech(e);

		if (!e.Handled && _mNextShush <= DateTime.UtcNow && InLOS(e.Mobile))
		{
			Direction = GetDirectionTo(e.Mobile);

			PlaySound(Female ? 0x32F : 0x441);
			PublicOverheadMessage(Network.MessageType.Regular, 0x3B2, 1073990); // Shhhh!

			_mNextShush = DateTime.UtcNow + ShushDelay;
			e.Handled = true;
		}
	}

	public Scribe(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		reader.ReadInt();
	}
}
