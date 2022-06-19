//  LOS system; this system relies on GENERATED CODE that is present in the other
//     partial class for LOS. The file "GeneratedLOS.cs" is not written by a human!
//     It is written by a computer program, so don't mess with it! The code generator
//     is INCLUDED with this program; if you want to make changes, you'll want to
//     learn how to use it!
//------------------------------------------------------------------------------
//  The generated program establishes the "fact" of a "shadow" for any obstruction
//  in a player's view. So, for example, the below figure shows # signs for the
//  obstruction in the 'X' position, where the view is at position 'O':
//
//  Figure 1:
//
// {{-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#,#,#},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#,#,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,#,#,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,#,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,#,#,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,#,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,#,#,#,#,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,X,#,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,O,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-},
//  {-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-,-}},
//
//  This matrix makes it a single instruction for us to determine whether or
//  not an object in any position in any of the # cells is visible (they aren't).
//  The story would end there, if it werent for the X's. As in plural 'X'. There
//  may be many obstructions, and many things needing to be tested for visibility,
//  at any given time. To do this efficiently, we need something better. That
//  'better' thing is what we call a "compiled" visibility mask. It's said to
//  be compiled, because what represent it is now instructions and not data.
//  The above example compresses down to something like this.
//
//  Figure 2:
//
//    delegate( int[,] shadow )
//    {
//        shadow[0,0] |= 510;
//        shadow[1,0] |= 1022;
//        shadow[2,0] |= 1022;
//        shadow[3,0] |= 2046;
//        shadow[4,0] |= 2046;
//        shadow[5,0] |= 4094;
//        shadow[6,0] |= 4094;
//        shadow[7,0] |= 8190;
//        shadow[8,0] |= 8188;
//        shadow[9,0] |= 16368;
//        shadow[10,0] |= 16320;
//        shadow[11,0] |= 32512;
//        shadow[12,0] |= 31744;
//        shadow[13,0] |= 61440;
//        shadow[14,0] |= 16384;
//    },
//  
//  In the above, the wierd numbers are actually numbers where the bits in the
//  binary field correspond to true or false for blockage of visibility in a
//  column of the visibility mask. The additional advantage here is that this
//  is a very memory compact structure. The other advantage is, it dramatically,
//  shortens the number of instructions we have to perform to create the matrix.
//  In our 37 x 37 matrix (range of 18, plus middle), we are doing one instruction
//  per row; quite a lot of compression there.
//
//  Anyways, once you have many examples like the above, ONE EACH, for each 
//  possible cell in the entire matrix (except the middle and outer edges),
//  you can simply call a method for each location of an obstruction, cycling
//  the (very fast) binary ors over the matrix, producing a final result that
//  can tell you whether or not something in a field of view is visibile at
//  its position, having taken into account every possible obstruction
//
//  Probes are very cheap. They look something like this:
//
//  Figure 3:
//
//    delegate ( ulong[,] shadow )
//    {
//        if( ( shadow[13,0] & 16777216ul ) == 16777216ul ) return true; else return false;
//    },
//
//  An astute observer will note that the cost of establishing the mask is
//  directly proportional to the number of obstructions, and has little indeed
//  to do with the number of things being obstructed. This is true. The cost
//  of establishing the mask is approximately 9 X the number of obstructions.
//  This '9' is the average number of shadowmask commands in the generated code
//  for a matrix of 37x37. The Figure 3 also happens to be (about) the worst case.
//  That's a pretty good worst case!
//------------------------------------------------------------------------------
using Server.Collections;
using System;
using System.Collections.Generic;

namespace Server.LOS
{
	//------------------------------------------------------------------------------
	//  Visibility Matrix; just a wrapper to hold our compiled shadow matrix and
	//    bits of processing code.
	//------------------------------------------------------------------------------
	public class VisibilityMatrix
	{
		public uint[,] m_Shadows;

		public int Range { get; }
		public uint[,] Shadows => m_Shadows;
		//--------------------------------------------------------------------------
		//  This is our main work horse; this records the fact of an obstruction;
		//  all we really do here is check to see if there is a maker (there isn't
		//  when there is nothing to do, we can avoid a call that way), and then call
		//  it when there is one.
		//--------------------------------------------------------------------------
		public void ProcessObstructionAt(Point3D beholder, int x, int y)
		{
			// VERY IMPORTANT, that the below be true, but tests removed for perf reasons
			// int xRange  = Math.Abs( beholder.X - x );
			// int yRange  = Math.Abs( beholder.Y - y );
			// if( xRange > 15 || yRange > 15 ) return;

			int xTarget = x - beholder.X + 15;
			int yTarget = y - beholder.Y + 15;

			// Now, potentially, process it:

			ShadowMaker maker = LineOfSight.m_ShadowMakers[xTarget, yTarget];

			if (maker == null)
				return;				   // no shadows for this obstruction; can
									   // occur at edge, where a shadow out of
									   // range would be irrelevant
			maker(m_Shadows);
		}
		//--------------------------------------------------------------------------
		//  Test for visibility; like the above, we call out to generated code;
		//  everything mentioned previously applies, we're just testing the bit masks
		//  of the matrix previously made.
		//--------------------------------------------------------------------------
		public bool IsVisible(Point3D beholder, int x, int y)
		{
			// VERY IMPORTANT, that the below be true, but tests removed for perf reasons
			// int xRange  = Math.Abs( beholder.X - x );
			// int yRange  = Math.Abs( beholder.Y - y );
			// if( xRange > 15 || yRange > 15 ) return false;

			// only x is relevant, because the fact of a bit mask position
			// is row independent; x tells us which bit we're probing for
			// Console.WriteLine("tester for ({0},{1})...", xTarget, yTarget);

			int xTarget = x - beholder.X + 15;
			int yTarget = y - beholder.Y + 15;

			ShadowTester tester = LineOfSight.m_ShadowTesters[xTarget];

			if (tester == null)
				return true;

			bool blocked = tester(m_Shadows, yTarget);

			return !blocked;
		}
		//--------------------------------------------------------------------------
		internal VisibilityMatrix(VisibilityMatrix v)
		{
			Range = v.Range;
			m_Shadows = new uint[v.m_Shadows.Length, 1];

			for (int i = 0; i < v.m_Shadows.Length; i++)
			{
				m_Shadows[i, 0] = v.m_Shadows[i, 0];
			}
		}
		//--------------------------------------------------------------------------
		public VisibilityMatrix(int range)
		{
			Range = range;
			int max = 2 * Range + 1;
			m_Shadows = new uint[max, 1];
		}
		//--------------------------------------------------------------------------
		public void Clear()
		{
			Array.Clear(m_Shadows, 0, m_Shadows.Length);
		}
		//--------------------------------------------------------------------------
		public void Dump()
		{
			for (int y = 0; y < (Range * 2) + 1; y++)
			{
				Console.WriteLine(CodeGenMain.BinaryRepr(m_Shadows[y, 0]));
			}
		}
	}
	//------------------------------------------------------------------------------
	//  Note the partial; the other part of this partial is in GeneratedLOS, which
	//  is entirely generated code.
	//------------------------------------------------------------------------------
	public partial class LineOfSight
	{
		Cache<VisibilityMatrix, int> m_cache;
		private readonly Map m_map;
		private readonly int m_size;

		public LineOfSight(Map map, int size)
		{
			m_size = size;
			m_map = map;
			m_cache = new Cache<VisibilityMatrix, int>(m_size);
		}

		public void Clear()
		{
			m_cache = new Cache<VisibilityMatrix, int>(m_size);
			Console.WriteLine("LOS: Cache Cleared for map \"{0}\"", m_map.Name);
		}

