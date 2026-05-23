using Timberborn.ModManagerScene;

namespace Cordial.Mods.ForestTool.Scripts
{
    // IModStarter requires StartMod(IModEnvironment modEnvironment) in V1.
    // This class is the entry point the game uses to recognize the mod.
    internal class ForestToolLogger : IModStarter
    {
        public void StartMod(IModEnvironment modEnvironment)
        {
            // No startup logic needed; DI handles everything via configurators.
        }
    }
}