using Timberborn.ToolSystem;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolFactory : IToolFactory
    {
        private readonly IForestTool _ForestTool;
        public string Id => "ForestTool";
        public ForestToolFactory(IForestTool ForestTool)
        {
            _ForestTool = ForestTool;
        }

        public ITool Create(ToolSpec toolSpecification, ToolGroupSpec toolGroup = null)
        {
            _ForestTool.SetToolGroup(toolGroup);
            return (Tool)_ForestTool;
        }

    }
}
