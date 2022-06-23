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
		private readonly object _mTargetObject;
		private readonly bool _mDosearchtype;
		private readonly bool _mDosearchname;
		private readonly bool _mDosearchage;
		private readonly bool _mSearchagedirection;
		private double _mSearchage;
		private readonly string _mSearchtype;
		private readonly string _mSearchname;
		private bool _mSorttype;
		private bool _mSortname;
		private readonly Mobile _mFrom;
		private bool _mDescendingsort;
		private int _mSelected;
		private int _mDisplayFrom;
		private readonly bool[] _mSelectionList;
		private bool _mSelectAll;
		private readonly List<XmlAttachment> _mSearchList;

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
		/*
		Removed string statusStr from code. Backup just incase i broke something
		private List<XmlAttachment> Search(object target, out string statusStr)
		{
			statusStr = null;
			List<XmlAttachment> newarray = new();
			Type targetType = null;
			// if the type is specified then get the search type
			if (_mDosearchtype && _mSearchtype != null)
			{
				targetType = SpawnerType.GetType(_mSearchtype);
				if (targetType == null)
				{
					statusStr = "Invalid type: " + _mSearchtype;
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
				if (targetType != null && _mDosearchtype && (i.GetType().IsSubclassOf(targetType) || i.GetType() == targetType))
				{
					hastype = true;
				}
				if (_mDosearchtype && !hastype)
					continue;

				// check for name
				if (_mDosearchname && i.Name != null && _mSearchname != null && (i.Name.ToLower().Contains(_mSearchname.ToLower(), StringComparison.CurrentCulture)))
				{
					hasname = true;
				}
				if (_mDosearchname && !hasname)
					continue;


				// satisfied all conditions so add it
				newarray.Add(i);
			}

			return newarray;
		}
		 */
		private List<XmlAttachment> Search(object target)
		{
			List<XmlAttachment> newarray = new();
			Type targetType = null;
			// if the type is specified then get the search type
			if (_mDosearchtype && _mSearchtype != null)
			{
				targetType = SpawnerType.GetType(_mSearchtype);
				if (targetType == null)
				{
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
				if (targetType != null && _mDosearchtype && (i.GetType().IsSubclassOf(targetType) || i.GetType() == targetType))
				{
					hastype = true;
				}
				if (_mDosearchtype && !hastype)
					continue;

				// check for name
				if (_mDosearchname && i.Name != null && _mSearchname != null && i.Name.ToLower().Contains(_mSearchname.ToLower(), StringComparison.CurrentCulture))
				{
					hasname = true;
				}
				if (_mDosearchname && !hasname)
					continue;


				// satisfied all conditions so add it
				newarray.Add(i);
			}

			return newarray;
		}

		private class GetAttachTarget : Target
		{
			private readonly CommandEventArgs _mE;

			public GetAttachTarget(CommandEventArgs e) : base(30, false, TargetFlags.None)
			{
				_mE = e;

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

			_mTargetObject = targeted;
			_mFrom = from;
			_mSelectionList = selectionlist ?? new bool[MaxEntries];
			_mSelectAll = selectall;
			_mSorttype = sorttype;
			_mSortname = sortname;

			_mDisplayFrom = displayfrom;
			_mSelected = selected;

			_mDescendingsort = descend;
			_mDosearchtype = dosearchtype;
			_mDosearchname = dosearchname;
			_mDosearchage = dosearchage;

			_mSearchagedirection = searchagedirection;

			_mSearchage = searchage;
			_mSearchtype = searchtype;
			_mSearchname = searchname;

			_mSearchList = searchlist;

			if (firststart)
			{
				_mSearchList = Search(_mTargetObject);
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
			if (_mDescendingsort)
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
			AddRadio(155, 450, 0xD2, 0xD3, _mSorttype, 0);
			AddLabel(155, 425, 0x384, "type");

			// add the Sort on name toggle
			AddRadio(200, 450, 0xD2, 0xD3, _mSortname, 1);
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
			AddTextEntry(501, 450, 60, 21, 0, 400, _mDisplayFrom.ToString());
			AddButton(560, 450, 0xFAB, 0xFAD, 9998, GumpButtonType.Reply, 0);

			// display the item list
			if (_mSearchList != null)
			{
				AddLabel(320, 425, 68, $"Found {_mSearchList.Count} attachments");
				AddLabel(500, 425, 68,
					$"Displaying {_mDisplayFrom}-{(_mDisplayFrom + MaxEntries < _mSearchList.Count ? _mDisplayFrom + MaxEntries : _mSearchList.Count)}");
			}

			// display the select-all-displayed toggle
			AddButton(620, 5, 0xD2, 0xD3, 3999, GumpButtonType.Reply, 0);

			// display the select-all toggle
			AddButton(600, 5, (_mSelectAll ? 0xD3 : 0xD2), (_mSelectAll ? 0xD2 : 0xD3), 3998, GumpButtonType.Reply, 0);

			for (var i = 0; i < MaxEntries; i++)
			{
				var index = i + _mDisplayFrom;
				if (_mSearchList == null || index >= _mSearchList.Count) break;
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

				object o = _mSearchList[index];

				if (o is XmlAttachment)
				{
					var a = _mSearchList[index];

					namestr = a.Name;
					typestr = a.GetType().Name;
					expirestr = a.Expiration.ToString();
					//description = a.OnIdentify(m_From);
					created = a.CreationTime.ToString(CultureInfo.InvariantCulture);
					attachedby = a.AttachedBy;
				}

				bool sel = false;
				if (_mSelectionList != null && i < _mSelectionList.Length)
				{
					sel = _mSelectionList[i];
				}
				if (sel) texthue = 33;

				if (i == _mSelected) texthue = 68;

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
			if (_mFrom == null || _mFrom.Deleted) return;

			if (index >= _mSearchList.Count)
				return;

			var x = _mSearchList[index];
			if (x == null || x.Deleted) return;

			_mFrom.SendGump(new PropertiesGump(_mFrom, x));
		}

		private void SortFindList()
		{
			if (_mSearchList is not {Count: > 0})
				return;

			if (_mSorttype)
			{
				_mSearchList.Sort(new ListTypeSorter(_mDescendingsort));
			}
			else if (_mSortname)
			{
				_mSearchList.Sort(new ListNameSorter(_mDescendingsort));
			}
		}

		private class ListTypeSorter : IComparer<XmlAttachment>
		{
			private readonly bool _mDsort;
			public ListTypeSorter(bool descend)
			{
				_mDsort = descend;
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

				return _mDsort ? string.Compare(ystr, xstr, StringComparison.OrdinalIgnoreCase) : string.Compare(xstr, ystr, StringComparison.OrdinalIgnoreCase);
			}
		}

		private class ListNameSorter : IComparer<XmlAttachment>
		{
			private readonly bool _mDsort;

			public ListNameSorter(bool descend)
			{
				_mDsort = descend;
			}
			public int Compare(XmlAttachment x, XmlAttachment y)
			{
				var xstr = x?.Name;
				var ystr = y?.Name;
				return _mDsort ? string.Compare(ystr, xstr, StringComparison.OrdinalIgnoreCase) : string.Compare(xstr, ystr, StringComparison.OrdinalIgnoreCase);
			}
		}

		private void Refresh(NetState state)
		{
			state.Mobile.SendGump(new XmlGetAttGump(_mFrom, _mTargetObject, false, _mDescendingsort,
				_mDosearchtype, _mDosearchname, _mDosearchage,
				_mSearchtype, _mSearchname, _mSearchagedirection, _mSearchage,
				_mSearchList, _mSelected, _mDisplayFrom,
				_mSorttype, _mSortname,
				_mSelectAll, _mSelectionList, X, Y));
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

			_mSearchage = 0;

			var tr = info.GetTextEntry(400);        // displayfrom info
			try
			{
				_mDisplayFrom = int.Parse(tr.Text);
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
						if (_mSearchList != null)
							allcount = _mSearchList.Count;
						state.Mobile.SendGump(new XmlConfirmDeleteGump(state.Mobile, _mTargetObject, _mSearchList, _mSelectionList, _mDisplayFrom, _mSelectAll, allcount));
						return;
					}

				case 201: // forward block
					{
						// clear the selections
						if (_mSelectionList != null && !_mSelectAll) Array.Clear(_mSelectionList, 0, _mSelectionList.Length);
						if (_mSearchList != null && _mDisplayFrom + MaxEntries < _mSearchList.Count)
						{
							_mDisplayFrom += MaxEntries;
							// clear any selection
							_mSelected = -1;
						}
						break;
					}
				case 202: // backward block
					{
						// clear the selections
						if (_mSelectionList != null && !_mSelectAll) Array.Clear(_mSelectionList, 0, _mSelectionList.Length);
						_mDisplayFrom -= MaxEntries;
						if (_mDisplayFrom < 0) _mDisplayFrom = 0;
						// clear any selection
						_mSelected = -1;
						break;
					}

				case 700: // Sort
					{
						// clear any selection
						_mSelected = -1;
						// clear the selections
						if (_mSelectionList != null && !_mSelectAll) Array.Clear(_mSelectionList, 0, _mSelectionList.Length);
						_mSorttype = false;
						_mSortname = false;

						// read the toggle switches that determine the sort
						if (radiostate == 0) // sort by type
						{
							_mSorttype = true;
						}
						if (radiostate == 1) // sort by name
						{
							_mSortname = true;
						}
						SortFindList();
						break;
					}
				case 701: // descending sort
					{
						_mDescendingsort = !_mDescendingsort;
						break;
					}
				case 9998:  // refresh the gump
					{
						// clear any selection
						_mSelected = -1;
						break;
					}
				default:
					{
						switch (info.ButtonID)
						{
							case >= 3000 and < 3000 + MaxEntries:
								_mSelected = info.ButtonID - 3000;
								// Show the props window
								Refresh(state);

								DoShowProps(info.ButtonID - 3000 + _mDisplayFrom);
								return;
							case 3998:
							{
								_mSelectAll = !_mSelectAll;

								// dont allow individual selection with the selectall button selected
								if (_mSelectionList != null)
								{
									for (var i = 0; i < MaxEntries; i++)
									{
										if (i < _mSelectionList.Length)
										{
											// only toggle the selection list entries for things that actually have entries
											_mSelectionList[i] = _mSelectAll;
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
								if (_mSelectionList != null && _mSearchList != null && !_mSelectAll)
								{
									for (var i = 0; i < MaxEntries; i++)
									{
										if (i < _mSelectionList.Length)
										{
											// only toggle the selection list entries for things that actually have entries
											if ((_mSearchList.Count - _mDisplayFrom > i))
											{
												_mSelectionList[i] = !_mSelectionList[i];
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
								if (_mSelectionList != null && i >= 0 && i < _mSelectionList.Length && !_mSelectAll)
								{
									// only toggle the selection list entries for things that actually have entries
									if (_mSearchList != null && _mSearchList.Count - _mDisplayFrom > i)
									{
										_mSelectionList[i] = !_mSelectionList[i];
									}
								}

								break;
							}
							case >= 5000 and < 5000 + MaxEntries:
							{
								var i = info.ButtonID - 5000;
								// dont allow individual selection with the selectall button selected
								if (_mSelectionList != null && i >= 0 && i < _mSelectionList.Length && !_mSelectAll)
								{
									// only toggle the selection list entries for things that actually have entries
									if (_mSearchList != null && _mSearchList.Count - _mDisplayFrom > i)
									{
										XmlAttachment a = _mSearchList[i + _mDisplayFrom];
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
			private readonly List<XmlAttachment> _mSearchList;
			private readonly bool[] _mSelectedList;
			private readonly Mobile _mFrom;
			private readonly int _mDisplayFrom;
			private readonly bool _mSelectAll;
			private readonly object _mTarget;

			public XmlConfirmDeleteGump(Mobile from, object target, List<XmlAttachment> searchlist, bool[] selectedlist, int displayfrom, bool selectall, int allcount) : base(0, 0)
			{
				_mSearchList = searchlist;
				_mSelectedList = selectedlist;
				_mDisplayFrom = displayfrom;
				_mSelectAll = selectall;
				_mTarget = target;
				_mFrom = from;
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
					count += _mSelectedList.Count(t => t);
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
							if (radiostate == 1 && _mSearchList != null && _mSelectedList != null)
							{    // accept
								for (var i = 0; i < _mSearchList.Count; i++)
								{
									var index = i - _mDisplayFrom;
									if ((index < 0 || index >= _mSelectedList.Length || !_mSelectedList[index]) &&
									    !_mSelectAll) continue;
									var o = _mSearchList[i];
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
								state.Mobile.SendGump(new XmlGetAttGump(state.Mobile, _mTarget, 0, 0));
							}
							break;
						}
				}
			}
		}

	}
}
