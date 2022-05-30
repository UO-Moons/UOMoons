using System;

namespace Server.Engines.Harvest
{
    public class BonusHarvestResource
    {
        public BonusHarvestResource(double reqSkill, double chance, TextDefinition message, Type type)
            : this(reqSkill, chance, message, type, null)
        { }
        public BonusHarvestResource(double reqSkill, double chance, TextDefinition message, Type type, Map requiredMap)
        {
            ReqSkill = reqSkill;

            Chance = chance;
            Type = type;
            SuccessMessage = message;
            RequiredMap = requiredMap;
        }

        public Map RequiredMap { get; private set; }

        public Type Type { get; set; }
        public double ReqSkill { get; set; }
        public double Chance { get; set; }
        public TextDefinition SuccessMessage { get; }
        public void SendSuccessTo(Mobile m)
        {
            TextDefinition.SendMessageTo(m, SuccessMessage);
        }
    }
}
