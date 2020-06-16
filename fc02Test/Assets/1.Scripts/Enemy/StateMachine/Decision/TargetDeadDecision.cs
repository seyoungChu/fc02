using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    /// <summary>
    /// target이 죽었는지 체크.
    /// </summary>
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Target Dead")]
    public class TargetDeadDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            try
            {
                // Check dead condition on target health manager.
                return controller.aimTarget.root.GetComponent<HealthBase>().isDead;
            }
            catch (UnassignedReferenceException)
            {
                // Ensure the target has a health manager set.
                Debug.LogError("Assign a health manager to" + controller.name);
            }
            return false;
        }
    }
}

