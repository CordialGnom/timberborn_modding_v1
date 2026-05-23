using System.Collections.Generic;
using System.Collections.Immutable;
using Timberborn.Forestry;
using Timberborn.GameFactionSystem;
using Timberborn.NaturalResources;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Cordial.Mods.ForestTool.Scripts
{
    /// <summary>
    /// Loads faction and plantable resource information at game start.
    /// Replaces the old SpecService-based approach with TemplateService,
    /// which is the native V1 way to enumerate specs.
    /// </summary>
    public class ForestToolSpecService : ILoadableSingleton
    {
        private readonly TemplateService _templateService;
        private readonly FactionService _factionService;

        private string _factionId = string.Empty;
        public string FactionId => _factionId;

        public ForestToolSpecService(
            TemplateService templateService,
            FactionService factionService)
        {
            _templateService = templateService;
            _factionService = factionService;
        }

        public void Load()
        {
            if (_templateService == null || _factionService == null)
            {
                Debug.LogError("ForestTool: Missing required service in ForestToolSpecService.");
                return;
            }

            _factionId = _factionService.Current?.Id ?? string.Empty;

            ForestToolParam.ForestToolSpecService = this;
            ForestToolParam.InitConfigDefault();
        }

        /// <summary>
        /// Returns names of all plantable trees and bushes (used to populate
        /// the randomisation pool in ForestToolParam).
        /// Uses NaturalResourceSpec to filter to actual forestry plantables.
        /// </summary>
        public ImmutableArray<string> GetAllForestryPlantables()
        {
            List<string> names = new();

            foreach (PlantableSpec plantable in _templateService.GetAll<PlantableSpec>())
            {
                // Only include things that have a NaturalResourceSpec
                // (i.e. trees and bushes, not crops)
                if (plantable.HasSpec<NaturalResourceSpec>()
                    && (plantable.HasSpec<BushSpec>() || plantable.HasSpec<TreeComponentSpec>()))
                {
                    names.Add(plantable.TemplateName);
                }
            }

            return names.ToImmutableArray();
        }
    }
}