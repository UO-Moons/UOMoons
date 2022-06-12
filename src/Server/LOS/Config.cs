using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Server.LOS
{
	//--------------------------------------------------------------------------------
	//  Reinitialize Timer: checks periodically to see if the config file has
	//     been written; reinitializes system if it has.
	//--------------------------------------------------------------------------------
	// 
	//  Left out because an auto read in of the config would cause a matrix cache
	//  flush. On larger shards, that will cause an immediate lag that would better
	//  confined to a server restart.
	//
	//    public class ReinitializeTimer : Timer
	//    {
	//        private DateTime        m_lastWriteTime;
	//
	//        public ReinitializeTimer( ) : base( TimeSpan.FromSeconds( 0 ), TimeSpan.FromSeconds( 15 ) )
	//        { 
	//            m_lastWriteTime = File.GetLastWriteTime( RandomEncounterEngine.EncountersFile );
	//        }
	//
	//        protected override void OnTick()
	//        {
	//            DateTime        checkTime = File.GetLastWriteTime( Config.ConfigFile );
	//
	//            if( checkTime > m_lastWriteTime )
	//            {
	//                Console.WriteLine("LOS: rereading config file at {0}", Config.ConfigFile);
	//                
	//                // need to do foreach map map.Los.Clear() here.
	//
	//                Config.Clear();
	//
	//                Config config = Config.GetInstance();
	//
	//                m_lastWriteTime = checkTime;
	//            }
	//        }
	//    }
	//--------------------------------------------------------------------------------
	//  Config class; parses the xml file and establishes a hastable of tiles that
	//   are not to block line of sight.
	//--------------------------------------------------------------------------------
	public class Config
	{
		private readonly static string m_ConfigFile = "./Data/LOS/Config.xml";
		private static Config m_OnlyInstance = null;
		private readonly Dictionary<int, int> m_NotLossed;
		private readonly Dictionary<int, int> m_WhiteListed;
		private readonly Dictionary<int, int> m_BlackListed;
		private readonly Dictionary<int, int> m_Trees;
		private readonly Dictionary<int, int> m_Mountains;

		private readonly Dictionary<string, Dictionary<string, string>> m_Warmups;

		private readonly Dictionary<string, string> m_FacetsOn;

		public static string ConfigFile { get { return m_ConfigFile; } }

		public bool RTFM { get; set; }
		public bool On { get; set; }
		public bool Mobiles { get; private set; }
		public bool Items { get; private set; }
		public bool LosForMobs { get; private set; }
		public bool Symmetric { get; private set; }
		public bool HighWalls { get; private set; }
		public int SquelchNames { get; private set; }
		public int EdgeRange { get; private set; }
		public int WindowRange { get; private set; }
		public int TreeRange { get; private set; }
		public int CacheRatio { get; private set; }
		public bool BackingStore { get; private set; }

		public bool NotLossed(int tile) { if (m_NotLossed.ContainsKey(tile)) return true; else return false; }
		public bool WhiteListed(int tile) { if (m_WhiteListed.ContainsKey(tile)) return true; else return false; }
		public bool BlackListed(int tile) { if (m_BlackListed.ContainsKey(tile)) return true; else return false; }
		public bool Tree(int tile) { if (m_Trees.ContainsKey(tile)) return true; else return false; }
		public bool Mountain(int tile) { if (m_Mountains.ContainsKey(tile)) return true; else return false; }

		public bool WarmupFacet(string facet)
		{
			if (!m_FacetsOn.ContainsKey(facet)) return false;

			if (m_Warmups.ContainsKey(facet)) return true;

			return false;
		}

		public bool WarmupRegion(string facet, string region)
		{
			if (!WarmupFacet(facet)) return false;

			if (region == null) return false;

			if (m_Warmups[facet].ContainsKey(region)) return true;

			return false;
		}

		public bool FacetOn(string name)
		{
			return m_FacetsOn.ContainsKey(name);
		}

		public static Config GetInstance()
		{
			if (m_OnlyInstance == null) m_OnlyInstance = new Config();

			return m_OnlyInstance;
		}

		public static void Clear()
		{
			m_OnlyInstance = null;
		}

		private Config()
		{
			m_FacetsOn = new Dictionary<string, string>();
			m_NotLossed = new Dictionary<int, int>();
			m_WhiteListed = new Dictionary<int, int>();
			m_BlackListed = new Dictionary<int, int>();
			m_Trees = new Dictionary<int, int>();
			m_Mountains = new Dictionary<int, int>();
			m_Warmups = new Dictionary<string, Dictionary<string, string>>();

			Console.WriteLine("LOS: Configuration system initializing");

			if (!MaybeLoadXml()) Console.WriteLine("#### LOS: Configuration system failed initialization");

			if (RTFM == false)
			{
				Console.WriteLine(
					"\n" +
					"#### LOS: FATAL. *drum roll*.... could it BE??!?!\n" +
					"####             You haven't read and removed the RTFM=\"false\" tag in the config file!!!!\n" +
					"####             It is suggested you go read and modify your config file now.\n"
					);

				throw new Exception("### LOS FATAL--RTFM");
			}
		}
		private bool MaybeLoadXml()
		{
			if (!System.IO.File.Exists(m_ConfigFile))
			{
				Console.WriteLine(
					"\n" +
					"#### LOS: FATAL. You haven't put the Config.xml file in the right spot.\n" +
					"####             It was being looked for in Data/Los/Config.xml\n"
					);

				throw new Exception("### LOS FATAL--CONFIG");
			}

			return LoadXml();
		}
		//----------------------------------------------------------------------
		// Actual XML load-out and node iteration
		//----------------------------------------------------------------------
		private bool LoadXml()
		{
			XmlLinePreservingDocument xmlDoc = new(m_ConfigFile);

			try
			{
				xmlDoc.DoLoad();

				string rtfm = "true";
				string on = "false";
				string facetsOn = "Felucca";
				string mobiles = "true";
				string items = "true";
				string losForMobs = "true";
				string symmetric = "false";
				string highWalls = "true";
				string squelchNames = "-1";
				string edgeRange = "1";
				string windowRange = "1";
				string treeRange = "1";
				string cacheRatio = "100";
				string backingStore = "true";

				XmlNode root = xmlDoc["LOS"];

				try { rtfm = root.Attributes["RTFM"].Value; } catch { }
				try { on = root.Attributes["on"].Value; } catch { }
				try { facetsOn = root.Attributes["facets"].Value; } catch { }
				try { mobiles = root.Attributes["mobiles"].Value; } catch { }
				try { items = root.Attributes["items"].Value; } catch { }
				try { losForMobs = root.Attributes["losForMobs"].Value; } catch { }
				try { symmetric = root.Attributes["symmetric"].Value; } catch { }
				try { highWalls = root.Attributes["highWalls"].Value; } catch { }
				try { squelchNames = root.Attributes["squelchNames"].Value; } catch { }
				try { edgeRange = root.Attributes["edgeRange"].Value; } catch { }
				try { windowRange = root.Attributes["windowRange"].Value; } catch { }
				try { treeRange = root.Attributes["treeRange"].Value; } catch { }
				try { cacheRatio = root.Attributes["cacheRatio"].Value; } catch { }
				try { backingStore = root.Attributes["backingStore"].Value; } catch { }

				RTFM = bool.Parse(rtfm);
				On = bool.Parse(on);
				Mobiles = bool.Parse(mobiles);
				Items = bool.Parse(items);
				LosForMobs = bool.Parse(losForMobs);
				Symmetric = bool.Parse(symmetric);
				HighWalls = bool.Parse(highWalls);
				SquelchNames = int.Parse(squelchNames);
				EdgeRange = int.Parse(edgeRange);
				WindowRange = int.Parse(windowRange);
				TreeRange = int.Parse(treeRange);
				CacheRatio = int.Parse(cacheRatio);
				BackingStore = bool.Parse(backingStore);

				string[] facets = facetsOn.Split(new Char[] { ',' });

				foreach (string facet in facets)
				{
					m_FacetsOn.Add(facet, facet);
				}

				//--------------------------------------------------------------
				// Load various tile sets
				//--------------------------------------------------------------

				LoadTileList(root, "NotLossed", NumberStyles.HexNumber, m_NotLossed);
				LoadTileList(root, "WhiteList", NumberStyles.HexNumber, m_WhiteListed);
				LoadTileList(root, "BlackList", NumberStyles.HexNumber, m_BlackListed);
				LoadTileList(root, "Trees", NumberStyles.HexNumber, m_Trees);
				LoadTileList(root, "Mountains", NumberStyles.None, m_Mountains);

				LoadWarmups(root);

				return true;
			}
			catch (Exception e)
			{
				Console.WriteLine("LOS: Exception encountered attempting to load file: " + m_ConfigFile);
				Console.WriteLine("{0}", e);
				xmlDoc.Close();
				return false;
			}
		}
		//----------------------------------------------------------------------
		//----------------------------------------------------------------------
		private static void LoadTileList(XmlNode parent, string listName, NumberStyles style, Dictionary<int, int> loadInto)
		{
			XmlNodeList treeNodes = parent.SelectNodes("./" + listName);

			if (treeNodes.Count == 1)
			{
				Console.WriteLine("LOS: Loading {0} ...", listName);
				XmlNode whiteNode = treeNodes[0];

				XmlNodeList listedTileNodes = whiteNode.SelectNodes("./Tile");

				//if (listedTileNodes.Count==0) Console.WriteLine( "LOS: There appeared to be NO {0} TILES defined in the configuration file!", listName);
				//------------------------------------------------------------------
				// Iterate over tiles
				//------------------------------------------------------------------
				foreach (XmlNode tileNode in listedTileNodes)
				{
					string str_id = "";

					try { str_id = tileNode.Attributes["id"].Value; }
					catch
					{
						Console.WriteLine(
							"LOS: Tile at {1} had no id. THIS IS ILLEGAL. IGNORED ENTIRE TILE!",
							tileNode.Attributes["lineNumber"].Value
							);
						continue;
					}

					int id = Int16.Parse(str_id, style);

					if (loadInto.ContainsKey(id))
						Console.WriteLine("LOS: ignoring duplicate key \"{0}\" in list {1}", id, listName);
					else
						loadInto.Add(id, id);
				}
			}
			//else Console.WriteLine("LOS: Warning, there appeared to be no {0} defined in the config file.", listName);
		}
		//----------------------------------------------------------------------
		//----------------------------------------------------------------------
		private void LoadWarmups(XmlNode parent)
		{
			XmlNodeList warmupNodes = parent.SelectNodes("./Warmups");

			if (warmupNodes.Count == 1)
			{
				Console.WriteLine("LOS: Loading Warmup Specifications...");
				XmlNode warmupNode = warmupNodes[0];

				XmlNodeList warmups = warmupNode.SelectNodes("./Warmup");

				//if (warmups.Count==0) Console.WriteLine( "LOS: There appeared to be NO WARMUP defined in the configuration file!");
				//------------------------------------------------------------------
				// Iterate over tiles
				//------------------------------------------------------------------
				foreach (XmlNode warmup in warmups)
				{
					string map = "";
					string region = "";

					try { map = warmup.Attributes["map"].Value; }
					catch
					{
						Console.WriteLine(
							"LOS: Warmup at {1} had no \"map\" attr. THIS IS ILLEGAL. IGNORED ENTIRE WARMUP!",
							warmup.Attributes["lineNumber"].Value
							);
						continue;
					}

					try { region = warmup.Attributes["region"].Value; }
					catch
					{
						Console.WriteLine(
							"LOS: Warmup at {1} had no \"region\" attr. THIS IS ILLEGAL. IGNORED ENTIRE WARMUP!",
							warmup.Attributes["lineNumber"].Value
							);
						continue;
					}

					Dictionary<string, string> regionHash;

					if (m_Warmups.ContainsKey(map))
					{
						regionHash = m_Warmups[map];
					}
					else
					{
						regionHash = new Dictionary<string, string>();
						m_Warmups.Add(map, regionHash);
					}

					regionHash.Add(region, region);
				}
			}
			//else Console.WriteLine("LOS: Warning, there appeared to be no Warmups section defined in the config file.");
		}
	}
}
