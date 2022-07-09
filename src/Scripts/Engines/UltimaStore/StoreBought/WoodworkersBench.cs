using Server.Gumps;
using System;
using System.Collections.Generic;

namespace Server.Items;

public class WoodworkersBench : BaseAddon
{
	public override BaseAddonDeed Deed => new WoodworkersBenchDeed();

	[Constructable]
	public WoodworkersBench()
		: this(true)
	{
	}

	[Constructable]
	public WoodworkersBench(bool east)
	{
		if (east)
		{
			AddComponent(new AddonComponent(0x19F2), 0, 0, 0);
			AddComponent(new AddonComponent(0x19F1), 0, 1, 0);
			AddComponent(new AddonComponent(0x19F3), 0, -1, 0);
		}
		else
		{
			AddComponent(new AddonComponent(0x19F6), 0, 0, 0);
			AddComponent(new AddonComponent(0x19F5), 1, 0, 0);
			AddComponent(new AddonComponent(0x19F7), -1, 0, 0);
		}
	}

	public override void OnComponentUsed(AddonComponent c, Mobile m)
	{
		if (IsInCooldown(m))
		{
			TimeSpan tsRem = _table[m].Item2 + TimeSpan.FromMinutes(BonusDuration + Cooldown) - DateTime.UtcNow;

			m.SendLocalizedMessage(1071505, tsRem.Minutes.ToString()); // In order to get a buff again, you have to wait for at least ~1_VAL~ minutes.
		}
		else if (HasBonus(m))
		{
			TimeSpan tsRem = _table[m].Item2 + TimeSpan.FromMinutes(BonusDuration) - DateTime.UtcNow;

			m.SendLocalizedMessage(1071522, $"{tsRem.Minutes.ToString()}\t{tsRem.Seconds.ToString()}"); // You already have the bench's buff. Time remaining of the buff: ~1_VAL~ min ~2_VAL~ sec
		}
		else
		{
			m.SendLocalizedMessage(1071504); // Carpentry will go smoothly now...

			AddBonus(m);
			m.FixedParticles(0x376A, 9, 32, 5030, EffectLayer.Waist);
		}
	}

	private static Dictionary<Mobile, Tuple<bool, DateTime, SkillMod>> _table;
	private static Timer _timer;

	public const int Cooldown = 45;
	public const int BonusDuration = 30;

	public static void AddBonus(Mobile m)
	{
		if (_table == null)
		{
			_table = new Dictionary<Mobile, Tuple<bool, DateTime, SkillMod>>();
		}

		DefaultSkillMod mod = new(SkillName.Carpentry, true, 5.0)
		{
			ObeyCap = false
		};
		m.AddSkillMod(mod);

		_table[m] = new Tuple<bool, DateTime, SkillMod>(true, DateTime.UtcNow, mod);

		if (_timer == null)
		{
			_timer = Timer.DelayCall(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), CheckTable);
			_timer.Priority = TimerPriority.FiveSeconds;
			_timer.Start();
		}
	}

	public static bool HasBonus(Mobile m, SkillName skill)
	{
		return skill == SkillName.Carpentry && HasBonus(m);
	}

	public static bool HasBonus(Mobile m)
	{
		if (_table == null)
			return false;

		return _table.ContainsKey(m) && _table[m].Item1;
	}

	public static bool IsInCooldown(Mobile m)
	{
		if (m.AccessLevel > AccessLevel.Player || _table == null || !_table.ContainsKey(m))
			return false;

		if (_table[m].Item2 + TimeSpan.FromMinutes(BonusDuration + Cooldown) < DateTime.UtcNow)
		{
			_table.Remove(m);
			return false;
		}

		return _table[m].Item2 + TimeSpan.FromMinutes(BonusDuration) < DateTime.UtcNow;
	}

	public static void CheckTable()
	{
		if (_table == null)
			return;

		List<Mobile> list = new(_table.Keys);

		foreach (Mobile m in list)
		{
			if (_table[m].Item2 + TimeSpan.FromMinutes(BonusDuration + Cooldown) < DateTime.UtcNow)
			{
				_table.Remove(m);
			}
			else if (_table[m].Item1 && _table[m].Item2 + TimeSpan.FromMinutes(BonusDuration) < DateTime.UtcNow)
			{
				m.RemoveSkillMod(_table[m].Item3);
				DateTime dt = _table[m].Item2;

				if (m.NetState != null)
				{
					m.SendLocalizedMessage(1071506); // Your carpentry bonus was removed...
				}

				_table[m] = new Tuple<bool, DateTime, SkillMod>(false, dt, null);
			}
		}

		if (_table.Count == 0)
		{
			_table = null;

			if (_timer != null)
			{
				_timer.Stop();
				_timer = null;
			}
		}

		ColUtility.Free(list);
	}

	public WoodworkersBench(Serial serial)
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

public class WoodworkersBenchDeed : BaseAddonDeed, IRewardOption
{
	public override int LabelNumber => 1026641;  // Woodworker's Bench
	public override BaseAddon Addon => new WoodworkersBench(_east);

	private bool _east;

	[Constructable]
	public WoodworkersBenchDeed()
	{
		LootType = LootType.Blessed;
	}

	public WoodworkersBenchDeed(Serial serial)
		: base(serial)
	{
	}

	public void GetOptions(RewardOptionList list)
	{
		list.Add(0, 1071502); // Woodworker's Bench (South)
		list.Add(1, 1071503); // Woodworker's Bench (East)
	}

	public void OnOptionSelected(Mobile from, int choice)
	{
		_east = choice == 1;

		if (!Deleted)
			base.OnDoubleClick(from);
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (IsChildOf(from.Backpack))
		{
			from.CloseGump(typeof(RewardOptionGump));
			from.SendGump(new RewardOptionGump(this));
		}
		else
			from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.       	
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
