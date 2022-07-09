using System;
using System.Collections.Generic;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server.Spells.Fifth;
using Server.Spells.Seventh;

namespace Server.Spells.Ninjitsu;

public class AnimalForm : NinjaSpell
{
	public static void Initialize()
	{
		EventSink.OnLogin += OnLogin;
	}

	public static void OnLogin(Mobile m)
	{
		AnimalFormContext context = GetContext(m);

		if (context is {SpeedBoost: true})
		{
			m.SendSpeedControl(SpeedControlType.MountSpeed);
		}
	}

	private static readonly SpellInfo m_Info = new("Animal Form", null, -1, 9002);

	public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(1.0);

	public override double RequiredSkill => 0.0;
	public override int RequiredMana => Core.ML ? 10 : 0;
	public override int CastRecoveryBase => Core.ML ? 10 : base.CastRecoveryBase;

	public override bool BlockedByAnimalForm => false;

	public AnimalForm(Mobile caster, Item scroll)
		: base(caster, scroll, m_Info)
	{ }

	public override bool Cast()
	{
		if (CasterIsMoving() && GetLastAnimalForm(Caster) == 16)
		{
			return false;
		}

		return base.Cast();
	}

	public override bool CheckCast()
	{
		if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
		{
			Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
			return false;
		}

		if (TransformationSpellHelper.UnderTransformation(Caster))
		{
			Caster.SendLocalizedMessage(1063219); // You cannot mimic an animal while in that form.
			return false;
		}

		if (DisguiseTimers.IsDisguised(Caster))
		{
			Caster.SendLocalizedMessage(1061631); // You can't do that while disguised.
			return false;
		}

		return base.CheckCast();
	}

	public override bool CheckDisturb(DisturbType type, bool firstCircle, bool resistable)
	{
		return false;
	}

	private bool CasterIsMoving()
	{
		return Core.TickCount - Caster.LastMoveTime <= Caster.ComputeMovementSpeed(Caster.Direction);
	}

	private bool _wasMoving;

	public override void OnBeginCast()
	{
		base.OnBeginCast();

		Caster.FixedEffect(0x37C4, 10, 14, 4, 3);
		_wasMoving = CasterIsMoving();
	}

	public override bool CheckFizzle()
	{
		// Spell is initially always successful, and with no skill gain.
		return true;
	}

	public override void OnCast()
	{
		if (!Caster.CanBeginAction(typeof(PolymorphSpell)))
		{
			Caster.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
		}
		else if (TransformationSpellHelper.UnderTransformation(Caster))
		{
			Caster.SendLocalizedMessage(1063219); // You cannot mimic an animal while in that form.
		}
		else if (!Caster.CanBeginAction(typeof(IncognitoSpell)) || (Caster.IsBodyMod && GetContext(Caster) == null))
		{
			DoFizzle();
		}
		else if (CheckSequence())
		{
			AnimalFormContext context = GetContext(Caster);

			int mana = ScaleMana(RequiredMana);
			if (mana > Caster.Mana)
			{
				Caster.SendLocalizedMessage(1060174, mana.ToString());
				// You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
			}
			else if (context != null)
			{
				RemoveContext(Caster, context, true);
				Caster.Mana -= mana;
			}
			else if (Caster is PlayerMobile)
			{
				bool skipGump = _wasMoving || CasterIsMoving();

				if (GetLastAnimalForm(Caster) == -1 || GetLastAnimalForm(Caster) == 16 || !skipGump)
				{
					Caster.CloseGump(typeof(AnimalFormGump));
					Caster.SendGump(new AnimalFormGump(Caster, m_Entries, this));
				}
				else
				{
					if (Morph(Caster, GetLastAnimalForm(Caster)) == MorphResult.Fail)
					{
						DoFizzle();
					}
					else
					{
						Caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
						Caster.Mana -= mana;
					}
				}
			}
			else
			{
				if (Morph(Caster, GetLastAnimalForm(Caster)) == MorphResult.Fail)
				{
					DoFizzle();
				}
				else
				{
					Caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
					Caster.Mana -= mana;
				}
			}
		}

		FinishSequence();
	}

	private static readonly Dictionary<Mobile, int> m_LastAnimalForms = new();

	public static void AddLastAnimalForm(Mobile m, int id)
	{
		m_LastAnimalForms[m] = id;
	}

