using Server.Mobiles;
using Server.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.Gumps
{
	public abstract class BaseGump : Gump, IDisposable
	{
		public static readonly int CenterLoc = 1154645;     // <center>~1_val~</center>
		public static readonly int AlignRightLoc = 1114514; // <DIV ALIGN=RIGHT>~1_TOKEN~</DIV>

		private Gump _Parent;

		public PlayerMobile User { get; private set; }
		public bool Open { get; set; }

		public virtual bool CloseOnMapChange => false;

		public Gump Parent
		{
			get => _Parent;
			set
			{
				_Parent = value;

				if (_Parent != null)
				{
					if (_Parent is BaseGump gump)
					{
						if (gump.Children.Contains(this))
						{
							gump.Children.Remove(this);
						}
						else
						{
							gump.Children.Add(this);
						}
					}
				}
				/*
                if (_Parent != null)
                {
                    if (_Parent is BaseGump && !((BaseGump)_Parent).Children.Contains(this))
                        ((BaseGump)_Parent).Children.Add(this);
                }
                else if (_Parent is BaseGump && ((BaseGump)_Parent).Children.Contains(this))
                {
                    ((BaseGump)_Parent).Children.Remove(this);
                }
                */
			}
		}

		public List<BaseGump> Children { get; set; }

		public BaseGump(PlayerMobile user, int x = 50, int y = 50, BaseGump parent = null)
			: base(x, y)
		{
			if (user == null)
				return;

			Children = new List<BaseGump>();

			User = user;
			Parent = parent;
		}

		~BaseGump()
		{
			Dispose();
		}

		public static BaseGump SendGump(BaseGump gump)
		{
			if (gump == null)
				return null;

			BaseGump g = gump.User.FindGump(gump.GetType()) as BaseGump;

			if (g == gump)
				gump.Refresh();
			else
				gump.SendGump();

			return gump;
		}

		public virtual void SendGump()
		{
			AddGumpLayout();
			Open = true;
			User.SendGump(this, false);
		}

		public void Dispose()
		{
			ColUtility.ForEach(Children.AsEnumerable(), child => Children.Remove(child));
			Children = null;

			Children = null;
			Parent = null;

			foreach (var kvp in _TextTooltips)
			{
				kvp.Value.Free();
			}

			foreach (var kvp in _ClilocTooltips)
			{
				kvp.Value.Free();
			}

			_ClilocTooltips.Clear();
			_TextTooltips.Clear();

			OnDispose();
		}

		public virtual void OnDispose()
		{
		}

		public abstract void AddGumpLayout();

		public virtual void Refresh(bool recompile = true, bool close = true)
		{
			OnBeforeRefresh();

			if (User == null || User.NetState == null)
				return;

			if (close)
			{
				User.NetState.Send(new CloseGump(TypeId, 0));
				User.NetState.RemoveGump(this);
			}
			else
			{
				User.NetState.RemoveGump(this);
			}

			if (recompile)
			{
				Entries.Clear();
				AddGumpLayout();
			}

			/*Children.ForEach(child => 
                {
                    if(child.Open)
                        child.Refresh(recompile, close);
                });*/

			User.SendGump(this);
			OnAfterRefresh();
		}

		public void RefreshParent(bool resend = false)
		{
			if (Parent is BaseGump gump)
				gump.Refresh();

			if (resend)
				Refresh();
		}

		public virtual void OnBeforeRefresh()
		{
		}

		public virtual void OnAfterRefresh()
		{
		}

		public virtual void OnClosed()
		{
			Children.ForEach(child => child.Close());
			Children.Clear();

			Open = false;

			if (Parent != null)
			{
				if (Parent is BaseGump gump)
					gump.OnChildClosed(this);

				Parent = null;
			}
		}

		public virtual void OnChildClosed(BaseGump gump)
		{
		}

		public override sealed void OnResponse(NetState state, RelayInfo info)
		{
			OnResponse(info);

			if (info.ButtonID == 0)
				OnClosed();
		}

		public virtual void OnResponse(RelayInfo info)
		{
		}

		public virtual void OnServerClosed(NetState state)
		{
			OnClosed();
		}

		public virtual void Close()
		{
			if (User == null || User.NetState == null)
				return;

			OnServerClose(User.NetState);

			User.Send(new CloseGump(TypeId, 0));
			User.NetState.RemoveGump(this);
		}

		public static T GetGump<T>(PlayerMobile pm, Func<T, bool> predicate) where T : Gump
		{
			return EnumerateGumps<T>(pm).FirstOrDefault(x => predicate == null || predicate(x));
		}

		public static IEnumerable<T> EnumerateGumps<T>(PlayerMobile pm, Func<T, bool> predicate = null) where T : Gump
		{
			var ns = pm.NetState;

			if (ns == null)
				yield break;

			foreach (BaseGump gump in ns.Gumps.OfType<BaseGump>().Where(g => g.GetType() == typeof(T) &&
				(predicate == null || predicate(g as T))))
			{
				yield return gump as T;
			}
		}

		public static List<T> GetGumps<T>(PlayerMobile pm) where T : Gump
		{
			var ns = pm.NetState;
			List<T> list = new();

			if (ns == null)
				return list;

			foreach (BaseGump gump in ns.Gumps.OfType<BaseGump>().Where(g => g.GetType() == typeof(T)))
			{
				list.Add(gump as T);
			}

			return list;
		}

		public static List<BaseGump> GetGumps(PlayerMobile pm, bool checkOpen = false)
		{
			var ns = pm.NetState;
			List<BaseGump> list = new();

			if (ns == null)
				return list;

			foreach (BaseGump gump in ns.Gumps.OfType<BaseGump>().Where(g => (!checkOpen || g.Open)))
			{
				list.Add(gump);
			}

			return list;
		}

		public static void CheckCloseGumps(PlayerMobile pm, bool checkOpen = false)
		{
			var ns = pm.NetState;

			if (ns != null)
			{
				var gumps = GetGumps(pm, checkOpen);

				foreach (BaseGump gump in gumps.Where(g => g.CloseOnMapChange))
				{
					pm.CloseGump(gump.GetType());
				}

				ColUtility.Free(gumps);
			}
		}

		public new void AddItemProperty(Item item)
		{
			item.SendPropertiesTo(User);

			base.AddItemProperty(item);
		}

		public void AddMobileProperty(Mobile mob)
		{
			mob.SendPropertiesTo(User);

			base.AddItemProperty(mob.Serial.Value);
		}

		public void AddProperties(Spoof spoof)
		{
			User.Send(spoof.PropertyList);

			base.AddItemProperty(spoof.Serial.Value);
		}

		#region Formatting
		public static int C16232(int c16)
		{
			c16 &= 0x7FFF;

			int r = (((c16 >> 10) & 0x1F) << 3);
			int g = (((c16 >> 05) & 0x1F) << 3);
			int b = (((c16 >> 00) & 0x1F) << 3);

			return (r << 16) | (g << 8) | (b << 0);
		}

		public static int C16216(int c16)
		{
			return c16 & 0x7FFF;
		}

		public static int C32216(int c32)
		{
			c32 &= 0xFFFFFF;

			int r = ((c32 >> 16) & 0xFF) >> 3;
			int g = ((c32 >> 08) & 0xFF) >> 3;
			int b = ((c32 >> 00) & 0xFF) >> 3;

			return (r << 10) | (g << 5) | (b << 0);
		}

		protected static string Color(string color, string str)
		{
			return $"<basefont color={color}>{str}";
		}

		protected static string ColorAndCenter(string color, string str)
		{
			return $"<center><basefont color={color}>{str}</center>";
		}

		protected static string ColorAndSize(string color, int size, string str)
		{
			return $"<basefont color={color} size={size}>{str}";
		}

		protected static string ColorAndCenterAndSize(string color, int size, string str)
		{
			return $"<basefont color={color} size={size}><center>{str}</center>";
		}

		protected static string Color(int color, string str)
		{
			return $"<basefont color=#{color:X6}>{str}";
		}

		protected static string ColorAndCenter(int color, string str)
		{
			return $"<basefont color=#{color:X6}><center>{str}</center>";
		}

		protected new static string Center(string str)
		{
			return $"<CENTER>{str}</CENTER>";
		}

		protected static string ColorAndAlignRight(int color, string str)
		{
			return $"<DIV ALIGN=RIGHT><basefont color=#{color:X6}>{str}</DIV>";
		}

		protected static string ColorAndAlignRight(string color, string str)
		{
			return $"<DIV ALIGN=RIGHT><basefont color={color}>{str}</DIV>";
		}

		protected static string AlignRight(string str)
		{
			return $"<DIV ALIGN=RIGHT>{str}</DIV>";
		}

		public void AddHtmlLocalizedCentered(int x, int y, int length, int height, int localization, bool background, bool scrollbar)
		{
			AddHtmlLocalized(x, y, length, height, 1113302, string.Format("#{0}", localization), 0, background, scrollbar);
		}

		public void AddHtmlLocalizedCentered(int x, int y, int length, int height, int localization, int hue, bool background, bool scrollbar)
		{
			AddHtmlLocalized(x, y, length, height, 1113302, string.Format("#{0}", localization), hue, background, scrollbar);
		}

		public void AddHtmlLocalizedAlignRight(int x, int y, int length, int height, int localization, bool background, bool scrollbar)
		{
			AddHtmlLocalized(x, y, length, height, 1114514, string.Format("#{0}", localization), 0, background, scrollbar);
		}

		public void AddHtmlLocalizedAlignRight(int x, int y, int length, int height, int localization, int hue, bool background, bool scrollbar)
		{
			AddHtmlLocalized(x, y, length, height, 1114514, string.Format("#{0}", localization), hue, background, scrollbar);
		}
		#endregion

		#region Tooltips
		private readonly Dictionary<string, Spoof> _TextTooltips = new();
		private readonly Dictionary<Dictionary<int, string>, Spoof> _ClilocTooltips = new();

		public void AddTooltip(string text)
		{
			AddTooltip(text, System.Drawing.Color.Empty);
		}

		public void AddTooltip(string text, System.Drawing.Color color)
		{
			AddTooltip(string.Empty, text, System.Drawing.Color.Empty, color);
		}

		public void AddTooltip(string title, string text)
		{
			AddTooltip(title, text, System.Drawing.Color.Empty, System.Drawing.Color.Empty);
		}

		public void AddTooltip(int cliloc, string format, params string[] args)
		{
			AddTooltip(cliloc, string.Format(format, args));
		}

		public void AddTooltip(int[] clilocs)
		{
			AddTooltip(clilocs, new string[clilocs.Length]);
		}

		public void AddTooltip(string[] args)
		{
			var clilocs = new int[Math.Min(Spoof.EmptyClilocs.Length, args.Length)];

			for (int i = 0; i < args.Length; i++)
			{
				if (i >= Spoof.EmptyClilocs.Length)
					break;

				clilocs[i] = Spoof.EmptyClilocs[i];
			}

			AddTooltip(clilocs, args);
		}
		/*
        public void AddTooltip(int cliloc, string args)
        {
            AddTooltip(new int[] { cliloc }, new string[] { args ?? string.Empty });
        }
        */
		public void AddTooltip(int[] clilocs, string[] args)
		{
			var dictionary = new Dictionary<int, string>();
			int emptyIndex = 0;

			for (int i = 0; i < clilocs.Length; i++)
			{
				var str = string.Empty;

				if (i < args.Length)
				{
					str = args[i] ?? string.Empty;
				}

				var cliloc = clilocs[i];

				if (cliloc <= 0)
				{
					if (emptyIndex <= Spoof.EmptyClilocs.Length)
					{
						cliloc = Spoof.EmptyClilocs[emptyIndex];
						emptyIndex++;
					}
				}

				if (cliloc > 0)
				{
					dictionary[cliloc] = str;
				}
			}


			if (!_ClilocTooltips.TryGetValue(dictionary, out Spoof spoof) || spoof == null || spoof.Deleted)
			{
				spoof = Spoof.Acquire();
			}

			spoof.ClilocTable = dictionary;

			_ClilocTooltips[dictionary] = spoof;
			AddProperties(spoof);
		}

		public void AddTooltip(string title, string text, System.Drawing.Color titleColor, System.Drawing.Color textColor)
		{
			title ??= string.Empty;
			text ??= string.Empty;

			if (titleColor.IsEmpty || titleColor == System.Drawing.Color.Transparent)
			{
				titleColor = System.Drawing.Color.White;
			}

			if (textColor.IsEmpty || textColor == System.Drawing.Color.Transparent)
			{
				textColor = System.Drawing.Color.White;
			}


			if (!_TextTooltips.TryGetValue(text, out Spoof spoof) || spoof == null || spoof.Deleted)
			{
				spoof = Spoof.Acquire();
			}

			if (!string.IsNullOrWhiteSpace(title))
			{
				spoof.Text = string.Concat(string.Format("<basefont color=#{0:X}>{1}", titleColor.ToArgb(), title),
							'\n',
							string.Format("<basefont color=#{0:X}>{1}", textColor.ToArgb(), text));
			}
			else
			{
				spoof.Text = string.Format("<basefont color=#{0:X}>{1}", textColor.ToArgb(), text); //  text.WrapUOHtmlColor(textColor, false);
			}

			_TextTooltips[text] = spoof;
			AddProperties(spoof);
		}

		public sealed class Spoof : Entity
		{
			private static readonly char[] _Split = { '\n' };
			private static int _UID = -1;

			private static int NewUID
			{
				get
				{
					if (_UID == int.MinValue)
					{
						_UID = -1;
					}

					return --_UID;
				}
			}

			public static readonly int[] EmptyClilocs =
			{
				1042971, 1070722, // ~1_NOTHING~
			    1114057, 1114778, 1114779, // ~1_val~
			    1150541, // ~1_TOKEN~
			    1153153, // ~1_year~
            };

			private static readonly List<Spoof> _SpoofPool = new();

			public static Spoof Acquire()
			{
				if (_SpoofPool.Count == 0)
				{
					return new Spoof();
				}
				else
				{
					var spoof = _SpoofPool[0];
					_SpoofPool.Remove(spoof);

					return spoof;
				}
			}

			public void Free()
			{
				Packet.Release(ref _PropertyList);

				_Text = string.Empty;
				_ClilocTable = null;

				_SpoofPool.Add(this);
			}

			// public int UID { get => Serial.Value; private set { } }

			private ObjectPropertyList _PropertyList;

			public ObjectPropertyList PropertyList
			{
				get
				{
					if (_PropertyList == null)
					{
						_PropertyList = new ObjectPropertyList(this);

						if (!string.IsNullOrEmpty(Text))
						{
							var text = StripHtmlBreaks(Text, true);

							if (text.Contains('\n'))
							{
								var lines = text.Split(_Split);

								foreach (var str in lines)
								{
									_PropertyList.Add(str);
								}
							}
							else
							{
								_PropertyList.Add(text);
							}
						}
						else if (_ClilocTable != null)
						{
							foreach (var kvp in _ClilocTable)
							{
								var cliloc = kvp.Key;
								var args = kvp.Value;

								if (cliloc <= 0 && !string.IsNullOrEmpty(args))
								{
									_PropertyList.Add(args);
								}
								else if (string.IsNullOrEmpty(args))
								{
									_PropertyList.Add(cliloc);
								}
								else
								{
									_PropertyList.Add(cliloc, args);
								}
							}
						}

						_PropertyList.Terminate();
						_PropertyList.SetStatic();
					}

					return _PropertyList;
				}
			}

			private string _Text = string.Empty;
			public string Text
			{
				get => _Text ?? string.Empty;
				set
				{
					if (_Text != value)
					{
						_Text = value;

						Packet.Release(ref _PropertyList);
					}
				}
			}

			private Dictionary<int, string> _ClilocTable;
			public Dictionary<int, string> ClilocTable
			{
				get => _ClilocTable;
				set
				{
					if (_ClilocTable != value)
					{
						_ClilocTable = value;

						Packet.Release(ref _PropertyList);
					}
				}
			}

			public Spoof()
				: base(NewUID, Point3D.Zero, Map.Internal)
			{ }
		}

		public static string StripHtmlBreaks(string str, bool preserve)
		{
			return Regex.Replace(str, @"<br[^>]?>", preserve ? "\n" : " ", RegexOptions.IgnoreCase);
		}
		#endregion
	}
}
