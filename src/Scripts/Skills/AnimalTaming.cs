using Server.Factions;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.Spellweaving;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server.SkillHandlers;

public class AnimalTaming
{
	private static readonly Dictionary<Mobile, Mobile> MBeingTamed = new();

	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.AnimalTaming].Callback = OnUse;
	}

	public static bool DisableMessage { get; set; }

	public static TimeSpan OnUse(Mobile m)
	{
		m.RevealingAction();

		m.Target = new InternalTarget();
		m.RevealingAction();

		if (!DisableMessage)
			m.SendLocalizedMessage(502789); // Tame which animal?

		return TimeSpan.FromHours(6.0);
	}

	public static bool CheckMastery(Mobile tamer, BaseCreature creature)
	{
		BaseCreature familiar = (BaseCreature)Spells.Necromancy.SummonFamiliarSpell.Table[tamer];

		if (familiar is {Deleted: false} and DarkWolfFamiliar)
		{
			if (creature is DireWolf or GreyWolf or TimberWolf or WhiteWolf or BakeKitsune)
				return true;
		}

		return false;
	}

	public static bool MustBeSubdued(BaseCreature bc)
	{
		if (bc.Owners.Count > 0) { return false; } //Checks to see if the animal has been tamed before
		return bc.SubdueBeforeTame && (bc.Hits > (bc.HitsMax / 10));
	}

	public static void ScaleStats(BaseCreature bc, double scalar)
	{
		if (bc.RawStr > 0)
			bc.RawStr = (int)Math.Max(1, bc.RawStr * scalar);

		if (bc.RawDex > 0)
			bc.RawDex = (int)Math.Max(1, bc.RawDex * scalar);

		if (bc.RawInt > 0)
			bc.RawInt = (int)Math.Max(1, bc.RawInt * scalar);

		if (bc.HitsMaxSeed > 0)
		{
			bc.HitsMaxSeed = (int)Math.Max(1, bc.HitsMaxSeed * scalar);
			bc.Hits = bc.Hits;
		}

		if (bc.StamMaxSeed > 0)
		{
			bc.StamMaxSeed = (int)Math.Max(1, bc.StamMaxSeed * scalar);
			bc.Stam = bc.Stam;
		}
	}

	public static void ScaleSkills(BaseCreature bc, double scalar)
	{
		ScaleSkills(bc, scalar, scalar);
	}

	public static void ScaleSkills(BaseCreature bc, double scalar, double capScalar)
	{
		for (int i = 0; i < bc.Skills.Length; ++i)
		{
			bc.Skills[i].Base *= scalar;

			bc.Skills[i].Cap = Math.Max(100.0, bc.Skills[i].Cap * capScalar);

			if (bc.Skills[i].Base > bc.Skills[i].Cap)
			{
				bc.Skills[i].Cap = bc.Skills[i].Base;
			}
		}
	}

	private sealed class InternalTarget : Target
	{
		private bool _mSetSkillTime = true;

		public InternalTarget() : base(Core.AOS ? 3 : 2, false, TargetFlags.None)
		{
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (_mSetSkillTime)
				from.NextSkillTime = Core.TickCount;
		}

		public static void ResetPacify(object obj)
		{
			if (obj is BaseCreature creature)
			{
				creature.BardPacified = true;
			}
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			from.RevealingAction();

			if (targeted is Mobile mobile)
			{
				if (targeted is BaseCreature creature)
				{
					if (!creature.Tamable)
					{
						creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1049655, from.NetState); // That creature cannot be tamed.
					}
					else if (creature.Controlled)
					{
						creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502804, from.NetState); // That animal looks tame already.
					}
					else switch (from.Female)
					{
						case true when !creature.AllowFemaleTamer:
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1049653, from.NetState); // That creature can only be tamed by males.
							break;
						case false when !creature.AllowMaleTamer:
							creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1049652, from.NetState); // That creature can only be tamed by females.
							break;
						default:
						{
							if (creature is CuSidhe && from.Race != Race.Elf)
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502801, from.NetState); // You can't tame that!
							}
							else if (from.Followers + creature.ControlSlots > from.FollowersMax)
							{
								from.SendLocalizedMessage(1049611); // You have too many followers to tame that creature.
							}
							else if (creature.Owners.Count >= BaseCreature.MaxOwners && !creature.Owners.Contains(from))
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1005615, from.NetState); // This animal has had too many owners and is too upset for you to tame.
							}
							else if (MustBeSubdued(creature))
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1054025, from.NetState); // You must subdue this creature before you can tame it!
							}
							else if (CheckMastery(from, creature) || from.Skills[SkillName.AnimalTaming].Value >= creature.MinTameSkill)
							{
								if (creature is FactionWarHorse warHorse)
								{
									Faction faction = Faction.Find(from);

									if (faction == null || faction != warHorse.Faction)
									{
										creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042590, from.NetState); // You cannot tame this creature.
										return;
									}
								}

								if (MBeingTamed.ContainsKey(creature))
								{
									creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502802, from.NetState); // Someone else is already taming this.
								}
								else if (creature.CanAngerOnTame && 0.95 >= Utility.RandomDouble())
								{
									creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502805, from.NetState); // You seem to anger the beast!
									creature.PlaySound(creature.GetAngerSound());
									creature.Direction = creature.GetDirectionTo(from);

									if (creature.BardPacified && Utility.RandomDouble() > .24)
									{
										Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerStateCallback(ResetPacify), creature);
									}
									else
									{
										creature.BardEndTime = DateTime.UtcNow;
									}

									creature.BardPacified = false;

									if (creature.AIObject != null)
										creature.AIObject.DoMove(creature.Direction);

									if (from is PlayerMobile player && !(player.HonorActive || TransformationSpellHelper.UnderTransformation(from, typeof(EtherealVoyageSpell))))
										creature.Combatant = from;
								}
								else
								{
									MBeingTamed[creature] = from;

									from.LocalOverheadMessage(MessageType.Emote, 0x59, 1010597); // You start to tame the creature.
									from.NonlocalOverheadMessage(MessageType.Emote, 0x59, 1010598); // *begins taming a creature.*

									new InternalTimer(from, creature, Utility.Random(3, 2)).Start();

									_mSetSkillTime = false;
								}
							}
							else
							{
								creature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502806, from.NetState); // You have no chance of taming this creature.
							}

							break;
						}
					}
				}
				else
				{
					mobile.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502469, from.NetState); // That being cannot be tamed.
				}
			}
			else
			{
				from.SendLocalizedMessage(502801); // You can't tame that!
			}
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile _mTamer;
			private readonly BaseCreature _mCreature;
			private readonly int _mMaxCount;
			private int _mCount;
			private bool _mParalyzed;
			private readonly DateTime _mStartTime;

			public InternalTimer(Mobile tamer, BaseCreature creature, int count) : base(TimeSpan.FromSeconds(3.0), TimeSpan.FromSeconds(3.0), count)
			{
				_mTamer = tamer;
				_mCreature = creature;
				_mMaxCount = count;
				_mParalyzed = creature.Paralyzed;
				_mStartTime = DateTime.UtcNow;
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				_mCount++;

				DamageEntry de = _mCreature.FindMostRecentDamageEntry(false);
				bool alreadyOwned = _mCreature.Owners.Contains(_mTamer);

				if (!_mTamer.InRange(_mCreature, Core.AOS ? 7 : 6))
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502795, _mTamer.NetState); // You are too far away to continue taming.
					Stop();
				}
				else if (!_mTamer.CheckAlive())
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502796, _mTamer.NetState); // You are dead, and cannot continue taming.
					Stop();
				}
				else if (!_mTamer.CanSee(_mCreature) || !_mTamer.InLOS(_mCreature) || !CanPath())
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mTamer.SendLocalizedMessage(1049654); // You do not have a clear path to the animal you are taming, and must cease your attempt.
					Stop();
				}
				else if (!_mCreature.Tamable)
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1049655, _mTamer.NetState); // That creature cannot be tamed.
					Stop();
				}
				else if (_mCreature.Controlled)
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502804, _mTamer.NetState); // That animal looks tame already.
					Stop();
				}
				else if (_mCreature.Owners.Count >= BaseCreature.MaxOwners && !_mCreature.Owners.Contains(_mTamer))
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1005615, _mTamer.NetState); // This animal has had too many owners and is too upset for you to tame.
					Stop();
				}
				else if (MustBeSubdued(_mCreature))
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1054025, _mTamer.NetState); // You must subdue this creature before you can tame it!
					Stop();
				}
				else if (de != null && de.LastDamage > _mStartTime)
				{
					MBeingTamed.Remove(_mCreature);
					_mTamer.NextSkillTime = Core.TickCount;
					_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502794, _mTamer.NetState); // The animal is too angry to continue taming.
					Stop();
				}
				else if (_mCount < _mMaxCount)
				{
					_mTamer.RevealingAction();

					switch (Utility.Random(3))
					{
						case 0: _mTamer.PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.Random(502790, 4)); break;
						case 1: _mTamer.PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.Random(1005608, 6)); break;
						case 2: _mTamer.PublicOverheadMessage(MessageType.Regular, 0x3B2, Utility.Random(1010593, 4)); break;
					}

					if (!alreadyOwned) // Passively check animal lore for gain
						_mTamer.CheckTargetSkill(SkillName.AnimalLore, _mCreature, 0.0, 120.0);

					if (_mCreature.Paralyzed)
						_mParalyzed = true;
				}
				else
				{
					_mTamer.RevealingAction();
					_mTamer.NextSkillTime = Core.TickCount;
					MBeingTamed.Remove(_mCreature);

					if (_mCreature.Paralyzed)
						_mParalyzed = true;

					if (!alreadyOwned) // Passively check animal lore for gain
						_mTamer.CheckTargetSkill(SkillName.AnimalLore, _mCreature, 0.0, 120.0);

					double minSkill = _mCreature.MinTameSkill + (_mCreature.Owners.Count * 6.0);

					if (minSkill > -24.9 && CheckMastery(_mTamer, _mCreature))
						minSkill = -24.9; // 50% at 0.0?

					minSkill += 24.9;

					if (CheckMastery(_mTamer, _mCreature) || alreadyOwned || _mTamer.CheckTargetSkill(SkillName.AnimalTaming, _mCreature, minSkill - 25.0, minSkill + 25.0))
					{
						if (_mCreature.Owners.Count == 0) // First tame
						{
							if (_mCreature is GreaterDragon)
							{
								ScaleSkills(_mCreature, 0.72, 0.90); // 72% of original skills trainable to 90%
								_mCreature.Skills[SkillName.Magery].Base = _mCreature.Skills[SkillName.Magery].Cap; // Greater dragons have a 90% cap reduction and 90% skill reduction on magery
							}
							else if (_mParalyzed)
								ScaleSkills(_mCreature, 0.86); // 86% of original skills if they were paralyzed during the taming
							else
								ScaleSkills(_mCreature, 0.90); // 90% of original skills

							if (_mCreature.StatLossAfterTame)
								ScaleStats(_mCreature, 0.50);
						}

						if (alreadyOwned)
						{
							_mTamer.SendLocalizedMessage(502797); // That wasn't even challenging.
						}
						else
						{
							_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502799, _mTamer.NetState); // It seems to accept you as master.
							_mCreature.Owners.Add(_mTamer);
						}

						_mCreature.SetControlMaster(_mTamer);
						_mCreature.IsBonded = false;

						EventSink.InvokeOnTameCreature(_mTamer, _mCreature);
					}
					else
					{
						_mCreature.PrivateOverheadMessage(MessageType.Regular, 0x3B2, 502798, _mTamer.NetState); // You fail to tame the creature.
					}
				}
			}

			private bool CanPath()
			{
				IPoint3D p = _mTamer;

				if (p == null)
					return false;

				if (_mCreature.InRange(new Point3D(p), 1))
					return true;

				MovementPath path = new(_mCreature, new Point3D(p));
				return path.Success;
			}
		}
	}
}
