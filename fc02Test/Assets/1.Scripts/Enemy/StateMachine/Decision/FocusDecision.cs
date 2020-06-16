using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 인지 타입에 따라 특정 거리로 부터 가깝진 않지만 시야는 막히지 않았지만 위험요소를 감지했거나
    /// 가까운 거리에 타겟이 있는지 판단
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Focus")]
    public class FocusDecision : Decision
    {
        // NPC Sense types.
        public enum Sense
        {
            NEAR,
            PERCEPTION,
            VIEW
        }
        
        [Tooltip("Which sense radius will be used?")]
        public Sense sense;

        [Tooltip("현재 엄폐물을 해제할까요?")]
        public bool invalidateCoverSpot;

        private float radius; // The sense radius that will be used.

        
        // The decision on enable function, triggered once after a FSM state transition.
        public override void OnEnableDecision(StateController controller)
        {
            // Define sense radius.
            switch (sense)
            {
                case Sense.NEAR:
                    radius = controller.nearRadius;
                    break;
                case Sense.PERCEPTION:
                    radius = controller.perceptionRadius;
                    break;
                case Sense.VIEW:
                    radius = controller.viewRadius;
                    break;
                default:
                    break;
            }
        }
        
        // The delegate for results of overlapping targets in focus decision.
        private bool MyHandleTargets(StateController controller, bool hasTargets, Collider[] targetsInHearRadius)
        {
            // Is there any target, with a clear sight to it?
            if (hasTargets && !controller.BlockedSight())
            {
                // Invalidade current cover spot (ex.: used to move from position when spotted).
                if (invalidateCoverSpot)
                {
                    controller.CoverSpot = Vector3.positiveInfinity;
                }
                // Set current target parameters.
                controller.targetInSight = true;
                controller.personalTarget = controller.aimTarget.position;
                return true;
            }

            // No target on sight.
            return false;
        }

        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // If target is not near: felt a shot and sight to target is clear, can focus.
            // If target is near, always check sense for target.
            return (sense != Sense.NEAR && controller.variables.feelAlert && !controller.BlockedSight()) ||
                   Decision.CheckTargetsInRadius(controller, radius, MyHandleTargets);
        }


    }
}