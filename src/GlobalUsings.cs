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

// Operation results
global using MoveResult = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.MoveResult>;
global using DiscardResult = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.DiscardResult>;
global using QuickFindResult = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.IItemOperationResult>;
global using ItemUiQuickFindResult = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.IItemOperationResult>;
global using InventoryStringError = Diz.LanguageExtensions.StringError;

// Stash grid
global using StashGridCollectionClass = EFT.InventoryLogic.GridItemCollection;
global using ContainerAddEventClass = EFT.InventoryLogic.GridAddResult;
global using ContainerRemoveEventClass = EFT.InventoryLogic.ContainerRemoveResult;
global using ContainerAddEventResultStruct = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.GridAddResult>;
global using ContainerRemoveEventResultStruct = Diz.LanguageExtensions.OperationResult<EFT.InventoryLogic.ContainerRemoveResult>;
global using StashGridClass = EFT.InventoryLogic.Grid;
global using GridNullLocationInventoryError = EFT.InventoryLogic.Grid.WontFitToGridError;
global using GridFilterInventoryError = EFT.InventoryLogic.Grid.PlaceTakenByAnotherItemError;
global using GridRemoveInventoryError = EFT.InventoryLogic.Grid.NoFreeSpaceError;

// Trader / world events
global using RemoveItemEventArgs = EFT.InventoryLogic.RemoveItemEventArgs;

// UI item contexts
global using RaidInventoryItemContext = EFT.InventoryLogic.AreaStashItemContext;
global using TransferItemContext = EFT.UI.ReferenceItemContext;
