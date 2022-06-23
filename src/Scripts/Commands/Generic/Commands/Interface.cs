using Server.Gumps;
using Server.Network;
using Server.Targets;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Commands.Generic;

public class InterfaceCommand : BaseCommand
{
	public InterfaceCommand()
	{
		AccessLevel = AccessLevel.GameMaster;
		Supports = CommandSupport.Complex | CommandSupport.Simple;
		Commands = new[] { "Interface" };
		ObjectTypes = ObjectTypes.Both;
		Usage = "Interface [view <properties ...>]";
		Description = "Opens an interface to interact with matched objects. Generally used with condition arguments.";
		ListOptimized = true;
	}

	public override void ExecuteList(CommandEventArgs e, ArrayList list)
	{
		if (list.Count > 0)
		{
			List<string> columns = new()
			{
				"Object"
			};

			if (e.Length > 0)
			{
				int offset = 0;

				if (Insensitive.Equals(e.GetString(0), "view"))
					++offset;

				while (offset < e.Length)
					columns.Add(e.GetString(offset++));
			}

			e.Mobile.SendGump(new InterfaceGump(e.Mobile, columns.ToArray(), list, 0, null));
		}
		else
		{
			AddResponse("No matching objects found.");
		}
	}
}

public class InterfaceGump : BaseGridGump
{
	private readonly Mobile _mFrom;

	private readonly string[] _mColumns;

	private readonly ArrayList _mList;
	private readonly int _mPage;

	private readonly object _mSelect;

	private const int EntriesPerPage = 15;

	public InterfaceGump(Mobile from, string[] columns, ArrayList list, int page, object select) : base(30, 30)
	{
		_mFrom = from;

		_mColumns = columns;

		_mList = list;
		_mPage = page;

		_mSelect = select;

		Render();
	}

	public void Render()
	{
		AddNewPage();

		if (_mPage > 0)
			AddEntryButton(20, ArrowLeftID1, ArrowLeftID2, 1, ArrowLeftWidth, ArrowLeftHeight);
		else
			AddEntryHeader(20);

		AddEntryHtml(40 + (_mColumns.Length * 130) - 20 + ((_mColumns.Length - 2) * OffsetSize), Center(string.Format("Page {0} of {1}", _mPage + 1, (_mList.Count + EntriesPerPage - 1) / EntriesPerPage)));

		if ((_mPage + 1) * EntriesPerPage < _mList.Count)
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, 2, ArrowRightWidth, ArrowRightHeight);
		else
			AddEntryHeader(20);

		if (_mColumns.Length > 1)
		{
			AddNewLine();

			for (int i = 0; i < _mColumns.Length; ++i)
			{
				if (i > 0 && _mList.Count > 0)
				{
					object obj = _mList[0];

					if (obj != null)
					{
						string failReason = null;
						PropertyInfo[] chain = Properties.GetPropertyInfoChain(_mFrom, obj.GetType(), _mColumns[i], PropertyAccess.Read, ref failReason);

						if (chain != null && chain.Length > 0)
						{
							_mColumns[i] = "";

							for (int j = 0; j < chain.Length; ++j)
							{
								if (j > 0)
									_mColumns[i] += '.';

								_mColumns[i] += chain[j].Name;
							}
						}
					}
				}

				AddEntryHtml(130 + (i == 0 ? 40 : 0), _mColumns[i]);
			}

			AddEntryHeader(20);
		}

		for (int i = _mPage * EntriesPerPage, line = 0; line < EntriesPerPage && i < _mList.Count; ++i, ++line)
		{
			AddNewLine();

			object obj = _mList[i];
			bool isDeleted = false;

			if (obj is Item)
			{
				Item item = (Item)obj;

				if (!(isDeleted = item.Deleted))
					AddEntryHtml(40 + 130, item.GetType().Name);
			}
			else if (obj is Mobile)
			{
				Mobile mob = (Mobile)obj;

				if (!(isDeleted = mob.Deleted))
					AddEntryHtml(40 + 130, mob.Name);
			}

			if (isDeleted)
			{
				AddEntryHtml(40 + 130, "(deleted)");

				for (int j = 1; j < _mColumns.Length; ++j)
					AddEntryHtml(130, "---");

				AddEntryHeader(20);
			}
			else
			{
				for (int j = 1; j < _mColumns.Length; ++j)
				{
					object src = obj;

					string value;
					string failReason = "";

					PropertyInfo[] chain = Properties.GetPropertyInfoChain(_mFrom, src.GetType(), _mColumns[j], PropertyAccess.Read, ref failReason);

					if (chain == null || chain.Length == 0)
					{
						value = "---";
					}
					else
					{
						PropertyInfo p = Properties.GetPropertyInfo(ref src, chain, ref failReason);

						value = p == null ? "---" : PropertiesGump.ValueToString(src, p);
					}

					AddEntryHtml(130, value);
				}

				bool isSelected = (_mSelect != null && obj == _mSelect);

				AddEntryButton(20, (isSelected ? 9762 : ArrowRightID1), (isSelected ? 9763 : ArrowRightID2), 3 + i, ArrowRightWidth, ArrowRightHeight);
			}
		}

