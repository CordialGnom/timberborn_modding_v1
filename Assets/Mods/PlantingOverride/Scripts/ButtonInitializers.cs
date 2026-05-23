using Timberborn.AssetSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolButtonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    // -------------------------------------------------------------------------
    // PlantingOverrideTree -> Forestry group
    // -------------------------------------------------------------------------
    public class PlantingOverrideTreeButtonInitializer : IPostLoadableSingleton
    {
        private static readonly string IconPath      = "Sprites/PlantingOverride/PlantingOverrideTreeIcon";
        private static readonly string GroupId       = "Forestry";

        private readonly PlantingOverrideTreeService _tool;
        private readonly ToolButtonFactory           _toolButtonFactory;
        private readonly ToolButtonService           _toolButtonService;
        private readonly ToolGroupService            _toolGroupService;
        private readonly ToolUnlockingService        _toolUnlockingService;
        private readonly IAssetLoader                _assetLoader;

        public PlantingOverrideTreeButtonInitializer(
            PlantingOverrideTreeService tool,
            ToolButtonFactory toolButtonFactory,
            ToolButtonService toolButtonService,
            ToolGroupService toolGroupService,
            ToolUnlockingService toolUnlockingService,
            IAssetLoader assetLoader)
        {
            _tool                = tool;
            _toolButtonFactory   = toolButtonFactory;
            _toolButtonService   = toolButtonService;
            _toolGroupService    = toolGroupService;
            _toolUnlockingService = toolUnlockingService;
            _assetLoader         = assetLoader;
        }

        public void PostLoad()
        {
            ToolGroupButton group = FindGroup(GroupId);
            if (group == null) { Debug.LogError("PO: Forestry group not found."); return; }

            Sprite icon = _assetLoader.Load<Sprite>(IconPath);
            var button  = _toolButtonFactory.Create(_tool, icon, group.ToolButtonsElement);
            _toolGroupService.AssignToGroup(_toolGroupService.GetGroup(GroupId), _tool);
            group.AddTool(button);
            button.PostLoad();
            _toolUnlockingService.LockIfNeeded(_tool);
        }

        private ToolGroupButton FindGroup(string id)
        {
            foreach (var g in _toolButtonService._toolGroupButtons)
                if (g._toolGroup.Id == id) return g;
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // PlantingOverrideCrop -> Fields group
    // -------------------------------------------------------------------------
    public class PlantingOverrideCropButtonInitializer : IPostLoadableSingleton
    {
        private static readonly string IconPath = "Sprites/PlantingOverride/PlantingOverrideCropIcon";
        private static readonly string GroupId  = "Fields";

        private readonly PlantingOverrideCropService _tool;
        private readonly ToolButtonFactory           _toolButtonFactory;
        private readonly ToolButtonService           _toolButtonService;
        private readonly ToolGroupService            _toolGroupService;
        private readonly ToolUnlockingService        _toolUnlockingService;
        private readonly IAssetLoader                _assetLoader;

        public PlantingOverrideCropButtonInitializer(
            PlantingOverrideCropService tool,
            ToolButtonFactory toolButtonFactory,
            ToolButtonService toolButtonService,
            ToolGroupService toolGroupService,
            ToolUnlockingService toolUnlockingService,
            IAssetLoader assetLoader)
        {
            _tool                = tool;
            _toolButtonFactory   = toolButtonFactory;
            _toolButtonService   = toolButtonService;
            _toolGroupService    = toolGroupService;
            _toolUnlockingService = toolUnlockingService;
            _assetLoader         = assetLoader;
        }

        public void PostLoad()
        {
            ToolGroupButton group = FindGroup(GroupId);
            if (group == null) { Debug.LogError("PO: Fields group not found."); return; }

            Sprite icon = _assetLoader.Load<Sprite>(IconPath);
            var button  = _toolButtonFactory.Create(_tool, icon, group.ToolButtonsElement);
            _toolGroupService.AssignToGroup(_toolGroupService.GetGroup(GroupId), _tool);
            group.AddTool(button);
            button.PostLoad();
            _toolUnlockingService.LockIfNeeded(_tool);
        }

        private ToolGroupButton FindGroup(string id)
        {
            foreach (var g in _toolButtonService._toolGroupButtons)
                if (g._toolGroup.Id == id) return g;
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // PlantBeehive -> Fields group
    // -------------------------------------------------------------------------
    public class PlantBeehiveButtonInitializer : IPostLoadableSingleton
    {
        private static readonly string IconPath = "Sprites/PlantingOverride/PlantBeehiveIcon";
        private static readonly string GroupId  = "Fields";

        private readonly PlantBeehive.PlantBeehiveToolService _tool;
        private readonly ToolButtonFactory                    _toolButtonFactory;
        private readonly ToolButtonService                    _toolButtonService;
        private readonly ToolGroupService                     _toolGroupService;
        private readonly ToolUnlockingService                 _toolUnlockingService;
        private readonly IAssetLoader                         _assetLoader;
        private readonly Common.PlantingOverrideSpecService   _specService;
        private readonly Timberborn.Buildings.BuildingService _buildingService;

        public PlantBeehiveButtonInitializer(
            PlantBeehive.PlantBeehiveToolService tool,
            ToolButtonFactory toolButtonFactory,
            ToolButtonService toolButtonService,
            ToolGroupService toolGroupService,
            ToolUnlockingService toolUnlockingService,
            IAssetLoader assetLoader,
            Common.PlantingOverrideSpecService specService,
            Timberborn.Buildings.BuildingService buildingService)
        {
            _tool                = tool;
            _toolButtonFactory   = toolButtonFactory;
            _toolButtonService   = toolButtonService;
            _toolGroupService    = toolGroupService;
            _toolUnlockingService = toolUnlockingService;
            _assetLoader         = assetLoader;
            _specService         = specService;
            _buildingService     = buildingService;
        }

        public void PostLoad()
        {
            // Only add button if a Beehive building exists for this faction/mod
            if (!_specService.BeehiveExists(_buildingService))
            {
                Debug.Log("PO: No Beehive building found, skipping PlantBeehive button.");
                return;
            }

            ToolGroupButton group = FindGroup(GroupId);
            if (group == null) { Debug.LogError("PO: Fields group not found for BeehiveTool."); return; }

            Sprite icon = _assetLoader.Load<Sprite>(IconPath);
            var button  = _toolButtonFactory.Create(_tool, icon, group.ToolButtonsElement);
            _toolGroupService.AssignToGroup(_toolGroupService.GetGroup(GroupId), _tool);
            group.AddTool(button);
            button.PostLoad();
            _toolUnlockingService.LockIfNeeded(_tool);
        }

        private ToolGroupButton FindGroup(string id)
        {
            foreach (var g in _toolButtonService._toolGroupButtons)
                if (g._toolGroup.Id == id) return g;
            return null;
        }
    }
}