	public int GetLastAnimalForm(Mobile m)
	{
		if (m_LastAnimalForms.ContainsKey(m))
		{
			return m_LastAnimalForms[m];
		}

		return -1;
	}

	public enum MorphResult
	{
		Success,
		Fail,
		NoSkill
	}

	public static MorphResult Morph(Mobile m, int entryId)
	{
		if (entryId < 0 || entryId >= m_Entries.Length)
		{
			return MorphResult.Fail;
		}

		AnimalFormEntry entry = m_Entries[entryId];

		AddLastAnimalForm(m, entryId); //On OSI, it's the last /attempted/ one not the last succeeded one

		if (m.Skills.Ninjitsu.Value < entry.ReqSkill)
		{
			string args = $"{entry.ReqSkill.ToString("F1")}\t{SkillName.Ninjitsu}\t ";
			m.SendLocalizedMessage(1063013, args);
			// You need at least ~1_SKILL_REQUIREMENT~ ~2_SKILL_NAME~ skill to use that ability.
			return MorphResult.NoSkill;
		}

		/*
		if( !m.CheckSkill( SkillName.Ninjitsu, entry.ReqSkill, entry.ReqSkill + 37.5 ) )
		return MorphResult.Fail;
		*
		* On OSI,it seems you can only gain starting at '0' using Animal form.
		*/

		double ninjitsu = m.Skills.Ninjitsu.Value;

		if (ninjitsu < entry.ReqSkill + 37.5)
		{
			double chance = (ninjitsu - entry.ReqSkill) / 37.5;

			if (chance < Utility.RandomDouble())
			{
				return MorphResult.Fail;
			}
		}

		m.CheckSkill(SkillName.Ninjitsu, 0.0, 37.5);

		if (!BaseFormTalisman.EntryEnabled(m, entry.Type))
		{
			return MorphResult.Success; // Still consumes mana, just no effect
		}

		BaseMount.BaseDismount(m);

		int bodyMod = entry.BodyMod;
		int hueMod = entry.HueMod;

		m.BodyMod = bodyMod;
		m.HueMod = hueMod;

		if (entry.SpeedBoost)
		{
			m.SendSpeedControl(SpeedControlType.MountSpeed);
		}

		SkillMod mod = null;

		if (entry.StealthBonus)
		{
			mod = new DefaultSkillMod(SkillName.Stealth, true, 20.0);
			mod.ObeyCap = true;
			m.AddSkillMod(mod);
		}

		SkillMod stealingMod = null;

		if (entry.StealingBonus)
		{
			stealingMod = new DefaultSkillMod(SkillName.Stealing, true, 10.0);
			stealingMod.ObeyCap = true;
			m.AddSkillMod(stealingMod);
		}

		Timer timer = new AnimalFormTimer(m, bodyMod, hueMod);
		timer.Start();

		AddContext(m, new AnimalFormContext(timer, mod, entry.SpeedBoost, entry.Type, stealingMod));
		return MorphResult.Success;
	}

	private static readonly Dictionary<Mobile, AnimalFormContext> m_Table = new();

	public static void AddContext(Mobile m, AnimalFormContext context)
	{
		m_Table[m] = context;

		if (context.Type == typeof(BakeKitsune) || context.Type == typeof(GreyWolf)
		                                        || context.Type == typeof(Dog) || context.Type == typeof(Cat))
		{
			m.ResetStatTimers();
		}

		m.Delta(MobileDelta.WeaponDamage);
	}

	public static void RemoveContext(Mobile m, bool resetGraphics)
	{
		AnimalFormContext context = GetContext(m);

		if (context != null)
		{
			RemoveContext(m, context, resetGraphics);
		}

		m.Delta(MobileDelta.WeaponDamage);
	}

	public static void RemoveContext(Mobile m, AnimalFormContext context, bool resetGraphics)
	{
		m_Table.Remove(m);

		if (context.SpeedBoost)
		{
			m.SendSpeedControl(m.Region is TwistedWealdDesert
				? SpeedControlType.WalkSpeed
				: SpeedControlType.Disable);
		}

		SkillMod mod = context.Mod;

		if (mod != null)
		{
			m.RemoveSkillMod(mod);
		}

		mod = context.StealingMod;

		if (mod != null)
		{
			m.RemoveSkillMod(mod);
		}

		if (resetGraphics)
		{
			m.HueMod = -1;
			m.BodyMod = 0;
		}

		m.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);

