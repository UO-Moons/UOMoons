using System;

namespace Server.Mobiles
{
    public class AuraCreature : BaseCreature
    {
        public DateTime m_AuraDelay;

        #region publicprops
        [CommandProperty(AccessLevel.GameMaster)]
        public int MinAuraDelay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxAuraDelay { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MinAuraDamage { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int MaxAuraDamage { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public int AuraCreatureRange { get; set; }

        [CommandProperty(AccessLevel.GameMaster)]
        public ResistanceType AuraType { get; set; } = ResistanceType.Physical;

        [CommandProperty(AccessLevel.GameMaster)]
        public Poison AuraPoison { get; set; } = null;

        [CommandProperty(AccessLevel.GameMaster)]
        public string AuraMessage { get; set; } = "";
        #endregion

        public AuraCreature(AIType aitype, FightMode fightmode, int spot, int meleerange, double passivespeed, double activespeed)
            : base(aitype, fightmode, spot, meleerange, passivespeed, activespeed)
        {
            m_AuraDelay = DateTime.UtcNow;
            /*
            Default is ?
            AuraMessage = "The intense cold is damaging you!";
            AuraType = ResistanceType.Fire;
            MinAuraDelay = 5;
            MaxAuraDelay = 15;
            MinAuraDamage = 15;
            MaxAuraDamage = 25;
            AuraRange = 3;
            */
        }

        public override void OnThink()
        {
            if (DateTime.UtcNow > m_AuraDelay)
            {
                DebugSay("Auraing");
                Ability.Aura(this, MinAuraDamage, MaxAuraDamage, AuraType, AuraCreatureRange, AuraPoison, AuraMessage);

                m_AuraDelay = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(MinAuraDelay, MaxAuraDelay));
            }

            base.OnThink();
        }

        public AuraCreature(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(MinAuraDelay);
            writer.Write(MaxAuraDelay);
            writer.Write(MinAuraDamage);
            writer.Write(MaxAuraDamage);
            writer.Write(AuraCreatureRange);
            writer.Write((int)AuraType);
            Poison.Serialize(AuraPoison, writer);
            writer.Write(AuraMessage);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            MinAuraDelay = reader.ReadInt();
            MaxAuraDelay = reader.ReadInt();
            MinAuraDamage = reader.ReadInt();
            MaxAuraDamage = reader.ReadInt();
			AuraCreatureRange = reader.ReadInt();
            AuraType = (ResistanceType)reader.ReadInt();
            AuraPoison = Poison.Deserialize(reader);
            AuraMessage = reader.ReadString();
            m_AuraDelay = DateTime.UtcNow;
        }
    }
}
