using Server.Engines.VeteranRewards;
using Server.Items;
using Server.Multis;
using Server.Network;
using Server.Spells;
using System;

namespace Server.Mobiles
{
	public class EtherealMount : Item, IMount, IMountItem, IRewardItem
	{
		public static readonly int DefaultEtherealHue = 0x4001;

		private Mobile m_Rider;
		private bool m_Transparent;

		private int m_TransparentMountedID;
		private int m_NonTransparentMountedID;
		private int m_TransparentMountedHue;
		private int m_NonTransparentMountedHue;

		private int m_StatueID;
		private int m_StatueHue;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Transparent
		{
			get { return m_Transparent; }
			set
			{
				if (Rider != null)
				{
					if (value && !m_Transparent)
					{
						ItemId = m_TransparentMountedID;
						Hue = m_TransparentMountedHue;
					}
					else if (!value && m_Transparent)
					{
						ItemId = m_NonTransparentMountedID;
						Hue = m_NonTransparentMountedHue;
					}
				}

				m_Transparent = value;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TransparentMountedID
		{
			get { return m_TransparentMountedID; }
			set
			{
				if (m_TransparentMountedID != value)
				{
					m_TransparentMountedID = value;

					if (m_Rider != null && Transparent)
					{
						ItemId = value;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int NonTransparentMountedID
		{
			get { return m_NonTransparentMountedID; }
			set
			{
				if (m_NonTransparentMountedID != value)
				{
					m_NonTransparentMountedID = value;

					if (m_Rider != null && !Transparent)
					{
						ItemId = value;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TransparentMountedHue
		{
			get { return m_TransparentMountedHue; }
			set
			{
				if (m_TransparentMountedHue != value)
				{
					m_TransparentMountedHue = value;

					if (m_Rider != null && Transparent)
					{
						Hue = value;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int NonTransparentMountedHue
		{
			get { return m_NonTransparentMountedHue; }
			set
			{
				if (m_NonTransparentMountedHue != value)
				{
					m_NonTransparentMountedHue = value;

					if (m_Rider != null && !Transparent)
					{
						Hue = value;
					}

					InvalidateProperties();
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StatueID
		{
			get { return m_StatueID; }
			set
			{
				if (m_StatueID != value)
				{
					m_StatueID = value;

					if (m_Rider == null)
					{
						ItemId = value;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int StatueHue
		{
			get { return m_StatueHue; }
			set
			{
				if (m_StatueHue != value)
				{
					m_StatueHue = value;

					if (Rider == null)
					{
						Hue = value;
					}
				}
			}
		}

		public int MountedID => Transparent ? TransparentMountedID : NonTransparentMountedID;

		public int MountedHue => Transparent ? TransparentMountedHue : NonTransparentMountedHue;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem { get; set; }

		public override double DefaultWeight => 1.0;

		public override bool DisplayLootType => Core.AOS;
		public virtual int FollowerSlots => 1;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Rider
		{
			get { return m_Rider; }
			set
			{
				if (value == m_Rider)
				{
					return;
				}

				if (value == null)
				{
					Internalize();
					UnmountMe();

					RemoveFollowers();
					m_Rider = null;
				}
				else
				{
					if (m_Rider != null)
					{
						Dismount(m_Rider);
					}

					Dismount(value);

					RemoveFollowers();
					m_Rider = value;
					AddFollowers();

					MountMe();
				}
			}
		}

		public IMount Mount => this;

		[Constructable]
		public EtherealMount(int itemID, int transMountedID, int nonTransMountedID, int transHue = 0, int nonTransHue = 0)
			: base(itemID)
		{
			m_Rider = null;
			Layer = Layer.Invalid;
			LootType = LootType.Blessed;

			m_Transparent = true;
			m_TransparentMountedID = transMountedID;
			m_NonTransparentMountedID = nonTransMountedID;
			m_TransparentMountedHue = transHue;
			m_NonTransparentMountedHue = nonTransHue;

			StatueID = itemID;
			StatueHue = 0;
		}

		public EtherealMount(Serial serial)
			: base(serial)
		{
		}

		public static void Dismount(Mobile m)
		{
			IMount mount = m.Mount;

			if (mount != null)
			{
				mount.Rider = null;
			}
		}

		public static void StopMounting(Mobile mob)
		{
			if (mob.Spell is EtherealSpell)
			{
				((EtherealSpell)mob.Spell).Stop();
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (Core.ML && IsRewardItem)
			{
				list.Add(RewardSystem.GetRewardYearLabel(this, new object[] { })); // X Year Veteran Reward
			}

			EtherealRetouchingTool.AddProperty(this, list);
		}

		public void RemoveFollowers()
		{
			if (m_Rider != null)
			{
				m_Rider.Followers -= FollowerSlots;
			}

			if (m_Rider != null && m_Rider.Followers < 0)
			{
				m_Rider.Followers = 0;
			}
		}

		public void AddFollowers()
		{
			if (m_Rider != null)
			{
				m_Rider.Followers += FollowerSlots;
			}
		}

		public virtual bool Validate(Mobile from)
		{
			if (from.Race == Race.Gargoyle)
			{
				from.SendLocalizedMessage(1112281); // gargs can't mount
				return false;
			}

			if (Parent == null)
			{
				from.SayTo(from, 1010095); // This must be on your person to use.
				return false;
			}

			if (IsRewardItem && !RewardSystem.CheckIsUsableBy(from, this, null))
			{
				// CheckIsUsableBy sends the message
				return false;
			}

			if (!BaseMount.CheckMountAllowed(from, true))
			{
				// CheckMountAllowed sends the message
				return false;
			}

			if (from.Mount is BaseBoat)
			{
				PrivateOverheadMessage(MessageType.Regular, 0x3B2, 1042146, from.NetState); // You cannot use this while mounted.
				return false;
			}

			if (from.Mounted)
			{
				from.SendLocalizedMessage(1005583); // Please dismount first.
				return false;
			}

			if (from.IsBodyMod && !from.Body.IsHuman)
			{
				from.SendLocalizedMessage(1061628); // You can't do that while polymorphed.
				return false;
			}

			if (from.HasTrade)
			{
				from.SendLocalizedMessage(1042317, "", 0x41); // You may not ride at this time
				return false;
			}

			if ((from.Followers + FollowerSlots) > from.FollowersMax)
			{
				from.SendLocalizedMessage(1049679); // You have too many followers to summon your mount.
				return false;
			}

			if (!DesignContext.Check(from))
			{
				// Check sends the message
				return false;
			}

			return true;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Validate(from))
			{
				new EtherealSpell(this, from).Cast();
			}
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			LabelTo(from, "Veteran Reward");
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(7);
			writer.Write(m_Transparent);
			writer.Write(m_TransparentMountedID);
			writer.Write(m_NonTransparentMountedID);
			writer.Write(m_TransparentMountedHue);
			writer.Write(m_NonTransparentMountedHue);
			writer.Write(m_StatueID);
			writer.Write(m_StatueHue);
			writer.Write(IsRewardItem);
			writer.Write(m_Rider);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			LootType = LootType.Blessed;

			int version = reader.ReadInt();

			switch (version)
			{
				case 7:
				case 6:
				case 5:
					m_Transparent = reader.ReadBool();
					m_TransparentMountedID = reader.ReadInt();
					m_NonTransparentMountedID = reader.ReadInt();
					m_TransparentMountedHue = reader.ReadInt();
					m_NonTransparentMountedHue = reader.ReadInt();
					m_StatueID = reader.ReadInt();
					m_StatueHue = reader.ReadInt();

					IsRewardItem = reader.ReadBool();
					m_Rider = reader.ReadMobile();
					break;
				case 4:
					m_NonTransparentMountedID = reader.ReadInt(); // m_DefaultMountedID = reader.ReadInt();
					m_NonTransparentMountedHue = reader.ReadInt(); // m_OriginalHue = reader.ReadInt();
					m_TransparentMountedHue = reader.ReadInt(); // m_EtherealHue = reader.ReadInt();
					goto case 3;
				case 3:
					reader.ReadBool();
					goto case 2;
				case 2:
					IsRewardItem = reader.ReadBool();
					goto case 0;
				case 1:
					reader.ReadInt();
					goto case 0;
				case 0:
					{
						m_TransparentMountedID = reader.ReadInt(); // m_MountedID = reader.ReadInt();
						m_StatueID = reader.ReadInt(); // m_RegularID = reader.ReadInt();
						m_Rider = reader.ReadMobile();

						if (m_TransparentMountedID == 0x3EA2)
						{
							m_TransparentMountedID = 0x3EAA;
						}
					}
					break;
			}

			AddFollowers();
		}

		public override DeathMoveResult OnParentDeath(Mobile parent)
		{
			Rider = null; //get off, move to pack

			return DeathMoveResult.RemainEquiped;
		}

		public void UnmountMe()
		{
			Container bp = m_Rider.Backpack;

			ItemId = m_StatueID;
			Layer = Layer.Invalid;

			Movable = true;

			if (Hue != m_StatueHue)
			{
				Hue = m_StatueHue;
			}

			if (bp != null)
			{
				bp.DropItem(this);
				return;
			}

			Point3D loc = m_Rider.Location;
			Map map = m_Rider.Map;

			if (map == null || map == Map.Internal)
			{
				loc = m_Rider.LogoutLocation;
				map = m_Rider.LogoutMap;
			}

			MoveToWorld(loc, map);
		}

		public void MountMe()
		{
			StatueHue = Hue;

			ItemId = MountedID;
			Hue = MountedHue;

			Layer = Layer.Mount;
			Movable = false;

			ProcessDelta();
			m_Rider.ProcessDelta();
			m_Rider.EquipItem(this);
			m_Rider.ProcessDelta();
			ProcessDelta();
		}

		public virtual void OnRiderDamaged(Mobile from, ref int amount, bool willKill)
		{ }

		private class EtherealSpell : Spell
		{
			private static readonly SpellInfo m_Info = new SpellInfo("Ethereal Mount", "", 230);
			private readonly EtherealMount m_Mount;
			private readonly Mobile m_Rider;
			private bool m_Stop;

			public EtherealSpell(EtherealMount mount, Mobile rider)
				: base(rider, null, m_Info)
			{
				m_Rider = rider;
				m_Mount = mount;
			}

			public override bool ClearHandsOnCast => false;
			public override bool RevealOnCast => false;
			public override double CastDelayFastScalar => 0;

			public override TimeSpan CastDelayBase => TimeSpan.FromSeconds(Core.AOS ? 3.0 : 2.0);

			public override TimeSpan GetCastRecovery()
			{
				return TimeSpan.Zero;
			}

			public override int GetMana()
			{
				return 0;
			}

			public override bool ConsumeReagents()
			{
				return true;
			}

			public override bool CheckFizzle()
			{
				return true;
			}

			public void Stop()
			{
				m_Stop = true;
				Disturb(DisturbType.Hurt, false, false);
			}

			public override bool CheckDisturb(DisturbType type, bool resistable)
			{
				if (type == DisturbType.EquipRequest || type == DisturbType.UseRequest /* || type == DisturbType.Hurt*/)
				{
					return false;
				}

				return true;
			}

			public override void DoHurtFizzle()
			{
				if (!m_Stop)
				{
					base.DoHurtFizzle();
				}
			}

			public override void DoFizzle()
			{
				if (!m_Stop)
				{
					base.DoFizzle();
				}
			}

			public override void OnDisturb(DisturbType type, bool message)
			{
				if (message && !m_Stop)
				{
					Caster.SendLocalizedMessage(1049455); // You have been disrupted while attempting to summon your ethereal mount!
				}

				//m_Mount.UnmountMe();
			}

			public override void OnCast()
			{
				if (!m_Mount.Deleted && m_Mount.Rider == null && m_Mount.Validate(m_Rider))
				{
					m_Mount.Rider = m_Rider;
				}

				FinishSequence();
			}
		}
	}
}
