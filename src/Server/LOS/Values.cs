using System;

namespace Server.LOS
{
	//--------------------------------------------------------------------------------
	//  Values class; encompasses static types found only on the scripts side of thing
	//                as well as various constants.
	//--------------------------------------------------------------------------------
	public class Values
	{
		public static readonly Type Corpse = Assembler.FindTypeByFullName("Server.Items.Corpse");
	}
}
