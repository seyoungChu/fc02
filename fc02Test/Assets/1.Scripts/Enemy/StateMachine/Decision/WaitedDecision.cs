using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// 랜덤 시간만큼 기다렸는가?
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Waited")]
    public class WaitedDecision : Decision
    {
        public float maxTimeToWait;  // Maximum time to wait on a round.

        private float timeToWait;	 // Time to wait on current round.
        private float startTime;     // Timestamp of when the NPC began to wait.

        // The decision on enable function, triggered once after a FSM state transition.
        public override void OnEnableDecision(StateController controller)
        {
            // Calculate time to wait on current round.
            timeToWait = Random.Range(0, maxTimeToWait);
            // Set start waiting time.
            startTime = Time.time;
        }
        
        // The decide function, called on Update() (State controller - current state - transition - decision).
        public override bool Decide(StateController controller)
        {
            return (Time.time - startTime) >= timeToWait;
        }
        
    }

}
