using Timberborn.AssetSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolButtonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.ForestTool.Scripts
{
    /// <summary>
    /// Injects the ForestTool button into the existing Forestry tool group.
    /// Runs after the bottom bar is fully built (IPostLoadableSingleton).
    /// </summary>
    public class ForestToolButtonInitializer : IPostLoadableSingleton
    {
        private static readonly string IconPath = "Sprites/ForestTool/ForestToolIcon";
        private static readonly string ForestryGroupId = "Forestry";

        private readonly ForestToolService _forestToolService;
        private readonly ToolButtonFactory _toolButtonFactory;
        private readonly ToolButtonService _toolButtonService;
        private readonly ToolGroupService _toolGroupService;
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly IAssetLoader _assetLoader;

        public ForestToolButtonInitializer(
            ForestToolService forestToolService,
            ToolButtonFactory toolButtonFactory,
            ToolButtonService toolButtonService,
            ToolGroupService toolGroupService,
            ToolUnlockingService toolUnlockingService,
            IAssetLoader assetLoader)
        {
            _forestToolService = forestToolService;
            _toolButtonFactory = toolButtonFactory;
            _toolButtonService = toolButtonService;
            _toolGroupService = toolGroupService;
            _toolUnlockingService = toolUnlockingService;
            _assetLoader = assetLoader;
        }

        public void PostLoad()
        {


            // Find the Forestry ToolGroupButton using the publicized _toolGroupButtons list
            ToolGroupButton forestryGroupButton = null;
            foreach (ToolGroupButton groupButton in _toolButtonService._toolGroupButtons)
            {
                if (groupButton._toolGroup.Id == ForestryGroupId)
                {
                    forestryGroupButton = groupButton;
                    break;
                }
            }

            if (forestryGroupButton == null)
            {
                Debug.LogError("ForestTool: Could not find Forestry tool group button.");
                return;
            }

            // Load the mod's icon from its asset bundle
            Sprite icon = _assetLoader.Load<Sprite>(IconPath);
            if (icon == null)
            {
                Debug.LogWarning("ForestTool: Icon not found at path: " + IconPath + ". Button will have no icon.");
            }

            // Create the tool button
            ToolButton toolButton = _toolButtonFactory.Create(
                _forestToolService,
                icon,
                forestryGroupButton.ToolButtonsElement);

            // Wire up the tool with the group
            _toolGroupService.AssignToGroup(
                _toolGroupService.GetGroup(ForestryGroupId),
                _forestToolService);

            forestryGroupButton.AddTool(toolButton);

            // ToolButtonService.PostLoad() has already run, so our button
            // missed the loop — call PostLoad() manually to register click/hover
            toolButton.PostLoad();

            // Manually trigger lock check since we missed ToolButtonService.PostLoad()
            _toolUnlockingService.LockIfNeeded(_forestToolService);
        }
    }
}