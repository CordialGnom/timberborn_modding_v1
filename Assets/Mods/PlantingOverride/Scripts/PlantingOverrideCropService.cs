using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Fields;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using Timberborn.ToolSystemUI;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    public class PlantingOverrideCropService : ITool, IToolDescriptor,
                                               ISaveableSingleton, IPostLoadableSingleton
    {
        private static readonly string TitleLocKey       = "Cordial.PlantingOverrideTool.Crop.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.Crop.Description";
        private static readonly string CursorKey         = "PlantingCursor";

        private static readonly SingletonKey SaveKey         = new(nameof(PlantingOverrideCropService));
        private static readonly ListKey<Vector3Int> CoordKey = new("Cordial.PlantingOverrideCropCoordKey");
        private static readonly ListKey<string>     TypeKey  = new("Cordial.PlantingOverrideCropTypeKey");

        private readonly ILoc _loc;
        private readonly EventBus _eventBus;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;
        private readonly PlantingService _plantingService;
        private readonly IBlockService _blockService;
        private readonly ISingletonLoader _singletonLoader;
        private readonly PlantingOverrideSpecService _specService;

        public Color _toolActionTileColor;
        public Color _toolNoActionTileColor;

        private string _activeCropType = string.Empty;
        private Dictionary<Vector3Int, string> _cropRegistry = new();
        private static bool _loaded;

        public PlantingOverrideCropService(
            SelectionToolProcessorFactory selectionToolProcessorFactory,
            AreaHighlightingService areaHighlightingService,
            PlantingOverrideSpecService specService,
            ToolUnlockingService toolUnlockingService,
            TerrainAreaService terrainAreaService,
            ISingletonLoader singletonLoader,
            PlantingService plantingService,
            IBlockService blockService,
            EventBus eventBus,
            ILoc loc)
        {
            _selectionToolProcessor = selectionToolProcessorFactory.Create(
                new Action<IEnumerable<Vector3Int>, Ray>(PreviewCallback),
                new Action<IEnumerable<Vector3Int>, Ray>(ActionCallback),
                new Action(ShowNoneCallback),
                CursorKey);

            _specService             = specService;
            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService    = toolUnlockingService;
            _terrainAreaService      = terrainAreaService;
            _plantingService         = plantingService;
            _singletonLoader         = singletonLoader;
            _blockService            = blockService;
            _eventBus                = eventBus;
            _loc                     = loc;
        }

        public void PostLoad()
        {
            _toolActionTileColor   = new Color(0.95f, 0.03f, 0.05f, 1);
            _toolNoActionTileColor = new Color(0.7f, 0.7f, 0.0f, 1);

            if (_singletonLoader.TryGetSingleton(SaveKey, out IObjectLoader loader))
            {
                if (loader.Has(TypeKey) && loader.Has(CoordKey))
                {
                    var types  = loader.Get(TypeKey);
                    var coords = loader.Get(CoordKey);

                    if (types.Count == coords.Count)
                    {
                        for (int i = 0; i < types.Count; i++)
                        {
                            if (!_cropRegistry.TryAdd(coords[i], types[i]))
                                _cropRegistry[coords[i]] = types[i];
                        }

                        foreach (var kvp in _cropRegistry.ToList())
                        {
                            var crop = _blockService.GetBottomObjectComponentAt<Crop>(kvp.Key);
                            if (crop != null && _specService.IsValidPlantable(kvp.Value))
                                _plantingService.SetPlantingCoordinates(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            _eventBus.Register(this);
            _loaded = true;
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            var s = singletonSaver.GetSingleton(SaveKey);
            s.Set(CoordKey, _cropRegistry.Keys);
            s.Set(TypeKey,  _cropRegistry.Values);
        }

        public ToolDescription DescribeTool()
        {
            return new ToolDescription.Builder(_loc.T(TitleLocKey))
                .AddSection(_loc.T(DescriptionLocKey))
                .Build();
        }

        public void Enter()
        {
            _selectionToolProcessor.Enter();
            _eventBus.Post(new PlantingOverrideCropSelectedEvent(this));
        }

        public void Exit()
        {
            _selectionToolProcessor.Exit();
            _eventBus.Post(new PlantingOverrideCropUnselectedEvent());
        }

        public void PostProcessInput() { }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int block in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                var crop = _blockService.GetBottomObjectComponentAt<Crop>(block);
                if (crop != null)
                {
                    _areaHighlightingService.AddForHighlight((BaseComponent)crop);
                    _areaHighlightingService.DrawTile(block, _toolActionTileColor);
                }
                else
                {
                    _areaHighlightingService.DrawTile(block, _toolNoActionTileColor);
                }
            }
            _areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (string.IsNullOrEmpty(_activeCropType)) return;
            if (!_specService.IsValidPlantable(_activeCropType)) return;

            _areaHighlightingService.UnhighlightAll();

            foreach (Vector3Int block in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                var crop = _blockService.GetBottomObjectComponentAt<Crop>(block);
                if (crop == null) continue;

                _plantingService.SetPlantingCoordinates(block, _activeCropType);

                if (_cropRegistry.ContainsKey(block))
                    _cropRegistry[block] = _activeCropType;
                else
                    _cropRegistry.Add(block, _activeCropType);
            }
        }

        private void ShowNoneCallback() => _areaHighlightingService.UnhighlightAll();

        [OnEvent]
        public void OnPlantingOverrideConfigChangeEvent(PlantingOverrideConfigChangeEvent evt)
        {
            if (evt == null || evt.IsTree) return;
            _activeCropType = evt.PlantName.Replace(" ", "");
        }

        [OnEvent]
        public void OnPlantingOverridePlantingEvent(PlantingOverridePlantingEvent evt)
        {
            if (evt == null) return;
            RemoveEntry(evt.Coordinates);
        }

        [OnEvent]
        public void OnPlantingOverrideRemoveEvent(PlantingOverrideRemoveEvent evt)
        {
            if (evt == null) return;
            RemoveEntry(evt.Coordinates);
        }

        private void RemoveEntry(Vector3Int coord)
        {
            if (_loaded) _cropRegistry.Remove(coord);
        }
    }
}
