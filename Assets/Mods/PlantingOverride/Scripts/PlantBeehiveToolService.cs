using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockObjectTools;
using Timberborn.BlockSystem;
using Timberborn.Buildings;
using Timberborn.BuilderPrioritySystem;
using Timberborn.Coordinates;
using Timberborn.Demolishing;
using Timberborn.Growing;
using Timberborn.Localization;
using Timberborn.Persistence;
using Timberborn.Planting;
using Timberborn.Pollination;
using Timberborn.ScienceSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SelectionSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using Timberborn.ToolSystemUI;
using Timberborn.WorldPersistence;
using UnityEngine;
using Timberborn.Common;
using Castle.Components.DictionaryAdapter.Xml;

namespace Cordial.Mods.PlantingOverride.Scripts.PlantBeehive
{
    public class PlantBeehiveToolService : ITool, IToolDescriptor,
                                           ILoadableSingleton, ISaveableSingleton, IPostLoadableSingleton
    {
        private const int BeehiveRadius = 3;

        private static readonly string TitleLocKey       = "Cordial.PlantBeehiveTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.PlantBeehiveTool.Description";
        private static readonly string CursorKey         = "PickDestinationCursor";

        private static readonly SingletonKey SaveKey         = new("Cordial.PlantBeehiveToolService");
        private static readonly ListKey<Vector3Int> CoordKey = new("Cordial.PlantBeehiveToolCoordKey");

        private readonly ILoc _loc;
        private readonly EventBus _eventBus;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly ToolUnlockingService _toolUnlockingService;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;
        private readonly PlantingService _plantingService;
        private readonly IBlockService _blockService;
        private readonly BuildingService _buildingService;
        private readonly BlockObjectPlacerService _blockObjectPlacerService;
        private readonly ISingletonLoader _singletonLoader;

        public Color _toolActionTileColor;

        private BuildingSpec _beehiveSpec;
        private List<Vector3Int> _hiveCoordsNew = new();
        private List<Hive> _hiveRegistry = new();

        public PlantBeehiveToolService(
            SelectionToolProcessorFactory selectionToolProcessorFactory,
            AreaHighlightingService areaHighlightingService,
            ToolUnlockingService toolUnlockingService,
            TerrainAreaService terrainAreaService,
            ISingletonLoader singletonLoader,
            BuildingService buildingService,
            PlantingService plantingService,
            BlockObjectPlacerService blockObjectPlacerService,
            IBlockService blockService,
            EventBus eventBus,
            ILoc loc)
        {
            _selectionToolProcessor  = selectionToolProcessorFactory.Create(
                new Action<IEnumerable<Vector3Int>, Ray>(PreviewCallback),
                new Action<IEnumerable<Vector3Int>, Ray>(ActionCallback),
                new Action(ShowNoneCallback),
                CursorKey);

            _areaHighlightingService  = areaHighlightingService;
            _toolUnlockingService     = toolUnlockingService;
            _terrainAreaService       = terrainAreaService;
            _singletonLoader          = singletonLoader;
            _buildingService          = buildingService;
            _plantingService          = plantingService;
            _blockObjectPlacerService = blockObjectPlacerService;
            _blockService             = blockService;
            _eventBus                 = eventBus;
            _loc                      = loc;
        }

        public void Load()
        {
            _toolActionTileColor = new Color(1, 0.6f, 0, 1);
            _eventBus.Register(this);
        }

        public void PostLoad()
        {
            if (_singletonLoader.TryGetSingleton(SaveKey, out IObjectLoader loader))
            {
                if (loader.Has(CoordKey))
                    _hiveCoordsNew = loader.Get(CoordKey).Distinct().ToList();
            }
        }

        public void Save(ISingletonSaver singletonSaver)
        {
            singletonSaver.GetSingleton(SaveKey).Set(CoordKey, _hiveCoordsNew);
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
                    () => EnterActive(),
                    () => { });
                return;
            }
            EnterActive();
        }

        private void EnterActive()
        {
            // Find beehive spec — supports any faction or mod with a Beehive building
            _beehiveSpec = null;
            foreach (var building in _buildingService.Buildings)
            {
                if (building.Blueprint.Name.Contains("Beehive"))
                {
                    _beehiveSpec = building;
                    break;
                }
            }

            if (_beehiveSpec == null)
            {
                Debug.LogError("PlantBeehive: No Beehive building spec found.");
                return;
            }

            _selectionToolProcessor.Enter();
        }

        public void Exit()
        {
            _selectionToolProcessor.Exit();
            _areaHighlightingService.UnhighlightAll();
        }

        public void PostProcessInput() { }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (_toolUnlockingService.IsLocked(this)) return;

            Vector3Int startCoord = GetStartCoord(inputBlocks, ray);
            if (startCoord == Vector3Int.zero) return;

            var highlightArea = GetBeehiveArea(startCoord);
            foreach (Vector3Int coord in highlightArea)
                _areaHighlightingService.DrawTile(coord, _toolActionTileColor);

            var bottomObject = _blockService.GetBottomObjectAt(startCoord);
            if (bottomObject != null)
                _areaHighlightingService.AddForHighlight((BaseComponent)bottomObject);

            _areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            if (_toolUnlockingService.IsLocked(this)) return;

            Vector3Int startCoord = GetStartCoord(inputBlocks, ray);
            if (startCoord == Vector3Int.zero) return;

            PrepareBeehivePlacement(startCoord);
            _areaHighlightingService.Highlight();
        }

        private void ShowNoneCallback() { }

        private Vector3Int GetStartCoord(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            Vector3Int raw = inputBlocks.FirstOrDefault();
            if (raw == Vector3Int.zero) return Vector3Int.zero;

            var coords = new List<Vector2Int> { raw.XY() };
            var mapped = _terrainAreaService.InMapCoordinates(coords).ToList();
            return mapped.Count > 0 ? mapped[0] : Vector3Int.zero;
        }

        private List<Vector3Int> GetBeehiveArea(Vector3Int center)
        {
            var area = new List<Vector3Int>();

            // Cursor area
            area.AddRange(GetBlocksInRadius(center, BeehiveRadius));

            // Existing hive areas
            foreach (var hive in _hiveRegistry)
                area.AddRange(hive.GetBlocksInRange());

            // Reserved hive areas
            foreach (var coord in _hiveCoordsNew)
                area.AddRange(GetBlocksInRadius(coord, BeehiveRadius));

            return area;
        }

        private IEnumerable<Vector3Int> GetBlocksInRadius(Vector3Int center, int radius)
        {
            var blocks = new List<Vector2Int>();
            for (int x = center.x - radius; x <= center.x + radius; x++)
                for (int y = center.y - radius; y <= center.y + radius; y++)
                    blocks.Add(new Vector2Int(x, y));

            return _terrainAreaService.InMapCoordinates(blocks);
        }

        private void PrepareBeehivePlacement(Vector3Int coord)
        {
            if (!_blockService.AnyObjectAt(coord))
            {
                // Empty spot — place immediately
                if (!_hiveCoordsNew.Contains(coord))
                    _hiveCoordsNew.Add(coord);
                PlaceBeehive(coord);
                return;
            }

            var objects = _blockService.GetObjectsAt(coord);
            if (objects.Count != 1) return;

            foreach (var block in objects)
            {
                if (block == null) continue;

                block.TryGetComponent<Demolishable>(out var demolishable);
                block.TryGetComponent<GrowableSpec>(out var growable);
                block.TryGetComponent<BuildingSpec>(out var building);

                if (building != null || block.Name.Contains("Path")) return;

                if (demolishable != null && growable != null && !_hiveCoordsNew.Contains(coord))
                {
                    _hiveCoordsNew.Add(coord);
                    demolishable.Mark();
                    _plantingService.UnsetPlantingCoordinates(coord);

                    block.TryGetComponent<BuilderPrioritizable>(out var priority);
                    priority?.SetPriority(Timberborn.PrioritySystem.Priority.High);
                }
            }

            _hiveCoordsNew = _hiveCoordsNew.Distinct().ToList();
        }

        private void PlaceBeehive(Vector3Int coord)
        {
            if (_beehiveSpec == null) return;

            var placement = new Placement(coord);
            var blockSpec = _beehiveSpec.GetSpec<BlockObjectSpec>();
            if (blockSpec == null) return;

            var placer = _blockObjectPlacerService.GetMatchingPlacer(blockSpec);
            if (placer == null) return;

            try
            {
                placer.Place(blockSpec, placement, null);
                _hiveCoordsNew.Remove(coord);
            }
            catch (Exception e)
            {
                Debug.LogError("PlantBeehive: placement failed at " + coord + ": " + e.Message);
            }
        }

        [OnEvent]
        public void OnPlantBeehiveToolUnmarkEvent(PlantBeehiveToolUnmarkEvent evt)
        {
            if (evt == null) return;
            if (!_hiveCoordsNew.Contains(evt.Coordinates)) return;

            _hiveCoordsNew.Remove(evt.Coordinates);
            if (evt.PlaceHive)
                PlaceBeehive(evt.Coordinates);
        }

        [OnEvent]
        public void OnPlantBeehiveToolRegisterHiveEvent(PlantBeehiveToolRegisterHiveEvent evt)
        {
            if (evt?.Hive == null) return;
            if (_hiveRegistry.Contains(evt.Hive)) return;

            _hiveRegistry.Add(evt.Hive);
            _hiveCoordsNew.Remove(evt.Hive.GetComponent<BlockObject>().Coordinates);
        }

        [OnEvent]
        public void OnPlantBeehiveToolUnregisterHiveEvent(PlantBeehiveToolUnregisterHiveEvent evt)
        {
            if (evt?.Hive == null) return;
            _hiveRegistry.Remove(evt.Hive);
            _hiveCoordsNew.Remove(evt.Hive.GetComponent<BlockObject>().Coordinates);
        }

        [OnEvent]
        public void OnBuildingUnlockedEvent(BuildingUnlockedEvent evt)
        {
            if (evt?.BuildingSpec == null) return;
            if (!evt.BuildingSpec.Blueprint.Name.Contains("Beehive")) return;
            if (_toolUnlockingService.IsLocked(this))
                _toolUnlockingService.TryToUnlock(this, () => { }, () => { });
        }
    }
}
