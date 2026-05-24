using Timberborn.Common;
using Timberborn.Goods;
using Timberborn.InventorySystem;
using Timberborn.TemplateInstantiation;
using UnityEngine;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    internal class GrowthFertilizationInventoryService :
    IDedicatedDecoratorInitializer<GrowthFertilizationBuildingSpec, Inventory>
    {
        private static readonly string InventoryComponentName = "GrowthFertilizationBuilding";
        private readonly InventoryInitializerFactory _inventoryInitializerFactory;

        public GrowthFertilizationInventoryService(
          InventoryInitializerFactory inventoryInitializerFactory)
        {
            this._inventoryInitializerFactory = inventoryInitializerFactory;
        }

        public void Initialize(GrowthFertilizationBuildingSpec subject, Inventory decorator)
        {
            // subject IS the spec — no GetComponent needed
            var inventoryInitializer = _inventoryInitializerFactory.Create(
                decorator, subject.Capacity, "GrowthFertilizationBuilding");
            inventoryInitializer.AddAllowedGoods(
                Enumerables.One<StorableGoodAmount>(
                    new StorableGoodAmount(
                        StorableGood.CreateAsGivable(subject.Supply), subject.Capacity)));
            inventoryInitializer.HasPublicOutput();
            inventoryInitializer.Initialize();

            // Now get the building component to call InitializeInventory
            // At this point GrowthFertilizationBuilding should be available
            var building = decorator.GetComponent<GrowthFertilizationBuilding>();
            building.InitializeInventory(decorator);
        }
    }
}
