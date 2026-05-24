using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Forestry;
using Timberborn.NaturalResources;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Cordial.Mods.CutterTool.Scripts
{
    public class CutterToolSpecService : ILoadableSingleton
    {
        private readonly TemplateService _templateService;
        private ImmutableArray<string> _trees;

        public CutterToolSpecService(TemplateService templateService)
        {
            _templateService = templateService;
        }

        public void Load()
        {
            _trees = BuildTreeList();
            Debug.Log("CutterTool: Loaded " + _trees.Length + " tree types.");
        }

        public ImmutableArray<string> GetAllTrees() => _trees;

        private ImmutableArray<string> BuildTreeList()
        {
            var list = new List<string>();
            foreach (var plantable in _templateService.GetAll<PlantableSpec>())
            {
                if (plantable.HasSpec<NaturalResourceSpec>()
                    && plantable.HasSpec<TreeComponentSpec>())
                {
                    list.Add(plantable.TemplateName);
                }
            }
            return list.ToImmutableArray();
        }
    }
}