		context.Timer.Stop();

		BuffInfo.RemoveBuff(m, BuffIcon.AnimalForm);
		BuffInfo.RemoveBuff(m, BuffIcon.WhiteTigerForm);
	}

	public static AnimalFormContext GetContext(Mobile m)
	{
		if (m_Table.ContainsKey(m))
			return m_Table[m];

		return null;
	}

	public static bool UnderTransformation(Mobile m)
	{
		return GetContext(m) != null;
	}

	public static bool UnderTransformation(Mobile m, Type type)
	{
		AnimalFormContext context = GetContext(m);

		return context != null && context.Type == type;
	}

	/*
	private delegate void AnimalFormCallback( Mobile from );
	private delegate bool AnimalFormRequirementCallback( Mobile from );
	*/

	public class AnimalFormEntry
	{
		private readonly int _hueModMin;
		private readonly int _hueModMax;

		public Type Type { get; }

		public string Name { get; }

		public int ItemId { get; }

		public int Hue { get; }

		public int Tooltip { get; }

		public double ReqSkill { get; }

		public int BodyMod { get; }

		public int HueMod => Utility.RandomMinMax(_hueModMin, _hueModMax);
		public bool StealthBonus { get; }

		public bool SpeedBoost { get; }

		public bool StealingBonus { get; }
		/*
		private AnimalFormCallback m_TransformCallback;
		private AnimalFormCallback m_UntransformCallback;
		private AnimalFormRequirementCallback m_RequirementCallback;
		*/

		public AnimalFormEntry(
			Type type,
			string name,
			int itemId,
			int hue,
			int tooltip,
			double reqSkill,
			int bodyMod,
			int hueModMin,
			int hueModMax,
			bool stealthBonus,
			bool speedBoost,
			bool stealingBonus)
		{
			Type = type;
			Name = name;
			ItemId = itemId;
			Hue = hue;
			Tooltip = tooltip;
			ReqSkill = reqSkill;
			BodyMod = bodyMod;
			_hueModMin = hueModMin;
			_hueModMax = hueModMax;
			StealthBonus = stealthBonus;
			SpeedBoost = speedBoost;
			StealingBonus = stealingBonus;
		}
	}

	private static readonly AnimalFormEntry[] m_Entries = new[]
	{
		new AnimalFormEntry(typeof(Kirin), "kirin", 9632, 0, 1070811, 100.0, 0x84, 0, 0, false, true, false),
		new AnimalFormEntry(typeof(Unicorn), "unicorn", 9678, 0, 1070812, 100.0, 0x7A, 0, 0, false, true, false),
		new AnimalFormEntry(typeof(BakeKitsune), "bake-kitsune", 10083, 0, 1070810, 82.5, 0xF6, 0, 0, false, true, false),
		new AnimalFormEntry(typeof(GreyWolf), "wolf", 9681, 2309, 1070810, 82.5, 0x19, 0x8FD, 0x90E, false, true, false),
		new AnimalFormEntry(typeof(Llama), "llama", 8438, 0, 1070809, 70.0, 0xDC, 0, 0, false, true, false),
		new AnimalFormEntry(typeof(ForestOstard), "ostard", 8503, 2212, 1070809, 70.0, 0xDB, 0x899, 0x8B0, false, true, false),
		new AnimalFormEntry(typeof(BullFrog), "bullfrog", 8496, 2003, 1070807, 50.0, 0x51, 0x7D1, 0x7D6, false, false, false),
		new AnimalFormEntry(typeof(GiantSerpent), "giant serpent", 9663, 2009, 1070808, 50.0, 0x15, 0x7D1, 0x7E2, false, false, false),
		new AnimalFormEntry(typeof(Dog), "dog", 8476, 2309, 1070806, 40.0, 0xD9, 0x8FD, 0x90E, false, false, false),
		new AnimalFormEntry(typeof(Cat), "cat", 8475, 2309, 1070806, 40.0, 0xC9, 0x8FD, 0x90E, false, false, false),
		new AnimalFormEntry(typeof(Rat), "rat", 8483, 2309, 1070805, 20.0, 0xEE, 0x8FD, 0x90E, true, false, false),
		new AnimalFormEntry(typeof(Rabbit), "rabbit", 8485, 2309, 1070805, 20.0, 0xCD, 0x8FD, 0x90E, true, false, false),
		new AnimalFormEntry(typeof(Squirrel), "squirrel", 11671, 0, 0, 20.0, 0x116, 0, 0, false, false, false),
		new AnimalFormEntry(typeof(Ferret), "ferret", 11672, 0, 1075220, 40.0, 0x117, 0, 0, false, false, true),
		new AnimalFormEntry(typeof(CuSidhe), "cu sidhe", 11670, 0, 1075221, 60.0, 0x115, 0, 0, false, false, false),
		new AnimalFormEntry(typeof(Reptalon), "reptalon", 11669, 0, 1075222, 90.0, 0x114, 0, 0, false, false, false),
	};

	public static AnimalFormEntry[] Entries => m_Entries;

	public class AnimalFormGump : Gump
	{
		//TODO: Convert this for ML to the BaseImageTileButtonsgump
		private readonly Mobile _caster;
		private readonly AnimalForm _spell;
		private readonly Item _talisman;

		public AnimalFormGump(Mobile caster, IReadOnlyList<AnimalFormEntry> entries, AnimalForm spell)
			: base(50, 50)
		{
			_caster = caster;
			_spell = spell;
			_talisman = caster.Talisman;

			AddPage(0);

			AddBackground(0, 0, 520, 404, 0x13BE);
			AddImageTiled(10, 10, 500, 20, 0xA40);
			AddImageTiled(10, 40, 500, 324, 0xA40);
			AddImageTiled(10, 374, 500, 20, 0xA40);
			AddAlphaRegion(10, 10, 500, 384);

			AddHtmlLocalized(14, 12, 500, 20, 1063394, 0x7FFF, false, false); // <center>Polymorph Selection Menu</center>

			AddButton(10, 374, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 376, 450, 20, 1011012, 0x7FFF, false, false); // CANCEL

			double ninjitsu = caster.Skills.Ninjitsu.Value;

			int current = 0;

			for (int i = 0; i < entries.Count; ++i)
			{
				bool enabled = ninjitsu >= entries[i].ReqSkill && BaseFormTalisman.EntryEnabled(caster, entries[i].Type);

				int page = current / 10 + 1;
				int pos = current % 10;

				if (pos == 0)
				{
					if (page > 1)
					{
						AddButton(400, 374, 0xFA5, 0xFA7, 0, GumpButtonType.Page, page);
						AddHtmlLocalized(440, 376, 60, 20, 1043353, 0x7FFF, false, false); // Next
					}

					AddPage(page);

					if (page > 1)
					{
						AddButton(300, 374, 0xFAE, 0xFB0, 0, GumpButtonType.Page, 1);
						AddHtmlLocalized(340, 376, 60, 20, 1011393, 0x7FFF, false, false); // Back
					}
				}

				if (enabled)
				{
					int x = pos % 2 == 0 ? 14 : 264;
					int y = pos / 2 * 64 + 44;

					Rectangle2D b = ItemBounds.Table[entries[i].ItemId];

					AddImageTiledButton(
						x,
						y,
						0x918,
						0x919,
						i + 1,
						GumpButtonType.Reply,
						0,
						entries[i].ItemId,
						entries[i].Hue,
						40 - b.Width / 2 - b.X,
						30 - b.Height / 2 - b.Y,
						entries[i].Tooltip);
					AddHtml(x + 84, y, 250, 60, Color(string.Format(entries[i].Name), 0xFFFFFF), false, false);

					current++;
				}
			}
		}

		private new string Color(string str, int color)
		{
			return $"<BASEFONT COLOR=#{color:X6}>{str}</BASEFONT>";
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			int entryId = info.ButtonID - 1;

			if (entryId < 0 || entryId >= m_Entries.Length)
			{
				return;
			}

			int mana = _spell.ScaleMana(_spell.RequiredMana);
			AnimalFormEntry entry = AnimalForm.Entries[entryId];

			if (mana > _caster.Mana)
			{
				_caster.SendLocalizedMessage(1060174, mana.ToString());
				// You must have at least ~1_MANA_REQUIREMENT~ Mana to use this ability.
			}
			else if (BaseFormTalisman.EntryEnabled(sender.Mobile, entry.Type))
			{
				if (Morph(_caster, entryId) == MorphResult.Fail)
				{
					_caster.LocalOverheadMessage(MessageType.Regular, 0x3B2, 502632); // The spell fizzles.
					_caster.FixedParticles(0x3735, 1, 30, 9503, EffectLayer.Waist);
					_caster.PlaySound(0x5C);
				}
				else
				{
					_caster.FixedParticles(0x3728, 10, 13, 2023, EffectLayer.Waist);
					_caster.Mana -= mana;

					string typename = entry.Name;

					BuffInfo.AddBuff(_caster, new BuffInfo(BuffIcon.AnimalForm, 1060612, 1075823,
						$"{("aeiouy".IndexOf(typename.ToLower()[0]) >= 0 ? "an" : "a")}\t{typename}"));
				}
			}
		}
	}
}

