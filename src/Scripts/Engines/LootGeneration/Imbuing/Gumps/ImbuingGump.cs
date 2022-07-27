using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.SkillHandlers;
using Server.Targeting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Gumps
{
    public class ImbuingGump : BaseGump
    {
        private const int LabelColor = 0x7FFF;

        public ImbuingGump(PlayerMobile pm)
            : base(pm, 25)
        {
            User.CloseGump(typeof(ImbueSelectGump));
            User.CloseGump(typeof(ImbueGump));
        }

        public override void AddGumpLayout()
        {
            ImbuingContext context = Imbuing.GetContext(User);

            context.ImbueModVal = 0;
            context.ImbMenuCat = 0;

            AddPage(0);
            AddBackground(0, 0, 520, 310, 5054);
            AddImageTiled(10, 10, 500, 290, 2624);
            AddImageTiled(10, 30, 500, 10, 5058);
            AddImageTiled(10, 270, 500, 10, 5058);
            AddAlphaRegion(10, 10, 520, 310);

            AddHtmlLocalized(10, 12, 520, 20, 1079588, LabelColor, false, false); //<CENTER>IMBUING MENU</CENTER>

            AddButton(15, 60, 4005, 4007, 10005, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 60, 430, 20, 1080432, LabelColor, false, false); //Imbue Item - Adds or modifies an item property on an item

            AddButton(15, 90, 4005, 4007, 10006, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 90, 430, 20, 1113622, LabelColor, false, false); //Reimbue Last - Repeats the last imbuing attempt

            AddButton(15, 120, 4005, 4007, 10007, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 120, 430, 20, 1113571, LabelColor, false, false); //Imbue Last Item - Auto targets the last imbued item

            AddButton(15, 150, 4005, 4007, 10008, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 150, 430, 20, 1114274, LabelColor, false, false); //Imbue Last Property - Imbues a new item with the last property

            AddButton(15, 180, 4005, 4007, 10010, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 180, 470, 20, 1080431, LabelColor, false, false); //Unravel Item - Extracts magical ingredients User an item, destroying it

            AddButton(15, 210, 4005, 4007, 10011, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 210, 430, 20, 1114275, LabelColor, false, false); //Unravel Container - Unravels all items in a container

            AddButton(15, 280, 4017, 4019, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(50, 280, 50, 20, 1011012, LabelColor, false, false); //CANCEL
        }

        public override void OnResponse(RelayInfo info)
        {
            ImbuingContext context = Imbuing.GetContext(User);

            switch (info.ButtonID)
            {
                case 0: // Close
                case 1:
                    {
                        User.EndAction(typeof(Imbuing));

                        break;
                    }
                case 10005:  // Imbue Item
                    {
                        User.SendLocalizedMessage(1079589);  //Target an item you wish to imbue.

                        User.Target = new ImbueItemTarget();
                        User.Target.BeginTimeout(User, TimeSpan.FromSeconds(10.0));

                        break;
                    }
                case 10006:  // Reimbue Last
                    {
                        Item item = context.LastImbued;
                        int mod = context.ImbueMod;
                        int modint = context.ImbueModInt;

                        if (item == null || mod < 0 || modint == 0)
                        {
                            User.SendLocalizedMessage(1113572); // You haven't imbued anything yet!
                            User.EndAction(typeof(Imbuing));
                            break;
                        }

                        if (Imbuing.CanImbueItem(User, item) && Imbuing.OnBeforeImbue(User, item, mod))
                        {
                            Imbuing.TryImbueItem(User, item, mod, modint);
                            ImbueGump.SendGumpDelayed(User);
                        }
                        break;
                    }

                case 10007:  // Imbue Last Item
                    {
                        Item item = context.LastImbued;

                        if (context.LastImbued == null)
                        {
                            User.SendLocalizedMessage(1113572); // You haven't imbued anything yet!
                            User.EndAction(typeof(Imbuing));
                            break;
                        }

                        ImbueStep1(User, item);
                        break;
                    }
                case 10008:  // Imbue Last Property
                    {
                        context.LastImbued = null;
                        int mod = context.ImbueMod;
                        int modint = context.ImbueModInt;

                        if (modint < 0)
                        {
                        }

                        if (mod < 0)
                        {
                            User.SendLocalizedMessage(1113572); // You haven't imbued anything yet!
                            User.EndAction(typeof(Imbuing));
                            break;
                        }

                        ImbueLastProp(User);

                        break;
                    }
                case 10010:  // Unravel Item
                    {
                        User.SendLocalizedMessage(1080422); // Target an item you wish to magically unravel.

                        User.Target = new UnravelTarget();
                        User.Target.BeginTimeout(User, TimeSpan.FromSeconds(10.0));

                        break;
                    }
                case 10011:  // Unravel Container
                    {
                        User.SendLocalizedMessage(1080422); // Target an item you wish to magically unravel.

                        User.Target = new UnravelContainerTarget();
                        User.Target.BeginTimeout(User, TimeSpan.FromSeconds(10.0));

                        break;
                    }
            }
        }

        private class UnravelTarget : Target
        {
            public UnravelTarget()
                : base(-1, false, TargetFlags.None)
            {
                AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile m, object o)
            {
                m.EndAction(typeof(Imbuing));

                if (o is not Item item)
                {
                    m.SendLocalizedMessage(1080425); // You cannot magically unravel this item.
                }
                else if (m is PlayerMobile mobile && Imbuing.CanUnravelItem(mobile, item))
                {
                    mobile.BeginAction(typeof(Imbuing));
                    SendGump(new UnravelGump(mobile, item));
                }
            }

            protected override void OnTargetCancel(Mobile user, TargetCancelType cancelType)
            {
                user.EndAction(typeof(Imbuing));
            }

            private class UnravelGump : BaseGump
            {
                private readonly Item m_Item;

                public UnravelGump(PlayerMobile pm, Item item)
                    : base(pm, 60, 36)
                {
                    m_Item = item;
                }

                public override void AddGumpLayout()
                {
                    AddPage(0);
                    AddBackground(0, 0, 520, 245, 5054);
                    AddImageTiled(10, 10, 500, 225, 2624);
                    AddImageTiled(10, 30, 500, 10, 5058);
                    AddImageTiled(10, 202, 500, 10, 5058);
                    AddAlphaRegion(10, 10, 500, 225);

                    AddHtmlLocalized(10, 12, 520, 20, 1112402, LabelColor, false, false); // <CENTER>UNRAVEL MAGIC ITEM CONFIRMATION</CENTER>

                    AddHtmlLocalized(15, 58, 490, 113, 1112403, true, true); // WARNING! You have targeted an item made out of special material.<BR><BR>This item will be DESTROYED.<BR><BR>Are you sure you wish to unravel this item?

                    AddButton(10, 180, 4005, 4007, 1, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(45, 180, 430, 20, 1114292, LabelColor, false, false); // Unravel Item

                    AddButton(10, 212, 4017, 4019, 0, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(45, 212, 50, 20, 1011012, LabelColor, false, false); // CANCEL
                }

                public override void OnResponse(RelayInfo info)
                {
                    User.EndAction(typeof(Imbuing));

                    if (info.ButtonID == 0 || m_Item.Deleted)
                        return;

                    if (!Imbuing.CanUnravelItem(User, m_Item) || !Imbuing.UnravelItem(User, m_Item))
	                    return;

                    Effects.SendPacket(User, User.Map, new GraphicalEffect(EffectType.FixedFrom, User.Serial, Server.Serial.Zero, 0x375A, User.Location, User.Location, 1, 17, true, false));
                    User.PlaySound(0x1EB);

                    User.SendLocalizedMessage(1080429); // You magically unravel the item!
                    User.SendLocalizedMessage(1072223); // An item has been placed in your backpack.
                }
            }
        }

        private class UnravelContainerTarget : Target
        {
            public UnravelContainerTarget() : base(-1, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile m, object o)
            {
                m.EndAction(typeof(Imbuing));

                if (o is not Container cont)
                    return;

                if (!cont.IsChildOf(m.Backpack))
                {
                    m.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
                    m.EndAction(typeof(Imbuing));
                }
                else if (cont is LockableContainer { Locked: true })
                {
                    m.SendLocalizedMessage(1111814, "0\t0"); // Unraveled: ~1_COUNT~/~2_NUM~ items
                    m.EndAction(typeof(Imbuing));
                }
                else if (m is PlayerMobile mobile)
                {
                    bool unraveled = cont.Items.FirstOrDefault(x => Imbuing.CanUnravelItem(m, x, false)) != null;

                    if (unraveled)
                    {
                        mobile.BeginAction(typeof(Imbuing));
                        SendGump(new UnravelContainerGump(mobile, cont));
                    }
                    else
                    {
                        TryUnravelContainer(mobile, cont);
                        mobile.EndAction(typeof(Imbuing));
                    }
                }
            }

            protected override void OnTargetCancel(Mobile user, TargetCancelType cancelType)
            {
                user.EndAction(typeof(Imbuing));
            }

            private static void TryUnravelContainer(Mobile user, Container c)
            {
                c.Items.ForEach(y =>
                {
                    Imbuing.CanUnravelItem(user, y);
                });

                user.SendLocalizedMessage(1111814, $"{0}\t{c.Items.Count}"); // Unraveled: ~1_COUNT~/~2_NUM~ items
            }

            private class UnravelContainerGump : BaseGump
            {
                private readonly Container m_Container;
                private readonly List<Item> m_List;

                public UnravelContainerGump(PlayerMobile pm, Container c)
                    : base(pm, 25)
                {
                    m_Container = c;
                    m_List = new List<Item>(c.Items);
                }

                public override void AddGumpLayout()
                {
                    AddPage(0);
                    AddBackground(0, 0, 520, 245, 5054);
                    AddImageTiled(10, 10, 500, 225, 2624);
                    AddImageTiled(10, 30, 500, 10, 5058);
                    AddImageTiled(10, 202, 500, 10, 5058);
                    AddAlphaRegion(10, 10, 500, 225);

                    AddHtmlLocalized(10, 12, 520, 20, 1112402, LabelColor, false, false); // <CENTER>UNRAVEL MAGIC ITEM CONFIRMATION</CENTER>

                    AddHtmlLocalized(15, 58, 490, 113, 1112404, true, true); // WARNING! The selected container contains items made with a special material.<BR><BR>These items will be DESTROYED.<BR><BR>Do you wish to unravel these items as well?

                    AddButton(10, 180, 4005, 4007, 1, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(45, 180, 430, 20, 1049717, LabelColor, false, false); // YES

                    AddButton(10, 212, 4017, 4019, 0, GumpButtonType.Reply, 0);
                    AddHtmlLocalized(45, 212, 50, 20, 1049718, LabelColor, false, false); // NO
                }

                public override void OnResponse(RelayInfo info)
                {
                    User.EndAction(typeof(Imbuing));

                    if (m_Container == null || m_List == null)
                        return;

                    if (info.ButtonID == 0)
                    {
                        TryUnravelContainer(User, m_Container);
                        return;
                    }

                    int count = 0;

                    m_List.ForEach(x =>
                    {
                        if (Imbuing.CanUnravelItem(User, x) && Imbuing.UnravelItem(User, x))
                        {
                            count++;
                        }
                    });

                    if (count > 0)
                    {
                        User.SendLocalizedMessage(1080429); // You magically unravel the item!
                        User.SendLocalizedMessage(1072223); // An item has been placed in your backpack.
                    }

                    User.SendLocalizedMessage(1111814, $"{count}\t{m_List.Count}");

                    ColUtility.Free(m_List);
                }
            }
        }

        private class ImbueItemTarget : Target
        {
            public ImbueItemTarget()
                : base(-1, false, TargetFlags.None)
            {
                AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile m, object o)
            {
	            if (o is not Item item)
                {
                    m.SendLocalizedMessage(1079576); // You cannot imbue this item.
                    return;
                }

                Imbuing.GetContext(m);
                ItemType itemType = ItemPropertyInfo.GetItemType(item);

                if (itemType == ItemType.Invalid)
                {
                    m.SendLocalizedMessage(1079576); // You cannot imbue this item.
                    return;
                }

                ImbueStep1(m, item);
            }

            protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
            {
                m.EndAction(typeof(Imbuing));
            }
        }

        private static void ImbueStep1(IEntity m, Item item)
        {
	        if (m is not PlayerMobile mobile || !Imbuing.CanImbueItem(mobile, item))
		        return;

	        ImbuingContext context = Imbuing.GetContext(mobile);
	        context.LastImbued = item;

	        if (context.ImbMenuCat == 0)
		        context.ImbMenuCat = 1;

	        mobile.CloseGump(typeof(ImbuingGump));
	        SendGump(new ImbueSelectGump(mobile, item));
        }

        private static void ImbueLastProp(Mobile m)
        {
            m.Target = new ImbueLastModTarget();
        }

        private class ImbueLastModTarget : Target
        {
            public ImbueLastModTarget()
                : base(-1, false, TargetFlags.None)
            {
                AllowNonlocal = true;
            }

            protected override void OnTarget(Mobile m, object o)
            {
	            if (o is not Item item || m is not PlayerMobile mobile)
                {
                    m.SendLocalizedMessage(1079576); // You cannot imbue this item.
                    return;
                }

                ImbuingContext context = Imbuing.GetContext(mobile);

                int mod = context.ImbueMod;
                int modInt = context.ImbueModInt;

                if (!Imbuing.CanImbueItem(mobile, item) || !Imbuing.OnBeforeImbue(mobile, item, mod) || !Imbuing.CanImbueProperty(mobile, item, mod))
                {
                    ImbueGump.SendGumpDelayed(mobile);
                }
                else
                {
                    Imbuing.TryImbueItem(mobile, item, mod, modInt);
                    ImbueGump.SendGumpDelayed(mobile);
                }
            }

            protected override void OnTargetCancel(Mobile m, TargetCancelType cancelType)
            {
                m.EndAction(typeof(Imbuing));
            }
        }
    }
}
