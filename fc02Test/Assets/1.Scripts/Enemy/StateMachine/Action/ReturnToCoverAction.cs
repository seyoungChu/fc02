﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    // The return to cover spot action.
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Return to Cover")]
    public class ReturnToCoverAction : Action
    {
        // The action on enable function, triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Is there a cover spot go?
            //장애물이 있으면 그곳으로 이동.
            if (!Equals(controller.CoverSpot, Vector3.positiveInfinity))
            {
                // Set navigation parameters.
                controller.nav.destination = controller.CoverSpot;
                controller.nav.speed = controller.generalStats.chaseSpeed;
                // The cover spot not near the current NPC position, stop aiming.
                if (Vector3.Distance(controller.CoverSpot, controller.transform.position) > 0.5f)
                {
                    controller.enemyAnimation.AbortPendingAim();
                }
            }
            // No cover spot, stand still.
            else
            {
                controller.nav.destination = controller.transform.position;
            }
        }
        
        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
            // Stop focusing on target, if there is a cover spot move to.
            //엄폐물로 이동하지 않았다면 타겟팅을 멈춤.
            if (!Equals(controller.CoverSpot, controller.transform.position))
            {
                 controller.focusSight = false;
            }
        }
        
    }

}
