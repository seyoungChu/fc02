using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "PluggableAI/Actions/Go To Shot Spot")]
    public class GoToShotSpotAction : Action
    {
        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
        }
        // The action on enable function, triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Setup initial values for the action.
            controller.focusSight = false;
            controller.nav.destination = controller.personalTarget;
            controller.nav.speed = controller.generalStats.chaseSpeed;
            controller.enemyAnimation.AbortPendingAim();
        }
    }
}

