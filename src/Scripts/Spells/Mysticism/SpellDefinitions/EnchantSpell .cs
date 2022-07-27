using System;
using Server.Gumps;
using Server.Items;
using System.Collections.Generic;
using Server.Spells.Spellweaving;

namespace Server.Spells.Mysticism;

public class EnchantSpell : MysticSpell
{
	private const string ModName = "EnchantAttribute";

	public override SpellCircle Circle => SpellCircle.Second;
	public override bool ClearHandsOnCast => false;

	private BaseWeapon Weapon { get; }
	private AosWeaponAttribute Attribute { get; }

	private static readonly SpellInfo m_Info = new(
		"Enchant", "In Ort Ylem",
		230,
		9022,
		Reagent.SpidersSilk,
		Reagent.MandrakeRoot,
		Reagent.SulfurousAsh
	);

	public EnchantSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public EnchantSpell(Mobile caster, Item scroll, BaseWeapon weapon, AosWeaponAttribute attribute) : base(caster, scroll, m_Info)
	{
		Weapon = weapon;
		Attribute = attribute;
	}

	public override bool CheckCast()
	{
		if (Weapon == null)
		{
			if (Caster.Weapon is not BaseWeapon wep)
			{
				Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
			}
			else
			{
				if (Caster.HasGump(typeof(EnchantSpellGump)))
				{
					Caster.CloseGump(typeof(EnchantSpellGump));
				}

				var gump = new EnchantSpellGump(Caster, Scroll, wep);
				int serial = gump.Serial;

				Caster.SendGump(gump);

				Timer.DelayCall(TimeSpan.FromSeconds(30), () =>
				{
					var current = Caster.FindGump(typeof(EnchantSpellGump));

					if (current == null || current.Serial != serial) return;
					Caster.CloseGump(typeof(EnchantSpellGump));
					FinishSequence();
				});
			}

			return false;
		}

		if (IsUnderSpellEffects(Caster, Weapon))
		{
			Caster.SendLocalizedMessage(501775); // This spell is already in effect.
			return false;
		}

		if (ImmolatingWeaponSpell.IsImmolating(Caster, Weapon) || Weapon.ConsecratedContext != null)
		{
			Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
			return false;
		}

		if (Weapon.FocusWeilder != null)
		{
			Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
			return false;
		}

		if (Weapon.WeaponAttributes.HitLightning <= 0 && Weapon.WeaponAttributes.HitFireball <= 0 &&
		    Weapon.WeaponAttributes.HitHarm <= 0 && Weapon.WeaponAttributes.HitMagicArrow <= 0 &&
		    Weapon.WeaponAttributes.HitDispel <= 0)
			return true;
		Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
		return false;

	}

