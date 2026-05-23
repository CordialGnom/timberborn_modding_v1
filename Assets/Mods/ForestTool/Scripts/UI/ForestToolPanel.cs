using System.Collections.Immutable;
using Timberborn.CoreUI;
using Timberborn.Localization;
using UnityEngine.UIElements;
using UnityEngine;

namespace Cordial.Mods.ForestTool.Scripts.UI
{
    public class ForestToolPanel
    {
        // Loc keys
        private static readonly string TitleLocKey = "Cordial.ForestTool.ForestToolPanel.Title";
        private static readonly string DescriptionLocKey = "Cordial.ForestTool.ForestToolPanel.Description";
        private static readonly string ToggleEmptyLocKey = "Cordial.ForestTool.ForestToolPanel.ToggleEmpty";
        private static readonly string ToggleAllLocKey = "Cordial.ForestTool.ForestToolPanel.ToggleTreeAll";

        // Native game CSS classes (from TimberUi/UiCssClasses)
        private static readonly string FragmentClass = "entity-sub-panel";
        private static readonly string FragmentBgClass = "bg-sub-box--green";
        private static readonly string TopRightClass = "top-right-item";
        private static readonly string TextNormal = "game-text-normal";
        private static readonly string TextBold = "text--bold";
        private static readonly string ToggleClass1 = "settings-element";
        private static readonly string ToggleClass2 = "settings-text";
        private static readonly string ToggleClass3 = "settings-toggle";

        private readonly ForestToolSpecService _forestToolSpecService;
        private readonly ILoc _loc;

        private VisualElement _root;
        private VisualElement _fragment;
        private Toggle _toggleAll;

        public ForestToolPanel(
            ForestToolSpecService forestToolSpecService,
            ILoc loc)
        {
            _forestToolSpecService = forestToolSpecService;
            _loc = loc;
        }

        public VisualElement Build()
        {
            // Outer wrapper — game positions "top-right-item" elements automatically
            _root = new VisualElement();
            _root.AddToClassList(TopRightClass);

            // Panel fragment with native game background
            _fragment = new NineSliceVisualElement();
            _fragment.AddToClassList(FragmentClass);
            _fragment.AddToClassList(FragmentBgClass);
            _fragment.style.paddingTop = 8;
            _fragment.style.paddingLeft = 8;
            _fragment.style.paddingBottom = 8;
            _fragment.style.paddingRight = 8;
            _root.Add(_fragment);

            // Title
            var title = new Label(_loc.T(TitleLocKey));
            title.AddToClassList(TextNormal);
            title.AddToClassList(TextBold);
            _fragment.Add(title);

            // Description
            var description = new Label(_loc.T(DescriptionLocKey));
            description.AddToClassList(TextNormal);
            _fragment.Add(description);

            // Empty spots toggle
            _fragment.Add(MakeToggle(
                _loc.T(ToggleEmptyLocKey),
                ForestToolParam.NameEmpty,
                defaultValue: true));

            // Select all toggle
            _toggleAll = MakeToggle(
                _loc.T(ToggleAllLocKey),
                "TreeAll",
                defaultValue: true);
            _toggleAll.RegisterValueChangedCallback(evt => OnToggleAllChanged(evt.newValue));
            _fragment.Add(_toggleAll);

            // Per-species toggles
            ImmutableArray<string> plantables = _forestToolSpecService.GetAllForestryPlantables();
            foreach (string name in plantables)
            {
                string locKey = "NaturalResource." + name.Replace("Bush", "") + ".DisplayName";
                string label = _loc.T(locKey);
                if (string.IsNullOrEmpty(label))
                    label = name;

                _fragment.Add(MakeToggle(label, name, defaultValue: true, rawLabel: true));
            }
            _root.ToggleDisplayStyle(false);
            return _root;
        }

        public void SetVisible(bool visible)
        {
            _root?.ToggleDisplayStyle(visible);
        }

        private Toggle MakeToggle(string labelOrLocKey, string name, bool defaultValue,
                                   bool rawLabel = false)
        {
            string label = rawLabel ? labelOrLocKey : labelOrLocKey; // already resolved by caller
            var toggle = new Toggle(label)
            {
                name = name,
                value = defaultValue
            };
            toggle.AddToClassList(ToggleClass1);
            toggle.AddToClassList(ToggleClass2);
            toggle.AddToClassList(ToggleClass3);

            if (name != "TreeAll")
            {
                toggle.RegisterValueChangedCallback(evt =>
                    ForestToolParam.SetResourceState(name, evt.newValue));
            }

            return toggle;
        }

        private void OnToggleAllChanged(bool value)
        {
            foreach (VisualElement child in _fragment.Children())
            {
                if (child is Toggle t
                    && t.name != "TreeAll"
                    && t.name != ForestToolParam.NameEmpty)
                {
                    t.SetValueWithoutNotify(value);
                    ForestToolParam.SetResourceState(t.name, value);
                }
            }
        }
    }
}