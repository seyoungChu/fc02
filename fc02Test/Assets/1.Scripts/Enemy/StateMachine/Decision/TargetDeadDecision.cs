using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "FC/PluggableAI/Decisions/Target Dead")]
    public class TargetDeadDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            try
            {
                // Check dead condition on target health manager.
                return controller.aimTarget.root.GetComponent<HealthManager>().dead;
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

