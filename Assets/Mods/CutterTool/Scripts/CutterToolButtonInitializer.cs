using Timberborn.AssetSystem;
using Timberborn.SingletonSystem;
using Timberborn.ToolButtonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolButtonInitializer : IPostLoadableSingleton
    {
        private static readonly string IconPath = "Sprites/CutterTool/CutterToolIcon";
        private static readonly string GroupId  = "TreeCutting";

        private readonly CutterToolService    _tool;
        private readonly ToolButtonFactory    _toolButtonFactory;
        private readonly ToolButtonService    _toolButtonService;
        private readonly ToolGroupService     _toolGroupService;
        private readonly IAssetLoader         _assetLoader;

        public CutterToolButtonInitializer(
            CutterToolService tool,
            ToolButtonFactory toolButtonFactory,
            ToolButtonService toolButtonService,
            ToolGroupService toolGroupService,
            IAssetLoader assetLoader)
        {
            _tool              = tool;
            _toolButtonFactory = toolButtonFactory;
            _toolButtonService = toolButtonService;
            _toolGroupService  = toolGroupService;
            _assetLoader       = assetLoader;
        }

        public void PostLoad()
        {
            ToolGroupButton group = null;
            foreach (var g in _toolButtonService._toolGroupButtons)
            {
                if (g._toolGroup.Id == GroupId)
                {
                    group = g;
                    break;
                }
            }

            if (group == null)
            {
                Debug.LogError("CutterTool: TreeCutting group not found.");
                return;
            }

            Sprite icon   = _assetLoader.Load<Sprite>(IconPath);
            var button    = _toolButtonFactory.Create(_tool, icon, group.ToolButtonsElement);
            _toolGroupService.AssignToGroup(_toolGroupService.GetGroup(GroupId), _tool);
            group.AddTool(button);
            button.PostLoad();
            // No locker — tool is always available
        }
    }
}
