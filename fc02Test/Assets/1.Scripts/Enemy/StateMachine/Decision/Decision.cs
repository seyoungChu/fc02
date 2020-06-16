using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 조건 체크하는 클래스.
    /// 조건 체크를 위해 특정 위치로 부터 원하는 검색 반경에 있는 충돌체를 찾아서 그 안에 타겟이 있는지
    /// 확인하는 기능
    /// </summary>
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
            {
                return false;
            }
            else // Target is alive.
            {
                Collider[] targetsInRadius =
                    Physics.OverlapSphere(controller.transform.position, radius, controller.generalStats.targetMask);

                return handleTargets(controller, targetsInRadius.Length > 0, targetsInRadius);
            }
        }
    }
   
}