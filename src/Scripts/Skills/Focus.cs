using Server.Mobiles;
using System.Collections.Generic;

namespace Server.Items;

public class Focus
{
	private static readonly Dictionary<Mobile, FocusInfo> MTable = new();
	private const int DefaultDamageBonus = -40;

	public static void Initialize()
	{
		EventSink.OnLogin += OnLogin;
	}

	public class FocusInfo
	{
		public Mobile Target { get; set; }
		public int DamageBonus { get; set; }

		public FocusInfo(Mobile defender, int bonus)
		{
			Target = defender;
			DamageBonus = bonus;
		}
	}

	public static void OnLogin(Mobile m)
	{
		if (m is PlayerMobile pm)
		{
			UpdateBuff(pm);
		}
	}

	public static void UpdateBuff(Mobile from, Mobile target = null)
	{
		var item = from.FindItemOnLayer(Layer.TwoHanded);

		if (item == null)
		{
			item = from.FindItemOnLayer(Layer.OneHanded);
		}

		if (item == null)
		{
			if (MTable.ContainsKey(from))
			{
				MTable.Remove(from);
				BuffInfo.RemoveBuff(from, BuffIcon.RageFocusingBuff);
			}
		}
		else if (item is BaseWeapon /*&& ((BaseWeapon)item).ExtendedWeaponAttributes.Focus > 0*/)
		{
			if (MTable.ContainsKey(from))
			{
				FocusInfo info = MTable[from];

				BuffInfo.AddBuff(from, new BuffInfo(BuffIcon.RageFocusingBuff, 1151393, 1151394,
					$"{(info.Target == null ? "NONE" : info.Target.Name)}\t{info.DamageBonus}"));
			}

			MTable[from] = new FocusInfo(target, DefaultDamageBonus);
		}
	}

	public static int GetBonus(Mobile from, Mobile target)
	{
		if (MTable.ContainsKey(from))
		{
			FocusInfo info = MTable[from];

			if (info.Target == target)
			{
				return info.DamageBonus;
			}
		}

		return 0;
	}

	public static void OnHit(Mobile attacker, Mobile defender)
	{
		if (MTable.ContainsKey(attacker))
		{
			FocusInfo info = MTable[attacker];

			if (info.Target == null)
			{
				info.DamageBonus -= 10;
			}
			else if (info.Target == defender)
			{
				if (info.DamageBonus < -40)
					info.DamageBonus += 10;
				else
					info.DamageBonus += 8;
			}
			else
			{
				if (info.DamageBonus >= -50)
					info.DamageBonus = DefaultDamageBonus;
			}

			if (info.Target != defender)
				info.Target = defender;

			UpdateBuff(attacker, defender);
		}
	}
}
