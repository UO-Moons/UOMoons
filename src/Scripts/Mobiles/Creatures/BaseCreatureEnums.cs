using System;

namespace Server.Mobiles
{
	#region Enums
	/// <summary>
	/// Summary description for MobileAI.
	/// </summary>
	///
	public enum FightMode
	{
		None,           // Never focus on others
		Aggressor,      // Only attack aggressors
		Strongest,      // Attack the strongest
		Weakest,        // Attack the weakest
		Closest,        // Attack the closest
		Evil,           // Only attack aggressor -or- negative karma
		Good,           // Only attack aggressor -or- positive karma
		Random          // attacks totally random mob
	}

	public enum OrderType
	{
		None,           //When no order, let's roam
		Come,           //"(All/Name) come"  Summons all or one pet to your location.
		Drop,           //"(Name) drop"  Drops its loot to the ground (if it carries any).
		Follow,         //"(Name) follow"  Follows targeted being.
						//"(All/Name) follow me"  Makes all or one pet follow you.
		Friend,         //"(Name) friend"  Allows targeted player to confirm resurrection.
		Unfriend,       // Remove a friend
		Guard,          //"(Name) guard"  Makes the specified pet guard you. Pets can only guard their owner.
						//"(All/Name) guard me"  Makes all or one pet guard you.
		Attack,         //"(All/Name) kill",
						//"(All/Name) attack"  All or the specified pet(s) currently under your control attack the target.
		Patrol,         //"(Name) patrol"  Roves between two or more guarded targets.
		Release,        //"(Name) release"  Releases pet back into the wild (removes "tame" status).
		Stay,           //"(All/Name) stay" All or the specified pet(s) will stop and stay in current spot.
		Stop,           //"(All/Name) stop Cancels any current orders to attack, guard or follow.
		Transfer        //"(Name) transfer" Transfers complete ownership to targeted player.
	}

	[Flags]
	public enum FoodType
	{
		None = 0x0000,
		Meat = 0x0001,
		FruitsAndVegies = 0x0002,
		GrainsAndHay = 0x0004,
		Fish = 0x0008,
		Eggs = 0x0010,
		Gold = 0x0020,
		Metal = 0x0040,
		Leather = 0x0060,
		BlackrockStew = 0x0080
	}

	[Flags]
	public enum PackInstinct
	{
		None = 0x0000,
		Canine = 0x0001,
		Ostard = 0x0002,
		Feline = 0x0004,
		Arachnid = 0x0008,
		Daemon = 0x0010,
		Bear = 0x0020,
		Equine = 0x0040,
		Bull = 0x0080
	}

	public enum ScaleType
	{
		Red,
		Yellow,
		Black,
		Green,
		White,
		Blue,
		MedusaLight,
		MedusaDark,
		All
	}

	public enum MeatType
	{
		Ribs,
		Bird,
		LambLeg,
		Rotworm,
		DinoRibs,
		SeaSerpentSteak
	}

	public enum FurType
	{
		None,
		Green,
		LightBrown,
		Yellow,
		Brown
	}

	public enum HideType
	{
		Regular,
		Spined,
		Horned,
		Barbed
	}

	public enum TribeType
	{
		None,
		Terathan,
		Ophidian,
		Savage,
		Orc,
		Fey,
		Undead,
		GrayGoblin,
		GreenGoblin
	}

	public enum LootStage
	{
		Spawning,
		Stolen,
		Death
	}

	#endregion
}
