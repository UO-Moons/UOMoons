using Server.Items;
using Server.Misc;
using Server.Network;
using Server.Targeting;
using System;

namespace Server.SkillHandlers;

public class Begging
{
	public static void Initialize()
	{
		SkillInfo.Table[(int)SkillName.Begging].Callback = OnUse;
	}

	public static TimeSpan OnUse(Mobile m)
	{
		m.RevealingAction();

		m.Target = new InternalTarget();
		m.RevealingAction();

		m.SendLocalizedMessage(500397); // To whom do you wish to grovel?

		return TimeSpan.FromHours(6.0);
	}

	private class InternalTarget : Target
	{
		private bool _mSetSkillTime = true;

		public InternalTarget() : base(12, false, TargetFlags.None)
		{
		}

		protected override void OnTargetFinish(Mobile from)
		{
			if (_mSetSkillTime)
				from.NextSkillTime = Core.TickCount;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			from.RevealingAction();

			int number = -1;

			if (targeted is Mobile targ)
			{
				if (targ.Player) // We can't beg from players
				{
					number = 500398; // Perhaps just asking would work better.
				}
				else if (!targ.Body.IsHuman) // Make sure the NPC is human
				{
					number = 500399; // There is little chance of getting money from that!
				}
				else if (!from.InRange(targ, 2))
				{
					// You are too far away to beg from him.// You are too far away to beg from her.
					number = !targ.Female ? 500401 : 500402;
				}
				else if (!Core.ML && from.Mounted) // If we're on a mount, who would give us money?
				{
					number = 500404; // They seem unwilling to give you any money.
				}
				else
				{
					// Face each other
					from.Direction = from.GetDirectionTo(targ);
					targ.Direction = targ.GetDirectionTo(from);

					from.Animate(32, 5, 1, true, false, 0); // Bow

					new InternalTimer(from, targ).Start();

					_mSetSkillTime = false;
				}
			}
			else // Not a Mobile
			{
				number = 500399; // There is little chance of getting money from that!
			}

			if (number != -1)
				from.SendLocalizedMessage(number);
		}

		private class InternalTimer : Timer
		{
			private readonly Mobile _mFrom;
			private readonly Mobile _mTarget;

			public InternalTimer(Mobile from, Mobile target) : base(TimeSpan.FromSeconds(2.0))
			{
				_mFrom = from;
				_mTarget = target;
				Priority = TimerPriority.TwoFiftyMs;
			}

			protected override void OnTick()
			{
				Container theirPack = _mTarget.Backpack;

				double badKarmaChance = 0.5 - ((double)_mFrom.Karma / 8570);

				if (theirPack == null)
				{
					_mFrom.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
				}
				else if (_mFrom.Karma < 0 && badKarmaChance > Utility.RandomDouble())
				{
					_mTarget.PublicOverheadMessage(MessageType.Regular, _mTarget.SpeechHue, 500406); // Thou dost not look trustworthy... no gold for thee today!
				}
				else if (_mFrom.CheckTargetSkill(SkillName.Begging, _mTarget, 0.0, 100.0))
				{
					int toConsume = theirPack.GetAmount(typeof(Gold)) / 10;
					int max = 10 + (_mFrom.Fame / 2500);

					max = max switch
					{
						> 14 => 14,
						< 10 => 10,
						_ => max
					};

					if (toConsume > max)
						toConsume = max;

					if (toConsume > 0)
					{
						int consumed = theirPack.ConsumeUpTo(typeof(Gold), toConsume);

						if (consumed > 0)
						{
							_mTarget.PublicOverheadMessage(MessageType.Regular, _mTarget.SpeechHue, 500405); // I feel sorry for thee...

							Gold gold = new(consumed);

							_mFrom.AddToBackpack(gold);
							_mFrom.PlaySound(gold.GetDropSound());

							if (_mFrom.Karma > -3000)
							{
								int toLose = _mFrom.Karma + 3000;

								if (toLose > 40)
									toLose = 40;

								Titles.AwardKarma(_mFrom, -toLose, true);
							}
						}
						else
						{
							_mTarget.PublicOverheadMessage(MessageType.Regular, _mTarget.SpeechHue, 500407); // I have not enough money to give thee any!
						}
					}
					else
					{
						_mTarget.PublicOverheadMessage(MessageType.Regular, _mTarget.SpeechHue, 500407); // I have not enough money to give thee any!
					}
				}
				else
				{
					_mTarget.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
				}

				_mFrom.NextSkillTime = Core.TickCount + 10000;
			}
		}
	}
}
