using System.Collections.Immutable;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Timberborn.CoreUI;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    // ---- CSS class constants (from TimberUi/UiCssClasses) ----
    internal static class UiClasses
    {
        public const string Fragment   = "entity-sub-panel";
        public const string FragmentBg = "bg-sub-box--green";
        public const string TopRight   = "top-right-item";
        public const string TextNormal = "game-text-normal";
        public const string TextBold   = "text--bold";
    }

    /// <summary>
    /// Config panel for all three PlantingOverride tools.
    /// Shows a dropdown to select which plant to apply.
    /// For the Tree tool, also shows a "remove cutting area" toggle.
    /// </summary>
    public class PlantingOverridePanel : ILoadableSingleton
    {
        private static readonly string TitleLocKey       = "Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Title";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.Description";
        private static readonly string CutAreaLocKey     = "Cordial.PlantingOverrideTool.PlantingOverrideToolPanel.CuttingAreaRemove";

        private readonly UILayout _uiLayout;
        private readonly EventBus _eventBus;
        private readonly PlantingOverrideSpecService _specService;
        private readonly ILoc _loc;

        private VisualElement _container;
        private VisualElement _fragment;
        private DropdownField _dropdown;
        private Toggle _cutAreaToggle;

        private ImmutableArray<string> _currentItems;
        private bool _isTree;

        public PlantingOverridePanel(
            UILayout uiLayout,
            EventBus eventBus,
            PlantingOverrideSpecService specService,
            ILoc loc)
        {
            _uiLayout    = uiLayout;
            _eventBus    = eventBus;
            _specService = specService;
            _loc         = loc;
        }

        public void Load()
        {
            Build();
            _eventBus.Register(this);
        }

        private void Build()
        {
            _container = new VisualElement();
            _container.style.position = Position.Absolute;
            _container.style.top      = 100;
            _container.style.right    = 10;
            _container.style.width    = 260;

            _fragment = new NineSliceVisualElement();
            _fragment.AddToClassList(UiClasses.Fragment);
            _fragment.AddToClassList(UiClasses.FragmentBg);
            _fragment.style.paddingTop    = 8;
            _fragment.style.paddingBottom = 8;
            _fragment.style.paddingLeft   = 10;
            _fragment.style.paddingRight  = 10;
            _container.Add(_fragment);

            // Title
            var title = new Label(_loc.T(TitleLocKey));
            title.AddToClassList(UiClasses.TextNormal);
            title.AddToClassList(UiClasses.TextBold);
            _fragment.Add(title);

            // Description
            var desc = new Label(_loc.T(DescriptionLocKey));
            desc.AddToClassList(UiClasses.TextNormal);
            desc.style.whiteSpace = WhiteSpace.Normal;
            _fragment.Add(desc);

            // Dropdown
            _dropdown = new DropdownField();
            _dropdown.style.marginTop = 6;
            _dropdown.RegisterValueChangedCallback(evt => OnDropdownChanged(evt.newValue));
            _fragment.Add(_dropdown);

            // Cut area toggle (tree tool only)
            _cutAreaToggle = new Toggle(_loc.T(CutAreaLocKey));
            _cutAreaToggle.AddToClassList("settings-element");
            _cutAreaToggle.AddToClassList("settings-text");
            _cutAreaToggle.AddToClassList("settings-toggle");
            _cutAreaToggle.style.marginTop = 4;
            _cutAreaToggle.RegisterValueChangedCallback(evt =>
                _eventBus.Post(new PlantingOverrideAreaRemoveEvent(evt.newValue)));
            _fragment.Add(_cutAreaToggle);

            _container.ToggleDisplayStyle(false);
            _uiLayout.AddTopRight(_container, 5);
        }

        [OnEvent]
        public void OnPlantingOverrideTreeSelectedEvent(PlantingOverrideTreeSelectedEvent evt)
        {
            if (evt == null) return;
            LoadItems(_specService.GetAllForestryPlantables(), isTree: true);
            _container.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnPlantingOverrideTreeUnselectedEvent(PlantingOverrideTreeUnselectedEvent evt)
        {
            if (evt == null) return;
            _container.ToggleDisplayStyle(false);
        }

        [OnEvent]
        public void OnPlantingOverrideCropSelectedEvent(PlantingOverrideCropSelectedEvent evt)
        {
            if (evt == null) return;
            LoadItems(_specService.GetAllCrops(), isTree: false);
            _container.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnPlantingOverrideCropUnselectedEvent(PlantingOverrideCropUnselectedEvent evt)
        {
            if (evt == null) return;
            _container.ToggleDisplayStyle(false);
        }

        private void LoadItems(ImmutableArray<string> items, bool isTree)
        {
            _isTree       = isTree;
            _currentItems = items;
            _cutAreaToggle.ToggleDisplayStyle(isTree);
            _cutAreaToggle.SetValueWithoutNotify(false);

            var choices = new System.Collections.Generic.List<string>();
            foreach (var name in items)
                choices.Add(LocalizeName(name));

            _dropdown.choices = choices;
            if (choices.Count > 0)
            {
                _dropdown.SetValueWithoutNotify(choices[0]);
                PostConfigChangeEvent(items[0]);
            }
        }

        private void OnDropdownChanged(string localizedValue)
        {
            for (int i = 0; i < _currentItems.Length; i++)
            {
                if (LocalizeName(_currentItems[i]) == localizedValue)
                {
                    PostConfigChangeEvent(_currentItems[i]);
                    return;
                }
            }
        }

        private void PostConfigChangeEvent(string templateName)
        {
            _eventBus.Post(new PlantingOverrideConfigChangeEvent(templateName, _isTree));
        }

        private string LocalizeName(string templateName)
        {
            string key    = "NaturalResource." + templateName.Replace("Bush", "") + ".DisplayName";
            string result = _loc.T(key);
            return string.IsNullOrEmpty(result) ? templateName : result;
        }
    }
}
