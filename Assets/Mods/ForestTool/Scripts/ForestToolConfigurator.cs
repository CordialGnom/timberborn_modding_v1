using Bindito.Core;
using Timberborn.BottomBarSystem;
using Timberborn.ToolSystem;

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

            // UI fragments - keep as-is, will need separate review
            //containerDefinition.bind<foresttoolinitializer>().assingleton();
            //containerdefinition.bind<panelfragment>().assingleton();
            //containerdefinition.bind<panelfragmentblue>().assingleton();
            //containerdefinition.bind<panelfragmentred>().assingleton();
            //containerdefinition.bind<foresttoolconfigfragment>().assingleton();
            //containerdefinition.Bind<ForestToolErrorPrompt>().AsSingleton();
        }
    }
}
