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

	private static TimeSpan OnUse(Mobile m)
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
			private readonly Mobile m_From;
			private readonly Mobile m_Target;

			public InternalTimer(Mobile from, Mobile target) : base(TimeSpan.FromSeconds(2.0))
			{
				m_From = from;
				m_Target = target;
				Priority = TimerPriority.TwoFiftyMs;
			}

			protected override void OnTick()
			{
				Container theirPack = m_Target.Backpack;

				double badKarmaChance = 0.5 - ((double)m_From.Karma / 8570);

				if (theirPack == null)
				{
					m_From.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
				}
				else if (m_From.Karma < 0 && badKarmaChance > Utility.RandomDouble())
				{
					m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue, 500406); // Thou dost not look trustworthy... no gold for thee today!
				}
				else if (m_From.CheckTargetSkill(SkillName.Begging, m_Target, 0.0, 100.0))
				{
					if (!Core.ML && m_Target.Race != Race.Elf)
					{
						var toConsume = theirPack.GetAmount(typeof(Gold)) / 10;
						var max = 10 + m_From.Fame / 2500;

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
							var consumed = theirPack.ConsumeUpTo(typeof(Gold), toConsume);

							if (consumed > 0)
							{
								m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue,
									500405); // I feel sorry for thee...

								Gold gold = new(consumed);

								m_From.AddToBackpack(gold);
								m_From.PlaySound(gold.GetDropSound());

								if (m_From.Karma > -3000)
								{
									int toLose = m_From.Karma + 3000;

									if (toLose > 40)
										toLose = 40;

									Titles.AwardKarma(m_From, -toLose, true);
								}
							}
							else
							{
								m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue,
									500407); // I have not enough money to give thee any!
							}
						}
						else
						{
							m_Target.PublicOverheadMessage(MessageType.Regular, m_Target.SpeechHue,
								500407); // I have not enough money to give thee any!
						}
					}
					else
					{
						double chance = Utility.RandomDouble();
						BaseItem reward = null;
						string rewardName = "";
						switch (chance)
						{
							case >= .99:
							{
								int rand = Utility.Random(8);

								switch (rand)
								{
									case 0:
										reward = new Bedroll
										{
											Begged = true
										};
										rewardName = "a bedroll";
										break;
									case 1:
										reward = new Cookies
										{
											Begged = true
										};
										rewardName = "a plate of cookies.";
										break;
									case 2:
										reward = new FishSteak
										{
											Begged = true
										};
										rewardName = "a fish steak.";
										break;
									case 3:
										reward = new FishingPole
										{
											Begged = true
										};
										rewardName = "a fishing pole.";
										break;
									case 4:
										reward = new FlowerGarland
										{
											Begged = true
										};
										rewardName = "a flower garland.";
										break;
									case 5:
										reward = new Sake
										{
											Begged = true
										};
										rewardName = "a bottle of Sake.";
										break;
									case 6:
										reward = new Turnip
										{
											Begged = true
										};
										rewardName = "a turnip.";
										break;
									case 7:
										reward = new BeverageBottle(BeverageType.Wine)
										{
											Begged = true
										};
										rewardName = "a Bottle of wine.";
										break;
									case 8:
										reward = new Pitcher(BeverageType.Wine)
										{
											Begged = true
										};
										rewardName = "a Pitcher of wine.";
										break;
								}

								break;
							}
							case >= .76:
							{
								int rand = Utility.Random(6);

								switch (rand)
								{
									case 0:
										reward = new WoodenBowlOfStew
										{
											Begged = true
										};
										rewardName = "a bowl of stew.";
										break;
									case 1:
										reward = new CheeseWedge
										{
											Begged = true
										};
										rewardName = "a wedge of cheese.";
										break;
									case 2:
										reward = new Dates
										{
											Begged = true
										};
										rewardName = "a bunch of dates.";
										break;
									case 3:
										reward = new Lantern
										{
											Begged = true
										};
										rewardName = "a lantern.";
										break;
									case 4:
										reward = new Pitcher(BeverageType.Liquor)
										{
											Begged = true
										};
										rewardName = "a Pitcher of liquor";
										break;
									case 5:
										reward = new CheesePizza
										{
											Begged = true
										};
										rewardName = "pizza";
										break;
									case 6:
										reward = new Shirt
										{
											Begged = true
										};
										rewardName = "a shirt.";
										break;
								}

								break;
							}
							case >= .25:
							{
								int rand = Utility.Random(1);

								if (rand == 0)
								{
									reward = new FrenchBread
									{
										Begged = true
									};
									rewardName = "french bread.";
								}
								else
								{
									reward = new Pitcher(BeverageType.Water)
									{
										Begged = true,
										ItemId = Utility.RandomDouble() > .5 ? 4088 : 4089
									};
									rewardName = "a Pitcher of water.";
								}

								break;
							}
						}

						reward ??= new Gold(1);

						m_Target.Say(1074854); // Here, take this...
						m_From.AddToBackpack(reward);
						m_From.SendLocalizedMessage(1074853, rewardName); // You have been given ~1_name~

						if (m_From.Karma > -3000)
						{
							int toLose = m_From.Karma + 3000;

							if (toLose > 40)
							{
								toLose = 40;
							}

							Titles.AwardKarma(m_From, -toLose, true);
						}

					}
				}
				else
				{
					m_Target.SendLocalizedMessage(500404); // They seem unwilling to give you any money.
				}

				m_From.NextSkillTime = Core.TickCount + 10000;
				
			}
		}
	}
}
