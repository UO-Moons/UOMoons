using Server.Commands;
using Server.Engines.XmlSpawner2;
using Server.Gumps;
using Server.Network;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Server.Mobiles
{
	public class XmlGetAttGump : Gump
	{
		private const int MaxEntries = 18;
		private const int MaxEntriesPerPage = 18;

		private readonly object m_TargetObject;

		private readonly bool m_Dosearchtype;
		private readonly bool m_Dosearchname;

		private readonly bool m_Dosearchage;

		private readonly bool m_Searchagedirection;
		private double m_Searchage;

		private readonly string m_Searchtype;
		private readonly string m_Searchname;

		private bool m_Sorttype;

		private bool m_Sortname;

		private readonly Mobile m_From;

		private bool m_Descendingsort;
		private int m_Selected;
		private int m_DisplayFrom;
		private readonly bool[] m_SelectionList;

		private bool m_SelectAll;

		private readonly List<XmlAttachment> m_SearchList;

		public static void Initialize()
		{
			CommandSystem.Register("XmlGetAtt", AccessLevel.GameMaster, XmlGetAtt_OnCommand);
		}
		/*
		private bool TestAge(object o)
		{
			if (Searchage <= 0) return true;

			if (o is XmlAttachment)
			{
				XmlAttachment a = (XmlAttachment)o;

				if (Searchagedirection)
				{
					// true means allow only mobs greater than the age
					if ((DateTime.UtcNow - a.CreationTime) > TimeSpan.FromHours(Searchage)) return true;
				}
				else
				{
					// false means allow only mobs less than the age
					if ((DateTime.UtcNow - a.CreationTime) < TimeSpan.FromHours(Searchage)) return true;
				}

			}
			return false;
		}
		*/

		private List<XmlAttachment> Search(object target, out string statusStr)
		{
			statusStr = null;
			List<XmlAttachment> newarray = new();
			Type targetType = null;
			// if the type is specified then get the search type
			if (m_Dosearchtype && m_Searchtype != null)
			{
				targetType = SpawnerType.GetType(m_Searchtype);
				if (targetType == null)
				{
					statusStr = "Invalid type: " + m_Searchtype;
					return newarray;
				}
			}

			List<XmlAttachment> attachments = XmlAttach.FindAttachments(target);

			// do the search through attachments
			if (attachments == null)
				return newarray;

			foreach (var i in attachments)
			{
				var hastype = false;
				var hasname = false;

				if (i == null || i.Deleted)
					continue;


				// check for type
				if (targetType != null && m_Dosearchtype && (i.GetType().IsSubclassOf(targetType) || i.GetType() == targetType))
				{
					hastype = true;
				}
				if (m_Dosearchtype && !hastype)
					continue;

				// check for name
				if (m_Dosearchname && i.Name != null && m_Searchname != null && (i.Name.ToLower().Contains(m_Searchname.ToLower(), StringComparison.CurrentCulture)))
				{
					hasname = true;
				}
				if (m_Dosearchname && !hasname)
					continue;


				// satisfied all conditions so add it
				newarray.Add(i);
			}

			return newarray;
		}

		private class GetAttachTarget : Target
		{
			private readonly CommandEventArgs m_E;

			public GetAttachTarget(CommandEventArgs e) : base(30, false, TargetFlags.None)
			{
				m_E = e;

			}
			protected override void OnTarget(Mobile from, object targeted)
			{
				if (from == null || targeted == null)
					return;

				from.SendGump(new XmlGetAttGump(from, targeted, 0, 0));
			}
		}

		[Usage("XmlGetAtt")]
		[Description("Gets attachments on an object")]
		public static void XmlGetAtt_OnCommand(CommandEventArgs e)
		{
			e.Mobile.Target = new GetAttachTarget(e);
		}

		public XmlGetAttGump(Mobile from, object targeted, int x, int y) : this(from, targeted, true, false,
		false, false, false,
		null, null, false, 0,
		null, -1, 0,
		false, false,
		false, null, x, y)
		{

		}

		public XmlGetAttGump(Mobile from, object targeted, bool firststart, bool descend,
			bool dosearchtype, bool dosearchname, bool dosearchage,
			string searchtype, string searchname, bool searchagedirection, double searchage,
			List<XmlAttachment> searchlist, int selected, int displayfrom,
			bool sorttype, bool sortname,
			bool selectall, bool[] selectionlist, int x, int y) : base(x, y)
		{

			m_TargetObject = targeted;
			m_From = from;
			m_SelectionList = selectionlist ?? new bool[MaxEntries];
			m_SelectAll = selectall;
			m_Sorttype = sorttype;
			m_Sortname = sortname;

			m_DisplayFrom = displayfrom;
			m_Selected = selected;

			m_Descendingsort = descend;
			m_Dosearchtype = dosearchtype;
			m_Dosearchname = dosearchname;
			m_Dosearchage = dosearchage;

			m_Searchagedirection = searchagedirection;

			m_Searchage = searchage;
			m_Searchtype = searchtype;
			m_Searchname = searchname;

			m_SearchList = searchlist;

			if (firststart)
			{
				m_SearchList = Search(m_TargetObject, out _);
			}

			// prepare the page

			AddPage(0);

			AddBackground(0, 0, 640, 474, 5054);
			AddAlphaRegion(0, 0, 640, 474);

			var tnamestr = targeted switch
			{
				Item item => item.Name,
				Mobile mobile => mobile.Name,
				_ => null
			};
			AddLabel(2, 0, 0x33, $"Attachments on {targeted.GetType().Name} : {tnamestr}");

			// add the Sort button
			AddButton(5, 450, 0xFAB, 0xFAD, 700, GumpButtonType.Reply, 0);
			AddLabel(38, 450, 0x384, "Sort");

			// add the sort direction button
			if (m_Descendingsort)
			{
				AddButton(75, 453, 0x15E2, 0x15E6, 701, GumpButtonType.Reply, 0);
				AddLabel(100, 450, 0x384, "descend");
			}
			else
			{
				AddButton(75, 453, 0x15E0, 0x15E4, 701, GumpButtonType.Reply, 0);
				AddLabel(100, 450, 0x384, "ascend");
			}

			// add the Sort on type toggle
			AddRadio(155, 450, 0xD2, 0xD3, m_Sorttype, 0);
			AddLabel(155, 425, 0x384, "type");

			// add the Sort on name toggle
			AddRadio(200, 450, 0xD2, 0xD3, m_Sortname, 1);
			AddLabel(200, 425, 0x384, "name");


			AddLabel(42, 13, 0x384, "Name");
			AddLabel(145, 13, 0x384, "Type");
			AddLabel(285, 13, 0x384, "Created");
			AddLabel(425, 13, 0x384, "Expires In");
			AddLabel(505, 13, 0x384, "Attached By");

			// add the Delete button
			AddButton(250, 450, 0xFB1, 0xFB3, 156, GumpButtonType.Reply, 0);
			AddLabel(283, 450, 0x384, "Delete");


			// add the page buttons
			for (var i = 0; i < MaxEntries / MaxEntriesPerPage; i++)
			{
				//AddButton( 38+i*30, 365, 2206, 2206, 0, GumpButtonType.Page, 1+i );
				AddButton(418 + i * 25, 450, 0x8B1 + i, 0x8B1 + i, 0, GumpButtonType.Page, 1 + i);
			}

			// add the advance pageblock buttons
			AddButton(415 + 25 * (MaxEntries / MaxEntriesPerPage), 450, 0x15E1, 0x15E5, 201, GumpButtonType.Reply, 0); // block forward
			AddButton(395, 450, 0x15E3, 0x15E7, 202, GumpButtonType.Reply, 0); // block backward

			// add the displayfrom entry
			AddLabel(460, 450, 0x384, "Display");
			AddImageTiled(500, 450, 60, 21, 0xBBC);
			AddTextEntry(501, 450, 60, 21, 0, 400, m_DisplayFrom.ToString());
			AddButton(560, 450, 0xFAB, 0xFAD, 9998, GumpButtonType.Reply, 0);

			// display the item list
			if (m_SearchList != null)
			{
				AddLabel(320, 425, 68, $"Found {m_SearchList.Count} attachments");
				AddLabel(500, 425, 68,
					$"Displaying {m_DisplayFrom}-{(m_DisplayFrom + MaxEntries < m_SearchList.Count ? m_DisplayFrom + MaxEntries : m_SearchList.Count)}");
			}

			// display the select-all-displayed toggle
			AddButton(620, 5, 0xD2, 0xD3, 3999, GumpButtonType.Reply, 0);

			// display the select-all toggle
			AddButton(600, 5, (m_SelectAll ? 0xD3 : 0xD2), (m_SelectAll ? 0xD2 : 0xD3), 3998, GumpButtonType.Reply, 0);

			for (var i = 0; i < MaxEntries; i++)
			{
				var index = i + m_DisplayFrom;
				if (m_SearchList == null || index >= m_SearchList.Count) break;
				var page = i / MaxEntriesPerPage;
				if (i % MaxEntriesPerPage == 0)
				{
					AddPage(page + 1);
				}

				// background for search results area
				//AddImageTiled( 235, 22 * (i%MaxEntriesPerPage)  + 30, 386, 23, 0x52 );
				//AddImageTiled( 236, 22 * (i%MaxEntriesPerPage) + 31, 384, 21, 0xBBC );

				// add the Props button for each entry
				AddButton(5, 22 * (i % MaxEntriesPerPage) + 30, 0xFAB, 0xFAD, 3000 + i, GumpButtonType.Reply, 0);

				string namestr = null;
				string typestr = null;
				string expirestr = null;
				//string description = null;
				string attachedby = null;
				string created = null;

				var texthue = 0;

				object o = m_SearchList[index];

				if (o is XmlAttachment)
				{
					var a = m_SearchList[index];

					namestr = a.Name;
					typestr = a.GetType().Name;
					expirestr = a.Expiration.ToString();
					//description = a.OnIdentify(m_From);
					created = a.CreationTime.ToString(CultureInfo.InvariantCulture);
					attachedby = a.AttachedBy;
				}

				bool sel = false;
				if (m_SelectionList != null && i < m_SelectionList.Length)
				{
					sel = m_SelectionList[i];
				}
				if (sel) texthue = 33;

				if (i == m_Selected) texthue = 68;

				// display the name
				AddImageTiled(36, 22 * (i % MaxEntriesPerPage) + 31, 102, 21, 0xBBC);
				AddLabelCropped(38, 22 * (i % MaxEntriesPerPage) + 31, 100, 21, texthue, namestr);

				// display the type
				AddImageTiled(140, 22 * (i % MaxEntriesPerPage) + 31, 133, 21, 0xBBC);
				AddLabelCropped(140, 22 * (i % MaxEntriesPerPage) + 31, 133, 21, texthue, typestr);

				// display the creation time
				AddImageTiled(275, 22 * (i % MaxEntriesPerPage) + 31, 138, 21, 0xBBC);
				AddLabelCropped(275, 22 * (i % MaxEntriesPerPage) + 31, 138, 21, texthue, created);

				// display the expiration
				AddImageTiled(415, 22 * (i % MaxEntriesPerPage) + 31, 78, 21, 0xBBC);
				AddLabelCropped(415, 22 * (i % MaxEntriesPerPage) + 31, 78, 21, texthue, expirestr);

				// display the attachedby
				AddImageTiled(495, 22 * (i % MaxEntriesPerPage) + 31, 125, 21, 0xBBC);
				AddLabelCropped(495, 22 * (i % MaxEntriesPerPage) + 31, 105, 21, texthue, attachedby);

				// display the descriptio button
				AddButton(600, 22 * (i % MaxEntriesPerPage) + 32, 0x5689, 0x568A, 5000 + i, GumpButtonType.Reply, 0);

				// display the selection button
				AddButton(620, 22 * (i % MaxEntriesPerPage) + 32, (sel ? 0xD3 : 0xD2), (sel ? 0xD2 : 0xD3), 4000 + i, GumpButtonType.Reply, 0);
			}
		}


		private void DoShowProps(int index)
		{
			if (m_From == null || m_From.Deleted) return;

			if (index >= m_SearchList.Count)
				return;

			var x = m_SearchList[index];
			if (x == null || x.Deleted) return;

			m_From.SendGump(new PropertiesGump(m_From, x));
		}

		private void SortFindList()
		{
			if (m_SearchList is not {Count: > 0})
				return;

			if (m_Sorttype)
			{
				m_SearchList.Sort(new ListTypeSorter(m_Descendingsort));
			}
			else if (m_Sortname)
			{
				m_SearchList.Sort(new ListNameSorter(m_Descendingsort));
			}
		}

		private class ListTypeSorter : IComparer<XmlAttachment>
		{
			private readonly bool m_Dsort;
			public ListTypeSorter(bool descend)
			{
				m_Dsort = descend;
			}
			public int Compare(XmlAttachment x, XmlAttachment y)
			{
				string xstr;
				string ystr;
				if (x == null)
					return string.Compare(null, null, StringComparison.OrdinalIgnoreCase);

				var str = x.GetType().ToString();
				{
					var arglist = str.Split('.');
					xstr = arglist[^1];
				}

				if (y != null) str = y.GetType().ToString();
				{
					var arglist = str.Split('.');
					ystr = arglist[^1];
				}

				return m_Dsort ? string.Compare(ystr, xstr, StringComparison.OrdinalIgnoreCase) : string.Compare(xstr, ystr, StringComparison.OrdinalIgnoreCase);
			}
		}

		private class ListNameSorter : IComparer<XmlAttachment>
		{
			private readonly bool m_Dsort;

			public ListNameSorter(bool descend)
			{
				m_Dsort = descend;
			}
			public int Compare(XmlAttachment x, XmlAttachment y)
			{
				var xstr = x?.Name;
				var ystr = y?.Name;
				return m_Dsort ? string.Compare(ystr, xstr, StringComparison.OrdinalIgnoreCase) : string.Compare(xstr, ystr, StringComparison.OrdinalIgnoreCase);
			}
		}

		private void Refresh(NetState state)
		{
			state.Mobile.SendGump(new XmlGetAttGump(m_From, m_TargetObject, false, m_Descendingsort,
				m_Dosearchtype, m_Dosearchname, m_Dosearchage,
				m_Searchtype, m_Searchname, m_Searchagedirection, m_Searchage,
				m_SearchList, m_Selected, m_DisplayFrom,
				m_Sorttype, m_Sortname,
				m_SelectAll, m_SelectionList, X, Y));
		}


		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (info == null || state == null || state.Mobile == null) return;

			var radiostate = -1;
			if (info.Switches.Length > 0)
			{
				radiostate = info.Switches[0];
			}

			// read the text entries for the search criteria

			m_Searchage = 0;

			var tr = info.GetTextEntry(400);        // displayfrom info
			try
			{
				m_DisplayFrom = int.Parse(tr.Text);
			}
			catch
			{
				// ignored
			}

			switch (info.ButtonID)
			{

				case 0: // Close
					{
						return;
					}

				case 156: // Delete selected items
					{
						Refresh(state);
						int allcount = 0;
						if (m_SearchList != null)
							allcount = m_SearchList.Count;
						state.Mobile.SendGump(new XmlConfirmDeleteGump(state.Mobile, m_TargetObject, m_SearchList, m_SelectionList, m_DisplayFrom, m_SelectAll, allcount));
						return;
					}

				case 201: // forward block
					{
						// clear the selections
						if (m_SelectionList != null && !m_SelectAll) Array.Clear(m_SelectionList, 0, m_SelectionList.Length);
						if (m_SearchList != null && m_DisplayFrom + MaxEntries < m_SearchList.Count)
						{
							m_DisplayFrom += MaxEntries;
							// clear any selection
							m_Selected = -1;
						}
						break;
					}
				case 202: // backward block
					{
						// clear the selections
						if (m_SelectionList != null && !m_SelectAll) Array.Clear(m_SelectionList, 0, m_SelectionList.Length);
						m_DisplayFrom -= MaxEntries;
						if (m_DisplayFrom < 0) m_DisplayFrom = 0;
						// clear any selection
						m_Selected = -1;
						break;
					}

				case 700: // Sort
					{
						// clear any selection
						m_Selected = -1;
						// clear the selections
						if (m_SelectionList != null && !m_SelectAll) Array.Clear(m_SelectionList, 0, m_SelectionList.Length);
						m_Sorttype = false;
						m_Sortname = false;

						// read the toggle switches that determine the sort
						if (radiostate == 0) // sort by type
						{
							m_Sorttype = true;
						}
						if (radiostate == 1) // sort by name
						{
							m_Sortname = true;
						}
						SortFindList();
						break;
					}
				case 701: // descending sort
					{
						m_Descendingsort = !m_Descendingsort;
						break;
					}
				case 9998:  // refresh the gump
					{
						// clear any selection
						m_Selected = -1;
						break;
					}
				default:
					{
						switch (info.ButtonID)
						{
							case >= 3000 and < 3000 + MaxEntries:
								m_Selected = info.ButtonID - 3000;
								// Show the props window
								Refresh(state);

								DoShowProps(info.ButtonID - 3000 + m_DisplayFrom);
								return;
							case 3998:
							{
								m_SelectAll = !m_SelectAll;

								// dont allow individual selection with the selectall button selected
								if (m_SelectionList != null)
								{
									for (var i = 0; i < MaxEntries; i++)
									{
										if (i < m_SelectionList.Length)
										{
											// only toggle the selection list entries for things that actually have entries
											m_SelectionList[i] = m_SelectAll;
										}
										else
										{
											break;
										}
									}
								}

								break;
							}
							case 3999:
							{
								// dont allow individual selection with the selectall button selected
								if (m_SelectionList != null && m_SearchList != null && !m_SelectAll)
								{
									for (var i = 0; i < MaxEntries; i++)
									{
										if (i < m_SelectionList.Length)
										{
											// only toggle the selection list entries for things that actually have entries
											if ((m_SearchList.Count - m_DisplayFrom > i))
											{
												m_SelectionList[i] = !m_SelectionList[i];
											}
										}
										else
										{
											break;
										}
									}
								}

								break;
							}
							case >= 4000 and < 4000 + MaxEntries:
							{
								var i = info.ButtonID - 4000;
								// dont allow individual selection with the selectall button selected
								if (m_SelectionList != null && i >= 0 && i < m_SelectionList.Length && !m_SelectAll)
								{
									// only toggle the selection list entries for things that actually have entries
									if (m_SearchList != null && m_SearchList.Count - m_DisplayFrom > i)
									{
										m_SelectionList[i] = !m_SelectionList[i];
									}
								}

								break;
							}
							case >= 5000 and < 5000 + MaxEntries:
							{
								var i = info.ButtonID - 5000;
								// dont allow individual selection with the selectall button selected
								if (m_SelectionList != null && i >= 0 && i < m_SelectionList.Length && !m_SelectAll)
								{
									// only toggle the selection list entries for things that actually have entries
									if (m_SearchList != null && m_SearchList.Count - m_DisplayFrom > i)
									{
										XmlAttachment a = m_SearchList[i + m_DisplayFrom];
										if (a != null)
										{
											state.Mobile.SendMessage(a.OnIdentify(state.Mobile));
										}
									}
								}

								break;
							}
						}

						break;
					}
			}
			// Create a new gump
			//m_Spawner.OnDoubleClick( state.Mobile);
			Refresh(state);
		}


		public class XmlConfirmDeleteGump : Gump
		{
			private readonly List<XmlAttachment> m_SearchList;
			private readonly bool[] m_SelectedList;
			private readonly Mobile m_From;
			private readonly int m_DisplayFrom;
			private readonly bool m_SelectAll;
			private readonly object m_Target;

			public XmlConfirmDeleteGump(Mobile from, object target, List<XmlAttachment> searchlist, bool[] selectedlist, int displayfrom, bool selectall, int allcount) : base(0, 0)
			{
				m_SearchList = searchlist;
				m_SelectedList = selectedlist;
				m_DisplayFrom = displayfrom;
				m_SelectAll = selectall;
				m_Target = target;
				m_From = from;
				Closable = false;
				Dragable = true;
				AddPage(0);
				AddBackground(10, 200, 200, 130, 5054);
				var count = 0;
				if (selectall)
				{
					count = allcount;
				}
				else
				{
					count += m_SelectedList.Count(t => t);
				}

				AddLabel(20, 225, 33, $"Delete {count} attachments?");
				AddRadio(35, 255, 9721, 9724, false, 1); // accept/yes radio
				AddRadio(135, 255, 9721, 9724, true, 2); // decline/no radio
				AddHtmlLocalized(72, 255, 200, 30, 1049016, 0x7fff, false, false); // Yes
				AddHtmlLocalized(172, 255, 200, 30, 1049017, 0x7fff, false, false); // No
				AddButton(80, 289, 2130, 2129, 3, GumpButtonType.Reply, 0); // Okay button

			}
			public override void OnResponse(NetState state, RelayInfo info)
			{

				if (info == null || state?.Mobile == null) return;

				var radiostate = -1;
				if (info.Switches.Length > 0)
				{
					radiostate = info.Switches[0];
				}
				switch (info.ButtonID)
				{

					default:
						{
							if (radiostate == 1 && m_SearchList != null && m_SelectedList != null)
							{    // accept
								for (var i = 0; i < m_SearchList.Count; i++)
								{
									var index = i - m_DisplayFrom;
									if ((index < 0 || index >= m_SelectedList.Length || !m_SelectedList[index]) &&
									    !m_SelectAll) continue;
									var o = m_SearchList[i];
									// some objects may not delete gracefully (null map items are particularly error prone) so trap them
									try
									{
										o.Delete();
									}
									catch
									{
										// ignored
									}
								}
								// refresh the gump
								state.Mobile.CloseGump(typeof(XmlGetAttGump));
								state.Mobile.SendGump(new XmlGetAttGump(state.Mobile, m_Target, 0, 0));
							}
							break;
						}
				}
			}
		}

	}
}
