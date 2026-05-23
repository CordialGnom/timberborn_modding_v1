using Cordial.Mods.PlantingOverride.Scripts.Common;
using Cordial.Mods.PlantingOverride.Scripts.UI;
using HarmonyLib;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.Demolishing;
using Timberborn.EntitySystem;
using Timberborn.ModManagerScene;
using Timberborn.Planting;
using Timberborn.Pollination;
using UnityEngine;

namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverrideHarmony : IModStarter
    {
        public void StartMod(IModEnvironment modEnvironment)
        {
            var harmony = new Harmony("Cordial.Mods.PlantingOverride");
            harmony.PatchAll();
        }

        // ---- PlantBehavior.Plant ----
        // Fires when a plant actually gets planted — used to clean up override registries
        [HarmonyPatch(typeof(PlantBehavior), "Plant")]
        public static class PlantBehaviourPatch
        {
            static bool Prefix(ref Vector3Int coordinates)
            {
                var plantingService = PlantingOverrideServiceCache.PlantingService;
                var eventBus        = PlantingOverrideServiceCache.EventBus;

                if (plantingService != null && eventBus != null)
                {
                    string oldResource = plantingService.GetResourceAt(coordinates);
                    eventBus.Post(new PlantingOverridePlantingEvent(coordinates, oldResource));
                    eventBus.Post(new PlantBeehiveToolUnmarkEvent(coordinates, false));
                }

                return true; // always let the original method run
            }
        }

        // ---- PlantingService.UnsetPlantingCoordinates ----
        // Fires when any tool clears a planting marker — remove from override registries
        // unless it's our own tool doing the clearing
        [HarmonyPatch(typeof(PlantingService), "UnsetPlantingCoordinates")]
        public static class PlantingServiceUnsetCoordinatePatch
        {
            static void Postfix(ref Vector3Int coordinates)
            {
                var eventBus    = PlantingOverrideServiceCache.EventBus;
                var toolService = PlantingOverrideServiceCache.ToolService;

                if (eventBus == null || toolService == null) return;

                var activeTool = toolService.ActiveTool;
                if (activeTool == null) return;

                // If our own override tool is active, skip — it manages its own registry
                if (activeTool.GetType().FullName?.Contains("PlantingOverride") == true) return;

                eventBus.Post(new PlantingOverrideRemoveEvent(coordinates));
            }
        }

        // ---- Demolishable.Unmark ----
        // Fires when a plant's demolish mark is removed — notify beehive tool
        [HarmonyPatch(typeof(Demolishable), "Unmark")]
        public static class DemolishableUnmarkPatch
        {
            static void Postfix(Demolishable __instance)
            {
                if (__instance == null) return;

                var blockObject = __instance.GetComponent<BlockObject>();
                if (blockObject == null) return;

                var eventBus    = PlantingOverrideServiceCache.EventBus;
                var toolService = PlantingOverrideServiceCache.ToolService;
                if (eventBus == null) return;

                // Only place hive if the active tool is NOT demolish/cancel
                bool placeHive = true;
                if (toolService?.ActiveTool != null)
                {
                    string toolName = toolService.ActiveTool.GetType().Name;
                    if (toolName.Contains("Delet") || toolName.Contains("Demol") || toolName.Contains("Cancel"))
                        placeHive = false;
                }

                eventBus.Post(new PlantBeehiveToolUnmarkEvent(blockObject.Coordinates, placeHive));
            }
        }

        // ---- Hive.Awake + OnEnterFinishedState ----
        // Register hives as they are created or finish construction
        [HarmonyPatch(typeof(Hive), "Awake")]
        public static class HiveAwakePatch
        {
            static void Postfix(Hive __instance)
            {
                if (__instance == null) return;
                PlantingOverrideServiceCache.EventBus?.Post(
                    new PlantBeehiveToolRegisterHiveEvent(__instance));
            }
        }

        [HarmonyPatch(typeof(Hive), "OnEnterFinishedState")]
        public static class HiveOnEnterFinishedStatePatch
        {
            static void Postfix(Hive __instance)
            {
                if (__instance == null) return;
                PlantingOverrideServiceCache.EventBus?.Post(
                    new PlantBeehiveToolRegisterHiveEvent(__instance));
            }
        }

        // ---- Hive.OnExitFinishedState ----
        [HarmonyPatch(typeof(Hive), "OnExitFinishedState")]
        public static class HiveOnExitFinishedStatePatch
        {
            static void Postfix(Hive __instance)
            {
                if (__instance == null) return;
                PlantingOverrideServiceCache.EventBus?.Post(
                    new PlantBeehiveToolUnregisterHiveEvent(__instance));
            }
        }

        // ---- EntityService.Delete ----
        // Unregister hives that get deleted (e.g. while under construction)
        [HarmonyPatch(typeof(EntityService), "Delete")]
        public static class EntityServiceOnDeletePatch
        {
            static void Postfix(ref BaseComponent entity)
            {
                if (entity == null) return;

                entity.TryGetComponent<Hive>(out var hive);
                if (hive == null) return;

                var eventBus = PlantingOverrideServiceCache.EventBus;
                if (eventBus == null) return;

                hive.TryGetComponent<BlockObject>(out var blockObject);
                if (blockObject != null)
                    eventBus.Post(new PlantBeehiveToolUnmarkEvent(blockObject.Coordinates, false));
                else
                    eventBus.Post(new PlantBeehiveToolUnregisterHiveEvent(hive));
            }
        }
    }
}
