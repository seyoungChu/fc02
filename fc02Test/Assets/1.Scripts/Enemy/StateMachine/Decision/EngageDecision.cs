using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 타겟이 보이거나 근처에 있으면 교전 대기 시간을 초기화하고
    /// 반대로 보이지 않거나 멀어져있으면 blindEngageTime 만큼 기다린다.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Engage")]
    public class EngageDecision : Decision
    {
        [Header("Extra Decisions")]
        [Tooltip("The NPC sight decision.")]
        public LookDecision isViewing;
        [Tooltip("The NPC near sense decision.")]
        public FocusDecision targetNear;

        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // The target is on sight, or is it in near sense?
            if (isViewing.Decide(controller) || targetNear.Decide(controller))
            {
                controller.variables.blindEngageTimer = 0;
            }
            // The blind engage timer surpassed the maximum time?
            else if (controller.variables.blindEngageTimer >= controller.blindEngageTime)
            {
                // Stop engaging.
                controller.variables.blindEngageTimer = 0;
                return false;
            }
            // Keep engaging.
            return true;
        }
    }

}
