using System;
using System.Collections.Generic;
using System.Linq;
using Cordial.Mods.CutterTool.Scripts.UI;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Cutting;
using Timberborn.Forestry;
using Timberborn.Gathering;
using Timberborn.GoodStackSystem;
using Timberborn.Growing;
using Timberborn.Localization;
using Timberborn.NaturalResourcesLifecycle;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using Timberborn.ToolSystemUI;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolService : ITool, IToolDescriptor, ILoadableSingleton
    {
        private static readonly string TitleLocKey       = "Cordial.CutterTool.DisplayName";
        private static readonly string DescriptionLocKey = "Cordial.CutterTool.Description";
        private static readonly string CursorKey         = "CutTreeCursor";

        private readonly ILoc _loc;
        private readonly EventBus _eventBus;
        private readonly SelectionToolProcessor _selectionToolProcessor;
        private readonly AreaHighlightingService _areaHighlightingService;
        private readonly TerrainAreaService _terrainAreaService;
        private readonly TreeCuttingArea _treeCuttingArea;
        private readonly PlantingService _plantingService;
        private readonly IBlockService _blockService;

        public Color _toolActionTileColor;
        public Color _toolNoActionTileColor;

        // Config state — updated via event from UI panel
        private CutterPattern _pattern     = CutterPattern.All;
        private bool _treeMarkOnly         = false;
        private bool _ignoreStumps         = false;
        private bool _ignoreSapling        = false;
        private bool _clearCut             = false;
        private List<string> _activeTypes  = new();

        public CutterToolService(
            SelectionToolProcessorFactory selectionToolProcessorFactory,
            AreaHighlightingService areaHighlightingService,
            TerrainAreaService terrainAreaService,
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

            _areaHighlightingService = areaHighlightingService;
            _terrainAreaService      = terrainAreaService;
            _treeCuttingArea         = treeCuttingArea;
            _plantingService         = plantingService;
            _blockService            = blockService;
            _eventBus                = eventBus;
            _loc                     = loc;
        }

        public void Load()
        {
            _toolActionTileColor   = new Color(0.95f, 0.03f, 0.05f, 1);
            _toolNoActionTileColor = new Color(0.7f, 0.7f, 0.0f, 1);
            _eventBus.Register(this);
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
            _eventBus.Post(new CutterToolSelectedEvent());
        }

        public void Exit()
        {
            _selectionToolProcessor.Exit();
            _eventBus.Post(new CutterToolUnselectedEvent());
        }

        public void PostProcessInput() { }

        private void PreviewCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            foreach (Vector3Int block in GetPatternCoordinates(inputBlocks, ray))
            {
                var spec = _blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                if (spec != null)
                {
                    string treeName = CleanName(spec.Name);
                    if (!_activeTypes.Contains(treeName))
                    {
                        _areaHighlightingService.DrawTile(block, _toolNoActionTileColor);
                        continue;
                    }

                    if (IsStump(spec) && _ignoreStumps)
                    { _areaHighlightingService.DrawTile(block, _toolNoActionTileColor); continue; }

                    if (IsDeadSapling(spec) && _ignoreSapling)
                    { _areaHighlightingService.DrawTile(block, _toolNoActionTileColor); continue; }

                    var obj = _blockService.GetBottomObjectAt(block);
                    if (obj != null)
                        _areaHighlightingService.AddForHighlight(obj);

                    _areaHighlightingService.DrawTile(block, _toolActionTileColor);
                }
                else
                {
                    // No tree component present — check if one is planned
                    string planned = _plantingService.GetResourceAt(block);
                    bool plannedMatchesActive = !string.IsNullOrEmpty(planned)
                                                && _activeTypes.Contains(planned);

                    if (_treeMarkOnly)
                    {
                        // Only mark if a matching tree is planned here
                        if (plannedMatchesActive)
                            _areaHighlightingService.DrawTile(block, _toolActionTileColor);
                        else
                            _areaHighlightingService.DrawTile(block, _toolNoActionTileColor);
                    }
                    else
                    {
                        // Mark everything, but exclude non-matching planned trees
                        if (string.IsNullOrEmpty(planned) || plannedMatchesActive)
                            _areaHighlightingService.DrawTile(block, _toolActionTileColor);
                        else
                            _areaHighlightingService.DrawTile(block, _toolNoActionTileColor);
                    }
                }
            }
            _areaHighlightingService.Highlight();
        }

        private void ActionCallback(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            var allBlocks = _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray);

            // Clear cutting area first if set
            if (_clearCut)
                _treeCuttingArea.RemoveCoordinates(allBlocks);

            _areaHighlightingService.UnhighlightAll();

            var toMark = new List<Vector3Int>();

            foreach (Vector3Int block in GetPatternCoordinates(inputBlocks, ray))
            {
                var spec = _blockService.GetBottomObjectComponentAt<TreeComponent>(block);

                if (spec != null)
                {
                    string treeName = CleanName(spec.Name);
                    
                    Debug.Log("CT: Tree: "  + treeName);

                    if (!_activeTypes.Contains(treeName)) continue;
                    if (IsStump(spec) && _ignoreStumps) continue;
                    if (IsDeadSapling(spec) && _ignoreSapling) continue;
                    toMark.Add(block);
                }
                else
                {
                    // No tree component present — check if one is planned
                    string planned = _plantingService.GetResourceAt(block);
                    bool plannedMatchesActive = !string.IsNullOrEmpty(planned)
                                                && _activeTypes.Contains(planned);

                    if (_treeMarkOnly)
                    {
                        if (plannedMatchesActive)
                            toMark.Add(block);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(planned) || plannedMatchesActive)
                            toMark.Add(block);
                    }
                }
            }

            _treeCuttingArea.AddCoordinates(toMark);
        }

        private void ShowNoneCallback() => _areaHighlightingService.UnhighlightAll();

        private IEnumerable<Vector3Int> GetPatternCoordinates(IEnumerable<Vector3Int> inputBlocks, Ray ray)
        {
            var blockList   = inputBlocks.ToList();
            var result      = new List<Vector3Int>();
            var singleBlock = new List<Vector3Int>(1) { Vector3Int.zero };

            Vector3Int minBlock = Vector3Int.zero;
            foreach (var b in blockList)
                minBlock = minBlock == Vector3Int.zero ? b : Vector3Int.Min(minBlock, b);

            foreach (var block in blockList)
            {
                bool include = _pattern switch
                {
                    CutterPattern.LinesX    => (minBlock.y - block.y) % 2 == 0,
                    CutterPattern.LinesY    => (minBlock.x - block.x) % 2 == 0,
                    CutterPattern.Checkered =>
                        (((minBlock.x - block.x) % 2 == 0) && ((minBlock.y - block.y) % 2 == 0)) ||
                        (((minBlock.x + 1 - block.x) % 2 == 0) && ((minBlock.y + 1 - block.y) % 2 == 0)),
                    _ => true // All
                };

                if (!include) continue;

                singleBlock[0] = block;
                foreach (var coord in _terrainAreaService.InMapLeveledCoordinates(singleBlock, ray))
                {
                    if (!_treeCuttingArea.IsInCuttingArea(coord))
                        result.Add(coord);
                }
            }

            return result;
        }

        private static string CleanName(string name) =>
            name.Replace("(Clone)", "").Replace(" ", "");

        private static bool IsStump(TreeComponent spec)
        {
            spec.TryGetComponent<Cuttable>(out var cuttable);
            spec.TryGetComponent<LivingNaturalResource>(out var living);
            spec.TryGetComponent<Gatherable>(out var gatherable);
            spec.TryGetComponent<Growable>(out var growable);

            if (cuttable == null || living == null || growable == null) return false;

            bool invEmpty         = living.GetComponent<GoodStack>().Inventory.IsEmpty;
            bool cuttableEmpty    = cuttable.Yielder.IsYieldRemoved;
            bool gatherableEmpty  = gatherable == null || gatherable.Yielder.IsYieldRemoved;
            bool grown            = growable.IsGrown;

            return invEmpty && cuttableEmpty && grown && gatherableEmpty;
        }

        private static bool IsDeadSapling(TreeComponent spec)
        {
            spec.TryGetComponent<LivingNaturalResource>(out var living);
            spec.TryGetComponent<Gatherable>(out var gatherable);
            spec.TryGetComponent<Growable>(out var growable);

            if (living == null || growable == null) return false;

            bool invEmpty        = living.GetComponent<GoodStack>().Inventory.IsEmpty;
            bool gatherableEmpty = gatherable == null || gatherable.Yielder.IsYieldRemoved;
            bool notGrown        = !growable.IsGrown;

            return invEmpty && gatherableEmpty && notGrown && living.IsDead;
        }

        [OnEvent]
        public void OnCutterToolConfigChangeEvent(CutterToolConfigChangeEvent evt)
        {
            if (evt == null) return;

            _pattern       = evt.Pattern;
            _treeMarkOnly  = evt.TreeMarkOnly;
            _ignoreStumps  = evt.IgnoreStumps;
            _clearCut      = evt.ClearCutArea;
            _ignoreSapling = evt.IgnoreDeadSapling;

            _activeTypes.Clear();
            foreach (var kvp in evt.TreeDict)
                if (kvp.Value) _activeTypes.Add(kvp.Key);
        }
    }
}
