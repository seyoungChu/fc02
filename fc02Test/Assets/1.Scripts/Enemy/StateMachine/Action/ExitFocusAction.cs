using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Exit Focus")]
    public class ExitFocusAction : Action
    {
        public override void Act(StateController controller)
        {
        }
        // The action on enable function, triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Setup initial values for the action.
            controller.focusSight = false;
            controller.variables.feelAlert = false;
            controller.variables.hearAlert = false;
            controller.Strafing = false;
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = 0f;
        }
    }
}

