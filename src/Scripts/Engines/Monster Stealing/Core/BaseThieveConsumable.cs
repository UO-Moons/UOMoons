using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Items;

public enum ThieveConsumableEffect
{
	None,
	BalmOfStrengthEffect,
	BalmOfWisdomEffect,
	BalmOfSwiftnessEffect,
	BalmOfProtectionEffect,
	StoneSkinLotionEffect,
	LifeShieldLotionEffect,
}

public class ThieveConsumableInfo
{
	public readonly ThieveConsumableEffect Effect;
	public readonly Timer EffectTimer;

	public ThieveConsumableInfo(BaseThieveConsumable.InternalTimer t, ThieveConsumableEffect e)
	{
		Effect = e;
		EffectTimer = t;
	}
}

public abstract class BaseThieveConsumable : Item
{
	public BaseThieveConsumable(int itemId)
		: base(itemId)
	{
	}

	public class InternalTimer : Timer
	{
		private readonly PlayerMobile m_Pm;
		private readonly ThieveConsumableEffect m_Effect;

		protected override void OnTick()
		{
			RemoveEffect(m_Pm, m_Effect);
		}

		public InternalTimer(PlayerMobile p, ThieveConsumableEffect e, TimeSpan delay)
			: base(delay)
		{
			m_Pm = p;
			m_Effect = e;
		}
	}

	public TimeSpan EffectDuration;
	protected ThieveConsumableEffect EffectType;

	public override void OnDoubleClick(Mobile m)
	{
		if (m is PlayerMobile mobile && IsChildOf(mobile.Backpack))
		{
			OnUse(mobile);
		}
	}

	protected virtual void OnUse(PlayerMobile by)
	{
	}

	protected virtual void ApplyEffect(PlayerMobile pm)
	{

		if (EffectDuration == TimeSpan.Zero)
		{
			EffectDuration = TimeSpan.FromMinutes(30);
		}

		InternalTimer t = new(pm, EffectType, EffectDuration);
		t.Start();

		ThieveConsumableInfo info = new(t, EffectType);

		if (EffectTable.ContainsKey(pm))
		{
			RemoveEffect(pm, EffectTable[pm].Effect);
		}

		EffectTable.Add(pm, info);
		Consume();
	}

	private static void RemoveEffect(PlayerMobile pm, ThieveConsumableEffect effectType)
	{
		if (EffectTable.ContainsKey(pm))
		{

			EffectTable[pm].EffectTimer.Stop();
			EffectTable.Remove(pm);

			pm.SendLocalizedMessage(1095134);//The effects of the balm or lotion have worn off.

			if (effectType == ThieveConsumableEffect.BalmOfStrengthEffect || effectType == ThieveConsumableEffect.BalmOfSwiftnessEffect || effectType == ThieveConsumableEffect.BalmOfWisdomEffect)
			{
				pm.RemoveStatMod("Balm");
			}
			else if (effectType == ThieveConsumableEffect.StoneSkinLotionEffect)
			{

				List<ResistanceMod> list = pm.ResistanceMods;

				for (int i = 0; i < list.Count; i++)
				{
					ResistanceMod curr = list[i];
					if ((curr.Type != ResistanceType.Cold || curr.Offset != -5) &&
					    (curr.Type != ResistanceType.Fire || curr.Offset != -5) &&
					    (curr.Type != ResistanceType.Physical || curr.Offset != 30)) continue;
					list.RemoveAt(i);
					i--;
				}
			}
		}
	}

	private static readonly Dictionary<PlayerMobile, ThieveConsumableInfo> EffectTable = new();

	public static bool CanUse(PlayerMobile pm, BaseThieveConsumable consum)
	{
		return CheckThieveConsumable(pm) == ThieveConsumableEffect.None;
	}

	public static bool IsUnderThieveConsumableEffect(PlayerMobile pm, ThieveConsumableEffect eff)
	{
		return EffectTable.ContainsKey(pm) && EffectTable[pm].Effect == eff;
	}

	private static ThieveConsumableEffect CheckThieveConsumable(PlayerMobile pm)
	{
		return EffectTable.ContainsKey(pm) ? EffectTable[pm].Effect : ThieveConsumableEffect.None;
	}

	public BaseThieveConsumable(Serial serial)
		: base(serial)
	{
	}


	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0); // version

		writer.Write((int)EffectType);
		writer.Write(EffectDuration);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadInt();

		EffectType = (ThieveConsumableEffect)reader.ReadInt();
		EffectDuration = reader.ReadTimeSpan();
	}
}
