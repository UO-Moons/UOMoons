using Server.Items;
using Server.Targeting;

namespace Server.ContextMenus
{
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
			private readonly SpellScroll m_Scroll;

			public InternalTarget(SpellScroll scroll) : base(3, false, TargetFlags.None)
			{
				m_Scroll = scroll;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (targeted is Spellbook spellbook)
				{
					if (from.CheckAlive() && !m_Scroll.Deleted && m_Scroll.Movable && m_Scroll.Amount >= 1 && m_Scroll.CheckItemUse(from))
					{
						SpellbookType type = Spellbook.GetTypeForSpell(m_Scroll.SpellID);

						if (type != spellbook.SpellbookType)
						{
						}
						else if (spellbook.HasSpell(m_Scroll.SpellID))
						{
							from.SendLocalizedMessage(500179); // That spell is already present in that spellbook.
						}
						else
						{
							int val = m_Scroll.SpellID - spellbook.BookOffset;

							if (val >= 0 && val < spellbook.BookCount)
							{
								spellbook.Content |= (ulong)1 << val;

								m_Scroll.Consume();

								from.Send(new Network.PlaySound(0x249, spellbook.GetWorldLocation()));
							}
						}
					}
				}
			}
		}
	}
}
