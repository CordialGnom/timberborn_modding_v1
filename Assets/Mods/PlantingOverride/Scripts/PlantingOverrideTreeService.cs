using System;
using System.Collections.Generic;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Forestry;
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
    public class PlantingOverrideTreeService : ITool, IToolDescriptor,
                                               ISaveableSingleton, IPostLoadableSingleton
    {
        private static readonly string TitleLocKey       = "Cordial.PlantingOverrideTool.Tree.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantingOverrideTool.Tree.Description";
        private static readonly string CursorKey         = "PlantingCursor";

        private static readonly SingletonKey SaveKey      = new(nameof(PlantingOverrideTreeService));
        private static readonly ListKey<Vector3Int> CoordKey = new("Cordial.PlantingOverrideTreeCoordKey");
        private static readonly ListKey<string>     TypeKey  = new("Cordial.PlantingOverrideTreeTypeKey");
        private static readonly ListKey<Vector3Int> AreaKey  = new("Cordial.PlantingOverrideAreaCoordKey");

        private readonly ILoc _loc;
        private readonly EventBus _eventBus;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;
        private readonly PlantingService _plantingService;
        private readonly IBlockService _blockService;
        private readonly TreeCuttingArea _treeCuttingArea;
        private readonly ISingletonLoader _singletonLoader;
        private readonly PlantingOverrideSpecService _specService;

        public Color _toolActionTileColor;
        public Color _toolNoActionTileColor;

        private string _activeTreeType = string.Empty;
        private bool _removeCuttingArea;
        private Dictionary<Vector3Int, string> _treeRegistry = new();
        private List<Vector3Int> _areaRegistry = new();
        private static bool _loaded;

        public PlantingOverrideTreeService(
            SelectionToolProcessorFactory selectionToolProcessorFactory,
            AreaHighlightingService areaHighlightingService,
            PlantingOverrideSpecService specService,
            ToolUnlockingService toolUnlockingService,
            TerrainAreaService terrainAreaService,
            ISingletonLoader singletonLoader,
            TreeCuttingArea treeCuttingArea,
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

            _specService            = specService;
            _areaHighlightingService = areaHighlightingService;
            _toolUnlockingService   = toolUnlockingService;
            _terrainAreaService     = terrainAreaService;
            _plantingService        = plantingService;
            _singletonLoader        = singletonLoader;
            _treeCuttingArea        = treeCuttingArea;
            _blockService           = blockService;
            _eventBus               = eventBus;
            _loc                    = loc;
        }

        public void PostLoad()
        {
            _toolActionTileColor   = new Color(0.95f, 0.03f, 0.05f, 1);
            _toolNoActionTileColor = new Color(0.7f, 0.7f, 0.0f, 1);

            if (_singletonLoader.TryGetSingleton(SaveKey, out IObjectLoader loader))
            {
                if (loader.Has(AreaKey))
                    _areaRegistry = loader.Get(AreaKey);

                if (loader.Has(TypeKey) && loader.Has(CoordKey))
                {
                    var types  = loader.Get(TypeKey);
                    var coords = loader.Get(CoordKey);

                    if (types.Count == coords.Count)
                    {
                        for (int i = 0; i < types.Count; i++)
                        {
                            if (!_treeRegistry.TryAdd(coords[i], types[i]))
                                _treeRegistry[coords[i]] = types[i];
                        }

                        foreach (var kvp in _treeRegistry)
                        {
                            var tree = _blockService.GetBottomObjectComponentAt<TreeComponentSpec>(kvp.Key);
                            var bush = _blockService.GetBottomObjectComponentAt<BushSpec>(kvp.Key);
                            if ((tree != null || bush != null) && _specService.IsValidPlantable(kvp.Value))
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
            s.Set(CoordKey, _treeRegistry.Keys);
            s.Set(TypeKey,  _treeRegistry.Values);
            s.Set(AreaKey,  _areaRegistry);
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
                    () => { _selectionToolProcessor.Enter();
                            _eventBus.Post(new PlantingOverrideTreeSelectedEvent(this)); },
                    () => { });
                return;
            }
            _selectionToolProcessor.Enter();
            _eventBus.Post(new PlantingOverrideTreeSelectedEvent(this));
        }

        public void Exit()
        {
            _selectionToolProcessor.Exit();
            _eventBus.Post(new PlantingOverrideTreeUnselectedEvent());
        }

        public void PostProcessInput() { }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int block in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                var tree = _blockService.GetBottomObjectComponentAt<TreeComponentSpec>(block);
                var bush = _blockService.GetBottomObjectComponentAt<BushSpec>(block);

                if (tree != null || bush != null)
                {
                    BlockObject obj = _blockService.GetBottomObjectAt(block);
                    if (obj != null)
                        _areaHighlightingService.AddForHighlight(obj);
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
            if (_toolUnlockingService.IsLocked(this)) return;
            if (string.IsNullOrEmpty(_activeTreeType)) return;
            if (!_specService.IsValidPlantable(_activeTreeType)) return;

            _areaHighlightingService.UnhighlightAll();

            foreach (Vector3Int block in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray))
            {
                var tree = _blockService.GetBottomObjectComponentAt<TreeComponentSpec>(block);
                var bush = _blockService.GetBottomObjectComponentAt<BushSpec>(block);

                if (tree == null && bush == null) continue;

                _plantingService.SetPlantingCoordinates(block, _activeTreeType);

                if (_removeCuttingArea && !_areaRegistry.Contains(block))
                    _areaRegistry.Add(block);
                else if (!_removeCuttingArea && _areaRegistry.Contains(block))
                    _areaRegistry.Remove(block);

                if (_treeRegistry.ContainsKey(block))
                    _treeRegistry[block] = _activeTreeType;
                else
                    _treeRegistry.Add(block, _activeTreeType);
            }
        }

        private void ShowNoneCallback() => _areaHighlightingService.UnhighlightAll();

        [OnEvent]
        public void OnPlantingOverrideConfigChangeEvent(PlantingOverrideConfigChangeEvent evt)
        {
            if (evt == null || !evt.IsTree) return;
            _activeTreeType = evt.PlantName.Replace(" ", "");
        }

        [OnEvent]
        public void OnPlantingOverrideAreaRemoveEvent(PlantingOverrideAreaRemoveEvent evt)
        {
            if (evt == null) return;
            _removeCuttingArea = evt.RemoveCuttingArea;
        }

        [OnEvent]
        public void OnPlantingOverridePlantingEvent(PlantingOverridePlantingEvent evt)
        {
            if (evt == null) return;
            if (!_treeRegistry.ContainsKey(evt.Coordinates)) return;
            RemoveEntryInCutArea(evt.Coordinates);
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
            if (!_loaded) return;
            _treeRegistry.Remove(coord);
            _areaRegistry.Remove(coord);
        }

        private void RemoveEntryInCutArea(Vector3Int coord)
        {
            if (!_areaRegistry.Contains(coord)) return;
            _treeCuttingArea.RemoveCoordinates(new[] { coord });
            _areaRegistry.Remove(coord);
        }
    }
}
