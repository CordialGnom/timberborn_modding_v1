using System;
using Timberborn.BaseComponentSystem;
using Timberborn.BlockSystem;
using Timberborn.GameDistricts;
using Timberborn.Hauling;
using Timberborn.InventorySystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    internal class GrowthFertilizationStatusService : BaseComponent, IFinishedStateListener
    {
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private Workplace _workplace;
        private DistrictBuilding _districtBuilding;
        private LackOfResourcesStatus _lackOfResourcesStatus;
        private NoHaulingPostStatus _noHaulingPostStatus;

        public void Awake()
        {
            this._growthFertilizationBuilding = this.GetComponent<GrowthFertilizationBuilding>();
            this._workplace = this.GetComponent<Workplace>();
            this._districtBuilding = this.GetComponent<DistrictBuilding>();
            this._lackOfResourcesStatus = this.GetComponent<LackOfResourcesStatus>();
            this._noHaulingPostStatus = this.GetComponent<NoHaulingPostStatus>();
        }

        public void OnEnterFinishedState()
        {
            this._lackOfResourcesStatus.Initialize(new Func<bool>(this.CheckIfSupplyIsUnavailable));
            if (null != this._workplace)
                return;
            this._noHaulingPostStatus.Initialize((Func<bool>)(() => true));
        }

        public void OnExitFinishedState()
        {
            this._lackOfResourcesStatus.Disable();
            if (null != this._workplace)
                return;
            this._noHaulingPostStatus.Disable();
        }

        private bool CheckIfSupplyIsUnavailable()
        {
            return (this._workplace == null || this._workplace.NumberOfAssignedWorkers != 0)
                && this._districtBuilding.District != null
                && this._growthFertilizationBuilding.SupplyAmount <= 0.0
                && this._districtBuilding.District.GetComponent<DistrictInventoryRegistry>()
                       .ActiveInventoriesWithStock(this._growthFertilizationBuilding.Supply).Count == 0;
        }
    }
}