public class AnimalFormContext
{
	public Timer Timer { get; }

	public SkillMod Mod { get; }

	public bool SpeedBoost { get; }

	public Type Type { get; }

	public SkillMod StealingMod { get; }

	public AnimalFormContext(Timer timer, SkillMod mod, bool speedBoost, Type type, SkillMod stealingMod)
	{
		Timer = timer;
		Mod = mod;
		SpeedBoost = speedBoost;
		Type = type;
		StealingMod = stealingMod;
	}
}

public class AnimalFormTimer : Timer
{
	private readonly Mobile _mobile;
	private readonly int _body;
	private readonly int _hue;
	private int _counter;
	private Mobile _lastTarget;

	public AnimalFormTimer(Mobile from, int body, int hue)
		: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
	{
		_mobile = from;
		_body = body;
		_hue = hue;
		_counter = 0;

		Priority = TimerPriority.FiftyMs;
	}

	protected override void OnTick()
	{
		if (_mobile.Deleted || !_mobile.Alive || _mobile.Body != _body || _mobile.Hue != _hue)
		{
			AnimalForm.RemoveContext(_mobile, true);
			Stop();
		}
		else
		{
			if (_body == 0x115) // Cu Sidhe
			{
				if (_counter++ >= 8)
				{
					if (_mobile.Hits < _mobile.HitsMax && _mobile.Backpack != null)
					{
						if (_mobile.Backpack.FindItemByType(typeof(Bandage)) is Bandage b)
						{
							_mobile.Hits += Utility.RandomMinMax(20, 50);
							b.Consume();
						}
					}

					_counter = 0;
				}
			}
			else if (_body == 0x114) // Reptalon
			{
				if (_mobile.Combatant is Mobile mobile && _mobile.Combatant != _lastTarget)
				{
					_counter = 1;
					_lastTarget = mobile;
				}

				if (_mobile.Warmode && _lastTarget is {Alive: true, Deleted: false} && _counter-- <= 0)
				{
					if (_mobile.CanBeHarmful(_lastTarget) && _lastTarget.Map == _mobile.Map &&
					    _lastTarget.InRange(_mobile.Location, BaseCreature.DefaultRangePerception) && _mobile.InLOS(_lastTarget))
					{
						_mobile.Direction = _mobile.GetDirectionTo(_lastTarget);
						_mobile.Freeze(TimeSpan.FromSeconds(1));
						_mobile.PlaySound(0x16A);

						DelayCall(TimeSpan.FromSeconds(1.3), BreathEffect_Callback, _lastTarget);
					}

					_counter = Math.Min((int)_mobile.GetDistanceToSqrt(_lastTarget), 10);
				}
			}
		}
	}

	public void BreathEffect_Callback(Mobile target)
	{
		if (_mobile.CanBeHarmful(target))
		{
			_mobile.RevealingAction();
			_mobile.PlaySound(0x227);
			Effects.SendMovingEffect(_mobile, target, 0x36D4, 5, 0, false, false, 0, 0);

			DelayCall(TimeSpan.FromSeconds(1), BreathDamage_Callback, target);
		}
	}

	public void BreathDamage_Callback(Mobile target)
	{
		if (_mobile.CanBeHarmful(target))
		{
			_mobile.RevealingAction();
			_mobile.DoHarmful(target);
			AOS.Damage(target, _mobile, 20, !target.Player, 0, 100, 0, 0, 0);
		}
	}
}
