using Timberborn.BlueprintSystem;
using Timberborn.SerializationSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public record GrowthFertilizationBuildingSpec : ComponentSpec
    {
        [Serialize] public float GrowthFactor { get; init; }
        [Serialize] public float GrowthConsumptionFactor { get; init; }
        [Serialize] public float YieldFactor { get; init; }
        [Serialize] public float YieldConsumptionFactor { get; init; }
        [Serialize] public int GrowthFertilizationRadius { get; init; }
        [Serialize] public int Capacity { get; init; }
        [Serialize] public string Supply { get; init; }
    }
}