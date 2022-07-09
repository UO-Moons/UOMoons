using Server.Mobiles;
using System;
using System.Collections.Generic;

namespace Server.Engines.ConPVP;

public class ArenaController : BaseItem
{
	[CommandProperty(AccessLevel.GameMaster)]
	public Arena Arena { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsPrivate { get; set; }

	public override string DefaultName => "arena controller";

	[Constructable]
	public ArenaController() : base(0x1B7A)
	{
		Visible = false;
		Movable = false;

		Arena = new Arena();

		Instances.Add(this);
	}

	public override void OnDelete()
	{
		base.OnDelete();

		Instances.Remove(this);
		Arena.Delete();
	}

	public override void OnDoubleClick(Mobile from)
	{
		if (from.AccessLevel >= AccessLevel.GameMaster)
			from.SendGump(new Gumps.PropertiesGump(from, Arena));
	}

	public ArenaController(Serial serial) : base(serial)
	{
	}

	public override void Serialize(GenericWriter writer)
	{
		base.Serialize(writer);

		writer.Write(0);

		writer.Write(IsPrivate);

		Arena.Serialize(writer);
	}

	public override void Deserialize(GenericReader reader)
	{
		base.Deserialize(reader);

		int version = reader.ReadInt();

		switch (version)
		{
			case 0:
			{
				IsPrivate = reader.ReadBool();

				Arena = new Arena(reader);
				break;
			}
		}

		Instances.Add(this);
	}

	public static List<ArenaController> Instances { get; set; } = new();
}

[PropertyObject]
public class ArenaStartPoints
{
	public Point3D[] Points { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D EdgeWest { get => Points[0]; set => Points[0] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D EdgeEast { get => Points[1]; set => Points[1] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D EdgeNorth { get => Points[2]; set => Points[2] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D EdgeSouth { get => Points[3]; set => Points[3] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D CornerNw { get => Points[4]; set => Points[4] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D CornerSe { get => Points[5]; set => Points[5] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D CornerSw { get => Points[6]; set => Points[6] = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D CornerNe { get => Points[7]; set => Points[7] = value; }

	public override string ToString()
	{
		return "...";
	}

	public ArenaStartPoints() : this(new Point3D[8])
	{
	}

	public ArenaStartPoints(Point3D[] points)
	{
		Points = points;
	}

	public ArenaStartPoints(GenericReader reader)
	{
		Points = new Point3D[reader.ReadEncodedInt()];

		for (int i = 0; i < Points.Length; ++i)
			Points[i] = reader.ReadPoint3D();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(Points.Length);

		for (int i = 0; i < Points.Length; ++i)
			writer.Write(Points[i]);
	}
}

[PropertyObject]
public class Arena : IComparable
{
	private Map _facet;
	private Rectangle2D _bounds;
	private Rectangle2D _zone;
	private Point3D _outside;
	private Point3D _wall;
	private Point3D _gateIn;
	private Point3D _gateOut;
	private bool _active;
	private string _name;

	private bool _isGuarded;
	private TournamentController _tournament;

	[CommandProperty(AccessLevel.GameMaster)]
	public LadderController Ladder { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsGuarded
	{
		get => _isGuarded;
		set
		{
			_isGuarded = value;

			if (_region != null)
				_region.Disabled = !_isGuarded;
		}
	}

	public Ladder AcquireLadder()
	{
		if (Ladder != null)
			return Ladder.Ladder;

		return Server.Engines.ConPVP.Ladder.Instance;
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public TournamentController Tournament
	{
		get => _tournament;
		set
		{
			if (_tournament != null)
				_tournament.Tournament.Arenas.Remove(this);

			_tournament = value;

			if (_tournament != null)
				_tournament.Tournament.Arenas.Add(this);
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Mobile Announcer { get; set; }

	[CommandProperty(AccessLevel.GameMaster)]
	public string Name
	{
		get => _name;
		set { _name = value; if (_active) Arenas.Sort(); }
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Map Facet
	{
		get => _facet;
		set
		{
			_facet = value;

			if (Teleporter != null)
				Teleporter.Map = value;

			_region?.Unregister();

			if (_zone.Start != Point2D.Zero && _zone.End != Point2D.Zero && _facet != null)
				_region = new SafeZone(_zone, _outside, _facet, _isGuarded);
			else
				_region = null;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle2D Bounds { get => _bounds; set => _bounds = value; }

	private SafeZone _region;

	public int Spectators
	{
		get
		{
			if (_region == null)
				return 0;

			int specs = _region.GetPlayerCount() - Players.Count;

			if (specs < 0)
				specs = 0;

			return specs;
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Rectangle2D Zone
	{
		get => _zone;
		set
		{
			_zone = value;

			if (_zone.Start != Point2D.Zero && _zone.End != Point2D.Zero && _facet != null)
			{
				_region?.Unregister();

				_region = new SafeZone(_zone, _outside, _facet, _isGuarded);
			}
			else
			{
				if (_region != null)
					_region.Unregister();

				_region = null;
			}
		}
	}

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Outside { get => _outside; set => _outside = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D GateIn { get => _gateIn; set => _gateIn = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D GateOut { get => _gateOut; set { _gateOut = value; if (Teleporter != null) Teleporter.Location = _gateOut; } }

	[CommandProperty(AccessLevel.GameMaster)]
	public Point3D Wall { get => _wall; set => _wall = value; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool IsOccupied => Players.Count > 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public ArenaStartPoints Points { get; set; }

	public Item Teleporter { get; set; }

	public List<Mobile> Players { get; }

	[CommandProperty(AccessLevel.GameMaster)]
	public bool Active
	{
		get => _active;
		set
		{
			if (_active == value)
				return;

			_active = value;

			if (_active)
			{
				Arenas.Add(this);
				Arenas.Sort();
			}
			else
			{
				Arenas.Remove(this);
			}
		}
	}

	public void Delete()
	{
		Active = false;

		_region?.Unregister();

		_region = null;
	}

	public override string ToString()
	{
		return "...";
	}

	public Point3D GetBaseStartPoint(int index)
	{
		if (index < 0)
			index = 0;

		return Points.Points[index % Points.Points.Length];
	}

	#region Offsets & Rotation
	private static readonly Point2D[] m_EdgeOffsets = {
		/*
		 *        /\
		 *       /\/\
		 *      /\/\/\
		 *      \/\/\/
		 *       \/\/\
		 *        \/\/
		 */
		new( 0, 0 ),
		new( 0, -1 ),
		new( 0, +1 ),
		new( 1, 0 ),
		new( 1, -1 ),
		new( 1, +1 ),
		new( 2, 0 ),
		new( 2, -1 ),
		new( 2, +1 ),
		new( 3, 0 )
	};

	// nw corner
	private static readonly Point2D[] m_CornerOffsets = new Point2D[]
	{
		/*
		 *         /\
		 *        /\/\
		 *       /\/\/\
		 *      /\/\/\/\
		 *      \/\/\/\/
		 */
		new( 0, 0 ),
		new( 0, 1 ),
		new( 1, 0 ),
		new( 1, 1 ),
		new( 0, 2 ),
		new( 2, 0 ),
		new( 2, 1 ),
		new( 1, 2 ),
		new( 0, 3 ),
		new( 3, 0 )
	};

	private static readonly int[][,] m_Rotate = {
		new[,]{ { +1, 0 }, { 0, +1 } }, // west
		new[,]{ { -1, 0 }, { 0, -1 } }, // east
		new[,]{ { 0, +1 }, { +1, 0 } }, // north
		new[,]{ { 0, -1 }, { -1, 0 } }, // south
		new[,]{ { +1, 0 }, { 0, +1 } }, // nw
		new[,]{ { -1, 0 }, { 0, -1 } }, // se
		new[,]{ { 0, +1 }, { +1, 0 } }, // sw
		new[,]{ { 0, -1 }, { -1, 0 } }, // ne
	};
	#endregion

	public void MoveInside(DuelPlayer[] players, int index)
	{
		if (index < 0)
			index = 0;
		else
			index %= Points.Points.Length;

		Point3D start = GetBaseStartPoint(index);

		int offset = 0;

		Point2D[] offsets = index < 4 ? m_EdgeOffsets : m_CornerOffsets;
		int[,] matrix = m_Rotate[index];

		for (int i = 0; i < players.Length; ++i)
		{
			DuelPlayer pl = players[i];

			if (pl == null)
				continue;

			Mobile mob = pl.Mobile;

			Point2D p;

			p = offset < offsets.Length ? offsets[offset++] : offsets[^1];

			p.X = p.X * matrix[0, 0] + p.Y * matrix[0, 1];
			p.Y = p.X * matrix[1, 0] + p.Y * matrix[1, 1];

			mob.MoveToWorld(new Point3D(start.X + p.X, start.Y + p.Y, start.Z), _facet);
			mob.Direction = mob.GetDirectionTo(_wall);

			Players.Add(mob);
		}
	}

	public Arena()
	{
		Points = new ArenaStartPoints();
		Players = new List<Mobile>();
	}

	public Arena(GenericReader reader)
	{
		int version = reader.ReadEncodedInt();

		switch (version)
		{
			case 0:
			{
				_isGuarded = reader.ReadBool();

				Ladder = reader.ReadItem() as LadderController;

				_tournament = reader.ReadItem() as TournamentController;
				Announcer = reader.ReadMobile();

				_name = reader.ReadString();

				_zone = reader.ReadRect2D();

				_gateIn = reader.ReadPoint3D();
				_gateOut = reader.ReadPoint3D();
				Teleporter = reader.ReadItem();

				Players = reader.ReadStrongMobileList();

				_facet = reader.ReadMap();
				_bounds = reader.ReadRect2D();
				_outside = reader.ReadPoint3D();
				_wall = reader.ReadPoint3D();

				_active = reader.ReadBool();
				Points = new ArenaStartPoints(reader);

				if (_active)
				{
					Arenas.Add(this);
					Arenas.Sort();
				}

				break;
			}
		}

		if (_zone.Start != Point2D.Zero && _zone.End != Point2D.Zero && _facet != null)
			_region = new SafeZone(_zone, _outside, _facet, _isGuarded);

		if (IsOccupied)
			Timer.DelayCall(TimeSpan.FromSeconds(2.0), Evict);

		if (_tournament != null)
			Timer.DelayCall(TimeSpan.Zero, AttachToTournament_Sandbox);
	}

	private void AttachToTournament_Sandbox()
	{
		_tournament?.Tournament.Arenas.Add(this);
	}

	[CommandProperty(AccessLevel.Administrator, AccessLevel.Administrator)]
	public bool ForceEvict { get => false; set { if (value) Evict(); } }

	public void Evict()
	{
		Point3D loc;
		Map facet;

		if (_facet == null)
		{
			loc = new Point3D(2715, 2165, 0);
			facet = Map.Felucca;
		}
		else
		{
			loc = _outside;
			facet = _facet;
		}

		bool hasBounds = _bounds.Start != Point2D.Zero && _bounds.End != Point2D.Zero;

		for (int i = 0; i < Players.Count; ++i)
		{
			Mobile mob = Players[i];

			if (mob == null)
				continue;

			if (mob.Map == Map.Internal)
			{
				if ((_facet == null || mob.LogoutMap == _facet) && (!hasBounds || _bounds.Contains(mob.LogoutLocation)))
					mob.LogoutLocation = loc;
			}
			else if ((_facet == null || mob.Map == _facet) && (!hasBounds || _bounds.Contains(mob.Location)))
			{
				mob.MoveToWorld(loc, facet);
			}

			mob.Combatant = null;
			mob.Frozen = false;
			DuelContext.Debuff(mob);
			DuelContext.CancelSpell(mob);
		}

		if (hasBounds)
		{
			List<Mobile> pets = new();

			foreach (Mobile mob in facet.GetMobilesInBounds(_bounds))
			{
				if (mob is BaseCreature {Controlled: true, ControlMaster: { }} pet)
				{
					if (Players.Contains(pet.ControlMaster))
					{
						pets.Add(pet);
					}
				}
			}

			foreach (Mobile pet in pets)
			{
				pet.Combatant = null;
				pet.Frozen = false;

				pet.MoveToWorld(loc, facet);
			}
		}

		Players.Clear();
	}

	public void Serialize(GenericWriter writer)
	{
		writer.WriteEncodedInt(0);

		writer.Write(_isGuarded);

		writer.Write(Ladder);

		writer.Write(_tournament);
		writer.Write(Announcer);

		writer.Write(_name);

		writer.Write(_zone);

		writer.Write(_gateIn);
		writer.Write(_gateOut);
		writer.Write(Teleporter);

		writer.Write(Players);

		writer.Write(_facet);
		writer.Write(_bounds);
		writer.Write(_outside);
		writer.Write(_wall);
		writer.Write(_active);

		Points.Serialize(writer);
	}

	public static List<Arena> Arenas { get; } = new();

	public static Arena FindArena(List<Mobile> players)
	{
		Preferences prefs = Preferences.Instance;

		if (prefs == null)
			return FindArena();

		if (Arenas.Count == 0)
			return null;

		if (players.Count > 0)
		{
			Mobile first = players[0];

			List<ArenaController> allControllers = ArenaController.Instances;

			for (int i = 0; i < allControllers.Count; ++i)
			{
				ArenaController controller = allControllers[i];

				if (controller is {Deleted: false, Arena: { }, IsPrivate: true} && controller.Map == first.Map && first.InRange(controller, 24))
				{
					Multis.BaseHouse house = Multis.BaseHouse.FindHouseAt(controller);
					bool allNear = true;

					for (int j = 0; j < players.Count; ++j)
					{
						Mobile check = players[j];
						bool isNear;

						if (house == null)
							isNear = controller.Map == check.Map && check.InRange(controller, 24);
						else
							isNear = Multis.BaseHouse.FindHouseAt(check) == house;

						if (!isNear)
						{
							allNear = false;
							break;
						}
					}

					if (allNear)
						return controller.Arena;
				}
			}
		}

		List<ArenaEntry> arenas = new();

		for (int i = 0; i < Arenas.Count; ++i)
		{
			Arena arena = Arenas[i];

			if (!arena.IsOccupied)
				arenas.Add(new ArenaEntry(arena));
		}

		if (arenas.Count == 0)
			return Arenas[0];

		int tc = 0;

		for (int i = 0; i < arenas.Count; ++i)
		{
			ArenaEntry ae = arenas[i];

			for (int j = 0; j < players.Count; ++j)
			{
				PreferencesEntry pe = prefs.Find(players[j]);

				if (pe.Disliked.Contains(ae.Arena.Name))
				{
				}
				else
					++ae.VotesFor;
			}

			tc += ae.Value;
		}

		int rn = Utility.Random(tc);

		for (int i = 0; i < arenas.Count; ++i)
		{
			ArenaEntry ae = arenas[i];

			if (rn < ae.Value)
				return ae.Arena;

			rn -= ae.Value;
		}

		return arenas[Utility.Random(arenas.Count)].Arena;
	}

	private class ArenaEntry
	{
		public readonly Arena Arena;
		public int VotesFor;

		public int Value => VotesFor;/*if ( m_VotesFor > m_VotesAgainst )
						return m_VotesFor - m_VotesAgainst;
					else if ( m_VotesFor > 0 )
						return 1;
					else
						return 0;*/

		public ArenaEntry(Arena arena)
		{
			Arena = arena;
		}
	}

	public static Arena FindArena()
	{
		if (Arenas.Count == 0)
			return null;

		int offset = Utility.Random(Arenas.Count);

		for (int i = 0; i < Arenas.Count; ++i)
		{
			Arena arena = Arenas[(i + offset) % Arenas.Count];

			if (!arena.IsOccupied)
				return arena;
		}

		return Arenas[offset];
	}

	public int CompareTo(object obj)
	{
		Arena c = (Arena)obj;

		string a = _name;
		string b = c._name;

		switch (a)
		{
			case null when b == null:
				return 0;
			case null:
				return -1;
		}

		if (b == null)
			return +1;

		return string.Compare(a, b, StringComparison.Ordinal);
	}
}
