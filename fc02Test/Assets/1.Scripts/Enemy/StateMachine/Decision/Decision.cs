using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    public abstract class Decision : ScriptableObject
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public abstract bool Decide(StateController controller);

        // The decision on enable function, triggered once after a FSM state transition.
        public virtual void OnEnableDecision(StateController controller)
        {
        }

        // The delegate for results of overlapping targets in senses decisions.
        public delegate bool HandeTargets(StateController controller, bool hasTargets, Collider[] targetsInRadius);
        
        // The common overlap function for senses decisions (look, hear, near, etc.)
        public static bool CheckTargetsInRadius(StateController controller, float radius, HandeTargets handleTargets)
        {
            // Target is dead, ignore sense triggers.
            if (controller.aimTarget.root.GetComponent<HealthBase>().isDead)
                return false;
            // Target is alive.
            else
            {
                Collider[] targetsInRadius =
                    Physics.OverlapSphere(controller.transform.position, radius, controller.generalStats.targetMask);

                return handleTargets(controller, targetsInRadius.Length > 0, targetsInRadius);
            }
        }
    }
   
}