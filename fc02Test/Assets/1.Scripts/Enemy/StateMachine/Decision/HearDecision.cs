using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// alertChecker를 통해 경고를 들었거나
    /// 특정거리에서 시야가 막혀있어도 특정위치에서 다른위치에 타겟이 이동했을경우를 판단.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Hear")]
    public class HearDecision : Decision
    {
        private Vector3 lastPos, currentPos;   // Last and current evidence positions.
        
        // The delegate for results of overlapping targets in hear decision.
        private bool MyHandleTargets(StateController controller, bool hasTargets, Collider[] targetsInHearRadius)
        {
            // Is there any evidence noticed?
            if (hasTargets)
            {
                // Grab current evidence position.
                currentPos = targetsInHearRadius[0].transform.position;
                // Evidence is already on track, check if it has moved.
                if (!Equals(lastPos, Vector3.positiveInfinity))
                {
                    // The hear sense is only triggered if the evidence is in movement.
                    if(!Equals(lastPos, currentPos))
                    {
                        controller.personalTarget = currentPos;
                        return true;
                    }
                }
                // Set evidence position for next game loop.
                lastPos = currentPos;
            }
            // No moving evidence was noticed.
            return false;
        }
        
        // The decision on enable function, triggered once after a FSM state transition.
        public override void OnEnableDecision(StateController controller)
        {
            lastPos = currentPos = Vector3.positiveInfinity;
        }

        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            // Handle external alert received.
            if(controller.variables.hearAlert)
            {
                controller.variables.hearAlert = false;
                return true;
            }
            // Check if something was heard by the NPC.
            else
            {
                return Decision.CheckTargetsInRadius(controller, controller.perceptionRadius, MyHandleTargets);
            }
        }

        
    }
}

