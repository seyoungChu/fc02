using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 타겟이 있다면 타겟까지 이동하지만 없다면 서있게 된다.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Search")]
    public class SearchAction : Action
    {
        // The action on enable function, Triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Setup initial values for the action.
            controller.focusSight = false;
            controller.enemyAnimation.AbortPendingAim();
            controller.enemyAnimation.anim.SetBool(AnimatorKey.Crouch, false);
            controller.CoverSpot = Vector3.positiveInfinity;
        }
        
        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
            if (Equals(controller.personalTarget, Vector3.positiveInfinity))
            {
                controller.nav.destination = controller.transform.position;
            }
            else
            {
                // Set navigation parameters.
                controller.nav.speed = controller.generalStats.chaseSpeed;
                controller.nav.destination = controller.personalTarget;
            }
        }

        
    }   
}

