using Server.Mobiles;
using System;

namespace Server.Engines.XmlSpawner2
{
	public class XmlDeathAction : XmlAttachment
	{
		[CommandProperty(AccessLevel.GameMaster)]
		public string Action { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public string Condition { get; set; }

		// These are the various ways in which the message attachment can be constructed.  
		// These can be called via the [addatt interface, via scripts, via the spawner ATTACH keyword.
		// Other overloads could be defined to handle other types of arguments

		// a serial constructor is REQUIRED
		public XmlDeathAction(ASerial serial)
			: base(serial)
		{
		}

		[Attachable]
		public XmlDeathAction(string action)
		{
			Action = action;
		}

		[Attachable]
		public XmlDeathAction()
		{
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(Condition);
			writer.Write(Action);

		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
			switch (version)
			{
				case 0:
					Condition = reader.ReadString();
					Action = reader.ReadString();
					break;
			}
		}

		public override void OnAttach()
		{
			base.OnAttach();

			if (AttachedTo is Item)
			{
				Delete();
			}
		}

		public override bool HandlesOnKilled => true;

		public override void OnKilled(Mobile killed, Mobile killer)
		{
			base.OnKilled(killed, killer);

			if (killed == null)
				return;

			// now check for any conditions as well
			// check for any condition that must be met for this entry to be processed
			if (Condition != null)
			{
				if (!BaseXmlSpawner.CheckPropertyString(null, killed, Condition, killer, out string status_str))
				{
					return;
				}
			}

			ExecuteDeathActions(killed.Corpse, killer, Action);
		}

		private void ExecuteDeathActions(Item corpse, Mobile killer, string actions)
		{
			if (actions == null || actions.Length <= 0)
				return;
			// execute any action associated with it
			// allow for multiple action strings on a single line separated by a semicolon

			string[] args = actions.Split(';');

			for (int j = 0; j < args.Length; j++)
			{
				ExecuteDeathAction(corpse, killer, args[j]);
			}

		}

		private static void ExecuteDeathAction(Item corpse, Mobile killer, string action)
		{
			if (action == null || action.Length <= 0 || corpse == null)
				return;
			Mobiles.XmlSpawner.SpawnObject TheSpawn = new(null, 0)
			{
				TypeName = action
			};
			string substitutedtypeName = BaseXmlSpawner.ApplySubstitution(null, corpse, killer, action);
			string typeName = BaseXmlSpawner.ParseObjectType(substitutedtypeName);

			Point3D loc = corpse.Location;
			Map map = corpse.Map;


			string status_str;
			if (BaseXmlSpawner.IsTypeOrItemKeyword(typeName))
			{
				BaseXmlSpawner.SpawnTypeKeyword(corpse, TheSpawn, typeName, substitutedtypeName, true, killer, loc, map, out status_str);
			}
			else
			{
				// its a regular type descriptor so find out what it is
				Type type = SpawnerType.GetType(typeName);
				try
				{
					string[] arglist = BaseXmlSpawner.ParseString(substitutedtypeName, 3, "/");
					object o = Mobiles.XmlSpawner.CreateObject(type, arglist[0]);

					if (o == null)
					{
						status_str = "invalid type specification: " + arglist[0];
					}
					else if (o is Mobile m)
					{
						if (m is BaseCreature c)
						{
							c.Home = loc; // Spawners location is the home point
						}

						m.Location = loc;
						m.Map = map;

						BaseXmlSpawner.ApplyObjectStringProperties(null, substitutedtypeName, m, killer, corpse, out status_str);
					}
					else if (o is Item item)
					{
						BaseXmlSpawner.AddSpawnItem(null, corpse, TheSpawn, item, loc, map, killer, false, substitutedtypeName, out status_str);
					}
				}
				catch { }
			}
		}

	}
}
