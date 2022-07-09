using Server.Gumps;
using Server.Network;
using System.Collections.Generic;

namespace Server;

public delegate void OnVirtueUsed(Mobile from);

public class VirtueGump : Gump
{
	private static readonly Dictionary<int, OnVirtueUsed> m_Callbacks = new();

	public static void Initialize()
	{
		EventSink.OnVirtueGumpRequest += EventSink_VirtueGumpRequest;
		EventSink.OnVirtueItemRequest += EventSink_VirtueItemRequest;
		EventSink.OnVirtueMacroRequest += EventSink_VirtueMacroRequest;
	}

	public static void Register(int gumpId, OnVirtueUsed callback)
	{
		m_Callbacks[gumpId] = callback;
	}

	private static void EventSink_VirtueItemRequest(Mobile beholder, Mobile beheld, int gumpId)
	{
		if (beholder != beheld)
			return;

		beholder.CloseGump(typeof(VirtueGump));

		if (beholder.Murderer)
		{
			beholder.SendLocalizedMessage(1049609); // Murderers cannot invoke this virtue.
			return;
		}


		m_Callbacks.TryGetValue(gumpId, out OnVirtueUsed callback);

		if (callback != null)
			callback(beholder);
		else
			beholder.SendLocalizedMessage(1052066); // That virtue is not active yet.
	}


	private static void EventSink_VirtueMacroRequest(Mobile mob, int virtueId)
	{
		var virtueid = virtueId switch
		{
			0 => // Honor
				107,
			1 => // Sacrifice
				110,
			2 => // Valor;
				112,
			_ => 0
		};

		EventSink_VirtueItemRequest(mob, mob, virtueid);
	}

	private static void EventSink_VirtueGumpRequest(Mobile beholder, Mobile beheld)
	{
		if (beholder == beheld && beholder.Murderer)
		{
			beholder.SendLocalizedMessage(1049609); // Murderers cannot invoke this virtue.
		}
		else if (beholder.Map == beheld.Map && beholder.InRange(beheld, 12))
		{
			beholder.CloseGump(typeof(VirtueGump));
			beholder.SendGump(new VirtueGump(beholder, beheld));
		}
	}

	private readonly Mobile _beholder, _beheld;

	public VirtueGump(Mobile beholder, Mobile beheld) : base(0, 0)
	{
		_beholder = beholder;
		_beheld = beheld;

		Serial = beheld.Serial;

		AddPage(0);

		AddImage(30, 40, 104);

		AddPage(1);

		Add(new InternalEntry(61, 71, 108, GetHueFor(0))); // Humility
		Add(new InternalEntry(123, 46, 112, GetHueFor(4))); // Valor
		Add(new InternalEntry(187, 70, 107, GetHueFor(5))); // Honor
		Add(new InternalEntry(35, 135, 110, GetHueFor(1))); // Sacrifice
		Add(new InternalEntry(211, 133, 105, GetHueFor(2))); // Compassion
		Add(new InternalEntry(61, 195, 111, GetHueFor(3))); // Spiritulaity
		Add(new InternalEntry(186, 195, 109, GetHueFor(6))); // Justice
		Add(new InternalEntry(121, 221, 106, GetHueFor(7))); // Honesty

		if (_beholder == _beheld)
		{
			AddButton(57, 269, 2027, 2027, 1, GumpButtonType.Reply, 0);
			AddButton(186, 269, 2071, 2071, 2, GumpButtonType.Reply, 0);
		}
	}

	private static readonly int[] m_Table = new int[]
	{
		0x0481, 0x0963, 0x0965,
		0x060A, 0x060F, 0x002A,
		0x08A4, 0x08A7, 0x0034,
		0x0965, 0x08FD, 0x0480,
		0x00EA, 0x0845, 0x0020,
		0x0011, 0x0269, 0x013D,
		0x08A1, 0x08A3, 0x0042,
		0x0543, 0x0547, 0x0061
	};

	private int GetHueFor(int index)
	{
		if (_beheld.Virtues.GetValue(index) == 0)
			return 2402;

		int value = _beheld.Virtues.GetValue(index);

		switch (value)
		{
			case < 4000:
				return 2402;
			case >= 30000:
				value = 20000;  //Sanity
				break;
		}


		int vl;

		switch (value)
		{
			case < 10000:
				vl = 0;
				break;
			case >= 20000 when index == 5:
			case >= 21000 when index != 1:
			case >= 22000 when true:
				vl = 2;
				break;
			default:
				vl = 1;
				break;
		}


		return m_Table[index * 3 + vl];
	}

	private class InternalEntry : GumpImage
	{
		public InternalEntry(int x, int y, int gumpId, int hue) : base(x, y, gumpId, hue)
		{
		}

		public override string Compile()
		{
			return $"{{ gumppic {X} {Y} {GumpID} hue={Hue} class=VirtueGumpItem }}";
		}

		private static readonly byte[] m_Class = StringToBuffer(" class=VirtueGumpItem");

		public override void AppendTo(IGumpWriter disp)
		{
			base.AppendTo(disp);

			disp.AppendLayout(m_Class);
		}
	}

	public override void OnResponse(NetState state, RelayInfo info)
	{
		if (info.ButtonID == 1 && _beholder == _beheld)
			_beholder.SendGump(new VirtueStatusGump(_beholder));
	}
}
