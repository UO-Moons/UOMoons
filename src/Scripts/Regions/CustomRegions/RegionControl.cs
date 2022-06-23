using Server.Gumps;
using Server.Regions;
using Server.Spells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static Server.Region;

namespace Server.Items;

[Flags]
public enum RegionFlag : uint
{
	None = 0x00000000,
	AllowBenefitPlayer = 0x00000001,
	AllowHarmPlayer = 0x00000002,
	AllowHousing = 0x00000004,
	AllowSpawn = 0x00000008,

	CanBeDamaged = 0x00000010,
	CanHeal = 0x00000020,
	CanRessurect = 0x00000040,
	CanUseStuckMenu = 0x00000080,
	ItemDecay = 0x00000100,

	ShowEnterMessage = 0x00000200,
	ShowExitMessage = 0x00000400,

	AllowBenefitNpc = 0x00000800,
	AllowHarmNpc = 0x00001000,

	CanMountEthereal = 0x000002000,
	// ToDo: Change to "CanEnter"
	CanEnter = 0x000004000,

	CanLootPlayerCorpse = 0x000008000,
	CanLootNpcCorpse = 0x000010000,
	// ToDo: Change to "CanLootOwnCorpse"
	CanLootOwnCorpse = 0x000020000,

	CanUsePotions = 0x000040000,

	IsGuarded = 0x000080000,

	// Obsolete, needed for old versions for DeSer.
	NoPlayerCorpses = 0x000100000,
	NoItemDrop = 0x000200000,
	//

	EmptyNpcCorpse = 0x000400000,
	EmptyPlayerCorpse = 0x000800000,
	DeleteNpcCorpse = 0x001000000,
	DeletePlayerCorpse = 0x002000000,
	ResNpcOnDeath = 0x004000000,
	ResPlayerOnDeath = 0x008000000,
	MoveNpcOnDeath = 0x010000000,
	MovePlayerOnDeath = 0x020000000,

	NoPlayerItemDrop = 0x040000000,
	NoNpcItemDrop = 0x080000000

	// CancelTargetOnEnter = 0x100000000,
	//AllowBallOfSummoning = 0x200000000,
	//AllowBraceletOfBinding = 0x400000000
}

public class RegionControl : Item
{
	public static List<RegionControl> AllControls { get; private set; } = new();


	#region Region Flags

	public RegionFlag Flags { get; set; }

	public bool GetFlag(RegionFlag flag)
	{
		return (Flags & flag) != 0;
	}

