using Bindito.Core;
using Timberborn.EntityPanelSystem;

namespace Cordial.Mods.BoosterJuice.Scripts.UI
{
    [Context("Game")]
    public class GrowthFertilizationUIConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<GrowthFertilizationAreaService>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationBuildingFragment>().AsSingleton();
            containerDefinition.Bind<GrowthFertilizationGrowableFragment>().AsSingleton();
            containerDefinition.MultiBind<EntityPanelModule>()
                .ToProvider<EntityPanelModuleProvider>().AsSingleton();
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
    }
}