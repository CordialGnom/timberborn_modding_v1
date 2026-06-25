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
            Debug.Log("HaulBehaviour Awake - fill null=" + (_fillInputWorkplaceBehavior == null)
                + ", empty null=" + (_emptyOutputWorkplaceBehavior == null));

            var providers = new List<IHaulBehaviorProvider>();
            this.GetComponents<IHaulBehaviorProvider>(providers);
            Debug.Log("Providers on entity: " + providers.Count);
            foreach (var p in providers)
                Debug.Log("Provider: " + p.GetType().Name);
        }

        public void GetWeightedBehaviors(IList<WeightedBehavior> weightedBehaviors)
        {
            Debug.Log("GetWeightedBehaviors called, unblocked=" + _blockableObject.IsUnblocked);
            
            if (!this._growthFertilizationBuilding || !this._blockableObject.IsUnblocked)
                return;

            foreach (Inventory enabledInventory in this._inventories.EnabledInventories)
            {
                if (enabledInventory.IsInput && _fillInputWorkplaceBehavior != null)
                {
                    float weight = 1f - this._inventoryFillCalculator.GetInputFillPercentage(enabledInventory);
                    if (weight > 0f)
                    {
                        Debug.Log("Adding input behavior, null=" + (_fillInputWorkplaceBehavior == null));
                        weightedBehaviors.Add(new WeightedBehavior(weight,
                            (WorkplaceBehavior)this._fillInputWorkplaceBehavior));
                    }
                }
                if (enabledInventory.IsOutput && _emptyOutputWorkplaceBehavior != null)
                {
                    float fill = this._inventoryFillCalculator.GetOutputFillPercentage(enabledInventory);
                    if (fill > 0f)
                    {
                        Debug.Log("Adding output behavior, null=" + (_emptyOutputWorkplaceBehavior == null));
                        weightedBehaviors.Add(new WeightedBehavior(fill,
                            (WorkplaceBehavior)this._emptyOutputWorkplaceBehavior));
                    }
                }
            }
        }
    }
}