	public void SetFlag(RegionFlag flag, bool value)
	{
		if (value)
			Flags |= flag;
		else
		{
			Flags &= ~flag;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowBenefitPlayer
	{
		get => GetFlag(RegionFlag.AllowBenefitPlayer);
		set => SetFlag(RegionFlag.AllowBenefitPlayer, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowHarmPlayer
	{
		get => GetFlag(RegionFlag.AllowHarmPlayer);
		set => SetFlag(RegionFlag.AllowHarmPlayer, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowHousing
	{
		get => GetFlag(RegionFlag.AllowHousing);
		set => SetFlag(RegionFlag.AllowHousing, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowSpawn
	{
		get => GetFlag(RegionFlag.AllowSpawn);
		set => SetFlag(RegionFlag.AllowSpawn, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanBeDamaged
	{
		get => GetFlag(RegionFlag.CanBeDamaged);
		set => SetFlag(RegionFlag.CanBeDamaged, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanMountEthereal
	{
		get => GetFlag(RegionFlag.CanMountEthereal);
		set => SetFlag(RegionFlag.CanMountEthereal, value);
	}

	// ToDo: Change to "CanEnter"
	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanEnter
	{
		get => GetFlag(RegionFlag.CanEnter);
		set => SetFlag(RegionFlag.CanEnter, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanHeal
	{
		get => GetFlag(RegionFlag.CanHeal);
		set => SetFlag(RegionFlag.CanHeal, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanRessurect
	{
		get => GetFlag(RegionFlag.CanRessurect);
		set => SetFlag(RegionFlag.CanRessurect, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanUseStuckMenu
	{
		get => GetFlag(RegionFlag.CanUseStuckMenu);
		set => SetFlag(RegionFlag.CanUseStuckMenu, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ItemDecay
	{
		get => GetFlag(RegionFlag.ItemDecay);
		set => SetFlag(RegionFlag.ItemDecay, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowBenefitNpc
	{
		get => GetFlag(RegionFlag.AllowBenefitNpc);
		set => SetFlag(RegionFlag.AllowBenefitNpc, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool AllowHarmNpc
	{
		get => GetFlag(RegionFlag.AllowHarmNpc);
		set => SetFlag(RegionFlag.AllowHarmNpc, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ShowEnterMessage
	{
		get => GetFlag(RegionFlag.ShowEnterMessage);
		set => SetFlag(RegionFlag.ShowEnterMessage, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ShowExitMessage
	{
		get => GetFlag(RegionFlag.ShowExitMessage);
		set => SetFlag(RegionFlag.ShowExitMessage, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanLootPlayerCorpse
	{
		get => GetFlag(RegionFlag.CanLootPlayerCorpse);
		set => SetFlag(RegionFlag.CanLootPlayerCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanLootNpcCorpse
	{
		get => GetFlag(RegionFlag.CanLootNpcCorpse);
		set => SetFlag(RegionFlag.CanLootNpcCorpse, value);
	}

	// ToDo: Change to "CanLootOwnCorpse"
	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanLootOwnCorpse
	{
		get => GetFlag(RegionFlag.CanLootOwnCorpse);
		set => SetFlag(RegionFlag.CanLootOwnCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool CanUsePotions
	{
		get => GetFlag(RegionFlag.CanUsePotions);
		set => SetFlag(RegionFlag.CanUsePotions, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsGuarded
	{
		get => GetFlag(RegionFlag.IsGuarded);
		set
		{
			SetFlag(RegionFlag.IsGuarded, value);
			if (MRegion != null)
				MRegion.Disabled = !value;

			Timer.DelayCall(TimeSpan.FromSeconds(2.0), new TimerCallback(UpdateRegion));
		}
	}

	// OBSOLETE, needed for old Deser
	public bool NoPlayerCorpses
	{
		get => GetFlag(RegionFlag.NoPlayerCorpses);
		set => SetFlag(RegionFlag.NoPlayerCorpses, value);
	}

	public bool NoItemDrop
	{
		get => GetFlag(RegionFlag.NoItemDrop);
		set => SetFlag(RegionFlag.NoItemDrop, value);
	}
	// END OBSOLETE

	[CommandProperty(AccessLevel.GameMaster)]
	public bool EmptyNpcCorpse
	{
		get => GetFlag(RegionFlag.EmptyNpcCorpse);
		set => SetFlag(RegionFlag.EmptyNpcCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool EmptyPlayerCorpse
	{
		get => GetFlag(RegionFlag.EmptyPlayerCorpse);
		set => SetFlag(RegionFlag.EmptyPlayerCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool DeleteNpcCorpse
	{
		get => GetFlag(RegionFlag.DeleteNpcCorpse);
		set => SetFlag(RegionFlag.DeleteNpcCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool DeletePlayerCorpse
	{
		get => GetFlag(RegionFlag.DeletePlayerCorpse);
		set => SetFlag(RegionFlag.DeletePlayerCorpse, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ResNpcOnDeath
	{
		get => GetFlag(RegionFlag.ResNpcOnDeath);
		set => SetFlag(RegionFlag.ResNpcOnDeath, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool ResPlayerOnDeath
	{
		get => GetFlag(RegionFlag.ResPlayerOnDeath);
		set => SetFlag(RegionFlag.ResPlayerOnDeath, value);
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool MoveNpcOnDeath
	{
		get => GetFlag(RegionFlag.MoveNpcOnDeath);
		set
		{
			if (MoveNpcToMap == null || MoveNpcToMap == Map.Internal || MoveNpcToLoc == Point3D.Zero)
				SetFlag(RegionFlag.MoveNpcOnDeath, false);
			else
				SetFlag(RegionFlag.MoveNpcOnDeath, value);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool MovePlayerOnDeath
	{
		get => GetFlag(RegionFlag.MovePlayerOnDeath);
		set
		{
			if (MovePlayerToMap == null || MovePlayerToMap == Map.Internal || MovePlayerToLoc == Point3D.Zero)
				SetFlag(RegionFlag.MovePlayerOnDeath, false);
			else
				SetFlag(RegionFlag.MovePlayerOnDeath, value);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public bool NoPlayerItemDrop
	{
		get => GetFlag(RegionFlag.NoPlayerItemDrop);
		set => SetFlag(RegionFlag.NoPlayerItemDrop, value);
	}


	[CommandProperty(AccessLevel.GameMaster)]
	public bool NoNpcItemDrop
	{
		get => GetFlag(RegionFlag.NoNpcItemDrop);
		set => SetFlag(RegionFlag.NoNpcItemDrop, value);
	}
	/*
    #region flags added by ttxman

    [CommandProperty(AccessLevel.GameMaster)]
    public bool CancelTargetOnEnter
    {
        get { return GetFlag(RegionFlag.CancelTargetOnEnter); }
        set { SetFlag(RegionFlag.CancelTargetOnEnter, value); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowBallOfSummoning
    {
        get { return GetFlag(RegionFlag.AllowBallOfSummoning); }
        set { SetFlag(RegionFlag.AllowBallOfSummoning, value); }
    }
    [CommandProperty(AccessLevel.GameMaster)]
    public bool AllowBraceletOfBinding
    {
        get { return GetFlag(RegionFlag.AllowBraceletOfBinding); }
        set { SetFlag(RegionFlag.AllowBraceletOfBinding, value); }
    }
    #endregion
     */

	#endregion


	#region Region Restrictions

	public BitArray RestrictedSpells { get; private set; }

	public BitArray RestrictedSkills { get; private set; }

	#endregion


	#region Region Related Objects

	protected CustomRegion MRegion;

	public CustomRegion Region => MRegion;

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle3D[] RegionArea { get; set; }

	#endregion


	#region Control Properties

	private bool _mActive = true;

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Active
	{
		get => _mActive;
		set
		{
			if (_mActive != value)
			{
				_mActive = value;
				UpdateRegion();
			}
		}

	}

	#endregion


	#region Region Properties

	private string _mRegionName;
	private int _mRegionPriority;
	private MusicName _mMusic;
	private TimeSpan _mPlayerLogoutDelay;
	private int _mLightLevel;

	private Map _mMoveNpcToMap;
	private Point3D _mMoveNpcToLoc;
	private Map _mMovePlayerToMap;
	private Point3D _mMovePlayerToLoc;

	[CommandProperty(AccessLevel.GameMaster)]
	public string RegionName
	{
		get => _mRegionName;
		set
		{
			if (Map != null && !RegionNameTaken(value))
				_mRegionName = value;
			else if (Map != null)
				Console.WriteLine("RegionName not changed for {0}, {1} already has a Region with the name of {2}", this, Map, value);
			else if (Map == null)
				Console.WriteLine("RegionName not changed for {0} to {1}, it's Map value was null", this, value);

			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int RegionPriority
	{
		get => _mRegionPriority;
		set
		{
			_mRegionPriority = value;
			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public MusicName Music
	{
		get => _mMusic;
		set
		{
			_mMusic = value;
			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public TimeSpan PlayerLogoutDelay
	{
		get => _mPlayerLogoutDelay;
		set
		{
			_mPlayerLogoutDelay = value;
			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public int LightLevel
	{
		get => _mLightLevel;
		set
		{
			_mLightLevel = value;
			UpdateRegion();
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Map MoveNpcToMap
	{
		get => _mMoveNpcToMap;
		set
		{
			if (value != Map.Internal)
				_mMoveNpcToMap = value;
			else
				SetFlag(RegionFlag.MoveNpcOnDeath, false);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D MoveNpcToLoc
	{
		get => _mMoveNpcToLoc;
		set
		{
			if (value != Point3D.Zero)
				_mMoveNpcToLoc = value;
			else
				SetFlag(RegionFlag.MoveNpcOnDeath, false);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Map MovePlayerToMap
	{
		get => _mMovePlayerToMap;
		set
		{
			if (value != Map.Internal)
				_mMovePlayerToMap = value;
			else
				SetFlag(RegionFlag.MovePlayerOnDeath, false);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D MovePlayerToLoc
	{
		get => _mMovePlayerToLoc;
		set
		{
			if (value != Point3D.Zero)
				_mMovePlayerToLoc = value;
			else
				SetFlag(RegionFlag.MovePlayerOnDeath, false);
		}
	}

	// REMOVED
	/*
    private Point3D m_CustomGoLocation;

    [CommandProperty(AccessLevel.GameMaster)]
    public Point3D CustomGoLocation
    {
        get { return m_Region.GoLocation; }
        set 
        { 
            m_Region.GoLocation = value;
            m_CustomGoLocation = value;
            UpdateRegion();           
        }
    }
     */

	#endregion


	[Constructable]
	public RegionControl() : base(5609)
	{
		Visible = false;
		Movable = false;
		Name = "Region Controller";

		if (AllControls == null)
			AllControls = new List<RegionControl>();
		AllControls.Add(this);

		_mRegionName = FindNewName("Custom Region");
		_mRegionPriority = DefaultPriority;

		RestrictedSpells = new BitArray(SpellRegistry.Types.Length);
		RestrictedSkills = new BitArray(SkillInfo.Table.Length);
	}

	[Constructable]
	public RegionControl(Rectangle2D rect) : base(5609)
	{
		Visible = false;
		Movable = false;
		Name = "Region Controller";

		AllControls ??= new List<RegionControl>();
		AllControls.Add(this);

		_mRegionName = FindNewName("Custom Region");
		_mRegionPriority = DefaultPriority;

		RestrictedSpells = new BitArray(SpellRegistry.Types.Length);
		RestrictedSkills = new BitArray(SkillInfo.Table.Length);

		Rectangle3D newrect = ConvertTo3D(rect);
		DoChooseArea(null, Map, newrect.Start, newrect.End, this);

		if (Region != null)
		{
			Region.GoLocation = new Point3D(0, 0, 0);
		}

		UpdateRegion();
	}

	[Constructable]
	public RegionControl(Rectangle3D rect) : base(5609)
	{
		Visible = false;
		Movable = false;
		Name = "Region Controller";

		AllControls ??= new List<RegionControl>();
		AllControls.Add(this);

		_mRegionName = FindNewName("Custom Region");
		_mRegionPriority = DefaultPriority;

		RestrictedSpells = new BitArray(SpellRegistry.Types.Length);
		RestrictedSkills = new BitArray(SkillInfo.Table.Length);

		DoChooseArea(null, Map, rect.Start, rect.End, this);

		if (Region != null)
		{
			Region.GoLocation = new Point3D(0, 0, 0);
		}

		UpdateRegion();
	}

	[Constructable]
	public RegionControl(Rectangle2D[] rects) : base(5609)
	{
		Visible = false;
		Movable = false;
		Name = "Region Controller";

		AllControls ??= new List<RegionControl>();
		AllControls.Add(this);

		_mRegionName = FindNewName("Custom Region");
		_mRegionPriority = DefaultPriority;

		RestrictedSpells = new BitArray(SpellRegistry.Types.Length);
		RestrictedSkills = new BitArray(SkillInfo.Table.Length);

		foreach (Rectangle2D rect2d in rects)
		{
			Rectangle3D newrect = ConvertTo3D(rect2d);
			DoChooseArea(null, Map, newrect.Start, newrect.End, this);
		}

		if (Region != null)
		{
			Region.GoLocation = new Point3D(0, 0, 0);
		}

		UpdateRegion();
	}

	[Constructable]
	public RegionControl(Rectangle3D[] rects) : base(5609)
	{
		Visible = false;
		Movable = false;
		Name = "Region Controller";

		AllControls ??= new List<RegionControl>();
		AllControls.Add(this);

		_mRegionName = FindNewName("Custom Region");
		_mRegionPriority = DefaultPriority;

		RestrictedSpells = new BitArray(SpellRegistry.Types.Length);
		RestrictedSkills = new BitArray(SkillInfo.Table.Length);

		foreach (Rectangle3D rect3d in rects)
		{
			DoChooseArea(null, Map, rect3d.Start, rect3d.End, this);
		}

		if (Region != null)
		{
			Region.GoLocation = new Point3D(0, 0, 0);
		}

		UpdateRegion();
	}

	public RegionControl(Serial serial) : base(serial)
	{
	}


	#region Control Special Voids

	public bool RegionNameTaken(string testName)
	{

		if (AllControls != null)
		{
			return AllControls.Any(control => control.RegionName == testName && control != this);
		}

		return false;
	}

	public string FindNewName(string oldName)
	{
		int i = 1;

		string newName = oldName;
		while (RegionNameTaken(newName))
		{
			newName = oldName;
			newName += $" {i}";
			i++;
		}

		return newName;
	}

	public virtual void UpdateRegion()
	{
		MRegion?.Unregister();

		if (Map != null && Active)
		{
			if (RegionArea is {Length: > 0})
			{
				MRegion = new CustomRegion(this);
				// m_Region.GoLocation = m_CustomGoLocation;  // REMOVED
				MRegion.Register();
			}
			else
				MRegion = null;
		}
		else
			MRegion = null;
	}

	public void RemoveArea(int index, Mobile from)
	{
		try
		{
			List<Rectangle3D> rects = RegionArea.ToList();

			rects.RemoveAt(index);
			RegionArea = rects.ToArray();

			UpdateRegion();
			from.SendMessage("Area Removed!");
		}
		catch
		{
			from.SendMessage("Removing of Area Failed!");
		}
	}
	public static int GetRegistryNumber(ISpell s)
	{
		Type[] t = SpellRegistry.Types;

		for (int i = 0; i < t.Length; i++)
		{
			if (s.GetType() == t[i])
				return i;
		}

		return -1;
	}


	public bool IsRestrictedSpell(ISpell s)
	{

		if (RestrictedSpells.Length != SpellRegistry.Types.Length)
		{

			RestrictedSpells = new BitArray(SpellRegistry.Types.Length);

			for (int i = 0; i < RestrictedSpells.Length; i++)
				RestrictedSpells[i] = false;

		}

		int regNum = GetRegistryNumber(s);


		if (regNum < 0) //Happens with unregistered Spells
			return false;

		return RestrictedSpells[regNum];
	}

	public bool IsRestrictedSkill(int skill)
	{
		if (RestrictedSkills.Length != SkillInfo.Table.Length)
		{

			RestrictedSkills = new BitArray(SkillInfo.Table.Length);

			for (int i = 0; i < RestrictedSkills.Length; i++)
				RestrictedSkills[i] = false;

		}

		if (skill < 0)
			return false;


		return RestrictedSkills[skill];
	}

	public void ChooseArea(Mobile m)
	{
		BoundingBoxPicker.Begin(m, CustomRegion_Callback, this);
	}

	public void CustomRegion_Callback(Mobile from, Map map, Point3D start, Point3D end, object state)
	{
		DoChooseArea(from, map, start, end, state);
	}

	public void DoChooseArea(Mobile from, Map map, Point3D start, Point3D end, object control)
	{
		List<Rectangle3D> areas = new();

		if (RegionArea != null)
		{
			areas.AddRange(RegionArea);
		}
		// Added Lord Dio's Z Value Fix
		if (start.Z == end.Z || start.Z < end.Z)
		{
			if (start.Z != MinZ)
				start.Z = MinZ;
			if (end.Z != MaxZ)
				end.Z = MaxZ;
		}
		else
		{
			if (start.Z != MaxZ)
				start.Z = MaxZ;
			if (end.Z != MinZ)
				end.Z = MinZ;
		}

		Rectangle3D newrect = new(start, end);
		areas.Add(newrect);

		RegionArea = areas.ToArray();

		UpdateRegion();
		// Added by nerun, so the RemoveAreaGump will be refreshed after added a new area
		from.CloseGump(typeof(RegionControlGump));
		from.SendGump(new RegionControlGump(this));
		from.CloseGump(typeof(RemoveAreaGump));
		from.SendGump(new RemoveAreaGump(this));
	}

	#endregion


	#region Control Overrides

	public override void OnDoubleClick(Mobile m)
	{
		if (m.AccessLevel >= AccessLevel.GameMaster)
		{
			if (RestrictedSpells.Length != SpellRegistry.Types.Length)
			{
				RestrictedSpells = new BitArray(SpellRegistry.Types.Length);

				for (int i = 0; i < RestrictedSpells.Length; i++)
					RestrictedSpells[i] = false;

				m.SendMessage("Resetting all restricted Spells due to Spell change");
			}

			if (RestrictedSkills.Length != SkillInfo.Table.Length)
			{

				RestrictedSkills = new BitArray(SkillInfo.Table.Length);

				for (int i = 0; i < RestrictedSkills.Length; i++)
					RestrictedSkills[i] = false;

				m.SendMessage("Resetting all restricted Skills due to Skill change");

			}

			m.CloseGump(typeof(RegionControlGump));
			m.SendGump(new RegionControlGump(this));
			m.SendMessage("Don't forget to props this object for more options!");
			m.CloseGump(typeof(RemoveAreaGump));
			m.SendGump(new RemoveAreaGump(this));
		}
	}

	public override void OnMapChange()
	{
		UpdateRegion();
		base.OnMapChange();
	}

	public override void OnDelete()
	{
		MRegion?.Unregister();

		AllControls?.Remove(this);

		base.OnDelete();
	}

	#endregion


	#region Ser/Deser Helpers

	public static void WriteBitArray(GenericWriter writer, BitArray ba)
	{
		writer.Write(ba.Length);

		for (int i = 0; i < ba.Length; i++)
		{
			writer.Write(ba[i]);
		}
	}

	public static BitArray ReadBitArray(GenericReader reader)
	{
		int size = reader.ReadInt();

		BitArray newBa = new(size);

		for (int i = 0; i < size; i++)
		{
			newBa[i] = reader.ReadBool();
		}

		return newBa;
	}


	public static void WriteRect3DArray(GenericWriter writer, Rectangle3D[] ary)
	{
		if (ary == null)
		{
			writer.Write(0);
			return;
		}

		writer.Write(ary.Length);

		for (int i = 0; i < ary.Length; i++)
		{
			Rectangle3D rect = ary[i];
			writer.Write(rect.Start);
			writer.Write(rect.End);
		}
	}

	public static List<Rectangle2D> ReadRect2DArray(GenericReader reader)
	{
		int size = reader.ReadInt();
		List<Rectangle2D> newAry = new();

		for (int i = 0; i < size; i++)
		{
			newAry.Add(reader.ReadRect2D());
		}

		return newAry;
	}

	public static Rectangle3D[] ReadRect3DArray(GenericReader reader)
	{
		int size = reader.ReadInt();
		List<Rectangle3D> newAry = new();

		for (int i = 0; i < size; i++)
		{
			Point3D start = reader.ReadPoint3D();
			Point3D end = reader.ReadPoint3D();
			newAry.Add(new Rectangle3D(start, end));
		}

		return newAry.ToArray();
	}

	#endregion


	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(5); // version

		// writer.Write((Point3D)CustomGoLocation);   // REMOVED

		WriteRect3DArray(writer, RegionArea);

		writer.Write((int)Flags);

		WriteBitArray(writer, RestrictedSpells);
		WriteBitArray(writer, RestrictedSkills);

		writer.Write(_mActive);

		writer.Write(_mRegionName);
		writer.Write(_mRegionPriority);
		writer.Write((int)_mMusic);
		writer.Write(_mPlayerLogoutDelay);
		writer.Write(_mLightLevel);

		writer.Write(_mMoveNpcToMap);
		writer.Write(_mMoveNpcToLoc);
		writer.Write(_mMovePlayerToMap);
		writer.Write(_mMovePlayerToLoc);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		// Point3D customGoLoc = new Point3D(0,0,0); // REMOVED
		switch (version)
		{
			case 5:
			{
				// customGoLoc = reader.ReadPoint3D(); // REMOVED
				goto case 4;
			}
			case 4:
			{
				RegionArea = ReadRect3DArray(reader);

				Flags = (RegionFlag)reader.ReadInt();

				RestrictedSpells = ReadBitArray(reader);
				RestrictedSkills = ReadBitArray(reader);

				_mActive = reader.ReadBool();

				_mRegionName = reader.ReadString();
				_mRegionPriority = reader.ReadInt();
				_mMusic = (MusicName)reader.ReadInt();
				_mPlayerLogoutDelay = reader.ReadTimeSpan();
				_mLightLevel = reader.ReadInt();

				_mMoveNpcToMap = reader.ReadMap();
				_mMoveNpcToLoc = reader.ReadPoint3D();
				_mMovePlayerToMap = reader.ReadMap();
				_mMovePlayerToLoc = reader.ReadPoint3D();

				break;
			}
			case 3:
			{
				_mLightLevel = reader.ReadInt();
				goto case 2;
			}
			case 2:
			{
				_mMusic = (MusicName)reader.ReadInt();
				goto case 1;
			}
			case 1:
			{
				List<Rectangle2D> rects2d = ReadRect2DArray(reader);
				foreach (Rectangle2D rect in rects2d)
				{
					Rectangle3D newrect = ConvertTo3D(rect);
					DoChooseArea(null, Map, newrect.Start, newrect.End, this);
				}

				_mRegionPriority = reader.ReadInt();
				_mPlayerLogoutDelay = reader.ReadTimeSpan();

				RestrictedSpells = ReadBitArray(reader);
				RestrictedSkills = ReadBitArray(reader);

				Flags = (RegionFlag)reader.ReadInt();
				if (NoPlayerCorpses)
				{
					DeleteNpcCorpse = true;
					DeletePlayerCorpse = true;
				}
				if (NoItemDrop)
				{
					NoPlayerItemDrop = true;
					NoNpcItemDrop = true;
				}
				// Invert because of change from "Cannot" to "Can"
				if (CanLootOwnCorpse)
				{
					CanLootOwnCorpse = false;
				}
				if (CanEnter)
				{
					CanEnter = false;
				}

				_mRegionName = reader.ReadString();
				break;
			}
			case 0:
			{
				List<Rectangle2D> rects2d = ReadRect2DArray(reader);
				foreach (Rectangle2D rect in rects2d)
				{
					Rectangle3D newrect = ConvertTo3D(rect);
					DoChooseArea(null, Map, newrect.Start, newrect.End, this);
				}

				RestrictedSpells = ReadBitArray(reader);
				RestrictedSkills = ReadBitArray(reader);

				Flags = (RegionFlag)reader.ReadInt();
				if (NoPlayerCorpses)
				{
					DeleteNpcCorpse = true;
					DeletePlayerCorpse = true;
				}
				if (NoItemDrop)
				{
					NoPlayerItemDrop = true;
					NoNpcItemDrop = true;
				}
				// Invert because of change from "Cannot" to "Can"
				if (CanLootOwnCorpse)
				{
					CanLootOwnCorpse = false;
				}
				if (CanEnter)
				{
					CanEnter = false;
				}

				_mRegionName = reader.ReadString();
				break;
			}
		}
		/*
        #region ttxman upravy kvuli tomu modu...
        if (version == 3)
        {
            if (GetFlag(RegionFlag.MoveNPCOnDeath))
            {
                SetFlag(RegionFlag.MoveNPCOnDeath, false);
                SetFlag(RegionFlag.CancelTargetOnEnter, true);
            }
            if (GetFlag(RegionFlag.MovePlayerOnDeath))
            {
                SetFlag(RegionFlag.MovePlayerOnDeath, false);
                SetFlag(RegionFlag.AllowBallOfSummoning, true);
            }
            if (GetFlag(RegionFlag.NoPlayerItemDrop))
            {
                SetFlag(RegionFlag.NoPlayerItemDrop, false);
                SetFlag(RegionFlag.AllowBraceletOfBinding, true);
            }

        }
        #endregion
         */

		AllControls.Add(this);

		if (RegionNameTaken(_mRegionName))
			_mRegionName = FindNewName(_mRegionName);

		UpdateRegion();
		// m_CustomGoLocation = customGoLoc;  // REMOVED
		// CustomGoLocation = customGoLoc;   // REMOVED
		UpdateRegion();
	}
}
