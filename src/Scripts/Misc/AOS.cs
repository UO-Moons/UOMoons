using Server.Engines.XmlSpawner2;
using Server.Items;
using Server.Mobiles;
using Server.Spells;
using Server.Spells.Fifth;
using Server.Spells.Necromancy;
using Server.Spells.Ninjitsu;
using Server.Spells.Seventh;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.SkillHandlers;
using Server.Spells.Bushido;
using Server.Spells.Chivalry;
using Server.Spells.Mysticism;
using Server.Spells.Spellweaving;

namespace Server;

public enum DamageType
{
	Melee,
	Ranged,
	Spell,
	SpellAOE
}

public class AOS
{
	public static void DisableStatInfluences()
	{
		for (int i = 0; i < SkillInfo.Table.Length; ++i)
		{
			SkillInfo info = SkillInfo.Table[i];

			info.StrScale = 0.0;
			info.DexScale = 0.0;
			info.IntScale = 0.0;
			info.StatTotal = 0.0;
		}
	}

	public static int Damage(IDamageable m, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
	{
		return Damage(m, null, damage, ignoreArmor, phys, fire, cold, pois, nrgy);
	}

	public static int Damage(IDamageable m, int damage, int phys, int fire, int cold, int pois, int nrgy)
	{
		return Damage(m, null, damage, phys, fire, cold, pois, nrgy);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, 0, false);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy)
	{
		return Damage(m, from, damage, ignoreArmor, phys, fire, cold, pois, nrgy, 0, 0, false);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, bool keepAlive)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, keepAlive);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, bool archer, bool deathStrike)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, keepAlive, archer ? DamageType.Ranged : DamageType.Melee); // old deathStrike damage, kept for compatibility
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, DamageType type)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, 0, 0, false, type);
	}

	public static int Damage(IDamageable m, Mobile from, int damage, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, DamageType type)
	{
		return Damage(m, from, damage, false, phys, fire, cold, pois, nrgy, chaos, direct, false, type);
	}

	public static int Damage(IDamageable damageable, Mobile from, int damage, bool ignoreArmor, int phys, int fire, int cold, int pois, int nrgy, int chaos, int direct, bool keepAlive, DamageType type = DamageType.Melee)
	{
		Mobile m = damageable as Mobile;

		if (damageable == null || damageable.Deleted || !damageable.Alive || damage <= 0)
			return 0;

		if (phys == 0 && fire == 100 && cold == 0 && pois == 0 && nrgy == 0)
			MeerMage.StopEffect(m, true);

		if (!Core.AOS)
		{
			m.Damage(damage, from);
			return damage;
		}

		m?.Items.ForEach(i =>
		{
			if (i is ITalismanProtection prot)
				damage = prot.Protection.ScaleDamage(from, damage);
		});

		Fix(ref phys);
		Fix(ref fire);
		Fix(ref cold);
		Fix(ref pois);
		Fix(ref nrgy);
		Fix(ref chaos);
		Fix(ref direct);

		if (Core.ML && chaos > 0)
		{
			switch (Utility.Random(5))
			{
				case 0: phys += chaos; break;
				case 1: fire += chaos; break;
				case 2: cold += chaos; break;
				case 3: pois += chaos; break;
				case 4: nrgy += chaos; break;
			}
		}

		bool ranged = type == DamageType.Ranged;
		BaseQuiver quiver = null;

		if (ranged && from.Race != Race.Gargoyle)
			quiver = from.FindItemOnLayer(Layer.Cloak) as BaseQuiver;

		int totalDamage;

		if (!ignoreArmor)
		{
			// Armor Ignore on OSI ignores all defenses, not just physical.
			int physDamage = damage * phys * (100 - damageable.PhysicalResistance);
			int fireDamage = damage * fire * (100 - damageable.FireResistance);
			int coldDamage = damage * cold * (100 - damageable.ColdResistance);
			int poisonDamage = damage * pois * (100 - damageable.PoisonResistance);
			int energyDamage = damage * nrgy * (100 - damageable.EnergyResistance);

			totalDamage = physDamage + fireDamage + coldDamage + poisonDamage + energyDamage;
			totalDamage /= 10000;

			if (Core.ML)
			{
				totalDamage += damage * direct / 100;

				if (quiver != null)
					totalDamage += totalDamage * quiver.DamageIncrease / 100;
			}

			if (totalDamage < 1)
				totalDamage = 1;
		}
		else if (Core.ML && m is PlayerMobile && from is PlayerMobile)
		{
			if (quiver != null)
				damage += damage * quiver.DamageIncrease / 100;

			totalDamage = Math.Min(damage, Core.TOL && ranged ? 30 : 35);   // Direct Damage cap of 30/35
		}
		else
		{
			totalDamage = damage;

			if (Core.ML && quiver != null)
				totalDamage += totalDamage * quiver.DamageIncrease / 100;
		}

		// object being damaged is not a mobile, so we will end here
		if (damageable is Item)
		{
			return damageable.Damage(totalDamage, from);
		}

		#region Evil Omen, Blood Oath and reflect physical
		if (EvilOmenSpell.TryEndEffect(m))
		{
			totalDamage = (int)(totalDamage * 1.25);
		}

		var fromCreature = from as BaseCreature;
		var toCreature = m as BaseCreature;

		if (from != null && !from.Deleted && from.Alive && !from.IsDeadBondedPet)
		{
			Mobile oath = BloodOathSpell.GetBloodOath(from);

			/* Per EA's UO Herald Pub48 (ML):
			* ((resist spellsx10)/20 + 10=percentage of damage resisted)
			* 
			* Tested 12/29/2017-
			* No cap, also, above forumula is only in effect vs. creatures
			*/

			if (oath == m)
			{
				int originalDamage = totalDamage;
				totalDamage = (int)(totalDamage * 1.2);

				if (toCreature != null)
				{
					from.Damage((int)(originalDamage * (1 - (from.Skills.MagicResist.Value * .5 + 10) / 100)), m);
				}
				else
				{
					from.Damage(originalDamage, m);
				}
			}
			else if (!ignoreArmor && from != m)
			{
				int reflectPhys = Math.Min(105, AosAttributes.GetValue(m, AosAttribute.ReflectPhysical));

				if (reflectPhys != 0)
				{
					if (from is ExodusMinion { FieldActive: true } || from is ExodusOverseer { FieldActive: true })
					{
						from.FixedParticles(0x376A, 20, 10, 0x2530, EffectLayer.Waist);
						from.PlaySound(0x2F4);
						m.SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier");
					}
					else
					{
						if (m != null)
							from.Damage(Scale(damage * phys * (100 - m.PhysicalResistance) / 10000, reflectPhys), m);
					}
				}
			}
		}
		#endregion

		//SHould this go in after or before dragon barding absorb?
		//if (ignoreArmor)
			//DamageEaterContext.CheckDamage(m, totalDamage, 0, 0, 0, 0, 0, 100);
		//else
			//DamageEaterContext.CheckDamage(m, totalDamage, phys, fire, cold, pois, nrgy, direct);

		//if (fire > 0 && totalDamage > 0)
			//SwarmContext.CheckRemove(m);

		//SpiritualityVirtue.GetDamageReduction(m, ref totalDamage);

		//BestialSetHelper.OnDamage(m, from, ref totalDamage);

		//EpiphanyHelper.OnHit(m, totalDamage);

		if (type == DamageType.Spell && m != null && Feint.Registry.ContainsKey(m) && Feint.Registry[m].Enemy == from)
			totalDamage -= (int)(damage * ((double)Feint.Registry[m].DamageReduction / 100));

		/*if (m.Hidden && type >= DamageType.Spell)
		{
			int chance = (int)Math.Min(33, 100 - (ShadowSpell.GetDifficultyFactor(m) * 100));

			if (Utility.Random(100) < chance)
			{
				m.RevealingAction();
				m.NextSkillTime = Core.TickCount + (12000 - ((int)m.Skills[SkillName.Hiding].Value) * 100);
			}
		}*/

		if ((from == null || !from.Player) && m.Player && m.Mount is SwampDragon)
		{
			if (m.Mount is SwampDragon pet && pet.HasBarding)
			{
				int percent = pet.BardingExceptional ? 20 : 10;
				int absorbed = Scale(totalDamage, percent);

				totalDamage -= absorbed;
				pet.BardingHP -= absorbed;

				if (pet.BardingHP < 0)
				{
					pet.HasBarding = false;
					pet.BardingHP = 0;

					m.SendLocalizedMessage(1053031); // Your dragon's barding has been destroyed!
				}
			}
		}

		if (type <= DamageType.Ranged)
		{
			AttuneWeaponSpell.TryAbsorb(m, ref totalDamage);
		}

		if (keepAlive && totalDamage > m.Hits)
			totalDamage = m.Hits;

		if (fromCreature != null && type <= DamageType.Ranged)
		{
			fromCreature.AlterMeleeDamageTo(m, ref totalDamage);
		}

		if (toCreature != null && type <= DamageType.Ranged)
		{
			toCreature.AlterMeleeDamageFrom(from, ref totalDamage);
			toCreature.OnBeforeDamage(from, ref totalDamage, type);
		}

		if (totalDamage <= 0)
		{
			return 0;
		}

		if (from != null)
		{
			DoLeech(totalDamage, from, m);
		}

		totalDamage = m.Damage(totalDamage, from, true, false);

		//ExplodingTarPotion.RemoveEffects(m);

		if (type == DamageType.Melee && fromCreature != null &&
		    (m is PlayerMobile || (toCreature != null && !toCreature.IsMonster)))
		{
			from.RegisterDamage(totalDamage / 4, m);
		}

		//SpiritSpeak.CheckDisrupt(m);

		if (m.Spell != null)
			((Spell)m.Spell).CheckCasterDisruption(true, phys, fire, cold, pois, nrgy);

		//BattleLust.IncreaseBattleLust(m, totalDamage);

		//if (ManaPhasingOrb.IsInManaPhase(m))
		//	ManaPhasingOrb.RemoveFromTable(m);

		//SoulChargeContext.CheckHit(from, m, totalDamage);

		SleepSpell.OnDamage(m);
		PurgeMagicSpell.OnMobileDoDamage(from);

		BaseCostume.OnDamaged(m);

		return totalDamage;

		/*if (from != null && !from.Deleted && from.Alive)
		{
			int reflectPhys = AosAttributes.GetValue(m, AosAttribute.ReflectPhysical);
			if (reflectPhys > 50) reflectPhys = 50;

			if (reflectPhys != 0)
			{
				if (from is ExodusMinion && ((ExodusMinion)from).FieldActive || from is ExodusOverseer && ((ExodusOverseer)from).FieldActive)
				{
					from.FixedParticles(0x376A, 20, 10, 0x2530, EffectLayer.Waist);
					from.PlaySound(0x2F4);
					m.SendAsciiMessage("Your weapon cannot penetrate the creature's magical barrier");
				}
				else
				{
					int rphysdmg = Scale(damage * phys * (100 - (ignoreArmor ? 0 : m.PhysicalResistance)) / 10000, reflectPhys);

					if (rphysdmg > 0)
					{
						from.Damage(rphysdmg, m);
						m.FixedParticles(0x22C6, 5, 5, 0x7F5, 0x960, 0x3, EffectLayer.Waist);
						//m.PlaySound( 0x3B9 );
						//m.PlaySound( 0x56 );
					}
				}
			}
		}

		m.Damage(totalDamage, from);
		return totalDamage;*/
	}

	public static void Fix(ref int val)
	{
		if (val < 0)
			val = 0;
	}

	public static int Scale(int input, int percent)
	{
		return input * percent / 100;
	}

	public static void DoLeech(int damageGiven, Mobile from, Mobile target)
	{
		TransformContext context = TransformationSpellHelper.GetContext(from);

		if (context != null)
		{
			if (context.Type == typeof(WraithFormSpell))
			{
				//    int manaLeech = AOS.Scale(damageGiven, Math.Min(target.Mana, (int)from.Skills.SpiritSpeak.Value / 5)); // Wraith form gives 5-20% mana leech
				int manaLeech = Scale(damageGiven, Math.Min(target.Mana, Math.Max(8, 5 + (int)(0.16 * from.Skills.SpiritSpeak.Value))));

				if (manaLeech != 0)
				{
					from.Mana += manaLeech;
					from.PlaySound(0x44D);

					target.Mana -= manaLeech;
				}
			}
			else if (context.Type == typeof(VampiricEmbraceSpell))
			{
				from.Hits += Scale(damageGiven, 20);
				from.PlaySound(0x44D);

			}
		}
	}

	public static int GetStatus(Mobile from, int index)
	{
		return index switch
		{
			// TODO: Account for buffs/debuffs
			0 => from.GetMaxResistance(ResistanceType.Physical),
			1 => from.GetMaxResistance(ResistanceType.Fire),
			2 => from.GetMaxResistance(ResistanceType.Cold),
			3 => from.GetMaxResistance(ResistanceType.Poison),
			4 => from.GetMaxResistance(ResistanceType.Energy),
			5 => AosAttributes.GetValue(from, AosAttribute.DefendChance),
			6 => 45,
			7 => AosAttributes.GetValue(from, AosAttribute.AttackChance),
			8 => AosAttributes.GetValue(from, AosAttribute.WeaponSpeed),
			9 => AosAttributes.GetValue(from, AosAttribute.WeaponDamage),
			10 => AosAttributes.GetValue(from, AosAttribute.LowerRegCost),
			11 => AosAttributes.GetValue(from, AosAttribute.SpellDamage),
			12 => AosAttributes.GetValue(from, AosAttribute.CastRecovery),
			13 => AosAttributes.GetValue(from, AosAttribute.CastSpeed),
			14 => AosAttributes.GetValue(from, AosAttribute.LowerManaCost),
			_ => 0,
		};
	}
}

