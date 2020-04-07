using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "PluggableAI/Decisions/Reached Point")]
    public class ReachedPointDecision : Decision
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            if (controller.nav.remainingDistance <= controller.nav.stoppingDistance && !controller.nav.pathPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
