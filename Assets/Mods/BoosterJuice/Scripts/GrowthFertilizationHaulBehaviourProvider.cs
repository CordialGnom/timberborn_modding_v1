using Bindito.Core;
using System.Collections.Generic;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockingSystem;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public class GrowthFertilizationHaulBehaviourProvider : BaseComponent, IAwakableComponent, IHaulBehaviorProvider
    {
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private InventoryFillCalculator _inventoryFillCalculator;
        private Inventories _inventories;
        private BlockableObject _blockableObject;
        private FillInputWorkplaceBehavior _fillInputWorkplaceBehavior;
        private EmptyOutputWorkplaceBehavior _emptyOutputWorkplaceBehavior;

        [Inject]
        public void InjectDependencies(InventoryFillCalculator inventoryFillCalculator)
        {
            this._inventoryFillCalculator = inventoryFillCalculator;
        }

        public void Awake()
        {
            this._growthFertilizationBuilding = this.GetComponent<GrowthFertilizationBuilding>();
            this._blockableObject = this.GetComponent<BlockableObject>();
            this._inventories = this.GetComponent<Inventories>();
            this._fillInputWorkplaceBehavior = this.GetComponent<FillInputWorkplaceBehavior>();
            this._emptyOutputWorkplaceBehavior = this.GetComponent<EmptyOutputWorkplaceBehavior>();
        }

        public void GetWeightedBehaviors(IList<WeightedBehavior> weightedBehaviors)
        {
            if (!this._growthFertilizationBuilding || !this._blockableObject.IsUnblocked)
                return;

            foreach (Inventory enabledInventory in this._inventories.EnabledInventories)
            {
                if (enabledInventory.IsInput)
                {
                    float weight = 1f - this._inventoryFillCalculator.GetInputFillPercentage(enabledInventory);
                    if ((double)weight > 0.0)
                        weightedBehaviors.Add(new WeightedBehavior(weight, (WorkplaceBehavior)this._fillInputWorkplaceBehavior));
                }
                if (enabledInventory.IsOutput)
                {
                    float outputFillPercentage = this._inventoryFillCalculator.GetOutputFillPercentage(enabledInventory);
                    if ((double)outputFillPercentage > 0.0)
                        weightedBehaviors.Add(new WeightedBehavior(outputFillPercentage, (WorkplaceBehavior)this._emptyOutputWorkplaceBehavior));
                }
            }
        }
    }
}
