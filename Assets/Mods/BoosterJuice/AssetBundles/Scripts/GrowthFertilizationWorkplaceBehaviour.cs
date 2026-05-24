using System;
using Timberborn.BehaviorSystem;
using Timberborn.EnterableSystem;
using Timberborn.WalkingSystem;
using Timberborn.Workshops;
using Timberborn.WorkSystem;

namespace Cordial.Mods.BoosterJuice.Scripts
{
    public class GrowthFertilizationWorkplaceBehaviour : WorkplaceBehavior
    {
        private GrowthFertilizationBuilding _growthFertilizationBuilding;
        private Workplace _workplace;
        private Enterable _enterable;

        public void Awake()
        {
            this._growthFertilizationBuilding = this.GetComponent<GrowthFertilizationBuilding>();
            this._workplace = this.GetComponent<Workplace>();
            this._enterable = this.GetComponent<Enterable>();
        }

        public override Decision Decide(BehaviorAgent agent)
        {
            if (!this._growthFertilizationBuilding.IsReadyToFertilize)
                return Decision.ReleaseNow();
            WalkInsideExecutor componentFast1 = agent.GetComponent<WalkInsideExecutor>();
            switch (componentFast1.Launch(this._enterable))
            {
                case ExecutorStatus.Success:
                    WorkExecutor componentFast2 = agent.GetComponent<WorkExecutor>();
                    if (componentFast2 != null)
                    {
                        componentFast2.Launch(0.25f);
                        return Decision.ReleaseWhenFinished((IExecutor)componentFast2);
                    }
                    else
                    {
                        return Decision.ReleaseNextTick();
                    }
                case ExecutorStatus.Failure:
                    this._workplace.UnassignWorker(agent.GetComponent<Worker>());
                    return Decision.ReleaseNextTick();
                case ExecutorStatus.Running:
                    return Decision.ReleaseWhenFinished((IExecutor)componentFast1);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
