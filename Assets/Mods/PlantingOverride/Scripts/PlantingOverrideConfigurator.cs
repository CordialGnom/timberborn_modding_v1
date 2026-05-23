using Bindito.Core;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    [Context("Game")]
    public class PlantingOverrideConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            // ---- Shared services ----
            containerDefinition.Bind<PlantingOverrideSpecService>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideServiceCache>().AsSingleton();

            // ---- Tree override tool ----
            containerDefinition.Bind<PlantingOverrideTreeService>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideTreeButtonInitializer>().AsSingleton();
            containerDefinition.MultiBind<IToolLocker>().To<PlantingOverrideTreeToolLocker>().AsSingleton();

            // ---- Crop override tool ----
            containerDefinition.Bind<PlantingOverrideCropService>().AsSingleton();
            containerDefinition.Bind<PlantingOverrideCropButtonInitializer>().AsSingleton();

            // ---- Beehive tool ----
            containerDefinition.Bind<PlantBeehive.PlantBeehiveToolService>().AsSingleton();
            containerDefinition.Bind<PlantBeehiveButtonInitializer>().AsSingleton();
            containerDefinition.MultiBind<IToolLocker>().To<PlantBeehiveToolLocker>().AsSingleton();

            // ---- UI ----
            containerDefinition.Bind<PlantingOverridePanel>().AsSingleton();
        }
    }
}
