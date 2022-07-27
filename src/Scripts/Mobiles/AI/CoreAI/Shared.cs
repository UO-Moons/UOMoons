using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Chivalry;
using Server.Spells.Fourth;
using Server.Spells.Mysticism;
using Server.Spells.Second;

namespace Server.Mobiles
{
    public sealed partial class CoreAi
    {
	    private DateTime m_NextWeaponSwap;
	    private bool CanStun => m_Mobile is BaseVendor or BaseEscortable or BaseChampion;

	    private static bool IsFieldSpell(int id)
	    {
		    return id switch
		    {
			    //poison field
			    >= 14612 and <= 14633 => true,
			    //paralysis field
			    >= 14695 and <= 14730 => true,
			    //fire field
			    >= 14732 and <= 14751 => true,
			    _ => false
		    };
	    }

	    private bool TryToHeal()
        {
            if (m_Mobile.Summoned)
            {
                return false;
            }

            if (DateTime.UtcNow < m_NextHealTime)
            {
	            return false;
            }

            int diff = m_Mobile.HitsMax - m_Mobile.Hits;
            diff = m_Mobile.HitsMax * (100 - diff) / 100;
            diff = 100 - diff;

            if ((int)(Utility.RandomDouble() * 100.0) > diff)
            {
                return false;
            }

            Spell spell = null;
            m_NextHealTime = DateTime.UtcNow + TimeSpan.FromSeconds(20);

            if (CanUseMagery)
            {
                if (m_Mobile.Poisoned)
                {
                    _ = new CureSpell(m_Mobile, null);
                }

                spell = new GreaterHealSpell(m_Mobile, null);
            }
            else if (CanUseNecromancy)
            {
                m_Mobile.UseSkill(SkillName.SpiritSpeak);
                m_NextHealTime = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            }
            else if (CanUseChivalry)
            {
                if (m_Mobile.Poisoned)
                {
                    spell = new CleanseByFireSpell(m_Mobile, null);
                }
                else
                {
                    spell = new CloseWoundsSpell(m_Mobile, null);
                }
            }
            else if (CanUseMystic)
            {
                spell = new CleansingWindsSpell(m_Mobile, null);
            }
            else if (m_Mobile.Skills[SkillName.Healing].Value > 10.0)
            {
                int delay = (int)(5.0 + 0.5 * ((120 - m_Mobile.Dex) / 10));
                new BandageContext(m_Mobile, m_Mobile, TimeSpan.FromSeconds(delay), false);
                m_NextHealTime = DateTime.UtcNow + TimeSpan.FromSeconds(delay + 1);
                return true;
            }

            if (spell != null)
            {
                spell.Cast();
            }

            return true;
        }

	    private void CheckArmed(bool swap)
	    {
		    if (DateTime.UtcNow > m_NextWeaponSwap)
		    {
			    return;
		    }

		    if (!SwapWeapons)
		    {
			    return;
		    }

		    Container pack = m_Mobile.Backpack;

		    if (pack == null)
		    {
			    m_Mobile.EquipItem(new Backpack());
			    pack = m_Mobile.Backpack;
		    }

		    if (m_Mobile.Weapon is BaseWeapon weapon)
		    {
			    if (!swap)
			    {
				    return;
			    }

			    pack.DropItem(weapon);
		    }

		    m_Mobile.DebugSay("Searching my pack for a weapon.");

		    Item[] weapons = pack.FindItemsByType(typeof(BaseMeleeWeapon));

		    if (weapons != null && weapons.Length != 0)
		    {
			    int max = weapons.Length == 1 ? 0 : weapons.Length - 1;
			    int whichone = Utility.RandomMinMax(0, max);
			    m_Mobile.EquipItem(weapons[whichone]);
		    }

		    m_NextWeaponSwap = DateTime.UtcNow + TimeSpan.FromSeconds(15);
	    }

	    private void UseWeaponStrike()
        {
            m_Mobile.DebugSay("Picking a weapon move");


            if (m_Mobile.FindItemOnLayer(Layer.OneHanded) is not BaseWeapon weapon)
            {
                weapon = m_Mobile.FindItemOnLayer(Layer.TwoHanded) as BaseWeapon;
            }

            if (weapon == null)
            {
                return;
            }

            int whichone = Utility.RandomMinMax(1, 2);

            if (whichone >= 2 && m_Mobile.Skills[weapon.Skill].Value >= 90.0)
            {
                WeaponAbility.SetCurrentAbility(m_Mobile, weapon.PrimaryAbility);
            }
            else if (m_Mobile.Skills[weapon.Skill].Value >= 60.0)
            {
                WeaponAbility.SetCurrentAbility(m_Mobile, weapon.SecondaryAbility);
            }
            else if (m_Mobile.Skills[SkillName.Wrestling].Value >= 60.0 && /*weapon == Fist &&*/ CanStun && !m_Mobile.StunReady)
            {
                EventSink.InvokeStunRequest(m_Mobile);
            }
        }

	    private void CheckForFieldSpells()
        {
            if (!IsSmart)
            {
                return;
            }

            bool move = false;

            IPooledEnumerable eable = m_Mobile.Map.GetItemsInRange(m_Mobile.Location, 0);

            foreach (Item item in eable)
            {
	            if (item == null)
                {
                    continue;
                }

	            if (item.Z != m_Mobile.Z)
	            {
		            continue;
	            }

	            move = IsFieldSpell(item.ItemId);
            }
            eable.Free();

            if (!move)
	            return;

            switch( Utility.Random(9) )
            {
	            case 0: DoMove(Direction.Up); break;
	            case 1: DoMove(Direction.North); break;
	            case 2: DoMove(Direction.Left); break;
	            case 3: DoMove(Direction.West); break;
	            case 5: DoMove(Direction.Down); break;
	            case 6: DoMove(Direction.South); break;
	            case 7: DoMove(Direction.Right); break;
	            case 8: DoMove(Direction.East); break;
	            default: DoMove(m_Mobile.Direction); break;
            }
        }
    }
}
