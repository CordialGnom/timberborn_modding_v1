using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Forestry;
using Timberborn.Gathering;
using Timberborn.Growing;
using Timberborn.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    sealed class GrowthFertilizationGrowableFragment : IEntityPanelFragment
    {
        private static readonly string TitleLocKey = "Cordial.TreeFragment.Title";
        private static readonly string GrowthDailyLocKey = "Cordial.TreeFragment.GrowthDaily";
        private static readonly string GrowthAvgLocKey = "Cordial.TreeFragment.GrowthAverage";
        private static readonly string YieldAvgLocKey = "Cordial.TreeFragment.YieldAverage";
        private static readonly string UnitDayLocKey = "Cordial.Unit.Days";

        private readonly ILoc _loc;
        private readonly GrowthFertilizationAreaService _areaService;

        private TreeComponent _treeComponent;
        private Vector3Int _coordinates = Vector3Int.zero;

        private VisualElement _root;
        private VisualElement _growthElement;
        private VisualElement _yieldElement;
        private Label _title;
        private Label _growthDailyInfo;
        private Label _growthAvgInfo;
        private Label _yieldAvgInfo;

        public GrowthFertilizationGrowableFragment(
            ILoc loc,
            GrowthFertilizationAreaService areaService)
        {
            _loc = loc;
            _areaService = areaService;
        }

        public VisualElement InitializeFragment()
        {
            _root = new NineSliceVisualElement();
            _root.AddToClassList("entity-sub-panel");
            _root.AddToClassList("bg-sub-box--green");
            _root.style.paddingTop = 6;
            _root.style.paddingBottom = 6;
            _root.style.paddingLeft = 8;
            _root.style.paddingRight = 8;

            _title = MakeLabel();
            _title.AddToClassList("text--bold");
            _root.Add(_title);

            _growthElement = new VisualElement();
            _growthDailyInfo = MakeLabel();
            _growthAvgInfo = MakeLabel();
            _growthElement.Add(_growthDailyInfo);
            _growthElement.Add(_growthAvgInfo);
            _root.Add(_growthElement);

            _yieldElement = new VisualElement();
            _yieldAvgInfo = MakeLabel();
            _yieldElement.Add(_yieldAvgInfo);
            _root.Add(_yieldElement);

            _root.ToggleDisplayStyle(false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            _treeComponent = entity.GetComponent<TreeComponent>();

            if (_treeComponent == null)
            {
                _root.ToggleDisplayStyle(false);
                return;
            }

            _treeComponent.TryGetComponent<BlockObject>(out var blockObject);
            _treeComponent.TryGetComponent<Growable>(out var growable);
            _treeComponent.TryGetComponent<GatherableYieldGrower>(out var yieldGrower);

            if (blockObject == null || growable == null
                || !_areaService.CheckCoordinateFertilizationArea(blockObject.Coordinates))
            {
                _coordinates = Vector3Int.zero;
                _root.ToggleDisplayStyle(false);
                return;
            }

            _coordinates = blockObject.Coordinates;
            _title.text = _loc.T(TitleLocKey);

            if (!growable.IsGrown)
            {
                _growthElement.ToggleDisplayStyle(true);
                _yieldElement.ToggleDisplayStyle(false);

                _growthDailyInfo.text = _loc.T(GrowthDailyLocKey) + " "
                    + _areaService.GetGrowthProgessDaily(_coordinates).ToString("0.0") + " %";

                float decrease = growable.GrowthTimeInDays
                    * _areaService.GetGrowthFactor()
                    * _areaService.GetGrowthProgessAverage(_coordinates);
                _growthAvgInfo.text = _loc.T(GrowthAvgLocKey) + " "
                    + decrease.ToString("0.0") + " " + _loc.T(UnitDayLocKey);
            }
            else if (yieldGrower != null)
            {
                _treeComponent.TryGetComponent<Gatherable>(out var gatherable);
                if (gatherable != null)
                {
                    float yieldDecrease = gatherable.YieldGrowthTimeInDays
                        * _areaService.GetYieldFactor()
                        * (_areaService.GetYieldProgessAverage(_coordinates) / 100f);
                    _yieldAvgInfo.text = _loc.T(YieldAvgLocKey) + " "
                        + yieldDecrease.ToString("0.0") + " " + _loc.T(UnitDayLocKey);
                    _yieldElement.ToggleDisplayStyle(true);
                }
                _growthElement.ToggleDisplayStyle(false);
            }
            else
            {
                _root.ToggleDisplayStyle(false);
                return;
            }

            _root.ToggleDisplayStyle(true);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(false);
            _treeComponent = null;
            _coordinates = Vector3Int.zero;
        }

        public void UpdateFragment()
        {
            if (_treeComponent == null || _coordinates == Vector3Int.zero) return;
            _growthDailyInfo.text = _loc.T(GrowthDailyLocKey) + " "
                + _areaService.GetGrowthProgessDaily(_coordinates).ToString("0.0") + " %";
        }

        private static Label MakeLabel()
        {
            var label = new Label();
            label.AddToClassList("game-text-normal");
            label.style.marginTop = 2;
            return label;
        }
    }
}