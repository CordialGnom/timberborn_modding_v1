using Bindito.Core;
using Cordial.Mods.CutterTool.Scripts.UI;

namespace Cordial.Mods.CutterTool.Scripts
{
    [Context("Game")]
    public class CutterToolConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<CutterToolSpecService>().AsSingleton();
            containerDefinition.Bind<CutterToolService>().AsSingleton();
            containerDefinition.Bind<CutterToolButtonInitializer>().AsSingleton();
            containerDefinition.Bind<CutterToolPanel>().AsSingleton();
        }
    }
}
