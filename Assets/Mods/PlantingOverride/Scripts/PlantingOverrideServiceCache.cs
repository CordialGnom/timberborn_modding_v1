using Timberborn.BlockObjectTools;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    /// <summary>
    /// Provides static access to game services for Harmony patches,
    /// replacing DependencyContainer.GetInstance calls.
    /// Populated at load time via ILoadableSingleton.
    /// </summary>
    public class PlantingOverrideServiceCache : ILoadableSingleton
    {
        // Accessed statically by Harmony patches
        public static PlantingService PlantingService { get; private set; }
        public static EventBus EventBus { get; private set; }
        public static ToolService ToolService { get; private set; }
        public static BlockObjectPlacerService BlockObjectPlacerService { get; private set; }
        public static PlantingOverrideSpecService SpecService { get; private set; }

        private readonly PlantingService _plantingService;
        private readonly EventBus _eventBus;
        private readonly ToolService _toolService;
        private readonly BlockObjectPlacerService _blockObjectPlacerService;
        private readonly PlantingOverrideSpecService _specService;

        public PlantingOverrideServiceCache(
            PlantingService plantingService,
            EventBus eventBus,
            ToolService toolService,
            BlockObjectPlacerService blockObjectPlacerService,
            PlantingOverrideSpecService specService)
        {
            _plantingService         = plantingService;
            _eventBus                = eventBus;
            _toolService             = toolService;
            _blockObjectPlacerService = blockObjectPlacerService;
            _specService             = specService;
        }

        public void Load()
        {
            PlantingService          = _plantingService;
            EventBus                 = _eventBus;
            ToolService              = _toolService;
            BlockObjectPlacerService = _blockObjectPlacerService;
            SpecService              = _specService;
        }
    }
}
