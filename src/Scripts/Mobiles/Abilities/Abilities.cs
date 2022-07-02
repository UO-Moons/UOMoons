using Server.Engines.PartySystem;
using Server.Guilds;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells.Fourth;
using Server.Targeting;
using System;
using System.Collections.Generic;

namespace Server
{
	public partial class Ability
	{

		public static bool CanUse(Mobile from)
		{
			if (from == null)
				return false;

			return !(from.Frozen || from.Paralyzed || from.Map == null || from.Map == Map.Internal || !from.Alive);
		}

		public static bool CanUse(Mobile from, Mobile target)
		{
			return CanUse(from) && CanUse(from, target, true);
		}

		public static bool CanUse(Mobile from, Mobile target, bool harm)
		{
			if (!CanUse(from) || target == null)
				return false;
			else if (!from.CanSee(target))
				return false;
			else
				return CanTarget(from, target, harm);
		}

		#region Aura
		// Support for the old Aura permaiters
		public static void Aura(IDamageable m, Mobile from, int min, int max, int type, int range, int poisons, string text)
		{
			ResistanceType rt = ResistanceType.Physical;
			Poison p = null;

			switch (type)
			{
				case 1:
					rt = ResistanceType.Fire;
					break;
				case 2:
					rt = ResistanceType.Cold;
					break;
				case 3:
					rt = ResistanceType.Poison;
					break;
				case 4:
					rt = ResistanceType.Energy;
					break;
			}

			switch (poisons)
			{
				case 1:
					p = Poison.Lesser;
					break;
				case 2:
					p = Poison.Regular;
					break;
				case 3:
					p = Poison.Greater;
					break;
				case 4:
					p = Poison.Deadly;
					break;
				case 5:
					p = Poison.Lethal;
					break;
			}

			Aura(m, from.Location, from.Map, from, min, max, rt, range, p, text, true, false, false, 0, 0);
		}

		// Mobile based Aura
		public static void Aura(IDamageable m, Mobile from, int min, int max, ResistanceType type, int range, Poison poison, string text)
		{
			Aura(m, from.Location, from.Map, from, min, max, type, range, poison, text, true, false, false, 0, 0);
		}

		// Null based Aura
		public static void Aura(IDamageable m, Point3D location, Map map, Mobile from, int min, int max, ResistanceType type, int range, Poison poison, string text)
		{
			Aura(m, location, map, from, min, max, type, range, poison, text, true, false, false, 0, 0);
		}

		// No Effects
		public static void Aura(IDamageable m, Point3D location, Map map, Mobile from, int min, int max, ResistanceType type, int range, Poison poison, string text, bool scales, bool allownull)
		{
			Aura(m, location, map, from, min, max, type, range, poison, text, scales, allownull, false, 0, 0);
		}

		// Main Aura Method
		public static void Aura(IDamageable damageable, Point3D location, Map map, Mobile from, int min, int max, ResistanceType type, int range, Poison poison, string text, bool scales, bool allownull, bool effects, int itemid, int hue)
		{
			if (damageable == null && !allownull)
				return;

			List<Mobile> targets = new List<Mobile>();

			foreach (Mobile m in Map.AllMaps[map.MapID].GetMobilesInRange(location, range))
			{
				if (CanTarget(from, m, true, false, allownull))
					targets.Add(m);
			}

			if (effects && from != null)
				from.Animate(12, 5, 1, true, false, 0);

			for (int i = 0; i < targets.Count; i++)
			{
				Mobile m = (Mobile)targets[i];
				m.RevealingAction();

				if (text != "")
					m.SendMessage(text);

				int auradamage = Utility.RandomMinMax(min, max);

				if (scales)
					auradamage = (int)((auradamage / GetDist(location, m.Location)) * range);

				if (poison != null)
					m.ApplyPoison((from == null) ? m : from, poison);

				if (effects)
					m.FixedParticles(itemid, 10, 15, 5030/*what the hell does this number do?*/, hue, 0, EffectLayer.Waist);

				switch (type)
				{
					case ResistanceType.Physical:
						AOS.Damage(damageable, (from == null) ? m : from, auradamage, 100, 0, 0, 0, 0);
						break;
					case ResistanceType.Fire:
						AOS.Damage(damageable, (from == null) ? m : from, auradamage, 0, 100, 0, 0, 0);
						break;
					case ResistanceType.Cold:
						AOS.Damage(damageable, (from == null) ? m : from, auradamage, 0, 0, 100, 0, 0);
						break;
					case ResistanceType.Poison:
						AOS.Damage(damageable, (from == null) ? m : from, auradamage, 0, 0, 0, 100, 0);
						break;
					case ResistanceType.Energy:
						AOS.Damage(damageable, (from == null) ? m : from, auradamage, 0, 0, 0, 0, 100);
						break;
				}
			}

			targets.Clear();
		}

		#endregion

		#region UseBandage
		public static int UseBandage(BaseCreature from)
		{
			return UseBandage(from, false);
		}

		public static int UseBandage(BaseCreature from, bool healmaster)
		{
			if (from.IsDeadPet)
				return 12;

			int delay = (500 + (50 * ((120 - from.Dex) / 10))) / 100;

			if (delay < 3)
				delay = 3;

			if (from.Controlled && from.ControlMaster != null && from.Hits >= (from.Hits / 2) && healmaster)
			{
				if (from.InRange(from.ControlMaster, 2) && from.ControlMaster.Alive && from.ControlMaster.Hits < from.ControlMaster.HitsMax)
					BandageContext.BeginHeal(from, from.ControlMaster, false);
			}
			else if (from.Hits < from.HitsMax)
			{
				BandageContext.BeginHeal(from, from, false);
			}

			return delay + 3;
		}

		#endregion

		#region Bard Skills
		// Warning: Untested
		public static bool CheckBarding(BaseCreature from)
		{
			BaseInstrument inst = BaseInstrument.GetInstrument(from);

			if (inst == null)
			{
				if (from.Backpack == null)
					return false;

				inst = (BaseInstrument)from.Backpack.FindItemByType(typeof(BaseInstrument));

				if (inst == null)
				{
					inst = new Harp();
					inst.SuccessSound = 0x58B;
					// inst.DiscordSound = inst.PeaceSound = 0x58B;
					// inst.ProvocationSound = 0x58A;
					inst.FailureSound = 0x58C;
				}
			}

			BaseInstrument.SetInstrument(from, inst);

			if (from.Skills[SkillName.Discordance].Base == 0)
				from.Skills[SkillName.Discordance].Base = 100.0;

			if (from.Skills[SkillName.Peacemaking].Base == 0)
				from.Skills[SkillName.Peacemaking].Base = 100.0;

			if (from.Skills[SkillName.Provocation].Base == 0)
				from.Skills[SkillName.Provocation].Base = 100.0;

			return true;
		}

