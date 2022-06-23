using Server.Items;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class Poisoning
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Poisoning].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.Target = new InternalTargetPoison();

		m.SendLocalizedMessage(502137); // Select the poison you wish to use

		return TimeSpan.FromSeconds(10.0); // 10 second delay before beign able to re-use a skill
	}

	private class InternalTargetPoison : Target
	{
		public InternalTargetPoison() : base(2, false, TargetFlags.None)
		{
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is BasePoisonPotion potion)
			{
				from.SendLocalizedMessage(502142); // To what do you wish to apply the poison?
				from.Target = new InternalTarget(potion);
			}
			else // Not a Poison Potion
			{
				from.SendLocalizedMessage(502139); // That is not a poison potion.
			}
		}

		private class InternalTarget : Target
		{
			private readonly BasePoisonPotion _mPotion;

			public InternalTarget(BasePoisonPotion potion) : base(2, false, TargetFlags.None)
			{
				_mPotion = potion;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (_mPotion.Deleted)
					return;

				bool startTimer = false;

				switch (targeted)
				{
					case Food or FukiyaDarts or Shuriken:
						startTimer = true;
						break;
					case BaseWeapon weapon when Core.AOS:
						startTimer = (weapon.PrimaryAbility == WeaponAbility.InfectiousStrike || weapon.SecondaryAbility == WeaponAbility.InfectiousStrike);
						break;
					case BaseWeapon weapon:
					{
						if (weapon.Layer == Layer.OneHanded)
						{
							// Only Bladed or Piercing weapon can be poisoned
							startTimer = (weapon.Type == WeaponType.Slashing || weapon.Type == WeaponType.Piercing);
						}

						break;
					}
				}

				if (startTimer)
				{
					new InternalTimer(from, (Item)targeted, _mPotion).Start();

					from.PlaySound(0x4F);

					if (!Engines.ConPVP.DuelContext.IsFreeConsume(from))
					{
						_mPotion.Consume();
						from.AddToBackpack(new EmptyBottle());
					}
				}
				else // Target can't be poisoned
				{
					// You cannot poison that! You can only poison infectious weapons, food or drink.// You cannot poison that! You can only poison bladed or piercing weapons, food or drink.
					from.SendLocalizedMessage(Core.AOS ? 1060204 : 502145);
				}
			}

			private class InternalTimer : Timer
			{
				private readonly Mobile _mFrom;
				private readonly Item _mTarget;
				private readonly Poison _mPoison;
				private readonly double _mMinSkill, _mMaxSkill;

				public InternalTimer(Mobile from, Item target, BasePoisonPotion potion) : base(TimeSpan.FromSeconds(2.0))
				{
					_mFrom = from;
					_mTarget = target;
					_mPoison = potion.Poison;
					_mMinSkill = potion.MinPoisoningSkill;
					_mMaxSkill = potion.MaxPoisoningSkill;
					Priority = TimerPriority.TwoFiftyMS;
				}

				protected override void OnTick()
				{
					if (_mFrom.CheckTargetSkill(SkillName.Poisoning, _mTarget, _mMinSkill, _mMaxSkill))
					{
						switch (_mTarget)
						{
							case Food food:
								food.Poison = _mPoison;
								break;
							case BaseWeapon weapon:
								weapon.Poison = _mPoison;
								weapon.PoisonCharges = 18 - _mPoison.Level * 2;
								break;
							case FukiyaDarts fukiya:
								fukiya.Poison = _mPoison;
								fukiya.PoisonCharges = Math.Min(18 - _mPoison.Level * 2, fukiya.UsesRemaining);
								break;
							case Shuriken shuriken:
								shuriken.Poison = _mPoison;
								shuriken.PoisonCharges = Math.Min(18 - _mPoison.Level * 2, shuriken.UsesRemaining);
								break;
						}

						_mFrom.SendLocalizedMessage(1010517); // You apply the poison

						Misc.Titles.AwardKarma(_mFrom, -20, true);
					}
					else // Failed
					{
						// 5% of chance of getting poisoned if failed
						if (_mFrom.Skills[SkillName.Poisoning].Base < 80.0 && Utility.Random(20) == 0)
						{
							_mFrom.SendLocalizedMessage(502148); // You make a grave mistake while applying the poison.
							_mFrom.ApplyPoison(_mFrom, _mPoison);
						}
						else
						{
							if (_mTarget is BaseWeapon weapon)
							{
								// You fail to apply a sufficient dose of poison on the blade// You fail to apply a sufficient dose of poison
								_mFrom.SendLocalizedMessage(weapon.Type == WeaponType.Slashing ? 1010516 : 1010518);
							}
							else
							{
								_mFrom.SendLocalizedMessage(1010518); // You fail to apply a sufficient dose of poison
							}
						}
					}
				}
			}
		}
	}
}
