using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    //1회 전투시 한번에 재장전 전까지 쏠수있는 총알을 갯수.
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/End Burst")]
    public class EndBurstDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.variables.currentShots >= controller.variables.shotsInRound;
        }
    }
}

