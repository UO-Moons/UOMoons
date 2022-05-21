using Server.Items;
using Server.Multis;
using System;
using System.Collections;
using System.Linq;

namespace Server.Engines.TownHouses
{
	public sealed class RentalContract : TownHouseSign
	{
		private Mobile m_CRentalClient;
		public BaseHouse ParentHouse { get; private set; }
		public Mobile RentalClient { get => m_CRentalClient; set { m_CRentalClient = value; InvalidateProperties(); } }
		public Mobile RentalMaster { get; private set; }
		public bool Completed { get; set; }
		public bool EntireHouse { get; set; }

		public RentalContract() : base()
		{
			ItemID = 0x14F0;
			Movable = true;
			RentByTime = TimeSpan.FromDays(1);
			RecurRent = true;
			MaxZ = MinZ;
		}

		public bool HasContractedArea(Rectangle2D rect, int z)
		{
			foreach (var item in AllSigns)
			{
				if (item is not RentalContract contract || item == this || item.Map != Map ||
				    ParentHouse != contract.ParentHouse)
					continue;

				foreach (var rect2 in contract.Blocks)
				{
					for (var x = rect.Start.X; x < rect.End.X; ++x)
					{
						for (var y = rect.Start.Y; y < rect.End.Y; ++y)
						{
							if (!rect2.Contains(new Point2D(x, y)))
								continue;

							if (contract.MinZ <= z && contract.MaxZ >= z)
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		public bool HasContractedArea(int z)
		{
			return AllSigns.Cast<Item>().Where(item => item is RentalContract contract && item != this && item.Map == Map && ParentHouse == contract.ParentHouse).Any(item => ((RentalContract)item).MinZ <= z && ((RentalContract)item).MaxZ >= z);
		}

		private void DepositTo(Mobile m)
		{
			if (m == null)
			{
				return;
			}

			if (Free)
			{
				m.SendMessage("Since this home is free, you do not receive the deposit.");
				return;
			}

			m.BankBox.DropItem(new Gold(Price));
			m.SendMessage("You have received a {0} gold deposit from your town house.", Price);
		}

		public override void ValidateOwnership()
		{
			if (Completed && RentalMaster == null)
			{
				Delete();
				return;
			}

			if (m_CRentalClient != null && (ParentHouse == null || ParentHouse.Deleted))
			{
				Delete();
				return;
			}

			if (m_CRentalClient != null && !Owned)
			{
				Delete();
				return;
			}

			if (ParentHouse == null)
			{
				return;
			}

			if (!ValidateLocSec())
			{
				if (DemolishTimer == null)
				{
					BeginDemolishTimer(TimeSpan.FromHours(48));
				}
			}
			else
			{
				ClearDemolishTimer();
			}
		}

		protected override void DemolishAlert()
		{
			if (ParentHouse == null || RentalMaster == null || m_CRentalClient == null)
			{
				return;
			}

			RentalMaster.SendMessage("You have begun to use lockdowns reserved for {0}, and their rental unit will collapse in {1}.", m_CRentalClient.Name, Math.Round((DemolishTime - DateTime.UtcNow).TotalHours, 2));
			m_CRentalClient.SendMessage("Alert your land lord, {0}, they are using storage reserved for you.  They have violated the rental agreement, which will end in {1} if nothing is done.", RentalMaster.Name, Math.Round((DemolishTime - DateTime.UtcNow).TotalHours, 2));
		}

		public void FixLocSec()
		{
			int count;
			if ((count = General.RemainingSecures(ParentHouse) + Secures) < Secures)
			{
				Secures = count;
			}

			if ((count = General.RemainingLocks(ParentHouse) + Locks) < Locks)
			{
				Locks = count;
			}
		}

		public bool ValidateLocSec()
		{
			if (General.RemainingSecures(ParentHouse) + Secures < Secures)
			{
				return false;
			}

			return General.RemainingLocks(ParentHouse) + Locks >= Locks;
		}

		protected override void ConvertItems(bool keep)
		{
			if (House == null || ParentHouse == null || RentalMaster == null)
			{
				return;
			}

			foreach (BaseDoor door in new ArrayList(ParentHouse.Doors).Cast<BaseDoor>().Where(door => door.Map == House.Map && House.Region.Contains(door.Location)))
			{
				ConvertDoor(door);
			}

			foreach (SecureInfo info in new ArrayList(ParentHouse.Secures).Cast<SecureInfo>().Where(info => info.Item.Map == House.Map && House.Region.Contains(info.Item.Location)))
			{
				ParentHouse.Release(RentalMaster, info.Item);
			}

			foreach (Item item in new ArrayList(ParentHouse.LockDowns).Cast<Item>().Where(item => item.Map == House.Map && House.Region.Contains(item.Location)))
			{
				ParentHouse.Release(RentalMaster, item);
			}
		}

		protected override void UnconvertDoors()
		{
			if (House == null || ParentHouse == null)
			{
				return;
			}

			foreach (BaseDoor door in new ArrayList(House.Doors))
			{
				House.Doors.Remove(door);
			}
		}

		protected override void OnRentPaid()
		{
			if (RentalMaster == null || m_CRentalClient == null)
			{
				return;
			}

			if (Free)
			{
				return;
			}

			RentalMaster.BankBox.DropItem(new Gold(Price));
			RentalMaster.SendMessage("The bank has transfered your rent from {0}.", m_CRentalClient.Name);
		}

		public override void ClearHouse()
		{
			if (!Deleted)
			{
				Delete();
			}

			base.ClearHouse();
		}

		public override void OnDoubleClick(Mobile m)
		{
			ValidateOwnership();

			if (Deleted)
			{
				return;
			}

			RentalMaster ??= m;

			var house = BaseHouse.FindHouseAt(m);

			ParentHouse ??= house;

			if (house == null || (house != ParentHouse && house != House))
			{
				m.SendMessage("You must be in the home to view this contract.");
				return;
			}

			if (m == RentalMaster
			 && !Completed
			 && house is TownHouse house1
			 && house1.ForSaleSign.PriceType != "Sale")
			{
				ParentHouse = null;
				m.SendMessage("You can only rent property you own.");
				return;
			}

			if (m == RentalMaster && !Completed && General.EntireHouseContracted(ParentHouse))
			{
				m.SendMessage("This entire house already has a rental contract.");
				return;
			}

			if (Completed)
			{
				_ = new ContractConfirmGump(m, this);
			}
			else if (m == RentalMaster)
			{
				_ = new ContractSetupGump(m, this);
			}
			else
			{
				m.SendMessage("This rental contract has not yet been completed.");
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			if (m_CRentalClient != null)
			{
				list.Add("a house rental contract with " + m_CRentalClient.Name);
			}
			else if (Completed)
			{
				list.Add("a completed house rental contract");
			}
			else
			{
				list.Add("an uncompleted house rental contract");
			}
		}

		public override void Delete()
		{
			if (ParentHouse == null)
			{
				base.Delete();
				return;
			}

			if (!Owned && !ParentHouse.IsFriend(m_CRentalClient))
			{
				if (m_CRentalClient != null && RentalMaster != null)
				{
					RentalMaster.SendMessage("{0} has ended your rental agreement.  Because you revoked their access, their last payment will be refunded.", RentalMaster.Name);
					m_CRentalClient.SendMessage("You have ended your rental agreement with {0}.  Because your access was revoked, your last payment is refunded.", m_CRentalClient.Name);
				}

				DepositTo(m_CRentalClient);
			}
			else if (Owned)
			{
				if (m_CRentalClient != null && RentalMaster != null)
				{
					m_CRentalClient.SendMessage("{0} has ended your rental agreement.  Since they broke the contract, your are refunded the last payment.", RentalMaster.Name);
					RentalMaster.SendMessage("You have ended your rental agreement with {0}.  They will be refunded their last payment.", m_CRentalClient.Name);
				}

				DepositTo(m_CRentalClient);

				PackUpHouse();
			}
			else
			{
				if (m_CRentalClient != null && RentalMaster != null)
				{
					RentalMaster.SendMessage("{0} has ended your rental agreement.", m_CRentalClient.Name);
					m_CRentalClient.SendMessage("You have ended your rental agreement with {0}.", RentalMaster.Name);
				}

				DepositTo(RentalMaster);
			}

			ClearRentTimer();
			base.Delete();
		}

		public RentalContract(Serial serial) : base(serial)
		{
			RecurRent = true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			// Version 1

			writer.Write(EntireHouse);

			writer.Write(RentalMaster);
			writer.Write(m_CRentalClient);
			writer.Write(ParentHouse);
			writer.Write(Completed);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (version >= 1)
			{
				EntireHouse = reader.ReadBool();
			}

			RentalMaster = reader.ReadMobile();
			m_CRentalClient = reader.ReadMobile();
			ParentHouse = reader.ReadItem() as BaseHouse;
			Completed = reader.ReadBool();
		}
	}
}
