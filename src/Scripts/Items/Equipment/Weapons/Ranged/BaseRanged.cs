using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;

namespace Server.Items
{
	public abstract class BaseRanged : BaseMeleeWeapon
	{
		public abstract int EffectId { get; }
		public abstract Type AmmoType { get; }
		public abstract Item Ammo { get; }

		public override int DefHitSound => 0x234;
		public override int DefMissSound => 0x238;

		public override SkillName DefSkill => SkillName.Archery;
		public override WeaponType DefType => WeaponType.Ranged;
		public override WeaponAnimation DefAnimation => WeaponAnimation.ShootXBow;

		public override SkillName AccuracySkill => SkillName.Archery;

		private Timer m_RecoveryTimer; // so we don't start too many timers
		private bool m_Balanced;
		private int m_Velocity;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Balanced
		{
			get => m_Balanced;
			protected init { m_Balanced = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Velocity
		{
			get => m_Velocity;
			set { m_Velocity = value; InvalidateProperties(); }
		}

		public BaseRanged(int itemId) : base(itemId)
		{
		}

		public BaseRanged(Serial serial) : base(serial)
		{
		}

		public override TimeSpan OnSwing(Mobile attacker, IDamageable damageable)
		{
			_ = WeaponAbility.GetCurrentAbility(attacker);

			// Make sure we've been standing still for .25/.5/1 second depending on Era
			if (Core.TickCount - attacker.LastMoveTime >= (Core.SE ? 250 : Core.AOS ? 500 : 1000) || (Core.AOS && WeaponAbility.GetCurrentAbility(attacker) is MovingShot))
			{
				bool canSwing = true;

				if (Core.AOS)
				{
					canSwing = !attacker.Paralyzed && !attacker.Frozen;

					if (canSwing)
					{
						canSwing = attacker.Spell is not Spell { IsCasting: true, BlocksMovement: true };
					}
				}

				#region Dueling
				if (attacker is PlayerMobile pm)
				{
					if (pm.DuelContext != null && !pm.DuelContext.CheckItemEquip(attacker, this))
						canSwing = false;
				}
				#endregion

				if (canSwing && attacker.HarmfulCheck(damageable))
				{
					attacker.DisruptiveAction();
					_ = attacker.Send(new Swing(0, attacker, damageable));

					if (OnFired(attacker, damageable))
					{
						if (CheckHit(attacker, damageable))
							OnHit(attacker, damageable);
						else
							OnMiss(attacker, damageable);
					}
				}

				attacker.RevealingAction();

				return GetDelay(attacker);
			}

			attacker.RevealingAction();

			return TimeSpan.FromSeconds(0.25);
		}

		public override void OnHit(Mobile attacker, IDamageable damageable, double damageBonus)
		{
			if (AmmoType != null && attacker.Player && damageable is Mobile { Player: false } mobile && (mobile.Body.IsAnimal || mobile.Body.IsMonster) && 0.4 >= Utility.RandomDouble())
			{
				var ammo = Ammo;

				if (ammo != null)
				{
					mobile.AddToBackpack(ammo);
				}
			}

			base.OnHit(attacker, damageable, damageBonus);
		}

		public override void OnMiss(Mobile attacker, IDamageable damageable)
		{
			if (attacker.Player && 0.4 >= Utility.RandomDouble())
			{
				if (Core.SE)
				{
					if (attacker is PlayerMobile p && AmmoType != null)
					{
						Type ammo = AmmoType;

						if (p.RecoverableAmmo.ContainsKey(ammo))
						{
							p.RecoverableAmmo[ammo]++;
						}
						else
						{
							p.RecoverableAmmo.Add(ammo, 1);
						}

						if (!p.Warmode)
						{
							m_RecoveryTimer ??= Timer.DelayCall(TimeSpan.FromSeconds(10), p.RecoverAmmo);

							if (!m_RecoveryTimer.Running)
							{
								m_RecoveryTimer.Start();
							}
						}
					}
				}
				else
				{
					Point3D loc = damageable.Location;

					var ammo = Ammo;

					ammo?.MoveToWorld(
						new Point3D(loc.X + Utility.RandomMinMax(-1, 1), loc.Y + Utility.RandomMinMax(-1, 1), loc.Z),
						damageable.Map);
				}
			}

			base.OnMiss(attacker, damageable);
		}

		public virtual bool OnFired(Mobile attacker, IDamageable damageable)
		{
			WeaponAbility ability = WeaponAbility.GetCurrentAbility(attacker);

			// Respect special moves that use no ammo
			if (ability is { ConsumeAmmo: false })
			{
				return true;
			}

			if (attacker.Player)
			{
				BaseQuiver quiver = attacker.FindItemOnLayer(Layer.Cloak) as BaseQuiver;
				Container pack = attacker.Backpack;

				int lowerAmmo = AosAttributes.GetValue(attacker, AosAttribute.LowerAmmoCost);

				if (quiver == null || Utility.Random(100) >= lowerAmmo)
				{
					// consume ammo
					if (quiver != null && quiver.ConsumeTotal(AmmoType, 1))
					{
						quiver.InvalidateWeight();
					}
					else if (pack == null || !pack.ConsumeTotal(AmmoType, 1))
					{
						return false;
					}
				}
				else if (quiver.FindItemByType(AmmoType) == null && (pack == null || pack.FindItemByType(AmmoType) == null))
				{
					// lower ammo cost should not work when we have no ammo at all
					return false;
				}
			}

			attacker.MovingEffect(damageable, EffectId, 18, 1, false, false);

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Balanced);
			writer.Write(m_Velocity);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_Balanced = reader.ReadBool();
						m_Velocity = reader.ReadInt();
						break;
					}
			}
		}
	}
}
