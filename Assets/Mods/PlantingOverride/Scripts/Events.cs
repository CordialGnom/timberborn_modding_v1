using UnityEngine;
using Timberborn.Pollination;

// ---- PlantingOverride events ----
namespace Cordial.Mods.PlantingOverride.Scripts.Common
{
    public class PlantingOverridePlantingEvent
    {
        public Vector3Int Coordinates { get; }
        public string PlantableName   { get; }

        public PlantingOverridePlantingEvent(Vector3Int coordinates, string plantableName = "")
        {
            Coordinates   = coordinates;
            PlantableName = plantableName;
        }
    }

    public class PlantingOverrideRemoveEvent
    {
        public Vector3Int Coordinates { get; }

        public PlantingOverrideRemoveEvent(Vector3Int coordinates)
        {
            Coordinates = coordinates;
        }
    }
}

namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantingOverrideConfigChangeEvent
    {
        public string PlantName { get; }
        public bool   IsTree    { get; }

        public PlantingOverrideConfigChangeEvent(string plantName, bool isTree)
        {
            PlantName = plantName;
            IsTree    = isTree;
        }
    }

    public class PlantingOverrideAreaRemoveEvent
    {
        public bool RemoveCuttingArea { get; }

        public PlantingOverrideAreaRemoveEvent(bool removeCuttingArea)
        {
            RemoveCuttingArea = removeCuttingArea;
        }
    }

    public class PlantingOverrideTreeSelectedEvent
    {
        public PlantingOverrideTreeService Service { get; }
        public PlantingOverrideTreeSelectedEvent(PlantingOverrideTreeService service) => Service = service;
    }

    public class PlantingOverrideTreeUnselectedEvent
    {
        public PlantingOverrideTreeUnselectedEvent() { }
    }

    public class PlantingOverrideCropSelectedEvent
    {
        public PlantingOverrideCropService Service { get; }
        public PlantingOverrideCropSelectedEvent(PlantingOverrideCropService service) => Service = service;
    }

    public class PlantingOverrideCropUnselectedEvent
    {
        public PlantingOverrideCropUnselectedEvent() { }
    }
}

// ---- PlantBeehive events ----
namespace Cordial.Mods.PlantingOverride.Scripts.UI
{
    public class PlantBeehiveToolUnmarkEvent
    {
        public Vector3Int Coordinates { get; }
        public bool       PlaceHive   { get; }

        public PlantBeehiveToolUnmarkEvent(Vector3Int coordinates, bool placeHive)
        {
            Coordinates = coordinates;
            PlaceHive   = placeHive;
        }
    }

    public class PlantBeehiveToolRegisterHiveEvent
    {
        public Hive Hive { get; }
        public PlantBeehiveToolRegisterHiveEvent(Hive hive) => Hive = hive;
    }

    public class PlantBeehiveToolUnregisterHiveEvent
    {
        public Hive Hive { get; }
        public PlantBeehiveToolUnregisterHiveEvent(Hive hive) => Hive = hive;
    }
}
