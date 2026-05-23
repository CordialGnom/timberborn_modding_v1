using System;
using Timberborn.Buildings;
using Timberborn.CoreUI;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.ScienceSystem;
using Timberborn.ToolSystem;

namespace Cordial.Mods.PlantingOverride.Scripts
{
    // -------------------------------------------------------------------------
    // Tree tool locker — requires Forester building
    // -------------------------------------------------------------------------
    public class PlantingOverrideTreeToolLocker : IToolLocker
    {
        private static readonly string BuildingLockKey   = "Cordial.PlantingOverrideTool.BuildingLock";
        private static readonly string MissingSpecLockKey = "Cordial.PlantingOverrideTool.SpecLock";

        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService          _buildingService;
        private readonly DialogBoxShower          _dialogBoxShower;
        private readonly ILoc                     _loc;

        public PlantingOverrideTreeToolLocker(
            DialogBoxShower dialogBoxShower,
            BuildingUnlockingService buildingUnlockingService,
            BuildingService buildingService,
            ILoc loc)
        {
            _buildingUnlockingService = buildingUnlockingService;
            _buildingService          = buildingService;
            _dialogBoxShower          = dialogBoxShower;
            _loc                      = loc;
        }

        public bool ShouldLock(ITool tool)
        {
            if (tool is not PlantingOverrideTreeService) return false;
            return !FindAndCheckBuilding("Forester");
        }

        public void TryToUnlock(ITool tool, Action successCallback, Action failCallback)
        {
            BuildingSpec spec = FindBuilding("Forester");
            if (spec == null)
            {
                ShowMissingMessage("Forester", failCallback);
                return;
            }
            if (!_buildingUnlockingService.Unlocked(spec))
            {
                ShowLockedMessage(spec, failCallback);
                return;
            }
            successCallback();
        }

        private bool FindAndCheckBuilding(string nameFragment)
        {
            var spec = FindBuilding(nameFragment);
            return spec != null && _buildingUnlockingService.Unlocked(spec);
        }

        private BuildingSpec FindBuilding(string nameFragment)
        {
            foreach (var b in _buildingService.Buildings)
                if (b.Blueprint.Name.Contains(nameFragment)) return b;
            return null;
        }

        private void ShowLockedMessage(BuildingSpec building, Action fail) =>
            _dialogBoxShower.Create()
                .SetMessage(_loc.T(BuildingLockKey) + " "
                    + _loc.T(building.GetSpec<LabeledEntitySpec>().DisplayNameLocKey))
                .SetConfirmButton(fail).Show();

        private void ShowMissingMessage(string target, Action fail) =>
            _dialogBoxShower.Create()
                .SetMessage(_loc.T(MissingSpecLockKey) + " " + target)
                .SetConfirmButton(fail).Show();
    }

    // -------------------------------------------------------------------------
    // Beehive tool locker — requires Beehive building
    // -------------------------------------------------------------------------
    public class PlantBeehiveToolLocker : IToolLocker
    {
        private static readonly string BuildingLockKey = "Cordial.PlantingOverrideTool.BuildingLock";
        private static readonly string FactionLockKey  = "Cordial.PlantingOverrideTool.FactionLock";

        private readonly BuildingUnlockingService _buildingUnlockingService;
        private readonly BuildingService          _buildingService;
        private readonly DialogBoxShower          _dialogBoxShower;
        private readonly ILoc                     _loc;

        public PlantBeehiveToolLocker(
            DialogBoxShower dialogBoxShower,
            BuildingUnlockingService buildingUnlockingService,
            BuildingService buildingService,
            ILoc loc)
        {
            _buildingUnlockingService = buildingUnlockingService;
            _buildingService          = buildingService;
            _dialogBoxShower          = dialogBoxShower;
            _loc                      = loc;
        }

        public bool ShouldLock(ITool tool)
        {
            if (tool is not PlantBeehive.PlantBeehiveToolService) return false;
            var spec = FindBuilding("Beehive");
            return spec == null || !_buildingUnlockingService.Unlocked(spec);
        }

        public void TryToUnlock(ITool tool, Action successCallback, Action failCallback)
        {
            BuildingSpec spec = FindBuilding("Beehive");
            if (spec == null)
            {
                ShowMissingMessage(failCallback);
                return;
            }
            if (!_buildingUnlockingService.Unlocked(spec))
            {
                ShowLockedMessage(spec, failCallback);
                return;
            }
            successCallback();
        }

        private BuildingSpec FindBuilding(string nameFragment)
        {
            foreach (var b in _buildingService.Buildings)
                if (b.Blueprint.Name.Contains(nameFragment)) return b;
            return null;
        }

        private void ShowLockedMessage(BuildingSpec building, Action fail) =>
            _dialogBoxShower.Create()
                .SetMessage(_loc.T(BuildingLockKey) + " "
                    + _loc.T(building.GetSpec<LabeledEntitySpec>().DisplayNameLocKey))
                .SetConfirmButton(fail).Show();

        private void ShowMissingMessage(Action fail) =>
            _dialogBoxShower.Create()
                .SetMessage(_loc.T(FactionLockKey))
                .SetConfirmButton(fail).Show();
    }
}
