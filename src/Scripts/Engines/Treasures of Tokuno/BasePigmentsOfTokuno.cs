using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

public abstract class BasePigmentsOfTokuno : BaseItem, IUsesRemaining
{
	private static readonly Type[] m_Glasses = {
		typeof( MaritimeGlasses ),
		typeof( WizardsGlasses ),
		typeof( TradeGlasses ),
		typeof( LyricalGlasses ),
		typeof( NecromanticGlasses ),
		typeof( LightOfWayGlasses ),
		typeof( FoldedSteelGlasses ),
		typeof( PoisonedGlasses ),
		typeof( TreasureTrinketGlasses ),
		typeof( MaceShieldGlasses ),
		typeof( ArtsGlasses ),
		typeof( AnthropomorphistGlasses )
	};

	private static readonly Type[] m_Replicas = {
		typeof( ANecromancerShroud ),
		typeof( BraveKnightOfTheBritannia ),
		typeof( CaptainJohnsHat ),
		typeof( LegendaryDetectiveBoots ),
		typeof( DjinnisRing ),
		typeof( EmbroideredOakLeafCloak ),
		typeof( GuantletsOfAnger ),
		typeof( LieutenantOfTheBritannianRoyalGuard ),
		typeof( OblivionsNeedle ),
		typeof( RoyalGuardSurvivalKnife ),
		typeof( SamaritanRobe ),
		typeof( TheMostKnowledgePerson ),
		typeof( TheRobeOfBritanniaAri ),
		typeof( AcidProofRobe ),
		typeof( Calm ),
		typeof( CrownOfTalKeesh ),
		typeof( FangOfRactus ),
		typeof( GladiatorsCollar ),
		typeof( OrcChieftainHelm ),
		typeof( Pacify ),
		typeof( Quell ),
		typeof( ShroudOfDeciet ),
		typeof( Subdue )
	};

	private static readonly Type[] m_DyableHeritageItems = {
		typeof( ChargerOfTheFallen ),
		typeof( SamuraiHelm ),
		typeof( HolySword ),
		typeof( LeggingsOfEmbers ),
		typeof( ShaminoCrossbow )
	};

	public override int LabelNumber => 1070933;  // Pigments of Tokuno

	private int _usesRemaining;
	private TextDefinition _label;

	protected TextDefinition Label
	{
		get => _label;
		set { _label = value; InvalidateProperties(); }
	}

	public BasePigmentsOfTokuno() : base(0xEFF)
	{
		Weight = 1.0;
		_usesRemaining = 1;
	}

	public BasePigmentsOfTokuno(int uses) : base(0xEFF)
	{
		Weight = 1.0;
		_usesRemaining = uses;
	}

	public BasePigmentsOfTokuno(Serial serial) : base(serial)
	{
	}

	public override void GetProperties(ObjectPropertyList list)
	{
		base.GetProperties(list);

		if (_label > 0)
			TextDefinition.AddTo(list, _label);

		list.Add(1060584, _usesRemaining.ToString()); // uses remaining: ~1_val~
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsAccessibleTo(from) && from.InRange(GetWorldLocation(), 3))
		{
			from.SendLocalizedMessage(1070929); // Select the artifact or enhanced magic item to dye.
			from.BeginTarget(3, false, TargetFlags.None, new TargetStateCallback(InternalCallback), this);
		}
		else
			from.SendLocalizedMessage(502436); // That is not accessible.
	}

	private void InternalCallback(Mobile from, object targeted, object state)
	{
		BasePigmentsOfTokuno pigment = (BasePigmentsOfTokuno)state;

		if (pigment.Deleted || pigment.UsesRemaining <= 0 || !from.InRange(pigment.GetWorldLocation(), 3) || !pigment.IsAccessibleTo(from))
			return;

		if (targeted is not Item i)
			from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.
		else if (!from.InRange(i.GetWorldLocation(), 3) || !IsAccessibleTo(from))
			from.SendLocalizedMessage(502436); // That is not accessible.
		else if (from.Items.Contains(i))
			from.SendLocalizedMessage(1070930); // Can't dye artifacts or enhanced magic items that are being worn.
		else if (i.IsLockedDown)
			from.SendLocalizedMessage(1070932); // You may not dye artifacts and enhanced magic items which are locked down.
		else if (i.QuestItem)
			from.SendLocalizedMessage(1151836); // You may not dye toggled quest items.
		else switch (i)
		{
			case MetalPigmentsOfTokuno:
			// You cannot dye that.
			case LesserPigmentsOfTokuno:
			// You cannot dye that.
			case PigmentsOfTokuno:
				from.SendLocalizedMessage(1042417); // You cannot dye that.
				break;
			default:
			{
				if (!IsValidItem(i))
					from.SendLocalizedMessage(1070931); // You can only dye artifacts and enhanced magic items with this tub.	//Yes, it says tub on OSI.  Don't ask me why ;p
				else
				{
					//Notes: on OSI there IS no hue check to see if it's already hued.  and no messages on successful hue either
					i.Hue = Hue;

					if (--pigment.UsesRemaining <= 0)
						pigment.Delete();

					from.PlaySound(0x23E); // As per OSI TC1
				}

				break;
			}
		}
	}

	public static bool IsValidItem(Item i)
	{
		if (i is BasePigmentsOfTokuno)
			return false;

		Type t = i.GetType();

		CraftResource resource = i switch
		{
			BaseWeapon weapon => weapon.Resource,
			BaseArmor armor => armor.Resource,
			BaseClothing clothing => clothing.Resource,
			_ => CraftResource.None
		};

		if (!CraftResources.IsStandard(resource))
			return true;

		if (i is ITokunoDyable)
			return true;

		return (
			IsInTypeList(t, TreasuresOfTokuno.LesserArtifactsTotal)
			|| IsInTypeList(t, TreasuresOfTokuno.GreaterArtifacts)
			|| IsInTypeList(t, DemonKnight.ArtifactRarity10)
			|| IsInTypeList(t, DemonKnight.ArtifactRarity11)
			|| IsInTypeList(t, MondainsLegacy.Artifacts)
			|| IsInTypeList(t, StealableArtifactsSpawner.TypesOfEntires)
			|| IsInTypeList(t, Paragon.Artifacts)
			|| IsInTypeList(t, Leviathan.Artifacts)
			|| IsInTypeList(t, LootHelpers.Artifacts)
			|| IsInTypeList(t, m_Replicas)
			|| IsInTypeList(t, m_DyableHeritageItems)
			|| IsInTypeList(t, m_Glasses)
		);
	}

	private static bool IsInTypeList(Type t, IEnumerable<Type> list)
	{
		return list.Any(t1 => t1 == t);
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.WriteEncodedInt(_usesRemaining);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		_usesRemaining = version switch
		{
			0 => reader.ReadEncodedInt(),
			_ => _usesRemaining
		};
	}

	#region IUsesRemaining Members

	[CommandProperty(AccessLevel.GameMaster)]
	public int UsesRemaining
	{
		get => _usesRemaining;
		set { _usesRemaining = value; InvalidateProperties(); }
	}

	public bool ShowUsesRemaining
	{
		get => true;
		set { }
	}

	#endregion
}