		//--------------------------------------------------------------------------
		//  Test the Player's visibility matrix to see if they can see things at the
		//   target point. This is the main part of our LOS "can see" function.
		//--------------------------------------------------------------------------
		public bool Visible(Point3D beholder, Point3D beheld)
		{
			//----------------------------------------------------------------------
			//  The center of the visibility matrix, which is always the anchor 
			//  point of the player mobile, is cell [range+1-1,range+1-1]. E.g.,
			//  to calculate where the player mobile is in the 37x37 matrix, use
			//  18+1-1 = 18. 0-17,0-17 are leftwards and upwards, 18,18 is the middle,
			//  and 19-36,19-36 are rightwards and downwards.
			//----------------------------------------------------------------------
			bool visible = false;
			//double  start = DateTime.Now.Ticks / 10000000.0;
			//for(int i=0; i<500; i++)
			//{
			int key = beholder.X * m_map.Width * m_map.Height + beholder.Y * m_map.Height + beholder.Z;

			//----------------------------------------------------------------------
			//  First see if we have a matrix in the cache:
			//----------------------------------------------------------------------

			VisibilityMatrix baseMatrix = m_cache.Hit(key);

			//----------------------------------------------------------------------
			//  If we don't, create it and put it in the cache:
			//----------------------------------------------------------------------

			if (baseMatrix == null)
			{
				baseMatrix = CreateBaseMatrix(beholder);

				m_cache.Store(key, baseMatrix);
			}

			//----------------------------------------------------------------------
			//  Rapidly create a clone of the matric fetched from the cache for
			//  temporary use (this copy is implemented by cheap memcpy in the
			//  CLR, don't wory much about it)
			//----------------------------------------------------------------------

			VisibilityMatrix v = new(baseMatrix);

			//----------------------------------------------------------------------
			//  Add in the obstructions from items into the cache
			//----------------------------------------------------------------------

			ProcessMatrixItems(beholder, v);

			//----------------------------------------------------------------------
			//  Now finally do the test for visibility:
			//----------------------------------------------------------------------

			visible = v.IsVisible(beholder, beheld.X, beheld.Y);

			//}
			// double  stop = DateTime.Now.Ticks / 10000000.0;
			// double  elapsed = stop - start;
			// Console.WriteLine( "VisibleTime: " + elapsed );
			return visible;
		}
		//--------------------------------------------------------------------------
		//  Create the base matrix; this is the "cacheable" portion of the matrix,
		//  the stuff like land and statics that never really change.
		//--------------------------------------------------------------------------
		public VisibilityMatrix CreateBaseMatrix(Point3D beholder)
		{
			VisibilityMatrix v = new(15);
			// VisibilityMatrix v = null;
			// for performance testing--leave in
			// double  start = DateTime.Now.Ticks / 10000000.0;
			// for(int i=0; i<500; i++)
			// {
			int eyelevel = beholder.Z + 12;

			//        v = new VisibilityMatrix( 15 );

			//----------------------------------------------------------------------
			//  This is a spiral search pattern from center; the reason we do this
			//  is that starting from the center allows us to benefit from
			//  not having to load up data relating tiles and so forth further out
			//  if x,y cursor is in the shadow of a prior obstruction...
			//----------------------------------------------------------------------

			int locX = beholder.X;
			int locY = beholder.Y;

			for (int j = 0; j < m_SpiralPattern.Length; j++)
			{
				int xOffset = m_SpiralPattern[j][0];
				int yOffset = m_SpiralPattern[j][1];

				int x = locX + xOffset;
				int y = locY + yOffset;

				//------------------------------------------------------------------
				//  the spiral pattern could make us walk off the edge of the
				//  map; break out of that here:
				//------------------------------------------------------------------

				if (x < 0 || x > m_map.Width || y < 0 || y > m_map.Height)
					goto cutpoint;

				//------------------------------------------------------------------
				//  Here we optimize by skipping the pulling up of tiles in the
				//  event we are in a shadow; it turns out that in certain diagonal
				//  'L' cases (corners), this doesn't work perfectly, hence the check
				//  for matching abs. x/y offsets ... we don't cut point those.
				//------------------------------------------------------------------

				if (!v.IsVisible(beholder, x, y) && Math.Abs(xOffset - yOffset) <= 1)
					goto cutpoint;

				//------------------------------------------------------------------
				//  Now process statics:
				//------------------------------------------------------------------

				StaticTile[] staticTiles = m_map.Tiles.GetStaticTiles(x, y, true);

				foreach (StaticTile staticTile in staticTiles)
				{
					if (TileBlocksVis(staticTile, eyelevel, x, y, beholder))
					{
						v.ProcessObstructionAt(beholder, x, y);
						goto cutpoint; // further obstructions irrelevant
					}
				}

				//------------------------------------------------------------------
				//  Now process land:
				//------------------------------------------------------------------

				LandTile landTile = m_map.Tiles.GetLandTile(x, y);

				if (LandBlocksVis(landTile, x, y, eyelevel + 5, beholder))
				{
					v.ProcessObstructionAt(beholder, x, y);
					goto cutpoint; // further obstructions irrelevant
				}

				//------------------------------------------------------------------
				//  Now process "invalid" tiles (the black areas in dungeons):
				//------------------------------------------------------------------

				if (IsVizBlockingInvalidLand(landTile, staticTiles, x, y))
				{
					v.ProcessObstructionAt(beholder, x, y);
					goto cutpoint; // further obstructions irrelevant
				}

				//------------------------------------------------------------------
				//  Items left deliberately out here (that's a follow up stage)
				//------------------------------------------------------------------

				//------------------------------------------------------------------
				cutpoint:;  // sorry about the evil goto; please leave this alone for now
							// think of it this way; this is like a destionation for "continue",
							// where code can be if needed.
							//------------------------------------------------------------------
			}
			//}
			// double  stop = DateTime.Now.Ticks / 10000000.0;
			// double  elapsed = stop - start;
			// Console.WriteLine( "CreateBaseMatrixInternalTime: " + elapsed );
			return v;
		}
		//--------------------------------------------------------------------------
		//  Add supplemental (non-static/non-land) items into the obstruction matrix;
		//  this is very cheap in comparison to the cost of processing land and statics
		//--------------------------------------------------------------------------
		public void ProcessMatrixItems(Point3D beholder, VisibilityMatrix v)
		{
			// double  start = DateTime.Now.Ticks / 10000000.0;
			// for(int i=0; i<100000; i++)
			// {
			int eyelevel = beholder.Z + 12;

			foreach (Item item in m_map.GetItemsInRange(new Point3D(beholder.X, beholder.Y, 0), 15))
			{
				//  cutpoint optimization; if the cell is in shadow, don't bother with
				//  any more tests; such a test would be irrelevant:

				if (!v.IsVisible(beholder, item.Location.X, item.Location.Y))
				{
					int xOffset = item.Location.X - beholder.X;
					int yOffset = item.Location.Y - beholder.Y;

					// this check solves the diagonal problem mentioned in the
					// previous function comments; we don't "cut" for the diagonals.

					if (Math.Abs(xOffset - yOffset) <= 1)
						continue;
				}

				if (ItemBlocksVis(item, eyelevel, beholder))
					v.ProcessObstructionAt(beholder, item.Location.X, item.Location.Y);
			}
			//}
			// double  stop = DateTime.Now.Ticks / 10000000.0;
			// double  elapsed = stop - start;
			// Console.WriteLine( "ProcessMatrixItems: " + elapsed );
		}
		//--------------------------------------------------------------------------
		//  Determine whether or not a tile is an 'invalid' one. These are the black
		//  tiles that sepearate the spaces between dungeons--they are intrinsically
		//  impassable, block everything.
		//--------------------------------------------------------------------------
		private static bool IsInvalidLandTile(LandTile tile)
		{
			int id = tile.ID;

			int[] invalidLandTiles = Map.InvalidLandTiles;

			for (int i = 0; i < invalidLandTiles.Length; i++)
				if (id == invalidLandTiles[i])
					return true;

			return false;
		}
		//--------------------------------------------------------------------------
		//  Determine wether or not a visible item is at the specified location on the
		//  map.
		//--------------------------------------------------------------------------
		private bool ContainsVisibleItem(int x, int y)
		{
			foreach (Item item in m_map.GetItemsInRange(new Point3D(x, y, 0), 0))
				if (item.Visible)
					return true;

			return false;
		}
		//--------------------------------------------------------------------------
		//  Does the item block my vision?
		//--------------------------------------------------------------------------
		public bool ItemBlocksVis(Item item, int eyelevel, Point3D beholder)
		{
			//----------------------------------------------------------------------
			//  invisible items dont block vis
			//----------------------------------------------------------------------
			if (!item.Visible)
				return false;
			//----------------------------------------------------------------------
			//  Anything above eyelevel does not block
			//----------------------------------------------------------------------
			if (item.Location.Z > eyelevel)
				return false;
			//----------------------------------------------------------------------
			//  multis don't block vis (their parts do, that's different)
			//----------------------------------------------------------------------
			if (item.ItemId >= 0x4000)
				return false;
			//----------------------------------------------------------------------
			//  whitelisted items don't block viz
			//----------------------------------------------------------------------
			if (Config.GetInstance().WhiteListed(item.ItemId | 0x4000))
				return false;

			//----------------------------------------------------------------------
			//  Windows
			//----------------------------------------------------------------------

			ItemData data = item.ItemData;
			int height = data.CalcHeight;
			int windowRange = Config.GetInstance().WindowRange;
			// Some tiles are windows but not flagged properly; we can tell by the name
			if ((data.Flags & TileFlag.Window) != 0 || item.Name == "window")
			{
				if (windowRange > 0)
				{
					if (Utility.InRange(item.Location, beholder, windowRange))
						return false;
					else
						return true;
				}
				else
					return false;
			}

			//----------------------------------------------------------------------
			//  Trees
			//----------------------------------------------------------------------

			if (Config.GetInstance().Tree(item.ItemId | 0x4000))
			{
				int treeRange = Config.GetInstance().TreeRange;

				if (treeRange > 0)
				{
					if (Utility.InRange(item.Location, beholder, treeRange))
						return false;
					else
						return true;
				}
				else
					return false;
			}

			//----------------------------------------------------------------------
			//  Walls
			//----------------------------------------------------------------------

			bool tallEnough = item.Location.Z + height >= eyelevel;

			if ((data.Flags & TileFlag.Wall) != 0)
			{
				//------------------------------------------------------------------
				//  high walls: walls that block LOS regardless of player Z
				//------------------------------------------------------------------
				if (Config.GetInstance().HighWalls)
				{
					// solid-wall
					if ((data.Flags & TileFlag.NoShoot) != 0)
					{
						if (Utility.InRange(item.Location, beholder, Config.GetInstance().EdgeRange))
						{
							if (tallEnough)
								return true;
							else
								return false;
						}

						return true;
					}
					// window-wall
					else
					{
						if (!tallEnough) return false; // window walls never infinitely high

						if (windowRange > 0)
						{
							if (Utility.InRange(item.Location, beholder, windowRange)) return false;
							else return true;
						}
						else return false;
					}
				}
				//------------------------------------------------------------------
				//  ordinary walls
				//------------------------------------------------------------------
				else
				{
					if (!tallEnough)
						return false;

					// solid-wall
					if ((data.Flags & TileFlag.NoShoot) != 0)
					{
						return true;
					}
					// window-wall
					else
					{
						if (windowRange > 0)
						{
							if (Utility.InRange(item.Location, beholder, windowRange))
								return false;
							else
								return true;
						}
						else
							return false;
					}
				}
			}

			//----------------------------------------------------------------------
			//  Other Physical Obstructions
			//----------------------------------------------------------------------

			if (!tallEnough)
				return false;

			if ((data.Flags & TileFlag.NoShoot) != 0)// is NOT see-thru
			{
				return true;
			}

			//----------------------------------------------------------------------
			//  Blacklisted Items
			//----------------------------------------------------------------------

			if (Config.GetInstance().BlackListed(item.ItemId))
			{
				return true;
			}

			//----------------------------------------------------------------------
			//  Otherwise, not:
			//----------------------------------------------------------------------

			return false;
		}
		//--------------------------------------------------------------------------
		//  Does the land block my vision?
		//--------------------------------------------------------------------------
		public bool LandBlocksVis(LandTile landTile, int x, int y, int eyelevel, Point3D beholder)
		{
			if (!Config.GetInstance().Mountain(landTile.ID))
			{
				return false;
			}

			//ItemData    data = TileData.ItemTable[landTile.ID];
			int minZ = 0, avgZ = 0, maxZ = 0;

			m_map.GetAverageZ(x, y, ref minZ, ref avgZ, ref maxZ);
			// for caves
			if (minZ <= eyelevel && maxZ >= eyelevel && !landTile.Ignored)
			{
				return true;
			}

			return false;
		}
		//--------------------------------------------------------------------------
		//   Does a tile block my vision?
		//--------------------------------------------------------------------------
		public bool TileBlocksVis(StaticTile tile, int eyelevel, int x, int y, Point3D beholder)
		{
			ItemData data = TileData.ItemTable[tile.ID & 0x3FFF];
			int height = data.CalcHeight;
			int windowRange = Config.GetInstance().WindowRange;

			//----------------------------------------------------------------------
			//  tiles starting higher than eyelevel don't block LOS
			//----------------------------------------------------------------------

			if (tile.Z > eyelevel)
				return false;

			//----------------------------------------------------------------------
			//  whitelisted tils dont block vis
			//----------------------------------------------------------------------

			if (Config.GetInstance().WhiteListed(tile.ID))
				return false;

			//----------------------------------------------------------------------
			//  Windows
			//----------------------------------------------------------------------
			// Some tiles are windows but not flagged properly; we can tell by the name
			if ((data.Flags & TileFlag.Window) != 0 || data.Name == "window")
			{
				if (windowRange > 0)
				{
					if (Utility.InRange(new Point3D(x, y, 0), beholder, windowRange))
						return false;
					else
						return true;
				}
				else
					return false;
			}

			//----------------------------------------------------------------------
			//  Trees
			//----------------------------------------------------------------------

			if (Config.GetInstance().Tree(tile.ID))
			{
				int treeRange = Config.GetInstance().TreeRange;

				if (treeRange > 0)
				{
					if (Utility.InRange(new Point3D(x, y, 0), beholder, treeRange))
						return false;
					else
						return true;
				}
				else
					return false;
			}

			//----------------------------------------------------------------------
			//  Walls
			//----------------------------------------------------------------------

			bool tallEnough = tile.Z + height >= eyelevel;

			if ((data.Flags & TileFlag.Wall) != 0)
			{
				//------------------------------------------------------------------
				//  high walls: walls that block LOS regardless of player Z
				//------------------------------------------------------------------
				if (Config.GetInstance().HighWalls)
				{
					// solid-wall
					if ((data.Flags & TileFlag.NoShoot) != 0)
					{
						if (Utility.InRange(new Point3D(x, y, 0), beholder, Config.GetInstance().EdgeRange))
						{
							if (tallEnough)
								return true;
							else
								return false;
						}

						return true;
					}
					// window-wall
					else
					{
						if (!tallEnough)
							return false; // window walls never infinitely high

						if (windowRange > 0)
						{
							if (Utility.InRange(new Point3D(x, y, 0), beholder, windowRange))
								return false;
							else
								return true;
						}
						else
							return false;
					}
				}
				//------------------------------------------------------------------
				//  ordinary walls
				//------------------------------------------------------------------
				else
				{
					if (!tallEnough)
						return false;

					// solid-wall
					if ((data.Flags & TileFlag.NoShoot) != 0)
					{
						return true;
					}
					// window-wall
					else
					{
						if (windowRange > 0)
						{
							if (Utility.InRange(new Point3D(x, y, 0), beholder, windowRange))
								return false;
							else
								return true;
						}
						else
							return false;
					}
				}
			}

			//----------------------------------------------------------------------
			//  Other Physical Obstructions
			//----------------------------------------------------------------------

			if (!tallEnough)
				return false;

			if ((data.Flags & TileFlag.NoShoot) != 0)// is NOT see-thru
			{
				return true;
			}

			//----------------------------------------------------------------------
			//  Blacklisted Items
			//----------------------------------------------------------------------

			if (Config.GetInstance().BlackListed(tile.ID))
			{
				return true;
			}

			//----------------------------------------------------------------------
			//  Otherwise, not:
			//----------------------------------------------------------------------

			return false;
		}
		//--------------------------------------------------------------------------
		//   Is this an invalid tile which also blocks vix?
		//--------------------------------------------------------------------------
		public bool IsVizBlockingInvalidLand(LandTile landTile, StaticTile[] staticTiles, int x, int y)
		{
			if (IsInvalidLandTile(landTile))
			{
				if (staticTiles.Length == 0)
				{
					if (!ContainsVisibleItem(x, y))
					{
						return true;
					}
				}
			}
			return false;
		}
		//--------------------------------------------------------------------------
		//  Cache warmup code
		//--------------------------------------------------------------------------
		public void Warmup()
		{
			Dictionary<string, Region> regions = m_map.Regions;
			Dictionary<int, Point3D> tiles = new();

			Console.WriteLine("LOS: WARMING UP");

			if (!Config.GetInstance().WarmupFacet(m_map.Name))
				return;

			foreach (Region region in regions.Values)
			{
				if (Config.GetInstance().WarmupRegion(m_map.Name, region.Name))
				{
					// if we match a whole top region, don't search for sub regions
					MarkRegionForWarmup(region, tiles);
				}
				else
				{
					// if we didn't match, keep going down the tree
					WarmupRecursive(region, tiles);
				}
			}

			ReallyWarmup(tiles);
		}