		FinishPage();
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		switch (info.ButtonID)
		{
			case 1:
			{
				if (_mPage > 0)
					_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage - 1, _mSelect));

				break;
			}
			case 2:
			{
				if ((_mPage + 1) * EntriesPerPage < _mList.Count)
					_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage + 1, _mSelect));

				break;
			}
			default:
			{
				int v = info.ButtonID - 3;

				if (v >= 0 && v < _mList.Count)
				{
					object obj = _mList[v];

					if (!BaseCommand.IsAccessible(_mFrom, obj))
					{
						_mFrom.SendMessage("That is not accessible.");
						_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mSelect));
						break;
					}

					switch (obj)
					{
						case Item {Deleted: false} item:
							_mFrom.SendGump(new InterfaceItemGump(_mFrom, _mColumns, _mList, _mPage, item));
							break;
						case Mobile {Deleted: false} mobile:
							_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, mobile));
							break;
						default:
							_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mSelect));
							break;
					}
				}

				break;
			}
		}
	}
}

public class InterfaceItemGump : BaseGridGump
{
	private readonly Mobile _mFrom;

	private readonly string[] _mColumns;

	private readonly ArrayList _mList;
	private readonly int _mPage;

	private readonly Item _mItem;

	public InterfaceItemGump(Mobile from, string[] columns, ArrayList list, int page, Item item) : base(30, 30)
	{
		_mFrom = from;

		_mColumns = columns;

		_mList = list;
		_mPage = page;

		_mItem = item;

		Render();
	}

