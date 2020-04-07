using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "PluggableAI/Actions/Spot Focus")]
    public class SpotFocusAction : Action
    {
        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = 0f;
        }
    }

}
