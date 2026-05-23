using System;
using System.Collections.Generic;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolSystem;
using UnityEngine;
using Cordial.Mods.ForestTool.Scripts.UI.Events;
using Timberborn.PlantingUI;
using Timberborn.SelectionSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystemUI;

namespace Cordial.Mods.ForestTool.Scripts
{
    public class ForestToolService : ITool, ILoadableSingleton, IToolDescriptor
    {
        private static readonly string TitleLocKey = "Cordial.ForestTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.ForestTool.Description";
        private static readonly string CursorKey = "PlantingCursor";

        private readonly ILoc _loc;
        private readonly EventBus _eventBus;

        // Area selection
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;

        // Planting
        private readonly PlantingAreaValidator _plantingAreaValidator;
        private readonly PlantingSelectionService _plantingSelectionService;
        private readonly PlantingService _plantingService;
        private readonly TerrainAreaService _terrainAreaService;

        // Highlighting
        private readonly AreaHighlightingService _areaHighlightingService;
        public Color _plantingToolTile;
        public Color _toolNoActionTileColor;

        private readonly ForestToolSpecService _forestToolSpecService;

        // Runtime config
        private Dictionary<string, bool> _treeToggleDict = new();

        public ForestToolService(
            SelectionToolProcessorFactory selectionToolProcessorFactory,
            PlantingSelectionService plantingSelectionService,
            PlantingAreaValidator plantingAreaValidator,
            AreaHighlightingService areaHighlightingService,
            PlantingService plantingService,
            TerrainAreaService terrainAreaService,
            ToolUnlockingService toolUnlockingService,
            ILoc loc,
            EventBus eventBus,
            ForestToolSpecService forestToolSpecService)
        {
            _selectionToolProcessor = selectionToolProcessorFactory.Create(
                new Action<IEnumerable<Vector3Int>, Ray>(PreviewCallback),
                new Action<IEnumerable<Vector3Int>, Ray>(ActionCallback),
                new Action(ShowNoneCallback),
                CursorKey);

            _plantingAreaValidator = plantingAreaValidator;
            _plantingService = plantingService;
            _plantingSelectionService = plantingSelectionService;
            _terrainAreaService = terrainAreaService;
            _toolUnlockingService = toolUnlockingService;
            _forestToolSpecService = forestToolSpecService;
            _areaHighlightingService = areaHighlightingService;
            _eventBus = eventBus;
            _loc = loc;
        }

        public void Load()
        {
            _eventBus.Register(this);
            _plantingToolTile = new Color(0, 0.8f, 0, 1);
            _toolNoActionTileColor = new Color(0.7f, 0.7f, 0, 1);
        }

        public ToolDescription DescribeTool()
        {
            return new ToolDescription.Builder(_loc.T(TitleLocKey))
                .AddSection(_loc.T(DescriptionLocKey))
                .Build();
        }

        public void Enter()
        {
            if (_toolUnlockingService.IsLocked(this))
            {
                _toolUnlockingService.TryToUnlock(this,
                    successCallback: () => {
                        _selectionToolProcessor.Enter();
                        _eventBus.Post(new ForestToolSelectedEvent(this));
                    },
                    failCallback: () => { }  // dialog already shown by locker
                );
                return;
            }
            _selectionToolProcessor.Enter();
            _eventBus.Post(new ForestToolSelectedEvent(this));
        }

        public void Exit()
        {
            _plantingSelectionService.UnhighlightAll();
            _selectionToolProcessor.Exit();
            _eventBus.Post(new ForestToolUnselectedEvent(this));
        }

        public void PostProcessInput()
        {
            // placeholder
        }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int coord in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                _areaHighlightingService.DrawTile(coord, _plantingToolTile);
            }
            _areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (!_toolUnlockingService.IsLocked(this))
            {
                Plant(inputBlocks, ray);
            }
        }

        private void ShowNoneCallback()
        {
            _areaHighlightingService.UnhighlightAll();
        }

        private void Plant(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int coord in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                string resourceName = ForestToolParam.GetNextRandomResourceName();

                if (string.IsNullOrEmpty(resourceName)
                    || resourceName.Equals(ForestToolParam.NameEmpty, StringComparison.OrdinalIgnoreCase))
                {
                    _plantingService.UnsetPlantingCoordinates(coord);
                }
                else if (_plantingAreaValidator.CanPlant(coord, resourceName))
                {
                    _plantingService.SetPlantingCoordinates(coord, resourceName);
                }
            }
            _eventBus.Post(new PlantingAreaMarkedEvent());
        }
    }
}