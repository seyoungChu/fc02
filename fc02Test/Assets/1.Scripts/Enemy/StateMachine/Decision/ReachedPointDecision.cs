using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// navMeshAgent에서 남은 거리가 멈추는 중일정도로 얼마 남지 않았거나, 경로를 검색중이 아니라면 true
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Reached Point")]
    public class ReachedPointDecision : Decision
    {
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            if (Application.isPlaying == false)
            {
                return false;
            }
            if (controller.nav.remainingDistance <= controller.nav.stoppingDistance && !controller.nav.pathPending)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