	public override void OnCast()
	{
		if (Caster.Weapon is not BaseWeapon wep || wep != Weapon)
		{
			Caster.SendLocalizedMessage(501078); // You must be holding a weapon.
		}
		else if (IsUnderSpellEffects(Caster, Weapon))
		{
			Caster.SendLocalizedMessage(501775); // This spell is already in effect.
		}
		else if (ImmolatingWeaponSpell.IsImmolating(Caster, Weapon) || Weapon.ConsecratedContext != null)
		{
			Caster.SendLocalizedMessage(1080128); //You cannot use this ability while your weapon is enchanted.
		}
		else if (Weapon.FocusWeilder != null)
		{
			Caster.SendLocalizedMessage(1080446); // You cannot enchant an item that is under the effects of the ninjitsu focus attack ability.
		}
		else if (Weapon.WeaponAttributes.HitLightning > 0 || Weapon.WeaponAttributes.HitFireball > 0 || Weapon.WeaponAttributes.HitHarm > 0 || Weapon.WeaponAttributes.HitMagicArrow > 0 || Weapon.WeaponAttributes.HitDispel > 0)
		{
			Caster.SendLocalizedMessage(1080127); // This weapon already has a hit spell effect and cannot be enchanted.
		}
		else if (CheckSequence() && Caster.Weapon == Weapon)
		{
			Caster.PlaySound(0x64E);
			Caster.FixedEffect(0x36CB, 1, 9, 1915, 0);

			int prim = (int)Caster.Skills[CastSkill].Value;
			int sec = (int)Caster.Skills[DamageSkill].Value;

			int value = (60 * (prim + sec)) / 240;
			double duration = ((prim + sec) / 2.0) + 30.0;
			int malus = 0;

			Table ??= new Dictionary<Mobile, EnchantmentTimer>();

			Enhancement.SetValue(Caster, Attribute, value, ModName);

			if (prim >= 80 && sec >= 80 && Weapon.Attributes.SpellChanneling == 0)
			{
				Enhancement.SetValue(Caster, AosAttribute.SpellChanneling, 1, ModName);
				Enhancement.SetValue(Caster, AosAttribute.CastSpeed, -1, ModName);
				malus = 1;
			}

			Table[Caster] = new EnchantmentTimer(Caster, Weapon, Attribute, value, malus, duration);

			int loc = Attribute switch
			{
				AosWeaponAttribute.HitLightning => 1060423,
				AosWeaponAttribute.HitFireball => 1060420,
				AosWeaponAttribute.HitHarm => 1060421,
				AosWeaponAttribute.HitMagicArrow => 1060426,
				AosWeaponAttribute.HitDispel => 1060417,
				_ => 1060423
			};

			BuffInfo.AddBuff(Caster, new BuffInfo(BuffIcon.Enchant, 1080126, loc, TimeSpan.FromSeconds(duration), Caster, value.ToString()));

			Weapon.EnchantedWeilder = Caster;
			Weapon.InvalidateProperties();
		}

		FinishSequence();
	}

	private static Dictionary<Mobile, EnchantmentTimer> Table { get; set; }

	public static bool IsUnderSpellEffects(Mobile caster, BaseWeapon wep)
	{
		if (Table == null)
			return false;

		return Table.ContainsKey(caster) && Table[caster].Weapon == wep;
	}

	public static AosWeaponAttribute BonusAttribute(Mobile from)
	{
		return Table.ContainsKey(from) ? Table[from].WeaponAttribute : AosWeaponAttribute.HitColdArea;
	}

	public static int BonusValue(Mobile from)
	{
		return Table.ContainsKey(from) ? Table[from].AttributeValue : 0;
	}

	public static bool CastingMalus(Mobile from)
	{
		if (Table.ContainsKey(from))
			return Table[from].CastingMalus > 0;

		return false;
	}

	public static void RemoveEnchantment(Mobile caster)
	{
		if (Table == null || !Table.ContainsKey(caster))
			return;

		Table[caster].Stop();
		Table[caster] = null;
		Table.Remove(caster);

		caster.SendLocalizedMessage(1115273); // The enchantment on your weapon has expired.
		caster.PlaySound(0x1E6);

		Enhancement.RemoveMobile(caster);
	}

	public static void OnWeaponRemoved(BaseWeapon wep, Mobile from)
	{
		if (IsUnderSpellEffects(from, wep))
			RemoveEnchantment(from);

		if (wep.EnchantedWeilder != null)
			wep.EnchantedWeilder = null;
	}
}

public class EnchantmentTimer : Timer
{
	private Mobile Owner { get; }
	public BaseWeapon Weapon { get; }
	public AosWeaponAttribute WeaponAttribute { get; }
	public int AttributeValue { get; }
	public int CastingMalus { get; }

	public EnchantmentTimer(Mobile owner, BaseWeapon wep, AosWeaponAttribute attribute, int value, int malus, double duration) : base(TimeSpan.FromSeconds(duration))
	{
		Owner = owner;
		Weapon = wep;
		WeaponAttribute = attribute;
		AttributeValue = value;
		CastingMalus = malus;

		Start();
	}

	protected override void OnTick()
	{
		if (Weapon != null)
			Weapon.EnchantedWeilder = null;

		EnchantSpell.RemoveEnchantment(Owner);
	}
}
