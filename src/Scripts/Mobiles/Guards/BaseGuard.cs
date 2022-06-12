using Server.Items;
using System;

namespace Server.Mobiles
{
	public abstract class BaseGuard : BaseConvo
	{
		public BaseGuard(Mobile target) : base(AIType.AI_Melee, FightMode.Aggressor, 14, 1, 0.2, 1.0)
		{
			GuardImmune = true;
			Title = "the guard";
			Job = JobFragment.guard;

			if (target != null)
			{
				Location = target.Location;
				Map = target.Map;

				Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 5023);
			}
		}

		public BaseGuard(Serial serial)
			: base(serial)
		{
		}

		public abstract Mobile Focus { get; set; }

		public override bool CanBeHarmful(IDamageable target, bool message, bool ignoreOurBlessedness)
		{
			if (target is Mobile mobile && mobile.GuardImmune)
			{
				return false;
			}

			return base.CanBeHarmful(target, message, ignoreOurBlessedness);
		}

		public static void Spawn(Mobile caller, Mobile target)
		{
			Spawn(caller, target, 1, false);
		}

		public static void Spawn(Mobile caller, Mobile target, int amount, bool onlyAdditional)
		{
			if (target == null || target.Deleted)
				return;

			foreach (Mobile m in target.GetMobilesInRange(15))
			{
				if (m is BaseGuard g)
				{
					if (g.Focus == null) // idling
					{
						g.Focus = target;

						--amount;
					}
					else if (g.Focus == target && !onlyAdditional)
					{
						--amount;
					}
				}
			}

			while (amount-- > 0)
				caller.Region.MakeGuard(target);
		}

		public override bool OnBeforeDeath()
		{
			Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 10, 2023);

			PlaySound(0x1FE);

			Delete();

			return false;
		}

		public override bool OnDragDrop(Mobile m, Item item)
		{
			if (item is Head head)
			{
				if (head.Owner is not PlayerMobile mobile || head.Age >= TimeSpan.FromDays(1.0) || head.MaxBounty <= 0 || mobile.Bounty <= 0 || head.Owner == m || (m.Account != null && head.Owner.Account != null && head.Owner.Account == m.Account))
				{
					Say(true, "'Tis a decapitated head. How disgusting.");
				}
				else
				{
					PlayerMobile owner = mobile;

					int total = owner.Bounty;
					if (head.MaxBounty < total)
						total = head.MaxBounty;

					if (total >= 15000)
						Say(true, String.Format("Thou hast brought the infamous {0} to justice!  Thy reward of {1}gp hath been deposited in thy bank account.", owner.Name, total));
					else if (total > 100)
						Say(true, String.Format("Tis a minor criminal, thank thee. Thy reward of {0}gp hath been deposited in thy bank account.", total));
					else
						Say(true, String.Format("Thou hast wasted thy time for {0}gp.", total));

					double statloss = 1.0;

					switch (total)
					{
						case > 750000:
							statloss = 0.90;
							break;
						case > 500000:
							statloss = 0.91;
							break;
						case > 300000:
							statloss = 0.92;
							break;
						case > 200000:
							statloss = 0.93;
							break;
						case > 125000:
							statloss = 0.94;
							break;
						case > 75000:
							statloss = 0.95;
							break;
						case > 50000:
							statloss = 0.96;
							break;
						case > 30000:
							statloss = 0.97;
							break;
						case > 15000:
							statloss = 0.98;
							break;
						case > 5000:
							statloss = 0.99;
							break;
					}

					if (statloss < 1.0)
					{
						if (statloss < 0.9)
							statloss = 0.9;

						if (owner.RawStr * statloss >= 10)
							owner.RawStr = (int)(owner.RawStr * statloss);
						else
							owner.RawStr = 10;

						if (owner.RawDex * statloss >= 10)
							owner.RawDex = (int)(owner.RawDex * statloss);
						else
							owner.RawDex = 10;

						if (owner.RawInt * statloss >= 10)
							owner.RawInt = (int)(owner.RawInt * statloss);
						else
							owner.RawInt = 10;

						for (int i = 0; i < owner.Skills.Length; i++)
						{
							if (owner.Skills[i].Base * statloss > 50)
								owner.Skills[i].Base *= statloss;
						}

						owner.SendAsciiMessage("Thy head has been turned in for a bounty of {0}gp.  Suffer thy punishment!", total);
					}

					if (total < owner.Bounty)
						owner.Kills /= 2;
					else
						owner.Kills = 0;

					owner.Bounty -= total;
					if (owner.Bounty < 0)
						owner.Bounty = 0;

					while (total > 0)
					{
						int amount = total > 65000 ? 65000 : total;
						m.BankBox.DropItem(new Gold(amount));
						total -= amount;
					}
				}

				return true;
			}
			else
			{
				this.Say(true, "I have no use for this.");
				return false;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}
}
