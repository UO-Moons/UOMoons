using System;
using Server.Items;
using Server.Spells;
using Server.Spells.Ninjitsu;

namespace Server.Mobiles
{
    public class MirrorImageClone : BaseCreature
    {
        private Mobile m_Caster;
        public MirrorImageClone(Mobile caster)
            : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.2, 0.4)
        {
            m_Caster = caster;

            Body = caster.Body;

            Hue = caster.Hue;
            Female = caster.Female;

            Name = caster.Name;
            NameHue = caster.NameHue;

            Title = caster.Title;
            Kills = caster.Kills;

            HairItemId = caster.HairItemId;
            HairHue = caster.HairHue;

            FacialHairItemId = caster.FacialHairItemId;
            FacialHairHue = caster.FacialHairHue;

            for (int i = 0; i < caster.Skills.Length; ++i)
            {
                Skills[i].Base = caster.Skills[i].Base;
                Skills[i].Cap = caster.Skills[i].Cap;
            }

            for (int i = 0; i < caster.Items.Count; i++)
            {
                AddItem(CloneItem(caster.Items[i]));
            }

            Warmode = true;

            Summoned = true;
            SummonMaster = caster;

            ControlOrder = OrderType.Follow;
            ControlTarget = caster;

            TimeSpan duration = TimeSpan.FromSeconds(30 + caster.Skills.Ninjitsu.Fixed / 40);

            new UnsummonTimer(caster, this, duration).Start();
            SummonEnd = DateTime.UtcNow + duration;

            MirrorImage.AddClone(m_Caster);

            IgnoreMobiles = true;
        }

        public MirrorImageClone(Serial serial)
            : base(serial)
        {
        }

        public override bool DeleteCorpseOnDeath => true;
        public override bool IsDispellable => false;
        public override bool Commandable => false;
        protected override BaseAI ForcedAi => new CloneAi(this);

        public override bool CanDetectHidden => false;

        public override bool IsHumanInTown()
        {
            return false;
        }

        public override bool OnMoveOver(Mobile m)
        {
            return true;
        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            Delete();
        }

        public override void OnDelete()
        {
            Effects.SendLocationParticles(EffectItem.Create(Location, Map, EffectItem.DefaultDuration), 0x3728, 10, 15, 5042);

            base.OnDelete();
        }

        public override void OnAfterDelete()
        {
            MirrorImage.RemoveClone(m_Caster);
            base.OnAfterDelete();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(m_Caster);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            reader.ReadEncodedInt();

            m_Caster = reader.ReadMobile();

            MirrorImage.AddClone(m_Caster);
        }

        private Item CloneItem(Item item)
        {
            Item newItem = new Item(item.ItemId)
            {
	            Hue = item.Hue,
	            Layer = item.Layer
            };

            return newItem;
        }
    }
}

namespace Server.Mobiles
{
    public class CloneAi : BaseAI
    {
        public CloneAi(MirrorImageClone m)
            : base(m)
        {
            m.CurrentSpeed = m.ActiveSpeed;
        }

        public override bool Think()
        {
            // Clones only follow their owners
            Mobile master = m_Mobile.SummonMaster;

            if (master != null && master.Map == m_Mobile.Map && master.InRange(m_Mobile, m_Mobile.RangePerception))
            {
                int iCurrDist = (int)m_Mobile.GetDistanceToSqrt(master);
                bool bRun = iCurrDist > 5;

                WalkMobileRange(master, 2, bRun, 0, 1);
            }
            else
                WalkRandom(2, 2, 1);

            return true;
        }
    }
}
