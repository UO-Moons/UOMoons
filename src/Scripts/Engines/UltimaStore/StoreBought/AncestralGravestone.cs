using Server.Multis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Items;

[Flipable(0x1173, 0x1174)]
public class AncestralGravestone : BaseItem
{
	public override int LabelNumber => 1071096;  // Ancestral Gravestone

	[Constructable]
	public AncestralGravestone()
		: base(0x1173)
	{
		LootType = LootType.Blessed;
	}

	public override void OnDoubleClick(Mobile m)
	{
		BaseHouse house = BaseHouse.FindHouseAt(this);

		if (house != null && house.IsFriend(m) && (IsLockedDown || IsSecure))
		{
			if (IsInCooldown(m))
			{
				TimeSpan tsRem = _cooldown[m] - DateTime.UtcNow;

				m.SendLocalizedMessage(1071505, ((int)tsRem.TotalMinutes).ToString()); // In order to get a buff again, you have to wait for at least ~1_VAL~ minutes.
			}
			else
			{
				AddBonus(m);
				m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
			}
		}
	}

	private static Dictionary<Mobile, SkillMod> _table;
	private static Dictionary<Mobile, DateTime> _cooldown;
	private static Timer _timer;

	public static bool UnderEffects(Mobile m)
	{
		return _table != null && _table.ContainsKey(m);
	}

	public static void AddBonus(Mobile m)
	{
		_table ??= new Dictionary<Mobile, SkillMod>();

		DefaultSkillMod mod = new(SkillName.SpiritSpeak, true, 5.0);
		_table[m] = mod;

		m.AddSkillMod(mod);
		AddToCooldown(m);

		Timer.DelayCall(TimeSpan.FromMinutes(Utility.RandomMinMax(5, 40)), ExpireBonus, new object[] { m, mod });
	}

	public static void ExpireBonus(object o)
	{
		object[] objects = (object[])o;
		SkillMod sm = objects[1] as SkillMod;

		if (objects[0] is Mobile mob) mob.RemoveSkillMod(sm);
	}

	public static bool IsInCooldown(Mobile m)
	{
		if (UnderEffects(m))
		{
			return true;
		}

		CheckCooldown();

		return _cooldown != null && _cooldown.ContainsKey(m);
	}

	public static void AddToCooldown(Mobile m)
	{
		_cooldown ??= new Dictionary<Mobile, DateTime>();

		_cooldown[m] = DateTime.UtcNow + TimeSpan.FromMinutes(90);

		if (_timer != null)
		{
			_timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), CheckCooldown);
			_timer.Priority = TimerPriority.FiveSeconds;
		}
	}

	public static void CheckCooldown()
	{
		if (_cooldown == null)
			return;

		List<Mobile> list = new(_cooldown.Keys);

		foreach (var m in list.Where(m => _cooldown[m] < DateTime.UtcNow))
		{
			_cooldown.Remove(m);
		}

		if (_cooldown.Count == 0)
		{
			_cooldown = null;

			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}
		}

		ColUtility.Free(list);
	}

	public AncestralGravestone(Serial serial)
		: base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.WriteEncodedInt(0); // version
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		reader.ReadEncodedInt();
	}
}
