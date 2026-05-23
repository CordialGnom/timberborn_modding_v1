using Bindito.Core;
using Timberborn.ToolSystem;
using Cordial.Mods.ForestTool.Scripts.UI;

namespace Cordial.Mods.ForestTool.Scripts
{
    [Context("Game")]
    public class ForestToolConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            // Core tool
            containerDefinition.Bind<ForestToolSpecService>().AsSingleton();
            containerDefinition.Bind<ForestToolService>().AsSingleton();

            // Injects the tool button into the Forestry group after the bottom bar is built
            containerDefinition.Bind<ForestToolButtonInitializer>().AsSingleton();

            // Locks the tool unless a Forester building is unlocked
            containerDefinition.MultiBind<IToolLocker>().To<ForestToolLocker>().AsSingleton();

            // UI
            containerDefinition.Bind<ForestToolPanel>().AsSingleton();
            containerDefinition.Bind<ForestToolInitializer>().AsSingleton();

        }
    }
}
