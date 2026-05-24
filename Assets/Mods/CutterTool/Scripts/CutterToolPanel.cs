using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.CoreUI;
using Timberborn.Localization;
using Timberborn.SingletonSystem;
using Timberborn.UILayoutSystem;
using UnityEngine.UIElements;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts.UI
{
    public class CutterToolPanel : ILoadableSingleton
    {
        // Loc keys
        private static readonly string TitleLocKey       = "Cordial.CutterTool.CutterToolPanel.Title";
        private static readonly string DescriptionLocKey = "Cordial.CutterTool.CutterToolPanel.Description";
        private static readonly string Pattern01LocKey   = "Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern01";
        private static readonly string Pattern02LocKey   = "Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern02";
        private static readonly string Pattern03LocKey   = "Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern03";
        private static readonly string Pattern04LocKey   = "Cordial.CutterTool.CutterToolPanel.AreaConfig.Pattern04";
        private static readonly string TreeMarkLocKey    = "Cordial.CutterTool.CutterToolPanel.TreeConfig.TreeMark";
        private static readonly string TreeAllLocKey     = "Cordial.CutterTool.CutterToolPanel.TreeConfig.TreeAll";
        private static readonly string StumpLocKey       = "Cordial.CutterTool.CutterToolPanel.TreeConfig.IgnoreStump";
        private static readonly string ClearCutLocKey    = "Cordial.CutterTool.CutterToolPanel.TreeConfig.ClearCut";
        private static readonly string SaplingLocKey     = "Cordial.CutterTool.CutterToolPanel.TreeConfig.IgnoreDeadSapling";

        private readonly UILayout _uiLayout;
        private readonly EventBus _eventBus;
        private readonly CutterToolSpecService _specService;
        private readonly ILoc _loc;

        // Pattern toggles (mutually exclusive)
        private Toggle _pattern01;
        private Toggle _pattern02;
        private Toggle _pattern03;
        private Toggle _pattern04;
        private CutterPattern _activePattern = CutterPattern.All;

        // Option toggles
        private Toggle _treeMarkOnly;
        private Toggle _ignoreStumps;
        private Toggle _clearCut;
        private Toggle _ignoreSapling;

        // Per-species toggles
        private Toggle _toggleAll;
        private readonly Dictionary<string, Toggle> _treeToggles = new();

        private VisualElement _container;
        private VisualElement _fragment;

        public CutterToolPanel(
            UILayout uiLayout,
            EventBus eventBus,
            CutterToolSpecService specService,
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
            _fragment.AddToClassList("entity-sub-panel");
            _fragment.AddToClassList("bg-sub-box--green");
            _fragment.style.paddingTop    = 8;
            _fragment.style.paddingBottom = 8;
            _fragment.style.paddingLeft   = 10;
            _fragment.style.paddingRight  = 10;
            _container.Add(_fragment);

            // Title
            var title = new Label(_loc.T(TitleLocKey));
            title.AddToClassList("game-text-normal");
            title.AddToClassList("text--bold");
            _fragment.Add(title);

            // Description
            var desc = new Label(_loc.T(DescriptionLocKey));
            desc.AddToClassList("game-text-normal");
            desc.style.whiteSpace  = WhiteSpace.Normal;
            desc.style.marginBottom = 6;
            _fragment.Add(desc);

            // ---- Pattern section (radio-style) ----
            _fragment.Add(MakeSectionLabel("Pattern"));

            _pattern01 = MakePatternToggle(Pattern01LocKey, CutterPattern.All);
            _pattern02 = MakePatternToggle(Pattern02LocKey, CutterPattern.Checkered);
            _pattern03 = MakePatternToggle(Pattern03LocKey, CutterPattern.LinesX);
            _pattern04 = MakePatternToggle(Pattern04LocKey, CutterPattern.LinesY);

            _fragment.Add(_pattern01);
            _fragment.Add(_pattern02);
            _fragment.Add(_pattern03);
            _fragment.Add(_pattern04);

            // ---- Options section ----
            _fragment.Add(MakeSectionLabel("Options"));

            _treeMarkOnly  = MakeOptionToggle(TreeMarkLocKey,  false);
            _ignoreStumps  = MakeOptionToggle(StumpLocKey,     false);
            _clearCut      = MakeOptionToggle(ClearCutLocKey,  false);
            _ignoreSapling = MakeOptionToggle(SaplingLocKey,   false);

            _fragment.Add(_treeMarkOnly);
            _fragment.Add(_ignoreStumps);
            _fragment.Add(_clearCut);
            _fragment.Add(_ignoreSapling);

            // ---- Tree species section ----
            _fragment.Add(MakeSectionLabel("Species"));

            _toggleAll = MakeOptionToggle(TreeAllLocKey, true);
            _toggleAll.RegisterValueChangedCallback(evt => OnToggleAllChanged(evt.newValue));
            _fragment.Add(_toggleAll);

            ImmutableArray<string> trees = _specService.GetAllTrees();
            foreach (string name in trees)
            {
                string locKey = "NaturalResource." + name + ".DisplayName";
                string label  = _loc.T(locKey);
                if (string.IsNullOrEmpty(label)) label = name;

                var toggle = new Toggle(label) { name = name, value = true };
                toggle.AddToClassList("settings-element");
                toggle.AddToClassList("settings-text");
                toggle.AddToClassList("settings-toggle");
                toggle.RegisterValueChangedCallback(_ => PostConfigEvent());
                _treeToggles[name] = toggle;
                _fragment.Add(toggle);
            }

            _container.ToggleDisplayStyle(false);
            _uiLayout.AddTopRight(_container, 5);

            // Post initial config so service has correct defaults on first use
            PostConfigEvent();
        }

        [OnEvent]
        public void OnCutterToolSelectedEvent(CutterToolSelectedEvent evt)
        {
            _container.ToggleDisplayStyle(true);
        }

        [OnEvent]
        public void OnCutterToolUnselectedEvent(CutterToolUnselectedEvent evt)
        {
            _container.ToggleDisplayStyle(false);
        }

        private Toggle MakePatternToggle(string locKey, CutterPattern pattern)
        {
            var toggle = new Toggle(_loc.T(locKey)) { value = pattern == CutterPattern.All };
            toggle.AddToClassList("settings-element");
            toggle.AddToClassList("settings-text");
            toggle.AddToClassList("settings-toggle");
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (!evt.newValue) { toggle.SetValueWithoutNotify(true); return; } // can't deselect
                _activePattern = pattern;
                SetPatternToggles(pattern);
                PostConfigEvent();
            });
            return toggle;
        }

        private Toggle MakeOptionToggle(string locKey, bool defaultValue)
        {
            var toggle = new Toggle(_loc.T(locKey)) { value = defaultValue };
            toggle.AddToClassList("settings-element");
            toggle.AddToClassList("settings-text");
            toggle.AddToClassList("settings-toggle");
            toggle.RegisterValueChangedCallback(_ => PostConfigEvent());
            return toggle;
        }

        private void SetPatternToggles(CutterPattern active)
        {
            _pattern01.SetValueWithoutNotify(active == CutterPattern.All);
            _pattern02.SetValueWithoutNotify(active == CutterPattern.Checkered);
            _pattern03.SetValueWithoutNotify(active == CutterPattern.LinesX);
            _pattern04.SetValueWithoutNotify(active == CutterPattern.LinesY);
        }

        private void OnToggleAllChanged(bool value)
        {
            foreach (var kvp in _treeToggles)
            {
                kvp.Value.SetValueWithoutNotify(value);
            }
            PostConfigEvent();
        }

        private void PostConfigEvent()
        {
            var dict = new Dictionary<string, bool>();
            foreach (var kvp in _treeToggles)
                dict[kvp.Key] = kvp.Value.value;

            _eventBus.Post(new CutterToolConfigChangeEvent(
                _activePattern,
                _treeMarkOnly.value,
                _ignoreStumps.value,
                _clearCut.value,
                _ignoreSapling.value,
                dict));
        }

        private Label MakeSectionLabel(string text)
        {
            var label = new Label(text);
            label.AddToClassList("game-text-normal");
            label.AddToClassList("text--bold");
            label.style.marginTop = 6;
            return label;
        }
    }
}
