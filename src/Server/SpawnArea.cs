using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Server;

public delegate bool SpawnValidator(Map map, int x, int y, int z);

public sealed class SpawnArea : ICollection<Point3D>
{
	private static readonly TileFlag[] m_EmptyFilters;
	private static readonly TileFlag[] m_AllFilters;

	private static readonly Dictionary<int, SpawnArea> m_Cache;

	public const ushort PixelColor = 0xFC1F;

	public const int Stride = 16;

	static SpawnArea()
	{
		m_EmptyFilters = Array.Empty<TileFlag>();

		m_AllFilters = Enum.GetValues(typeof(TileFlag)).Cast<TileFlag>().Where(f => f != TileFlag.None).ToArray();

		m_Cache = new Dictionary<int, SpawnArea>();
	}

	public static SpawnArea Instantiate(Region region, TileFlag filter, SpawnValidator validator, bool cache)
	{
		var name = region.Name;

		if (region.IsDefault || string.IsNullOrWhiteSpace(name))
		{
			name = "Default";
		}

		var filters = GetFilters(filter);

		var hash = GetHashCode(region.Map, name, filters, validator);

		SpawnArea o;

		if (!m_Cache.TryGetValue(hash, out o) || o == null)
		{
			o = new SpawnArea(region.Map, name, filters, validator);

			if (cache)
			{
				m_Cache[hash] = o;
			}

			o.Invalidate();
		}

		return o;
	}

	private static IEnumerable<Rectangle3D> Slice(Rectangle3D rect)
	{
		if (rect.Width <= Stride && rect.Height <= Stride)
		{
			yield return rect;
			yield break;
		}

		int z = Math.Min(rect.Start.Z, rect.End.Z);
		int od = rect.Depth;

		var x = rect.Start.X;

		while (x < rect.End.X)
		{
			var ow = Math.Min(Stride, rect.End.X - x);

			var y = rect.Start.Y;

			while (y < rect.End.Y)
			{
				var oh = Math.Min(Stride, rect.End.Y - y);

				yield return new Rectangle3D(x, y, z, ow, oh, od);

				y += oh;
			}

			x += ow;
		}
	}

	private static TileFlag[] GetFilters(TileFlag filter)
	{
		if (filter == TileFlag.None)
		{
			return m_EmptyFilters;
		}

		return m_AllFilters.Where(f => f != TileFlag.None && filter.HasFlag(f)).ToArray();
	}

	private static int GetHashCode(int x, int y)
	{
		unchecked
		{
			var hash = x + y;

			hash = (hash * 397) ^ x;
			hash = (hash * 397) ^ y;

			return hash;
		}
	}

	private static int GetHashCode(Map facet, string region, IEnumerable<TileFlag> filters, SpawnValidator validator)
	{
		unchecked
		{
			var hash = region.Length;

			hash = region.Aggregate(hash, (v, c) => unchecked((v * 397) ^ Convert.ToInt32(c)));

			hash = (hash * 397) ^ facet.MapID;
			hash = (hash * 397) ^ facet.MapIndex;

			var filter = TileFlag.None;

			foreach (var f in filters)
			{
				filter |= f;
			}

			if (filter != TileFlag.None)
			{
				hash = (hash * 397) ^ (int)(((long)filter >> 0) & 0x7FFFFFFF);
				hash = (hash * 397) ^ (int)(((long)filter >> 32) & 0x7FFFFFFF);
			}

			if (validator != null)
			{
				hash = (hash * 397) ^ validator.GetHashCode();
			}

			return hash;
		}
	}

	private Bitmap _image;

	private Rectangle3D _bounds;

	private readonly Dictionary<int, Point3D> _points;

	public SpawnValidator Validator { get; }

	public TileFlag[] Filters { get; }

	public Map Facet { get; }

	public string Region { get; }

	public Point2D Center { get; private set; }

	public Rectangle3D Bounds => _bounds;

	public int Count => _points.Count;

	bool ICollection<Point3D>.IsReadOnly => true;

	private SpawnArea(Map facet, string region, TileFlag[] filters, SpawnValidator validator)
	{
		_points = new Dictionary<int, Point3D>();

		Facet = facet;
		Region = region;
		Filters = filters;
		Validator = validator;
	}

	public bool Contains(int x, int y)
	{
		return _points.ContainsKey(GetHashCode(x, y));
	}

	public bool Contains(IPoint2D p)
	{
		return _points.ContainsKey(GetHashCode(p.X, p.Y));
	}

	public Point3D GetRandom()
	{
		if (Facet == null || Facet == Map.Internal || Count == 0)
		{
			return Point3D.Zero;
		}

		if (Count <= 1024)
		{
			return _points.Values.ElementAt(Utility.Random(Count));
		}

		int x, y;

		do
		{
			x = Utility.RandomMinMax(_bounds.Start.X, _bounds.End.X);
			y = Utility.RandomMinMax(_bounds.Start.Y, _bounds.End.Y);
		}
		while (!Contains(x, y));

		var z = Facet.GetAverageZ(x, y);

		if (Validator == null || Validator(Facet, x, y, z))
		{
			return new Point3D(x, y, z);
		}

		return GetRandom();
	}