		public static void UseDiscord(BaseCreature from)
		{
			if (from.Combatant == null || !CanUse(from) || !CheckBarding(from))
				return;

			//int effect = 0.0;

			//if ( SkillHandlers.Discordance.GetEffect( from.Combatant, ref effect ) )
			//return;

			if (!from.UseSkill(SkillName.Discordance))
				return;

			if (from.Combatant is BaseCreature)
				if (from.Target != null)
				{
					from.Target.Invoke(from, from.Combatant);
				}
				else
				{
					double effect = -(from.Skills[SkillName.Discordance].Value / 5.0);
					TimeSpan duration = TimeSpan.FromSeconds(from.Skills[SkillName.Discordance].Value * 2);

					ResistanceMod[] mods =
					{
						new ResistanceMod(ResistanceType.Physical, (int)(effect * 0.01)),
						new ResistanceMod(ResistanceType.Fire, (int)(effect * 0.01)),
						new ResistanceMod(ResistanceType.Cold, (int)(effect * 0.01)),
						new ResistanceMod(ResistanceType.Poison, (int)(effect * 0.01)),
						new ResistanceMod(ResistanceType.Energy, (int)(effect * 0.01))
					};
					var m = (Mobile)from.Combatant;
					TimedResistanceMod.AddMod((Mobile)from.Combatant, "Discordance", mods, duration);
					m.AddStatMod(new StatMod(StatType.Str, "DiscordanceStr", (int)(m.RawStr * effect), duration));
					m.AddStatMod(new StatMod(StatType.Int, "DiscordanceInt", (int)(m.RawInt * effect), duration));
					m.AddStatMod(new StatMod(StatType.Dex, "DiscordanceDex", (int)(m.RawDex * effect), duration));
				}
		}

		public class DiscordEffectTimer : Timer
		{
			public Mobile Mob;
			public int Count;
			public int MaxCount;

			public DiscordEffectTimer(Mobile mob, TimeSpan duration)
				: base(TimeSpan.FromSeconds(1.25), TimeSpan.FromSeconds(1.25))
			{
				this.Mob = mob;
				this.Count = 0;
				this.MaxCount = (int)((double)duration.TotalSeconds / 1.25);
			}

			protected override void OnTick()
			{
				if (this.Count >= this.MaxCount)
					this.Stop();
				else
				{
					this.Mob.FixedEffect(0x376A, 1, 32);
					this.Count++;
				}
			}
		}

		public static void UsePeace(BaseCreature from)
		{
			if (from.Combatant == null || !CanUse(from) || !CheckBarding(from))
				return;

			if (!from.UseSkill(SkillName.Peacemaking))
				return;

			if (from.Combatant is PlayerMobile)
			{
				PlayerMobile pm = (PlayerMobile)from.Combatant;
				if (pm.PeacedUntil <= DateTime.UtcNow)
				{
					pm.PeacedUntil = DateTime.UtcNow + TimeSpan.FromSeconds((int)(from.Skills[SkillName.Peacemaking].Value / 5));
					pm.SendLocalizedMessage(500616); // You hear lovely music, and forget to continue battling!                                   
				}
			}
			else if (from.Target != null)
				from.Target.Invoke(from, from.Combatant);
		}

		public static void UseProvo(BaseCreature from, bool randomly)
		{
			if (from.Combatant == null && randomly || !CheckBarding(from))
				return;

			if (!CanUse(from))
				return;

			if (!from.UseSkill(SkillName.Provocation))
				return;

			Mobile targetone = FindRandomTarget(from, randomly);

			if (targetone == null)
				return;

			if (from.Target != null)
				from.Target.Invoke(from, targetone);

			Mobile targettwo = randomly ? FindRandomTarget(from, randomly) : (Mobile)from.Combatant;

			if (targettwo == null)
				return;

			if (from.Target != null)
				from.Target.Invoke(from, targettwo);
		}

		#endregion

		#region MimicThem
		public static void MimicThem(BaseCreature from)
		{
			var targ = (Mobile)from.Combatant;
			MimicThem(from, false, false);
		}

		public static void MimicThem(BaseCreature from, bool allowskillchanges, bool allowAIchanges)
		{
			var targ = (Mobile)from.Combatant;
			MimicThem(from, targ, allowskillchanges, allowAIchanges);
		}

		public static void MimicThem(BaseCreature from, Mobile targ, bool allowskillchanges, bool allowAIchanges)
		{
			if (targ == null)
				return;

			if (from.BodyMod == 0)
			{
				from.BodyMod = targ.Body;
				from.Hue = targ.Hue;

				from.NameMod = targ.Name;
				from.Title = targ.Title;

				from.HairItemId = targ.HairItemId;
				from.FacialHairItemId = targ.FacialHairItemId;

				from.VirtualArmor = targ.VirtualArmor;

				foreach (Item item in targ.Items)
				{
					if (item.Layer != Layer.Backpack && item.Layer != Layer.Mount)
					{
						/*
                        We don't dupe armor because the creatures base seed stacks with armor
                        By duping a high resistance player we shoot the creature up into the 100's in res
                        Imagine being the player facing your 400+ HP creature and EVERY attack & spell only deals 1 damage to them.
                        */
						if (item is BaseShield)
						{
							Buckler shieldtomake = new Buckler();
							shieldtomake.PoisonBonus = 0;
							shieldtomake.ItemId = item.ItemId;
							shieldtomake.Hue = item.Hue;
							shieldtomake.Layer = item.Layer;
							shieldtomake.Movable = false;
							shieldtomake.Name = item.Name;
							from.EquipItem(shieldtomake);
						}
						else if (item is BaseWeapon)
						{
							Broadsword weapontomake = new Broadsword();
							weapontomake.ItemId = item.ItemId;
							weapontomake.Hue = item.Hue;
							weapontomake.Layer = item.Layer;
							weapontomake.Movable = false;
							weapontomake.Name = item.Name;

							BaseWeapon weapon = item as BaseWeapon;
							weapontomake.Animation = weapon.Animation;
							weapontomake.HitSound = weapon.HitSound;
							weapontomake.MissSound = weapon.MissSound;
							weapontomake.MinDamage = weapon.MinDamage;
							weapontomake.MaxDamage = weapon.MaxDamage;
							weapontomake.Speed = weapon.Speed;
							from.EquipItem(weapontomake);
						}
						else
						{
							Item itemtomake = new Item(item.ItemId);
							itemtomake.Hue = item.Hue;
							itemtomake.Layer = item.Layer;
							itemtomake.Movable = false;
							itemtomake.Name = item.Name;
							from.EquipItem(itemtomake);
						}
					}
				}

				/*
                Duping skills can mess up the AI.
                What good is trying to melee when you have 0 tactics?
                On the other side, What good is stopping it's attack to try and cast something it can't do?
                The bool allows you to use it as a staff command or spell or make clone creatures that don't run around with the same exact skills as the others.
                */

				if (allowskillchanges)
					for (int i = 0; i < targ.Skills.Length && i < from.Skills.Length; i++)
						from.Skills[i].Base = targ.Skills[i].Base;
			}
			else
			{
				from.BodyMod = 0;
				from.Hue = 0;

				from.NameMod = null;
				from.Title = null;

				from.HairItemId = 0;
				from.FacialHairItemId = 0;

				from.VirtualArmor = 0;

				List<Item> list = new List<Item>(from.Items);

				foreach (Item item in list)
				{
					if (item != null)
						item.Delete();
				}

				if (allowskillchanges)
				{
					for (int i = 0; i < targ.Skills.Length; ++i)
						from.Skills[i].Base = 50.0;
				}
			}

			return;
		}

