using Server.Network;

namespace Server.Gumps
{
    public class EcHandleInput : GumpEntry
    {
	    public override string Compile()
        {
            return "{ echandleinput }";
        }

        private static readonly byte[] MLayoutName = Gump.StringToBuffer("echandleinput");

        public override void AppendTo(IGumpWriter disp)
        {
            disp.AppendLayout(MLayoutName);
        }
    }
}