[Flags]
public enum AosAttribute
{
	RegenHits = 0x00000001,
	RegenStam = 0x00000002,
	RegenMana = 0x00000004,
	DefendChance = 0x00000008,
	AttackChance = 0x00000010,
	BonusStr = 0x00000020,
	BonusDex = 0x00000040,
	BonusInt = 0x00000080,
	BonusHits = 0x00000100,
	BonusStam = 0x00000200,
	BonusMana = 0x00000400,
	WeaponDamage = 0x00000800,
	WeaponSpeed = 0x00001000,
	SpellDamage = 0x00002000,
	CastRecovery = 0x00004000,
	CastSpeed = 0x00008000,
	LowerManaCost = 0x00010000,
	LowerRegCost = 0x00020000,
	ReflectPhysical = 0x00040000,
	EnhancePotions = 0x00080000,
	Luck = 0x00100000,
	SpellChanneling = 0x00200000,
	NightSight = 0x00400000,
	IncreasedKarmaLoss = 0x00800000,
	Brittle = 0x01000000,
	LowerAmmoCost = 0x02000000,
	BalancedWeapon = 0x04000000
}

public interface IAosAttribute
{
	[CommandProperty(AccessLevel.GameMaster)]
	public AosAttributes Attributes { get; }
}

public sealed class AosAttributes : BaseAttributes
{
	public AosAttributes(Item owner)
		: base(owner)
	{
	}

	public AosAttributes(Item owner, AosAttributes other)
		: base(owner, other)
	{
	}

	public AosAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public static bool IsValid(AosAttribute attribute)
	{
		return true;
	}

