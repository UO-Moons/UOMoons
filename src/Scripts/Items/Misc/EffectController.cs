using System;

namespace Server.Items;

public enum EcEffectType
{
	None,
	Moving,
	Location,
	Target,
	Lightning
}

public enum EffectTriggerType
{
	None,
	Sequenced,
	DoubleClick,
	InRange
}

public class EffectController : BaseItem
{
	private IEntity m_Source;
	private IEntity m_Target;

	[CommandProperty(AccessLevel.GameMaster)]
	private EcEffectType EffectType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public EffectTriggerType TriggerType { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private EffectLayer EffectLayer { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private TimeSpan EffectDelay { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private TimeSpan TriggerDelay { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private TimeSpan SoundDelay { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Item SourceItem { get => m_Source as Item; set => m_Source = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile SourceMobile { get => m_Source as Mobile; set => m_Source = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool SourceNull { get => m_Source == null; set { if (value) m_Source = null; } }


	[CommandProperty(AccessLevel.GameMaster)]
	public Item TargetItem { get => m_Target as Item; set => m_Target = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile TargetMobile { get => m_Target as Mobile; set => m_Target = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool TargetNull { get => m_Target == null; set { if (value) m_Target = null; } }

	[CommandProperty(AccessLevel.GameMaster)]
	private EffectController Sequence { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool FixedDirection { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool Explodes { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private bool PlaySoundAtTrigger { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int EffectItemId { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int EffectHue { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int RenderMode { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int Speed { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int Duration { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ParticleEffect { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ExplodeParticleEffect { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int ExplodeSound { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	private int Unknown { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SoundId { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int TriggerRange { get; set; }

	public override string DefaultName => "Effect Controller";

	[Constructable]
	public EffectController() : base(0x1B72)
	{
		Movable = false;
		Visible = false;
		TriggerType = EffectTriggerType.Sequenced;
		EffectLayer = (EffectLayer)255;
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (TriggerType == EffectTriggerType.DoubleClick)
			DoEffect(from);
	}

	public override bool HandlesOnMovement => TriggerType == EffectTriggerType.InRange;

	public override void OnMovement(Mobile m, Point3D oldLocation)
	{
		if (m.Location != oldLocation && TriggerType == EffectTriggerType.InRange && Utility.InRange(GetWorldLocation(), m.Location, TriggerRange) && !Utility.InRange(GetWorldLocation(), oldLocation, TriggerRange))
			DoEffect(m);
	}

	public EffectController(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write(EffectDelay);
		writer.Write(TriggerDelay);
		writer.Write(SoundDelay);

		if (m_Source is Item item)
			writer.Write(item);
		else
			writer.Write(m_Source as Mobile);

		if (m_Target is Item target)
			writer.Write(target);
		else
			writer.Write(m_Target as Mobile);

		writer.Write(Sequence);

		writer.Write(FixedDirection);
		writer.Write(Explodes);
		writer.Write(PlaySoundAtTrigger);

		writer.WriteEncodedInt((int)EffectType);
		writer.WriteEncodedInt((int)EffectLayer);
		writer.WriteEncodedInt((int)TriggerType);

		writer.WriteEncodedInt(EffectItemId);
		writer.WriteEncodedInt(EffectHue);
		writer.WriteEncodedInt(RenderMode);
		writer.WriteEncodedInt(Speed);
		writer.WriteEncodedInt(Duration);
		writer.WriteEncodedInt(ParticleEffect);
		writer.WriteEncodedInt(ExplodeParticleEffect);
		writer.WriteEncodedInt(ExplodeSound);
		writer.WriteEncodedInt(Unknown);
		writer.WriteEncodedInt(SoundId);
		writer.WriteEncodedInt(TriggerRange);
	}

	private IEntity ReadEntity(GenericReader reader)
	{
		return World.FindEntity(reader.ReadSerial());
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				EffectDelay = reader.ReadTimeSpan();
				TriggerDelay = reader.ReadTimeSpan();
				SoundDelay = reader.ReadTimeSpan();

				m_Source = ReadEntity(reader);
				m_Target = ReadEntity(reader);
				Sequence = reader.ReadItem() as EffectController;

				FixedDirection = reader.ReadBool();
				Explodes = reader.ReadBool();
				PlaySoundAtTrigger = reader.ReadBool();

				EffectType = (EcEffectType)reader.ReadEncodedInt();
				EffectLayer = (EffectLayer)reader.ReadEncodedInt();
				TriggerType = (EffectTriggerType)reader.ReadEncodedInt();

				EffectItemId = reader.ReadEncodedInt();
				EffectHue = reader.ReadEncodedInt();
				RenderMode = reader.ReadEncodedInt();
				Speed = reader.ReadEncodedInt();
				Duration = reader.ReadEncodedInt();
				ParticleEffect = reader.ReadEncodedInt();
				ExplodeParticleEffect = reader.ReadEncodedInt();
				ExplodeSound = reader.ReadEncodedInt();
				Unknown = reader.ReadEncodedInt();
				SoundId = reader.ReadEncodedInt();
				TriggerRange = reader.ReadEncodedInt();

				break;
			}
		}
	}

	private void PlaySound(IEntity trigger)
	{
		IEntity ent = null;

		if (PlaySoundAtTrigger)
			ent = trigger;

		ent ??= this;

		Effects.PlaySound(ent is Item item ? item.GetWorldLocation() : ent.Location, ent.Map, SoundId);
	}

	private void DoEffect(IEntity trigger)
	{
		if (Deleted || TriggerType == EffectTriggerType.None)
			return;

		if (trigger is Mobile { Hidden: true, AccessLevel: > AccessLevel.Player })
			return;

		if (SoundId > 0)
			Timer.DelayCall(SoundDelay, PlaySound, trigger);

		if (Sequence != null)
			Timer.DelayCall(TriggerDelay, Sequence.DoEffect, trigger);

		if (EffectType != EcEffectType.None)
			Timer.DelayCall(EffectDelay, InternalDoEffect, trigger);
	}

	private void InternalDoEffect(IEntity trigger)
	{
		IEntity from = m_Source, to = m_Target;

		from ??= trigger;

		to ??= trigger;

		switch (EffectType)
		{
			case EcEffectType.Lightning:
			{
				Effects.SendBoltEffect(from, false, EffectHue);
				break;
			}
			case EcEffectType.Location:
			{
				Effects.SendLocationParticles(EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration), EffectItemId, Speed, Duration, EffectHue, RenderMode, ParticleEffect, Unknown);
				break;
			}
			case EcEffectType.Moving:
			{
				if (from == this)
					from = EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration);

				if (to == this)
					to = EffectItem.Create(to.Location, to.Map, EffectItem.DefaultDuration);

				Effects.SendMovingParticles(from, to, EffectItemId, Speed, Duration, FixedDirection, Explodes, EffectHue, RenderMode, ParticleEffect, ExplodeParticleEffect, ExplodeSound, EffectLayer, Unknown);
				break;
			}
			case EcEffectType.Target:
			{
				Effects.SendTargetParticles(from, EffectItemId, Speed, Duration, EffectHue, RenderMode, ParticleEffect, EffectLayer, Unknown);
				break;
			}
		}
	}
}
