using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Timberborn.Fields;
using Timberborn.Forestry;
using Timberborn.NaturalResources;
using Timberborn.Planting;
using Timberborn.SingletonSystem;
using Timberborn.TemplateSystem;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    /// <summary>
    /// Replaces both PlantingOverridePrefabSpecService and PlantBeehivePrefabSpecService.
    /// Uses TemplateService (native V1) instead of PrefabService (gone).
    /// Also replaces PrefabNameMapper.VerifyPrefabName with direct template lookup.
    /// </summary>
    public class PlantingOverrideSpecService : ILoadableSingleton
    {
        private readonly TemplateService _templateService;

        private ImmutableArray<string> _forestryPlantables;
        private ImmutableArray<string> _cropPlantables;

        public PlantingOverrideSpecService(TemplateService templateService)
        {
            _templateService = templateService;
        }

        public void Load()
        {
            _forestryPlantables = BuildForestryList();
            _cropPlantables     = BuildCropList();

            Debug.Log("PO: Loaded " + _forestryPlantables.Length + " forestry plantables, "
                      + _cropPlantables.Length + " crop plantables.");
        }

        // ---- Forestry (trees + bushes) ----

        public ImmutableArray<string> GetAllForestryPlantables() => _forestryPlantables;

        public bool IsForestryPlantable(string templateName) =>
            _forestryPlantables.Contains(templateName);

        // ---- Crops ----

        public ImmutableArray<string> GetAllCrops() => _cropPlantables;

        public bool IsCrop(string templateName) =>
            _cropPlantables.Contains(templateName);

        // ---- Validation ----

        /// <summary>
        /// Replaces PrefabNameMapper.VerifyPrefabName — checks the name exists
        /// in either forestry or crop lists.
        /// </summary>
        public bool IsValidPlantable(string templateName)
        {
            return IsForestryPlantable(templateName) || IsCrop(templateName);
        }

        // ---- Beehive faction check ----

        /// <summary>
        /// Returns true if a Beehive building spec exists — used to determine
        /// whether PlantBeehive tool should be shown for this faction/mod combo.
        /// </summary>
        public bool BeehiveExists(Timberborn.Buildings.BuildingService buildingService)
        {
            foreach (var building in buildingService.Buildings)
            {
                if (building.Blueprint.Name.Contains("Beehive"))
                    return true;
            }
            return false;
        }

        // ---- Private builders ----

        private ImmutableArray<string> BuildForestryList()
        {
            var list = new List<string>();
            foreach (var plantable in _templateService.GetAll<PlantableSpec>())
            {
                if (plantable.HasSpec<NaturalResourceSpec>()
                    && (plantable.HasSpec<BushSpec>() || plantable.HasSpec<TreeComponentSpec>()))
                {
                    list.Add(plantable.TemplateName);
                }
            }
            return list.ToImmutableArray();
        }

        private ImmutableArray<string> BuildCropList()
        {
            var list = new List<string>();
            foreach (var plantable in _templateService.GetAll<PlantableSpec>())
            {
                if (plantable.HasSpec<CropSpec>())
                {
                    list.Add(plantable.TemplateName);
                }
            }
            return list.ToImmutableArray();
        }
    }
}
