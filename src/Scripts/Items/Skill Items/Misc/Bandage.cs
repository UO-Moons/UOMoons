#region References
using Server.Factions;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
#endregion

namespace Server.Items
{
	public class Bandage : Item, IDyable, ICommodity
	{
		public static void Initialize()
		{
			EventSink.OnBandageTargetRequest += BandageTargetRequest;
		}

		public static int Range = Core.AOS ? 2 : 1;

		public override double DefaultWeight => 0.1;

		[Constructable]
		public Bandage()
			: this(1)
		{ }

		[Constructable]
		public Bandage(int amount)
			: base(0xE21)
		{
			Stackable = true;
			Amount = amount;
		}

		public Bandage(Serial serial)
			: base(serial)
		{ }

		TextDefinition ICommodity.Description => LabelNumber;
		bool ICommodity.IsDeedable => true;

		public virtual bool Dye(Mobile from, DyeTub sender)
		{
			if (Deleted)
			{
				return false;
			}

			Hue = sender.DyedHue;

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		//This method added for [bandself command to call.
		public static void BandSelfCommandCall(Mobile from, Item m_Bandage)
		{
			from.RevealingAction();

			if (BandageContext.BeginHeal(from, from) != null)
			{
				m_Bandage.Consume();
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.InRange(GetWorldLocation(), Range))
			{
				from.RevealingAction();
				from.SendLocalizedMessage(500948); // Who will you use the bandages on?
				from.Target = new BandageTarget(this);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		public static void BandageTargetRequest(Mobile m, Item bandage, Mobile target)
		{
			BandageTargetRequest(bandage as Bandage, m, target);
		}

		public static void BandageTargetRequest(Bandage bandage, Mobile from, Mobile target)
		{
			if (bandage == null || bandage.Deleted)
			{
				return;
			}

			if (from.InRange(bandage.GetWorldLocation(), Range))
			{
				Target t = from.Target;

				if (t != null)
				{
					Target.Cancel(from);
					from.Target = null;
				}

				from.RevealingAction();
				from.SendLocalizedMessage(500948); // Who will you use the bandages on?

				new BandageTarget(bandage).Invoke(from, target);
			}
			else
			{
				from.SendLocalizedMessage(500295); // You are too far away to do that.
			}
		}

		private class BandageTarget : Target
		{
			private readonly Bandage m_Bandage;

			public BandageTarget(Bandage bandage)
				: base(Bandage.Range, false, TargetFlags.Beneficial)
			{
				m_Bandage = bandage;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (m_Bandage.Deleted)
				{
					return;
				}
				if (targeted is Mobile)
				{
					if (from.InRange(m_Bandage.GetWorldLocation(), Bandage.Range))
					{
						if (BandageContext.BeginHeal(from, (Mobile)targeted, m_Bandage is EnhancedBandage) != null)
						{
							if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
							{
								m_Bandage.Consume();
							}
						}
					}
					else
					{
						from.SendLocalizedMessage(500295); // You are too far away to do that.
					}
				}
				else if (targeted is PlagueBeastInnard)
				{
					if (((PlagueBeastInnard)targeted).OnBandage(from))
					{
						m_Bandage.Consume();
					}
				}
				else
				{
					from.SendLocalizedMessage(500970); // Bandages can not be used on that.
				}
			}

			protected override void OnNonlocalTarget(Mobile from, object targeted)
			{
				if (targeted is PlagueBeastInnard)
				{
					if (((PlagueBeastInnard)targeted).OnBandage(from))
					{
						m_Bandage.Consume();
					}
				}
				else
				{
					base.OnNonlocalTarget(from, targeted);
				}
			}
		}
	}

	public class BandageContext
	{
		public Mobile Healer { get; }
		public Mobile Patient { get; }
		public int Slips { get; set; }
		public int HealedPoisonOrBleed { get; set; }
		public Timer Timer { get; private set; }
		public int HealingBonus { get; private set; }

		public void Slip()
		{
			Healer.SendLocalizedMessage(500961); // Your fingers slip!
			++Slips;
		}

		public BandageContext(Mobile healer, Mobile patient, TimeSpan delay)
			: this(healer, patient, delay, false)
		{ }

		public BandageContext(Mobile healer, Mobile patient, TimeSpan delay, bool enhanced)
		{
			Healer = healer;
			Patient = patient;

			if (enhanced)
			{
				HealingBonus += EnhancedBandage.HealingBonus;
			}

			Timer = new BandageTimer(this, delay);
			Timer.Start();
		}

		public void StopHeal()
		{
			m_Table.Remove(Healer);

			if (Timer != null)
			{
				Timer.Stop();
			}

			Timer = null;
		}

		private static readonly Dictionary<Mobile, BandageContext> m_Table = new Dictionary<Mobile, BandageContext>();

		public static BandageContext GetContext(Mobile healer)
		{
			m_Table.TryGetValue(healer, out BandageContext bc);
			return bc;
		}

		public static SkillName GetPrimarySkill(Mobile healer, Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
			{
				return SkillName.Veterinary;
			}
			else
			{
				return SkillName.Healing;
			}
		}

		public static SkillName GetSecondarySkill(Mobile healer, Mobile m)
		{
			if (!m.Player && (m.Body.IsMonster || m.Body.IsAnimal))
			{
				return SkillName.AnimalLore;
			}
			else
			{
				return SkillName.Anatomy;
			}
		}

		public void CheckPoisonOrBleed()
		{
			bool bleeding = BleedAttack.IsBleeding(Patient);
			bool poisoned = Patient.Poisoned;

			if (bleeding || poisoned)
			{
				double healing = Healer.Skills[SkillName.Healing].Value;
				double anatomy = Healer.Skills[SkillName.Anatomy].Value;
				double chance = (healing + anatomy - 170) * 25;//120

				if (poisoned)
				{
					chance /= Patient.Poison.RealLevel * 20;
				}
				else
				{
					chance /= 3 * 20;
				}

				if (chance >= Utility.Random(100))
				{
					HealedPoisonOrBleed = poisoned ? Patient.Poison.RealLevel : 3;

					if (poisoned && Patient.CurePoison(Healer))
					{
						Patient.SendLocalizedMessage(1010059); // You have been cured of all poisons.
					}
					else
					{
						if (BleedAttack.IsBleeding(Patient))
						{
							BleedAttack.EndBleed(Patient, false);
						}

						Patient.SendLocalizedMessage(1060088); // You bind the wound and stop the bleeding
						Patient.SendLocalizedMessage(1060167); // The bleeding wounds have healed, you are no longer bleeding!
					}
				}
			}
		}

		public void EndHeal()
		{
			StopHeal();
			bool playSound = true;
			bool checkSkills = false;

			SkillName primarySkill = GetPrimarySkill(Healer, Patient);
			SkillName secondarySkill = GetSecondarySkill(Healer, Patient);

			BaseCreature petPatient = Patient as BaseCreature;
			int patientNumber;
			int healerNumber;
			if (!Healer.Alive)
			{
				healerNumber = 500962; // You were unable to finish your work before you died.
				patientNumber = -1;
				playSound = false;
			}
			else if (!Healer.InRange(Patient, Bandage.Range))
			{
				healerNumber = 500963; // You did not stay close enough to heal your target.
				patientNumber = -1;
				playSound = false;
			}
			else if (!Patient.Alive || (petPatient != null && petPatient.IsDeadPet))
			{
				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;
				double chance = ((healing - 68.0) / 50.0) - (Slips * 0.02);

				if (((checkSkills = healing >= 80.0 && anatomy >= 80.0) && chance > Utility.RandomDouble()) ||
					(Core.SE && petPatient is FactionWarHorse && petPatient.ControlMaster == Healer) ||
					(petPatient.ControlMaster == Healer))
				//TODO: Dbl check doesn't check for faction of the horse here?
				{
					if (Patient.Map == null || !Patient.Map.CanFit(Patient.Location, 16, false, false))
					{
						healerNumber = 501042; // Target can not be resurrected at that location.
						patientNumber = 502391; // Thou can not be resurrected there!
					}
					else if (Patient.Region != null && Patient.Region.IsPartOf("Khaldun"))
					{
						healerNumber = 1010395; // The veil of death in this area is too strong and resists thy efforts to restore life.
						patientNumber = -1;
					}
					else
					{
						healerNumber = 500965; // You are able to resurrect your patient.
						patientNumber = -1;

						Patient.PlaySound(0x214);
						Patient.FixedEffect(0x376A, 10, 16);

						if (petPatient != null && petPatient.IsDeadPet)
						{
							Mobile master = petPatient.ControlMaster;

							if (master != null && Healer == master)
							{
								petPatient.ResurrectPet();

								for (int i = 0; i < petPatient.Skills.Length; ++i)
								{
									petPatient.Skills[i].Base -= 0.1;
								}
							}
							else if (master != null && master.InRange(petPatient, 3))
							{
								if (!Core.EJ)
								{
									healerNumber = 503255; // You are able to resurrect the creature.
								}
								else
								{
									healerNumber = 1049658; //The owner has been asked to sanctify the resurrection.
								}

								master.CloseGump(typeof(PetResurrectGump));
								master.SendGump(new PetResurrectGump(Healer, petPatient));
							}
							else
							{
								bool found = false;

								var friends = petPatient.Friends;

								for (int i = 0; friends != null && i < friends.Count; ++i)
								{
									Mobile friend = friends[i];

									if (friend.InRange(petPatient, 3))
									{
										if (!Core.EJ)
										{
											healerNumber = 503255; // You are able to resurrect the creature.
										}
										else
										{
											healerNumber = 1049658; //The owner has been asked to sanctify the resurrection.
										}

										friend.CloseGump(typeof(PetResurrectGump));
										friend.SendGump(new PetResurrectGump(Healer, petPatient));

										found = true;
										break;
									}
								}

								if (!found)
								{
									if (!Core.EJ)
									{
										healerNumber = 1049670; // The pet's owner must be nearby to attempt resurrection.
									}
									else
									{
										healerNumber = 1049659; // Neither the owner or friends of the pet are nearby to sanctify the resurrection.
									}
								}
							}
						}
						else
						{
							Patient.CloseGump(typeof(ResurrectGump));
							Patient.SendGump(new ResurrectGump(Patient, Healer));
						}
					}
				}
				else
				{
					if (petPatient != null && petPatient.IsDeadPet)
					{
						healerNumber = 503256; // You fail to resurrect the creature.
					}
					else
					{
						healerNumber = 500966; // You are unable to resurrect your patient.
					}

					patientNumber = -1;
				}
			}
			else if (Patient.Poisoned)
			{
				Healer.SendLocalizedMessage(500969); // You finish applying the bandages.

				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;
				double chance = ((healing - 30.0) / 50.0) - (Patient.Poison.RealLevel * 0.1) - (Slips * 0.02);

				if ((checkSkills = healing >= 60.0 && anatomy >= 60.0) && chance > Utility.RandomDouble())
				{
					if (Patient.CurePoison(Healer))
					{
						healerNumber = (Healer == Patient) ? -1 : 1010058; // You have cured the target of all poisons.
						patientNumber = 1010059; // You have been cured of all poisons.
					}
					else
					{
						healerNumber = -1;
						patientNumber = -1;
					}
				}
				else
				{
					healerNumber = 1010060; // You have failed to cure your target!
					patientNumber = -1;
				}
			}
			else if (BleedAttack.IsBleeding(Patient))
			{
				healerNumber = 1060088; // You bind the wound and stop the bleeding
				patientNumber = 1060167; // The bleeding wounds have healed, you are no longer bleeding!

				BleedAttack.EndBleed(Patient, false);
			}
			else if (MortalStrike.IsWounded(Patient))
			{
				healerNumber = Healer == Patient ? 1005000 : 1010398;
				patientNumber = -1;
				playSound = false;
			}
			else if (Patient.Hits == Patient.HitsMax)
			{
				healerNumber = 500967; // You heal what little damage your patient had.
				patientNumber = -1;
			}
			else
			{
				checkSkills = true;
				patientNumber = -1;

				double healing = Healer.Skills[primarySkill].Value;
				double anatomy = Healer.Skills[secondarySkill].Value;

				Item item = Healer.FindItemOnLayer(Layer.TwoHanded);

				if (HealingBonus > 0)
				{
					healing += HealingBonus;
				}

				double chance = ((healing + 10.0) / 100.0) - (Slips * 0.02);

				if (chance > Utility.RandomDouble())
				{
					healerNumber = 500969; // You finish applying the bandages.

					double min, max;

					if (Core.AOS)
					{
						min = (anatomy / 8.0) + (healing / 5.0) + 4.0;
						max = (anatomy / 6.0) + (healing / 2.5) + 4.0;
					}
					else
					{
						min = (anatomy / 5.0) + (healing / 5.0) + 3.0;
						max = (anatomy / 5.0) + (healing / 2.0) + 10.0;
					}

					double toHeal = min + (Utility.RandomDouble() * (max - min));

					if (Patient.Body.IsMonster || Patient.Body.IsAnimal)
					{
						toHeal += Patient.HitsMax / 100;
					}

					if (Core.AOS)
					{
						toHeal -= toHeal * Slips * 0.35; // TODO: Verify algorithm
					}
					else
					{
						toHeal -= Slips * 4;
					}

					if (HealedPoisonOrBleed > 0)
					{
						toHeal /= HealedPoisonOrBleed;
					}

					if (toHeal < 1)
					{
						toHeal = 1;
						healerNumber = 500968; // You apply the bandages, but they barely help.
					}

					Patient.Heal((int)toHeal, Healer, false);
				}
				else
				{
					healerNumber = 500968; // You apply the bandages, but they barely help.
					playSound = false;
				}
			}

			if (healerNumber != -1)
			{
				Healer.SendLocalizedMessage(healerNumber);
			}

			if (patientNumber != -1)
			{
				Patient.SendLocalizedMessage(patientNumber);
			}

			if (playSound)
			{
				Patient.PlaySound(0x57);
			}

			if (checkSkills)
			{
				Healer.CheckSkill(secondarySkill, 0.0, 170.0);//120
				Healer.CheckSkill(primarySkill, 0.0, 170.0);//120
			}

			if (Patient is PlayerMobile)
			{
				BuffInfo.RemoveBuff(Healer, BuffIcon.Healing);
			}
			else
			{
				BuffInfo.RemoveBuff(Healer, BuffIcon.Veterinary);
			}
		}

		private class BandageTimer : Timer
		{
			private readonly BandageContext m_Context;
			private readonly long m_Begin;
			private readonly long m_Expires;
			private bool m_CheckedHealAndBleed;

			public bool CanCheckAtHalf => Core.SA &&
						   m_Context != null && m_Context.Healer == m_Context.Patient &&
						   m_Context.Healer.Skills[SkillName.Healing].Value >= 80 &&
						   m_Context.Healer.Skills[SkillName.Anatomy].Value >= 80;

			public BandageTimer(BandageContext context, TimeSpan delay)
				: base(TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250))
			{
				m_Context = context;
				Priority = TimerPriority.FiftyMS;

				m_Begin = Core.TickCount;
				m_Expires = Core.TickCount + (int)delay.TotalMilliseconds;
			}

			protected override void OnTick()
			{
				if (Core.TickCount >= m_Expires)
				{
					m_Context.EndHeal();
					Stop();
				}
				else if (!m_CheckedHealAndBleed && CanCheckAtHalf && m_Begin + ((m_Expires - m_Begin) / 2) < Core.TickCount)
				{
					m_Context.CheckPoisonOrBleed();
					m_CheckedHealAndBleed = true;
				}
			}
		}

		public static BandageContext BeginHeal(Mobile healer, Mobile patient)
		{
			return BeginHeal(healer, patient, false);
		}

		public static BandageContext BeginHeal(Mobile healer, Mobile patient, bool enhanced)
		{
			bool isDeadPet = patient is BaseCreature && ((BaseCreature)patient).IsDeadPet;

			if (patient is IRepairableMobile)
			{
				healer.SendLocalizedMessage(500970); // Bandages cannot be used on that.
			}
			else if (patient is BaseCreature && ((BaseCreature)patient).IsAnimatedDead)
			{
				healer.SendLocalizedMessage(500951); // You cannot heal that.
			}
			else if (!patient.Poisoned && patient.Hits == patient.HitsMax && !BleedAttack.IsBleeding(patient) && !isDeadPet)
			{
				healer.SendLocalizedMessage(500955); // That being is not damaged!
			}
			else if (!patient.Alive && (patient.Map == null || !patient.Map.CanFit(patient.Location, 16, false, false)))
			{
				healer.SendLocalizedMessage(501042); // Target cannot be resurrected at that location.
			}
			else if (healer.CanBeBeneficial(patient, true, true))
			{
				healer.DoBeneficial(patient);

				BandageContext context = GetContext(healer);

				if (context != null)
				{
					context.StopHeal();
				}

				var delay = GetDelay(healer, patient);

				if (patient is PlayerMobile)
				{
					BuffInfo.AddBuff(healer, new BuffInfo(BuffIcon.Healing, 1002082, 1151400, delay, healer, string.Format("{0}", patient.Name)));
				}
				else
				{
					BuffInfo.AddBuff(healer, new BuffInfo(BuffIcon.Veterinary, 1002167, 1151400, delay, healer, string.Format("{0}", patient.Name)));
				}

				context = new BandageContext(healer, patient, delay, enhanced);

				m_Table[healer] = context;

				if (healer != patient)
				{
					patient.SendLocalizedMessage(1008078, false, healer.Name); //  : Attempting to heal you.
				}

				healer.SendLocalizedMessage(500956); // You begin applying the bandages.

				if (healer.NetState != null && healer.NetState.IsEnhancedClient)
				{
					healer.NetState.Send(new BandageTimerPacket((int)delay.TotalSeconds));
				}

				return context;
			}

			return null;
		}

		public static TimeSpan GetDelay(Mobile healer, Mobile patient)
		{
			return GetDelay(healer, patient, !patient.Alive || patient.IsDeadBondedPet);
		}

		public static TimeSpan GetDelay(Mobile healer, Mobile patient, bool dead)
		{
			return GetDelay(healer, patient, dead, GetPrimarySkill(healer, patient));
		}

		public static TimeSpan GetDelay(Mobile healer, Mobile patient, bool dead, SkillName skill)
		{
			var resDelay = dead ? 5.0 : 0.0;
			var dex = healer.Dex;
			double seconds;

			if (healer == patient)
			{
				if (Core.AOS)
				{
					seconds = Math.Min(8, Math.Ceiling(11.0 - dex / 20));
					seconds = Math.Max(seconds, 4);
				}
				else
				{
					seconds = 9.4 + (0.6 * ((double)(170 - dex) / 10));//120
				}
			}
			else if (Core.AOS && skill == SkillName.Veterinary)
			{
				seconds = 2.0;
			}
			else if (Core.AOS)
			{
				seconds = Math.Ceiling((double)4 - dex / 60);
				seconds = Math.Max(seconds, 2);
			}
			else if (dex >= 100)
			{
				seconds = 3.0 + resDelay;
			}
			else if (dex >= 40)
			{
				seconds = 4.0 + resDelay;
			}
			else
			{
				seconds = 5.0 + resDelay;
			}

			return TimeSpan.FromSeconds(seconds);
		}
	}

	public sealed class BandageTimerPacket : Packet
	{
		public BandageTimerPacket(int duration)
			: base(0xBF)
		{
			EnsureCapacity(15);

			m_Stream.Write((short)0x31);
			m_Stream.Write((short)0x01);

			m_Stream.Write(0xE21);
			m_Stream.Write(duration);
		}
	}
}
