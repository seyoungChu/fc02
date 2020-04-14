using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Reached Point")]
    public class ReachedPointDecision : Decision
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            if (Application.isPlaying == false)
            {
                return false;
            }
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
