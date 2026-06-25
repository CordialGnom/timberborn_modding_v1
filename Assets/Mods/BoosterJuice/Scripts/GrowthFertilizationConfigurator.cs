using Bindito.Core;
using Timberborn.EntityPanelSystem;
using Timberborn.Emptying;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.TemplateInstantiation;
using Timberborn.Workshops;
using Timberborn.WorkSystem;
using Cordial.Mods.BoosterJuice.Scripts.UI;
using Timberborn.LaborSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    [Context("Game")]
    public class GrowthFertilizationConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            // Building components
            containerDefinition.Bind<GrowthFertilizationAreaService>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationBuilding>().AsTransient();
            containerDefinition.Bind<GrowthFertilizationWorkplaceBehaviour>().AsTransient();
            containerDefinition.Bind<GrowthFertilizationStatusService>().AsTransient();

            // UI fragments
            containerDefinition.Bind<GrowthFertilizationBuildingFragment>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationGrowableFragment>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>()
                .ToProvider<EntityPanelModuleProvider>().AsSingleton();

            // Template module
            containerDefinition.MultiBind<TemplateModule>()
                .ToProvider<TemplateModuleProvider>().AsSingleton();
        }

        private class EntityPanelModuleProvider : IProvider<EntityPanelModule>
        {
            private readonly GrowthFertilizationBuildingFragment _buildingFragment;
            private readonly GrowthFertilizationGrowableFragment _growableFragment;

            public EntityPanelModuleProvider(
                GrowthFertilizationBuildingFragment buildingFragment,
                GrowthFertilizationGrowableFragment growableFragment)
            {
                _buildingFragment = buildingFragment;
                _growableFragment = growableFragment;
            }

            public EntityPanelModule Get()
            {
                var builder = new EntityPanelModule.Builder();
                builder.AddBottomFragment(_buildingFragment);
                builder.AddBottomFragment(_growableFragment);
                return builder.Build();
            }
        }

        private class TemplateModuleProvider : IProvider<TemplateModule>
        {
            private readonly InventoryInitializerFactory _inventoryInitializerFactory;

            public TemplateModuleProvider(InventoryInitializerFactory inventoryInitializerFactory)
            {
                _inventoryInitializerFactory = inventoryInitializerFactory;
            }

            public TemplateModule Get()
            {
                var inventoryService = new GrowthFertilizationInventoryService(_inventoryInitializerFactory);
                var builder = new TemplateModule.Builder();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, GrowthFertilizationBuilding>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, Workshop>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, AutoEmptiableBlocker>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, AutoEmptiable>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, Emptiable>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, HaulCandidate>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, GrowthFertilizationStatusService>();
                //builder.AddDecorator<GrowthFertilizationBuildingSpec, WorkshopProductivityCounter>();
                builder.AddDecorator<Worker, WorkplaceWorkStarter>();
                builder.AddDecorator<GrowthFertilizationStatusService, LackOfResourcesStatus>();
                builder.AddDecorator<GrowthFertilizationStatusService, NoHaulingPostStatus>();
                builder.AddDedicatedDecorator<GrowthFertilizationBuildingSpec, Inventory>(inventoryService);
                builder.AddDecorator<GrowthFertilizationBuildingSpec, GrowthFertilizationWorkplaceBehaviour>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, FillInputWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, EmptyOutputWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, LaborWorkplaceBehavior>();
                builder.AddDecorator<GrowthFertilizationBuildingSpec, WaitInsideIdlyWorkplaceBehavior>();
                return builder.Build();
            }
        }
    }
}