using Server.Network;

namespace Server.Targeting;

public abstract class MultiTarget : Target
{
	public int MultiId { get; set; }
	public Point3D Offset { get; set; }

	protected MultiTarget(int multiId, Point3D offset)
		: this(multiId, offset, 10, true, TargetFlags.None)
	{
	}

	protected MultiTarget(int multiId, Point3D offset, int range, bool allowGround, TargetFlags flags)
		: base(range, allowGround, flags)
	{
		MultiId = multiId;
		Offset = offset;
	}

	public override Packet GetPacketFor(NetState ns)
	{
		if (ns.HighSeas)
			return new MultiTargetReqHS(this);
		return new MultiTargetReq(this);
	}
}
