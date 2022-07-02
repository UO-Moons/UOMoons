using System;
using System.Collections.Generic;
using Server.Items;

namespace Server;

public delegate bool SkillCheckTargetHandler(Mobile from, SkillName skill, object target, double minSkill, double maxSkill);
public delegate bool SkillCheckLocationHandler(Mobile from, SkillName skill, double minSkill, double maxSkill);
public delegate bool SkillCheckDirectTargetHandler(Mobile from, SkillName skill, object target, double chance);
public delegate bool SkillCheckDirectLocationHandler(Mobile from, SkillName skill, double chance);
public delegate TimeSpan RegenRateHandler(Mobile from);
public delegate bool AllowBeneficialHandler(Mobile from, Mobile target);
public delegate bool AllowHarmfulHandler(Mobile from, IDamageable target);
public delegate void FatigueHandler(Mobile m, int damage, DfAlgorithm df);
public delegate Container CreateCorpseHandler(Mobile from, HairInfo hair, FacialHairInfo facialhair, List<Item> initialContent, List<Item> equipedItems);
public delegate int AOSStatusHandler(Mobile from, int index);