		#endregion

		#region DarkKnightAbilities
		/* Bull Rush
        He gathers energy and then slams into you from a
        distance, dealing heavy damage and physically knocking
        you back, stunning you.
        */

		public static void BullRush(Mobile from)
		{
			BullRush(from, "", 7);
		}

		public static void BullRush(Mobile from, string text, int duration)
		{
			var target = (Mobile)from.Combatant;

			if (target == null || CanUse(from, target))
				return;

			int dist = from.Str / 20;
			SlideAway(target, from.Location, (dist > 12) ? 12 : dist);

			if (text != "")
				target.SendMessage(text);
		}

		/* Echo Strike
        The Dark Knight teleports to one of the platforms in
        the room, and calls down lightning several times to
        strike you or your pets. The lightning is slightly
        displaced, allowing you a chance to escape, or even
        give him a taste of his own medicine.
        */

		public static void EchoStrike(Mobile from, int min, int max)
		{
			from.Paralyze(TimeSpan.FromSeconds(1));
			from.Animate(17, 5, 1, true, false, 0);

			List<Mobile> mobiles = new List<Mobile>();
			Point3D point;

			foreach (Mobile m in from.Map.GetMobilesInRange(from.Location, 14))
			{
				if (m != from && CanTarget(from, m, true, false, false))
					mobiles.Add(m);
			}

			for (int i = 0; i < mobiles.Count; i++)
			{
				Mobile m = mobiles[i];

				if (Utility.Random(5) == 0)
				{
					Effects.SendBoltEffect(m);
					AOS.Damage(m, from, Utility.RandomMinMax(min, max), 0, 0, 0, 0, 100);
					m.SendMessage("You get hit by a lightning bolt");
				}
				else
				{
					point = RandomCloseLocation(from, 1);

					if (from.Location == point)
					{
						AOS.Damage(from, from, Utility.RandomMinMax(min, max), 0, 0, 0, 0, 100);
						Effects.SendBoltEffect(from);
					}
					else
						Effects.SendBoltEffect(new Entity(Serial.Zero, point, from.Map));
				}
			}
		}

		/* Rally
        There is a chance the Dark Knight will attempt to
        heal himself of serious wounds. In this case, he
        teleports to the forefront of the room and begins a
        cycle of healing himself, and knocking back his foes.
        Strike him, or he could gain as much as 50% of his health back. 
        */

		public static void Rally(Mobile from)
		{
			Rally(from, 7);
		}

		public static void Rally(Mobile from, int delay)
		{
			from.Paralyze(TimeSpan.FromSeconds(4.0));
			from.Animate(6, 5, 1, true, false, 0);

			Timer timer = new RallyTimer(from, delay);
			timer.Start();
		}

		private class RallyTimer : Timer
		{
			private Mobile m_User;
			private int m_Count;
			private int m_MaxCount;

			public RallyTimer(Mobile user, int delay)
				: base(TimeSpan.FromMilliseconds(100.0), TimeSpan.FromMilliseconds(100.0))
			{
				this.m_User = user;
				this.m_Count = 0;
				this.m_MaxCount = delay;
			}

			protected override void OnTick()
			{
				if (this.m_Count >= (this.m_MaxCount + 1) || this.m_User == null || !this.m_User.Paralyzed)
					this.Stop();

				if (this.m_Count == this.m_MaxCount)
				{
					this.m_User.Heal((this.m_User.HitsMax / 2));
					this.m_User.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
					this.m_User.PlaySound(0x202);
				}

				this.m_Count++;
			}
		}
		#endregion

		#region MiscAbilities
		public static void EtherealDrain(Mobile from, Mobile to, int type)
		{
			if (from == null || to == null)
				return;

			if (type == 1)
			{
				from.Say(1042156); //Your power is mine to use as I wish

				int amount = Utility.RandomMinMax(40, 80);
				to.Damage(amount, from);
				from.Hits += (amount / 2); //Halved to account for 50% resistance the target may have.
			}
			else if (type == 2)
			{
				from.Say(1042156); //Your power is mine to use as I wish

				int amount = (to.Mana * (100 - Utility.RandomMinMax(50, 90))) / 100;
				to.Mana -= amount;
				from.Mana += amount;
			}
			else
			{
				from.Say(1042157); //You shalt go nowhere unless I deem it be so

				int amount = (to.Stam * (100 - Utility.RandomMinMax(50, 100))) / 100;
				to.Stam -= amount;
				from.Stam += amount;
			}
		}

		public static void LowerStat(Mobile target, int minloss, int maxloss, int mintime, int maxtime, int type)
		{
			if (target.GetStatMod("LowerStats") != null)
				return;

			StatType stattype = StatType.Str;
			int offset = Utility.Random(minloss, maxloss);

			if (type <= 0 || type >= 4)
				type = Utility.RandomMinMax(1, 3);

			switch (type)
			{
				case 1:
					stattype = StatType.Str;
					break;
				case 2:
					stattype = StatType.Dex;
					break;
				case 3:
					stattype = StatType.Int;
					break;
			}

			target.AddStatMod(new StatMod(stattype, "LowerStats", -offset, TimeSpan.FromSeconds(Utility.Random(mintime, maxtime))));
		}

		public static void DamageArmor(Mobile target, int min, int max)
		{
			DamageArmor(target, min, max, 0);
		}

		public static void DamageArmor(Mobile target, int min, int max, int place)
		{
			double positionchance = Utility.RandomDouble();
			int ruin = Utility.RandomMinMax(min, max);

			if (place == 7 && target.Weapon is BaseWeapon targetwep)
			{
				//CS0266: Line 579: Cannot implicitly convert type 'Server.IWeapon' to 'Server
				//Items.BaseWeapon'. An explicit conversion exists (are you missing a cast?)
				BaseWeapon weapon = targetwep;
				if ( weapon is not Fists && weapon is not BaseRanged && weapon != null )
						weapon.HitPoints -= ruin;
			}
			else
			{
				BaseArmor armor = null;

				if (positionchance < 0.7 || place == 1)
					armor = target.NeckArmor as BaseArmor;
				else if (positionchance < 0.14 || place == 2)
					armor = target.HandArmor as BaseArmor;
				else if (positionchance < 0.28 || place == 3)
					armor = target.ArmsArmor as BaseArmor;
				else if (positionchance < 0.43 || place == 4)
					armor = target.HeadArmor as BaseArmor;
				else if (positionchance < 0.65 || place == 5)
					armor = target.LegsArmor as BaseArmor;
				else
					armor = target.ChestArmor as BaseArmor;

				if (armor != null)
					armor.HitPoints -= ruin;
			}
		}

