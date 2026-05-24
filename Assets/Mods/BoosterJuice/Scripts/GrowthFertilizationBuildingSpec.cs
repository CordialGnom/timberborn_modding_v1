using Timberborn.BlueprintSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public record GrowthFertilizationBuildingSpec : ComponentSpec
    {
        public float GrowthFactor { get; init; }
        public float GrowthConsumptionFactor { get; init; }
        public float YieldFactor { get; init; }
        public float YieldConsumptionFactor { get; init; }
        public int GrowthFertilizationRadius { get; init; }
        public int Capacity { get; init; }
        public string Supply { get; init; }
    }
}