	public static int[] GetValues(Mobile m, params AosAttribute[] attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static int[] GetValues(Mobile m, IEnumerable<AosAttribute> attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosAttribute> attributes)
	{
		return attributes.Select(a => GetValue(m, a));
	}

	public static int GetValue(Mobile m, AosAttribute attribute)
	{
		if (!Core.AOS)
		{
			return 0;
		}

		if (World.Loading || !IsValid(attribute))
		{
			return 0;
		}
		List<Item> items = m.Items;
		int value = 0;

		//if (attribute == AosAttribute.Luck || attribute == AosAttribute.RegenMana || attribute == AosAttribute.DefendChance || attribute == AosAttribute.EnhancePotions)
		//	value += SphynxFortune.GetAosAttributeBonus(m, attribute);

		value += Enhancement.GetValue(m, attribute);

		for (int i = 0; i < items.Count; ++i)
		{
			Item obj = items[i];

			if (obj is IAosAttribute attributeItem)
			{
				AosAttributes attrs = attributeItem.Attributes;

				if (attrs != null)
					value += attrs[attribute];

				if (attribute == AosAttribute.Luck && attributeItem is BaseEquipment equipment)
					value += equipment.GetLuckBonus();

				if (obj is ISetItem item)
				{
					attrs = item.SetAttributes;

					if (attrs != null && item.LastEquipped)
						value += attrs[attribute];
				}
			}
			else if (obj is BaseQuiver quiver)
			{
				AosAttributes attrs = quiver.Attributes;

				if (attrs != null)
					value += attrs[attribute];
			}
		}

		#region Malus/Buff Handler
		//value += SkillMasterySpell.GetAttributeBonus(m, attribute);

		if (attribute == AosAttribute.WeaponDamage)
		{
			//if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
			//	value += 35;

			// attacker gets 10% bonus when they're under divine fury
			if (DivineFurySpell.UnderEffect(m))
				value += DivineFurySpell.GetDamageBonus(m);

			// Horrific Beast transformation gives a +25% bonus to damage.
			if (TransformationSpellHelper.UnderTransformation(m, typeof(HorrificBeastSpell)))
				value += 25;

			int defenseMasteryMalus = 0;
			int discordanceEffect = 0;

			// Defense Mastery gives a -50%/-80% malus to damage.
			if (DefenseMastery.GetMalus(m, ref defenseMasteryMalus))
				value -= defenseMasteryMalus;

			// Discordance gives a -2%/-48% malus to damage.
			if (Discordance.GetEffect(m, ref discordanceEffect))
				value -= discordanceEffect * 2;

			if (Block.IsBlocking(m))
				value -= 30;

			//if (m is PlayerMobile && m.Race == Race.Gargoyle)
			//{
			//	value += ((PlayerMobile)m).GetRacialBerserkBuff(false);
			//}

			//if (BaseFishPie.IsUnderEffects(m, FishPieEffect.WeaponDam))
			//	value += 5;
		}
		else if (attribute == AosAttribute.SpellDamage)
		{
			//if (BaseMagicalFood.IsUnderInfluence(m, MagicalFood.GrapesOfWrath))
			//	value += 15;

			if (PsychicAttack.Registry.ContainsKey(m))
				value -= PsychicAttack.Registry[m].SpellDamageMalus;

			TransformContext context = TransformationSpellHelper.GetContext(m);

			if (context != null && context.Spell is ReaperFormSpell)
				value += ((ReaperFormSpell)context.Spell).SpellDamageBonus;

			value += ArcaneEmpowermentSpell.GetSpellBonus(m, true);

			//if (m is PlayerMobile && m.Race == Race.Gargoyle)
			//{
			//	value += ((PlayerMobile)m).GetRacialBerserkBuff(true);
			//}

			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.GuildOfArcaneArts))
			//	value += 5;

			//if (BaseFishPie.IsUnderEffects(m, FishPieEffect.SpellDamage))
			//	value += 5;
		}
		else if (attribute == AosAttribute.CastSpeed)
		{
			//if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
			//	value -= 5;

			if (EssenceOfWindSpell.IsDebuffed(m))
				value -= EssenceOfWindSpell.GetFcMalus(m);

			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.BardicCollegium))
			//	value += 1;

			if (SleepSpell.IsUnderSleepEffects(m))
				value -= 2;

			if (TransformationSpellHelper.UnderTransformation(m, typeof(StoneFormSpell)))
				value -= 2;
		}
		else if (attribute == AosAttribute.CastRecovery)
		{
			//if (HowlOfCacophony.IsUnderEffects(m))
			//	value -= 5;

			value -= ThunderstormSpell.GetCastRecoveryMalus(m);

			if (SleepSpell.IsUnderSleepEffects(m))
				value -= 3;
		}
		else if (attribute == AosAttribute.WeaponSpeed)
		{
			//if (HowlOfCacophony.IsUnderEffects(m) || AuraOfNausea.UnderNausea(m))
			//	value -= 60;

			if (DivineFurySpell.UnderEffect(m))
				value += DivineFurySpell.GetWeaponSpeedBonus(m);

			value += HonorableExecution.GetSwingBonus(m);

			TransformContext context = TransformationSpellHelper.GetContext(m);

			if (context != null && context.Spell is ReaperFormSpell)
				value += ((ReaperFormSpell)context.Spell).SwingSpeedBonus;

			int discordanceEffect = 0;

			// Discordance gives a malus of -0/-28% to swing speed.
			if (Discordance.GetEffect(m, ref discordanceEffect))
				value -= discordanceEffect;

			if (EssenceOfWindSpell.IsDebuffed(m))
				value -= EssenceOfWindSpell.GetSsiMalus(m);

			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.GuildOfAssassins))
			//	value += 5;

			if (SleepSpell.IsUnderSleepEffects(m))
				value -= 45;

			if (TransformationSpellHelper.UnderTransformation(m, typeof(Spells.Mysticism.StoneFormSpell)))
				value -= 10;

			//if (StickySkin.IsUnderEffects(m))
			//	value -= 30;
		}
		else if (attribute == AosAttribute.AttackChance)
		{
			//if (AuraOfNausea.UnderNausea(m))
			//	value -= 60;

			if (DivineFurySpell.UnderEffect(m))
				value += DivineFurySpell.GetAttackBonus(m);

			if (BaseWeapon.CheckAnimal(m, typeof(GreyWolf)) || BaseWeapon.CheckAnimal(m, typeof(BakeKitsune)))
				value += 20; // attacker gets 20% bonus when under Wolf or Bake Kitsune form

			if (HitLower.IsUnderAttackEffect(m))
				value -= 25; // Under Hit Lower Attack effect -> 25% malus

			WeaponAbility ability = WeaponAbility.GetCurrentAbility(m);

			if (ability != null)
				value += ability.AccuracyBonus;

			SpecialMove move = SpecialMove.GetCurrentMove(m);

			if (move != null)
				value += move.GetAccuracyBonus(m);

			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.WarriorsGuild))
			//	value += 5;

			if (SleepSpell.IsUnderSleepEffects(m))
				value -= 45;

			if (m.Race == Race.Gargoyle)
				value += 5;  //Gargoyles get a +5 HCI

			//if (BaseFishPie.IsUnderEffects(m, FishPieEffect.HitChance))
			//	value += 8;
		}
		else if (attribute == AosAttribute.DefendChance)
		{
			//if (AuraOfNausea.UnderNausea(m))
			//	value -= 60;

			if (DivineFurySpell.UnderEffect(m))
				value -= DivineFurySpell.GetDefendMalus(m);

			value -= HitLower.GetDefenseMalus(m);

			int discordanceEffect = 0;
			int surpriseMalus = 0;

			value += Block.GetBonus(m);

			if (SurpriseAttack.GetMalus(m, ref surpriseMalus))
				value -= surpriseMalus;

			// Defender loses -0/-28% if under the effect of Discordance.
			if (Discordance.GetEffect(m, ref discordanceEffect))
				value -= discordanceEffect;

			//if (BaseFishPie.IsUnderEffects(m, FishPieEffect.DefChance))
			//	value += 8;
		}
		else if (attribute == AosAttribute.RegenHits)
		{
			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.MaritimeGuild))
				//value += 2;

			//if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.HitsRegen))
			//	value += 3;

			//if (SurgeShield.IsUnderEffects(m, SurgeType.Hits))
			//	value += 10;

			//if (SearingWeaponContext.HasContext(m))
			//	value -= m is PlayerMobile ? 20 : 60;

			//Virtue Artifacts
			//value += AnkhPendant.GetHitsRegenModifier(m);
		}
		else if (attribute == AosAttribute.RegenStam)
		{
			//if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.StamRegen))
			//	value += 3;

			//if (SurgeShield.IsUnderEffects(m, SurgeType.Stam))
			//	value += 10;

			//Virtue Artifacts
			//value += AnkhPendant.GetStamRegenModifier(m);
		}
		else if (attribute == AosAttribute.RegenMana)
		{
			//if (CityLoyaltySystem.HasTradeDeal(m, TradeDeal.MerchantsAssociation))
			//	value += 1;

			//if (m is PlayerMobile && BaseFishPie.IsUnderEffects(m, FishPieEffect.ManaRegen))
			//	value += 3;
			//
			//if (SurgeShield.IsUnderEffects(m, SurgeType.Mana))
			//	value += 10;

			//Virtue Artifacts
			//value += AnkhPendant.GetManaRegenModifier(m);
		}
		#endregion

		return value;
	}

	public override void SetValue(int bitmask, int value)
	{
		if (bitmask == (int)AosAttribute.WeaponSpeed && Owner is BaseWeapon)
		{
			((BaseWeapon)Owner).WeaponAttributes.ScaleLeech(value);
		}

		base.SetValue(bitmask, value);
	}

	public int this[AosAttribute attribute]
	{
		get => GetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override string ToString()
	{
		return "...";
	}

	public void AddStatBonuses(Mobile to)
	{
		int strBonus = BonusStr;
		int dexBonus = BonusDex;
		int intBonus = BonusInt;

		if (strBonus != 0 || dexBonus != 0 || intBonus != 0)
		{
			string modName = Owner.Serial.ToString();

			if (strBonus != 0)
				to.AddStatMod(new StatMod(StatType.Str, modName + "Str", strBonus, TimeSpan.Zero));

			if (dexBonus != 0)
				to.AddStatMod(new StatMod(StatType.Dex, modName + "Dex", dexBonus, TimeSpan.Zero));

			if (intBonus != 0)
				to.AddStatMod(new StatMod(StatType.Int, modName + "Int", intBonus, TimeSpan.Zero));
		}

		to.CheckStatTimers();
	}

	public void RemoveStatBonuses(Mobile from)
	{
		string modName = Owner.Serial.ToString();

		from.RemoveStatMod(modName + "Str");
		from.RemoveStatMod(modName + "Dex");
		from.RemoveStatMod(modName + "Int");

		from.CheckStatTimers();
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int RegenHits { get => this[AosAttribute.RegenHits]; set => this[AosAttribute.RegenHits] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RegenStam { get => this[AosAttribute.RegenStam]; set => this[AosAttribute.RegenStam] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int RegenMana { get => this[AosAttribute.RegenMana]; set => this[AosAttribute.RegenMana] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DefendChance { get => this[AosAttribute.DefendChance]; set => this[AosAttribute.DefendChance] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int AttackChance { get => this[AosAttribute.AttackChance]; set => this[AosAttribute.AttackChance] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusStr { get => this[AosAttribute.BonusStr]; set => this[AosAttribute.BonusStr] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusDex { get => this[AosAttribute.BonusDex]; set => this[AosAttribute.BonusDex] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusInt { get => this[AosAttribute.BonusInt]; set => this[AosAttribute.BonusInt] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusHits { get => this[AosAttribute.BonusHits]; set => this[AosAttribute.BonusHits] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusStam { get => this[AosAttribute.BonusStam]; set => this[AosAttribute.BonusStam] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int BonusMana { get => this[AosAttribute.BonusMana]; set => this[AosAttribute.BonusMana] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int WeaponDamage { get => this[AosAttribute.WeaponDamage]; set => this[AosAttribute.WeaponDamage] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int WeaponSpeed { get => this[AosAttribute.WeaponSpeed]; set => this[AosAttribute.WeaponSpeed] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SpellDamage { get => this[AosAttribute.SpellDamage]; set => this[AosAttribute.SpellDamage] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int CastRecovery { get => this[AosAttribute.CastRecovery]; set => this[AosAttribute.CastRecovery] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int CastSpeed { get => this[AosAttribute.CastSpeed]; set => this[AosAttribute.CastSpeed] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int LowerManaCost { get => this[AosAttribute.LowerManaCost]; set => this[AosAttribute.LowerManaCost] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int LowerRegCost { get => this[AosAttribute.LowerRegCost]; set => this[AosAttribute.LowerRegCost] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ReflectPhysical { get => this[AosAttribute.ReflectPhysical]; set => this[AosAttribute.ReflectPhysical] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int EnhancePotions { get => this[AosAttribute.EnhancePotions]; set => this[AosAttribute.EnhancePotions] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Luck { get => this[AosAttribute.Luck]; set => this[AosAttribute.Luck] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SpellChanneling { get => this[AosAttribute.SpellChanneling]; set => this[AosAttribute.SpellChanneling] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int NightSight { get => this[AosAttribute.NightSight]; set => this[AosAttribute.NightSight] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int IncreasedKarmaLoss { get => this[AosAttribute.IncreasedKarmaLoss]; set => this[AosAttribute.IncreasedKarmaLoss] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Brittle
	{
		get => this[AosAttribute.Brittle];
		set => this[AosAttribute.Brittle] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int LowerAmmoCost
	{
		get => this[AosAttribute.LowerAmmoCost];
		set => this[AosAttribute.LowerAmmoCost] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BalancedWeapon
	{
		get => this[AosAttribute.BalancedWeapon];
		set => this[AosAttribute.BalancedWeapon] = value;
	}
}

[Flags]
public enum AosWeaponAttribute : long
{
	LowerStatReq = 0x00000001,
	SelfRepair = 0x00000002,
	HitLeechHits = 0x00000004,
	HitLeechStam = 0x00000008,
	HitLeechMana = 0x00000010,
	HitLowerAttack = 0x00000020,
	HitLowerDefend = 0x00000040,
	HitMagicArrow = 0x00000080,
	HitHarm = 0x00000100,
	HitFireball = 0x00000200,
	HitLightning = 0x00000400,
	HitDispel = 0x00000800,
	HitColdArea = 0x00001000,
	HitFireArea = 0x00002000,
	HitPoisonArea = 0x00004000,
	HitEnergyArea = 0x00008000,
	HitPhysicalArea = 0x00010000,
	ResistPhysicalBonus = 0x00020000,
	ResistFireBonus = 0x00040000,
	ResistColdBonus = 0x00080000,
	ResistPoisonBonus = 0x00100000,
	ResistEnergyBonus = 0x00200000,
	UseBestSkill = 0x00400000,
	MageWeapon = 0x00800000,
	DurabilityBonus = 0x01000000,
	BloodDrinker = 0x02000000,
	BattleLust = 0x04000000,
	HitCurse = 0x08000000,
	HitFatigue = 0x10000000,
	HitManaDrain = 0x20000000,
	SplinteringWeapon = 0x40000000,
	ReactiveParalyze = 0x80000000,
}

public sealed class AosWeaponAttributes : BaseAttributes
{
	public static bool IsValid(AosWeaponAttribute attribute)
	{
		if (!Core.AOS)
		{
			return false;
		}

		if (!Core.SA && attribute >= AosWeaponAttribute.BloodDrinker)
		{
			return false;
		}

		return true;
	}

	public static int[] GetValues(Mobile m, params AosWeaponAttribute[] attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static int[] GetValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosWeaponAttribute> attributes)
	{
		return attributes.Select(a => GetValue(m, a));
	}

	public static int GetValue(Mobile m, AosWeaponAttribute attribute)
	{
		if (!Core.AOS)
			return 0;

		if (World.Loading || !IsValid(attribute))
		{
			return 0;
		}

		List<Item> items = m.Items;
		int value = 0;

		#region Enhancement
		value += Enhancement.GetValue(m, attribute);
		#endregion

		for (int i = 0; i < items.Count; ++i)
		{
			Item obj = items[i];

			if (obj is BaseWeapon weapon)
			{
				AosWeaponAttributes attrs = weapon.WeaponAttributes;

				if (attrs != null)
					value += attrs[attribute];
			}
			else if (obj is ElvenGlasses glasses)
			{
				AosWeaponAttributes attrs = glasses.WeaponAttributes;

				if (attrs != null)
					value += attrs[attribute];
			}
		}

		return value;
	}

	public override void SetValue(int bitmask, int value)
	{
		if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
		{
			((BaseWeapon)Owner).UnscaleDurability();
		}

		base.SetValue(bitmask, value);

		if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && Owner is BaseWeapon)
		{
			((BaseWeapon)Owner).ScaleDurability();
		}
	}

	public AosWeaponAttributes(Item owner)
		: base(owner)
	{
	}

	public AosWeaponAttributes(Item owner, AosWeaponAttributes other)
		: base(owner, other)
	{
	}

	public AosWeaponAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public int this[AosWeaponAttribute attribute]
	{
		get => ExtendedGetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public int ExtendedGetValue(int bitmask)
	{
		int value = GetValue(bitmask);

		XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

		if (xaos != null)
		{
			value += xaos.GetValue(bitmask);
		}

		return value;
	}

	public void ScaleLeech(int weaponSpeed)
	{
		BaseWeapon wep = Owner as BaseWeapon;

		if (wep == null || wep.IsArtifact)
			return;

		if (HitLeechHits > 0)
		{
			double postcap = HitLeechHits;
			if (postcap < 1.0) postcap = 1.0;

			int newhits = (int)(wep.Speed * 2500 / (100 + weaponSpeed) * postcap);

			if (wep is BaseRanged)
				newhits /= 2;

			if (HitLeechHits > newhits)
				HitLeechHits = newhits;
		}

		if (HitLeechMana > 0)
		{
			double postcap = HitLeechMana;
			if (postcap < 1.0) postcap = 1.0;

			int newmana = (int)(wep.Speed * 2500 / (100 + weaponSpeed) * postcap);

			if (wep is BaseRanged)
				newmana /= 2;

			if (HitLeechMana > newmana)
				HitLeechMana = newmana;
		}
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int LowerStatReq { get => this[AosWeaponAttribute.LowerStatReq]; set => this[AosWeaponAttribute.LowerStatReq] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SelfRepair { get => this[AosWeaponAttribute.SelfRepair]; set => this[AosWeaponAttribute.SelfRepair] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLeechHits { get => this[AosWeaponAttribute.HitLeechHits]; set => this[AosWeaponAttribute.HitLeechHits] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLeechStam { get => this[AosWeaponAttribute.HitLeechStam]; set => this[AosWeaponAttribute.HitLeechStam] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLeechMana { get => this[AosWeaponAttribute.HitLeechMana]; set => this[AosWeaponAttribute.HitLeechMana] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLowerAttack { get => this[AosWeaponAttribute.HitLowerAttack]; set => this[AosWeaponAttribute.HitLowerAttack] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLowerDefend { get => this[AosWeaponAttribute.HitLowerDefend]; set => this[AosWeaponAttribute.HitLowerDefend] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitMagicArrow { get => this[AosWeaponAttribute.HitMagicArrow]; set => this[AosWeaponAttribute.HitMagicArrow] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitHarm { get => this[AosWeaponAttribute.HitHarm]; set => this[AosWeaponAttribute.HitHarm] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitFireball { get => this[AosWeaponAttribute.HitFireball]; set => this[AosWeaponAttribute.HitFireball] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitLightning { get => this[AosWeaponAttribute.HitLightning]; set => this[AosWeaponAttribute.HitLightning] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitDispel { get => this[AosWeaponAttribute.HitDispel]; set => this[AosWeaponAttribute.HitDispel] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitColdArea { get => this[AosWeaponAttribute.HitColdArea]; set => this[AosWeaponAttribute.HitColdArea] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitFireArea { get => this[AosWeaponAttribute.HitFireArea]; set => this[AosWeaponAttribute.HitFireArea] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitPoisonArea { get => this[AosWeaponAttribute.HitPoisonArea]; set => this[AosWeaponAttribute.HitPoisonArea] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitEnergyArea { get => this[AosWeaponAttribute.HitEnergyArea]; set => this[AosWeaponAttribute.HitEnergyArea] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitPhysicalArea { get => this[AosWeaponAttribute.HitPhysicalArea]; set => this[AosWeaponAttribute.HitPhysicalArea] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResistPhysicalBonus { get => this[AosWeaponAttribute.ResistPhysicalBonus]; set => this[AosWeaponAttribute.ResistPhysicalBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResistFireBonus { get => this[AosWeaponAttribute.ResistFireBonus]; set => this[AosWeaponAttribute.ResistFireBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResistColdBonus { get => this[AosWeaponAttribute.ResistColdBonus]; set => this[AosWeaponAttribute.ResistColdBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResistPoisonBonus { get => this[AosWeaponAttribute.ResistPoisonBonus]; set => this[AosWeaponAttribute.ResistPoisonBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResistEnergyBonus { get => this[AosWeaponAttribute.ResistEnergyBonus]; set => this[AosWeaponAttribute.ResistEnergyBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int UseBestSkill { get => this[AosWeaponAttribute.UseBestSkill]; set => this[AosWeaponAttribute.UseBestSkill] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MageWeapon { get => this[AosWeaponAttribute.MageWeapon]; set => this[AosWeaponAttribute.MageWeapon] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DurabilityBonus { get => this[AosWeaponAttribute.DurabilityBonus]; set => this[AosWeaponAttribute.DurabilityBonus] = value; }

	#region SA
	[CommandProperty(AccessLevel.GameMaster)]
	public int BloodDrinker
	{
		get => this[AosWeaponAttribute.BloodDrinker];
		set => this[AosWeaponAttribute.BloodDrinker] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BattleLust
	{
		get => this[AosWeaponAttribute.BattleLust];
		set => this[AosWeaponAttribute.BattleLust] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitCurse
	{
		get => this[AosWeaponAttribute.HitCurse];
		set => this[AosWeaponAttribute.HitCurse] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitFatigue
	{
		get => this[AosWeaponAttribute.HitFatigue];
		set => this[AosWeaponAttribute.HitFatigue] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitManaDrain
	{
		get => this[AosWeaponAttribute.HitManaDrain];
		set => this[AosWeaponAttribute.HitManaDrain] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SplinteringWeapon
	{
		get => this[AosWeaponAttribute.SplinteringWeapon];
		set => this[AosWeaponAttribute.SplinteringWeapon] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ReactiveParalyze
	{
		get => this[AosWeaponAttribute.ReactiveParalyze];
		set => this[AosWeaponAttribute.ReactiveParalyze] = value;
	}
	#endregion
}

[Flags]
public enum ExtendedWeaponAttribute
{
	BoneBreaker = 0x00000001,
	HitSwarm = 0x00000002,
	HitSparks = 0x00000004,
	Bane = 0x00000008,
	MysticWeapon = 0x00000010,
	AssassinHoned = 0x00000020,
	Focus = 0x00000040,
	HitExplosion = 0x00000080
}

public sealed class ExtendedWeaponAttributes : BaseAttributes
{
	public ExtendedWeaponAttributes(Item owner)
		: base(owner)
	{
	}

	public ExtendedWeaponAttributes(Item owner, ExtendedWeaponAttributes other)
		: base(owner, other)
	{
	}

	public ExtendedWeaponAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public static int GetValue(Mobile m, ExtendedWeaponAttribute attribute)
	{
		int value = 0;

		value += Enhancement.GetValue(m, attribute);

		for (int i = 0; i < m.Items.Count; ++i)
		{
			Item obj = m.Items[i];

			if (obj is BaseWeapon)
			{
				ExtendedWeaponAttributes attrs = ((BaseWeapon)obj).ExtendedWeaponAttributes;

				if (attrs != null)
					value += attrs[attribute];
			}
		}

		return value;
	}

	public int this[ExtendedWeaponAttribute attribute]
	{
		get => GetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int BoneBreaker
	{
		get => this[ExtendedWeaponAttribute.BoneBreaker];
		set => this[ExtendedWeaponAttribute.BoneBreaker] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitSwarm
	{
		get => this[ExtendedWeaponAttribute.HitSwarm];
		set => this[ExtendedWeaponAttribute.HitSwarm] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitSparks
	{
		get => this[ExtendedWeaponAttribute.HitSparks];
		set => this[ExtendedWeaponAttribute.HitSparks] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Bane
	{
		get => this[ExtendedWeaponAttribute.Bane];
		set => this[ExtendedWeaponAttribute.Bane] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int MysticWeapon
	{
		get => this[ExtendedWeaponAttribute.MysticWeapon];
		set => this[ExtendedWeaponAttribute.MysticWeapon] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int AssassinHoned
	{
		get => this[ExtendedWeaponAttribute.AssassinHoned];
		set => this[ExtendedWeaponAttribute.AssassinHoned] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Focus
	{
		get => this[ExtendedWeaponAttribute.Focus];
		set => this[ExtendedWeaponAttribute.Focus] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int HitExplosion
	{
		get => this[ExtendedWeaponAttribute.HitExplosion];
		set => this[ExtendedWeaponAttribute.HitExplosion] = value;
	}
}

[Flags]
public enum AosArmorAttribute
{
	LowerStatReq = 0x00000001,
	SelfRepair = 0x00000002,
	MageArmor = 0x00000004,
	DurabilityBonus = 0x00000008,
	ReactiveParalyze = 0x00000010,
	SoulCharge = 0x00000020
}

public sealed class AosArmorAttributes : BaseAttributes
{
	public AosArmorAttributes(Item owner)
		: base(owner)
	{
	}

	public AosArmorAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public AosArmorAttributes(Item owner, AosArmorAttributes other)
		: base(owner, other)
	{
	}

	public static int GetValue(Mobile m, AosArmorAttribute attribute)
	{
		if (!Core.AOS)
			return 0;

		List<Item> items = m.Items;
		int value = 0;

		for (int i = 0; i < items.Count; ++i)
		{
			Item obj = items[i];

			if (obj is BaseArmor armor)
			{
				AosArmorAttributes attrs = armor.ArmorAttributes;

				if (attrs != null)
					value += attrs[attribute];
			}
			else if (obj is BaseClothing clothing)
			{
				AosArmorAttributes attrs = clothing.ClothingAttributes;

				if (attrs != null)
					value += attrs[attribute];
			}
		}

		return value;
	}

	public static int[] GetValues(Mobile m, params AosArmorAttribute[] attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static int[] GetValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<AosArmorAttribute> attributes)
	{
		return attributes.Select(a => GetValue(m, a));
	}

	public int this[AosArmorAttribute attribute]
	{
		get => ExtendedGetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override void SetValue(int bitmask, int value)
	{
		if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
		{
			if (Owner is BaseArmor armor)
			{
				armor.UnscaleDurability();
			}
			else if (Owner is BaseClothing clothing)
			{
				clothing.UnscaleDurability();
			}
		}

		base.SetValue(bitmask, value);

		if (bitmask == (int)AosArmorAttribute.DurabilityBonus)
		{
			if (Owner is BaseArmor armor)
			{
				armor.ScaleDurability();
			}
			else if (Owner is BaseClothing clothing)
			{
				clothing.ScaleDurability();
			}
		}
	}

	public int ExtendedGetValue(int bitmask)
	{
		int value = GetValue(bitmask);

		XmlAosAttributes xaos = (XmlAosAttributes)XmlAttach.FindAttachment(Owner, typeof(XmlAosAttributes));

		if (xaos != null)
		{
			value += xaos.GetValue(bitmask);
		}

		return value;
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int LowerStatReq { get => this[AosArmorAttribute.LowerStatReq]; set => this[AosArmorAttribute.LowerStatReq] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int SelfRepair { get => this[AosArmorAttribute.SelfRepair]; set => this[AosArmorAttribute.SelfRepair] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int MageArmor { get => this[AosArmorAttribute.MageArmor]; set => this[AosArmorAttribute.MageArmor] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int DurabilityBonus { get => this[AosArmorAttribute.DurabilityBonus]; set => this[AosArmorAttribute.DurabilityBonus] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int ReactiveParalyze
	{
		get => this[AosArmorAttribute.ReactiveParalyze];
		set => this[AosArmorAttribute.ReactiveParalyze] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int SoulCharge
	{
		get => this[AosArmorAttribute.SoulCharge];
		set => this[AosArmorAttribute.SoulCharge] = value;
	}
}

public sealed class AosSkillBonuses : BaseAttributes
{
	private List<SkillMod> m_Mods;

	public AosSkillBonuses(Item owner)
		: base(owner)
	{
	}

	public AosSkillBonuses(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public AosSkillBonuses(Item owner, AosSkillBonuses other)
		: base(owner, other)
	{
	}

	public void GetProperties(ObjectPropertyList list)
	{
		for (int i = 0; i < 5; ++i)
		{
			if (!GetValues(i, out SkillName skill, out double bonus))
				continue;

			list.Add(1060451 + i, "#{0}\t{1}", GetLabel(skill), bonus);
		}
	}

	public static int GetLabel(SkillName skill)
	{
		return skill switch
		{
			SkillName.EvalInt => 1002070,// Evaluate Intelligence
			SkillName.Forensics => 1002078,// Forensic Evaluation
			SkillName.Lockpicking => 1002097,// Lockpicking
			_ => 1044060 + (int)skill,
		};
	}

	public void AddTo(Mobile m)
	{
		Remove();

		for (int i = 0; i < 5; ++i)
		{
			if (!GetValues(i, out SkillName skill, out double bonus))
				continue;

			m_Mods ??= new List<SkillMod>();

			SkillMod sk = new DefaultSkillMod(skill, true, bonus)
			{
				ObeyCap = true
			};
			m.AddSkillMod(sk);
			m_Mods.Add(sk);
		}
	}

	public void Remove()
	{
		if (m_Mods == null)
			return;

		for (int i = 0; i < m_Mods.Count; ++i)
		{
			Mobile m = m_Mods[i].Owner;
			m_Mods[i].Remove();

			CheckCancelMorph(m);
		}
		m_Mods = null;
	}

	public bool GetValues(int index, out SkillName skill, out double bonus)
	{
		int v = GetValue(1 << index);
		int vSkill = 0;
		int vBonus = 0;

		for (int i = 0; i < 16; ++i)
		{
			vSkill <<= 1;
			vSkill |= v & 1;
			v >>= 1;

			vBonus <<= 1;
			vBonus |= v & 1;
			v >>= 1;
		}

		skill = (SkillName)vSkill;
		bonus = (double)vBonus / 10;

		return bonus != 0;
	}

	public void SetValues(int index, SkillName skill, double bonus)
	{
		int v = 0;
		int vSkill = (int)skill;
		int vBonus = (int)(bonus * 10);

		for (int i = 0; i < 16; ++i)
		{
			v <<= 1;
			v |= vBonus & 1;
			vBonus >>= 1;

			v <<= 1;
			v |= vSkill & 1;
			vSkill >>= 1;
		}

		SetValue(1 << index, v);
	}

	public SkillName GetSkill(int index)
	{
		GetValues(index, out SkillName skill, out double _);

		return skill;
	}

	public void SetSkill(int index, SkillName skill)
	{
		SetValues(index, skill, GetBonus(index));
	}

	public double GetBonus(int index)
	{
		GetValues(index, out SkillName _, out double bonus);

		return bonus;
	}

	public void SetBonus(int index, double bonus)
	{
		SetValues(index, GetSkill(index), bonus);
	}

	public override string ToString()
	{
		return "...";
	}

	public static void CheckCancelMorph(Mobile m)
	{
		if (m == null)
			return;

		AnimalFormContext acontext = AnimalForm.GetContext(m);
		TransformContext context = TransformationSpellHelper.GetContext(m);

		if (context != null)
		{
			if (context.Spell is Spell spell)
			{
				spell.GetCastSkills(out double minSkill, out double _);
				if (m.Skills[spell.CastSkill].Value < minSkill)
					TransformationSpellHelper.RemoveContext(m, context, true);
			}
		}
		if (acontext != null)
		{
			int i;
			for (i = 0; i < AnimalForm.Entries.Length; ++i)
				if (AnimalForm.Entries[i].Type == acontext.Type)
					break;
			if (m.Skills[SkillName.Ninjitsu].Value < AnimalForm.Entries[i].ReqSkill)
				AnimalForm.RemoveContext(m, true);
		}
		if (!m.CanBeginAction(typeof(PolymorphSpell)) && m.Skills[SkillName.Magery].Value < 66.1)
		{
			m.BodyMod = 0;
			m.HueMod = -1;
			m.NameMod = null;
			m.EndAction(typeof(PolymorphSpell));
			BaseEquipment.ValidateMobile(m);
		}
		if (!m.CanBeginAction(typeof(IncognitoSpell)) && m.Skills[SkillName.Magery].Value < 38.1)
		{
			if (m is PlayerMobile mobile)
				mobile.SetHairMods(-1, -1);
			m.BodyMod = 0;
			m.HueMod = -1;
			m.NameMod = null;
			m.EndAction(typeof(IncognitoSpell));
			BaseEquipment.ValidateMobile(m);
			BuffInfo.RemoveBuff(m, BuffIcon.Incognito);
		}
		return;
	}


	[CommandProperty(AccessLevel.GameMaster)]
	public double Skill_1_Value { get => GetBonus(0); set => SetBonus(0, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public SkillName Skill_1_Name { get => GetSkill(0); set => SetSkill(0, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public double Skill_2_Value { get => GetBonus(1); set => SetBonus(1, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public SkillName Skill_2_Name { get => GetSkill(1); set => SetSkill(1, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public double Skill_3_Value { get => GetBonus(2); set => SetBonus(2, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public SkillName Skill_3_Name { get => GetSkill(2); set => SetSkill(2, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public double Skill_4_Value { get => GetBonus(3); set => SetBonus(3, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public SkillName Skill_4_Name { get => GetSkill(3); set => SetSkill(3, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public double Skill_5_Value { get => GetBonus(4); set => SetBonus(4, value); }

	[CommandProperty(AccessLevel.GameMaster)]
	public SkillName Skill_5_Name { get => GetSkill(4); set => SetSkill(4, value); }
}

[Flags]
public enum SAAbsorptionAttribute
{
	EaterFire = 0x00000001,
	EaterCold = 0x00000002,
	EaterPoison = 0x00000004,
	EaterEnergy = 0x00000008,
	EaterKinetic = 0x00000010,
	EaterDamage = 0x00000020,
	ResonanceFire = 0x00000040,
	ResonanceCold = 0x00000080,
	ResonancePoison = 0x00000100,
	ResonanceEnergy = 0x00000200,
	ResonanceKinetic = 0x00000400,
	/*Soul Charge is wrong. 
	 * Do not use these types. 
	 * Use AosArmorAttribute type only.
	 * Fill these in with any new attributes.*/
	SoulChargeFire = 0x00000800,
	SoulChargeCold = 0x00001000,
	SoulChargePoison = 0x00002000,
	SoulChargeEnergy = 0x00004000,
	SoulChargeKinetic = 0x00008000,
	CastingFocus = 0x00010000
}

public sealed class SAAbsorptionAttributes : BaseAttributes
{
	public static bool IsValid(SAAbsorptionAttribute attribute)
	{
		return true;
	}

	public static int[] GetValues(Mobile m, params SAAbsorptionAttribute[] attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static int[] GetValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
	{
		return EnumerateValues(m, attributes).ToArray();
	}

	public static IEnumerable<int> EnumerateValues(Mobile m, IEnumerable<SAAbsorptionAttribute> attributes)
	{
		return attributes.Select(a => GetValue(m, a));
	}

	public static int GetValue(Mobile m, SAAbsorptionAttribute attribute)
	{
		if (World.Loading || !IsValid(attribute))
		{
			return 0;
		}

		int value = 0;

		value += Enhancement.GetValue(m, attribute);

		for (int i = 0; i < m.Items.Count; ++i)
		{
			SAAbsorptionAttributes attrs = RunicReforging.GetSaAbsorptionAttributes(m.Items[i]);

			if (attrs != null)
				value += attrs[attribute];
		}

		//value += SkillMasterySpell.GetAttributeBonus(m, attribute);

		return value;
	}

	public SAAbsorptionAttributes(Item owner)
		: base(owner)
	{
	}

	public SAAbsorptionAttributes(Item owner, SAAbsorptionAttributes other)
		: base(owner, other)
	{
	}

	public SAAbsorptionAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public int this[SAAbsorptionAttribute attribute]
	{
		get => GetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterFire
	{
		get => this[SAAbsorptionAttribute.EaterFire];
		set => this[SAAbsorptionAttribute.EaterFire] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterCold
	{
		get => this[SAAbsorptionAttribute.EaterCold];
		set => this[SAAbsorptionAttribute.EaterCold] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterPoison
	{
		get => this[SAAbsorptionAttribute.EaterPoison];
		set => this[SAAbsorptionAttribute.EaterPoison] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterEnergy
	{
		get => this[SAAbsorptionAttribute.EaterEnergy];
		set => this[SAAbsorptionAttribute.EaterEnergy] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterKinetic
	{
		get => this[SAAbsorptionAttribute.EaterKinetic];
		set => this[SAAbsorptionAttribute.EaterKinetic] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int EaterDamage
	{
		get => this[SAAbsorptionAttribute.EaterDamage];
		set => this[SAAbsorptionAttribute.EaterDamage] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResonanceFire
	{
		get => this[SAAbsorptionAttribute.ResonanceFire];
		set => this[SAAbsorptionAttribute.ResonanceFire] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResonanceCold
	{
		get => this[SAAbsorptionAttribute.ResonanceCold];
		set => this[SAAbsorptionAttribute.ResonanceCold] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResonancePoison
	{
		get => this[SAAbsorptionAttribute.ResonancePoison];
		set => this[SAAbsorptionAttribute.ResonancePoison] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResonanceEnergy
	{
		get => this[SAAbsorptionAttribute.ResonanceEnergy];
		set => this[SAAbsorptionAttribute.ResonanceEnergy] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int ResonanceKinetic
	{
		get => this[SAAbsorptionAttribute.ResonanceKinetic];
		set => this[SAAbsorptionAttribute.ResonanceKinetic] = value;
	}

	//[CommandProperty(AccessLevel.GameMaster)]
	public int SoulChargeFire
	{
		get => this[SAAbsorptionAttribute.SoulChargeFire];
		set
		{
			//this[SAAbsorptionAttribute.SoulChargeFire] = value;
		}
	}

	//[CommandProperty(AccessLevel.GameMaster)]
	public int SoulChargeCold
	{
		get => this[SAAbsorptionAttribute.SoulChargeCold];
		set
		{
			//this[SAAbsorptionAttribute.SoulChargeCold] = value;
		}
	}

	//[CommandProperty(AccessLevel.GameMaster)]
	public int SoulChargePoison
	{
		get => this[SAAbsorptionAttribute.SoulChargePoison];
		set
		{
			//this[SAAbsorptionAttribute.SoulChargePoison] = value;
		}
	}

	//[CommandProperty(AccessLevel.GameMaster)]
	public int SoulChargeEnergy
	{
		get => this[SAAbsorptionAttribute.SoulChargeEnergy];
		set
		{
			//this[SAAbsorptionAttribute.SoulChargeEnergy] = value;
		}
	}

	//[CommandProperty(AccessLevel.GameMaster)]
	public int SoulChargeKinetic
	{
		get => this[SAAbsorptionAttribute.SoulChargeKinetic];
		set
		{
			//this[SAAbsorptionAttribute.SoulChargeKinetic] = value;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int CastingFocus
	{
		get => this[SAAbsorptionAttribute.CastingFocus];
		set => this[SAAbsorptionAttribute.CastingFocus] = value;
	}
}

[Flags]
public enum AosElementAttribute
{
	Physical = 0x00000001,
	Fire = 0x00000002,
	Cold = 0x00000004,
	Poison = 0x00000008,
	Energy = 0x00000010,
	Chaos = 0x00000020,
	Direct = 0x00000040
}

public sealed class AosElementAttributes : BaseAttributes
{
	public AosElementAttributes(Item owner)
		: base(owner)
	{
	}

	public AosElementAttributes(Item owner, AosElementAttributes other)
		: base(owner, other)
	{
	}

	public AosElementAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public int this[AosElementAttribute attribute]
	{
		get => GetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Physical { get => this[AosElementAttribute.Physical]; set => this[AosElementAttribute.Physical] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Fire { get => this[AosElementAttribute.Fire]; set => this[AosElementAttribute.Fire] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Cold { get => this[AosElementAttribute.Cold]; set => this[AosElementAttribute.Cold] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Poison { get => this[AosElementAttribute.Poison]; set => this[AosElementAttribute.Poison] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Energy { get => this[AosElementAttribute.Energy]; set => this[AosElementAttribute.Energy] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Chaos { get => this[AosElementAttribute.Chaos]; set => this[AosElementAttribute.Chaos] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public int Direct { get => this[AosElementAttribute.Direct]; set => this[AosElementAttribute.Direct] = value; }
}

[Flags]
public enum NegativeAttribute
{
	Brittle = 0x00000001,
	Prized = 0x00000002,
	Massive = 0x00000004,
	Unwieldly = 0x00000008,
	Antique = 0x00000010,
	NoRepair = 0x00000020
}

public sealed class NegativeAttributes : BaseAttributes
{
	public NegativeAttributes(Item owner)
		: base(owner)
	{
	}

	public NegativeAttributes(Item owner, NegativeAttributes other)
		: base(owner, other)
	{
	}

	public NegativeAttributes(Item owner, GenericReader reader)
		: base(owner, reader)
	{
	}

	public void GetProperties(ObjectPropertyList list, Item item)
	{
		if (NoRepair > 0)
			list.Add(1151782);

		if (Brittle > 0 ||
			item is BaseWeapon && ((BaseWeapon)item).Attributes.Brittle > 0 ||
			item is BaseArmor && ((BaseArmor)item).Attributes.Brittle > 0 ||
			item is BaseJewel && ((BaseJewel)item).Attributes.Brittle > 0 ||
			item is BaseClothing && ((BaseClothing)item).Attributes.Brittle > 0)
			list.Add(1116209);

		if (Prized > 0)
			list.Add(1154910);

		if (Antique > 0)
			list.Add(1076187);
	}

	public const double CombatDecayChance = 0.02;

	public static void OnCombatAction(Mobile m)
	{
		if (m == null || !m.Alive)
			return;

		List<Item> list = new List<Item>();

		foreach (Item item in m.Items.Where(i => i is IDurability))
		{
			NegativeAttributes attrs = RunicReforging.GetNegativeAttributes(item);

			if (attrs != null && attrs.Antique > 0 && CombatDecayChance > Utility.RandomDouble())
			{
				list.Add(item);
			}
		}

		foreach (Item item in list)
		{
			IDurability dur = item as IDurability;

			if (dur == null)
				continue;

			if (dur.HitPoints >= 1)
			{
				if (dur.HitPoints >= 4)
				{
					dur.HitPoints -= 4;
				}
				else
				{
					dur.HitPoints = 0;
				}
			}
			else
			{
				if (dur.MaxHitPoints > 1)
				{
					dur.MaxHitPoints--;

					if (item.Parent is Mobile)
						((Mobile)item.Parent).LocalOverheadMessage(Network.MessageType.Regular, 0x3B2, 1061121); // Your equipment is severely damaged.
				}
				else
				{
					item.Delete();
				}
			}
		}

		ColUtility.Free(list);
	}

	public int this[NegativeAttribute attribute]
	{
		get => GetValue((int)attribute);
		set => SetValue((int)attribute, value);
	}

	public override string ToString()
	{
		return "...";
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Brittle { get => this[NegativeAttribute.Brittle];
		set => this[NegativeAttribute.Brittle] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Prized { get => this[NegativeAttribute.Prized];
		set => this[NegativeAttribute.Prized] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Massive { get => this[NegativeAttribute.Massive];
		set => this[NegativeAttribute.Massive] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Unwieldly { get => this[NegativeAttribute.Unwieldly];
		set => this[NegativeAttribute.Unwieldly] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int Antique { get => this[NegativeAttribute.Antique];
		set => this[NegativeAttribute.Antique] = value;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int NoRepair { get => this[NegativeAttribute.NoRepair];
		set => this[NegativeAttribute.NoRepair] = value;
	}
}

[PropertyObject]
public abstract class BaseAttributes
{
	private uint m_Names;
	private int[] m_Values;

	private static readonly int[] m_Empty = Array.Empty<int>();

	public bool IsEmpty => m_Names == 0;
	public Item Owner { get; }

	public BaseAttributes(Item owner)
	{
		Owner = owner;
		m_Values = m_Empty;
	}

	public BaseAttributes(Item owner, BaseAttributes other)
	{
		Owner = owner;
		m_Values = new int[other.m_Values.Length];
		other.m_Values.CopyTo(m_Values, 0);
		m_Names = other.m_Names;
	}

	public BaseAttributes(Item owner, GenericReader reader)
	{
		Owner = owner;

		int version = reader.ReadByte();

		switch (version)
		{
			case 1:
			{
				m_Names = reader.ReadUInt();
				m_Values = new int[reader.ReadEncodedInt()];

				for (int i = 0; i < m_Values.Length; ++i)
					m_Values[i] = reader.ReadEncodedInt();

				break;
			}
			case 0:
			{
				m_Names = reader.ReadUInt();
				m_Values = new int[reader.ReadInt()];

				for (int i = 0; i < m_Values.Length; ++i)
					m_Values[i] = reader.ReadInt();

				break;
			}
		}
	}

	public void Serialize(GenericWriter writer)
	{
		writer.Write((byte)1); // version;

		writer.Write(m_Names);
		writer.WriteEncodedInt(m_Values.Length);

		for (int i = 0; i < m_Values.Length; ++i)
			writer.WriteEncodedInt(m_Values[i]);
	}

	public int GetValue(int bitmask)
	{
		if (!Core.AOS)
			return 0;

		uint mask = (uint)bitmask;

		if ((m_Names & mask) == 0)
			return 0;

		int index = GetIndex(mask);

		if (index >= 0 && index < m_Values.Length)
			return m_Values[index];

		return 0;
	}

	public virtual void SetValue(int bitmask, int value)
	{
		uint mask = (uint)bitmask;

		if (value != 0)
		{
			if ((m_Names & mask) != 0)
			{
				int index = GetIndex(mask);

				if (index >= 0 && index < m_Values.Length)
					m_Values[index] = value;
			}
			else
			{
				int index = GetIndex(mask);

				if (index >= 0 && index <= m_Values.Length)
				{
					int[] old = m_Values;
					m_Values = new int[old.Length + 1];

					for (int i = 0; i < index; ++i)
						m_Values[i] = old[i];

					m_Values[index] = value;

					for (int i = index; i < old.Length; ++i)
						m_Values[i + 1] = old[i];

					m_Names |= mask;
				}
			}
		}
		else if ((m_Names & mask) != 0)
		{
			int index = GetIndex(mask);

			if (index >= 0 && index < m_Values.Length)
			{
				m_Names &= ~mask;

				if (m_Values.Length == 1)
				{
					m_Values = m_Empty;
				}
				else
				{
					int[] old = m_Values;
					m_Values = new int[old.Length - 1];

					for (int i = 0; i < index; ++i)
						m_Values[i] = old[i];

					for (int i = index + 1; i < old.Length; ++i)
						m_Values[i - 1] = old[i];
				}
			}
		}

		if (bitmask == (int)AosWeaponAttribute.DurabilityBonus && this is AosWeaponAttributes)
		{
			if (Owner is BaseWeapon weapon)
				weapon.ScaleDurability();
		}
		else if (bitmask == (int)AosArmorAttribute.DurabilityBonus && this is AosArmorAttributes)
		{
			switch (Owner)
			{
				case BaseArmor armor:
					armor.ScaleDurability();
					break;
				case BaseClothing clothing:
					clothing.ScaleDurability();
					break;
			}
		}

		if (Owner.Parent is Mobile m)
		{
			m.CheckStatTimers();
			m.UpdateResistances();
			m.Delta(MobileDelta.Stat | MobileDelta.WeaponDamage | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);

			if (this is AosSkillBonuses bonuses)
			{
				bonuses.Remove();
				bonuses.AddTo(m);
			}
		}

		Owner.InvalidateProperties();
	}

	private int GetIndex(uint mask)
	{
		int index = 0;
		uint ourNames = m_Names;
		uint currentBit = 1;

		while (currentBit != mask)
		{
			if ((ourNames & currentBit) != 0)
				++index;

			if (currentBit == 0x80000000)
				return -1;

			currentBit <<= 1;
		}

		return index;
	}
}
