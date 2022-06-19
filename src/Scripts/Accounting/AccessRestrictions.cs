using Server.Misc;
using System;
using System.IO;
using System.Net;

namespace Server;

public interface IAccountRestricted
{
	string Account { get; set; }
}

public class AccessRestrictions
{
	public static void Initialize()
	{
		EventSink.SocketConnect += EventSink_SocketConnect;
	}

	private static void EventSink_SocketConnect(SocketConnectEventArgs e)
	{
		try
		{
			var ip = ((IPEndPoint)e.Socket.RemoteEndPoint)?.Address;

			if (Firewall.IsBlocked(ip))
			{
				Console.WriteLine("Client: {0}: Firewall blocked connection attempt.", ip);
				e.AllowConnection = false;
			}
			else if (IpLimiter.SocketBlock && !IpLimiter.Verify(ip))
			{
				Console.WriteLine("Client: {0}: Past IP limit threshold", ip);

				using (StreamWriter op = new("ipLimits.log", true))
					op.WriteLine("{0}\tPast IP limit threshold\t{1}", ip, DateTime.UtcNow);

				e.AllowConnection = false;
			}
		}
		catch
		{
			e.AllowConnection = false;
		}
	}
}
