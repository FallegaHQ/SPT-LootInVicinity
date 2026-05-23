using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Softwyx.LootInVicinity.Ui;

/// <summary>Cached Harmony field access for raid inventory UI types.</summary>
internal static class VicinityUiReflection{
    private static FieldInfo _rightPaneField;
    private static FieldInfo _itemsPanelStashField;
    private static FieldInfo _simplePanelField;
    private static FieldInfo _containedGridsViewField;
    private static FieldInfo _simpleGridNameField;
    private static FieldInfo _containerNameField;
    private static FieldInfo _complexStashPanelField;
    private static FieldInfo _panelInventoryField;
    private static FieldInfo _itemsPanelUiField;
    private static FieldInfo _dragLayerField;
    private static bool      _dragLayerResolveAttempted;

    public static bool EnsureItemsPanelFields(){
        if(_itemsPanelStashField != null) return true;

        _rightPaneField =
            AccessTools.Field(typeof(ItemUiContext), GameAssemblyNames.ItemUiContextFields.RightPaneCompoundItems)
         ?? AccessTools.GetDeclaredFields(typeof(ItemUiContext)).
                        FirstOrDefault(f => f.FieldType == typeof(CompoundItem[]));
        _itemsPanelStashField =
            AccessTools.Field(typeof(ItemsPanel), GameAssemblyNames.ItemsPanelFields.SimpleStashPanel)
         ?? AccessTools.GetDeclaredFields(typeof(ItemsPanel)).
                        FirstOrDefault(f => f.FieldType == typeof(SimpleStashPanel));
        _simplePanelField = AccessTools.Field(
                                              typeof(SimpleStashPanel),
                                              GameAssemblyNames.SimpleStashPanelFields.SimplePanel
                                             );
        _containedGridsViewField = AccessTools.Field(
                                                     typeof(SearchableItemView),
                                                     GameAssemblyNames.SearchableItemViewFields.ContainedGridsView
                                                    );
        _simpleGridNameField = AccessTools.Field(
                                                 typeof(SimpleStashPanel),
                                                 GameAssemblyNames.SimpleStashPanelFields.SimpleGridName
                                                );
        _containerNameField = AccessTools.Field(
                                                typeof(SimpleStashPanel),
                                                GameAssemblyNames.SimpleStashPanelFields.ContainerName
                                               );

        return _itemsPanelStashField != null;
    }

    public static SimpleStashPanel GetSimpleStashPanel(ItemsPanel itemsPanel){
        if(!itemsPanel || !EnsureItemsPanelFields()) return null;

        return _itemsPanelStashField.GetValue(itemsPanel) as SimpleStashPanel;
    }

    public static bool IsComplexStashPanelVisible(ItemsPanel itemsPanel){
        if(!itemsPanel) return false;

        _complexStashPanelField ??= AccessTools.Field(
                                                      typeof(ItemsPanel),
                                                      GameAssemblyNames.ItemsPanelFields.ComplexStashPanel
                                                     );
        var complex = _complexStashPanelField?.GetValue(itemsPanel) as Component;

        return complex != null && complex.gameObject.activeInHierarchy;
    }

    public static Inventory GetPanelInventory(ItemsPanel itemsPanel){
        _panelInventoryField ??= AccessTools.Field(typeof(ItemsPanel), GameAssemblyNames.ItemsPanelFields.Inventory);

        return _panelInventoryField?.GetValue(itemsPanel) as Inventory;
    }

    public static AddViewListClass GetItemsPanelUi(ItemsPanel itemsPanel){
        if(!itemsPanel) return null;

        _itemsPanelUiField ??= AccessTools.Field(typeof(UIElement), GameAssemblyNames.UiElementFields.Ui);

        return _itemsPanelUiField?.GetValue(itemsPanel) as AddViewListClass;
    }

    public static Transform GetDragLayer(){
        if(_dragLayerResolveAttempted) return _dragLayerField?.GetValue(ItemUiContext.Instance) as Transform;

        _dragLayerResolveAttempted = true;
        var context = ItemUiContext.Instance;

        if(!context) return null;

        _dragLayerField = AccessTools.Field(typeof(ItemUiContext), GameAssemblyNames.ItemUiContextFields.DragLayer)
                       ?? AccessTools.GetDeclaredFields(typeof(ItemUiContext)).
                                      FirstOrDefault(
                                                     f => f.FieldType == typeof(Transform)
                                                       && f.Name.IndexOf(
                                                                         "drag",
                                                                         System.StringComparison.OrdinalIgnoreCase
                                                                        )
                                                       >= 0
                                                    );

        return _dragLayerField?.GetValue(context) as Transform;
    }

    public static void ClearRightPaneCompoundItem(CompoundItem stash){
        if(stash == null || !EnsureItemsPanelFields() || !ItemUiContext.Instance) return;

        if(_rightPaneField.GetValue(ItemUiContext.Instance) is CompoundItem[]{
                                                                   Length: 1
                                                               } pane
        && pane[0] == stash)
            _rightPaneField.SetValue(ItemUiContext.Instance, null);
    }

    public static void BindGridViews(VicinityStashGrid grid, SimpleStashPanel simpleStashPanel){
        if(grid == null || simpleStashPanel == null || !EnsureItemsPanelFields()) return;

        try{
            var simplePanel = _simplePanelField.GetValue(simpleStashPanel) as SearchableItemView;
            var containedGridsView = simplePanel != null
                                         ? _containedGridsViewField.GetValue(simplePanel) as ContainedGridsView
                                         : null;

            if(containedGridsView != null) grid.GridViews = containedGridsView.GridViews;
        }
        catch{
            // ignored
        }
    }

    public static void ApplyPanelTitle(SimpleStashPanel simpleStashPanel, string title){
        if(simpleStashPanel == null || string.IsNullOrEmpty(title) || !EnsureItemsPanelFields()) return;

        try{
            SetUiText(_simpleGridNameField.GetValue(simpleStashPanel), title);
            SetUiText(_containerNameField.GetValue(simpleStashPanel),  title);
        }
        catch{
            // ignored
        }
    }

    private static void SetUiText(object textComponent, string value){
        if(textComponent == null) return;

        var textProperty = textComponent.GetType().
                                         GetProperty("text");

        if(textProperty != null && textProperty.CanWrite) textProperty.SetValue(textComponent, value, null);
    }
}
