global using Softwyx.LootInVicinity.Localization;
global using Softwyx.LootInVicinity.Config;
global using Softwyx.LootInVicinity.Experience;
global using Softwyx.LootInVicinity.Grid;
global using Softwyx.LootInVicinity.Interop;
global using Softwyx.LootInVicinity.Loot;
global using Softwyx.LootInVicinity.LivPlayer;
global using Softwyx.LootInVicinity.Quest;
global using Softwyx.LootInVicinity.Raid;
global using Softwyx.LootInVicinity.Session;
global using Softwyx.LootInVicinity.Take;
global using Softwyx.LootInVicinity.Ui;
global using Softwyx.LootInVicinity.Ui.Handlers;
global using Softwyx.LootInVicinity.World;

// Operation results (GStruct154 envelope)
global using MoveResult = GStruct154<GClass3411>;
global using QuickFindResult = GStruct154<GInterface424>;
global using ItemUiQuickFindResult = GStruct153;

// Stash grid
global using StashGridCollectionClass = GClass3120;
global using ContainerAddEventClass = GClass3415;
global using ContainerRemoveEventClass = GClass3413;
global using ContainerAddEventResultStruct = GStruct154<GClass3415>;
global using ContainerRemoveEventResultStruct = GStruct154<GClass3413>;
global using GridNullLocationInventoryError = StashGridClass.GClass1542;
global using GridFilterInventoryError = StashGridClass.GClass1543;
global using GridRemoveInventoryError = StashGridClass.GClass1544;

// Trader / world events
global using RemoveItemEventArgs = GEventArgs3;

// UI item contexts
global using RaidInventoryItemContext = GClass3459;

// Physics -- nearest hit along a ray (occlusion / line of sight)
global using NearestHitRaycast = GClass943;
