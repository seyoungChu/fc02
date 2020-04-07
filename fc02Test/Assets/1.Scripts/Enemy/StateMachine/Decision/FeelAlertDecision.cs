using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FC;

namespace FC
{
    [CreateAssetMenu(menuName = "PluggableAI/Decisions/Feel Alert")]
    public class FeelAlertDecision : Decision
    {
        public override bool Decide(StateController controller)
        {
            return controller.variables.feelAlert;
        }
    }
}