		public static void TossBola(Mobile from)
		{
			if (from == null)
				return;

			var target = (Mobile)from.Combatant;

			if (target == null)
				return;
			else if (!target.Mounted)
				return;

			from.NonlocalOverheadMessage(MessageType.Emote, 0x3B2, 1049633, from.Name); // ~1_NAME~ begins to menacingly swing a bola...
			from.Direction = from.GetDirectionTo(target);
			from.Animate(11, 5, 1, true, false, 0);
			from.MovingEffect(target, 0x26AC, 10, 0, false, false);

			IMount mt = target.Mount;

			if (mt != null)
			{
				mt.Rider = null;
				target.SendLocalizedMessage(1040023); // You have been knocked off of your mount!
				BaseMount.SetMountPrevention(target, BlockMountType.Dazed, TimeSpan.FromSeconds(3.0));
			}
		}

		public static void TurnPet(Mobile target)
		{
			if (target is BaseCreature)
			{
				BaseCreature c = (BaseCreature)target;

				if (c.Controlled && c.ControlMaster != null)
				{
					c.ControlTarget = c.ControlMaster;
					c.ControlOrder = OrderType.Attack;
					c.Combatant = c.ControlMaster;
				}
			}
		}

		private static int EnergyDrainCount = 0;

		public static void EnergyDrain(Mobile from, Mobile target)
		{
			EnergyDrain(from, target, 1, 5, true);
		}

		public static void EnergyDrain(Mobile from, Mobile target, int amount, int duration, bool skills)
		{
			if (amount < 0)
				amount = 1;

			target.AddStatMod(new StatMod(StatType.Str, "Energy Drain Str: " + EnergyDrainCount.ToString(), -amount, TimeSpan.FromMinutes(5)));
			target.AddStatMod(new StatMod(StatType.Dex, "Energy Drain Dex: " + EnergyDrainCount.ToString(), -amount, TimeSpan.FromMinutes(5)));
			target.AddStatMod(new StatMod(StatType.Int, "Energy Drain Int: " + EnergyDrainCount.ToString(), -amount, TimeSpan.FromMinutes(5)));

			if (skills)
				for (int i = 0; i < target.Skills.Length; ++i)
					target.AddSkillMod(new TimedSkillMod((SkillName)i, true, (double)-amount, TimeSpan.FromMinutes(duration)));

			if (from != null)
				from.Hits += 5 * amount;

			EnergyDrainCount++;

			if (EnergyDrainCount > 65535)
				EnergyDrainCount = 0;
		}

		#endregion

		#region ToolHandOuts
		public static bool GiveItem(Mobile to, Item item)
		{
			return GiveItem(to, 0, item, false);
		}

		public static bool GiveItem(Mobile to, int hue, Item item)
		{
			return GiveItem(to, hue, item, false);
		}

		public static bool GiveItem(Mobile to, int hue, Item item, bool mustequip)
		{
			if (to == null && item == null)
				return false;

			if (hue != 0)
				item.Hue = hue;

			item.Movable = false;

			if (to.EquipItem(item))
				return true;

			Container pack = to.Backpack;

			if (pack != null && !mustequip)
			{
				pack.DropItem(item);
				return true;
			}
			else
				item.Delete();

			return false;
		}

		#endregion

		#region ToolTargeting
		public static Mobile FindRandomTarget(Mobile from)
		{
			return FindRandomTarget(from, true);
		}

		public static Mobile FindRandomTarget(Mobile from, bool allowcombatant)
		{
			List<Mobile> list = new List<Mobile>();

			foreach (Mobile m in from.GetMobilesInRange(12))
			{
				if (m != null && m != from)
					if (CanTarget(from, m) && from.InLOS(m))
					{
						if (allowcombatant && m == from.Combatant)
							continue;
						else
							list.Add(m);
					}
			}

			if (list.Count == 0)
				return null;
			if (list.Count == 1)
				return list[0];

			return list[Utility.Random(list.Count)];
		}

		public static bool CanTarget(Mobile from, Mobile to)
		{
			return CanTarget(from, to, true, false, false);
		}

		public static bool CanTarget(Mobile from, Mobile to, bool harm)
		{
			return CanTarget(from, to, harm, false, false);
		}

		public static bool CanTarget(Mobile from, Mobile to, bool harm, bool checkguildparty, bool allownull)
		{
			if (to == null)
				return false;
			else if (from == null)
				return allownull;
			else if (from == to && !harm)
				return true;
			else if ((harm && to.Blessed) || (to.AccessLevel != AccessLevel.Player && to.Hidden))
				return false;
			else if (harm)
			{
				if (!to.Alive)
					return false;
				else if (to is BaseCreature)
				{
					if (((BaseCreature)to).IsDeadPet)
						return false;
				}
			}

			if (checkguildparty)
			{
				//Guilds
				Guild fromguild = GetGuild(from);
				Guild toguild = GetGuild(to);

				if (fromguild != null && toguild != null)
					if (fromguild == toguild || fromguild.IsAlly(toguild))
						return !harm;

				//Parties
				Party p = GetParty(from);

				if (p != null && p.Contains(to))
					return !harm;
			}

			//Default
			if (harm)
				return (IsGoodGuy(from) && !(IsGoodGuy(to))) | (!(IsGoodGuy(from)) && IsGoodGuy(to));
			else
				return (IsGoodGuy(from) && IsGoodGuy(to)) | (!(IsGoodGuy(from)) && !(IsGoodGuy(to)));
		}

		public static bool IsGoodGuy(Mobile m)
		{
			if (m.Criminal)
				return false;

			if (m.Player && m.Kills < 5)
				return true;

			if (m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)m;

				if (bc.Controlled || bc.Summoned)
				{
					if (bc.ControlMaster != null)
						return IsGoodGuy(bc.ControlMaster);
					else if (bc.SummonMaster != null)
						return IsGoodGuy(bc.SummonMaster);
				}
			}

			return false;
		}

		public static Guild GetGuild(Mobile m)
		{
			Guild guild = m.Guild as Guild;

			if (guild == null && m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)m;
				m = bc.ControlMaster;

				if (m != null)
					guild = m.Guild as Guild;

				m = bc.SummonMaster;

				if (m != null && guild == null)
					guild = m.Guild as Guild;
			}