	public void Render()
	{
		AddNewPage();

		AddEntryButton(20, ArrowLeftID1, ArrowLeftID2, 1, ArrowLeftWidth, ArrowLeftHeight);
		AddEntryHtml(160, _mItem.GetType().Name);
		AddEntryHeader(20);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Properties");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 2, ArrowRightWidth, ArrowRightHeight);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Delete");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 3, ArrowRightWidth, ArrowRightHeight);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Go there");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 4, ArrowRightWidth, ArrowRightHeight);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Move to target");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 5, ArrowRightWidth, ArrowRightHeight);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Bring to pack");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 6, ArrowRightWidth, ArrowRightHeight);

		FinishPage();
	}

	private void InvokeCommand(string ip)
	{
		CommandSystem.Handle(_mFrom, $"{CommandSystem.Prefix}{ip}");
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_mItem.Deleted)
		{
			_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
			return;
		}
		else if (!BaseCommand.IsAccessible(_mFrom, _mItem))
		{
			_mFrom.SendMessage("That is no longer accessible.");
			_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
			return;
		}

		switch (info.ButtonID)
		{
			case 0:
			case 1:
			{
				_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
				break;
			}
			case 2: // Properties
			{
				_mFrom.SendGump(new InterfaceItemGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
				_mFrom.SendGump(new PropertiesGump(_mFrom, _mItem));
				break;
			}
			case 3: // Delete
			{
				CommandLogging.WriteLine(_mFrom, "{0} {1} deleting {2}", _mFrom.AccessLevel, CommandLogging.Format(_mFrom), CommandLogging.Format(_mItem));
				_mItem.Delete();
				_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
				break;
			}
			case 4: // Go there
			{
				_mFrom.SendGump(new InterfaceItemGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
				InvokeCommand($"Go {_mItem.Serial.Value}");
				break;
			}
			case 5: // Move to target
			{
				_mFrom.SendGump(new InterfaceItemGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
				_mFrom.Target = new MoveTarget(_mItem);
				break;
			}
			case 6: // Bring to pack
			{
				Mobile owner = _mItem.RootParent as Mobile;

				if (owner != null && (owner.Map != null && owner.Map != Map.Internal) && !BaseCommand.IsAccessible(_mFrom, owner) /* !m_From.CanSee( owner )*/ )
				{
					_mFrom.SendMessage("You can not get what you can not see.");
				}
				else if (owner != null && (owner.Map == null || owner.Map == Map.Internal) && owner.Hidden && owner.AccessLevel >= _mFrom.AccessLevel)
				{
					_mFrom.SendMessage("You can not get what you can not see.");
				}
				else
				{
					_mFrom.SendGump(new InterfaceItemGump(_mFrom, _mColumns, _mList, _mPage, _mItem));
					_mFrom.AddToBackpack(_mItem);
				}

				break;
			}
		}
	}
}

public class InterfaceMobileGump : BaseGridGump
{
	private readonly Mobile _mFrom;

	private readonly string[] _mColumns;

	private readonly ArrayList _mList;
	private readonly int _mPage;

	private readonly Mobile _mMobile;

	public InterfaceMobileGump(Mobile from, string[] columns, ArrayList list, int page, Mobile mob)
		: base(30, 30)
	{
		_mFrom = from;

		_mColumns = columns;

		_mList = list;
		_mPage = page;

		_mMobile = mob;

		Render();
	}

	public void Render()
	{
		AddNewPage();

		AddEntryButton(20, ArrowLeftID1, ArrowLeftID2, 1, ArrowLeftWidth, ArrowLeftHeight);
		AddEntryHtml(160, _mMobile.Name);
		AddEntryHeader(20);

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Properties");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 2, ArrowRightWidth, ArrowRightHeight);

		if (!_mMobile.Player)
		{
			AddNewLine();
			AddEntryHtml(20 + OffsetSize + 160, "Delete");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, 3, ArrowRightWidth, ArrowRightHeight);
		}

		if (_mMobile != _mFrom)
		{
			AddNewLine();
			AddEntryHtml(20 + OffsetSize + 160, "Go to there");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, 4, ArrowRightWidth, ArrowRightHeight);

			AddNewLine();
			AddEntryHtml(20 + OffsetSize + 160, "Bring them here");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, 5, ArrowRightWidth, ArrowRightHeight);
		}

		AddNewLine();
		AddEntryHtml(20 + OffsetSize + 160, "Move to target");
		AddEntryButton(20, ArrowRightID1, ArrowRightID2, 6, ArrowRightWidth, ArrowRightHeight);

		if (_mFrom == _mMobile || _mFrom.AccessLevel > _mMobile.AccessLevel)
		{
			AddNewLine();
			if (_mMobile.Alive)
			{
				AddEntryHtml(20 + OffsetSize + 160, "Kill");
				AddEntryButton(20, ArrowRightID1, ArrowRightID2, 7, ArrowRightWidth, ArrowRightHeight);
			}
			else
			{
				AddEntryHtml(20 + OffsetSize + 160, "Resurrect");
				AddEntryButton(20, ArrowRightID1, ArrowRightID2, 8, ArrowRightWidth, ArrowRightHeight);
			}
		}

		if (_mMobile.NetState != null)
		{
			AddNewLine();
			AddEntryHtml(20 + OffsetSize + 160, "Client");
			AddEntryButton(20, ArrowRightID1, ArrowRightID2, 9, ArrowRightWidth, ArrowRightHeight);
		}

		FinishPage();
	}

	private void InvokeCommand(string ip)
	{
		CommandSystem.Handle(_mFrom, $"{CommandSystem.Prefix}{ip}");
	}

	public override void OnResponse(NetState sender, RelayInfo info)
	{
		if (_mMobile.Deleted)
		{
			_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
			return;
		}
		else if (!BaseCommand.IsAccessible(_mFrom, _mMobile))
		{
			_mFrom.SendMessage("That is no longer accessible.");
			_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
			return;
		}

		switch (info.ButtonID)
		{
			case 0:
			case 1:
			{
				_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
				break;
			}
			case 2: // Properties
			{
				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
				_mFrom.SendGump(new PropertiesGump(_mFrom, _mMobile));
				break;
			}
			case 3: // Delete
			{
				if (!_mMobile.Player)
				{
					CommandLogging.WriteLine(_mFrom, "{0} {1} deleting {2}", _mFrom.AccessLevel, CommandLogging.Format(_mFrom), CommandLogging.Format(_mMobile));
					_mMobile.Delete();
					_mFrom.SendGump(new InterfaceGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
				}

				break;
			}
			case 4: // Go there
			{
				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
				InvokeCommand($"Go {_mMobile.Serial.Value}");
				break;
			}
			case 5: // Bring them here
			{
				if (_mFrom.Map == null || _mFrom.Map == Map.Internal)
				{
					_mFrom.SendMessage("You cannot bring that person here.");
				}
				else
				{
					_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
					_mMobile.MoveToWorld(_mFrom.Location, _mFrom.Map);
				}

				break;
			}
			case 6: // Move to target
			{
				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));
				_mFrom.Target = new MoveTarget(_mMobile);
				break;
			}
			case 7: // Kill
			{
				if (_mFrom == _mMobile || _mFrom.AccessLevel > _mMobile.AccessLevel)
					_mMobile.Kill();

				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));

				break;
			}
			case 8: // Res
			{
				if (_mFrom == _mMobile || _mFrom.AccessLevel > _mMobile.AccessLevel)
				{
					_mMobile.PlaySound(0x214);
					_mMobile.FixedEffect(0x376A, 10, 16);

					_mMobile.Resurrect();
				}

				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));

				break;
			}
			case 9: // Client
			{
				_mFrom.SendGump(new InterfaceMobileGump(_mFrom, _mColumns, _mList, _mPage, _mMobile));

				if (_mMobile.NetState != null)
					_mFrom.SendGump(new ClientGump(_mFrom, _mMobile.NetState));

				break;
			}
		}
	}
}
