using Server.Items;
using Server.Targeting;

namespace Server.ContextMenus;

public class AddToSpellbookEntry : ContextMenuEntry
{
	public AddToSpellbookEntry() : base(6144, 3)
	{
	}

	public override void OnClick()
	{
		if (Owner.From.CheckAlive() && Owner.Target is SpellScroll scroll)
			Owner.From.Target = new InternalTarget(scroll);
	}

	private class InternalTarget : Target
	{
		private readonly SpellScroll _mScroll;

		public InternalTarget(SpellScroll scroll) : base(3, false, TargetFlags.None)
		{
			_mScroll = scroll;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (targeted is Spellbook spellbook)
			{
				if (from.CheckAlive() && !_mScroll.Deleted && _mScroll.Movable && _mScroll.Amount >= 1 && _mScroll.CheckItemUse(from))
				{
					SpellbookType type = Spellbook.GetTypeForSpell(_mScroll.SpellID);

					if (type != spellbook.SpellbookType)
					{
					}
					else if (spellbook.HasSpell(_mScroll.SpellID))
					{
						from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
					}
					else
					{
						int val = _mScroll.SpellID - spellbook.BookOffset;

						if (val >= 0 && val < spellbook.BookCount)
						{
							spellbook.Content |= (ulong)1 << val;

							_mScroll.Consume();

							from.Send(new Network.PlaySound(0x249, spellbook.GetWorldLocation()));
						}
					}
				}
			}
		}
	}
}
