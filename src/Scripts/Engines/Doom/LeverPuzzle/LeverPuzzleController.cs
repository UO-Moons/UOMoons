using Server.Commands;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Engines.Doom;

public class LeverPuzzleController : BaseItem
{
	private static bool _installed;
	private List<Item> _mLevers;
	private List<Item> _mTeles;
	private List<Item> _mStatues;
	private List<LeverPuzzleRegion> _mTiles;
	private LampRoomBox _mBox;
	private Region _mLampRoom;

	private Timer _mTimer;
	private Timer _lTimer;

	public static void Initialize()
	{
		CommandSystem.Register("GenLeverPuzzle", AccessLevel.Administrator, GenLampPuzzle_OnCommand);
	}

	[Usage("GenLeverPuzzle")]
	[Description("Generates lamp room and lever puzzle in doom.")]
	public static void GenLampPuzzle_OnCommand(CommandEventArgs e)
	{
		if (Map.Malas.GetItemsInRange(LpCenter, 0).OfType<LeverPuzzleController>().Any())
		{
			e.Mobile.SendMessage("Lamp room puzzle already exists: please delete the existing controller first ...");
			return;
		}
		e.Mobile.SendMessage("Generating Lamp Room puzzle...");
		new LeverPuzzleController().MoveToWorld(LpCenter, Map.Malas);

		e.Mobile.SendMessage(!_installed
			? "There was a problem generating the puzzle."
			: "Lamp room puzzle successfully generated.");
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public ushort MyKey { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public ushort TheirKey { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Enabled { get; set; }

	public Mobile Successful { get; private set; }

	public bool CircleComplete
	{
		get /* OSI: all 5 must be occupied */
		{
			for (int i = 0; i < 5; i++)
			{
				if (GetOccupant(i) == null)
				{
					return false;
				}
			}
			return true;
		}
	}

	public LeverPuzzleController() : base(0x1822)
	{
		Movable = false;
		Hue = 0x4c;
		_installed = true;
		int i = 0;

		_mLevers = new List<Item>();    /* codes are 0x1 shifted left x # of bits, easily handled here */
		for (; i < 4; i++)
			_mLevers.Add(AddLeverPuzzlePart(Ta[i], new LeverPuzzleLever((ushort)(1 << i), this)));

		_mTiles = new List<LeverPuzzleRegion>();
		for (; i < 9; i++)
			_mTiles.Add(new LeverPuzzleRegion(this, Ta[i]));

		_mTeles = new List<Item>();
		for (; i < 15; i++)
			_mTeles.Add(AddLeverPuzzlePart(Ta[i], new LampRoomTeleporter(Ta[++i])));

		_mStatues = new List<Item>();
		for (; i < 19; i++)
			_mStatues.Add(AddLeverPuzzlePart(Ta[i], new LeverPuzzleStatue(Ta[++i], this)));

		if (!_installed)
			Delete();
		else
			Enabled = true;

		_mBox = (LampRoomBox)AddLeverPuzzlePart(Ta[i], new LampRoomBox(this));
		_mLampRoom = new LampRoomRegion(this);
		GenKey();
	}

	public static Item AddLeverPuzzlePart(int[] loc, Item newitem)
	{
		if (newitem == null || newitem.Deleted)
		{
			_installed = false;
		}
		else
		{
			newitem.MoveToWorld(new Point3D(loc[0], loc[1], loc[2]), Map.Malas);
		}
		return newitem;
	}

	public override void OnDelete()
	{
		KillTimers();
		base.OnDelete();
	}
	public override void OnAfterDelete()
	{
		NukeItemList(_mTeles);
		NukeItemList(_mStatues);
		NukeItemList(_mLevers);

		_mLampRoom?.Unregister();
		if (_mTiles != null)
		{
			foreach (LeverPuzzleRegion region in _mTiles)
			{
				region.Unregister();
			}
		}
		if (_mBox != null && !_mBox.Deleted)
		{
			_mBox.Delete();
		}
	}

	public static void NukeItemList(List<Item> list)
	{
		if (list == null || list.Count == 0) return;
		foreach (var item in list.Where(item => item != null && !item.Deleted))
		{
			item.Delete();
		}
	}

	public virtual PlayerMobile GetOccupant(int index)
	{
		LeverPuzzleRegion region = _mTiles[index];

		if (region?.Occupant != null && region.Occupant.Alive)
		{
			return (PlayerMobile)region.Occupant;
		}
		return null;
	}

	public virtual LeverPuzzleStatue GetStatue(int index)
	{
		LeverPuzzleStatue statue = (LeverPuzzleStatue)_mStatues[index];

		return statue is {Deleted: false} ? statue : null;
	}

	public virtual LeverPuzzleLever GetLever(int index)
	{
		LeverPuzzleLever lever = (LeverPuzzleLever)_mLevers[index];

		if (lever != null && !lever.Deleted)
		{
			return lever;
		}
		return null;
	}

	public virtual void PuzzleStatus(int message, string fstring)
	{
		for (var i = 0; i < 2; i++)
		{
			Item s;
			if ((s = GetStatue(i)) != null)
			{
				s.PublicOverheadMessage(MessageType.Regular, 0x3B2, message, fstring);
			}
		}
	}

	public virtual void ResetPuzzle()
	{
		PuzzleStatus(1062053, null);
		ResetLevers();
	}

	public virtual void ResetLevers()
	{
		for (var i = 0; i < 4; i++)
		{
			Item l;
			if ((l = GetLever(i)) == null) continue;
			l.ItemId = 0x108E;
			Effects.PlaySound(l.Location, Map, 0x3E8);
		}
		TheirKey ^= TheirKey;
	}

	public virtual void KillTimers()
	{
		if (_lTimer is {Running: true})
		{
			_lTimer.Stop();
		}
		if (_mTimer is {Running: true})
		{
			_mTimer.Stop();
		}
	}

	public virtual void RemoveSuccessful()
	{
		Successful = null;
	}

	public virtual void LeverPulled(ushort code)
	{
		int correct = 0;

		KillTimers();

		/* if one bit in each of the four nibbles is set, this is false */

		if ((TheirKey = (ushort)(code | (TheirKey <<= 4))) < 0x0FFF)
		{
			_lTimer = Timer.DelayCall(TimeSpan.FromSeconds(30.0), new TimerCallback(ResetPuzzle));
			return;
		}

		if (!CircleComplete)
		{
			PuzzleStatus(1050004, null); // The circle is the key...
		}
		else
		{
			Mobile mPlayer;
			if (TheirKey == MyKey)
			{
				GenKey();
				if ((Successful = mPlayer = GetOccupant(0)) != null)
				{
					SendLocationEffect(LpCenter, 0x1153, 0, 60, 1);
					PlaySounds(LpCenter, Cs1);

					Effects.SendBoltEffect(mPlayer, true);
					mPlayer.MoveToWorld(LrEnter, Map.Malas);

					_mTimer = new LampRoomTimer(this);
					_mTimer.Start();
					Enabled = false;
				}
			}
			else
			{
				for (int i = 0; i < 16; i++)  /* Count matching SET bits, ie correct codes */
				{
					if (((MyKey >> i) & 1) == 1 && ((TheirKey >> i) & 1) == 1)
					{
						correct++;
					}
				}

				PuzzleStatus(StatueMsg[correct], (correct > 0) ? correct.ToString() : null);

				for (int i = 0; i < 5; i++)
				{
					if ((mPlayer = GetOccupant(i)) == null) continue;
					Timer smash = new RockTimer(mPlayer, this);
					smash.Start();
				}
			}
		}
		ResetLevers();
	}

	public virtual void GenKey() /* Shuffle & build key */
	{
		int n, i; ushort[] ca = { 1, 2, 4, 8 };
		for (i = 0; i < 4; i++)
		{
			n = (n = Utility.Random(0, 3)) == i ? n & ~i : n; /* if(i==n) { return pointless; } */
			(ca[i], ca[n]) = (ca[n], ca[i]);
		}
		for (i = 0; i < 4; MyKey = (ushort)(ca[(i++)] | (MyKey <<= 4))) { }
	}

	public class RockTimer : Timer
	{
		private int _count;
		private readonly Mobile _mPlayer;
		private readonly LeverPuzzleController _mController;

		public RockTimer(Mobile player, LeverPuzzleController controller)
			: base(TimeSpan.Zero, TimeSpan.FromSeconds(.25))
		{
			_count = 0;
			_mPlayer = player;
			_mController = controller;
		}

		private static int Rock()
		{
			return 0x1363 + Utility.Random(0, 11);
		}

		protected override void OnTick()
		{
			if (_mPlayer == null || _mPlayer.Map != Map.Malas)
			{
				Stop();
			}
			else
			{
				_count++;
				switch (_count)
				{
					case 1:
						_mPlayer.Paralyze(TimeSpan.FromSeconds(2));
						Effects.SendTargetEffect(_mPlayer, 0x11B7, 20, 10);
						PlayerSendAscii(_mPlayer, 0);  // You are pinned down ...

						PlaySounds(_mPlayer.Location, (!_mPlayer.Female) ? Fs : Ms);
						PlayEffect(ZAdjustedIeFromMobile(_mPlayer, 50), _mPlayer, 0x11B7, 20, false);
						break;
					case 2:
					{
						DoDamage(_mPlayer, 80, 90, false);
						Effects.SendTargetEffect(_mPlayer, 0x36BD, 20, 10);
						PlaySounds(_mPlayer.Location, Exp);
						PlayerSendAscii(_mPlayer, 1); // A speeding rock  ...

						if (AniSafe(_mPlayer))
						{
							_mPlayer.Animate(21, 10, 1, true, true, 0);
						}

						break;
					}
					case 3:
					{
						Stop();

						Effects.SendTargetEffect(_mPlayer, 0x36B0, 20, 10);
						PlayerSendAscii(_mPlayer, 1); // A speeding rock  ...
						PlaySounds(_mPlayer.Location, (!_mPlayer.Female) ? Fs2 : Ms2);

						int j = Utility.Random(6, 10);
						for (int i = 0; i < j; i++)
						{
							IEntity mIEntity = new Entity(Serial.Zero, RandomPointIn(_mPlayer.Location, 10), _mPlayer.Map);

							List<Mobile> mobiles = mIEntity.Map.GetMobilesInRange(mIEntity.Location, 2).ToList();
							for (var k = 0; k < mobiles.Count; k++)
							{
								if (!IsValidDamagable(mobiles[k]) || mobiles[k] == _mPlayer) continue;
								PlayEffect(_mPlayer, mobiles[k], Rock(), 8, true);
								DoDamage(mobiles[k], 25, 30, false);

								if (mobiles[k].Player)
								{
									PohMessage(mobiles[k], 2); // OUCH!
								}
							}
							PlayEffect(_mPlayer, mIEntity, Rock(), 8, false);
						}

						break;
					}
				}
			}
		}
	}

	public class LampRoomKickTimer : Timer
	{
		private readonly Mobile _m;

		public LampRoomKickTimer(Mobile player)
			: base(TimeSpan.FromSeconds(.25))
		{
			_m = player;
		}
		protected override void OnTick()
		{
			MoveMobileOut(_m);
		}
	}

	public class LampRoomTimer : Timer
	{
		public LeverPuzzleController MController;
		public int Ticks;
		public int Level;

		public LampRoomTimer(LeverPuzzleController controller)
			: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
		{
			Level = 0;
			Ticks = 0;
			MController = controller;
		}

		protected override void OnTick()
		{
			Ticks++;
			List<Mobile> mobiles = MController._mLampRoom.GetMobiles();

			if (Ticks >= 71 || MController._mLampRoom.GetPlayerCount() == 0)
			{
				foreach (var mobile in mobiles.Where(mobile => mobile != null && !mobile.Deleted && !mobile.IsDeadBondedPet))
				{
					mobile.Kill();
				}
				MController.Enabled = true;
				Stop();
			}
			else
			{
				if (Ticks % 12 == 0)
				{
					Level++;
				}
				foreach (var mobile in mobiles.Where(IsValidDamagable))
				{
					if (Ticks % 2 == 0 && Level == 5)
					{
						if (mobile.Player)
						{
							mobile.Say(1062092);
							if (AniSafe(mobile))
							{
								mobile.Animate(32, 5, 1, true, false, 0);
							}
						}
						DoDamage(mobile, 15, 20, true);
					}
					if (Utility.Random((int)(Level & ~0xfffffffc), 3) == 3)
					{
						mobile.ApplyPoison(mobile, Pa2[Level]);
					}
					if (Ticks % 12 == 0 && Level > 0 && mobile.Player)
					{
						mobile.SendLocalizedMessage(Pa[Level][0], Pa[Level][1]);
					}
				}
				for (var i = 0; i <= Level; i++)
				{
					SendLocationEffect(RandomPointIn(LrRect, -1), 0x36B0, Utility.Random(150, 200), 0, Pa[Level][2]);
				}
			}
		}
	}

	private static bool IsValidDamagable(Mobile m)
	{
		if (m is not {Deleted: false}) return false;
		if (m.Player && m.Alive)
		{
			return true;
		}

		if (m is not BaseCreature bc) return false;
		return (bc.Controlled || bc.Summoned) && !bc.IsDeadBondedPet;
	}

	public static void MoveMobileOut(Mobile m)
	{
		if (m == null) return;
		if (m is PlayerMobile && !m.Alive)
		{
			if (m.Corpse is {Deleted: false})
			{
				m.Corpse.MoveToWorld(LrExit, Map.Malas);
			}
		}
		BaseCreature.TeleportPets(m, LrExit, Map.Malas);
		m.Location = LrExit;
		m.ProcessDelta();
	}

	public static bool AniSafe(Mobile m)
	{
		return (m != null && !TransformationSpellHelper.UnderTransformation(m) && m.BodyMod == 0 && m.Alive);
	}

	public static IEntity ZAdjustedIeFromMobile(Mobile m, int zDelta)
	{
		return new Entity(Serial.Zero, new Point3D(m.X, m.Y, m.Z + zDelta), m.Map);
	}

	public static void DoDamage(Mobile m, int min, int max, bool poison)
	{
		if (m == null || m.Deleted || !m.Alive) return;
		int damage = Utility.Random(min, max);
		AOS.Damage(m, damage, (poison) ? 0 : 100, 0, 0, (poison) ? 100 : 0, 0);
	}

	public static Point3D RandomPointIn(Point3D point, int range)
	{
		return RandomPointIn(point.X - range, point.Y - range, range * 2, range * 2, point.Z);
	}
	public static Point3D RandomPointIn(Rectangle2D rect, int z)
	{
		return RandomPointIn(rect.X, rect.Y, rect.Height, rect.Width, z);
	}
	public static Point3D RandomPointIn(int x, int y, int x2, int y2, int z)
	{
		return new Point3D(Utility.Random(x, x2), Utility.Random(y, y2), z);
	}

	public static void PlaySounds(Point3D location, int[] sounds)
	{
		foreach (var soundid in sounds)
			Effects.PlaySound(location, Map.Malas, soundid);
	}

	public static void PlayEffect(IEntity from, IEntity to, int itemid, int speed, bool explodes)
	{
		Effects.SendMovingParticles(from, to, itemid, speed, 0, true, explodes, 2, 0, 0);
	}

	public static void SendLocationEffect(IPoint3D p, int itemId, int speed, int duration, int hue)
	{
		Effects.SendPacket(p, Map.Malas, new LocationEffect(p, itemId, speed, duration, hue, 0));
	}

	public static void PlayerSendAscii(Mobile player, int index)
	{
		player.Send(new AsciiMessage(Serial.MinusOne, 0xFFFF, MessageType.Label, MsgParams[index][0], MsgParams[index][1], null, Msgs[index]));
	}

	public static void PohMessage(Mobile from, int index)
	{
		Packet p = new AsciiMessage(from.Serial, from.Body, MessageType.Regular, MsgParams[index][0], MsgParams[index][1], from.Name, Msgs[index]);
		p.Acquire();
		foreach (NetState state in from.Map.GetClientsInRange(from.Location))
			state.Send(p);

		Packet.Release(p);
	}

	private static readonly string[] Msgs =
	{
		"You are pinned down by the weight of the boulder!!!",	// 0
		"A speeding rock hits you in the head!",		// 1
		"OUCH!"							// 2
	};
	/* font&hue for above msgs. index matches */

	private static readonly int[][] MsgParams =
	{
		new[]{ 0x66d, 3 },
		new[]{ 0x66d, 3 },
		new[]{ 0x34, 3 }
	};
	/* World data for items */

	private static readonly int[][] Ta =
	{

		new[]{316, 64, 5},					/* 3D Coords for levers */
		new[]{323, 58, 5},
		new[]{332, 63, 5},
		new[]{323, 71, 5},

		new[]{324, 64},					/* 2D Coords for standing regions */
		new[]{316, 65},
		new[]{324, 58},
		new[]{332, 64},
		new[]{323, 72},

		new[]{468, 92, -1}, new[]{0x181D, 0x482}, 	/* 3D coord, itemid+hue for L.R. teles */
		new[]{469, 92, -1}, new[]{0x1821, 0x3fd},
		new[]{470, 92, -1}, new[]{0x1825, 0x66d},

		new[]{319, 70, 18}, new[]{0x12d8}, 		/* 3D coord, itemid for statues */
		new[]{329, 60, 18}, new[]{0x12d9},

		new[]{469, 96, 6}					/* 3D Coords for Fake Box */
	};

	/* CLILOC data for statue "correct souls" messages */

	private static readonly int[] StatueMsg = { 1050009, 1050007, 1050008, 1050008 };

	/* Exit & Enter locations for the lamp room */

	public static Point3D LrExit = new(353, 172, -1);
	public static Point3D LrEnter = new(467, 96, -1);

	/* "Center" location in puzzle */

	public static Point3D LpCenter = new(324, 64, -1);

	/* Lamp Room Area */

	public static Rectangle2D LrRect = new(465, 92, 10, 10);

	/* Lamp Room area Poison message data */

	private static readonly int[][] Pa =
	{
		new[]{ 0, 0, 0xA6 },
		new[]{ 1050001, 0x485, 0xAA },
		new[]{ 1050003, 0x485, 0xAC },
		new[]{ 1050056, 0x485, 0xA8 },
		new[]{ 1050057, 0x485, 0xA4 },
		new[]{ 1062091, 0x23F3, 0xAC }
	};
	private static readonly Poison[] Pa2 =
	{
		Poison.Lesser,
		Poison.Regular,
		Poison.Greater,
		Poison.Deadly,
		Poison.Lethal,
		Poison.Lethal
	};

	/* SOUNDS */

	private static readonly int[] Fs = { 0x144, 0x154 };
	private static readonly int[] Ms = { 0x144, 0x14B };
	private static readonly int[] Fs2 = { 0x13F, 0x154 };
	private static readonly int[] Ms2 = { 0x13F, 0x14B };
	private static readonly int[] Cs1 = { 0x244 };
	private static readonly int[] Exp = { 0x307 };

	public LeverPuzzleController(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);
		writer.Write(0);
		writer.WriteItemList(_mLevers, true);
		writer.WriteItemList(_mStatues, true);
		writer.WriteItemList(_mTeles, true);
		writer.Write(_mBox);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);
		_ = reader.ReadInt();

		_mLevers = reader.ReadStrongItemList();
		_mStatues = reader.ReadStrongItemList();
		_mTeles = reader.ReadStrongItemList();

		_mBox = reader.ReadItem() as LampRoomBox;

		_mTiles = new List<LeverPuzzleRegion>();
		for (var i = 4; i < 9; i++)
			_mTiles.Add(new LeverPuzzleRegion(this, Ta[i]));

		_mLampRoom = new LampRoomRegion(this);
		Enabled = true;
		TheirKey = 0;
		MyKey = 0;
		GenKey();
	}
}