	public void Invalidate()
	{
		_image = null;

		_points.Clear();

		if (Facet == null || Facet == Map.Internal)
		{
			return;
		}

		Region region;

		if (string.IsNullOrWhiteSpace(Region) || Region == "Default")
		{
			region = Facet.DefaultRegion;
		}
		else if (!Facet.Regions.TryGetValue(Region, out region))
		{
			return;
		}

		if (region == null || (!region.IsDefault && (region.Area == null || region.Area.Length == 0)))
		{
			return;
		}

		if (region.IsDefault)
		{
			var fw = Facet.MapID <= 1 ? 5119 : Facet.Width;
			var fh = Facet.MapID <= 1 ? 4095 : Facet.Height;
			var fd = Server.Region.MaxZ - Server.Region.MinZ;

			_bounds = new Rectangle3D(0, 0, Server.Region.MinZ, fw, fh, fd);

			Parallel.ForEach(Slice(_bounds), Compute);
		}
		else
		{
			int x1 = short.MaxValue, y1 = short.MaxValue, z1 = sbyte.MaxValue;
			int x2 = short.MinValue, y2 = short.MinValue, z2 = sbyte.MinValue;

			foreach (var o in region.Area)
			{
				x1 = Math.Min(x1, o.Start.X);
				y1 = Math.Min(y1, o.Start.Y);
				z1 = Math.Min(z1, o.Start.Z);

				x2 = Math.Max(x2, o.End.X);
				y2 = Math.Max(y2, o.End.Y);
				z2 = Math.Max(z2, o.End.Z);
			}

			_bounds = new Rectangle3D(x1, y1, z1, x2 - x1, y2 - y1, z2 - z1);

			Parallel.ForEach(region.Area.SelectMany(Slice), Compute);
		}

		Center = new Point2D(_bounds.Start.X + (_bounds.Width / 2), _bounds.Start.Y + (_bounds.Height / 2));
	}

	private void Compute(Rectangle3D area)
	{
		// Check all corners to skip large bodies of water.
		if (Filters.Contains(TileFlag.Wet))
		{
			var land1 = Facet.Tiles.GetLandTile(area.Start.X, area.Start.Y); // TL
			var land2 = Facet.Tiles.GetLandTile(area.End.X, area.Start.Y); // TR
			var land3 = Facet.Tiles.GetLandTile(area.Start.X, area.End.Y); // BL
			var land4 = Facet.Tiles.GetLandTile(area.End.X, area.End.Y); // BR

			if ((land1.Ignored || TileData.LandTable[land1.Id].Flags.HasFlag(TileFlag.Wet)) &&
			    (land2.Ignored || TileData.LandTable[land2.Id].Flags.HasFlag(TileFlag.Wet)) &&
			    (land3.Ignored || TileData.LandTable[land3.Id].Flags.HasFlag(TileFlag.Wet)) &&
			    (land4.Ignored || TileData.LandTable[land4.Id].Flags.HasFlag(TileFlag.Wet)))
			{
				return;
			}
		}

		int x, y, z, h;

		for (x = area.Start.X; x < area.End.X; x++)
		{
			for (y = area.Start.Y; y < area.End.Y; y++)
			{
				h = GetHashCode(x, y);

				if (_points.ContainsKey(h))
				{
					continue;
				}

				z = Facet.Tiles.GetLandTile(x, y).Z;//.GetAverageZ(x, y);

				if (!CanSpawn(x, y, z))
				{
					continue;
				}

				if (Filters.Length > 0)
				{
					var land = Facet.Tiles.GetLandTile(x, y);

					if (land.Ignored)
					{
						continue;
					}

					var flags = TileData.LandTable[land.Id].Flags;

					if (Filters.Any(f => flags.HasFlag(f)))
					{
						continue;
					}

					var valid = true;

					foreach (var tile in Facet.Tiles.GetStaticTiles(x, y))
					{
						flags = TileData.ItemTable[tile.Id].Flags;

						if (Filters.Any(f => flags.HasFlag(f)))
						{
							valid = false;
							break;
						}
					}

					if (!valid)
					{
						continue;
					}
				}

				if (Validator != null && !Validator(Facet, x, y, z))
				{
					continue;
				}

				lock (_points)
				{
					_points[h] = new Point3D(x, y, z);
				}
			}
		}
	}

	private bool CanSpawn(int x, int y, int z)
	{
		return Facet.CanFit(x, y, z, Server.Region.MaxZ - z, true, false, true);
	}

	public override int GetHashCode()
	{
		return GetHashCode(Facet, Region, Filters, Validator);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<Point3D> GetEnumerator()
	{
		return _points.Values.GetEnumerator();
	}

	void ICollection<Point3D>.Clear()
	{
		_points.Clear();
	}

	void ICollection<Point3D>.Add(Point3D p)
	{
		_points[GetHashCode(p.X, p.Y)] = p;
	}

	bool ICollection<Point3D>.Remove(Point3D p)
	{
		return _points.Remove(GetHashCode(p.X, p.Y));
	}

	bool ICollection<Point3D>.Contains(Point3D p)
	{
		return _points.ContainsKey(GetHashCode(p.X, p.Y));
	}

	void ICollection<Point3D>.CopyTo(Point3D[] array, int index)
	{
		_points.Values.CopyTo(array, index);
	}
}
