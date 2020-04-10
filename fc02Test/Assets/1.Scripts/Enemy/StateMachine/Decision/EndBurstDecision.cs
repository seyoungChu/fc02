using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/End Burst")]
    public class EndBurstDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.variables.currentShots >= controller.variables.shotsInRound;
        }
    }
}