			return guild;
		}

		public static Party GetParty(Mobile m)
		{
			Party party = Party.Get(m);

			if (party == null && m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)m;
				m = bc.ControlMaster;

				if (m != null)
					party = Party.Get(m);

				m = bc.SummonMaster;

				if (m != null && party == null)
					party = Party.Get(m);
			}

			return party;
		}

		#endregion

		#region ToolPlaces
		public static double GetDist(Point3D start, Point3D end)
		{
			int xdiff = start.X - end.X;
			int ydiff = start.Y - end.Y;
			return Math.Sqrt((xdiff * xdiff) + (ydiff * ydiff));
		}

		public static void IncreaseByDirection(ref Point3D point, Direction d)
		{
			switch (d)
			{
				case (Direction)0x0:
				case (Direction)0x80:
					point.Y--;
					break; //North
				case (Direction)0x1:
				case (Direction)0x81:
					{
						point.X++;
						point.Y--;
						break;
					}//Right
				case (Direction)0x2:
				case (Direction)0x82:
					point.X++;
					break; //East
				case (Direction)0x3:
				case (Direction)0x83:
					{
						point.X++;
						point.Y++;
						break;
					}//Down
				case (Direction)0x4:
				case (Direction)0x84:
					point.Y++;
					break; //South
				case (Direction)0x5:
				case (Direction)0x85:
					{
						point.X--;
						point.Y++;
						break;
					}//Left
				case (Direction)0x6:
				case (Direction)0x86:
					point.X--;
					break; //West
				case (Direction)0x7:
				case (Direction)0x87:
					{
						point.X--;
						point.Y--;
						break;
					}//Up
				default:
					{
						break;
					}
			}
		}

		public static bool TooManyCreatures(Type type, int maxcount, Mobile from)
		{
			if (from == null)
				return false;

			int count = 0;

			foreach (Mobile m in from.GetMobilesInRange(10))
			{
				if (m != null)
					if (m.GetType() == type)
						count++;
			}

			return count >= maxcount;
		}

		public static bool TooManyCreatures(Type[] types, int maxcount, Mobile from)
		{
			if (from == null)
				return false;

			int count = 0;

			foreach (Mobile m in from.GetMobilesInRange(10))
			{
				for (int i = 0; i < types.Length; i++)
					if (m != null)
						if (m.GetType() == types[i])
							count++;
			}

			return count >= maxcount;
		}

		public static Point3D RandomCloseLocation(Mobile target)
		{
			return RandomCloseLocation(target, 1);
		}

		public static Point3D RandomCloseLocation(Mobile target, int range)
		{
			Point3D point = target.Location;
			bool canfit = false;

			for (int i = 0; !canfit && i < 10; i++)
			{
				point = target.Location;
				point.X += Utility.RandomMinMax(-range, range);
				point.Y += Utility.RandomMinMax(-range, range);
				point.Z = target.Map.GetAverageZ(point.X, point.Y);

				canfit = target.Map.CanFit(point.X, point.Y, point.Z, 16, false, false);
			}

			return (canfit) ? point : target.Location;
		}

		public static void SlideAway(Mobile target, Point3D point, int dist)
		{
			new SlideTimer(target, point, dist, true).Start();
		}

		public static void SlideTo(Mobile target, Point3D point, int dist)
		{
			new SlideTimer(target, point, dist, false).Start();
		}

		private class SlideTimer : Timer
		{
			private Mobile m_Mob;
			private Point3D m_Point;
			private int m_Dist;
			private bool m_Push;
			private int m_Count;

			public SlideTimer(Mobile mob, Point3D point, int dist, bool push)
				: base(TimeSpan.FromMilliseconds(100.0), TimeSpan.FromMilliseconds(100.0))
			{
				this.m_Mob = mob;
				this.m_Point = point;
				this.m_Dist = dist;
				this.m_Push = push;
				this.m_Count = 0;

				this.m_Mob.CantWalk = true;
			}

			protected override void OnTick()
			{
				if (this.m_Mob == null)
				{
					this.Stop();
					return;
				}
				else if (this.m_Count >= this.m_Dist)
				{
					this.m_Mob.CantWalk = false;
					this.Stop();
					return;
				}

				Direction d = this.m_Mob.GetDirectionTo(this.m_Point);
				Point3D moveto = new Point3D(this.m_Mob.X, this.m_Mob.Y, this.m_Mob.Z);

				if (this.m_Push)
				{
					switch (d)
					{
						case (Direction)0x0:
						case (Direction)0x80:
							d = (Direction)0x4;
							break; // North to South
						case (Direction)0x1:
						case (Direction)0x81:
							d = (Direction)0x5;
							break; // Right to Left
						case (Direction)0x2:
						case (Direction)0x82:
							d = (Direction)0x6;
							break; // East to West
						case (Direction)0x3:
						case (Direction)0x83:
							d = (Direction)0x7;
							break; // Down to Up
						case (Direction)0x4:
						case (Direction)0x84:
							d = (Direction)0x0;
							break; // South to North
						case (Direction)0x5:
						case (Direction)0x85:
							d = (Direction)0x1;
							break; // Left to Right
						case (Direction)0x6:
						case (Direction)0x86:
							d = (Direction)0x2;
							break; // West to East
						case (Direction)0x7:
						case (Direction)0x87:
							d = (Direction)0x3;
							break; // Up to Down
						default:
							{
								break;
							}
					}
				}

				IncreaseByDirection(ref moveto, d);
				this.m_Mob.Direction = d;

				if (this.m_Mob.Map.CanFit(moveto.X, moveto.Y, this.m_Mob.Map.GetAverageZ(moveto.X, moveto.Y), 16, false, false))
					this.m_Mob.Location = moveto;

				this.m_Count++;
			}
		}

		#endregion

		#region ToolWeapons
		public static void Strike(Mobile from)
		{
			Strike(from, 1);
		}

		public static void Strike(Mobile from, int count)
		{
			if (from.Frozen || from.Paralyzed)
				return;

			var target = (Mobile)from.Combatant;

			if (target == null)
				return;

			if (from.InRange(target.Location, 1))
				if (from.Weapon != null)
					if (from.Weapon is BaseWeapon)
					{
						BaseWeapon weapon = (BaseWeapon)from.Weapon;

						for (int i = 0; i < count + 1; i++)
							if (target != null)
								weapon.OnHit(from, target, 1.0);
					}
		}

		#endregion

		#region SimpleFlame
		public static void SimpleFlame(Mobile from, Mobile target)
		{
			if (!CanUse(from, target))
				return;

			from.Say("*Ul Flam*");

			Effects.SendLocationParticles(
				EffectItem.Create(new Point3D(from.X - 1, from.Y - 1, from.Z), from.Map, EffectItem.DefaultDuration),
				0x3709, 10, 30, 0, 4, 0, 0);
			Effects.SendLocationParticles(
				EffectItem.Create(new Point3D(from.X - 1, from.Y + 1, from.Z), from.Map, EffectItem.DefaultDuration),
				0x3709, 10, 30, 0, 4, 0, 0);
			Effects.SendLocationParticles(
				EffectItem.Create(new Point3D(from.X + 1, from.Y - 1, from.Z), from.Map, EffectItem.DefaultDuration),
				0x3709, 10, 30, 0, 4, 0, 0);
			Effects.SendLocationParticles(
				EffectItem.Create(new Point3D(from.X + 1, from.Y + 1, from.Z), from.Map, EffectItem.DefaultDuration),
				0x3709, 10, 30, 0, 4, 0, 0);

			new SimpleFlameTimer(from, target).Start();
		}
		#endregion

		#region SimpleFlame
		public static void SimpleFlame(Mobile from, Mobile target, int damage)
		{
			SimpleFlame(from, target, damage, false);
		}

		public static void SimpleFlame(Mobile from, Mobile target, int damage, bool skip)
		{
			if (!CanUse(from, target))
				return;

			if (!skip)
			{
				from.Say("*Ul Flam*");

				Effects.SendLocationParticles(
					EffectItem.Create(new Point3D(from.X - 1, from.Y - 1, from.Z), from.Map, EffectItem.DefaultDuration),
					0x3709, 10, 30, 0, 4, 0, 0);
				Effects.SendLocationParticles(
					EffectItem.Create(new Point3D(from.X - 1, from.Y + 1, from.Z), from.Map, EffectItem.DefaultDuration),
					0x3709, 10, 30, 0, 4, 0, 0);
				Effects.SendLocationParticles(
					EffectItem.Create(new Point3D(from.X + 1, from.Y - 1, from.Z), from.Map, EffectItem.DefaultDuration),
					0x3709, 10, 30, 0, 4, 0, 0);
				Effects.SendLocationParticles(
					EffectItem.Create(new Point3D(from.X + 1, from.Y + 1, from.Z), from.Map, EffectItem.DefaultDuration),
					0x3709, 10, 30, 0, 4, 0, 0);
			}

			new SimpleFlameTimer(from, target).Start();
		}

		public class SimpleFlameTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly Mobile m_Target;
			//private int m_Damage;
			private int m_Count;

			private Point3D Point;

			public SimpleFlameTimer(Mobile from, Mobile target)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_From = from;
				m_Target = target;
				m_Count = 0;
				Point = m_From.Location;
			}

			protected override void OnTick()
			{
				if (m_From == null || m_From.Deleted)
				{
					Stop();
					return;
				}

				if (m_Count == 0)
				{
					for (int i = -2; i < 3; i++)
						for (int j = -2; j < 5; j++)
							if ((i == -2 || i == 2) || (j == -2 || j == 2))
								Effects.SendMovingParticles(
									new Entity(Serial.Zero, new Point3D(m_From.X + i, m_From.Y + j, m_From.Z + 14), m_From.Map),
									m_Target, 0x46E9, 2, 0, false, false, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
							else
								continue;
				}
				else
				{ // It looked like it delt 67 damage, presuming 70% fire res thats about 223 damage delt before resistance.                                          
					AOS.Damage(m_Target, m_From, Utility.RandomMinMax(210, 230), 0, 100, 0, 0, 0);

					Stop();
				}

				m_Count++;

				Effects.PlaySound(Point, m_From.Map, 0x160);
			}
		}
		#endregion

		#region SoulDrain
		// since the video shows living people nearby only the players present at the time of casting are effected.
		public static void SoulDrain(Mobile from)
		{
			from.Say("*Vas Grav Hur*");

			List<PlayerMobile> list = new();

			foreach (Mobile m in from.GetMobilesInRange(8))
				if (m != null)
					if (m is PlayerMobile mobile)
						list.Add(mobile);

			new SoulDrainTimer(from, list).Start();
		}

		public class SoulDrainTimer : Timer
		{
			private readonly Mobile m_From;
			//private int m_Damage;
			private readonly List<PlayerMobile> m_List;
			private int m_Count;

			public SoulDrainTimer(Mobile from, List<PlayerMobile> list)
				: base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
			{
				m_From = from;
				//m_Damage = damage;
				m_List = list;
				m_Count = 0;
			}

			protected override void OnTick()
			{
				if (m_From == null || m_From.Deleted)
				{
					Stop();
					return;
				}

				if (m_Count == 0)
					for (int i = 0; i < m_List.Count; i++)
					{
						m_List[i].Frozen = true;
						m_List[i].Kill();
					}
				else if (m_Count < 10)
				{
					for (int i = 0; i < m_List.Count; i++)
					{
						if (m_Count == 1)
							m_List[i].SendMessage("Unnatural forces hold you free from the ground and swirl around you!"); //TODO find cliloc.

						// Prevent them from resing during this trick.
						if (m_List[i].Alive)
							m_List[i].Kill();

						m_List[i].Z++;
						int effects = Utility.RandomMinMax(3, 5) + 1;
						for (int j = 0; j < effects; j++)
						{
							int x = Utility.RandomMinMax(-1, 2);
							int y = Utility.RandomMinMax(-1, 2);

							//TODO Match the look
							Effects.SendMovingParticles(
								new Entity(Serial.Zero, new Point3D(m_List[i].X + x, m_List[i].Y + y, m_List[i].Z - 5), m_List[i].Map),
								new Entity(Serial.Zero, new Point3D(m_List[i].X + x, m_List[i].Y + y, m_List[i].Z + 60), m_List[i].Map),
								0x378A + Utility.Random(19)/*ItemID*/, 10, 0, false, false, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
						}
					}
				}
				else
				{
					for (int i = 0; i < m_List.Count; i++)
					{
						m_List[i].Z -= 0;
						m_List[i].Frozen = false;
					}
					Stop();
				}

				m_Count++;
			}
		}
		#endregion

		#region FlameWave
		public static void FlameWave(Mobile from)
		{
			if (!CanUse(from))
				return;

			from.Say("*Vas Grav Consume !*");

			new FlameWaveTimer(from).Start();
		}

		internal class FlameWaveTimer : Timer
		{
			private readonly Mobile m_From;
			private Point3D m_StartingLocation;
			private readonly Map m_Map;
			private int m_Count;
			private Point3D m_Point;

			public FlameWaveTimer(Mobile from)
				: base(TimeSpan.FromMilliseconds(300.0), TimeSpan.FromMilliseconds(300.0))
			{
				m_From = from;
				m_StartingLocation = from.Location;
				m_Map = from.Map;
				m_Count = 0;
				m_Point = new Point3D();
				SetupDamage(from);
			}

			protected override void OnTick()
			{
				if (m_From == null || m_From.Deleted)
				{
					Stop();
					return;
				}

				for (int i = -m_Count; i < m_Count + 1; i++)
				{
					for (int j = -m_Count; j < m_Count + 1; j++)
					{
						m_Point.X = m_StartingLocation.X + i;
						m_Point.Y = m_StartingLocation.Y + j;
						m_Point.Z = m_Map.GetAverageZ(m_Point.X, m_Point.Y);
						double dist = GetDist(m_StartingLocation, m_Point);
						if (dist < (m_Count + 0.1) && dist > (m_Count - 3.1))
						{
							Effects.SendLocationParticles(EffectItem.Create(m_Point, m_Map, EffectItem.DefaultDuration), 0x3709, 10, 30, 5052);
						}
					}
				}

				m_Count += 3;

				if (m_Count > 15)
					Stop();
			}

			private void SetupDamage(Mobile from)
			{
				foreach (Mobile m in from.GetMobilesInRange(10))
				{
					if (CanTarget(from, m, true, false, false))
					{
						Timer.DelayCall(TimeSpan.FromMilliseconds(300 * (GetDist(m_StartingLocation, m.Location) / 3)), new TimerStateCallback(Hurt), m);
					}
				}
			}

			public void Hurt(object o)
			{
				if (m_From == null || o is not Mobile m || m.Deleted)
					return;

				int damage = m_From.Hits / 4;

				if (damage > 200)
					damage = 400;

				AOS.Damage(m, m_From, damage, 0, 100, 0, 0, 0);
				m.SendMessage("You are being burnt alive by the seering heat!");
			}

			private static double GetDist(Point3D start, Point3D end)
			{
				int xdiff = start.X - end.X;
				int ydiff = start.Y - end.Y;
				return Math.Sqrt((xdiff * xdiff) + (ydiff * ydiff));
			}
		}
		#endregion

		#region FlameCross
		public static void FlameCross(Mobile from)
		{
			if (!CanUse(from))
				return;

			Point3D point = from.Location;
			Direction d = Direction.North;
			int itemid = 0x3996;
			Map map = from.Map;

			for (int i = 0; i < 8; i++)
			{
				switch (i)
				{
					case 1:
						{
							d = Direction.Right;
							itemid = 0;
						}
						break;
					case 2:
						{
							d = Direction.East;
							itemid = 0x398C;
						}
						break;
					case 3:
						{
							d = Direction.Down;
							itemid = 0;
						}
						break;
					case 4:
						{
							d = Direction.South;
							itemid = 0x3996;
						}
						break;
					case 5:
						{
							d = Direction.Left;
							itemid = 0;
						}
						break;
					case 6:
						{
							d = Direction.West;
							itemid = 0x398C;
						}
						break;
					case 7:
						{
							d = Direction.Up;
							itemid = 0;
						}
						break;
				}

				for (int j = 0; j < 16; j++)
				{
					Ability.IncreaseByDirection(ref point, d);

					if (from.CanSee(point))
					{
						// Damage was 2 on the nightmare which has 30~40% fire res. 4 - 35% = 2.6, close enough for me.
						if (itemid != 0)
							new FireFieldSpell.FireFieldItem(itemid, point, from, from.Map, TimeSpan.FromSeconds(30));
						else
						{
							new OtherFireFieldItem(0x3996, point, from, from.Map, TimeSpan.FromSeconds(30));
							new OtherFireFieldItem(0x398C, point, from, from.Map, TimeSpan.FromSeconds(30));
						}
					}
				}

				point = from.Location;
			}

			Effects.PlaySound(point, map, 0x44B);
		}

		/*public static void IncreaseByDirection(ref Point3D point, Direction d)
        {
        switch (d)
        {
        case (Direction)0x0:
        case (Direction)0x80: point.Y--; break; //North
        case (Direction)0x1:
        case (Direction)0x81: { point.X++; point.Y--; break; } //Right
        case (Direction)0x2:
        case (Direction)0x82: point.X++; break; //East
        case (Direction)0x3:
        case (Direction)0x83: { point.X++; point.Y++; break; } //Down
        case (Direction)0x4:
        case (Direction)0x84: point.Y++; break; //South
        case (Direction)0x5:
        case (Direction)0x85: { point.X--; point.Y++; break; } //Left
        case (Direction)0x6:
        case (Direction)0x86: point.X--; break; //West
        case (Direction)0x7:
        case (Direction)0x87: { point.X--; point.Y--; break; } //Up
        default: { break; }
        }
        }*/
		public class OtherFireFieldItem : FireFieldSpell.FireFieldItem
		{
			public override bool BlocksFit
			{
				get
				{
					return false;
				}
			}

			public OtherFireFieldItem(int itemID, Point3D loc, Mobile caster, Map map, TimeSpan duration)
				: base(itemID, loc, caster, map, duration)
			{
			}

			public OtherFireFieldItem(Serial serial)
				: base(serial)
			{
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
		#endregion

		#region CrimsonMeteor
		public static void CrimsonMeteor(Mobile from, int damage)
		{
			if (!CanUse(from))
				return;

			from.Say("*Shooting Meteor !!*");

			new CrimsonMeteorTimer(from, damage).Start();
		}

		public class CrimsonMeteorTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly int m_Damage;
			private int m_Count;
			private readonly int m_MaxCount;
			private Point3D m_LastTarget;
			private Point3D m_ShowerLocation;

			public CrimsonMeteorTimer(Mobile from, int damage)
				: base(TimeSpan.FromMilliseconds(300.0), TimeSpan.FromMilliseconds(300.0))
			{
				m_From = from;
				m_Damage = damage;
				m_Count = 0;
				m_MaxCount = 30;
				m_LastTarget = new Point3D(0, 0, 0);
				m_ShowerLocation = new Point3D(from.Location);
			}

			protected override void OnTick()
			{
				if (m_From == null || m_From.Deleted)
				{
					Stop();
					return;
				}

				new FireField(m_From, 50, m_Damage, m_Damage, Utility.RandomBool(), m_LastTarget, m_From.Map);

				Point3D point = new Point3D();
				int tries = 0;

				while (tries < 5)
				{
					point.X = m_ShowerLocation.X += Utility.RandomMinMax(-5, 5);
					point.Y = m_ShowerLocation.Y += Utility.RandomMinMax(-5, 5);
					point.Z = m_From.Map.GetAverageZ(point.X, point.Y);

					if (m_From.CanSee(point))
						break;

					tries++;
				}

				Effects.SendMovingParticles(
					new Entity(Serial.Zero, new Point3D(point.X, point.Y, point.Z + 30), m_From.Map),
					new Entity(Serial.Zero, point, m_From.Map),
					0x36D4, 5, 0, false, false, 0, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

				Effects.PlaySound(point, m_From.Map, 0x11D);

				m_LastTarget = point;
				m_Count++;

				if (m_Count >= m_MaxCount)
				{
					Stop();
					return;
				}
			}
		}
		#endregion

		#region JaggedFire
		public static void JaggedLineEffect(Mobile from, int range, int speed)
		{
			if (CanUse(from))
				new JaggedLineTimer(from, range, speed).Start();
		}

		public static Direction JaggedLine(Direction d)
		{
			int number = (int)d + Utility.RandomMinMax(-1, 1);

			if (number < 0)
				number = 8;

			number %= 8;

			return (Direction)number;
		}

		public class JaggedLineTimer : Timer
		{
			private readonly Mobile m_From;
			private readonly Direction m_D;
			private Point3D m_Point;
			private readonly Map m_Map;
			private int m_Count;
			private readonly int m_MaxCount;

			public JaggedLineTimer(Mobile from, int range, int speed)
				: base(TimeSpan.FromMilliseconds(speed), TimeSpan.FromMilliseconds(speed))
			{
				m_From = from;
				m_D = from.Direction;
				m_Point = new Point3D(from.Location);
				m_Map = from.Map;
				m_Count = 0;
				m_MaxCount = range;
			}

			protected override void OnTick()
			{
				if (m_From == null || m_From.Deleted)
				{
					Stop();
					return;
				}

				m_Count++;

				if (m_Count == 0)
					IncreaseByDirection(ref m_Point, m_D);
				else
					IncreaseByDirection(ref m_Point, JaggedLine(m_D));

				Point3D p = new(m_Point.X, m_Point.Y, m_Map.GetAverageZ(m_Point.X, m_Point.Y));

				if (m_Map.CanFit(p, 16, false, false))
				{
					bool canplace = true;

					foreach (Item item in m_Map.GetItemsInRange(p, 0))
					{
						if (item != null)
						{
							if (item is FireField && item.Visible == false)
							{
								canplace = false;
								break;
							}
						}
					}

					if (canplace)
					{
						new FireField(m_From, 30, 25, 35, false, new Point3D(p.X, p.Y, p.Z), m_Map).Visible = false;
						new FireField(m_From, 30, 0, 0, true, new Point3D(p.X, p.Y + 1, p.Z), m_Map);
						new FireField(m_From, 30, 0, 0, false, new Point3D(p.X + 1, p.Y, p.Z), m_Map);
					}
				}
				else
					m_Count = 999;

				if (m_Count > m_MaxCount)
				{
					Stop();
				}
			}
		}
		#endregion

		#region FireField
		public class FireField : Item
		{
			private Mobile m_Owner;
			private readonly int m_MinDamage;
			private readonly int m_MaxDamage;
			private readonly DateTime m_Destroy;
			private Point3D m_MoveToPoint;
			private readonly Map m_MoveToMap;
			private readonly Timer m_Timer;
			private List<Mobile> m_List;

			[Constructable]
			public FireField(int duration, int min, int max, bool south, Point3D point, Map map)
				: this(null, duration, min, max, south, point, map)
			{
			}

			[Constructable]
			public FireField(Mobile owner, int duration, int min, int max, bool south, Point3D point, Map map)
				: base(GetItemID(south))
			{
				Movable = false;

				m_Owner = owner;
				m_MinDamage = min;
				m_MaxDamage = max;
				m_Destroy = DateTime.UtcNow + TimeSpan.FromSeconds((double)duration + 1.5);
				m_MoveToPoint = point;
				m_MoveToMap = map;
				m_List = new List<Mobile>();
				m_Timer = Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1), new TimerCallback(OnTick));
				Timer.DelayCall(TimeSpan.Zero, TimeSpan.FromSeconds(1.5), new TimerCallback(Move));
			}

			private static int GetItemID(bool south)
			{
				if (south)
					return 0x398C;
				else
					return 0x3996;
			}

			public override void OnAfterDelete()
			{
				if (m_Timer != null)
					m_Timer.Stop();
			}

			private void Move()
			{
				if (!Visible)
					ItemId = 0x36FE;

				MoveToWorld(m_MoveToPoint, m_MoveToMap);
			}

			private void OnTick()
			{
				if (DateTime.UtcNow > m_Destroy)
					Delete();
				else if (m_MinDamage != 0)
				{
					foreach (Mobile m in GetMobilesInRange(0))
					{
						if (m == null)
							continue;
						else if (m_Owner != null)
						{
							if (Ability.CanTarget(m_Owner, m, true, true, false))
								m_List.Add(m);
						}
						else
							m_List.Add(m);
					}

					for (int i = 0; i < m_List.Count; i++)
					{
						if (m_List[i] != null)
							DealDamage(m_List[i]);
					}

					m_List.Clear();
					m_List = new List<Mobile>();
				}
			}

			public override bool OnMoveOver(Mobile m)
			{
				if (m_MinDamage != 0)
					DealDamage(m);

				return true;
			}

			public void DealDamage(Mobile m)
			{
				if (m != m_Owner)
					AOS.Damage(m, m_Owner ?? m, Utility.RandomMinMax(m_MinDamage, m_MaxDamage), 0, 100, 0, 0, 0);
			}

			public FireField(Serial serial)
				: base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				// Unsaved.
			}

			public override void Deserialize(GenericReader reader)
			{
			}
		}
		#endregion
	}
}

