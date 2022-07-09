using Server.Items;
using System;

namespace Server.Engines.Distillation;

public class CraftDefinition
{
	public Group Group { get; }

	public Liquor Liquor { get; }

	public Type[] Ingredients { get; }

	public int[] Amounts { get; }

	public int[] Labels { get; }

	public TimeSpan MaturationDuration { get; }

	public CraftDefinition(Group group, Liquor liquor, Type[] ingredients, int[] amounts, TimeSpan matureperiod)
	{
		Group = group;
		Liquor = liquor;
		Ingredients = ingredients;
		Amounts = amounts;
		MaturationDuration = matureperiod;

		Labels = new int[Ingredients.Length];

		for (int i = 0; i < Ingredients.Length; i++)
		{
			Type type = Ingredients[i];

			if (type == typeof(Yeast))
				Labels[i] = 1150453;
			else if (type == typeof(WheatWort))
				Labels[i] = 1150275;
			else if (type == typeof(PewterBowlOfCorn))
				Labels[i] = 1025631;
			else if (type == typeof(PewterBowlOfPotatos))
				Labels[i] = 1025634;
			else if (type == typeof(TribalBerry))
				Labels[i] = 1040001;
			else if (type == typeof(HoneydewMelon))
				Labels[i] = 1023189;
			else if (type == typeof(JarHoney))
				Labels[i] = 1022540;
			else if (type == typeof(Pitcher))
			{
				if (Liquor == Liquor.Brandy)
					Labels[i] = 1028091;      // pitcher of wine
				else
					Labels[i] = 1024088;      // pitcher of water
			}
			else if (type == typeof(Dates))
				Labels[i] = 1025927;
			else
			{
				Item item = Loot.Construct(type);
				if (item != null)
				{
					Labels[i] = item.LabelNumber;
					item.Delete();
				}
			}

		}
	}
}
