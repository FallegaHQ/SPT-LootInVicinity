using System;
using System.Reflection;
using EFT.InventoryLogic;

namespace Softwyx.LootInVicinity.Interop;

/// <summary>Subscribe to world/trader <c>RemoveItemEvent</c> without direct member access.</summary>
internal static class VicinityItemOwnerEvents{
    private const string RemoveItemEventName = "RemoveItemEvent";

    private static readonly EventInfo RemoveItemEventInfo =
        typeof(IItemOwner).GetEvent(RemoveItemEventName) ?? typeof(TraderControllerClass).GetEvent(RemoveItemEventName);

    public static void AddRemoveHandler(IItemOwner owner, Action<RemoveItemEventArgs> handler){
        if(owner == null || handler == null || RemoveItemEventInfo == null) return;

        RemoveItemEventInfo.AddEventHandler(owner, handler);
    }

    public static void RemoveRemoveHandler(IItemOwner owner, Action<RemoveItemEventArgs> handler){
        if(owner == null || handler == null || RemoveItemEventInfo == null) return;

        RemoveItemEventInfo.RemoveEventHandler(owner, handler);
    }
}