		public void WarmupRecursive(Region region, Dictionary<int, Point3D> tiles)
		{
			List<Region> children = region.Children;

			foreach (Region child in children)
			{
				if (Config.GetInstance().WarmupRegion(m_map.Name, child.Name))
				{
					// if we match a whole top region, don't search for sub regions
					MarkRegionForWarmup(child, tiles);
				}
				else
				{
					// if we didn't match, keep going down the tree
					WarmupRecursive(child, tiles);
				}
			}
		}

		public void MarkRegionForWarmup(Region region, Dictionary<int, Point3D> tiles)
		{
			Console.WriteLine("LOS: PREP-WARMING {0}: {1}", m_map.Name, region.Name);

			Rectangle3D[] areas = region.Area;

			foreach (Rectangle3D area in areas)
			{
				int width = area.Width;
				int height = area.Height;

				Point3D ul = area.Start;
				Point3D lr = area.End;

				for (int x = ul.X; x < lr.X; x++)
				{
					for (int y = ul.Y; y < lr.Y; y++)
					{
						//------------------------------------------------------------------
						//  Process land:
						//------------------------------------------------------------------

						LandTile landTile = m_map.Tiles.GetLandTile(x, y);

						ItemData landData = TileData.ItemTable[landTile.ID & 0x3FFF];

						if (landData.Impassable)
						{
							goto cutpoint;
						}

						//------------------------------------------------------------------
						//  Process statics
						//------------------------------------------------------------------

						StaticTile[] staticTiles = m_map.Tiles.GetStaticTiles(x, y, true);

						foreach (StaticTile staticTile in staticTiles)
						{
							ItemData staticData = TileData.ItemTable[staticTile.ID & 0x3FFF];

							if (staticData.Impassable || staticTile.Z > landTile.Z)
							{
								goto cutpoint;
							}
						}

						//------------------------------------------------------------------
						//  Process "invalid" tiles (black area between dungeons) 
						//------------------------------------------------------------------

						if (IsVizBlockingInvalidLand(landTile, staticTiles, x, y))
						{
							goto cutpoint;
						}

						//------------------------------------------------------------------
						//  Now look for impassable items
						//------------------------------------------------------------------

						foreach (Item item in m_map.GetItemsInRange(new Point3D(x, y, 0), 0))
						{
							if (item.ItemData.Impassable || item.Z > landTile.Z)
							{
								goto cutpoint;
							}
						}

						int key = x * area.Width + y;

						if (!tiles.ContainsKey(key)) tiles.Add(key, new Point3D(x, y, landTile.Z));
						//---------------------------------------------------------------------------
						cutpoint:; // Cutpoint: don't process cell if it's impassable in any way
								   //---------------------------------------------------------------------------
					}
				}
			}
		}

