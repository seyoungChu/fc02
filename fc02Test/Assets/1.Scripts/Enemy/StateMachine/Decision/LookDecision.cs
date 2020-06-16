using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 타겟이 시야가 막히지 않은 상태에서 타겟이 시야각(viewAngle/2)사이에 있는가 판정.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Look")]
    public class LookDecision : Decision
    {
        // The delegate for results of overlapping targets in look decision.
        private bool MyHandleTargets(StateController controller, bool hasTargets, Collider[] targetsInViewRadius)
        {
            // Is there any sight on view radius?
            if(hasTargets)
            {
                Vector3 target = targetsInViewRadius[0].transform.position;
                // Check if target is in field of view.
                Vector3 dirToTarget = target - controller.transform.position;
                bool inFOVCondition = (Vector3.Angle(controller.transform.forward, dirToTarget) < controller.viewAngle / 2);
                // Is target in FOV and NPC have a clear sight?
                if (inFOVCondition && !controller.BlockedSight())
                {
                    // Set current target parameters.
                    controller.targetInSight = true;
                    controller.personalTarget = controller.aimTarget.position;
                    return true;
                }
            }
            // No target on sight.
            return false;
        }
        
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // Reset sight status on loop before checking.
            controller.targetInSight = false;
            // Check sight.
            return Decision.CheckTargetsInRadius(controller, controller.viewRadius, MyHandleTargets);
        }


    }
}

