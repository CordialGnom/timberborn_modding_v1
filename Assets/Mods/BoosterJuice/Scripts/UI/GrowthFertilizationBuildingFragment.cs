using Timberborn.BaseComponentSystem;
using Timberborn.CoreUI;
using Timberborn.EntityPanelSystem;
using Timberborn.Localization;
using UnityEngine.UIElements;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    sealed class GrowthFertilizationBuildingFragment : IEntityPanelFragment
    {
        private static readonly string ConsumptionLocKey = "Cordial.Building.FertilizerDump.Consumption";
        private static readonly string TreeCountLocKey = "Cordial.Building.FertilizerDump.TreeCount";
        private static readonly string FertilizerNameLocKey = "Cordial.Good.Fertilizer.DisplayName";
        private static readonly string UnitPerHourLocKey = "Cordial.Unit.PerHour";
        private static readonly string FertilizeYieldLocKey = "Cordial.Building.FertilizerDump.FertilizeYield";

        private readonly ILoc _loc;

        private GrowthFertilizationBuilding _building;

        private VisualElement _root;
        private Label _growthStateText;
        private Label _consumptionText;
        private Label _capacityStateText;
        private Toggle _yieldFertilize;

        public GrowthFertilizationBuildingFragment(ILoc loc)
        {
            _loc = loc;
        }

        public VisualElement InitializeFragment()
        {
            _root = new VisualElement();

            var fragment = new NineSliceVisualElement();
            fragment.AddToClassList("entity-sub-panel");
            fragment.AddToClassList("bg-sub-box--green");
            fragment.style.paddingTop = 6;
            fragment.style.paddingBottom = 6;
            fragment.style.paddingLeft = 8;
            fragment.style.paddingRight = 8;
            _root.Add(fragment);

            // Yield fertilize toggle
            _yieldFertilize = new Toggle(_loc.T(FertilizeYieldLocKey));
            _yieldFertilize.AddToClassList("settings-element");
            _yieldFertilize.AddToClassList("settings-text");
            _yieldFertilize.AddToClassList("settings-toggle");
            _yieldFertilize.RegisterValueChangedCallback(evt =>
            {
                if (_building != null)
                    _building.FertilizeYieldActive = evt.newValue;
            });
            fragment.Add(_yieldFertilize);

            // Status labels
            _growthStateText = MakeLabel();
            _capacityStateText = MakeLabel();
            _consumptionText = MakeLabel();

            fragment.Add(_growthStateText);
            fragment.Add(_capacityStateText);
            fragment.Add(_consumptionText);

            _root.ToggleDisplayStyle(false);
            return _root;
        }

        public void ShowFragment(BaseComponent entity)
        {
            _building = entity.GetComponent<GrowthFertilizationBuilding>();

            if (_building == null)
            {
                _root.ToggleDisplayStyle(false);
                return;
            }

            UpdateGrowthState();
            UpdateInventoryState();
            UpdateConsumptionState();
            _yieldFertilize.SetValueWithoutNotify(_building.FertilizeYieldActive);

            _root.ToggleDisplayStyle(true);
        }

        public void ClearFragment()
        {
            _root.ToggleDisplayStyle(false);
            _building = null;
        }

        public void UpdateFragment()
        {
            if (_building == null) return;
            UpdateGrowthState();
            UpdateConsumptionState();
            UpdateInventoryState();
        }

        private void UpdateInventoryState()
        {
            _capacityStateText.text = _loc.T(FertilizerNameLocKey)
                + ": " + _building.SupplyLeft
                + "/" + _building.Capacity;
        }

        private void UpdateConsumptionState()
        {
            _consumptionText.text = _loc.T(ConsumptionLocKey) + " "
                + _building.ConsumptionPerHour.ToString("0.0")
                + _loc.T(UnitPerHourLocKey);
        }

        private void UpdateGrowthState()
        {
            _growthStateText.text = _loc.T(TreeCountLocKey)
                + ": " + _building.TreesGrowCount
                + "/" + _building.TreesTotalCount;
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