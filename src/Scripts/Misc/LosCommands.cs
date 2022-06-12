//--------------------------------------------------------------------------------
// Copyright Joe Kraska, 2006. This file is restricted according to the GPL.
// Terms and conditions can be found in COPYING.txt.
//--------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Commands.Generic;
using Server.Items;
using Server.Mobiles;
using Server.LOS;
using Server.Targeting;
using Server.Collections;
using Server.Regions;
//--------------------------------------------------------------------------------
//  Warning: this code won't function when compiled directly into the core (which
//    it is right now). This is a remnant from a previous utility. Please retain,
//    pending fix up...
//--------------------------------------------------------------------------------
namespace Server.Scripts.Commands
{
    public class LosControl
    {
        public static void Initialize()
        { 
            Server.Commands.CommandSystem.Register( "los", AccessLevel.Player, new CommandEventHandler( OnCommand ) );
        }
 
        public static void OnCommand( CommandEventArgs e )
        {
            if( e.Length == 1 )
            {
                switch( e.Arguments[0] )
                {
                    case "on":

                        On( e );
                        break;

                    case "off":

                        Off( e );
                        break;

                    case "init":

                        Init( e );
                        break;

                    case "cacheinfo":

                        CacheInfo( e );
                        break;

                    case "list":

                        LosList( e );
                        break;

                    case "listtarget":

                        LosListTarget( e );
                        break;

                    case "warmup":

                        Warmup( e );
                        break;

                    case "los":

                        Los( e );
                        break;

//                    case "blocks":
//
//                        Blocks( e );
//                        break;

                    case "effect":

                        Effect( e );
                        break;

//                    case "gen":
//
//                        Gen( e );
//                        break;

                }
            }
        }
        public static void On( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            LineOfSight     los = map.LOS;

            Console.WriteLine("LOS: Turned on by command");
            Config.GetInstance().On = true;
        }
        public static void Off( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            LineOfSight     los = map.LOS;

            Console.WriteLine("LOS: Turned off by command");
            Config.GetInstance().On = false;
        }
        public static void Init( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            LineOfSight     los = map.LOS;

            Console.WriteLine("LOS: system reinitialization");

            Config.Clear();
            Config.GetInstance();

            foreach( Map m in Map.AllMaps )
                m.LOS.Clear();
        }
        public static void CacheInfo( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            LineOfSight     los = map.LOS;

            los.CacheInfo( );
        }


        private class ListTarget :  Target
        {
            public ListTarget() : base ( -1, true, TargetFlags.None )
            {
            }

            protected override void OnTarget( Mobile from, object o )
            {
                if( !BaseCommand.IsAccessible( from, o ) )
                {
                    from.SendMessage( "That is not accessible." );
                }
                else if( o is Mobile )
                {
                    Mobile                          mob = (Mobile) o;

                    Dictionary<Object,Object>       losCurrent = mob.LosCurrent;

                    Console.WriteLine("Los visibility list for {0}:", from.Name);

                    foreach ( Object key in losCurrent.Keys )
                    {
                        if( key is Mobile )
                        {
                            Mobile m = (Mobile) key;
                            Console.WriteLine("    Mobile: {0}, {1}", m.Name, m.Serial.Value);
                        }
                    }
                }
                else from.SendMessage( "Can only target a mobile." );
            }
        }

        public static void LosListTarget( CommandEventArgs e )
        {
            e.Mobile.Target = new ListTarget();
        }

        public static void LosList( CommandEventArgs e )
        {
            Mobile                          mob = e.Mobile;
            Dictionary<Object,Object>       losCurrent = mob.LosCurrent;

            Console.WriteLine("Los visibility list for {0}:", mob.Name);

            foreach ( Object o in losCurrent.Keys )
            {
                //if( o is Item )
                //{
                //    Item i = (Item) o;
                //    Console.WriteLine("    Item  : {0}, {1}", i.Name, i.Serial.Value);
                //}
                if( o is Mobile )
                {
                    Mobile m = (Mobile) o;
                    Console.WriteLine("    Mobile: {0}, {1}", m.Name, m.Serial.Value);
                }
            }
        }
        public static void Warmup( CommandEventArgs e )
        {
            Mobile                      mob = e.Mobile;
            Map                         map = mob.Map;
            map.LOS.Warmup();
        }

        public static void Los( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            Point3D         loc = mob.Location;
            LineOfSight     los = map.LOS;

            Console.WriteLine("You are at ({0},{1}).", loc.X, loc.Y);
            Console.WriteLine("*** NORTH IS UP. THIS IS UO UPPER RIGHT. ***");

            los.Viz( loc );
        }
        public static void Blocks( CommandEventArgs e )
        {
            Mobile      mob = e.Mobile;
            Map         map = mob.Map;
            Point3D     loc = mob.Location;
            LineOfSight     los = map.LOS;

            Console.WriteLine("You are at ({0},{1}).", loc.X, loc.Y);
            Console.WriteLine("*** NORTH IS UP. THIS IS UO UPPER RIGHT. ***");

            los.Dump( loc );
        }
        public static void Effect( CommandEventArgs e )
        {
            Mobile          mob = e.Mobile;
            Map             map = mob.Map;
            Point3D         loc = mob.Location;
            LineOfSight     los = map.LOS;

            //Console.WriteLine("You are at ({0},{1}).", loc.X, loc.Y);
            //los.ResetVis( loc );

            for( int x = loc.X-15; x < loc.X+15; x++ ) 
            {
                for( int y = loc.Y-15; y < loc.Y+15; y++ ) 
                {
                    LandTile landTile  = map.Tiles.GetLandTile( x, y );

                    Point3D target = new Point3D( x, y, mob.Location.Z );
                    if( los.Visible( mob.Location, target ) )
                        Effects.SendLocationParticles( 
                            EffectItem.Create( new Point3D( x, y, landTile.Z), map, EffectItem.DefaultDuration ), 
                            0x37CC, 1, 40, 96, 3, 9917, 0 
                            );
                    else
                        Effects.SendLocationParticles( 
                            EffectItem.Create( new Point3D( x, y, landTile.Z), map, EffectItem.DefaultDuration ), 
                            0x37CC, 1, 40, 36, 3, 9917, 0 
                            );
                }
            }
        }
        public static void Gen( CommandEventArgs e )
        {
            try
            { 
                CodeGenMain ms = new CodeGenMain ( 31 );
                ms.Execute();
            }
            catch( Exception ex )
            {
                Console.WriteLine( ex );
            }
        }
    }
}
