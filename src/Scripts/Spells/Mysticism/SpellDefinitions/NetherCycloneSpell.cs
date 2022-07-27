using Server.Items;
using Server.Targeting;
using System;

namespace Server.Spells.Mysticism;

public class NetherCycloneSpell : MysticSpell
{
	public override SpellCircle Circle => SpellCircle.Eighth;
	public override DamageType SpellDamageType => DamageType.SpellAOE;

	private static readonly SpellInfo m_Info = new(
		"Nether Cyclone", "Grav Hur",
		230,
		9022,
		Reagent.MandrakeRoot,
		Reagent.Nightshade,
		Reagent.SulfurousAsh,
		Reagent.Bloodmoss
	);

	public NetherCycloneSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
	{
	}

	public override void OnCast()
	{
		Caster.Target = new NetherCycloneSpellTarget(this);
	}

	private void OnTarget(IPoint3D p)
	{
		if (p != null && CheckSequence())
		{
			SpellHelper.Turn(Caster, p);
			SpellHelper.GetSurfaceTop(ref p);
			Map map = Caster.Map;

			if (map != null)
			{
				Rectangle2D effectArea = new(p.X - 3, p.Y - 3, 6, 6);
				Effects.PlaySound(p, map, 0x64F);

				for (int x = effectArea.X; x <= effectArea.X + effectArea.Width; x++)
				{
					for (int y = effectArea.Y; y <= effectArea.Y + effectArea.Height; y++)
					{
						if (x == effectArea.X && y == effectArea.Y ||
						    x >= effectArea.X + effectArea.Width - 1 && y >= effectArea.Y + effectArea.Height - 1 ||
						    y >= effectArea.Y + effectArea.Height - 1 && x == effectArea.X ||
						    y == effectArea.Y && x >= effectArea.X + effectArea.Width - 1)
						{
							continue;
						}

						IPoint3D pnt = new Point3D(x, y, p.Z);
						SpellHelper.GetSurfaceTop(ref pnt);

						Timer.DelayCall(TimeSpan.FromMilliseconds(Utility.RandomMinMax(100, 300)), point =>
							{
								Effects.SendLocationEffect(point, map, 0x375A, 8, 11, 0x49A, 0);
							},
							new Point3D(pnt));
					}
				}

				foreach (var d in AcquireIndirectTargets(p, 3))
				{
					Effects.SendTargetParticles(d, 0x374A, 1, 15, 9502, 97, 3, (EffectLayer)255, 0);

					double damage = (Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value / 2) * .66 + Utility.RandomMinMax(1, 6);
					Caster.DoHarmful(d);
					SpellHelper.Damage(this, d, damage, 0, 0, 0, 0, 0, 100, 0);

					if (d is Mobile mobile)
					{
						double stamSap = damage / 3;
						double manaSap = damage / 3;
						double mod = mobile.Skills[SkillName.MagicResist].Value - (Caster.Skills[CastSkill].Value + Caster.Skills[DamageSkill].Value) / 2;

						if (mod > 0)
						{
							mod /= 100;

							stamSap *= mod;
							manaSap *= mod;
						}

						mobile.Stam -= (int)stamSap;
						mobile.Mana -= (int)manaSap;

						Timer.DelayCall(TimeSpan.FromSeconds(10), () =>
						{
							if (!mobile.Alive) return;
							mobile.Stam += (int)stamSap;
							mobile.Mana += (int)manaSap;
						});
					}

					Effects.SendLocationParticles(EffectItem.Create(d.Location, map, EffectItem.DefaultDuration), 0x37CC, 1, 40, 97, 3, 9917, 0);
				}
			}
		}

		FinishSequence();
	}

	public class NetherCycloneSpellTarget : Target
	{
		private NetherCycloneSpell Owner { get; }

		public NetherCycloneSpellTarget(NetherCycloneSpell owner, bool allowland = false)
			: base(12, allowland, TargetFlags.None)
		{
			Owner = owner;
		}

		protected override void OnTarget(Mobile from, object o)
		{
			if (o == null)
			{
				return;
			}

			if (!from.CanSee(o))
			{
				from.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (o is IPoint3D point3D)
			{
				SpellHelper.Turn(from, point3D);
				Owner.OnTarget(point3D);
			}
		}

		protected override void OnTargetFinish(Mobile from)
		{
			Owner.FinishSequence();
		}
	}
}