namespace Server.Commands
{
	public partial class TheSixCommands
	{
		public static void Initialize()
		{
			CommandSystem.Register("SimpleFlame", AccessLevel.Seer, new CommandEventHandler(SimpleFlame_OnCommand));
			CommandSystem.Register("SoulDrain", AccessLevel.Seer, new CommandEventHandler(SoulDrain_OnCommand));
			CommandSystem.Register("FlameWave", AccessLevel.Seer, new CommandEventHandler(FlameWave_OnCommand));
			CommandSystem.Register("FlameCross", AccessLevel.Seer, new CommandEventHandler(FlameCross_OnCommand));
			CommandSystem.Register("CrimsonMeteor", AccessLevel.Seer, new CommandEventHandler(CrimsonMeteor_OnCommand));
			CommandSystem.Register("JaggedFire", AccessLevel.Seer, new CommandEventHandler(JaggedFire_OnCommand));
		}

		[Description("Use the Six's Simple Flame attack")]
		public static void SimpleFlame_OnCommand(CommandEventArgs e)
		{
			e.Mobile.BeginTarget(10, false, TargetFlags.Harmful, new TargetCallback(SimpleFlame_CallBack));
		}

		public static void SimpleFlame_CallBack(Mobile from, object targeted)
		{
			if (targeted is Mobile mobile)
				Ability.SimpleFlame(from, mobile, 35);
			else
				from.SendMessage("That is not a mobile");
		}

		[Description("Use the Six's Soul Drain attack")]
		public static void SoulDrain_OnCommand(CommandEventArgs e)
		{
			Ability.SoulDrain(e.Mobile);
		}

		[Description("Use the Six's Flame Wave attack")]
		public static void FlameWave_OnCommand(CommandEventArgs e)
		{
			Ability.FlameWave(e.Mobile);
		}

		[Description("Use the Six's Fire Field attack")]
		public static void FlameCross_OnCommand(CommandEventArgs e)
		{
			Ability.FlameCross(e.Mobile);
		}

		[Description("Use the Crimson Meteor attack")]
		public static void CrimsonMeteor_OnCommand(CommandEventArgs e)
		{
			Ability.CrimsonMeteor(e.Mobile, 35);
		}

		[Description("Shoot a Jagged line of fire")]
		public static void JaggedFire_OnCommand(CommandEventArgs e)
		{
			Ability.JaggedLineEffect(e.Mobile, 25, 500);
			Ability.JaggedLineEffect(e.Mobile, 25, 500);
			Ability.JaggedLineEffect(e.Mobile, 25, 500);
		}
	}
}
