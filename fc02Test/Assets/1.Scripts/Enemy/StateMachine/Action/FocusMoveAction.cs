using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 공격과 동시에 이동하는 액션이며, 일단 회전할때는 회전에 집중하고 회전이 끝나면
    /// strafing 이 활성화된다.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Actions/Focus Move")]
    public class FocusMoveAction : Action
    {
        [Tooltip("The aim toggle decision while on focus.")]
        public ClearShotDecision clearShotDecision;

        private Vector3 currentDest;   // Current navigation destination.
        private bool aligned;          // Is the NPC orientation aligned to the target?

        // The action on enable function, triggered once after a FSM state transition.
        public override void OnReadyAction(StateController controller)
        {
            // Setup initial values for the action.
            controller.hadClearShot = controller.haveClearShot = false;
            currentDest = controller.nav.destination;
            controller.focusSight = true;
            aligned = false;
        }
        
        // The act function, called on Update() (State controller - current state - action).
        public override void Act(StateController controller)
        {
            // Align the NPC orientation.
            if (!aligned)
            {
                controller.nav.destination = controller.personalTarget;
                controller.nav.speed = 0f;
                // Only start strafing after orientation is aligned.
                if (controller.enemyAnimation.angularSpeed == 0)
                {
                    controller.Strafing = true;
                    aligned = true;
                    controller.nav.destination = currentDest;
                    controller.nav.speed = controller.generalStats.evadeSpeed;
                }
            }
            // Orientation is aligned, check if NPC has clear shot to the target.
            else
            {
                controller.haveClearShot = clearShotDecision.Decide(controller);
                // Grab clearShot instant (frame) change.
                if (controller.hadClearShot != controller.haveClearShot)
                {
                    // Aim on target if sight is clear.
                    controller.Aiming = controller.haveClearShot;
                    // NPC is not returning to cover, will stop to shot.
                    // 사격이 가능하다면 현재 이동 목표가 엄폐물과 달라도 일단 이동을 시키지 않는다.
                    if (controller.haveClearShot && !Equals(currentDest, controller.CoverSpot))
                    {
                        controller.nav.destination = controller.transform.position;
                    }
                        
                }
                controller.hadClearShot = controller.haveClearShot;
            }
        }
        
    }
}

