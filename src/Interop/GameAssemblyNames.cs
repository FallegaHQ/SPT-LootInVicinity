using EFT.UI;
using EFT.UI.DragAndDrop;

namespace Softwyx.LootInVicinity.Interop;

/// <summary>SPT 4.1.x private/internal field and property names. Reference these instead of raw strings in mod logic.</summary>
internal static class GameAssemblyNames{
    internal static class ItemUiContextFields{
        /// <summary>Field: <see cref="ItemUiContext._rightPanelItem" /></summary>
        public const string RightPaneCompoundItems = "_rightPanelItem";

        /// <summary>Property: <see cref="ItemUiContext.DragLayer" /></summary>
        public const string DragLayer = "DragLayer";
    }

    internal static class ItemsPanelFields{
        /// <summary>Field: <see cref="ItemsPanel._inventory" /></summary>
        public const string Inventory = "_inventory";

        /// <summary>Field: <see cref="ItemsPanel._simpleStashPanel" /></summary>
        public const string SimpleStashPanel = "_simpleStashPanel";

        /// <summary>Field: <see cref="ItemsPanel._complexStashPanel" /></summary>
        public const string ComplexStashPanel = "_complexStashPanel";
    }

    internal static class SimpleStashPanelFields{
        /// <summary>Field: <see cref="SimpleStashPanel._simplePanel" /></summary>
        public const string SimplePanel = "_simplePanel";

        /// <summary>Field: <see cref="SimpleStashPanel._simpleGridName" /></summary>
        public const string SimpleGridName = "_simpleGridName";

        /// <summary>Field: <see cref="SimpleStashPanel._containerName" /></summary>
        public const string ContainerName = "_containerName";
    }

    internal static class SearchableItemViewFields{
        /// <summary>Field: <see cref="SearchableItemView._containedGridsView" /></summary>
        public const string ContainedGridsView = "_containedGridsView";
    }

    internal static class UiElementFields{
        /// <summary>Field: <see cref="UIElement.UI" /></summary>
        public const string Ui = "UI";
    }
}