		public void ReallyWarmup(Dictionary<int, Point3D> tiles)
		{
			Console.WriteLine("LOS: Warming Up {0} POSSIBLE LOSSING POSITIONS....", tiles.Count);

			int count = 0;

			foreach (Point3D point in tiles.Values)
			{
				count++;

				int key = point.X * m_map.Width * m_map.Height + point.Y * m_map.Height + point.Z;

				VisibilityMatrix baseMatrix = m_cache.Hit(key);

				if (baseMatrix == null)
				{
					baseMatrix = CreateBaseMatrix(point);

					m_cache.Store(key, baseMatrix);
				}

				if (count % 2000 == 0)
					Console.WriteLine(" {0}%", count * 100 / tiles.Count);
				else if (count % 25 == 0)
					Console.Write(".");
			}
			Console.WriteLine(" **DONE**");
		}
		//--------------------------------------------------------------------------
		//  Debug helper
		//--------------------------------------------------------------------------
		public void Dump(Point3D loc)
		{
			//string key = m.Map.Name + ":" + m.Location.X + "," + m.Location.Y + "," + m.Location.Z;
			//VisibilityMatrix v = m_cache.Hit( key );
			//if( v == null ) v = CreateBaseMatrix( loc );
			//v.Dump();

			int key = loc.X * m_map.Width * m_map.Height + loc.Y * m_map.Height + loc.Z;

			VisibilityMatrix v = m_cache.Hit(key);

			if (v == null)
				v = CreateBaseMatrix(loc);

			for (int y = loc.Y - 15; y < loc.Y + 15; y++)
			{
				for (int x = loc.X - 15; x < loc.X + 15; x++)
				{
					Point3D target = new Point3D(x, y, loc.Z);
					bool visible = Visible(loc, target);

					Console.Write("" + (visible ? " -" : " #"));
				}
				Console.WriteLine("");
			}
		}
		//--------------------------------------------------------------------------
		//  Debug helper
		//--------------------------------------------------------------------------
		public void Viz(Point3D loc)
		{
			int key = loc.X * m_map.Width * m_map.Height + loc.Y * m_map.Height + loc.Z;

			VisibilityMatrix v = m_cache.Hit(key);

			if (v == null)
				v = CreateBaseMatrix(loc);

			for (int y = loc.Y - 15; y < loc.Y + 15; y++)
			{
				for (int x = loc.X - 15; x < loc.X + 15; x++)
				{
					Point3D target = new Point3D(x, y, loc.Z);
					bool visible = Visible(loc, target);

					Console.Write("" + (visible ? " V" : " -"));
				}
				Console.WriteLine("");
			}
		}
		//--------------------------------------------------------------------------
		//
		//--------------------------------------------------------------------------
		public void CacheInfo()
		{
			double hitRate = (double)m_cache.Hits / ((double)m_cache.Hits + (double)m_cache.Misses);

			Console.WriteLine("Cache Info");
			Console.WriteLine("    Nentries:   " + m_cache.Nentries);
			Console.WriteLine("    Size:      " + m_cache.Size);
			Console.WriteLine("    Hits:      " + m_cache.Hits);
			Console.WriteLine("    Misses:    " + m_cache.Misses);
			Console.WriteLine("    Hit Rate%  " + hitRate * 100.0);
			Console.WriteLine("    Ejections: " + m_cache.Ejections);
			Console.WriteLine("    Stores:    " + m_cache.Stores);
		}
		//--------------------------------------------------------------------------
		//  This is a spiral-from center pattern; we use it in the LOS system, because
		//  early LOS blockages allow us to "cut" away from doing further tests.
		//  We're most likely to encounter those blockages early by moving away from
		//  the beholder in a spiral (shadows are cast away from the center, so 
		//  we'll get the best performance by doing circular outward spiraling tests)
		//
		//  This code was generated by the genspiral.py file (no, I didn't write it!)
		//--------------------------------------------------------------------------
		internal static int[][] m_SpiralPattern = new int[][]
		{
		new int[] {   0,   1 },
		new int[] {   1,   1 },
		new int[] {   1,   0 },
		new int[] {   1,  -1 },
		new int[] {   0,  -1 },
		new int[] {  -1,  -1 },
		new int[] {  -1,   0 },
		new int[] {  -1,   1 },
		new int[] {  -1,   2 },
		new int[] {   0,   2 },
		new int[] {   1,   2 },
		new int[] {   2,   2 },
		new int[] {   2,   1 },
		new int[] {   2,   0 },
		new int[] {   2,  -1 },
		new int[] {   2,  -2 },
		new int[] {   1,  -2 },
		new int[] {   0,  -2 },
		new int[] {  -1,  -2 },
		new int[] {  -2,  -2 },
		new int[] {  -2,  -1 },
		new int[] {  -2,   0 },
		new int[] {  -2,   1 },
		new int[] {  -2,   2 },
		new int[] {  -2,   3 },
		new int[] {  -1,   3 },
		new int[] {   0,   3 },
		new int[] {   1,   3 },
		new int[] {   2,   3 },
		new int[] {   3,   3 },
		new int[] {   3,   2 },
		new int[] {   3,   1 },
		new int[] {   3,   0 },
		new int[] {   3,  -1 },
		new int[] {   3,  -2 },
		new int[] {   3,  -3 },
		new int[] {   2,  -3 },
		new int[] {   1,  -3 },
		new int[] {   0,  -3 },
		new int[] {  -1,  -3 },
		new int[] {  -2,  -3 },
		new int[] {  -3,  -3 },
		new int[] {  -3,  -2 },
		new int[] {  -3,  -1 },
		new int[] {  -3,   0 },
		new int[] {  -3,   1 },
		new int[] {  -3,   2 },
		new int[] {  -3,   3 },
		new int[] {  -3,   4 },
		new int[] {  -2,   4 },
		new int[] {  -1,   4 },
		new int[] {   0,   4 },
		new int[] {   1,   4 },
		new int[] {   2,   4 },
		new int[] {   3,   4 },
		new int[] {   4,   4 },
		new int[] {   4,   3 },
		new int[] {   4,   2 },
		new int[] {   4,   1 },
		new int[] {   4,   0 },
		new int[] {   4,  -1 },
		new int[] {   4,  -2 },
		new int[] {   4,  -3 },
		new int[] {   4,  -4 },
		new int[] {   3,  -4 },
		new int[] {   2,  -4 },
		new int[] {   1,  -4 },
		new int[] {   0,  -4 },
		new int[] {  -1,  -4 },
		new int[] {  -2,  -4 },
		new int[] {  -3,  -4 },
		new int[] {  -4,  -4 },
		new int[] {  -4,  -3 },
		new int[] {  -4,  -2 },
		new int[] {  -4,  -1 },
		new int[] {  -4,   0 },
		new int[] {  -4,   1 },
		new int[] {  -4,   2 },
		new int[] {  -4,   3 },
		new int[] {  -4,   4 },
		new int[] {  -4,   5 },
		new int[] {  -3,   5 },
		new int[] {  -2,   5 },
		new int[] {  -1,   5 },
		new int[] {   0,   5 },
		new int[] {   1,   5 },
		new int[] {   2,   5 },
		new int[] {   3,   5 },
		new int[] {   4,   5 },
		new int[] {   5,   5 },
		new int[] {   5,   4 },
		new int[] {   5,   3 },
		new int[] {   5,   2 },
		new int[] {   5,   1 },
		new int[] {   5,   0 },
		new int[] {   5,  -1 },
		new int[] {   5,  -2 },
		new int[] {   5,  -3 },
		new int[] {   5,  -4 },
		new int[] {   5,  -5 },
		new int[] {   4,  -5 },
		new int[] {   3,  -5 },
		new int[] {   2,  -5 },
		new int[] {   1,  -5 },
		new int[] {   0,  -5 },
		new int[] {  -1,  -5 },
		new int[] {  -2,  -5 },
		new int[] {  -3,  -5 },
		new int[] {  -4,  -5 },
		new int[] {  -5,  -5 },
		new int[] {  -5,  -4 },
		new int[] {  -5,  -3 },
		new int[] {  -5,  -2 },
		new int[] {  -5,  -1 },
		new int[] {  -5,   0 },
		new int[] {  -5,   1 },
		new int[] {  -5,   2 },
		new int[] {  -5,   3 },
		new int[] {  -5,   4 },
		new int[] {  -5,   5 },
		new int[] {  -5,   6 },
		new int[] {  -4,   6 },
		new int[] {  -3,   6 },
		new int[] {  -2,   6 },
		new int[] {  -1,   6 },
		new int[] {   0,   6 },
		new int[] {   1,   6 },
		new int[] {   2,   6 },
		new int[] {   3,   6 },
		new int[] {   4,   6 },
		new int[] {   5,   6 },
		new int[] {   6,   6 },
		new int[] {   6,   5 },
		new int[] {   6,   4 },
		new int[] {   6,   3 },
		new int[] {   6,   2 },
		new int[] {   6,   1 },
		new int[] {   6,   0 },
		new int[] {   6,  -1 },
		new int[] {   6,  -2 },
		new int[] {   6,  -3 },
		new int[] {   6,  -4 },
		new int[] {   6,  -5 },
		new int[] {   6,  -6 },
		new int[] {   5,  -6 },
		new int[] {   4,  -6 },
		new int[] {   3,  -6 },
		new int[] {   2,  -6 },
		new int[] {   1,  -6 },
		new int[] {   0,  -6 },
		new int[] {  -1,  -6 },
		new int[] {  -2,  -6 },
		new int[] {  -3,  -6 },
		new int[] {  -4,  -6 },
		new int[] {  -5,  -6 },
		new int[] {  -6,  -6 },
		new int[] {  -6,  -5 },
		new int[] {  -6,  -4 },
		new int[] {  -6,  -3 },
		new int[] {  -6,  -2 },
		new int[] {  -6,  -1 },
		new int[] {  -6,   0 },
		new int[] {  -6,   1 },
		new int[] {  -6,   2 },
		new int[] {  -6,   3 },
		new int[] {  -6,   4 },
		new int[] {  -6,   5 },
		new int[] {  -6,   6 },
		new int[] {  -6,   7 },
		new int[] {  -5,   7 },
		new int[] {  -4,   7 },
		new int[] {  -3,   7 },
		new int[] {  -2,   7 },
		new int[] {  -1,   7 },
		new int[] {   0,   7 },
		new int[] {   1,   7 },
		new int[] {   2,   7 },
		new int[] {   3,   7 },
		new int[] {   4,   7 },
		new int[] {   5,   7 },
		new int[] {   6,   7 },
		new int[] {   7,   7 },
		new int[] {   7,   6 },
		new int[] {   7,   5 },
		new int[] {   7,   4 },
		new int[] {   7,   3 },
		new int[] {   7,   2 },
		new int[] {   7,   1 },
		new int[] {   7,   0 },
		new int[] {   7,  -1 },
		new int[] {   7,  -2 },
		new int[] {   7,  -3 },
		new int[] {   7,  -4 },
		new int[] {   7,  -5 },
		new int[] {   7,  -6 },
		new int[] {   7,  -7 },
		new int[] {   6,  -7 },
		new int[] {   5,  -7 },
		new int[] {   4,  -7 },
		new int[] {   3,  -7 },
		new int[] {   2,  -7 },
		new int[] {   1,  -7 },
		new int[] {   0,  -7 },
		new int[] {  -1,  -7 },
		new int[] {  -2,  -7 },
		new int[] {  -3,  -7 },
		new int[] {  -4,  -7 },
		new int[] {  -5,  -7 },
		new int[] {  -6,  -7 },
		new int[] {  -7,  -7 },
		new int[] {  -7,  -6 },
		new int[] {  -7,  -5 },
		new int[] {  -7,  -4 },
		new int[] {  -7,  -3 },
		new int[] {  -7,  -2 },
		new int[] {  -7,  -1 },
		new int[] {  -7,   0 },
		new int[] {  -7,   1 },
		new int[] {  -7,   2 },
		new int[] {  -7,   3 },
		new int[] {  -7,   4 },
		new int[] {  -7,   5 },
		new int[] {  -7,   6 },
		new int[] {  -7,   7 },
		new int[] {  -7,   8 },
		new int[] {  -6,   8 },
		new int[] {  -5,   8 },
		new int[] {  -4,   8 },
		new int[] {  -3,   8 },
		new int[] {  -2,   8 },
		new int[] {  -1,   8 },
		new int[] {   0,   8 },
		new int[] {   1,   8 },
		new int[] {   2,   8 },
		new int[] {   3,   8 },
		new int[] {   4,   8 },
		new int[] {   5,   8 },
		new int[] {   6,   8 },
		new int[] {   7,   8 },
		new int[] {   8,   8 },
		new int[] {   8,   7 },
		new int[] {   8,   6 },
		new int[] {   8,   5 },
		new int[] {   8,   4 },
		new int[] {   8,   3 },
		new int[] {   8,   2 },
		new int[] {   8,   1 },
		new int[] {   8,   0 },
		new int[] {   8,  -1 },
		new int[] {   8,  -2 },
		new int[] {   8,  -3 },
		new int[] {   8,  -4 },
		new int[] {   8,  -5 },
		new int[] {   8,  -6 },
		new int[] {   8,  -7 },
		new int[] {   8,  -8 },
		new int[] {   7,  -8 },
		new int[] {   6,  -8 },
		new int[] {   5,  -8 },
		new int[] {   4,  -8 },
		new int[] {   3,  -8 },
		new int[] {   2,  -8 },
		new int[] {   1,  -8 },
		new int[] {   0,  -8 },
		new int[] {  -1,  -8 },
		new int[] {  -2,  -8 },
		new int[] {  -3,  -8 },
		new int[] {  -4,  -8 },
		new int[] {  -5,  -8 },
		new int[] {  -6,  -8 },
		new int[] {  -7,  -8 },
		new int[] {  -8,  -8 },
		new int[] {  -8,  -7 },
		new int[] {  -8,  -6 },
		new int[] {  -8,  -5 },
		new int[] {  -8,  -4 },
		new int[] {  -8,  -3 },
		new int[] {  -8,  -2 },
		new int[] {  -8,  -1 },
		new int[] {  -8,   0 },
		new int[] {  -8,   1 },
		new int[] {  -8,   2 },
		new int[] {  -8,   3 },
		new int[] {  -8,   4 },
		new int[] {  -8,   5 },
		new int[] {  -8,   6 },
		new int[] {  -8,   7 },
		new int[] {  -8,   8 },
		new int[] {  -8,   9 },
		new int[] {  -7,   9 },
		new int[] {  -6,   9 },
		new int[] {  -5,   9 },
		new int[] {  -4,   9 },
		new int[] {  -3,   9 },
		new int[] {  -2,   9 },
		new int[] {  -1,   9 },
		new int[] {   0,   9 },
		new int[] {   1,   9 },
		new int[] {   2,   9 },
		new int[] {   3,   9 },
		new int[] {   4,   9 },
		new int[] {   5,   9 },
		new int[] {   6,   9 },
		new int[] {   7,   9 },
		new int[] {   8,   9 },
		new int[] {   9,   9 },
		new int[] {   9,   8 },
		new int[] {   9,   7 },
		new int[] {   9,   6 },
		new int[] {   9,   5 },
		new int[] {   9,   4 },
		new int[] {   9,   3 },
		new int[] {   9,   2 },
		new int[] {   9,   1 },
		new int[] {   9,   0 },
		new int[] {   9,  -1 },
		new int[] {   9,  -2 },
		new int[] {   9,  -3 },
		new int[] {   9,  -4 },
		new int[] {   9,  -5 },
		new int[] {   9,  -6 },
		new int[] {   9,  -7 },
		new int[] {   9,  -8 },
		new int[] {   9,  -9 },
		new int[] {   8,  -9 },
		new int[] {   7,  -9 },
		new int[] {   6,  -9 },
		new int[] {   5,  -9 },
		new int[] {   4,  -9 },
		new int[] {   3,  -9 },
		new int[] {   2,  -9 },
		new int[] {   1,  -9 },
		new int[] {   0,  -9 },
		new int[] {  -1,  -9 },
		new int[] {  -2,  -9 },
		new int[] {  -3,  -9 },
		new int[] {  -4,  -9 },
		new int[] {  -5,  -9 },
		new int[] {  -6,  -9 },
		new int[] {  -7,  -9 },
		new int[] {  -8,  -9 },
		new int[] {  -9,  -9 },
		new int[] {  -9,  -8 },
		new int[] {  -9,  -7 },
		new int[] {  -9,  -6 },
		new int[] {  -9,  -5 },
		new int[] {  -9,  -4 },
		new int[] {  -9,  -3 },
		new int[] {  -9,  -2 },
		new int[] {  -9,  -1 },
		new int[] {  -9,   0 },
		new int[] {  -9,   1 },
		new int[] {  -9,   2 },
		new int[] {  -9,   3 },
		new int[] {  -9,   4 },
		new int[] {  -9,   5 },
		new int[] {  -9,   6 },
		new int[] {  -9,   7 },
		new int[] {  -9,   8 },
		new int[] {  -9,   9 },
		new int[] {  -9,  10 },
		new int[] {  -8,  10 },
		new int[] {  -7,  10 },
		new int[] {  -6,  10 },
		new int[] {  -5,  10 },
		new int[] {  -4,  10 },
		new int[] {  -3,  10 },
		new int[] {  -2,  10 },
		new int[] {  -1,  10 },
		new int[] {   0,  10 },
		new int[] {   1,  10 },
		new int[] {   2,  10 },
		new int[] {   3,  10 },
		new int[] {   4,  10 },
		new int[] {   5,  10 },
		new int[] {   6,  10 },
		new int[] {   7,  10 },
		new int[] {   8,  10 },
		new int[] {   9,  10 },
		new int[] {  10,  10 },
		new int[] {  10,   9 },
		new int[] {  10,   8 },
		new int[] {  10,   7 },
		new int[] {  10,   6 },
		new int[] {  10,   5 },
		new int[] {  10,   4 },
		new int[] {  10,   3 },
		new int[] {  10,   2 },
		new int[] {  10,   1 },
		new int[] {  10,   0 },
		new int[] {  10,  -1 },
		new int[] {  10,  -2 },
		new int[] {  10,  -3 },
		new int[] {  10,  -4 },
		new int[] {  10,  -5 },
		new int[] {  10,  -6 },
		new int[] {  10,  -7 },
		new int[] {  10,  -8 },
		new int[] {  10,  -9 },
		new int[] {  10, -10 },
		new int[] {   9, -10 },
		new int[] {   8, -10 },
		new int[] {   7, -10 },
		new int[] {   6, -10 },
		new int[] {   5, -10 },
		new int[] {   4, -10 },
		new int[] {   3, -10 },
		new int[] {   2, -10 },
		new int[] {   1, -10 },
		new int[] {   0, -10 },
		new int[] {  -1, -10 },
		new int[] {  -2, -10 },
		new int[] {  -3, -10 },
		new int[] {  -4, -10 },
		new int[] {  -5, -10 },
		new int[] {  -6, -10 },
		new int[] {  -7, -10 },
		new int[] {  -8, -10 },
		new int[] {  -9, -10 },
		new int[] { -10, -10 },
		new int[] { -10,  -9 },
		new int[] { -10,  -8 },
		new int[] { -10,  -7 },
		new int[] { -10,  -6 },
		new int[] { -10,  -5 },
		new int[] { -10,  -4 },
		new int[] { -10,  -3 },
		new int[] { -10,  -2 },
		new int[] { -10,  -1 },
		new int[] { -10,   0 },
		new int[] { -10,   1 },
		new int[] { -10,   2 },
		new int[] { -10,   3 },
		new int[] { -10,   4 },
		new int[] { -10,   5 },
		new int[] { -10,   6 },
		new int[] { -10,   7 },
		new int[] { -10,   8 },
		new int[] { -10,   9 },
		new int[] { -10,  10 },
		new int[] { -10,  11 },
		new int[] {  -9,  11 },
		new int[] {  -8,  11 },
		new int[] {  -7,  11 },
		new int[] {  -6,  11 },
		new int[] {  -5,  11 },
		new int[] {  -4,  11 },
		new int[] {  -3,  11 },
		new int[] {  -2,  11 },
		new int[] {  -1,  11 },
		new int[] {   0,  11 },
		new int[] {   1,  11 },
		new int[] {   2,  11 },
		new int[] {   3,  11 },
		new int[] {   4,  11 },
		new int[] {   5,  11 },
		new int[] {   6,  11 },
		new int[] {   7,  11 },
		new int[] {   8,  11 },
		new int[] {   9,  11 },
		new int[] {  10,  11 },
		new int[] {  11,  11 },
		new int[] {  11,  10 },
		new int[] {  11,   9 },
		new int[] {  11,   8 },
		new int[] {  11,   7 },
		new int[] {  11,   6 },
		new int[] {  11,   5 },
		new int[] {  11,   4 },
		new int[] {  11,   3 },
		new int[] {  11,   2 },
		new int[] {  11,   1 },
		new int[] {  11,   0 },
		new int[] {  11,  -1 },
		new int[] {  11,  -2 },
		new int[] {  11,  -3 },
		new int[] {  11,  -4 },
		new int[] {  11,  -5 },
		new int[] {  11,  -6 },
		new int[] {  11,  -7 },
		new int[] {  11,  -8 },
		new int[] {  11,  -9 },
		new int[] {  11, -10 },
		new int[] {  11, -11 },
		new int[] {  10, -11 },
		new int[] {   9, -11 },
		new int[] {   8, -11 },
		new int[] {   7, -11 },
		new int[] {   6, -11 },
		new int[] {   5, -11 },
		new int[] {   4, -11 },
		new int[] {   3, -11 },
		new int[] {   2, -11 },
		new int[] {   1, -11 },
		new int[] {   0, -11 },
		new int[] {  -1, -11 },
		new int[] {  -2, -11 },
		new int[] {  -3, -11 },
		new int[] {  -4, -11 },
		new int[] {  -5, -11 },
		new int[] {  -6, -11 },
		new int[] {  -7, -11 },
		new int[] {  -8, -11 },
		new int[] {  -9, -11 },
		new int[] { -10, -11 },
		new int[] { -11, -11 },
		new int[] { -11, -10 },
		new int[] { -11,  -9 },
		new int[] { -11,  -8 },
		new int[] { -11,  -7 },
		new int[] { -11,  -6 },
		new int[] { -11,  -5 },
		new int[] { -11,  -4 },
		new int[] { -11,  -3 },
		new int[] { -11,  -2 },
		new int[] { -11,  -1 },
		new int[] { -11,   0 },
		new int[] { -11,   1 },
		new int[] { -11,   2 },
		new int[] { -11,   3 },
		new int[] { -11,   4 },
		new int[] { -11,   5 },
		new int[] { -11,   6 },
		new int[] { -11,   7 },
		new int[] { -11,   8 },
		new int[] { -11,   9 },
		new int[] { -11,  10 },
		new int[] { -11,  11 },
		new int[] { -11,  12 },
		new int[] { -10,  12 },
		new int[] {  -9,  12 },
		new int[] {  -8,  12 },
		new int[] {  -7,  12 },
		new int[] {  -6,  12 },
		new int[] {  -5,  12 },
		new int[] {  -4,  12 },
		new int[] {  -3,  12 },
		new int[] {  -2,  12 },
		new int[] {  -1,  12 },
		new int[] {   0,  12 },
		new int[] {   1,  12 },
		new int[] {   2,  12 },
		new int[] {   3,  12 },
		new int[] {   4,  12 },
		new int[] {   5,  12 },
		new int[] {   6,  12 },
		new int[] {   7,  12 },
		new int[] {   8,  12 },
		new int[] {   9,  12 },
		new int[] {  10,  12 },
		new int[] {  11,  12 },
		new int[] {  12,  12 },
		new int[] {  12,  11 },
		new int[] {  12,  10 },
		new int[] {  12,   9 },
		new int[] {  12,   8 },
		new int[] {  12,   7 },
		new int[] {  12,   6 },
		new int[] {  12,   5 },
		new int[] {  12,   4 },
		new int[] {  12,   3 },
		new int[] {  12,   2 },
		new int[] {  12,   1 },
		new int[] {  12,   0 },
		new int[] {  12,  -1 },
		new int[] {  12,  -2 },
		new int[] {  12,  -3 },
		new int[] {  12,  -4 },
		new int[] {  12,  -5 },
		new int[] {  12,  -6 },
		new int[] {  12,  -7 },
		new int[] {  12,  -8 },
		new int[] {  12,  -9 },
		new int[] {  12, -10 },
		new int[] {  12, -11 },
		new int[] {  12, -12 },
		new int[] {  11, -12 },
		new int[] {  10, -12 },
		new int[] {   9, -12 },
		new int[] {   8, -12 },
		new int[] {   7, -12 },
		new int[] {   6, -12 },
		new int[] {   5, -12 },
		new int[] {   4, -12 },
		new int[] {   3, -12 },
		new int[] {   2, -12 },
		new int[] {   1, -12 },
		new int[] {   0, -12 },
		new int[] {  -1, -12 },
		new int[] {  -2, -12 },
		new int[] {  -3, -12 },
		new int[] {  -4, -12 },
		new int[] {  -5, -12 },
		new int[] {  -6, -12 },
		new int[] {  -7, -12 },
		new int[] {  -8, -12 },
		new int[] {  -9, -12 },
		new int[] { -10, -12 },
		new int[] { -11, -12 },
		new int[] { -12, -12 },
		new int[] { -12, -11 },
		new int[] { -12, -10 },
		new int[] { -12,  -9 },
		new int[] { -12,  -8 },
		new int[] { -12,  -7 },
		new int[] { -12,  -6 },
		new int[] { -12,  -5 },
		new int[] { -12,  -4 },
		new int[] { -12,  -3 },
		new int[] { -12,  -2 },
		new int[] { -12,  -1 },
		new int[] { -12,   0 },
		new int[] { -12,   1 },
		new int[] { -12,   2 },
		new int[] { -12,   3 },
		new int[] { -12,   4 },
		new int[] { -12,   5 },
		new int[] { -12,   6 },
		new int[] { -12,   7 },
		new int[] { -12,   8 },
		new int[] { -12,   9 },
		new int[] { -12,  10 },
		new int[] { -12,  11 },
		new int[] { -12,  12 },
		new int[] { -12,  13 },
		new int[] { -11,  13 },
		new int[] { -10,  13 },
		new int[] {  -9,  13 },
		new int[] {  -8,  13 },
		new int[] {  -7,  13 },
		new int[] {  -6,  13 },
		new int[] {  -5,  13 },
		new int[] {  -4,  13 },
		new int[] {  -3,  13 },
		new int[] {  -2,  13 },
		new int[] {  -1,  13 },
		new int[] {   0,  13 },
		new int[] {   1,  13 },
		new int[] {   2,  13 },
		new int[] {   3,  13 },
		new int[] {   4,  13 },
		new int[] {   5,  13 },
		new int[] {   6,  13 },
		new int[] {   7,  13 },
		new int[] {   8,  13 },
		new int[] {   9,  13 },
		new int[] {  10,  13 },
		new int[] {  11,  13 },
		new int[] {  12,  13 },
		new int[] {  13,  13 },
		new int[] {  13,  12 },
		new int[] {  13,  11 },
		new int[] {  13,  10 },
		new int[] {  13,   9 },
		new int[] {  13,   8 },
		new int[] {  13,   7 },
		new int[] {  13,   6 },
		new int[] {  13,   5 },
		new int[] {  13,   4 },
		new int[] {  13,   3 },
		new int[] {  13,   2 },
		new int[] {  13,   1 },
		new int[] {  13,   0 },
		new int[] {  13,  -1 },
		new int[] {  13,  -2 },
		new int[] {  13,  -3 },
		new int[] {  13,  -4 },
		new int[] {  13,  -5 },
		new int[] {  13,  -6 },
		new int[] {  13,  -7 },
		new int[] {  13,  -8 },
		new int[] {  13,  -9 },
		new int[] {  13, -10 },
		new int[] {  13, -11 },
		new int[] {  13, -12 },
		new int[] {  13, -13 },
		new int[] {  12, -13 },
		new int[] {  11, -13 },
		new int[] {  10, -13 },
		new int[] {   9, -13 },
		new int[] {   8, -13 },
		new int[] {   7, -13 },
		new int[] {   6, -13 },
		new int[] {   5, -13 },
		new int[] {   4, -13 },
		new int[] {   3, -13 },
		new int[] {   2, -13 },
		new int[] {   1, -13 },
		new int[] {   0, -13 },
		new int[] {  -1, -13 },
		new int[] {  -2, -13 },
		new int[] {  -3, -13 },
		new int[] {  -4, -13 },
		new int[] {  -5, -13 },
		new int[] {  -6, -13 },
		new int[] {  -7, -13 },
		new int[] {  -8, -13 },
		new int[] {  -9, -13 },
		new int[] { -10, -13 },
		new int[] { -11, -13 },
		new int[] { -12, -13 },
		new int[] { -13, -13 },
		new int[] { -13, -12 },
		new int[] { -13, -11 },
		new int[] { -13, -10 },
		new int[] { -13,  -9 },
		new int[] { -13,  -8 },
		new int[] { -13,  -7 },
		new int[] { -13,  -6 },
		new int[] { -13,  -5 },
		new int[] { -13,  -4 },
		new int[] { -13,  -3 },
		new int[] { -13,  -2 },
		new int[] { -13,  -1 },
		new int[] { -13,   0 },
		new int[] { -13,   1 },
		new int[] { -13,   2 },
		new int[] { -13,   3 },
		new int[] { -13,   4 },
		new int[] { -13,   5 },
		new int[] { -13,   6 },
		new int[] { -13,   7 },
		new int[] { -13,   8 },
		new int[] { -13,   9 },
		new int[] { -13,  10 },
		new int[] { -13,  11 },
		new int[] { -13,  12 },
		new int[] { -13,  13 },
		new int[] { -13,  14 },
		new int[] { -12,  14 },
		new int[] { -11,  14 },
		new int[] { -10,  14 },
		new int[] {  -9,  14 },
		new int[] {  -8,  14 },
		new int[] {  -7,  14 },
		new int[] {  -6,  14 },
		new int[] {  -5,  14 },
		new int[] {  -4,  14 },
		new int[] {  -3,  14 },
		new int[] {  -2,  14 },
		new int[] {  -1,  14 },
		new int[] {   0,  14 },
		new int[] {   1,  14 },
		new int[] {   2,  14 },
		new int[] {   3,  14 },
		new int[] {   4,  14 },
		new int[] {   5,  14 },
		new int[] {   6,  14 },
		new int[] {   7,  14 },
		new int[] {   8,  14 },
		new int[] {   9,  14 },
		new int[] {  10,  14 },
		new int[] {  11,  14 },
		new int[] {  12,  14 },
		new int[] {  13,  14 },
		new int[] {  14,  14 },
		new int[] {  14,  13 },
		new int[] {  14,  12 },
		new int[] {  14,  11 },
		new int[] {  14,  10 },
		new int[] {  14,   9 },
		new int[] {  14,   8 },
		new int[] {  14,   7 },
		new int[] {  14,   6 },
		new int[] {  14,   5 },
		new int[] {  14,   4 },
		new int[] {  14,   3 },
		new int[] {  14,   2 },
		new int[] {  14,   1 },
		new int[] {  14,   0 },
		new int[] {  14,  -1 },
		new int[] {  14,  -2 },
		new int[] {  14,  -3 },
		new int[] {  14,  -4 },
		new int[] {  14,  -5 },
		new int[] {  14,  -6 },
		new int[] {  14,  -7 },
		new int[] {  14,  -8 },
		new int[] {  14,  -9 },
		new int[] {  14, -10 },
		new int[] {  14, -11 },
		new int[] {  14, -12 },
		new int[] {  14, -13 },
		new int[] {  14, -14 },
		new int[] {  13, -14 },
		new int[] {  12, -14 },
		new int[] {  11, -14 },
		new int[] {  10, -14 },
		new int[] {   9, -14 },
		new int[] {   8, -14 },
		new int[] {   7, -14 },
		new int[] {   6, -14 },
		new int[] {   5, -14 },
		new int[] {   4, -14 },
		new int[] {   3, -14 },
		new int[] {   2, -14 },
		new int[] {   1, -14 },
		new int[] {   0, -14 },
		new int[] {  -1, -14 },
		new int[] {  -2, -14 },
		new int[] {  -3, -14 },
		new int[] {  -4, -14 },
		new int[] {  -5, -14 },
		new int[] {  -6, -14 },
		new int[] {  -7, -14 },
		new int[] {  -8, -14 },
		new int[] {  -9, -14 },
		new int[] { -10, -14 },
		new int[] { -11, -14 },
		new int[] { -12, -14 },
		new int[] { -13, -14 },
		new int[] { -14, -14 },
		new int[] { -14, -13 },
		new int[] { -14, -12 },
		new int[] { -14, -11 },
		new int[] { -14, -10 },
		new int[] { -14,  -9 },
		new int[] { -14,  -8 },
		new int[] { -14,  -7 },
		new int[] { -14,  -6 },
		new int[] { -14,  -5 },
		new int[] { -14,  -4 },
		new int[] { -14,  -3 },
		new int[] { -14,  -2 },
		new int[] { -14,  -1 },
		new int[] { -14,   0 },
		new int[] { -14,   1 },
		new int[] { -14,   2 },
		new int[] { -14,   3 },
		new int[] { -14,   4 },
		new int[] { -14,   5 },
		new int[] { -14,   6 },
		new int[] { -14,   7 },
		new int[] { -14,   8 },
		new int[] { -14,   9 },
		new int[] { -14,  10 },
		new int[] { -14,  11 },
		new int[] { -14,  12 },
		new int[] { -14,  13 },
		new int[] { -14,  14 },
		new int[] { -14,  15 },
		new int[] { -13,  15 },
		new int[] { -12,  15 },
		new int[] { -11,  15 },
		new int[] { -10,  15 },
		new int[] {  -9,  15 },
		new int[] {  -8,  15 },
		new int[] {  -7,  15 },
		new int[] {  -6,  15 },
		new int[] {  -5,  15 },
		new int[] {  -4,  15 },
		new int[] {  -3,  15 },
		new int[] {  -2,  15 },
		new int[] {  -1,  15 },
		new int[] {   0,  15 },
		new int[] {   1,  15 },
		new int[] {   2,  15 },
		new int[] {   3,  15 },
		new int[] {   4,  15 },
		new int[] {   5,  15 },
		new int[] {   6,  15 },
		new int[] {   7,  15 },
		new int[] {   8,  15 },
		new int[] {   9,  15 },
		new int[] {  10,  15 },
		new int[] {  11,  15 },
		new int[] {  12,  15 },
		new int[] {  13,  15 },
		new int[] {  14,  15 },
		new int[] {  15,  15 },
		new int[] {  15,  14 },
		new int[] {  15,  13 },
		new int[] {  15,  12 },
		new int[] {  15,  11 },
		new int[] {  15,  10 },
		new int[] {  15,   9 },
		new int[] {  15,   8 },
		new int[] {  15,   7 },
		new int[] {  15,   6 },
		new int[] {  15,   5 },
		new int[] {  15,   4 },
		new int[] {  15,   3 },
		new int[] {  15,   2 },
		new int[] {  15,   1 },
		new int[] {  15,   0 },
		new int[] {  15,  -1 },
		new int[] {  15,  -2 },
		new int[] {  15,  -3 },
		new int[] {  15,  -4 },
		new int[] {  15,  -5 },
		new int[] {  15,  -6 },
		new int[] {  15,  -7 },
		new int[] {  15,  -8 },
		new int[] {  15,  -9 },
		new int[] {  15, -10 },
		new int[] {  15, -11 },
		new int[] {  15, -12 },
		new int[] {  15, -13 },
		new int[] {  15, -14 },
		new int[] {  15, -15 },
		new int[] {  14, -15 },
		new int[] {  13, -15 },
		new int[] {  12, -15 },
		new int[] {  11, -15 },
		new int[] {  10, -15 },
		new int[] {   9, -15 },
		new int[] {   8, -15 },
		new int[] {   7, -15 },
		new int[] {   6, -15 },
		new int[] {   5, -15 },
		new int[] {   4, -15 },
		new int[] {   3, -15 },
		new int[] {   2, -15 },
		new int[] {   1, -15 },
		new int[] {   0, -15 },
		new int[] {  -1, -15 },
		new int[] {  -2, -15 },
		new int[] {  -3, -15 },
		new int[] {  -4, -15 },
		new int[] {  -5, -15 },
		new int[] {  -6, -15 },
		new int[] {  -7, -15 },
		new int[] {  -8, -15 },
		new int[] {  -9, -15 },
		new int[] { -10, -15 },
		new int[] { -11, -15 },
		new int[] { -12, -15 },
		new int[] { -13, -15 },
		new int[] { -14, -15 },
		new int[] { -15, -15 },
		new int[] { -15, -14 },
		new int[] { -15, -13 },
		new int[] { -15, -12 },
		new int[] { -15, -11 },
		new int[] { -15, -10 },
		new int[] { -15,  -9 },
		new int[] { -15,  -8 },
		new int[] { -15,  -7 },
		new int[] { -15,  -6 },
		new int[] { -15,  -5 },
		new int[] { -15,  -4 },
		new int[] { -15,  -3 },
		new int[] { -15,  -2 },
		new int[] { -15,  -1 },
		new int[] { -15,   0 },
		new int[] { -15,   1 },
		new int[] { -15,   2 },
		new int[] { -15,   3 },
		new int[] { -15,   4 },
		new int[] { -15,   5 },
		new int[] { -15,   6 },
		new int[] { -15,   7 },
		new int[] { -15,   8 },
		new int[] { -15,   9 },
		new int[] { -15,  10 },
		new int[] { -15,  11 },
		new int[] { -15,  12 },
		new int[] { -15,  13 },
		new int[] { -15,  14 },
		new int[] { -15,  15 },
			//        new int[] { -15,  16 },
			//        new int[] { -14,  16 },
			//        new int[] { -13,  16 },
			//        new int[] { -12,  16 },
			//        new int[] { -11,  16 },
			//        new int[] { -10,  16 },
			//        new int[] {  -9,  16 },
			//        new int[] {  -8,  16 },
			//        new int[] {  -7,  16 },
			//        new int[] {  -6,  16 },
			//        new int[] {  -5,  16 },
			//        new int[] {  -4,  16 },
			//        new int[] {  -3,  16 },
			//        new int[] {  -2,  16 },
			//        new int[] {  -1,  16 },
			//        new int[] {   0,  16 },
			//        new int[] {   1,  16 },
			//        new int[] {   2,  16 },
			//        new int[] {   3,  16 },
			//        new int[] {   4,  16 },
			//        new int[] {   5,  16 },
			//        new int[] {   6,  16 },
			//        new int[] {   7,  16 },
			//        new int[] {   8,  16 },
			//        new int[] {   9,  16 },
			//        new int[] {  10,  16 },
			//        new int[] {  11,  16 },
			//        new int[] {  12,  16 },
			//        new int[] {  13,  16 },
			//        new int[] {  14,  16 },
			//        new int[] {  15,  16 },
			//        new int[] {  16,  16 },
			//        new int[] {  16,  15 },
			//        new int[] {  16,  14 },
			//        new int[] {  16,  13 },
			//        new int[] {  16,  12 },
			//        new int[] {  16,  11 },
			//        new int[] {  16,  10 },
			//        new int[] {  16,   9 },
			//        new int[] {  16,   8 },
			//        new int[] {  16,   7 },
			//        new int[] {  16,   6 },
			//        new int[] {  16,   5 },
			//        new int[] {  16,   4 },
			//        new int[] {  16,   3 },
			//        new int[] {  16,   2 },
			//        new int[] {  16,   1 },
			//        new int[] {  16,   0 },
			//        new int[] {  16,  -1 },
			//        new int[] {  16,  -2 },
			//        new int[] {  16,  -3 },
			//        new int[] {  16,  -4 },
			//        new int[] {  16,  -5 },
			//        new int[] {  16,  -6 },
			//        new int[] {  16,  -7 },
			//        new int[] {  16,  -8 },
			//        new int[] {  16,  -9 },
			//        new int[] {  16, -10 },
			//        new int[] {  16, -11 },
			//        new int[] {  16, -12 },
			//        new int[] {  16, -13 },
			//        new int[] {  16, -14 },
			//        new int[] {  16, -15 },
			//        new int[] {  16, -16 },
			//        new int[] {  15, -16 },
			//        new int[] {  14, -16 },
			//        new int[] {  13, -16 },
			//        new int[] {  12, -16 },
			//        new int[] {  11, -16 },
			//        new int[] {  10, -16 },
			//        new int[] {   9, -16 },
			//        new int[] {   8, -16 },
			//        new int[] {   7, -16 },
			//        new int[] {   6, -16 },
			//        new int[] {   5, -16 },
			//        new int[] {   4, -16 },
			//        new int[] {   3, -16 },
			//        new int[] {   2, -16 },
			//        new int[] {   1, -16 },
			//        new int[] {   0, -16 },
			//        new int[] {  -1, -16 },
			//        new int[] {  -2, -16 },
			//        new int[] {  -3, -16 },
			//        new int[] {  -4, -16 },
			//        new int[] {  -5, -16 },
			//        new int[] {  -6, -16 },
			//        new int[] {  -7, -16 },
			//        new int[] {  -8, -16 },
			//        new int[] {  -9, -16 },
			//        new int[] { -10, -16 },
			//        new int[] { -11, -16 },
			//        new int[] { -12, -16 },
			//        new int[] { -13, -16 },
			//        new int[] { -14, -16 },
			//        new int[] { -15, -16 },
			//        new int[] { -16, -16 },
			//        new int[] { -16, -15 },
			//        new int[] { -16, -14 },
			//        new int[] { -16, -13 },
			//        new int[] { -16, -12 },
			//        new int[] { -16, -11 },
			//        new int[] { -16, -10 },
			//        new int[] { -16,  -9 },
			//        new int[] { -16,  -8 },
			//        new int[] { -16,  -7 },
			//        new int[] { -16,  -6 },
			//        new int[] { -16,  -5 },
			//        new int[] { -16,  -4 },
			//        new int[] { -16,  -3 },
			//        new int[] { -16,  -2 },
			//        new int[] { -16,  -1 },
			//        new int[] { -16,   0 },
			//        new int[] { -16,   1 },
			//        new int[] { -16,   2 },
			//        new int[] { -16,   3 },
			//        new int[] { -16,   4 },
			//        new int[] { -16,   5 },
			//        new int[] { -16,   6 },
			//        new int[] { -16,   7 },
			//        new int[] { -16,   8 },
			//        new int[] { -16,   9 },
			//        new int[] { -16,  10 },
			//        new int[] { -16,  11 },
			//        new int[] { -16,  12 },
			//        new int[] { -16,  13 },
			//        new int[] { -16,  14 },
			//        new int[] { -16,  15 },
			//        new int[] { -16,  16 },
			//        new int[] { -16,  17 },
			//        new int[] { -15,  17 },
			//        new int[] { -14,  17 },
			//        new int[] { -13,  17 },
			//        new int[] { -12,  17 },
			//        new int[] { -11,  17 },
			//        new int[] { -10,  17 },
			//        new int[] {  -9,  17 },
			//        new int[] {  -8,  17 },
			//        new int[] {  -7,  17 },
			//        new int[] {  -6,  17 },
			//        new int[] {  -5,  17 },
			//        new int[] {  -4,  17 },
			//        new int[] {  -3,  17 },
			//        new int[] {  -2,  17 },
			//        new int[] {  -1,  17 },
			//        new int[] {   0,  17 },
			//        new int[] {   1,  17 },
			//        new int[] {   2,  17 },
			//        new int[] {   3,  17 },
			//        new int[] {   4,  17 },
			//        new int[] {   5,  17 },
			//        new int[] {   6,  17 },
			//        new int[] {   7,  17 },
			//        new int[] {   8,  17 },
			//        new int[] {   9,  17 },
			//        new int[] {  10,  17 },
			//        new int[] {  11,  17 },
			//        new int[] {  12,  17 },
			//        new int[] {  13,  17 },
			//        new int[] {  14,  17 },
			//        new int[] {  15,  17 },
			//        new int[] {  16,  17 },
			//        new int[] {  17,  17 },
			//        new int[] {  17,  16 },
			//        new int[] {  17,  15 },
			//        new int[] {  17,  14 },
			//        new int[] {  17,  13 },
			//        new int[] {  17,  12 },
			//        new int[] {  17,  11 },
			//        new int[] {  17,  10 },
			//        new int[] {  17,   9 },
			//        new int[] {  17,   8 },
			//        new int[] {  17,   7 },
			//        new int[] {  17,   6 },
			//        new int[] {  17,   5 },
			//        new int[] {  17,   4 },
			//        new int[] {  17,   3 },
			//        new int[] {  17,   2 },
			//        new int[] {  17,   1 },
			//        new int[] {  17,   0 },
			//        new int[] {  17,  -1 },
			//        new int[] {  17,  -2 },
			//        new int[] {  17,  -3 },
			//        new int[] {  17,  -4 },
			//        new int[] {  17,  -5 },
			//        new int[] {  17,  -6 },
			//        new int[] {  17,  -7 },
			//        new int[] {  17,  -8 },
			//        new int[] {  17,  -9 },
			//        new int[] {  17, -10 },
			//        new int[] {  17, -11 },
			//        new int[] {  17, -12 },
			//        new int[] {  17, -13 },
			//        new int[] {  17, -14 },
			//        new int[] {  17, -15 },
			//        new int[] {  17, -16 },
			//        new int[] {  17, -17 },
			//        new int[] {  16, -17 },
			//        new int[] {  15, -17 },
			//        new int[] {  14, -17 },
			//        new int[] {  13, -17 },
			//        new int[] {  12, -17 },
			//        new int[] {  11, -17 },
			//        new int[] {  10, -17 },
			//        new int[] {   9, -17 },
			//        new int[] {   8, -17 },
			//        new int[] {   7, -17 },
			//        new int[] {   6, -17 },
			//        new int[] {   5, -17 },
			//        new int[] {   4, -17 },
			//        new int[] {   3, -17 },
			//        new int[] {   2, -17 },
			//        new int[] {   1, -17 },
			//        new int[] {   0, -17 },
			//        new int[] {  -1, -17 },
			//        new int[] {  -2, -17 },
			//        new int[] {  -3, -17 },
			//        new int[] {  -4, -17 },
			//        new int[] {  -5, -17 },
			//        new int[] {  -6, -17 },
			//        new int[] {  -7, -17 },
			//        new int[] {  -8, -17 },
			//        new int[] {  -9, -17 },
			//        new int[] { -10, -17 },
			//        new int[] { -11, -17 },
			//        new int[] { -12, -17 },
			//        new int[] { -13, -17 },
			//        new int[] { -14, -17 },
			//        new int[] { -15, -17 },
			//        new int[] { -16, -17 },
			//        new int[] { -17, -17 },
			//        new int[] { -17, -16 },
			//        new int[] { -17, -15 },
			//        new int[] { -17, -14 },
			//        new int[] { -17, -13 },
			//        new int[] { -17, -12 },
			//        new int[] { -17, -11 },
			//        new int[] { -17, -10 },
			//        new int[] { -17,  -9 },
			//        new int[] { -17,  -8 },
			//        new int[] { -17,  -7 },
			//        new int[] { -17,  -6 },
			//        new int[] { -17,  -5 },
			//        new int[] { -17,  -4 },
			//        new int[] { -17,  -3 },
			//        new int[] { -17,  -2 },
			//        new int[] { -17,  -1 },
			//        new int[] { -17,   0 },
			//        new int[] { -17,   1 },
			//        new int[] { -17,   2 },
			//        new int[] { -17,   3 },
			//        new int[] { -17,   4 },
			//        new int[] { -17,   5 },
			//        new int[] { -17,   6 },
			//        new int[] { -17,   7 },
			//        new int[] { -17,   8 },
			//        new int[] { -17,   9 },
			//        new int[] { -17,  10 },
			//        new int[] { -17,  11 },
			//        new int[] { -17,  12 },
			//        new int[] { -17,  13 },
			//        new int[] { -17,  14 },
			//        new int[] { -17,  15 },
			//        new int[] { -17,  16 },
			//        new int[] { -17,  17 },
			//        new int[] { -17,  18 },
			//        new int[] { -16,  18 },
			//        new int[] { -15,  18 },
			//        new int[] { -14,  18 },
			//        new int[] { -13,  18 },
			//        new int[] { -12,  18 },
			//        new int[] { -11,  18 },
			//        new int[] { -10,  18 },
			//        new int[] {  -9,  18 },
			//        new int[] {  -8,  18 },
			//        new int[] {  -7,  18 },
			//        new int[] {  -6,  18 },
			//        new int[] {  -5,  18 },
			//        new int[] {  -4,  18 },
			//        new int[] {  -3,  18 },
			//        new int[] {  -2,  18 },
			//        new int[] {  -1,  18 },
			//        new int[] {   0,  18 },
			//        new int[] {   1,  18 },
			//        new int[] {   2,  18 },
			//        new int[] {   3,  18 },
			//        new int[] {   4,  18 },
			//        new int[] {   5,  18 },
			//        new int[] {   6,  18 },
			//        new int[] {   7,  18 },
			//        new int[] {   8,  18 },
			//        new int[] {   9,  18 },
			//        new int[] {  10,  18 },
			//        new int[] {  11,  18 },
			//        new int[] {  12,  18 },
			//        new int[] {  13,  18 },
			//        new int[] {  14,  18 },
			//        new int[] {  15,  18 },
			//        new int[] {  16,  18 },
			//        new int[] {  17,  18 },
			//        new int[] {  18,  18 },
			//        new int[] {  18,  17 },
			//        new int[] {  18,  16 },
			//        new int[] {  18,  15 },
			//        new int[] {  18,  14 },
			//        new int[] {  18,  13 },
			//        new int[] {  18,  12 },
			//        new int[] {  18,  11 },
			//        new int[] {  18,  10 },
			//        new int[] {  18,   9 },
			//        new int[] {  18,   8 },
			//        new int[] {  18,   7 },
			//        new int[] {  18,   6 },
			//        new int[] {  18,   5 },
			//        new int[] {  18,   4 },
			//        new int[] {  18,   3 },
			//        new int[] {  18,   2 },
			//        new int[] {  18,   1 },
			//        new int[] {  18,   0 },
			//        new int[] {  18,  -1 },
			//        new int[] {  18,  -2 },
			//        new int[] {  18,  -3 },
			//        new int[] {  18,  -4 },
			//        new int[] {  18,  -5 },
			//        new int[] {  18,  -6 },
			//        new int[] {  18,  -7 },
			//        new int[] {  18,  -8 },
			//        new int[] {  18,  -9 },
			//        new int[] {  18, -10 },
			//        new int[] {  18, -11 },
			//        new int[] {  18, -12 },
			//        new int[] {  18, -13 },
			//        new int[] {  18, -14 },
			//        new int[] {  18, -15 },
			//        new int[] {  18, -16 },
			//        new int[] {  18, -17 },
			//        new int[] {  18, -18 },
			//        new int[] {  17, -18 },
			//        new int[] {  16, -18 },
			//        new int[] {  15, -18 },
			//        new int[] {  14, -18 },
			//        new int[] {  13, -18 },
			//        new int[] {  12, -18 },
			//        new int[] {  11, -18 },
			//        new int[] {  10, -18 },
			//        new int[] {   9, -18 },
			//        new int[] {   8, -18 },
			//        new int[] {   7, -18 },
			//        new int[] {   6, -18 },
			//        new int[] {   5, -18 },
			//        new int[] {   4, -18 },
			//        new int[] {   3, -18 },
			//        new int[] {   2, -18 },
			//        new int[] {   1, -18 },
			//        new int[] {   0, -18 },
			//        new int[] {  -1, -18 },
			//        new int[] {  -2, -18 },
			//        new int[] {  -3, -18 },
			//        new int[] {  -4, -18 },
			//        new int[] {  -5, -18 },
			//        new int[] {  -6, -18 },
			//        new int[] {  -7, -18 },
			//        new int[] {  -8, -18 },
			//        new int[] {  -9, -18 },
			//        new int[] { -10, -18 },
			//        new int[] { -11, -18 },
			//        new int[] { -12, -18 },
			//        new int[] { -13, -18 },
			//        new int[] { -14, -18 },
			//        new int[] { -15, -18 },
			//        new int[] { -16, -18 },
			//        new int[] { -17, -18 },
			//        new int[] { -18, -18 },
		};
	}
}
