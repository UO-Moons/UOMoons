using System;
namespace Server.Mobiles;

[Flags]
public enum PlayerFlag // First 16 bits are reserved for default-distro use, start custom flags at 0x00010000
{
	None = 0x00000000,
	Glassblowing = 0x00000001,
	Masonry = 0x00000002,
	SandMining = 0x00000004,
	StoneMining = 0x00000008,
	ToggleMiningStone = 0x00000010,
	KarmaLocked = 0x00000020,
	AutoRenewInsurance = 0x00000040,
	UseOwnFilter = 0x00000080,
	Unused = 0x00000100,
	PagingSquelched = 0x00000200,
	Young = 0x00000400,
	AcceptGuildInvites = 0x00000800,
	DisplayChampionTitle = 0x00001000,
	HasStatReward = 0x00002000,
	Bedlam = 0x00010000,
	LibraryFriend = 0x00020000,
	Spellweaving = 0x00040000,
	GemMining = 0x00080000,
	ToggleMiningGem = 0x00100000,
	BasketWeaving = 0x00200000,
	AbyssEntry = 0x00400000,
	ToggleClippings = 0x00800000,
	ToggleCutClippings = 0x01000000,
	ToggleCutReeds = 0x02000000,
	MechanicalLife = 0x04000000,
	Unusesd = 0x08000000,
	ToggleCutTopiaries = 0x10000000,
	HasValiantStatReward = 0x20000000,
	RefuseTrades = 0x40000000,
}

[Flags]
public enum ExtendedPlayerFlag
{
	Unused = 0x00000001,
	ToggleStoneOnly = 0x00000002,
	CanBuyCarpets = 0x00000004,
	VoidPool = 0x00000008,
	DisabledPvpWarning = 0x00000010,
}

public enum NpcGuild
{
	None,
	MagesGuild,
	WarriorsGuild,
	ThievesGuild,
	RangersGuild,
	HealersGuild,
	MinersGuild,
	MerchantsGuild,
	TinkersGuild,
	TailorsGuild,
	FishermensGuild,
	BardsGuild,
	BlacksmithsGuild
}

public enum SolenFriendship
{
	None,
	Red,
	Black
}
